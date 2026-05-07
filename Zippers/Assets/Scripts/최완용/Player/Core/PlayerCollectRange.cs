using UnityEngine;

public class PlayerCollectRange : MonoBehaviour
{
    [Header("줍기범위")]
    [SerializeField] private SphereCollider _collider;

    private PlayerStats _playerStats;
    private float _lastCollectRange;

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();

        if(_collider == null)
        {
            _collider = GetComponent<SphereCollider>();
        }
        CollectTrigger();
        UpdateCollectTrigger();
    }

    private void Update()
    {
        if (_playerStats == null || _collider == null)
            return;

        if (Mathf.Approximately(_lastCollectRange, _playerStats.CollectRange))
            return;

        UpdateCollectTrigger();
    }

    private void CollectTrigger()
    {
        _collider.isTrigger = true;
    }

    private void UpdateCollectTrigger()
    {
        float collectRange = Mathf.Max(0f, _playerStats.CollectRange);

        _collider.radius = collectRange;
        _lastCollectRange = collectRange;
    }

    private void OnDrawGizmos()
    {
        PlayerStats playerStats = _playerStats;

        // 에디터 상태에서는 Awake가 실행되지 않았을 수 있어서 직접 다시 찾는다.
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }

        if (playerStats == null)
            return;

        Gizmos.DrawWireSphere(transform.position, playerStats.CollectRange);
    }
}
