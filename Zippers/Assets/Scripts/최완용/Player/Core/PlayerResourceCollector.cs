using UnityEngine;
using System;

public class PlayerResourceCollector : MonoBehaviour, IResourceCollectable
{
    public Action<ResourcesType, int> OnResourceCollected;

    private int _scrap;
    private int _supplies;
    private int _infectionSample;

    public int Scrap => _scrap;
    public int Supplies => _supplies;
    public int InfectionSample => _infectionSample;

    public void CollectResource(ResourcesType type, int amount)
    {
        bool isAdded = AddResource(type, amount);

        OnResourceCollected?.Invoke(type, amount);
    }

    private bool AddResource(ResourcesType type, int amount)
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
