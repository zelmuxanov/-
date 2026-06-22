using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRental.DAL.Data;
using System.Xml.Linq;

namespace CarRental.Web.Controllers
{
    public class SitemapController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public SitemapController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [Route("/sitemap.xml")]
        public async Task<IActionResult> Index()
        {
            // Базовый URL из настроек (например, "https://o-prokat.ru")
            var baseUrl = _configuration["SiteSettings:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                baseUrl = "https://o-prokat.ru"; // запасной вариант

            var today = DateTime.UtcNow.Date;

            // ----- 1. Статические публичные страницы -----
            var staticPages = new List<(string url, string changefreq, double priority)>
            {
                ("", "daily", 1.0),                     // главная
                ("/Cars", "daily", 0.9),                // каталог
                ("/Documents", "weekly", 0.8),          // страница документов
                ("/Home/Privacy", "yearly", 0.5),       // политика конфиденциальности
                // Если есть другие публичные страницы, добавьте их сюда, например:
                ("/Home/About", "monthly", 0.6),
                ("/Home/Contact", "monthly", 0.6),
                ("/Home/FAQ", "monthly", 0.6),
            };

            // ----- 2. Динамические страницы: все автомобили -----
            var cars = await _context.Cars
                .Where(c => c.IsAvailable) // если у вас есть поле IsAvailable; иначе уберите Where
                .ToListAsync();

            var carUrls = cars.Select(c => (
                url: $"/Cars/Details/{c.Id}",
                changefreq: "weekly",
                priority: 0.8
            )).ToList();

            // Объединяем все URL
            var allUrls = staticPages.Concat(carUrls);

            // Формируем XML
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var sitemap = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement(ns + "urlset",
                    from item in allUrls
                    select new XElement(ns + "url",
                        new XElement(ns + "loc", $"{baseUrl.TrimEnd('/')}{item.url}"),
                        new XElement(ns + "lastmod", today.ToString("yyyy-MM-dd")),
                        new XElement(ns + "changefreq", item.changefreq),
                        new XElement(ns + "priority", item.priority.ToString(System.Globalization.CultureInfo.InvariantCulture))
                    )
                )
            );

            return Content(sitemap.ToString(), "application/xml");
        }
    }
}