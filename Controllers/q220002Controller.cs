using TAPI_Interviewer.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using TAPI_Interviewer.Models;
using TAPI_Interviewer.Models.Project;

//2022년 주거실태 시범 조사 
namespace TAPI_Interviewer.Controllers
{
    public class q220002Controller : Controller
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


        public List<SP_q220002_GETLISTResult> GetList()
        {

            List<SP_q220002_GETLISTResult> list = new List<SP_q220002_GETLISTResult>();
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220002_GETLIST(UID).ToList();

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
            List<SP_q220002_GETLISTResult> list = new List<SP_q220002_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220002_GETLIST(UID).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "완료").Count().ToString();
                progressCount.End2 = list.Where(w => w.완료수 != 0).Select(s => s.완료수).Sum().ToString();

            }

            return PartialView("ProgressPartial", progressCount);
        }

        public List<SP_q220002_GETHOUSELISTResult> GetHouseList(int keyid)
        {
            List<SP_q220002_GETHOUSELISTResult> list = new List<SP_q220002_GETHOUSELISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220002_GETHOUSELIST(keyid).ToList();

                List<string> statusList = list.GroupBy(g => g.진행상태).Where(w => string.IsNullOrEmpty(w.Key) == false).Select(s => s.Key).ToList();

                statusList.Insert(0, "");

                ViewBag.StatusList = statusList;
                ViewBag.조사구일련번호 = keyid;

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


        public ActionResult AnswerInfo(int hid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            q220002_HouseList q220002_HouseList = null; ;
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220002_HouseList = TAPI.q220002_HouseList.First(f => f.HID == hid);
            }
            ViewBag.Error = "";
            return View("AnswerInfo", q220002_HouseList);
        }


  

        public string SaveHouseInfo(int hid, string name, string ftype, string addr2, string phone,  string tel,string ftype4)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                
                q220002_HouseList HouseList = TAPI.q220002_HouseList.FirstOrDefault(f => f.HID == hid);

                HouseList.응답자이름 = name;
                HouseList.건물명 = addr2;
                HouseList.휴대전화 = phone;
                HouseList.집전화 = tel;
                HouseList.주택유형 = ftype;
                HouseList.주택유형기타 = ftype4;

                int hp_count = TAPI.q220002_HouseList.Count(f => f.휴대전화 == phone && f.HID != hid);
                if (hp_count > 0)
                {
                    return "이미 존재하는 휴대전화번호 입니다. 휴대전화번호를 확인해주세요.";
                }
                int tel_count = TAPI.q220002_HouseList.Count(f => f.집전화 == tel && f.HID != hid);
                if (tel_count > 0)
                {
                    return "이미 존재하는 집전화번호 입니다. 집전화번호를 확인해주세요.";
                }
                TAPI.SubmitChanges();
            }

            return "저장";
        }


        
        public ActionResult Survey(int hid,int keyid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                List<AnswerList> answerList = new List<AnswerList>();

                bool eqnum = false;
                int? qID = 13275;
                int answerStateCode = 0;
                using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q220002 WHERE PID = '{0}'", hid)).FirstOrDefault();
                    answerStateCode = t_SamplingList != null ? (int)(t_SamplingList.answerStateCode != null ? t_SamplingList.answerStateCode : 0) : 0;
                }
                q220002_HouseList houseList = null;
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {

                    houseList = TAPI.q220002_HouseList.First(f => f.HID == hid);
                    if (houseList.진행상태 == "완료" || answerStateCode == 4)
                    {
                        eqnum = true;
                    }

                    ViewBag.hid = hid;
                    TAPI.SP_q220002_SETHOUSELIST(hid, "진행중");
                }

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q220002&keyid={keyid}&pid={hid}&uid={UID}&t=TAPI&atype=1");
                }
                else
                {
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + qID.ToString()).Replace("/q220002", ""), hid);

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
                                              Url = TripleDESCryptoService.Encrypt($"qn=q220002&keyid={keyid}&pid={hid}&atype=1&t=TAPI&eqnum={element.Name.ToString()}"),
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

        public ActionResult Contact(int hid,int keyid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }



            List<q220002_Contact> ContactList = null;

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220002_HouseList List = TAPI.q220002_HouseList.First(f => f.HID == hid);

                ContactList = TAPI.q220002_Contact.Where(w => w.HID == hid).ToList();
                ViewBag.keyid = keyid;
                ViewBag.HID = hid;
                ViewBag.주소 = List.주소 + " " + List.세부주소;
                ViewBag.거처번호 = List.거처번호;
                ViewBag.가구번호 = List.가구번호;
                ViewBag.세부주소 = List.세부주소;
                ViewBag.건물명 = List.건물명;
                ViewBag.Ranking = (ContactList.Count + 1).ToString();
                ViewBag.contactYear = DateTime.Now.ToString("yyyy");
                ViewBag.contactMonth = DateTime.Now.ToString("MM");
                ViewBag.contactDay = DateTime.Now.ToString("dd");
                TAPI.SP_q220002_SETHOUSELIST(hid, "진행중");

            }

            return View("Contact", ContactList);
        }



        public string ContactOK(int hid, int ranking, string contact, string contactMonth, string contactDay, string contactTime)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }


            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                List<q220002_Contact> ContactList = new List<q220002_Contact>();
                string contactDate = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;

                //ContactList = (from contactT in TAPI.q221657_Contact
                //               where contactT.PID == pid & contactT.방문일자 == contactDate & contactT.방문시간 == contactTime
                //               select contactT).ToList();
                //if (ContactList.Count > 0)
                //{
                //    return "같은 시간대 방문기록이 이미 있습니다.";
                //}


                q220002_Contact Contact = new q220002_Contact();
                Contact.HID = hid;
                Contact.면접원아이디 = UID;
                Contact.방문횟수 = ranking;
                Contact.방문일자 = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;
                Contact.방문시간 = contactTime;
                Contact.비성공사유 = contact;
                Contact.inputDate = DateTime.Now;

                TAPI.q220002_Contact.InsertOnSubmit(Contact);

                q220002_HouseList List = TAPI.q220002_HouseList.First(f => f.HID == hid);
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
                q220002_AreaList list = TAPI.q220002_AreaList.First(f => f.조사구일련번호 == keyid);
                ViewBag.조사구일련번호 = keyid;
                ViewBag.주소 = list.시도 + " " + list.시군구 + " " + list.읍면동;
                ViewBag.조사구번호 = list.조사구번호;
                ViewBag.원대체 = list.원대체;
            }

            return View("Replace");

        }


        public ActionResult ReplaceOK(int keyid, string status, string status_etc)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220002_AreaList AreaList = TAPI.q220002_AreaList.First(f => f.조사구일련번호 == keyid);

                AreaList.진행상태 = "대체요청";

                q220002_Replace replace = new q220002_Replace();
                replace.조사구일련번호 = keyid;
                replace.면접원아이디 = UID;
                replace.상태 = "대체요청";
                replace.대체요청시간 = DateTime.Now;
                replace.대체사유 = status;
                replace.세부사유 = status_etc;

                TAPI.q220002_Replace.InsertOnSubmit(replace);

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