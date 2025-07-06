using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using grade_management.Models;
using grade_management.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace grade_management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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

        // Display all students list for grade management
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

        // Display student's subjects management page
        public async Task<IActionResult> ManageSubjects(string studentId)
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

        // Display student's grades and available subjects
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

        // Add subject to student
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSubject(string studentId, int subjectId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(studentId);
            var subject = await _context.Subjects.FindAsync(subjectId);

            if (student == null || subject == null)
            {
                return NotFound();
            }

            // Check subject status
            if (subject.SubjectStatus != "Open")
            {
                TempData["Error"] = $"Cannot add student to this subject. Subject status is {subject.SubjectStatus}.";
                return RedirectToAction(nameof(ManageSubjects), new { studentId });
            }

            var existingGrade = await _context.Grades
                .FirstOrDefaultAsync(g => g.StudentID == studentId && g.SubjectID == subjectId);

            if (existingGrade != null)
            {
                TempData["Error"] = "Student is already enrolled in this subject.";
                return RedirectToAction(nameof(ManageSubjects), new { studentId });
            }

            var grade = new GradeModel
            {
                StudentID = studentId,
                SubjectID = subjectId,
                Student = student,
                Subject = subject,
                FormativeGrade = 0,
                FinalGrade = 0
            };

            // Calculate initial grades
            grade.CalculateGrades();

            _context.Grades.Add(grade);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Subject added successfully.";
            return RedirectToAction(nameof(ManageSubjects), new { studentId });
        }

        // Display form to modify grade
        public async Task<IActionResult> ModifyGrade(string studentId, int subjectId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return NotFound();
            }

            var grade = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Subject)
                .FirstOrDefaultAsync(g => g.StudentID == studentId && g.SubjectID == subjectId);

            if (grade == null)
            {
                return NotFound();
            }

            return View(grade);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifyGrade(GradeModel model)
        {
            // Get the existing grade with navigation properties
            var existingGrade = await _context.Grades
                .Include(g => g.Student)
                .Include(g => g.Subject)
                .FirstOrDefaultAsync(g => g.GradeID == model.GradeID);

            if (existingGrade == null)
            {
                TempData["Error"] = "Grade not found.";
                return RedirectToAction(nameof(StudentGrades), new { studentId = model.StudentID });
            }

            // Update the grade values
            existingGrade.FormativeGrade = model.FormativeGrade;
            existingGrade.FinalGrade = model.FinalGrade;

            // Calculate grades
            existingGrade.CalculateGrades();

            try
            {
                // Only update the specific fields we want to change
                _context.Entry(existingGrade).Property(x => x.FormativeGrade).IsModified = true;
                _context.Entry(existingGrade).Property(x => x.FinalGrade).IsModified = true;
                _context.Entry(existingGrade).Property(x => x.TenGradeScale).IsModified = true;
                _context.Entry(existingGrade).Property(x => x.FourGradeScale).IsModified = true;
                _context.Entry(existingGrade).Property(x => x.GradeToLetter).IsModified = true;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Grades updated successfully.";
                return RedirectToAction(nameof(StudentGrades), new { studentId = existingGrade.StudentID });
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Error updating grade: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                TempData["Error"] = "An error occurred while updating the grades.";
                return View(existingGrade);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGrade(string studentId, int subjectId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return NotFound();
            }

            var grade = await _context.Grades
                .FirstOrDefaultAsync(g => g.StudentID == studentId && g.SubjectID == subjectId);

            if (grade == null)
            {
                return NotFound();
            }

            _context.Grades.Remove(grade);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Subject removed successfully.";
            return RedirectToAction(nameof(ManageSubjects), new { studentId });
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

            // Create ExcelPackage with NonCommercial license for EPPlus 8+
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

            var fileName = $"DanhSachSinhVien_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                fileName);
        }

        private bool GradeExists(string studentId, int subjectId)
        {
            return _context.Grades.Any(g => g.StudentID == studentId && g.SubjectID == subjectId);
        }
    }
} 