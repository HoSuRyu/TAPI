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

//2022년 주거실태 조사 
namespace TAPI_Interviewer.Controllers
{
    public class q220754_PNFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.Controller.ViewBag.조사명 = "2022년 주거실태 조사";
        }
    }

    [q220754_PNFilter]
    public class q220754Controller : Controller
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

        public List<SP_H2022_GETLISTResult> GetList()
        {            
            List<SP_H2022_GETLISTResult> list = new List<SP_H2022_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_H2022_GETLIST(UID, null).ToList();
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 12803).FirstOrDefault();

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

        public List<SP_H2022_GETAPTLISTResult> GetListAPT()
        {
            List<SP_H2022_GETAPTLISTResult> list = new List<SP_H2022_GETAPTLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_H2022_GETAPTLIST(UID, null).ToList();
            }
            
            
            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 12803).FirstOrDefault();

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

        public List<SP_H2022_GETETCLISTResult> GetListETC()
        {
            List<SP_H2022_GETETCLISTResult> list = new List<SP_H2022_GETETCLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_H2022_GETETCLIST(UID, null).ToList();
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber == UID && w.questionnaireID == 12803).FirstOrDefault();

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

        public ActionResult ListAPT()
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            return View("ListAPT", GetListAPT());
        }

        public ActionResult ListAPTPartial()
        {
            return PartialView("ListAPTPartial", GetListAPT());
        }

        public ActionResult ListETC()
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            return View("ListETC", GetListETC());
        }

        public ActionResult ListETCPartial()
        {
            return PartialView("ListETCPartial", GetListETC());
        }

        public ActionResult ProgressPartial()
        {
            ProgressCount progressCount = new ProgressCount();

            progressCount.All = "0";
            progressCount.Ing = "0";
            progressCount.End = "0";

            List<SP_H2022_GETLISTResult> list = new List<SP_H2022_GETLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_H2022_GETLIST(UID, null).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "완료").Count().ToString();
            }

            return PartialView("ProgressPartial", progressCount);
        }

        public ActionResult ProgressAPTPartial()
        {
            ProgressCount progressCount = new ProgressCount();

            progressCount.All = "0";
            progressCount.Ing = "0";
            progressCount.End = "0";

            List<SP_H2022_GETAPTLISTResult> list = new List<SP_H2022_GETAPTLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_H2022_GETAPTLIST(UID, null).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "완료").Count().ToString();
            }

            return PartialView("ProgressPartial", progressCount);
        }

        public ActionResult ProgressETCPartial()
        {
            ProgressCount progressCount = new ProgressCount();

            progressCount.All = "0";
            progressCount.Ing = "0";
            progressCount.End = "0";

            List<SP_H2022_GETETCLISTResult> list = new List<SP_H2022_GETETCLISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_H2022_GETETCLIST(UID, null).ToList();

                progressCount.All = list.Count.ToString();
                progressCount.Ing = list.Where(w => w.진행상태 != null && w.진행상태 == "진행중").Count().ToString();
                progressCount.End = list.Where(w => w.진행상태 != null && w.진행상태 == "완료").Count().ToString();
            }

            return PartialView("ProgressPartial", progressCount);
        }

        public List<SP_H2022_GETHOUSELISTResult> GetHouseList(int keyid)
        {
            List<SP_H2022_GETHOUSELISTResult> list = new List<SP_H2022_GETHOUSELISTResult>();
            
            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_H2022_GETHOUSELIST(UID, keyid, null).ToList();
                
                List<string> statusList = list.GroupBy(g => g.진행상태).Where(w => string.IsNullOrEmpty(w.Key) == false).Select(s => s.Key).ToList();

                statusList.Insert(0, "");

                ViewBag.StatusList = statusList;
                
                HOUSE2022_AreaList AreaList = TAPI.HOUSE2022_AreaList.First(f => f.HRC일련번호 == keyid);

                ViewBag.HRC일련번호 = AreaList.HRC일련번호;
                ViewBag.조사구_번호 = AreaList.조사구번호;
                ViewBag.주소 = AreaList.전체주소;
               
            }

            return list;
        }

        public List<SP_H2022_GETAPTHOUSELISTResult> GetAPTHouseList(int keyid)
        {
            List<SP_H2022_GETAPTHOUSELISTResult> list = new List<SP_H2022_GETAPTHOUSELISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_H2022_GETAPTHOUSELIST(UID, keyid, null).ToList();

                List<string> statusList = list.GroupBy(g => g.진행상태).Where(w => string.IsNullOrEmpty(w.Key) == false).Select(s => s.Key).ToList();

                statusList.Insert(0, "");

                ViewBag.StatusList = statusList;

                
                HOUSE2022_APTList APTList = TAPI.HOUSE2022_APTList.First(f => f.HRC일련번호 == keyid);

                ViewBag.HRC일련번호 = APTList.HRC일련번호;                    
                ViewBag.주소 = APTList.주소;
                ViewBag.단지명 = APTList.단지명;
                ViewBag.동 = APTList.동명칭;
                ViewBag.가구수 = 0;
                if (APTList.가구수 != null)
                {
                    ViewBag.가구수 = APTList.가구수;
                }

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

        public ActionResult HouseListAPT(int keyid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            return View("HouseListAPT", GetAPTHouseList(keyid));
        }

        public ActionResult HouseListAPTPartial(int keyid)
        {
            return PartialView("HouseListAPTPartial", GetAPTHouseList(keyid));
        }

        public List<SP_H2022_GETETCHOUSELISTResult> GetHouseListEtc(int keyid)
        {
            List<SP_H2022_GETETCHOUSELISTResult> list = new List<SP_H2022_GETETCHOUSELISTResult>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.SP_H2022_GETETCHOUSELIST(UID, keyid, null).ToList();

                List<string> statusList = list.GroupBy(g => g.진행상태).Where(w => string.IsNullOrEmpty(w.Key) == false).Select(s => s.Key).ToList();

                statusList.Insert(0, "");

                ViewBag.StatusList = statusList;
                ViewBag.HRC일련번호 = keyid;

                HOUSE2022_EtcList EtcList = TAPI.HOUSE2022_EtcList.First(f => f.HRC일련번호 == keyid);
                ViewBag.표본구분 = EtcList.표본구분;
            }

            return list;
        }

        public ActionResult HouseListEtc(int keyid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            return View("HouseListEtc", GetHouseListEtc(keyid));
        }

        public ActionResult HouseListEtcPartial(int keyid)
        {
            return PartialView("HouseListEtcPartial", GetHouseListEtc(keyid));
        }

        
        public string CreateHouse(int keyid, string dong, int houseCnt)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                
                int hc = TAPI.HOUSE2022_APTHouseList.Count(c => c.HRC일련번호 == keyid);

                
                HOUSE2022_APTList APTList = TAPI.HOUSE2022_APTList.First(f => f.HRC일련번호 == keyid);
                APTList.동명칭 = dong;
                APTList.가구수 = houseCnt;
                TAPI.SubmitChanges();

                for (int i=hc+1; i<= houseCnt; i++)
                {
                    HOUSE2022_APTHouseList APTHouseList = new HOUSE2022_APTHouseList();
                    APTHouseList.HRC일련번호 = keyid;
                    APTHouseList.HID = keyid * 1000 + i;
                    APTHouseList.동 = dong;
                    APTHouseList.주소 = APTList.주소;
                    APTHouseList.단지명 = APTList.단지명;


                    TAPI.HOUSE2022_APTHouseList.InsertOnSubmit(APTHouseList);
                    TAPI.SubmitChanges();

                    TAPI.SP_H2022_SETHOUSE(keyid * 1000 + i, "", 2);
                }
            }

            return result;
        }

        public ActionResult AnswerInfo(int keyid, int hid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            SP_H2022_GETHOUSELISTResult GETHOUSELISTResult = null;

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                GETHOUSELISTResult = TAPI.SP_H2022_GETHOUSELIST(UID, keyid, hid).First();
            }

            return View("AnswerInfo", GETHOUSELISTResult);           
        }


        public ActionResult AnswerInfoAPT(int keyid, int hid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }
            SP_H2022_GETAPTHOUSELISTResult GETAPTHOUSELISTResult = null;

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                GETAPTHOUSELISTResult = TAPI.SP_H2022_GETAPTHOUSELIST(UID, keyid, hid).First();
            }
            
            return View("AnswerInfoAPT", GETAPTHOUSELISTResult);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyid"></param>
        /// <param name="pid">작년 대비 자료형 변경 </param>
        /// <returns></returns>
        public ActionResult AnswerInfoEtc(int keyid, string hid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            SP_H2022_GETETCHOUSELISTResult GETETCHOUSELISTResult = null;

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                GETETCHOUSELISTResult = TAPI.SP_H2022_GETETCHOUSELIST(UID, keyid, hid).First();
            }

            return View("AnswerInfoEtc", GETETCHOUSELISTResult);
        }

        public string CheckPhone(string hid, string hp)
        {
            
            // 핫라인 번호 중복 검사 하지 않음.
            if (hp == "010-000-0000" || hp == "010-0000-0000")
                return "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                int phoneC1 = TAPI.HOUSE2022_HouseList.Count(f => f.휴대전화 == hp && f.HID != int.Parse(hid));
                int phoneC2 = TAPI.HOUSE2022_APTHouseList.Count(f => f.휴대전화 == hp && f.HID != int.Parse(hid));
                int phoneC3 = TAPI.HOUSE2022_EtcHouseList.Count(f => f.휴대전화 == hp && f.HID != hid);
                if (phoneC1 > 0)
                {
                    HOUSE2022_HouseList hList = TAPI.HOUSE2022_HouseList.First(f => f.휴대전화 == hp && f.HID != int.Parse(hid));
                    return "가구일련번호 " + hList.HID + "에서 이미 입력된 휴대전화번호 입니다.\r\n휴대전화번호가 맞는지 다시 확인해 주세요.\r\n맞다면 수퍼바이저에게 보고해 주세요.";
                }
                else if (phoneC2 > 0)
                {
                    HOUSE2022_APTHouseList hList = TAPI.HOUSE2022_APTHouseList.First(f => f.휴대전화 == hp && f.HID != int.Parse(hid));
                    return "가구일련번호 " + hList.HID + "에서 이미 입력된 휴대전화번호 입니다.\r\n휴대전화번호가 맞는지 다시 확인해 주세요.\r\n맞다면 수퍼바이저에게 보고해 주세요.";
                }
                else if (phoneC3 > 0)
                {
                    HOUSE2022_EtcHouseList hList = TAPI.HOUSE2022_EtcHouseList.First(f => f.휴대전화 == hp && f.HID != hid);
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
                int phoneC1 = TAPI.HOUSE2022_HouseList.Count(f => f.집전화 == tel && f.HID != int.Parse(hid));
                int phoneC2 = TAPI.HOUSE2022_APTHouseList.Count(f => f.집전화 == tel && f.HID != int.Parse(hid));
                int phoneC3 = TAPI.HOUSE2022_EtcHouseList.Count(f => f.집전화 == tel && f.HID != hid);
                if (phoneC1 > 0)
                {
                    HOUSE2022_HouseList hList = TAPI.HOUSE2022_HouseList.First(f => f.집전화 == tel && f.HID != int.Parse(hid));
                    return "가구일련번호 " + hList.HID + "에서 이미 입력된 전화번호 입니다.\r\n전화번호가 맞는지 다시 확인해 주세요.\r\n맞다면 수퍼바이저에게 보고해 주세요.";
                }
                else if (phoneC2 > 0)
                {
                    HOUSE2022_APTHouseList hList = TAPI.HOUSE2022_APTHouseList.First(f => f.집전화 == tel && f.HID != int.Parse(hid));
                    return "가구일련번호 " + hList.HID + "에서 이미 입력된 전화번호 입니다.\r\n전화번호가 맞는지 다시 확인해 주세요.\r\n맞다면 수퍼바이저에게 보고해 주세요.";
                }
                else if (phoneC3 > 0)
                {
                    HOUSE2022_EtcHouseList hList = TAPI.HOUSE2022_EtcHouseList.First(f => f.집전화 == tel && f.HID != hid);
                    return "가구일련번호 " + hList.HID + "에서 이미 입력된 전화번호 입니다.\r\n전화번호가 맞는지 다시 확인해 주세요.\r\n맞다면 수퍼바이저에게 보고해 주세요.";
                }
            }
            return "";
        }
        public string SaveInfo(int keyid, int hid, string building, string pname, string gender, string tel, string hp)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                if (hp != "" && hp != "없음")
                {
                    result = CheckPhone(hid.ToString(), hp);
                    if( result != "")
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

                HOUSE2022_HouseList HouseList = TAPI.HOUSE2022_HouseList.First(f => f.HID == hid);
                HouseList.세부주소건물명 = building;

                HouseList.응답자이름 = pname;
                HouseList.응답자성별 = gender;
                HouseList.집전화 = tel;
               
                HouseList.휴대전화 = hp;
                

                string QMListData = "";
                QMListData = $@"<RPS_SamplingLIST_INFO>
<mainPurpsCdNms VALUE=""{HouseList.mainPurpsCdNm}"" />
<useAprDay VALUE=""{HouseList.useAprDay}"" />
<area VALUE=""{HouseList.area}"" />
<sidoCode VALUE=""{HouseList.시도코드}"" />
<sido VALUE=""{HouseList.시도명}"" />
<listType VALUE=""1"" />
<조사전용면적 VALUE="""" />
<공공임대여부 VALUE=""{HouseList.공공임대여부}"" />
<도로명주소 VALUE=""{HouseList.도로명주소.Replace('&', '＆').Replace('\'', '’')}"" />
<지번주소 VALUE=""{HouseList.지번주소.Replace('&', '＆').Replace('\'', '’')}"" />
</RPS_SamplingLIST_INFO>";

                string query = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q220754]') AND type in (N'U'))
BEGIN
    MERGE SURVEY.dbo.T_SamplingList_q220754 as SLIST
    USING
    (
        SELECT	'{0}' as PID
    ) as Answer on SLIST.PID = Answer.PID
    WHEN MATCHED THEN
	    UPDATE	SET	QMListData = N'{1}'
    WHEN NOT MATCHED THEN
	    INSERT (PID, QMListData)
	    VALUES (PID, N'{1}');
END", hid, QMListData);

                if (TAPI.ExecuteCommand(query, "") < 1)
                {
                    result = "정보 저장 오류 다시 시도 해 주세요.";
                }

                TAPI.SubmitChanges();

                TAPI.SP_H2022_SETHOUSE(hid, "", 1);
            }

            return result;
        }

        public string InitInfo(int hid)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                HOUSE2022_HouseList HouseList = TAPI.HOUSE2022_HouseList.First(f => f.HID == hid);

                HouseList.세부주소건물명 = "";

                HouseList.응답자이름 = "";
                HouseList.응답자성별 = "";
                HouseList.집전화 = "";
                HouseList.휴대전화 = "";

                
                TAPI.SubmitChanges();

                TAPI.SP_H2022_SETHOUSE(hid, "", 1);
            }

            return result;
        }

        public string SaveInfoAPT(int keyid, int hid, string pname, string gender, string tel, string hp, string dong, string hosu)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                if (hp != "" && hp != "없음")
                {
                    result = CheckPhone(hid.ToString(), hp);
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
                HOUSE2022_APTHouseList APTHouseList = TAPI.HOUSE2022_APTHouseList.First(f => f.HID == hid);

                APTHouseList.동 = dong;
                APTHouseList.호수 = hosu;

                APTHouseList.응답자이름 = pname;
                APTHouseList.응답자성별 = gender;
                APTHouseList.집전화 = tel;
                APTHouseList.휴대전화 = hp;

                HOUSE2022_APTList APTList = TAPI.HOUSE2022_APTList.First(f => f.HRC일련번호 == keyid);
                string address = APTList.주소 + " " + APTList.단지명 + " " + dong + " " + hosu;
                address = address.Replace('\'', '’').Replace('&', '＆');
                string QMListData = "";

                QMListData = $@"<RPS_SamplingLIST_INFO>
<mainPurpsCdNms VALUE="""" />
<useAprDay VALUE="""" />
<area VALUE="""" />
<sidoCode VALUE=""{APTList.시도코드}"" />
<sido VALUE=""{APTList.시도}"" />
<listType VALUE=""2"" />
<조사전용면적 VALUE="""" />
<공공임대여부 VALUE=""{APTList.공공임대여부}"" />
<도로명주소 VALUE="""" />
<지번주소 VALUE=""{address}"" />
</RPS_SamplingLIST_INFO>";

//                < 주소1 VALUE = ""{ APTHouseList.도로명주소}
//                "" />
//< 주소2 VALUE = ""{ APTList.지번주소}
//                "" />

     string query = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q220754]') AND type in (N'U'))
