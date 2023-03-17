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
    public class q221643Controller : Controller
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

        public List<SP_q221643_GETLISTResult> GetList()
        {
            List<SP_q221643_GETLISTResult> list = new List<SP_q221643_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221643_GETLIST(UID, null).ToList();
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 13187).FirstOrDefault();

                if (t_QuestionnaireDistribution != null)
                {
                    t_QuestionnaireDistribution.sendDateTime = DateTime.Now;

                    SURVEYTOOL.SubmitChanges();
                }
            }
            

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
        
        public List<q221643_PersonList> GetPersonList(int keyid)
        {
            List<q221643_PersonList> list = new List<q221643_PersonList>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.q221643_PersonList.Where(f => f.DID == keyid && f.deleteYN == 0 && f.면접원아이디==UID).ToList();

                q221643_DongList dongList = TAPI.q221643_DongList.First(f => f.DID == keyid);
                ViewBag.DID = keyid;
                ViewBag.전체주소정보 = dongList.전체주소정보;
            }

           
            return list;
        }

        public ActionResult PersonList(int keyid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            return View("PersonList", GetPersonList(keyid));
        }

        public ActionResult PersonListPartial(int keyid)
        {
            return PartialView("PersonListPartial", GetPersonList(keyid));
        }
        

        public string CreatePerson(int keyid)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";

            try
            {
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    TAPI.SP_q221643_INSERTPERSON(UID, keyid);
                    
                }
            }
            catch
            {
                result = "오류가 발생 했습니다.";
            }

            return result;
        }

        
        public ActionResult Survey(int keyid, int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                List<AnswerList> answerList = new List<AnswerList>();
                
                bool eqnum = false;
                int qID = 13187;

                int answerStateCode = 0;
                using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q221643 WHERE PID = '{0}'", pid)).FirstOrDefault();
                    answerStateCode = t_SamplingList != null ? (int)(t_SamplingList.answerStateCode != null ? t_SamplingList.answerStateCode : 0) : 0;
                }

                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {

                    q221643_DongList DongList = TAPI.q221643_DongList.First(f => f.DID == keyid);
                    string QMListData = $@"<RPS_SamplingLIST_INFO>
                                         <시도 VALUE=""{DongList.시도}"" />
                                         <시군구 VALUE=""{DongList.시군구}"" />
                                         <읍면동 VALUE=""{DongList.읍면동}"" />
                                         <세부시도 VALUE=""{DongList.세부시도}"" />
                                        <전체주소정보 VALUE=""{DongList.전체주소정보}"" />
                                        </RPS_SamplingLIST_INFO>";

                    

                    string query = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q221643]') AND type in (N'U'))
BEGIN
    MERGE SURVEY.dbo.T_SamplingList_q221643 as SLIST
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
                    TAPI.SubmitChanges();

                    q221643_PersonList PersonList = TAPI.q221643_PersonList.First(f => f.PID == pid);
                    if ((string.IsNullOrEmpty(PersonList.설문진행상태) == false && PersonList.설문진행상태 != "진행중"  ) ||  answerStateCode == 4 || answerStateCode==1 )
                    {
                        eqnum = true;
                    }

                    TAPI.SP_q221643_SETLIST(pid, "진행중", "","","","",null,null);
                }

                
                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q221643&pid={pid}&did={keyid}&t=TAPI&atype=1");
                }
                else
                {
                    ViewBag.PID = pid.ToString();                    

                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + qID.ToString()).Replace("/q221643", ""), pid);
                    
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
                                              Url = TripleDESCryptoService.Encrypt($"qn=q221643&pid={pid}&did={keyid}&atype=1&t=TAPI&eqnum={element.Name.ToString()}"),
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
        

        //public ActionResult Replace(int keyid)
        //{
        //    if (UID <= 0)
        //    {
        //        return RedirectToAction("Login", "Home");
        //    }

        //    using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
        //    {
        //        q221454_DongList DongList = TAPI.q221454_DongList.First(f => f.DID == keyid);
        //        return View("Replace", DongList);
        //    }
        //}

        //public ActionResult ReplaceOK(int keyid, string status, string etc)
        //{
        //    if (UID <= 0)
        //    {
        //        return RedirectToAction("Login", "Home");
        //    }

        //    using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
        //    {

        //        q221454_Replace Replace = new q221454_Replace();

        //        Replace.DID = keyid;
        //        Replace.면접원아이디 = UID;
        //        Replace.대체사유 = status;
        //        Replace.대체세부사유 = etc;
        //        Replace.대체요청일자 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        //        TAPI.q221454_Replace.InsertOnSubmit(Replace);


        //        q221454_DongList DongList = TAPI.q221454_DongList.First(f => f.DID == keyid);
        //        DongList.진행상태 = "대체요청";

        //        TAPI.SubmitChanges();
        //    }

        //    return null;
        //}
        
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