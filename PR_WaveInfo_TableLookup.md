## PR 종류
- [ ] 기능 추가
- [x] 수정
- [ ] 버그
- [ ] 개발
- [ ] 기획
- [ ] 질의응답

## 작업 내용
- `GetWaveInfo` 검색 방식을 **BattleNodeIndex 기반 리스트 반환**으로 변경 (기존 WaveId 단일 조회 폐기)
- WaveSpawn과 동일한 그룹화 패턴을 WaveInfo에도 적용 (`WaveInfoTableSO` + `WaveInfoGroup` 추가)
- 사용 안 하던 `GetWave` / `WaveBundle` 제거

## 작업 이유
- 인게임에서 "특정 배틀 노드의 모든 웨이브를 순서대로" 가져오는 패턴이 필요한데, 기존 `GetWaveInfo(waveId)`는 1건씩만 반환해서 호출자가 매번 직접 그룹핑해야 했음
- WaveSpawn은 이미 같은 패턴(`GetWaveSpawns(waveId)` → 정렬된 리스트)으로 동작 중이라 두 API 형태를 통일

## 변경 사항
- 신규 `Assets/Scripts/이수형/Data/Wave/WaveInfoGroup.cs` — `BattleNodeIndex` + `List<WaveInfoSO>` (Serializable, WaveSpawnGroup 패턴 미러)
- 신규 `Assets/Scripts/DataSO/Wave/WaveInfoTableSO.cs` — 단일 SO. `Build(infos)`로 BattleNodeIndex 그룹핑 + WaveIndex 정렬, `GetEntries(battleNodeIndex)` 빠른 조회 (lazy 인덱스)
- 수정 `DataManager.cs` — `_waveInfoTable` 인스펙터 필드 추가. WaveInfo 시트 로드 후 `Build` + `RegisterWaveInfoTable` 호출
- 수정 `GameDataModule.cs` — `_waveInfos` 사전 제거, `_waveInfoTable` 참조로 교체. `GetWaveInfo(int battleNodeIndex) → List<WaveInfoSO>`로 시그니처 변경. `GetWave`, `GetWaveInfoSilent`, `GetAllWaveIds` 제거. `GetAllBattleNodeIndices` 신규
- 수정 `DataAccessTest.cs` — 새 시그니처에 맞춰 테스트 갱신 (BattleNode 0 / 2 조회)
- 삭제 `WaveBundle.cs` (`GetWave` 사라지므로 불필요)
- 갱신 `DataSystem_API_Guide.md` — WaveInfo 섹션 새 시그니처로 갱신

## 확인 방법
- [x] 정상 실행 확인
- [x] 직접 플레이 또는 동작 확인 (`(Init)DataLoadScene` → `Data` 씬에서 `DataAccessTest`의 WaveInfo 버튼)
- [x] 오류 로그 확인 (DebugTool DebugType.Data — Build 로그, Register 로그)
- [ ] 팀원 확인 필요 없음 / 또는 확인 필요 (외부에서 `GetWaveInfo(waveId)` 또는 `GetWave` 직접 호출하던 코드가 있다면 마이그레이션 필요 — 현재 프로젝트 내 검색 기준 외부 사용처 없음)

## 리뷰 포인트
- `WaveInfoTableSO`가 시트 직접 로드 대신 `Build(infos)`로 외부에서 데이터 받아 그룹핑함 (WaveSpawnTableSO는 직접 시트 파싱 — 둘의 차이 의도적)
- BattleNodeIndex와 WaveIndex 모두 시트에서 0-based로 입력되어 있어야 함 (list 인덱스와 자연스럽게 매칭)
- `_waveInfoTable` 인스펙터 슬롯에 `WaveInfoTable.asset` 할당 안 하면 Error 로그 + 빈 리스트 반환 (DataManager prefab 신규 슬롯 누락 주의)

## 관련 이슈
- close #
- related to #

## 참고 자료
- API 사용법 가이드: `DataSystem_API_Guide.md`
- 종합 데이터 시트: https://docs.google.com/spreadsheets/d/1H4POPGGxh0x_jcwinL-_VVhK1FWRPD0jASYWtFIzxrQ/edit
