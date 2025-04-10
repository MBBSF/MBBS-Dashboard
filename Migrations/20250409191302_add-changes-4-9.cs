using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MBBS.Dashboard.web.Migrations
{
    /// <inheritdoc />
    public partial class addchanges49 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MARIEBARNEYBOSTONSCHOLARSHIPFOU_Id",
                table: "ExcelDataCognitoMasterList",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MARIEBARNEYBOSTONSCHOLARSHIPFOU_Id",
                table: "ExcelDataCognitoMasterList");
        }
    }
}
