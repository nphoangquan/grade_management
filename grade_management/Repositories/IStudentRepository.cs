using grade_management.Models;

namespace grade_management.Repositories
{
    public interface IStudentRepository : IRepository<StudentModel>
    {
        Task<IEnumerable<StudentModel>> GetStudentsByClassAsync(string classId);
        Task<StudentModel?> GetStudentWithClassAsync(string studentId);
        Task<IEnumerable<StudentModel>> GetStudentsBySubjectAsync(int subjectId);
        Task<bool> IsStudentIdExistsAsync(string studentId);
    }
} 