using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MBBS.Dashboard.web.Models
{
    public class ExcelDataCognitoMasterList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MARIEBARNEYBOSTONSCHOLARSHIPFOU_Id { get; set; }
        public string Name_First { get; set; }
        public string Name_Middle { get; set; }
        public string Name_Last { get; set; }
        public string Phone { get; set; }
        public int? Age { get; set; }
        public string Address_Line1 { get; set; }
        public string Address_Line2 { get; set; }
        public string Address_City { get; set; }
        public string Address_State { get; set; }
        public string Address_PostalCode { get; set; }
        public string IntendedMajor { get; set; }
        public string ExpectedGraduation { get; set; }
        public string CollegePlanToAttend_Name { get; set; }
        public string CollegePlanToAttend_CityState { get; set; }
        public string HighSchoolCollegeData_CurrentStudent { get; set; }
        public string HighSchoolCollegeData_HighSchoolCollegeInformation { get; set; }
        public string HighSchoolCollegeData_Phone { get; set; }
        public DateTime? HighSchoolCollegeData_HighSchoolGraduation { get; set; }
        [Precision(5, 2)]
        public decimal? HighSchoolCollegeData_CumulativeGPA { get; set; }
        public int? HighSchoolCollegeData_ACTCompositeScore { get; set; }
        public int? HighSchoolCollegeData_SATCompositeScore { get; set; }
        public string HighSchoolCollegeData_SchoolCommunityRelatedActivities { get; set; }
        public string HighSchoolCollegeData_HonorsAndSpecialRecognition { get; set; }
        public string HighSchoolCollegeData_ExplainYourNeedForAssistance { get; set; }
        public string WriteYourNameAsFormOfSignature { get; set; }
        public DateTime? Date { get; set; }
        public string Entry_Status { get; set; }
        public DateTime? Entry_DateCreated { get; set; }
        public DateTime? Entry_DateSubmitted { get; set; }
        public DateTime? Entry_DateUpdated { get; set; }
    }
}