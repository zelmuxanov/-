using Microsoft.AspNetCore.Mvc;
using CarRental.BLL.Interfaces.Services;

namespace CarRental.Web.Controllers.Api
{
    [Route("api/bookings")]
    [ApiController]
    public class BookingsApiController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsApiController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("occupied-dates")]
        public async Task<IActionResult> GetOccupiedDates(Guid carId)
        {
            try
            {
                var bookings = await _bookingService.GetBookingsForCarAsync(carId);
                var disabledDates = new HashSet<string>();
                foreach (var b in bookings)
                {
                    for (var d = b.StartDate; d <= b.EndDate; d = d.AddDays(1))
                        disabledDates.Add(d.ToString("yyyy-MM-dd"));
                }
                return Ok(disabledDates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}