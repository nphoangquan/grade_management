using System.Collections.Generic;

namespace grade_management.Models
{
    public class StudentGradesViewModel
    {
        public StudentModel Student { get; set; }
        public IEnumerable<SubjectModel> AvailableSubjects { get; set; }
        public IEnumerable<GradeModel> ExistingGrades { get; set; }
    }
} 