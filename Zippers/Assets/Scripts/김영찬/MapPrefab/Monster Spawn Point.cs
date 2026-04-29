using UnityEngine;

public class MonsterSpawnPoint : MonoBehaviour
{
    [field: SerializeField] public Transform SpawnPoint { get; private set; }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = SpawnPoint.localToWorldMatrix;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(1,2,1));
    }
}
