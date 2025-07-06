using grade_management.Models;

namespace grade_management.Repositories
{
    public interface IGradeRepository : IRepository<GradeModel>
    {
        Task<IEnumerable<GradeModel>> GetGradesByStudentAsync(string studentId);
        Task<IEnumerable<GradeModel>> GetGradesBySubjectAsync(int subjectId);
        Task<GradeModel?> GetGradeWithDetailsAsync(int gradeId);
        Task<GradeModel?> GetGradeByStudentAndSubjectAsync(string studentId, int subjectId);
        Task<double> GetAverageGradeByStudentAsync(string studentId);
        Task<double> GetAverageGradeBySubjectAsync(int subjectId);
    }
} 