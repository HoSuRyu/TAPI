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

// 체크인 확인용 주석처리

namespace TAPI_Interviewer.Controllers
{
    public class q220486Controller : Controller
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

        public List<SP_q220486_GETLISTResult> GetList(int? pid = null)
        {
            List<SP_q220486_GETLISTResult> list = new List<SP_q220486_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220486_GETLIST(UID, pid).ToList();
            }

            List<string> statusList = list.GroupBy(g => g.최종설문진행상태).Where(w => string.IsNullOrEmpty(w.Key) == false).Select(s => s.Key).ToList();

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
            progressCount.endEtc = "0";

            progressCount.endDir = "0";
            progressCount.endTel = "0";

            List<SP_q220486_GETLISTResult> list = new List<SP_q220486_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220486_GETLIST(UID, null).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.최종설문진행상태 != null && (w.최종설문진행상태 == "사전조사 진행중" || w.최종설문진행상태== "본조사 진행중")).Count().ToString();
                progressCount.End = list.Where(w => w.최종설문진행상태 != null && w.최종설문진행상태 == "완료").Count().ToString();
            
            }

            return PartialView("ProgressPartial", progressCount);
        }


        public ActionResult Replace(int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220486_List list = TAPI.q220486_List.First(f => f.PID == pid);
                return View("Replace", list);
            }
        }


        public ActionResult ReplaceOK(int pid, string status, string etc)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            q220486_Replace replace = new q220486_Replace();
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                
                q220486_List list = TAPI.q220486_List.First(f => f.PID == pid);
                list.최종설문진행상태 = "대체요청";


                replace.PID = pid;
                replace.면접원아이디 = UID;
                replace.상태 = "대체요청";
                replace.대체사유 = status;
                replace.대체사유기타 = etc;
                replace.대체요청시간 = DateTime.Now;

                TAPI.q220486_Replace.InsertOnSubmit(replace);

                TAPI.SubmitChanges();
            }

            return null;
        }

        public ActionResult Transfer(int pid)
        {

            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220486_List list = TAPI.q220486_List.First(f => f.PID == pid);

                return View("Transfer", list);
            }
            
        }

        public ActionResult TransferOK(int pid, 
             string address1, string address2, string emdNm, string address, string dongho, string edit_tel, string status, string etc, string relation)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }
            
           
            q220486_Transfer transfer = new q220486_Transfer();
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220486_List list = TAPI.q220486_List.First(f => f.PID == pid);
              
                list.최종설문진행상태 = "이관요청";


                transfer.PID = pid;
                transfer.면접원아이디 = UID;
                transfer.시도 = address1;
                transfer.시군구 = address2;
                transfer.읍면동 = emdNm;
                transfer.세부주소 = dongho;
                transfer.전체주소 = address + " " + dongho;
                transfer.변경연락처 = edit_tel;
                transfer.요청시간 = DateTime.Now;
                transfer.deleteyn = "0";
                transfer.상태 = "이관요청";

                transfer.이관사유 = status;
                transfer.이관사유기타 = etc;
                transfer.아동과의관계 = relation;

                // 이관 취소 시 복구 할 데이터 저장
                transfer.기존시도 = list.시도;
                transfer.기존시군구 = list.시군구;
                transfer.기존상세주소 = list.상세주소;
                transfer.기존연락처1 = list.연락처1;
                transfer.기존아동과의관계 = list.연락처1아동과의관계;

                TAPI.q220486_Transfer.InsertOnSubmit(transfer);
                TAPI.SubmitChanges();
            }
            return null;
        }



        
        public ActionResult AnswerInfo(int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                SP_q220486_GETLISTResult listResult = TAPI.SP_q220486_GETLIST(UID, pid).First();
               

                return View("AnswerInfo", listResult);
            }
        }

        public ActionResult SendInfo(int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }


            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220486_List list = TAPI.q220486_List.First(f => f.PID == pid);

                return View("SendInfo", list);
            }
        }

        public string SendInfoMSG(int pid, string phone, string uTel )
        {
             
            q220486_List list;
            try
            {
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    list = TAPI.q220486_List.FirstOrDefault(f => f.PID == pid);
            
            
                string mm = string.Format(@"안녕하십니까? ㈜한국리서치입니다.
이전에 문자로 안내드렸듯이, ㈜한국리서치는 아동권리보장원의 의뢰를 받아 향후 아동 관련 정책 수립지원을 위한 중요한 기초자료를 마련하고자 [아동보호통합패널] 조사를 수행하고 있습니다.

귀 댁을 담당하는 ㈜한국리서치 면접원 배정이 완료되었습니다.
귀 댁을 담당하는 ㈜한국리서치 면접원의 정보는 아래와 같습니다.
면접원 이름 : {0}
", list.면접원이름);

                    //if(uTel != "선택안함")
                    //{
                    //    mm = mm + "면접원 연락처:" + uTel;
                    //}
                    
mm = mm+ @"
귀 댁을 담당하는 면접원이 조사 안내와 승낙을 위해 곧 연락을 드릴 예정입니다.
귀하의 참여를 부탁드립니다.

관련하여 문의사항이 있으실 경우, 담당 면접원 또는 아래 콜센터로 문의주시기 바랍니다.

※ 아동보호통합패널조사 콜센터 02-3014-1025";
                
                    using (SENDMASTERDataContext SENDMASTER = new SENDMASTERDataContext("Data Source=172.19.100.85,15000;Initial Catalog=SEND_MASTER;Persist Security Info=True;User ID=hrcdv;password=Hrc#3014!#"))
                    {
                    
                        SENDMASTER.PROC_SEND_MESSAGE("interviwer", "0230141025", phone, "아동보호통합패널 조사 안내", mm, DateTime.Now, "", "2022-97-0486");
                    }

                    list.사전안내발송연락처 = phone;
                    list.안내문자발송일시 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    TAPI.SubmitChanges();
                }
            }catch(Exception e)
            {
                return e.ToString();
            }

            return "성공";
        }

        public ActionResult PreSurvey(int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                List<AnswerList> answerList = new List<AnswerList>();
                ViewBag.PID = pid.ToString();

                q220486_List List = null;
                bool eqnum = false;
                int answerStateCode = 0;
                using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q220486_99 WHERE PID = '{0}'", pid)).FirstOrDefault();
                    answerStateCode = t_SamplingList != null ? (int)(t_SamplingList.answerStateCode != null ? t_SamplingList.answerStateCode : 0) : 0;
                }

                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {

                    List = TAPI.q220486_List.First(f => f.PID == pid);
                    if ((List.사전조사진행상태 != null && List.사전조사진행상태 == "완료" ) || answerStateCode == 4 )
                    {
                        eqnum = true;
                    }

                    TAPI.SP_q220486_SETLIST(pid, 0, 0, "진행중");
                }

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt(string.Format("qn=q220486_99&t=tapi&pid={0}&atype=1&uid={1}&nv=true=1", pid, UID));
                }
                else
                {

                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\13094").Replace("/q220486_99", ""), pid);
                    using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                    {
                        StreamReader srAnswerData = new StreamReader(dataFile, Encoding.Default);
                        string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());
                        XElement AnswerData = XElement.Parse(answerDataDecrypt);

                        srAnswerData.Close();
                        srAnswerData.Dispose();

                        T_Script t_Script = SURVEYTOOL.T_Scripts.Where(w => w.questionnaireID == 13094).First();

                        if (t_Script != null)
                        {
                            XElement variableList = t_Script.variableList;

                            answerList = (from element in variableList.Elements()
                                          select new AnswerList
                                          {
                                              Variable = element.Name.ToString(),
                                              Title = element.Value,
                                              Answer = GetAnswerData(AnswerData, element.Name.ToString()),
                                              Url = TripleDESCryptoService.Encrypt(string.Format("qn=q220486_99&pid={0}&uid={1}&t=tapi&eqnum={2}&nv=true&atype=1", pid, UID, element.Name.ToString())),
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

        /// <param name="pid"></param>
        /// <param name="type">
        /// 0:기본인적설문
        /// 1:양육자설문
        /// 2:아동설문
        /// 3:자립준비청년설문
        /// </param>
        /// <returns></returns>
        public ActionResult Survey(int pid, int type, int isSend=0)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }
            
            
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                try
                { 
                    q220486_List list = TAPI.q220486_List.First(f => f.PID == pid);


                    string QMListData = $@"<RPS_SamplingLIST_INFO>
                                        <아동이름 VALUE=""{list.아동이름}"" />
                                        <양육자이름 VALUE=""{list.양육자성명}"" />
                                        <면접원이름 VALUE=""{list.면접원이름}"" />
                                        <양육자성별 VALUE=""{list.양육자성별}"" />
                                        <양육자생년 VALUE=""{list.양육자생년}"" />
                                        <양육자생월 VALUE=""{list.양육자생월}"" />
                                        <양육자생일 VALUE=""{list.양육자생일}"" />
                                        <아동생년 VALUE=""{list.아동생년}"" />
                                        <아동생월 VALUE=""{list.아동생월}"" />
                                        <아동생일 VALUE=""{list.아동생일}"" />
                                        <아동성별 VALUE=""{list.아동성별}"" />
                                        <양육자아동과의관계 VALUE=""{list.양육자아동과의관계}"" />
                                        <양육자아동과의관계기타 VALUE=""{list.양육자아동과의관계기타}"" />
                                        <보호유형 VALUE=""{list.보호유형}"" />
                                        <아동구분 VALUE=""{list.아동구분}"" />
                                        <설문유형 VALUE=""{list.설문유형}"" />
                                        <학년구분 VALUE=""{list.학년구분}"" />
                                        </RPS_SamplingLIST_INFO>";

                    string endPN = "";
                    int qID = 13090;
                    if (type == 0)
                    {
                        qID = 13094;
                        endPN = "_99";
                    }
                    else if (type == 2)
                    {
                        qID = 13095;
                        endPN = "_2";
                    }
                    if (type == 3)
                    {
                        qID = 13092;
                        endPN = "_3";
                    }

                    string query = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q220486{2}]') AND type in (N'U'))
BEGIN
    MERGE SURVEY.dbo.T_SamplingList_q220486{2} as SLIST
    USING
    (
        SELECT	'{0}' as PID
    ) as Answer on SLIST.PID = Answer.PID
    WHEN MATCHED THEN
	    UPDATE	SET	QMListData = N'{1}'
    WHEN NOT MATCHED THEN
	    INSERT (PID, QMListData)
	    VALUES (PID, N'{1}');
END", pid, QMListData, endPN);

                    TAPI.ExecuteCommand(query, "");
                    TAPI.SubmitChanges();
                    
                    
                    ViewBag.PID = pid.ToString();
                   

                    if (isSend == 1)
                    {
                        ViewBag.type = type;
                        if (string.IsNullOrEmpty(list.발송여부) == true)
                        {
                            return View("SendSurveyCheck", list);
                        }
                        else
                        {
                            return SendSurvey( pid, type);
                        }

                    }
                    else
                    {
                        List<AnswerList> answerList = new List<AnswerList>();
                        ViewBag.PID = pid.ToString();

                        bool eqnum = false;
                        int answerStateCode = 0;
                        using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                        {
                            T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q220486{1} WHERE PID = '{0}'", pid, endPN)).FirstOrDefault();
                            answerStateCode = t_SamplingList != null ? (int)(t_SamplingList.answerStateCode != null ? t_SamplingList.answerStateCode : 0) : 0;
                        }

                        // (list.사전조사진행상태 != null && list.사전조사진행상태 == "완료") ||
                        if (answerStateCode == 4
                            || (type == 0 && list.사전조사진행상태 != null && list.사전조사진행상태 == "완료")
                            || (type == 1 && list.양육자설문진행상태 != null && list.양육자설문진행상태 == "완료")
                            || (type > 1 && list.아동설문진행상태 != null && list.아동설문진행상태 == "완료")
                        )
                        {
                            eqnum = true;
                        }

                        TAPI.SP_q220486_SETLIST(pid, 0, type, "진행중");
                        if (eqnum == false)
                        {

                            ViewBag.C = TripleDESCryptoService.Encrypt(string.Format("qn=q220486{2}&t=tapi&pid={0}&atype=1&uid={1}&nv=true=1", pid, UID, endPN));
                        }
                        else
                        {
                            string dataFile =   string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + qID.ToString()).Replace("/q220486" + endPN, ""), pid);
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
                                                      Url = TripleDESCryptoService.Encrypt(string.Format("qn=q220486{3}&pid={0}&uid={1}&t=tapi&eqnum={2}&nv=true&atype=1", pid, UID, element.Name.ToString(), endPN)),
                                                  }).Where(w => string.IsNullOrEmpty(w.Answer) == false).ToList();
                                }
                            }
                        }
                        return View("EditSurvey", answerList);
                    }

                }
                catch (Exception ee)
                {
                    return View("Error", (object)(ee.Message));
                }

            }
        }

        public ActionResult SendSurvey(int pid, int type)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            q220486_List list = null;
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.q220486_List.First(f => f.PID == pid);
                if (string.IsNullOrEmpty(list.발송여부) == true)
                {
                    ViewBag.타이틀문구 = "온라인 조사 진행 ";
                    ViewBag.상단안내문구 = "웹발송 신청을 위해 필요한 정보를 입력해 주십시오.";
                }
                else
                {
                    ViewBag.타이틀문구 = "이미 발송완료되었습니다 ";
                    ViewBag.상단안내문구 = "이미 발송이 완료되었습니다. 재전달을 위해 발송 신청을 다시 하시겠습니까?";
                }

                ViewBag.발송조사구분값 = "아동";
                if (type == 1)
                {
                    ViewBag.발송조사구분값 = "양육자";
                }
            }
            return View("SendSurvey", list);
        }

        /// <param name="pid"></param>
        /// <param name="phone"></param>
        /// <param name="surveyType">발송조사 구분값 = 양육자/아동</param>
        /// <returns></returns>
        public string SendMSG(int pid, string phone, string surveyType )
        {
            string nm = "양육자 설문";
            string PN = "q220486";
            string gubun = "1"; //설명문 파일 구분자  1:영유아보호자, 2_1:초저보호자, 2_2:초저아동, 3_1:초고보호자, 3_2:초고아동, 4_1:청소년보호자, 4_2:청소년아동, 5:자립준비청년
            string name = "";

            q220486_List list;
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.q220486_List.FirstOrDefault(f => f.PID == pid);
                name = list.아동이름;

                if (surveyType == "아동")
                {
                    nm = "아동용 설문";
                    PN = "q220486_2";
                    if (list.설문유형 == "초저")
                    {
                        gubun = "2_2";
                    }
                    else if (list.설문유형 == "초고")
                    {
                        gubun = "3_2";
                    }
                    else if (list.설문유형 == "청소년")
                    {
                        gubun = "4_2";
                    }
                    else if (list.설문유형 == "자립준비청년")
                    {
                        nm = "자립준비청년 설문";
                        PN = "q220486_3";
                        gubun = "5";
                    }
                }
                else
                {
                    if (list.설문유형 == "영유아")
                    {
                        nm = "영유아 양육자 설문";
                        gubun = "1";
                    }
                    else if (list.설문유형 == "초저")
                    {
                        gubun = "2_1";
                    }
                    else if (list.설문유형 == "초고")
                    {
                        gubun = "3_1";
                    }
                    else if (list.설문유형 == "청소년")
                    {
                        gubun = "4_1";
                    }
                }
               

            string surveyURL = "https://rpssurvey.hrcglobal.com/?c="+TripleDESCryptoService.Encrypt(string.Format("qn={2}&pid={0}&uid={1}&nv=true&atype=2", pid, UID, PN));

            string mm = string.Format(@"안녕하십니까? ㈜한국리서치입니다.
전화로 사전에 안내드린 내용과 같이, ㈜한국리서치는 아동권리보장원의 의뢰를 받아 ‘아동보호통합패널조사 본조사’를 실시하고 있습니다. 
귀하께서는 비대면 온라인 조사 형태로 참여를 요청하셨기에 온라인 조사 참여 링크를 문자로 전달드립니다.
본 문자는 “{0} ” 조사 참여 링크입니다.
요청하신 설문 유형이 맞는지 확인하시고, 아래의 설문링크를 클릭하여 참여해주십시오.

■ 설문링크 : 
{1}

■ 참여시간: 약 20분(이어하기 가능)
■ 사례품: 모바일 문화상품권 3만원 (조사 완료 후 4주내 문자발송)
■ 설명문 : https://ms.hrcglobal.com/etc/220486/{2}.pdf

관련하여 문의사항이 있으실 경우, 귀하께 연락을 드렸던 담당 면접원에게 문의해주시기 바랍니다.

※  문의전화: 02-3149-1896
", nm, surveyURL, gubun);

            using (SENDMASTERDataContext SENDMASTER = new SENDMASTERDataContext("Data Source=172.19.100.85,15000;Initial Catalog=SEND_MASTER;Persist Security Info=True;User ID=hrcdv;password=Hrc#3014!#"))
            {   

                SENDMASTER.PROC_SEND_MESSAGE("interviwer", "0231491896", phone, "[한국리서치] 설문참여 안내", mm, DateTime.Now, "", "2022-97-0486");
            }

                list.발송연락처 = phone;
                list.발송여부 = "문자발송";
               
                if (surveyType == "아동")
                {
                    list.아동설문문자발송일시 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    list.양육자설문문자발송일시 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
                TAPI.SubmitChanges();

                if (surveyType == "아동")
                {
                    TAPI.SP_q220486_SETLIST(pid, 0, 2, "웹발송");
                }
                else
                {
                    TAPI.SP_q220486_SETLIST(pid, 0, 1, "웹발송");
                }

                
            }
            return "성공";
        }

        public string SendEmail(int pid, string email1, string email2, string surveyType)
        {
            string email = email1 + '@' + email2;
            string nm = "양육자 설문";
            string gubun = "1"; //설명문 파일 구분자  1:영유아보호자, 2_1:초저보호자, 2_2:초저아동, 3_1:초고보호자, 3_2:초고아동, 4_1:청소년보호자, 4_2:청소년아동, 5:자립준비청년
            string PN = "q220486";

            string name = "";
            q220486_List list;
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.q220486_List.FirstOrDefault(f => f.PID == pid);
                name = list.아동이름;

                if (surveyType == "아동")
                {
                    nm = "아동용 설문";
                    PN = "q220486_2";
                    if (list.설문유형 == "초저")
                    {
                        gubun = "2_2";
                    }
                    else if (list.설문유형 == "초고")
                    {
                        gubun = "3_2";
                    }
                    else if (list.설문유형 == "청소년")
                    {
                        gubun = "4_2";
                    }
                    else if (list.설문유형 == "자립준비청년")
                    {
                        nm = "자립준비청년 설문";
                        PN = "q220486_3";
                        gubun = "5";
                    }
                }
                else
                {
                    if (list.설문유형 == "영유아")
                    {
                        nm = "영유아 양육자 설문";
                        gubun = "1";
                    }
                    else if (list.설문유형 == "초저")
                    {
                        gubun = "2_1";
                    }
                    else if (list.설문유형 == "초고")
                    {
                        gubun = "3_1";
                    }
                    else if (list.설문유형 == "청소년")
                    {
                        gubun = "4_1";
                    }
                }

                string surveyURL = "https://rpssurvey.hrcglobal.com/?c=" + TripleDESCryptoService.Encrypt(string.Format("qn={2}&pid={0}&uid={1}&nv=true&atype=2", pid, UID, PN));

                string tel = string.IsNullOrEmpty(list.면접원연락처) ? "" : list.면접원연락처;
            string body = string.Format(@"<html><meta charset='etc-kr'>
<title>:::: 한국리서치 ::::</title>
</head>
<body>
	<img src='http://a21.hrcglobal.com/survey_progress.cgi?PN=q220486&email={0}&PID={1}&F=1' width='1' height='1' /> 
<table width='640' cellspacing='0' cellpadding='0' border='1' style='border-collapse:collapse; border:1px #eaeaea solid;'>
  <tbody>
    <tr><td><table width='640' border='0' cellspacing='0' cellpadding='0'>
  <tbody>
    <tr>
      <td height='189' align='center' valign='top' bgcolor='#247FF5'>
	    <table width='580' border='0' cellspacing='0' cellpadding='0'>
        <tbody>
			<tr><td height='20' colspan='2'></td></tr>
          <tr>
<td><table width='100%' border='0' cellspacing='0' cellpadding='0'>
<tbody>
<tr>
<td width='110' height='38'><a href='https://www.hrc.co.kr/' target='_blank'><img src='http://ms.hrcglobal.com/mailform/2021/images/logo.png' width='110' height='38' border='0'/></a></td>
<td align='right'><table width='0%' border='0' cellspacing='0' cellpadding='0'>
<tbody>
<tr>
<!--모바일 참여영역-->
<td><img src='http://ms.hrcglobal.com/mailform/2021/images/ico_mobile.png' width='155' height='36' alt=''/></td>
<!--//모바일 참여영역 끝-->
<td width='30'>&nbsp;</td>
<!--pc 참여영역-->
<td><img src='http://ms.hrcglobal.com/mailform/2021/images/ico_pc.png' width='128' height='36' alt=''/></td>
<!--//pc 참여영역-->
</tr>
</tbody>
</table></td>
</tr>
</tbody>
</table></td>
</tr>
<tr>
<td height='131' align='center'><span style='font-size:20pt; color:#ffffff;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕' color='#ffffff'><b>아동보호통합패널조사 예비조사<br>비대면 온라인 {2}</b></font></span></td>
</tr>
</tbody>
</table></td>
</tr>
    <tr><td height='40'>&nbsp;</td></tr>
    <tr>
      <td align='center'><table width='540' border='0' cellspacing='0' cellpadding='0'>
        <tbody>
          <tr>
            <td align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'>안녕하십니까? ㈜한국리서치입니다.<br><br>
전화로 사전에 안내드린 내용과 같이, ㈜한국리서치는 아동권리보장원의 의뢰를 받아 ‘아동보호통합패널조사 본조사’를 실시하고 있습니다.<br>귀하께서는 비대면 온라인 조사 형태로 참여를 요청하셨기에 온라인 조사 참여 링크를 이메일로 전달드립니다.<br><br>
본 메일은 “{2}” 조사 참여 링크입니다.<br><br>
요청하신 설문 유형 링크가 맞는지 확인하시고, 아래의 조사참여하기 버튼을 눌러 참여해주십시오.<br><br>
관련하여 문의사항이 있으실 경우, 귀하께 연락을 드렸던 담당 면접원에게 문의해주시기 바랍니다.<br>
※  문의전화: {5}<br>
</font></span></td>
</tr>
<tr>
<td height='30'>&nbsp;</td>
</tr>
<tr>
<td><table width='540' border='0' cellspacing='0' cellpadding='0'>
<tbody>
<tr>
<td width='25' height='50' bgcolor='#eeeeee'>&nbsp;</td>
<td width='120' bgcolor='#eeeeee' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><b>설명문</b></font></span></td>
<td width='20' bgcolor='#fafafa'>&nbsp;</td>
<td bgcolor='#fafafa' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><a href='https://ms.hrcglobal.com/etc/220486/{4}.pdf' target='_blank'>설명문 보기(클릭)</a></font></span></td>
</tr>
<tr>
<td height='1' colspan='4'></td>
</tr> <tr>
<td width='25' bgcolor='#eeeeee'>&nbsp;</td>
<td width='120' bgcolor='#eeeeee' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><b>사례품</b></font></span></td>
<td width='20' bgcolor='#fafafa'>&nbsp;</td>
<td height='80' bgcolor='#fafafa' align='left'>
<p><font face='맑은고딕,맑은 고딕'><span style='font-size:12pt;'><b>모바일 문화상품권 3만원 (본조사 완료시)</b></span></font></p>
<p><font face='맑은고딕,맑은 고딕'><span style='font-size:10pt;'><b>*사례금은 조사 완료 후 4주내 문자발송됩니다.</b></span></font></p>
</td>
</tr>
<tr> <td height='1' colspan='4'></td> </tr>
<tr>
<td width='25' height='50' bgcolor='#eeeeee'>&nbsp;</td>
<td width='120' bgcolor='#eeeeee' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><b>소요시간</b></font></span></td>
<td width='20' bgcolor='#fafafa'>&nbsp;</td>
<td bgcolor='#fafafa' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'>약 20분</font></span></td>
</tr>
</tbody>
</table></td>
</tr>
<tr>
<td height='20'>&nbsp;</td>
</tr>
<tr><td><table width='100%' border='0' cellspacing='0' cellpadding='0'>
<tbody><tr>
<td width='24' height='20' valign='top'><img src='http://ms.hrcglobal.com/mailform/2021/images/info.png' width='16' height='16' alt=''/></td>
<td valign='top' align='left'><span style='font-size:9pt;letter-spacing: -0.1em;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕' color='#414141'>조사 참여 중 화면을 닫고 다시 접속 하실 경우 참여하셨던 이후의 문항부터 재 참여가 가능합니다.</font></span></td>
</tr>
</tbody>
</table></td>
</tr>
<tr> <td height='35'>&nbsp;</td></tr><tr>
<td align='center'>
<!--조사참여버튼-->
<table border='0' cellspacing='0' cellpadding='0'>
<tr>
<td width='200' height='55' align='center' bgcolor='#247FF5'>
<!--링크주소는 #위치에 넣어주세요--><a href='{3}' target='_blank' style='display: block; height:100%; line-height:60px; color:#fff;font-weight: bold; text-decoration: none;font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'>조사참여하기</font></a>
</td>
</tr></table>
<!--//조사참여버튼 끝-->
</td>
</tr>
<tr>
<td height='30'>&nbsp;</td>
</tr>
</tbody>
</table></td>
</tr>
<tr><td height='1' bgcolor='#eaeaea'></td></tr>
 <tr><td height='1' bgcolor='#eaeaea'></td></tr> 
<tr>
<td align='center' valign='top' bgcolor='#fafafa'><table width='540' border='0' cellspacing='0' cellpadding='0'>
<tbody>
<tr><td height='20'></td></tr>
<tr>
<td align='left' valign='bottom'>
<span style='font-size:9pt; color:#616161; letter-spacing: -0.1em; line-height:150%;'>
<font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'>
- 본 메일은 발신전용으로 회신이 불가능합니다.
</font><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'></span></td>
</tr>
<tr>
<td height='20' align='left'></td>
</tr>
<tr> <td align='left'><span style='font-size:9pt; color:#222;'>
<font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><b>COPYRIGHT 2021 (C) HANKOOK RESERARCH ALL RIGHTS RESEVED.</b></font></span></td>
</tr>
<tr> <td height='20' align='left'></td></tr>
</tbody>
</table></td>
</tr>
</tbody>
</table>	
</td>
</tr>
</tbody>
</table>
</body>
</html>", email, pid, nm, surveyURL, gubun, tel);

                using (SENDMASTERDataContext SENDMASTER = new SENDMASTERDataContext("Data Source=172.19.100.85,15000;Initial Catalog=SEND_MASTER;Persist Security Info=True;User ID=hrcdv;password=Hrc#3014!#"))
                {

                    SENDMASTER.PROC_SEND_EMAIL("interviwer", "apanel@hrc.co.kr", email, "[아동보호통합패널조사] 설문 참여 안내", body, "", "", DateTime.Now, "", "", "2022-97-0486");
                    //SENDMASTER.PROC_SEND_EMAIL("interviwer", "apanel@hrc.co.kr", email, "[아동보호통합패널조사] 설문 참여 안내", body, "","",DateTime.Now, "","", "2022-97-0486");

                }

             
                list.발송이메일 = email;
                list.발송여부 = "이메일발송";
                if (surveyType == "아동")
                {
                    list.아동설문이메일발송일시 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    list.양육자설문이메일발송일시 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
                    
                TAPI.SubmitChanges();

                if (surveyType == "아동")
                {
                    TAPI.SP_q220486_SETLIST(pid, 0, 2, "웹발송");
                }
                else
                {
                    TAPI.SP_q220486_SETLIST(pid, 0, 1, "웹발송");
                }
                
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
        public ActionResult Contact(int pid, int isPreSurveyCall = 0)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }
            
            List<q220486_Contact> ContactList = new List<q220486_Contact>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                
                q220486_List list = TAPI.q220486_List.FirstOrDefault(F => F.PID == pid);
                ContactList = (from contact in TAPI.q220486_Contact
                               where contact.PID == pid
                                       select contact).ToList();

                ViewBag.PID = list.PID;
                ViewBag.아동이름 = list.아동이름;
                ViewBag.원대체구분 = list.원대체구분;
                ViewBag.자격취득년도 = list.자격취득년도;
                ViewBag.아동성별 = list.아동성별;
                ViewBag.학년구분 = list.학년구분;
                ViewBag.아동생년월일 = list.아동생년 + "년 " + list.아동생월 + "월 " + list.아동생일 + "일";
                ViewBag.아동만나이 = list.아동만나이;
                ViewBag.시도 = list.시도;
                ViewBag.시군구 = list.시군구;
                ViewBag.상세주소 = list.상세주소;
                ViewBag.연락처1 = list.연락처1;
                ViewBag.연락처1관계 = list.연락처1아동과의관계;
                ViewBag.연락처2 = list.연락처2;
                ViewBag.연락처2관계 = list.연락처2아동과의관계;
                ViewBag.연락처3 = list.연락처3;
                ViewBag.연락처3관계 = list.연락처3아동과의관계;
                ViewBag.특이사항 = list.특이사항;                
                ViewBag.양육자성명 = list.양육자성명;
                ViewBag.양육자아동과의관계 = list.양육자아동과의관계;
                ViewBag.양육자전화번호 = list.양육자전화번호;
                ViewBag.사전안내발송연락처 = list.사전안내발송연락처;

                ViewBag.Ranking = (ContactList.Count + 1).ToString();
                ViewBag.contactYear = DateTime.Now.ToString("yyyy");
                ViewBag.contactMonth = DateTime.Now.ToString("MM");
                ViewBag.contactDay = DateTime.Now.ToString("dd");

                ViewBag.사전조사오픈여부 = "불가능";
                if (isPreSurveyCall == 1 && list.최종설문진행상태!= "대체요청")
                {
                    ViewBag.사전조사오픈여부 = "가능";
                }                
            }

            return View("Contact", ContactList);
        }
       
        public string ContactOK(int pid, int ranking, string contact, string contactMonth, string contactDay, string contactTime, string etc)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }            

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220486_Contact contactTable = new q220486_Contact();

                contactTable.PID = pid;
                contactTable.면접원아이디 = UID;
                contactTable.방문횟수 = ranking;
                contactTable.방문일자 = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;
                contactTable.방문시간 = contactTime;
                contactTable.비성공사유 = contact;
                contactTable.세부사유 = etc;
                contactTable.inputDate = DateTime.Now;

                TAPI.q220486_Contact.InsertOnSubmit(contactTable);
                


                q220486_List list = TAPI.q220486_List.First(f => f.PID == pid);

                list.최종접촉횟수 = ranking;
                list.최종접촉결과 = contact;
                list.최종접촉결과세부사유 = etc;

                TAPI.SubmitChanges();
            }

            return "저장";
        }
        public string SaveEtc(int pid, string etc)
        {

            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {

                q220486_List list = TAPI.q220486_List.First(f => f.PID == pid);
                
                list.면접원특이사항 = etc;

                TAPI.SubmitChanges();
            }

            return "저장";
        }

    }
}
