using Microsoft.EntityFrameworkCore;
using grade_management.Data;
using grade_management.Models;

namespace grade_management.Repositories
{
    public class ClassRepository : Repository<ClassModel>, IClassRepository
    {
        public ClassRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ClassModel?> GetClassWithStudentsAsync(string classId)
        {
            return await _dbSet
                .Include(c => c.Students)
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.ClassID == classId);
        }

        public async Task<ClassModel?> GetClassWithDetailsAsync(string classId)
        {
            return await _dbSet
                .Where(c => c.ClassID == classId)
                .Include(c => c.Students)
                .Include(c => c.Department)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsClassIdExistsAsync(string classId)
        {
            return await _dbSet.AnyAsync(c => c.ClassID == classId);
        }

        public async Task<bool> IsClassNameExistsAsync(string className)
        {
            return await _dbSet.AnyAsync(c => c.ClassName == className);
        }

        public override async Task<IEnumerable<ClassModel>> GetAllAsync()
        {
            return await _dbSet
                .Include(c => c.Students)
                .Include(c => c.Department)
                .OrderBy(c => c.ClassName)
                .ToListAsync();
        }

        public override async Task<ClassModel?> GetByIdAsync(object id)
        {
            if (id is string classId)
            {
                return await _dbSet
                    .Where(c => c.ClassID == classId)
                    .Include(c => c.Students)
                    .Include(c => c.Department)
                    .FirstOrDefaultAsync();
            }
            return null;
        }
    }
} 