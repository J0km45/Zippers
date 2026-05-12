using UnityEngine;
using System;

public class PlayerResourceCollector : MonoBehaviour, IResourceCollectable
{
    public Action<ResourcesType, float> OnResourceCollected;
    public Action<ResourcesType, float> OnResourceUsed;

    private float _scrap;
    private float _supplies;
    private float _infectionSample;

    public float Scrap => _scrap;
    public float Supplies => _supplies;
    public float InfectionSample => _infectionSample;

    public void CollectResource(ResourcesType type, float amount)
    {
        bool isAdded = AddResource(type, amount);

        if (!isAdded)
        {
            return;
        }

        OnResourceCollected?.Invoke(type, amount);
    }
    // 재화 보유량 확인
    public bool HasEnoughResource(ResourcesType type, float amount)
    {
        if(amount <= 0f)
        {
            return false;
        }
        return GetResourceAmount(type) >= amount;
    }

    // 재화 사용
    public bool UseResource(ResourcesType type, float amount)
    {
        if(amount <= 0f)
        {
            return false;
        }
        if (!HasEnoughResource(type, amount))
        {
            DebugTool.Log($"{type} 부족 / 필요: {amount}, 보유: {GetResourceAmount(type)}", DebugType.Data, this);
            return false;
        }

        switch (type)
        {
            case ResourcesType.Scrap:
                _scrap -= amount;
                break;

            case ResourcesType.Supplies:
                _supplies -= amount;
                break;

            case ResourcesType.InfectionSample:
                _infectionSample -= amount;
                break;

            case ResourcesType.None:
                return false;

            default:
                return false;
        }
        OnResourceUsed?.Invoke(type, amount);
        DebugTool.Log($"{type} 사용 : -{amount} / 남은 수량 : {GetResourceAmount(type)}", DebugType.Data, this);

        return true;
    }
    private bool AddResource(ResourcesType type, float amount)
    {
        switch (type)
        {
            case ResourcesType.Scrap:
                _scrap += amount;
                DebugTool.Log($"Scrap 획득 : +{amount} / 현재 Scrap : {_scrap}", DebugType.Data, this);
                return true;
            case ResourcesType.Supplies:
                _supplies += amount;
                DebugTool.Log($"Supplies 획득 : +{amount} / 현재 Supplies : {_supplies}", DebugType.Data, this);
                return true;
            case ResourcesType.InfectionSample:
                _infectionSample += amount;
                DebugTool.Log($"InfectionSample 획득 : +{amount} / 현재 InfectionSample : {_infectionSample}", DebugType.Data, this);
                return true;
            case ResourcesType.None:
                return false;
            default:
                return false;
        }
    }
    private float GetResourceAmount(ResourcesType type)
    {
        switch (type)
        {
            case ResourcesType.Scrap:
                return _scrap;

            case ResourcesType.Supplies:
                return _supplies;

            case ResourcesType.InfectionSample:
                return _infectionSample;

            case ResourcesType.None:
                return 0f;

            default:
                return 0f;
        }
    }
}
