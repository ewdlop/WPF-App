# 🚀 具有高级数据访问层的 WPF 企业应用程序

## 📋 项目概述

**WPF 企业应用程序** 是一个使用 .NET 9 构建的复杂 Windows Presentation Foundation (WPF) 应用程序，展示了现代开发实践、先进的UI/UX模式和企业级架构。该项目演示了一个全面的业务管理系统，具有基于 Entity Framework Core 的强大数据访问层，实现了存储库和工作单元模式以实现最佳数据管理。

## ✨ 主要特性

### 🏗️ 架构与设计模式
- **MVVM (Model-View-ViewModel)** 模式，具有适当的关注点分离
- **存储库模式** 提供通用和专用存储库
- **工作单元模式** 用于事务管理
- **Entity Framework Core** 采用 Code-First 方法
- **依赖注入** 使用 Microsoft.Extensions.DependencyInjection
- **命令模式** 使用 ICommand 实现
- **观察者模式** 使用 INotifyPropertyChanged
- **工厂模式** 用于对象创建

### 🗄️ 高级数据访问层
- **Entity Framework Core 8.0** 配合 SQLite 数据库
- **通用存储库模式** 提供全面的 CRUD 操作
- **工作单元模式** 用于事务协调
- **自动审计日志** 配合变更追踪
- **优化的数据库查询** 具有适当的索引
- **批量操作** 用于性能优化
- **数据库迁移** 配合种子数据
- **连接恢复能力** 和错误处理

### 📊 业务领域模型
- **员工管理** 具有层次关系
- **部门管理** 配合预算跟踪
- **项目管理** 配合状态和优先级跟踪
- **项目分配** 配合基于角色的分配
- **项目里程碑** 配合进度跟踪
- **全面的审计日志** 用于合规性

### 🎨 现代UI/UX特性
- **Material Design** 风格界面，配合自定义样式
- **深色/浅色主题** 切换，具有平滑过渡
- **响应式布局** 适应不同屏幕尺寸
- **自定义控件** 和 UserControls 以实现可重用性
- **流畅动画** 和微交互
- **无障碍支持** 配合适当的 ARIA 标签

## 🏛️ 项目结构

```
WpfApp2/
├── 📁 Models/                      # 领域模型和实体
│   ├── Employee.cs                 # 具有关系的员工实体
│   ├── Department.cs               # 具有预算跟踪的部门实体
│   ├── Project.cs                  # 具有状态/优先级的项目实体
│   ├── ProjectAssignment.cs        # 多对多关系实体
│   ├── ProjectMilestone.cs         # 项目里程碑跟踪
│   └── AuditLog.cs                 # 全面的审计日志
├── 📁 Data/                        # Entity Framework Core 数据层
│   ├── ApplicationDbContext.cs     # 主要 EF DbContext 配置
│   ├── Repositories/               # 存储库模式实现
│   │   ├── Interfaces/
│   │   │   ├── IRepository.cs      # 通用存储库接口
│   │   │   ├── IUnitOfWork.cs      # 工作单元接口
│   │   │   ├── IEmployeeRepository.cs
│   │   │   ├── IDepartmentRepository.cs
│   │   │   └── IProjectRepository.cs
│   │   ├── Repository.cs           # 通用存储库实现
│   │   ├── UnitOfWork.cs          # 工作单元实现
│   │   ├── EmployeeRepository.cs   # 专用员工操作
│   │   ├── DepartmentRepository.cs # 专用部门操作
│   │   └── ProjectRepository.cs    # 专用项目操作
│   └── Migrations/                 # EF Core 迁移
├── 📁 Services/                    # 业务逻辑服务
│   ├── Interfaces/
│   │   ├── IEmployeeService.cs
│   │   ├── IDepartmentService.cs
│   │   ├── IProjectService.cs
│   │   └── IAuditService.cs
│   ├── EmployeeService.cs
│   ├── DepartmentService.cs
│   ├── ProjectService.cs
│   └── AuditService.cs
├── 📁 ViewModels/                  # MVVM 视图模型
│   ├── Base/
│   │   ├── BaseViewModel.cs
│   │   └── RelayCommand.cs
│   ├── MainViewModel.cs
│   ├── EmployeeViewModel.cs
│   ├── DepartmentViewModel.cs
│   └── ProjectViewModel.cs
├── 📁 Views/                       # WPF 视图和窗口
│   ├── MainWindow.xaml
│   ├── EmployeeManagementView.xaml
│   ├── DepartmentView.xaml
│   └── ProjectManagementView.xaml
├── 📁 Configuration/               # 应用程序配置
│   ├── appsettings.json           # 数据库连接字符串
│   ├── appsettings.Development.json
│   └── appsettings.Production.json
└── 📁 Tests/                      # 单元和集成测试
    ├── RepositoryTests/
    ├── ServiceTests/
    └── IntegrationTests/
```

