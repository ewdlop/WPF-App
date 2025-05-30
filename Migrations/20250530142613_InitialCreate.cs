using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WpfApp2.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "INTEGER", nullable: true),
                    TableName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RecordId = table.Column<int>(type: "INTEGER", nullable: true),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SessionId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsSuccessful = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ApplicationVersion = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    MachineName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ExecutionTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    EmployeeId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ManagerId = table.Column<int>(type: "INTEGER", nullable: true),
                    Budget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EmployeeNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    Position = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DepartmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ManagerId = table.Column<int>(type: "INTEGER", nullable: true),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HireDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TerminationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    EmergencyContact = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ProfileImagePath = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Employees_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Priority = table.Column<string>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstimatedEndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Budget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProgressPercentage = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    DepartmentId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectManagerId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Employees_ProjectManagerId",
                        column: x => x.ProjectManagerId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    EmployeeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UnassignedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    AllocationPercentage = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 100),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectAssignments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectAssignments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectMilestones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    IsCritical = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    ProgressPercentage = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMilestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectMilestones_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "Budget", "Code", "CreatedAt", "CreatedBy", "Description", "IsActive", "ManagerId", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, 500000m, "IT", new DateTime(2025, 5, 30, 14, 26, 12, 817, DateTimeKind.Utc).AddTicks(9489), "System", "Responsible for all technology infrastructure and software development", true, null, "Information Technology", new DateTime(2025, 5, 30, 14, 26, 12, 817, DateTimeKind.Utc).AddTicks(9592), "System" },
                    { 2, 200000m, "HR", new DateTime(2025, 5, 30, 14, 26, 12, 817, DateTimeKind.Utc).AddTicks(9957), "System", "Manages employee relations, recruitment, and organizational development", true, null, "Human Resources", new DateTime(2025, 5, 30, 14, 26, 12, 817, DateTimeKind.Utc).AddTicks(9958), "System" },
                    { 3, 300000m, "FIN", new DateTime(2025, 5, 30, 14, 26, 12, 817, DateTimeKind.Utc).AddTicks(9960), "System", "Handles financial planning, accounting, and budget management", true, null, "Finance", new DateTime(2025, 5, 30, 14, 26, 12, 817, DateTimeKind.Utc).AddTicks(9960), "System" },
                    { 4, 250000m, "MKT", new DateTime(2025, 5, 30, 14, 26, 12, 817, DateTimeKind.Utc).AddTicks(9962), "System", "Responsible for marketing strategies and customer engagement", true, null, "Marketing", new DateTime(2025, 5, 30, 14, 26, 12, 817, DateTimeKind.Utc).AddTicks(9978), "System" }
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "Id", "Address", "CreatedAt", "CreatedBy", "DateOfBirth", "DepartmentId", "Email", "EmergencyContact", "EmployeeNumber", "FirstName", "HireDate", "IsActive", "LastName", "ManagerId", "Notes", "PhoneNumber", "Position", "ProfileImagePath", "Salary", "TerminationDate", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, "123 Main St, City, State 12345", new DateTime(2025, 5, 30, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(6180), "System", new DateTime(1995, 5, 30, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(5875), 1, "john.doe@company.com", "Jane Doe - 555-0102", "EMP001", "John", new DateTime(2024, 5, 30, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(5617), true, "Doe", null, null, "+1-555-0101", "Software Engineer", null, 75000m, null, new DateTime(2025, 5, 30, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(6243), "System" },
                    { 2, "456 Oak Ave, City, State 12345", new DateTime(2025, 5, 30, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(6433), "System", new DateTime(1993, 5, 30, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(6432), 1, "jane.smith@company.com", "John Smith - 555-0104", "EMP002", "Jane", new DateTime(2024, 11, 11, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(6431), true, "Smith", null, null, "+1-555-0103", "Project Manager", null, 85000m, null, new DateTime(2025, 5, 30, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(6433), "System" },
                    { 3, "789 Pine St, City, State 12345", new DateTime(2025, 5, 30, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(6438), "System", new DateTime(1990, 5, 30, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(6437), 2, "bob.johnson@company.com", "Alice Johnson - 555-0106", "EMP003", "Bob", new DateTime(2024, 12, 31, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(6436), true, "Johnson", null, null, "+1-555-0105", "HR Manager", null, 70000m, null, new DateTime(2025, 5, 30, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(6438), "System" },
                    { 5, "654 Maple Ln, City, State 12345", new DateTime(2025, 5, 30, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(6524), "System", new DateTime(1999, 5, 30, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(6523), 3, "charlie.wilson@company.com", "Sarah Wilson - 555-0110", "EMP005", "Charlie", new DateTime(2025, 2, 19, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(6522), true, "Wilson", null, null, "+1-555-0109", "Financial Analyst", null, 60000m, null, new DateTime(2025, 5, 30, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(6524), "System" },
                    { 4, "321 Elm Dr, City, State 12345", new DateTime(2025, 5, 30, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(6519), "System", new DateTime(1997, 5, 30, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(6441), 1, "alice.brown@company.com", "Tom Brown - 555-0108", "EMP004", "Alice", new DateTime(2024, 1, 16, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(6440), true, "Brown", 2, null, "+1-555-0107", "Senior Developer", null, 95000m, null, new DateTime(2025, 5, 30, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(6520), "System" }
                });

            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "Id", "ActualCost", "Budget", "Code", "CreatedAt", "CreatedBy", "DepartmentId", "Description", "EndDate", "EstimatedEndDate", "Name", "Priority", "ProgressPercentage", "ProjectManagerId", "StartDate", "Status", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, 75000m, 150000m, "EPD2024", new DateTime(2025, 3, 31, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(8130), "System", 1, "Development of a comprehensive enterprise portal for internal operations", new DateTime(2025, 8, 28, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(7780), new DateTime(2025, 8, 23, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(7890), "Enterprise Portal Development", "High", 50, 2, new DateTime(2025, 3, 31, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(7711), "InProgress", new DateTime(2025, 5, 30, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(8192), "System" },
                    { 2, 10000m, 80000m, "HSM2024", new DateTime(2025, 4, 30, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(8379), "System", 2, "Modernization of the human resources management system", new DateTime(2025, 11, 26, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(8378), new DateTime(2025, 11, 21, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(8378), "HR System Modernization", "Medium", 10, 2, new DateTime(2025, 6, 29, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(8377), "Planning", new DateTime(2025, 5, 30, 14, 26, 12, 818, DateTimeKind.Utc).AddTicks(8380), "System" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action",
                table: "AuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EmployeeId",
                table: "AuditLogs",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityId",
                table: "AuditLogs",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType",
                table: "AuditLogs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_IsSuccessful",
                table: "AuditLogs",
                column: "IsSuccessful");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TableName",
                table: "AuditLogs",
                column: "TableName");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Timestamp",
                table: "AuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Code",
                table: "Departments",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_IsActive",
                table: "Departments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ManagerId",
                table: "Departments",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Name",
                table: "Departments",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeNumber",
                table: "Employees",
                column: "EmployeeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_HireDate",
                table: "Employees",
                column: "HireDate");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_IsActive",
                table: "Employees",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ManagerId",
                table: "Employees",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAssignments_AssignedDate",
                table: "ProjectAssignments",
                column: "AssignedDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAssignments_EmployeeId",
                table: "ProjectAssignments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAssignments_IsActive",
                table: "ProjectAssignments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAssignments_Project_Employee",
                table: "ProjectAssignments",
                columns: new[] { "ProjectId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAssignments_ProjectId",
                table: "ProjectAssignments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMilestones_DueDate",
                table: "ProjectMilestones",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMilestones_IsCompleted",
                table: "ProjectMilestones",
                column: "IsCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMilestones_IsCritical",
                table: "ProjectMilestones",
                column: "IsCritical");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMilestones_ProjectId",
                table: "ProjectMilestones",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Code",
                table: "Projects",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_DepartmentId",
                table: "Projects",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_EstimatedEndDate",
                table: "Projects",
                column: "EstimatedEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Priority",
                table: "Projects",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectManagerId",
                table: "Projects",
                column: "ProjectManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_StartDate",
                table: "Projects",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Status",
                table: "Projects",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Employees_EmployeeId",
                table: "AuditLogs",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Employees_ManagerId",
                table: "Departments",
                column: "ManagerId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Employees_ManagerId",
                table: "Departments");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ProjectAssignments");

            migrationBuilder.DropTable(
                name: "ProjectMilestones");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
