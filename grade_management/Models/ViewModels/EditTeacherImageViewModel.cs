using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace grade_management.Models.ViewModels
{
    public class EditTeacherImageViewModel
    {
        [Required(ErrorMessage = "Please select an image")]
        [Display(Name = "Profile Picture")]
        public IFormFile? ImageFile { get; set; }

        public string? CurrentImagePath { get; set; }
        public TeacherModel? Teacher { get; set; }
    }
} 