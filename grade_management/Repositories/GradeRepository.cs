using Microsoft.EntityFrameworkCore;
using grade_management.Data;
using grade_management.Models;

namespace grade_management.Repositories
{
    public class GradeRepository : Repository<GradeModel>, IGradeRepository
    {
        public GradeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<GradeModel>> GetGradesByStudentAsync(string studentId)
        {
            return await _dbSet
                .Where(g => g.StudentID == studentId)
                .Include(g => g.Student)
                .Include(g => g.Subject)
                .ToListAsync();
        }

        public async Task<IEnumerable<GradeModel>> GetGradesBySubjectAsync(int subjectId)
        {
            return await _dbSet
                .Where(g => g.SubjectID == subjectId)
                .Include(g => g.Student)
                .Include(g => g.Subject)
                .ToListAsync();
        }

        public async Task<GradeModel?> GetGradeWithDetailsAsync(int gradeId)
        {
            return await _dbSet
                .Where(g => g.GradeID == gradeId)
                .Include(g => g.Student)
                .Include(g => g.Subject)
                .FirstOrDefaultAsync();
        }

        public async Task<GradeModel?> GetGradeByStudentAndSubjectAsync(string studentId, int subjectId)
        {
            return await _dbSet
                .Where(g => g.StudentID == studentId && g.SubjectID == subjectId)
                .Include(g => g.Student)
                .Include(g => g.Subject)
                .FirstOrDefaultAsync();
        }

        public async Task<double> GetAverageGradeByStudentAsync(string studentId)
        {
            var grades = await _dbSet
                .Where(g => g.StudentID == studentId)
                .ToListAsync();

            if (!grades.Any())
                return 0;

            return grades.Average(g => g.TenGradeScale);
        }

        public async Task<double> GetAverageGradeBySubjectAsync(int subjectId)
        {
            var grades = await _dbSet
                .Where(g => g.SubjectID == subjectId)
                .ToListAsync();

            if (!grades.Any())
                return 0;

            return grades.Average(g => g.TenGradeScale);
        }

        public override async Task<IEnumerable<GradeModel>> GetAllAsync()
        {
            return await _dbSet
                .Include(g => g.Student)
                .Include(g => g.Subject)
                .ToListAsync();
        }

        public override async Task<GradeModel?> GetByIdAsync(object id)
        {
            if (id is int gradeId)
            {
                return await _dbSet
                    .Where(g => g.GradeID == gradeId)
                    .Include(g => g.Student)
                    .Include(g => g.Subject)
                    .FirstOrDefaultAsync();
            }
            return null;
        }
    }
}