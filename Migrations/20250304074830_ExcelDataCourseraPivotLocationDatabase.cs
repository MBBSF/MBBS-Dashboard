using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MBBS.Dashboard.web.Migrations
{
    /// <inheritdoc />
    public partial class ExcelDataCourseraPivotLocationDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExcelDataCourseraPivotLocationCityReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationCity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentMembers = table.Column<int>(type: "int", nullable: true),
                    CurrentLearners = table.Column<int>(type: "int", nullable: true),
                    TotalEnrollments = table.Column<int>(type: "int", nullable: true),
                    TotalCompletedCourses = table.Column<int>(type: "int", nullable: true),
                    AverageProgress = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalEstimatedLearningHours = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AverageEstimatedLearningHours = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeletedMembers = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcelDataCourseraPivotLocationCityReports", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExcelDataCourseraPivotLocationCityReports");
        }
    }
}
