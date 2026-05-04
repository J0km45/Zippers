using UnityEngine;

public class PoolObject : MonoBehaviour
{
    public GameObject OriginPrefab { get; private set; }

    public void SetOrigin(GameObject prefab)
    {
        OriginPrefab = prefab;
    }
}