using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using grade_management.Models;

namespace grade_management.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets for domain models
        public DbSet<StudentModel> Students { get; set; }
        public DbSet<TeacherModel> Teachers { get; set; }
        public DbSet<SubjectModel> Subjects { get; set; }
        public DbSet<ClassModel> Classes { get; set; }
        public DbSet<GradeModel> Grades { get; set; }
        public DbSet<DepartmentModel> Departments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure StudentModel
            builder.Entity<StudentModel>(entity =>
            {
                entity.HasKey(e => e.StudentID);
                entity.Property(e => e.StudentName).IsRequired();
                entity.Property(e => e.StudentSex).IsRequired();
                entity.Property(e => e.StudentEmail).IsRequired();
                entity.Property(e => e.ClassID).IsRequired();
                entity.Property(e => e.StudentCode).IsRequired();
                entity.Property(e => e.DepartmentID).IsRequired();
                entity.Property(e => e.ImagePath).IsRequired(false);

                // Add unique index for StudentCode
                entity.HasIndex(e => e.StudentCode).IsUnique();
                
                // Add index for foreign keys
                entity.HasIndex(e => e.ClassID);
                entity.HasIndex(e => e.DepartmentID);

                // Relationships
                entity.HasOne(s => s.Class)
                      .WithMany(c => c.Students)
                      .HasForeignKey(s => s.ClassID)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Department)
                      .WithMany(d => d.Students)
                      .HasForeignKey(s => s.DepartmentID)
                      .OnDelete(DeleteBehavior.Restrict);

                // User account relationship (optional)
                entity.HasOne(s => s.User)
                      .WithOne()
                      .HasForeignKey<StudentModel>(s => s.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure TeacherModel
            builder.Entity<TeacherModel>(entity =>
            {
                entity.HasKey(e => e.TeacherID);
                entity.Property(e => e.TeacherCode).IsRequired();
                entity.Property(e => e.TeacherName).IsRequired();
                entity.Property(e => e.TeacherSex).IsRequired();
                entity.Property(e => e.TeacherEmail).IsRequired();
                entity.Property(e => e.DepartmentID).IsRequired();
                entity.Property(e => e.ImagePath).IsRequired(false);

                // Add unique index for email and teacher code
                entity.HasIndex(e => e.TeacherEmail).IsUnique();
                entity.HasIndex(e => new { e.TeacherCode, e.DepartmentID }).IsUnique();
                
                // Add index for foreign key
                entity.HasIndex(e => e.DepartmentID);

                // Department relationship
                entity.HasOne(t => t.Department)
                      .WithMany(d => d.Teachers)
                      .HasForeignKey(t => t.DepartmentID)
                      .OnDelete(DeleteBehavior.Restrict);

                // User account relationship (optional)
                entity.HasOne(t => t.User)
                      .WithOne()
                      .HasForeignKey<TeacherModel>(t => t.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure SubjectModel
            builder.Entity<SubjectModel>(entity =>
            {
                entity.HasKey(e => e.SubjectID);
                entity.Property(e => e.SubjectCode).IsRequired();
                entity.Property(e => e.SubjectName).IsRequired();
                entity.Property(e => e.SubjectStatus).IsRequired();
                entity.Property(e => e.SubjectType).IsRequired();
                entity.Property(e => e.SubjectCredits).IsRequired();
                entity.Property(e => e.DepartmentID).IsRequired();
                
                // Add unique index for subject code within department
                entity.HasIndex(e => new { e.SubjectCode, e.DepartmentID }).IsUnique();
                
                // Add index for foreign key
                entity.HasIndex(e => e.DepartmentID);

                // Department relationship
                entity.HasOne(s => s.Department)
                      .WithMany(d => d.Subjects)
                      .HasForeignKey(s => s.DepartmentID)
                      .OnDelete(DeleteBehavior.Restrict);

                // Configure check constraints using ToTable
                entity.ToTable(t => t.HasCheckConstraint("CK_Subject_Type", "SubjectType IN ('ThucHanh', 'LyThuyet')"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Subject_Status", "SubjectStatus IN ('Open', 'Close', 'Full')"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Subject_Credits", "SubjectCredits >= 1 AND SubjectCredits <= 10"));
            });

            // Configure ClassModel
            builder.Entity<ClassModel>(entity =>
            {
                entity.HasKey(e => e.ClassID);
                entity.Property(e => e.ClassName).IsRequired();
                entity.Property(e => e.DepartmentID).IsRequired();

                // Add unique index for class name within department
                entity.HasIndex(e => new { e.ClassName, e.DepartmentID }).IsUnique();
                
                // Add index for foreign key
                entity.HasIndex(e => e.DepartmentID);

                // Department relationship
                entity.HasOne(c => c.Department)
                      .WithMany(d => d.Classes)
                      .HasForeignKey(c => c.DepartmentID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure DepartmentModel
            builder.Entity<DepartmentModel>(entity =>
            {
                entity.HasKey(e => e.DepartmentID);
                entity.Property(e => e.DepartmentCode).IsRequired();
                entity.Property(e => e.DepartmentName).IsRequired();

                // Add unique indexes
                entity.HasIndex(e => e.DepartmentCode).IsUnique();
                entity.HasIndex(e => e.DepartmentName).IsUnique();
            });

            // Configure GradeModel
            builder.Entity<GradeModel>(entity =>
            {
                entity.HasKey(e => e.GradeID);
                
                // Required fields
                entity.Property(e => e.StudentID).IsRequired();
                entity.Property(e => e.SubjectID).IsRequired();
                
                // Grade fields with proper column types
                entity.Property(e => e.FormativeGrade)
                    .IsRequired()
                    .HasColumnType("float");
                
                entity.Property(e => e.FinalGrade)
                    .IsRequired()
                    .HasColumnType("float");
                
                entity.Property(e => e.TenGradeScale)
                    .IsRequired()
                    .HasColumnType("float");
                
                entity.Property(e => e.FourGradeScale)
                    .IsRequired()
                    .HasColumnType("float");
                
                entity.Property(e => e.GradeToLetter)
                    .IsRequired()
                    .HasMaxLength(2);

                // Add unique index to prevent duplicate grades for same student and subject
                entity.HasIndex(e => new { e.StudentID, e.SubjectID }).IsUnique();
                
                // Add indexes for foreign keys
                entity.HasIndex(e => e.StudentID);
                entity.HasIndex(e => e.SubjectID);

                // Configure check constraints using ToTable
                entity.ToTable(t => t.HasCheckConstraint("CK_Grade_Formative", "FormativeGrade >= 0 AND FormativeGrade <= 10"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Grade_Final", "FinalGrade >= 0 AND FinalGrade <= 10"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Grade_Ten", "TenGradeScale >= 0 AND TenGradeScale <= 10"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Grade_Four", "FourGradeScale >= 0 AND FourGradeScale <= 4"));
                entity.ToTable(t => t.HasCheckConstraint("CK_Grade_Letter", "GradeToLetter IN ('A', 'B+', 'B', 'C+', 'C', 'D+', 'D', 'F+', 'F')"));

                // Relationships
                entity.HasOne(g => g.Student)
                      .WithMany(s => s.Grades)
                      .HasForeignKey(g => g.StudentID)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.Subject)
                      .WithMany(s => s.Grades)
                      .HasForeignKey(g => g.SubjectID)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
} 