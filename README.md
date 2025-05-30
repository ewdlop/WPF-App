# 🚀 WPF Enterprise Application with Advanced Data Access Layer

## 📋 Project Overview

**WPF Enterprise Application** is a sophisticated Windows Presentation Foundation (WPF) application built with .NET 9, showcasing modern development practices, advanced UI/UX patterns, and enterprise-grade architecture. This project demonstrates a comprehensive business management system with a robust Entity Framework Core-based data access layer, implementing Repository and Unit of Work patterns for optimal data management.

## ✨ Key Features

### 🏗️ Architecture & Patterns
- **MVVM (Model-View-ViewModel)** pattern with proper separation of concerns
- **Repository Pattern** with generic and specialized repositories
- **Unit of Work Pattern** for transaction management
- **Entity Framework Core** with Code-First approach
- **Dependency Injection** using Microsoft.Extensions.DependencyInjection
- **Command Pattern** with ICommand implementations
- **Observer Pattern** with INotifyPropertyChanged
- **Factory Pattern** for object creation

### 🗄️ Advanced Data Access Layer
- **Entity Framework Core 8.0** with SQLite database
- **Generic Repository Pattern** with comprehensive CRUD operations
- **Unit of Work Pattern** for transaction coordination
- **Automatic Audit Logging** with change tracking
- **Optimized Database Queries** with proper indexing
- **Bulk Operations** for performance optimization
- **Database Migrations** with seed data
- **Connection Resilience** and error handling

### 📊 Business Domain Models
- **Employee Management** with hierarchical relationships
- **Department Management** with budget tracking
- **Project Management** with status and priority tracking
- **Project Assignments** with role-based allocations
- **Project Milestones** with progress tracking
- **Comprehensive Audit Logging** for compliance

### 🎨 Modern UI/UX Features
- **Material Design** inspired interface with custom styling
- **Dark/Light Theme** switching with smooth transitions
- **Responsive Layout** that adapts to different screen sizes
- **Custom Controls** and UserControls for reusability
- **Smooth Animations** and micro-interactions
- **Accessibility Support** with proper ARIA labels

## 🏛️ Project Structure

```
WpfApp2/
├── 📁 Models/                      # Domain Models & Entities
│   ├── Employee.cs                 # Employee entity with relationships
│   ├── Department.cs               # Department entity with budget tracking
│   ├── Project.cs                  # Project entity with status/priority
│   ├── ProjectAssignment.cs        # Many-to-many relationship entity
│   ├── ProjectMilestone.cs         # Project milestone tracking
│   └── AuditLog.cs                 # Comprehensive audit logging
├── 📁 Data/                        # Entity Framework Core Data Layer
│   ├── ApplicationDbContext.cs     # Main EF DbContext with configurations
│   ├── Repositories/               # Repository pattern implementation
│   │   ├── Interfaces/
│   │   │   ├── IRepository.cs      # Generic repository interface
│   │   │   ├── IUnitOfWork.cs      # Unit of work interface
│   │   │   ├── IEmployeeRepository.cs
│   │   │   ├── IDepartmentRepository.cs
│   │   │   └── IProjectRepository.cs
│   │   ├── Repository.cs           # Generic repository implementation
│   │   ├── UnitOfWork.cs          # Unit of work implementation
│   │   ├── EmployeeRepository.cs   # Specialized employee operations
│   │   ├── DepartmentRepository.cs # Specialized department operations
│   │   └── ProjectRepository.cs    # Specialized project operations
│   └── Migrations/                 # EF Core migrations
├── 📁 Services/                    # Business Logic Services
│   ├── Interfaces/
│   │   ├── IEmployeeService.cs
│   │   ├── IDepartmentService.cs
│   │   ├── IProjectService.cs
│   │   └── IAuditService.cs
│   ├── EmployeeService.cs
│   ├── DepartmentService.cs
│   ├── ProjectService.cs
│   └── AuditService.cs
├── 📁 ViewModels/                  # MVVM ViewModels
│   ├── Base/
│   │   ├── BaseViewModel.cs
│   │   └── RelayCommand.cs
│   ├── MainViewModel.cs
│   ├── EmployeeViewModel.cs
│   ├── DepartmentViewModel.cs
│   └── ProjectViewModel.cs
├── 📁 Views/                       # WPF Views and Windows
│   ├── MainWindow.xaml
│   ├── EmployeeManagementView.xaml
│   ├── DepartmentView.xaml
│   └── ProjectManagementView.xaml
├── 📁 Configuration/               # Application Configuration
│   ├── appsettings.json           # Database connection strings
│   ├── appsettings.Development.json
│   └── appsettings.Production.json
└── 📁 Tests/                      # Unit and Integration Tests
    ├── RepositoryTests/
    ├── ServiceTests/
    └── IntegrationTests/
```

## 🗄️ Data Access Layer Architecture

### Entity Relationships
```
Employee (1) ←→ (N) Department
Employee (1) ←→ (N) Employee (Manager-DirectReports)
Project (1) ←→ (N) Department
Project (1) ←→ (N) ProjectAssignment ←→ (N) Employee
Project (1) ←→ (N) ProjectMilestone
Employee (1) ←→ (N) AuditLog
```

### Repository Pattern Features
```csharp
// Generic Repository Interface
public interface IRepository<T> where T : class
{
    // Basic CRUD Operations
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    
    // Advanced Querying
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    
    // Pagination & Sorting
    Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize);
    Task<IEnumerable<T>> GetAllSortedAsync<TKey>(Expression<Func<T, TKey>> keySelector);
    
    // Bulk Operations
    Task AddRangeAsync(IEnumerable<T> entities);
    Task UpdateRangeAsync(IEnumerable<T> entities);
    Task DeleteRangeAsync(IEnumerable<T> entities);
    
    // Counting
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
}
```

