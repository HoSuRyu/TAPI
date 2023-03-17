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
    public class q220615Controller : Controller
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

        public List<SP_q220615_GETLISTResult> GetList()
        {
            
            List<SP_q220615_GETLISTResult> list = new List<SP_q220615_GETLISTResult>();
            SP_q220615_GETQUOTAResult quota = new SP_q220615_GETQUOTAResult();
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220615_GETLIST(UID, null).ToList();

                quota = TAPI.SP_q220615_GETQUOTA(UID).ToList()[0];
                ViewBag.쿼터1 = quota.col1;
                ViewBag.쿼터2 = quota.col2;
                ViewBag.쿼터3 = quota.col3;
                ViewBag.쿼터4 = quota.col4;
                ViewBag.쿼터5 = quota.col5;
                ViewBag.쿼터6 = quota.col6;
                ViewBag.쿼터7 = quota.col7;
                ViewBag.쿼터8 = quota.col8;
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 12915).FirstOrDefault();

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
            List<SP_q220615_GETLISTResult> list = new List<SP_q220615_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220615_GETLIST(UID, null).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "완료").Count().ToString();
                progressCount.End2 = list.Where(w => w.완료수 != 0).Select(s => s.완료수).Sum().ToString();
                  
            }

            return PartialView("ProgressPartial", progressCount);
        }

        public List<SP_q220615_GETHOUSELISTResult> GetHouseList(int keyid)
        {
            List<SP_q220615_GETHOUSELISTResult> list = new List<SP_q220615_GETHOUSELISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220615_GETHOUSELIST(UID, keyid, null).ToList();
                
                q220615_AreaList AreaList = TAPI.q220615_AreaList.First(f => f.집계구일련번호 == keyid);

                ViewBag.집계구일련번호 = AreaList.집계구일련번호;
                
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

        public string CreateHouse(int keyid, string address1, string address2, string sAgree)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                TAPI.SP_q220615_INSERTHOUSE(UID, keyid, address1, address2, sAgree);
            }

            return result;
        }

        public string ChangeSagree(int hid, string sAgree)
        {
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220615_HouseList HouseList = TAPI.q220615_HouseList.First(f => f.HID == hid);
                HouseList.조사가능여부 = sAgree;

                TAPI.SubmitChanges();

            }

            return "";
        }

        public List<SP_q220615_GETPERSONLISTResult> GetPersonList(int keyid, int hid)
        {
            List<SP_q220615_GETPERSONLISTResult> list = new List<SP_q220615_GETPERSONLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220615_GETPERSONLIST(UID, hid).ToList();
                
                q220615_HouseList HouseList = TAPI.q220615_HouseList.First(f => f.HID == hid);
                ViewBag.HID = hid.ToString();
                ViewBag.집계구일련번호 = HouseList.집계구일련번호;
                ViewBag.시군구 = HouseList.시군구;
                ViewBag.세부주소 = HouseList.세부주소;

                ViewBag.주택유형 = HouseList.주택유형;
                ViewBag.주택유형기타 = HouseList.주택유형기타;
                ViewBag.가구대표성명 = HouseList.가구대표성명;
                ViewBag.가구대표연락처 = HouseList.가구대표연락처;

                ViewBag.총가구원수남 = HouseList.총가구원수남;
                ViewBag.총가구원수여 = HouseList.총가구원수여;
                ViewBag.청년가구원수남 = HouseList.청년가구원수남;
                ViewBag.청년가구원수여 = HouseList.청년가구원수여;
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

        public string SaveInfo(int keyid, int hid,  string hType, string hTypeEtc, string hName, string hTel, int personCount1, int personCount2, int personCount5, int personCount6)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {

                //q210667_HouseList Q210667_HouseList = TAPI.q210667_HouseList.FirstOrDefault(f => f.조사구일련번호 == keyid && f.HID != hid && f.거쳐번호 == param1 && f.가구번호 == param2);

                //if (Q210667_HouseList != null)
                //{
                //    return "중복된 거쳐번호/가구번호 입니다. 다시 확인 해 주세요.";
                //}

                // 연락처 중복 불가 로직 삭제 요청으로 삭제
                //int phoneC = TAPI.q220615_HouseList.Count(f => f.가구대표연락처 == hTel && f.HID != hid);
                //return "이미 입력 된 가구대표연락처입니다.";

                q220615_HouseList HouseList = TAPI.q220615_HouseList.First(f => f.HID == hid);

                HouseList.주택유형 = hType;
                HouseList.주택유형기타 = hTypeEtc;
                HouseList.가구대표성명 = hName;
                HouseList.가구대표연락처 = hTel;
                
                HouseList.총가구원수남 = personCount1;
                HouseList.총가구원수여 = personCount2;
                HouseList.청년가구원수남 = personCount5;
                HouseList.청년가구원수여 = personCount6;
                
                
                TAPI.SubmitChanges();
            }

            return "";
        }

        public void CreatePerson(int keyid, int hid)
        {
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                int i = TAPI.q220615_PersonList.Count(c => c.집계구일련번호 == keyid && c.HID == hid);
                int personCount = (int)TAPI.q220615_HouseList.Where(f => f.집계구일련번호 == keyid && f.HID == hid).Select( s => s.청년가구원수남 + s.청년가구원수여).First();
                

                for (; i < personCount; i++)
                {
                    TAPI.SP_q220615_INSERTPERSON(UID, hid);
                }
            }
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
                int qID = 12915;

                int answerStateCode = 0;
                using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q220615 WHERE PID = '{0}'", pid)).FirstOrDefault();
                    answerStateCode = t_SamplingList != null ? (int)(t_SamplingList.answerStateCode != null ? t_SamplingList.answerStateCode : 0) : 0;
                }
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    q220615_PersonList PersonList = TAPI.q220615_PersonList.First(f => f.PID == pid);
                    TAPI.SP_q220615_SETLIST(pid, "진행중", "", "", "", "", "");

                    if ((string.IsNullOrEmpty(PersonList.설문진행상태) == false && PersonList.설문진행상태 != "진행중") || answerStateCode == 4)
                    {
                        eqnum = true;
                    }
                }
                

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q220615&pid={pid}&keyid={keyid}&hid={hid}&t=TAPI&uid={UID}");
                }
                else
                {
                    ViewBag.집계구일련번호 = keyid;
                    ViewBag.HID = hid.ToString();

                    
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + qID.ToString()).Replace("/q220615", ""), pid);
                    
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
                                              Url = TripleDESCryptoService.Encrypt($"qn=q220615&pid={pid}&keyid={keyid}&hid={hid}&t=TAPI&eqnum={element.Name.ToString()}"),
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
                q220615_Contact Contact = TAPI.q220615_Contact.FirstOrDefault(f => f.집계구일련번호 == keyid && f.면접원아이디 == UID);

                if (Contact == null)
                {
                    Contact = new q220615_Contact();

                    Contact.집계구일련번호 = keyid;
                    Contact.면접원아이디 = UID;
                    Contact.성공비성공 = 0;
                    Contact.성공 = 0;
                    Contact.비성공 = 0;
                    Contact.비성공A = 0;
                    Contact.비성공B = 0;
                    Contact.비성공C = 0;
                    Contact.비성공D = 0;
                    Contact.총방문횟수 = 0;
                    Contact.완료여부 = 0;

                    TAPI.q220615_Contact.InsertOnSubmit(Contact);
                }

                Contact.성공 = TAPI.SP_q220615_GETHOUSELIST(UID, keyid, null).Count(c => c.총완료수 > 0);

                TAPI.SubmitChanges();

                q220615_AreaList AreaList = TAPI.q220615_AreaList.First(f => f.집계구일련번호 == keyid);

                ViewBag.집계구일련번호 = AreaList.집계구일련번호;
                ViewBag.집계구주소 = $"{AreaList.시도} {AreaList.시군구} {AreaList.읍면동}";
                //ViewBag.전체가구수 = AreaList.가구수;

                return View("Contact", Contact);
            }
        }

        public string SaveContact(int keyid, int con0, int con1, int con2, int con4, int con5, int con6, int con7, int con9)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220615_Contact Contact = TAPI.q220615_Contact.FirstOrDefault(f => f.집계구일련번호 == keyid && f.면접원아이디 == UID);

                Contact.성공비성공 = con0;
                Contact.성공 = con1;
                Contact.비성공 = con2;
                Contact.비성공A = con4;
                Contact.비성공B = con5;
                Contact.비성공C = con6;
                Contact.비성공D = con7;
                Contact.총방문횟수 = con9;

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
                q220615_AreaList AreaList = TAPI.q220615_AreaList.First(f => f.집계구일련번호 == keyid);
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
                q220615_Replace Replace = new q220615_Replace();

                Replace.면접원아이디 = UID.ToString();
                Replace.집계구일련번호 = keyid;

                Replace.대체사유 = status;
                Replace.대체세부사유 = etc;
                Replace.대체요청일자 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                TAPI.q220615_Replace.InsertOnSubmit(Replace);

                q220615_AreaList AreaList = TAPI.q220615_AreaList.First(f => f.집계구일련번호 == keyid);
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