using System;
using Unity.Netcode;
using UnityEngine;
using Zippers.Network;

/// <summary>
/// [MIGRATION · Step 2] 호환 래퍼.
///
/// 원래 이 컴포넌트가 자체적으로 _scrap / _infectionSample 을 들고 있었지만,
/// 멀티 전환을 위해 데이터를 다음 NetworkBehaviour 로 옮겼다:
///   - Scrap / InfectionSample → PlayerEconomyNetState (같은 GameObject)
///   - Supplies → TeamEconomyNetState (씬 singleton, TeamResourceManager 래퍼 경유)
///
/// 이 파일은 IResourceCollectable 인터페이스 시그니처와 외부 이벤트를 보존하기 위해 유지.
/// 외부 호출(아이템 픽업 / 몬스터 보상 / 업그레이드 비용 검증)은 변경 없이 동작.
///
/// 외부 시그니처 (호출 코드 변경 불필요):
///   - Scrap / Supplies / InfectionSample 프로퍼티
///   - CollectResource(type, amount)         — IResourceCollectable
///   - HasEnoughResource(type, amount)
///   - UseResource(type, amount)
///   - OnResourceCollected / OnResourceUsed 이벤트
/// </summary>
public class PlayerResourceCollector : MonoBehaviour, IResourceCollectable
{
    public Action<ResourcesType, float> OnResourceCollected;
    public Action<ResourcesType, float> OnResourceUsed;

    private PlayerEconomyNetState _economyNetState;
    private TeamResourceManager _teamResourceManager;

    private bool _economyEventBound;
    private bool _teamEventBound;

    // ── 프로퍼티 ─────────────────────────────────────────────────────
    public float Scrap =>
        _economyNetState != null ? _economyNetState.CurrentScrap : 0f;

    public float InfectionSample =>
        _economyNetState != null ? _economyNetState.CurrentInfectionSample : 0f;

    public float Supplies =>
        _teamResourceManager != null ? _teamResourceManager.Supplies : 0f;

    // ── Lifecycle ───────────────────────────────────────────────────

    private void Awake()
    {
        _economyNetState = GetComponent<PlayerEconomyNetState>();
        if (_economyNetState == null)
        {
            DebugTool.Log("[PlayerResourceCollector] PlayerEconomyNetState 가 같은 GameObject 에 없습니다.", DebugType.Data, this);
        }

        TryConnectTeamResourceManager();
    }

    private void OnDisable()
    {
        UnbindEvents();
    }

    private void OnDestroy()
    {
        UnbindEvents();
    }

    /// <summary>
    /// PlayerEconomyNetState 의 NetworkVariable 콜백은 OnNetworkSpawn 이후에 안정적으로 등록 가능.
    /// TeamResourceManager 의 이벤트도 마찬가지로 늦게 준비될 수 있어서 매 프레임 lazy bind.
    /// </summary>
    private void Update()
    {
        TryBindEvents();
    }

    private void TryBindEvents()
    {
        // PlayerEconomyNetState 이벤트
        if (!_economyEventBound && _economyNetState != null)
        {
            _economyNetState.OnResourceChanged += HandleEconomyResourceChanged;
            _economyEventBound = true;
        }

        // TeamResourceManager 이벤트
        if (!_teamEventBound)
        {
            TryConnectTeamResourceManager();
            if (_teamResourceManager != null)
            {
                _teamResourceManager.TeamResourceChanged += HandleTeamResourceChanged;
                _teamEventBound = true;
            }
        }
    }

    private void UnbindEvents()
    {
        if (_economyEventBound && _economyNetState != null)
        {
            _economyNetState.OnResourceChanged -= HandleEconomyResourceChanged;
            _economyEventBound = false;
        }
        if (_teamEventBound && _teamResourceManager != null)
        {
            _teamResourceManager.TeamResourceChanged -= HandleTeamResourceChanged;
            _teamEventBound = false;
        }
    }

    private void TryConnectTeamResourceManager()
    {
        if (_teamResourceManager != null) return;

        _teamResourceManager = TeamResourceManager.Instance;
        if (_teamResourceManager == null)
        {
            _teamResourceManager = FindFirstObjectByType<TeamResourceManager>();
        }
    }

    // ── IResourceCollectable 구현 ───────────────────────────────────

