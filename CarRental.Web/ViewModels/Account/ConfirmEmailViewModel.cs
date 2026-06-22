namespace CarRental.Web.ViewModels.Account;

public class ConfirmEmailViewModel
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public string? Email { get; set; }
}