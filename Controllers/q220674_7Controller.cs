using TAPI_Interviewer.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using TAPI_Interviewer.Models;
using TAPI_Interviewer.Models.Project;

namespace TAPI_Interviewer.Controllers
{
    public class q220674_7Controller : Controller
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

        public List<SP_q220674_GETLISTResult> GetList()
        {

            List<SP_q220674_GETLISTResult> list = new List<SP_q220674_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220674_GETLIST(UID, 7, null).ToList();
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

        public ActionResult SetInfo(string pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }


            string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\12979").Replace("/q220674_7", ""), pid);
            StreamReader srAnswerData = new StreamReader(dataFile, Encoding.Default);
            string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());
            XElement AnswerData = XElement.Parse(answerDataDecrypt);


            srAnswerData.Close();
            srAnswerData.Dispose();
            


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

            List<SP_q220674_GETLISTResult> list = new List<SP_q220674_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220674_GETLIST(UID,7, null).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "완료").Count().ToString();
            }

            return PartialView("ProgressPartial", progressCount);
        }

        
        public ActionResult Survey(string pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {

                
            
                List<AnswerList> answerList = new List<AnswerList>();

                bool eqnum = false;
                

                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    q220674_7_List list = TAPI.q220674_7_List.First(f => f.PID == pid);

                    if (string.IsNullOrEmpty(list.진행상태) == false && (list.진행상태 == "재조사요청" || (list.진행상태=="진행중" && list.최종검증== "0. 검증 탈락")))
                    {
                        eqnum = true;
                    }
                    TAPI.SP_q220674_SETINFO(pid, 7, "진행중");

                   
                    string QMListData = $@"<RPS_SamplingLIST_INFO>
                                <광고내용 VALUE=""{list.광고내용}"" />
                                <광고주명 VALUE=""{list.광고주명}"" />
                                </RPS_SamplingLIST_INFO>";

                    string query = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q220674_7]') AND type in (N'U'))
BEGIN
    MERGE SURVEY.dbo.T_SamplingList_q220674_7 as SLIST
    USING
    (
        SELECT	'{0}' as PID
    ) as Answer on SLIST.PID = Answer.PID
    WHEN MATCHED THEN
	    UPDATE	SET	QMListData = N'{1}'
    WHEN NOT MATCHED THEN
	    INSERT (PID, QMListData)
	    VALUES (PID, N'{1}');
END", pid, QMListData);

                    TAPI.ExecuteCommand(query, "");

                    if (eqnum == true)
                    {
                        int answerStateCode = 0;
                        using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                        {
                            T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q220674_7 WHERE PID = '{0}'", pid)).FirstOrDefault();

                            answerStateCode = t_SamplingList != null ? (int)(t_SamplingList.answerStateCode != null ? t_SamplingList.answerStateCode : 0) : 0;
                            if (answerStateCode != 4)
                            {

                                eqnum = false;
                            }

                        }
                    }

                }

                if (eqnum == false)
                {
                    
                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q220674_7&pid={pid}&uid={UID}");
                }
                else
                {
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\12979").Replace("/q220674_7", ""), pid);

                    using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                    {
                        StreamReader srAnswerData = new StreamReader(dataFile, Encoding.Default);
                        string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());
                        XElement AnswerData = XElement.Parse(answerDataDecrypt);


                        srAnswerData.Close();
                        srAnswerData.Dispose();

                        T_Script t_Script = SURVEYTOOL.T_Scripts.Where(w => w.questionnaireID == 12979).First();

                        if (t_Script != null)
                        {
                            XElement variableList = t_Script.variableList;

                            answerList = (from element in variableList.Elements()
                                          select new AnswerList
                                          {
                                              Variable = element.Name.ToString(),
                                              Title = element.Value,
                                              Answer = GetAnswerData(AnswerData, element.Name.ToString()),
                                              Url = TripleDESCryptoService.Encrypt(string.Format("qn=q220674_7&pid={0}&uid={1}&eqnum={2}&nv=true", pid, UID, element.Name.ToString())),
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