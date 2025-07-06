using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using grade_management.Data;
using grade_management.Models;
using System.Linq;
using System.Threading.Tasks;

namespace grade_management.Areas.Admin.Controllers
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
    }

    public class EditUserRoleViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<RoleSelection> Roles { get; set; }
    }

    public class RoleSelection
    {
        public string RoleName { get; set; }
        public bool IsSelected { get; set; }
    }

    public class DeleteUserViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
    }

    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.FullName ?? user.UserName,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            return View(userViewModels);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();

            var model = new EditUserRoleViewModel
            {
                UserId = user.Id,
                UserName = user.FullName ?? user.UserName,
                Email = user.Email,
                Roles = allRoles.Select(r => new RoleSelection
                {
                    RoleName = r.Name,
                    IsSelected = userRoles.Contains(r.Name)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("UserId,UserName,Email")] EditUserRoleViewModel model, string SelectedRole)
        {
            if (id != model.UserId)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            try
            {
                // Remove all existing roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                // Add the selected role
                if (!string.IsNullOrEmpty(SelectedRole))
                {
                    await _userManager.AddToRoleAsync(user, SelectedRole);
                    TempData["Success"] = "User role updated successfully!";
                }
                else
                {
                    TempData["Error"] = "Please select a role.";
                    return RedirectToAction(nameof(Edit), new { id = user.Id });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating user role: " + ex.Message;
                
                // Reload the roles for the view
                var allRoles = await _roleManager.Roles.ToListAsync();
                var userRoles = await _userManager.GetRolesAsync(user);
                model.Roles = allRoles.Select(r => new RoleSelection
                {
                    RoleName = r.Name,
                    IsSelected = userRoles.Contains(r.Name)
                }).ToList();
                
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if user is an admin
            if (await _userManager.IsInRoleAsync(user, SD.Role_Admin))
            {
                TempData["Error"] = "Cannot delete admin accounts.";
                return RedirectToAction(nameof(Index));
            }

            // Get current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                TempData["Error"] = "Cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            // Check for associated records
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == id);
            if (teacher != null)
            {
                TempData["Error"] = $"Please delete the teacher record (ID: {teacher.TeacherID}) from Teacher Management first.";
                return RedirectToAction(nameof(Index));
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == id);
            if (student != null)
            {
                var hasGrades = await _context.Grades.AnyAsync(g => g.StudentID == student.StudentID);
                if (hasGrades)
                {
                    TempData["Error"] = $"Please delete all grades and the student record (ID: {student.StudentID}) from Student Management first.";
                }
                else
                {
                    TempData["Error"] = $"Please delete the student record (ID: {student.StudentID}) from Student Management first.";
                }
                return RedirectToAction(nameof(Index));
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var model = new DeleteUserViewModel
            {
                UserId = user.Id,
                UserName = user.FullName ?? user.UserName,
                Email = user.Email,
                Roles = userRoles.ToList()
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if user is an admin
            if (await _userManager.IsInRoleAsync(user, SD.Role_Admin))
            {
                TempData["Error"] = "Cannot delete admin accounts.";
                return RedirectToAction(nameof(Index));
            }

            // Get current user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                TempData["Error"] = "Cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            // Double check for associated records
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == id);
            if (teacher != null)
            {
                TempData["Error"] = $"Please delete the teacher record (ID: {teacher.TeacherID}) from Teacher Management first.";
                return RedirectToAction(nameof(Index));
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == id);
            if (student != null)
            {
                var hasGrades = await _context.Grades.AnyAsync(g => g.StudentID == student.StudentID);
                if (hasGrades)
                {
                    TempData["Error"] = $"Please delete all grades and the student record (ID: {student.StudentID}) from Student Management first.";
                }
                else
                {
                    TempData["Error"] = $"Please delete the student record (ID: {student.StudentID}) from Student Management first.";
                }
                return RedirectToAction(nameof(Index));
            }

            // If no associated records found, proceed with user account deletion
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "User account deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Error deleting user account: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }
    }
} 