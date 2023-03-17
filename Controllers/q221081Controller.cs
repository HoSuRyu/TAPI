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
using System.Net.Http;

namespace TAPI_Interviewer.Controllers
{
    public class q221081Controller : Controller
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

        public List<SP_q221081_GETLISTResult> GetList(int? pid = null)
        {
            List<SP_q221081_GETLISTResult> list = new List<SP_q221081_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221081_GETLIST(UID, pid).ToList();
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 12950).FirstOrDefault();

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
            

            List<SP_q221081_GETLISTResult> list = new List<SP_q221081_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221081_GETLIST(UID, null).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "설문완료").Count().ToString();
            }

            return PartialView("ProgressPartial", progressCount);
        }

      

        public ActionResult AnswerInfo(int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            List<SP_q221081_GETHOUSELISTResult> HList = null;
            q221081_List List = null;
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                HList = TAPI.SP_q221081_GETHOUSELIST(UID, pid).ToList();
                List = TAPI.q221081_List.First(f => f.PID == pid);

                ViewBag.PID = pid;
                ViewBag.집전화 = List.집전화;
                ViewBag.휴대전화 = List.휴대전화;
                ViewBag.성명 = List.성명;
                ViewBag.HID = List.HID;
                ViewBag.주소 = List.시도 + " " + List.시군구 + " " + List.세부주소;
                ViewBag.가구대표여부 = List.가구대표여부;
                ViewBag.주소확인 = List.주소확인;
            }

            return View("AnswerInfo", HList);
        }


        
        public string SaveInfo(int pid, string familyPIO, string familyInfo, string tel, string phone, string IsNewAddress, string siNm, string sggNm, string emdNm, string roadAddrPart1, string sebuAddr)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                if(phone != "" && phone != "없음")
                {
                    int checkPhone = TAPI.q221081_List.Count(w => w.PID != pid && w.휴대전화 == phone && w.testYN == 0);
                    if(checkPhone > 0)
                    {
                        return "중복된 휴대전화 번호를 입력하셨습니다. \r\n휴대전화 번호를 확인해주세요.";
                    }
                }
              

                string[] fType = familyInfo.Split('|');
                int idx = 1;
                foreach (string hpid in familyPIO.Split('|'))
                {   if (hpid == "")
                        continue;

                    string query = string.Format(@"UPDATE [TAPI].[dbo].[q221081_List] SET 가구대표여부='{1}' WHERE PID={0}", hpid, fType[idx++]);

                    if (TAPI.ExecuteCommand(query, "") < 1)
                    {
                        result = "정보 저장 오류 다시 시도 해 주세요.";
                    }
                }

                q221081_List List = TAPI.q221081_List.First(f => f.PID == pid);
                List.집전화 = tel;
                List.휴대전화 = phone;
                if (IsNewAddress == "주소 틀림")
                {
                    List.시도 = siNm;
                    List.시군구 = sggNm;
                    string addr = roadAddrPart1.Replace(siNm, "").Replace(sggNm, "") + " " + sebuAddr;
                    List.세부주소 = addr.TrimStart().TrimEnd();
                }
                List.주소확인 = "주소 맞음";
                TAPI.SubmitChanges();

                return result;
            }
        }
        public ActionResult Survey(int pid, int isDir = 0)
        //public ActionResult Survey(int pid)
        {
            if (UID <= 0 && isDir == 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                List<AnswerList> answerList = new List<AnswerList>();

                ViewBag.PID = pid.ToString();
                ViewBag.isDir = isDir;
                q221081_List List = null;
                bool eqnum = false;
                int ftype = 1;
                int answerStateCode = 0;
                using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT  * FROM OPENQUERY([QMASTER] ,'SELECT TOP 1 id, pid, answerStateCode  FROM [SURVEY].[dbo].[T_SamplingList_q221081] WHERE PID = ''{0}''')", pid)).FirstOrDefault();
                    answerStateCode = t_SamplingList != null ? (int)(t_SamplingList.answerStateCode != null ? t_SamplingList.answerStateCode : 0) : 0;
                }

                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {


                    List = TAPI.q221081_List.First(f => f.PID == pid);
                    if (List.가구대표여부 == "가구원")
                    {
                        ftype = 2;
                    }
                    if ((List.진행상태 != null && List.진행상태 == "설문완료") || answerStateCode == 4)
                    {
                        eqnum = true;
                    }

                    TAPI.SP_q221081_SETLIST(pid, "진행중");
                }

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt(string.Format("qn=q221081&t=tapi&pid={0}&uid={1}&nv=true&atype=1&ftype={2}", pid, UID, ftype));
                }
                else
                {



                    using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                    {

                        String fileName = $"{pid}.hrcdata";
                        string url = $"https://qm.hrcglobal.com/Data/100004/{fileName}";
                        string answerDataDecrypt = "";


                        using (HttpClient httpClient = new HttpClient())
                        {
                            httpClient.Timeout = TimeSpan.FromHours(1);



                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            try
                            {
                                var response = httpClient.GetAsync(url).Result;

                                if (response.IsSuccessStatusCode == true)
                                {
                                    String hrcdataString = response.Content.ReadAsStringAsync().Result;

                                    answerDataDecrypt = AES256.Crypto.decryptAES256(hrcdataString);

                                    XElement AnswerData = XElement.Parse(answerDataDecrypt);

                                    T_Script t_Script = SURVEYTOOL.T_Scripts.Where(w => w.questionnaireID == 12950).First();

                                    if (t_Script != null)
                                    {
                                        XElement variableList = t_Script.variableList;

                                        answerList = (from element in variableList.Elements()
                                                      select new AnswerList
                                                      {
                                                          Variable = element.Name.ToString(),
                                                          Title = element.Value,
                                                          Answer = GetAnswerData(AnswerData, element.Name.ToString()),
                                                          Url = TripleDESCryptoService.Encrypt(string.Format("qn=q221081&pid={0}&uid={1}&t=tapi&eqnum={2}&nv=true&atype=1&ftype={3}", pid, UID, element.Name.ToString(), ftype)),
                                                      }).Where(w => string.IsNullOrEmpty(w.Answer) == false).ToList();
                                    }
                                }
                                else
                                {
                                    answerDataDecrypt = @"없는 PID입니다.\r\nPID를 다시 확인해 주세요.";
                                }



                                response.Dispose();
                            }
                            catch (Exception ee)
                            {
                                // xmlString = $@"{ee.Message}\r\n{ee.StackTrace}";
                            }
                        }
                    }
                }

                return View("NewEditSurvey", answerList);
            }
            catch (Exception ee)
            {
                return View("Error", (object)(ee.Message));
            }
        }

        public ActionResult SurveyBackup(int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                List<AnswerList> answerList = new List<AnswerList>();

                ViewBag.PID = pid.ToString();

                q221081_List List = null;
                bool eqnum = false;
                int ftype = 1;
                int answerStateCode = 0;
                using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q221081 WHERE PID = '{0}'", pid)).FirstOrDefault();
                    answerStateCode = t_SamplingList != null ? (int)(t_SamplingList.answerStateCode != null ? t_SamplingList.answerStateCode : 0) : 0;
                }

                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    

                    List = TAPI.q221081_List.First(f => f.PID == pid);
                    if( List.가구대표여부 =="가구원")
                    {
                        ftype = 2;
                    }
                    if ((List.진행상태 != null && List.진행상태 == "설문완료") || answerStateCode== 4)
                    {
                        eqnum = true;
                    }

                    TAPI.SP_q221081_SETLIST(pid, "진행중");
                }

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt(string.Format("qn=q221081&t=tapi&pid={0}&uid={1}&nv=true&atype=1&ftype={2}", pid, UID, ftype));
                }
                else
                {
                    
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\12950").Replace("/q221081", ""), pid);
                    using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                    {
                        StreamReader srAnswerData = new StreamReader(dataFile, Encoding.Default);
                        string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());
                        XElement AnswerData = XElement.Parse(answerDataDecrypt);

                        srAnswerData.Close();
                        srAnswerData.Dispose();

                        T_Script t_Script = SURVEYTOOL.T_Scripts.Where(w => w.questionnaireID == 12950).First();

                        if (t_Script != null)
                        {
                            XElement variableList = t_Script.variableList;

                            answerList = (from element in variableList.Elements()
                                          select new AnswerList
                                          {
                                              Variable = element.Name.ToString(),
                                              Title = element.Value,
                                              Answer = GetAnswerData(AnswerData, element.Name.ToString()),
                                              Url = TripleDESCryptoService.Encrypt(string.Format("qn=q221081&pid={0}&uid={1}&t=tapi&eqnum={2}&nv=true&atype=1&ftype={3}", pid, UID, element.Name.ToString(), ftype)),
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



            List<q221081_Contact> ContactList = null;

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q221081_List List = TAPI.q221081_List.First(f => f.PID == pid);


                ContactList = TAPI.q221081_Contact.Where(w => w.PID == pid).ToList();

                ViewBag.PID = List.PID;
                ViewBag.성명 = List.성명;

                ViewBag.Ranking = (ContactList.Count + 1).ToString();


                ViewBag.contactYear = DateTime.Now.ToString("yyyy");
                ViewBag.contactMonth = DateTime.Now.ToString("MM");
                ViewBag.contactDay = DateTime.Now.ToString("dd");
                
            }

            return View("Contact", ContactList);
        }


        public string ContactOK(int pid, int ranking, string contact, string contactEtc ,string contactMonth, string contactDay, string contactTime)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }


            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q221081_Contact Contact = new q221081_Contact();
                Contact.PID = pid;
                Contact.면접원아이디 = UID;
                Contact.방문횟수 = ranking;
                Contact.방문일자 = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;
                Contact.방문시간 = contactTime;
                Contact.비성공사유 = contact;
                Contact.비성공사유기타 = contactEtc;
                Contact.inputDate = DateTime.Now;

                TAPI.q221081_Contact.InsertOnSubmit(Contact);

                q221081_List List = TAPI.q221081_List.First(f => f.PID == pid);                
                List.접촉일지상태 = contact;
                
                if( contact == "3. 부재(외출, 전화 안받음)" || contact== "5. 유학, 해외 장기 출장(취업)" || contact == "6. 이민" || contact == "7. 질병 및 사고" || contact == "9. 사망")
                {
                    List.진행상태 = "조사불가";
                }
                else if (contact == "1. 단순거절" || contact== "8. 고소, 고발 등의 수준으로 거절")
                {
                    List.진행상태 = "거절";
                }
                else
                {
                    List.진행상태 = "접촉중";
                }
                
                TAPI.SubmitChanges();
            }

            return "저장";
        }
        public ActionResult Transfer(int pid)
        {

            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }
            
            List<SP_q221081_GETHOUSELISTResult> hList = new List<SP_q221081_GETHOUSELISTResult>();
            
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                hList = TAPI.SP_q221081_GETHOUSELIST(UID, pid).ToList();
                q221081_List list = TAPI.q221081_List.First(f => f.PID == pid);

                ViewBag.HID = list.HID;
                ViewBag.PID = pid;
                ViewBag.성명 = list.성명;
                ViewBag.집전화 = list.집전화;
                ViewBag.휴대전화 = list.휴대전화;
                ViewBag.주소 = list.시도+" "+list.시군구+" "+list.세부주소;


                ViewBag.rowspan = hList.Count();
                ViewBag.분가불가 = "";
                int hidLen = list.HID.ToString().Length;
                if(hidLen > 7)
                {
                    ViewBag.분가불가 = "분가불가";
                }

                ViewBag.조사성공가구원수 = TAPI.q221081_List.Count(c => c.HID == list.HID && c.진행상태 == "설문완료" && c.deleteYN == 0);
                

            }

            return View("Transfer", hList);
        }

        public ActionResult TransferOK(int pid, string tel, string phone, 
             string address1, string address2, string emdNm, string address, string dongho, string reason, string etc, string isTogether)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                string addressAll = address + " " + dongho;
                string sebuAddress = addressAll.Replace(address1, "").Replace(address2, "").Replace("  ", " ");
                

                if (isTogether == "1")
                {
                    TAPI.SP_q221081_DOTRANSFER(pid, UID, address1, address2, emdNm, sebuAddress, addressAll, tel, phone, reason, etc);
                }
                else
                {
                    List<SP_q221081_GETHOUSELISTResult> hList = hList = TAPI.SP_q221081_GETHOUSELIST(UID, pid).ToList();
                    foreach(SP_q221081_GETHOUSELISTResult list in hList)
                    {
                        if( list.PID == pid)
                        {
                            TAPI.SP_q221081_DOTRANSFER(pid, UID, address1, address2, emdNm, sebuAddress, addressAll, tel, phone, reason, etc);
                        }
                        else
                        {
                            TAPI.SP_q221081_DOTRANSFER(list.PID, UID, address1, address2, emdNm, sebuAddress, addressAll, tel, "", reason, etc);
                        }
                    }
                }
            }
            return null;
        }

        public ActionResult SplitOK(int pid, int hid, string tel, string phone, string address1, string address2, string emdNm, string address, string dongho)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }


            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                string addressAll = address + " " + dongho;
                string sebuAddress = addressAll.Replace(address1, "").Replace(address2, "").Replace("  ", " ");
                TAPI.SP_q221081_DOSPLIT(pid, hid,  UID, address1, address2, emdNm, sebuAddress, addressAll, tel, phone);
            }

            return null;
        }


    }
}
