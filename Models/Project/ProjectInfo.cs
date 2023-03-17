using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TAPI_Interviewer.Models.Project
{
    public class ProjectInfo
    {
        public decimal QuestionnaireID { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string InterviewerName { get; set; }
        public List<AnswerInfo> AnswerList { get; set; }
        public bool IsEdit { get; set; }
        public bool IsViewCode { get; set; }
    }
    public class ProgressCount
    {
        public string All { get; set; }
        public string Ing { get; set; }
        public string End { get; set; }
        public string End2 { get; set; }

        public string endTel { get; set;  }
        public string endDir { get; set; }

        public string endEtc { get; set; }
    }
}