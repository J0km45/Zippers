using UnityEngine;

/// <summary>
/// 팀 업그레이드 성공 후,
/// PlayerHealth / PlayerStamina처럼 값을 따로 저장하는 컴포넌트를 갱신한다.
/// </summary>
public class TeamUpgradeRuntimeApplier : MonoBehaviour
{
    [Header("팀 업그레이드 데이터")]
    [SerializeField] private TeamUpgradeData _teamUpgradeData;

    private void Awake()
    {
        if (_teamUpgradeData == null)
        {
            _teamUpgradeData = GetComponent<TeamUpgradeData>();
        }

        if (_teamUpgradeData == null)
        {
            DebugTool.Log("[TeamUpgradeRuntimeApplier] TeamUpgradeData가 없습니다.", DebugType.Data, this);
        }
    }

    private void OnEnable()
    {
        if (_teamUpgradeData == null)
        {
            return;
        }

        _teamUpgradeData.TeamUpgradeChanged += ApplyRuntimeValue;
    }

    private void OnDisable()
    {
        if (_teamUpgradeData == null)
        {
            return;
        }

        _teamUpgradeData.TeamUpgradeChanged -= ApplyRuntimeValue;
    }

    /// <summary>
    /// 팀 업그레이드 성공 시 실제 런타임 값을 갱신한다.
    /// </summary>
    private void ApplyRuntimeValue(TeamUpgradeEntry entry, int level)
    {
        if (entry == null)
        {
            return;
        }

        switch (entry.StatKey)
        {
            case TeamUpgradeStatKey.MaxHealth:
                RefreshAllPlayerHealth();
                break;

            case TeamUpgradeStatKey.Stamina:
                RefreshAllPlayerStamina();
                break;

            case TeamUpgradeStatKey.Damage:
            case TeamUpgradeStatKey.AttackSpeed:
            case TeamUpgradeStatKey.MoveSpeed:
                DebugTool.Log(
                    $"[TeamUpgradeRuntimeApplier] {entry.StatKey}는 PlayerStats에서 즉시 계산됩니다.",
                    DebugType.Data,
                    this
                );
                break;

            default:
                DebugTool.Log(
                    $"[TeamUpgradeRuntimeApplier] {entry.StatKey}는 아직 런타임 갱신 대상이 아닙니다.",
                    DebugType.Data,
                    this
                );
                break;
        }
    }

    /// <summary>
    /// 모든 플레이어의 최대 체력을 갱신한다.
    /// </summary>
    private void RefreshAllPlayerHealth()
    {
        PlayerHealth[] playerHealths = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);

        foreach (PlayerHealth playerHealth in playerHealths)
        {
            if (playerHealth == null)
            {
                continue;
            }

            playerHealth.RefreshHealth();
        }

        DebugTool.Log(
            $"[TeamUpgradeRuntimeApplier] 모든 PlayerHealth 갱신 완료 / Count: {playerHealths.Length}",
            DebugType.Data,
            this
        );
    }

    /// <summary>
    /// 모든 플레이어의 최대 스테미나를 갱신한다.
    /// </summary>
    private void RefreshAllPlayerStamina()
    {
        PlayerStamina[] playerStaminas = FindObjectsByType<PlayerStamina>(FindObjectsSortMode.None);

        foreach (PlayerStamina playerStamina in playerStaminas)
        {
            if (playerStamina == null)
            {
                continue;
            }

            playerStamina.RefreshMaxStamina();
        }

        DebugTool.Log(
            $"[TeamUpgradeRuntimeApplier] 모든 PlayerStamina 갱신 완료 / Count: {playerStaminas.Length}",
            DebugType.Data,
            this
        );
    }
}