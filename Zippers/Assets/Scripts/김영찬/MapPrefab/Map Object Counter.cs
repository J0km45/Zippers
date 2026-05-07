using UnityEngine;

public class MapObjectCounter : MonoBehaviour
{
    [SerializeField] private BoxCollider _countArea;
    [SerializeField] private MapController _controller;
    [SerializeField] LayerMask _unitLayer;
    [SerializeField] bool isGizmoActive = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == _unitLayer)
        {
            _controller.Data.PlusAlivePlayerCount();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == _unitLayer)
        {
            _controller.Data.MinusAlivePlayerCount();
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
