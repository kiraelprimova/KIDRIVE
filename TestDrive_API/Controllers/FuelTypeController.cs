using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet("GetAllFuelTypes")]
        public async Task<ActionResult> GetAllFuelTypes()
        {
            var fuelTypes = await _context.FuelsTypes.ToListAsync();
            return Ok(fuelTypes);
        }

        [HttpGet("GetFuelType")]
        public async Task<ActionResult> GetFuelType(int id)
        {
            var fuelType = await _context.FuelsTypes.FindAsync(id);
            if (fuelType == null)
                return NotFound();

            return Ok(fuelType);
        }
    }
}
