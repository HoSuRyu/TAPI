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
    public class q220614Controller : Controller
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

        public List<SP_q220614_GETLISTResult> GetList()
        {
            
            List<SP_q220614_GETLISTResult> list = new List<SP_q220614_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220614_GETLIST(UID, null).ToList();
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 12823).FirstOrDefault();

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

            List<SP_q220614_GETLISTResult> list = new List<SP_q220614_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220614_GETLIST(UID, null).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "완료").Count().ToString();
            }

            return PartialView("ProgressPartial", progressCount);
        }

        public List<SP_q220614_GETHOUSELISTResult> GetHouseList(int keyid)
        {
            List<SP_q220614_GETHOUSELISTResult> list = new List<SP_q220614_GETHOUSELISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220614_GETHOUSELIST(keyid, null).ToList();
                
                q220614_AreaList AreaList = TAPI.q220614_AreaList.First(f => f.HRC일련번호 == keyid);

                ViewBag.HRC일련번호 = AreaList.HRC일련번호;
                ViewBag.조사구번호 = AreaList.조사구번호;
                ViewBag.주소 = AreaList.전체주소;
            }

            List<string> statusList = list.GroupBy(g => g.진행상태).Where(w => string.IsNullOrEmpty(w.Key) == false).Select(s => s.Key).ToList();
            
            ViewBag.StatusList = statusList;

            return list;
        }

        public ActionResult HouseList(int keyid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }
            ViewBag.HRC일련번호 = keyid;
            return View("HouseList", GetHouseList(keyid));
        }

        public ActionResult HouseListPartial(int keyid)
        {
            return PartialView("HouseListPartial", GetHouseList(keyid));
        }
        

        public List<SP_q220614_GETPERSONLISTResult> GetPersonList(int keyid, int hid)
        {
            List<SP_q220614_GETPERSONLISTResult> list = new List<SP_q220614_GETPERSONLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220614_GETPERSONLIST(UID, hid).ToList();

                SP_q220614_GETHOUSELISTResult GETHOUSELISTResult = TAPI.SP_q220614_GETHOUSELIST(keyid, hid).First();

                ViewBag.HID = hid.ToString();

                ViewBag.HRC일련번호 = GETHOUSELISTResult.HRC일련번호;
                ViewBag.가구일련번호 = GETHOUSELISTResult.HID;
                ViewBag.가구번호 = GETHOUSELISTResult.가구번호;
                ViewBag.거처번호 = GETHOUSELISTResult.거처번호;
                ViewBag.주소 = GETHOUSELISTResult.주소;
                ViewBag.세부주소 = GETHOUSELISTResult.세부주소건물명;
                ViewBag.가구주이름 = GETHOUSELISTResult.가구주이름;
                ViewBag.집전화 = GETHOUSELISTResult.집전화;
                ViewBag.휴대전화 = GETHOUSELISTResult.휴대전화;
                ViewBag.적격가구원수 = GETHOUSELISTResult.적격가구원수;
                ViewBag.완료가구원수 = GETHOUSELISTResult.완료가구원수;
                ViewBag.가구원수 = GETHOUSELISTResult.가구원수;
                ViewBag.생성가구원수 = GETHOUSELISTResult.생성가구원수;
                ViewBag.가구특이사항메모 = GETHOUSELISTResult.가구특이사항메모;
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

        public string PersonInfoCheck(int keyid, int hid)
        {

            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                
                int cc = TAPI.q220614_PersonList.Count(c => c.HRC일련번호 == keyid && c.HID == hid && (c.가구주와의관계코드 == 1 || c.가구주와의관계코드 == 2));
                if (cc == 0)
                {
                    return "가구에 가구주/가구주의 배우자가 없습니다.\r가구주의 관계 코드를 다시 확인해 주십시오. ";
                }
            }
            return "";
        }
            
        public string SaveInfo(int keyid, int hid,  string sebuAddr, string tel,  string phone, int personCount, string houseMemo)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {

                q220614_HouseList HouseList = TAPI.q220614_HouseList.FirstOrDefault(f => f.HRC일련번호 == keyid && f.HID == hid);

                HouseList.세부주소건물명 = sebuAddr;
                if (tel != "" && tel != "--")
                {   
                    int telC = TAPI.q220614_HouseList.Count(f => f.집전화 == tel && f.HID != hid);
                    if (telC > 0)
                    {
                        return "중복된 집전화 번호를 입력하셨습니다.\r\n연락처를 확인해 주세요.";
                    }
                    HouseList.집전화 = tel;
                }
                if (phone != "" && phone != "--")
                {
                    int phoneC = TAPI.q220614_HouseList.Count(f => f.휴대전화 == phone && f.HID != hid);
                    if( phoneC > 0)
                    {
                        return "중복된 휴대전화 번호를 입력하셨습니다.\r\n연락처를 확인해 주세요.";
                    }
                    HouseList.휴대전화 = phone;
                }


                HouseList.가구원수 = personCount;
                HouseList.가구특이사항메모 = houseMemo;

                TAPI.SubmitChanges();
            }

            return "";
        }

        public string CreatePerson(int keyid, int hid, string ftype, string fTypeEtc, string name, string gender, int byear
            ,string sebuAddr, string tel, string phone, int personCount, string houseMemo)
        {
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {

                if (phone != "" && phone != "--")
                {
                    
                    int phoneC = TAPI.q220614_HouseList.Count(f => f.휴대전화 == phone && f.HID != hid);
                    if (phoneC > 0)
                    {
                        return "중복된 휴대전화 번호를 입력하셨습니다.\r\n연락처를 확인해 주세요.";
                    }
                }
                if (tel != "" && tel != "--")
                {
                    int telC = TAPI.q220614_HouseList.Count(f => f.집전화 == tel && f.HID != hid);
                    if (telC > 0)
                    {
                        return "중복된 집전화 번호를 입력하셨습니다.\r\n연락처를 확인해 주세요.";
                    }
                }
                string[] ftypeArr = ftype.Split('.');
                if( ftypeArr[0] == "1" || ftypeArr[0] == "2")
                {
                    int cc = TAPI.q220614_PersonList.Count(c => c.HRC일련번호 == keyid && c.HID == hid && c.가구주와의관계코드 == int.Parse(ftypeArr[0]));
                    if( cc > 0)
                    {
                        return "가구에 이미 가구주/가구주의 배우자가 있습니다.\r가구주와의 관계 코드를 다시 확인해 주십시오.";
                    }
                    else
                    {
                        int dftype = 1;
                        if( ftypeArr[0]=="1")
                        {
                            dftype = 2;
                        }

                        cc = TAPI.q220614_PersonList.Count(c => c.HRC일련번호 == keyid && c.HID == hid && c.가구주와의관계코드 == dftype);
                        if (cc > 0)
                        {
                            q220614_PersonList Person = TAPI.q220614_PersonList.First(f => f.HID == hid && f.가구주와의관계코드 == dftype);
                            if (Person != null && Person.성별 == gender)
                            {
                                return "가구주와 가구주의 배우자의 성별이 같을 수 없습니다.\r성별을 다시 확인해주십시오. ";
                            }
                        }
                    }
                }

                int i = TAPI.q220614_PersonList.Count(c => c.HRC일련번호 == keyid && c.HID == hid) + 1;
                
                q220614_PersonList PersonList = new q220614_PersonList();

                PersonList.HRC일련번호 = keyid;
                PersonList.HID = hid;
                PersonList.면접원아이디 = UID.ToString();

                int PID = (hid * 100) + i;
                PersonList.PID = PID;

                PersonList.가구주와의관계코드 = int.Parse(ftypeArr[0]);
                PersonList.가구주와의관계 = ftypeArr[1];
                PersonList.가구주와의관계기타 = fTypeEtc;
                PersonList.성명 = name;
                PersonList.성별 = gender;
                PersonList.생년 = byear;
                
                
                if( byear<2004)
                {
                    PersonList.조사대상여부 = "적격";
                }
                else
                {
                    PersonList.조사대상여부 = "비적격";
                }
                
                q220614_HouseList HouseList = TAPI.q220614_HouseList.FirstOrDefault(f => f.HRC일련번호 == keyid && f.HID == hid);
                if (ftypeArr[0] == "1")
                {
                    HouseList.가구주이름 = name;
                }
                HouseList.가구특이사항메모 = houseMemo;

                TAPI.q220614_PersonList.InsertOnSubmit(PersonList);
                TAPI.SubmitChanges();

                TAPI.SP_q220614_SETHOUSE(PID, "미완료");
                
            }

            return SaveInfo( keyid,  hid,  sebuAddr,  tel,  phone,  personCount, houseMemo);
        }

        public ActionResult PersonInfo(int keyid, int hid, int pid)
        {
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220614_PersonList Person = TAPI.q220614_PersonList.First(f => f.PID == pid);

                
                ViewBag.HRC일련번호 = keyid;
                ViewBag.가구일련번호 = hid;
                ViewBag.가구원번호 = pid;

                ViewBag.ptype = Person.가구주와의관계코드 + "." + Person.가구주와의관계;
                ViewBag.ftypeCode = Person.가구주와의관계코드;


                return View("PersonInfo", Person);
            }
        }


        public string PersonInfoOK(int pid, string ftype, string fTypeEtc, string name, string gender, int byear, string contact, string phone, int hid)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                string[] ftypeArr = ftype.Split('.');
                if(ftypeArr[0] == "1" || ftypeArr[0] == "2")
                {
                    int cc = TAPI.q220614_PersonList.Count(c => c.PID != pid && c.가구주와의관계코드 == int.Parse(ftypeArr[0]) && c.HID==hid);
                    if( cc > 0)
                    {
                        return "가구에 이미 가구주/가구주의 배우자가 있습니다.\r가구주와의 관계 코드를 다시 확인해 주십시오.";
                    }
                }

                q220614_PersonList Person = TAPI.q220614_PersonList.FirstOrDefault(f => f.PID == pid);

                Person.가구주와의관계코드 = int.Parse(ftypeArr[0]);
                Person.가구주와의관계 = ftypeArr[1];
                Person.성명 = name;
                Person.성별 = gender;
                Person.생년 = byear;
                if (ftypeArr[0] == "9")
                {
                    Person.가구주와의관계기타 = fTypeEtc;
                }
                else
                {
                    Person.가구주와의관계기타 = "";
                }
                if (byear < 2004)
                {
                    Person.조사대상여부 = "적격";
                }
                else
                {
                    Person.조사대상여부 = "비적격";
                }

                Person.휴대폰번호 = phone;

                if(contact == "" )
                {
                    contact = null;
                }
                else
                {
                    Person.접촉결과 = contact;
                }
            
                
                if ( contact== "가구원 아님")
                {
                    Person.조사대상여부 = "";

                    Person.가구주와의관계코드 = 9;
                    Person.가구주와의관계 = "기타";
                    Person.가구주와의관계기타 = "가구원 아님";
                }

                TAPI.SubmitChanges();

                if (contact != null && Person.진행상태 != "완료")
                {
                    TAPI.SP_q220614_SETHOUSE(pid, contact);
                }
                else
                {
                    TAPI.SP_q220614_SETHOUSE(pid, Person.진행상태);
                   
                }

                
            }

            return "저장";
        }

        public ActionResult Survey(int keyid, int hid, int pid, string ftype)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {

                string[] ftypeArr = ftype.Split('.');
                int relcode = int.Parse(ftypeArr[0]);
                List<AnswerList> answerList = new List<AnswerList>();
                
                bool eqnum = false;
                int qID = 12823;
                int answerStateCode = 0;
                using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q220614 WHERE PID = '{0}'", pid)).FirstOrDefault();
                    answerStateCode = t_SamplingList != null ? (int)(t_SamplingList.answerStateCode != null ? t_SamplingList.answerStateCode : 0) : 0;
                }

                int gender = 1;
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    q220614_PersonList PersonList = TAPI.q220614_PersonList.First(f => f.PID == pid);
                    
                    if( PersonList.성별 == "여")
                    {
                        gender = 2;
                    }
                    if ((string.IsNullOrEmpty(PersonList.진행상태) == false && PersonList.진행상태 == "완료") || answerStateCode == 4)
                    {
                        eqnum = true;
                    }
                    TAPI.SP_q220614_SETHOUSE(pid, "진행중");
                    TAPI.SubmitChanges();
                }

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q220614&pid={pid}&keyid={keyid}&hid={hid}&uid={UID}&atype=1&relcode={relcode}&gender={gender}&t=TAPI");
                }
                else
                {
                    ViewBag.HRC일련번호 = keyid;
                    ViewBag.가구일련번호 = hid.ToString(); 
                    ViewBag.가구원번호 = pid;

                    
                    
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + qID.ToString()).Replace("/q220614", ""), pid);
                    
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
                                              Url = TripleDESCryptoService.Encrypt($"qn=q220614&pid={pid}&keyid={keyid}&hid={hid}&t=TAPI&atype=1&relcode={relcode}&gender={gender}&eqnum={element.Name.ToString()}"),
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
        
        public string SendMSG(int pid , string phone, string ftype ,string fTypeEtc, string name,string gender,int byear,int hid)
        {

            string r = PersonInfoOK(pid, ftype, fTypeEtc, name, gender,  byear,   "", phone, hid);
            if( r != "저장")
            {
                return r;
            }

            string[] ftypeArr = ftype.Split('.');            
            int relcode = int.Parse(ftypeArr[0]);
            int genderCode = 1;
            if (gender == "여")
            {
                genderCode = 2;
            }
            using (SENDMASTERDataContext SENDMASTER = new SENDMASTERDataContext("Data Source=172.19.100.85,15000;Initial Catalog=SEND_MASTER;Persist Security Info=True;User ID=hrcdv;password=Hrc#3014!#"))
            {
                string mm = string.Format(@"안녕하십니까? 국가인권위원회와 ㈜한국리서치에서는 「2022년 인권의식실태조사」를 
실시하고 있으며, 귀댁이 올해 「2022년 인권의식실태조사」의 대상 가구로 선정되어 설문링크를 보내드립니다.

아래 설문링크를 통해 조사에 참여부탁드립니다.

□	설문링크 : https://rpssurvey.hrcglobal.com/?qn=q220614&pid={0}&atype=2&gender={2}&relcode={1}
□	소요시간 : 약 15분 내외
□	답례품 : 모바일상품권 5천원권

□	공문 : https://rpssurvey.hrcglobal.com/Media/12823/220614.pdf
□	팜플렛 : https://rpssurvey.hrcglobal.com/Media/12823/12823.pdf
□  인권의식실태조사 홈페이지 : https://nshrc.kr

인권의식실태조사는 우리 국민의 인권의식 및 국내 인권상황에 대한 평가와 일상생활에서 
직면하는 인권침해 및 차별경험을 파악함으로써, 인권 증진 방안과 인권정책 도출을 위한 
기초자료 제공을 목적으로 실시하는 국가승인통계(제129001호) 조사입니다. 본 조사에 응해주신
귀하의 응답 내용은 통계작성 목적으로만 사용되며, 통계법 제33조(비밀의 보호)에 따라 엄격히 보호됨을 알려드립니다.
바쁘시겠지만 잠시 시간을 내어 협조 부탁드립니다.

□	조사 관련 문의처 : 02-3014-1025
", pid, relcode,genderCode);

                SENDMASTER.PROC_SEND_MESSAGE("interviwer", "0230141025", phone, "2022년 인권의식실태조사 참여부탁드립니다.", mm, DateTime.Now, "", "2022-96-0614");

            }
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {

                q220614_PersonList Person = TAPI.q220614_PersonList.FirstOrDefault(f => f.PID == pid);
                Person.휴대폰번호 = phone;
                TAPI.SubmitChanges();

                TAPI.SP_q220614_SETHOUSE(pid, "문자발송");
            }
            return "성공";
        }


        public ActionResult Contact(int hid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }
            
            List<q220614_Contact> ContactList = new List<q220614_Contact>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220614_HouseList HouseList = TAPI.q220614_HouseList.FirstOrDefault(f => f.HID == hid);


                ContactList = (from contact in TAPI.q220614_Contact
                                       where contact.HID == hid
                                       select contact).ToList();

                ViewBag.HRC일련번호 = HouseList.HRC일련번호;
                ViewBag.가구일련번호 = HouseList.HID;
                ViewBag.가구번호 = HouseList.가구번호;
                ViewBag.거처번호 = HouseList.거처번호;
                ViewBag.주소 = HouseList.주소;
                ViewBag.세부주소 = HouseList.세부주소건물명;

                ViewBag.Ranking = (ContactList.Count + 1).ToString();

                ViewBag.contactYear = DateTime.Now.ToString("yyyy");
                ViewBag.contactMonth = DateTime.Now.ToString("MM");
                ViewBag.contactDay = DateTime.Now.ToString("dd");
                
            }

            return View("Contact", ContactList);
        }

        public string ContactOK(int hid, int ranking, string contact, string contactMonth, string contactDay, string contactTime, string etc)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            q220614_Contact Contact = new q220614_Contact();
            List<q220614_Contact> ContactList = new List<q220614_Contact>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {

                string contactDate = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;
                ContactList = (from contactT in TAPI.q220614_Contact
                                       where contactT.HID == hid & contactT.방문일자== contactDate & contactT.방문시간== contactTime
                                       select contactT).ToList();

                if(ContactList.Count > 0)
                {
                    return "같은 시간대 방문기록이 이미 있습니다.";
                }
                q220614_HouseList HouseList = TAPI.q220614_HouseList.FirstOrDefault(f => f.HID == hid);

                HouseList.접촉일지상태 = "기입";

                string status = "진행중";
                if (contact.Contains("31") || contact.Contains("32") || contact.Contains("33") || contact.Contains("34") || contact.Contains("39"))
                {
                    status = "거절";
                }
                else if (contact.Contains("41") || contact.Contains("42") || contact.Contains("43") || contact.Contains("44") || contact.Contains("49"))
                {
                    status = "조사불가";
                }
                HouseList.진행상태 = status;

                Contact.HID = hid;
                Contact.면접원아이디 = UID.ToString();
                Contact.방문횟수 = ranking;
                Contact.방문일자 = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;
                Contact.방문시간 = contactTime;
                Contact.비성공사유 = contact;
                Contact.비성공사유기타 = etc;

                Contact.inputDate = DateTime.Now;

                TAPI.q220614_Contact.InsertOnSubmit(Contact);
                

                TAPI.SubmitChanges();

                TAPI.SP_q220614_SETAREA(hid);
            }

            return "저장";
        }

        public ActionResult Replace(int keyid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220614_AreaList AreaList = TAPI.q220614_AreaList.First(f => f.HRC일련번호 == keyid);

                return View("Replace", AreaList);
            }
        }

        public ActionResult ReplaceOK(int keyid, string status, string etc)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            q220614_Replace Replace = new q220614_Replace();
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220614_AreaList AreaList = TAPI.q220614_AreaList.First(f => f.HRC일련번호 == keyid);
                AreaList.진행상태 = "대체요청";


                Replace.HRC일련번호 = keyid;
                Replace.면접원아이디 = UID.ToString();
                Replace.상태 = "대체요청";
                Replace.대체사유 = status;
                Replace.세부사유 = etc;
                Replace.대체요청시간 = DateTime.Now;

                TAPI.q220614_Replace.InsertOnSubmit(Replace);

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