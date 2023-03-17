using TAPI_Interviewer.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Xml.Linq;
using TAPI_Interviewer.Models;
using TAPI_Interviewer.Models.KLOWF;



namespace TAPI_Interviewer.Controllers
{
    public class q221411Controller : Controller
    {
        private string UID
        {
            get
            {
                string value = HttpContext.Session["HRC_TAPI_UID"] as string;


                return value;
            }
        }

        private string LastUID
        {
            get
            {
                string value = "";

                if (Request.Cookies["HRC_TAPI_INFO"] != null)
                {
                    value = Request.Cookies["HRC_TAPI_INFO"]["LastUID"];
                }

                return value;
            }
        }

        public List<KLOWF2022_Panel> GetList()
        {
            List<KLOWF2022_Panel> list = new List<KLOWF2022_Panel>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                list = TAPI.KLOWF2022_Panel.Where(w => w.uid == UID).ToList();
            }

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_QuestionnaireDistribution t_QuestionnaireDistribution = SURVEYTOOL.T_QuestionnaireDistribution.Where(w => w.employeeNumber.ToString() == UID && w.questionnaireID == 13204).FirstOrDefault();

                if (t_QuestionnaireDistribution != null)
                {
                    t_QuestionnaireDistribution.sendDateTime = DateTime.Now;

                    SURVEYTOOL.SubmitChanges();
                }
            }

            List<string> statusList = new List<string>();

            statusList.Add("");
            statusList.Add("진행중");
            statusList.Add("완료");
            statusList.Add("접촉중");

            ViewBag.StatusList = statusList;

