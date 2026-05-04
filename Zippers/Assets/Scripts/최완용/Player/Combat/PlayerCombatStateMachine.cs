using Audio;
using UnityEngine;

public class PlayerCombatStateMachine : MonoBehaviour
{
    [Header("전투 상태 설정")]
    [SerializeField] private float _attackStateTime = 0.12f;

    private StateMachine _stateMachine;

    private WeaponSFXController _weaponSfxController;
    private PlayerStats _playerStats;

    private PlayerAim _playerAim;
    private PlayerAnimation _playerAnimation;
    private PlayerReload _playerReload;

    private PlayerCombatNoneState _noneState;
    private PlayerCombatAimState _aimState;
    private PlayerCombatAttackState _attackState;
    private PlayerCombatReloadState _reloadState;
    

    private PlayerCombatStateType _currentStateType;

    public PlayerCombatStateType CurrentStateType => _currentStateType;
    public bool IsAiming => _playerAim != null && _playerAim.IsAiming;
    public bool IsReloading => _playerReload != null && _playerReload.IsReloading;

    private void Awake()
    {
        _stateMachine = new StateMachine();

        _playerAim = GetComponent<PlayerAim>();
        _playerAnimation = GetComponent<PlayerAnimation>();
        _playerReload = GetComponent<PlayerReload>();
        _weaponSfxController = GetComponentInChildren<WeaponSFXController>();
        _playerStats = GetComponent<PlayerStats>();

        _noneState = new PlayerCombatNoneState(this);
        _aimState = new PlayerCombatAimState(this);
        _attackState = new PlayerCombatAttackState(this, _playerAnimation, _attackStateTime, _weaponSfxController, _playerStats);
        _reloadState = new PlayerCombatReloadState(this, _playerAnimation, _playerReload, _weaponSfxController);
    }

    private void OnEnable()
    {
        if (_playerReload != null)
        {
            _playerReload.OnReloadCompleted += OnReloadCompleted;
        }
    }

    private void OnDisable()
    {
        if (_playerReload != null)
        {
            _playerReload.OnReloadCompleted -= OnReloadCompleted;
        }
    }

    private void Start()
    {
        ChangeState(PlayerCombatStateType.None);
    }

    private void Update()
    {
        _stateMachine.Update();
    }

    public void SetAiming(bool isAiming)
    {
        if (_playerAim == null)
        {
            Debug.LogWarning("[PlayerCombatStateMachine] PlayerAim이 없습니다.");
            return;
        }

        _playerAim.SetAiming(isAiming);

        if (_currentStateType == PlayerCombatStateType.Attack ||
            _currentStateType == PlayerCombatStateType.Reload)
        {
            return;
        }
        ChangeState(isAiming ? PlayerCombatStateType.Aim : PlayerCombatStateType.None);
    }

    public void RequestAttack()
    {
        if (_currentStateType == PlayerCombatStateType.Reload)
        {
            Debug.Log("[PlayerCombatStateMachine] 재장전 중이라 Attack 상태 진입 불가");
            return;
        }

        ChangeState(PlayerCombatStateType.Attack);
    }

    public void RequestReload()
    {
        if (_currentStateType == PlayerCombatStateType.Reload)
        {
            Debug.Log("[PlayerCombatStateMachine] 이미 Reload 상태입니다.");
            return;
        }

        ChangeState(PlayerCombatStateType.Reload);
    }

    public void ReturnCombatState()
    {
        ChangeState(IsAiming ? PlayerCombatStateType.Aim : PlayerCombatStateType.None);
    }

    public void ChangeState(PlayerCombatStateType stateType)
    {
        if (_currentStateType == stateType)
            return;

        _currentStateType = stateType;

        switch (stateType)
        {
            case PlayerCombatStateType.None:
                _stateMachine.ChangeState(_noneState);
                break;

            case PlayerCombatStateType.Aim:
                _stateMachine.ChangeState(_aimState);
                break;

            case PlayerCombatStateType.Attack:
                _stateMachine.ChangeState(_attackState);
                break;

            case PlayerCombatStateType.Reload:
                _stateMachine.ChangeState(_reloadState);
                break;
        }
    }

    private void OnReloadCompleted()
    {
        if (_currentStateType != PlayerCombatStateType.Reload)
            return;

        Debug.Log("[PlayerCombatStateMachine] 재장전 완료 이벤트 수신");
        ReturnCombatState();
    }
}