BEGIN
    MERGE SURVEY.dbo.T_SamplingList_q220754 as SLIST
    USING
    (
        SELECT	'{0}' as PID
    ) as Answer on SLIST.PID = Answer.PID
    WHEN MATCHED THEN
	    UPDATE	SET	QMListData = N'{1}'
    WHEN NOT MATCHED THEN
	    INSERT (PID, QMListData)
	    VALUES (PID, N'{1}');
END", hid, QMListData);

                if (TAPI.ExecuteCommand(query, "") < 1)
                {
                    result = "정보 저장 오류 다시 시도 해 주세요.";
                }

                TAPI.SubmitChanges();
                TAPI.SP_H2022_SETHOUSE(hid, "", 2);
            }

            return result;
        }

        public string InitInfoAPT(int hid)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                HOUSE2022_APTHouseList APTHouseList = TAPI.HOUSE2022_APTHouseList.First(f => f.HID == hid);

                //APTHouseList.동 = "";
                APTHouseList.호수 = "";

                APTHouseList.응답자이름 = "";
                APTHouseList.응답자성별 = "";
                APTHouseList.집전화 = "";
                APTHouseList.휴대전화 = "";
                

                TAPI.SubmitChanges();
                TAPI.SP_H2022_SETHOUSE(hid, "", 2);
            }

            return result;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="keyid"></param>
        /// <param name="pid">자료형 변경</param>
        /// <param name="roadAddrPart1"></param>
        /// <param name="sebuAddr"></param>
        /// <param name="jibunAddr"></param>
        /// <param name="pname"></param>
        /// <param name="gender"></param>
        /// <param name="tel"></param>
        /// <param name="hp"></param>
        /// <param name="IsNewAddress"></param>
        /// <param name="contactCount"></param>
        /// <param name="contact1"></param>
        /// <param name="contact2"></param>
        /// <param name="contact1Etc"></param>
        /// <param name="contact2Etc"></param>
        /// <returns></returns>
        public string SaveInfoEtc(int keyid, string hid, string building, string hosu, string pname, string gender, string tel, string hp, int contactCount, string contact1, string contact2, string contact1Etc, string contact2Etc)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                if (hp != "" && hp != "없음")
                {
                    result = CheckPhone(hid, hp);
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

                HOUSE2022_EtcHouseList EtcHouseList = TAPI.HOUSE2022_EtcHouseList.First(f => f.HID == hid);

                EtcHouseList.건물명 = building;
                EtcHouseList.호수 = hosu;

                EtcHouseList.응답자이름 = pname;
                EtcHouseList.응답자성별 = gender;
                EtcHouseList.집전화 = tel;
                EtcHouseList.휴대전화 = hp;

                EtcHouseList.총방문횟수 = contactCount;
                EtcHouseList.접촉결과 = contact1;
                EtcHouseList.접촉결과기타 = contact1Etc;
                EtcHouseList.주택유형 = contact2;
                EtcHouseList.주택유형기타 = contact2Etc;

                if(contact1 != "완료" && contact1 != "" && contact2 != "")
                {
                    EtcHouseList.진행상태 = "비성공(접촉일지 완료)";
                }

                string QMListData = "";

                HOUSE2022_EtcList EtcList = TAPI.HOUSE2022_EtcList.First(f => f.HRC일련번호 == keyid);

                QMListData = $@"<RPS_SamplingLIST_INFO>
<mainPurpsCdNms VALUE="""" />
<useAprDay VALUE="""" />
<area VALUE="""" />
<sidoCode VALUE=""{EtcList.시도코드}"" />
<sido VALUE=""{EtcList.시도}"" />
<listType VALUE=""3"" />
<조사전용면적 VALUE="""" />
<도로명주소 VALUE=""{EtcHouseList.도로명주소.Replace('&', '＆').Replace('\'', '’')}"" />
<지번주소 VALUE=""{EtcHouseList.지번주소.Replace('&', '＆').Replace('\'', '’')}"" />
</RPS_SamplingLIST_INFO>";


string query = string.Format(@"IF EXISTS (SELECT * FROM SURVEY.sys.objects WHERE object_id = OBJECT_ID(N'[SURVEY].[dbo].[T_SamplingList_q220754]') AND type in (N'U'))
BEGIN
    MERGE SURVEY.dbo.T_SamplingList_q220754 as SLIST
    USING
    (
        SELECT	'{0}' as PID
    ) as Answer on SLIST.PID = Answer.PID
    WHEN MATCHED THEN
	    UPDATE	SET	QMListData = N'{1}'
    WHEN NOT MATCHED THEN
	    INSERT (PID, QMListData)
	    VALUES (PID, N'{1}');
END", hid, QMListData);

                if (TAPI.ExecuteCommand(query, "") < 1)
                {
                    result = "정보 저장 오류 다시 시도 해 주세요.";
                }
                TAPI.SubmitChanges();
            }
            
            return result;
        }

        public string InitInfoEtc(string hid)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            string result = "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                HOUSE2022_EtcHouseList EtcHouseList = TAPI.HOUSE2022_EtcHouseList.First(f => f.HID == hid);

                EtcHouseList.건물명 = "";
                EtcHouseList.호수 = "";

                EtcHouseList.응답자이름 = "";
                EtcHouseList.응답자성별 = "";
                EtcHouseList.집전화 = "";
                EtcHouseList.휴대전화 = "";

                EtcHouseList.총방문횟수 = 0;
                EtcHouseList.접촉결과 = "";
                EtcHouseList.접촉결과기타 = "";
                EtcHouseList.주택유형 = "";
                EtcHouseList.주택유형기타 = "";
                
                TAPI.SubmitChanges();
            }

            return result;
        }

        public ActionResult Survey(int keyid, int hid, int listType)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                List<AnswerList> answerList = new List<AnswerList>();
                
                bool eqnum = false;
                int? qID = 12803;
                int answerStateCode = 0;
                using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q220754 WHERE PID = '{0}'", hid)).FirstOrDefault();
                    answerStateCode = t_SamplingList != null ? (int)(t_SamplingList.answerStateCode != null ? t_SamplingList.answerStateCode : 0) : 0;
                }

                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
               
                    if(listType == 1)
                    {
                        ViewBag.exitLink = "AnswerInfo";
                        SP_H2022_GETHOUSELISTResult GETHOUSELISTResult = GetHouseList(keyid).First(f => f.HID == hid);

                        if (GETHOUSELISTResult.진행상태 == "완료" || answerStateCode == 4)
                        {
                            eqnum = true;
                        }
                    }
                    else
                    {
                        ViewBag.exitLink = "AnswerInfoAPT";
                        SP_H2022_GETAPTHOUSELISTResult GETAPTHOUSELISTResult = GetAPTHouseList(keyid).First(f => f.HID == hid);
                        if (GETAPTHOUSELISTResult.진행상태 == "완료" || answerStateCode == 4)
                        {
                            eqnum = true;
                        }
                    }
                    ViewBag.hid = hid;
                    TAPI.SP_H2022_SETHOUSE(hid, "진행중", listType);

                }

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q220754&pid={hid}&keyid={keyid}&t=TAPI");
                }
                else
                {
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + qID.ToString()).Replace("/q220754", ""), hid);
                    
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
                                              Url = TripleDESCryptoService.Encrypt($"qn=q220754&pid={hid}&keyid={keyid}&t=TAPI&eqnum={element.Name.ToString()}"),
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

        /// <summary>
        /// ETC 용으로 오버라이팅 
        /// </summary>
        /// <param name="keyid"></param>
        /// <param name="pid"></param>
        /// <param name="listType"></param>
        /// <returns></returns>
        public ActionResult SurveyETC(int keyid, string hid, int listType)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                List<AnswerList> answerList = new List<AnswerList>();

                bool eqnum = false;
                int? qID = 12803;

                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    if (listType == 3)
                    {
                        SP_H2022_GETETCHOUSELISTResult GETETCHOUSELISTResult = GetHouseListEtc(keyid).First(f => f.HID == hid);

                        if (GETETCHOUSELISTResult.진행상태 == "완료")
                        {
                            eqnum = true;
                        }
                    }
                    
                }

                if (eqnum == false)
                {
                    ViewBag.C = TripleDESCryptoService.Encrypt($"qn=q220754&pid={hid}&keyid={keyid}&t=TAPI");
                }
                else
                {
                    
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + qID.ToString()).Replace("/q220754", ""), hid);

                    
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
                                              Url = TripleDESCryptoService.Encrypt($"qn=q220754&pid={hid}&keyid={keyid}&t=TAPI&eqnum={element.Name.ToString()}"),
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



            List<HOUSE2022_Contact> Contact = new List<HOUSE2022_Contact>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                HOUSE2022_HouseList HouseList = TAPI.HOUSE2022_HouseList.FirstOrDefault(f => f.HID == hid);



                Contact = (from contact in TAPI.HOUSE2022_Contact
                                     where contact.HID == hid
                                       select contact).ToList();

                ViewBag.HRC일련번호 = HouseList.HRC일련번호;
                ViewBag.가구일련번호 = HouseList.HID;
                ViewBag.가구번호 = HouseList.가구번호;
                ViewBag.거처번호 = HouseList.거처번호;
                ViewBag.주소 = HouseList.주소;
                ViewBag.세부주소 = HouseList.건물명;


                ViewBag.Ranking = (Contact.Count + 1).ToString();


                ViewBag.contactYear = DateTime.Now.ToString("yyyy");
                ViewBag.contactMonth = DateTime.Now.ToString("MM");
                ViewBag.contactDay = DateTime.Now.ToString("dd");
            }

            return View("Contact", Contact);
        }

        public ActionResult ContactAPT(int hid)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            List<HOUSE2022_Contact> Contact = new List<HOUSE2022_Contact>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                HOUSE2022_APTHouseList APTHouseList = TAPI.HOUSE2022_APTHouseList.FirstOrDefault(f => f.HID == hid);

                HOUSE2022_APTList APTList = TAPI.HOUSE2022_APTList.FirstOrDefault(f => f.HRC일련번호 == APTHouseList.HRC일련번호);

                Contact = (from contact in TAPI.HOUSE2022_Contact
                                     where contact.HID== hid
                                     select contact).ToList();

                ViewBag.HRC일련번호 = APTHouseList.HRC일련번호;
                ViewBag.가구일련번호 = APTHouseList.HID;
                ViewBag.동 = APTHouseList.동;
                ViewBag.호수 = APTHouseList.호수;

                ViewBag.주소 = APTList.주소;
                ViewBag.단지명 = APTList.단지명;
                
                ViewBag.Ranking = (Contact.Count + 1).ToString();


                ViewBag.contactYear = DateTime.Now.ToString("yyyy");
                ViewBag.contactMonth = DateTime.Now.ToString("MM");
                ViewBag.contactDay = DateTime.Now.ToString("dd");
            }

            return View("ContactAPT", Contact);
        }


        public string ContactOK(int hid, int ranking, string contact, string contactMonth, string contactDay, string contactTime)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            HOUSE2022_Contact Contact = new HOUSE2022_Contact();
            List<HOUSE2022_Contact> ContactList = new List<HOUSE2022_Contact>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {

                string contactDate = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;
                ContactList = (from contactT in TAPI.HOUSE2022_Contact
                                         where contactT.HID == hid & contactT.방문일자 == contactDate & contactT.방문시간 == contactTime
                                       select contactT).ToList();

                if (ContactList.Count > 0)
                {
                    return "같은 시간대 방문기록이 이미 있습니다.";
                }

                HOUSE2022_HouseList HouseList = TAPI.HOUSE2022_HouseList.FirstOrDefault(f => f.HID == hid);

                HouseList.접촉일지상태 = "기입";



                Contact.HID = hid;
                Contact.면접원아이디 = UID.ToString();
                Contact.방문횟수 = ranking;
                Contact.방문일자 = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;
                Contact.방문시간 = contactTime;
                Contact.비성공사유 = contact;
                Contact.조사구_신축 = 1;

                Contact.inputDate = DateTime.Now;

                TAPI.HOUSE2022_Contact.InsertOnSubmit(Contact);


                TAPI.SubmitChanges();

                TAPI.SP_H2022_SETHOUSE(hid, "", 1);
            }

            return "저장";
        }

        public string ContactAPTOK(int hid, int ranking, string contact, string contactMonth, string contactDay, string contactTime)
        {
            if (UID <= 0)
            {
                return "에러. 재접속 해 주세요.";
            }

            HOUSE2022_Contact Contact = new HOUSE2022_Contact();
            List<HOUSE2022_Contact> ContactList = new List<HOUSE2022_Contact>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {

                string contactDate = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;
                ContactList = (from contactT in TAPI.HOUSE2022_Contact
                                         where contactT.HID == hid & contactT.방문일자 == contactDate & contactT.방문시간 == contactTime
                                         select contactT).ToList();

                if (ContactList.Count > 0)
                {
                    return "같은 시간대 방문기록이 이미 있습니다.";
                }

                HOUSE2022_APTHouseList APTHouseList= TAPI.HOUSE2022_APTHouseList.FirstOrDefault(f => f.HID == hid);
                APTHouseList.접촉일지상태 = "기입";

                Contact.HID = hid;
                Contact.면접원아이디 = UID.ToString();
                Contact.방문횟수 = ranking;
                Contact.방문일자 = $"{DateTime.Now.Year}-" + contactMonth + "-" + contactDay;
                Contact.방문시간 = contactTime;
                Contact.비성공사유 = contact;
                Contact.조사구_신축 = 2;

                Contact.inputDate = DateTime.Now;

                TAPI.HOUSE2022_Contact.InsertOnSubmit(Contact);
                TAPI.SubmitChanges();

                TAPI.SP_H2022_SETHOUSE(hid, "", 2);
            }

            return "저장";
        }

        public ActionResult Replace(int keyid, int listType)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {

                if (listType == 1)
                {
                    HOUSE2022_AreaList AreaList = TAPI.HOUSE2022_AreaList.First(f => f.HRC일련번호 == keyid);

                    return View("Replace", AreaList);
                }
                else if (listType == 2)
                {
                    HOUSE2022_APTList APTList = TAPI.HOUSE2022_APTList.First(f => f.HRC일련번호 == keyid);

                    return View("ReplaceAPT", APTList);
                }
                else
                {
                    HOUSE2022_EtcList EtcList = TAPI.HOUSE2022_EtcList.First(f => f.HRC일련번호 == keyid);

                    ViewBag.대체가능 = (TAPI.HOUSE2022_EtcHouseList.Where(w => w.HRC일련번호 == keyid && (w.접촉결과 ?? "") == "").Count() == 0).ToString().ToLower();

                    return View("ReplaceEtc", EtcList);
                }
            }
        }

        public ActionResult ReplaceOK(int keyid, string status, string etc, int listType)
        {
            if (UID <= 0)
            {
                return RedirectToAction("Login", "Home");
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                if (listType == 1)
                {
                    HOUSE2022_AreaList AreaList = TAPI.HOUSE2022_AreaList.First(f => f.HRC일련번호 == keyid);

                    AreaList.대체요청사유 = status;
                    AreaList.대체요청세부사유 = etc;
                    AreaList.대체요청일자 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    AreaList.진행상태 = "대체요청";
                }
                else if (listType == 2)
                {
                    HOUSE2022_APTList APTList = TAPI.HOUSE2022_APTList.First(f => f.HRC일련번호 == keyid);

                    APTList.대체요청사유 = status;
                    APTList.대체요청세부사유 = etc;
                    APTList.대체요청일자 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    APTList.진행상태 = "대체요청";
                }
                else
                {
                    HOUSE2022_EtcList EtcList = TAPI.HOUSE2022_EtcList.First(f => f.HRC일련번호 == keyid);

                    EtcList.대체요청사유 = status;
                    EtcList.대체요청세부사유 = etc;
                    EtcList.대체요청일자 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    
                    EtcList.진행상태 = "대체요청";
                }

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