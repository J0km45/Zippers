using System;
using TMPro;
using UnityEngine;

public class WeaponUIController : MonoBehaviour
{
    [Header("무기 정보")]
    [SerializeField] private int _maxAmmo;
    public int MaxAmmo => _maxAmmo;
    [SerializeField] private int _currentAmmo;
    public int CurrentAmmo => _currentAmmo;

    [Space(6)] [Header("UI 컴포넌트")] [Header("최대 탄환 수")] [SerializeField]
    private TMP_Text _maxAmmoText;
    [Header("현재 탄환 수")] [SerializeField]
    private TMP_Text _currentAmmoText;
    [Header("근접 전환 텍스트")] [SerializeField]
    private TMP_Text _meleeText;
    
    public event Action<int> OnMaxAmmoChange;
    public event Action<int> OnCurrentAmmoChange;

    private void OnEnable()
    {
        OnMaxAmmoChange += SetMaxAmmoText;
        OnCurrentAmmoChange += SetCurrentAmmoText;
    }

    private void Start()
        => AmmoInit();

    private void OnDisable()
    {
        OnMaxAmmoChange -= SetMaxAmmo;
        OnCurrentAmmoChange -= SetCurrentAmmo;
    }

    public void SetMaxAmmo(int maxAmmo)
    {
        _maxAmmo = maxAmmo;
        OnMaxAmmoChange?.Invoke(maxAmmo);
    }

    public void SetCurrentAmmo(int currentAmmo)
    {
        _currentAmmo = currentAmmo;
        OnCurrentAmmoChange?.Invoke(currentAmmo);
    }

    private void SetMaxAmmoText(int currentAmmo)
        => _maxAmmoText.text = currentAmmo.ToString();

    private void SetCurrentAmmoText(int currentAmmo)
        => _currentAmmoText.text = currentAmmo.ToString();

    public void CheckClass(WeaponType type)
    {
        if (type == WeaponType.Melee)
        {
            _maxAmmoText.text = "";
            _currentAmmoText.text = "";
            _meleeText.text = "근접 무기";
        }
    }

    private void AmmoInit()
    {
        _currentAmmo = _maxAmmo;
        
        OnMaxAmmoChange?.Invoke(_maxAmmo);
        OnCurrentAmmoChange?.Invoke(_currentAmmo);
    }
}
