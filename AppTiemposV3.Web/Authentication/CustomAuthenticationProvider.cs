using System.Security.Claims;
using AppTiemposV3.SharedClases.DTOs;
using static AppTiemposV3.SharedClases.GenericModels.Generics;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace AppTiemposV3.Web.Authentication;

public class CustomAuthenticationProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorageService;
    private readonly ISessionStorageService _sessionStorageService;
    private ClaimsPrincipal anonymus = new(new ClaimsIdentity());
    private readonly string key =  "token";

    public CustomAuthenticationProvider(ILocalStorageService localStorageService, ISessionStorageService sessionStorageService)
    {
        _localStorageService = localStorageService;
        _sessionStorageService = sessionStorageService;
    }
    
    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            string? tokenLocal = await _sessionStorageService.GetItemAsStringAsync(key);

            if (string.IsNullOrEmpty(tokenLocal))
            {
                tokenLocal = await _localStorageService.GetItemAsStringAsync(key);
            }

            if (string.IsNullOrEmpty(tokenLocal))
                return new AuthenticationState(anonymus);
            
            UserSession? claims = GetClaimsFromToken(tokenLocal);
            
            ClaimsPrincipal? claimsPrincipal = SetClaimPrincipal(claims);
            
            return await Task.FromResult<AuthenticationState>(new AuthenticationState(claimsPrincipal));
        }
        catch
        {
            return await Task.FromResult<AuthenticationState>(new AuthenticationState(anonymus));
        }
    }

    public async Task UpdateAuthenticationState(string? token = null, bool rememberMe = false)
    {
        ClaimsPrincipal? claimsPrincipal = new();

        if (!string.IsNullOrEmpty(token))
        {
            UserSession? claims = GetClaimsFromToken(token);
            
            claimsPrincipal = SetClaimPrincipal(claims);

            if (rememberMe)
            {
                await _localStorageService.SetItemAsStringAsync(key, token);
                await _sessionStorageService.RemoveItemAsync(key);
            }
            else
            {
                await _sessionStorageService.SetItemAsStringAsync(key, token);
                await _localStorageService.RemoveItemAsync(key);
            }
        }
        else
        {
            claimsPrincipal = anonymus;
            if (await _localStorageService.ContainKeyAsync(key))
                await _localStorageService.RemoveItemAsync(key);

            if (await _sessionStorageService.ContainKeyAsync(key))
                await _sessionStorageService.RemoveItemAsync(key);
        }
        
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }
}