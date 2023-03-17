using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TAPI_Interviewer.Models.Project
{
    public class AnswerInfo
    {
        public string PID { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
    }

    public class AnswerList
    {
        public string Variable { get; set; }
        public string Title { get; set; }
        public string Answer { get; set; }
        public string Url { get; set; }
    }
}