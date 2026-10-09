using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestDrive_API.Models;
using TestDrive_API.Classes;

namespace TestDrive_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ValidationHelper _validation;

        public CarController(AppDbContext context, ValidationHelper validation)
        {
            _context = context;
            _validation = validation;
        }

        // Получение полного списка автомобилей
        [HttpGet("GetAllCars")]
        public async Task<IActionResult> GetAllCars()
        {
            var cars = await _context.Cars
                .Where(c => c.IsDeleted == 0)
                .ToListAsync();

            return Ok(cars);
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public Task<IActionResult> GetAllCarsLegacy() => GetAllCars();

        // Получение конкретного автомобиля
        [HttpGet("GetCar/{id}")]
        public async Task<IActionResult> GetCar(int id)
        {
            var car = await _context.Cars
                .FirstOrDefaultAsync(c => c.IdCar == id && c.IsDeleted == 0);

            if (car == null)
                return NotFound("Автомобиль не найден");

            return Ok(car);
        }

        [HttpGet("{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public Task<IActionResult> GetCarLegacy(int id) => GetCar(id);

        // Добавление автомобиля
        [HttpPost("AddCar")]
        public async Task<IActionResult> AddCar(CarDTO dto)
        {
            if (dto == null)
                return BadRequest("Неверные данные");

            var car = new Car
            {
                IdCategory = dto.IdCategory,
                Model = dto.Model,
                YearCar = (short)dto.YearCar,
                Description = dto.Description,
                ImagePath = dto.ImagePath,
                IsDeleted = 0,
                Price = dto.Price,
                PowerHp = dto.PowerHp,
                IdFuelType = dto.IdFuelType,
                IdTreansmission = dto.IdTreansmission,
            };

            var validationResult = _validation.ValidateCar(car);
            if (validationResult != null)
                return BadRequest(validationResult);

            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            return Ok("Автомобиль добавлен");
        }

        [HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        public Task<IActionResult> AddCarLegacy(CarDTO dto) => AddCar(dto);

        // Изменение автомобиля
        [HttpPut("EditCar/{id}")]
        public async Task<IActionResult> UpdateCar(int id, CarDTO dto)
        {
            if (dto == null)
                return BadRequest("Неверные данные");

            var car = await _context.Cars.FindAsync(id);

            if (car == null || car.IsDeleted == 1)
                return NotFound("Автомобиль не найден");

            if (await HasActiveBookingAsync(id))
            {
                return BadRequest("Автомобиль сейчас забронирован. Изменение невозможно");
            }

            car.IdCategory = dto.IdCategory;
            car.Model = dto.Model;
            car.YearCar = (short)dto.YearCar;
            car.Description = dto.Description;
            car.ImagePath = dto.ImagePath;
            car.Price = dto.Price;
            car.PowerHp = dto.PowerHp;
            car.IdFuelType = dto.IdFuelType;
            car.IdTreansmission = dto.IdTreansmission;

            var validationResult = _validation.ValidateCar(car);
            if (validationResult != null)
                return BadRequest(validationResult);

            await _context.SaveChangesAsync();

            return Ok("Автомобиль обновлён");
        }

        [HttpPut("{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public Task<IActionResult> UpdateCarLegacy(int id, CarDTO dto) => UpdateCar(id, dto);

        // Удаление автомобиля (мягкое)
        [HttpPut("DeleteCar/{id}")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);

            if (car == null || car.IsDeleted == 1)
                return NotFound("Автомобиль не найден");

            if (await HasActiveBookingAsync(id))
            {
                return BadRequest("Автомобиль сейчас забронирован. Удаление невозможно");
            }

            car.IsDeleted = 1;

            await _context.SaveChangesAsync();

            return Ok("Автомобиль удалён");
        }

        // Восстановление автомобиля
        [HttpPut("RestoreCar/{id}")]
        public async Task<IActionResult> RestoreCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);

            if (car == null)
                return NotFound("Автомобиль не найден");

            car.IsDeleted = 0;

            await _context.SaveChangesAsync();

            return Ok("Автомобиль восстановлен");
        }

        // Проверка наличия активного бронирования (Исправлено)
        private async Task<bool> HasActiveBookingAsync(int carId)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            TimeOnly nowTime = TimeOnly.FromDateTime(DateTime.Now);

            // Получаем бронирования автомобиля на сегодня или будущее
            var bookings = await _context.Bookings
                .Where(b => b.IdCar == carId && b.IdStatus == 2 && b.BookingDate >= today)
                .ToListAsync();

            return bookings.Any(b =>
                b.BookingDate > today ||
                (b.BookingDate == today && b.BookingTime.AddHours(4) > nowTime)
            );
        }

        // Получение занятого времени автомобиля
        [HttpGet("GetBusyTimes")]
        public async Task<IActionResult> GetBusyTimes(int carId, string date)
        {
            if (!DateOnly.TryParse(date, out DateOnly selectedDate))
                return BadRequest("Неверный формат даты");

            var busyBookings = await _context.Bookings
                .Where(b => b.IdCar == carId && b.IdStatus != 3 && b.BookingDate == selectedDate)
                .Select(b => b.BookingTime)
                .ToListAsync();

            var availableTimes = new List<TimeOnly>
            {
                new TimeOnly(9, 0), new TimeOnly(10, 0), new TimeOnly(11, 0),
                new TimeOnly(12, 0), new TimeOnly(13, 0), new TimeOnly(14, 0),
                new TimeOnly(15, 0), new TimeOnly(16, 0), new TimeOnly(17, 0)
            };

            var busyTimes = availableTimes
                .Where(time => busyBookings.Any(b => Math.Abs((b.ToTimeSpan() - time.ToTimeSpan()).TotalMinutes) < 240))
                .ToList();

            return Ok(busyTimes);
        }
    }
}