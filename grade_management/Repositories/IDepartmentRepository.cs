using grade_management.Models;

namespace grade_management.Repositories
{
    public interface IDepartmentRepository : IRepository<DepartmentModel>
    {
        Task<DepartmentModel?> GetDepartmentWithDetailsAsync(string departmentId);
        Task<bool> IsDepartmentCodeExistsAsync(string departmentCode);
        Task<bool> IsDepartmentCodeInUseAsync(string departmentCode, string? excludeDepartmentId = null);
        Task<IEnumerable<DepartmentModel>> GetDepartmentsWithStudentsAsync();
        Task<IEnumerable<DepartmentModel>> GetDepartmentsWithTeachersAsync();
        Task<IEnumerable<DepartmentModel>> GetDepartmentsWithClassesAsync();
    }
} 