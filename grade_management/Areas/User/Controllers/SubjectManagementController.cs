using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using grade_management.Models;
using grade_management.Data;
using Microsoft.EntityFrameworkCore;

namespace grade_management.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = SD.Role_User)]
    public class SubjectManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubjectManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, string statusFilter)
        {
            var subjectsQuery = _context.Subjects.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                subjectsQuery = subjectsQuery.Where(s => 
                    s.SubjectName.Contains(searchString) ||
                    s.SubjectCode.Contains(searchString));
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                subjectsQuery = subjectsQuery.Where(s => s.SubjectStatus == statusFilter);
            }

            var subjects = await subjectsQuery
                .OrderBy(s => s.SubjectCode)
                .ToListAsync();

            // Populate filter dropdowns
            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = statusFilter;
            ViewData["StatusList"] = new List<string> { "All", "Open", "Full", "Closed" };

            return View(subjects);
        }

        public async Task<IActionResult> Details(int id)
        {
            var subject = await _context.Subjects
                .FirstOrDefaultAsync(m => m.SubjectID == id);

            if (subject == null)
            {
                return NotFound();
            }

            return View(subject);
        }
    }
} 