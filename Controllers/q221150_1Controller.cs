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
    public class q221150_1Controller : Controller
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

        public List<SP_q221150_GETLISTResult> GetList()
        {
            List<SP_q221150_GETLISTResult> list = new List<SP_q221150_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221150_GETLIST(UID, null).ToList();
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 13051).FirstOrDefault();

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

            List<SP_q221150_GETLISTResult> list = new List<SP_q221150_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221150_GETLIST(UID, null).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "완료").Count().ToString();
                //progressCount.End = list.Where(w => w.완료수 != 0).Select(s => s.완료수).Sum().ToString();

            }

            return PartialView("ProgressPartial", progressCount);
        }

        public List<SP_q221150_GETHOUSELISTResult> GetHouseList(int keyid)
        {
            List<SP_q221150_GETHOUSELISTResult> list = new List<SP_q221150_GETHOUSELISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221150_GETHOUSELIST(UID, keyid, null).ToList();
            }

            ViewBag.HRC일련번호 = keyid;
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
        
        public String CreateHouse(int keyid)
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
                    int houseC = TAPI.q221150_HouseList.Count(c => c.HRC일련번호 == keyid );
                    if (houseC > 999)
                        return "더이상 가구를 생성할 수 없습니다.";

                    TAPI.SP_q221150_INSERTHOUSE(UID, keyid);
                }
            }
            catch
            {
                result = "오류가 발생 했습니다.";
            }

            return result;
        }
        public List<SP_q221150_GETPERSONLISTResult> GetPersonList(int keyid, int hid)
        {
            List<SP_q221150_GETPERSONLISTResult> list = new List<SP_q221150_GETPERSONLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221150_GETPERSONLIST(UID, hid).ToList();

                q221150_HouseList houseList = TAPI.q221150_HouseList.First(f => f.HID == hid);

                ViewBag.HRC일련번호 = keyid;
                ViewBag.HID = hid.ToString();
                ViewBag.거처번호 = houseList.거처번호;
                ViewBag.가구번호 = houseList.가구번호;
                ViewBag.주소 = houseList.주소;
                ViewBag.세부주소 = houseList.세부주소;
                ViewBag.가구원수 = houseList.가구원수 == null ? 0 : houseList.가구원수;
                ViewBag.대상가구원수 = houseList._19_69가구원수==null ? 0: houseList._19_69가구원수;
                ViewBag.추가가구여부 = "";
                if(string.IsNullOrEmpty(houseList.기준년도.ToString()) == true)
                {
                    ViewBag.추가가구여부 = "추가가구";
                }


                ViewBag.생성된가구원수 = TAPI.q221150_PersonList.Count(f => f.HID == hid && f.deleteYN == 0);
                
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

        public ActionResult AnswerInfo(int keyid, int hid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }
            
            SP_q221150_GETHOUSELISTResult houseResult = null;
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                houseResult = TAPI.SP_q221150_GETHOUSELIST(UID, keyid, hid).First();
            }

            return View("AnswerInfo", houseResult);
        }


        public string SaveInfo(int keyid, int hid, string sebuAddr, string pname, string tel, string phone)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {

                int checkTel = TAPI.q221150_HouseList.Where(w => w.HID != hid && w.응답자휴대전화 == phone).Count();
                if( checkTel > 0 && phone != "")
                {
                    return "중복된 휴대전화번호를 입력하셨습니다. \r\n휴대전화번호를 확인해주세요.";
                }

                q221150_HouseList HouseList = TAPI.q221150_HouseList.First(f => f.HID == hid);
                                
                HouseList.세부주소 = sebuAddr;
                HouseList.응답자이름 = pname;
                HouseList.응답자집전화 = tel;
                HouseList.응답자휴대전화 = phone;
                
                TAPI.SubmitChanges();
            }

            return "";
        }

        public string CreatePerson(int keyid, int hid, int fAllCount, int fCount, string addr, string sebuAddr)
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
                    int i = TAPI.q221150_PersonList.Count(c => c.HRC일련번호 == keyid && c.HID == hid && c.deleteYN==0) + 1;
                    for (; i <= fCount; i++)
                    {
                        TAPI.SP_q221150_INSERTPERSON(UID, hid);
                    }

                    q221150_HouseList houseList = TAPI.q221150_HouseList.First(f => f.HID == hid);
                    houseList.가구원수 = fAllCount;
                    houseList._19_69가구원수 = fCount;
                    houseList.주소 = addr;
                    houseList.세부주소 = sebuAddr;
                    TAPI.SubmitChanges();
                }
            }
            catch
            {
                result = "오류가 발생 했습니다.";
            }

            return result;
        }

        public string DeletePerson(long pid)
        {
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {

                string query = string.Format(@"update q221150_PersonList set deleteyn=1  where PID={0} ", pid);


                if (TAPI.ExecuteCommand(query, "") < 1)
                {
                    return "처리 오류 다시 시도 해 주세요.";
                }

                TAPI.SubmitChanges();
            }

            return "삭제되었습니다.";
        }



        public string UpdatePerson(int hid, string pid, string ftype, string gender, string byear, string bmonth, string useI, string targetPID)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string[] pidArr = pid.Split('|');
            string[] ftypeArr = ftype.Split('|');
            string[] genderArr = gender.Split('|');
            string[] byearArr = byear.Split('|');
            string[] bmonthArr = bmonth.Split('|');
            string[] useIArr = useI.Split('|');

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                for (int i = 1; i < pidArr.Count(); i++)
                {   
                   
                    string query = string.Format(@"UPDATE [TAPI].[dbo].[q221150_personList] SET  가구주와의관계='{0}', 성별='{1}', 출생년도='{2}', 출생월='{3}', 인터넷이용여부='{4}' , 선정된응답자=0 WHERE PID={5}"
                    , ftypeArr[i], genderArr[i], byearArr[i], bmonthArr[i], useIArr[i], pidArr[i]);
                    if (pidArr[i] == targetPID)
                    {
                        query = string.Format(@"UPDATE [TAPI].[dbo].[q221150_personList] SET  가구주와의관계='{0}', 성별='{1}', 출생년도='{2}', 출생월='{3}', 인터넷이용여부='{4}'  , 선정된응답자=1 WHERE PID={5}"
                    , ftypeArr[i], genderArr[i], byearArr[i], bmonthArr[i], useIArr[i], pidArr[i]);
                    }
                    if (TAPI.ExecuteCommand(query, "") < 1)
                    {
                        return "정보 저장 오류 다시 시도 해 주세요.";
                    }
                    TAPI.SubmitChanges();
                }
            }


            return "저장되었습니다.";
        }
        public string SavePersonInfo(int keyid, int hid, long pid, string fcount, string targetCount, string gaguju, string gender, int byear, int bmonth, string useNet)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {

                q221150_PersonList personList = TAPI.q221150_PersonList.FirstOrDefault(f => f.PID == pid);
                
                personList.가구주와의관계 = gaguju;
                personList.성별 = gender;
                personList.출생년도 = bmonth;
                personList.출생월 = bmonth;

                TAPI.SubmitChanges();
            }

            return result;
        }

        public ActionResult Survey(int keyid, int hid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                List<AnswerList> answerList = new List<AnswerList>();
                
                bool eqnum = false;
                int qID = 13051;

                int answerStateCode = 0;
                using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q221150_1 WHERE PID = '{0}'", hid)).FirstOrDefault();
                    answerStateCode = t_SamplingList != null ? (int)(t_SamplingList.answerStateCode != null ? t_SamplingList.answerStateCode : 0) : 0;
                }

                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    q221150_HouseList HouseList = TAPI.q221150_HouseList.First(f => f.HID == hid);

                    if ((string.IsNullOrEmpty(HouseList.설문진행상태) == false && HouseList.설문진행상태 != "진행중"  ) ||  answerStateCode == 4)
                    {
                        eqnum = true;
                    }

                    TAPI.SP_q221150_SETLIST(hid, "진행중");
                }

                
                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q221150_1&pid={hid}&keyid={keyid}&t=TAPI&atype=1");
                }
                else
                {
                    ViewBag.HID = hid.ToString();                    

                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + qID.ToString()).Replace("/q221150_1", ""), hid);

                    

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
                                              Url = TripleDESCryptoService.Encrypt($"qn=q221150_1&pid={hid}&keyid={keyid}&atype=1&t=TAPI&eqnum={element.Name.ToString()}"),
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
        
        public ActionResult Contact(int keyid, int hid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            List<q221150_Contact> ContactList = new List<q221150_Contact>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                ContactList = (from contact in TAPI.q221150_Contact
                                        where contact.HID == hid
                                        select contact).ToList();

                ViewBag.Ranking = (ContactList.Count + 1).ToString();


                ViewBag.contactYear = DateTime.Now.ToString("yyyy");
                ViewBag.contactMonth = DateTime.Now.ToString("MM");
                ViewBag.contactDay = DateTime.Now.ToString("dd");
                

                q221150_HouseList houseList = TAPI.q221150_HouseList.First(f => f.HID == hid);
                ViewBag.HRC일련번호 = keyid;
                ViewBag.HID = hid;
                ViewBag.거처번호 = houseList.거처번호;
                ViewBag.가구번호 = houseList.가구번호;
                ViewBag.주소 = houseList.주소;
                ViewBag.세부주소 = houseList.세부주소;
            }

            return View("Contact", ContactList);
        }

        public string ContactOK(int hid, int ranking, string contact, string contactEtc, string contactMonth, string contactDay, string contactTime)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }


            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q221150_Contact Contact = new q221150_Contact();
                Contact.HID = hid;
                Contact.면접원아이디 = UID;
                Contact.방문횟수 = ranking;
                Contact.방문일자 = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;
                Contact.방문시간 = contactTime;
                Contact.비성공사유 = contact;
                Contact.비성공사유기타 = contactEtc;
                Contact.inputDate = DateTime.Now;

                TAPI.q221150_Contact.InsertOnSubmit(Contact);

                q221150_HouseList List = TAPI.q221150_HouseList.First(f => f.HID == hid);
                List.접촉일지상태 = contact;
                

                TAPI.SubmitChanges();
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
                q221150_AreaList AreaList = TAPI.q221150_AreaList.First(f => f.HRC일련번호 == keyid);

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

                q221150_Replace Replace = new q221150_Replace();

                Replace.HRC일련번호 = keyid;
                Replace.면접원아이디 = UID;
                Replace.대체사유 = status;
                Replace.대체세부사유 = etc;
                Replace.대체요청일자 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                TAPI.q221150_Replace.InsertOnSubmit(Replace);


                q221150_AreaList AreaList = TAPI.q221150_AreaList.First(f => f.HRC일련번호 == keyid);
                AreaList.진행상태 = "대체요청";
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