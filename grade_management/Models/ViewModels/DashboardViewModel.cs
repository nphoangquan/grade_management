namespace grade_management.Models
{
    public class DashboardViewModel
    {
        public int StudentCount { get; set; }
        public int TeacherCount { get; set; }
        public int SubjectCount { get; set; }
        public double AverageGPA { get; set; }
        public GradeDistributionViewModel GradeDistribution { get; set; }
    }

    public class GradeDistributionViewModel
    {
        public int AGrade { get; set; }
        public int BPlusGrade { get; set; }
        public int BGrade { get; set; }
        public int CPlusGrade { get; set; }
        public int CGrade { get; set; }
        public int DPlusGrade { get; set; }
        public int DGrade { get; set; }
        public int FPlusGrade { get; set; }
        public int FGrade { get; set; }
    }
} 