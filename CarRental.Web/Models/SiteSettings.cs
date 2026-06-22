using System.Collections.Generic;
using CarRental.BLL.DTOs.Settings;

namespace CarRental.Web.Models;

public class SiteSettings
{
    public string CompanyName { get; set; } = "О! Прокат";
    public string Phone { get; set; } = "+7 (968) 287-03-83";
    public string Email { get; set; } = "bookings.orental@mail.ru";
    public string Address { get; set; } = "Московская область, Одинцово, микрорайон Новая Трёхгорка, Кутузовская улица, 12";
    public string LogoUrl { get; set; } = "/images/default-logo.png";
    public string WhatsAppUrl { get; set; } = "https://wa.me/79859910837?text=%D0%9E%D0%B1%D1%80%D0%B0%D1%89%D0%B5%D0%BD%D0%B8%D0%B5+%D0%B8%D0%B7+%D0%AF%D0%BD%D0%B4%D0%B5%D0%BA%D1%81+%D0%9A%D0%B0%D1%80%D1%82%0A%D0%97%D0%B4%D1%80%D0%B0%D0%B2%D1%81%D1%82%D0%B2%D1%83%D0%B9%D1%82%D0%B5!+%D0%9C%D0%B5%D0%BD%D1%8F+%D0%B7%D0%B0%D0%B8%D0%BD%D1%82%D0%B5%D1%80%D0%B5%D1%81%D0%BE%D0%B2%D0%B0%D0%BB%D0%BE+%D0%B2%D0%B0%D1%88%D0%B5+%D0%BF%D1%80%D0%B5%D0%B4%D0%BB%D0%BE%D0%B6%D0%B5%D0%BD%D0%B8%D0%B5";
    public string TelegramUrl { get; set; } = "https://t.me/Vasilina177";
    public string MaxUrl { get; set; } = "";
    public List<RenterRequirementDto> RenterRequirements { get; set; } = new();
}