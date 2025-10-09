using System.Net.Http.Headers;
using Blazored.LocalStorage;
using Blazored.SessionStorage;

namespace AppTiemposV3.Web.Handlers;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;
    private readonly ISessionStorageService _sessionStorage;
    private readonly string key = "token";

    public AuthHeaderHandler(ILocalStorageService localStorage, ISessionStorageService sessionStorage)
    {
        _localStorage = localStorage;
        _sessionStorage = sessionStorage;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? token = await _sessionStorage.GetItemAsStringAsync(key);

        if (string.IsNullOrEmpty(token))
        {
            token = await _localStorage.GetItemAsStringAsync(key);
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}