using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusController : MonoBehaviour
{
    [Header("클래스 아이콘")]
    [SerializeField] private int _classId;

    [SerializeField] private Sprite[] _classSprites = new Sprite[4];

    [Header("플레이어 스테이터스")]
    [Header("최대 체력")]
    [SerializeField] private float _maxHeatlh = 100;
    public float MaxHealth => _maxHeatlh;
    
    [Header("현재 체력")]
    [SerializeField] private float _currentHeatlh;
    public float CurrentHealth =>  _currentHeatlh;
    
    [Header("최대 스테미나")]
    [SerializeField] private float _maxStamina = 50;
    public float MaxStamina => _maxStamina;
    
    [Header("현재 체력")]
    [SerializeField] private float _currentStamina;
    public float CurrentStamina => _currentStamina;

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

    public event Action<float, float> OnPlayerHealthChanged;
    public event Action<float, float> OnPlayerStaminaChanged;

    private void OnEnable()
    {
        OnPlayerHealthChanged += SetHealthValueText;
        OnPlayerHealthChanged += SetHealthBar;
        OnPlayerStaminaChanged += SetStaminaValueText;
        OnPlayerStaminaChanged += SetStaminaBar;
    }

    private void Start()
    {
        StatusInit();
        GetClassSprite(_classId);
    }

    private void OnDisable()
    {
        OnPlayerHealthChanged -= SetHealthValueText;
        OnPlayerHealthChanged -= SetHealthBar;
        OnPlayerStaminaChanged -= SetStaminaValueText;
        OnPlayerStaminaChanged -= SetStaminaBar;
    }

    public void SetMaxHealth(float value)
    {
        _maxHeatlh = value;
        OnPlayerHealthChanged?.Invoke(_maxHeatlh, _currentHeatlh);
    }

    public void SetCurrentHealth(float value)
    {
        _currentHeatlh = value;
        OnPlayerHealthChanged?.Invoke(_maxHeatlh, _currentHeatlh);
    }

    public void SetMaxStamina(float value)
    {
        _maxStamina = value;
        OnPlayerStaminaChanged?.Invoke(_maxStamina, _currentStamina);
    }

    public void SetCurrentStamina(float value)
    {
        _currentStamina = value;
        OnPlayerStaminaChanged?.Invoke(_maxStamina, _currentStamina);
    }

    private void SetHealthValueText(float maxHealth, float currentHealth)
    {
        string text = $"{currentHealth} / {maxHealth}";
        _healthText.text = text;
    }

    private void SetHealthBar(float maxHealth, float currentHealth)
    {
        _hpBar.fillAmount = currentHealth / maxHealth;
    }

    private void SetStaminaValueText(float maxStamina, float currentStamina)
    {
        string text = $"{currentStamina} / {maxStamina}";
        _staminaText.text = text;
    }

    private void SetStaminaBar(float maxStamina, float currentStamina)
    {
        _staminaBar.fillAmount = currentStamina / maxStamina;
    }

    private void GetClassSprite(int classId)
    {
        int index = classId - 10001;
        _classIcon.sprite = _classSprites[index];
    }

    private void StatusInit()
    {
        _currentHeatlh = _maxHeatlh;
        _currentStamina = _maxStamina;
        
        OnPlayerHealthChanged?.Invoke(_maxHeatlh, _currentHeatlh);
        OnPlayerStaminaChanged?.Invoke(_maxStamina, _currentStamina);
    }
}