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
    public class q220945Controller : Controller
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

        public List<SP_q220945_GETLISTResult> GetList()
        {
            List<SP_q220945_GETLISTResult> list = new List<SP_q220945_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220945_GETLIST(UID, null).ToList();
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 12925).FirstOrDefault();

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

            List<SP_q220945_GETLISTResult> list = new List<SP_q220945_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220945_GETLIST(UID, null).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                //progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "완료").Count().ToString();
                progressCount.End = list.Where(w => w.완료수 != 0).Select(s => s.완료수).Sum().ToString();

            }

            return PartialView("ProgressPartial", progressCount);
        }

        public List<SP_q220945_GETHOUSELISTResult> GetHouseList(int keyid)
        {
            List<SP_q220945_GETHOUSELISTResult> list = new List<SP_q220945_GETHOUSELISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220945_GETHOUSELIST(UID, keyid, null).ToList();

                q220945_AreaList AreaList = TAPI.q220945_AreaList.First(f => f.조사구일련번호 == keyid);

                ViewBag.조사구일련번호 = AreaList.조사구일련번호;
                ViewBag.조사구번호 = AreaList.조사구번호;
                ViewBag.주소 = $"{AreaList.시도} {AreaList.시군구} {AreaList.읍면동}";
            }

            return list;
        }

        public ActionResult HouseList(int keyid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            return View("HouseList", GetHouseList(keyid));
        }

        public ActionResult HouseListPartial(int keyid)
        {
            return PartialView("HouseListPartial", GetHouseList(keyid));
        }

        public string CreateHouse(int keyid, string param1, string param2)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220945_HouseList HouseList = TAPI.q220945_HouseList.FirstOrDefault(f => f.조사구일련번호 == keyid && f.거처번호 == param1 && f.가구번호 == param2);

                if (HouseList == null)
                {
                    TAPI.SP_q220945_INSERTHOUSE(UID, keyid, param1, param2);
                }
                else
                {
                    result = "이미 생성된 거쳐번호/가구번호 입니다. 다시 확인 해 주세요.";
                }
            }

            return result;
        }

        public List<SP_q220945_GETPERSONLISTResult> GetPersonList(int keyid, int hid)
        {
            List<SP_q220945_GETPERSONLISTResult> list = new List<SP_q220945_GETPERSONLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220945_GETPERSONLIST(UID, hid, null).ToList();

                SP_q220945_GETHOUSELISTResult HOUSELISTResult = TAPI.SP_q220945_GETHOUSELIST(UID, keyid, hid).First();

                ViewBag.HID = hid.ToString();
                ViewBag.조사구일련번호 = HOUSELISTResult.조사구일련번호;

                ViewBag.주소 = HOUSELISTResult.주소;
                ViewBag.세부주소 = HOUSELISTResult.세부주소;
                ViewBag.주택유형 = HOUSELISTResult.주택유형;
                ViewBag.주택유형기타 = HOUSELISTResult.주택유형기타;
                ViewBag.응답자성명 = HOUSELISTResult.응답자성명;
                ViewBag.응답자연락처 = HOUSELISTResult.응답자연락처;

                ViewBag.총가구원수 = HOUSELISTResult.총가구원수;
                ViewBag.적격여성수 = HOUSELISTResult.적격여성수;
                ViewBag.완료수 = HOUSELISTResult.완료수;
                ViewBag.진행수 = HOUSELISTResult.진행수;
                ViewBag.거처번호 = HOUSELISTResult.거처번호;
                ViewBag.가구번호 = HOUSELISTResult.가구번호;
                ViewBag.가구원대체 = HOUSELISTResult.가구원대체;

                
            }

            return list;
        }

        public ActionResult PersonList(int keyid, int hid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            return View("PersonList", GetPersonList(keyid, hid));
        }

        public ActionResult PersonListPartial(int keyid, int hid)
        {
            return PartialView("PersonListPartial", GetPersonList(keyid, hid));
        }

        public string SaveInfo(int keyid, int hid, string address, string sebuAddr, string pname, string hType, string hTypeEtc, string tel, int personCount)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                int checkTel = TAPI.q220945_HouseList.Where(w => w.HID != hid && w.응답자연락처 == tel).Count();
                if( checkTel > 0)
                {
                    return "중복된 연락처를 입력하셨습니다. \r\n연락처를 확인해주세요.";
                }

                q220945_HouseList HouseList = TAPI.q220945_HouseList.First(f => f.HID == hid);

                if (address != "")
                {
                    HouseList.주소 = address;
                }
                HouseList.세부주소 = sebuAddr;


                HouseList.주택유형 = hType;
                HouseList.주택유형기타 = hTypeEtc;
                HouseList.응답자성명 = pname;
                HouseList.응답자연락처 = tel;
             
                HouseList.총가구원수 = personCount;

                TAPI.SubmitChanges();
            }

            return "";
        }

        public string CreatePerson(int keyid, int hid)
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
                    int i = TAPI.q220945_PersonList.Count(c => c.조사구일련번호 == keyid && c.HID == hid) + 1;
                    int? personCount = TAPI.q220945_HouseList.First(f => f.조사구일련번호 == keyid && f.HID == hid).총가구원수;

                    for (; i <= personCount; i++)
                    {
                        TAPI.SP_q220945_INSERTPERSON(UID, hid);
                    }
                }
            }
            catch
            {
                result = "오류가 발생 했습니다.";
            }

            return result;
        }

        public ActionResult PersonInfo(int keyid, int hid, int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {

                q220945_PersonList personList = TAPI.q220945_PersonList.Where(f => f.PID == pid).First();

                return View("PersonInfo", personList);
            }
        }

        public string SavePersonInfo(int keyid, int hid, long pid, string pname, string gaguju, string gender, string lunar, int byear, int bmonth, int bday, string ptype, string phone)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                //if (phone != "")
                //{
                //    int checkPhone = TAPI.q220945_PersonList.Where(w => w.PID != pid && w.휴대폰번호 == phone).Count();
                //    if( checkPhone > 0)
                //    {
                //        return "중복된 연락처를 입력하셨습니다. \r\n연락처를 확인해주세요.";
                //    }
                //}
                

                q220945_PersonList personList = TAPI.q220945_PersonList.FirstOrDefault(f => f.PID == pid);

         
                if (new string[] { "가구주", "배우자" }.Contains(gaguju) == true)
                {
                    q220945_PersonList GAGUJU = TAPI.q220945_PersonList.FirstOrDefault(f => f.HID == hid && f.PID != pid && f.가구주와의관계 == gaguju);

                    if (GAGUJU != null)
                    {
                        return $"가구주와의 관계에서 '{gaguju}'가 이미 등록 되어있습니다.";
                    }
                }
                personList.성명 = pname;
                personList.가구주와의관계 = gaguju;
                personList.성별 = gender;
                personList.양음력 = lunar;
                personList.적격여부 = ptype;
                if( phone != "")
                {
                    personList.휴대폰번호 = phone;
                }

                if (gaguju == "가구원 아님")
                {
                    
                    personList.생년 = null;
                    personList.생월 = null;
                    personList.생일 = null;
                    personList.만나이 = null;

                }
                else
                {
                    personList.생년 = byear.ToString();
                    personList.생월 = bmonth.ToString("00");
                    personList.생일 = bday.ToString("00");

                    // 만나이 계산
                    int calYear = byear;
                    
                    if( (lunar == "양력" && (bmonth > 5 || (bmonth==5 && bday>=17)) )
                        || (lunar == "음력" && (bmonth > 4 || (bmonth == 4 && bday >= 9))))
                    {
                        calYear++;
                    }
                    personList.만나이 = (2022 - calYear).ToString();

                }
                if (ptype == "적격")
                {
                    int? _시도코드 = TAPI.q220945_AreaList.First(f => f.조사구일련번호 == keyid).시도코드;

                    string QMListData = $@"<RPS_SamplingLIST_INFO>
    <양음력 VALUE=""{lunar}"" />
    <생년 VALUE=""{byear}"" />
    <생월 VALUE=""{bmonth}"" />
    <생일 VALUE=""{bday}"" />
    <시도코드 VALUE=""{_시도코드}"" />
</RPS_SamplingLIST_INFO>";

                    string query = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q220945]') AND type in (N'U'))
