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
    public class q220134Controller : Controller
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

        public List<SP_q220134_GETHOUSELISTResult> GetHouseList()
        {
            
            List<SP_q220134_GETHOUSELISTResult> list = new List<SP_q220134_GETHOUSELISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220134_GETHOUSELIST(UID, null).ToList();
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 12496).FirstOrDefault();

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

            return View("HouseList", GetHouseList());
        }

        public ActionResult ListPartial()
        {
            return PartialView("HouseListPartial", GetHouseList());
        }

 

        public ActionResult ProgressPartial()
        {
            ProgressCount progressCount = new ProgressCount();

            progressCount.All = "0";
            progressCount.Ing = "0";
            progressCount.End = "0";

            List<SP_q220134_GETHOUSELISTResult> list = new List<SP_q220134_GETHOUSELISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220134_GETHOUSELIST(UID, null).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "완료").Count().ToString();
            }

            return PartialView("ProgressPartial", progressCount);
        }
        
       


        public List<SP_q220134_GETPERSONLISTResult> GetPersonList(int hid)
        {
            List<SP_q220134_GETPERSONLISTResult> list = new List<SP_q220134_GETPERSONLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220134_GETPERSONLIST(UID, hid).ToList();

                SP_q220134_GETHOUSELISTResult GETHOUSELISTResult = TAPI.SP_q220134_GETHOUSELIST(UID, hid).First();

                ViewBag.HID = hid.ToString();

                ViewBag.리 = GETHOUSELISTResult.리;
                ViewBag.시도 = GETHOUSELISTResult.시도;
                ViewBag.시군구= GETHOUSELISTResult.시군구;
                ViewBag.읍면동= GETHOUSELISTResult.읍면동;
                ViewBag.상세주소 = GETHOUSELISTResult.상세주소;
                ViewBag.주소메모 = GETHOUSELISTResult.주소메모;

                ViewBag.조사대상가구원수 = GETHOUSELISTResult.조사대상가구원수;

                ViewBag.가구원대표응답여부 = TAPI.q220134_PersonList.Count(c => c.HID == hid && c.세대주여부=="가구원 대표" && c.진행상태=="완료");
            }

            return list;
        }

        public ActionResult PersonList(int hid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {               
                TAPI.SubmitChanges();
            }
            return View("PersonList", GetPersonList(hid));
        }

        public ActionResult PersonListPartial(int hid)
        {
            return PartialView("PersonListPartial", GetPersonList(hid));
        }

        public ActionResult CreateHouse()
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

           
            return View("CreateHouse");
        }
        public string CreateHouseOK(string address1, string address2)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                TAPI.SP_q220134_INSERTHOUSE(UID, address1, address2);
            }
            
            return "생성";
        }


        public string UpdatePerson( int hid, string pid, string name,  string tel, string cap, string isAnswer , string noanswerText)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string[] pidArr = pid.Split('|');
            string[] nameArr = name.Split('|');
            string[] capArr = cap.Split('|');
            string[] isAnswerArr = isAnswer.Split('|');
            string[] noanswerTextArr = noanswerText.Split('|');
            string[] telArr = tel.Split('|');

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                for (int i = 1; i < pidArr.Count(); i++)
                {
                    int deleteyn = 0;
                  
                    if( isAnswerArr[i] == "잘못 입력함")
                    {
                        deleteyn = 1;
                    }

                    string query = string.Format(@"UPDATE [TAPI].[dbo].[q220134_personList] SET  이름='{0}', 휴대전화='{1}', 세대주여부='{2}', 응답가능여부='{3}', 불가능사유='{4}', deleteyn={6}  WHERE PID={5}"
                    , nameArr[i], telArr[i], capArr[i], isAnswerArr[i], noanswerTextArr[i],pidArr[i], deleteyn);

                    if (TAPI.ExecuteCommand(query, "") < 1)
                    {
                        return "정보 저장 오류 다시 시도 해 주세요.";
                    }

                    string QMListData = $@"<RPS_SamplingLIST_INFO>
                                        <SQ5 VALUE=""{capArr[i]}"" />                                        
                                        <이름 VALUE=""{nameArr[i]}"" />
                                        <HID VALUE=""{hid}"" />                                        
                                       </RPS_SamplingLIST_INFO>";


                    string query2 = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q220134]') AND type in (N'U'))
