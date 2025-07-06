# Grade Management System

A comprehensive student grade management system built with ASP.NET Core MVC for educational institutions.

## Features

- **Student Management**: Student profiles and academic records
- **Grade Management**: Multi-scale grading system (10-point, 4-point, letter grades)
- **Subject Management**: Course enrollment and tracking
- **Class & Department Management**: Organization and structure
- **Teacher Management**: Faculty profiles and assignments
- **Role-Based Access**: Admin, Teacher (Moderator), Student (User) roles
- **Data Export**: Excel reports for academic records
- **Responsive Design**: Mobile-friendly interface

## Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Identity
- **Frontend**: Bootstrap 5, HTML5, CSS3
- **Export**: EPPlus for Excel generation
- **Language**: C# 12

## User Roles

- **🔑 Admin**: Full system management, user accounts, all operations
- **👨‍🏫 Moderator (Teacher)**: Grade management, student oversight, reporting
- **👨‍🎓 User (Student)**: View grades, personal profile, academic records

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code

### Installation

1. **Clone and navigate**
   ```bash
   git clone <repository-url>
   cd grade-management-system
   ```

2. **Configure database**
   Update `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GradeManagement;Trusted_Connection=true"
     }
   }
   ```

3. **Run migrations and start**
   ```bash
   dotnet ef database update
   dotnet run
   ```

4. **Access application**
   - Go to `https://localhost:5001`
   - Register first account with "Admin" role
   - Setup departments, classes, subjects, then users

## Project Structure

```
grade_management/
├── Areas/                    # Role-based UI areas
│   ├── Admin/               # Administrator interface
│   ├── Moderator/           # Teacher interface
│   └── User/                # Student interface
├── Data/                    # Database context
├── Models/                  # Domain models
├── Repositories/            # Data access layer
├── Views/                   # Razor views
└── wwwroot/                 # Static files
```

## Grade System

- **Formative Grade**: Continuous assessment (0-10)
- **Final Grade**: Final examination (0-10)
- **Auto-calculated**: 10-point, 4-point, and letter grade scales
