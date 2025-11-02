using AppTiemposV3.SharedClases.DTOs;
using Blazored.LocalStorage;

namespace AppTiemposV3.Web.Services;

public class LayoutState
{
    private readonly ILocalStorageService _localStorage;
    
    public event Action? OnSidebarChanged;
    public event Action? OnDisabledButtonChanged;
    public event Action<SavedEventArgs>? OnActivityCreated;

    public bool IsSidebarClosed { get; private set; } = false;
    
    public LayoutState(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }
    
    public async Task InitializeAsync()
    {
        bool? stored = await _localStorage.GetItemAsync<bool?>("SidebarClosed");
        IsSidebarClosed = stored ?? false; 
        NotifyStateChanged();
    }

    public async Task ToggleSidebar()
    {
        IsSidebarClosed = !IsSidebarClosed;
        await _localStorage.SetItemAsync("SidebarClosed", IsSidebarClosed);
        NotifyStateChanged();
    }
    
    public async Task SetSidebar(bool closed)
    {
        IsSidebarClosed = closed;
        await _localStorage.SetItemAsync("SidebarClosed", IsSidebarClosed);
        NotifyStateChanged();
    }

    public void NotifyButtonStatusChanged()
    {
        OnDisabledButtonChanged?.Invoke();
    }

    public void NotifyStateChangedAct(SavedEventArgs args)
    {
        OnActivityCreated?.Invoke(args);
    }

    private void NotifyStateChanged() => OnSidebarChanged?.Invoke();
}