using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerStats _playerStats;
    private Rigidbody _rb;
    public bool IsSprinting { get; private set; }
    public Vector3 MoveDir{ get; private set; }

    public void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Move();
    }
    // 입력 처리(대각선이동도 속도 같음)
    public void SetMoveInput(Vector2 input)
    {
        Vector2 normalizedInput = input.normalized;
        MoveDir = new Vector3(normalizedInput.x, 0f, normalizedInput.y);
    }

    public void SetSprint(bool isSprinting)
    {
        //TODO : 달리기 가능여부강 SprintSpeed적용은 서버가 스테미나 검증한 후 처리해야됨
        IsSprinting = isSprinting;
    }

    // 이동 처리
    private void Move()
    {
        Vector3 move = MoveDir * CalMoveSpeed() * Time.fixedDeltaTime;
        Vector3 nextPosition = _rb.position + move;

        _rb.MovePosition(nextPosition);
    }
    public float CalMoveSpeed()
    {
        if(!IsSprinting)
        {
            return _playerStats.TotalMoveSpeed;
        }
        return _playerStats.TotalSprintMoveSpeed;
    }
    public void StopMove()
    {
        MoveDir = Vector3.zero;
        IsSprinting = false;

        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }

}