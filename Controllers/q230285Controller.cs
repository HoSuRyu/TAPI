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
    public class q230285_PNFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.Controller.ViewBag.조사명 = "2023년 한국노동패널 조사";
        }
    }

    [q230285_PNFilter]
    public class q230285Controller : Controller
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

        public List<SP_KLIPS2023_GETLISTResult> GetList(long? hid = null)
        {
            List<SP_KLIPS2023_GETLISTResult> list = new List<SP_KLIPS2023_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_KLIPS2023_GETLIST(UID, hid).ToList();
                if (list.Count  > 0)
                {
                    ViewBag.면접원이름 = list[0].면접원이름;
                    ViewBag.면접원아이디 = list[0].면접원아이디;
                }
            }
            
            List<string> statusList = list.GroupBy(g => g.진행상태).Where(w => string.IsNullOrEmpty(w.Key) == false).Select(s => s.Key).ToList();
            statusList.Insert(0, "");
            ViewBag.StatusList = statusList;

            return list;
        }

        public ActionResult List(string seoPanelList)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            ViewBag.seoPage = seoPanelList;
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
            progressCount.endEtc = "0";

           

            List<SP_KLIPS2023_GETLISTResult> list = new List<SP_KLIPS2023_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_KLIPS2023_GETLIST(UID, null).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "완료").Count().ToString();

            }

            return PartialView("ProgressPartial", progressCount);
        }
        


        public ActionResult Contact(long hid, string page)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            ViewBag.seoPage = page;
            List<KLIPS2023_Contact> ContactList = new List<KLIPS2023_Contact>();
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                KLIPS2023_HouseList houseList = TAPI.KLIPS2023_HouseList.FirstOrDefault(f => f.HID == hid);


                ContactList = (from contact in TAPI.KLIPS2023_Contact
                               where contact.HID == hid
                                       select contact).ToList();

                ViewBag.HID = houseList.HID;
                ViewBag.가구주 = houseList.가구주성명;
                ViewBag.주소 = houseList.조사주소;
                ViewBag.집전화 = houseList.집전화;
                ViewBag.휴대전화 = houseList.휴대전화;


                ViewBag.Ranking = (ContactList.Count + 1).ToString();
                ViewBag.contactYear = DateTime.Now.ToString("yyyy");
                ViewBag.contactMonth = DateTime.Now.ToString("MM");
                ViewBag.contactDay = DateTime.Now.ToString("dd");

            }
            return View("Contact", ContactList);
        }


        public string ContactOK(long hid, int ranking, string contact, string contactMonth, string contactDay, string contactTime, string etc7, string etc8)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            KLIPS2023_Contact Contact = new KLIPS2023_Contact();
            KLIPS2023_HouseList HouseList = new KLIPS2023_HouseList();
            

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                Contact.HID = hid;
                Contact.면접원아이디 = UID.ToString();
                Contact.방문횟수 = ranking;
                Contact.방문일자 = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;
                Contact.방문시간 = contactTime;
                Contact.비성공사유 = contact;
                Contact.inputDate = DateTime.Now;
                Contact.세부사유 = etc8;

                TAPI.KLIPS2023_Contact.InsertOnSubmit(Contact);

                HouseList = (from surveyInfo in TAPI.KLIPS2023_HouseList
                             where surveyInfo.HID == hid
                             select surveyInfo).SingleOrDefault();

                HouseList.최종방문횟수 = ranking;
                HouseList.최종방문결과 = contact;
                HouseList.진행상태 = contact;

                TAPI.SubmitChanges();
            }

            return "저장";
        }

        public ActionResult Transfer(long hid, string page)
        {

            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            ViewBag.seoPage = page;
            KLIPS2023_HouseList HouseList = new KLIPS2023_HouseList();
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {

                HouseList = (from surveyInfo in TAPI.KLIPS2023_HouseList
                             where surveyInfo.HID == hid
                             select surveyInfo).SingleOrDefault();
            }

            ViewBag.HID = hid;
            return View("Transfer", HouseList);
        }


        public ActionResult TransferOK(long hid,  string siNm, string sggNm, string emdNm, string roadAddrPart1, string sebuAddr, string edit_tel1, string edit_tel2, string reason, string reasonetc, string etc)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            KLIPS2023_HouseList HouseList = new KLIPS2023_HouseList();
            
            KLIPS2023_Transfer Transfer = new KLIPS2023_Transfer();
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {


                HouseList = (from surveyInfo in TAPI.KLIPS2023_HouseList
                             where surveyInfo.HID == hid
                             select surveyInfo).SingleOrDefault();


                HouseList.진행상태 = "이관요청";

                Transfer.HID = hid;
                Transfer.면접원아이디 = UID.ToString();
                Transfer.시도 = siNm;
                Transfer.시군구 = sggNm;
                Transfer.읍면동 = emdNm;
                Transfer.세부주소 = sebuAddr;
                Transfer.전체주소 = roadAddrPart1 + " " + sebuAddr;
                Transfer.특이사항 = etc;

                Transfer.최종집전화 = edit_tel1;
                Transfer.최종휴대전화 = edit_tel2;
                Transfer.요청시간 = DateTime.Now;
                Transfer.deleteyn = "0";
                Transfer.상태 = "이관요청";
                Transfer.이관신청사유 = reason;
                Transfer.기타사유 = reasonetc;

                TAPI.KLIPS2023_Transfer.InsertOnSubmit(Transfer);
                TAPI.SubmitChanges();
            }
            return null;
        }

        public ActionResult Agree(long hid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            KLIPS2023_HouseList houseList = null;

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                houseList = TAPI.KLIPS2023_HouseList.First(f => f.HID == hid);
                ViewBag.동의여부 = houseList.동의여부;
                ViewBag.가구유형 = houseList.유형;
                ViewBag.응답자이름 = houseList.가구주성명;


            }
            ViewBag.설문타입 = "가구설문";

            ViewBag.오늘날짜 = DateTime.Now;
            return View("Agree", houseList);

        }

        public ActionResult AgreePerson(long hid, string pid, string surveyType )
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            KLIPS2023_HouseList houseList = null;
            KLIPS2023_PersonList personList = null;
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                houseList = TAPI.KLIPS2023_HouseList.First(f => f.HID == hid);
                ViewBag.동의여부 = houseList.동의여부;
                ViewBag.가구유형 = houseList.유형;

                personList = TAPI.KLIPS2023_PersonList.First(f => f.PID == pid);
                ViewBag.응답자이름 = personList.이름;
                ViewBag.PID = pid;
            }

            ViewBag.설문타입 = surveyType;
            ViewBag.오늘날짜 = DateTime.Now;
            return View("Agree", houseList);

        }

        public ActionResult AnswerInfo(long hid, string page)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            ViewBag.seoPage = page;
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {   

                KLIPS2023_HouseList houseList = TAPI.KLIPS2023_HouseList.First(f => f.HID == hid);
                houseList.동의여부 = "동의";

                int personC = TAPI.KLIPS2023_PersonList.Count(c => c.HID == hid.ToString() && c.deleteYN==0);               
                int pAnswerC = TAPI.KLIPS2023_PersonList.Count(c => c.HID == hid.ToString() && c.deleteYN == 0 && (c.최종진행상태 == "완료" || c.최종진행상태 =="설문종료" || c.최종진행상태 =="탈락"));

                
                ViewBag.가구소득문항응답가능여부 = "불가";
                if( houseList.설문진행상태=="완료" && personC == pAnswerC)
                {
                    ViewBag.가구소득문항응답가능여부 = "가능";
                }
                ViewBag.가구유형 = houseList.유형;

                TAPI.SubmitChanges();
            }

            return View("AnswerInfo", GetPersoninfo(hid));

        }

        public ActionResult AnswerInfoPartial(long hid)
        {
            return PartialView("HouseListPartial", GetPersoninfo(hid));
        }

        /// <summary>
        /// 설문 진입 전 기존 응답 BASE로 세팅하는 부분 
        /// </summary>
        /// <param name="hid"></param>
        /// <returns></returns>
        public List<SP_KLIPS2023_GETPERSONLISTResult> GetPersoninfo(long hid)
        {
            List<SP_KLIPS2023_GETPERSONLISTResult> list = new List<SP_KLIPS2023_GETPERSONLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            { 
                list = TAPI.SP_KLIPS2023_GETPERSONLIST(hid).ToList();

                KLIPS2023_HouseList houseList = TAPI.KLIPS2023_HouseList.First(f => f.HID == hid);

                ViewBag.HID = houseList.HID;
                ViewBag.가구주 = houseList.가구주성명;
                ViewBag.주소 = houseList.조사주소;
                ViewBag.집전화 = houseList.집전화;
                ViewBag.휴대전화 = houseList.휴대전화;
                ViewBag.설문진행상태 = houseList.설문진행상태;
                ViewBag.소득진행상태 = houseList.소득진행상태;
                ViewBag.방문횟수 = houseList.최종방문횟수;
                ViewBag.방문결과 = houseList.최종방문결과;
                ViewBag.진행상태 = houseList.진행상태;
                
                // 가구 설문 완료인 경우 개인용 데이터 끌어오기
                if (houseList.설문진행상태 != null && houseList.설문진행상태 != "" && houseList.설문진행상태 == "완료")
                {
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"Web\DATA\13641"), hid);
                    if(houseList.유형 == "분가")
                    {
                        dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"Web\DATA\13646"), hid);
                    }

                    if (System.IO.File.Exists(dataFile) == true)
                    {
                        
                        StreamReader srAnswerData = new StreamReader(dataFile, Encoding.Default);
                        string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());
                        XElement AnswerData = XElement.Parse(answerDataDecrypt);
                        srAnswerData.Close();
                        srAnswerData.Dispose();

                        int AllpersonC = 0;
                        int targetPersonC = 0;
                        // 기존 가구원
                        for (int i = 1; i <= 20; i++)
                        {

                            string ptype = GetAnswerData(AnswerData, "PTYPE" + i);
                            if( ptype != "분가" && ptype !="사망" && ptype !="")
                            {
                                AllpersonC++;
                            }

                            string tperson = GetAnswerData(AnswerData, "tperson" + i);
                            if (tperson == "")
                            {
                                continue;
                            }
                            targetPersonC++;

                            string ppid = GetAnswerData(AnswerData, "PID_" + i);                       
                            string pname = GetAnswerData(AnswerData, "INAMEa" + tperson);
                            string page = GetAnswerData(AnswerData, "IAGEa" + tperson);
                            string pgender = GetAnswerData(AnswerData, "ISEXa" + tperson);
                            
                            if( ptype == "신규합가")
                            {
                                string oldPid = GetAnswerData(AnswerData, "SearchPID" + i);
                                TAPI.SP_KLIPS2023_SETJOINHOUSE(ppid, oldPid);
                            }
                            else if (houseList.유형 == "분가")
                            {
                                string oldpid = "";
                                KLIPS2023_SplitHouseList ShouseList = (from house in TAPI.KLIPS2023_SplitHouseList
                                                                       where house.HID == houseList.HID.ToString()
                                                                       select house).SingleOrDefault();
                                oldpid = string.Format("{0}{1}", ShouseList.분가전HID, ppid.Substring(ppid.Length - 2));

                                TAPI.SP_KLIPS2023_SETJOINHOUSE(ppid, oldpid);
                            }

                            int personYN = TAPI.KLIPS2023_PersonList.Count(c => c.PID == ppid);
                            KLIPS2023_PersonList personInfo = null;
                            if (personYN == 1)
                            {
                                personInfo = (from surveyInfo in TAPI.KLIPS2023_PersonList
                                              where surveyInfo.PID == ppid
                                              select surveyInfo).SingleOrDefault();

                                personInfo.HID = hid.ToString();
                                personInfo.PID = ppid;
                                personInfo.PersonIDX = i;
                                personInfo.이름 = pname;
                                personInfo.나이 = page;
                                personInfo.성별 = pgender;

                                personInfo.유형 = "기존";
                                if(ptype=="신규" )
                                {
                                    personInfo.유형 = "신규";
                                }
                            
                                personInfo.deleteYN = 0;
                                personInfo.면접원id = UID.ToString();
                            }
                            else
                            {
                                personInfo = new KLIPS2023_PersonList();
                                personInfo.HID = hid.ToString();
                                personInfo.PID = ppid;
                                personInfo.PersonIDX = i;
                                personInfo.이름 = pname;
                                personInfo.나이 = page;
                                personInfo.성별 = pgender;
                                personInfo.유형 = "기존";
                                if (ptype == "신규" )
                                {
                                    personInfo.유형 = "신규";
                                }
                                personInfo.deleteYN = 0;
                                personInfo.면접원id = UID.ToString();
                                
                                TAPI.KLIPS2023_PersonList.InsertOnSubmit(personInfo);
                            }

                          
                            TAPI.SubmitChanges();

                        }


                        // 생성 된 가구원 중 제외 된 가구원 있는지 확인
                        List<SP_KLIPS2023_GETPERSONLISTResult> personList = new List<SP_KLIPS2023_GETPERSONLISTResult>();
                        personList = TAPI.SP_KLIPS2023_GETPERSONLIST(hid).ToList();

                        for(int i=1; i<=20; i++)
                        {
                            string tperson = GetAnswerData(AnswerData, "tperson" + i);
                            string ppid = GetAnswerData(AnswerData, "PID_" + i);
                            if ( tperson == "" && ppid != "")
                            {
                                int isPerson = TAPI.KLIPS2023_PersonList.Count(c => c.PID == ppid);
                                if(isPerson != 0)
                                {
                                    KLIPS2023_PersonList personInfo = (from surveyInfo in TAPI.KLIPS2023_PersonList
                                                                       where surveyInfo.PID == ppid
                                                                       select surveyInfo).SingleOrDefault();
                                    personInfo.deleteYN = 1;
                                    TAPI.SubmitChanges();
                                }
                            }
                        }
                        houseList.전체가구원수 = AllpersonC;
                        houseList.적격가구원수 = targetPersonC;
                        TAPI.SubmitChanges();
                    }

                }
              

                list = TAPI.SP_KLIPS2023_GETPERSONLIST(hid).ToList();
            }

            
            return list;
        }

        /// <summary>
        /// 가구용 설문 시작
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public ActionResult Survey(long hid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                List<AnswerList> answerList = new List<AnswerList>();

                ViewBag.hid = hid.ToString();


                KLIPS2023_HouseList houseList = new KLIPS2023_HouseList();
                bool eqnum = false;

                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    houseList = (from surveyInfo in TAPI.KLIPS2023_HouseList
                                 where surveyInfo.HID == hid
                                 select surveyInfo).SingleOrDefault();

                    if (houseList.설문진행상태 != null && houseList.설문진행상태 != "" && houseList.설문진행상태 == "완료")
                    {
                        eqnum = true;
                    }
                    
                    houseList.설문진행상태 = "진행중";
                    if (houseList.시작일자 == null || houseList.시작일자 == "")
                    {
                        houseList.시작일자 = DateTime.Now.ToString("yyyy-MM-dd");

                        KLIPS2023_Contact Contact = new KLIPS2023_Contact();
                        int ranking = TAPI.KLIPS2023_Contact.Count(c => c.HID == hid ) + 1;
                        Contact.HID = hid;
                        Contact.면접원아이디 = UID.ToString();
                        Contact.방문횟수 = ranking;
                        Contact.방문일자 = DateTime.Now.ToString("yyyy-MM-dd");
                        string[] timeArray = new string[] { "자정12시-오전1시", "오전1시-오전2시", "오전2시-오전3시", "오전3시-오전4시", "오전4시-오전5시", "오전5시-오전6시", "오전6시-오전7시", "오전7시-오전8시", "오전8시-오전9시", "오전9시-오전10시", "오전10시-오전11시", "오전11시-정오12시", "정오12시-오후1시", "오후1시-오후2시", "오후2시-오후3시", "오후3시-오후4시", "오후4시-오후5시", "오후5시-오후6시", "오후6시-오후7시", "오후7시-오후8시", "오후8시-오후9시", "오후9시-오후10시", "오후10시-오후11시", "오후11시-자정12시" };                        
                        int hour = DateTime.Now.Hour;

                        Contact.방문시간 = timeArray[hour];
                        Contact.비성공사유 = "조사협조";
                        Contact.inputDate = DateTime.Now;
                        Contact.세부사유 = "";

                        TAPI.KLIPS2023_Contact.InsertOnSubmit(Contact);
                        houseList.최종방문횟수 = ranking;
                    }
                    houseList.최종방문결과 = "조사협조";

                    TAPI.SubmitChanges();
                }

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt(string.Format("qn=q230285&t=tapi&pid={0}&uid={1}&atype=1&nv=true", hid, UID));
                }
                else
                {
                    using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                    {
                        StreamReader srAnswerData = null; 
                        XElement AnswerData = null;

                        string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"Web\DATA\13641").Replace("/q230285", ""), hid);
                        srAnswerData = new StreamReader(dataFile, Encoding.Default);
                        string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());
                            
                        AnswerData = XElement.Parse(answerDataDecrypt);
                       

                        srAnswerData.Close();
                        srAnswerData.Dispose();

                        T_Script t_Script = SURVEYTOOL.T_Scripts.Where(w => w.questionnaireID == 13641).First();

                        if (t_Script != null)
                        {
                            XElement variableList = t_Script.variableList;

                            answerList = (from element in variableList.Elements()
                                          select new AnswerList
                                          {
                                              Variable = element.Name.ToString(),
                                              Title = element.Value,
                                              Answer = GetAnswerData(AnswerData, element.Name.ToString()),
                                              Url = TripleDESCryptoService.Encrypt(string.Format("qn=q230285&pid={0}&uid={1}&t=tapi&eqnum={2}&nv=true", hid, UID, element.Name.ToString())),
                                          }).Where(w => string.IsNullOrEmpty(w.Answer) == false && w.Title.StartsWith("[더미]") == false).ToList();
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
        /// 분가 가구 설문 시작
        /// </summary>
        /// <param name="hid"></param>
        /// <returns></returns>
        public ActionResult SplitSurvey(long hid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                List<AnswerList> answerList = new List<AnswerList>();
                ViewBag.hid = hid.ToString();


                KLIPS2023_HouseList houseList = new KLIPS2023_HouseList();
                bool eqnum = false;

                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    houseList = (from surveyInfo in TAPI.KLIPS2023_HouseList
                                 where surveyInfo.HID == hid
                                 select surveyInfo).SingleOrDefault();

                    if (houseList.설문진행상태 != null && houseList.설문진행상태 != "" && houseList.설문진행상태 == "완료")
                    {
                        eqnum = true;

                    }

                    houseList.진행상태 = "진행중";
                    houseList.설문진행상태 = "진행중";
                    if (houseList.시작일자 == null || houseList.시작일자 == "")
                    {
                        houseList.시작일자 = DateTime.Now.ToString("yyyy-MM-dd");
                    }
                    houseList.최종방문결과 = "조사협조";


                    TAPI.SubmitChanges();
                }


                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt(string.Format("qn=q230285_7&t=tapi&pid={0}&uid={1}&atype=1&nv=true", hid, UID));
                }
                else
                {
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"Web\DATA\13646").Replace("/q230285_7", ""), hid);
                    
                    using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                    {
                        StreamReader srAnswerData = new StreamReader(dataFile, Encoding.Default);
                        string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());

                        XElement AnswerData = XElement.Parse(answerDataDecrypt);

                        srAnswerData.Close();
                        srAnswerData.Dispose();

                        T_Script t_Script = SURVEYTOOL.T_Scripts.Where(w => w.questionnaireID == 13646).First();

                        if (t_Script != null)
                        {
                            XElement variableList = t_Script.variableList;

                            answerList = (from element in variableList.Elements()
                                          select new AnswerList
                                          {
                                              Variable = element.Name.ToString(),
                                              Title = element.Value,
                                              Answer = GetAnswerData(AnswerData, element.Name.ToString()),
                                              Url = TripleDESCryptoService.Encrypt(string.Format("qn=q230285_7&pid={0}&uid={1}&t=tapi&eqnum={2}&nv=true", hid, UID, element.Name.ToString())),
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

        public ActionResult PersonSurvey(long pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                List<AnswerList> answerList = new List<AnswerList>();

                ViewBag.pid = pid.ToString();
                

                KLIPS2023_PersonList personList = new KLIPS2023_PersonList();
                bool eqnum = false;
                int splitH = 0;
                int pid2 = int.Parse(pid.ToString().Substring(pid.ToString().Length - 2));
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    
                    personList = (from surveyInfo in TAPI.KLIPS2023_PersonList
                                  where surveyInfo.PID == pid.ToString()
                                 select surveyInfo).SingleOrDefault();

                    if (personList.설문진행상태 != null && personList.설문진행상태 != "" && personList.설문진행상태 != "진행중")
                    {
                        eqnum = true;
                       

                    }
                    KLIPS2023_HouseList houseList = (from house in TAPI.KLIPS2023_HouseList
                                                     where house.HID == long.Parse(personList.HID)
                                                     select house).SingleOrDefault();

                    ViewBag.hid = houseList.HID;

                    if (houseList.유형 == "분가")
                    {
                        splitH = 1;
                    }               
                      
                    TAPI.SubmitChanges();

                    TAPI.SP_KLIPS2023_SETLIST(pid, "진행중", 3);

                }

                if (eqnum == false)
                {
                    // 분가 시 samplingData HID 사용 할 수 없기때문에 직접 넘김
                    ViewBag.C = TripleDESCryptoService.Encrypt(string.Format("qn=q230285_1&t=tapi&atype=1&pid={0}&uid={1}&phid={3}&ppid2={4}&splith={2}&nv=true", pid, UID, splitH, personList.HID, pid2));
                }
                else
                {
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"Web\DATA\13642").Replace("/q230285_1", ""), pid);
                    FileInfo fi = new FileInfo(dataFile);
                    if (fi.Exists == false)
                    {
                        ViewBag.C = TripleDESCryptoService.Encrypt(string.Format("qn=q230285_1&t=tapi&atype=1&pid={0}&uid={1}&phid={3}&ppid2={4}&splith={2}&nv=true", pid, UID, splitH, personList.HID, pid2));
                    }
                    else
                    {
                        using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                        {


                            StreamReader srAnswerData = new StreamReader(dataFile, Encoding.Default);
                            string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());
                            XElement AnswerData = XElement.Parse(answerDataDecrypt);

                            srAnswerData.Close();
                            srAnswerData.Dispose();

                            T_Script t_Script = SURVEYTOOL.T_Scripts.Where(w => w.questionnaireID == 13642).First();

                            if (t_Script != null)
                            {
                                XElement variableList = t_Script.variableList;

                                answerList = (from element in variableList.Elements()
                                              select new AnswerList
                                              {
                                                  Variable = element.Name.ToString(),
                                                  Title = element.Value,
                                                  Answer = GetAnswerData(AnswerData, element.Name.ToString()),
                                                  Url = TripleDESCryptoService.Encrypt(string.Format("qn=q230285_1&pid={0}&uid={1}&t=tapi&eqnum={2}&phid={3}&ppid2={4}&nv=true", pid, UID, element.Name.ToString(), personList.HID, pid2)),
                                              }).Where(w => string.IsNullOrEmpty(w.Answer) == false).ToList();
                            }

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

        public ActionResult NewPersonSurvey(long pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                List<AnswerList> answerList = new List<AnswerList>();

                ViewBag.pid = pid.ToString();


                KLIPS2023_PersonList personList = new KLIPS2023_PersonList();
                bool eqnum = false;
                int splitH = 0;
                int pid2 = int.Parse(pid.ToString().Substring(pid.ToString().Length - 2));
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    personList = (from surveyInfo in TAPI.KLIPS2023_PersonList
                                  where surveyInfo.PID == pid.ToString()
                                  select surveyInfo).SingleOrDefault();

                    if (personList.설문진행상태 != null && personList.설문진행상태 != "" && personList.설문진행상태 != "진행중")
                    {
                        eqnum = true;
                        

                    }

                    KLIPS2023_HouseList houseList = (from house in TAPI.KLIPS2023_HouseList
                                                     where house.HID == long.Parse(personList.HID)
                                                     select house).SingleOrDefault();
                    ViewBag.hid = houseList.HID;
                    if ( houseList.유형 =="분가")
                    {
                        splitH = 1;
                    }

                    string QMListData = $@"<RPS_SamplingLIST_INFO>
                                                <hid VALUE=""{personList.HID}"" />                              
                                           </RPS_SamplingLIST_INFO>";

                    string query = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q230285_4]') AND type in (N'U'))
                    BEGIN
                        MERGE SURVEY.dbo.T_SamplingList_q230285_4 as SLIST
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

                    TAPI.SP_KLIPS2023_SETLIST(pid, "진행중", 3);
                }

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt(string.Format("qn=q230285_4&t=tapi&atype=1&pid={0}&uid={1}&pid2={2}&splith={3}&nv=true", pid, UID, pid2, splitH));
                }
                else
                {
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"Web\DATA\13643").Replace("/q230285_4", ""), pid);
                    FileInfo fi = new FileInfo(dataFile);
                    if (fi.Exists == false)
                    {
                        ViewBag.C = TripleDESCryptoService.Encrypt(string.Format("qn=q230285_1&t=tapi&atype=1&pid={0}&uid={1}&phid={3}&ppid2={4}&splith={2}&nv=true", pid, UID, splitH, personList.HID, pid2));
                    }
                    else
                    {
                        using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                        {
                            StreamReader srAnswerData = new StreamReader(dataFile, Encoding.Default);
                            string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());
                            XElement AnswerData = XElement.Parse(answerDataDecrypt);


                            srAnswerData.Close();
                            srAnswerData.Dispose();

                            T_Script t_Script = SURVEYTOOL.T_Scripts.Where(w => w.questionnaireID == 13643).First();

                            if (t_Script != null)
                            {
                                XElement variableList = t_Script.variableList;

                                answerList = (from element in variableList.Elements()
                                              select new AnswerList
                                              {
                                                  Variable = element.Name.ToString(),
                                                  Title = element.Value,
                                                  Answer = GetAnswerData(AnswerData, element.Name.ToString()),
                                                  Url = TripleDESCryptoService.Encrypt(string.Format("qn=q230285_4&pid={0}&uid={1}&t=tapi&eqnum={2}&pid2={3}&nv=true", pid, UID, element.Name.ToString(), pid2)),
                                              }).Where(w => string.IsNullOrEmpty(w.Answer) == false).ToList();
                            }
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

        public ActionResult AttitudeSurvey(long pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                List<AnswerList> answerList = new List<AnswerList>();
                ViewBag.pid = pid.ToString();
                
                KLIPS2023_PersonList personList = new KLIPS2023_PersonList();
                bool eqnum = false;
                string hid = "";
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    personList = (from surveyInfo in TAPI.KLIPS2023_PersonList
                                  where surveyInfo.PID == pid.ToString()
                                  select surveyInfo).SingleOrDefault();

                    if (personList.응답태도진행상태 != null && personList.응답태도진행상태 != "" && personList.응답태도진행상태 == "완료")
                    {
                        eqnum = true;

                    }
                    personList.응답태도진행상태 = "진행중";
                    hid = personList.HID;
                    ViewBag.hid = personList.HID;

                    TAPI.SubmitChanges();
                }

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt(string.Format("qn=q230285_3&t=tapi&atype=1&pid={0}&uid={1}&hid={2}&nv=true", pid, UID, hid));
                }
                else
                {
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"Web\DATA\13644").Replace("/q230285_3", ""), pid);

                    using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                    {
                        StreamReader srAnswerData = new StreamReader(dataFile, Encoding.Default);
                        string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());
                        XElement AnswerData = XElement.Parse(answerDataDecrypt);

                        srAnswerData.Close();
                        srAnswerData.Dispose();

                        T_Script t_Script = SURVEYTOOL.T_Scripts.Where(w => w.questionnaireID == 13644).First();

                        if (t_Script != null)
                        {
                            XElement variableList = t_Script.variableList;

                            answerList = (from element in variableList.Elements()
                                          select new AnswerList
                                          {
                                              Variable = element.Name.ToString(),
                                              Title = element.Value,
                                              Answer = GetAnswerData(AnswerData, element.Name.ToString()),
                                              Url = TripleDESCryptoService.Encrypt(string.Format("qn=q230285_3&pid={0}&uid={1}&hid={3}&t=tapi&eqnum={2}&nv=true", pid, UID, element.Name.ToString(), hid)),
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

        public ActionResult IncomeSuvey(long hid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                List<AnswerList> answerList = new List<AnswerList>();

                ViewBag.hid = hid.ToString();


                KLIPS2023_HouseList houseList = new KLIPS2023_HouseList();
                bool eqnum = false;
                int splitH = 0;
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    houseList = (from surveyInfo in TAPI.KLIPS2023_HouseList
                                 where surveyInfo.HID == hid
                                 select surveyInfo).SingleOrDefault();
                    ViewBag.hid = houseList.HID;

                    if (houseList.소득진행상태 != null && houseList.소득진행상태 != "" && houseList.소득진행상태 == "완료")
                    {
                        eqnum = true;

                    }
                    if (houseList.진행상태 == null || houseList.진행상태 == "")
                    {
                        houseList.진행상태 = "진행중";
                    }

                   
                    if (houseList.유형 == "분가")
                    {
                        splitH = 1;
                    }

                    houseList.소득진행상태 = "진행중";
                    string QMListData = $@"<RPS_SamplingLIST_INFO>
                                                <가구주 VALUE=""{houseList.가구주성명}"" />                                
                                            </RPS_SamplingLIST_INFO>";


                    string query = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q230285_6]') AND type in (N'U'))
                    BEGIN
                        MERGE SURVEY.dbo.T_SamplingList_q230285_6 as SLIST
                        USING
                        (
                            SELECT	'{0}' as PID
                        ) as Answer on SLIST.PID = Answer.PID
                        WHEN MATCHED THEN
                    	    UPDATE	SET	QMListData = N'{1}'
                        WHEN NOT MATCHED THEN
                    	    INSERT (PID, QMListData)
                    	    VALUES (PID, N'{1}');
                    END", hid, QMListData);

                    TAPI.ExecuteCommand(query, "");
                    TAPI.SubmitChanges();
                }

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt(string.Format("qn=q230285_6&t=tapi&atype=1&pid={0}&uid={1}&splith={2}&nv=true", hid, UID, splitH));
                }
                else
                {
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"Web\DATA\13645").Replace("/q230285_6", ""), hid);
                    
                    using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                    {
                        StreamReader srAnswerData = new StreamReader(dataFile, Encoding.Default);
                        string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());
                        XElement AnswerData = XElement.Parse(answerDataDecrypt);

                        srAnswerData.Close();
                        srAnswerData.Dispose();

                        T_Script t_Script = SURVEYTOOL.T_Scripts.Where(w => w.questionnaireID == 13645).First();

                        if (t_Script != null)
                        {
                            XElement variableList = t_Script.variableList;

                            answerList = (from element in variableList.Elements()
                                          select new AnswerList
                                          {
                                              Variable = element.Name.ToString(),
                                              Title = element.Value,
                                              Answer = GetAnswerData(AnswerData, element.Name.ToString()),
                                              Url = TripleDESCryptoService.Encrypt(string.Format("qn=q230285_6&pid={0}&uid={1}&t=tapi&eqnum={2}&splith={3}&nv=true", hid, UID, element.Name.ToString(), splitH)),
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

        public ActionResult InfoSheet(long hid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                ViewBag.hid = hid.ToString();

                KLIPS2023_InfoSheet infoSheet = new KLIPS2023_InfoSheet();
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    infoSheet = (from info in TAPI.KLIPS2023_InfoSheet
                                                     where info.HID == hid
                                                      select info).SingleOrDefault();
                    
                    
                }
                

                return View("InfoSheet", infoSheet);
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
