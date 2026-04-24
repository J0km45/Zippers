using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]private float _moveSpeed = 5f;
    private Vector3 _moveDir;

    
    private void Update()
    {
        Move();
    }

    // 입력 처리(대각선이동도 속도 같음)
    public void SetMoveInput(Vector2 input)
    {
        Vector2 normalizedInput = input.normalized;
        _moveDir = new Vector3(normalizedInput.x, 0f, normalizedInput.y);
    }

    // 이동 처리
    public void Move()
    {
        transform.Translate(_moveDir * _moveSpeed * Time.deltaTime, Space.World);
    }

}