namespace CarRental.BLL.DTOs.User;

public class UserDocumentsDto
{
    public string? DriverLicenseNumber { get; set; }
    public DateTime? DriverLicenseExpiry { get; set; }
    public bool HasPassport { get; set; }
    public bool HasDriverLicense { get; set; }
    public DateTime? DocumentsVerifiedAt { get; set; }
    public string VerificationStatus { get; set; } = "NotUploaded"; // NotUploaded, Pending, Verified, Rejected
}