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
    public class q220001Controller : Controller
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

        public List<SP_q220001_GETLISTResult> GetList()
        {
            
            List<SP_q220001_GETLISTResult> list = new List<SP_q220001_GETLISTResult>();
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220001_GETLIST(UID).ToList();
                
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 13226).FirstOrDefault();

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
            progressCount.End2 = "0";
            List<SP_q220001_GETLISTResult> list = new List<SP_q220001_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220001_GETLIST(UID).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "완료").Count().ToString();
              
            }

            return PartialView("ProgressPartial", progressCount);
        }

       

        public ActionResult AnswerInfo(int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            SP_q220001_GETINFOResult GETINFOResult = null;


            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                GETINFOResult = TAPI.SP_q220001_GETINFO(pid).First();
            }

            return View("AnswerInfo", GETINFOResult);
        }


        

        public string SaveInfo(int PID,  string name, string gender,  string tel, string hp, string IsNewAddress, string jibunAddr, string roadAddrPart1, string siNm, string sggNm, string emdNm, string sebuAddr)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                
                
                q220001_List list = TAPI.q220001_List.First(f => f.PID == PID);
                list.응답자이름 = name;
                list.응답자성별 = gender;
                list.휴대전화 = hp;
                list.집전화 = tel;


                int hp_count = TAPI.q220001_List.Count(f => f.휴대전화 == hp && f.PID != PID);
                if (hp_count > 0) 
                {
                    return "이미 존재하는 휴대전화번호 입니다. 휴대전화번호를 확인해주세요.";
                }
                int tel_count = TAPI.q220001_List.Count(f => f.집전화 == tel && f.PID != PID);
                if (tel_count > 0)
                {
                    return "이미 존재하는 집전화번호 입니다. 집전화번호를 확인해주세요.";
                }

                if (IsNewAddress == "주소 틀림")
                {
                    list.시도 = siNm;
                    list.시군구 = sggNm;
                    list.읍면동 = emdNm;
                    list.전체주소정보 = roadAddrPart1 + " " + sebuAddr;
                }
                else 
                {
                    list.주소확인 = 1;
                }



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
                List<AnswerList> answerList = new List<AnswerList>();
                
                bool eqnum = false;
                int qID = 13321;

                int answerStateCode = 0;
                using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q220001 WHERE PID = '{0}'", pid)).FirstOrDefault();
                    answerStateCode = t_SamplingList != null ? (int)(t_SamplingList.answerStateCode != null ? t_SamplingList.answerStateCode : 0) : 0;
                }
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {

                    q220001_List list = TAPI.q220001_List.First(f => f.PID == pid);
                    string QMListData = $@"<RPS_SamplingLIST_INFO>
                                        <응답자이름 VALUE=""{list.응답자이름}"" />
                                        <성별 VALUE=""{list.응답자성별}"" />
                                        </RPS_SamplingLIST_INFO>";



                    string query = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q220001]') AND type in (N'U'))
BEGIN
    MERGE SURVEY.dbo.T_SamplingList_q220001 as SLIST
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
                    TAPI.SP_q220001_SETLIST(pid, "진행중");

                    if ((string.IsNullOrEmpty(list.진행상태) == false && list.진행상태 == "완료") || answerStateCode == 4 || answerStateCode == 1)
                    {
                        eqnum = true;
                    }
                }
                

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q220001&pid={pid}&t=TAPI&uid={UID}");
                }
                else
                {
                    ViewBag.PID = pid.ToString();

                    
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + qID.ToString()).Replace("/q220001", ""), pid);
                    
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
                                              Url = TripleDESCryptoService.Encrypt($"qn=q220001&pid={pid}&t=TAPI&eqnum={element.Name.ToString()}"),
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

        public ActionResult Contact(int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            List<q220001_Contact> ContactList = null;

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220001_List List = TAPI.q220001_List.First(f => f.PID == pid);

                ContactList = TAPI.q220001_Contact.Where(w => w.PID == pid).ToList();

                ViewBag.PID = pid;
                ViewBag.주소 = List.전체주소정보;
                ViewBag.Ranking = (ContactList.Count + 1).ToString();
                ViewBag.contactYear = DateTime.Now.ToString("yyyy");
                ViewBag.contactMonth = DateTime.Now.ToString("MM");
                ViewBag.contactDay = DateTime.Now.ToString("dd");

            }
            return View("Contact", ContactList);
        }

        public string ContactOK(int pid, int ranking, string contact,string contactMonth, string contactDay, string contactTime)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }



            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                List<q220001_Contact> ContactList = new List<q220001_Contact>();
                string contactDate = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;

                //ContactList = (from contactT in TAPI.q221657_Contact
                //               where contactT.PID == pid & contactT.방문일자 == contactDate & contactT.방문시간 == contactTime
                //               select contactT).ToList();
                //if (ContactList.Count > 0)
                //{
                //    return "같은 시간대 방문기록이 이미 있습니다.";
                //}

                TAPI.SP_q220001_SETLIST(pid, "진행중");

                q220001_Contact Contact = new q220001_Contact();
                Contact.PID = pid;
                Contact.면접원아이디 = UID;
                Contact.방문횟수 = ranking;
                Contact.방문일자 = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;
                Contact.방문시간 = contactTime;
                Contact.비성공사유 = contact;
                Contact.inputDate = DateTime.Now;

                TAPI.q220001_Contact.InsertOnSubmit(Contact);

                q220001_List List = TAPI.q220001_List.First(f => f.PID == pid);
                List.최종접촉일지 = contact;
                TAPI.SubmitChanges();
            }

            return "저장";
        }

        public ActionResult Replace(int PID)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220001_List list = TAPI.q220001_List.First(f => f.PID == PID);
                ViewBag.PID = PID;
                ViewBag.주소 = list.전체주소정보;
            }
            
            return View("Replace");
          
        }

        public ActionResult ReplaceOK(int PID, string status )
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220001_Replace Replace = new q220001_Replace();
        
                Replace.면접원아이디 = UID;
                Replace.PID = PID;
                Replace.상태 = "대체요청";
                Replace.대체사유 = status;
                Replace.대체요청시간 = DateTime.Now;
                
                TAPI.q220001_Replace.InsertOnSubmit(Replace);

                q220001_List list = TAPI.q220001_List.First(f => f.PID == PID);
                list.진행상태 = "대체요청";
                TAPI.SubmitChanges();

            }

            return null;
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