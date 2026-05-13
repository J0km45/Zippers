using System;
using UnityEngine;

// 팀 공용 재화를 관리한다.
// 현재 팀 업그레이드는 Supplies를 사용한다.
public class TeamResourceManager : MonoBehaviour
{
    public static TeamResourceManager Instance { get; private set; }

    public event Action<ResourcesType, float, float> TeamResourceChanged;

    private float _supplies;

    public float Supplies => _supplies;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }

        if (Instance != this)
        {
            DebugTool.Log("[TeamResourceManager] 중복 인스턴스가 있어 제거합니다.", DebugType.Data, this);
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }


    //팀 재화를 증가시킨다.

    public bool AddResource(ResourcesType type, float amount)
    {
        //TODO : 팀 공용 Supplies증가는 서버에서 관리해야됨
        if (amount <= 0f)
        {
            DebugTool.Log($"[TeamResourceManager] 증가량이 올바르지 않습니다. amount: {amount}", DebugType.Data, this);
            return false;
        }

        switch (type)
        {
            //TODO : 짐 재화 번경 이벤트는 서버가 확정한 후 값은 받아서 호출해야됨
            case ResourcesType.Supplies:
                _supplies += amount;
                TeamResourceChanged?.Invoke(type, _supplies, amount);

                DebugTool.Log($"[TeamResourceManager] 팀 Supplies 획득 : +{amount} / 현재 Supplies : {_supplies}", DebugType.Data, this);

                return true;

            default:
                DebugTool.Log($"[TeamResourceManager] 팀 재화로 관리하지 않는 타입입니다. type: {type}", DebugType.Data, this);
                return false;
        }
    }


    // 팀 재화가 충분한지 확인한다.
    public bool HasEnoughResource(ResourcesType type, float amount)
    {
        if (amount <= 0f)
        {
            return false;
        }

        return GetResourceAmount(type) >= amount;
    }


    // 팀 재화를 사용한다.
    public bool UseResource(ResourcesType type, float amount)
    {
        //TODO : 팀 공용 제화는 서버가 보유량을 검증한 뒤 모든 클라이언트에 동기화 해야된다.
        if (amount <= 0f)
        {
            return false;
        }

        if (!HasEnoughResource(type, amount))
        {
            DebugTool.Log(
                $"[TeamResourceManager] {type} 부족 / 필요: {amount}, 보유: {GetResourceAmount(type)}",
                DebugType.Data,
                this
            );

            return false;
        }

        switch (type)
        {
            case ResourcesType.Supplies:
                _supplies -= amount;
                TeamResourceChanged?.Invoke(type, _supplies, -amount);

                DebugTool.Log(
                    $"[TeamResourceManager] 팀 Supplies 사용 : -{amount} / 남은 Supplies : {_supplies}",
                    DebugType.Data,
                    this
                );

                return true;

            default:
                DebugTool.Log($"[TeamResourceManager] 팀 재화로 사용하지 않는 타입입니다. type: {type}", DebugType.Data, this);
                return false;
        }
    }

    // 팀 재화 보유량을 반환한다.
    public float GetResourceAmount(ResourcesType type)
    {
        switch (type)
        {
            case ResourcesType.Supplies:
                return _supplies;

            default:
                return 0f;
        }
    }
}