using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TAPI_Interviewer.Models;
using TAPI_Interviewer.Models.Manager;

namespace TAPI_Interviewer.Controllers
{
    public class ManagerController : Controller
    {
        private string Employee
        {
            get
            {
                string value = "";

                if (Request.Cookies["HRC_TAPI_INFO"] != null)
                {
                    value = Request.Cookies["HRC_TAPI_INFO"]["Employee"];
                }

                return value;
            }
            set
            {
                HttpCookie cookie;

                if (Request.Cookies["HRC_TAPI_INFO"] != null)
                {
                    cookie = Request.Cookies["HRC_TAPI_INFO"];
                }
                else
                {
                    cookie = new HttpCookie("HRC_TAPI_INFO");
                }

                cookie.Values["Employee"] = value;
                cookie.Expires = DateTime.Now.AddDays(1);
                Response.AppendCookie(cookie);
            }
        }

        public ActionResult Index()
        {
            return View("Login");
        }

        public ActionResult Login()
        {
            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                ViewBag.Error = "";

                string uid = string.Format("{0}", Request["uid"]);
                string password = string.Format("{0}", Request["password"]);

                ViewBag.Uid = uid;
                ViewBag.Password = password;

                if (string.IsNullOrEmpty(uid) == true || string.IsNullOrEmpty(password) == true)
                {
                    ViewBag.Error = "이름/사원번호를 모두 입력 해 주세요.";

                    return View();
                }

                SURVEYTOOL.GET_RPS_LIVE_T_Employee();

                T_EmployeeInfo t_EmployeeInfo = SURVEYTOOL.T_EmployeeInfo.Where(w => w.이름 == uid && w.사원번호 == password).FirstOrDefault();

                if (t_EmployeeInfo == null)
                {
                    ViewBag.Error = "이름/사원번호를 다시 확인하세요.";

                    return View();
                }

                LogoutCookie();

                Employee = password;

                return View("Interviewer", GetList());
            }
        }

        private void LogoutCookie()
        {
            Employee = "";
        }

        public ActionResult Logout()
        {
            LogoutCookie();

            return View("Login");
        }

        public ActionResult Interviewer()
        {
            if (Employee == "")
            {
                return View("Login");
            }

            return View("Interviewer", GetList());
        }

        public ActionResult InterviewerPartial(string projectNumber)
        {
            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                List<GET_InterviewerInfoResult> InterviewerInfo = SURVEYTOOL.GET_InterviewerInfo(SURVEYTOOL.T_Questionnaires.First(f => f.questionnaireNumber == projectNumber).id).ToList();

                return PartialView("InterviewerPartial", InterviewerInfo);
            }
        }

        public Info GetList()
        {
            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_EmployeeInfo t_EmployeeInfo = SURVEYTOOL.T_EmployeeInfo.Where(w => w.사원번호 == Employee).FirstOrDefault();

                Info info = new Info();

                info.ProjectList = SURVEYTOOL.T_Questionnaires.Where(w => w.questionnaireType.Contains("TAPI") == true && w.questionnaireType.Contains("공용") && w.deleteYN == 0 && w.fieldWorkYN == 0)
                                                              .Select(s => $"{s.questionnaireNumber}:{s.questionnaireName}").ToList();

                info.T_EmployeeInfoList = SURVEYTOOL.T_EmployeeInfo.Where(w => w.부서 == t_EmployeeInfo.부서).ToList();
                                
                return info;
            }
        }

        public ActionResult AddInterviewer(string project, string employee, string uidList)
        {
            string projectNumber = project.Split(':')[0];

            try
            {
                using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    T_EmployeeInfo t_EmployeeInfo = SURVEYTOOL.T_EmployeeInfo.Where(w => w.사원번호 == employee).FirstOrDefault();
                    decimal qid = SURVEYTOOL.T_Questionnaires.First(f => f.questionnaireNumber == projectNumber).id;
                    List<T_QuestionnaireDistribution> t_QuestionnaireDistributionList = new List<T_QuestionnaireDistribution>();

                    foreach (string uid in uidList.Split('\n'))
                    {
                        decimal dUid = 0;

                        if (decimal.TryParse(uid.Trim(), out dUid) == true)
                        {
                            if (dUid > 0 && 
                                SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == dUid && w.questionnaireID == qid).Count() == 0 &&
                                t_QuestionnaireDistributionList.Where(w => w.employeeNumber == dUid).Count() == 0)
                            {
                                T_QuestionnaireDistribution t_QuestionnaireDistribution = new T_QuestionnaireDistribution();

                                t_QuestionnaireDistribution.employeeNumber = dUid;
                                t_QuestionnaireDistribution.questionnaireID = qid;
                                t_QuestionnaireDistribution.deleteYN = 0;
                                t_QuestionnaireDistribution.createDateTime = DateTime.Now;
                                t_QuestionnaireDistribution.updateDateTime = DateTime.Now;
                                t_QuestionnaireDistribution.office = $"{t_EmployeeInfo.부서.Replace(" ", "")}({t_EmployeeInfo.이름})";
                                t_QuestionnaireDistribution.sv_name = $"{t_EmployeeInfo.이름}";
                                t_QuestionnaireDistribution.testYN = 0;
                                t_QuestionnaireDistribution.etc1 = $"{t_EmployeeInfo.사원번호}";

                                t_QuestionnaireDistributionList.Add(t_QuestionnaireDistribution);
                            }
                        }
                    }

                    SURVEYTOOL.T_QuestionnaireDistribution.InsertAllOnSubmit(t_QuestionnaireDistributionList);
                    SURVEYTOOL.SubmitChanges();
                }

                ViewBag.OK = "완료되었습니다.";
            }
            catch (Exception ee)
            {
                ViewBag.OK = "오류가 발생 했습니다." + ee.Message;
            }

            return InterviewerPartial(projectNumber);
        }

        public ActionResult SelectProject(string project)
        {
            string projectNumber = project.Split(':')[0];

            return InterviewerPartial(projectNumber);
        }
    }
}