BEGIN
    MERGE SURVEY.dbo.T_SamplingList_q220134 as SLIST
    USING
    (
        SELECT	'{0}' as PID
    ) as Answer on SLIST.PID = Answer.PID
    WHEN MATCHED THEN
	    UPDATE	SET	QMListData = N'{1}'
    WHEN NOT MATCHED THEN
	    INSERT (PID, QMListData)
	    VALUES (PID, N'{1}');
END", pidArr[i], QMListData);

                    if (TAPI.ExecuteCommand(query2, "") < 1)
                    {
                        return "정보 저장 오류 다시 시도 해 주세요.";
                    }


                    TAPI.SubmitChanges();

                   
                }

                TAPI.SP_q220134_SETHOUSE(int.Parse(pidArr[1]), "");
            }


            return "저장되었습니다.";
        }
        public string SaveInfo(int hid, int personCount, string addressMemo)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                
                q220134_HouseList HouseList = TAPI.q220134_HouseList.FirstOrDefault(f => f.HID == hid);

                HouseList.주소메모 = addressMemo;
                HouseList.조사대상가구원수 = personCount;
                

                TAPI.SubmitChanges();
            }

            return "";
        }
        public string CreatePerson( int hid, int personCount, string addressMemo)
        {
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {


                int familyC = TAPI.q220134_PersonList.Count(c =>  c.HID == hid && c.deleteyn == 0);
                if (familyC >= personCount)
                {
                    return SaveInfo( hid,  personCount, addressMemo);
                }

                if ( familyC == 0)
                {
                    TAPI.SP_q220134_INSERTPERSON(hid, 1);
                    for (int i = 2; i <= personCount; i++)
                    {

                        TAPI.SP_q220134_INSERTPERSON(hid, 0);
                    }
                }
                else
                {
                    for (int i = familyC + 1; i <= personCount; i++)
                    {

                        TAPI.SP_q220134_INSERTPERSON(hid, 0);
                    }
                }
            }

            return SaveInfo(hid, personCount, addressMemo);
        }
        public string DeletePerson(int pid)
        {
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {

                string query = string.Format(@"update q220134_PersonList set deleteyn=1  where PID={0} ", pid);


                if (TAPI.ExecuteCommand(query, "") < 1)
                {
                    return "처리 오류 다시 시도 해 주세요.";
                }

                TAPI.SubmitChanges();

                TAPI.SP_q220134_SETHOUSE(pid, "");
            }

            return "삭제되었습니다.";
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
                
                bool eqnum = false;
                int qID = 12496;
            
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    q220134_PersonList PersonList = TAPI.q220134_PersonList.First(f => f.PID == pid);
                 
                    if (string.IsNullOrEmpty(PersonList.진행상태) == false && PersonList.진행상태 == "완료")
                    {
                        eqnum = true;
                    }

                    //TAPI.SP_q211224_SETHOUSE(pid, "진행중");
                    TAPI.SubmitChanges();

                 
                }

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q220134&pid={pid}&t=TAPI");
                }
                else
                {
                   
                    
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + qID.ToString()).Replace("/q220134", ""), pid);
                    
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
                                              Url = TripleDESCryptoService.Encrypt($"qn=q220134&pid={pid}&t=TAPI&eqnum={element.Name.ToString()}"),
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

            q220134_HouseList HouseList;
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                HouseList = TAPI.q220134_HouseList.FirstOrDefault(f => f.HID == hid);
                
            }

            return View("Contact", HouseList);
        }

        public string ContactOK(int hid, string contact,string etc)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }


            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                
                 q220134_HouseList HouseList = TAPI.q220134_HouseList.FirstOrDefault(f => f.HID == hid);

                HouseList.접촉상태 = contact;
                HouseList.접촉상태기타 = etc;
                HouseList.접촉일지작성시간  = $"{DateTime.Now}";
                
                TAPI.SubmitChanges();
                
            }

            return "저장";
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