    public void CollectResource(ResourcesType type, float amount)
    {
        // Supplies 는 팀 공용 → TeamResourceManager 경유 (내부적으로 TeamEconomyNetState 로 위임)
        if (type == ResourcesType.Supplies)
        {
            AddTeamSupplies(amount);
            return;
        }

        // Scrap / InfectionSample 은 본인 PlayerEconomyNetState 로 위임
        if (_economyNetState == null)
        {
            DebugTool.Log("[PlayerResourceCollector] PlayerEconomyNetState 미준비 - 무시", DebugType.Data, this);
            return;
        }

        ulong ownerId = _economyNetState.OwnerClientId;

        if (IsServer())
        {
            _economyNetState.ServerGrantResource(ownerId, type, amount, "PlayerResourceCollector.CollectResource");
        }
        else
        {
            // 본인 자원이 아니면 RPC 보낼 권한이 없음 (RequireOwnership=true)
            // CollectResource 는 보통 본인 행위(아이템 픽업 등)라 IsOwner 가정 가능.
            if (_economyNetState.IsOwner)
            {
                _economyNetState.RequestGrantResourceServerRpc((int)type, amount);
            }
            else
            {
                DebugTool.Log(
                    $"[PlayerResourceCollector] CollectResource 클라 호출 무시 (IsOwner=false, type={type})",
                    DebugType.Data, this);
            }
        }
    }

    public bool HasEnoughResource(ResourcesType type, float amount)
    {
        if (amount <= 0f) return false;

        if (type == ResourcesType.Supplies)
        {
            TryConnectTeamResourceManager();
            return _teamResourceManager != null && _teamResourceManager.HasEnoughResource(type, amount);
        }

        if (_economyNetState == null) return false;
        return _economyNetState.HasEnoughResource(type, amount);
    }

    public bool UseResource(ResourcesType type, float amount)
    {
        if (amount <= 0f) return false;

        if (type == ResourcesType.Supplies)
        {
            TryConnectTeamResourceManager();
            if (_teamResourceManager == null)
            {
                DebugTool.Log("[PlayerResourceCollector] TeamResourceManager 없음 - Supplies 사용 불가", DebugType.Data, this);
                return false;
            }
            return _teamResourceManager.UseResource(type, amount);
        }

        if (_economyNetState == null) return false;

        ulong ownerId = _economyNetState.OwnerClientId;

        if (IsServer())
        {
            return _economyNetState.ServerSpendResource(ownerId, type, amount, "PlayerResourceCollector.UseResource");
        }

        if (_economyNetState.IsOwner)
        {
            _economyNetState.RequestSpendResourceServerRpc((int)type, amount);
            return true; // 요청 송신 — 실제 차감 결과는 NetworkVariable 동기화 후 확인
        }

        DebugTool.Log(
            $"[PlayerResourceCollector] UseResource 클라 호출 무시 (IsOwner=false, type={type})",
            DebugType.Data, this);
        return false;
    }

    // ── 내부 헬퍼 ──────────────────────────────────────────────────

    private void AddTeamSupplies(float amount)
    {
        TryConnectTeamResourceManager();
        if (_teamResourceManager == null)
        {
            DebugTool.Log("[PlayerResourceCollector] TeamResourceManager 없음 - Supplies 획득 불가", DebugType.Data, this);
            return;
        }
        _teamResourceManager.AddResource(ResourcesType.Supplies, amount);
    }

    // PlayerEconomyNetState 의 자원 변경 → 기존 OnResourceCollected/OnResourceUsed 로 재방출
    private void HandleEconomyResourceChanged(ResourcesType type, float current, float delta)
    {
        if (delta > 0f)
        {
            OnResourceCollected?.Invoke(type, delta);
        }
        else if (delta < 0f)
        {
            OnResourceUsed?.Invoke(type, Mathf.Abs(delta));
        }
    }

    // TeamResourceManager 의 Supplies 변경 → 기존 OnResourceCollected/OnResourceUsed 로 재방출
    private void HandleTeamResourceChanged(ResourcesType type, float current, float delta)
    {
        if (type != ResourcesType.Supplies) return;

        if (delta > 0f)
        {
            OnResourceCollected?.Invoke(type, delta);
        }
        else if (delta < 0f)
        {
            OnResourceUsed?.Invoke(type, Mathf.Abs(delta));
        }
    }

    private static bool IsServer()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
    }
}
