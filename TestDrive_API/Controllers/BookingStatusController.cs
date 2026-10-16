using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestDrive_API.Models;

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

        [HttpGet("GetAllStatuses")]
        public async Task<ActionResult> GetAllStatuses()
        {
            var statuses = await _context.BookingStatuses.ToListAsync();
            return Ok(statuses);
        }

        [HttpGet("GetStatus")]
        public async Task<ActionResult> GetStatus(int id)
        {
            var status = await _context.BookingStatuses.FindAsync(id);

            if (status == null)
                return NotFound();

            return Ok(status);
        }
    }
}