## 🗄️ 数据访问层架构

### 实体关系
```
Employee (1) ←→ (N) Department
Employee (1) ←→ (N) Employee (Manager-DirectReports)
Project (1) ←→ (N) Department
Project (1) ←→ (N) ProjectAssignment ←→ (N) Employee
Project (1) ←→ (N) ProjectMilestone
Employee (1) ←→ (N) AuditLog
```

### 存储库模式特性
```csharp
// 通用存储库接口
public interface IRepository<T> where T : class
{
    // 基本 CRUD 操作
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    
    // 高级查询
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    
    // 分页和排序
    Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize);
    Task<IEnumerable<T>> GetAllSortedAsync<TKey>(Expression<Func<T, TKey>> keySelector);
    
    // 批量操作
    Task AddRangeAsync(IEnumerable<T> entities);
    Task UpdateRangeAsync(IEnumerable<T> entities);
    Task DeleteRangeAsync(IEnumerable<T> entities);
    
    // 计数
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
}
```

### 工作单元模式
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

### 实体配置
- **完整的属性映射** 具有适当的数据类型
- **优化的索引** 提供更好的查询性能
- **外键关系** 配合适当的级联行为
- **默认值** 和约束
- **审计字段自动化** 配合时间戳

## 🚀 快速开始

### 先决条件
- **Visual Studio 2022** (17.8 或更高版本) 或 **JetBrains Rider**
- **.NET 9 SDK** 或更高版本
- **Windows 10/11** (版本 1903 或更高)

### 安装

1. **克隆仓库**
   ```bash
   git clone https://github.com/yourusername/WpfApp2.git
   cd WpfApp2
   ```

2. **还原 NuGet 包**
   ```bash
   dotnet restore
   ```

3. **数据库设置** (SQLite - 无需额外设置)
   ```bash
   # 数据库文件将自动创建在: ./Database/WpfApp2.db
   ```

4. **运行数据库迁移**
   ```bash
   dotnet ef database update
   ```

5. **构建和运行**
   ```bash
   dotnet build
   dotnet run
   ```

## 📦 NuGet 包

### 核心依赖
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

### 测试
```xml
<PackageReference Include="xunit" Version="2.6.1" />
<PackageReference Include="Moq" Version="4.20.69" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
```

## 🗃️ 数据库架构

### 员工表
- **个人信息**: FirstName, LastName, Email, Phone
- **雇佣详情**: EmployeeNumber, Position, HireDate, Salary
- **关系**: DepartmentId, ManagerId
- **审计字段**: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy

### 部门表
- **基本信息**: Name, Code, Description
- **财务**: 预算跟踪
- **管理**: ManagerId (自引用)
- **审计字段**: 完整的审计跟踪

### 项目表
- **项目详情**: Name, Code, Description
- **时间线**: StartDate, EndDate, EstimatedEndDate
- **财务**: Budget, ActualCost
- **状态跟踪**: Status, Priority, ProgressPercentage
- **关系**: DepartmentId, ProjectManagerId

### 项目分配表
- **分配详情**: Role, HourlyRate, AllocationPercentage
- **时间线**: AssignedDate, UnassignedDate
- **状态**: IsActive 标志

