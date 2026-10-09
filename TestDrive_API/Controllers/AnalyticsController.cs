using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestDrive_API.Models;

namespace TestDrive_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            // Считаем только активные (не удаленные) автомобили
            var totalCars = await _context.Cars.CountAsync(c => c.IsDeleted == 0);
            var totalBookings = await _context.Bookings.CountAsync();
            var totalClients = await _context.Users.CountAsync(u => u.IsDelete == 0 && u.IdRole == 1);

            var confirmedBookings = await _context.Bookings.CountAsync(b => b.IdStatus == 2);

            // Безопасное вычисление суммы с дефолтным значением 0
            decimal revenue = await _context.Bookings
                .Where(b => b.IdStatus == 2)
                .SumAsync(b => (decimal?)b.IdCarNavigation.Price) ?? 0m;

            return Ok(new
            {
                totalCars,
                totalBookings,
                totalClients,
                confirmedBookings,
                totalRevenue = revenue
            });
        }

        // Популярные машины
        [HttpGet("PopularCars")]
        public async Task<IActionResult> PopularCars()
        {
            var data = await _context.Bookings
                .GroupBy(b => b.IdCarNavigation.Model)
                .Select(g => new
                {
                    name = g.Key,
                    total = g.Count()
                })
                .OrderByDescending(x => x.total)
                .Take(5)
                .ToListAsync();

            return Ok(data);
        }

        // Статусы бронирований
        [HttpGet("BookingStatuses")]
        public async Task<IActionResult> BookingStatuses()
        {
            var data = await _context.Bookings
                .GroupBy(b => b.IdStatusNavigation.StatusName)
                .Select(g => new
                {
                    status = g.Key,
                    total = g.Count()
                })
                .ToListAsync();

            return Ok(data);
        }

        // Частые даты бронирований
        [HttpGet("BookingsByDate")]
        public async Task<IActionResult> BookingsByDate()
        {
            var data = await _context.Bookings
                .GroupBy(b => b.BookingDate)
                .Select(g => new
                {
                    date = g.Key,
                    total = g.Count()
                })
                .OrderBy(x => x.date)
                .ToListAsync();

            return Ok(data);
        }

        // Популярные бренды / категории автомобилей
        [HttpGet("PopularBrands")]
        public async Task<IActionResult> PopularBrands()
        {
            var data = await _context.Bookings
                .GroupBy(b => b.IdCarNavigation.IdCategoryNavigation.NameCategory)
                .Select(g => new
                {
                    brand = g.Key,
                    total = g.Count()
                })
                .OrderByDescending(x => x.total)
                .ToListAsync();

            return Ok(data);
        }
    }
}