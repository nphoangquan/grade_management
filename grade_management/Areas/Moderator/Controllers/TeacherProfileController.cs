using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using grade_management.Data;
using grade_management.Extensions;
using grade_management.Models;
using grade_management.Models.ViewModels;

namespace grade_management.Areas.User.Controllers
{
    [Area("Moderator")]
    [Authorize(Roles = SD.Role_Moderator)]
    public class TeacherProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TeacherProfileController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var teacher = await user.GetTeacherAsync(_context);
            if (teacher == null)
            {
                TempData["Error"] = "No teacher profile found for this account.";
                return RedirectToAction("Index", "UserDashboard");
            }

            // Get teaching statistics (placeholder - will be implemented when we have subjects/classes relationship)
            var teachingStats = await GetTeachingStatistics(teacher.TeacherID);

            var viewModel = new TeacherProfileViewModel
            {
                TeacherID = teacher.TeacherID,
                TeacherCode = teacher.TeacherCode,
                TeacherName = teacher.TeacherName,
                TeacherSex = teacher.TeacherSex,
                TeacherEmail = teacher.TeacherEmail,
                UserName = user.UserName,
                FullName = user.FullName,
                EmailConfirmed = user.EmailConfirmed,
                ImagePath = teacher.ImagePath,
                TotalSubjectsTaught = teachingStats.SubjectCount,
                TotalStudentsTeaching = teachingStats.StudentCount,
                TotalClassesManaged = teachingStats.ClassCount
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Students()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var teacher = await user.GetTeacherAsync(_context);
            if (teacher == null)
            {
                TempData["Error"] = "No teacher profile found for this account.";
                return RedirectToAction("Index", "UserDashboard");
            }

            // For now, show all students. Later this will be filtered by teacher's subjects/classes
            var students = await _context.Students
                .Include(s => s.Class)
                .OrderBy(s => s.StudentName)
                .ToListAsync();

            ViewBag.TeacherInfo = teacher;
            return View(students);
        }

        public async Task<IActionResult> Subjects()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var teacher = await user.GetTeacherAsync(_context);
            if (teacher == null)
            {
                TempData["Error"] = "No teacher profile found for this account.";
                return RedirectToAction("Index", "UserDashboard");
            }

            // For now, show all subjects. Later this will be filtered by teacher's assigned subjects
            var subjects = await _context.Subjects
                .OrderBy(s => s.SubjectName)
                .ToListAsync();

            ViewBag.TeacherInfo = teacher;
            return View(subjects);
        }

        private async Task<(int SubjectCount, int StudentCount, int ClassCount)> GetTeachingStatistics(string teacherId)
        {
            // Placeholder implementation - will be updated when we have proper relationships
            var totalSubjects = await _context.Subjects.CountAsync();
            var totalStudents = await _context.Students.CountAsync();
            var totalClasses = await _context.Classes.CountAsync();

            return (totalSubjects, totalStudents, totalClasses);
        }

        public async Task<IActionResult> EditImage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var teacher = await user.GetTeacherAsync(_context);
            if (teacher == null)
            {
                TempData["Error"] = "No teacher profile found for this account.";
                return RedirectToAction("Index", "UserDashboard");
            }

            var viewModel = new EditTeacherImageViewModel
            {
                CurrentImagePath = teacher.ImagePath,
                Teacher = teacher
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditImage(EditTeacherImageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var teacher = await user.GetTeacherAsync(_context);
            if (teacher == null)
            {
                TempData["Error"] = "No teacher profile found for this account.";
                return RedirectToAction("Index", "UserDashboard");
            }

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(teacher.ImagePath))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, teacher.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Save new image
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "teachers");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = $"{teacher.TeacherID}_{Guid.NewGuid()}{Path.GetExtension(model.ImageFile.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

                teacher.ImagePath = $"/images/teachers/{uniqueFileName}";
                await _context.SaveChangesAsync();

                TempData["Success"] = "Profile picture updated successfully";
            }

            return RedirectToAction("Index");
        }
    }
} 