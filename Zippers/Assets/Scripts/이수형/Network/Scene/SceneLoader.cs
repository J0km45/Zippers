using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환 진입점. 모든 씬 전환은 이 클래스를 통해서만 처리한다.
/// LoadLocal: NGO 미실행 상태에서 단일 클라이언트 씬 전환.
/// LoadNetworked: NGO 실행 중 호스트가 호출 — 모든 멤버에게 자동 동기화.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string _sceneName;
    [SerializeField] private bool _isLoadOnStart;

    private void Start()
    {
        if (!_isLoadOnStart) return;
        LoadScene();
    }

    /// <summary>인스펙터에 지정된 씬을 로컬 로드 (UI 버튼 OnClick 등에서 호출).</summary>
    public void LoadScene()
    {
        LoadLocal(_sceneName);
    }

    /// <summary>로컬 씬 로드 (SceneId 권장). NGO 미실행 상태에서 사용.</summary>
    public static void LoadLocal(SceneId id) => LoadLocal(id.GetName());

    /// <summary>로컬 씬 로드 (string).</summary>
    public static void LoadLocal(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    /// <summary>NGO 동기화 씬 로드 (SceneId 권장). 호스트에서만 호출하면 모든 멤버에게 자동 전파됨.</summary>
    public static bool LoadNetworked(SceneId id) => LoadNetworked(id.GetName());

    /// <summary>NGO 동기화 씬 로드 (string).</summary>
    /// <returns>로드 요청 성공 시 true (호스트가 아니거나 SceneManager 미세팅이면 false).</returns>
    public static bool LoadNetworked(string sceneName)
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager == null || !networkManager.IsServer || networkManager.SceneManager == null)
        {
            DebugTool.Error($"NGO 동기화 로드 실패 - Host가 아니거나 SceneManager 없음 (scene='{sceneName}')", DebugType.Network);
            return false;
        }
        networkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        return true;
    }
}
