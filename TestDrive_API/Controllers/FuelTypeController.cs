using Microsoft.AspNetCore.Mvc;
using TestDrive_API.Models;

namespace TestDrive_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FuelTypeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FuelTypeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAllFuelTypes()
        {
            return Ok(_context.FuelsTypes.ToList());
        }

        [HttpGet("{id}")]
        public IActionResult GetFuelType(int id)
        {
            var fuelType = _context.FuelsTypes.Find(id);
            if (fuelType == null)
                return NotFound();

            return Ok(fuelType);
        }
    }
}
