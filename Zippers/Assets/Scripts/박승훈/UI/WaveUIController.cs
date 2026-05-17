using Audio;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveUIController : MonoBehaviour
{
    [Header("UI 오디오 컴포넌트")]
    [SerializeField] private UIController _uiController;

    [Header("현재 노드 좀비 수")]
    [SerializeField] private ZombieCountManager _zombieCountManager;
    [SerializeField] private WaveManager _waveManager;
    
    [Header("웨이브 정보")]
    [SerializeField] private int _maxWave;
    [SerializeField] private int _currentWave;
    [SerializeField] private int _leftZombieCount;

    [SerializeField][Range(0f, 1f)] private float _battleNoticeTime = 0.5f;
    [SerializeField][Range(0.1f, 5f)] private float _NoticeFadeTime = 1f;
    private Color color = new Color(1f, 1f, 1f, 0f);
    [SerializeField] private float _scaleNoticeImage = 0.5f;
    
    [Header("UI 컴포넌트")]
    [SerializeField] private TMP_Text _maxWaveText;
    [SerializeField] private TMP_Text _currentWaveText;
    [SerializeField] private TMP_Text _leftZombieCountText;

    [SerializeField] private Image _battleNoticeImage;

    [Header("스프라이트")]
    [SerializeField] private Sprite _battleStartSprite;
    [SerializeField] private Sprite _battleClearSprite;
    [SerializeField] private Sprite _nextWaveSprite;
    
    private void OnEnable()
    {
        _zombieCountManager.OnZombieCountChanged += SetLeftZombieCount;
        _waveManager.OnBattleStarted += BattleStart;
        _waveManager.OnBattleNodeCleared += BattleClear;
        _waveManager.OnNextWave += NextWave;
    }

    private void OnDisable()
    {
        _zombieCountManager.OnZombieCountChanged -= SetLeftZombieCount;
        _waveManager.OnBattleStarted -= BattleStart;
        _waveManager.OnBattleNodeCleared -= BattleClear;
        _waveManager.OnNextWave -= NextWave;
    }

    public void SetMaxWaveText(int maxWave)
    {
        _maxWave = maxWave;
        _maxWaveText.text = maxWave.ToString();
    }

    public void SetCurrentWaveText(int currentWave)
    {
        _currentWave = currentWave;
        _currentWaveText.text = currentWave.ToString();
    }

    public void SetLeftZombieCount(int leftZombieCount)
    {
        _leftZombieCount = leftZombieCount;
        _leftZombieCountText.text = leftZombieCount.ToString();
    }

    private void BattleStart()
    {
        _battleNoticeImage.sprite = _battleStartSprite;
        _battleNoticeImage.SetNativeSize();
        SetImageSize();
        NoticeBattleStatus();
        _uiController.PlayBattleStart();
    }

    private void BattleClear(int a)
    {
        _battleNoticeImage.sprite = _battleClearSprite;
        _battleNoticeImage.SetNativeSize();
        SetImageSize();
        NoticeBattleStatus();
        _uiController.PlayBattleClear();
    }

    private void NextWave()
    {
        _battleNoticeImage.sprite = _nextWaveSprite;
        _battleNoticeImage.SetNativeSize();
        SetImageSize();
        NoticeBattleStatus();
        _uiController.PlayNextWave();
    }

    private void NoticeBattleStatus()
        => StartCoroutine(ImageFadeInOut());

    private IEnumerator ImageFadeInOut()
    {
        _battleNoticeImage.gameObject.SetActive(true);
        
        float time = 0;
        while (time < _NoticeFadeTime)
        {
            time += Time.deltaTime;
            float t = time / _NoticeFadeTime;
            
            color.a = Mathf.Lerp(0f, 1f, t);
            _battleNoticeImage.color = color;
            
            yield return null;
        }
        
        yield return YieldContainer.Seconds(_battleNoticeTime);
        
        time = 0;
        while (time < _NoticeFadeTime)
        {
            time += Time.deltaTime;
            float t = time / _NoticeFadeTime;
            
            color.a = Mathf.Lerp(1f, 0f, t);
            _battleNoticeImage.color = color;
            
            yield return null;
        }

        _battleNoticeImage.sprite = null;
        _battleNoticeImage.gameObject.SetActive(false);
    } 

    public void WaveInit()
    {
        SetMaxWaveText(_maxWave);
        SetCurrentWaveText(_currentWave);
        SetLeftZombieCount(_leftZombieCount);

        _battleNoticeImage.gameObject.SetActive(false);
        _battleNoticeImage.color = color;
    }

    public void SetImageSize()
    {
        _battleNoticeImage.SetNativeSize();
        _battleNoticeImage.rectTransform.localScale =
            new Vector3(_scaleNoticeImage, _scaleNoticeImage, _scaleNoticeImage);
    }
}