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
using System.Threading;
using System.Globalization;

namespace TAPI_Interviewer.Controllers
{
    public class ProjectController : Controller
    {
        private string UID
        {
            get
            {
                string value = HttpContext.Session["HRC_TAPI_UID"] as string;
                
                return value;
            }
            set
            {
               
                HttpContext.Session["HRC_TAPI_UID"] = value;
                HttpContext.Session.Timeout = 720;
            }
        }

        private string LastUID
        {
            get
            {
                string value = "";

                if (Request.Cookies["HRC_TAPI_INFO"] != null)
                {
                    value = Request.Cookies["HRC_TAPI_INFO"]["LastUID"];
                }

                return value;
            }
        }

        private string LANG
        {
            get
            {
                string value = HttpContext.Session["HRC_TAPI_Lang"] as string;
                if (string.IsNullOrEmpty(value) == true)
                {
                    return "";
                }
                return value;
            }
            set
            {
                HttpContext.Session["HRC_TAPI_Lang"] = value;
                HttpContext.Session.Timeout = 720;
            }
        }

        public ProjectInfo GetProjectInfo(string projectNumber, bool isLogin = true)
        {
            ProjectInfo projectInfo = new ProjectInfo();
            projectInfo.Number = projectNumber;

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_Questionnaire t_Questionnaire = SURVEYTOOL.T_Questionnaires.Where(w => w.questionnaireNumber == projectNumber).FirstOrDefault();

                if (t_Questionnaire != null)
                {
                    projectInfo.QuestionnaireID = t_Questionnaire.id;
                    projectInfo.Name = t_Questionnaire.questionnaireName;
                    projectInfo.IsEdit = t_Questionnaire.editYN == 1;
                }

                T_Interviewer t_Interviewer = SURVEYTOOL.T_Interviewers.Where(w => w.employeeNumber.ToString() == UID).FirstOrDefault();

                if (t_Interviewer != null)
                {
                    projectInfo.InterviewerName = t_Interviewer.userName;
                }

                if (isLogin == true)
                {
                    T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber.ToString() == UID && w.questionnaireID == projectInfo.QuestionnaireID).FirstOrDefault();

                    if (t_QuestionnaireDistribution != null)
                    {
                        t_QuestionnaireDistribution.sendDateTime = DateTime.Now;

                        SURVEYTOOL.SubmitChanges();
                    }
                }
            }

            projectInfo.AnswerList = GetList(projectNumber);

