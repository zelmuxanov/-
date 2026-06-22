namespace CarRental.BLL.DTOs.Settings
{
    public class SiteSettingsDto
    {
        public string CompanyName { get; set; } = "О! Прокат";
        public string Phone { get; set; } = "+7 (968) 287-03-83";
        public string Email { get; set; } = "bookings.orental@mail.ru";
        public string Address { get; set; } = "Московская область, Одинцово, микрорайон Новая Трёхгорка, Кутузовская улица, 12";
        public string LogoUrl { get; set; } = "/images/default-logo.png";
        public string WhatsAppUrl { get; set; } = "https://wa.me/79859910837";
        public string TelegramUrl { get; set; } = "https://t.me/Vasilina177";
        public string MaxUrl { get; set; } = "";
        public List<RenterRequirementDto> RenterRequirements { get; set; } = new();
    }
}