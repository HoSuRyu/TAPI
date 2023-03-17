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
    public class q220846Controller : Controller
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

        public List<SP_q220846_GETLISTResult> GetList()
        {
            
            List<SP_q220846_GETLISTResult> list = new List<SP_q220846_GETLISTResult>();
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220846_GETLIST(UID, null).ToList();
                
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 13015).FirstOrDefault();

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
            List<SP_q220846_GETLISTResult> list = new List<SP_q220846_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220846_GETLIST(UID, null).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "완료").Count().ToString();
                progressCount.End2 = list.Where(w => w.총완료수 != 0).Select(s => s.총완료수).Sum().ToString();
                
            }

            return PartialView("ProgressPartial", progressCount);
        }

        public List<SP_q220846_GETHOUSELISTResult> GetHouseList(int keyid)
        {
            List<SP_q220846_GETHOUSELISTResult> list = new List<SP_q220846_GETHOUSELISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220846_GETHOUSELIST(UID, keyid, null).ToList();
                
                q220846_AreaList AreaList = TAPI.q220846_AreaList.First(f => f.HRC일련번호 == keyid);
                ViewBag.HRC일련번호 = AreaList.HRC일련번호;
                
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

        public string CreateHouse(int keyid)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                TAPI.SP_q220846_INSERTHOUSE(UID, keyid);
            }

            return result;
        }

        public ActionResult AnswerInfo(int keyid, int hid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            SP_q220846_GETHOUSELISTResult GETHOUSELISTResult = null;

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                GETHOUSELISTResult = TAPI.SP_q220846_GETHOUSELIST(UID, keyid, hid).First();
            }

            return View("AnswerInfo", GETHOUSELISTResult);
        }


        public string SaveInfo(int keyid, int hid, string sebuaddr, string building, string name, string gender, int personC, string hType, string hTypeEtc, string tel, string phone )
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {

                if (phone != "" && phone != "없음")
                {
                    result = CheckPhone(hid.ToString(), phone);
                    if (result != "")
                    {
                        return result;
                    }
                }
                if (tel != "" && tel != "없음")
                {
                    result = CheckTel(hid.ToString(), tel);
                    if (result != "")
                    {
                        return result;
                    }
                }

                q220846_HouseList HouseList = TAPI.q220846_HouseList.First(f => f.HID == hid);


                HouseList.세부주소 = sebuaddr;
                HouseList.건물동명 = building;
                HouseList.응답자이름 = name;
                HouseList.응답자성별 = gender;
                HouseList.만15세이상가구원수 = personC;
                HouseList.주택유형 = hType;
                HouseList.주택유형기타 = hTypeEtc;
                HouseList.집전화 = tel;
                HouseList.휴대전화 = phone;
                

                TAPI.SubmitChanges();
            }

            return "";
        }


        public string CheckPhone(string hid, string hp)
        {

            // 핫라인 번호 중복 검사 하지 않음.
            if (hp == "010-000-0000" || hp == "010-0000-0000")
                return "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                int phoneC = TAPI.q220846_HouseList.Count(f => f.휴대전화 == hp && f.HID != int.Parse(hid));
                if (phoneC > 0)
                {
                    q220846_HouseList hList = TAPI.q220846_HouseList.First(f => f.휴대전화 == hp && f.HID != int.Parse(hid));
                    return "가구일련번호 " + hList.HID + "에서 이미 입력된 휴대전화번호 입니다.\r\n휴대전화번호가 맞는지 다시 확인해 주세요.\r\n맞다면 수퍼바이저에게 보고해 주세요.";
                }
            }
            return "";
        }

        public string CheckTel(string hid, string tel)
        {
            // 핫라인 번호 중복 검사 하지 않음.
            if (tel == "02-000-0000" || tel == "02-0000-0000")
                return "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                int phoneC = TAPI.q220846_HouseList.Count(f => f.집전화 == tel && f.HID != int.Parse(hid));
                if (phoneC > 0)
                {
                    q220846_HouseList hList = TAPI.q220846_HouseList.First(f => f.집전화 == tel && f.HID != int.Parse(hid));
                    return "가구일련번호 " + hList.HID + "에서 이미 입력된 전화번호 입니다.\r\n전화번호가 맞는지 다시 확인해 주세요.\r\n맞다면 수퍼바이저에게 보고해 주세요.";
                }
            }
            return "";
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
                int qID = 13015;

                int answerStateCode = 0;
                using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q220846 WHERE PID = '{0}'", hid)).FirstOrDefault();
                    answerStateCode = t_SamplingList != null ? (int)(t_SamplingList.answerStateCode != null ? t_SamplingList.answerStateCode : 0) : 0;
                }
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    q220846_HouseList HouseList = TAPI.q220846_HouseList.First(f => f.HID == hid);
                    TAPI.SP_q220846_SETHOUSE(hid, "진행중");

                    if ((string.IsNullOrEmpty(HouseList.설문진행상태) == false && HouseList.설문진행상태 == "완료") || answerStateCode == 4 || answerStateCode == 1)
                    {
                        eqnum = true;
                    }
                }
                

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q220846&pid={hid}&keyid={keyid}&t=TAPI&uid={UID}");
                }
                else
                {
                    ViewBag.HRC일련번호 = keyid;
                    ViewBag.HID = hid.ToString();

                    
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + qID.ToString()).Replace("/q220846", ""), hid);
                    
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
                                              Url = TripleDESCryptoService.Encrypt($"qn=q220846&pid={hid}&keyid={keyid}&t=TAPI&eqnum={element.Name.ToString()}"),
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

        public ActionResult Contact(int hid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            List<q220846_Contact> ContactList = null;

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220846_HouseList List = TAPI.q220846_HouseList.First(f => f.HID == hid);

                ContactList = TAPI.q220846_Contact.Where(w => w.HID == hid).ToList();

                ViewBag.HID = hid;
                ViewBag.HRC일련번호 = List.HRC일련번호;
                ViewBag.거처번호 = List.거처번호;
                ViewBag.가구번호 = List.가구번호;
                ViewBag.세부주소 = List.세부주소;
                ViewBag.건물동명 = List.건물동명;

                ViewBag.Ranking = (ContactList.Count + 1).ToString();
                ViewBag.contactYear = DateTime.Now.ToString("yyyy");
                ViewBag.contactMonth = DateTime.Now.ToString("MM");
                ViewBag.contactDay = DateTime.Now.ToString("dd");

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
                List<q220846_Contact> ContactList = new List<q220846_Contact>();
                string contactDate = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;
                ContactList = (from contactT in TAPI.q220846_Contact
                               where contactT.HID == hid & contactT.방문일자 == contactDate & contactT.방문시간 == contactTime
                               select contactT).ToList();
                if (ContactList.Count > 0)
                {
                    return "같은 시간대 방문기록이 이미 있습니다.";
                }

                q220846_Contact Contact = new q220846_Contact();
                Contact.HID = hid;
                Contact.면접원아이디 = UID;
                Contact.방문횟수 = ranking;
                Contact.방문일자 = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;
                Contact.방문시간 = contactTime;
                Contact.비성공사유 = contact;
                Contact.세부사유 = contactEtc;
                Contact.inputDate = DateTime.Now;

                TAPI.q220846_Contact.InsertOnSubmit(Contact);

                q220846_HouseList List = TAPI.q220846_HouseList.First(f => f.HID == hid);
                List.최종접촉일지상태 = contact;
                List.접촉일지기입여부 = "기입";

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
                q220846_AreaList AreaList = TAPI.q220846_AreaList.First(f => f.HRC일련번호 == keyid);
                ViewBag.주소 = AreaList.시도 + " "+ AreaList.시군구 + " " + AreaList.읍면동;

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
                q220846_Replace Replace = new q220846_Replace();

                Replace.면접원아이디 = UID;
                Replace.HRC일련번호 = keyid;

                Replace.대체사유 = status;
                Replace.대체세부사유 = etc;
                Replace.대체요청일자 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                TAPI.q220846_Replace.InsertOnSubmit(Replace);

                q220846_AreaList AreaList = TAPI.q220846_AreaList.First(f => f.HRC일련번호 == keyid);
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