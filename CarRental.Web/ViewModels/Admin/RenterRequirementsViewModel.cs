using CarRental.BLL.DTOs.Settings;

namespace CarRental.Web.ViewModels.Admin;

public class RenterRequirementsViewModel
{
    public List<RenterRequirementDto> Requirements { get; set; } = new();
}