using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using grade_management.Data;
using grade_management.Extensions;
using grade_management.Models;
using grade_management.Models.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace grade_management.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = SD.Role_User)]
    public class StudentProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StudentProfileController(
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
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("User not found");
            }

            var studentInfo = await currentUser.GetStudentAsync(_context);
            if (studentInfo == null)
            {
                ViewBag.NoStudentAccount = true;
                return View();
            }

            var recentGrades = await _context.Grades
                .Include(g => g.Subject)
                .Where(g => g.StudentID == studentInfo.StudentID)
                .OrderByDescending(g => g.GradeID)
                .Take(10)
                .ToListAsync();

            var allGrades = await _context.Grades
                .Where(g => g.StudentID == studentInfo.StudentID)
                .ToListAsync();

            var gpa = allGrades.Any() ? allGrades.Average(g => g.FourGradeScale) : 0;

            var viewModel = new StudentProfileViewModel
            {
                Student = studentInfo,
                User = currentUser,
                RecentGrades = recentGrades,
                GPA = Math.Round(gpa, 2),
                TotalSubjects = allGrades.Count,
                CompletedSubjects = allGrades.Count(g => g.FourGradeScale >= 1.0),
                AcademicStatus = gpa >= 2.0 ? "Tốt" : "Cảnh báo học tập"
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Grades()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("User not found");
            }

            var studentInfo = await currentUser.GetStudentAsync(_context);
            if (studentInfo == null)
            {
                TempData["Error"] = "No student account linked to this user";
                return RedirectToAction("Index");
            }

            var grades = await _context.Grades
                .Include(g => g.Subject)
                .Where(g => g.StudentID == studentInfo.StudentID)
                .OrderBy(g => g.Subject.SubjectName)
                .ToListAsync();

            ViewBag.StudentInfo = studentInfo;
            return View(grades);
        }

        public async Task<IActionResult> Schedule()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("User not found");
            }

            var studentInfo = await currentUser.GetStudentAsync(_context);
            if (studentInfo == null)
            {
                TempData["Error"] = "No student account linked to this user";
                return RedirectToAction("Index");
            }

            var enrolledSubjects = await _context.Grades
                .Include(g => g.Subject)
                .Where(g => g.StudentID == studentInfo.StudentID)
                .Select(g => g.Subject)
                .Distinct()
                .ToListAsync();

            ViewBag.StudentInfo = studentInfo;
            return View(enrolledSubjects);
        }

        public async Task<IActionResult> EditImage()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("User not found");
            }

            var studentInfo = await currentUser.GetStudentAsync(_context);
            if (studentInfo == null)
            {
                TempData["Error"] = "No student account linked to this user";
                return RedirectToAction("Index");
            }

            var viewModel = new EditImageViewModel
            {
                CurrentImagePath = studentInfo.ImagePath,
                Student = studentInfo
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditImage(EditImageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("User not found");
            }

            var studentInfo = await currentUser.GetStudentAsync(_context);
            if (studentInfo == null)
            {
                TempData["Error"] = "No student account linked to this user";
                return RedirectToAction("Index");
            }

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(studentInfo.ImagePath))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, studentInfo.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Save new image
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = $"{studentInfo.StudentID}_{Guid.NewGuid()}{Path.GetExtension(model.ImageFile.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

                studentInfo.ImagePath = $"/images/profiles/{uniqueFileName}";
                await _context.SaveChangesAsync();

                TempData["Success"] = "Profile picture updated successfully";
            }

            return RedirectToAction("Index");
        }

        // Export grades to Excel
        public async Task<IActionResult> ExportGradesToExcel()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound("User not found");
            }

            var studentInfo = await currentUser.GetStudentAsync(_context);
            if (studentInfo == null)
            {
                TempData["Error"] = "No student account linked to this user";
                return RedirectToAction("Index");
            }

            var grades = await _context.Grades
                .Include(g => g.Subject)
                .Where(g => g.StudentID == studentInfo.StudentID)
                .OrderBy(g => g.Subject.SubjectCode)
                .ToListAsync();

            if (!grades.Any())
            {
                TempData["Error"] = "Không có điểm để xuất";
                return RedirectToAction("Grades");
            }

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Bảng điểm cá nhân");

            // Student info header
            worksheet.Cells[1, 1].Value = "BẢNG ĐIỂM CÁ NHÂN";
            worksheet.Cells[1, 1, 1, 9].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells[2, 1].Value = $"Sinh viên: {studentInfo.StudentName}";
            worksheet.Cells[2, 1, 2, 4].Merge = true;
            worksheet.Cells[2, 6].Value = $"Mã SV: {studentInfo.StudentCode}";
            worksheet.Cells[2, 6, 2, 9].Merge = true;

            worksheet.Cells[3, 1].Value = $"Lớp: {studentInfo.Class?.ClassName ?? "Chưa có lớp"}";
            worksheet.Cells[3, 1, 3, 4].Merge = true;
            var gpa = grades.Average(g => g.FourGradeScale);
            worksheet.Cells[3, 6].Value = $"GPA: {gpa:F2}";
            worksheet.Cells[3, 6, 3, 9].Merge = true;

            // Headers
            int headerRow = 5;
            worksheet.Cells[headerRow, 1].Value = "STT";
            worksheet.Cells[headerRow, 2].Value = "Mã môn học";
            worksheet.Cells[headerRow, 3].Value = "Tên môn học";
            worksheet.Cells[headerRow, 4].Value = "Tín chỉ";
            worksheet.Cells[headerRow, 5].Value = "Điểm quá trình";
            worksheet.Cells[headerRow, 6].Value = "Điểm cuối kỳ";
            worksheet.Cells[headerRow, 7].Value = "Điểm 10";
            worksheet.Cells[headerRow, 8].Value = "Điểm 4";
            worksheet.Cells[headerRow, 9].Value = "Điểm chữ";
            worksheet.Cells[headerRow, 10].Value = "Trạng thái";

            // Style headers
            using (var range = worksheet.Cells[headerRow, 1, headerRow, 10])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // Data
            int row = headerRow + 1;
            int stt = 1;
            
            foreach (var grade in grades)
            {
                worksheet.Cells[row, 1].Value = stt++;
                worksheet.Cells[row, 2].Value = grade.Subject?.SubjectCode;
                worksheet.Cells[row, 3].Value = grade.Subject?.SubjectName;
                worksheet.Cells[row, 4].Value = grade.Subject?.SubjectCredits;
                worksheet.Cells[row, 5].Value = grade.FormativeGrade;
                worksheet.Cells[row, 6].Value = grade.FinalGrade;
                worksheet.Cells[row, 7].Value = grade.TenGradeScale;
                worksheet.Cells[row, 8].Value = grade.FourGradeScale;
                worksheet.Cells[row, 9].Value = grade.GradeToLetter;
                worksheet.Cells[row, 10].Value = grade.FourGradeScale >= 1.0 ? "Đạt" : "Không đạt";

                row++;
            }

            // Summary statistics
            int summaryRow = row + 2;
            worksheet.Cells[summaryRow, 1].Value = "THỐNG KÊ TỔNG KẾT";
            worksheet.Cells[summaryRow, 1, summaryRow, 10].Merge = true;
            worksheet.Cells[summaryRow, 1].Style.Font.Bold = true;
            worksheet.Cells[summaryRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            summaryRow++;
            worksheet.Cells[summaryRow, 1].Value = "Tổng số môn học:";
            worksheet.Cells[summaryRow, 2].Value = grades.Count;
            worksheet.Cells[summaryRow, 4].Value = "Số môn đạt:";
            worksheet.Cells[summaryRow, 5].Value = grades.Count(g => g.FourGradeScale >= 1.0);
            worksheet.Cells[summaryRow, 7].Value = "Số môn không đạt:";
            worksheet.Cells[summaryRow, 8].Value = grades.Count(g => g.FourGradeScale < 1.0);

            summaryRow++;
            worksheet.Cells[summaryRow, 1].Value = "GPA:";
            worksheet.Cells[summaryRow, 2].Value = gpa.ToString("F2");

            // Grade distribution
            summaryRow += 2;
            worksheet.Cells[summaryRow, 1].Value = "PHÂN PHỐI ĐIỂM";
            worksheet.Cells[summaryRow, 1, summaryRow, 10].Merge = true;
            worksheet.Cells[summaryRow, 1].Style.Font.Bold = true;
            worksheet.Cells[summaryRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            summaryRow++;
            var gradeGroups = grades.GroupBy(g => g.GradeToLetter).OrderBy(g => g.Key);
            int col = 1;
            foreach (var group in gradeGroups)
            {
                worksheet.Cells[summaryRow, col].Value = $"{group.Key}:";
                worksheet.Cells[summaryRow, col + 1].Value = group.Count();
                col += 2;
            }

            // Auto-fit columns
            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            // Add borders to all data
            if (row > headerRow + 1)
            {
                using (var range = worksheet.Cells[headerRow, 1, row - 1, 10])
                {
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                // Center align numeric columns
                worksheet.Cells[headerRow + 1, 1, row - 1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[headerRow + 1, 4, row - 1, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            var fileName = $"BangDiem_{studentInfo.StudentCode}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                fileName);
        }
    }
} 