using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace TAPI_Interviewer.Models.p160954
{
    public class ProgressCount
    {
        public string All { get; set; }
        public string Ing { get; set; }
        public string End { get; set; }
    }

    public class AnswerList
    {
        public string Variable { get; set; }
        public string Title { get; set; }
        public string Answer { get; set; }
        public string Url { get; set; }
    }
}