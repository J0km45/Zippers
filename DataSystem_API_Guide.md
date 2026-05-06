# LocalDataAccess API 가이드

게임 콘텐츠/밸런스 데이터(클래스, 좀비, 웨이브, 스폰)를 인게임 스크립트에서 가져다 쓰는 방법.

---

## TL;DR (핵심 한 줄)

게임플레이 씬에선 그냥 호출하면 됨. 대기 코드 필요 없음:
```csharp
var wave = LocalDataAccess.Instance.Game.GetWave(30001);
```

---

## 워크플로우 (왜 이게 가능한가)

```
[씬 1: Title / Loading 씬]
 ├ DataManager       시트 다운로드 시작 (수백 ms)
 ├ LocalDataAccess   DontDestroyOnLoad
 └ 데이터 로드 완료 → "Play" 버튼 활성화 / 다음 씬 전환

         ↓ 씬 전환

[씬 2~N: Gameplay 씬]
 └ LocalDataAccess만 살아있음 (이미 데이터 로드 완료 상태)
   → 게임플레이 스크립트는 Awake/Start에서 GetXxx 바로 호출
```

**핵심**: 시트 로드는 Title 씬에서 다 끝남. 유저가 "Play" 누르고 게임플레이 씬 진입할 시점엔 데이터가 무조건 준비됨. **게임플레이 씬 스크립트는 IsReady/OnReady 신경 안 써도 됨.**

---

## 1. 씬 세팅

### Title / Loading 씬 (한 번만)

| GameObject | 컴포넌트 | 인스펙터 할당 |
|---|---|---|
| `LocalDataAccess` | `LocalDataAccess` | (없음) |
| `DataManager` | `DataManager` | 시트 URL들 + 데이터 SO 리스트 + `WaveSpawnTable.asset` |

`LocalDataAccess`는 자동으로 `DontDestroyOnLoad`. 이후 모든 씬에서 살아있음.

### Gameplay 씬

별도 세팅 불필요. `LocalDataAccess.Instance`로 어디서나 접근.

---

## 2. 데이터 종류 & API

### Player Class
```csharp
PlayerClassDataSO cls = LocalDataAccess.Instance.Game.GetClass(10001);
Debug.Log(cls.MaxHealth);
Debug.Log(cls.WeaponType);
```

### Zombie Stat
```csharp
ZombieStatSO stat = LocalDataAccess.Instance.Game.GetZombieStat(20001);
Debug.Log(stat.MaxHp);
Debug.Log(stat.MoveSpeed);
```

### Wave Info
```csharp
WaveInfoSO info = LocalDataAccess.Instance.Game.GetWaveInfo(30001);
Debug.Log(info.TimeLimit);
Debug.Log(info.SpawnGroupCount);
```

### Wave Spawn 리스트
```csharp
List<WaveSpawnEntry> spawns = LocalDataAccess.Instance.Game.GetWaveSpawns(30001);
// GroupIndex 0, 1, 2 ... 순으로 정렬됨
```

### Wave 통합 조회 ★ 추천
```csharp
WaveBundle wave = LocalDataAccess.Instance.Game.GetWave(30001);
// wave.Info: WaveInfoSO
// wave.SpawnEntries: GroupIndex 순 정렬된 List<WaveSpawnEntry>

float timeLimit = wave.Info.TimeLimit;

foreach (WaveSpawnEntry spawn in wave.SpawnEntries)
{
    // spawn.SpawnId, spawn.GroupIndex, spawn.ZombieId, spawn.Count,
    // spawn.StartDelay, spawn.Interval, spawn.BatchCount, spawn.SpawnRadius
}
```

### 모든 ID 순회
```csharp
foreach (int id in LocalDataAccess.Instance.Game.GetAllClassIds()) { ... }
foreach (int id in LocalDataAccess.Instance.Game.GetAllZombieIds()) { ... }
foreach (int id in LocalDataAccess.Instance.Game.GetAllWaveIds()) { ... }
```

---

## 3. 게임플레이 스크립트 사용 예시

### 가장 일반적인 사용 (Awake/Start에서 그냥 호출)
```csharp
public class WaveSpawner : MonoBehaviour
{
    private void Start()
    {
        var wave = LocalDataAccess.Instance.Game.GetWave(30001);
        if (wave == null) return;

        StartCoroutine(RunWave(wave));
    }

    private IEnumerator RunWave(WaveBundle wave)
    {
        foreach (var spawn in wave.SpawnEntries)
        {
            yield return new WaitForSeconds(spawn.StartDelay);
            SpawnZombies(spawn.ZombieId, spawn.Count, spawn.BatchCount, spawn.Interval);
        }
    }
}
```

