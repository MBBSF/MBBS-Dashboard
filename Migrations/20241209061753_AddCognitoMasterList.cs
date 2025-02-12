using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MBBS.Dashboard.web.Migrations
{
    /// <inheritdoc />
    public partial class AddCognitoMasterList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExcelDataCognitoMasterList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name_First = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name_Middle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name_Last = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: true),
                    Address_Line1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Line2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IntendedMajor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedGraduation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CollegePlanToAttend_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CollegePlanToAttend_CityState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HighSchoolCollegeData_CurrentStudent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HighSchoolCollegeData_HighSchoolCollegeInformation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HighSchoolCollegeData_Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HighSchoolCollegeData_HighSchoolGraduation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HighSchoolCollegeData_CumulativeGPA = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    HighSchoolCollegeData_ACTCompositeScore = table.Column<int>(type: "int", nullable: true),
                    HighSchoolCollegeData_SATCompositeScore = table.Column<int>(type: "int", nullable: true),
                    HighSchoolCollegeData_SchoolCommunityRelatedActivities = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HighSchoolCollegeData_HonorsAndSpecialRecognition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HighSchoolCollegeData_ExplainYourNeedForAssistance = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WriteYourNameAsFormOfSignature = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Entry_Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Entry_DateCreated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Entry_DateSubmitted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Entry_DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExcelDataCognitoMasterList", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExcelDataCognitoMasterList");
        }
    }
}
