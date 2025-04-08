using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MBBS.Dashboard.web.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseAccountId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "ExcelDataGoogleFormsVolunteerProgram",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "ExcelDataCourseraUsageReports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "ExcelDataCourseraSpecialization",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "ExcelDataCourseraPivotLocationCityReports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "ExcelDataCourseraMembershipReports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "ExcelDataCognitoMasterList",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
