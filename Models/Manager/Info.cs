using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TAPI_Interviewer.Models.Manager
{
    public class Info
    {
        public List<string> ProjectList { get; set; }
        public List<T_EmployeeInfo> T_EmployeeInfoList { get; set; }
    }
}