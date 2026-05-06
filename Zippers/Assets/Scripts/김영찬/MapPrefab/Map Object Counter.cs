using UnityEngine;

public class MapObjectCounter : MonoBehaviour
{
    [SerializeField] private BoxCollider _countArea;
    [SerializeField] private MapController _controller;
    [SerializeField] LayerMask _unitLayer;
    [SerializeField] LayerMask _monsterLayer;
    [SerializeField] bool isGizmoActive = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == _unitLayer)
        {
            _controller.Data.PlusAlivePlayerCount();
        }

        if (other.gameObject.layer == _monsterLayer)
        {
            _controller.Data.PlusAliveMonsterCount();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == _unitLayer)
        {
            _controller.Data.MinusAlivePlayerCount();
        }
        
        if (other.gameObject.layer == _monsterLayer)
        {
            _controller.Data.MinusAliveMonsterCount();
        }
    }
    
    private void OnDrawGizmos()
    {
        if(!isGizmoActive) return;
        if(_countArea == null) return;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(_countArea.center, _countArea.size);
    }
}
