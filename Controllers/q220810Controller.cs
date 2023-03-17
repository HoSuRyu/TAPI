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

/// <summary>
/// 콜센터 계정 따로 관리한 인터뷰어 ㅣ 999499
/// </summary>

namespace TAPI_Interviewer.Controllers
{
    public class q220810Controller : Controller
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

        public List<SP_q220810_GETLISTResult> GetList()
        {
            
            List<SP_q220810_GETLISTResult> list = new List<SP_q220810_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220810_GETLIST(UID, null).ToList();
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 12986).FirstOrDefault();

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

            //if(UID==999499)
            //{
            //    return View("CallList", GetList());
            //}
            return View("List", GetList());
        }

        public ActionResult ListPartial()
        {
            //if (UID == 999499)
            //{
            //    return PartialView("CallListPartial", GetList());
            //}
            return PartialView("ListPartial", GetList());
        }


        
        public ActionResult ProgressPartial()
        {
            ProgressCount progressCount = new ProgressCount();

            progressCount.All = "0";
            progressCount.Ing = "0";
            progressCount.End = "0";

            List<SP_q220810_GETLISTResult> list = new List<SP_q220810_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220810_GETLIST(UID, null).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "조사완료").Count().ToString();
            }

            return PartialView("ProgressPartial", progressCount);
        }
        


        public ActionResult Contact(int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }



            List<q220810_Contact> ContactList = new List<q220810_Contact>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220810_List PanelList = TAPI.q220810_List.FirstOrDefault(f => f.PID == pid);


                ContactList = (from contact in TAPI.q220810_Contact
                               where contact.PID == pid
                                       select contact).ToList();

                ViewBag.PID = PanelList.PID;
                ViewBag.패널구분 = PanelList.패널구분;
                ViewBag.이름 = PanelList.이름;
                ViewBag.연령대 = PanelList.연령대;
                ViewBag.성별 = PanelList.성별;
                ViewBag.세부주소 = PanelList.세부주소;
                ViewBag.휴대전화 = PanelList.휴대전화;
                

                ViewBag.Ranking = (ContactList.Count + 1).ToString();
                ViewBag.contactYear = DateTime.Now.ToString("yyyy");
                ViewBag.contactMonth = DateTime.Now.ToString("MM");
                ViewBag.contactDay = DateTime.Now.ToString("dd");

            }
            return View("Contact", ContactList);
        }


        public string ContactOK(int pid, int ranking, string contact, string contactMonth, string contactDay, string contactTime)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            q220810_Contact Contact = new q220810_Contact();
           


            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                Contact.PID = pid;
                Contact.면접원아이디 = UID;
                Contact.방문횟수 = ranking;
                Contact.방문일자 = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;
                Contact.방문시간 = contactTime;
                Contact.비성공사유 = contact;
                Contact.inputDate = DateTime.Now;
                Contact.담당자 = "면접원";
                if( UID == 999499)
                {
                    Contact.담당자 = "콜센터";
                }

                TAPI.q220810_Contact.InsertOnSubmit(Contact);
                
                if( contact == "거절(사생활 노출 기피)" || contact == "거절(정부 불신)" || contact == "장애, 사고, 질병" || contact == "전화번호 틀림" || contact == "조사대상 아님")
                {
                    TAPI.SP_q220810_SETLIST(pid, "탈락요청");
                }

                TAPI.SubmitChanges();
            }

            return "저장";
        }
        


        public string SendMSG(int pid)
        {
            string phone = "";
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220810_List list = TAPI.q220810_List.First(f => f.PID == pid);
                phone = list.휴대전화;
                if (phone == "")
                    return "휴대전화 번호를 확인해 주세요.";

            }

            using (SENDMASTERDataContext SENDMASTER = new SENDMASTERDataContext("Data Source=172.19.100.85,15000;Initial Catalog=SEND_MASTER;Persist Security Info=True;User ID=hrcdv;password=Hrc#3014!#"))
            {

                int itype = 1;
                if( UID == 999499)
                {
                    itype = 0;
                }

                //  https://rpssurvey.hrcglobal.com/?qn=q211144_1&PID={0}&atype=1&itype={1}
                string mm = string.Format(@"안녕하십니까? 여론조사 전문회사인 한국리서치입니다.
한국리서치에서는 서울연구원과 서울시의 의뢰를 받아 서울 청년패널조사를 실시하고 있습니다. 
아래 링크를 클릭하시면 조사 페이지로 이동이 됩니다.

■ 링크 : https://rpssurvey.hrcglobal.com/?qn=q220810&PID={0}&itype={1}


■ 조사기간 : ~11/30(화) 까지
■ 사례비 : 2만원(본 조사 완료 시)
■ 소요시간 : 약 35-40분
■ 12월까지 조사 완료 시 추첨을 통한 경품 제공


본 조사는 청년의 삶을 다각적으로 파악하여 청년 정책 수립 및 개선의 기초자료를 마련하는데 그 목적이 있습니다.

본 조사는 약 35분 정도 소요되며, 저희가 드리는 질문에는 맞고 틀림이 없습니다.
평소에 가지고 계셨던 생각이나 느낌을 자유롭게 응답해주시기 바랍니다.

또한 본 조사의 모든 응답 내용은 통계법 제33조 및 제34조에 의거하여
통계목적에만 사용되며 그 비밀은 반드시 보장됩니다. 

* 본 조사는 2020년 서울청년패널조사에 참여하셨거나, 2021년 사전조사에 참여해 주신 분들을 대상으로 하며, 본 조사 링크는 문자 받으신 본인만 참여 가능합니다. (전달, 배포 금지)
* 조사 참여 중 화면을 닫고 다시 접속 하실 경우 참여하셨던 이후의 문항부터 재 참여가 가능합니다. 
* 뒤로가기가 불가능하오니, 참고하시어 진행 부탁 드립니다.
* 응답하신 내용에 따라 조사 대상이 아니시거나, 대상자가 초과되어 설문이 조기에 마감될 수 있습니다. 

문의전화: 02-3014-1025(월~금, 09:30~17:30)
무료수신거부 : 080-863-5608
", pid, itype);
                


                SENDMASTER.PROC_SEND_MESSAGE("interviwer", "02-3014-1025", phone, "2022년 서울청년패널 조사 참여 안내", mm, DateTime.Now, "", "2022-97-0810");

            }
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                TAPI.SP_q220810_SETLIST(pid, "문자발송");
            }
            return "성공";
        }
        

        

        //public ActionResult Quota()
        //{
        //    if (UID <= 0)
        //    {
        //        return RedirectToAction("Login", "Home");
        //    }

        //    List<SP_q211144_GETQUOTAResult> list = new List<SP_q211144_GETQUOTAResult>();


        //    using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
        //    {
        //        list = TAPI.SP_q211144_GETQUOTA(UID).ToList();
        //    }

        //    return View("Quota", list);
        //}
    }
}