using UnityEngine;

/// <summary>
/// 노드 현황을 관리 및 제어
/// </summary>
public class NodeManager : MonoBehaviour
{
    [field: SerializeField] public NodeDataContainer DataContainer { get; private set; }

    public static NodeManager Instance { get; private set; }

    private void Awake()
    {
        SetSingleton();
    }
    
    
    
    private void SetSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
