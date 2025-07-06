using Microsoft.EntityFrameworkCore;
using grade_management.Data;
using grade_management.Models;

namespace grade_management.Repositories
{
    public class SubjectRepository : Repository<SubjectModel>, ISubjectRepository
    {
        public SubjectRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<SubjectModel?> GetSubjectWithDetailsAsync(int subjectId)
        {
            return await _dbSet
                .Where(s => s.SubjectID == subjectId)
                .Include(s => s.Grades)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<SubjectModel>> GetSubjectsByStatusAsync(string status)
        {
            return await _dbSet
                .Where(s => s.SubjectStatus == status)
                .OrderBy(s => s.SubjectName)
                .ToListAsync();
        }

        public async Task<IEnumerable<SubjectModel>> GetSubjectsByTypeAsync(string type)
        {
            // Validate that type is either "ThucHanh" or "LyThuyet"
            if (type != "ThucHanh" && type != "LyThuyet")
            {
                throw new ArgumentException("Subject type must be either 'ThucHanh' or 'LyThuyet'", nameof(type));
            }

            return await _dbSet
                .Where(s => s.SubjectType == type)
                .OrderBy(s => s.SubjectName)
                .ToListAsync();
        }

        public async Task<bool> IsSubjectCodeExistsAsync(string subjectCode)
        {
            return await _dbSet.AnyAsync(s => s.SubjectCode == subjectCode);
        }

        public async Task<bool> IsSubjectCodeInUseAsync(string subjectCode, int? excludeSubjectId = null)
        {
            var query = _dbSet.Where(s => s.SubjectCode == subjectCode);
            
            if (excludeSubjectId.HasValue)
            {
                query = query.Where(s => s.SubjectID != excludeSubjectId.Value);
            }

            return await query.AnyAsync();
        }

        public override async Task<IEnumerable<SubjectModel>> GetAllAsync()
        {
            return await _dbSet
                .Include(s => s.Grades)
                .OrderBy(s => s.SubjectName)
                .ToListAsync();
        }

        public override async Task<SubjectModel?> GetByIdAsync(object id)
        {
            if (id is int subjectId)
            {
                return await _dbSet
                    .Where(s => s.SubjectID == subjectId)
                    .Include(s => s.Grades)
                    .FirstOrDefaultAsync();
            }
            return null;
        }

        public async Task<bool> ValidateSubjectTypeAsync(string type)
        {
            return type == "ThucHanh" || type == "LyThuyet";
        }
    }
} 