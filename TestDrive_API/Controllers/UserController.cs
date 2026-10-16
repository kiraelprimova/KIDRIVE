using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpPost("Registration")]
        public async Task<ActionResult> Registration(User user)
        {
            if (user == null)
                return BadRequest("Неверные данные");

            if (string.IsNullOrWhiteSpace(user.Password) || user.Password.Length < 8)
                return BadRequest("Пароль должен содержать не менее 8 символов");

            if (_context.Users.Any(u => u.Phone == user.Phone))
                return BadRequest("Телефон уже используется");

            user.Password = PasswordHelper.HashPassword(user.Password);
            user.IdRole = 1;
            user.IsDelete = 0;

            var validationResult = _validation.ValidateUser(user);
            if (validationResult != null)
                return BadRequest(validationResult);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("Регистрация успешна");
        }

        [HttpPost("Authorization")]
        public async Task<ActionResult> Authorization(string phone, string password)
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

            if (user.Password == password)
            {
                user.Password = PasswordHelper.HashPassword(password);
            }

            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [HttpGet("GetAllClients")]
        public async Task<ActionResult> GetAllClients()
        {
            var clients = await _context.Users.Where(u => u.IsDelete == 0).ToListAsync();
            return Ok(clients);
        }

        [HttpGet("GetClient")]
        public async Task<ActionResult> GetClient(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.IdUser == id && u.IsDelete == 0);

            if (user == null)
                return NotFound("Пользователь не найден");

            return Ok(user);
        }

        [HttpPut("UpdateClient")]
        public async Task<ActionResult> UpdateClient(int id, User model)
        {
            if (model == null)
                return BadRequest("Неверные данные");

            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound("Пользователь не найден");

            if (_context.Users.Any(u => u.Phone == model.Phone && u.IdUser != id))
                return BadRequest("Телефон уже используется");

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (model.Password.Length < 8)
                    return BadRequest("Пароль должен содержать не менее 8 символов");

                user.Password = model.Password.Length == 64
                    ? model.Password
                    : PasswordHelper.HashPassword(model.Password);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Patronymic = model.Patronymic;
            user.Phone = model.Phone;
            user.Email = model.Email;
            user.BirthDate = model.BirthDate;

            var validationResult = _validation.ValidateUser(user);
            if (validationResult != null)
                return BadRequest(validationResult);

            await _context.SaveChangesAsync();

            return Ok("Данные обновлены");
        }

        [HttpPut("DeleteUser")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound("Пользователь не найден");

            user.IsDelete = 1;

            await _context.SaveChangesAsync();

            return Ok("Аккаунт удален");
        }

        [HttpPut("RestoreUser")]
        public async Task<ActionResult> RestoreUser(string phone, string password)
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

        [HttpPut("ChangePassword")]
        public async Task<ActionResult> ChangePassword(string phone, string newPassword)
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
                    string dbPhone = new string(u.Phone.Where(char.IsDigit).ToArray());

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

        [HttpGet("ClientsWithBookings")]
        public async Task<ActionResult> GetClientsWithBookings()
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