            return list;
        }

        public ActionResult List()
        {
            if (string.IsNullOrEmpty(UID) == true)
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

            List<KLOWF2022_Panel> list = GetList();

            progressCount.All = list.Count.ToString();
            progressCount.Ing = list.Where(w => w.status == "진행중").Count().ToString();
            progressCount.End = list.Where(w => w.status == "완료").Count().ToString();

            return PartialView("ProgressPartial", progressCount);
        }

        public HouseInfo GetPanelInfo(string hid = "")
        {
            Dictionary<decimal?, string> dicStateCodeToLabel = new Dictionary<decimal?, string>();
            dicStateCodeToLabel[0] = "";
            dicStateCodeToLabel[1] = "선정탈락";
            dicStateCodeToLabel[2] = "쿼터오버";
            dicStateCodeToLabel[3] = "진행중";
            dicStateCodeToLabel[4] = "완료";
            dicStateCodeToLabel[5] = "사용안함";
            dicStateCodeToLabel[6] = "오류";

            KLOWF2022_Panel surveyinfo;
            HouseInfo list = new HouseInfo();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("select * from( SELECT id, PID, name, uid, surveySequenceNumber, surveyMethodCode, lastSurveyMethodCode, answerStateCode, lastQ FROM T_SamplingList_q221411 union SELECT id, PID, name, uid, surveySequenceNumber, surveyMethodCode, lastSurveyMethodCode, answerStateCode, lastQ FROM T_SamplingList_q221411_1) T where T.pid = '{0}'", hid)).FirstOrDefault();
                    surveyinfo = (from s in TAPI.KLOWF2022_Panel
                                  where s.hid == hid && s.uid == UID
                                  select s).First();

                    string status = "진행중";
                    string houseStatus = t_SamplingList != null ? dicStateCodeToLabel[t_SamplingList.answerStateCode ?? 0] : "";

                    list = (from p in TAPI.KLOWF2022_Panel
                            where p.uid == UID && p.hid == hid
                            select new HouseInfo
                            {
                                hid = p.hid,
                                hname = p.hname,
                                iname = p.iname,
                                address = p.addr1,
                                phone = p.phone,
                                houseStatus = houseStatus
                            }).First();

                    list.PersonList = new List<PersonInfo>();

                    //string dataFile = string.Format(@"{0}{1}.xml", Server.MapPath("../").Replace(@"TAPI", @"DATA\11090"), hid);
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\13204").Replace("/q221411", ""), hid);
                    //_sj : 일반 설문에 데이터 없는 경우 분가용 데이터 확인
                    string isSplit = "";
                    string ohid = "";
                    if (System.IO.File.Exists(dataFile) == false)
                    {
                        isSplit = "분가";
                        dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\13208").Replace("/q221411", ""), hid);
                    }

                    if (System.IO.File.Exists(dataFile) == true)
                    {
                        StreamReader srAnswerData = new StreamReader(dataFile, Encoding.Default);
                        string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());
                        XElement AnswerData = XElement.Parse(answerDataDecrypt);
                        
                        srAnswerData.Close();
                        srAnswerData.Dispose();

                        //sj : 조사 전 변수 변경 여부 확인 필요
                        string fleader = GetAnswerData(AnswerData, "fleader").Trim();
                        string q760 = GetAnswerData(AnswerData, "q760").Trim();
                        string q10address_fin = GetAnswerData(AnswerData, "q10address_fin").Trim();
                        string q10address_fin_new = GetAnswerData(AnswerData, "q10address_fin_new").Trim();
                        string q10tel_fin = GetAnswerData(AnswerData, "q10tel_fin").Trim();
                        string q10hp_fin = GetAnswerData(AnswerData, "q10hp_fin").Trim();
                        string q10email_fin = GetAnswerData(AnswerData, "q10email_fin").Trim();

                        if (isSplit.Equals("분가"))
                        {
                            ohid = GetAnswerData(AnswerData, "OHID").Trim();
                        }

                        if (string.IsNullOrEmpty(fleader) == false)
                        {
                            list.hname = fleader;
                            surveyinfo.hname = fleader;
                        }

                        if (string.IsNullOrEmpty(q760) == false)
                        {
                            list.iname = q760;
                            surveyinfo.iname = q760;
                        }

                        if (string.IsNullOrEmpty(q10address_fin) == false)
                        {
                            surveyinfo.addr1 = q10address_fin;
                        }

                        if (string.IsNullOrEmpty(q10address_fin_new) == false)
                        {
                            surveyinfo.newAddr = q10address_fin_new;
                        }

                        if (string.IsNullOrEmpty(q10tel_fin) == false)
                        {
                            surveyinfo.phone = q10tel_fin;
                        }

                        if (string.IsNullOrEmpty(q10hp_fin) == false)
                        {
                            surveyinfo.mobile = q10hp_fin;
                        }

                        if (string.IsNullOrEmpty(q10email_fin) == false)
                        {
                            surveyinfo.email = q10email_fin;
                        }

                        surveyinfo.MONEY1 = GetAnswerData(AnswerData, "q830").Trim();
                        surveyinfo.BNAME = GetAnswerData(AnswerData, "q850_op1").Trim();
                        surveyinfo.BANK = GetAnswerData(AnswerData, "q850_op2").Trim();
                        surveyinfo.BNO = GetAnswerData(AnswerData, "q850_op3").Trim();
                        surveyinfo.gift = GetAnswerData(AnswerData, "q855_op1").Trim();

                        int personReal = 0;

                        for (int p = 1; p < 16; p++)
                        {
                            string pid = GetAnswerData(AnswerData, "PID_r" + p).Trim();
                            string ptype = GetAnswerData(AnswerData, "survey_NewYN_r" + p).Trim();

                            if (string.IsNullOrEmpty(pid) == false &&
                                GetAnswerData(AnswerData, "survey_target_r" + p).Trim() == "1")
                            {
                                PersonInfo personInfo = new PersonInfo();

                                personInfo.pid = pid;
                                personInfo.type = ptype;
                                personInfo.pname = GetAnswerData(AnswerData, "q100name_r" + p);
                                personInfo.age = GetAnswerData(AnswerData, "FamAge_r" + p);

                                decimal dataPath = 13205;

                                if (ptype == "기존")
                                {
                                    T_SamplingList_q221411_2 t_SamplingList_q221411_2 = SURVEY.T_SamplingList_q221411_2.FirstOrDefault(f => f.PID == pid);

                                    t_SamplingList_q221411_2.quota15 = hid;
                                    t_SamplingList_q221411_2.quota16 = p.ToString();

                                    t_SamplingList_q221411_2.quota17 = ohid;
                                    t_SamplingList_q221411_2.quota18 = isSplit;

                                    personInfo.personStatus = t_SamplingList_q221411_2 != null ? dicStateCodeToLabel[t_SamplingList_q221411_2.answerStateCode ?? 0] : "";
                                    personInfo.personURL = string.Format("./PersonSurvey?hid={0}&pid={1}&ptype=1", hid, pid);
                                }
                                else if (ptype == "사망")
                                {
                                    personInfo.pname = GetAnswerData(AnswerData, "q20name_r" + p);

                                    T_SamplingList_q221411_2 t_SamplingList_q221411_2 = SURVEY.T_SamplingList_q221411_2.FirstOrDefault(f => f.PID == pid);

                                    if (t_SamplingList_q221411_2 != null)
                                    {
                                        t_SamplingList_q221411_2.quota15 = hid;
                                        t_SamplingList_q221411_2.quota16 = p.ToString();

                                        t_SamplingList_q221411_2.quota17 = ohid;
                                        t_SamplingList_q221411_2.quota18 = isSplit;

                                        personInfo.personStatus = t_SamplingList_q221411_2 != null ? dicStateCodeToLabel[t_SamplingList_q221411_2.answerStateCode ?? 0] : "";
                                        personInfo.personURL = string.Format("./PersonSurvey?hid={0}&pid={1}&ptype=1", hid, pid);
                                    }
                                    else
                                    {
                                        T_SamplingList_q221411_3 t_SamplingList_q221411_3 = SURVEY.T_SamplingList_q221411_3.FirstOrDefault(f => f.PID == pid);

                                        if (t_SamplingList_q221411_3 == null)
                                        {
                                            t_SamplingList_q221411_3 = new T_SamplingList_q221411_3();
                                            t_SamplingList_q221411_3.PID = pid;

                                            SURVEY.T_SamplingList_q221411_3.InsertOnSubmit(t_SamplingList_q221411_3);
                                        }

                                        t_SamplingList_q221411_3.quota15 = hid;
                                        t_SamplingList_q221411_3.quota16 = p.ToString();

                                        t_SamplingList_q221411_3.quota17 = ohid;
                                        t_SamplingList_q221411_3.quota18 = isSplit;

                                        personInfo.personStatus = t_SamplingList_q221411_3 != null ? dicStateCodeToLabel[t_SamplingList_q221411_3.answerStateCode ?? 0] : "";
                                        personInfo.personURL = string.Format("./PersonSurvey?hid={0}&pid={1}&ptype=2", hid, pid);

                                        dataPath = 13207;
                                    }
                                }
                                else if (ptype == "신규")
                                {
                                    T_SamplingList_q221411_3 t_SamplingList_q221411_3 = SURVEY.T_SamplingList_q221411_3.FirstOrDefault(f => f.PID == pid);

                                    if (t_SamplingList_q221411_3 == null)
                                    {
                                        t_SamplingList_q221411_3 = new T_SamplingList_q221411_3();
                                        t_SamplingList_q221411_3.PID = pid;

                                        SURVEY.T_SamplingList_q221411_3.InsertOnSubmit(t_SamplingList_q221411_3);
                                    }

                                    t_SamplingList_q221411_3.quota15 = hid;
                                    t_SamplingList_q221411_3.quota16 = p.ToString();

                                    t_SamplingList_q221411_3.quota17 = ohid;
                                    t_SamplingList_q221411_3.quota18 = isSplit;

                                    personInfo.personStatus = t_SamplingList_q221411_3 != null ? dicStateCodeToLabel[t_SamplingList_q221411_3.answerStateCode ?? 0] : "";
                                    personInfo.personURL = string.Format("./PersonSurvey?hid={0}&pid={1}&ptype=2", hid, pid);

                                    dataPath = 13207;
                                }

                                if (personInfo.personStatus == "완료")
                                {
                                    string personDataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + dataPath.ToString()).Replace("/q221411", ""), pid);
                                    

                                    StreamReader srPersonAnswerData = new StreamReader(personDataFile, Encoding.Default);
                                    string PersonAnswerDataDecrypt = AES256.Crypto.decryptAES256(srPersonAnswerData.ReadToEnd());
                                    XElement PersonAnswerData = XElement.Parse(PersonAnswerDataDecrypt);

                                    srPersonAnswerData.Close();
                                    srPersonAnswerData.Dispose();

                                    if (GetAnswerData(PersonAnswerData, "q10") != "조사시작")
                                    {
                                        personInfo.personStatus = GetAnswerData(PersonAnswerData, "q10");
                                    }
                                }

                                list.PersonList.Add(personInfo);

                                SURVEY.SubmitChanges();

                                KLOWF2022_PersonInfo kLOWF2022_PersonInfo = TAPI.KLOWF2022_PersonInfo.FirstOrDefault(f => f.pid == pid && f.hid == hid && f.uid == UID);

                                if (kLOWF2022_PersonInfo == null)
                                {
                                    kLOWF2022_PersonInfo = new KLOWF2022_PersonInfo();

                                    kLOWF2022_PersonInfo.pid = pid;
                                    kLOWF2022_PersonInfo.hid = hid;
                                    kLOWF2022_PersonInfo.uid = UID;

                                    TAPI.KLOWF2022_PersonInfo.InsertOnSubmit(kLOWF2022_PersonInfo);
                                }

                                kLOWF2022_PersonInfo.pname = personInfo.pname;
                                kLOWF2022_PersonInfo.statusText = personInfo.personStatus;
                                kLOWF2022_PersonInfo.isNew = ptype;

                                TAPI.SubmitChanges();

                                personReal++;
                            }
                            else if (ptype == "분가")
                            {
                                // 분가 가구아이디 700000대로 작업
                                //string splitHID = hid + "_" + p;
                                int _hid = 700000000 + (p * 1000000) + Convert.ToInt32(hid);
                                string splitHID = _hid.ToString();
                                string area = GetAnswerData(AnswerData, "q80_r" + p + "_op14");
                                string office = "";
                                if (area.Contains("서울특별시") == true
                                    || area.Contains("인천광역시") == true
                                    || area.Contains("경기도") == true
                                    || area.Contains("강원도") == true)
                                {
                                    office = "서울사무소";
                                }
                                else if (area.Contains("광주광역시") == true
                                    || area.Contains("전라북도") == true
                                    || area.Contains("전라남도") == true
                                    || area.Contains("제주") == true)
                                {
                                    office = "광주사무소";
                                }
                                else if (area.Contains("대구광역시") == true
                                    || area.Contains("경상북도") == true)
                                {
                                    office = "대구사무소";
                                }
                                else if (area.Contains("대전광역시") == true
                                    || area.Contains("충청북도") == true
                                    || area.Contains("충청남도") == true
                                    || area.Contains("세종") == true)
                                {
                                    office = "대전사무소";
                                }
                                else if (area.Contains("부산광역시") == true
                                    || area.Contains("울산광역시") == true
                                    || area.Contains("경상남도") == true)
                                {
                                    office = "부산사무소";
                                }

                                KLOWF2022_Panel kLOWF2022_Panel = TAPI.KLOWF2022_Panel.FirstOrDefault(f => f.hid == hid);
                                string gubun = kLOWF2022_Panel.gubun;


                                KLOWF2022_Split kLOWF2022_Split = TAPI.KLOWF2022_Split.FirstOrDefault(f => f.hid == splitHID);

                                if (kLOWF2022_Split == null)
                                {
                                    kLOWF2022_Split = new KLOWF2022_Split();

                                    kLOWF2022_Split.hid = splitHID;
                                    kLOWF2022_Split.uid = UID;
                                    kLOWF2022_Split.addr1 = GetAnswerData(AnswerData, "q80_r" + p + "_op13");
                                    kLOWF2022_Split.addr2 = GetAnswerData(AnswerData, "q80_r" + p + "_op10");
                                    kLOWF2022_Split.phone = "";
                                    kLOWF2022_Split.mobile = $"{GetAnswerData(AnswerData, "q80_r" + p + "_op2")}-{GetAnswerData(AnswerData, "q80_r" + p + "_op3")}-{GetAnswerData(AnswerData, "q80_r" + p + "_op4")}";
                                    kLOWF2022_Split.DateTransfer = DateTime.Now.ToString("yyyy-MM-dd");
                                    kLOWF2022_Split.hname = GetAnswerData(AnswerData, "q100name_r" + p);

                                    kLOWF2022_Split.area = area;
                                    kLOWF2022_Split.office = office;
                                    kLOWF2022_Split.gubun = gubun;

                                    TAPI.KLOWF2022_Split.InsertOnSubmit(kLOWF2022_Split);
                                    TAPI.SubmitChanges();
                                }
                                else
                                {
                                    string addr1 = GetAnswerData(AnswerData, "q80_r" + p + "_op13");
                                    string addr2 = GetAnswerData(AnswerData, "q80_r" + p + "_op10");
                                    string mobile = $"{GetAnswerData(AnswerData, "q80_r" + p + "_op2")}-{GetAnswerData(AnswerData, "q80_r" + p + "_op3")}-{GetAnswerData(AnswerData, "q80_r" + p + "_op4")}";
                                    string DateTransfer = DateTime.Now.ToString("yyyy-MM-dd");
                                    string hname = GetAnswerData(AnswerData, "q100name_r" + p);

                                    TAPI.ExecuteCommand($"UPDATE KLOWF2022_Split SET addr1 = '{addr1}', addr2 = '{addr2}', mobile = '{mobile}', DateTransfer = '{DateTransfer}', hname='{hname}', area='{area}', office='{office}',gubun='{gubun}' WHERE hid = '{splitHID}'");
                                    TAPI.SubmitChanges();
                                }

                                // 가구용 SamplingList Insert - 09.22 201277_1로 insert 대상 수정
                                T_SamplingList_q221411_1 t_SamplingList_q221411_split = SURVEY.T_SamplingList_q221411_1.FirstOrDefault(f => f.PID == splitHID);
                                if (t_SamplingList_q221411_split == null)
                                {
                                    T_SamplingList_q221411 t_SamplingList_q221411 = SURVEY.T_SamplingList_q221411.FirstOrDefault(f => f.PID == hid);
                                    t_SamplingList_q221411_split = new T_SamplingList_q221411_1();
                                    t_SamplingList_q221411_split.PID = splitHID;
                                    t_SamplingList_q221411_split.QMListData = t_SamplingList_q221411.QMListData;

                                    SURVEY.T_SamplingList_q221411_1.InsertOnSubmit(t_SamplingList_q221411_split);
                                    SURVEY.SubmitChanges();
                                }
                                

                            }
                        }

                        for (int p = 1; p < 6; p++)
                        {
                            string pid = GetAnswerData(AnswerData, "New_PID_r" + p).Trim();

                            if (string.IsNullOrEmpty(pid) == false &&
                                GetAnswerData(AnswerData, "New_survey_target_r" + p).Trim() == "1")
                            {
                                string ptype = "신규";

                                PersonInfo personInfo = new PersonInfo();

                                personInfo.pid = pid;
                                personInfo.pname = GetAnswerData(AnswerData, "q190_r" + p + "_op1");
                                personInfo.age = GetAnswerData(AnswerData, "NewFamAge_r" + p);

                                decimal dataPath = 13205;

                                if (GetAnswerData(AnswerData, "q210_r" + p).Trim() == "분가 후 다시 원가구로 합가(개인용 한번이라도 한적있음)")
                                {
                                    ptype = "기존합가";

                                    personInfo.personURL = string.Format("./NewCode6Person?hid={0}&pid={1}&pnum={2}", hid, pid, GetAnswerData(AnswerData, "NewFamOrder_r" + p));

                                    T_SamplingList_q221411_2 t_SamplingList_q221411_2 = SURVEY.T_SamplingList_q221411_2.FirstOrDefault(f => f.PID == pid);

                                    if (t_SamplingList_q221411_2 == null)
                                    {
                                        T_SamplingList_q221411_3 t_SamplingList_q221411_3 = SURVEY.T_SamplingList_q221411_3.FirstOrDefault(f => f.PID == pid);

                                        personInfo.personStatus = t_SamplingList_q221411_3 != null ? dicStateCodeToLabel[t_SamplingList_q221411_3.answerStateCode ?? 0] : "";

                                        dataPath = 13207;

                                        if ($"{t_SamplingList_q221411_3?.quota17}".Trim() != "")
                                        {
                                            personInfo.personURL = string.Format("./PersonSurvey?hid={0}&pid={1}&ptype=2", hid, pid);
                                        }

                                        ptype = "신규합가";
                                    }
                                    else
                                    {
                                        if ($"{t_SamplingList_q221411_2?.quota17}".Trim() != "")
                                        {
                                            personInfo.personURL = string.Format("./PersonSurvey?hid={0}&pid={1}&ptype=1", hid, pid);
                                        }

                                        personInfo.personStatus = t_SamplingList_q221411_2 != null ? dicStateCodeToLabel[t_SamplingList_q221411_2.answerStateCode ?? 0] : "";
                                    }
                                }
                                else
                                {
                                    T_SamplingList_q221411_3 t_SamplingList_q221411_3 = SURVEY.T_SamplingList_q221411_3.FirstOrDefault(f => f.PID == pid);

                                    if (t_SamplingList_q221411_3 == null)
                                    {
                                        t_SamplingList_q221411_3 = new T_SamplingList_q221411_3();
                                        t_SamplingList_q221411_3.PID = pid;

                                        SURVEY.T_SamplingList_q221411_3.InsertOnSubmit(t_SamplingList_q221411_3);
                                    }

                                    t_SamplingList_q221411_3.quota15 = hid;
                                    t_SamplingList_q221411_3.quota16 = GetAnswerData(AnswerData, "NewFamOrder_r" + p);

                                    t_SamplingList_q221411_3.quota17 = ohid;
                                    t_SamplingList_q221411_3.quota18 = isSplit;

                                    personInfo.personStatus = t_SamplingList_q221411_3 != null ? dicStateCodeToLabel[t_SamplingList_q221411_3.answerStateCode ?? 0] : "";
                                    personInfo.personURL = string.Format("./PersonSurvey?hid={0}&pid={1}&ptype=2", hid, pid);

                                    dataPath = 13207;
                                }

                                personInfo.type = ptype;

                                if (personInfo.personStatus == "완료")
                                {
                                    
                                    string personDataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + dataPath.ToString()).Replace("/q221411", ""), pid);
                                    StreamReader srPersonAnswerData = new StreamReader(personDataFile, Encoding.Default);
                                    string personAnswerDataDecrypt = AES256.Crypto.decryptAES256(srPersonAnswerData.ReadToEnd());
                                    XElement PersonAnswerData = XElement.Parse(personAnswerDataDecrypt);

                                    srPersonAnswerData.Close();
                                    srPersonAnswerData.Dispose();

                                    if (GetAnswerData(PersonAnswerData, "q10") != "조사시작")
                                    {
                                        personInfo.personStatus = GetAnswerData(PersonAnswerData, "q10");
                                    }
                                }

                                list.PersonList.Add(personInfo);

                                SURVEY.SubmitChanges();

                                KLOWF2022_PersonInfo kLOWF2022_PersonInfo = TAPI.KLOWF2022_PersonInfo.FirstOrDefault(f => f.pid == pid && f.hid == hid && f.uid == UID);

                                if (kLOWF2022_PersonInfo == null)
                                {
                                    kLOWF2022_PersonInfo = new KLOWF2022_PersonInfo();

                                    kLOWF2022_PersonInfo.pid = pid;
                                    kLOWF2022_PersonInfo.hid = hid;
                                    kLOWF2022_PersonInfo.uid = UID;

                                    TAPI.KLOWF2022_PersonInfo.InsertOnSubmit(kLOWF2022_PersonInfo);
                                }

                                kLOWF2022_PersonInfo.pname = personInfo.pname;
                                kLOWF2022_PersonInfo.statusText = personInfo.personStatus;
                                kLOWF2022_PersonInfo.isNew = ptype;

                                TAPI.SubmitChanges();

                                personReal++;
                            }
                        }

                        surveyinfo.personReal = personReal.ToString();
                        surveyinfo.personTotal = GetAnswerData(AnswerData, "TotalFamCount");
                    }


                    int personCompleted = list.PersonList.Where(w => string.IsNullOrEmpty(w.personStatus) == false && w.personStatus != "진행중").Count();

                    surveyinfo.personCompleted = personCompleted.ToString();

                    if (houseStatus == "완료" && list.PersonList.Count > 0 && list.PersonList.Count == personCompleted)
                    {
                        status = "완료";
                    }

                    List<KLOWF2022_PersonInfo> deleteList = TAPI.KLOWF2022_PersonInfo.Where(w => w.hid == hid && list.PersonList.Select(s => s.pid).ToList().Contains(w.pid) == false).ToList();

                    TAPI.KLOWF2022_PersonInfo.DeleteAllOnSubmit(deleteList);

                    surveyinfo.status = status;
                    surveyinfo.houseStatus = houseStatus;

                    KLOWF2022_Contact kLOWF2022_ContactLast = (from contact in TAPI.KLOWF2022_Contact
                                                               where contact.HID == hid
                                                               select contact).OrderByDescending(o => o.ranking).FirstOrDefault();

                    if (kLOWF2022_ContactLast == null || kLOWF2022_ContactLast.status != "조사협조")
                    {
                        KLOWF2022_Contact kLOWF2022_Contact = new KLOWF2022_Contact();

                        kLOWF2022_Contact.HID = hid;
                        kLOWF2022_Contact.uid = UID;
                        kLOWF2022_Contact.contactDate = DateTime.Now.ToString("yyyy-MM-dd");
                        kLOWF2022_Contact.contactTime = DateTime.Now.ToString("HH:mm");
                        kLOWF2022_Contact.status = "조사협조";
                        kLOWF2022_Contact.ranking = kLOWF2022_ContactLast == null ? 1 : kLOWF2022_ContactLast.ranking + 1;

                        kLOWF2022_Contact.inputDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        TAPI.KLOWF2022_Contact.InsertOnSubmit(kLOWF2022_Contact);
                    }

                    TAPI.SubmitChanges();
                }
            }

            return list;
        }

        public ActionResult Agree(string hid)
        {
            if (string.IsNullOrEmpty(UID) == true)
            {
                return RedirectToAction("Login", "Home");
            }

            string viewName = "Agree";

            using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    KLOWF2022_Panel surveyinfo = (from s in TAPI.KLOWF2022_Panel
                                                  where s.hid == hid && s.uid == UID
                                                  select s).First();

                    if (surveyinfo.agreeYN == 1)
                    {
                        viewName = "PanelInfo";
                    }
                    else
                    {
                        T_Interviewer t_Interviewer = SURVEYTOOL.T_Interviewers.Where(w => w.employeeNumber.ToString() == UID).FirstOrDefault();

                        if (t_Interviewer != null)
                        {
                            ViewBag.UName = t_Interviewer.userName;
                        }
                        else
                        {
                            ViewBag.UName = UID;
                        }

                        viewName = "Agree";
                    }
                }
            }

            return View(viewName, GetPanelInfo(hid));
        }

        public ActionResult AgreeOK(string hid)
        {
            if (string.IsNullOrEmpty(UID) == true)
            {
                return RedirectToAction("Login", "Home");
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                KLOWF2022_Panel surveyinfo = (from s in TAPI.KLOWF2022_Panel
                                              where s.hid == hid && s.uid == UID
                                              select s).First();

                surveyinfo.agreeYN = 1;
                surveyinfo.agreeDate = DateTime.Today.ToString("yyyy-MM-dd");

                TAPI.SubmitChanges();
            }

            return null;
        }

        public ActionResult Disagree(string hid)
        {
            if (string.IsNullOrEmpty(UID) == true)
            {
                return RedirectToAction("Login", "Home");
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                KLOWF2022_Contact kLOWF2022_ContactLast = (from contact in TAPI.KLOWF2022_Contact
                                                           where contact.HID == hid
                                                           select contact).OrderByDescending(o => o.ranking).FirstOrDefault();

                KLOWF2022_Contact kLOWF2022_Contact = new KLOWF2022_Contact();

                kLOWF2022_Contact.HID = hid;
                kLOWF2022_Contact.uid = UID;
                kLOWF2022_Contact.contactDate = DateTime.Now.ToString("yyyy-MM-dd");
                kLOWF2022_Contact.contactTime = DateTime.Now.ToString("HH:mm");
                kLOWF2022_Contact.status = "단순거절";
                kLOWF2022_Contact.ranking = kLOWF2022_ContactLast == null ? 1 : kLOWF2022_ContactLast.ranking + 1;

                kLOWF2022_Contact.inputDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                TAPI.KLOWF2022_Contact.InsertOnSubmit(kLOWF2022_Contact);

                KLOWF2022_Panel surveyinfo = (from s in TAPI.KLOWF2022_Panel
                                              where s.hid == hid && s.uid == UID
                                              select s).First();

                surveyinfo.houseStatus = "컨택중";
                surveyinfo.status = "단순거절";

                surveyinfo.agreeYN = 0;
                surveyinfo.agreeDate = DateTime.Today.ToString("yyyy-MM-dd");

                TAPI.SubmitChanges();
            }

            return null;
        }

        public ActionResult PanelInfo(string hid)
        {
            if (string.IsNullOrEmpty(UID) == true)
            {
                return RedirectToAction("Login", "Home");
            }

            return View("PanelInfo", GetPanelInfo(hid));
        }

        public ActionResult HouseSurvey(string hid)
        {
            if (string.IsNullOrEmpty(UID) == true)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                ViewBag.HID = hid;

                KLOWF2022_Panel surveyinfo;
                string isSplit = "";

                bool eqnum = false;

                using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                    {
                        T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q221411 WHERE PID = '{0}'", hid)).FirstOrDefault();

                        surveyinfo = (from s in TAPI.KLOWF2022_Panel
                                      where s.hid == hid && s.uid == UID
                                      select s).SingleOrDefault();

                        if (t_SamplingList != null &&
                            t_SamplingList.answerStateCode == 4)
                        {
                            eqnum = true;
                        }

                        surveyinfo.dateStart = string.IsNullOrEmpty(surveyinfo.dateStart) == true ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : surveyinfo.dateStart;
                        if (surveyinfo.htype == "원패널(9차 분가가구)")
                        {
                            isSplit = "분가";
                            T_SamplingList_0 t_SamplingList_1 = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_q221411_1 WHERE PID = '{0}'", hid)).FirstOrDefault();


                            if (t_SamplingList_1 != null &&
                                t_SamplingList_1.answerStateCode == 4)
                            {
                                eqnum = true;
                            }
                        }

                        TAPI.SubmitChanges();
                    }
                }

                if (eqnum == false)
                {
                    ViewBag.C = "https://rpssurvey.hrcglobal.com/?c=" + TripleDESCryptoService.Encrypt(string.Format("qn=q221411&pid={0}&uid={1}&t=tapi", hid, UID, eqnum));
                    if (isSplit == "분가")
                    {
                        ViewBag.C = "https://rpssurvey.hrcglobal.com/?c=" + TripleDESCryptoService.Encrypt(string.Format("qn=q221411_1&pid={0}&uid={1}&t=tapi", hid, UID, eqnum));
                    }
                    return View("PanelInfo", null);
                }
                else
                {
                    List<AnswerList> answerList = new List<AnswerList>();
                    if (isSplit == "")
                    {
                        string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\13204").Replace("/q221411", ""), hid);
                        
                        
                        using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                        {
                            StreamReader srAnswerData = new StreamReader(dataFile, Encoding.Default);
                            string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());
                            XElement AnswerData = XElement.Parse(answerDataDecrypt);

                            srAnswerData.Close();
                            srAnswerData.Dispose();

                            T_Script t_Script = SURVEYTOOL.T_Scripts.Where(w => w.questionnaireID == 13204).First();

                            if (t_Script != null)
                            {
                                XElement variableList = t_Script.variableList;

                                answerList = (from element in variableList.Elements()
                                              select new AnswerList
                                              {
                                                  Variable = element.Name.ToString(),
                                                  Title = element.Value,
                                                  Answer = GetAnswerData(AnswerData, element.Name.ToString()),
                                                  Url = TripleDESCryptoService.Encrypt(string.Format("qn=q221411&pid={0}&uid={1}&eqnum={2}&t=tapi", hid, UID, element.Name.ToString())),
                                              }).Where(w => string.IsNullOrEmpty(w.Answer) == false).ToList();
                            }
                        }
                    }
                    else
                    {
                        // 분가 가구 일 경우
                        string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\13208").Replace("/q221411_1", ""), hid);
                        using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                        {
                            StreamReader srAnswerData = new StreamReader(dataFile, Encoding.Default);
                            string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());
                            XElement AnswerData = XElement.Parse(answerDataDecrypt);

                            srAnswerData.Close();
                            srAnswerData.Dispose();

                            T_Script t_Script = SURVEYTOOL.T_Scripts.Where(w => w.questionnaireID == 13208).First();

                            if (t_Script != null)
                            {
                                XElement variableList = t_Script.variableList;

                                answerList = (from element in variableList.Elements()
                                              select new AnswerList
                                              {
                                                  Variable = element.Name.ToString(),
                                                  Title = element.Value,
                                                  Answer = GetAnswerData(AnswerData, element.Name.ToString()),
                                                  Url = TripleDESCryptoService.Encrypt(string.Format("qn=q221411_1&pid={0}&uid={1}&eqnum={2}&t=tapi", hid, UID, element.Name.ToString())),
                                              }).Where(w => string.IsNullOrEmpty(w.Answer) == false).ToList();
                            }
                        }
                    }

                    return View("EditHouseSurvey", answerList);
                }
            }
            catch (Exception ee)
            {
                return View("Error", (object)(ee.Message));
            }
        }

        public ActionResult PersonSurvey(string hid, string pid, int ptype)
        {
            if (string.IsNullOrEmpty(UID) == true)
            {
                return RedirectToAction("Login", "Home");
            }

            try
            {
                ViewBag.HID = hid;

                bool eqnum = false;

                string qn = "q221411_2";
                decimal dataPath = 13205;

                if (ptype == 2)
                {
                    qn = "q221411_3";
                    dataPath = 13207;
                }

                using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                {
                    T_SamplingList_0 t_SamplingList = SURVEY.ExecuteQuery<T_SamplingList_0>(string.Format("SELECT * FROM T_SamplingList_{0} WHERE PID = '{1}'", qn, pid)).FirstOrDefault();

                    if (t_SamplingList != null &&
                        t_SamplingList.answerStateCode == 4)
                    {
                        eqnum = true;
                    }
                }

                if (eqnum == false)
                {
                    ViewBag.C = "https://rpssurvey.hrcglobal.com/?c=" + TripleDESCryptoService.Encrypt(string.Format("qn={0}&pid={1}&uid={2}&t=tapi", qn, pid, UID));

                    return View("EditPersonSurvey", null);
                }
                else
                {
                    List<AnswerList> answerList = new List<AnswerList>();

                    
                    string dataFile = string.Format(@"{0}{1}.hrcdata", Server.MapPath("../").Replace(@"TAPI", @"DATA\" + dataPath.ToString()).Replace("/q221411", ""), pid);

                    using (SURVEYTOOLDataContext SURVEYTOOL = new SURVEYTOOLDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEYTOOL;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
                    {
                        StreamReader srAnswerData = new StreamReader(dataFile, Encoding.Default);
                        string answerDataDecrypt = AES256.Crypto.decryptAES256(srAnswerData.ReadToEnd());
                        XElement AnswerData = XElement.Parse(answerDataDecrypt);

                        srAnswerData.Close();
                        srAnswerData.Dispose();

                        T_Script t_Script = SURVEYTOOL.T_Scripts.Where(w => w.questionnaireID == dataPath).First();

                        if (t_Script != null)
                        {
                            XElement variableList = t_Script.variableList;

                            answerList = (from element in variableList.Elements()
                                          select new AnswerList
                                          {
                                              Variable = element.Name.ToString(),
                                              Title = element.Value,
                                              Answer = GetAnswerData(AnswerData, element.Name.ToString()),
                                              Url = TripleDESCryptoService.Encrypt(string.Format("qn={0}&pid={1}&uid={2}&eqnum={3}&t=tapi", qn, pid, UID, element.Name.ToString())),
                                          }).Where(w => string.IsNullOrEmpty(w.Answer) == false).ToList();
                        }
                    }

                    return View("EditPersonSurvey", answerList);
                }
            }
            catch (Exception ee)
            {
                return View("Error", (object)(ee.Message));
            }
        }

        public ActionResult Contact(string hid)
        {
            if (string.IsNullOrEmpty(UID) == true)
            {
                return RedirectToAction("Login", "Home");
            }

            List<KLOWF2022_Contact> kLOWF2022_Contact = new List<KLOWF2022_Contact>();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                KLOWF2022_Panel surveyinfo = (from s in TAPI.KLOWF2022_Panel
                                              where s.hid == hid && s.uid == UID
                                              select s).First();

                kLOWF2022_Contact = (from contact in TAPI.KLOWF2022_Contact
                                     where contact.HID == hid
                                     select contact).ToList();

                ViewBag.HID = surveyinfo.hid;
                ViewBag.hname = surveyinfo.hname;
                ViewBag.Ranking = (kLOWF2022_Contact.Count + 1).ToString();

                ViewBag.contactMonth = DateTime.Now.ToString("MM");
                ViewBag.contactDay = DateTime.Now.ToString("dd");
                ViewBag.contactHour = DateTime.Now.ToString("HH");
                ViewBag.contactMin = DateTime.Now.ToString("mm");
                ViewBag.status = surveyinfo.status;

                if (kLOWF2022_Contact.Count > 0)
                {
                    ViewBag.lastContact = kLOWF2022_Contact?.OrderBy(o => o.ranking)?.Last()?.status ?? "";
                }
                else
                {
                    ViewBag.lastContact = "";
                }
            }

            return View("Contact", kLOWF2022_Contact);
        }

        public ActionResult ContactOK(string hid, string contactMonth, string contactDay, string contactHour, string contactMin, string status, string etc, int ranking)
        {
            if (string.IsNullOrEmpty(UID) == true)
            {
                return RedirectToAction("Login", "Home");
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                KLOWF2022_Contact kLOWF2022_Contact = new KLOWF2022_Contact();

                kLOWF2022_Contact.HID = hid;
                kLOWF2022_Contact.uid = UID;
                kLOWF2022_Contact.ranking = ranking;
                kLOWF2022_Contact.contactDate = DateTime.Now.Year.ToString() + "-" + contactMonth + "-" + contactDay;
                kLOWF2022_Contact.contactTime = $"{contactHour}:{contactMin}";
                kLOWF2022_Contact.status = status;
                kLOWF2022_Contact.etc = etc;
                kLOWF2022_Contact.inputDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                TAPI.KLOWF2022_Contact.InsertOnSubmit(kLOWF2022_Contact);

                KLOWF2022_Panel surveyinfo = (from s in TAPI.KLOWF2022_Panel
                                              where s.hid == hid && s.uid == UID
                                              select s).First();

                surveyinfo.houseStatus = "컨택중";
                surveyinfo.status = status;

                TAPI.SubmitChanges();
            }

            return null;
        }

        public ActionResult NewCode6Person(string hid, string pid, string pnum)
        {
            if (string.IsNullOrEmpty(UID) == true)
            {
                return RedirectToAction("Login", "Home");
            }

            ViewBag.HID = hid;
            ViewBag.PID = pid;
            ViewBag.Name = "";
            ViewBag.PNum = pnum;
            ViewBag.OPID = "";

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                KLOWF2022_PersonInfo kLOWF2022_PersonInfo = TAPI.KLOWF2022_PersonInfo.FirstOrDefault(f => f.pid == pid && f.hid == hid && f.uid == UID);

                if (kLOWF2022_PersonInfo != null)
                {
                    ViewBag.Name = kLOWF2022_PersonInfo.pname;
                    ViewBag.OPID = kLOWF2022_PersonInfo.opid;
                }

                TAPI.SubmitChanges();
            }

            return View();
        }

        public ActionResult CheckOPID(string opid)
        {
            using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                T_SamplingList_q221411_2 t_SamplingList_q221411_2 = SURVEY.T_SamplingList_q221411_2.FirstOrDefault(f => f.PID == opid);

                return PartialView("CheckOPID", t_SamplingList_q221411_2);
            }
        }

        public string NewCode6PersonSurvey(string hid, string pid, string pnum, string opid)
        {
            if (string.IsNullOrEmpty(UID) == true)
            {
                return "https://rpssurvey.hrcglobal.com/TAPI";
            }

            string url = "";
            string ptype = "";

            using (SURVEYDataContext SURVEY = new SURVEYDataContext("Data Source=10.0.11.175,15000;Initial Catalog=SURVEY;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                if (opid == "모름")
                {
                    T_SamplingList_q221411_3 t_SamplingList_q221411_3 = SURVEY.T_SamplingList_q221411_3.FirstOrDefault(f => f.PID == pid);

                    if (t_SamplingList_q221411_3 == null)
                    {
                        t_SamplingList_q221411_3 = new T_SamplingList_q221411_3();
                        t_SamplingList_q221411_3.PID = pid;

                        SURVEY.T_SamplingList_q221411_3.InsertOnSubmit(t_SamplingList_q221411_3);
                    }

                    t_SamplingList_q221411_3.quota15 = hid;
                    t_SamplingList_q221411_3.quota16 = pnum;
                    t_SamplingList_q221411_3.quota17 = opid;

                    SURVEY.SubmitChanges();

                    url = string.Format("./PersonSurvey?hid={0}&pid={1}&ptype=2", hid, pid);
                    ptype = "신규합가";
                }
                else
                {
                    SURVEY.ExecuteCommand($"UPDATE T_SamplingList_q221411_2 SET PID = '{pid}', quota15 = '{hid}', quota16 = '{pnum}', quota17 = '{opid}' WHERE PID = '{opid}'");

                    SURVEY.SubmitChanges();

                    url = string.Format("./PersonSurvey?hid={0}&pid={1}&ptype=1", hid, pid);
                    ptype = "기존합가";
                }
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                KLOWF2022_PersonInfo kLOWF2022_PersonInfo = TAPI.KLOWF2022_PersonInfo.FirstOrDefault(f => f.pid == pid && f.hid == hid && f.uid == UID);

                kLOWF2022_PersonInfo.opid = opid;
                kLOWF2022_PersonInfo.isNew = ptype;

                TAPI.SubmitChanges();
            }

            return url;
        }

        public ActionResult Transfer(string hid)
        {
            if (string.IsNullOrEmpty(UID) == true)
            {
                return RedirectToAction("Login", "Home");
            }

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                KLOWF2022_Panel kLOWF2022_Panel = TAPI.KLOWF2022_Panel.Where(w => w.uid == UID && w.hid == hid).First();

                return View("Transfer", kLOWF2022_Panel);
            }
        }

        public ActionResult TransferOK(string hid, string addr1, string addr2, string addr3, string tel1, string tel2, string etc)
        {
            if (string.IsNullOrEmpty(UID) == true)
            {
                return RedirectToAction("Login", "Home");
            }

            KLOWF2022_Transfer kLOWF2022_Transfer = new KLOWF2022_Transfer();
            KLOWF2022_Panel kLOWF2022_Panel = new KLOWF2022_Panel();

            using (TAPIDataContext TAPI = new TAPIDataContext("Data Source=10.0.11.175,15000;Initial Catalog=TAPI;Persist Security Info=True;User ID=kimsj;password=dv5000157$"))
            {
                kLOWF2022_Transfer.HID = hid;
                kLOWF2022_Transfer.uid = UID;
                kLOWF2022_Transfer.addr1 = addr1;
                kLOWF2022_Transfer.addr2 = addr2;
                kLOWF2022_Transfer.addr3 = addr3;
                kLOWF2022_Transfer.tel1 = tel1.Replace("--", "");
                kLOWF2022_Transfer.tel2 = tel2.Replace("--", "");
                kLOWF2022_Transfer.etc = etc;
                kLOWF2022_Transfer.transferDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                TAPI.KLOWF2022_Transfer.InsertOnSubmit(kLOWF2022_Transfer);

                kLOWF2022_Panel = (from panel in TAPI.KLOWF2022_Panel
                                   where panel.hid == hid
                                   select panel).SingleOrDefault();

                kLOWF2022_Panel.transfer_address = string.Format("(이관){0} {1} {2}", addr1, addr2, addr3);
                kLOWF2022_Panel.transfer_etc = etc;
                kLOWF2022_Panel.status = "이관요청";

                if (tel1.Contains("없음") == false && tel1 != "--")
                {
                    kLOWF2022_Panel.transfer_tel1 = string.Format("(이관){0}", tel1);
                }

                if (tel2.Contains("없음") == false && tel2 != "--")
                {
                    kLOWF2022_Panel.transfer_tel2 = string.Format("(이관){0}", tel2);
                }

                TAPI.SubmitChanges();
            }

            return null;
        }

        public string GetAnswerData(XElement answerData, string variable, bool isCheckData = false)
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

                if (isCheckData == true && string.IsNullOrEmpty(value) == true)
                {
                    value = "응답값 확인";
                }
            }

            value = Regex.Replace(value, @"<(/)?([a-zA-Z]*)(\s[a-zA-Z]*=[^>]*)?(\s)*(/)?>", string.Empty);

            return value;
        }
    }
}