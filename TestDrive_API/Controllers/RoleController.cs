using Microsoft.AspNetCore.Mvc;
using TestDrive_API.Models;
using TestDrive_API.Classes;

namespace TestDrive_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoleController(AppDbContext context)
        {
            _context = context;
        }
        // Получение полного списка ролей
        [HttpGet]
        public IActionResult GetAllRoles()
        {
            return Ok(_context.Roles.ToList());
        }
        // Получение конкретной роли
        [HttpGet("{id}")]
        public IActionResult GetRole(int id)
        {
            var role = _context.Roles.Find(id);

            if (role == null)
                return NotFound();

            return Ok(role);
        }
    }
}