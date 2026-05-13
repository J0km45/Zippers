using UnityEngine;

public class PlayerTransform : MonoBehaviour
{
    private bool _isRegistered;

    private void OnEnable()
    {
        Register();
    }

    private void OnDisable()
    {
        Unregister();
    }

    // 플레이어 위치 리스트에 등록
    public void Register()
    {
        if (_isRegistered)
        {
            return;
        }

        if (PlayerTransformList.Instance == null)
        {
            DebugTool.Log("PlayerTransformList가 없습니다.", DebugType.Character, this);
            return;
        }

        PlayerTransformList.Instance.AddPlayer(transform);
        _isRegistered = true;
    }

    // 플레이어 위치 리스트에서 제거
    public void Unregister()
    {
        if (!_isRegistered)
        {
            return;
        }

        if (PlayerTransformList.Instance == null)
        {
            _isRegistered = false;
            return;
        }

        PlayerTransformList.Instance.DeletePlayer(transform);
        _isRegistered = false;
    }
}