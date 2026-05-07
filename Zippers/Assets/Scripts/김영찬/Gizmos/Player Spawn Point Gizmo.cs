using System;
using UnityEngine;

public class PlayerSpawnPointGizmo : MonoBehaviour
{
    [SerializeField] bool isGizmoActive = true;
    
    private void OnDrawGizmos()
    {
        if(!isGizmoActive) return;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(1,2,1));
    }
}
