using Microsoft.AspNetCore.Mvc;
using TestDrive_API.Models;
using TestDrive_API.Classes;

namespace TestDrive_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }
        // Получение полного списка категорий
        [HttpGet("GetAllCategories")]
        public IActionResult GetAllCategories()
        {
            return Ok(_context.Categories.ToList());
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult GetAllCategoriesLegacy() => GetAllCategories();

        // Получение конкретной категории
        [HttpGet("GetCategory/{id}")]
        public IActionResult GetCategory(int id)
        {
            var category = _context.Categories.Find(id);

            if (category == null)
                return NotFound();

            return Ok(category);
        }

        [HttpGet("{id}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult GetCategoryLegacy(int id) => GetCategory(id);

        //[HttpPost("AddCategory")]
        //public async Task<IActionResult> AddCategory(Category category)
        //{
        //    if (_context.Categories.Any(c =>
        //        c.NameCategory.ToLower() ==
        //        category.NameCategory.ToLower()))
        //    {
        //        return Ok(_context.Categories.First(c =>
        //            c.NameCategory.ToLower() ==
        //            category.NameCategory.ToLower()));
        //    }

        //    _context.Categories.Add(category);

        //    await _context.SaveChangesAsync();

        //    return Ok(category);
        //}
    }
}