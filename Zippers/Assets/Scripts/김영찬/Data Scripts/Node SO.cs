using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 노드의 Data를 저장하기 위한 SO<br/>
/// 맵의 프리팹과 연결되어 있다.
/// </summary>
[CreateAssetMenu(fileName = "Node SO", menuName = "Node Data/Node SO")]
public class NodeSO : ScriptableObject
{
    [Tooltip("노드 타입")]
    [field:SerializeField] public NodeType NodeType {get; private set;}
    
    [FormerlySerializedAs("Maps")]
    [Tooltip("이 노드에 해당 되는 Map 프리팹")] 
    [SerializeField] private GameObject[] _maps;

    private void Awake()
    {
        SetNodeInfo();
    }

    private void SetNodeInfo()
    {
        if(NodeType == NodeType.Empty) return;
        
        if(_maps == null || _maps.Length == 0)
        {
            DebugTool.Warning($"Not SerializeField Map : {NodeType}", DebugType.Node, this);
            return;
        }

        for (int i = 0; i < _maps.Length; i++)
        {
            bool verification = _maps[i].TryGetComponent(out MapData data);
            if (verification) data.SetNodeInfo(NodeType, i);
        }
    }
    
    /// <summary>
    /// 인덱스에 해당되는 맵 데이터 호출
    /// </summary>
    /// <param name="index">호출할 맵의 인덱스</param>
    /// <returns>MapData 컴포넌트</returns>
    public MapData GetNodeMapData(int index)
    {
        if(NodeType == NodeType.Empty) return null;
        
        if(_maps == null)
        {
            DebugTool.Warning($"Not SerializeField Map : {NodeType}", DebugType.Node, this);
            return null;
        }
        
        if (index < 0)
        {
            DebugTool.Error($"Index Wrong range : {index}", DebugType.Node, this);
            return null;
        }
        
        if (index > _maps.Length - 1)
        {
            DebugTool.Error($"Index out of range : {NodeType}_{index}", DebugType.Node, this);
            return null;
        }
        
        return _maps[index].GetComponent<MapData>();
    }

    /// <summary>
    /// Index에 해당 되는 Map을 활성화
    /// </summary>
    /// <param name="index"></param>
    public void MapEnable(int index)
    {
        _maps[index].SetActive(true);
    }

    /// <summary>
    /// Index에 해당 되는 Map을 비활성화
    /// </summary>
    /// <param name="index"></param>
    public void MapDisable(int index)
    {
        _maps[index].SetActive(false);
    }
}
