using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TAPI_Interviewer.Helpers;
using TAPI_Interviewer.Models;
using TAPI_Interviewer.Models.Project;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System.Net;
using System.Xml;
using System.Web;
using System.Threading;
using System.Data;

namespace TAPI_Interviewer.Controllers
{
    public class q221599Controller : Controller
    {
        private int UID
        {
            get
            {
                string value = HttpContext.Session["HRC_TAPI_UID"] as string;

                int uid = 0;

                int.TryParse(value, out uid);

                return uid;
            }
        }

        public List<SP_q221599_GETLISTResult> GetList(int keyID = 0)
        {
            List<SP_q221599_GETLISTResult> list = new List<SP_q221599_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221599_GETLIST(UID, keyID).ToList();
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 13361).FirstOrDefault();

                if (t_QuestionnaireDistribution != null)
                {
                    t_QuestionnaireDistribution.sendDateTime = DateTime.Now;

                    SURVEYTOOL.SubmitChanges();
                }
            }

            List<string> statusList = list.GroupBy(g => g.진행상태).Where(w => string.IsNullOrEmpty(w.Key) == false).Select(s => s.Key).ToList();

            statusList.Insert(0, "");

            ViewBag.StatusList = statusList;

            return list;
        }

        public ActionResult List()
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            return View("List", GetList());
        }

        public ActionResult ListPartial()
        {
            return PartialView("ListPartial", GetList());
        }

        public ActionResult ProgressPartial()
        {
            ProgressCount progressCount = new ProgressCount();

            progressCount.All = "0";
            progressCount.Ing = "0";
            progressCount.End = "0";

            List<SP_q221599_GETLISTResult> list = new List<SP_q221599_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221599_GETLIST(UID, 0).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "성공").Count().ToString();
            }

            return PartialView("ProgressPartial", progressCount);
        }

        public ActionResult AnswerInfo(int keyID)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            q221599_List Info;
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                Info = TAPI.q221599_List.First(f => f.KeyID == keyID);
            }
            return View("AnswerInfo", Info);

        }

        public string SaveInfo(int keyID, string group, string grade, string name, string tel, string hp, string whyModify)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q221599_List SurveyInfo = TAPI.q221599_List.First(f => f.KeyID == keyID);

                if (whyModify != "")
                {
                    SurveyInfo.소속부서 = group;
                    SurveyInfo.직급 = grade;
                    SurveyInfo.이름 = name;
                    SurveyInfo.연락처1 = tel;
                    SurveyInfo.연락처2 = hp;
                    SurveyInfo.수정이유 = whyModify;

                    TAPI.SubmitChanges();
                }

                string QMListData = $@"<RPS_SamplingLIST_INFO>
                                        <OPID VALUE=""{SurveyInfo.PID_}"" />
                                        <OTYPE VALUE=""{SurveyInfo.OTYPE}"" />                                        
                                        <PANSWER VALUE=""{SurveyInfo.PANSWER}"" />
                                        <ATYPE VALUE=""{SurveyInfo.ATYPE}"" />
                                        <A101 VALUE=""{SurveyInfo.A101}"" />
                                    </RPS_SamplingLIST_INFO>";


                string query = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q221599]') AND type in (N'U'))
BEGIN
    MERGE SURVEY.dbo.T_SamplingList_q221599 as SLIST
    USING
    (
        SELECT	'{0}' as PID
    ) as Answer on SLIST.PID = Answer.PID
    WHEN MATCHED THEN
	    UPDATE	SET	QMListData = N'{1}'
    WHEN NOT MATCHED THEN
	    INSERT (PID, QMListData)
	    VALUES (PID, N'{1}');
