using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using grade_management.Models;
using grade_management.Data;
using grade_management.Repositories;
using Microsoft.EntityFrameworkCore;

namespace grade_management.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class AdminDashboardController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IGradeRepository _gradeRepository;

        public AdminDashboardController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            IGradeRepository gradeRepository)
        {
            _logger = logger;
            _context = context;
            _gradeRepository = gradeRepository;
        }

        public async Task<IActionResult> Index()
        {
            var dashboardData = new DashboardViewModel
            {
                StudentCount = await _context.Students.CountAsync(),
                TeacherCount = await _context.Teachers.CountAsync(),
                SubjectCount = await _context.Subjects.CountAsync(),
                AverageGPA = await CalculateAverageGPA(),
                GradeDistribution = await CalculateGradeDistribution()
            };

            return View(dashboardData);
        }

        private async Task<double> CalculateAverageGPA()
        {
            var allGrades = await _context.Grades.ToListAsync();
            if (!allGrades.Any())
                return 0;

            var averageGPA = allGrades.Average(g => (g.FormativeGrade + g.FinalGrade) / 2.0);
            return Math.Round(averageGPA, 2);
        }

        private async Task<GradeDistributionViewModel> CalculateGradeDistribution()
        {
            var allGrades = await _context.Grades.ToListAsync();
            var distribution = new GradeDistributionViewModel();

            foreach (var grade in allGrades)
            {
                var average = (grade.FormativeGrade + grade.FinalGrade) / 2.0;
                
                if (average >= 8.5) distribution.AGrade++;
                else if (average >= 7.8) distribution.BPlusGrade++;
                else if (average >= 7.0) distribution.BGrade++;
                else if (average >= 6.3) distribution.CPlusGrade++;
                else if (average >= 5.5) distribution.CGrade++;
                else if (average >= 4.8) distribution.DPlusGrade++;
                else if (average >= 4.0) distribution.DGrade++;
                else if (average >= 3.0) distribution.FPlusGrade++;
                else distribution.FGrade++;
            }

            return distribution;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
