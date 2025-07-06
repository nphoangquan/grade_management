using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using grade_management.Models;
using grade_management.Data;
using Microsoft.EntityFrameworkCore;

namespace grade_management.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = SD.Role_User)]
    public class ClassManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClassManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var classesQuery = _context.Classes
                .Include(c => c.Students)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                classesQuery = classesQuery.Where(c => 
                    c.ClassName.Contains(searchString));
            }

            var classes = await classesQuery
                .OrderBy(c => c.ClassName)
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(classes);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classModel = await _context.Classes
                .Include(c => c.Students)
                .FirstOrDefaultAsync(m => m.ClassID == id);

            if (classModel == null)
            {
                return NotFound();
            }

            return View(classModel);
        }
    }
} 