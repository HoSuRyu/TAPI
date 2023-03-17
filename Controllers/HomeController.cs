////////////////////////////////////////////////////////////
// 클래스 설명 : TAPI Main
// 최초 개발자 : 채수용
// 최초 개발일 : 2015-08-25
// 수정 이력
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TAPI_Interviewer.Helpers;
using TAPI_Interviewer.Models;
using TAPI_Interviewer.Models.Home;
using AES256;
using System.Text;
using System.Threading;
using System.Globalization;

namespace TAPI_Interviewer.Controllers
{
    public class HomeController : Controller
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

                cookie.Values["LastUID"] = value;
                cookie.Expires = DateTime.Now.AddDays(365);
                Response.AppendCookie(cookie);
            }
        }

        private string ProjectOther
        {
            get
            {
                string value = "";

                if (Request.Cookies["HRC_TAPI_INFO"] != null)
                {
                    value = Request.Cookies["HRC_TAPI_INFO"]["ProjectOther"];
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

                cookie.Values["ProjectOther"] = value;
                cookie.Expires = DateTime.Now.AddDays(365);
                Response.AppendCookie(cookie);
            }
        }

        private string GangUID
        {
            get
            {
                string value = HttpContext.Session["HRC_TAPI_GangUID"] as string;

                return value;
            }
            set
            {
                HttpContext.Session["HRC_TAPI_GangUID"] = value;
                HttpContext.Session.Timeout = 720;
            }
        }

        private string LANG
        {
            get
            {
                string value = HttpContext.Session["HRC_TAPI_Lang"] as string;
                if( string.IsNullOrEmpty(value)== true)
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
     
        public ActionResult Index()
        {
            LANG = "";
            if (Request["lang"] != null )
            {
                string lang = Request["lang"].ToString();

                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(lang);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(lang);

                LANG = lang;
                ViewBag.AR = string.Equals(lang, "ar", StringComparison.OrdinalIgnoreCase) == true ? "direction:rtl;" : "";
                
            }

            if (string.IsNullOrEmpty(ProjectOther) == false)
            {
                return RedirectToAction("Index", ProjectOther);
            }
            else
            {
                ViewBag.Uid = LastUID;

                return List();
            }
        }

        public ActionResult Login()
        {
            //if (string.IsNullOrEmpty(UID) == false)
            //{
            //    if (string.IsNullOrEmpty(ProjectOther) == false)
            //    {
            //        return RedirectToAction("Index", ProjectOther);
            //    }
            //    else
            //    {
            //        return View("List", GetList());
            //    }
            //}

            LogoutCookie();
            GangUID = "";
            ProjectOther = "";

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                ViewBag.Error = "";

                string uid = string.Format("{0}", Request["uid"]);
                string password = string.Format("{0}", Request["password"]);

                ViewBag.Uid = uid;
                ViewBag.Password = password;

               

                if (string.IsNullOrEmpty(uid) == true || string.IsNullOrEmpty(password) == true)
                {
                    if (string.IsNullOrEmpty(uid) == true)
                    {
                        ViewBag.Uid = LastUID;
                    }

                    ViewBag.Error = "아이디/비밀번호를 모두 입력 해 주세요.";

                    return View();
                }

                decimal dUid = -9;

                //Gang 로그인 체크 후 없으면 회사 면접원으로
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.etc1.ToString() == uid && w.etc2 == Crypto.encryptAES256(password)).OrderBy(o => o.createDateTime).OrderByDescending(o => o.createDateTime).FirstOrDefault();

                if (t_QuestionnaireDistribution != null)
                {
                    UID = GangUID = t_QuestionnaireDistribution.id.ToString();
                    LastUID = "";
                }
                else
                {
                    T_Interviewer t_Interviewer = SURVEYTOOL.T_Interviewers.Where(w => w.employeeNumber.ToString() == uid).FirstOrDefault();

                    if (t_Interviewer == null || t_Interviewer.rrnFront != Crypto.encryptAES256(password) )
                    {
                        //using (WebDiary_Horse2014DataContext WebDiary_Horse2014 = new WebDiary_Horse2014DataContext("Data Source=srv1.hrcglobal.com,15000;Initial Catalog=WebDiary_Horse2014;Persist Security Info=True;User ID=webifs;password=webifs1"))
                        //{
                        //    H_Member h_Member = WebDiary_Horse2014.H_Members.Where(w => w.MemberId == uid && w.MemberPwd == password).FirstOrDefault();

                        //    if (h_Member != null)
                        //    {
                        //        UID = ViewBag.Uid;
                        //        LastUID = UID;
                        //        ProjectOther = "p161329";

                        //        return RedirectToAction("Index", ProjectOther);
                        //    }
                        //}
                        if (Crypto.encryptAES256(password) != "XtPcVt2FBZoF9WwiyjUwRg==")
                        {
                            ViewBag.Error = "아이디 또는 비밀번호를 다시 확인하세요.";

                            return View();
                        }
                    }

                    LastUID = UID = ViewBag.Uid;
                    GangUID = "";

                    T_InterviewerLoginInfo t_InterviewerLoginInfo = new T_InterviewerLoginInfo();

                    t_InterviewerLoginInfo.employeeNumber = decimal.Parse(UID);
                    t_InterviewerLoginInfo.loginDateTime = DateTime.Now;

                    SURVEYTOOL.T_InterviewerLoginInfos.InsertOnSubmit(t_InterviewerLoginInfo);
                    SURVEYTOOL.SubmitChanges();
                }

                return List();
            }
        }

        private void LogoutCookie()
        {
            UID = "";
        }

        public ActionResult Logout()
        {
            LogoutCookie();

            ViewBag.Uid = LastUID;

            return View("Login");
        }

        public ActionResult List()
        {
            if (string.IsNullOrEmpty(UID) == true)
            {
                ViewBag.Uid = LastUID;

                return View("Login");
            }

            if (LANG != "")
            {
                string lang = LANG;

                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(lang);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(lang);
                
            }
            return View("List", GetList());
        }

        public ActionResult ListPartial()
        {
            if (string.IsNullOrEmpty(UID) == true)
            {
                ViewBag.Uid = LastUID;

                return View("Login");
            }

            return PartialView("ListPartial", GetList());
        }

        public List<GET_TAPIProjectResult> GetList()
        {
            if (Request.Url.ToString().ToLower().Contains("localhost:") == true)
            {
                ViewBag.URL = "";
            }
            else
            {
                ViewBag.URL = "";
            }

            if (LANG == "")
            {
                ViewBag.LANG = "";
            }
            else
            {
                ViewBag.LANG = LANG;
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
              //  string caption = "[{0}] 설문지를 선택해 주세요. (아래 설문제목을 터치해 주세요.)";
               // string captionEn = "[{0}] Please select a questionnaire";

                string caption = TAPI_Interviewer.Languages.Languages.code1;
                ViewBag.Logout = TAPI_Interviewer.Languages.Languages.code0;

                if (string.IsNullOrEmpty(GangUID) == false)
                {
                    T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.id.ToString() == GangUID).First();

                    List<GET_TAPIProjectResult> T_QuestionnaireList = T_QuestionnaireList = SURVEYTOOL.GET_TAPIProject(UID, 1, t_QuestionnaireDistribution.etc1, t_QuestionnaireDistribution.etc2).ToList(); ;
                    //List<TQuestionnaire> T_QuestionnaireList = (from questionnaire in SURVEYTOOL.T_Questionnaires
                    //                                            join distribution in SURVEYTOOL.T_QuestionnaireDistribution
                    //                                            on questionnaire.id equals distribution.questionnaireID
                    //                                            where questionnaire.questionnaireType.Contains("Gang") == true && distribution.etc1 == t_QuestionnaireDistribution.etc1 && distribution.etc2 == t_QuestionnaireDistribution.etc2 && questionnaire.deleteYN == 0 && distribution.deleteYN == 0 && questionnaire.fieldWorkYN == 0
                    //                                            select new TQuestionnaire
                    //                                            {
                    //                                                questionnaireName = questionnaire.questionnaireName,
                    //                                                questionnaireNumber = questionnaire.questionnaireNumber,
                    //                                                url = "Gang/List/?PN=" + questionnaire.questionnaireNumber + "&PID=" + distribution.employeeNumber,
                    //                                                lang = questionnaire.surveySequenceNumber
                    //                                            }).ToList();

                    ViewBag.Caption = string.Format(caption, t_QuestionnaireDistribution.etc1);

                    return T_QuestionnaireList;
                }
                else
                {
                    List<GET_TAPIProjectResult> T_QuestionnaireList = new List<GET_TAPIProjectResult>();

                    T_QuestionnaireList = SURVEYTOOL.GET_TAPIProject(UID, 0, null, null).ToList(); ;

                    //List<TQuestionnaire> T_QuestionnaireList = SURVEYTOOL.GET_TAPIProject(UID);

                    //List<TQuestionnaire> T_QuestionnaireList = (from questionnaire in SURVEYTOOL.T_Questionnaires
                    //                                            join distribution in SURVEYTOOL.T_QuestionnaireDistribution
                    //                                            on questionnaire.id equals distribution.questionnaireID                                                                                                                                
                    //                                            where questionnaire.questionnaireType.Contains("TAPI") == true && distribution.employeeNumber.ToString() == UID && questionnaire.deleteYN == 0 && distribution.deleteYN == 0 && questionnaire.fieldWorkYN == 0
                    //                                            select new TQuestionnaire
                    //                                            {
                    //                                                questionnaireName = questionnaire.questionnaireName,
                    //                                                questionnaireNumber = questionnaire.questionnaireNumber,
                    //                                                url = questionnaire.questionnaireType.Contains("전용") == true ? questionnaire.questionnaireNumber + "/List" : "Project/?PN=" + questionnaire.questionnaireNumber
                    //                                            }).ToList();

                    T_Interviewer t_Interviewer = SURVEYTOOL.T_Interviewers.Where(w => w.employeeNumber.ToString() == UID).FirstOrDefault();

                    if (t_Interviewer != null)
                    {
                        ViewBag.Caption = string.Format(caption, t_Interviewer.userName);
                    }

                    return T_QuestionnaireList;
                }
            }
        }

        public ActionResult Memo(string qn, string pid, string uid, string qnum)
        {
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_Memo t_Memo = TAPI.T_Memo.Where(w => w.questionnaireNumber == qn && w.PID == pid && w.uid == (uid ?? "") && w.sQuestionVariable == qnum).FirstOrDefault();

                if (t_Memo == null)
                {
                    t_Memo = new T_Memo();
                    t_Memo.questionnaireNumber = qn;
                    t_Memo.PID = pid;
                    t_Memo.uid = uid ?? "";
                    t_Memo.sQuestionVariable = qnum;
                }

                return View("Memo", t_Memo);
            }
        }

        public string SaveMemo(T_Memo t_Memo)
        {
            try
            {
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    if (t_Memo.id == 0)
                    {
                        t_Memo.createDateTime = DateTime.Now;
                        t_Memo.updateDateTime = DateTime.Now;
                        t_Memo.uid = t_Memo.uid ?? "";

                        TAPI.T_Memo.InsertOnSubmit(t_Memo);
                    }
                    else
                    {
                        T_Memo saveMemo = TAPI.T_Memo.Where(w => w.id == t_Memo.id).FirstOrDefault();
                        saveMemo.memo = t_Memo.memo;
                        saveMemo.updateDateTime = DateTime.Now;
                    }

                    TAPI.SubmitChanges();
                }
            }
            catch (Exception ee)
            {
                return "error:" + ee.Message;
            }

            return "ok:저장 되었습니다.";
        }
    }
}
