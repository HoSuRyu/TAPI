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
    public class q221657_3Controller : Controller
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

        public List<SP_q221657_GETLISTResult> GetList()
        {
            
            List<SP_q221657_GETLISTResult> list = new List<SP_q221657_GETLISTResult>();
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221657_GETLIST(UID, null).ToList();
                
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
            List<SP_q221657_GETLISTResult> list = new List<SP_q221657_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221657_GETLIST(UID, null).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "완료").Count().ToString();
                progressCount.End2 = list.Where(w => w.총완료수 != 0).Select(s => s.총완료수).Sum().ToString();
                
            }

            return PartialView("ProgressPartial", progressCount);
        }

        public ActionResult ProgressPartial2(int keyid)
        {
            ProgressCount progressCount = new ProgressCount();
            
            progressCount.End = "0";
            List<SP_q221657_GETCOMPANYLISTResult> list = new List<SP_q221657_GETCOMPANYLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221657_GETCOMPANYLIST(keyid,null).ToList();
                progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "완료").Count().ToString();
                

            }

            return PartialView("ProgressPartial2", progressCount);
        }

        public List<SP_q221657_GETCOMPANYLISTResult> GetCompanyList(int keyid)
        {
            List<SP_q221657_GETCOMPANYLISTResult> list = new List<SP_q221657_GETCOMPANYLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221657_GETCOMPANYLIST(keyid, null).ToList();                
                ViewBag.상권ID = keyid;

                List<string> statusList = list.GroupBy(g => g.진행상태).Where(w => string.IsNullOrEmpty(w.Key) == false).Select(s => s.Key).ToList();
                statusList.Insert(0, "");
                ViewBag.StatusList = statusList;

            }


            List<string> bTypeList = new List<string>() { "소매업", "기타 개인 서비스업", "음식 및 주점업", "오락 및 스포츠업" };
            ViewBag.bType = bTypeList;
            return list;
        }

        public ActionResult CompanyList(int keyid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            return View("CompanyList", GetCompanyList(keyid));
        }

        public ActionResult CompanyListPartial(int keyid)
        {

            return PartialView("CompanyListPartial", GetCompanyList(keyid));
        }
        

        public ActionResult AnswerInfo(int keyid, int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            SP_q221657_GETCOMPANYLISTResult GETCOMPANYResult = null;


            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                GETCOMPANYResult = TAPI.SP_q221657_GETCOMPANYLIST( keyid, pid).First();
            }

            return View("AnswerInfo", GETCOMPANYResult);
        }


        public string SendMSG(int keyid, int pid, string phone)
        {
            
            using (SENDMASTERDataContext SENDMASTER = new SENDMASTERDataContext("Data Source=172.19.100.85,15000;Initial Catalog=SEND_MASTER;Persist Security Info=True;User ID=hrcdv;password=Hrc#3014!#"))
            {
                string mm = string.Format(@"안녕하십니까? 

한국형사·법무정책연구원은 ㈜한국리서치와 함께 「상가 임대차 실태조사」를 실시하고 있으며, 귀 사업체가 대상으로 선정되어 설문링크를 보내드립니다.
아래 설문링크를 통해 조사에 참여 부탁드립니다.


□	설문링크 : https://rpssurvey.hrcglobal.com/?qn=q221657_3&pid={0}&atype=2&keyid={1}
□	소요시간 : 약 10분 내외
□	답례품 : 문화상품권 1만원권 (조사 완료 확인 후 조사원 방문 지급 예정)


상가 임대차 실태조사 결과는 상가 영업권 보장을 위한 정책을 수립하기 위한 연구의 기초 자료로 활용됩니다. 본 조사에 응해주신 귀하의 응답 내용은 통계작성 목적으로만 사용되며, 통계법 제33조(비밀의 보호)에 따라 엄격히 보호됨을 알려드립니다.
바쁘시겠지만 잠시 시간을 내어 협조 부탁드립니다


□	조사 관련 문의처 : 02-3014-1025(월~금, 09:30~17:30)　
", pid, keyid);

                SENDMASTER.PROC_SEND_MESSAGE("interviwer", "0230141025", phone, "[한국리서치] 상가 임대차 실태조사 안내", mm, DateTime.Now, "", "2022-98-1657");

            }
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                TAPI.SP_q221657_SETLIST(pid, "문자발송", "");

                
            }
            return "성공";
        }

        public string SaveInfo(int keyid, int pid, string changeCompanyName,  string name, string age , string gender,  string tel, string companyType, string companyTypeEtc, string companyArea, string memberCount, string positionType, string positionTypeEtc, string atype, bool isSend, string address, string btype)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                
                if (tel != "" && tel != "없음")
                {
                    result = CheckTel(pid, tel);
                    if (result != "")
                    {
                        return result;
                    }
                }
                
                q221657_CompanyList CompanyList = TAPI.q221657_CompanyList.First(f => f.PID == pid);
                

                CompanyList.응답자이름 = name;
                CompanyList.응답자나이 = age;
                CompanyList.응답자성별 = gender;
                CompanyList.휴대전화 = tel;
                CompanyList.사업체형태 = companyType;
                CompanyList.사업체형태기타 = companyTypeEtc;
                CompanyList.사업장면적 = companyArea;
                CompanyList.종업원수 = memberCount;
                CompanyList.입지유형 = positionType;
                CompanyList.입지유형기타 = positionTypeEtc;
                CompanyList.조사방법 = atype;

                if(string.IsNullOrEmpty(CompanyList.생성여부) == false)
                {
                    CompanyList.사업체명 = changeCompanyName;
                    if(address != "" )
                    {
                        CompanyList.주소 = address;
                    }
                    CompanyList.업종 = btype;
                    int rows = TAPI.q221657_CompanyList.Count(f => f.상권ID == keyid && f.업종 == btype);
                    int c = 0;
                    if ( rows > 0)
                    {
                        c = int.Parse(TAPI.q221657_CompanyList.FirstOrDefault(f => f.상권ID == keyid && f.업종 == btype).업종목표.ToString());
                    }
                    CompanyList.업종목표 = c;
                }
                else
                {
                    CompanyList.변경된사업체명 = changeCompanyName;
                }                
                TAPI.SubmitChanges();
            }

            if( isSend == true)
            {
                SendMSG(keyid, pid, tel);
            }

            return "";
        }


        public string CheckPhone(int  pid, string hp)
        {

            // 핫라인 번호 중복 검사 하지 않음.
            if (hp == "010-000-0000" || hp == "010-0000-0000")
                return "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                int phoneC = TAPI.q221657_CompanyList.Count(f => f.휴대전화 == hp && f.PID != pid);
                if (phoneC > 0)
                {
                    q221657_CompanyList cList = TAPI.q221657_CompanyList.First(f => f.휴대전화 == hp && f.PID != pid);
                    return "사업체 일련번호 " + cList.PID + "에서 이미 입력된 휴대전화번호 입니다.\r\n휴대전화번호가 맞는지 다시 확인해 주세요.\r\n맞다면 수퍼바이저에게 보고해 주세요.";
                }
            }
            return "";
        }

        public string CheckTel(int pid, string tel)
        {
            // 핫라인 번호 중복 검사 하지 않음.
            if (tel == "02-000-0000" || tel == "02-0000-0000")
                return "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                int phoneC = TAPI.q221657_CompanyList.Count(f => f.휴대전화 == tel && f.PID != pid);
                if (phoneC > 0)
                {
                    q221657_CompanyList cList = TAPI.q221657_CompanyList.First(f => f.휴대전화 == tel && f.PID != pid);
                    return "사업체 일련번호 " + cList.PID + "에서 이미 입력된 전화번호 입니다.\r\n전화번호가 맞는지 다시 확인해 주세요.\r\n맞다면 수퍼바이저에게 보고해 주세요.";
                }
            }
            return "";
        }


        public ActionResult Survey(int keyid, int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                List<AnswerList> answerList = new List<AnswerList>();
                
                bool eqnum = false;
                int qID = 13226;

                int answerStateCode = 0;
                using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q221657_3 WHERE PID = '{0}'", pid)).FirstOrDefault();
                    answerStateCode = t_SamplingList != null ? (int)(t_SamplingList.answerStateCode != null ? t_SamplingList.answerStateCode : 0) : 0;
                }
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {

                    q221657_CompanyList CompanyList = TAPI.q221657_CompanyList.First(f => f.PID == pid);

                    string query = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q221657_3]') AND type in (N'U'))
