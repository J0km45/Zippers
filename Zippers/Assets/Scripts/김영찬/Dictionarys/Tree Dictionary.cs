using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TreeSO를 목록화 하고 검색
/// </summary>
public class TreeDictionary : MonoBehaviour
{
    /// <summary>
    /// SingleTon Instance
    /// </summary>
    public static TreeDictionary Instance { get; private set; }

    [SerializeField] private TreeSO _testTree;
    
    [Header("테스트 트리 데이터 제외한 나머지 데이터들")]
    [SerializeField] private TreeSO[] _trees;
    
    private Dictionary<int, TreeSO> _dict_Lv1;
    private Dictionary<int, TreeSO> _dict_Lv2;
    private Dictionary<int, TreeSO> _dict_Lv3;
    private Dictionary<int, TreeSO> _dict_Lv4;
    private Dictionary<int, TreeSO> _dict_Lv5;

    public bool IsDictionaryReady { get; private set; }

    private void Awake()
    {
        SetSingleTon();
        IsDictionaryReady = false;
        InitDict();
    }
    
    private void SetSingleTon()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void InitDict()
    {
        _dict_Lv1 = new Dictionary<int, TreeSO>();
        _dict_Lv2 = new Dictionary<int, TreeSO>();
        _dict_Lv3 = new Dictionary<int, TreeSO>();
        _dict_Lv4 = new Dictionary<int, TreeSO>();
        _dict_Lv5 = new Dictionary<int, TreeSO>();
        
        foreach (TreeSO tree in _trees)
        {
            switch (tree.Difficulty)
            {
                case NodeDifficulty.Level1:
                    bool tempLv1 = _dict_Lv1.TryAdd(tree.TreeIndex, tree);
                    if (!tempLv1) DebugTool.Error($"TreeSO Index Duplicate : {tree.Difficulty}_{tree.TreeIndex}", DebugType.Node, this);
                    break;
                case NodeDifficulty.Level2:
                    bool tempLv2 = _dict_Lv2.TryAdd(tree.TreeIndex, tree);
                    if (!tempLv2) DebugTool.Error($"TreeSO Index Duplicate : {tree.Difficulty}_{tree.TreeIndex}", DebugType.Node, this);
                    break;
                case NodeDifficulty.Level3:
                    bool tempLv3 = _dict_Lv3.TryAdd(tree.TreeIndex, tree);
                    if (!tempLv3) DebugTool.Error($"TreeSO Index Duplicate : {tree.Difficulty}_{tree.TreeIndex}", DebugType.Node, this);
                    break;
                case NodeDifficulty.Level4:
                    bool tempLv4 = _dict_Lv4.TryAdd(tree.TreeIndex, tree);
                    if (!tempLv4) DebugTool.Error($"TreeSO Index Duplicate : {tree.Difficulty}_{tree.TreeIndex}", DebugType.Node, this);
                    break;
                case NodeDifficulty.Level5:
                    bool tempLv5 = _dict_Lv5.TryAdd(tree.TreeIndex, tree);
                    if (!tempLv5) DebugTool.Error($"TreeSO Index Duplicate : {tree.Difficulty}_{tree.TreeIndex}", DebugType.Node, this);
                    break;
                default:
                    break;
            }
        }
        
        IsDictionaryReady = true;
        DebugTool.Log("Node Dictionary Ready", DebugType.Node, this);
    }

