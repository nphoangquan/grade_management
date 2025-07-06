using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using grade_management.Models;
using grade_management.Models.ViewModels;
using grade_management.Data;
using Microsoft.EntityFrameworkCore;

namespace grade_management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class StudentManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public StudentManagementController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var studentsQuery = _context.Students
                .Include(s => s.Class)
                .Include(s => s.Department)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                studentsQuery = studentsQuery.Where(s => 
                    s.StudentName.Contains(searchString) || 
                    s.StudentCode.Contains(searchString) ||
                    s.StudentEmail.Contains(searchString) ||
                    s.Class.ClassName.Contains(searchString) ||
                    s.Department.DepartmentName.Contains(searchString) ||
                    s.Department.DepartmentCode.Contains(searchString));
            }

            var students = await studentsQuery
                .OrderBy(s => s.Department.DepartmentName)
                .ThenBy(s => s.Class.ClassName)
                .ThenBy(s => s.StudentName)
                .ToListAsync();

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
                .Include(s => s.Department)
                .FirstOrDefaultAsync(m => m.StudentID == id);

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        private async Task LoadClassesAsync()
        {
            var classes = await _context.Classes
                .Include(c => c.Department)
                .OrderBy(c => c.Department.DepartmentName)
                .ThenBy(c => c.ClassName)
                .ToListAsync();

            var classesWithGroups = classes.Select(c => new
            {
                ClassID = c.ClassID,
                ClassName = c.ClassName,
                Group = c.Department?.DepartmentName ?? "No Department"
            });

            ViewBag.Classes = new SelectList(classesWithGroups, "ClassID", "ClassName", null, "Group");
        }

        private async Task LoadDepartmentsAsync()
        {
            ViewBag.Departments = new SelectList(
                await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync(),
                "DepartmentID",
                "DepartmentName"
            );
        }

        public async Task<IActionResult> Create()
        {
            await LoadClassesAsync();
            await LoadDepartmentsAsync();
            return View(new CreateStudentWithAccountViewModel { StudentID = Guid.NewGuid().ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateStudentWithAccountViewModel model)
        {
            // Debug: Log initial model state
            var modelStateErrors = string.Join(", ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            
            if (!string.IsNullOrEmpty(modelStateErrors))
            {
                TempData["Debug"] = $"ModelState Errors: {modelStateErrors}";
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Debug: Log student creation attempt
                    TempData["Debug"] = TempData["Debug"] + "\nAttempting to create student with: " +
                        $"\nCode: {model.StudentCode}" +
                        $"\nName: {model.StudentName}" +
                        $"\nEmail: {model.StudentEmail}" +
                        $"\nClassID: {model.ClassID}" +
                        $"\nDepartmentID: {model.DepartmentID}" +
                        $"\nCreateAccount: {model.CreateUserAccount}";

                    // Check if StudentCode already exists
                    if (await _context.Students.AnyAsync(s => s.StudentCode == model.StudentCode))
                    {
                        ModelState.AddModelError("StudentCode", "This Student Code is already in use.");
                        TempData["Debug"] = TempData["Debug"] + "\nError: Student code already exists";
                        await LoadClassesAsync();
                        await LoadDepartmentsAsync();
                        return View(model);
                    }

                    // Check if email already exists in students
                    if (await _context.Students.AnyAsync(s => s.StudentEmail == model.StudentEmail))
                    {
                        ModelState.AddModelError("StudentEmail", "This email is already registered for a student.");
                        TempData["Debug"] = TempData["Debug"] + "\nError: Student email already exists";
                        await LoadClassesAsync();
                        await LoadDepartmentsAsync();
                        return View(model);
                    }

                    // Check if email already exists in users (if creating account)
                    if (model.CreateUserAccount)
                    {
                        var existingUser = await _userManager.FindByEmailAsync(model.StudentEmail);
                        if (existingUser != null)
                        {
                            ModelState.AddModelError("StudentEmail", "This email is already registered as a user account.");
                            TempData["Debug"] = TempData["Debug"] + "\nError: Email already exists as user account";
                            await LoadClassesAsync();
                            await LoadDepartmentsAsync();
                            return View(model);
                        }
                    }

                    // Check if class exists
                    var classExists = await _context.Classes.AnyAsync(c => c.ClassID == model.ClassID);
                    if (!classExists)
                    {
                        ModelState.AddModelError("ClassID", "Selected class does not exist.");
                        TempData["Debug"] = TempData["Debug"] + "\nError: Selected class does not exist";
                        await LoadClassesAsync();
                        await LoadDepartmentsAsync();
                        return View(model);
                    }

                    // Check if department exists
                    var departmentExists = await _context.Departments.AnyAsync(d => d.DepartmentID == model.DepartmentID);
                    if (!departmentExists)
                    {
                        ModelState.AddModelError("DepartmentID", "Selected department does not exist.");
                        TempData["Debug"] = TempData["Debug"] + "\nError: Selected department does not exist";
                        await LoadClassesAsync();
                        await LoadDepartmentsAsync();
                        return View(model);
                    }

                    // Create the student
                    var student = new StudentModel
                    {
                        StudentID = string.IsNullOrEmpty(model.StudentID) ? Guid.NewGuid().ToString() : model.StudentID,
                        StudentCode = model.StudentCode,
                        StudentName = model.StudentName,
                        StudentSex = model.StudentSex,
                        StudentEmail = model.StudentEmail,
                        ClassID = model.ClassID,
                        DepartmentID = model.DepartmentID
                    };

                    _context.Add(student);

                    // Create user account if requested
                    ApplicationUser? createdUser = null;
                    if (model.CreateUserAccount)
                    {
                        try
                        {
                            // Ensure User role exists
                            if (!await _roleManager.RoleExistsAsync(SD.Role_User))
                            {
                                var roleResult = await _roleManager.CreateAsync(new IdentityRole(SD.Role_User));
                                if (!roleResult.Succeeded)
                                {
                                    TempData["Debug"] = TempData["Debug"] + $"\nError creating role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}";
                                }
                            }

                            var user = new ApplicationUser
                            {
                                UserName = model.StudentEmail,
                                Email = model.StudentEmail,
                                FullName = model.StudentName,
                                EmailConfirmed = true // Auto-confirm email for admin-created accounts
                            };

                            var result = await _userManager.CreateAsync(user, model.Password);
                            if (result.Succeeded)
                            {
                                var roleResult = await _userManager.AddToRoleAsync(user, SD.Role_User);
                                if (!roleResult.Succeeded)
                                {
                                    TempData["Debug"] = TempData["Debug"] + $"\nError adding user to role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}";
                                }
                                createdUser = user;
                                
                                // Link student to user account
                                student.UserId = user.Id;
                            }
                            else
                            {
                                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                                ModelState.AddModelError("", $"Failed to create user account: {errors}");
                                TempData["Debug"] = TempData["Debug"] + $"\nError creating user: {errors}";
                                await LoadClassesAsync();
                                await LoadDepartmentsAsync();
                                return View(model);
                            }
                        }
                        catch (Exception ex)
                        {
                            TempData["Debug"] = TempData["Debug"] + $"\nError in user account creation: {ex.Message}";
                            throw;
                        }
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                        TempData["Debug"] = TempData["Debug"] + "\nSuccessfully saved student to database";
                    }
                    catch (Exception ex)
                    {
                        TempData["Debug"] = TempData["Debug"] + $"\nError saving to database: {ex.Message}";
                        throw;
                    }

                    var successMessage = model.CreateUserAccount && createdUser != null 
                        ? "Student and user account created successfully!" 
                        : "Student created successfully!";
                    
                    TempData["SuccessMessage"] = successMessage;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while creating the student. Please try again.");
                    TempData["Debug"] = TempData["Debug"] + $"\nUnhandled error: {ex.Message}\nStack trace: {ex.StackTrace}";
                    await LoadClassesAsync();
                    await LoadDepartmentsAsync();
                    return View(model);
                }
            }
            await LoadClassesAsync();
            await LoadDepartmentsAsync();
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Class)
                .Include(s => s.Department)
                .FirstOrDefaultAsync(m => m.StudentID == id);

            if (student == null)
            {
                return NotFound();
            }

            await LoadClassesAsync();
            await LoadDepartmentsAsync();
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, StudentModel student)
        {
            if (id != student.StudentID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if the student exists
                    var existingStudent = await _context.Students
                        .Include(s => s.Class)
                        .Include(s => s.Department)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.StudentID == id);

                    if (existingStudent == null)
                    {
                        return NotFound();
                    }

                    // Check if the student code is unique (excluding current student)
                    if (await _context.Students
                        .AnyAsync(s => s.StudentCode == student.StudentCode && s.StudentID != student.StudentID))
                    {
                        ModelState.AddModelError("StudentCode", "This Student Code is already in use.");
                        await LoadClassesAsync();
                        await LoadDepartmentsAsync();
                        return View(student);
                    }

                    // Check if the email is unique (excluding current student)
                    if (await _context.Students
                        .AnyAsync(s => s.StudentEmail == student.StudentEmail && s.StudentID != student.StudentID))
                    {
                        ModelState.AddModelError("StudentEmail", "This email is already registered for another student.");
                        await LoadClassesAsync();
                        await LoadDepartmentsAsync();
                        return View(student);
                    }

                    // Check if class exists
                    if (!await _context.Classes.AnyAsync(c => c.ClassID == student.ClassID))
                    {
                        ModelState.AddModelError("ClassID", "Selected class does not exist.");
                        await LoadClassesAsync();
                        await LoadDepartmentsAsync();
                        return View(student);
                    }

                    // Check if department exists
                    if (!await _context.Departments.AnyAsync(d => d.DepartmentID == student.DepartmentID))
                    {
                        ModelState.AddModelError("DepartmentID", "Selected department does not exist.");
                        await LoadClassesAsync();
                        await LoadDepartmentsAsync();
                        return View(student);
                    }

                    _context.Update(student);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Student updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.StudentID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            await LoadClassesAsync();
            await LoadDepartmentsAsync();
            return View(student);
        }

        public async Task<IActionResult> Delete(string id)
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var student = await _context.Students
                .Include(s => s.Grades)
                .FirstOrDefaultAsync(s => s.StudentID == id);

            if (student == null)
            {
                return NotFound();
            }

            try
            {
                // Check if student has any grades
                if (student.Grades != null && student.Grades.Any())
                {
                    TempData["Error"] = "Không thể xóa sinh viên này vì sinh viên đang có điểm trong hệ thống.";
                    return RedirectToAction(nameof(Index));
                }

                // If student has an associated user account, delete it first
                if (!string.IsNullOrEmpty(student.UserId))
                {
                    var user = await _userManager.FindByIdAsync(student.UserId);
                    if (user != null)
                    {
                        var result = await _userManager.DeleteAsync(user);
                        if (!result.Succeeded)
                        {
                            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                            TempData["Error"] = $"Lỗi khi xóa tài khoản người dùng: {errors}";
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }

                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa sinh viên và tài khoản thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi xóa sinh viên: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool StudentExists(string id)
        {
            return _context.Students.Any(e => e.StudentID == id);
        }
    }
} 
