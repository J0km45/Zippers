using UnityEngine;
using System.Collections.Generic;

public class PlayerTransformList : MonoBehaviour
{
    public static PlayerTransformList instance = null;

    private readonly List<Transform> _playerPosition = new List<Transform>();


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            return;
        }

        else
        {
            Destroy(this.gameObject);
        }
    }

    public static PlayerTransformList Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    //플레이어 위치 저장
    public void AddPlayer(Transform playerPosition)
    {
        _playerPosition.Add(playerPosition);
        DebugTool.Log("플레이어 위치 정보 저장", DebugType.Character, this);
    }

    //플레이어 위치 삭제
    public void DeletePlayer(Transform playerPosition)
    {
        _playerPosition.Remove(playerPosition);
        DebugTool.Log("플레이어 위치 정보 삭제", DebugType.Character, this);
    }

    public int GetPlayerCount()
    {
        return _playerPosition.Count;
    }

    //null이 된 플레이어 List에서 제거
    public void RemoveNullPlayer()
    {
        for (int i = 0; i < _playerPosition.Count - 1; i++)
        {
            if (_playerPosition[i] != null)
            {
                continue;
            }

            _playerPosition.RemoveAt(i);
        }
    }
}
