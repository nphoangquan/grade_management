using grade_management.Models;

namespace grade_management.Repositories
{
    public interface IClassRepository : IRepository<ClassModel>
    {
        Task<ClassModel?> GetClassWithStudentsAsync(string classId);
        Task<ClassModel?> GetClassWithDetailsAsync(string classId);
        Task<bool> IsClassIdExistsAsync(string classId);
        Task<bool> IsClassNameExistsAsync(string className);
    }
} 