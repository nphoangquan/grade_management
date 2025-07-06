using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using grade_management.Models;
using grade_management.Data;
using Microsoft.EntityFrameworkCore;

namespace grade_management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ClassManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClassManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var classesQuery = _context.Classes
                .Include(c => c.Students)
                .Include(c => c.Department)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                classesQuery = classesQuery.Where(c => 
                    c.ClassName.Contains(searchString) ||
                    c.Department.DepartmentName.Contains(searchString));
            }

            var classes = await classesQuery
                .OrderBy(c => c.Department.DepartmentName)
                .ThenBy(c => c.ClassName)
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(classes);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classModel = await _context.Classes
                .Include(c => c.Students)
                .Include(c => c.Department)
                .FirstOrDefaultAsync(m => m.ClassID == id);

            if (classModel == null)
            {
                return NotFound();
            }

            return View(classModel);
        }

        public async Task<IActionResult> Create()
        {
            await LoadDepartmentsAsync();
            return View(new ClassModel { ClassID = Guid.NewGuid().ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassModel classModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(classModel.ClassID))
                    {
                        classModel.ClassID = Guid.NewGuid().ToString();
                    }

                    // Check if class name already exists in the same department
                    if (await _context.Classes.AnyAsync(c => 
                        c.ClassName == classModel.ClassName && 
                        c.DepartmentID == classModel.DepartmentID))
                    {
                        ModelState.AddModelError("ClassName", "A class with this name already exists in this department.");
                        await LoadDepartmentsAsync();
                        return View(classModel);
                    }

                    _context.Add(classModel);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Class created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while creating the class. Please try again.");
            }

            await LoadDepartmentsAsync();
            return View(classModel);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classModel = await _context.Classes
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.ClassID == id);
            if (classModel == null)
            {
                return NotFound();
            }

            await LoadDepartmentsAsync();
            return View(classModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ClassModel classModel)
        {
            if (id != classModel.ClassID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if class name already exists for other classes in the same department
                    if (await _context.Classes.AnyAsync(c => 
                        c.ClassName == classModel.ClassName && 
                        c.DepartmentID == classModel.DepartmentID && 
                        c.ClassID != classModel.ClassID))
                    {
                        ModelState.AddModelError("ClassName", "A class with this name already exists in this department.");
                        await LoadDepartmentsAsync();
                        return View(classModel);
                    }

                    _context.Update(classModel);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Class updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassExists(classModel.ClassID))
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
            return View(classModel);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classModel = await _context.Classes
                .Include(c => c.Students)
                .Include(c => c.Department)
                .FirstOrDefaultAsync(m => m.ClassID == id);
            if (classModel == null)
            {
                return NotFound();
            }

            return View(classModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var classModel = await _context.Classes
                .Include(c => c.Students)
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.ClassID == id);

            if (classModel == null)
            {
                return NotFound();
            }

            try
            {
                if (classModel.Students?.Any() == true)
                {
                    TempData["Error"] = "Cannot delete class. Please remove all students from this class first.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                _context.Classes.Remove(classModel);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Class deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while deleting the class. Please try again.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private bool ClassExists(string id)
        {
            return _context.Classes.Any(e => e.ClassID == id);
        }

        private async Task LoadDepartmentsAsync()
        {
            ViewBag.Departments = new SelectList(
                await _context.Departments.OrderBy(d => d.DepartmentName).ToListAsync(),
                "DepartmentID",
                "DepartmentName"
            );
        }
    }
}