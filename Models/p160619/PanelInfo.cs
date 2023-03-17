using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace TAPI_Interviewer.Models.p160619
{
    public class ProgressCount
    {
        public string All { get; set; }
        public string Ing { get; set; }
        public string End { get; set; }
    }

    public class PanelList
    {
        public string PID { get; set; }
        public string name { get; set; }
        public string sex { get; set; }
        public string age { get; set; }
        public string disability { get; set; }
        public string disability_g { get; set; }
        public string rehab { get; set; }
        public string rehab_count { get; set; }
        public string address { get; set; }
        public string address1 { get; set; }
        public string address2_4 { get; set; }
        public string tel_h { get; set; }
        public string tel_p { get; set; }
        public string email { get; set; }
        public string pre_survey { get; set; }
        public string status { get; set; }
        public string contactCount { get; set; }
        public string lastContact { get; set; }
        public string distributionDate { get; set; }
        public string startDate { get; set; }
        public string quota { get; set; }
        public string etc1 { get; set; }

        public string new_address1_1 { get; set; }
        public string new_address1_2 { get; set; }
        public string new_address1_3 { get; set; }
        public string new_address2_1 { get; set; }
        public string new_address2_2 { get; set; }
        public string new_address2_3 { get; set; }
        public string new_tel_h1 { get; set; }
        public string new_tel_h2 { get; set; }
        public string new_tel_h3 { get; set; }
        public string new_tel_p1 { get; set; }
        public string new_tel_p2 { get; set; }
        public string new_tel_p3 { get; set; }
    }

    public class AnswerList
    {
        public string Variable { get; set; }
        public string Title { get; set; }
        public string Answer { get; set; }
        public string Url { get; set; }
    }
}