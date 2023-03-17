using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using TAPI_Interviewer.Helpers;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TAPI_Interviewer.Models;
using TAPI_Interviewer.Models.Project;

namespace TAPI_Interviewer.Controllers
{
    public class q221024Controller : Controller
    {
        // GET: q221024
        public ActionResult Index()
        {
            return View();
        }

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

        public List<SP_q221024_GETLISTResult> GetList()
        {
            List<SP_q221024_GETLISTResult> list = new List<SP_q221024_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221024_GETLIST(UID).ToList();
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution1 = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 12938).FirstOrDefault();
                T_QuestionnaireDistribution t_QuestionnaireDistribution2 = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 12959).FirstOrDefault();

                if (t_QuestionnaireDistribution1 != null)
                {
                    t_QuestionnaireDistribution1.sendDateTime = DateTime.Now;

                    SURVEYTOOL.SubmitChanges();
                }


                if (t_QuestionnaireDistribution2 != null)
                {
                    t_QuestionnaireDistribution2.sendDateTime = DateTime.Now;

                    SURVEYTOOL.SubmitChanges();
                }
            }

            List<string> statusList1 = list.GroupBy(g => g.개별진행상태).Where(w => string.IsNullOrEmpty(w.Key) == false).Select(s => s.Key).ToList();
            List<string> statusList2 = list.GroupBy(g => g.심층진행상태).Where(w => string.IsNullOrEmpty(w.Key) == false).Select(s => s.Key).ToList();
            List<string> surveytype = list.GroupBy(g => g.설문참여유형).Where(w => string.IsNullOrEmpty(w.Key) == false).Select(s => s.Key).ToList();

            statusList1.Insert(0, "");
            statusList2.Insert(0, "");
            surveytype.Insert(0, "");

            ViewBag.StatusList1 = statusList1;
            ViewBag.StatusList2 = statusList2;
            ViewBag.Surveytype = surveytype;

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

            List<SP_q221024_GETLISTResult> list = new List<SP_q221024_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221024_GETLIST(UID).ToList();
                int ing = 0;
                int end = 0;

                ing += list.Where(w => w.설문참여유형 == "개별만" && w.개별진행상태 == "진행중" ).Count();
                ing += list.Where(w => w.설문참여유형 == "심층만" && w.심층진행상태 == "진행중").Count();
                //ing += list.Where(w => w.설문참여유형 == "개별+심층" &&(w.개별진행상태 != "완료" && w.개별진행상태 != "") || (w.심층진행상태 != "완료" && w.심층진행상태 != "")).Count();
                ing += list.Where(w => w.설문참여유형 == "개별+심층" && w.개별진행상태 == "완료"  && (w.심층진행상태 != "완료" && w.심층진행상태 != "")).Count();
                ing += list.Where(w => w.설문참여유형 == "개별+심층" && w.개별진행상태 == "진행중").Count();

                end += list.Where(w => w.설문참여유형== "개별만" && w.개별진행상태 == "완료" ).Count();
                end += list.Where(w => w.설문참여유형== "심층만" && w.심층진행상태 == "완료").Count();
                end += list.Where(w => w.설문참여유형 == "개별+심층" && (w.개별진행상태 == "완료" && w.심층진행상태 == "완료")).Count();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = ing.ToString();
                progressCount.End = end.ToString();
            }

            return PartialView("ProgressPartial", progressCount);
        }

        public ActionResult Contact(int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            SP_q221024_GETLISTResult LISTResult = GetList().First(f => f.pid == pid);

            return View("Contact", LISTResult);
        }

        public string ContactOK(int pid, string status1, string etc1, string status2, string etc2, string status3, string etc3, string etc, string surveytype, string phonenum , string weakYn )
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            if (string.IsNullOrWhiteSpace(phonenum.Replace('-', ' ')))
            {
                return "휴대폰 번호를 확인 해 주세요.";
            }
            else
            {
                Regex regex = new Regex(@"[0-9]{3}-[0-9]{3,4}-[0-9]{4}");
                Match m = regex.Match(phonenum);
                if (!m.Success)
                {
                    return "휴대폰 번호를 확인 해 주세요.1";
                }
            }

            if(string.IsNullOrEmpty(weakYn))
            {
                return "주거약자 여부를 확인 해 주세요.";
            }

            if(string.IsNullOrEmpty(surveytype))
            {
                return "설문 참여 유형을 선택 해 주세요.";
            }

            string result = "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q221024_List List = TAPI.q221024_List.First(f => f.pid == pid);

                List.컨택1차 = status1;
                List.컨택1차특이사항 = etc1;
                List.컨택2차 = status2;
                List.컨택2차특이사항 = etc2;
                List.컨택최종 = status3;
                List.컨택최종특이사항 = etc3;

                List.특이사항 = etc;
                List.휴대폰번호 = phonenum;
                List.주거약자주택여부 = weakYn;
                List.설문참여유형 = surveytype;

                if(surveytype=="개별만")
                {
                    string QMListData = $@"<RPS_SamplingLIST_INFO>
    <면접원아이디 VALUE=""{List.면접원아이디}"" />
    <면접원이름 VALUE=""{List.면접원이름}"" />
    <조사방법유형코드 VALUE=""{List.설문참여유형}"" />
</RPS_SamplingLIST_INFO>";

                    string query = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q221024]') AND type in (N'U'))
