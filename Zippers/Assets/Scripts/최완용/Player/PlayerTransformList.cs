using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerTransformList : MonoBehaviour
{
    public static PlayerTransformList instance = null;

    public readonly List<Transform> _playerPosition = new List<Transform>();

    public event Action OnAllPlayerDead;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            return;
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
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
        if (playerPosition ==null)
        {
            return;
        }
        if(_playerPosition.Contains(playerPosition))
        {
            return;
        }
        _playerPosition.Add(playerPosition);
        DebugTool.Log("플레이어 위치 정보 저장", DebugType.Character, this);
    }

    //플레이어 위치 삭제
    public void DeletePlayer(Transform playerPosition)
    {
        if (playerPosition == null)
        {
            return;
        }

        bool isRemoved = _playerPosition.Remove(playerPosition);
        if (!isRemoved)
        {
            return;
        }

        CheckAllPlayerDead();
    }

    public int GetPlayerCount()
    {
        return _playerPosition.Count;
    }

    public Transform GetClosestPlayer(Vector3 pos)
    {
        if(_playerPosition.Count == 0) return null;

        Transform closestPlayer = null;
        float closestDistance = float.MaxValue;

        foreach (Transform player in _playerPosition)
        {
            if (player == null) continue;
            float distance = Vector3.Distance(pos, player.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

    //null이 된 플레이어 List에서 제거
    public void RemoveNullPlayer()
    {
        for (int i = _playerPosition.Count -1; i >=0; i--)
        {
            if (_playerPosition[i] != null)
            {
                continue;
            }

            _playerPosition.RemoveAt(i);
        }
    }

    private void CheckAllPlayerDead()
    {
        if(_playerPosition.Count > 0)
        {
            return;
        }

        OnAllPlayerDead?.Invoke();
    }
}
