using UnityEngine;

[System.Serializable]
public struct NodeTypeWeight
{
    public NodeType NodeType;
    public int Weight;

    public NodeTypeWeight(NodeType nodeType, int weight)
    {
        NodeType = nodeType;
        Weight = weight;
    }
}
