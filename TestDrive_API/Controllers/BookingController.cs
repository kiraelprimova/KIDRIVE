using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestDrive_API.Models;
using TestDrive_API.Classes;
using TestDrive_API.Services;

namespace TestDrive_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ValidationHelper _validation;

        public BookingController(AppDbContext context, ValidationHelper validation)
        {
            _context = context;
            _validation = validation;
        }

        [HttpPost("CreateBooking")]
        [HttpPost]
        public async Task<ActionResult> CreateBooking([FromBody] Booking booking)
        {
            if (booking == null)
                return BadRequest("Неверные данные");

            if (booking.IdPayment == 0)
                booking.IdPayment = 1;

            booking.IdStatus = 1;

            var validationError = _validation.ValidateBooking(booking);
            if (validationError != null)
                return BadRequest(validationError);

            int newBookingMinutes = booking.BookingTime.Hour * 60 + booking.BookingTime.Minute;

            var sameDateBookings = await _context.Bookings
                .Where(b => b.BookingDate == booking.BookingDate && b.IdStatus != 3)
                .Select(b => new { b.IdCar, b.IdUser, b.BookingTime })
                .ToListAsync();

            bool carBusy = sameDateBookings.Any(b =>
                b.IdCar == booking.IdCar &&
                Math.Abs((b.BookingTime.Hour * 60 + b.BookingTime.Minute) - newBookingMinutes) < 240
            );

            if (carBusy)
                return BadRequest("Автомобиль уже забронирован на это время");

            bool userBusy = sameDateBookings.Any(b =>
                b.IdUser == booking.IdUser &&
                Math.Abs((b.BookingTime.Hour * 60 + b.BookingTime.Minute) - newBookingMinutes) < 240
            );

            if (userBusy)
                return BadRequest("У вас уже есть бронь в этот промежуток времени");

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return Ok("Заявка на бронирование создана");
        }

        [HttpGet("GetAllBookings")]
        [HttpGet]
        public async Task<ActionResult> GetAllBookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.IdUserNavigation)
                .Include(b => b.IdCarNavigation)
                    .ThenInclude(c => c.IdCategoryNavigation)
                .Include(b => b.IdStatusNavigation)
                .Include(b => b.IdPaymentNavigation)
                .Select(b => new
                {
                    idBooking = b.IdBooking,
                    idUser = b.IdUser,
                    bookingDate = b.BookingDate,
                    bookingTime = b.BookingTime,
                    idStatus = b.IdStatus,
                    idUserNavigation = new
                    {
                        idUser = b.IdUserNavigation.IdUser,
                        firstName = b.IdUserNavigation.FirstName,
                        lastName = b.IdUserNavigation.LastName,
                        patronymic = b.IdUserNavigation.Patronymic,
                        phone = b.IdUserNavigation.Phone,
                        email = b.IdUserNavigation.Email
                    },
                    idCarNavigation = new
                    {
                        idCar = b.IdCarNavigation.IdCar,
                        model = b.IdCarNavigation.Model,
                        idCategory = b.IdCarNavigation.IdCategory,
                        price = b.IdCarNavigation.Price,
                        yearCar = b.IdCarNavigation.YearCar
                    },
                    idStatusNavigation = new
                    {
                        statusName = b.IdStatusNavigation.StatusName
                    }
                })
                .ToListAsync();

            return Ok(bookings);
        }

        [HttpPut("UpdateStatus")]
        public async Task<ActionResult> UpdateBookingStatus(int id, int statusId)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
                return NotFound("Бронирование не найдено");

            booking.IdStatus = statusId;
            await _context.SaveChangesAsync();

            if (statusId == 2)
            {
                var fullBooking = await _context.Bookings
                    .Include(b => b.IdUserNavigation)
                    .Include(b => b.IdCarNavigation)
                        .ThenInclude(c => c.IdCategoryNavigation)
                    .Include(b => b.IdPaymentNavigation)
                    .Include(b => b.IdStatusNavigation)
                    .FirstOrDefaultAsync(b => b.IdBooking == id);

                if (fullBooking != null)
                {
                    try
                    {
                        string fullName = $"{fullBooking.IdUserNavigation?.LastName} " +
                                         $"{fullBooking.IdUserNavigation?.FirstName} " +
                                         $"{fullBooking.IdUserNavigation?.Patronymic}".Trim();

                        string pdfPath = PdfService.CreateContract(
                            fullName,
                            fullBooking.IdUserNavigation?.Phone ?? "",
                            fullBooking.IdUserNavigation?.Email ?? "",
                            fullBooking.IdCarNavigation?.IdCategoryNavigation?.NameCategory ?? "",
                            fullBooking.IdCarNavigation?.Model ?? "",
                            fullBooking.IdCarNavigation?.YearCar ?? 0,
                            fullBooking.IdCarNavigation?.Price ?? 0,
                            fullBooking.BookingDate,
                            fullBooking.BookingTime,
                            fullBooking.IdPaymentNavigation?.MethodName ?? "Наличные",
                            "Подтверждено"
                        );

                        if (!string.IsNullOrEmpty(fullBooking.IdUserNavigation?.Email))
                        {
                            EmailService.SendEmail(fullBooking.IdUserNavigation.Email, pdfPath);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                return Ok("Бронирование подтверждено.");
            }

            if (statusId == 3)
            {
                return Ok("Бронирование отменено.");
            }

            if (statusId == 4)
            {
                return Ok("Тест-драйв завершён.");
            }

            return Ok("Статус бронирования обновлён.");
        }
    }
}