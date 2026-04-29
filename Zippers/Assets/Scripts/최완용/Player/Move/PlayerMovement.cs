using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private PlayerStats _playerStats;
    public bool IsSprinting { get; private set; }

    //[SerializeField] private float _moveSpeed = 5f;

    public Vector3 MoveDir{ get; private set; }

    public void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
    }
    // 입력 처리(대각선이동도 속도 같음)
    public void SetMoveInput(Vector2 input)
    {
        Vector2 normalizedInput = input.normalized;
        MoveDir = new Vector3(normalizedInput.x, 0f, normalizedInput.y);
    }
    public void SetSprint(bool isSprinting)
    {
        IsSprinting = isSprinting;
    }

    // 이동 처리
    public void Move()
    {
        transform.Translate(MoveDir * CalMoveSpeed() * Time.deltaTime, Space.World);
    }
    public float CalMoveSpeed()
    {
        if(!IsSprinting)
        {
            return _playerStats.MoveSpeed;
        }
        return _playerStats.MoveSpeed + _playerStats.SprintSpeed;
    }

}