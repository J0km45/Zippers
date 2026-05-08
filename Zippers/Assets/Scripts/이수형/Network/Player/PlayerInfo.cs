/// <summary>
/// 한 플레이어의 로비 상태를 한 번에 표현하는 읽기 전용 스냅샷.
/// PlayerProperty / SessionProperty 어디에 보관되든, UI 측은 이 구조로만 접근하면 되도록 통일.
///
/// 보관 권위:
/// - SlotIndex   : SessionProperty (호스트 결정 / 모두 읽음)
/// - PlayerClass : PlayerProperty (본인 결정, 호스트가 ServerRpc 로 사전 검증)
/// - IsReady     : PlayerProperty (본인 결정)
///
/// "클래스 선택 완료 여부" 는 PlayerClass != None 한 가지로 표현 (별도 ClassConfirmed 필드 없음).
/// 클래스 클릭 = 즉시 변경 + 모델 spawn, 같은 클래스 다시 클릭 또는 None = 취소 + 모델 despawn.
///
/// 확장 여지: 외형/색상/팀 식별자/누적 스탯 등은 같은 패턴으로 PlayerProperty 또는 SessionProperty 에 추가.
/// 단, 닉네임은 사용하지 않음.
/// </summary>
public readonly struct PlayerInfo
{
    /// <summary>호스트가 배정한 슬롯 (0 ~ MaxPlayers-1). 미배정은 -1.</summary>
    public readonly int SlotIndex;

    /// <summary>본인이 선택한 클래스. 미선택은 None.</summary>
    public readonly PlayerClass PlayerClass;

    /// <summary>준비 완료 여부. PlayerClass != None 일 때만 true 가능.</summary>
    public readonly bool IsReady;

    public PlayerInfo(int slotIndex, PlayerClass playerClass, bool isReady)
    {
        SlotIndex   = slotIndex;
        PlayerClass = playerClass;
        IsReady     = isReady;
    }

    /// <summary>아직 슬롯 배정도 받지 못한 초기 상태.</summary>
    public static PlayerInfo Unassigned => new PlayerInfo(-1, PlayerClass.None, false);

    /// <summary>편의 속성: 클래스를 선택한 상태인가.</summary>
    public bool HasClass => PlayerClass != PlayerClass.None;
}