BEGIN
    MERGE SURVEY.dbo.T_SamplingList_q220945 as SLIST
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

                    if (TAPI.ExecuteCommand(query, "") < 1)
                    {
                        result = "정보 저장 오류 다시 시도 해 주세요.";
                    }
                }

                TAPI.SubmitChanges();
            }

            return result;
        }

        public string CheckPersonInfo(int keyid, int hid)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                int emptyCount = TAPI.q220945_PersonList.Where(w => w.HID == hid && ((w.가구주와의관계 ?? "") == "" ||
                                                                                                 (w.성명 ?? "") == "" ||
                                                                                                 (w.성별 ?? "") == "" ||
                                                                                                 (w.생년 ?? "") == "" ||
                                                                                                 (w.생월 ?? "") == "" ||
                                                                                                 (w.생일 ?? "") == "") && (w.가구주와의관계 ?? "") != "가구원 아님").Count();


                int answerCount = TAPI.q220945_PersonList.Where(w => w.HID == hid && (w.성명 ?? "") != "").Count();
                q220945_HouseList HouseList = TAPI.q220945_HouseList.FirstOrDefault(f => f.HID == hid);
                if (emptyCount > 0 || answerCount < HouseList.총가구원수)
                {
                    result = "가구원 정보를 모두 입력 해 주세요.";
                }
            }

            return result;
        }

        public ActionResult Survey(int keyid, int hid, int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                List<AnswerList> answerList = new List<AnswerList>();
                
                bool eqnum = false;
                int qID = 12925;

                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    List<SP_q220945_GETPERSONLISTResult> personlist = new List<SP_q220945_GETPERSONLISTResult>();
                    personlist = TAPI.SP_q220945_GETPERSONLIST(UID, hid, pid).ToList();

                    if ((string.IsNullOrEmpty(personlist[0].설문진행상태) == false && personlist[0].설문진행상태 != "진행중"  ) || personlist[0].answerStateCode == 4 || personlist[0].answerStateCode == 1)
                    {
                        eqnum = true;
                    }

                    TAPI.SP_q220945_SETLIST(pid, "진행중", "");
                }

                
                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q220945&pid={pid}&keyid={keyid}&hid={hid}&t=TAPI&atype=1");
                }
                else
                {
                    ViewBag.조사구일련번호 = keyid;
                    ViewBag.HID = hid.ToString();                    

                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + qID.ToString()).Replace("/q220945", ""), pid);

                    

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
                                              Url = TripleDESCryptoService.Encrypt($"qn=q220945&pid={pid}&keyid={keyid}&hid={hid}&atype=1&t=TAPI&eqnum={element.Name.ToString()}"),
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

        public ActionResult Contact(int keyid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220945_Contact Contact = TAPI.q220945_Contact.FirstOrDefault(f => f.조사구일련번호 == keyid && f.면접원아이디 == UID);

                if (Contact == null)
                {
                    Contact = new q220945_Contact();

                    Contact.조사구일련번호 = keyid;
                    Contact.면접원아이디 = UID;
                    Contact.성공비성공 = 0;
                    Contact.성공 = 0;
                    Contact.비성공 = 0;
                    Contact.비성공A = 0;
                    Contact.비성공B = 0;
                    Contact.비성공C = 0;
                    Contact.비성공D = 0;
                    Contact.비성공E = 0;
                    Contact.미접촉 = 0;
                    Contact.총방문횟수 = 0;
                    Contact.완료여부 = 0;

                    TAPI.q220945_Contact.InsertOnSubmit(Contact);
                }

                Contact.성공 = TAPI.SP_q220945_GETHOUSELIST(UID, keyid, null).Count(c => c.완료수 > 0);
                TAPI.SubmitChanges();

                q220945_AreaList AreaList = TAPI.q220945_AreaList.First(f => f.조사구일련번호 == keyid);

                ViewBag.조사구일련번호 = AreaList.조사구일련번호;
                ViewBag.조사구주소 = $"{AreaList.시도} {AreaList.시군구} {AreaList.읍면동}";
                ViewBag.전체가구수 = AreaList.가구수;

                return View("Contact", Contact);
            }
        }

        public string SaveContact(int keyid, int con0, int con1, int con2, int con3, int con4, int con5, int con6, int con7, int con8, int con9)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220945_Contact Contact = TAPI.q220945_Contact.FirstOrDefault(f => f.조사구일련번호 == keyid && f.면접원아이디 == UID);

                Contact.성공비성공 = con0;
                Contact.성공 = con1;
                Contact.비성공 = con2;
                Contact.비성공A = con3;
                Contact.비성공B = con4;
                Contact.비성공C = con5;
                Contact.비성공D = con6;
                Contact.비성공E = con7;
                Contact.미접촉 = con8;
                Contact.총방문횟수 = con9;

                TAPI.SubmitChanges();
            }

            return "";
        }

        public ActionResult HouseContact(int keyid, int hid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            List<q220945_HouseContact> HouseContact = new List<q220945_HouseContact>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                HouseContact = (from contact in TAPI.q220945_HouseContact
                                        where contact.HID == hid
                                        select contact).ToList();

                ViewBag.Ranking = (HouseContact.Count + 1).ToString();

                ViewBag.contactMonth = DateTime.Now.ToString("MM");
                ViewBag.contactDay = DateTime.Now.ToString("dd");
                ViewBag.contactHour = DateTime.Now.ToString("HH");
                ViewBag.contactMin = DateTime.Now.ToString("mm");

                ViewBag.조사구일련번호 = keyid;
                ViewBag.HID = hid;
            }

            return View("HouseContact", HouseContact);
        }

        public string SaveHouseContact(int hid, string contactMonth, string contactDay, string contactHour, string contactMin, int statusCode, string status, string etc, int ranking)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            Dictionary<int, string> _코드를그룹레이블로 = new Dictionary<int, string>();
            _코드를그룹레이블로[11] = "완료";
            _코드를그룹레이블로[21] = "진행";
            _코드를그룹레이블로[22] = "진행";
            _코드를그룹레이블로[23] = "진행";
            _코드를그룹레이블로[31] = "불응";
            _코드를그룹레이블로[32] = "불응";
            _코드를그룹레이블로[33] = "불응";
            _코드를그룹레이블로[34] = "불응";
            _코드를그룹레이블로[39] = "불응";
            _코드를그룹레이블로[41] = "불능";
            _코드를그룹레이블로[42] = "불능";
            _코드를그룹레이블로[43] = "불능";
            _코드를그룹레이블로[44] = "불능";
            _코드를그룹레이블로[49] = "불능";
            _코드를그룹레이블로[91] = "부적격";


            q220945_HouseContact HouseContact = new q220945_HouseContact();
            

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                HouseContact.HID = hid;
                HouseContact.면접원아이디 = UID.ToString();
                HouseContact.방문횟수 = ranking;
                HouseContact.방문일자 = "2022-" + contactMonth + "-" + contactDay;
                HouseContact.방문시간 = $"{contactHour}:{contactMin}";

                HouseContact.비성공사유 = status;
                HouseContact.비성공사유기타 = etc;
                HouseContact.inputDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                TAPI.q220945_HouseContact.InsertOnSubmit(HouseContact);

                q220945_HouseList HouseList = TAPI.q220945_HouseList.FirstOrDefault(f => f.HID == hid);
                if (_코드를그룹레이블로.ContainsKey(statusCode) == true)
                {
                    HouseList.최종접촉상태 = _코드를그룹레이블로[statusCode];
                    HouseList.진행상태 = _코드를그룹레이블로[statusCode];
                }

                TAPI.SubmitChanges();
            }

            return "";
        }

        public ActionResult Replace(int keyid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220945_AreaList AreaList = TAPI.q220945_AreaList.First(f => f.조사구일련번호 == keyid);

                return View("Replace", AreaList);
            }
        }

        public ActionResult ReplaceOK(int keyid, string status, string etc)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {

                q220945_Replace Replace = new q220945_Replace();

                Replace.조사구일련번호 = keyid;
                Replace.면접원아이디 = UID.ToString();
                Replace.대체사유 = status;
                Replace.대체세부사유 = etc;
                Replace.대체요청일자 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                TAPI.q220945_Replace.InsertOnSubmit(Replace);


                q220945_AreaList AreaList = TAPI.q220945_AreaList.First(f => f.조사구일련번호 == keyid);
                AreaList.진행상태 = "대체요청";
                TAPI.SubmitChanges();
            }

            return null;
        }

        public string SendMSG(int pid, string phone, int keyid, int hid)
        {
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {

                q220945_PersonList Person = TAPI.q220945_PersonList.FirstOrDefault(f => f.PID == pid);
                if( Person.적격여부 != "적격")
                {
                    return "가구원 정보를 저장 후 진행해 주세요.";
                }
            }


            using (SENDMASTERDataContext SENDMASTER = new SENDMASTERDataContext("Data Source=172.19.100.85,15000;Initial Catalog=SEND_MASTER;Persist Security Info=True;User ID=hrcdv;password=Hrc#3014!#"))
            {
                string mm = string.Format(@"안녕하십니까? 여성가족부와 한국여성정책연구원은 ㈜한국리서치와 함께 「2022년 경력단절여성등의 경제활동실태조사」를 실시하고 있으며, 귀댁이 올해 「2022년 경력단절여성등의 경제활동실태조사」의 대상 가구로 선정되어 설문링크를 보내드립니다.

아래 설문링크를 통해 조사에 참여부탁드립니다.

□	설문링크 : https://rpssurvey.hrcglobal.com/?qn=q220945&pid={0}&keyid={1}&hid={2}&atype=2
□	소요시간 : 약 20분 내외
□	답례품 : 모바일상품권 1만원권

□	공문 : https://rpssurvey.hrcglobal.com/Media/12925/2220945.pdf
□	팜플렛 : https://rpssurvey.hrcglobal.com/Media/12925/12925.pdf

경력단절여성등의 경제활동실태조사는 여성가족부, 한국여성정책연구원이 함께 결혼, 임신, 출산, 육아와 가족구성원 돌봄 등의 이유로 노동시장에서 경력이 단절된 여성 등의 경제활동 실태를 파악하여 재취업을 지원하고, 여성의 경력 유지와 관련된 정책을 수립하기 위한 자료로 활용하기 위해 실시하는 국가승인통계(제154020호) 조사입니다. 본 조사에 응해주신 귀하의 응답 내용은 통계작성 목적으로만 사용되며, 통계법 제33조(비밀의 보호)에 따라 엄격히 보호됨을 알려드립니다.
바쁘시겠지만 잠시 시간을 내어 협조 부탁드립니다

□	조사 관련 문의처 : 02-3014-1025
", pid, keyid, hid);

                SENDMASTER.PROC_SEND_MESSAGE("interviwer", "0230141025", phone, "2022년 경력단절여성등의 경제활동실태조사 참여부탁드립니다.", mm, DateTime.Now, "", "2022-96-0945");

            }
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {

                q220945_PersonList Person = TAPI.q220945_PersonList.FirstOrDefault(f => f.PID == pid);
                Person.휴대폰번호 = phone;
                TAPI.SubmitChanges();

                TAPI.SP_q220945_SETLIST(pid, "문자발송", "");
                
            }
            return "성공";
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