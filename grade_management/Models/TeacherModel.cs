using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using grade_management.Data;

namespace grade_management.Models
{
    public class TeacherModel
    {
        [Key]
        public string TeacherID { get; set; }
        
        [Required(ErrorMessage = "Teacher code is required")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Teacher code must be between 2 and 20 characters")]
        [Display(Name = "Teacher Code")]
        public string TeacherCode { get; set; }
        
        [Required(ErrorMessage = "Teacher name is required")]
        [Display(Name = "Teacher Name")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string TeacherName { get; set; }
        
        [Required(ErrorMessage = "Gender is required")]
        [Display(Name = "Gender")]
        public string TeacherSex { get; set; }
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string TeacherEmail { get; set; }
        
        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public string DepartmentID { get; set; }

        [Display(Name = "Profile Picture")]
        public string? ImagePath { get; set; }
        
        [ForeignKey("DepartmentID")]
        public virtual DepartmentModel? Department { get; set; }
        
        // User account link (optional - only if teacher has an account)
        public string? UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
} 