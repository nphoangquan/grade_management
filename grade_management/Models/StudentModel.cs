using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using grade_management.Data;

namespace grade_management.Models
{
    public class StudentModel
    {
        [Key]
        public string StudentID { get; set; }
        
        [Required(ErrorMessage = "Student code is required")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Student code must be between 2 and 20 characters")]
        [Display(Name = "Student Code")]
        public string StudentCode { get; set; }
        
        [Required(ErrorMessage = "Student name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        [Display(Name = "Student Name")]
        public string StudentName { get; set; }
        
        [Required(ErrorMessage = "Gender is required")]
        [Display(Name = "Gender")]
        public string StudentSex { get; set; }
        
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string StudentEmail { get; set; }
        
        [Required(ErrorMessage = "Class is required")]
        [Display(Name = "Class")]
        public string ClassID { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public string DepartmentID { get; set; }

        [Display(Name = "Profile Picture")]
        public string? ImagePath { get; set; }

        [ForeignKey("ClassID")]
        public virtual ClassModel? Class { get; set; }

        [ForeignKey("DepartmentID")]
        public virtual DepartmentModel? Department { get; set; }
        
        // User account link (optional - only if student has an account)
        public string? UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
        
        // Navigation property for grades
        public virtual ICollection<GradeModel> Grades { get; set; }

        public StudentModel()
        {
            Grades = new List<GradeModel>();
        }
    }
} 