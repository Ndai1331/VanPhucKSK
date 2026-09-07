using CoreAdminWeb.Model;
using CoreAdminWeb.Model.Menus;
using CoreAdminWeb.Model.Settings;
using CoreAdminWeb.Model.User;
using CoreAdminWeb.Services.Files;
using CoreAdminWeb.Services.Menus;
using CoreAdminWeb.Services.Settings;
using CoreAdminWeb.Services.Users;

namespace CoreAdminWeb.Services.ApplicationState;

/// <summary>
/// Circuit-scoped cache for data shared by the shell and pages.
/// The request tasks are cached, rather than only their results, so concurrent
/// components share one in-flight API call during the first render.
/// </summary>
public interface IApplicationStateService
{
    Task<ApplicationSettingsSnapshot> GetSettingsAsync();
    Task<UserModel?> GetCurrentUserAsync();
    Task<IReadOnlyList<MenuResponse>> GetMenusAsync();
    void Reset();
}

public sealed record ApplicationSettingsSnapshot(
    SettingModel Setting,
    FileModel? Logo,
    FileModel? Favicon);

public sealed class ApplicationStateService(
    ISettingService settingService,
    IFileService fileService,
    IUserService userService,
    IMenuService menuService) : IApplicationStateService
{
    private readonly object _sync = new();
    private Task<ApplicationSettingsSnapshot>? _settingsTask;
    private Task<UserModel?>? _currentUserTask;
    private Task<IReadOnlyList<MenuResponse>>? _menusTask;

    public Task<ApplicationSettingsSnapshot> GetSettingsAsync()
    {
        lock (_sync)
        {
            return _settingsTask ??= LoadSettingsAsync();
        }
    }

    public Task<UserModel?> GetCurrentUserAsync()
    {
        lock (_sync)
        {
            return _currentUserTask ??= LoadCurrentUserAsync();
        }
    }

    public Task<IReadOnlyList<MenuResponse>> GetMenusAsync()
    {
        lock (_sync)
        {
            return _menusTask ??= LoadMenusAsync();
        }
    }

    public void Reset()
    {
        lock (_sync)
        {
            _settingsTask = null;
            _currentUserTask = null;
            _menusTask = null;
        }
    }

    private async Task<ApplicationSettingsSnapshot> LoadSettingsAsync()
    {
        var response = await settingService.GetCurrentSettingAsync();
        if (!response.IsSuccess || response.Data is null)
        {
            return new ApplicationSettingsSnapshot(new SettingModel(), null, null);
        }

        var setting = response.Data;
        var logoTask = string.IsNullOrWhiteSpace(setting.project_logo)
            ? Task.FromResult<FileModel?>(null)
            : LoadFileAsync(setting.project_logo);
        var faviconTask = string.IsNullOrWhiteSpace(setting.public_favicon)
            ? Task.FromResult<FileModel?>(null)
            : LoadFileAsync(setting.public_favicon);

        await Task.WhenAll(logoTask, faviconTask);
        return new ApplicationSettingsSnapshot(setting, logoTask.Result, faviconTask.Result);
    }

    private async Task<FileModel?> LoadFileAsync(string id)
    {
        var response = await fileService.GetFileAsync(id);
        return response.IsSuccess ? response.Data : null;
    }

    private async Task<UserModel?> LoadCurrentUserAsync()
    {
        var response = await userService.GetCurrentUserAsync();
        return response.IsSuccess ? response.Data : null;
    }

    private async Task<IReadOnlyList<MenuResponse>> LoadMenusAsync()
    {
        var response = await menuService.GetMenusAsync();
        if (!response.IsSuccess || response.Data is null)
        {
            return Array.Empty<MenuResponse>();
        }

        var childrenByParent = response.Data
            .Where(menu => menu.parent_id.HasValue)
            .GroupBy(menu => menu.parent_id!.Value)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(menu => menu.sort).ToList());

        var parents = response.Data
            .Where(menu => !menu.parent_id.HasValue)
            .OrderBy(menu => menu.sort)
            .ToList();

        foreach (var parent in parents)
        {
            parent.sub_menus = childrenByParent.TryGetValue(parent.id, out var children)
                ? children
                : [];
        }

        return parents;
    }
}
