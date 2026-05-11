using System;
using UnityEngine;

/// <summary>
/// 팀 업그레이드 시트의 한 행에 해당하는 데이터 컨테이너.
/// 시트 컬럼명을 그대로 필드명으로 사용한다.
///
/// 사용 예:
///   var entry = LocalDataAccess.Instance.Game.GetTeamUpgrade(51009);
///   string name = entry.UpgradeName;     // "기동 훈련"
///   string desc = entry.Description;     // "모든 플레이어의 이동속도가 n% 증가한다."
///   int maxLv   = entry.MaxLevel;        // 5
/// </summary>
[Serializable]
public class TeamUpgradeEntry
{
    [Tooltip("업그레이드 ID (시트 첫 컬럼, 51001~)")]
    public int UpgradeId;

    [Tooltip("업그레이드 이름 (한글 표시명)")]
    public string UpgradeName;

    [Tooltip("최대 레벨")]
    public int MaxLevel;

    [Tooltip("스탯 키 (시트 문자열과 enum 멤버명이 일치해야 함)")]
    public TeamUpgradeStatKey StatKey;

    [Tooltip("적용 방식 (Add 또는 AddPercent)")]
    public TeamUpgradeApplyType ApplyType;

    [Tooltip("레벨당 적용 값 (ApplyType에 따라 의미 다름)")]
    public float ValuePerLevel;

    [Tooltip("기본 비용 (레벨 0 → 1 강화 시 비용)")]
    public int BaseCost;

    [Tooltip("레벨당 비용 증가량")]
    public int CostIncrease;

    [Tooltip("업그레이드 설명 (UI 표시용)")]
    [TextArea]
    public string Description;
}
