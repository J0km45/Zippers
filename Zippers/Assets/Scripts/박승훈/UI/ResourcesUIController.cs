using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourcesUIController : MonoBehaviour
{
    [Header("보유 자원")] [Header("고철")] [SerializeField]
    private int _scrap;
    public int Scrap => _scrap;
    [Header("보급품")] [SerializeField]
    private int _supplies;
    public int Supplies => _supplies;
    [Header("감염 샘플")] [SerializeField]
    private int _infectionSample;
    public int InfectionSample => _infectionSample;
    
    [Space(6)] [Header("UI 컴포넌트")]
    [Header("스크랩 텍스트")]
    [SerializeField] private TMP_Text _scrapText;
    [Header("보급품 텍스트")]
    [SerializeField] private TMP_Text _suppliesText;
    [Header("감염 샘플 텍스트")]
    [SerializeField] private TMP_Text _infectionSampleText;
    
    public event Action<int> OnScrapChanged;
    public event Action<int> OnSuppliesChanged;
    public event Action<int> OnInfectionSampleChanged;

    private void OnEnable()
    {
        OnScrapChanged += SetScrapText;
        OnSuppliesChanged += SetSuppliesText;
        OnInfectionSampleChanged += SetInfectionSampleText;
    }

    private void Start()
        => ResourcesInit();

    private void OnDisable()
    {
        OnScrapChanged -= SetScrapText;
        OnSuppliesChanged -= SetSuppliesText;
        OnInfectionSampleChanged -= SetInfectionSampleText;
    }

    public void SetScrap(int value)
    {
        _scrap = value;
        OnScrapChanged?.Invoke(value);
    }
    
    public void SetSupplies(int value)
    {
        _supplies = value;
        OnSuppliesChanged?.Invoke(value);
    }
    
    public void SetInfectionSample(int value)
    {
        _infectionSample = value;
        OnInfectionSampleChanged?.Invoke(value);
    }
    

    public void SetScrapText(int value)
        => _scrapText.text = $"{value}";

    public void SetSuppliesText(int value)
        => _suppliesText.text = $"{value}";

    public void SetInfectionSampleText(int value)
        => _infectionSampleText.text = $"{value}";

    private void ResourcesInit()
    {
        OnScrapChanged?.Invoke(_scrap);
        OnSuppliesChanged?.Invoke(_supplies);
        OnInfectionSampleChanged?.Invoke(_infectionSample);
    }
}