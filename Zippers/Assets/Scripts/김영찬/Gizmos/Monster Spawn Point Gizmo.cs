using UnityEngine;

public class MonsterSpawnPointGizmo : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(1,2,1));
        Gizmos.color = new Color(1, 1f, 0.5f, 0.25f);
        Gizmos.DrawSphere(Vector3.zero, 3f);
    }
}
