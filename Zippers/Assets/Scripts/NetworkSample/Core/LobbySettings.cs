using UnityEngine;

/// <summary>
/// 로비/게임 시작 흐름의 튜닝값을 모아둔 ScriptableObject.
/// 씬 진입점은 SceneId enum + Build Settings 순서로 관리 (Assets/Scripts/Scene/SceneId.cs)
/// </summary>
[CreateAssetMenu(fileName = "LobbySettings", menuName = "Lobby/Lobby Settings")]
public class LobbySettings : ScriptableObject
{
    [Header("Players")]
    public int MaxPlayers = 4;
    public int MinPlayersToStart = 2;

    [Header("Game Start")]
    public float GameRestartCooldownSec = 5f;

    [Header("Relay")]
    [Tooltip("Relay region 코드. 비워두면 SDK가 QoS로 자동 선택(가끔 먼 region 잡혀 NGO 핸드셰이크가 5초 timeout). 한국 기준 'asia-northeast3'(서울) 또는 'asia-northeast1'(도쿄) 권장")]
    public string RelayRegion = "asia-northeast3";
}
