using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using grade_management.Models;
using grade_management.Data;
using grade_management.Repositories;
using System.Diagnostics;

namespace grade_management.Controllers
{
    public class StudentsController : Controller
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IClassRepository _classRepository;

        public StudentsController(IStudentRepository studentRepository, IClassRepository classRepository)
        {
            _studentRepository = studentRepository;
            _classRepository = classRepository;
        }

        // GET: Students
        public async Task<IActionResult> Index()
        {
            var students = await _studentRepository.GetAllAsync();
            return View(students);
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var classes = await _classRepository.GetAllAsync();
                Debug.WriteLine($"Found {classes.Count()} classes");
                
                foreach (var c in classes)
                {
                    Debug.WriteLine($"Class: ID={c.ClassID}, Name={c.ClassName}");
                }

                if (!classes.Any())
                {
                    TempData["Error"] = "No classes available. Please create a class first.";
                    return RedirectToAction("Create", "Classes");
                }

                ViewBag.Classes = new SelectList(classes, "ClassID", "ClassName");
                return View();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Create GET: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StudentID,StudentName,StudentSex,StudentEmail,ClassID")] StudentModel student)
        {
            try
            {
                Debug.WriteLine($"Received student data: Name={student.StudentName}, ClassID={student.ClassID}");
                
                if (ModelState.IsValid)
                {
                    Debug.WriteLine("ModelState is valid");
                    
                    // Verify if the selected class exists
                    var classExists = await _classRepository.ExistsAsync(student.ClassID);
                    Debug.WriteLine($"Class exists check: {classExists}");
                    
                    if (!classExists)
                    {
                        Debug.WriteLine($"Selected class {student.ClassID} does not exist");
                        ModelState.AddModelError("ClassID", "Selected class does not exist.");
                    }
                    else
                    {
                        await _studentRepository.AddAsync(student);
                        TempData["Success"] = "Student created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    Debug.WriteLine("ModelState is invalid. Errors:");
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            Debug.WriteLine($"Error: {error.ErrorMessage}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in Create POST: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                ModelState.AddModelError("", "An error occurred while creating the student. Please try again.");
            }

            // If we got this far, something failed, redisplay form
            var classes = await _classRepository.GetAllAsync();
            ViewBag.Classes = new SelectList(classes, "ClassID", "ClassName", student.ClassID);
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            var classes = await _classRepository.GetAllAsync();
            ViewBag.Classes = new SelectList(classes, "ClassID", "ClassName", student.ClassID);
            return View(student);
        }

        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("StudentID,StudentName,StudentSex,StudentEmail,ClassID")] StudentModel student)
        {
            if (id != student.StudentID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _studentRepository.UpdateAsync(student);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _studentRepository.ExistsAsync(student.StudentID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            var classes = await _classRepository.GetAllAsync();
            ViewBag.Classes = new SelectList(classes, "ClassID", "ClassName", student.ClassID);
            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _studentRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Students/Grades/5
        public async Task<IActionResult> Grades(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }
    }
} 