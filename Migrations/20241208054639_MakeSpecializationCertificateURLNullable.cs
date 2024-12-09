using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirstIterationProductRelease.Migrations
{
    /// <inheritdoc />
    public partial class MakeSpecializationCertificateURLNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SpecializationCertificateURL",
                table: "ExcelDataCourseraSpecialization",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SpecializationCertificateURL",
                table: "ExcelDataCourseraSpecialization",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
