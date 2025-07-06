using Microsoft.EntityFrameworkCore;
using grade_management.Data;
using grade_management.Models;

namespace grade_management.Repositories
{
    public class TeacherRepository : Repository<TeacherModel>, ITeacherRepository
    {
        public TeacherRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> IsTeacherIdExistsAsync(string teacherId)
        {
            return await _dbSet.AnyAsync(t => t.TeacherID == teacherId);
        }

        public async Task<bool> IsEmailInUseAsync(string email, string? excludeTeacherId = null)
        {
            var query = _dbSet.Where(t => t.TeacherEmail == email);
            
            if (!string.IsNullOrEmpty(excludeTeacherId))
            {
                query = query.Where(t => t.TeacherID != excludeTeacherId);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> IsTeacherCodeInUseAsync(string teacherCode, string? excludeTeacherId = null)
        {
            var query = _dbSet.Where(t => t.TeacherCode == teacherCode);
            
            if (!string.IsNullOrEmpty(excludeTeacherId))
            {
                query = query.Where(t => t.TeacherID != excludeTeacherId);
            }

            return await query.AnyAsync();
        }

        public override async Task<IEnumerable<TeacherModel>> GetAllAsync()
        {
            return await _dbSet
                .Include(t => t.Department)
                .OrderBy(t => t.TeacherName)
                .ToListAsync();
        }

        public override async Task<TeacherModel?> GetByIdAsync(object id)
        {
            if (id is string teacherId)
            {
                return await _dbSet
                    .Where(t => t.TeacherID == teacherId)
                    .Include(t => t.Department)
                    .FirstOrDefaultAsync();
            }
            return null;
        }
    }
} 