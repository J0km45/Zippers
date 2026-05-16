using TMPro;
using UnityEngine;

public class ResourcesUIController : MonoBehaviour
{
    [Header("보유 자원")] [Header("고철")] [SerializeField]
    private int _scrap;
    [Header("보급품")] [SerializeField]
    private int _supplies;
    [Header("감염 샘플")] [SerializeField]
    private int _infectionSample;
    
    [Space(6)] [Header("UI 컴포넌트")]
    [SerializeField] private TMP_Text _scrapText;
    [SerializeField] private TMP_Text _suppliesText;
    [SerializeField] private TMP_Text _infectionSampleText;

    public void SetResourceText(ResourcesType resourceType, float current, float delta)
    {
        switch (resourceType)
        {
            case ResourcesType.Scrap:
                _scrap = (int)current;
                if (!IsBoundComponent(_scrapText))
                    return;
                _scrapText.text = $"{(int)current}";
                break;
            case ResourcesType.Supplies:
                _supplies = (int)current;
                if (!IsBoundComponent(_suppliesText))
                    return;
                _suppliesText.text = $"{(int)current}";
                break;
            case ResourcesType.InfectionSample:
                _infectionSample = (int)current;
                if (!IsBoundComponent(_infectionSampleText))
                    return;
                _infectionSampleText.text = $"{(int)current}";
                break;
            default: return;
        }
    }

    private bool IsBoundComponent(TMP_Text text)
    {
        if (text != null)
            return true;
        
        DebugTool.Error("자원 텍스트 컴포넌트가 업습니다.", DebugType.UI, this);
        return false;
    }
}