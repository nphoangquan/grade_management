using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using grade_management.Models;
using grade_management.Data;
using grade_management.Repositories;
using grade_management.Extensions;
using Microsoft.EntityFrameworkCore;

namespace grade_management.Areas.User.Controllers
{
    [Area("User")]
[Authorize(Roles = SD.Role_User + "," + SD.Role_Moderator)]
    public class UserDashboardController : Controller
    {
        private readonly ILogger<UserDashboardController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IGradeRepository _gradeRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserDashboardController(
            ILogger<UserDashboardController> logger,
            ApplicationDbContext context,
            IGradeRepository gradeRepository,
            UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _gradeRepository = gradeRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Get current user and their associated information
            var currentUser = await _userManager.GetUserAsync(User);
            StudentModel? studentInfo = null;
            TeacherModel? teacherInfo = null;
            
            if (currentUser != null)
            {
                studentInfo = await currentUser.GetStudentAsync(_context);
                teacherInfo = await currentUser.GetTeacherAsync(_context);
            }

            var dashboardData = new DashboardViewModel
            {
                StudentCount = await _context.Students.CountAsync(),
                TeacherCount = await _context.Teachers.CountAsync(),
                SubjectCount = await _context.Subjects.CountAsync(),
                AverageGPA = studentInfo != null ? await CalculateStudentGPA(studentInfo.StudentID) : await CalculateAverageGPA(),
                GradeDistribution = studentInfo != null ? await CalculateStudentGradeDistribution(studentInfo.StudentID) : await CalculateGradeDistribution()
            };

            // Pass information to view
            ViewBag.StudentInfo = studentInfo;
            ViewBag.TeacherInfo = teacherInfo;
            ViewBag.HasStudentAccount = studentInfo != null;
            ViewBag.HasTeacherAccount = teacherInfo != null;

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

        private async Task<double> CalculateStudentGPA(string studentId)
        {
            var studentGrades = await _context.Grades
                .Where(g => g.StudentID == studentId)
                .ToListAsync();

            if (!studentGrades.Any())
                return 0;

            var gpa = studentGrades.Average(g => g.FourGradeScale);
            return Math.Round(gpa, 2);
        }

        private async Task<GradeDistributionViewModel> CalculateStudentGradeDistribution(string studentId)
        {
            var studentGrades = await _context.Grades
                .Where(g => g.StudentID == studentId)
                .ToListAsync();

            var distribution = new GradeDistributionViewModel();

            foreach (var grade in studentGrades)
            {
                switch (grade.GradeToLetter)
                {
                    case "A": distribution.AGrade++; break;
                    case "B+": distribution.BPlusGrade++; break;
                    case "B": distribution.BGrade++; break;
                    case "C+": distribution.CPlusGrade++; break;
                    case "C": distribution.CGrade++; break;
                    case "D+": distribution.DPlusGrade++; break;
                    case "D": distribution.DGrade++; break;
                    case "F+": distribution.FPlusGrade++; break;
                    case "F": distribution.FGrade++; break;
                }
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