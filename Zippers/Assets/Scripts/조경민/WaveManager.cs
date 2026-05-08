using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private ZombieSpawnManager _zombieSpawnManager;
    [SerializeField] private ZombieCountManager _zombieCountManager;

    // 노드 클리어 이벤트(인자: 클리어한 노드 인덱스)
    public event Action<int> OnBattleNodeCleared;

    public void OnStart(int battleNodeIndex)
    {
        // TODO : 나중에 삭제
        // 테스트용(버튼)
        StartBattleNode(battleNodeIndex);
    }

    public void StartBattleNode(int battleNodeIndex)
    {
        StartCoroutine(RunBattleNode(battleNodeIndex));
    }

    private IEnumerator RunBattleNode(int battleNodeIndex)
    {
        // 전체 카운트 초기화
        _zombieCountManager.ResetTotalCount();

        List<WaveInfoSO> waves = LocalDataAccess.Instance.Game.GetWaveInfo(battleNodeIndex);

        if (waves.Count == 0)
        {
            DebugTool.Log($"BattleNodeIndex {battleNodeIndex} 웨이브 없음", DebugType.Zombie, this);
            yield break;
        }

        for (int i = 0; i < waves.Count; i++)
        {
            WaveInfoSO waveInfo = waves[i];

            // 첫 웨이브 시작 대기
            if (i == 0)
            {
                DebugTool.Log($"첫 웨이브 스폰 시작 대기({waveInfo.StartDelay}초)", DebugType.Zombie, this);
                yield return new WaitForSeconds(waveInfo.StartDelay);
            }

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
            if (_zombieSpawnManager.IsSpawnStopped) yield break;

            // 웨이브 클리어 조건 체크
            if (_zombieCountManager.IsWaveCleared)
            {
                DebugTool.Log($"웨이브 ({waveInfo.WaveIndex}) {waveInfo.WaveId} 클리어", DebugType.Zombie, this);
                yield break;
            }
            // TODO: 네트워크 적용 후 서버시간으로 변경
            timer += Time.deltaTime;
            yield return null;
        }

        DebugTool.Log($"웨이브 ({waveInfo.WaveIndex}) {waveInfo.WaveId} 제한 시간 종료", DebugType.Zombie, this);
    }

    private IEnumerator WaitAllZombiesDead()
    {
        while (!_zombieSpawnManager.IsSpawnStopped && !_zombieCountManager.IsNodeCleared)
        {
            yield return null;
        }
    }
}
