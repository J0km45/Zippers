using Unity.Netcode;
using UnityEngine;

public class NetworkMapData : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<int> _alivePlayerCount;
    [SerializeField] private NetworkVariable<NodeState> _nodeState;
    
    /// <summary>
    /// 맵에 생존한 플레이어의 수
    /// </summary>
    public NetworkVariable<int> AlivePlayerCount => _alivePlayerCount;
    
    /// <summary>
    /// 현재 맵의 진행 상태
    /// </summary>
    public NetworkVariable<NodeState> NodeState => _nodeState;
    
    private void Awake()
    {
        DebugTool.Log($"{gameObject.name} Network Map Data Awake", DebugType.Node, this);
    }

    /// <summary>
    /// 현재 맵에서 살아남은 플레이어 숫자 증가
    /// </summary>
    public void PlusAlivePlayerCount()
    {
        if(!IsServer) return;
        if(AlivePlayerCount.Value >= 4) return;
        _alivePlayerCount.Value++;
        DebugTool.Log($"{gameObject.name} Player Income, Current Player : {AlivePlayerCount}", DebugType.Node, this);
    }

    /// <summary>
    /// 현재 맵에서 살아남은 플레이어 숫자 감소
    /// </summary>
    public void MinusAlivePlayerCount()
    {
        if(!IsServer) return;
        if (AlivePlayerCount.Value <= 0) return;
        _alivePlayerCount.Value--;
        DebugTool.Log($"{gameObject.name} Player Out, Current Player : {AlivePlayerCount}", DebugType.Node, this);
    }

    /// <summary>
    /// 현재 맵에서 살아남은 플레이어 숫자 리셋
    /// </summary>
    public void ResetAlivePlayerCount()
    {
        if(!IsServer) return;
        _alivePlayerCount.Value = 0;
        DebugTool.Log($"{gameObject.name} Player Count Reset", DebugType.Node, this);
    }

    /// <summary>
    /// 현재 맵에서 살아남은 플레이어 숫자를 직접 지정 (호스트 권위).
    /// MapObjectCounter 가 HashSet 기반으로 추적한 set.Count 를 한 번에 반영하는 용도.
    /// Plus/Minus 가 누락/중복 시에도 set.Count 가 정답이므로 이쪽으로 동기화하면 정합성 보장.
    /// [0, 4] 클램프 (4 = MaxPlayers 컨벤션, 기존 Plus 의 4 캡과 일관).
    /// 동일 값이면 OnValueChanged 노이즈 방지 위해 skip.
    /// </summary>
    /// <param name="count">반영할 인원 수. 음수/4 초과 시 클램프됨.</param>
    public void SetAlivePlayerCount(int count)
    {
        if(!IsServer) return;
        int clamped = Mathf.Clamp(count, 0, 4);
        if (_alivePlayerCount.Value == clamped) return;
        _alivePlayerCount.Value = clamped;
        DebugTool.Log($"{gameObject.name} Set Player Count : {clamped} (input={count})", DebugType.Node, this);
    }
    
    /// <summary>
    /// 현재 맵의 Node State를 지정
    /// </summary>
    /// <param name="state">지정할 Node State</param>
    public void SetNodeState(NodeState state)
    {
        if(!IsServer) return;
        _nodeState.Value = state;
        DebugTool.Log($"{gameObject.name} Map Node State Change : {NodeState}", DebugType.Node, this);
    }
}
