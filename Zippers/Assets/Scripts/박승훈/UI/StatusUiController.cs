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
    
    private void Start()
    {
        StatusInit();
    }

    public void OnHealthValueChanged(float maxHealth, float currentHealth)
    {
        string text = $"{currentHealth} / {maxHealth}";
        _healthText.text = text;
        _hpBar.fillAmount = currentHealth / maxHealth;
    }

    public void OnStaminaValueChanged(float maxStamina, float currentStamina)
    {
        string text = $"{currentStamina} / {maxStamina}";
        _staminaText.text = text;
        _staminaBar.fillAmount = currentStamina / maxStamina;
    }

    public void SetClassSprite(WeaponType type)
    {
        int index = (int)type;
        _classIcon.sprite = _classSprites[index];
    }

    private void StatusInit()
    {
        _currentHeatlh = _maxHeatlh;
        _currentStamina = _maxStamina;
    }
}