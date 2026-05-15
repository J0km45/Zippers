using System;
using System.Collections;
using TMPro;
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
    [SerializeField] private bool IReader;
    private IPlayerStatusReader _playerStatusReader;
    [SerializeField] private bool IProvider;
    private IPlayerStatProvider _playerStatProvider;
    [SerializeField] private PlayerEconomyNetState economyNetState;

    private void Awake()
    {
        UIComponentInit();
        StartCoroutine(WaitForNetworkObject());
    }

    private void OnEnable()
    {
        StartCoroutine(AddListenerEvent());
        StartCoroutine(PlayerUiInit());
    }

    private void OnDisable()
    {
        RemoveListenerEvent();
    }

    private IEnumerator WaitForNetworkObject()
    {
        while (NetworkManager.Singleton == null ||
               NetworkManager.Singleton.LocalClient == null ||
               NetworkManager.Singleton.LocalClient.PlayerObject == null)
        {
            yield return null;
        }

        localPlayerObject = NetworkManager.Singleton.LocalClient.PlayerObject;

        yield return null;

        GetNetworkComponent(localPlayerObject);

        SceneChangeController.Instance.OnEnterScene();
    }

    private IEnumerator AddListenerEvent()
    {
        while (_playerStatusReader == null ||
               _playerStatProvider == null ||
               economyNetState == null)
            yield return null;
        
        _statusUiController?.SetClassSprite(_playerStatProvider.WeaponType);
        _weaponUiController?.CheckClass(_playerStatProvider.WeaponType);
        
        _playerStatusReader.OnHealthChanged += OnPlayerHealthChanged;
        _playerStatusReader.OnStaminaChanged += OnPlayerStaminaChanged;
        _playerStatusReader.OnAmmoChanged += OnPlayerAmmoChanged;
        economyNetState.OnResourceChanged += OnResourcesChanged;
    }

    private IEnumerator PlayerUiInit()
    {
        while (_playerStatusReader == null ||
               _playerStatProvider == null ||
               economyNetState == null)
        {
            yield return null;
        }

        _statusUiController?.OnHealthValueChanged(
            _playerStatusReader.CurrentHealth,
            _playerStatProvider.TotalMaxHealth);
        DebugTool.Log($"{_playerStatusReader.CurrentHealth} / {_playerStatProvider.TotalMaxHealth}", DebugType.UI, this);
        
        _statusUiController?.OnStaminaValueChanged(
            _playerStatusReader.CurrentStamina, 
            _playerStatProvider.TotalMaxStamina);
        
        _weaponUiController?.SetAmmoText(
            _playerStatusReader.CurrentAmmo,
            _playerStatProvider.TotalMagazineCapacity);
        
        _resourcesUiController?.ResourcesInit();
    }

    private void RemoveListenerEvent()
    {
        if(_playerStatusReader != null)
        {
            _playerStatusReader.OnHealthChanged -= OnPlayerHealthChanged;
            _playerStatusReader.OnStaminaChanged -= OnPlayerStaminaChanged;
            _playerStatusReader.OnAmmoChanged -= OnPlayerAmmoChanged;
        }
        if(economyNetState != null)
            economyNetState.OnResourceChanged -= OnResourcesChanged;
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

    private void GetNetworkComponent(NetworkObject netObj)
    {
        _playerStatusReader = netObj.GetComponent<IPlayerStatusReader>();
        if (_playerStatusReader != null)
            IReader = true;
        _playerStatProvider = netObj.GetComponent<IPlayerStatProvider>();
        if (_playerStatProvider != null)
            IProvider = true;
        economyNetState = netObj.GetComponent<PlayerEconomyNetState>();
    }

    private void UIComponentInit()
    {
        _statusUiController = GetComponentInChildren<StatusUiController>();
        _resourcesUiController = GetComponentInChildren<ResourcesUIController>();
        _weaponUiController = GetComponentInChildren<WeaponUIController>();
        _waveUiController = GetComponentInChildren<WaveUIController>();
    }
}