using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAimCal : MonoBehaviour
{
    [SerializeField] private Camera _camera;


    private void Awake()
    {
        if( _camera == null )
        {
            _camera = Camera.main;
        }
    }
    //현재 마우스가 가리키는 월드 좌표 위치
    public bool TryGetAimPoint(out Vector3 aimPoint)
    {
        aimPoint = Vector3.zero;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = _camera.ScreenPointToRay(mousePosition);

        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if(!groundPlane.Raycast(ray, out float distance))
        {
            return false;
        }

        aimPoint = ray.GetPoint(distance);
        return true;
    }
}
