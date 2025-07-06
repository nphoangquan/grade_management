using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace grade_management.Models
{
    public class ClassModel
    {
        [Key]
        public string ClassID { get; set; }
        
        [Required(ErrorMessage = "Class name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Class name must be between 2 and 100 characters")]
        [Display(Name = "Class Name")]
        public string ClassName { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public string DepartmentID { get; set; }

        [ForeignKey("DepartmentID")]
        public virtual DepartmentModel? Department { get; set; }
        
        // Navigation property
        public virtual ICollection<StudentModel> Students { get; set; }

        public ClassModel()
        {
            Students = new List<StudentModel>();
        }
    }
} 