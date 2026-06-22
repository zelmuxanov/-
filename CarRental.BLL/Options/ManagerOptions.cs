namespace CarRental.BLL.Options;

public class ManagerOptions
{
    public const string SectionName = "ManagerSettings";
    
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}