using System.Text.Json;
using CarRental.BLL.DTOs.Settings;
using CarRental.BLL.Interfaces.Services;

namespace CarRental.Web.Services;

public class SiteSettingsService : ISiteSettingsService
{
    private readonly IWebHostEnvironment _environment;
    private readonly string _settingsPath;

    public SiteSettingsService(IWebHostEnvironment environment)
    {
        _environment = environment;
        _settingsPath = Path.Combine(_environment.WebRootPath, "site-settings.json");
    }

    public SiteSettingsDto GetSettings()
    {
        if (!File.Exists(_settingsPath))
        {
            var defaultSettings = new SiteSettingsDto();
            SaveSettingsAsync(defaultSettings).Wait();
            return defaultSettings;
        }

        var json = File.ReadAllText(_settingsPath);
        return JsonSerializer.Deserialize<SiteSettingsDto>(json) ?? new SiteSettingsDto();
    }

    public async Task SaveSettingsAsync(SiteSettingsDto settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_settingsPath, json);
    }
}