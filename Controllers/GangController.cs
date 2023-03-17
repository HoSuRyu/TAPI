using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TAPI_Interviewer.Helpers;
using TAPI_Interviewer.Models;
using TAPI_Interviewer.Models.Project;
using System.IO;
using System.Xml.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TAPI_Interviewer.Controllers
{
    public class GangController : Controller
    {
        private string UID
        {
            get
            {
                string value = HttpContext.Session["HRC_TAPI_UID"] as string;

                //if (Request.Cookies["HRC_TAPI"] != null)
                //{
                //    value = Request.Cookies["HRC_TAPI"]["UID"];
                //}

                return value;
            }
        }

        public ActionResult List(string PN, decimal pid)
        {
            string backButton = "";
            string lang = "";
            
            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_Questionnaire t_Questionnaire = SURVEYTOOL.T_Questionnaires.Where(w => w.questionnaireNumber == PN).FirstOrDefault();

                if (t_Questionnaire != null)
                {
                    backButton = t_Questionnaire.editYN == 1 ? "&b=y" : "";
                    lang = string.IsNullOrEmpty(t_Questionnaire.surveySequenceNumber) == false ? "&lang=" + t_Questionnaire.surveySequenceNumber : "";
                }
            }

            pid = pid - 9000000000;

            ViewBag.C = TripleDESCryptoService.Encrypt(string.Format("qn={0}&pid={1}&p=y{2}{3}", PN, pid, backButton, lang));

            return View("List");
        }
    }
}