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
    public class q221268Controller : Controller
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

        public List<SP_q221268_GETLISTResult> GetList()
        {
            
             List<SP_q221268_GETLISTResult> list = new List<SP_q221268_GETLISTResult>();
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221268_GETLIST(UID, null).ToList();
                
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

            List<string> contactList = list.GroupBy(g => g.컨택현황).Where(w => string.IsNullOrEmpty(w.Key) == false).Select(s => s.Key).ToList();
            contactList.Insert(0, "");
            ViewBag.ContactList = contactList;

            ViewBag.마스터계정 = 0;
            if (UID == 700489 || UID == 700505)
            {
                ViewBag.마스터계정 = 1;
            }
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
            List<SP_q221268_GETLISTResult> list = new List<SP_q221268_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q221268_GETLIST(UID, null).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "완료").Count().ToString();
            }

            return PartialView("ProgressPartial", progressCount);
        }
        

        public ActionResult AnswerInfo(int pid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }
            
            q221268_List list = null;

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                 list = TAPI.q221268_List.First(f => f.PID == pid);
            }
            ViewBag.마스터계정 = 0;
            ViewBag.담당면접원 = "";
            if (UID == 700489 || UID == 700505)
            {
                ViewBag.마스터계정 = 1;
                if (list.담당면접원 > 0)
                {

                    for (int i = 1; i <= 15; i++)
                    {
                        int _uid = UID + i;
                        if (list.담당면접원 == _uid)
                        {
                            ViewBag.담당면접원 = i;
                            break;
                        }
                    }
                }
               
            }
           
            return View("AnswerInfo", list);
        }

        public string distributionUID(int pid, int uidIDX)
        {
            q221268_List list = null;
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.q221268_List.First(f => f.PID == pid);


                list.담당면접원 = UID + uidIDX;
                list.배부일자 = DateTime.Now.ToString();

                TAPI.SubmitChanges();
            }
            return "";
        }

        public string SendMSG( int pid, string phone)
        {
            q221268_List list = null;
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.q221268_List.First(f => f.PID == pid);
            }
            try {
                using (SENDMASTERDataContext SENDMASTER = new SENDMASTERDataContext("Data Source=172.19.100.85,15000;Initial Catalog=SEND_MASTER;Persist Security Info=True;User ID=hrcdv;password=Hrc#3014!#"))
                {
                    string mm = string.Format(@"안녕하십니까? {0} 담당자님,
정보통신정책연구원에서는 디지털 전환에 따른 산업의 변화를 점검하고, 국제 기준에 부합하는 디지털 산업 통계를 산출하기 위해 [디지털 산업 통계조사]를 진행하고 있습니다.
아래의 설문링크를 클릭하여 설문에 참여 부탁드립니다.
　
■ 설문링크 : https://survey.hrcglobal.com/kisdi/?gb=2&pid={1}

　
본 조사의 결과는 현재 우리나라의 디지털 산업의 매출, 인력구조, R&D 및 수출 등에 대한 일반적인 실태파악 뿐 아니라, 디지털 기술의 활용 및 확산을 위한 국가 차원의 지원과 세부 정책 개발의 근거 자료로 활용 될 예정입니다.
조사에 응해주시는 기업 담당자 여러분 개인에 대한 정보는 통계법 제33조에 의해 절대로 노출되지 않으며, 응답내용과 결과는 연구목적 이외에는 사용되지 않음을 알려드립니다.
　
■ 조사 기간 : 2022년 11월 30일까지
■ 사례금: 모바일문화상품권 20,000원 (12월 둘째주 일괄문자발송)
■ 공문 보기 : https://ms.hrcglobal.com/etc/221268_document.pdf

■ 설문지 보기 : https://ms.hrcglobal.com/etc/221268_qu.pdf

■ 응답가이드 보기 : https://ms.hrcglobal.com/etc/221268_guide.pdf

■ 조사주관 : 정보통신정책연구원 ICT데이터사이언스 연구본부 정부연 위원
■ 조사수행기관 : ㈜한국리서치 이태훈 부장(02-3014-1074, thlee@hrc.co.kr)

　
　
※  웹설문 문의전화: 080-558-9000
(한국리서치 월~금, 09:30~17:30)
　
※  카카오톡문의:
http://pf.kakao.com/_GYxhbM/chat

※  무료수신거부 : 
080-863-5608
", list.사업체명, pid);

                    SENDMASTER.PROC_SEND_MESSAGE("interviwer", "080-558-9000", phone, "[한국리서치]2022년 디지털 산업 통계조사", mm, DateTime.Now, "", "2022-86-1268");

                }
            }
            catch(Exception e)
            {
                return e.ToString();
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                TAPI.SP_q221268_SETLIST(pid, "문자발송");
                
            }
            return "성공";
        }

        public string SendEmail(int pid, string email)
        {
          
                 
                string body = string.Format(@"<!doctype html>
<html>
<head>
<meta charset='etc-kr'>
<title>:::: 한국리서치 ::::</title>
</head>
<body>
<img src='https://b22.hrcglobal.com/survey_progress.cgi?PN=p221268&email={0}&PID=LIST_{1}&F=1' width='1' height='1' /> 
<table width='640' cellspacing='0' cellpadding='0' border='1' style='border-collapse:collapse; border:1px #eaeaea solid;'>
<tbody>
<tr>
<td>
<table width='640' border='0' cellspacing='0' cellpadding='0'>
<tbody>
<tr>
<td height='189' align='center' valign='top' bgcolor='#D91616'>
<table width='580' border='0' cellspacing='0' cellpadding='0'>
<tbody>
<tr>
<td height='20' colspan='2'></td>
</tr>
<tr>
<td><table width='100%' border='0' cellspacing='0' cellpadding='0'>
<tbody>
<tr>
<td width='110' height='38'><a href='https://www.hrc.co.kr/' target='_blank'><img src='https://ms.hrcglobal.com/mailform/2021/images/logo.png' width='110' height='38' border='0'/></a></td>
<td align='right'><table width='0%' border='0' cellspacing='0' cellpadding='0'>
<tbody>
<tr><td><img src='https://ms.hrcglobal.com/mailform/2021/images/ico_mobile.png' width='155' height='36' alt=''/></td>
<td width='30'>&nbsp;</td> <td><img src='https://ms.hrcglobal.com/mailform/2021/images/ico_pc.png' width='128' height='36' alt=''/></td></tr>
</tbody>
</table></td>
</tr>
</tbody>
</table></td>
</tr>
<tr>
<td height='131' align='center'>
<p style='color: rgb(0, 0, 0); font-family: &quot;Malgun Gothic&quot;; font-size: medium; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: -webkit-center; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(217, 22, 22); text-decoration-thickness: initial; text-decoration-style: initial;
text-decoration-color: initial;'><span style='font-size: 20pt; color: rgb(255, 255, 255);'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕' color='#ffffff'><b>2022년 디지털 산업 통계조사</b></font></span><b></b></p>
</td></tr></tbody></table></td>    </tr>
<tr><td height='40'>&nbsp;</td></tr>
<tr><td align='center'><table width='540' border='0' cellspacing='0' cellpadding='0'>
<tbody>
<tr>
<td align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'>
안녕하십니까?<br>
정보통신정책연구원에서는 디지털 전환에 따른 산업의 변화를 점검하고, 국제 기준에 부합하는 디지털 산업 통계를 산출하기 위해 [디지털 산업 통계조사]를 진행하고 있습니다.<br><br>
본 조사의 결과는 현재 우리나라의 디지털 산업의 매출, 인력구조, R&D 및 수출 등에 대한 일반적인 실태파악 뿐 아니라, 디지털 기술의 활용 및 확산을 위한 국가 차원의 지원과 세부 정책 개발의 근거 자료로 활용 될 예정입니다.<br><br>

조사에 응해주시는 기업 담당자 여러분 개인에 대한 정보는 통계법 제33조에 의해 절대로 노출되지 않으며, 응답내용과 결과는 연구목적 이외에는 사용되지 않음을 알려드립니다. <br>
잠시만 시간을 내시어 솔직하게 응답해 주시기 바랍니다.<br>
감사합니다.<br><br>
<b><font color='#C00000'>아래의 조사참여하기 버튼을 클릭하시면 본 조사로 연결됩니다.</font></b>
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
<td width='120' bgcolor='#eeeeee' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><b>조사기간</b></font></span></td>
<td width='20' bgcolor='#fafafa'>&nbsp;</td>
<td bgcolor='#fafafa' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'>2022년 11월 30일(수)까지</font></span></td>
</tr>
<tr><td height='1' colspan='4'></td></tr>
<tr>
<td width='25' bgcolor='#eeeeee'>&nbsp;</td>
<td width='120' bgcolor='#eeeeee' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><b>사례금</b></font></span></td>
<td width='20' bgcolor='#fafafa'>&nbsp;</td>
<td height='80' bgcolor='#fafafa' align='left'>
<p><font face='맑은고딕,맑은 고딕'><span style='font-size:12pt;'><b><font color='#C00000'>모바일 문화상품권 2만원</span></font><br>
<br><span style='font-size:10pt;'>모바일 문화상품권 사용처 : <a href='https://www.cultureland.co.kr/voucher/voucheruse.do?TabCode=mobile' target='_blank'>https://www.cultureland.co.kr/voucher/voucheruse.do?TabCode=mobile</a></b></font><br>
<p><span style='font-size:10pt;'><font face='맑은고딕,맑은 고딕'><b>*사례금은 조사 완료 후 12월 둘째주에 일괄 지급됩니다.</b></span></font></p>
</td>
</tr>
<tr><td height='1' colspan='4'></td></tr>
<tr>
<td width='25' height='50' bgcolor='#eeeeee'>&nbsp;</td>
<td width='120' bgcolor='#eeeeee' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><b>소요시간</b></font></span></td>
<td width='20' bgcolor='#fafafa'>&nbsp;</td>
<td bgcolor='#fafafa' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'>약 15분</font></span></td>
</tr><tr><td height='1' colspan='4'></td></tr>
<tr><td width='25' height='50' bgcolor='#eeeeee'>&nbsp;</td><td width='120' bgcolor='#eeeeee' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><b>공문보기</b></font></span></td>
<td width='20' bgcolor='#fafafa'>&nbsp;</td><td bgcolor='#fafafa' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><a href='https://ms.hrcglobal.com/etc/221268_document.pdf' target='_blank'>조사참여 협조 공문보기(클릭)</a></font></span></td>
</tr>
<tr><td height='1' colspan='4'></td></tr>
<tr><td width='25' height='50' bgcolor='#eeeeee'>&nbsp;</td>
<td width='120' bgcolor='#eeeeee' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><b>설문지</b></font></span></td>
<td width='20' bgcolor='#fafafa'>&nbsp;</td>
<td bgcolor='#fafafa' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><a href='https://ms.hrcglobal.com/etc/221268_qu.pdf' target='_blank'>설문지 보기(클릭)</a></font></span></td>
</tr>
<tr><td height='1' colspan='4'></td></tr>
<tr>
<td width='25' height='50' bgcolor='#eeeeee'>&nbsp;</td>
<td width='120' bgcolor='#eeeeee' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><b>응답가이드</b></font></span></td>
<td width='20' bgcolor='#fafafa'>&nbsp;</td>
<td bgcolor='#fafafa' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><a href='https://ms.hrcglobal.com/etc/221268_guide.pdf' target='_blank'>응답가이드 보기(클릭)</a></font></span></td>
</tr>
<tr><td height='1' colspan='4'></td></tr>
<tr><td width='25' height='70' bgcolor='#eeeeee'>&nbsp;</td>
<td width='120' bgcolor='#eeeeee' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><b>조사주관</b></font></span></td>
<td width='20' bgcolor='#fafafa'>&nbsp;</td>
<td bgcolor='#fafafa' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'>정보통신정책연구원 ICT데이터사이언스 연구본부 정부연 위원 (043-531-4112)</font></span></td>
</tr>
<tr><td height='1' colspan='4'></td></tr>
<tr><td width='25' height='70' bgcolor='#eeeeee'>&nbsp;</td>
<td width='120' bgcolor='#eeeeee' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><b>조사수행기관</b></font></span></td>
<td width='20' bgcolor='#fafafa'>&nbsp;</td>
<td bgcolor='#fafafa' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'>(주) 한국리서치 이태훈 부장<br>(02-3014-1074)</font></span></td>
</tr></tbody></table></td></tr>
<tr><td height='20'>&nbsp;</td></tr>
<tr><td><table width='100%' border='0' cellspacing='0' cellpadding='0'>
<tbody>
<tr><td width='24' height='25' valign='top'><img src='https://ms.hrcglobal.com/mailform/2021/images/info.png' width='16' height='16' alt=''></td>
<td valign='top' align='left'><span style='font-size:9pt; letter-spacing: -0.1em;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕' color='#414141'>응답내용에 따라 조사 대상이 아니시거나, 대상자가 초과되어 설문이 조기에 마감될 수 있습니다.</font></span></td>
</tr><tr><td width='24' height='20' valign='top'><img src='https://ms.hrcglobal.com/mailform/2021/images/info.png' width='16' height='16' alt=''/></td>
<td valign='top' align='left'><span style='font-size:9pt;letter-spacing: -0.1em;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕' color='#414141'>조사 참여 중 화면을 닫고 다시 접속 하실 경우 참여하셨던 이후의 문항부터 재 참여가 가능합니다.</font></span></td>
</tr></tbody></table></td></tr>
<tr><td height='35'>&nbsp;</td></tr><tr>
<td align='center'> <table border='0' cellspacing='0' cellpadding='0'><tr>
<td width='200' height='55' align='center' bgcolor='#D91616'>
<a href='https://survey.hrcglobal.com/kisdi/?gb=3&pid={1}' target='_blank' style='display: block; height:100%; line-height:60px; color:#fff;font-weight: bold; text-decoration: none;font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'>조사참여하기</font></a>
</td></tr></table> </td></tr><tr><td height='30'>&nbsp;</td></tr>
</tbody>
</table></td></tr><tr><td height='1' bgcolor='#eaeaea'></td></tr> 
<tr><td align='center' valign='top' bgcolor='#fafafa'><table width='540' border='0' cellspacing='0' cellpadding='0'>
<tbody><tr><td height='20'></td></tr>
<tr><td align='left' valign='bottom'><span style='font-size:9pt; color:#616161; letter-spacing: -0.1em; line-height:150%;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'>
- 웹설문에 대한 문의 사항은 한국리서치 이태훈 부장 02-3014-1074 로 연락주십시오. (월~금 09:30 ~ 17:30) <br>
- 본 메일은 발신전용으로 회신이 불가능합니다.
</font><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'></span></td></tr>
<tr><td height='20' align='left'></td></tr>
<tr><td align='left'><span style='font-size:9pt; color:#222;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><b>COPYRIGHT 2022 (C) HANKOOK RESERARCH ALL RIGHTS RESEVED.</b></font></span></td></tr><tr>
<td height='20' align='left'></td></tr></tbody></table></td></tr></tbody>
</table>	</td></tr>
</tbody>
</table>
</body>
</html>
", email, pid);
            try {

                using (SENDMASTERDataContext SENDMASTER = new SENDMASTERDataContext("Data Source=172.19.100.85,15000;Initial Catalog=SEND_MASTER;Persist Security Info=True;User ID=hrcdv;password=Hrc#3014!#"))
                {

                    SENDMASTER.PROC_SEND_EMAIL("interviwer", "apanel@hrc.co.kr", email, "[한국리서치]2022년 디지털 산업 통계조사", body, "", "", DateTime.Now, "", "", "2022-86-1268");
                    
                }
            }
            catch(Exception e)
            {
                return e.ToString();
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                TAPI.SP_q221268_SETLIST(pid,  "이메일발송");

            }
            return "성공";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sendType">0=저장만 , 1=문자, 2=이메일</param>
        /// <returns></returns>
        public string SaveInfo(int pid, string contact,  string name, string grade , string company,  string phone, string email1, string email2,  string etc, int sendType)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q221268_List list = TAPI.q221268_List.First(f => f.PID == pid);
                list.컨택현황 = contact;
                list.이름 = name;
                list.직위 = grade;
                list.사무실 = company;
                list.핸드폰 = phone;
                list.이메일주소 = email1 + "@" + email2;
                list.비고 = etc;

                TAPI.SubmitChanges();
            }

            if(sendType == 1)
            {
                SendMSG(pid, phone);
            }
            else if(sendType == 2)
            {
                string email = email1 + "@" + email2;
                SendEmail(pid, email);
            }

            return "";
        }
        
    }
}