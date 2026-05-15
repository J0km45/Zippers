using System;
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
    private TMP_Text _meleeText;

    public void SetAmmoText(float currentAmmo, float maxAmmo)
    {
        _maxAmmo = (int)maxAmmo;
        _currentAmmo = (int)currentAmmo;
        _maxAmmoText.text = maxAmmo.ToString();
        _currentAmmoText.text = currentAmmo.ToString();
    }

    public void CheckClass(WeaponType type)
    {
        if (type == WeaponType.Melee)
        {
            _maxAmmoText.text = "";
            _currentAmmoText.text = "";
            _meleeText.text = "근접 무기";
        }
    }
}
