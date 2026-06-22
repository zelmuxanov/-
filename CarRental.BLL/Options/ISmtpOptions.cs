namespace CarRental.BLL.Options;

public interface ISmtpOptions
{
    string SmtpServer { get; set; }
    int SmtpPort { get; set; }
    string SenderName { get; set; }
    string SenderEmail { get; set; }
    string Username { get; set; }
    string Password { get; set; }
    bool EnableSsl { get; set; }
    bool UseDefaultCredentials { get; set; }
}