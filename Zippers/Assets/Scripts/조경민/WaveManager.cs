using Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WaveManager : NetworkBehaviour
{
    [SerializeField] private ZombieSpawnManager _zombieSpawnManager;
    [SerializeField] private ZombieCountManager _zombieCountManager;
    [SerializeField] private WaveUIController _waveUIController;

    // 노드 클리어 이벤트(인자: 클리어한 노드 인덱스)
    public event Action<int> OnBattleNodeCleared;
    
    // 전투, 웨이브 시작 시 연출
    public event Action OnBattleStarted;
    public event Action OnNextWave;

    [ClientRpc]
    private void SetWaveUIClientRpc(int currentWave, int maxWave)
    {
        _waveUIController.SetCurrentWaveText(currentWave);
        _waveUIController.SetMaxWaveText(maxWave);
    }

    public void StartBattleNode(int battleNodeIndex)
    {
        if (!IsServer) return;

        StartCoroutine(RunBattleNode(battleNodeIndex));
    }

    private IEnumerator RunBattleNode(int battleNodeIndex)
    {
        if (!IsServer) yield break;

        SetWaveUIClientRpc(0, 0);

        // 전체 카운트 초기화
        _zombieCountManager.ResetTotalCount();
        

        List<WaveInfoSO> waves = LocalDataAccess.Instance.Game.GetWaveInfo(battleNodeIndex);

        if (waves.Count == 0)
        {
            DebugTool.Log($"BattleNodeIndex {battleNodeIndex} 웨이브 없음", DebugType.Zombie, this);
            yield break;
        }

        // 총 웨이브 UI 설정
        SetWaveUIClientRpc(0, waves.Count);

        for (int i = 0; i < waves.Count; i++)
        {
            if (!IsServer) yield break;

            WaveInfoSO waveInfo = waves[i];

            // 현재 웨이브 UI 설정
            SetWaveUIClientRpc(i + 1, waves.Count);

            // 첫 웨이브 시작 대기
            if (i == 0)
            {
                DebugTool.Log($"첫 웨이브 스폰 시작 대기({waveInfo.StartDelay}초)", DebugType.Zombie, this);
                OnBattleStarted?.Invoke();
                yield return new WaitForSeconds(waveInfo.StartDelay);
            }
            else
                OnNextWave?.Invoke();
            
            DebugTool.Log($"웨이브 ({waveInfo.WaveIndex}) {waveInfo.WaveId} 스폰 시작", DebugType.Zombie, this);
            // 카운트 초기화
            _zombieCountManager.ResetCount();

            // 웨이브 시작
            _zombieSpawnManager.StartWave(waveInfo.WaveId);

            yield return CheckWaveEnd(waveInfo);
            if (_zombieSpawnManager.IsSpawnStopped) yield break;

            DebugTool.Log($"웨이브 ({waveInfo.WaveIndex}) {waveInfo.WaveId} 스폰 종료", DebugType.Zombie, this);

            // 다음 웨이브 시작 대기
            if (i < waves.Count - 1 && waveInfo.NextWaveDelay > 0f)
            {
                DebugTool.Log($"다음 웨이브 스폰 시작 대기({waveInfo.NextWaveDelay}초)", DebugType.Zombie, this);
                yield return new WaitForSeconds(waveInfo.NextWaveDelay);
            }
        }

        DebugTool.Log("모든 웨이브 스폰 종료", DebugType.Zombie, this);

        yield return WaitAllZombiesDead();

        if (_zombieSpawnManager.IsSpawnStopped) yield break;

        DebugTool.Log($"{battleNodeIndex} 노드 클리어", DebugType.Zombie, this);
        OnBattleNodeCleared?.Invoke(battleNodeIndex);
    }

    private IEnumerator CheckWaveEnd(WaveInfoSO waveInfo)
    {
        float timer = 0f;

        while (timer < waveInfo.TimeLimit)
        {
            if (!IsServer) yield break;
            if (_zombieSpawnManager.IsSpawnStopped) yield break;

            // 웨이브 클리어 조건 체크
            if (_zombieCountManager.IsWaveCleared)
            {
                DebugTool.Log($"웨이브 ({waveInfo.WaveIndex}) {waveInfo.WaveId} 클리어", DebugType.Zombie, this);
                yield break;
            }
            
            timer += Time.deltaTime;
            yield return null;
        }

        DebugTool.Log($"웨이브 ({waveInfo.WaveIndex}) {waveInfo.WaveId} 제한 시간 종료", DebugType.Zombie, this);
    }

    private IEnumerator WaitAllZombiesDead()
    {
        while (IsServer && !_zombieSpawnManager.IsSpawnStopped && !_zombieCountManager.IsNodeCleared)
        {
            yield return null;
        }
    }
}
