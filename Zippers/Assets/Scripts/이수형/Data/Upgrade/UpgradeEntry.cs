using System;
using UnityEngine;

/// <summary>
/// 시트의 업그레이드 한 줄에 해당하는 데이터 컨테이너.
/// ClassType은 그룹(PlayerUpgradeGroup) 레벨에서 관리하므로 여기엔 없음.
/// </summary>
[Serializable]
public class UpgradeEntry
{
    [Tooltip("업그레이드 ID (시트 첫 컬럼)")]
    public int Id;
    [Tooltip("업그레이드 이름 (한글 표시명)")]
    public string UpgradeName;
    [Tooltip("최대 레벨")]
    public int MaxLevel;
    [Tooltip("스탯 키 (예: MaxHealth, Damage, ...)")]
    public string StatKey;
    [Tooltip("적용 방식 (Add 등)")]
    public string ApplyType;
    [Tooltip("레벨당 적용 값")]
    public float ValuePerLevel;
    [Tooltip("기본 비용")]
    public int BaseCost;
    [Tooltip("레벨당 비용 증가량")]
    public int CostIncrease;
    [Tooltip("활성화 여부 (FALSE이면 UI에서 비활성/회색 처리 권장)")]
    public bool IsEnabled;
}
