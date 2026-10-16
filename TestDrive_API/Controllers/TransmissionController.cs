using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestDrive_API.Models;

namespace TestDrive_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransmissionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransmissionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetAllTransmissions")]
        public async Task<ActionResult> GetAllTransmissions()
        {
            var transmissions = await _context.Transmissions.ToListAsync();
            return Ok(transmissions);
        }

        [HttpGet("GetTransmission")]
        public async Task<ActionResult> GetTransmission(int id)
        {
            var transmission = await _context.Transmissions.FindAsync(id);
            if (transmission == null)
                return NotFound();

            return Ok(transmission);
        }
    }
}
