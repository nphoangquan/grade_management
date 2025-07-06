using Microsoft.EntityFrameworkCore;
using grade_management.Data;
using grade_management.Models;

namespace grade_management.Repositories
{
    public class StudentRepository : Repository<StudentModel>, IStudentRepository
    {
        public StudentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<StudentModel>> GetStudentsByClassAsync(string classId)
        {
            return await _dbSet
                .Where(s => s.ClassID == classId)
                .Include(s => s.Class)
                .Include(s => s.Department)
                .ToListAsync();
        }

        public async Task<StudentModel?> GetStudentWithClassAsync(string studentId)
        {
            return await _dbSet
                .Where(s => s.StudentID == studentId)
                .Include(s => s.Class)
                .Include(s => s.Department)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<StudentModel>> GetStudentsBySubjectAsync(int subjectId)
        {
            return await _dbSet
                .Where(s => s.Grades.Any(g => g.SubjectID == subjectId))
                .Include(s => s.Class)
                .Include(s => s.Department)
                .Include(s => s.Grades.Where(g => g.SubjectID == subjectId))
                .ToListAsync();
        }

        public async Task<bool> IsStudentIdExistsAsync(string studentId)
        {
            return await _dbSet.AnyAsync(s => s.StudentID == studentId);
        }

        public override async Task<IEnumerable<StudentModel>> GetAllAsync()
        {
            return await _dbSet
                .Include(s => s.Class)
                .Include(s => s.Department)
                .Include(s => s.Grades)
                .ToListAsync();
        }

        public override async Task<StudentModel?> GetByIdAsync(object id)
        {
            if (id is string studentId)
            {
                return await _dbSet
                    .Where(s => s.StudentID == studentId)
                    .Include(s => s.Class)
                    .Include(s => s.Department)
                    .Include(s => s.Grades)
                    .FirstOrDefaultAsync();
            }
            return null;
        }
    }
} 