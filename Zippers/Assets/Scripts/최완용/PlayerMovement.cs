using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    public Vector3 MoveDir{ get; private set; }

    // 입력 처리(대각선이동도 속도 같음)
    public void SetMoveInput(Vector2 input)
    {
        Vector2 normalizedInput = input.normalized;
        MoveDir = new Vector3(normalizedInput.x, 0f, normalizedInput.y);
    }

    // 이동 처리
    public void Move()
    {
        transform.Translate(MoveDir * _moveSpeed * Time.deltaTime, Space.World);
    }

}