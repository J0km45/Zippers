using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourcesUIController : MonoBehaviour
{
    [Header("보유 자원")] [Header("고철")] [SerializeField]
    private int _scrap;
    [Header("보급품")] [SerializeField]
    private int _supplies;
    [Header("감염 샘플")] [SerializeField]
    private int _infectionSample;
    
    [Space(6)] [Header("UI 컴포넌트")]
    [Header("스크랩 텍스트")]
    [SerializeField] private TMP_Text[] Text;
   
    public void SetResourceText(ResourcesType resourceType, float current, float delta)
        => Text[(int)resourceType].text = $"{current}";

    public void ResourcesInit()
    {
        SetResourceText(ResourcesType.Scrap, 0, 0);
        SetResourceText(ResourcesType.Supplies, 0, 0);
        SetResourceText(ResourcesType.InfectionSample, 0, 0);
    }
}