### 项目里程碑表
- **里程碑信息**: Name, Description, DueDate
- **完成情况**: CompletedDate, IsCompleted, ProgressPercentage
- **优先级**: IsCritical 标志

## 🎯 开发路线图

### 第一阶段: 数据基础 ✅ (已完成)
- ✅ Entity Framework Core 配合 SQLite 设置
- ✅ 具有关系的完整领域模型
- ✅ 存储库模式实现
- ✅ 工作单元模式
- ✅ 全面的实体配置
- ✅ 数据库迁移和种子数据
- ✅ 审计日志系统

### 第二阶段: 业务服务 🔄 (进行中)
- 🔄 服务层实现
- 🔄 业务逻辑验证
- 🔄 事务管理
- 🔄 错误处理和日志记录

### 第三阶段: UI 实现 📋 (计划中)
- 📋 MVVM 视图模型
- 📋 配合数据绑定的 WPF 视图
- 📋 CRUD 操作界面
- 📋 数据验证和错误显示

### 第四阶段: 高级功能 📋 (未来)
- 📋 报告和分析
- 📋 数据导出/导入
- 📋 高级搜索和过滤
- 📋 性能优化

## 🛠️ 开发指南

### 数据库最佳实践
- 使用 **迁移** 进行所有架构更改
- 实施 **适当的索引** 用于查询优化
- 遵循表和列的 **命名约定**
- 使用 **适当的数据类型** 和约束
- 为敏感数据实施 **审计跟踪**

### 存储库模式指南
- 保持存储库 **专注于** 数据访问
- 对所有数据库操作使用 **async/await**
- 实施 **适当的错误处理**
- 对常见操作使用 **通用存储库**
- 为复杂查询创建 **专用存储库**

### 性能注意事项
- 使用 **Include()** 进行相关数据的预先加载
- 为大型数据集实施 **分页**
- 对只读操作使用 **AsNoTracking()**
- 通过适当的索引优化 **查询执行**
- 定期监控 **数据库性能**

## 🧪 测试策略

### 存储库测试
```csharp
[Fact]
public async Task GetByIdAsync_WithValidId_ReturnsEmployee()
{
    // 准备
    using var context = GetInMemoryContext();
    var repository = new EmployeeRepository(context, logger);
    
    // 执行
    var employee = await repository.GetByIdAsync(1);
    
    // 断言
    Assert.NotNull(employee);
    Assert.Equal("John", employee.FirstName);
}
```

### 集成测试
- 使用真实数据库测试 **存储库操作**
- 验证工作单元的 **事务行为**
- 测试 **约束违规** 和错误处理
- 验证 **审计日志** 功能

## 📚 学习资源

### Entity Framework Core
- [EF Core 文档](https://docs.microsoft.com/en-us/ef/core/)
- [EF Core Code First 迁移](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [EF Core 的存储库模式](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)

### 设计模式
- [存储库模式](https://martinfowler.com/eaaCatalog/repository.html)
- [工作单元模式](https://martinfowler.com/eaaCatalog/unitOfWork.html)
- [领域驱动设计](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/ddd-oriented-microservice)

## 🤝 贡献

我们欢迎贡献！请查看我们的 [贡献指南](CONTRIBUTING.md) 了解详情。

### 开发设置
1. Fork 仓库
2. 创建功能分支 (`git checkout -b feature/amazing-feature`)
3. 提交更改 (`git commit -m 'Add amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 打开 Pull Request

## 📄 许可证

该项目根据 MIT 许可证授权 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 🙏 致谢

- **Microsoft Entity Framework 团队** 提供出色的 ORM 框架
- **SQLite 团队** 提供轻量级数据库引擎
- **开源社区** 提供宝贵的工具和库

## 📞 支持

- **问题**: [GitHub Issues](https://github.com/yourusername/WpfApp2/issues)
- **讨论**: [GitHub Discussions](https://github.com/yourusername/WpfApp2/discussions)
- **文档**: 查看 `/docs` 文件夹获取详细指南

---

**用 ❤️ 为企业开发而制** 