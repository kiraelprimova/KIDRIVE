using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestDrive_API.Models;
using TestDrive_API.Classes;

namespace TestDrive_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FavoriteController(AppDbContext context)
        {
            _context = context;
        }

        // Получить список избранных автомобилей пользователя (К7)
        [HttpGet("GetFavorites")]
        public async Task<IActionResult> GetFavorites([FromQuery] int userId)
        {
            var favorites = await _context.Favorites
                .Where(f => f.IdUser == userId && f.IdCarNavigation.IsDeleted == 0)
                .Include(f => f.IdCarNavigation)
                    .ThenInclude(c => c.IdCategoryNavigation)
                .Include(f => f.IdCarNavigation.IdFuelTypeNavigation)
                .Include(f => f.IdCarNavigation.IdTreansmissionNavigation)
                .Select(f => new
                {
                    f.IdFavorite,
                    f.IdUser,
                    f.IdCar,
                    f.CreatedAt,
                    car = new
                    {
                        f.IdCarNavigation.IdCar,
                        f.IdCarNavigation.Model,
                        f.IdCarNavigation.Price,
                        f.IdCarNavigation.YearCar,
                        f.IdCarNavigation.PowerHp,
                        f.IdCarNavigation.ImagePath,
                        f.IdCarNavigation.Description,
                        f.IdCarNavigation.IdCategory,
                        categoryName = f.IdCarNavigation.IdCategoryNavigation.NameCategory,
                        fuelTypeName = f.IdCarNavigation.IdFuelTypeNavigation != null ? f.IdCarNavigation.IdFuelTypeNavigation.Name : null,
                        transmissionName = f.IdCarNavigation.IdTreansmissionNavigation != null ? f.IdCarNavigation.IdTreansmissionNavigation.Name : null
                    }
                })
                .ToListAsync();

            return Ok(favorites);
        }

        // Добавить автомобиль в избранное (К7)
        [HttpPost("AddFavorite")]
        public async Task<IActionResult> AddFavorite([FromBody] FavoriteDTO dto)
        {
            if (dto == null)
                return BadRequest("Неверные данные");

            var userExists = await _context.Users.AnyAsync(u => u.IdUser == dto.IdUser && u.IsDelete == 0);
            if (!userExists)
                return NotFound("Пользователь не найден");

            var car = await _context.Cars.FirstOrDefaultAsync(c => c.IdCar == dto.IdCar && c.IsDeleted == 0);
            if (car == null)
                return NotFound("Автомобиль не найден");

            var alreadyExists = await _context.Favorites.AnyAsync(f => f.IdUser == dto.IdUser && f.IdCar == dto.IdCar);
            if (alreadyExists)
                return BadRequest("Автомобиль уже добавлен в избранное");

            var favorite = new Favorite
            {
                IdUser = dto.IdUser,
                IdCar = dto.IdCar,
                CreatedAt = DateTime.Now
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok("Автомобиль добавлен в избранное");
        }

        // Удалить автомобиль из избранного
        [HttpDelete("RemoveFavorite")]
        public async Task<IActionResult> RemoveFavorite([FromQuery] int userId, [FromQuery] int carId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.IdUser == userId && f.IdCar == carId);

            if (favorite == null)
                return NotFound("Запись в избранном не найдена");

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return Ok("Автомобиль удален из избранного");
        }

        // Проверить, находится ли автомобиль в избранном
        [HttpGet("CheckFavorite")]
        [HttpGet("Check")]
        public async Task<IActionResult> IsFavorite([FromQuery] int userId, [FromQuery] int carId)
        {
            bool exists = await _context.Favorites.AnyAsync(f => f.IdUser == userId && f.IdCar == carId);
            return Ok(exists);
        }
    }
}
