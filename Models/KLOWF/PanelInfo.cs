using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace TAPI_Interviewer.Models.KLOWF
{
    public class ProgressCount
    {
        public string All { get; set; }
        public string Ing { get; set; }
        public string End { get; set; }
    }

    public class HouseInfo
    {
        public string hid { get; set; }
        public string hname { get; set; }
        public string iname { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public string houseStatus { get; set; }
        public string email { get; set; }

        public List<PersonInfo> PersonList { get; set; }
    }

    public class PersonInfo
    {
        public string pid { get; set; }
        public string pname { get; set; }
        public string age { get; set; }
        public string gender { get; set; }
        public string type { get; set; }
        public string personStatus { get; set; }
        public string psurveyStatus { get; set; }
        public string status { get; set; }
        public string email { get; set; }
        public string personURL { get; set; }
        public string psurveyURL { get; set; }
    }

    public class AnswerList
    {
        public string Variable { get; set; }
        public string Title { get; set; }
        public string Answer { get; set; }
        public string Url { get; set; }
    }
}