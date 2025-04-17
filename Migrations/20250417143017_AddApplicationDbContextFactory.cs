﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MBBS.Dashboard.web.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationDbContextFactory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "AverageProgress",
                table: "ExcelDataCourseraPivotLocationCityReports",
                type: "decimal(5,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "AverageProgress",
                table: "ExcelDataCourseraPivotLocationCityReports",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldNullable: true);
        }
    }
}
