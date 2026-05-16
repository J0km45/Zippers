using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusUiController : MonoBehaviour
{
    [SerializeField] private Sprite[] _classSprites = new Sprite[4];

    [Header("플레이어 스테이터스")] [Header("최대 체력")] [SerializeField]
    private float _maxHealth = 100;

    [Header("현재 체력")] [SerializeField] private float _currentHealth;

    [Header("최대 스테미나")] [SerializeField] private float _maxStamina = 50;

    [Header("현재 스테미나")] [SerializeField] private float _currentStamina;

    [Header("UI 컴포넌트")] [Header("클래스 아이콘")] [SerializeField]
    private Image _classIcon;

    [Space(5)] [Header("체력")] [Header("체력 바")] [SerializeField]
    private Image _hpBar;

    [Header("체력 텍스트")] [SerializeField] private TMP_Text _healthText;

    [Space(5)] [Header("스테미나")] [Header("스테미나 바")] [SerializeField]
    private Image _staminaBar;

    [Header("스테미나 텍스트")] [SerializeField] private TMP_Text _staminaText;

    public void OnHealthValueChanged(float currentHealth, float maxHealth)
    {
        _currentHealth = currentHealth;
        _maxHealth = maxHealth;

        SetText(_healthText, _currentHealth, _maxHealth);
        SetBar(_hpBar, _currentHealth, _maxHealth);
    }

    public void OnStaminaValueChanged(float currentStamina, float maxStamina)
    {
        _currentStamina = currentStamina;
        _maxStamina = maxStamina;
        
        SetText(_staminaText, _currentStamina, _maxStamina);
        SetBar(_staminaBar, _currentStamina, _maxStamina);
    }

    private void SetText(TMP_Text text, float current, float max)
    {
        if (text == null)
            return;

        if (max <= 0f)
        {
            text.text = "0 / 0";
            return;
        }

        float safeCurrent = Mathf.Clamp(current, 0f, max);
        text.text = $"{(int)safeCurrent} / {(int)max}";
    }

    private void SetBar(Image image, float current, float max)
    {
        if (image == null)
            return;

        if (max <= 0f)
        {
            image.fillAmount = 0f;
            return;
        }

        float safeCurrent = Mathf.Clamp(current, 0f, max);
        image.fillAmount = Mathf.Clamp01(safeCurrent / max);
    }

    public void SetClassSprite(WeaponType type)
    {
        if (_classIcon == null)
        {
            DebugTool.Warning("클래스 아이콘 이미지 컴포넌트가 없습니다.", DebugType.UI, this);
            return;
        }
        
        foreach (Sprite sprite in _classSprites)
        {
            if (sprite == null)
            {
                _classIcon.sprite = null;
                DebugTool.Warning("클래스 아이콘이 비어있습니다. 총 4개 필요", DebugType.UI, this);
                return;
            }
        }
        int index = (int)type;
        if (index < 0 || index >= _classSprites.Length)
        {
            DebugTool.Warning("범위를 벗어난 값입니다. (Battle UI 클래스 아이콘)", DebugType.UI, this);
            _classIcon.sprite = null;
            return;
        }
        
        _classIcon.sprite = _classSprites[index];
    }
}