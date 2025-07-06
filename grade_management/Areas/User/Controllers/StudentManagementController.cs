using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using grade_management.Models;
using grade_management.Data;
using Microsoft.EntityFrameworkCore;

namespace grade_management.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = SD.Role_User)]
    public class StudentManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var studentsQuery = _context.Students
                .Include(s => s.Class)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                studentsQuery = studentsQuery.Where(s => 
                    s.StudentName.Contains(searchString) || 
                    s.StudentCode.Contains(searchString) ||
                    s.StudentEmail.Contains(searchString) ||
                    s.Class.ClassName.Contains(searchString));
            }

            var students = await studentsQuery
                .OrderBy(s => s.StudentName)
                .ToListAsync();

            // Pass the search string back to the view to maintain the search box value
            ViewData["CurrentFilter"] = searchString;

            return View(students);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Class)
                .FirstOrDefaultAsync(m => m.StudentID == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }
    }
} 