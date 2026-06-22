using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CarRental.BLL.Interfaces.Services;
using CarRental.BLL.DTOs.Car;
using CarRental.Domain.Enums;
using ClosedXML.Excel;

namespace CarRental.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/ImportExport")]
    [Authorize(Roles = "Admin")]
    public class ImportExportController : BaseAdminController
    {
        private readonly ICarService _carService;
        private readonly ILogger<ImportExportController> _logger;

        public ImportExportController(ICarService carService, ILogger<ImportExportController> logger)
        {
            _carService = carService;
            _logger = logger;
        }

        [HttpGet("Cars/Export")]
        public async Task<IActionResult> ExportCars()
        {
            var cars = await _carService.GetAllCarsAsync();
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Автомобили");

            var headers = new[] { "Brand", "Model", "Year", "Color", "PricePerDay",
                                  "PricePerDay15", "PricePerDay30", "Deposit",
                                  "MileageLimitPerDay", "OverMileagePricePerKm", "UnlimitedMileagePrice",
                                  "Class", "Transmission", "FuelType", "Seats",
                                  "EngineCapacity", "LicensePlate", "VIN",
                                  "Description", "IsAvailable" };
            for (int i = 0; i < headers.Length; i++)
                ws.Cell(1, i + 1).Value = headers[i];

            int row = 2;
            foreach (var car in cars)
            {
                ws.Cell(row, 1).Value = car.Brand;
                ws.Cell(row, 2).Value = car.Model;
                ws.Cell(row, 3).Value = car.Year;
                ws.Cell(row, 4).Value = car.Color;
                ws.Cell(row, 5).Value = car.PricePerDay;
                ws.Cell(row, 6).Value = car.PricePerDay15;
                ws.Cell(row, 7).Value = car.PricePerDay30;
                ws.Cell(row, 8).Value = car.Deposit;
                ws.Cell(row, 9).Value = car.MileageLimitPerDay;
                ws.Cell(row, 10).Value = car.OverMileagePricePerKm;
                ws.Cell(row, 11).Value = car.UnlimitedMileagePrice;
                ws.Cell(row, 12).Value = car.Class.ToString();
                ws.Cell(row, 13).Value = car.Transmission.ToString();
                ws.Cell(row, 14).Value = car.FuelType.ToString();
                ws.Cell(row, 15).Value = car.Seats;
                ws.Cell(row, 16).Value = car.EngineCapacity;
                ws.Cell(row, 17).Value = car.LicensePlate;
                ws.Cell(row, 18).Value = car.VIN;
                ws.Cell(row, 19).Value = car.Description;
                ws.Cell(row, 20).Value = car.IsAvailable ? "Yes" : "No";
                row++;
            }

            ws.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Cars.xlsx");
        }

        [HttpGet("Cars/Import")]
        public IActionResult ImportCars()
        {
            return View(new ImportCarsViewModel());
        }

        [HttpPost("Cars/Import")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportCars(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Файл не выбран";
                return RedirectToAction("ImportCars");
            }

            var rows = new List<CarImportRow>();
            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                using var workbook = new XLWorkbook(stream);
                var ws = workbook.Worksheet(1);
                var range = ws.RangeUsed();
                if (range == null || range.RowCount() < 2)
                {
                    TempData["ErrorMessage"] = "Файл пуст или не содержит данных";
                    return RedirectToAction("ImportCars");
                }

                for (int r = 2; r <= range.RowCount(); r++)
                {
                    // Вспомогательные функции теперь принимают номер строки
                    decimal ReadDecimal(int col, decimal defaultValue = 0)
                    {
                        var cell = ws.Cell(r, col);
                        if (cell.IsEmpty()) return defaultValue;
                        if (cell.TryGetValue<decimal>(out var dec)) return dec;
                        if (cell.TryGetValue<double>(out var d)) return (decimal)d;
                        return defaultValue;
                    }

                    int ReadInt(int col, int defaultValue = 0)
                    {
                        var cell = ws.Cell(r, col);
                        if (cell.IsEmpty()) return defaultValue;
                        if (cell.TryGetValue<int>(out var i)) return i;
                        if (cell.TryGetValue<double>(out var d)) return (int)d;
                        return defaultValue;
                    }

                    double ReadDouble(int col, double defaultValue = 0.0)
                    {
                        var cell = ws.Cell(r, col);
                        if (cell.IsEmpty()) return defaultValue;
                        if (cell.TryGetValue<double>(out var d)) return d;
                        if (cell.TryGetValue<decimal>(out var dec)) return (double)dec;
                        return defaultValue;
                    }

                    string ReadString(int col, string defaultValue = "") =>
                        ws.Cell(r, col).IsEmpty() ? defaultValue : ws.Cell(r, col).GetString().Trim();

                    rows.Add(new CarImportRow
                    {
                        Brand = ReadString(1),
                        Model = ReadString(2),
                        Year = ReadInt(3),
                        Color = ReadString(4),
                        PricePerDay = ReadDecimal(5),
                        PricePerDay15 = ReadDecimal(6),
                        PricePerDay30 = ReadDecimal(7),
                        Deposit = ReadDecimal(8),
                        MileageLimitPerDay = ReadInt(9, 250),
                        OverMileagePricePerKm = ReadDecimal(10),
                        UnlimitedMileagePrice = ReadDecimal(11),
                        Class = ReadString(12),
                        Transmission = ReadString(13),
                        FuelType = ReadString(14),
                        Seats = ReadInt(15, 5),
                        EngineCapacity = ReadDouble(16),
                        LicensePlate = ReadString(17),
                        VIN = ReadString(18),
                        Description = ReadString(19),
                        IsAvailable = ReadString(20).ToLower() == "yes"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка импорта автомобилей");
                TempData["ErrorMessage"] = "Ошибка при чтении файла";
                return RedirectToAction("ImportCars");
            }

            var invalidRows = rows.Where(r => string.IsNullOrWhiteSpace(r.Brand) || string.IsNullOrWhiteSpace(r.Model)).ToList();
            if (invalidRows.Any())
            {
                TempData["ErrorMessage"] = "Некоторые строки не содержат Brand или Model. Импорт отменён.";
                return RedirectToAction("ImportCars");
            }

            return View("ImportCarsPreview", new ImportCarsViewModel { Rows = rows });
        }

        [HttpPost("Cars/SaveImport")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveImportedCars(List<CarImportRow> rows)
        {
            if (rows == null || !rows.Any())
                return RedirectToAction("Index", "Cars");

            int successCount = 0;
            int errorCount = 0;

            foreach (var row in rows)
            {
                try
                {
                    // Используем default(CarClass) если парсинг не удался (вместо Economy)
                    if (!Enum.TryParse<CarClass>(row.Class, true, out var carClass))
                    {
                        _logger.LogWarning("Неизвестное значение CarClass: {Class}, используется значение по умолчанию", row.Class);
                        carClass = default;
                    }
                    if (!Enum.TryParse<TransmissionType>(row.Transmission, true, out var transmission))
                    {
                        _logger.LogWarning("Неизвестное значение Transmission: {Transmission}, используется значение по умолчанию", row.Transmission);
                        transmission = default;
                    }
                    if (!Enum.TryParse<FuelType>(row.FuelType, true, out var fuelType))
                    {
                        _logger.LogWarning("Неизвестное значение FuelType: {FuelType}, используется значение по умолчанию", row.FuelType);
                        fuelType = default;
                    }

                    var dto = new CarCreateDto
                    {
                        Brand = row.Brand,
                        Model = row.Model,
                        Year = row.Year,
                        Color = row.Color,
                        PricePerDay = row.PricePerDay,
                        IsAvailable = row.IsAvailable,
                        Description = row.Description,
                        Class = carClass,
                        Transmission = transmission,
                        FuelType = fuelType,
                        Seats = row.Seats,
                        EngineCapacity = row.EngineCapacity,
                        LicensePlate = row.LicensePlate,
                        VIN = row.VIN,
                        PricePerDay15 = row.PricePerDay15,
                        PricePerDay30 = row.PricePerDay30,
                        Deposit = row.Deposit,
                        MileageLimitPerDay = row.MileageLimitPerDay,
                        OverMileagePricePerKm = row.OverMileagePricePerKm,
                        UnlimitedMileagePrice = row.UnlimitedMileagePrice
                    };
                    await _carService.CreateCarAsync(dto);
                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка сохранения автомобиля {Brand} {Model}", row.Brand, row.Model);
                    errorCount++;
                }
            }

            TempData["SuccessMessage"] = $"Импортировано {successCount} автомобилей. Ошибок: {errorCount}";
            return RedirectToAction("Index", "Cars");
        }

        public class ImportCarsViewModel
        {
            public List<CarImportRow> Rows { get; set; } = new();
        }

        public class CarImportRow
        {
            public string Brand { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
            public int Year { get; set; }
            public string Color { get; set; } = string.Empty;
            public decimal PricePerDay { get; set; }
            public decimal PricePerDay15 { get; set; }
            public decimal PricePerDay30 { get; set; }
            public decimal Deposit { get; set; }
            public int MileageLimitPerDay { get; set; } = 250;
            public decimal OverMileagePricePerKm { get; set; }
            public decimal UnlimitedMileagePrice { get; set; }
            public string Class { get; set; } = string.Empty;
            public string Transmission { get; set; } = string.Empty;
            public string FuelType { get; set; } = string.Empty;
            public int Seats { get; set; }
            public double EngineCapacity { get; set; }
            public string LicensePlate { get; set; } = string.Empty;
            public string VIN { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public bool IsAvailable { get; set; }
        }
    }
}