### Unit of Work Pattern
```csharp
public interface IUnitOfWork : IDisposable
{
    IEmployeeRepository Employees { get; }
    IDepartmentRepository Departments { get; }
    IProjectRepository Projects { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

### Entity Configurations
- **Complete Property Mapping** with proper data types
- **Optimized Indexing** for better query performance
- **Foreign Key Relationships** with appropriate cascade behaviors
- **Default Values** and constraints
- **Audit Field Automation** with timestamps

## 🚀 Getting Started

### Prerequisites
- **Visual Studio 2022** (17.8 or later) or **JetBrains Rider**
- **.NET 9 SDK** or later
- **Windows 10/11** (version 1903 or later)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/WpfApp2.git
   cd WpfApp2
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Database Setup** (SQLite - No additional setup required)
   ```bash
   # Database file will be created automatically at: ./Database/WpfApp2.db
   ```

4. **Run database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Build and run**
   ```bash
   dotnet build
   dotnet run
   ```

## 📦 NuGet Packages

### Core Dependencies
```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
```

### Entity Framework Core
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
```

### Testing
```xml
<PackageReference Include="xunit" Version="2.6.1" />
<PackageReference Include="Moq" Version="4.20.69" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
```

## 🗃️ Database Schema

### Employee Table
- **Personal Information**: FirstName, LastName, Email, Phone
- **Employment Details**: EmployeeNumber, Position, HireDate, Salary
- **Relationships**: DepartmentId, ManagerId
- **Audit Fields**: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy

### Department Table
- **Basic Information**: Name, Code, Description
- **Financial**: Budget tracking
- **Management**: ManagerId (self-referencing)
- **Audit Fields**: Complete audit trail

### Project Table
- **Project Details**: Name, Code, Description
- **Timeline**: StartDate, EndDate, EstimatedEndDate
- **Financial**: Budget, ActualCost
- **Status Tracking**: Status, Priority, ProgressPercentage
- **Relationships**: DepartmentId, ProjectManagerId

### Project Assignment Table
- **Assignment Details**: Role, HourlyRate, AllocationPercentage
- **Timeline**: AssignedDate, UnassignedDate
- **Status**: IsActive flag

### Project Milestone Table
- **Milestone Information**: Name, Description, DueDate
- **Completion**: CompletedDate, IsCompleted, ProgressPercentage
- **Priority**: IsCritical flag

## 🎯 Development Roadmap

### Phase 1: Data Foundation ✅ (Completed)
- ✅ Entity Framework Core setup with SQLite
- ✅ Complete domain models with relationships
- ✅ Repository pattern implementation
- ✅ Unit of Work pattern
- ✅ Comprehensive entity configurations
- ✅ Database migrations and seed data
- ✅ Audit logging system

### Phase 2: Business Services 🔄 (In Progress)
- 🔄 Service layer implementation
- 🔄 Business logic validation
- 🔄 Transaction management
- 🔄 Error handling and logging

### Phase 3: UI Implementation 📋 (Planned)
- 📋 MVVM ViewModels
- 📋 WPF Views with data binding
- 📋 CRUD operations interface
- 📋 Data validation and error display

### Phase 4: Advanced Features 📋 (Future)
- 📋 Reporting and analytics
- 📋 Data export/import
- 📋 Advanced search and filtering
- 📋 Performance optimizations

## 🛠️ Development Guidelines

### Database Best Practices
- Use **migrations** for all schema changes
- Implement **proper indexing** for query optimization
- Follow **naming conventions** for tables and columns
- Use **appropriate data types** and constraints
- Implement **audit trails** for sensitive data

### Repository Pattern Guidelines
- Keep repositories **focused** on data access only
- Use **async/await** for all database operations
- Implement **proper error handling**
- Use **generic repositories** for common operations
- Create **specialized repositories** for complex queries

### Performance Considerations
- Use **Include()** for eager loading related data
- Implement **pagination** for large datasets
- Use **AsNoTracking()** for read-only operations
- Optimize **query execution** with proper indexing
- Monitor **database performance** regularly

## 🧪 Testing Strategy

### Repository Testing
```csharp
[Fact]
public async Task GetByIdAsync_WithValidId_ReturnsEmployee()
{
    // Arrange
    using var context = GetInMemoryContext();
    var repository = new EmployeeRepository(context, logger);
    
    // Act
    var employee = await repository.GetByIdAsync(1);
    
    // Assert
    Assert.NotNull(employee);
    Assert.Equal("John", employee.FirstName);
}
```

### Integration Testing
- Test **repository operations** with real database
- Verify **transaction behavior** with Unit of Work
- Test **constraint violations** and error handling
- Validate **audit logging** functionality

## 📚 Learning Resources

### Entity Framework Core
- [EF Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [EF Core Code First Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Repository Pattern with EF Core](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)

### Design Patterns
- [Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)
- [Unit of Work Pattern](https://martinfowler.com/eaaCatalog/unitOfWork.html)
- [Domain-Driven Design](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/ddd-oriented-microservice)

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Development Setup
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Microsoft Entity Framework Team** for the excellent ORM framework
- **SQLite Team** for the lightweight database engine
- **Open Source Community** for invaluable tools and libraries

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/yourusername/WpfApp2/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/WpfApp2/discussions)
- **Documentation**: Check the `/docs` folder for detailed guides

---

**Made with ❤️ for Enterprise Development** 