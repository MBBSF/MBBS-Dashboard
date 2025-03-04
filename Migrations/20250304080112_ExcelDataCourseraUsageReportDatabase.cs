using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MBBS.Dashboard.web.Migrations
{
    /// <inheritdoc />
    public partial class ExcelDataCourseraUsageReportDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExcelDataCourseraUsageReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Course = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseSlug = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    University = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnrollmentTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClassStartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClassEndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastCourseActivityTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OverallProgress = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EstimatedLearningHours = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Completed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RemovedFromProgram = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProgramSlug = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProgramName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnrollmentSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CourseGrade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseCertificateURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ForCredit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocationCity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocationRegion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocationCountry = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcelDataCourseraUsageReports", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExcelDataCourseraUsageReports");
        }
    }
}
