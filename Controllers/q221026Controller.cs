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
    public class q221026Controller : Controller
    {
        // GET: q221026
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

        public List<SP_q221026_GETHOUSELISTResult> GetList()
        {
            List<SP_q221026_GETHOUSELISTResult> list = new List<SP_q221026_GETHOUSELISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221026_GETHOUSELIST(UID).ToList();
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 12961).FirstOrDefault();
                
                if (t_QuestionnaireDistribution != null)
                {
                    t_QuestionnaireDistribution.sendDateTime = DateTime.Now;

                    SURVEYTOOL.SubmitChanges();
                }


            }

            List<string> statusList1 = list.GroupBy(g => g.진행상태).Where(w => string.IsNullOrEmpty(w.Key) == false).Select(s => s.Key).ToList();


            ViewBag.StatusList1 = statusList1;

            return list;
        }

        public ActionResult List()
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            return View("HouseList", GetList());
        }

        public ActionResult ListPartial()
        {
            return PartialView("HouseListPartial", GetList());
        }

        public ActionResult ProgressPartial()
        {
            ProgressCount progressCount = new ProgressCount();

            progressCount.All = "0";
            progressCount.Ing = "0";
            progressCount.End = "0";

            List<SP_q221026_GETHOUSELISTResult> list = new List<SP_q221026_GETHOUSELISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221026_GETHOUSELIST(UID).ToList();
                
                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(row => row.진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(row => row.진행상태 == "완료").Count().ToString();
            }

            return PartialView("ProgressPartial", progressCount);
        }

        public ActionResult PartialList(string pid,string code)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }
            string code2 = pid.Split('=')[1].ToString();
            List<SP_q221026_GETLISTResult> list = new List<SP_q221026_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221026_GETLIST(UID, code2).ToList();
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 12961).FirstOrDefault();

                if (t_QuestionnaireDistribution != null)
                {
                    t_QuestionnaireDistribution.sendDateTime = DateTime.Now;

                    SURVEYTOOL.SubmitChanges();
                }
            }

            List<string> statusList1 = list.GroupBy(g => g.진행상태).Where(w => string.IsNullOrEmpty(w.Key) == false).Select(s => s.Key).ToList();
            List<string> statusList2 = list.GroupBy(g => g.직책2).Where(w => string.IsNullOrEmpty(w.Key) == false).Select(s => s.Key).ToList();


            ViewBag.StatusList1 = statusList1;
            ViewBag.StatusList2 = statusList2;


            return View("List", list);
        }


        public List<SP_q221026_GETLISTResult> GetPersonList(string code)
        {
            List<SP_q221026_GETLISTResult> list = new List<SP_q221026_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221026_GETLIST(UID, code).ToList();

                SP_q221026_GETLISTResult GETHOUSELISTResult = TAPI.SP_q221026_GETLIST(UID, code).First();

                ViewBag.단지코드 = GETHOUSELISTResult.단지코드;
                ViewBag.단지명 = GETHOUSELISTResult.단지명;
                ViewBag.code = code;
            }

            return list;
        }

        public ActionResult PersonList(string pid, string code)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }
            code = pid.Split('=')[1].ToString();
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                TAPI.SubmitChanges();
            }

            return View("PersonList", GetPersonList(code));
        }

        public ActionResult PersonListPartial(string code)
        {
            return PartialView("PersonListPartial", GetPersonList(code));
        }
        public string SaveInfo(int pid, string cap, string name,string num)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            if(string.IsNullOrEmpty(cap))
            {
                return "직책을 선택해주세요.";
            }

            if (string.IsNullOrEmpty(name))
            {
                return "이름을 입력해주세요.";
            }

            if (string.IsNullOrWhiteSpace(num.Replace('-', ' ')))
            {
                return "연락처를 확인 해 주세요.";
            }
            else
            {
                Regex regex = new Regex(@"[0-9]{3}-[0-9]{3,4}-[0-9]{4}");
                Match m = regex.Match(num);
                if (!m.Success)
                {
                    return "휴대폰 번호를 확인 해 주세요.";
                }
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                int checkTel = TAPI.q221026_List.Where(w => w.pid != pid && w.연락처2 == num).Count();
                if (checkTel > 0)
                {
                    return "중복된 연락처를 입력하셨습니다. \r\n연락처를 확인해주세요.";
                }


                q221026_List List = TAPI.q221026_List.FirstOrDefault(f => f.pid == pid );

                List.직책2 = cap;
                List.이름2 = name;
                List.연락처2 = num;


                TAPI.SubmitChanges();
            }

            return "";
        }
        public ActionResult Survey(int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    q221026_List List = TAPI.q221026_List.First(f => f.pid == pid);

                    string QMListData = $@"<RPS_SamplingLIST_INFO>
    <직책 VALUE=""{List.직책2}"" />
    <성함 VALUE=""{List.이름2}"" />
    <연락처 VALUE=""{List.연락처2}"" />
    <단지코드 VALUE=""{List.단지코드}"" />
</RPS_SamplingLIST_INFO>";

                    string query = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q221026]') AND type in (N'U'))
BEGIN
    MERGE SURVEY.dbo.T_SamplingList_q221026 as SLIST
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
                    TAPI.ExecuteCommand(query, "");
                }

                List<AnswerList> answerList = new List<AnswerList>();

                bool eqnum = false;
                int qID = 12961;

                int answerStateCode = 0;
                using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q221026 WHERE PID = '{0}'", pid)).FirstOrDefault();
                    answerStateCode = t_SamplingList != null ? (int)(t_SamplingList.answerStateCode != null ? t_SamplingList.answerStateCode : 0) : 0;
                }
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    q221026_List List = TAPI.q221026_List.First(f => f.pid == pid);

                    if (List.진행상태 == "완료" || List.진행상태 == "선정탈락" || answerStateCode == 4)
                    {
                        eqnum = true;
                    }

                    TAPI.SP_q221026_SETLIST(pid, "진행중");
                }

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q221026&pid={pid}&p=y&uid={UID}&atype=1&t=tapi");
                }
                else
                {

                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + qID.ToString()).Replace("/q221026", ""), pid);

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
                                              Url = TripleDESCryptoService.Encrypt($"qn=q221026&pid={pid}&p=y&eqnum={element.Name.ToString()}&uid={UID}&atype=1&t=tapi"),
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