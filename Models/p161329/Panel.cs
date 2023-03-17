using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TAPI_Interviewer.Models.p161329
{
    public class Panel
    {
        public string PanelCode { get; set; }
        public string PanelName { get; set; }
        public string Owner { get; set; }
        public string Address { get; set; }
        public string SidoLabel { get; set; }
        public int? SidoCode { get; set; }
        public string SigunguLabel { get; set; }
        public int? SigunguCode { get; set; }
        public string DongLabel { get; set; }
        public string DetailAddress { get; set; }
        public string Phone1 { get; set; }
    }

    public class AnswerList
    {
        public string Variable { get; set; }
        public string Title { get; set; }
        public string Answer { get; set; }
        public string Url { get; set; }
    }
}