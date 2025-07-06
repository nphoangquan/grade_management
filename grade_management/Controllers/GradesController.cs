using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using grade_management.Models;
using grade_management.Data;
using grade_management.Repositories;

namespace grade_management.Controllers
{
    public class GradesController(IGradeRepository gradeRepository, 
                                IStudentRepository studentRepository, 
                                ISubjectRepository subjectRepository) : Controller
    {
        private readonly IGradeRepository _gradeRepository = gradeRepository;
        private readonly IStudentRepository _studentRepository = studentRepository;
        private readonly ISubjectRepository _subjectRepository = subjectRepository;

        // GET: Grades
        public async Task<IActionResult> Index(string searchString)
        {
            var students = await _studentRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => 
                    s.StudentName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    s.Class?.ClassName.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true
                );
            }

            return View(students);
        }

        // GET: Grades/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var grade = await _gradeRepository.GetGradeWithDetailsAsync(id);

            if (grade == null)
            {
                return NotFound();
            }

            return View(grade);
        }

        // GET: Grades/Create
        public async Task<IActionResult> Create()
        {
            var students = await _studentRepository.GetAllAsync();
            var subjects = await _subjectRepository.GetAllAsync();
            ViewBag.Students = new SelectList(students, "StudentID", "StudentName");
            ViewBag.Subjects = new SelectList(subjects, "SubjectID", "SubjectName");
            return View();
        }

        // POST: Grades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GradeID,AttendanceGrade,AddGrade,ExamGrade,StudentID,SubjectID")] GradeModel grade)
        {
            if (ModelState.IsValid)
            {
                await _gradeRepository.AddAsync(grade);
                return RedirectToAction(nameof(Index));
            }

            var students = await _studentRepository.GetAllAsync();
            var subjects = await _subjectRepository.GetAllAsync();
            ViewBag.Students = new SelectList(students, "StudentID", "StudentName", grade.StudentID);
            ViewBag.Subjects = new SelectList(subjects, "SubjectID", "SubjectName", grade.SubjectID);
            return View(grade);
        }

        // GET: Grades/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var grade = await _gradeRepository.GetGradeWithDetailsAsync(id);
                
            if (grade == null)
            {
                return NotFound();
            }

            var students = await _studentRepository.GetAllAsync();
            var subjects = await _subjectRepository.GetAllAsync();
            ViewBag.Students = new SelectList(students, "StudentID", "StudentName", grade.StudentID);
            ViewBag.Subjects = new SelectList(subjects, "SubjectID", "SubjectName", grade.SubjectID);
            return View(grade);
        }

        // POST: Grades/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GradeID,AttendanceGrade,AddGrade,ExamGrade,StudentID,SubjectID")] GradeModel grade)
        {
            if (id != grade.GradeID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _gradeRepository.UpdateAsync(grade);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _gradeRepository.ExistsAsync(grade.GradeID))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            var students = await _studentRepository.GetAllAsync();
            var subjects = await _subjectRepository.GetAllAsync();
            ViewBag.Students = new SelectList(students, "StudentID", "StudentName", grade.StudentID);
            ViewBag.Subjects = new SelectList(subjects, "SubjectID", "SubjectName", grade.SubjectID);
            return View(grade);
        }

        // GET: Grades/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var grade = await _gradeRepository.GetGradeWithDetailsAsync(id);

            if (grade == null)
            {
                return NotFound();
            }

            return View(grade);
        }

        // POST: Grades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _gradeRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // GET: Grades/StudentSubjects/5
        public async Task<IActionResult> StudentSubjects(string id)
        {
            if (string.IsNullOrEmpty(id))
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