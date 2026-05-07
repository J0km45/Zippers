using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 같은 ClassType에 속하는 UpgradeEntry 묶음.
/// 인스펙터에서 업그레이드 데이터를 시각화할 때 사용된다.
/// </summary>
[Serializable]
public class PlayerUpgradeGroup
{
    [Tooltip("이 그룹의 클래스 타입 (Melee/Rifle/Shotgun/Pistol)")]
    public WeaponType ClassType;
    [Tooltip("이 클래스의 업그레이드 엔트리들 (시트 순서)")]
    public List<UpgradeEntry> Entries = new();
}
