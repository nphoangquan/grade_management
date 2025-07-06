using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using grade_management.Data;
using grade_management.Models;

namespace grade_management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class SubjectManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubjectManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            try
            {
                var subjectsQuery = _context.Subjects
                    .Include(s => s.Department)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchString))
                {
                    subjectsQuery = subjectsQuery.Where(s => 
                        s.SubjectCode.Contains(searchString) || 
                        s.SubjectName.Contains(searchString) ||
                        s.SubjectTime.Contains(searchString) ||
                        s.SubjectStatus.Contains(searchString) ||
                        s.Department.DepartmentName.Contains(searchString) ||
                        s.Department.DepartmentCode.Contains(searchString));
                }

                var subjects = await subjectsQuery
                    .OrderBy(s => s.Department.DepartmentName)
                    .ThenBy(s => s.SubjectName)
                    .ToListAsync();

                // Pass the search string back to the view to maintain the search box value
                ViewData["CurrentFilter"] = searchString;

                return View(subjects);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while loading subjects. Please try again.";
                return View(new List<SubjectModel>());
            }
        }

        private void PopulateDepartmentsDropDown(object selectedDepartment = null)
        {
            var departmentsQuery = _context.Departments
                .OrderBy(d => d.DepartmentName)
                .AsNoTracking();
            ViewBag.Departments = new SelectList(departmentsQuery, "DepartmentID", "DepartmentName", selectedDepartment);
        }

        public IActionResult Create()
        {
            PopulateDepartmentsDropDown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubjectModel subject)
        {
            // Log the model state
            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        TempData["Error"] = TempData["Error"] + error.ErrorMessage + "; ";
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if subject is null
                    if (subject == null)
                    {
                        TempData["Error"] = "Subject data is null";
                        PopulateDepartmentsDropDown();
                        return View(subject);
                    }

                    // Log the subject data
                    TempData["Debug"] = $"Subject Code: {subject.SubjectCode}, Name: {subject.SubjectName}, Department: {subject.DepartmentID}";

                    // Check for duplicate subject code within the same department
                    if (await _context.Subjects.AnyAsync(s => 
                        s.SubjectCode == subject.SubjectCode && 
                        s.DepartmentID == subject.DepartmentID))
                    {
                        ModelState.AddModelError("SubjectCode", "This subject code is already in use in the selected department.");
                        TempData["Error"] = "Duplicate subject code in department";
                        PopulateDepartmentsDropDown(subject.DepartmentID);
                        return View(subject);
                    }

                    _context.Add(subject);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Subject created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Unable to create subject. Please try again, and if the problem persists, contact your system administrator.");
                    TempData["Error"] = $"Database error: {ex.InnerException?.Message ?? ex.Message}";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                    TempData["Error"] = $"Unexpected error: {ex.Message}";
                }
            }
            PopulateDepartmentsDropDown(subject.DepartmentID);
            return View(subject);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var subject = await _context.Subjects
                    .Include(s => s.Department)
                    .FirstOrDefaultAsync(s => s.SubjectID == id);

                if (subject == null)
                {
                    return NotFound();
                }

                PopulateDepartmentsDropDown(subject.DepartmentID);
                return View(subject);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while loading the subject. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubjectModel subject)
        {
            if (id != subject.SubjectID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check for duplicate subject code within the same department, excluding current subject
                    if (await _context.Subjects.AnyAsync(s => 
                        s.SubjectCode == subject.SubjectCode && 
                        s.DepartmentID == subject.DepartmentID && 
                        s.SubjectID != subject.SubjectID))
                    {
                        ModelState.AddModelError("SubjectCode", "This subject code is already in use in the selected department.");
                        PopulateDepartmentsDropDown(subject.DepartmentID);
                        return View(subject);
                    }

                    _context.Update(subject);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Subject updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectExists(subject.SubjectID))
                    {
                        return NotFound();
                    }
                    ModelState.AddModelError("", "Unable to save changes. The subject was modified by another user.");
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Please try again, and if the problem persists, contact your system administrator.");
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                }
            }
            PopulateDepartmentsDropDown(subject.DepartmentID);
            return View(subject);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var subject = await _context.Subjects
                    .Include(s => s.Department)
                    .Include(s => s.Grades)
                    .FirstOrDefaultAsync(s => s.SubjectID == id);

                if (subject == null)
                {
                    return NotFound();
                }

                // Check if there are any students enrolled (grades exist)
                if (subject.Grades != null && subject.Grades.Any())
                {
                    TempData["Error"] = "Cannot delete this subject because it has enrolled students.";
                    return RedirectToAction(nameof(Index));
                }

                return View(subject);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while loading the subject. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var subject = await _context.Subjects
                    .Include(s => s.Department)
                    .Include(s => s.Grades)
                    .FirstOrDefaultAsync(s => s.SubjectID == id);

                if (subject == null)
                {
                    return NotFound();
                }

                // Double-check for enrolled students before deletion
                if (subject.Grades != null && subject.Grades.Any())
                {
                    TempData["Error"] = "Cannot delete this subject because it has enrolled students.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Subjects.Remove(subject);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Subject deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while deleting the subject. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var subject = await _context.Subjects
                    .Include(s => s.Department)
                    .Include(s => s.Grades)
                    .ThenInclude(g => g.Student)
                    .FirstOrDefaultAsync(s => s.SubjectID == id);

                if (subject == null)
                {
                    return NotFound();
                }

                return View(subject);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while loading the subject details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool SubjectExists(int id)
        {
            return _context.Subjects.Any(e => e.SubjectID == id);
        }

        // Display form to add students to subject
        public async Task<IActionResult> AddStudents(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subject = await _context.Subjects
                .Include(s => s.Department)
                .Include(s => s.Grades)
                    .ThenInclude(g => g.Student)
                .FirstOrDefaultAsync(s => s.SubjectID == id);

            if (subject == null)
            {
                return NotFound();
            }

            // Get all students not enrolled in this subject and from the same department
            var enrolledStudentIds = subject.Grades?.Select(g => g.StudentID).ToList() ?? new List<string>();
            var availableStudents = await _context.Students
                .Include(s => s.Class)
                .Where(s => !enrolledStudentIds.Contains(s.StudentID) && 
                           s.Class != null && 
                           s.Class.DepartmentID == subject.DepartmentID)
                .OrderBy(s => s.Class.ClassName)
                .ThenBy(s => s.StudentName)
                .ToListAsync();

            ViewBag.Subject = subject;
            return View(availableStudents);
        }

        // Add students to subject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudents(int id, List<string> selectedStudents)
        {
            if (id == null || selectedStudents == null || !selectedStudents.Any())
            {
                TempData["Error"] = "Please select at least one student to add.";
                return RedirectToAction(nameof(AddStudents), new { id });
            }

            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return NotFound();
            }

            // Check subject status
            if (subject.SubjectStatus != "Open")
            {
                TempData["Error"] = $"Cannot add students to this subject. Subject status is {subject.SubjectStatus}.";
                return RedirectToAction(nameof(AddStudents), new { id });
            }

            try
            {
                foreach (var studentId in selectedStudents)
                {
                    // Check if student is already enrolled
                    var existingGrade = await _context.Grades
                        .FirstOrDefaultAsync(g => g.StudentID == studentId && g.SubjectID == id);

                    if (existingGrade == null)
                    {
                        var student = await _context.Students.FindAsync(studentId);
                        if (student != null)
                        {
                            var grade = new GradeModel
                            {
                                StudentID = studentId,
                                SubjectID = id,
                                Student = student,
                                Subject = subject,
                                FormativeGrade = 0,
                                FinalGrade = 0
                            };

                            // Calculate initial grades
                            grade.CalculateGrades();

                            _context.Grades.Add(grade);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Students added successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while adding students: {ex.Message}";
                return RedirectToAction(nameof(AddStudents), new { id });
            }
        }
    }
} 