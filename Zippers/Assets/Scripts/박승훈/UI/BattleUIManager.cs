using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Zippers.Network;
using Zippers.Network.Contracts;

public class BattleUIManager : MonoBehaviour
{
    [Header("UI 컨트롤러 컴포넌트")]
    [SerializeField] private StatusUiController _statusUiController;

    [SerializeField] private ResourcesUIController _resourcesUiController;
    [SerializeField] private WeaponUIController _weaponUiController;
    [SerializeField] private WaveUIController _waveUiController;

    [Header("네트워크 오브젝트")]
    [SerializeField] private NetworkObject localPlayerObject;

    [Header("네트워크 컴포넌트")]
    private IPlayerStatusReader _playerStatusReader;
    [SerializeField] private bool _hasStatusReader;
    private IPlayerStatProvider _playerStatProvider;
    [SerializeField] private bool _hasStatProvider;
    [SerializeField] private PlayerEconomyNetState economyNetState;
    [SerializeField] private bool _hasPlayerEconomy;
    [SerializeField] private bool _hasTeamEconomy;

    private Coroutine _initializeRoutine;

    private bool _isBound;

    private void Awake()
    {
        _isBound = false;
        UIComponentInit();
    }

    private void OnEnable()
    {
        if(_initializeRoutine == null && !_isBound)
            _initializeRoutine = StartCoroutine(SetupBattleUIRoutine());
    }

    private void OnDisable()
    {
        if (_initializeRoutine != null)
        {
            StopCoroutine(_initializeRoutine);
            _initializeRoutine = null;
        }
        
        RemoveListenerEvent();
    }

    private IEnumerator SetupBattleUIRoutine()
    {
        while (NetworkManager.Singleton == null ||
               NetworkManager.Singleton.LocalClient == null ||
               NetworkManager.Singleton.LocalClient.PlayerObject == null ||
               TeamEconomyNetState.Instance == null)
        {
            yield return null;
        }

        localPlayerObject = NetworkManager.Singleton.LocalClient.PlayerObject;

        yield return null;
        
        while(!TryCacheRequiredNetworkComponents())
            yield return null;
        
        AddListenerEvent();
        PlayerUiInit();

        _initializeRoutine = null;
    }

    private void AddListenerEvent()
    {
        if (_isBound)
            return;

        if (!IsNetworkComponentReady())
        {
            DebugTool.Error("인터페이스, 컴포넌트 활성화 여부" +
                            $"리더 인터페이스 : {_hasStatusReader} / 프로바이더 인터페이스 : {_hasStatProvider} " +
                            $"/ 플레이어 자원 : {_hasPlayerEconomy} / 팀자원 : {_hasTeamEconomy}" +
                            "UI를 초기화할 수 없습니다.", DebugType.UI);
            return;
        }
        
        _playerStatusReader.OnHealthChanged += OnPlayerHealthChanged;
        _playerStatusReader.OnStaminaChanged += OnPlayerStaminaChanged;
        _playerStatusReader.OnAmmoChanged += OnPlayerAmmoChanged;
        
        economyNetState.OnResourceChanged += OnResourcesChanged;
        TeamEconomyNetState.Instance.OnTeamResourceChanged += OnResourcesChanged;

        _isBound = true;
    }