BEGIN
    MERGE SURVEY.dbo.T_SamplingList_q221657_3 as SLIST
    USING
    (
        SELECT	'{0}' as PID
    ) as Answer on SLIST.PID = Answer.PID
    WHEN MATCHED THEN
	    UPDATE	SET	name = N'{1}'
    WHEN NOT MATCHED THEN
	    INSERT (PID, name)
	    VALUES (PID, N'{1}');
END", pid, CompanyList.응답자이름);

                   
                    TAPI.SP_q221657_SETLIST(pid, "진행중","");

                    if ((string.IsNullOrEmpty(CompanyList.진행상태) == false && CompanyList.진행상태 == "완료") || answerStateCode == 4 || answerStateCode == 1)
                    {
                        eqnum = true;
                    }
                }
                

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q221657_3&pid={pid}&keyid={keyid}&t=TAPI&uid={UID}");
                }
                else
                {
                    ViewBag.HRC일련번호 = keyid;
                    ViewBag.PID = pid.ToString();

                    
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + qID.ToString()).Replace("/q221657_3", ""), pid);
                    
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
                                              Url = TripleDESCryptoService.Encrypt($"qn=q221657_3&pid={pid}&keyid={keyid}&t=TAPI&eqnum={element.Name.ToString()}"),
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

        public ActionResult Contact(int keyid, int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            List<q221657_Contact> ContactList = null;

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q221657_CompanyList List = TAPI.q221657_CompanyList.First(f => f.PID == pid);

                ContactList = TAPI.q221657_Contact.Where(w => w.PID == pid).ToList();

                ViewBag.PID = pid;
                ViewBag.사업체명 = List.사업체명;
                ViewBag.업종 = List.업종;
                ViewBag.주소 = List.주소;

                ViewBag.Ranking = (ContactList.Count + 1).ToString();
                ViewBag.contactYear = DateTime.Now.ToString("yyyy");
                ViewBag.contactMonth = DateTime.Now.ToString("MM");
                ViewBag.contactDay = DateTime.Now.ToString("dd");

            }
            ViewBag.상권ID = keyid;
            return View("Contact", ContactList);
        }

        public string ContactOK(int pid, int ranking, string contact, string contactEtc, string contactMonth, string contactDay, string contactTime)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }



            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                List<q221657_Contact> ContactList = new List<q221657_Contact>();
                string contactDate = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;

                //ContactList = (from contactT in TAPI.q221657_Contact
                //               where contactT.PID == pid & contactT.방문일자 == contactDate & contactT.방문시간 == contactTime
                //               select contactT).ToList();
                //if (ContactList.Count > 0)
                //{
                //    return "같은 시간대 방문기록이 이미 있습니다.";
                //}

                TAPI.SP_q221657_SETLIST(pid, "진행중", "");

                q221657_Contact Contact = new q221657_Contact();
                Contact.PID = pid;
                Contact.면접원아이디 = UID;
                Contact.방문횟수 = ranking;
                Contact.방문일자 = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;
                Contact.방문시간 = contactTime;
                Contact.비성공사유 = contact;
                Contact.비성공사유기타 = contactEtc;
                Contact.inputDate = DateTime.Now;

                TAPI.q221657_Contact.InsertOnSubmit(Contact);

                q221657_CompanyList List = TAPI.q221657_CompanyList.First(f => f.PID == pid);
                List.최종접촉일지 = contact;
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

            ViewBag.상권ID = keyid;
            return View("Replace");
          
        }

        public ActionResult ReplaceOK(int keyid, string status )
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q221657_Replace Replace = new q221657_Replace();

                Replace.면접원아이디 = UID;
                Replace.상권ID = keyid;

                Replace.대체사유 = status;
                Replace.대체요청시간 = DateTime.Now;
                
                TAPI.q221657_Replace.InsertOnSubmit(Replace);

                q221657_AreaList AreaList = TAPI.q221657_AreaList.First(f => f.상권ID == keyid);
                AreaList.진행상태 = "대체요청";
                TAPI.SubmitChanges();

            }

            return null;
        }

        public string CreateCompany(int keyid)
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
                    TAPI.SP_q221657_INSERTCOMPANY(UID, keyid);

                }
            }
            catch
            {
                result = "오류가 발생 했습니다.";
            }

            return result;
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