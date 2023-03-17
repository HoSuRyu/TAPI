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
    public class q220850TController : Controller
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

        public List<SP_q220850_GETLISTResult> GetList()
        {
            
            List<SP_q220850_GETLISTResult> list = new List<SP_q220850_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220850_GETLIST(UID).ToList();
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 12953).FirstOrDefault();

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

            List<SP_q220850_GETLISTResult> list = new List<SP_q220850_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220850_GETLIST(UID).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "완료").Count().ToString();
            }

            return PartialView("ProgressPartial", progressCount);
        }
        
       


        public List<SP_q220850_GETPERSONLISTResult> GetPersonList(int keyid)
        {
            List<SP_q220850_GETPERSONLISTResult> list = new List<SP_q220850_GETPERSONLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                
                list = TAPI.SP_q220850_GETPERSONLIST(UID, keyid).ToList();

                
                q220850_AreaList areaList = TAPI.q220850_AreaList.Where(w => w.집계구번호 == keyid.ToString()).First();

                ViewBag.집계구번호 = keyid.ToString();

                ViewBag.집계구내지점 = areaList.집계구내지점;
                ViewBag.조사지점상세 = areaList.조사지점상세;
                
            }

            return list;
        }

        public ActionResult PersonList(int keyid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                int covideAnswer = TAPI.q220850_Covid19.Count(f => f.면접원아이디 == UID && f.inputDate == DateTime.Today);
                if (covideAnswer == 0)
                {
                    ViewBag.keyid = keyid;
                    return View("Covid19");
                }


                TAPI.SubmitChanges();
            }
            return View("PersonList", GetPersonList(keyid));
        }

        public ActionResult PersonListPartial(int keyid)
        {
            return PartialView("PersonListPartial", GetPersonList(keyid));
        }

        public ActionResult Covid19OK(int q1, int q2, int q3, int q4)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }


            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220850_Covid19 Convid19 = new q220850_Covid19();
                Convid19.면접원아이디 = UID;
                Convid19.inputDate = DateTime.Today;
                Convid19.문1 = q1;
                Convid19.문2 = q2;
                Convid19.문3 = q3;
                Convid19.문4 = q4;

                TAPI.q220850_Covid19.InsertOnSubmit(Convid19);
                TAPI.SubmitChanges();
            }

            return null;
        }


        public string CreatePerson( int keyid, string name, string phone)
        {
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                int checkPhone = TAPI.q220850_PersonList.Where(w => w.연락처 == phone).Count();
                if(checkPhone > 0)
                {
                    return "중복된 연락처를 입력하셨습니다. \r\n연락처를 확인해주세요.";
                }

                TAPI.SP_q220850_INSERTPERSON(UID, keyid, name, phone);
                
            }

            return "응답자 생성 완료";
        }

  

        public ActionResult Survey(int pid)
        {
            if (UID <= 0 )
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                
                List<AnswerList> answerList = new List<AnswerList>();
                
                
                int answerStateCode = 0;
                using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q220850T WHERE PID = '{0}'", pid)).FirstOrDefault();

                    answerStateCode = t_SamplingList != null ? (int)(t_SamplingList.answerStateCode != null ? t_SamplingList.answerStateCode : 0) : 0;

                }

                int qID = 12953;
                string keyid;
                bool eqnum = false;
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    q220850_PersonList PersonList = TAPI.q220850_PersonList.First(f => f.PID == pid);
                    keyid = PersonList.집계구번호;
                    // 완료/스크린아웃/쿼터풀 모두 이어하기 되도록
                    if ((string.IsNullOrEmpty(PersonList.설문진행상태) == false && PersonList.설문진행상태 == "완료") || (answerStateCode==4 || answerStateCode==1 || answerStateCode==2))
                    {
                        eqnum = true;
                    }

                    TAPI.SP_q220850_SETLIST(pid, "진행중", "");
                    TAPI.SubmitChanges();
                }

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q220850T&pid={pid}&t=TAPI&atype=1&keyid={keyid}");
                }
                else if(answerStateCode == 2)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q220850T&pid={pid}&t=TAPI&qnum=q10&atype=1&keyid={keyid}");
                }
                else
                {
                   
                    
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + qID.ToString()).Replace("/q220850T", ""), pid);
                    
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
                                              Url = TripleDESCryptoService.Encrypt($"qn=q220850T&pid={pid}&t=TAPI&eqnum={element.Name.ToString()}"),
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
    }
}