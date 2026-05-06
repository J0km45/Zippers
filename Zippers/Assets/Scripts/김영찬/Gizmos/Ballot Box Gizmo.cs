using UnityEngine;

public class BallotBoxGizmo : MonoBehaviour
{
    [SerializeField] bool isGizmoActive = true;
    
    private void OnDrawGizmos()
    {
        if(!isGizmoActive) return;
        BoxCollider collider = GetComponent<BoxCollider>();
        if(collider == null) return;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(collider.center, collider.size);
    }
}
