using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grade_management.Models
{
    public class SubjectModel
    {
        [Key]
        public int SubjectID { get; set; }
        
        [Required(ErrorMessage = "Subject code is required")]
        [Display(Name = "Subject Code")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Subject code must be between 2 and 20 characters")]
        public string SubjectCode { get; set; }
        
        [Required(ErrorMessage = "Subject name is required")]
        [Display(Name = "Subject Name")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Subject name must be between 2 and 100 characters")]
        public string SubjectName { get; set; }
        
        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public string SubjectStatus { get; set; } // Status is: Close, Open, Full
        
        [Required(ErrorMessage = "Subject type is required")]
        [Display(Name = "Subject Type")]
        public string SubjectType { get; set; } // ThucHanh or LyThuyet
        
        [Display(Name = "Time")]
        public string SubjectTime { get; set; } // Allow to select time of Subject
        
        [Required(ErrorMessage = "Credits is required")]
        [Display(Name = "Credits")]
        [Range(1, 10, ErrorMessage = "Credits must be between 1 and 10")]
        public int SubjectCredits { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public string DepartmentID { get; set; }

        [ForeignKey("DepartmentID")]
        public virtual DepartmentModel? Department { get; set; }
        
        // Navigation property for grades
        public virtual ICollection<GradeModel> Grades { get; set; }

        public SubjectModel()
        {
            Grades = new List<GradeModel>();
        }
    }
} 