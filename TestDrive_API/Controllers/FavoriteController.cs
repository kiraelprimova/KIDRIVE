using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestDrive_API.Models;

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

        [HttpGet("GetFavorites")]
        public async Task<ActionResult> GetFavorites(int userId)
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

        [HttpPost("AddFavorite")]
        public async Task<ActionResult> AddFavorite(int idUser, int idCar)
        {
            var userExists = await _context.Users.AnyAsync(u => u.IdUser == idUser && u.IsDelete == 0);
            if (!userExists)
                return NotFound("Пользователь не найден");

            var car = await _context.Cars.FirstOrDefaultAsync(c => c.IdCar == idCar && c.IsDeleted == 0);
            if (car == null)
                return NotFound("Автомобиль не найден");

            var alreadyExists = await _context.Favorites.AnyAsync(f => f.IdUser == idUser && f.IdCar == idCar);
            if (alreadyExists)
                return BadRequest("Автомобиль уже добавлен в избранное");

            var favorite = new Favorite
            {
                IdUser = idUser,
                IdCar = idCar,
                CreatedAt = DateTime.Now
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok("Автомобиль добавлен в избранное");
        }

        [HttpDelete("RemoveFavorite")]
        public async Task<ActionResult> RemoveFavorite(int userId, int carId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.IdUser == userId && f.IdCar == carId);

            if (favorite == null)
                return NotFound("Запись в избранном не найдена");

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return Ok("Автомобиль удален из избранного");
        }

        [HttpGet("CheckFavorite")]
        public async Task<ActionResult> IsFavorite(int userId, int carId)
        {
            bool exists = await _context.Favorites.AnyAsync(f => f.IdUser == userId && f.IdCar == carId);
            return Ok(exists);
        }
    }
}
