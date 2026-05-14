using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LoadingPanel : MonoBehaviour
{
    [Header("로딩 이미지")]
    [SerializeField] private Sprite[] _loadingSprites = new Sprite[3];
    
    [Space(3)] [Header("배경 이미지")]
    [SerializeField] private Image _loadingImage;
    [Header("로딩 바 이미지")]
    [SerializeField] private Image _loadingBar;
    [Space(3)] [Header("로딩 텍스트")]
    [SerializeField] private TMP_Text _loadingText;
    [Header("로딩 텍스트 배경")]
    [SerializeField] private Image _loadingTextBackground;

    [NonSerialized] public int TotalProgrss;
    private int _loadingProgress = 0;
    
    private void Awake()
        => Init();

    private void Start()
        => ComponentInit();

    public void OnProceedLoading()
    {
        _loadingProgress++;
        _loadingBar.fillAmount = (float)_loadingProgress / TotalProgrss;
        _loadingText.text = "불러오는 중 ..." + $"{_loadingProgress} / {TotalProgrss}";
        
        if (_loadingProgress >= TotalProgrss)
            _loadingText.text = "불러오기 완료.";
    }

    public void PrintImage()
    {
        int random = Random.Range(0, _loadingSprites.Length);
        _loadingImage.sprite = _loadingSprites[random];
    }

    private void Init()
    {
        foreach (Sprite img in _loadingSprites)
            if (img == null)
                DebugTool.Warning("로딩 스프라이트가 없습니다.", DebugType.Missing);

        if (_loadingImage == null)
            DebugTool.Warning("로딩 배경 이미지 컴포넌트가 없습니다.", DebugType.Missing);
         
        if(_loadingBar == null)
            DebugTool.Warning("로딩 바 이미지 컴포넌트가 없습니다.", DebugType.Missing);
        
        if(_loadingText == null)
            DebugTool.Warning("로딩 텍스트 컴포넌트가 없습니다.", DebugType.Missing);
    }

    private void ComponentInit()
    {
        _loadingBar.fillAmount = 0f;
    }
}