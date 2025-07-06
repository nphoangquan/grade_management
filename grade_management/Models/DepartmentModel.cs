using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace grade_management.Models
{
    public class DepartmentModel
    {
        [Key]
        public string DepartmentID { get; set; }
        
        [Required(ErrorMessage = "Department code is required")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Department code must be between 2 and 20 characters")]
        [Display(Name = "Department Code")]
        public string DepartmentCode { get; set; }
        
        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Department name must be between 2 and 100 characters")]
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; }
        
        // Navigation properties
        public virtual ICollection<StudentModel> Students { get; set; }
        public virtual ICollection<TeacherModel> Teachers { get; set; }
        public virtual ICollection<ClassModel> Classes { get; set; }
        public virtual ICollection<SubjectModel> Subjects { get; set; }

        public DepartmentModel()
        {
            Students = new List<StudentModel>();
            Teachers = new List<TeacherModel>();
            Classes = new List<ClassModel>();
            Subjects = new List<SubjectModel>();
        }
    }
} 