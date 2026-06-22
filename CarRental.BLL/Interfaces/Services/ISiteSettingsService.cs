using CarRental.BLL.DTOs.Settings;

namespace CarRental.BLL.Interfaces.Services;

public interface ISiteSettingsService
{
    SiteSettingsDto GetSettings();
    Task SaveSettingsAsync(SiteSettingsDto settings);
}