BEGIN
    MERGE SURVEY.dbo.T_SamplingList_q221024 as SLIST
    USING
    (
        SELECT	'{0}' as PID
    ) as Answer on SLIST.PID = Answer.PID
    WHEN MATCHED THEN
	    UPDATE	SET	QMListData = N'{1}'
    WHEN NOT MATCHED THEN
	    INSERT (PID,  QMListData)
	    VALUES (PID,  N'{1}');
END", pid, QMListData);
                    if (TAPI.ExecuteCommand(query, "") < 1)
                    {
                        result = "정보 저장 오류 다시 시도 해 주세요.";
                    }
                }
                else if(surveytype=="심층만")
                {
                    string QMListData = $@"<RPS_SamplingLIST_INFO>
    <면접원아이디 VALUE=""{List.면접원아이디}"" />
    <면접원이름 VALUE=""{List.면접원이름}"" />
    <조사방법유형코드 VALUE=""{List.설문참여유형}"" />
</RPS_SamplingLIST_INFO>";

                    string query = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q221025]') AND type in (N'U'))
BEGIN
    MERGE SURVEY.dbo.T_SamplingList_q221025 as SLIST
    USING
    (
        SELECT	'{0}' as PID
    ) as Answer on SLIST.PID = Answer.PID
    WHEN MATCHED THEN
	    UPDATE	SET	QMListData = N'{1}'
    WHEN NOT MATCHED THEN
	    INSERT (PID,  QMListData)
	    VALUES (PID,  N'{1}');
END", pid, QMListData);
                    if (TAPI.ExecuteCommand(query, "") < 1)
                    {
                        result = "정보 저장 오류 다시 시도 해 주세요.";
                    }
                }
                else //# 개별+심층
                {
                    string QMListData = $@"<RPS_SamplingLIST_INFO>
    <면접원아이디 VALUE=""{List.면접원아이디}"" />
    <면접원이름 VALUE=""{List.면접원이름}"" />
    <조사방법유형코드 VALUE=""{List.설문참여유형}"" />
</RPS_SamplingLIST_INFO>";

                    string query = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q221024]') AND type in (N'U'))
BEGIN
    MERGE SURVEY.dbo.T_SamplingList_q221024 as SLIST
    USING
    (
        SELECT	'{0}' as PID
    ) as Answer on SLIST.PID = Answer.PID
    WHEN MATCHED THEN
	    UPDATE	SET	QMListData = N'{1}'
    WHEN NOT MATCHED THEN
	    INSERT (PID,  QMListData)
	    VALUES (PID,  N'{1}');
END", pid, QMListData);
                    if (TAPI.ExecuteCommand(query, "") < 1)
                    {
                        result = "정보 저장 오류 다시 시도 해 주세요.";
                    }

                    string QMListData1 = $@"<RPS_SamplingLIST_INFO>
    <면접원아이디 VALUE=""{List.면접원아이디}"" />
    <면접원이름 VALUE=""{List.면접원이름}"" />
    <조사방법유형코드 VALUE=""{List.설문참여유형}"" />
</RPS_SamplingLIST_INFO>";

                    string query1 = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q221025]') AND type in (N'U'))
