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

        [HttpGet("GetAllCars")]
        public async Task<ActionResult> GetAllCars()
        {
            var cars = await _context.Cars
                .Where(c => c.IsDeleted == 0)
                .ToListAsync();

            return Ok(cars);
        }

        [HttpGet("GetCar")]
        public async Task<ActionResult> GetCar(int id)
        {
            var car = await _context.Cars
                .FirstOrDefaultAsync(c => c.IdCar == id && c.IsDeleted == 0);

            if (car == null)
                return NotFound("Автомобиль не найден");

            return Ok(car);
        }

        [HttpPost("AddCar")]
        public async Task<ActionResult> AddCar(Car car)
        {
            if (car == null)
                return BadRequest("Неверные данные");

            car.IsDeleted = 0;

            var validationResult = _validation.ValidateCar(car);
            if (validationResult != null)
                return BadRequest(validationResult);

            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            return Ok("Автомобиль добавлен");
        }

        [HttpPut("EditCar")]
        public async Task<ActionResult> UpdateCar(int id, Car model)
        {
            if (model == null)
                return BadRequest("Неверные данные");

            var car = await _context.Cars.FindAsync(id);

            if (car == null || car.IsDeleted == 1)
                return NotFound("Автомобиль не найден");

            if (await HasActiveBookingAsync(id))
            {
                return BadRequest("Автомобиль сейчас забронирован. Изменение невозможно");
            }

            car.IdCategory = model.IdCategory;
            car.Model = model.Model;
            car.YearCar = model.YearCar;
            car.Description = model.Description;
            car.ImagePath = model.ImagePath;
            car.Price = model.Price;
            car.PowerHp = model.PowerHp;
            car.IdFuelType = model.IdFuelType;
            car.IdTreansmission = model.IdTreansmission;

            var validationResult = _validation.ValidateCar(car);
            if (validationResult != null)
                return BadRequest(validationResult);

            await _context.SaveChangesAsync();

            return Ok("Автомобиль обновлён");
        }

        [HttpPut("DeleteCar")]
        public async Task<ActionResult> DeleteCar(int id)
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

        [HttpPut("RestoreCar")]
        public async Task<ActionResult> RestoreCar(int id)
        {
            var car = await _context.Cars.FindAsync(id);

            if (car == null)
                return NotFound("Автомобиль не найден");

            car.IsDeleted = 0;

            await _context.SaveChangesAsync();

            return Ok("Автомобиль восстановлен");
        }

        private async Task<bool> HasActiveBookingAsync(int carId)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            TimeOnly nowTime = TimeOnly.FromDateTime(DateTime.Now);

            var bookings = await _context.Bookings
                .Where(b => b.IdCar == carId && b.IdStatus == 2 && b.BookingDate >= today)
                .ToListAsync();

            return bookings.Any(b =>
                b.BookingDate > today ||
                (b.BookingDate == today && b.BookingTime.AddHours(4) > nowTime)
            );
        }

        [HttpGet("GetBusyTimes")]
        public async Task<ActionResult> GetBusyTimes(int carId, string date)
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