using grade_management.Models;

namespace grade_management.Repositories
{
    public interface ISubjectRepository : IRepository<SubjectModel>
    {
        Task<SubjectModel?> GetSubjectWithDetailsAsync(int subjectId);
        Task<IEnumerable<SubjectModel>> GetSubjectsByStatusAsync(string status);
        Task<IEnumerable<SubjectModel>> GetSubjectsByTypeAsync(string type);
        Task<bool> IsSubjectCodeExistsAsync(string subjectCode);
        Task<bool> IsSubjectCodeInUseAsync(string subjectCode, int? excludeSubjectId = null);
    }
} 