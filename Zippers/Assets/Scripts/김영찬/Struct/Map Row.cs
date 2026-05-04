using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public struct MapRow
{
    public NodeType[] RowData;

    public MapRow(NodeType leftSide, NodeType upSide, NodeType rightSide)
    {
        RowData = new[] { leftSide, upSide, rightSide };
    }
}
