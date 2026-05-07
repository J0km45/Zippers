using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 클래스(Melee/Rifle/Shotgun/Pistol)의 업그레이드 데이터를 단일 .asset에 담는 테이블 SO.
/// 시트 한 줄 = UpgradeEntry 1개. 같은 ClassType끼리 PlayerUpgradeGroup으로 묶여 보관된다.
/// 인스펙터에는 그룹 List 형태로 노출되고, 런타임 조회는 lazy build된 ClassUpgradeData 사전으로 O(1).
/// </summary>
[CreateAssetMenu(menuName = "Zippers/Player Upgrade Table")]
public class PlayerUpgradeTableSO : ScriptableObject
{
    [SerializeField] private List<PlayerUpgradeGroup> _groups = new();

    // 런타임 조회 가속용 사전. _groups가 바뀌면 EnsureIndex에서 다시 빌드.
    private Dictionary<WeaponType, ClassUpgradeData> _byClassType;

    /// <summary>현재 보관 중인 그룹(ClassType) 수.</summary>
    public int GroupCount => _groups.Count;

    /// <summary>모든 ClassType 순회.</summary>
    public IEnumerable<WeaponType> ClassTypes
    {
        get
        {
            foreach (var g in _groups) yield return g.ClassType;
        }
    }

    /// <summary>
    /// 시트 줄들을 받아 _groups를 채운다.
    /// ClassType별로 묶이고, IsEnabled FALSE 행도 포함된다.
    /// 호출 후 _byClassType은 무효화되어 다음 조회 시 재빌드된다.
    /// </summary>
    public void LoadFromSheet(char split, string[] lines, int headerRowCount = 1)
    {
        if (lines == null)
        {
            DebugTool.Error("[PlayerUpgradeTableSO] lines가 null - 로드 중단", DebugType.Data, this);
            return;
        }

        _groups.Clear();
        _byClassType = null;

        var temp = new Dictionary<WeaponType, PlayerUpgradeGroup>();

        for (int i = headerRowCount; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] cols = line.Split(split);
            if (cols.Length < 10)
            {
                DebugTool.Error(
                    $"[PlayerUpgradeTableSO] {i}번째 줄 컬럼 수 부족: {cols.Length}/10",
                    DebugType.Data, this);
                continue;
            }

            try
            {
                int id = int.Parse(cols[0]);

                if (!Enum.TryParse<WeaponType>(cols[1], true, out var classType))
                {
                    DebugTool.Error(
                        $"[PlayerUpgradeTableSO] {i}번째 줄 ClassType 파싱 실패: '{cols[1]}'",
                        DebugType.Data, this);
                    continue;
                }

                bool isEnabled = cols[9].Trim().Equals("TRUE", StringComparison.OrdinalIgnoreCase);

                var entry = new UpgradeEntry
                {
                    Id            = id,
                    UpgradeName   = cols[2],
                    MaxLevel      = int.Parse(cols[3]),
                    StatKey       = cols[4],
                    ApplyType     = cols[5],
                    ValuePerLevel = float.Parse(cols[6]),
                    BaseCost      = int.Parse(cols[7]),
                    CostIncrease  = int.Parse(cols[8]),
                    IsEnabled     = isEnabled,
                };

                if (!temp.TryGetValue(classType, out var group))
                {
                    group = new PlayerUpgradeGroup { ClassType = classType };
                    temp[classType] = group;
                    _groups.Add(group);
                }
                group.Entries.Add(entry);
            }
            catch (Exception e)
            {
                DebugTool.Error(
                    $"[PlayerUpgradeTableSO] {i}번째 줄 파싱 실패: {e.Message}",
                    DebugType.Data, this);
            }
        }

        DebugTool.Log(
            $"[PlayerUpgradeTableSO] 시트 로드 완료 (그룹 {_groups.Count}개)",
            DebugType.Data, this);
    }

    /// <summary>
    /// ClassType 문자열로 업그레이드 데이터 조회. 대소문자 무관.
    /// 잘못된 문자열이거나 데이터 없으면 null + Warning.
    /// </summary>
    public ClassUpgradeData GetUpgrade(string classType)
    {
        if (string.IsNullOrEmpty(classType))
        {
            DebugTool.Warning("[PlayerUpgradeTableSO] GetUpgrade: classType이 null/empty", DebugType.Data, this);
            return null;
        }
        if (!Enum.TryParse<WeaponType>(classType, true, out var wt))
        {
            DebugTool.Warning(
                $"[PlayerUpgradeTableSO] 알 수 없는 ClassType: '{classType}'",
                DebugType.Data, this);
            return null;
        }
        return GetUpgrade(wt);
    }

    /// <summary>WeaponType enum으로 업그레이드 데이터 조회.</summary>
    public ClassUpgradeData GetUpgrade(WeaponType classType)
    {
        EnsureIndex();
        return _byClassType.TryGetValue(classType, out var data) ? data : null;
    }

    private void EnsureIndex()
    {
        if (_byClassType != null) return;
        _byClassType = new Dictionary<WeaponType, ClassUpgradeData>(_groups.Count);

        foreach (var group in _groups)
        {
            ClassUpgradeData data = group.ClassType switch
            {
                WeaponType.Melee   => new MeleeUpgradeData   { ClassType = group.ClassType },
                WeaponType.Rifle   => new RifleUpgradeData   { ClassType = group.ClassType },
                WeaponType.Shotgun => new ShotgunUpgradeData { ClassType = group.ClassType },
                WeaponType.Pistol  => new PistolUpgradeData  { ClassType = group.ClassType },
                _ => null,
            };

            if (data == null)
            {
                DebugTool.Warning(
                    $"[PlayerUpgradeTableSO] 알 수 없는 ClassType 그룹: {group.ClassType}",
                    DebugType.Data, this);
                continue;
            }

            foreach (var entry in group.Entries)
            {
                data.AssignByStatKey(entry.StatKey, entry);
            }

            _byClassType[group.ClassType] = data;
        }
    }
}
