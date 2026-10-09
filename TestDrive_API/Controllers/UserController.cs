using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using TestDrive_API.Classes;
using TestDrive_API.Models;

namespace TestDrive_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ValidationHelper _validation;

        public UserController(AppDbContext context, ValidationHelper validation)
        {
            _context = context;
            _validation = validation;
        }

        // Регистрация пользователя

        [HttpPost("Registration")]
        public async Task<IActionResult> Registration(UserDTO dto)
        {
            if (dto == null)
                return BadRequest("Неверные данные");

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 8)
                return BadRequest("Пароль должен содержать не менее 8 символов");

            if (_context.Users.Any(u => u.Phone == dto.Phone))
                return BadRequest("Телефон уже используется");

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Patronymic = dto.Patronymic,
                Phone = dto.Phone,
                Email = dto.Email,
                Password = PasswordHelper.HashPassword(dto.Password),
                BirthDate = dto.BirthDate,
                IdRole = 1,
                IsDelete = 0
            };

            var validationResult = _validation.ValidateUser(user);
            if (validationResult != null)
                return BadRequest(validationResult);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Регистрация успешна");
        }

        // Авторизация пользователя

        [HttpPost("Authorization")]
        public IActionResult Authorization(string phone, string password)
        {
            if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(password))
                return BadRequest("Введите телефон и пароль");

            string cleanPhone = new string(phone.Where(char.IsDigit).ToArray());

            if (cleanPhone.StartsWith("8"))
                cleanPhone = "7" + cleanPhone.Substring(1);

            var user = _context.Users
                .AsEnumerable()
                .FirstOrDefault(u =>
                {
                    string dbPhone = new string(u.Phone.Where(char.IsDigit).ToArray());

                    if (dbPhone.StartsWith("8"))
                        dbPhone = "7" + dbPhone.Substring(1);

                    return dbPhone == cleanPhone &&
                           PasswordHelper.VerifyPassword(password, u.Password) &&
                           u.IsDelete == 0;
                });

            if (user == null)
                return NotFound("Пользователя не существует");

            user.Account = 1;

            // Если пароль был в открытом виде, обновляем его на хэш
            if (user.Password == password)
            {
                user.Password = PasswordHelper.HashPassword(password);
            }

            _context.SaveChanges();

            return Ok(user);
        }

        // Получить список всех клиентов

        [HttpGet("GetAllClients")]
        public IActionResult GetAllClients()
        {
            return Ok(_context.Users.Where(u => u.IsDelete == 0).ToList());
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult GetAllClientsLegacy() => GetAllClients();

        // Получить конкретного пользователя

        [HttpGet("GetClient/{id}")]
        public IActionResult GetClient(int id)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.IdUser == id && u.IsDelete == 0);

            if (user == null)
                return NotFound("Пользователь не найден");

            return Ok(user);
        }

        [HttpGet("{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult GetClientLegacy(int id) => GetClient(id);

        // Изменение профиля пользователя

        [HttpPut("UpdateClient/{id}")]
        public async Task<IActionResult> UpdateClient(int id, UserDTO dto)
        {
            if (dto == null)
                return BadRequest("Неверные данные");

            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound("Пользователь не найден");

            if (_context.Users.Any(u => u.Phone == dto.Phone && u.IdUser != id))
                return BadRequest("Телефон уже используется");

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                if (dto.Password.Length < 8)
                    return BadRequest("Пароль должен содержать не менее 8 символов");

                user.Password = dto.Password.Length == 64
                    ? dto.Password
                    : PasswordHelper.HashPassword(dto.Password);
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.Patronymic = dto.Patronymic;
            user.Phone = dto.Phone;
            user.Email = dto.Email;
            user.BirthDate = dto.BirthDate;

            var validationResult = _validation.ValidateUser(user);
            if (validationResult != null)
                return BadRequest(validationResult);

            await _context.SaveChangesAsync();

            return Ok("Данные обновлены");
        }

        [HttpPut("{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public Task<IActionResult> UpdateClientLegacy(int id, UserDTO dto) => UpdateClient(id, dto);

        // Удаление профиля пользователя

        [HttpPut("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound("Пользователь не найден");

            user.IsDelete = 1;

            await _context.SaveChangesAsync();

            return Ok("Аккаунт удален");
        }

        // Восстановление аккаунта пользователя

        [HttpPut("RestoreUser")]
        public async Task<IActionResult> RestoreUser(string phone, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Phone == phone && u.Password == password);

            if (user == null)
                return NotFound("Пользователь не найден");

            if (user.IsDelete == 0)
                return BadRequest("Аккаунт уже активен");

            user.IsDelete = 0;

            await _context.SaveChangesAsync();

            return Ok("Аккаунт восстановлен");
        }
        // Восстановление пароля пользователя
        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword(string phone, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
                return BadRequest("Пароль должен содержать не менее 8 символов");

            string cleanPhone = new string(phone.Where(char.IsDigit).ToArray());

            if (cleanPhone.StartsWith("8"))
                cleanPhone = "7" + cleanPhone.Substring(1);

            var user = _context.Users
                .AsEnumerable()
                .FirstOrDefault(u =>
                {
                    string dbPhone =
                        new string(u.Phone.Where(char.IsDigit).ToArray());

                    if (dbPhone.StartsWith("8"))
                        dbPhone = "7" + dbPhone.Substring(1);

                    return dbPhone == cleanPhone;
                });

            if (user == null)
                return NotFound("Пользователь не найден");

            user.Password = PasswordHelper.HashPassword(newPassword);

            await _context.SaveChangesAsync();

            return Ok("Пароль успешно изменён");
        }

        // Получение списка клиентов, оформивших бронирования (для менеджера М7)
        [HttpGet("ClientsWithBookings")]
        public async Task<IActionResult> GetClientsWithBookings()
        {
            var clients = await _context.Users
                .Where(u => u.IsDelete == 0 && u.Bookings.Any())
                .Select(u => new
                {
                    u.IdUser,
                    u.FirstName,
                    u.LastName,
                    u.Patronymic,
                    u.Phone,
                    u.Email,
                    u.BirthDate,
                    totalBookings = u.Bookings.Count()
                })
                .ToListAsync();

            return Ok(clients);
        }
    }
}
