using System.ComponentModel.DataAnnotations;

namespace grade_management.Models.ViewModels
{
    public class CreateStudentWithAccountViewModel
    {
        // Student Information
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

        // Account Information
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Create User Account")]
        public bool CreateUserAccount { get; set; } = true;
    }
} 