### 좀비 스탯 적용
```csharp
private void SpawnZombie(int zombieId)
{
    var stat = LocalDataAccess.Instance.Game.GetZombieStat(zombieId);
    if (stat == null) return;

    var zombie = Instantiate(zombiePrefab);
    zombie.GetComponent<ZombieController>().Init(stat);
}
```

### 특정 배틀 노드 웨이브들만 필터링
```csharp
foreach (int waveId in LocalDataAccess.Instance.Game.GetAllWaveIds())
{
    var wave = LocalDataAccess.Instance.Game.GetWave(waveId);
    if (wave.Info.BattleNodeIndex != currentNode) continue;
    // ...
}
```

---

## 4. Title 씬 전용 — OnReady / IsReady

Title 씬에선 시트 로드 완료 시점을 알아야 "Play 버튼 활성화" 또는 "자동 씬 전환"이 가능. 이때만 OnReady 사용:

```csharp
// TitleSceneController
public class TitleSceneController : MonoBehaviour
{
    [SerializeField] private Button _playButton;

    private void Start()
    {
        _playButton.interactable = false;
        LocalDataAccess.Instance.Game.OnReady += HandleDataReady;
    }

    private void OnDestroy()
    {
        if (LocalDataAccess.Instance != null)
            LocalDataAccess.Instance.Game.OnReady -= HandleDataReady;
    }

    private void HandleDataReady()
    {
        _playButton.interactable = true;
    }
}
```

> **편의 기능**: 이미 IsReady=true인 상태에서 OnReady 구독해도 즉시 호출. 늦게 구독해도 콜백 놓치지 않음.

게임플레이 씬에선 데이터가 이미 준비된 상태이니 이 패턴 쓸 일 없음.

---

## 5. 반환값 규칙

| 상황 | 반환 |
|---|---|
| 정상 조회 | 해당 데이터 |
| 잘못된 ID | `null` 또는 빈 리스트 + Warning 로그 |
| 미준비 상태 호출 (Title 씬에서 일찍 호출 시) | `null` 또는 빈 리스트 + Warning 로그 |
| `GetWave` Info 없음 | `null` |
| `GetWave` Spawn 없음 | Bundle 반환 (SpawnEntries는 빈 리스트) |
| `GetWaveSpawns` 스폰 0개 (정상 케이스) | 빈 리스트 (Warning 없음) |

→ **방어적 코드 권장**. null 체크 안 하면 NullReferenceException 가능.

---

## 6. FAQ

### Q. ID는 어디서 확인?
구글 스프레드시트 첫 컬럼:
- 클래스: 10001~
- 좀비: 20001~
- 웨이브: 30001~
- 스폰: 31001~

### Q. 시트 변경 후 게임 재시작 안 하고 반영?
현재는 게임 시작 시 1회 로드. 핫리로드 필요하면 별도 요청.

### Q. 게임플레이 씬에서 LocalDataAccess가 없어요!
Title 씬을 빌드 인덱스 **0번**으로 두고 거기서 시작해야 함. 직접 게임플레이 씬을 첫 씬으로 Play하면 LocalDataAccess가 안 만들어짐. 빌드/Play 시 항상 Title 씬부터 시작.

### Q. DataManager는 게임플레이 씬에 둬야 해?
**아니. Title 씬에만**. 게임플레이 씬에선 LocalDataAccess만 알면 됨 (DontDestroyOnLoad로 자동 보존).

### Q. 호스트에서 받아오는 데이터는?
`HostDataAccess` (별도, 미구현). 멀티플레이 동기화 데이터는 그쪽으로 분리 예정.

### Q. 인스펙터에 SO 미리 연결 안 하면?
시트에서 데이터 받아와서 메모리 인스턴스로 자동 생성됨 (Play 종료 시 휘발). .asset이 미리 등록돼있으면 그 .asset을 시트값으로 갱신.

---

## 7. DebugTool 로그 필터

데이터 시스템 로그는 모두 `DebugType.Data`. DebugTool 콘솔에서 Data 카테고리만 켜면 데이터 흐름만 볼 수 있음. 색상은 청록색(`#00D1B2`).

주요 로그:
- `[GameDataModule] 모든 데이터 준비 완료 (IsReady = true)` — 데이터 사용 가능 시점
- `[WaveSpawnTableSO] 시트 로드 완료 (그룹 N개)` — 스폰 테이블 로드 완료
- `[GameDataModule] WaveId XXX 없음` — 잘못된 ID 조회
- `미준비 상태에서 GetXxx 호출` — Title 씬에서 너무 일찍 호출 (게임플레이 씬에선 안 떠야 정상)

---

## 문의
데이터 시스템: 이수형
