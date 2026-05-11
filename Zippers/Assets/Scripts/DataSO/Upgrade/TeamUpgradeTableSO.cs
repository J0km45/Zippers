using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 팀 업그레이드 시트의 모든 행을 단일 .asset에 담는 테이블 SO.
/// 시트 한 줄 = TeamUpgradeEntry 1개. UpgradeId(int) 키 기반 사전으로 O(1) 조회.
///
/// 사용 예:
///   var entry = LocalDataAccess.Instance.Game.GetTeamUpgrade(51009);
///   Debug.Log(entry.UpgradeName);   // "기동 훈련"
///   Debug.Log(entry.Description);   // "모든 플레이어의 이동속도가 n% 증가한다."
/// </summary>
[CreateAssetMenu(menuName = "Zippers/Team Upgrade Table")]
public class TeamUpgradeTableSO : ScriptableObject
{
    // 시트 컬럼 수 (UpgradeId, UpgradeName, MaxLevel, StatKey, ApplyType,
    //              ValuePerLevel, BaseCost, CostIncrease, Description)
    private const int ExpectedColumnCount = 9;

    [SerializeField] private List<TeamUpgradeEntry> _entries = new();

    // 런타임 조회 가속용 사전. _entries가 바뀌면 EnsureIndex에서 다시 빌드.
    private Dictionary<int, TeamUpgradeEntry> _byId;

    /// <summary>현재 보관 중인 엔트리 수.</summary>
    public int EntryCount => _entries.Count;

    /// <summary>모든 UpgradeId 순회.</summary>
    public IEnumerable<int> Ids
    {
        get
        {
            foreach (var e in _entries) yield return e.UpgradeId;
        }
    }

    /// <summary>
    /// 시트 줄들을 받아 _entries를 채운다.
    /// 호출 후 _byId는 무효화되어 다음 조회 시 재빌드된다.
    /// </summary>
    public void LoadFromSheet(char split, string[] lines, int headerRowCount = 1)
    {
        if (lines == null)
        {
            DebugTool.Error("[TeamUpgradeTableSO] lines가 null - 로드 중단", DebugType.Data, this);
            return;
        }

        _entries.Clear();
        _byId = null;

        for (int i = headerRowCount; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] cols = line.Split(split);
            if (cols.Length < ExpectedColumnCount)
            {
                DebugTool.Error(
                    $"[TeamUpgradeTableSO] {i}번째 줄 컬럼 수 부족: {cols.Length}/{ExpectedColumnCount}",
                    DebugType.Data, this);
                continue;
            }

            try
            {
                if (!int.TryParse(cols[0], out int upgradeId))
                {
                    DebugTool.Error(
                        $"[TeamUpgradeTableSO] {i}번째 줄 UpgradeId 파싱 실패: '{cols[0]}'",
                        DebugType.Data, this);
                    continue;
                }

                if (!int.TryParse(cols[2], out int maxLevel))
                {
                    DebugTool.Error(
                        $"[TeamUpgradeTableSO] {i}번째 줄 MaxLevel 파싱 실패: '{cols[2]}' (UpgradeId={upgradeId})",
                        DebugType.Data, this);
                    continue;
                }

                if (!Enum.TryParse<TeamUpgradeStatKey>(cols[3].Trim(), false, out var statKey))
                {
                    DebugTool.Error(
                        $"[TeamUpgradeTableSO] {i}번째 줄 StatKey 파싱 실패: '{cols[3]}' (UpgradeId={upgradeId})",
                        DebugType.Data, this);
                    continue;
                }

                if (!Enum.TryParse<TeamUpgradeApplyType>(cols[4].Trim(), false, out var applyType))
                {
                    DebugTool.Error(
                        $"[TeamUpgradeTableSO] {i}번째 줄 ApplyType 파싱 실패: '{cols[4]}' (UpgradeId={upgradeId})",
                        DebugType.Data, this);
                    continue;
                }

                if (!float.TryParse(cols[5], out float valuePerLevel))
                {
                    DebugTool.Error(
                        $"[TeamUpgradeTableSO] {i}번째 줄 ValuePerLevel 파싱 실패: '{cols[5]}' (UpgradeId={upgradeId})",
                        DebugType.Data, this);
                    continue;
                }

                if (!int.TryParse(cols[6], out int baseCost))
                {
                    DebugTool.Error(
                        $"[TeamUpgradeTableSO] {i}번째 줄 BaseCost 파싱 실패: '{cols[6]}' (UpgradeId={upgradeId})",
                        DebugType.Data, this);
                    continue;
                }

                if (!int.TryParse(cols[7], out int costIncrease))
                {
                    DebugTool.Error(
                        $"[TeamUpgradeTableSO] {i}번째 줄 CostIncrease 파싱 실패: '{cols[7]}' (UpgradeId={upgradeId})",
                        DebugType.Data, this);
                    continue;
                }

                // Description은 마지막 컬럼이라 CSV에서 쉼표가 들어가면 split 시 분할될 수 있다.
                // cols[8..]을 다시 join하여 원본 텍스트를 복구한다. (TSV에선 무해)
                string description = cols.Length > ExpectedColumnCount
                    ? string.Join(split, cols, 8, cols.Length - 8).TrimEnd('\r')
                    : cols[8].TrimEnd('\r');

                var entry = new TeamUpgradeEntry
                {
                    UpgradeId     = upgradeId,
                    UpgradeName   = cols[1],
                    MaxLevel      = maxLevel,
                    StatKey       = statKey,
                    ApplyType     = applyType,
                    ValuePerLevel = valuePerLevel,
                    BaseCost      = baseCost,
                    CostIncrease  = costIncrease,
                    Description   = description,
                };

                _entries.Add(entry);
            }
            catch (Exception e)
            {
                DebugTool.Error(
                    $"[TeamUpgradeTableSO] {i}번째 줄 파싱 실패: {e.Message}",
                    DebugType.Data, this);
            }
        }

        DebugTool.Log(
            $"[TeamUpgradeTableSO] 시트 로드 완료 (엔트리 {_entries.Count}개)",
            DebugType.Data, this);
    }

    /// <summary>
    /// UpgradeId로 엔트리 조회. 없으면 null + Warning.
    /// </summary>
    public TeamUpgradeEntry GetEntry(int upgradeId)
    {
        EnsureIndex();
        if (_byId.TryGetValue(upgradeId, out var entry)) return entry;

        DebugTool.Warning(
            $"[TeamUpgradeTableSO] UpgradeId {upgradeId} 없음",
            DebugType.Data, this);
        return null;
    }

    private void EnsureIndex()
    {
        if (_byId != null) return;
        _byId = new Dictionary<int, TeamUpgradeEntry>(_entries.Count);

        foreach (var entry in _entries)
        {
            if (_byId.ContainsKey(entry.UpgradeId))
            {
                DebugTool.Warning(
                    $"[TeamUpgradeTableSO] UpgradeId {entry.UpgradeId} 중복 - 첫 항목 유지",
                    DebugType.Data, this);
                continue;
            }
            _byId[entry.UpgradeId] = entry;
        }
    }
}
