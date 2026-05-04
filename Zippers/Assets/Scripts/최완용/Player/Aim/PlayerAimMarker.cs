using UnityEngine;

public class PlayerAimMarker : MonoBehaviour
{
    [SerializeField] private PlayerAim _playerAim;
    [SerializeField] private PlayerAimCal _playerAimCal;

    [Header("조준시 표시되는 오브젝트")]
    [SerializeField] private GameObject _aimMarker;
    [SerializeField] private float _markerHeight = 0.05f;

    private void Awake()
    {
        HideMarker();
    }

    private void Update()
    {
        if(!_playerAim.IsAiming)
        {
            HideMarker();
            return;
        }

        UpdateMarkerPosition();
    }

    private void UpdateMarkerPosition()
    {
        if (!_playerAimCal.TryGetAimPoint(out Vector3 aimPoint))
        {
            HideMarker();
            return;
        }

        aimPoint.y += _markerHeight;

        if(!_aimMarker.activeSelf)
        {
            _aimMarker.SetActive(true);
        }
        _aimMarker.transform.position = aimPoint;
    }
    void HideMarker()
    {
        if(_aimMarker.activeSelf)
        {
            _aimMarker.SetActive(false);
        }
    }
}
