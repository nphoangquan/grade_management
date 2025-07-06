using Microsoft.EntityFrameworkCore;
using grade_management.Data;
using grade_management.Models;

namespace grade_management.Extensions
{
    public static class UserExtensions
    {
        /// <summary>
        /// Get student information linked to this user account
        /// </summary>
        public static async Task<StudentModel?> GetStudentAsync(this ApplicationUser user, ApplicationDbContext context)
        {
            if (string.IsNullOrEmpty(user.Id))
                return null;

            return await context.Students
                .Include(s => s.Class)
                .Include(s => s.Grades)
                    .ThenInclude(g => g.Subject)
                .FirstOrDefaultAsync(s => s.UserId == user.Id);
        }

        /// <summary>
        /// Check if user has a linked student account
        /// </summary>
        public static async Task<bool> HasStudentAccountAsync(this ApplicationUser user, ApplicationDbContext context)
        {
            if (string.IsNullOrEmpty(user.Id))
                return false;

            return await context.Students.AnyAsync(s => s.UserId == user.Id);
        }

        /// <summary>
        /// Get student ID linked to this user
        /// </summary>
        public static async Task<string?> GetStudentIdAsync(this ApplicationUser user, ApplicationDbContext context)
        {
            if (string.IsNullOrEmpty(user.Id))
                return null;

            var student = await context.Students
                .Where(s => s.UserId == user.Id)
                .Select(s => s.StudentID)
                .FirstOrDefaultAsync();

            return student;
        }
    }
} 