BEGIN
    MERGE SURVEY.dbo.T_SamplingList_q221025 as SLIST
    USING
    (
        SELECT	'{0}' as PID
    ) as Answer on SLIST.PID = Answer.PID
    WHEN MATCHED THEN
	    UPDATE	SET	QMListData = N'{1}'
    WHEN NOT MATCHED THEN
	    INSERT (PID,  QMListData)
	    VALUES (PID,  N'{1}');
END", pid, QMListData1);
                    if (TAPI.ExecuteCommand(query1, "") < 1)
                    {
                        result = "정보 저장 오류 다시 시도 해 주세요.";
                    }
                }

                TAPI.SubmitChanges();
            }

            return result;
        }
        /// <summary>
        /// 개별조사
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public ActionResult Survey_1(int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }


            try
            {
                List<AnswerList> answerList = new List<AnswerList>();

                bool eqnum = false;
                int qID = 12938;

                int answerStateCode = 0;
                using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q221024 WHERE PID = '{0}'", pid)).FirstOrDefault();
                    answerStateCode = t_SamplingList != null ? (int)(t_SamplingList.answerStateCode != null ? t_SamplingList.answerStateCode : 0) : 0;
                }
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    q221024_List List = TAPI.q221024_List.First(f => f.pid == pid);

                    if (List.개별진행상태 == "완료" || List.개별진행상태=="선정탈락" || answerStateCode==4)
                    {
                        eqnum = true;
                    }

                    TAPI.SP_q221024_SETLIST(pid, "진행중", null);
                }

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q221024&pid={pid}&p=y&uid={UID}&atype=1&t=tapi");
                }
                else
                {

                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + qID.ToString()).Replace("/q221024", ""), pid);

                    using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                    {
                        StreamReader srAnswerData = new StreamReader(dataFile, Encoding.Default);
                        string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());
                        XElement AnswerData = XElement.Parse(answerDataDecrypt);

                        srAnswerData.Close();
                        srAnswerData.Dispose();

                        T_Script t_Script = SURVEYTOOL.T_Scripts.Where(w => w.questionnaireID == qID).First();


                        if (t_Script != null)
                        {
                            XElement variableList = t_Script.variableList;

                            answerList = (from element in variableList.Elements()
                                          select new AnswerList
                                          {
                                              Variable = element.Name.ToString(),
                                              Title = element.Value,
                                              Answer = GetAnswerData(AnswerData, element.Name.ToString()),
                                              Url = TripleDESCryptoService.Encrypt($"qn=q221024&pid={pid}&p=y&eqnum={element.Name.ToString()}&uid={UID}&atype=1&t=tapi"),
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
        /// <summary>
        /// 심층조사
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public ActionResult Survey_2(int ?pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                List<AnswerList> answerList = new List<AnswerList>();

                bool eqnum = false;
                int qID = 12959;
                int answerStateCode = 0;
                using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q221025 WHERE PID = '{0}'", pid)).FirstOrDefault();
                    answerStateCode = t_SamplingList != null ? (int)(t_SamplingList.answerStateCode != null ? t_SamplingList.answerStateCode : 0) : 0;
                }
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    q221024_List List = TAPI.q221024_List.First(f => f.pid == pid);

                    if (List.심층진행상태 == "완료" || List.심층진행상태 == "선정탈락"|| answerStateCode==4)
                    {
                        eqnum = true;
                    }

                    TAPI.SP_q221024_SETLIST(pid, null, "진행중");
                }

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q221025&pid={pid}&p=y&uid={UID}&atype=1&t=tapi");
                }
                else
                {

                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + qID.ToString()).Replace("/q221025", ""), pid);

                    using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                    {
                        StreamReader srAnswerData = new StreamReader(dataFile, Encoding.Default);
                        string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());
                        XElement AnswerData = XElement.Parse(answerDataDecrypt);

                        srAnswerData.Close();
                        srAnswerData.Dispose();

                        T_Script t_Script = SURVEYTOOL.T_Scripts.Where(w => w.questionnaireID == qID).First();


                        if (t_Script != null)
                        {
                            XElement variableList = t_Script.variableList;

                            answerList = (from element in variableList.Elements()
                                          select new AnswerList
                                          {
                                              Variable = element.Name.ToString(),
                                              Title = element.Value,
                                              Answer = GetAnswerData(AnswerData, element.Name.ToString()),
                                              Url = TripleDESCryptoService.Encrypt($"qn=q221025&pid={pid}&p=y&eqnum={element.Name.ToString()}&uid={UID}&atype=1&t=tapi"),
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
    }
}