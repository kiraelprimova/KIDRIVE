using TestDrive_API.Models;

namespace TestDrive_API.Classes
{
    public class ValidationHelper
    {
        private readonly AppDbContext _context;

        public ValidationHelper(AppDbContext context)
        {
            _context = context;
        }

        // Проверка пользователя
        public string? ValidateUser(User user)
        {
            if (!_context.Roles.Any(r => r.IdRole == user.IdRole))
                return "Роль не найдена";

            return null;
        }

        // Проверка автомобиля
        public string? ValidateCar(Car car)
        {
            if (!_context.Categories.Any(c => c.IdCategory == car.IdCategory))
                return "Категория не найдена";

            if (car.IdFuelType.HasValue && !_context.FuelsTypes.Any(f => f.Id == car.IdFuelType.Value))
                return "Тип топлива не найден";

            if (car.IdTreansmission.HasValue && !_context.Transmissions.Any(t => t.Id == car.IdTreansmission.Value))
                return "Коробка передач не найдена";

            return null;
        }

        // Проверка бронирования
        public string? ValidateBooking(Booking booking)
        {
            if (!_context.Users.Any(u => u.IdUser == booking.IdUser))
                return "Пользователь не найден";

            if (!_context.Cars.Any(c => c.IdCar == booking.IdCar))
                return "Авто не найдено";

            if (!_context.BookingStatuses.Any(s => s.IdStatus == booking.IdStatus))
                return "Статус не найден";

            if (booking.BookingDate < DateOnly.FromDateTime(DateTime.Today))
                return "Нельзя оформить бронирование на прошедшую дату";

            return null;
        }
    }
}
