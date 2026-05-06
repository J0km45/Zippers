using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;

/// <summary>
/// UGS(Unity Gaming Service) Core 초기화 + 익명 로그인.
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

        Debug.Log($"AuthService: 로그인 완료: {AuthenticationService.Instance.PlayerId}");
    }
}
