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
    public class q220557Controller : Controller
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

        public List<SP_q220557_GETLISTResult> GetList()
        {

            List<SP_q220557_GETLISTResult> list = new List<SP_q220557_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220557_GETLIST(UID, null).ToList();
            }

            List<string> statusList = list.GroupBy(g => g.최종진행상태).Where(w => string.IsNullOrEmpty(w.Key) == false).Select(s => s.Key).ToList();


            ViewBag.보안각서 = "";

            statusList.Insert(0, "");
            ViewBag.StatusList = statusList;
            if (list.Count > 0)
                ViewBag.보안각서 = list[0].보안각서;



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

            List<SP_q220557_GETLISTResult> list = new List<SP_q220557_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220557_GETLIST(UID, null).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.최종진행상태 != null && w.최종진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(w => w.최종진행상태 != null && w.최종진행상태 == "완료").Count().ToString();
            }

            return PartialView("ProgressPartial", progressCount);
        }

        public ActionResult AnswerInfo(int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            SP_q220557_GETLISTResult ListResult = null;
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                ListResult = TAPI.SP_q220557_GETLIST(UID, pid).First();
            }

            return View("AnswerInfo", ListResult);
        }

        public String SaveInfo(int pid, string fName, string name, string fStatus, string IsNewAddress, string jibunAddr, string roadAddrPart1, string siNm, string sggNm, string emdNm, string sebuAddr, string phone, string memo, string sendPhone)
        {

            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                int phoneC = TAPI.q220557_List.Count(f => f.휴대폰번호 == phone && f.PID != pid);
                if (phoneC > 0)
                {
                    // return "이미 입력 된 휴대폰번호 입니다.\r\n휴대전화 번호를 확인해 주세요.";
                }

                q220557_List list = TAPI.q220557_List.First(f => f.PID == pid);

                list.농장명 = fName;
                list.농장주 = name;
                list.운영상태 = fStatus;
                list.휴대폰번호 = phone;
                list.방문특이사항 = memo;

                list.주소확인 = IsNewAddress;
                if (IsNewAddress == "주소 틀림")
                {
                    list.시도 = siNm;
                    list.시군구 = sggNm;
                    list.읍면동 = emdNm;

                    list.법정동주소 = jibunAddr + " " + sebuAddr;
                    list.도로명주소 = roadAddrPart1 + " " + sebuAddr;
                }

                TAPI.SubmitChanges();

                if (sendPhone != "")
                {
                    if (SendMSG(pid, sendPhone) != "문자 발송")
                    {
                        result = "문자 발송 오류";
                    }

                }

                return result;
            }
        }
        public string SendMSG(int pid, string phone)
        {

            using (SENDMASTERDataContext SENDMASTER = new SENDMASTERDataContext("Data Source=172.19.100.85,15000;Initial Catalog=SEND_MASTER;Persist Security Info=True;User ID=hrcdv;password=Hrc#3014!#"))
            {
                string mm = string.Format(@"안녕하세요. 축산환경관리원입니다.

저희는 축산환경 개선 및 온실가스 감축방안을 제시하기 위해 『2022년 전국 축산환경 실태조사』를 진행중에 있습니다.

본 조사는 지속가능한 축산기반 마련의 기초자료로 활용하기 위해 주요 축종을 사육하시는 농장주 분들을 대상으로 축사의 설치‧운영 현황과 분뇨배출‧처리 현황 등을 조사하고 있습니다.

응답해주신 내용은 농림축산식품부 및 축산환경관리원의 개인정보보호지침에 의거해 엄격히 보호되며, 개인을 식별할 수 없도록 처리 후 통계분석에만 활용됩니다.
국내 축산환경의 발전을 위해 소중한 의견 부탁드립니다.

설문링크: https://rpssurvey.hrcglobal.com/?qn=q220557&pid={0}&atype=3

감사합니다.
관련 문의는 1533-0565 로 연락 부탁 드립니다.
", pid);
                SENDMASTER.PROC_SEND_MESSAGE("interviwer", "15330565", phone, "[2022년 전국 축산환경 실태조사] 설문 참여 안내", mm, DateTime.Now, "", "2022-97-0557");

            }
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220557_List list = TAPI.q220557_List.First(f => f.PID == pid);
                string QMListData = $@"<RPS_SamplingLIST_INFO>
                                    <품종 VALUE=""{list.품종}"" />
                                    <축종 VALUE=""{list.축종}"" />
                                    <운영상태 VALUE=""{list.운영상태}"" />
                                     <농장명성명 VALUE=""{list.농장주}"" />
                                     <농장명연락처 VALUE=""{list.휴대폰번호}"" />
                                     <농장명주소 VALUE=""{list.법정동주소}"" />
                                     <인허가번호 VALUE=""{list.인허가번호}"" />
                                    </RPS_SamplingLIST_INFO>";

                string query = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q220557]') AND type in (N'U'))
BEGIN
    MERGE SURVEY.dbo.T_SamplingList_q220557 as SLIST
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

                list.문자발송 = DateTime.Now.ToString();
                list.발송연락처 = phone;

                TAPI.SubmitChanges();
            }

            return "문자 발송 완료";

        }

        public string SendInfo(int pid, string phone)
        {

            using (SENDMASTERDataContext SENDMASTER = new SENDMASTERDataContext("Data Source=172.19.100.85,15000;Initial Catalog=SEND_MASTER;Persist Security Info=True;User ID=hrcdv;password=Hrc#3014!#"))
            {
                string mm = string.Format(@"안녕하세요. 축산환경관리원입니다.
저희는 축산환경 개선 및 온실가스 감축방안을 제시하기 위해 『2022년 전국 축산환경 실태조사』를 진행중에 있습니다.
축산환경 실태조사가 원활히 진행될 수 있도록 적극적인 관심과 협조를 요청드리고자
아래 링크로 협조 요청 공문을 전달 드립니다.

공문 링크: https://rpssurvey.hrcglobal.com/Media/12778/info.pdf

감사합니다.
관련 문의는 1533-0565 로 연락 부탁 드립니다.
");
                SENDMASTER.PROC_SEND_MESSAGE("interviwer", "15330565", phone, "[축산환경 실태조사] 협조공문", mm, DateTime.Now, "", "2022-97-0557");

            }
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220557_List list = TAPI.q220557_List.First(f => f.PID == pid);


                list.공문발송 = DateTime.Now.ToString();
                list.발송연락처 = phone;

                TAPI.SubmitChanges();
            }

            return "공문이 발송 완료";

        }

        public string SendSurvey(int pid, string phone, int surveyType)
        {
            //int surveyType = 0;
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220557_List list = TAPI.q220557_List.First(f => f.PID == pid);

                //1:한우육우 
                //2:젖소
                //3:돼지
                //4:닭 
                //5:오리
                //if (list.축종 == "소")
                //{
                //    if (list.품종 == "육우" || list.품종 == "한우")
                //    {
                //        surveyType = 1;
                //    }
                //    else if (list.품종 == "젖소")
                //    {
                //        surveyType = 2;
                //    }
                //    else
                //    {
                //        // 품종 명확하지 않은 경우 한우로 발송
                //        surveyType = 1;
                //    }
                //}
                //else if (list.축종 == "돼지")
                //{
                //    surveyType = 3;
                //}
                //else if (list.축종 == "닭")
                //{
                //    surveyType = 4;
                //}
                //else if (list.축종 == "오리")
                //{
                //    surveyType = 5;
                //}

                if (surveyType == 0)
                {
                    return "발송 실패";
                }
                list.종이설문발송 = DateTime.Now.ToString();
                list.발송연락처 = phone;

                TAPI.SubmitChanges();
            }


            using (SENDMASTERDataContext SENDMASTER = new SENDMASTERDataContext("Data Source=172.19.100.85,15000;Initial Catalog=SEND_MASTER;Persist Security Info=True;User ID=hrcdv;password=Hrc#3014!#"))
            {
                string mm = string.Format(@"안녕하세요. 축산환경관리원입니다.
저희는 축산환경 개선 및 온실가스 감축방안을 제시하기 위해 『2022년 전국 축산환경 실태조사』를 진행중에 있습니다.
『2022년 전국 축산환경 실태조사』 설문지를 보내드리오니 확인 부탁 드립니다

설문지 링크: https://rpssurvey.hrcglobal.com/Media/12778/survey{0}.pdf

감사합니다.
관련 문의는 1533-0565 로 연락 부탁 드립니다.
", surveyType);
                SENDMASTER.PROC_SEND_MESSAGE("interviwer", "15330565", phone, "[축산환경 실태조사] 설문지 발송", mm, DateTime.Now, "", "2022-97-0557");

            }

            return "설문지 발송 완료";

        }

        public ActionResult Contact(int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }



            List<q220557_Contact> ContactList = new List<q220557_Contact>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220557_List List = TAPI.q220557_List.FirstOrDefault(f => f.PID == pid);

                ContactList = (from contact in TAPI.q220557_Contact
                               where contact.PID == pid
                               select contact).ToList();


                ViewBag.PID = List.PID;
                ViewBag.농장주 = List.농장주;
                ViewBag.농장명 = List.농장명;
                ViewBag.축종 = List.축종;
                ViewBag.주소 = List.법정동주소;
                ViewBag.휴대전화 = List.휴대폰번호;

                ViewBag.Ranking = (ContactList.Count + 1).ToString();
                ViewBag.contactYear = DateTime.Now.ToString("yyyy");
                ViewBag.contactMonth = DateTime.Now.ToString("MM");
                ViewBag.contactDay = DateTime.Now.ToString("dd");

            }
            return View("Contact", ContactList);
        }
        public string ContactOK(int pid, int ranking, string contact, string contactMonth, string contactDay, string contactTime)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            q220557_Contact Contact = new q220557_Contact();
            q220557_List List = new q220557_List();


            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                Contact.PID = pid;
                Contact.면접원아이디 = UID.ToString();
                Contact.방문횟수 = ranking;
                Contact.방문일자 = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;
                Contact.방문시간 = contactTime;
                Contact.비성공사유 = contact;
                Contact.inputDate = DateTime.Now;
                Contact.담당자회사 = List.담당자회사;

                TAPI.q220557_Contact.InsertOnSubmit(Contact);

                List = (from surveyInfo in TAPI.q220557_List
                        where surveyInfo.PID == pid
                        select surveyInfo).SingleOrDefault();

                List.접촉일지횟수 = ranking.ToString();
                List.접촉일지최종상태 = contact;

                // 추후 수정 가능성 있음
                string lastStatus = "진행중";
                if (contact == "고소, 고발 등 협박수준으로 거절" || contact == "실거주지 다름, 변경 주소 거절")
                {
                    lastStatus = "거절";
                }
                else if (contact == "추적불가(전화번호, 주소 모두 틀림)" || contact == "접근자체가 불가능한 축사" || contact == "사망 및 실종" || contact == "폐업" || contact == "휴업" || contact == "리스트 중복")
                {
                    lastStatus = "조사불가";
                }

                if (List.최종진행상태 != "완료")
                {
                    List.최종진행상태 = lastStatus;
                }


                TAPI.SubmitChanges();
            }

            return "저장";
        }


        public ActionResult PictureSurvey(int pid)
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
                    q220557_List list = TAPI.q220557_List.First(f => f.PID == pid);

                    if (string.IsNullOrEmpty(list.사진업로드상태) == false && list.사진업로드상태 == "완료")
                    {
                        eqnum = true;
                    }
                }

                if (eqnum == false)
                {

                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q220557_0&pid={pid}&uid={UID}&t=tapi");
                }
                else
                {
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\12721").Replace("/q220557_0", ""), pid);

                    using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                    {
                        StreamReader srAnswerData = new StreamReader(dataFile, Encoding.Default);
                        string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());
                        XElement AnswerData = XElement.Parse(answerDataDecrypt);


                        srAnswerData.Close();
                        srAnswerData.Dispose();

                        T_Script t_Script = SURVEYTOOL.T_Scripts.Where(w => w.questionnaireID == 12721).First();

                        if (t_Script != null)
                        {
                            XElement variableList = t_Script.variableList;

                            answerList = (from element in variableList.Elements()
                                          select new AnswerList
                                          {
                                              Variable = element.Name.ToString(),
                                              Title = element.Value,
                                              Answer = GetAnswerData(AnswerData, element.Name.ToString()),
                                              Url = TripleDESCryptoService.Encrypt(string.Format("qn=q220557_0&pid={0}&uid={1}&eqnum={2}&t=tapi&nv=true", pid, UID, element.Name.ToString())),
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
                int answerStateCode = 0;
                using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q220557 WHERE PID = '{0}'", pid)).FirstOrDefault();

                    answerStateCode = t_SamplingList != null ? (int)(t_SamplingList.answerStateCode != null ? t_SamplingList.answerStateCode : 0) : 0;

                }

                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    q220557_List list = TAPI.q220557_List.First(f => f.PID == pid);
                    if ((string.IsNullOrEmpty(list.진행상태) == false && list.진행상태 == "완료") || answerStateCode == 4)
                    {
                        eqnum = true;
                    }
                    TAPI.SP_q220557_SETINFO(pid, "진행중", 1, "");

                    string QMListData = $@"<RPS_SamplingLIST_INFO>
                                    <품종 VALUE=""{list.품종}"" />
                                    <축종 VALUE=""{list.축종}"" />
                                    <운영상태 VALUE=""{list.운영상태}"" />
                                     <농장명성명 VALUE=""{list.농장주}"" />
                                     <농장명연락처 VALUE=""{list.휴대폰번호}"" />
                                     <농장명주소 VALUE=""{list.법정동주소}"" />
                                    <인허가번호 VALUE=""{list.인허가번호}"" />
                                    </RPS_SamplingLIST_INFO>";

                    string query = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q220557]') AND type in (N'U'))
BEGIN
    MERGE SURVEY.dbo.T_SamplingList_q220557 as SLIST
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
                }

                ViewBag.pid = pid;
                if (eqnum == false)
                {

                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q220557&pid={pid}&uid={UID}&t=tapi");
                }
                else
                {
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\12778").Replace("/q220557", ""), pid);

                    using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                    {
                        StreamReader srAnswerData = new StreamReader(dataFile, Encoding.Default);
                        string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());
                        XElement AnswerData = XElement.Parse(answerDataDecrypt);


                        srAnswerData.Close();
                        srAnswerData.Dispose();

                        T_Script t_Script = SURVEYTOOL.T_Scripts.Where(w => w.questionnaireID == 12778).First();

                        if (t_Script != null)
                        {
                            XElement variableList = t_Script.variableList;

                            answerList = (from element in variableList.Elements()
                                          select new AnswerList
                                          {
                                              Variable = element.Name.ToString(),
                                              Title = element.Value,
                                              Answer = GetAnswerData(AnswerData, element.Name.ToString()),
                                              Url = TripleDESCryptoService.Encrypt(string.Format("qn=q220557&pid={0}&uid={1}&eqnum={2}&t=tapi&nv=true", pid, UID, element.Name.ToString())),
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

        public ActionResult SignSurvey()
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                string iname = "";
                List<AnswerList> answerList = new List<AnswerList>();
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {

                    // q220557_List list = TAPI.q220557_List.First(f => f.면접원아이디 == UID.ToString());
                    q220557_UnameList list = TAPI.q220557_UnameList.First(f => f.면접원아이디 == UID);
                    iname = list.면접원이름;
                }
                ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q220557_2&pid={UID}&iname={iname}&t=tapi");

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