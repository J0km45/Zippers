using UnityEngine;

public class TransparentWallGizmo : MonoBehaviour
{
    [SerializeField] bool isGizmoActive = true;
    
    private void OnDrawGizmos()
    {
        if(!isGizmoActive) return;
        BoxCollider collider = GetComponent<BoxCollider>();
        if(collider == null) return;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(collider.center, collider.size);
        Gizmos.color = new Color(1, 1, 1, 0.25f);
        Gizmos.DrawCube(collider.center, collider.size);
    }
}