            return projectInfo;
        }

        public List<AnswerInfo> GetList(string projectNumber)
        {
            Dictionary<decimal?, string> dicStateCodeToLabel = new Dictionary<decimal?, string>();
            dicStateCodeToLabel[0] = "";
            dicStateCodeToLabel[1] = Languages.Languages.code16;
            dicStateCodeToLabel[2] = Languages.Languages.code17;
            dicStateCodeToLabel[3] = Languages.Languages.code18;
            dicStateCodeToLabel[4] = Languages.Languages.code19;
            dicStateCodeToLabel[5] = Languages.Languages.code20;
            dicStateCodeToLabel[6] = Languages.Languages.code21;

            List<string> statusList = new List<string>();

            statusList = dicStateCodeToLabel.Values.ToList();

            ViewBag.StatusList = statusList;

            using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                List<AnswerInfo> t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_{0} WHERE uid = '{1}'", projectNumber, UID)).Select(s => {
                    string status = Languages.Languages.code21;

                    if (dicStateCodeToLabel.ContainsKey(s.answerStateCode ?? 6) == true)
                    {
                        status = dicStateCodeToLabel[s.answerStateCode ?? 6];
                    }

                    return new AnswerInfo() { PID = s.PID, Name = s.name, Status = status };
                }).ToList();

                return t_SamplingList;
            }
        }

        public ActionResult Index(string PN, string lang="")
        {
            if (string.IsNullOrEmpty(UID) == true)
            {
                return RedirectToAction("Login", "Home");
            }
            LANG = "";
            if (lang != "")
            {
                LANG = lang;
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(lang);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(lang);
            }

            ViewBag.lang = lang;
            return View(GetProjectInfo(PN));
        }

        public ActionResult Survey(string projectNumber, string name, string pid, bool isEdit = false)
        {
            if (string.IsNullOrEmpty(UID) == true)
            {
                return RedirectToAction("Login", "Home");
            }

            if (LANG != "")
            {
                string lang = LANG;

                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(lang);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(lang);

            }

            string eqnum = "";
            string nv = "";

            using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                if (pid == "0")
                {
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_{0} WHERE uid = '{1}' and name = '{2}'", projectNumber, UID, name)).FirstOrDefault();

                    if (t_SamplingList == null)
                    {
                        pid = DateTime.Now.ToString("MddHHmmss") + Regex.Replace(Guid.NewGuid().ToString(), @"\D", "").Substring(0, 4);

                        string insert = string.Format(@"INSERT INTO T_SamplingList_{0} (PID, name, answerStateCode) VALUES ('{1}', '{2}', 0)", projectNumber, pid, name, UID);

                        SURVEY.ExecuteCommand(insert);
                    }
                    else
                    {
                        ViewBag.Error = Languages.Languages.code15;
                    }
                }

                SURVEY.SubmitChanges();
            }
            
            ProjectInfo projectInfo = new ProjectInfo();
            projectInfo.Number = projectNumber;

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_Questionnaire t_Questionnaire = SURVEYTOOL.T_Questionnaires.Where(w => w.questionnaireNumber == projectNumber).FirstOrDefault();

                if (t_Questionnaire != null)
                {
                    projectInfo.QuestionnaireID = t_Questionnaire.id;
                    projectInfo.IsViewCode = !(t_Questionnaire.viewCodeYN == 0);
                }

                //if (projectInfo.IsViewCode == false)
                //{
                //    nv = "&nv=y";
                //}

                if (isEdit == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt(string.Format("qn={0}&p=y&pid={1}&uid={2}{3}{4}", projectNumber, pid, UID, eqnum, nv));

                    return View("Survey", projectInfo);
                }
                else
                {
                    List<AnswerList> answerList = new List<AnswerList>();

                    try
                    {
                        string dataFile = string.Format(@"{0}{1}\{2}.xml", Server.MapPath("../").Replace(@"TAPI", @"DATA\"), projectInfo.QuestionnaireID, pid);
                        

                        StreamReader srAnswerData = null;
                        XElement AnswerData = null;
                        if (System.IO.File.Exists(dataFile) == true)
                        {
                            srAnswerData = new StreamReader(dataFile, Encoding.Default);
                            AnswerData = XElement.Load(srAnswerData);
                        }
                        else
                        {
                            dataFile = string.Format(@"{0}{1}\{2}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\"), projectInfo.QuestionnaireID, pid);                            
                            srAnswerData = new StreamReader(dataFile, Encoding.Default);
                            string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());

                            AnswerData = XElement.Parse(answerDataDecrypt);
                        }



                        srAnswerData.Close();
                        srAnswerData.Dispose();

                        T_Script t_Script = SURVEYTOOL.T_Scripts.Where(w => w.questionnaireID == projectInfo.QuestionnaireID).First();

                        if (t_Script != null)
                        {
                            XElement variableList = t_Script.variableList;

                            answerList = (from element in variableList.Elements()
                                          select new AnswerList
                                          {
                                              Variable = element.Name.ToString(),
                                              Title = element.Value,
                                              Answer = GetAnswerData(AnswerData, element.Name.ToString()),
                                              Url = TripleDESCryptoService.Encrypt(string.Format("qn={0}&p=y&pid={1}&uid={2}{3}&eqnum={4}", projectNumber, pid, UID, nv, element.Name.ToString())),
                                          }).Where(w => string.IsNullOrEmpty(w.Answer) == false).ToList();
                        }
                    }
                    catch (Exception ee)
                    {
                        return View("Error", (object)ee.Message);
                    }

                    return View("EditSurvey", answerList);
                }
            }
        }

        public string GetAnswerData(XElement answerData, string variable)
        {
            XElement xValue = (from element in answerData.Elements()
                               where (element.Name.ToString().Equals(variable) == true ||
                                      element.Name.ToString().StartsWith(variable + "_") == true) &&
                                     element.Attribute("Enable") != null &&
                                     element.Attribute("Enable").Value.Equals("true", StringComparison.OrdinalIgnoreCase) == true
                               select element).FirstOrDefault();

            if (xValue == null)
            {
                xValue = (from element in answerData.Elements()
                          where element.Elements().Where(p => p.Name.ToString().Equals(variable)).Count() > 0
                          select element).FirstOrDefault();

                if (xValue != null)
                {
                    xValue = xValue.Elements().Where(p => p.Name.ToString().Equals(variable)).FirstOrDefault();
                }
            }

            string value = "";

            if (xValue != null)
            {
                value = string.Format("{0} {1}", xValue.Attribute("LABEL") == null ? "" : xValue.Attribute("LABEL").Value, xValue.Attribute("OPEN") == null ? "" : xValue.Attribute("OPEN").Value).Trim();

                if (string.IsNullOrEmpty(value) == true)
                {
                    foreach (XElement element in xValue.Elements())
                    {
                        value += string.Format("{0} {1}", element.Attribute("LABEL") == null ? "" : element.Attribute("LABEL").Value, element.Attribute("OPEN") == null ? "" : element.Attribute("OPEN").Value).Trim();
                        value += "///";
                    }

                    value = value.Trim(new char[] { '/' });
                }

                if (string.IsNullOrEmpty(value) == true)
                {
                    value = "응답값 확인";
                }
            }

            value = Regex.Replace(value, @"<(/)?([a-zA-Z]*)(\s[a-zA-Z]*=[^>]*)?(\s)*(/)?>", string.Empty);

            return value;
        }

        public ActionResult ListPartial(string projectNumber)
        {
            return PartialView("ListPartial", GetProjectInfo(projectNumber));
        }

        public ActionResult OpenProgress(string QN)
        {
            if (string.IsNullOrEmpty(UID) == true)
            {
                return RedirectToAction("Login", "Home");
            }

            string url = "https://rpssurvey.hrcglobal.com/Silsa/Progress?c=" + TripleDESCryptoService.Encrypt(string.Format("QN={0}&nuid={1}", QN, UID));

            return View((object)url);
        }
    }
}
