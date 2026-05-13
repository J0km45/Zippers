using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ObstacleFadeTarget : MonoBehaviour
{
    [Header("투명화 대상 Renderer")]
    [SerializeField] private Renderer[] _fadeRenderers;

    [Header("디버그")]
    [SerializeField] private bool _showDebugLog = false;

    private readonly List<Material> _materials = new();

    private float _currentAlpha = 1f;
    private float _targetAlpha = 1f;

    private bool _isInitialized;
    private bool _isTransparentMode;

    private const float OpaqueAlpha = 1f;

    private void Awake()
    {
        Init();
    }

    private void Reset()
    {
        // 처음 컴포넌트를 붙였을 때 자식 Renderer 자동 수집
        _fadeRenderers = GetComponentsInChildren<Renderer>();
    }

    private void Init()
    {
        if (_isInitialized)
            return;

        if (_fadeRenderers == null || _fadeRenderers.Length == 0)
        {
            _fadeRenderers = GetComponentsInChildren<Renderer>();
        }

        _materials.Clear();

        foreach (Renderer fadeRenderer in _fadeRenderers)
        {
            if (fadeRenderer == null)
                continue;

            Material[] rendererMaterials = fadeRenderer.materials;

            foreach (Material material in rendererMaterials)
            {
                if (material == null)
                    continue;

                _materials.Add(material);
            }
        }

        _currentAlpha = OpaqueAlpha;
        _targetAlpha = OpaqueAlpha;
        _isInitialized = true;

        if (_showDebugLog)
        {
            DebugTool.Log($"[ObstacleFadeTarget] 초기화 완료 / Material 수: {_materials.Count}", DebugType.Data, this);
        }
    }

    //투명화 요청 함수
    public void SetFade(bool isFaded, float fadeAlpha)
    {
        Init();

        float nextAlpha = isFaded ? fadeAlpha : OpaqueAlpha;

        // 이미 같은 목표 Alpha면 중복 처리하지 않음
        if (Mathf.Approximately(_targetAlpha, nextAlpha))
            return;

        _targetAlpha = nextAlpha;

        if (isFaded)
        {
            SetTransparentMode();
        }

        if (_showDebugLog)
        {
            Debug.Log($"[ObstacleFadeTarget] 목표 Alpha 변경: {_targetAlpha}", this);
        }
    }


    public void UpdateFade(float fadeSpeed)
    {
        if (!_isInitialized)
            return;

        if (Mathf.Approximately(_currentAlpha, _targetAlpha))
            return;

        _currentAlpha = Mathf.MoveTowards(
            _currentAlpha,
            _targetAlpha,
            fadeSpeed * Time.deltaTime
        );

        ApplyAlpha(_currentAlpha);

        // 완전히 원래 Alpha로 돌아왔을 때만 Opaque 모드 복구
        if (Mathf.Approximately(_currentAlpha, OpaqueAlpha))
        {
            SetOpaqueMode();
        }
    }

    //목표 alpha에 도달했는지 확인
    public bool IsFadeFinished()
    {
        return Mathf.Approximately(_currentAlpha, _targetAlpha);
    }

    //불투명 상태인지 확인
    public bool IsOpaque()
    {
        return Mathf.Approximately(_currentAlpha, OpaqueAlpha);
    }

    //alpha변경
    private void ApplyAlpha(float alpha)
    {
        foreach (Material material in _materials)
        {
            if (material == null)
                continue;

            if (material.HasProperty("_BaseColor"))
            {
                Color color = material.GetColor("_BaseColor");
                color.a = alpha;
                material.SetColor("_BaseColor", color);
                continue;
            }

            if (material.HasProperty("_Color"))
            {
                Color color = material.GetColor("_Color");
                color.a = alpha;
                material.SetColor("_Color", color);
            }
        }
    }

    //변경
    private void SetTransparentMode()
    {
        if (_isTransparentMode)
            return;

        foreach (Material material in _materials)
        {
            if (material == null)
                continue;

            // URP Lit 기준 Transparent 설정
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        _isTransparentMode = true;

        if (_showDebugLog)
        {
            DebugTool.Log("[ObstacleFadeTarget] Transparent 모드 적용", DebugType.Data, this);
        }
    }
    //변경
    private void SetOpaqueMode()
    {
        if (!_isTransparentMode)
            return;

        foreach (Material material in _materials)
        {
            if (material == null)
                continue;

            // URP Lit 기준 Opaque 설정
            material.SetFloat("_Surface", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.One);
            material.SetFloat("_DstBlend", (float)BlendMode.Zero);
            material.SetFloat("_ZWrite", 1f);
            material.renderQueue = (int)RenderQueue.Geometry;
        }

        _isTransparentMode = false;

        if (_showDebugLog)
        {
            DebugTool.Log("[ObstacleFadeTarget] Opaque 모드 적용",DebugType.Data, this);
        }
    }
}