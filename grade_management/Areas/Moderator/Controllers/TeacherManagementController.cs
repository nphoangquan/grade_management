using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using grade_management.Models;
using grade_management.Data;
using Microsoft.EntityFrameworkCore;

namespace grade_management.Areas.Moderator.Controllers
{
    [Area("Moderator")]
    [Authorize(Roles = SD.Role_Moderator)]
    public class TeacherManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeacherManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var teachersQuery = _context.Teachers.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                teachersQuery = teachersQuery.Where(t => 
                    t.TeacherName.Contains(searchString) ||
                    t.TeacherEmail.Contains(searchString));
            }

            var teachers = await teachersQuery
                .OrderBy(t => t.TeacherName)
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(teachers);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(m => m.TeacherID == id);

            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }
    }
} 