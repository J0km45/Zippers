using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerTransformList : MonoBehaviour
{
    public static PlayerTransformList instance = null;

    public List<Transform> _playerPosition = new List<Transform>();

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
        //TODO : 멀티 전환 시 살아있는 플레이어 관리는 서버에서 관리해야 됨
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
        //TODO : 살아있는 플레이어 목록 제거는 서버 PlayerDied이벤트를 기존으로 처리해야됨 
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
        //TODO : 모든 플레이어 사망 판단은 클라이언트 리스트가 아니라 서버 AllivePlayerList 기준으로 처리해야됨
        if (_playerPosition.Count > 0)
        {
            return;
        }

        //TODO : 전멸 이벤트는  서버에서 AllPlayersDead를 확정한 뒤 클라이언트에서 전달해야됨
        OnAllPlayerDead?.Invoke();
    }
}
