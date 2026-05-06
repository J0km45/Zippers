using UnityEngine;

public class NoPlayerZoneGizmo : MonoBehaviour
{
    [SerializeField] bool isGizmoActive = true;
    
    private void OnDrawGizmos()
    {
        if(!isGizmoActive) return;
        BoxCollider collider = GetComponent<BoxCollider>();
        if(collider == null) return;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.darkOrange;
        Gizmos.DrawWireCube(collider.center, collider.size);
        Gizmos.color = new Color(1, 0.25f, 0.25f, 0.25f);
        Gizmos.DrawCube(collider.center, collider.size);
    }
}
