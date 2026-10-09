using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
        public IActionResult GetAllTransmissions()
        {
            return Ok(_context.Transmissions.ToList());
        }

        [HttpGet("{id}")]
        public IActionResult GetTransmission(int id)
        {
            var transmission = _context.Transmissions.Find(id);
            if (transmission == null)
                return NotFound();

            return Ok(transmission);
        }
    }
}
