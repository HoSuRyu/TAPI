using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TAPI_Interviewer.Models.Home
{
    public class TQuestionnaire
    {
        public string questionnaireNumber { get; set; }
        public string questionnaireName { get; set; }        
        public string url { get; set; }
        public string lang { get; set; }
    }
}