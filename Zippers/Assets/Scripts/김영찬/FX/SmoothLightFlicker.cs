using UnityEngine;

public class SmoothLightFlicker : MonoBehaviour
{
    [Header("조명 연결")]
    [SerializeField] private Light _spotLight;  // 스포트라이트 연결
    [SerializeField] private Light _pointLight; // 포인트라이트 연결

    [Header("깜빡임 공통 설정")]
    [SerializeField] private float _flickerSpeed = 10.0f; // 깜빡이는 속도

    [Header("SpotLight 밝기 설정")]
    [SerializeField] private float _spotMinIntensity = 3.0f;
    [SerializeField] private float _spotMaxIntensity = 28.0f;

    [Header("PointLight 밝기 설정")]
    [SerializeField] private float _pointMinIntensity = 0;
    [SerializeField] private float _pointMaxIntensity = 6.0f;

    private float _randomOffset;

    private void Awake()
    {
        // 맵에 여러 전등이 있을 때 서로 다르게 깜빡이도록 고유 오프셋 생성
        _randomOffset = transform.position.x + transform.position.y; 
    }

    private void Update()
    {
        // 1. 이번 프레임의 깜빡임 정도(0.0 ~ 1.0)를 딱 한 번만 계산합니다!
        float noise = Mathf.PerlinNoise(Time.time * _flickerSpeed, _randomOffset);
        
        // 2. 계산된 똑같은 노이즈 값을 두 조명에 동시에 적용합니다.
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