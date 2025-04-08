using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MBBS.Dashboard.web.Migrations
{
    /// <inheritdoc />
    public partial class BaslineMigration2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "ExcelDataGoogleFormsVolunteerProgram");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "ExcelDataCourseraUsageReports");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "ExcelDataCourseraSpecialization");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "ExcelDataCourseraPivotLocationCityReports");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "ExcelDataCourseraMembershipReports");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "ExcelDataCognitoMasterList");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
