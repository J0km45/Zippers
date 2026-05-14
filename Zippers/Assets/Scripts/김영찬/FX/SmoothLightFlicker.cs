using UnityEngine;

public class SmoothLightFlicker : MonoBehaviour
{
    private enum LightState { On, Off, Flicker }
    
    [Header("조명 연결")]
    [SerializeField] private Light _spotLight;
    [SerializeField] private Light _pointLight;

    [Header("깜빡임 공통 설정")]
    [SerializeField] private float _flickerSpeed;
    
    [Header("상태 유지 시간 설정 (초)")]
    [SerializeField] private float _minStateDuration = 10.0f;
    [SerializeField] private float _maxStateDuration = 60.0f;

    [Header("SpotLight 밝기 설정")] 
    [SerializeField] private float _spotNormalIntensity;
    [SerializeField] private float _spotMinIntensity;
    [SerializeField] private float _spotMaxIntensity;

    [Header("PointLight 밝기 설정")]
    [SerializeField] private float _pointNormalIntensity;
    [SerializeField] private float _pointMinIntensity;
    [SerializeField] private float _pointMaxIntensity;

    private float _randomOffset;
    private LightState _currentState;
    private float _stateTimer;
    private System.Random _sysRandom;

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        _stateTimer -= Time.deltaTime;
        
        if (_stateTimer <= 0)
        {
            ChangeToRandomState();
        }
        
        switch (_currentState)
        {
            case LightState.On:
                TurnOn();
                break;
            case LightState.Off:
                TurnOff();
                break;
            case LightState.Flicker:
                Flicker();
                break;
        }
    }

    private void Init()
    {
        _randomOffset = transform.position.x + transform.position.y;
        _sysRandom = new System.Random(); 
        ChangeToRandomState();
    }
    
    private void ChangeToRandomState()
    {
        int randomStateIndex = _sysRandom.Next(0, 3);
        _currentState = (LightState)randomStateIndex;
        
        float randomDuration = (float)_sysRandom.NextDouble();
        _stateTimer = Mathf.Lerp(_minStateDuration, _maxStateDuration, randomDuration);
    }

    private void TurnOn()
    {
        if (_spotLight != null) _spotLight.intensity = _spotNormalIntensity;
        if (_pointLight != null) _pointLight.intensity = _pointNormalIntensity;
    }

    private void TurnOff()
    {
        if (_spotLight != null) _spotLight.intensity = 0f;
        if (_pointLight != null) _pointLight.intensity = 0f;
    }
    
    private void Flicker()
    {
        float noise = Mathf.PerlinNoise(Time.time * _flickerSpeed, _randomOffset);
        
        if (_spotLight != null)
        {
            _spotLight.intensity = Mathf.Lerp(_spotMinIntensity, _spotMaxIntensity, noise);
        }

        if (_pointLight != null)
        {
            _pointLight.intensity = Mathf.Lerp(_pointMinIntensity, _pointMaxIntensity, noise);
        }
    }
}