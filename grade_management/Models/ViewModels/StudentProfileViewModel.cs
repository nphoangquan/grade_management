using grade_management.Data;

namespace grade_management.Models.ViewModels
{
    public class StudentProfileViewModel
    {
        public StudentModel Student { get; set; }
        public ApplicationUser User { get; set; }
        public List<GradeModel> RecentGrades { get; set; } = new();
        public double GPA { get; set; }
        public int TotalSubjects { get; set; }
        public int CompletedSubjects { get; set; }
        public string AcademicStatus { get; set; } = "Active";
    }
} 