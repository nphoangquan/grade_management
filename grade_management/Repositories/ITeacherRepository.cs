using grade_management.Models;

namespace grade_management.Repositories
{
    public interface ITeacherRepository : IRepository<TeacherModel>
    {
        Task<bool> IsTeacherIdExistsAsync(string teacherId);
        Task<bool> IsEmailInUseAsync(string email, string? excludeTeacherId = null);
        Task<bool> IsTeacherCodeInUseAsync(string teacherCode, string? excludeTeacherId = null);
    }
} 