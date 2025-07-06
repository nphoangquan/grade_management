using Microsoft.EntityFrameworkCore;
using grade_management.Data;
using grade_management.Models;

namespace grade_management.Repositories
{
    public class DepartmentRepository : Repository<DepartmentModel>, IDepartmentRepository
    {
        public DepartmentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<DepartmentModel?> GetDepartmentWithDetailsAsync(string departmentId)
        {
            return await _dbSet
                .Where(d => d.DepartmentID == departmentId)
                .Include(d => d.Students)
                .Include(d => d.Teachers)
                .Include(d => d.Classes)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsDepartmentCodeExistsAsync(string departmentCode)
        {
            return await _dbSet.AnyAsync(d => d.DepartmentCode == departmentCode);
        }

        public async Task<bool> IsDepartmentCodeInUseAsync(string departmentCode, string? excludeDepartmentId = null)
        {
            var query = _dbSet.Where(d => d.DepartmentCode == departmentCode);
            
            if (!string.IsNullOrEmpty(excludeDepartmentId))
            {
                query = query.Where(d => d.DepartmentID != excludeDepartmentId);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<DepartmentModel>> GetDepartmentsWithStudentsAsync()
        {
            return await _dbSet
                .Include(d => d.Students)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();
        }

        public async Task<IEnumerable<DepartmentModel>> GetDepartmentsWithTeachersAsync()
        {
            return await _dbSet
                .Include(d => d.Teachers)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();
        }

        public async Task<IEnumerable<DepartmentModel>> GetDepartmentsWithClassesAsync()
        {
            return await _dbSet
                .Include(d => d.Classes)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();
        }

        public override async Task<IEnumerable<DepartmentModel>> GetAllAsync()
        {
            return await _dbSet
                .Include(d => d.Students)
                .Include(d => d.Teachers)
                .Include(d => d.Classes)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();
        }

        public override async Task<DepartmentModel?> GetByIdAsync(object id)
        {
            if (id is string departmentId)
            {
                return await _dbSet
                    .Where(d => d.DepartmentID == departmentId)
                    .Include(d => d.Students)
                    .Include(d => d.Teachers)
                    .Include(d => d.Classes)
                    .FirstOrDefaultAsync();
            }
            return null;
        }
    }
} 