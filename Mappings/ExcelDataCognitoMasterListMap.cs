using CsvHelper.Configuration;

namespace FirstIterationProductRelease.Models
{
    public class ExcelDataCognitoMasterListMap : ClassMap<ExcelDataCognitoMasterList>
    {
        public ExcelDataCognitoMasterListMap()
        {
            Map(m => m.MARIEBARNEYBOSTONSCHOLARSHIPFOU_Id).Name("MARIEBARNEYBOSTONSCHOLARSHIPFOU_Id");
            Map(m => m.Name_First).Name("Name_First");
            Map(m => m.Name_Middle).Name("Name_Middle");
            Map(m => m.Name_Last).Name("Name_Last");
            Map(m => m.Phone).Name("Phone");
            Map(m => m.Age).Name("Age");
            Map(m => m.Address_Line1).Name("Address_Line1");
            Map(m => m.Address_Line2).Name("Address_Line2");
            Map(m => m.Address_City).Name("Address_City");
            Map(m => m.Address_State).Name("Address_State");
            Map(m => m.Address_PostalCode).Name("Address_PostalCode");
            Map(m => m.IntendedMajor).Name("IntendedMajor");
            Map(m => m.ExpectedGraduation).Name("ExpectedGraduation");
            Map(m => m.CollegePlanToAttend_Name).Name("CollegePlanToAttend_Name");
            Map(m => m.CollegePlanToAttend_CityState).Name("CollegePlanToAttend_CityState");
            Map(m => m.HighSchoolCollegeData_CurrentStudent).Name("HighSchoolCollegeData_CurrentStudent");
            Map(m => m.HighSchoolCollegeData_HighSchoolCollegeInformation).Name("HighSchoolCollegeData_HighSchoolCollegeInformation");
            Map(m => m.HighSchoolCollegeData_Phone).Name("HighSchoolCollegeData_Phone");
            Map(m => m.HighSchoolCollegeData_HighSchoolGraduation).Name("HighSchoolCollegeData_HighSchoolGraduation");
            Map(m => m.HighSchoolCollegeData_CumulativeGPA).Name("HighSchoolCollegeData_CumulativeGPA");
            Map(m => m.HighSchoolCollegeData_ACTCompositeScore).Name("HighSchoolCollegeData_ACTCompositeScore");
            Map(m => m.HighSchoolCollegeData_SATCompositeScore).Name("HighSchoolCollegeData_SATCompositeScore");
            Map(m => m.HighSchoolCollegeData_SchoolCommunityRelatedActivities).Name("HighSchoolCollegeData_SchoolCommunityRelatedActivities");
            Map(m => m.HighSchoolCollegeData_HonorsAndSpecialRecognition).Name("HighSchoolCollegeData_HonorsAndSpecialRecognition");
            Map(m => m.HighSchoolCollegeData_ExplainYourNeedForAssistance).Name("HighSchoolCollegeData_ExplainYourNeedForAssistance");
            Map(m => m.WriteYourNameAsFormOfSignature).Name("WriteYourNameAsFormOfSignature");
            Map(m => m.Date).Name("Date");
            Map(m => m.Entry_Status).Name("Entry_Status");
            Map(m => m.Entry_DateCreated).Name("Entry_DateCreated");
            Map(m => m.Entry_DateSubmitted).Name("Entry_DateSubmitted");
            Map(m => m.Entry_DateUpdated).Name("Entry_DateUpdated");
        }
    }
}