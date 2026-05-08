using UnityEngine;
using System;

public class PlayerResourceCollector : MonoBehaviour, IResourceCollectable
{
    public Action<ResourcesType, float> OnResourceCollected;

    private float _scrap;
    private float _supplies;
    private float _infectionSample;

    public float Scrap => _scrap;
    public float Supplies => _supplies;
    public float InfectionSample => _infectionSample;

    public void CollectResource(ResourcesType type, float amount)
    {
        bool isAdded = AddResource(type, amount);

        OnResourceCollected?.Invoke(type, amount);
    }

    private bool AddResource(ResourcesType type, float amount)
    {
        switch (type)
        {
            case ResourcesType.Scrap:
                _scrap = amount;
                DebugTool.Log($"Scrap 획득 : +{amount} / 현재 Scrap : {_scrap}", DebugType.Data, this);
                return true;
            case ResourcesType.Supplies:
                _supplies = amount;
                DebugTool.Log($"Supplies 획득 : +{amount} / 현재 Supplies : {_supplies}", DebugType.Data, this);
                return true;
            case ResourcesType.InfectionSample:
                _infectionSample = amount;
                DebugTool.Log($"InfectionSample 획득 : +{amount} / 현재 InfectionSample : {_infectionSample}", DebugType.Data, this);
                return true;
            case ResourcesType.None:
                return false;
            default:
                return false;
        }
    }
}
