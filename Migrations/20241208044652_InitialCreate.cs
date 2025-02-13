using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MBBS.Dashboard.web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Listings");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropColumn(
                name: "AppliedReferal",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "CreditedFunds",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "ReferalCode",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "cardNumber",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "cvc",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "expirationDate",
                table: "Accounts",
                newName: "UserRole");

            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExcelDataCourseraSpecialization",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Specialization = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpecializationSlug = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    University = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnrollmentTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSpecializationActivityTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedCourses = table.Column<int>(type: "int", nullable: true),
                    CoursesInSpecialization = table.Column<int>(type: "int", nullable: true),
                    Completed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RemovedFromProgram = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProgramSlug = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProgramName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnrollmentSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpecializationCompletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SpecializationCertificateURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocationCity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocationRegion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LocationCountry = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcelDataCourseraSpecialization", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "ExcelDataCourseraSpecialization");

            migrationBuilder.RenameColumn(
                name: "UserRole",
                table: "Accounts",
                newName: "expirationDate");

            migrationBuilder.AddColumn<string>(
                name: "AppliedReferal",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CreditedFunds",
                table: "Accounts",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "ReferalCode",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cardNumber",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "cvc",
                table: "Accounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Listings",
                columns: table => new
                {
                    ListingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Picture = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Sold = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Listings", x => x.ListingID);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemPrice = table.Column<double>(type: "float", nullable: false),
                    Refunded = table.Column<bool>(type: "bit", nullable: false),
                    SellerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });
        }
    }
}