    private void PlayerUiInit()
    {
        if (!IsNetworkComponentReady())
        {
            DebugTool.Error("인터페이스, 컴포넌트 활성화 여부" +
                            $"리더 인터페이스 : {_hasStatusReader} / 프로바이더 인터페이스 : {_hasStatProvider} " +
                            $"/ 플레이어 자원 : {_hasPlayerEconomy} / 팀자원 : {_hasTeamEconomy}" +
                            "UI를 초기화할 수 없습니다.", DebugType.UI);
            return;
        }
        
        _statusUiController?.OnHealthValueChanged(
            _playerStatusReader.CurrentHealth,
            _playerStatusReader.MaxHealth);
        
        _statusUiController?.OnStaminaValueChanged(
            _playerStatusReader.CurrentStamina, 
            _playerStatusReader.MaxStamina);
        
        _statusUiController?.SetClassType(_playerStatProvider.WeaponType);
        _weaponUiController?.CheckWeaponType(_playerStatProvider.WeaponType);
        
        _weaponUiController?.SetAmmoText(
            _playerStatusReader.CurrentAmmo,
            _playerStatusReader.MaxAmmo);
        
        _resourcesUiController?.SetResourceText(ResourcesType.Scrap,
            economyNetState.CurrentScrap, 0f);
        
        _resourcesUiController?.SetResourceText(ResourcesType.Supplies,
            TeamEconomyNetState.Instance.CurrentSupplies, 0f);
        
        _resourcesUiController?.SetResourceText(ResourcesType.InfectionSample,
            economyNetState.CurrentInfectionSample, 0f);
        
        _waveUiController?.WaveInit();
        
        SceneChangeController.Instance?.OnEnterScene();
    }
    
    private void RemoveListenerEvent()
    {
        if (!_isBound)
            return;
        
        if(_playerStatusReader != null)
        {
            _playerStatusReader.OnHealthChanged -= OnPlayerHealthChanged;
            _playerStatusReader.OnStaminaChanged -= OnPlayerStaminaChanged;
            _playerStatusReader.OnAmmoChanged -= OnPlayerAmmoChanged;
        }
        if(economyNetState != null)
            economyNetState.OnResourceChanged -= OnResourcesChanged;
        
        if(TeamEconomyNetState.Instance != null)
            TeamEconomyNetState.Instance.OnTeamResourceChanged -= OnResourcesChanged;
        
        _isBound = false;
    }
    
    /*private void OnPlayerHealthChanged()
    {
        // 체력 변경
        _statusUiController?.OnHealthValueChanged(
            _playerStatProvider.TotalMaxHealth,
            _playerStatusReader.CurrentHealth);
    }

    private void OnPlayerStaminaChanged()
    {
        // 스테미나 변경
        _statusUiController?.OnStaminaValueChanged(
            _playerStatProvider.TotalMaxStamina,
            _playerStatusReader.CurrentStamina);
    }*/
    
    private void OnPlayerHealthChanged(float current, float max)
    {
        // 체력 변경
        _statusUiController?.OnHealthValueChanged(current, max);
    }

    private void OnPlayerStaminaChanged(float current, float max)
    {
        // 스테미나 변경
        _statusUiController?.OnStaminaValueChanged(current, max);
    }

    private void OnPlayerAmmoChanged(float current, float max)
    {
        _weaponUiController?.SetAmmoText(current, max);
    }

    private void OnResourcesChanged(ResourcesType resourceType, float current, float delta)
    {
        _resourcesUiController?.SetResourceText(resourceType, current, delta);
    }

    private bool TryCacheRequiredNetworkComponents()
    {
        if (localPlayerObject == null)
            return false;

        _playerStatusReader = localPlayerObject.GetComponent<IPlayerStatusReader>();
        _hasStatusReader = _playerStatusReader != null;

        _playerStatProvider = localPlayerObject.GetComponent<IPlayerStatProvider>();
        _hasStatProvider = _playerStatProvider != null;

        economyNetState = localPlayerObject.GetComponent<PlayerEconomyNetState>();
        _hasPlayerEconomy = economyNetState != null;
        
        _hasTeamEconomy = TeamEconomyNetState.Instance != null;

        return _hasStatusReader && _hasStatProvider && _hasPlayerEconomy && _hasTeamEconomy;
    }

    private void UIComponentInit()
    {
        _statusUiController = GetComponentInChildren<StatusUiController>();
        _resourcesUiController = GetComponentInChildren<ResourcesUIController>();
        _weaponUiController = GetComponentInChildren<WeaponUIController>();
        _waveUiController = GetComponentInChildren<WaveUIController>();
    }

    private bool IsNetworkComponentReady()
        => _playerStatusReader != null && 
           _playerStatProvider != null && 
           economyNetState != null &&
           TeamEconomyNetState.Instance != null;
}