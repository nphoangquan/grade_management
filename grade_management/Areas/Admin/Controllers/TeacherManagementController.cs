using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using grade_management.Models;
using grade_management.Models.ViewModels;
using grade_management.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace grade_management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class TeacherManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TeacherManagementController(
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
            var teachersQuery = _context.Teachers
                .Include(t => t.Department)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                teachersQuery = teachersQuery.Where(t => 
                    t.TeacherName.Contains(searchString) || 
                    t.TeacherEmail.Contains(searchString) ||
                    t.TeacherCode.Contains(searchString) ||
                    t.Department.DepartmentName.Contains(searchString));
            }

            var teachers = await teachersQuery
                .OrderBy(t => t.Department.DepartmentName)
                .ThenBy(t => t.TeacherName)
                .ToListAsync();

            // Pass the search string back to the view to maintain the search box value
            ViewData["CurrentFilter"] = searchString;

            return View(teachers);
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
            await LoadDepartmentsAsync();
            return View(new CreateTeacherWithAccountViewModel { TeacherID = Guid.NewGuid().ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTeacherWithAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if teacher code already exists in the same department
                    if (await _context.Teachers.AnyAsync(t => 
                        t.TeacherCode == model.TeacherCode && 
                        t.DepartmentID == model.DepartmentID))
                    {
                        ModelState.AddModelError("TeacherCode", "This Teacher Code is already in use in the selected department.");
                        await LoadDepartmentsAsync();
                        return View(model);
                    }

                    // Check if email already exists in teachers
                    if (await _context.Teachers.AnyAsync(t => t.TeacherEmail == model.TeacherEmail))
                    {
                        ModelState.AddModelError("TeacherEmail", "This email is already registered for a teacher.");
                        await LoadDepartmentsAsync();
                        return View(model);
                    }

                    // Check if email already exists in users (if creating account)
                    if (model.CreateUserAccount)
                    {
                        var existingUser = await _userManager.FindByEmailAsync(model.TeacherEmail);
                        if (existingUser != null)
                        {
                            ModelState.AddModelError("TeacherEmail", "This email is already registered as a user account.");
                            await LoadDepartmentsAsync();
                            return View(model);
                        }
                    }

                    // Check if department exists
                    var departmentExists = await _context.Departments.AnyAsync(d => d.DepartmentID == model.DepartmentID);
                    if (!departmentExists)
                    {
                        ModelState.AddModelError("DepartmentID", "Selected department does not exist.");
                        await LoadDepartmentsAsync();
                        return View(model);
                    }

                    // Create the teacher
                    var teacher = new TeacherModel
                    {
                        TeacherID = string.IsNullOrEmpty(model.TeacherID) ? Guid.NewGuid().ToString() : model.TeacherID,
                        TeacherName = model.TeacherName,
                        TeacherSex = model.TeacherSex,
                        TeacherEmail = model.TeacherEmail,
                        TeacherCode = model.TeacherCode,
                        DepartmentID = model.DepartmentID
                    };

                    _context.Add(teacher);

                    // Create user account if requested
                    ApplicationUser? createdUser = null;
                    if (model.CreateUserAccount)
                    {
                        // Ensure Moderator role exists
                        if (!await _roleManager.RoleExistsAsync(SD.Role_Moderator))
                        {
                            await _roleManager.CreateAsync(new IdentityRole(SD.Role_Moderator));
                        }

                        var user = new ApplicationUser
                        {
                            UserName = model.TeacherEmail,
                            Email = model.TeacherEmail,
                            FullName = model.TeacherName,
                            EmailConfirmed = true // Auto-confirm email for admin-created accounts
                        };

                        var result = await _userManager.CreateAsync(user, model.Password);
                        if (result.Succeeded)
                        {
                            await _userManager.AddToRoleAsync(user, SD.Role_Moderator);
                            createdUser = user;
                            
                            // Link teacher to user account
                            teacher.UserId = user.Id;
                        }
                        else
                        {
                            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                            ModelState.AddModelError("", $"Failed to create user account: {errors}");
                            await LoadDepartmentsAsync();
                            return View(model);
                        }
                    }

                    await _context.SaveChangesAsync();

                    var successMessage = model.CreateUserAccount && createdUser != null 
                        ? "Teacher and moderator account created successfully!" 
                        : "Teacher created successfully!";
                    
                    TempData["Success"] = successMessage;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while creating the teacher. Please try again.");
                    await LoadDepartmentsAsync();
                    return View(model);
                }
            }
            await LoadDepartmentsAsync();
            return View(model);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .Include(t => t.Department)
                .FirstOrDefaultAsync(m => m.TeacherID == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .Include(t => t.Department)
                .FirstOrDefaultAsync(m => m.TeacherID == id);
            if (teacher == null)
            {
                return NotFound();
            }

            await LoadDepartmentsAsync();
            return View(teacher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, TeacherModel teacher)
        {
            if (id != teacher.TeacherID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check for duplicate teacher code in the same department, excluding current teacher
                    if (await _context.Teachers.AnyAsync(t => 
                        t.TeacherCode == teacher.TeacherCode && 
                        t.DepartmentID == teacher.DepartmentID && 
                        t.TeacherID != teacher.TeacherID))
                    {
                        ModelState.AddModelError("TeacherCode", "This Teacher Code is already in use in the selected department.");
                        await LoadDepartmentsAsync();
                        return View(teacher);
                    }

                    // Check for duplicate email, excluding current teacher
                    if (await _context.Teachers.AnyAsync(t => 
                        t.TeacherEmail == teacher.TeacherEmail && 
                        t.TeacherID != teacher.TeacherID))
                    {
                        ModelState.AddModelError("TeacherEmail", "This email address is already in use.");
                        await LoadDepartmentsAsync();
                        return View(teacher);
                    }

                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Teacher updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(teacher.TeacherID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            await LoadDepartmentsAsync();
            return View(teacher);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(m => m.TeacherID == id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }

            try
            {
                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Teacher deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Unable to delete teacher. Please try again, and if the problem persists, contact your system administrator.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool TeacherExists(string id)
        {
            return _context.Teachers.Any(e => e.TeacherID == id);
        }
    }
} 