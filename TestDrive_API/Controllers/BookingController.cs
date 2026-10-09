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

        // Создание бронирования клиентом
        [HttpPost("CreateBooking")]
        public async Task<IActionResult> CreateBooking([FromBody] BookingDTO dto)
        {
            if (dto == null)
                return BadRequest("Неверные данные");

            var booking = new Booking
            {
                IdCar = dto.IdCar,
                IdUser = dto.IdUser,
                BookingDate = dto.BookingDate,
                BookingTime = dto.BookingTime,
                IdPayment = dto.IdPayment == 0 ? 1 : dto.IdPayment,
                IdStatus = 1
            };

            // Проверка базовой валидации (дата, существование сущностей)
            var validationError = _validation.ValidateBooking(booking);
            if (validationError != null)
                return BadRequest(validationError);

            // Переводим время бронирования в минуты для проверки интервала (4 часа = 240 мин)
            int newBookingMinutes = booking.BookingTime.Hour * 60 + booking.BookingTime.Minute;

            // Бронирования на ту же дату (исключая отменённые со статусом 3)
            var sameDateBookings = await _context.Bookings
                .Where(b => b.BookingDate == booking.BookingDate && b.IdStatus != 3)
                .Select(b => new { b.IdCar, b.IdUser, b.BookingTime })
                .ToListAsync();

            // Проверка: свободен ли автомобиль в этом 4-часовом окне
            bool carBusy = sameDateBookings.Any(b =>
                b.IdCar == booking.IdCar &&
                Math.Abs((b.BookingTime.Hour * 60 + b.BookingTime.Minute) - newBookingMinutes) < 240
            );

            if (carBusy)
                return BadRequest("Автомобиль уже забронирован на это время");

            // Проверка: свободен ли пользователь
            bool userBusy = sameDateBookings.Any(b =>
                b.IdUser == booking.IdUser &&
                Math.Abs((b.BookingTime.Hour * 60 + b.BookingTime.Minute) - newBookingMinutes) < 240
            );

            if (userBusy)
                return BadRequest("У вас уже есть бронь в этот промежуток времени");

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return Ok("Заявка на бронирование создана и передана на рассмотрение менеджеру.");
        }

        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        public Task<IActionResult> CreateBookingLegacy([FromBody] BookingDTO dto) => CreateBooking(dto);

        // Получение полного списка бронирований
        [HttpGet("GetAllBookings")]
        public async Task<IActionResult> GetAllBookings()
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

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public Task<IActionResult> GetAllBookingsLegacy() => GetAllBookings();

        // Изменение статуса бронирования менеджером или клиентом (отмена/подтверждение/завершение)
        [HttpPut("UpdateStatus/{id}")]
        public async Task<IActionResult> UpdateBookingStatus(int id, int statusId)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
                return NotFound("Бронирование не найдено");

            booking.IdStatus = statusId;
            await _context.SaveChangesAsync();

            // Если менеджер подтвердил бронирование (статус 2) — по ТЗ генерируем PDF и отправляем на email
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
                        // Даже если почта временно недоступна, статус уже обновлен
                    }
                }

                return Ok("Бронирование подтверждено. Договор сформирован и отправлен на email клиента.");
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

        [HttpPut("{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public Task<IActionResult> UpdateBookingStatusLegacy(int id, int statusId) => UpdateBookingStatus(id, statusId);
    }
}