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
    
    [SerializeField] private TreeSO[] _trees;
    
    private Dictionary<(NodeDifficulty , int), TreeSO> _dict;

    private int _countLv1Tree;
    private int _countLv2Tree;
    private int _countLv3Tree;
    private int _countLv4Tree;
    private int _countLv5Tree;
    
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
        _dict = new Dictionary<(NodeDifficulty, int), TreeSO>();

        _countLv1Tree = 0;
        _countLv2Tree = 0;
        _countLv3Tree = 0;
        _countLv4Tree = 0;
        _countLv5Tree = 0;
        
        foreach (TreeSO tree in _trees)
        {
            bool verification = _dict.TryAdd((tree.Difficulty, tree.TreeIndex), tree);
            if(!verification) DebugTool.Error($"Tree Dictionary Duplication Error : {tree.Difficulty}_{tree.TreeIndex}", DebugType.Node, this);
            else
            {
                switch (tree.Difficulty)
                {
                    case NodeDifficulty.Level1:
                        _countLv1Tree++;
                        break;
                    case NodeDifficulty.Level2:
                        _countLv2Tree++;
                        break;
                    case NodeDifficulty.Level3:
                        _countLv3Tree++;
                        break;
                    case NodeDifficulty.Level4:
                        _countLv4Tree++;
                        break;
                    case NodeDifficulty.Level5:
                        _countLv5Tree++;
                        break;
                }
            }
        }
        
        IsDictionaryReady = true;
        DebugTool.Log("Tree Dictionary Ready", DebugType.Node, this);
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

        int count = 0;
        
        switch (difficulty)
        {
            case NodeDifficulty.Test:
                first = _dict.GetValueOrDefault((difficulty, 0));
                return;
            case NodeDifficulty.Level1:
                count = _countLv1Tree;
                break;
            case NodeDifficulty.Level2:
                count = _countLv2Tree;
                break;
            case NodeDifficulty.Level3:
                count = _countLv3Tree;
                break;
            case NodeDifficulty.Level4:
                count = _countLv4Tree;
                break;
            case NodeDifficulty.Level5:
                count = _countLv5Tree;
                break;
        }

        if (count <= 0)
        {
            DebugTool.Warning($"Not Found Current Difficulty Tree Data : {difficulty}", DebugType.Node, this);
            return;
        }
        
        if(count == 1)
        {
            first = _dict.GetValueOrDefault((difficulty, 0));
            if(first == null) DebugTool.Warning($"Not Found Current Difficulty Tree Data : {difficulty}", DebugType.Node, this);
            return;
        }
        
        for (int i = 0; i < count; i++)
        {
            HashSet<(NodeDifficulty difficulty,int index)> usedTree = new HashSet<(NodeDifficulty,int)>();
            int index = Random.Range(0, count - 1);
            if (!isSetFirst)
            {
                first = _dict.GetValueOrDefault((difficulty, 0));
                isSetFirst = true;
                usedTree.Add((difficulty, index));
            }
            else if (!isSetSecond) 
            {
                while (usedTree.Contains((difficulty, index)))
                {
                    index = Random.Range(0, count - 1);
                }
                
                second = _dict.GetValueOrDefault((difficulty, 0));
                isSetSecond = true;
                usedTree.Add((difficulty, index));
            }
            else if (!isSetThird) 
            {
                if(count <= 2) return;
                
                while (usedTree.Contains((difficulty, index)))
                {
                    index = Random.Range(0, count - 1);
                }
                third = _dict.GetValueOrDefault((difficulty, 0));
                isSetThird = true;
                usedTree.Add((difficulty, index));
            }
            else if (!isSetFourth) 
            {
                if(count <= 3) return;
                
                while (usedTree.Contains((difficulty, index)))
                {
                    index = Random.Range(0, count - 1);
                }
                
                fourth = _dict.GetValueOrDefault((difficulty, 0));
                isSetFourth = true;
                usedTree.Add((difficulty, index));
            }
            else if (!isSetFifth) 
            {
                if(count <= 4) return;
                
                while (usedTree.Contains((difficulty, index)))
                {
                    index = Random.Range(0, count - 1);
                }
                
                fifth = _dict.GetValueOrDefault((difficulty, 0));
                isSetFifth = true;
            }
            else return;
        }
        
    }
}
