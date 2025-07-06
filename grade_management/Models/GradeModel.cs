using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System;

namespace grade_management.Models
{
    public class GradeModel
    {
        [Key]
        public int GradeID { get; set; }
        
        [Required]
        public string StudentID { get; set; }
        
        [Required]
        public int SubjectID { get; set; }
        
        [Range(0, 10)]
        [Display(Name = "Formative Grade")]
        public float FormativeGrade { get; set; }

        [Range(0, 10)]
        [Display(Name = "Final Grade")]
        public float FinalGrade { get; set; }
        
        [Display(Name = "10-Scale Grade")]
        [Range(0, 10, ErrorMessage = "Grade must be between 0 and 10")]
        public float TenGradeScale { get; private set; }
        
        [Display(Name = "4-Scale Grade")]
        [Range(0, 4, ErrorMessage = "Grade must be between 0 and 4")]
        public float FourGradeScale { get; private set; }
        
        [Display(Name = "Grade To Letter")]
        public string GradeToLetter { get; private set; }
        
        public virtual StudentModel Student { get; set; }
        public virtual SubjectModel Subject { get; set; }

        public void CalculateGrades()
        {
            // Calculate 10-scale grade (average of formative and final)
            TenGradeScale = (FormativeGrade + FinalGrade) / 2;

            // Calculate letter grade and 4-scale grade based on 10-scale grade
            if (TenGradeScale >= 8.5f && TenGradeScale <= 10f)
            {
                GradeToLetter = "A";
                FourGradeScale = 4.0f;
            }
            else if (TenGradeScale >= 7.8f && TenGradeScale <= 8.4f)
            {
                GradeToLetter = "B+";
                FourGradeScale = 3.5f;
            }
            else if (TenGradeScale >= 7.0f && TenGradeScale <= 7.7f)
            {
                GradeToLetter = "B";
                FourGradeScale = 3.0f;
            }
            else if (TenGradeScale >= 6.3f && TenGradeScale <= 6.9f)
            {
                GradeToLetter = "C+";
                FourGradeScale = 2.5f;
            }
            else if (TenGradeScale >= 5.5f && TenGradeScale <= 6.2f)
            {
                GradeToLetter = "C";
                FourGradeScale = 2.0f;
            }
            else if (TenGradeScale >= 4.8f && TenGradeScale <= 5.4f)
            {
                GradeToLetter = "D+";
                FourGradeScale = 1.5f;
            }
            else if (TenGradeScale >= 4.0f && TenGradeScale <= 4.7f)
            {
                GradeToLetter = "D";
                FourGradeScale = 1.0f;
            }
            else if (TenGradeScale >= 3.0f && TenGradeScale <= 3.9f)
            {
                GradeToLetter = "F+";
                FourGradeScale = 0.5f;
            }
            else
            {
                GradeToLetter = "F";
                FourGradeScale = 0.0f;
            }
        }
    }
} 