using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using grade_management.Data;
using grade_management.Models;
using grade_management.Repositories;

namespace grade_management.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class DepartmentManagementController : Controller
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly ApplicationDbContext _context;

        public DepartmentManagementController(
            IDepartmentRepository departmentRepository,
            ApplicationDbContext context)
        {
            _departmentRepository = departmentRepository;
            _context = context;
        }

        // GET: Admin/DepartmentManagement
        public async Task<IActionResult> Index(string searchString)
        {
            var departmentsQuery = _context.Departments
                .Include(d => d.Students)
                .Include(d => d.Teachers)
                .Include(d => d.Classes)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                departmentsQuery = departmentsQuery.Where(d => 
                    d.DepartmentCode.Contains(searchString) || 
                    d.DepartmentName.Contains(searchString));
            }

            var departments = await departmentsQuery
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(departments);
        }

        // GET: Admin/DepartmentManagement/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var department = await _departmentRepository.GetDepartmentWithDetailsAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // GET: Admin/DepartmentManagement/Create
        public IActionResult Create()
        {
            return View(new DepartmentModel { DepartmentID = Guid.NewGuid().ToString() });
        }

        // POST: Admin/DepartmentManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentModel department)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if department code already exists
                    if (await _departmentRepository.IsDepartmentCodeExistsAsync(department.DepartmentCode))
                    {
                        ModelState.AddModelError("DepartmentCode", "This department code is already in use.");
                        return View(department);
                    }

                    await _departmentRepository.AddAsync(department);
                    TempData["Success"] = "Department created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An error occurred while creating the department. Please try again.");
                }
            }
            return View(department);
        }

        // GET: Admin/DepartmentManagement/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        // POST: Admin/DepartmentManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, DepartmentModel department)
        {
            if (id != department.DepartmentID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if department code is already in use by another department
                    if (await _departmentRepository.IsDepartmentCodeInUseAsync(department.DepartmentCode, department.DepartmentID))
                    {
                        ModelState.AddModelError("DepartmentCode", "This department code is already in use by another department.");
                        return View(department);
                    }

                    await _departmentRepository.UpdateAsync(department);
                    TempData["Success"] = "Department updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await DepartmentExists(department.DepartmentID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(department);
        }

        // GET: Admin/DepartmentManagement/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var department = await _departmentRepository.GetDepartmentWithDetailsAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }

        // POST: Admin/DepartmentManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            try
            {
                await _departmentRepository.DeleteAsync(department);
                TempData["Success"] = "Department deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["Error"] = "Cannot delete this department because it has associated records.";
                return RedirectToAction(nameof(Delete), new { id = id });
            }
        }

        private async Task<bool> DepartmentExists(string id)
        {
            return await _departmentRepository.ExistsAsync(id);
        }
    }
} 