using grade_management.Models;
using System.ComponentModel.DataAnnotations;

namespace grade_management.Models.ViewModels
{
    public class TeacherProfileViewModel
    {
        // Teacher Information
        public string TeacherID { get; set; }
        public string TeacherCode { get; set; }
        public string TeacherName { get; set; }
        public string TeacherSex { get; set; }
        public string TeacherEmail { get; set; }
        
        // User Account Information
        public string UserName { get; set; }
        public string FullName { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? ImagePath { get; set; }
        
        // Teaching Statistics (placeholder for future implementation)
        public int TotalSubjectsTaught { get; set; }
        public int TotalStudentsTeaching { get; set; }
        public int TotalClassesManaged { get; set; }
        
        // Quick Actions
        public bool CanViewStudents { get; set; } = true;
        public bool CanManageGrades { get; set; } = true;
        public bool CanViewReports { get; set; } = true;
    }
} 