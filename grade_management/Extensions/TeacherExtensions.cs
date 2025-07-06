using Microsoft.EntityFrameworkCore;
using grade_management.Data;
using grade_management.Models;

namespace grade_management.Extensions
{
    public static class TeacherExtensions
    {
        /// <summary>
        /// Get the teacher record associated with this user account
        /// </summary>
        public static async Task<TeacherModel?> GetTeacherAsync(this ApplicationUser user, ApplicationDbContext context)
        {
            if (user?.Id == null) return null;

            return await context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == user.Id);
        }

        /// <summary>
        /// Check if this user has an associated teacher account
        /// </summary>
        public static async Task<bool> HasTeacherAccountAsync(this ApplicationUser user, ApplicationDbContext context)
        {
            if (user?.Id == null) return false;

            return await context.Teachers
                .AnyAsync(t => t.UserId == user.Id);
        }

        /// <summary>
        /// Get the teacher ID for this user (if exists)
        /// </summary>
        public static async Task<string?> GetTeacherIdAsync(this ApplicationUser user, ApplicationDbContext context)
        {
            if (user?.Id == null) return null;

            var teacher = await context.Teachers
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            return teacher?.TeacherID;
        }
    }
} 