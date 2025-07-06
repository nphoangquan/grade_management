using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using grade_management.Models;
using grade_management.Repositories;

namespace grade_management.Controllers
{
    public class ClassesController : Controller
    {
        private readonly IClassRepository _classRepository;

        public ClassesController(IClassRepository classRepository)
        {
            _classRepository = classRepository;
        }

        // GET: Classes
        public async Task<IActionResult> Index()
        {
            var classes = await _classRepository.GetAllAsync();
            return View(classes);
        }

        // GET: Classes/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classModel = await _classRepository.GetByIdAsync(id);
            if (classModel == null)
            {
                return NotFound();
            }

            return View(classModel);
        }

        // GET: Classes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Classes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClassID,ClassName")] ClassModel classModel)
        {
            if (ModelState.IsValid)
            {
                await _classRepository.AddAsync(classModel);
                return RedirectToAction(nameof(Index));
            }
            return View(classModel);
        }

        // GET: Classes/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classModel = await _classRepository.GetByIdAsync(id);
            if (classModel == null)
            {
                return NotFound();
            }
            return View(classModel);
        }

        // POST: Classes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ClassID,ClassName")] ClassModel classModel)
        {
            if (id != classModel.ClassID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _classRepository.UpdateAsync(classModel);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ClassExists(classModel.ClassID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(classModel);
        }

        // GET: Classes/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classModel = await _classRepository.GetByIdAsync(id);
            if (classModel == null)
            {
                return NotFound();
            }

            return View(classModel);
        }

        // POST: Classes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _classRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Classes/Students/5
        public async Task<IActionResult> Students(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classModel = await _classRepository.GetByIdAsync(id.ToString());

            if (classModel == null)
            {
                return NotFound();
            }

            return View(classModel);
        }

        // GET: Classes/Teachers/5
        public async Task<IActionResult> Teachers(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classModel = await _classRepository.GetByIdAsync(id.ToString());

            if (classModel == null)
            {
                return NotFound();
            }

            return View(classModel);
        }

        // GET: Classes/Subjects/5
        public async Task<IActionResult> Subjects(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classModel = await _classRepository.GetByIdAsync(id.ToString());

            if (classModel == null)
            {
                return NotFound();
            }

            return View(classModel);
        }

        private async Task<bool> ClassExists(string id)
        {
            return await _classRepository.IsClassIdExistsAsync(id);
        }
    }
} 