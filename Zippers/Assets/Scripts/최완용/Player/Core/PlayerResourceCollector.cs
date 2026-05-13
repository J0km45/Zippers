using UnityEngine;
using System;

public class PlayerResourceCollector : MonoBehaviour, IResourceCollectable
{
    public Action<ResourcesType, float> OnResourceCollected;
    public Action<ResourcesType, float> OnResourceUsed;

    private float _scrap;
    private float _infectionSample;

    private TeamResourceManager _teamResourceManager;

    public float Scrap => _scrap;
    public float Supplies => _teamResourceManager != null ? _teamResourceManager.Supplies : 0f;
    public float InfectionSample => _infectionSample;

    private void Awake()
    {
        TryConnectTeamResourceManager();
    }

    private void OnEnable()
    {
        TryConnectTeamResourceManager();

        if (_teamResourceManager != null)
        {
            _teamResourceManager.TeamResourceChanged += HandleTeamResourceChanged;
        }
    }

    private void OnDisable()
    {
        if (_teamResourceManager != null)
        {
            _teamResourceManager.TeamResourceChanged -= HandleTeamResourceChanged;
        }
    }

    public void CollectResource(ResourcesType type, float amount)
    {
        if (type == ResourcesType.Supplies)
        {
            AddTeamSupplies(amount);
            return;
        }

        bool isAdded = AddResource(type, amount);

        if (!isAdded)
        {
            return;
        }

        OnResourceCollected?.Invoke(type, amount);
    }

    // 재화 보유량 확인
    public bool HasEnoughResource(ResourcesType type, float amount)
    {
        if (amount <= 0f)
        {
            return false;
        }

        if (type == ResourcesType.Supplies)
        {
            TryConnectTeamResourceManager();

            if (_teamResourceManager == null)
            {
                return false;
            }

            return _teamResourceManager.HasEnoughResource(type, amount);
        }

        return GetResourceAmount(type) >= amount;
    }

    // 재화 사용
    public bool UseResource(ResourcesType type, float amount)
    {
        if (amount <= 0f)
        {
            return false;
        }

        if (type == ResourcesType.Supplies)
        {
            TryConnectTeamResourceManager();

            if (_teamResourceManager == null)
            {
                DebugTool.Log("[PlayerResourceCollector] TeamResourceManager가 없어 Supplies를 사용할 수 없습니다.", DebugType.Data, this);
                return false;
            }

            return _teamResourceManager.UseResource(type, amount);
        }

        if (!HasEnoughResource(type, amount))
        {
            DebugTool.Log($"{type} 부족 / 필요: {amount}, 보유: {GetResourceAmount(type)}", DebugType.Data, this);
            return false;
        }

        switch (type)
        {
            case ResourcesType.Scrap:
                _scrap -= amount;
                break;

            case ResourcesType.InfectionSample:
                _infectionSample -= amount;
                break;

            case ResourcesType.None:
                return false;

            default:
                return false;
        }

        OnResourceUsed?.Invoke(type, amount);
        DebugTool.Log($"{type} 사용 : -{amount} / 남은 수량 : {GetResourceAmount(type)}", DebugType.Data, this);

        return true;
    }

    private bool AddResource(ResourcesType type, float amount)
    {
        switch (type)
        {
            case ResourcesType.Scrap:
                _scrap += amount;
                DebugTool.Log($"Scrap 획득 : +{amount} / 현재 Scrap : {_scrap}", DebugType.Data, this);
                return true;

            case ResourcesType.Supplies:
                DebugTool.Log("[PlayerResourceCollector] Supplies는 TeamResourceManager에서 관리합니다.", DebugType.Data, this);
                return false;

            case ResourcesType.InfectionSample:
                _infectionSample += amount;
                DebugTool.Log($"InfectionSample 획득 : +{amount} / 현재 InfectionSample : {_infectionSample}", DebugType.Data, this);
                return true;

            case ResourcesType.None:
                return false;

            default:
                return false;
        }
    }

    private float GetResourceAmount(ResourcesType type)
    {
        switch (type)
        {
            case ResourcesType.Scrap:
                return _scrap;

            case ResourcesType.Supplies:
                TryConnectTeamResourceManager();

                if (_teamResourceManager != null)
                {
                    return _teamResourceManager.Supplies;
                }

                return 0f;

            case ResourcesType.InfectionSample:
                return _infectionSample;

            case ResourcesType.None:
                return 0f;

            default:
                return 0f;
        }
    }

    // TeamResourceManager 참조 연결
    private void TryConnectTeamResourceManager()
    {
        if (_teamResourceManager != null)
        {
            return;
        }

        _teamResourceManager = TeamResourceManager.Instance;

        if (_teamResourceManager == null)
        {
            _teamResourceManager = FindFirstObjectByType<TeamResourceManager>();
        }
    }

    // Supplies는 개인 재화가 아니라 팀 공용 재화로 추가
    private void AddTeamSupplies(float amount)
    {
        TryConnectTeamResourceManager();

        if (_teamResourceManager == null)
        {
            DebugTool.Log("[PlayerResourceCollector] TeamResourceManager가 없어 Supplies를 추가할 수 없습니다.", DebugType.Data, this);
            return;
        }

        _teamResourceManager.AddResource(ResourcesType.Supplies, amount);
    }

    // 팀 Supplies 변경 이벤트를 받아 UI 갱신용 이벤트만 전달
    private void HandleTeamResourceChanged(ResourcesType type, float currentAmount, float changedAmount)
    {
        if (type != ResourcesType.Supplies)
        {
            return;
        }

        if (changedAmount > 0f)
        {
            OnResourceCollected?.Invoke(type, changedAmount);
        }
        else if (changedAmount < 0f)
        {
            OnResourceUsed?.Invoke(type, Mathf.Abs(changedAmount));
        }

        DebugTool.Log($"[PlayerResourceCollector] 팀 Supplies 변경 감지 / 현재 Supplies : {currentAmount}", DebugType.Data, this);
    }
}