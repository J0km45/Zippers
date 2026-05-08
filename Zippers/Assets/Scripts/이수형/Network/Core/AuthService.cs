using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;

/// <summary>
/// UGS(Unity Gaming Service) Core 초기화 + 익명 로그인.
/// LobbyManager 의 세션 진입(Create/Join) 이전에 1회 호출되어야 한다.
/// 이미 초기화/로그인 된 상태면 no-op (중복 호출 안전).
/// </summary>
public static class AuthService
{
    public static async Task InitializeAsync()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        DebugTool.Log($"로그인 완료: {AuthenticationService.Instance.PlayerId}", DebugType.Network);
    }
}
