using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusUiController : MonoBehaviour
{
    [Header("클래스 아이콘")]
    [SerializeField] private WeaponType _weaponType;

    [SerializeField] private Sprite[] _classSprites = new Sprite[4];

    [Header("플레이어 스테이터스")]
    [Header("최대 체력")]
    [SerializeField] private float _maxHeatlh = 100;

    [Header("현재 체력")]
    [SerializeField] private float _currentHeatlh;
    
    [Header("최대 스테미나")]
    [SerializeField] private float _maxStamina = 50;
    
    [Header("현재 체력")]
    [SerializeField] private float _currentStamina;

    [Header("UI 컴포넌트")]
    [Header("클래스 아이콘")]
    [SerializeField] private Image _classIcon;
    [SerializeField] private TMP_Text _className;
    [Space(5)] [Header("체력")]
    [Header("체력 바")]
    [SerializeField] private Image _hpBar;
    [Header("체력 텍스트")]
    [SerializeField] private TMP_Text _healthText;
    [Space(5)] [Header("스테미나")]
    [Header("스테미나 바")]
    [SerializeField] private Image _staminaBar;
    [Header("스테미나 텍스트")]
    [SerializeField] private TMP_Text _staminaText;

    public void OnHealthValueChanged( float currentHealth,float maxHealth)
    {
        _currentHeatlh = currentHealth;
        _maxHeatlh = maxHealth;
        
        string text = $"{(int)_currentHeatlh} / {(int)_maxHeatlh}";
        _healthText.text = text;
        
        _hpBar.fillAmount = _currentHeatlh / _maxHeatlh;
    }

    public void OnStaminaValueChanged(float currentStamina, float maxStamina)
    {
        _currentStamina = currentStamina;
        _maxStamina= maxStamina;
        
        string text = $"{(int)_currentStamina} / {(int)_maxStamina}";
        _staminaText.text = text;
        
        _staminaBar.fillAmount = _currentStamina / _maxStamina;
    }

    public void SetClassSprite(WeaponType type)
    {
        int index = (int)type;
        _classIcon.sprite = _classSprites[index];
    }
}