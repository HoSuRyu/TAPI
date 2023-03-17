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
    public class q220755Controller : Controller
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

        public List<SP_q220755_GETLISTResult> GetList()
        {
            List<SP_q220755_GETLISTResult> list = new List<SP_q220755_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220755_GETLIST(UID, null).ToList();
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 13275).FirstOrDefault();

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

            List<SP_q220755_GETLISTResult> list = new List<SP_q220755_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220755_GETLIST(UID, null).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "완료").Count().ToString();
            }

            return PartialView("ProgressPartial", progressCount);
        }
        
        public List<SP_q220755_GETHOUSELISTResult> GetHouseList(int keyid)
        {
            List<SP_q220755_GETHOUSELISTResult> list = new List<SP_q220755_GETHOUSELISTResult>();
            
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_q220755_GETHOUSELIST(UID, keyid, null).ToList();
                
                List<string> statusList = list.GroupBy(g => g.진행상태).Where(w => string.IsNullOrEmpty(w.Key) == false).Select(s => s.Key).ToList();

                statusList.Insert(0, "");

                ViewBag.StatusList = statusList;
                ViewBag.일련번호 = keyid;
            
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
            
            q220755_HouseList   q220755_HouseList = null; ;
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220755_HouseList = TAPI.q220755_HouseList.First(f => f.HID == hid);
            }
            ViewBag.Error = "";
            return View("AnswerInfo", q220755_HouseList);           
        }

        
        public string CheckPhone(int hid, string hp)
        {

            // 핫라인 번호 중복 검사 하지 않음.
            if (hp == "010-000-0000" || hp == "010-0000-0000")
                return "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                int phoneC1 = TAPI.q220755_HouseList.Count(f => f.모바일 == hp && f.HID != hid && f.모바일수정=="인터뷰어");
                
                if (phoneC1 > 0)
                {
                    q220755_HouseList hList = TAPI.q220755_HouseList.First(f => f.모바일 == hp && f.HID !=  hid && f.모바일수정 == "인터뷰어");
                    return "전화번호가 조사구 번호 "+hList.일련번호 +", 가구일련번호 " + hList.HID + "과 중복됩니다.\r\n수퍼바이저에게 확인해 주세요.";
                }
                
            }
            return "";
        }

        public string CheckTel(int hid, string tel)
        {
            // 핫라인 번호 중복 검사 하지 않음.
            //if (tel == "02-000-0000" || tel == "02-0000-0000")
            //    return "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                int phoneC1 = TAPI.q220755_HouseList.Count(f => f.유선전화 == tel && f.HID != hid && f.유선전화수정 == "인터뷰어");
                
                if (phoneC1 > 0)
                {
                    q220755_HouseList hList = TAPI.q220755_HouseList.First(f => f.유선전화 == tel && f.HID !=hid && f.유선전화수정 == "인터뷰어");
                    return "전화번호가 조사구 번호 " + hList.일련번호 + ", 가구일련번호 " + hList.HID + "과 중복됩니다.\r\n수퍼바이저에게 확인해 주세요.";
                }
               
            }
            return "";
        }

        public string SaveHouseInfo(int hid, string name, string gender, string ftype, string addr2, string phone, string email1, string email2, string tel, int times)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220755_HouseList HouseList = TAPI.q220755_HouseList.FirstOrDefault(f => f.HID == hid);

                HouseList.응답자이름 = name;
                HouseList.응답자성별 = gender;
                HouseList.가구주여부 = ftype;
                HouseList.입력건물명 = addr2;
                HouseList.총방문횟수 = times;

                if (phone != "")
                {
                    if (HouseList.모바일 == null || HouseList.모바일 != phone)
                    {
                        string result = CheckPhone(hid, phone);
                        if (result  != "")
                        {
                            return result;
                        }
                        HouseList.모바일수정 = "인터뷰어";
                    }
                    HouseList.모바일 = phone;

                }
                if (email1 != "" && email2 != "")
                {
                    string email = email1 + "@" + email2;
                    HouseList.이메일 = email;
                    if (HouseList.이메일 == null || HouseList.이메일 != email)
                    {   
                        HouseList.이메일수정 = "인터뷰어";
                    }
                }
                if( tel != "")
                {
                    if (HouseList.유선전화 == null || HouseList.유선전화 != tel)
                    {
                        string result = CheckTel(hid, tel);
                        if (result != "")
                        {
                            return result;
                        }
                        HouseList.유선전화수정 = "인터뷰어";
                    }
                    HouseList.유선전화 = tel;
                }

                TAPI.SubmitChanges();
            }

            return "저장";
        }


        public string DirectSendMSG(int hid, string phone)
        {

            using (SENDMASTERDataContext SENDMASTER = new SENDMASTERDataContext("Data Source=172.19.100.85,15000;Initial Catalog=SEND_MASTER;Persist Security Info=True;User ID=hrcdv;password=Hrc#3014!#"))
            {

                
                string mm = string.Format(@"국토교통부와 ㈜한국리서치가 진행하는 주거실태조사 인터넷조사 안내

안녕하세요.
본 조사는 주거기본법 20조에 의거하여 국토교통부에서 시행하는 '주거실태조사'입니다.
국민의 대표로서 주거실태조사에 참여해 주세요.

본 문자는 '주거실태조사' 조사 대상으로 선정된 가구의 '가구주/배우자'께 발송하는 문자입니다.
링크를 받으신 가구만 진행하셔야 하고, 가구주 또는 가구주의 배우자 한 분만 참여해 주시면 됩니다.

아래 링크를 클릭하셔서 설문을 진행해주세요.

■ 주거실태조사 인터넷조사 안내
- 조사대상: 조사 대상으로 선정된 가구의 가구주 또는 배우자
- 조사기간: 2022년 11월 28일 ~2023년 1월 20일
- 참여혜택
① 설문에 모두 응답하신 응답자께 모바일상품권 5천원권 증정
    *입력 휴대폰 번호로 조사 완료후 10일이내 발송
② 설문을 마치신 응답자 중 추첨을 통해 백화점 상품권 10만원 증정(30명)    
    *추첨후 입력 휴대폰 번호로 개별 발송
    *당첨자 추첨시기: 2023년 2월 3일까지 추첨 및 개별통보

▶참여하기: https://rpssurvey.hrcglobal.com/?qn=q220755&PID={0}&uid={1}&atype=2

                ", hid,UID);



                SENDMASTER.PROC_SEND_MESSAGE("interviwer", "0230140702", phone, "주거실태조사 인터넷조사 참여 안내", mm, DateTime.Now, "", "2022-98-0755");



            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220755_HouseList houseList = TAPI.q220755_HouseList.FirstOrDefault(f => f.HID == hid);

                houseList.문자발송일 = DateTime.Now.ToString();

                int c = 0;

                if (houseList.모바일발송횟수 != null)
                {
                    c = (int)houseList.모바일발송횟수;
                }
                c++;
                houseList.모바일발송횟수 = c;
                houseList.접촉일지방문결과 = "링크 발송";
                houseList.접촉일지상태 = "기입";

                TAPI.SubmitChanges();
                TAPI.sp_q220755_SETAREA(houseList.일련번호);
            }
            return "성공";
        }

        public string SendMSG(int hid, string phone, string name, string gender, string ftype, string addr2, string email1, string email2, string tel, int times)
        {
            if (SaveHouseInfo(hid, name, gender, ftype, addr2, phone, email1, email2, tel, times) != "저장")
            {
                return "";
            }

            return DirectSendMSG(hid, phone);
        }
        public string DirectSendEmail(int hid, string email)
        {

            using (SENDMASTERDataContext SENDMASTER = new SENDMASTERDataContext("Data Source=172.19.100.85,15000;Initial Catalog=SEND_MASTER;Persist Security Info=True;User ID=hrcdv;password=Hrc#3014!#"))
            {

                string body = string.Format(@"<!doctype html>
<html>
<head>
<meta charset='etc-kr'>
<title>:::: 한국리서치 ::::</title>
</head>

<body>
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
                                <td height='131' align='center'>
                                    <p style='color: rgb(0, 0, 0); font-family: &quot;Malgun Gothic&quot;; font-size: medium; font-style: normal; font-variant-ligatures: normal; font-variant-caps: normal; font-weight: 400; letter-spacing: normal; orphans: 2; text-align: -webkit-center; text-indent: 0px; text-transform: none; white-space: normal; widows: 2; word-spacing: 0px; -webkit-text-stroke-width: 0px; background-color: rgb(217, 22, 22); text-decoration-thickness: initial; text-decoration-style: initial;
text-decoration-color: initial;'><span style='font-size: 20pt; color: rgb(255, 255, 255);'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕' color='#ffffff'><b>주거실태조사 인터넷조사 참여 안내</b></font></span><b></b></p>
                                </td>
                            </tr>
        </tbody>
      </table></td>
    </tr>
    <tr>
      <td height='40'>&nbsp;</td>
    </tr>
    <tr>
      <td align='center'><table width='540' border='0' cellspacing='0' cellpadding='0'>
        <tbody>
                            <tr>
                               <td align='left'><span style='font-size:11pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'>
<b>안녕하세요.<br>
본 조사는 주거기본법 20조에 의거하여 국토교통부에서 시행하는 '주거실태조사'입니다.<br>
국민의 대표로서 주거실태조사에 참여해 주세요.<br><br>

본 메일은 '주거실태조사' 조사 대상으로 선정된 가구의 '가구주/배우자'께 발송하는 메일입니다.<br>
링크를 받으신 가구만 진행하셔야 하고, 가구주 또는 가구주의 배우자 한 분만 참여해 주시면 됩니다.<br><br>

하단의 조사참여하기 버튼을 클릭하셔서 설문을 진행해주세요.</b></font></span></td>
          </tr>
          <tr>
            <td height='30'>&nbsp;</td>
          </tr>
          <tr>
            <td><table width='540' border='0' cellspacing='0' cellpadding='0'>
              <tbody>
              	 <tr>
                  <td width='25' height='50' bgcolor='#eeeeee'>&nbsp;</td>
                  <td width='100' bgcolor='#eeeeee' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><b>조사대상</b></font></span></td>
                  <td width='20' bgcolor='#fafafa'>&nbsp;</td>
                  <td bgcolor='#fafafa' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'>조사 대상으로 선정된 가구의 가구주 또는 배우자</font></span></td>
                </tr>
                <tr>
                  <td height='1' colspan='4'></td>
                  </tr>
                <tr>
                  <td width='25' height='50' bgcolor='#eeeeee'>&nbsp;</td>
                  <td width='100' bgcolor='#eeeeee' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><b>조사기간</b></font></span></td>
                  <td width='20' bgcolor='#fafafa'>&nbsp;</td>
                  <td bgcolor='#fafafa' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'>2022년 11월 28일 ~ 2023년 1월 20일</font></span></td>
                </tr>
                <tr>
                  <td height='1' colspan='4'></td>
                  </tr>
                                        <tr>
                  <td width='25' bgcolor='#eeeeee'>&nbsp;</td>
                  <td width='100' bgcolor='#eeeeee' align='left'><span style='font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><b>참여혜택</b></font></span></td>
                  <td width='20' bgcolor='#fafafa'>&nbsp;</td>
                                            <td height='80' bgcolor='#fafafa' align='left'>
                                                <p><font face='맑은고딕,맑은 고딕'><span style='font-size:10pt;'><b>
① 설문에 모두 응답하신 응답자께 모바일상품권 5천원권 증정<br>
* 입력 휴대폰 번호로 조사 완료후 10일이내 발송<br>
② 설문을 마치신 응답자 중 추첨을 통해 백화점 상품권 10만원 증정(30명)    <br>
* 추첨후 입력 휴대폰 번호로 개별 발송<br>
* 당첨자 추첨시기 : 2023년 2월 3일까지 추첨 및 개별통보<br></b></span></font></p>
                                            </td>
                                        </tr>
			
              </tbody>
            </table></td>
          </tr>
			<tr>
				<td height='20'>&nbsp;</td>
			</tr>
			<tr>
			  <td><table width='100%' border='0' cellspacing='0' cellpadding='0'>
			    <tbody>
			      <tr>
			        <td width='24' height='25' valign='top'><img src='http://ms.hrcglobal.com/mailform/2021/images/info.png' width='16' height='16' alt=''></td>
			        <td valign='top' align='left'><span style='font-size:9pt; letter-spacing: -0.1em;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕' color='#414141'>응답내용에 따라 조사 대상이 아니시거나, 대상자가 초과되어 설문이 조기에 마감될 수 있습니다.</font></span></td>
			        </tr>
					<tr>
			        <td width='24' height='20' valign='top'><img src='http://ms.hrcglobal.com/mailform/2021/images/info.png' width='16' height='16' alt=''/></td>
			        <td valign='top' align='left'><span style='font-size:9pt;letter-spacing: -0.1em;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕' color='#414141'>조사 참여 중 화면을 닫고 다시 접속 하실 경우 참여하셨던 이후의 문항부터 재 참여가 가능합니다.</font></span></td>
			        </tr>
			      </tbody>
		      </table></td>
		    </tr>
			<tr>
			  <td height='35'>&nbsp;</td>
		    </tr>
			<tr>
			  <td align='center'>
				  <!--조사참여버튼-->
			    <table border='0' cellspacing='0' cellpadding='0'>
					<tr>
						<td width='200' height='55' align='center' bgcolor='#D91616'>
			    <!--링크주소는 #위치에 넣어주세요--><a href='https://rpssurvey.hrcglobal.com/?qn=q220755&PID={0}&atype=2&uid={1}' target='_blank' style='display: block; height:100%; line-height:60px; color:#fff;font-weight: bold; text-decoration: none;font-size:12pt;'><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'>조사참여하기</font></a>
							</td>
				  </tr>
				</table>
				  <!--//조사참여버튼 끝-->
			  </td>
		    </tr>
			<tr>
			  <td height='30'>&nbsp;</td>
		    </tr>
        </tbody>
      </table></td>
    </tr>
	
    
	  <tr>
	  	<td height='1' bgcolor='#eaeaea'></td>
	  </tr> 
    <tr>
      <td align='center' valign='top' bgcolor='#fafafa'><table width='540' border='0' cellspacing='0' cellpadding='0'>
        <tbody>
			<tr>
				<td height='20'></td>
			</tr>
          <tr>
            <td align='left' valign='bottom'>
				<span style='font-size:9pt; color:#616161; letter-spacing: -0.1em; line-height:150%;'>
				<font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'>
				주거실태조사에 대한 문의 사항은 02-3014-0702 (송신자 부담) / <a href='mailto:korhs@hrc.co.kr'>korhs@hrc.co.kr</a> 으로 연락주십시오.<br>(월~금 09:00 ~ 19:00 공휴일제외) <br>
				</font><font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'></span></td>
          </tr>
          <tr>
            <td height='20' align='left'></td>
          </tr>
          <tr>
            <td align='left'>
				<span style='font-size:9pt; color:#222;'>
			  <font face='맑은고딕,맑은 고딕,나눔고딕,나눔바른고딕'><b>COPYRIGHT 2021 (C) HANKOOK RESERARCH ALL RIGHTS RESEVED.</b></font></span></td>
          </tr>
			<tr>
            <td height='20' align='left'></td>
          </tr>
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
</html>
", hid,UID);




                SENDMASTER.PROC_SEND_EMAIL("interviwer", "korhs@hrc.co.kr", email, "주거실태조사 인터넷조사 참여 안내", body, "", "", DateTime.Now, "", "", "2022-98-0755");

            }
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220755_HouseList houseList = TAPI.q220755_HouseList.FirstOrDefault(f => f.HID == hid);

                houseList.메일발송일 = DateTime.Now.ToString();

                int c = 0;

                if (houseList.이메일발송횟수 != null)
                {
                    c = (int)houseList.이메일발송횟수;
                }
                c++;
                houseList.이메일발송횟수 = c;
                houseList.접촉일지방문결과 = "링크 발송";
                houseList.접촉일지상태 = "기입";
                
                TAPI.SubmitChanges();
                TAPI.sp_q220755_SETAREA(houseList.일련번호);
            }
            return "성공";
        }

        public string SendEmail(int hid, string email1, string email2, string name, string gender, string ftype, string addr2, string phone, string tel, int times)
        {

            if (SaveHouseInfo(hid, name, gender, ftype, addr2, phone, email1, email2, tel, times) != "저장")
            {
                return "";
            }

            string email = email1 + "@" + email2;

            return DirectSendEmail(hid, email);
        }

        /// <summary>
        /// 아직 사용하지 않지만 만들어둠
        /// </summary>
        /// <param name="hid"></param>
        /// <returns></returns>
        public string InitInfo(int hid)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                q220755_HouseList HouseList = TAPI.q220755_HouseList.First(f => f.HID == hid);

                HouseList.입력건물명 = "";
                HouseList.가구주여부 = ""; ;
                HouseList.응답자이름 = ""; ;
                HouseList.응답자성별 = ""; ;
                HouseList.유선전화 = ""; ;
                HouseList.모바일 = ""; ;
                HouseList.이메일 = "";

                TAPI.SubmitChanges();

                TAPI.SP_q220755_SETHOUSE(hid, "초기화", null);
            }

            return result;
        }
        public ActionResult Survey(int hid)
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
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q220755 WHERE PID = '{0}'", hid)).FirstOrDefault();
                    answerStateCode = t_SamplingList != null ? (int)(t_SamplingList.answerStateCode != null ? t_SamplingList.answerStateCode : 0) : 0;
                }
                q220755_HouseList houseList = null;
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    
                    houseList = TAPI.q220755_HouseList.First(f => f.HID == hid);
                    if (houseList.진행상태 == "완료" || answerStateCode == 4 || answerStateCode == 1)
                    {
                        eqnum = true;
                    }
                    
                    ViewBag.hid = hid;
                    TAPI.SP_q220755_SETHOUSE(hid, "진행중", 1);
                }

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q220755&pid={hid}&keyid={houseList.일련번호}&uid={UID}&t=TAPI&atype=1");
                }
                else
                {
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + qID.ToString()).Replace("/q220755", ""), hid);
                    
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
                                              Url = TripleDESCryptoService.Encrypt($"qn=q220755&pid={hid}&keyid={houseList.일련번호}&atype=1&t=TAPI&eqnum={element.Name.ToString()}"),
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



            q220755_HouseList HouseList = null;
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                HouseList = TAPI.q220755_HouseList.FirstOrDefault(f => f.HID == hid);

                                
            }

            return View("Contact", HouseList);
        }
        


        public string ContactOK(int hid, int ranking, string contact)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

           
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                

                q220755_HouseList HouseList = TAPI.q220755_HouseList.FirstOrDefault(f => f.HID == hid);

                HouseList.접촉일지상태 = "기입";
                HouseList.접촉일지방문결과 = contact;
                HouseList.총방문횟수 = ranking;

                TAPI.SubmitChanges();
                TAPI.sp_q220755_SETAREA(HouseList.일련번호);
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

                q220755_AreaList AreaList = TAPI.q220755_AreaList.First(f => f.일련번호 == keyid);

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
                q220755_AreaList AreaList = TAPI.q220755_AreaList.First(f => f.일련번호 == keyid);
                
                AreaList.진행상태 = "대체요청";

                q220755_Replace replace = new q220755_Replace();
                replace.일련번호 = keyid;
                replace.면접원아이디 = UID;
                replace.상태 = "대체요청";
                replace.대체요청시간= DateTime.Now;
                replace.대체사유 = status;
                replace.세부사유 = etc;
                TAPI.q220755_Replace.InsertOnSubmit(replace);

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