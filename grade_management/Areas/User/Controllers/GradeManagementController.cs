using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using grade_management.Models;
using grade_management.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace grade_management.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = SD.Role_User)]
    public class GradeManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GradeManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display list of classes
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

        // Display all students list for grade viewing
        public async Task<IActionResult> AllStudents(string searchString)
        {
            var studentsQuery = _context.Students
                .Include(s => s.Class)
                .Include(s => s.Grades)
                    .ThenInclude(g => g.Subject)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                studentsQuery = studentsQuery.Where(s => 
                    s.StudentName.Contains(searchString) ||
                    s.StudentCode.Contains(searchString) ||
                    s.StudentEmail.Contains(searchString) ||
                    (s.Class != null && s.Class.ClassName.Contains(searchString)));
            }

            var students = await studentsQuery
                .OrderBy(s => s.Class != null ? s.Class.ClassName : "")
                .ThenBy(s => s.StudentName)
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(students);
        }

        // Display students in a specific class
        public async Task<IActionResult> ClassStudents(string classId)
        {
            if (string.IsNullOrEmpty(classId))
            {
                return NotFound();
            }

            var classModel = await _context.Classes
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.ClassID == classId);

            if (classModel == null)
            {
                return NotFound();
            }

            return View(classModel);
        }

        // Display student's grades and subjects (read-only)
        public async Task<IActionResult> StudentGrades(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Class)
                .Include(s => s.Grades)
                    .ThenInclude(g => g.Subject)
                .FirstOrDefaultAsync(s => s.StudentID == studentId);

            if (student == null)
            {
                return NotFound();
            }

            // Get all available subjects
            var allSubjects = await _context.Subjects
                .Where(s => s.SubjectStatus == "Open" || s.SubjectStatus == "Full")
                .ToListAsync();

            // Create view model
            var viewModel = new StudentGradesViewModel
            {
                Student = student,
                AvailableSubjects = allSubjects,
                ExistingGrades = student.Grades ?? new List<GradeModel>()
            };

            return View(viewModel);
        }

        // Display grade details (read-only)
        public async Task<IActionResult> GradeDetails(string studentId, int subjectId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return NotFound();
            }

            var grade = await _context.Grades
                .Include(g => g.Student)
                    .ThenInclude(s => s.Class)
                .Include(g => g.Subject)
                .FirstOrDefaultAsync(g => g.StudentID == studentId && g.SubjectID == subjectId);

            if (grade == null)
            {
                return NotFound();
            }

            return View(grade);
        }

        // Export to Excel
        public async Task<IActionResult> ExportToExcel(string searchString)
        {
            // EPPlus license configured in Program.cs
            var studentsQuery = _context.Students
                .Include(s => s.Class)
                .Include(s => s.Grades)
                    .ThenInclude(g => g.Subject)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                studentsQuery = studentsQuery.Where(s => 
                    s.StudentName.Contains(searchString) ||
                    s.StudentCode.Contains(searchString) ||
                    s.StudentEmail.Contains(searchString) ||
                    (s.Class != null && s.Class.ClassName.Contains(searchString)));
            }

            var students = await studentsQuery
                .OrderBy(s => s.Class != null ? s.Class.ClassName : "")
                .ThenBy(s => s.StudentName)
                .ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Danh sách sinh viên");

            // Headers
            worksheet.Cells[1, 1].Value = "STT";
            worksheet.Cells[1, 2].Value = "Mã sinh viên";
            worksheet.Cells[1, 3].Value = "Tên sinh viên";
            worksheet.Cells[1, 4].Value = "Giới tính";
            worksheet.Cells[1, 5].Value = "Email";
            worksheet.Cells[1, 6].Value = "Lớp";
            worksheet.Cells[1, 7].Value = "Số môn học";
            worksheet.Cells[1, 8].Value = "Điểm trung bình";
            worksheet.Cells[1, 9].Value = "Xếp loại";

            // Style headers
            using (var range = worksheet.Cells[1, 1, 1, 9])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Data
            int row = 2;
            int stt = 1;
            
            foreach (var student in students)
            {
                var enrolledSubjects = student.Grades?.Count() ?? 0;
                var averageGpa = enrolledSubjects > 0 && student.Grades != null
                    ? Math.Round(student.Grades.Average(g => (g.FormativeGrade + g.FinalGrade) / 2.0), 2)
                    : 0.0;

                string classification = averageGpa >= 8.5 ? "Xuất sắc" :
                                       averageGpa >= 7.0 ? "Giỏi" :
                                       averageGpa >= 6.5 ? "Khá" :
                                       averageGpa >= 5.0 ? "Trung bình" :
                                       enrolledSubjects > 0 ? "Yếu" : "Chưa có điểm";

                worksheet.Cells[row, 1].Value = stt++;
                worksheet.Cells[row, 2].Value = student.StudentCode;
                worksheet.Cells[row, 3].Value = student.StudentName;
                worksheet.Cells[row, 4].Value = student.StudentSex;
                worksheet.Cells[row, 5].Value = student.StudentEmail;
                worksheet.Cells[row, 6].Value = student.Class?.ClassName ?? "Chưa có lớp";
                worksheet.Cells[row, 7].Value = enrolledSubjects;
                worksheet.Cells[row, 8].Value = enrolledSubjects > 0 ? averageGpa.ToString("F2") : "N/A";
                worksheet.Cells[row, 9].Value = classification;

                row++;
            }

            // Auto-fit columns
            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            // Add borders to all data
            if (row > 2)
            {
                using (var range = worksheet.Cells[1, 1, row - 1, 9])
                {
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                // Center align numeric columns
                worksheet.Cells[2, 1, row - 1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[2, 7, row - 1, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            var fileName = $"DanhSachSinhVien_User_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                fileName);
        }
    }
} 