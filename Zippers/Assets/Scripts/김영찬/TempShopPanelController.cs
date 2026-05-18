using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Zippers.Network;

/// <summary>
/// [임시] ExchangeArea 진입 시 자동 표시되는 테스트용 UI.
/// 자원·성장 시스템 동작 검증 목적. 정식 UI 작업 전까지만 사용.
/// </summary>
public class TempShopPanelController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject _panelRoot;

    [Header("Buttons")]
    [SerializeField] private Button _personalMaxHealthBtn;
    [SerializeField] private Button _personalDamageBtn;
    [SerializeField] private Button _suppliesGrantBtn;
    [SerializeField] private Button _teamMaxHealthBtn;
    [SerializeField] private Button _teamBattleDamageBtn;

    [Header("Debug Labels")]
    [SerializeField] private TMP_Text _scrapLabel;
    [SerializeField] private TMP_Text _suppliesLabel;
    [SerializeField] private TMP_Text _battleStateLabel;

    [Header("Test Upgrade IDs (시트 보고 입력)")]
    [SerializeField] private int _testPersonalMaxHealthId = 50001;
    [SerializeField] private int _testPersonalDamageId = 50004;
    [SerializeField] private int _testTeamMaxHealthId = 51005;
    [SerializeField] private int _testTeamBattleDamageId = 51001;
    [SerializeField] private float _suppliesGrantAmount = 50f;

    private void Awake()
    {
        if (_panelRoot != null) _panelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        // ExchangeArea 트리거 이벤트 구독
        ShopExchangeAreaWatcher.OnLocalPlayerEntered += Show;
        ShopExchangeAreaWatcher.OnLocalPlayerExited += Hide;

        if (_personalMaxHealthBtn != null) _personalMaxHealthBtn.onClick.AddListener(OnPersonalMaxHealth);
        if (_personalDamageBtn != null) _personalDamageBtn.onClick.AddListener(OnPersonalDamage);
        if (_suppliesGrantBtn != null) _suppliesGrantBtn.onClick.AddListener(OnSuppliesGrant);
        if (_teamMaxHealthBtn != null) _teamMaxHealthBtn.onClick.AddListener(OnTeamMaxHealth);
        if (_teamBattleDamageBtn != null) _teamBattleDamageBtn.onClick.AddListener(OnTeamBattleDamage);
    }

    private void OnDisable()
    {
        ShopExchangeAreaWatcher.OnLocalPlayerEntered -= Show;
        ShopExchangeAreaWatcher.OnLocalPlayerExited -= Hide;

        if (_personalMaxHealthBtn != null) _personalMaxHealthBtn.onClick.RemoveListener(OnPersonalMaxHealth);
        if (_personalDamageBtn != null) _personalDamageBtn.onClick.RemoveListener(OnPersonalDamage);
        if (_suppliesGrantBtn != null) _suppliesGrantBtn.onClick.RemoveListener(OnSuppliesGrant);
        if (_teamMaxHealthBtn != null) _teamMaxHealthBtn.onClick.RemoveListener(OnTeamMaxHealth);
        if (_teamBattleDamageBtn != null) _teamBattleDamageBtn.onClick.RemoveListener(OnTeamBattleDamage);
    }

    private void Show()
    {
        if (_panelRoot != null) _panelRoot.SetActive(true);
        Debug.Log("[TempShop] ExchangeArea 진입 — UI 표시");
    }

    private void Hide()
    {
        if (_panelRoot != null) _panelRoot.SetActive(false);
        Debug.Log("[TempShop] ExchangeArea 이탈 — UI 숨김");
    }

    private void Update()
    {
        if (_panelRoot == null || !_panelRoot.activeSelf) return;
        RefreshLabels();
    }

    private void RefreshLabels()
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null) return;

        var localPlayer = nm.LocalClient?.PlayerObject;
        PlayerEconomyNetState myEco = localPlayer != null ? localPlayer.GetComponent<PlayerEconomyNetState>() : null;

        if (_scrapLabel != null)
        {
            _scrapLabel.text = myEco != null ? $"Scrap: {myEco.CurrentScrap}" : "Scrap: -";
        }

        if (_suppliesLabel != null)
        {
            _suppliesLabel.text = TeamEconomyNetState.Instance != null
                ? $"Supplies: {TeamEconomyNetState.Instance.CurrentSupplies}"
                : "Supplies: -";
        }

        if (_battleStateLabel != null)
        {
            _battleStateLabel.text = TeamBattleNetState.Instance != null
                ? $"BattleState: {TeamBattleNetState.Instance.CurrentState}"
                : "BattleState: -";
        }
    }

    // ── 버튼 핸들러 ─────────────────────────────────────────

    private void OnPersonalMaxHealth() => TryPersonalUpgrade(_testPersonalMaxHealthId);
    private void OnPersonalDamage() => TryPersonalUpgrade(_testPersonalDamageId);

    private void TryPersonalUpgrade(int upgradeId)
    {
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null) return;

        var localPlayer = nm.LocalClient?.PlayerObject;
        PlayerEconomyNetState myEco = localPlayer != null ? localPlayer.GetComponent<PlayerEconomyNetState>() : null;
        if (myEco == null)
        {
            Debug.LogWarning("[TempShop] 본인 PlayerEconomyNetState 못 찾음");
            return;
        }

        if (nm.IsServer)
        {
            myEco.ServerTryUpgradePersonal(upgradeId);
        }
        else if (myEco.IsOwner)
        {
            myEco.RequestUpgradePersonalServerRpc(upgradeId);
        }

        Debug.Log($"[TempShop] 개인 업그레이드 요청: id={upgradeId}");
    }

    private void OnSuppliesGrant()
    {
        if (TeamEconomyNetState.Instance == null) return;
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null) return;

        if (nm.IsServer)
        {
            TeamEconomyNetState.Instance.ServerGrantResource(0, ResourcesType.Supplies, _suppliesGrantAmount, "TempShop");
        }
        else
        {
            TeamEconomyNetState.Instance.RequestGrantSuppliesServerRpc(_suppliesGrantAmount);
        }

        Debug.Log($"[TempShop] Supplies +{_suppliesGrantAmount} 요청");
    }

    private void OnTeamMaxHealth() => TryTeamUpgrade(_testTeamMaxHealthId);
    private void OnTeamBattleDamage() => TryTeamUpgrade(_testTeamBattleDamageId);

    private void TryTeamUpgrade(int upgradeId)
    {
        if (TeamEconomyNetState.Instance == null) return;
        NetworkManager nm = NetworkManager.Singleton;
        if (nm == null) return;

        if (nm.IsServer)
        {
            TeamEconomyNetState.Instance.ServerTryUpgradeTeam(upgradeId);
        }
        else
        {
            TeamEconomyNetState.Instance.RequestUpgradeTeamServerRpc(upgradeId);
        }

        Debug.Log($"[TempShop] 팀 업그레이드 요청: id={upgradeId}");
    }
}