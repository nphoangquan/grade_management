using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using grade_management.Models;
using grade_management.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace grade_management.Areas.Moderator.Controllers
{
    [Area("Moderator")]
    [Authorize(Roles = SD.Role_Moderator)]
    public class SubjectManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubjectManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var subjectsQuery = _context.Subjects
                .Include(s => s.Grades)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                subjectsQuery = subjectsQuery.Where(s => 
                    s.SubjectName.Contains(searchString) ||
                    s.SubjectCode.Contains(searchString));
            }

            var subjects = await subjectsQuery
                .OrderBy(s => s.SubjectName)
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(subjects);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subject = await _context.Subjects
                .Include(s => s.Grades)
                    .ThenInclude(g => g.Student)
                .FirstOrDefaultAsync(m => m.SubjectID == id);

            if (subject == null)
            {
                return NotFound();
            }

            return View(subject);
        }

        public async Task<IActionResult> SubjectStudents(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subject = await _context.Subjects
                .Include(s => s.Grades)
                    .ThenInclude(g => g.Student)
                        .ThenInclude(s => s.Class)
                .FirstOrDefaultAsync(m => m.SubjectID == id);

            if (subject == null)
            {
                return NotFound();
            }

            return View(subject);
        }

        public async Task<IActionResult> ExportToExcel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subject = await _context.Subjects
                .Include(s => s.Grades)
                    .ThenInclude(g => g.Student)
                        .ThenInclude(s => s.Class)
                .FirstOrDefaultAsync(m => m.SubjectID == id);

            if (subject == null)
            {
                return NotFound();
            }


            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Danh sách sinh viên");

                // Add title
                worksheet.Cells[1, 1].Value = $"Danh sách sinh viên - {subject.SubjectName} ({subject.SubjectCode})";
                worksheet.Cells[1, 1, 1, 8].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Size = 14;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Add headers
                var headers = new[] { "Mã sinh viên", "Họ và tên", "Lớp", "Điểm quá trình", "Điểm cuối kỳ", "Thang điểm 10", "Thang điểm 4", "Điểm chữ" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[3, i + 1].Value = headers[i];
                    worksheet.Cells[3, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[3, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[3, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }

                // Add data
                var row = 4;
                foreach (var grade in subject.Grades.OrderBy(g => g.Student.StudentName))
                {
                    worksheet.Cells[row, 1].Value = grade.Student.StudentCode;
                    worksheet.Cells[row, 2].Value = grade.Student.StudentName;
                    worksheet.Cells[row, 3].Value = grade.Student.Class?.ClassName;
                    worksheet.Cells[row, 4].Value = grade.FormativeGrade;
                    worksheet.Cells[row, 5].Value = grade.FinalGrade;
                    worksheet.Cells[row, 6].Value = grade.TenGradeScale;
                    worksheet.Cells[row, 7].Value = grade.FourGradeScale;
                    worksheet.Cells[row, 8].Value = grade.GradeToLetter;
                    row++;
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Add borders
                var dataRange = worksheet.Cells[3, 1, row - 1, 8];
                dataRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                dataRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                // Return the Excel file
                var content = package.GetAsByteArray();
                var fileName = $"Danh_sach_sinh_vien_{subject.SubjectCode}_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }
} 