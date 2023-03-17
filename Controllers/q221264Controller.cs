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


namespace TAPI_Interviewer.Controllers
{
    public class q221264Controller : Controller
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

        public List<SP_q221264_GETLISTResult> GetList(int? pid = null)
        {
            List<SP_q221264_GETLISTResult> ResultList = new List<SP_q221264_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                ResultList = TAPI.SP_q221264_GETLIST(UID, pid).ToList();
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 12739).FirstOrDefault();

                if (t_QuestionnaireDistribution != null)
                {
                    t_QuestionnaireDistribution.sendDateTime = DateTime.Now;

                    SURVEYTOOL.SubmitChanges();
                }
            }

            List<string> statusList = ResultList.GroupBy(g => g.진행상태).Where(w => string.IsNullOrEmpty(w.Key) == false).Select(s => s.Key).ToList();

            statusList.Insert(0, "");

            ViewBag.StatusList = statusList;

            return ResultList;
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

            progressCount.endDir = "0";
            progressCount.endTel = "0";
            

            List<SP_q221264_GETLISTResult> ResultList = new List<SP_q221264_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                ResultList = TAPI.SP_q221264_GETLIST(UID, null).ToList();

                progressCount.All = ResultList.Count.ToString();
                progressCount.Ing = ResultList.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                progressCount.End = ResultList.Where(w => w.진행상태 != null && (w.진행상태 == "완료(전화)" || w.진행상태 == "완료(대면)")).Count().ToString();

                if (ResultList.Where(w => w.진행상태 != null && (w.진행상태 == "완료(전화)" || w.진행상태 == "완료(대면)")).Count() == 0)
                {
                    progressCount.endDir = "0%";
                    progressCount.endTel = "0%";
                }
                else
                {
                    progressCount.endDir = (ResultList.Where(w => w.진행상태 != null && w.진행상태 == "완료(대면)").Count() / ResultList.Where(w => w.진행상태 != null && (w.진행상태 == "완료(전화)" || w.진행상태 == "완료(대면)")).Count() * 100 ).ToString() + "%";
                    progressCount.endTel = (ResultList.Where(w => w.진행상태 != null && w.진행상태 == "완료(전화)").Count() / ResultList.Where(w => w.진행상태 != null && (w.진행상태 == "완료(전화)" || w.진행상태 == "완료(대면)")).Count() * 100 ).ToString() + "%";
                }
                
            }

            return PartialView("ProgressPartial", progressCount);
        }

        public ActionResult Select(int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

           q221264_List list = null;
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.q221264_List.First(f => f.PID == pid);
            }

            return View("Select", list);
        }

        public ActionResult AnswerInfo(int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            q221264_List list = null;

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.q221264_List.First(f => f.PID == pid);
            }

            return View("AnswerInfo", list);
        }


        
        public string SaveInfo(int pid, string roadAddrPart1, string siNm, string sggNm, string emdNm, string sebuAddr, string IsNewAddress, string ctel, string relation, string IsNewTel)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q221264_List list = TAPI.q221264_List.First(f => f.PID == pid);

                list.주소확인여부 = IsNewAddress;
                if (IsNewAddress == "주소 틀림")
                {
                    list.주소수정 = "인터뷰어";
                    list.시도 = siNm;
                    list.시군구 = sggNm;
                    list.읍면동 = emdNm;

                    list.전체주소 = roadAddrPart1 + " " + sebuAddr;
                }

                list.컨택연락처확인여부 = IsNewTel;
                if (IsNewTel == "컨택연락처 틀림" )
                {
                    list.컨택전화번호 = ctel;
                    list.컨택전화번호관계 = relation;
                }
                
                TAPI.SubmitChanges();

                return result;
            }
        }

        public ActionResult Subsatitute(int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            q221264_List list  = null;

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.q221264_List.First(f => f.PID == pid);
            }

            return View("Subsatitute", list);
        }

        
        public string SaveSub(int pid, string isSubsatitute, string name, string tel, string sameContact, string reason, string reasonEtc, string relation, string relationEtc, string withAnswer)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q221264_List list = TAPI.q221264_List.First(f => f.PID == pid);

                list.대리응답여부 = isSubsatitute;

                // 대리 응답 여부와 상관없이 전체 업데이트
                list.대리응답자성명 = name;
                if (tel != "" && tel != "--")
                {
                    list.대리응답자전화번호 = tel;
                }
                else
                {
                    list.대리응답자전화번호 = "";
                }
                list.대리응답자컨택연락처와동일 = sameContact;

                list.대리응답이유 = reason;
                list.대리응답이유기타 = reasonEtc;

                list.대리응답자와의관계 = relation;
                list.대리응답자와의관계기타 = relationEtc;

                list.동석여부 = withAnswer;
                
                TAPI.SubmitChanges();
                return result;
            }
            
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

                ViewBag.PID = pid.ToString();

                q221264_List list = new q221264_List();

                bool eqnum = false;

                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    list = (from surveyInfo in TAPI.q221264_List
                            where surveyInfo.PID == pid
                            select surveyInfo).SingleOrDefault();
                    

                    if (list.진행상태 != null && list.진행상태!="" && list.진행상태.Substring(0, 2) == "완료")
                    {
                        eqnum = true;
                        list.진행상태 = "수정중";
                    }
                    else if (list.진행상태 == "수정중")
                    {
                        list.진행상태 = "수정중";
                    }
                    else
                    {
                        list.진행상태 = "진행중";
                    }

                    if (list.최종접촉결과 != "조사협조")
                    {
                        List<q221264_Contact> ContactList = new List<q221264_Contact>();
                        ContactList = (from contact in TAPI.q221264_Contact
                                               where contact.PID == pid
                                               select contact).ToList();

                        list.최종접촉횟수 = (ContactList.Count + 1).ToString();
                        list.최종접촉결과 = "조사협조";

                        q221264_Contact Contact = new q221264_Contact();
                        
                        string[] timeArray = new string[] { "", "", "", "", "", "", "", "오전 7이전", "오전 7시~ 오전8시 이전", "오전 8시~ 오전9시 이전", "오전 9시~ 오전10시 이전", "오전 10시~ 오전11시 이전", "오전 11시~ 오전12시 이전", "오전 12시~ 오후1시 이전", "오후 1시~ 오후2시 이전", "오후2시~ 오후3시 이전", "오후 3시~ 오후4시 이전", "오후 4시~ 오후5시 이전", "오후 5시~ 오후6시 이전", "오후 6시~ 오후7시 이전", "오후 7시~ 오후8시 이전", "오후 8시~ 오후9시 이전", "오후 9시~ 오후10시 이전", "오후 10시~ 오후11시 이전", "오후 11시 이후" };
                        string contactTime = timeArray[24];
                        int hh = DateTime.Now.Hour;
                        if (hh < 7)
                        {
                            contactTime = timeArray[7];
                        }
                        for (int i = 8; i < 24; i++)
                        {
                            if (hh < i && hh >= i - 1)
                            {
                                contactTime = timeArray[i];
                            }
                        }
                        


                        Contact.PID = pid;
                        Contact.면접원아이디 = UID;
                        Contact.방문횟수 = ContactList.Count + 1;
                        Contact.방문일자 = DateTime.Now.ToString("yyyy-MM-dd");
                        Contact.방문시간 = contactTime;
                        Contact.비성공사유 = "조사협조";
                        Contact.세부사유 = "";
                        Contact.inputType = "";
                        Contact.inputDate = DateTime.Now;

                        TAPI.q221264_Contact.InsertOnSubmit(Contact);

                      

                    }

                    
                    string QMListData = $@"<RPS_SamplingLIST_INFO>
                                    <장애유형 VALUE=""{list.장애유형}"" />
                                    <장애등급 VALUE=""{list.장애등급}"" />
                                    <경제활동상태 VALUE=""{list.경제활동상태}"" />
                                    <직업 VALUE=""{list.직업}"" />
                                    <성명 VALUE=""{list.성명}"" />
                                    <발달조사중복 VALUE=""{list.발달조사중복}"" />
                                    <gender VALUE=""{list.gender}"" />
                                    <birth VALUE=""{list.birth}"" />
                                    <연령 VALUE=""{list.연령}"" />
                                     <대리응답여부 VALUE=""{list.대리응답여부}"" />
                                     <대리응답자와의관계 VALUE=""{list.대리응답자와의관계}"" />
                                     <대리응답자성명 VALUE=""{list.대리응답자성명}"" />
                                     <대리응답이유 VALUE=""{list.대리응답이유}"" />
                                    <대리응답이유기타 VALUE=""{list.대리응답이유기타}"" />
                                    </RPS_SamplingLIST_INFO>";

                    string query = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q221264]') AND type in (N'U'))
