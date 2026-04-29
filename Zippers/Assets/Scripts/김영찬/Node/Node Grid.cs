using System.Collections.Generic;
using UnityEngine;

public class NodeGrid : MonoBehaviour
{
    [SerializeField] private List<Vector3> line_0;
    [SerializeField] private List<Vector3> line_1;
    [SerializeField] private List<Vector3> line_2;
    [SerializeField] private List<Vector3> line_3;
    [SerializeField] private List<Vector3> line_4;
    [SerializeField] private List<Vector3> line_5;
    [SerializeField] private List<Vector3> line_6;
    [SerializeField] private List<Vector3> line_7;
    [SerializeField] private List<Vector3> line_8;
    [SerializeField] private List<Vector3> line_9;
    [SerializeField] private List<Vector3> line_10;

    public List<Vector3>[] GridMap { get; private set; }

    private void Awake()
    {
        GridMap = new []
        {
            line_0, line_1, line_2, line_3, line_4, line_5, line_6, line_7, line_8, line_9, line_10
        };
    }

    #region Gizmos

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blueViolet;
        if (line_0 != null)
        {
            foreach (Vector3 point in line_0)
            {
                Gizmos.DrawWireCube(point, new Vector3(150, 0, 150));
            }
        }

        if (line_1 != null)
        {
            foreach (Vector3 point in line_1)
            {
                Gizmos.DrawWireCube(point, new Vector3(150, 0, 150));
            }
        }

        if (line_2 != null)
        {
            foreach (Vector3 point in line_2)
            {
                Gizmos.DrawWireCube(point, new Vector3(150, 0, 150));
            }
        }
        
        if (line_3 != null)
        {
            foreach (Vector3 point in line_3)
            {
                Gizmos.DrawWireCube(point, new Vector3(150, 0, 150));
            }
        }

        if (line_4 != null)
        {
            foreach (Vector3 point in line_4)
            {
                Gizmos.DrawWireCube(point, new Vector3(150, 0, 150));
            }
        }
        
        if (line_5 != null)
        {
            foreach (Vector3 point in line_5)
            {
                Gizmos.DrawWireCube(point, new Vector3(150, 0, 150));
            }
        }
        
        if (line_6 != null)
        {
            foreach (Vector3 point in line_6)
            {
                Gizmos.DrawWireCube(point, new Vector3(150, 0, 150));
            }
        }
        if (line_7 != null)
        {
            foreach (Vector3 point in line_7)
            {
                Gizmos.DrawWireCube(point, new Vector3(150, 0, 150));
            }
        }
        if (line_8 != null)
        {
            foreach (Vector3 point in line_8)
            {
                Gizmos.DrawWireCube(point, new Vector3(150, 0, 150));
            }
        }
        if (line_9 != null)
        {
            foreach (Vector3 point in line_9)
            {
                Gizmos.DrawWireCube(point, new Vector3(150, 0, 150));
            }
        }
        
        if (line_10 != null)
        {
            foreach (Vector3 point in line_10)
            {
                Gizmos.DrawWireCube(point, new Vector3(150, 0, 150));
            }
        }
    }

    #endregion
}
