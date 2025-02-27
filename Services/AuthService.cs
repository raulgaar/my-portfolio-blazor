using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Blazored.LocalStorage.StorageOptions;
using Microsoft.AspNetCore.Components.Authorization;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;
    public event Action? OnAuthStateChanged;

    public AuthService(HttpClient http, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<bool> Login(string username, string password)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new { username, password });

        if(!response.IsSuccessStatusCode)
            return false;
        
        var jsonResponse = await response.Content.ReadAsStringAsync();
        var token = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonResponse)?["token"];

        if (!string.IsNullOrEmpty(token))
        {
            await _localStorage.SetItemAsync("token", token);
            ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(token);
            OnAuthStateChanged?.Invoke();
            return true;
        }

        return false;
    }

    public async Task Logout()
    {
        await _localStorage.RemoveItemAsync("token");
        ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
        OnAuthStateChanged?.Invoke();
    }

    public async Task<bool> IsAuthenticated()
    {
        var token = await _localStorage.GetItemAsync<string>("token");
        return !string.IsNullOrEmpty(token);
    }

    public async Task<string> GetToken()
    {
        var token = await _localStorage.GetItemAsync<string>("token");
        return token;
    }
}