BEGIN
    MERGE SURVEY.dbo.T_SamplingList_q221264 as SLIST
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
                }

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt(string.Format("qn=q221264&t=tapi&pid={0}&uid={1}&nv=true", pid, UID));
                }
                else
                {
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\13160").Replace("/q221264", ""), pid);

                    using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                    {
                        StreamReader srAnswerData = new StreamReader(dataFile, Encoding.Default);
                        string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());
                        XElement AnswerData = XElement.Parse(answerDataDecrypt);


                        srAnswerData.Close();
                        srAnswerData.Dispose();

                        T_Script t_Script = SURVEYTOOL.T_Scripts.Where(w => w.questionnaireID == 13160).First();

                        if (t_Script != null)
                        {
                            XElement variableList = t_Script.variableList;

                            answerList = (from element in variableList.Elements()
                                          select new AnswerList
                                          {
                                              Variable = element.Name.ToString(),
                                              Title = element.Value,
                                              Answer = GetAnswerData(AnswerData, element.Name.ToString()),
                                              Url = TripleDESCryptoService.Encrypt(string.Format("qn=q221264&pid={0}&uid={1}&t=tapi&eqnum={2}&nv=true", pid, UID, element.Name.ToString())),
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

        public ActionResult Contact(int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }
            
            q221264_List List = new q221264_List();
            List<q221264_Contact> ContactList = new List<q221264_Contact>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                List = (from surveyInfo in TAPI.q221264_List
                        where surveyInfo.PID == pid
                        select surveyInfo).SingleOrDefault();

                ContactList = (from contact in TAPI.q221264_Contact
                               where contact.PID == pid
                               select contact).ToList();

                ViewBag.pid = List.PID;
                ViewBag.이름 = List.성명;
                ViewBag.주소 = List.전체주소;
                
                ViewBag.Ranking = (ContactList.Count + 1).ToString();

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


                ViewBag.진행상태 = List.진행상태;
            }

            return View("Contact", ContactList);
        }

        
        public string ContactOK(int pid, int ranking, string contact, string etc, string contactMonth, string contactDay, string contactTime, string contact2, string type)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            q221264_Contact Contact = new q221264_Contact();
            q221264_Replace Replace = new q221264_Replace();
            q221264_List List = new q221264_List();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                Contact.PID = pid;
                Contact.면접원아이디 = UID;
                Contact.방문횟수 = ranking;
                Contact.방문일자 = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;
                Contact.방문시간 = contactTime;
                Contact.비성공사유 = contact;
                Contact.세부사유 = etc;
                Contact.재접촉가능여부 = contact2;
                Contact.inputType = type;
                Contact.inputDate = DateTime.Now;
                
                TAPI.q221264_Contact.InsertOnSubmit(Contact);

                List = (from surveyInfo in TAPI.q221264_List
                                      where surveyInfo.PID == pid
                                      select surveyInfo).SingleOrDefault();

                List.최종접촉횟수 = ranking.ToString();
                List.최종접촉결과 = contact;


                if (type == "대체요청")
                {
                    List.진행상태 = "대체요청";

                    Replace.PID = pid;
                    Replace.면접원아이디 = UID;
                    Replace.상태 = "대체요청";
                    Replace.대체요청시간 = DateTime.Now;

                    TAPI.q221264_Replace.InsertOnSubmit(Replace);
                    
                }
                TAPI.SubmitChanges();
            }

            return "저장";
        }
        public ActionResult Transfer(string cd, int pid)
        {

            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            
            string[] contactData = cd.Split('|');
            ViewBag.pid = pid;
            ViewBag.ranking = contactData[0];
            ViewBag.contactMonth = contactData[1];
            ViewBag.contactDay = contactData[2];
            ViewBag.contactTime = contactData[3];
            ViewBag.contact = contactData[4];
            ViewBag.etc = contactData[5];
            ViewBag.contact2 = contactData[6];


            q221264_List List = null;
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                List = (from surveyInfo in TAPI.q221264_List
                        where surveyInfo.PID == pid
                        select surveyInfo).SingleOrDefault();
            }


            return View("Transfer", List);
        }
        
        

        public ActionResult TransferOK(int pid, int ranking, string contact, string etc, string contactMonth, string contactDay, string contactTime, 
             string address1, string address2, string emdNm, string address, string dongho, string edit_tel)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            q221264_Contact Contact = new q221264_Contact();            
            q221264_List List = new q221264_List();
            q221264_Transfer Transfer = new q221264_Transfer();
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                Contact.PID = pid;
                Contact.면접원아이디 = UID;
                Contact.방문횟수 = ranking;
                Contact.방문일자 = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;
                Contact.방문시간 = contactTime;
                Contact.비성공사유 = contact;
                Contact.세부사유 = etc;
                Contact.inputType = "이관요청";
                Contact.inputDate = DateTime.Now;

                TAPI.q221264_Contact.InsertOnSubmit(Contact);

                List = (from surveyInfo in TAPI.q221264_List
                                      where surveyInfo.PID == pid
                                      select surveyInfo).SingleOrDefault();

                List.최종접촉횟수 = ranking.ToString();
                List.최종접촉결과 = contact;

                List.진행상태 = "이관요청";
                List.시도 = address1;
                List.시군구 = address2;
                List.읍면동 = emdNm;
                List.전체주소 = address + " " + dongho;
                List.주소수정 = "인터뷰어";

                Transfer.PID = pid;
                Transfer.면접원아이디 = UID.ToString();
                Transfer.시도 = address1;
                Transfer.시군구 = address2;
                Transfer.읍면동 = emdNm;
                Transfer.세부주소 = dongho;
                Transfer.전체주소 = address + " " + dongho;
                Transfer.변경연락처 = edit_tel;
                Transfer.요청시간 = DateTime.Now;
                Transfer.deleteyn = "0";
                Transfer.상태 = "이관요청";


                TAPI.q221264_Transfer.InsertOnSubmit(Transfer);
                
                TAPI.SubmitChanges();
            }
            return null;
        }


    }
}