    /// <summary>
    /// 노드 트리 설정값 불러오기
    /// </summary>
    /// <param name="difficulty">이번 Game Scene 난이도</param>
    /// <param name="first">첫번째 설정 값<br/>해당 난이도에 설정값 없으면 null 반환</param>
    /// <param name="second">두번째 설정 값<br/>해당 난이도에 설정값 없으면 null 반환</param>
    /// <param name="third">세번째 설정 값<br/>해당 난이도에 설정값 없으면 null 반환</param>
    /// <param name="fourth">네번째 설정 값<br/>해당 난이도에 설정값 없으면 null 반환</param>
    /// <param name="fifth">다섯번째 설정 값<br/>해당 난이도에 설정값 없으면 null 반환</param>
    public void GetTreeData(NodeDifficulty difficulty, out TreeSO first, out TreeSO second, out TreeSO third, out TreeSO fourth, out TreeSO fifth)
    {
        first = null;
        second = null;
        third = null;
        fourth = null;
        fifth = null;
        
        bool isSetFirst = false;
        bool isSetSecond = false;
        bool isSetThird = false;
        bool isSetFourth = false;
        bool isSetFifth = false;
        
        switch (difficulty)
        {
            case NodeDifficulty.Test:
                first = _testTree;
                break;
            
            case NodeDifficulty.Level1:
                if(_dict_Lv1 == null) break;
                for (int i = 0; i < _dict_Lv1.Count; i++)
                {
                    int temp = Random.Range(0, _dict_Lv1.Count - 1);
                    if (!isSetFirst)
                    {
                        first = _dict_Lv1[temp];
                        isSetFirst = true;
                    }
                    else if (!isSetSecond) 
                    {
                        second = _dict_Lv1[temp];
                        isSetSecond = true;
                    }
                    else if (!isSetThird) 
                    {
                        third = _dict_Lv1[temp];
                        isSetThird = true;
                    }
                    else if (!isSetFourth) 
                    {
                        fourth = _dict_Lv1[temp];
                        isSetFourth = true;
                    }
                    else if (!isSetFifth) 
                    {
                        fifth = _dict_Lv1[temp];
                        isSetFifth = true;
                    }
                    else break;
                }
                break;
            case NodeDifficulty.Level2:
                if(_dict_Lv2 == null) break;
                for (int i = 0; i < _dict_Lv2.Count; i++)
                {
                    int temp = Random.Range(0, _dict_Lv2.Count - 1);
                    if (!isSetFirst)
                    {
                        first = _dict_Lv2[temp];
                        isSetFirst = true;
                    }
                    else if (!isSetSecond) 
                    {
                        second = _dict_Lv2[temp];
                        isSetSecond = true;
                    }
                    else if (!isSetThird) 
                    {
                        third = _dict_Lv2[temp];
                        isSetThird = true;
                    }
                    else if (!isSetFourth) 
                    {
                        fourth = _dict_Lv2[temp];
                        isSetFourth = true;
                    }
                    else if (!isSetFifth) 
                    {
                        fifth = _dict_Lv2[temp];
                        isSetFifth = true;
                    }
                    else break;
                }
                break;
            case NodeDifficulty.Level3:
                if(_dict_Lv3 == null) break;
                for (int i = 0; i < _dict_Lv3.Count; i++)
                {
                    int temp = Random.Range(0, _dict_Lv3.Count - 1);
                    if (!isSetFirst)
                    {
                        first = _dict_Lv3[temp];
                        isSetFirst = true;
                    }
                    else if (!isSetSecond) 
                    {
                        second = _dict_Lv3[temp];
                        isSetSecond = true;
                    }
                    else if (!isSetThird) 
                    {
                        third = _dict_Lv3[temp];
                        isSetThird = true;
                    }
                    else if (!isSetFourth) 
                    {
                        fourth = _dict_Lv3[temp];
                        isSetFourth = true;
                    }
                    else if (!isSetFifth) 
                    {
                        fifth = _dict_Lv3[temp];
                        isSetFifth = true;
                    }
                    else break;
                }
                break;
            case NodeDifficulty.Level4:
                if(_dict_Lv4 == null) break;
                for (int i = 0; i < _dict_Lv4.Count; i++)
                {
                    int temp = Random.Range(0, _dict_Lv4.Count - 1);
                    if (!isSetFirst)
                    {
                        first = _dict_Lv4[temp];
                        isSetFirst = true;
                    }
                    else if (!isSetSecond) 
                    {
                        second = _dict_Lv4[temp];
                        isSetSecond = true;
                    }
                    else if (!isSetThird) 
                    {
                        third = _dict_Lv4[temp];
                        isSetThird = true;
                    }
                    else if (!isSetFourth) 
                    {
                        fourth = _dict_Lv4[temp];
                        isSetFourth = true;
                    }
                    else if (!isSetFifth) 
                    {
                        fifth = _dict_Lv4[temp];
                        isSetFifth = true;
                    }
                    else break;
                }
                break;
            case NodeDifficulty.Level5:
                if(_dict_Lv5 == null) break;
                for (int i = 0; i < _dict_Lv5.Count; i++)
                {
                    int temp = Random.Range(0, _dict_Lv5.Count - 1);
                    if (!isSetFirst)
                    {
                        first = _dict_Lv5[temp];
                        isSetFirst = true;
                    }
                    else if (!isSetSecond) 
                    {
                        second = _dict_Lv5[temp];
                        isSetSecond = true;
                    }
                    else if (!isSetThird) 
                    {
                        third = _dict_Lv5[temp];
                        isSetThird = true;
                    }
                    else if (!isSetFourth) 
                    {
                        fourth = _dict_Lv5[temp];
                        isSetFourth = true;
                    }
                    else if (!isSetFifth) 
                    {
                        fifth = _dict_Lv5[temp];
                        isSetFifth = true;
                    }
                    else break;
                }
                break;
        }
    }
}