END", keyID, QMListData);
                TAPI.ExecuteCommand(query, "");
                TAPI.SubmitChanges();

            }

            return result;

        }


        public ActionResult Survey(int keyID)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                List<AnswerList> answerList = new List<AnswerList>();

                ViewBag.KeyID = keyID;

                q221599_List SurveyInfo = new q221599_List();

                bool eqnum = false;

                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    SurveyInfo = (from surveyInfo in TAPI.q221599_List
                                  where surveyInfo.KeyID == keyID
                                  select surveyInfo).SingleOrDefault();

                    if (SurveyInfo.진행상태 != null && SurveyInfo.진행상태 == "성공")
                    {
                        eqnum = true;

                    }

                    SurveyInfo.진행상태 = "진행중";
                    TAPI.SubmitChanges();
                }

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt(string.Format("qn=q221599&pid={0}&uid={1}&p=y", keyID, UID));
                }
                else
                {
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\13361").Replace("/q221599", ""), keyID);
                    

                    using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                    {
                        StreamReader srAnswerData = new StreamReader(dataFile, Encoding.Default);
                        string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());
                        XElement AnswerData = XElement.Parse(answerDataDecrypt);
                    
                        srAnswerData.Close();
                        srAnswerData.Dispose();

                        T_Script t_Script = SURVEYTOOL.T_Scripts.Where(w => w.questionnaireID == 13361).First();

                        if (t_Script != null)
                        {
                            XElement variableList = t_Script.variableList;

                            answerList = (from element in variableList.Elements()
                                          select new AnswerList
                                          {
                                              Variable = element.Name.ToString(),
                                              Title = element.Value,
                                              Answer = GetAnswerData(AnswerData, element.Name.ToString()),
                                              Url = TripleDESCryptoService.Encrypt(string.Format("qn=q221599&pid={0}&uid={1}&eqnum={2}&p=y", keyID, UID, element.Name.ToString())),
                                          }).Where(w => string.IsNullOrEmpty(w.Answer) == false).ToList();
                        }
                    }
                }

                return View("EditSurvey", answerList);
            }
            catch (Exception ee)
            {
                return View("Error", (object)(ee.Message));
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



        public ActionResult Contact(int keyID)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }


            SP_q221599_GETLISTResult sp_q221599_GETLISTResult = GetList(keyID).First();


            List<q221599_Contact> ContactList = new List<q221599_Contact>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {


                ContactList = (from contact in TAPI.q221599_Contact
                               where contact.KeyID == keyID
                               select contact).ToList();

                ViewBag.KeyID = sp_q221599_GETLISTResult.KeyID;
                ViewBag.PID = sp_q221599_GETLISTResult.PID;
                ViewBag.기관명 = sp_q221599_GETLISTResult.기관명;
                ViewBag.ATYPE = sp_q221599_GETLISTResult.ATYPETEXT;

                ViewBag.Ranking = ContactList.Count() + 1;


                ViewBag.contactYear = DateTime.Now.ToString("yyyy");
                ViewBag.contactMonth = DateTime.Now.ToString("MM");
                ViewBag.contactDay = DateTime.Now.ToString("dd");


                string[] timeArray = new string[] { "", "", "", "", "", "", "", "오전 7이전", "오전 7시~ 오전8시 이전", "오전 8시~ 오전9시 이전", "오전 9시~ 오전10시 이전", "오전 10시~ 오전11시 이전", "오전 11시~ 오전12시 이전", "오전 12시~ 오후1시 이전", "오후 1시~ 오후2시 이전", "오후2시~ 오후3시 이전", "오후 3시~ 오후4시 이전", "오후 4시~ 오후5시 이전", "오후 5시~ 오후6시 이전", "오후 6시~ 오후7시 이전", "오후 7시~ 오후8시 이전", "오후 8시~ 오후9시 이전", "오후 9시~ 오후10시 이전", "오후 10시~ 오후11시 이전", "오후 11시 이후" };


                ViewBag.contactTime = timeArray[24];

                int hh = DateTime.Now.Hour;
                if (hh < 7)
                {
                    ViewBag.contactTime = timeArray[7];
                }
                for (int i = 8; i < 24; i++)
                {
                    if (hh < i && hh >= i - 1)
                    {
                        ViewBag.contactTime = timeArray[i];
                    }
                }


                ViewBag.lastContact = sp_q221599_GETLISTResult.진행상태;
            }

            return View("Contact", ContactList);
        }


        public string ContactOK(int keyID, string PID, int ranking, string contactMonth, string contactDay, string contactTime,
            string status, string promiseMonth, string promiseDay, string promiseTime, string etc, string bigo)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            q221599_Contact Contact = new q221599_Contact();
            q221599_List SurveyInfo = new q221599_List();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                Contact.KeyID = keyID;
                Contact.PID = PID;
                Contact.면접원아이디 = UID;
                Contact.접촉횟수 = ranking;

                Contact.접촉일자 = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;
                Contact.접촉시간 = contactTime;
                Contact.접촉결과 = status;

                if (promiseMonth != "")
                {
                    Contact.약속일자 = $"{DateTime.Now.Year}-" + promiseMonth + "-" + promiseDay;
                }
                Contact.약속시간 = promiseTime;

                Contact.비성공세부사유 = etc;
                Contact.특이사항 = bigo;

                Contact.inputDate = DateTime.Now.ToString();


                TAPI.q221599_Contact.InsertOnSubmit(Contact);

                SurveyInfo = (from surveyInfo in TAPI.q221599_List
                              where surveyInfo.KeyID == keyID
                              select surveyInfo).SingleOrDefault();

                SurveyInfo.진행상태 = status;
                SurveyInfo.최종접촉결과 = status;

                if (promiseMonth == "")
                {
                    SurveyInfo.약속일자 = "";
                }
                else
                {
                    SurveyInfo.약속일자 = $"{DateTime.Now.Year}-" + promiseMonth + "-" + promiseDay;
                }

                TAPI.SubmitChanges();
            }

            return "저장 되었습니다.";
        }

        public ActionResult CreateList()
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            List<SP_q221599_GETOFFICELISTResult> list = new List<SP_q221599_GETOFFICELISTResult>();
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221599_GETOFFICELIST(UID).ToList();
            }
            
            return View("CreateList", list);
        }


        public string CreateOK(
            int PID, int atype, string addtype, string group, string grade, string name, string tel, string hp)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }


            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {


                int idx = 2 + atype;
                SP_q221599_GETOFFICEINFOResult sp_q221599_GETOFFICEINFOResult = new SP_q221599_GETOFFICEINFOResult();
                if (TAPI.SP_q221599_GETOFFICEINFO(PID, atype).Count() > 0)
                {
                    sp_q221599_GETOFFICEINFOResult = TAPI.SP_q221599_GETOFFICEINFO(PID, atype).First();
                    idx = 2 * sp_q221599_GETOFFICEINFOResult.COUNT + atype;
                }
                else
                {
                    int atype2 = 1;
                    if (atype == 1)
                    {
                        atype2 = 2;
                    }
                    sp_q221599_GETOFFICEINFOResult = TAPI.SP_q221599_GETOFFICEINFO(PID, atype2).First();
                }

                q221599_List SurveyInfo = new q221599_List();

                string[] textType = { "", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };


                // keyID 자동 증가 할 수 있도록 테이블 쿼리 필요

                SurveyInfo.PID = PID + textType[idx];
                SurveyInfo.PID_ = PID;
                SurveyInfo.OTYPE = sp_q221599_GETOFFICEINFOResult.OTYPE;
                SurveyInfo.ATYPE = atype;
                SurveyInfo.PANSWER = sp_q221599_GETOFFICEINFOResult.PANSWER;
                SurveyInfo.A101 = sp_q221599_GETOFFICEINFOResult.A101;
                SurveyInfo.기관명 = sp_q221599_GETOFFICEINFOResult.기관명;
                SurveyInfo.시도 = sp_q221599_GETOFFICEINFOResult.시도;
                SurveyInfo.시군구 = sp_q221599_GETOFFICEINFOResult.시군구;
                SurveyInfo.상세주소 = sp_q221599_GETOFFICEINFOResult.상세주소;
                SurveyInfo.지역코드 = sp_q221599_GETOFFICEINFOResult.지역코드;
                SurveyInfo.담당업무 = sp_q221599_GETOFFICEINFOResult.담당업무;


                SurveyInfo.이름 = name;
                SurveyInfo.연락처1 = tel;
                SurveyInfo.연락처2 = hp;
                SurveyInfo.소속부서 = group;
                SurveyInfo.직급 = grade;

                SurveyInfo.면접원아이디 = UID;
                SurveyInfo.면접원이름 = sp_q221599_GETOFFICEINFOResult.면접원이름;
                SurveyInfo.배부일자 = DateTime.Now.ToString();
                SurveyInfo.담당자사번 = sp_q221599_GETOFFICEINFOResult.담당자사번;
                SurveyInfo.담당자이름 = sp_q221599_GETOFFICEINFOResult.담당자이름;


                SurveyInfo.useyn = 1;
                SurveyInfo.deleteyn = 0;
                SurveyInfo.testyn = 0;

                SurveyInfo.응답자컨택유형 = addtype;

                TAPI.q221599_List.InsertOnSubmit(SurveyInfo);

                TAPI.SubmitChanges();
            }

            return "생성 되었습니다.";
        }



    }
}
