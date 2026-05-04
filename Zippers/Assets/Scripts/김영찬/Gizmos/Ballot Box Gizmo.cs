using UnityEngine;

public class BallotBoxGizmo : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        BoxCollider collider = GetComponent<BoxCollider>();
        if(collider == null) return;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(collider.center, collider.size);
    }
}
