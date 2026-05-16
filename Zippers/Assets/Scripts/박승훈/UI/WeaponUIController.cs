using TMPro;
using UnityEngine;

public class WeaponUIController : MonoBehaviour
{
    [Header("무기 정보")]
    [SerializeField] private int _maxAmmo;
    [SerializeField] private int _currentAmmo;

    [Space(6)] [Header("UI 컴포넌트")] [Header("최대 탄환 수")] [SerializeField]
    private TMP_Text _maxAmmoText;
    [Header("현재 탄환 수")] [SerializeField]
    private TMP_Text _currentAmmoText;
    [Header("근접 전환 텍스트")] [SerializeField]
    private TMP_Text _weaponStatusText;
    
    [SerializeField] private bool _isRanged;

    public void SetAmmoText(float currentAmmo, float maxAmmo)
    {
        int safeMaxAmmo = Mathf.Max(0, (int)maxAmmo);
        int safeCurrentAmmo = Mathf.Clamp((int)currentAmmo, 0, safeMaxAmmo);

        _maxAmmo = safeMaxAmmo;
        _currentAmmo = safeCurrentAmmo;

        if (_currentAmmoText != null)
            _currentAmmoText.text = $"{_currentAmmo}";

        if (_maxAmmoText != null)
            _maxAmmoText.text = $"{_maxAmmo}";
    }

    public void CheckWeaponType(WeaponType type)
    {
        _isRanged = (type != WeaponType.Melee);
        
        _maxAmmoText.gameObject.SetActive(_isRanged);
        _currentAmmoText.gameObject.SetActive(_isRanged);
        
        if(!_isRanged)
            _weaponStatusText.text = "근접 무기";
        else
            _weaponStatusText.text = "/";
    }
}
