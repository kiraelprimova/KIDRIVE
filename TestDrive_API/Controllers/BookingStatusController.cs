using Microsoft.AspNetCore.Mvc;
using TestDrive_API.Models;
using TestDrive_API.Classes;

namespace TestDrive_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingStatusController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookingStatusController(AppDbContext context)
        {
            _context = context;
        }
        // Получение полного списка статусов
        [HttpGet]
        public IActionResult GetAllStatuses()
        {
            return Ok(_context.BookingStatuses.ToList());
        }
        // Получение конкретного статуса
        [HttpGet("{id}")]
        public IActionResult GetStatus(int id)
        {
            var status = _context.BookingStatuses.Find(id);

            if (status == null)
                return NotFound();

            return Ok(status);
        }
    }
}