// ***********************************************************************
// Assembly         : GAMSDemo
// Author           : Administrator
// Created          : 03-18-2019
//
// Last Modified By : Administrator
// Last Modified On : 03-19-2019
// ***********************************************************************
// <copyright file="GAMSModel.cs" company="Microsoft">
//     Copyright © Microsoft 2019
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Diagnostics;
using GAMS;
//using NPOI.SS.UserModel;
//using NPOI.HSSF.UserModel;
//using NPOI.XSSF.UserModel;

/// <summary>
/// The GAMSHelper namespace.
/// </summary>
namespace GAMSHelper
{
    /// <summary>
    /// Class GAMSModel.
    /// </summary>
    public class GAMSModel
    {
        /// <summary>
        /// The workspace path
        /// </summary>
        private string workspacePath = null;
        private int model_n = 0;
        private string work_start_time;
        private bool hasPriorityOne = false;

        public int Model_N
        {
            get { return model_n; }
            set { model_n = value; }
        }

        public string Work_Start_Time { get => work_start_time; set => work_start_time = value; }


        /// <summary>
        /// Initializes a new instance of the <see cref="GAMSModel"/> class.
        /// </summary>
        /// <param name="workingDirectory">The working directory.</param>
        public GAMSModel(string workingDirectory = null)
        {
            workspacePath = workingDirectory;
        }
        /// <summary>
        /// Finalizes an instance of the <see cref="GAMSModel"/> class.
        /// </summary>
        ~GAMSModel() { }

        #region 从List（可以来源于数据库或者Excel）读取数据并运行GAMS model
        /// <summary>
        /// Runs the specified list data.
        /// </summary>
        /// <param name="listData">The list data.</param>
        /// <returns>List&lt;System.String&gt;[].</returns>
        public List<string>[] Run(List<string>[] listData)
        {
            int sheetNumber = 4;

            //以下参数来源于GAMS模型
            string[] setsTemplate = new string[5] { "i", "j","j1","j2","j3" };
            string[] setsName = new string[5] { "所有维修员元素点", "所有任务元素点","紧急任务","非紧急任务","必须完成的任务" };
            string[] parametersTemplate = new string[4] { "PL", "TL", "Tij", "Tjj" };
            string[] parametersName = new string[4] { "PLi", "TLj", "Tij", "Tjj" };

            GAMSWorkspace ws;
            //if (Environment.GetCommandLineArgs().Length > 1)
            //    ws = new GAMSWorkspace(systemDirectory: Environment.GetCommandLineArgs()[1]);
            //else
                ws = new GAMSWorkspace(workspacePath, null, DebugLevel.Off);
            GAMSDatabase db = ws.AddDatabase();
            GAMSSet[] gSet = new GAMSSet[setsTemplate.Length];
            GAMSParameter[] gPar = new GAMSParameter[parametersTemplate.Length];

            for (int i = 0; i < 2; i++) //前两个表设置，i、j以及PLi、TLj
            {
                gSet[i] = db.AddSet(setsTemplate[i], 1, setsName[i]);
                gPar[i] = db.AddParameter(parametersTemplate[i], 1, parametersName[i]);
                for (int j = 0; j < listData[2*i].Count; j++)
                {
                    gSet[i].AddRecord(listData[2 * i][j]);
                    gPar[i].AddRecord(listData[2 * i][j]).Value = Convert.ToDouble(listData[2 * i + 1][j]);
                }
            }

            //j1、j2、j3设置
            for (int i = 2; i < setsTemplate.Length; i++)
            {
                gSet[i] = db.AddSet(setsTemplate[i], 1, setsName[i]);
            }
            for (int i = 0; i < listData[10].Count; i++)
            {
                int priority = Convert.ToInt32(listData[10][i]);
                if (priority == 1)
                {
                    hasPriorityOne = true;
                }
                string taskID = listData[2][i];
                switch (priority)
                {
                    case 1: //j1、j3
                        gSet[2].AddRecord(taskID);
                        gSet[4].AddRecord(taskID);
                        break;
                    case 2: //j2、j3
                        gSet[3].AddRecord(taskID);
                        gSet[4].AddRecord(taskID);
                        break;
                    case 3: //j2
                        gSet[3].AddRecord(taskID);
                        break;
                    default:
                        break;
                }
            }

            for (int i = 2; i < 4; i++) //后两个表设置
            {
                gPar[i] = db.AddParameter(parametersTemplate[i], 2, parametersName[i]);
                for (int j = 0; j < listData[3*i-2].Count; j++)
                {
                    gPar[i].AddRecord(listData[3*i-2][j], listData[3*i-1][j]).Value = Convert.ToDouble(listData[3*i][j]);
                }
			}

            GAMSJob t = ws.AddJobFromString(GetModelText(model_n, work_start_time));
            using (GAMSOptions opt = ws.AddOptions())
            {
                opt.Defines.Add("gdxincname", db.Name);
                opt.AllModelTypes = "cplex";
                t.Run(opt, db);
            }
            List<string>[] resultData = GetResultData(t);

            return resultData;
        }
        #endregion

        /// <summary>
        /// Gets the result data.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns>List&lt;System.String&gt;[].</returns>
        //private List<string>[] GetResultData(GAMSJob t)
        //{
        //    List<string> Ts_Pid = new List<string>();
        //    List<string> Ts_Tid = new List<string>();
        //    List<string> Ts_id = new List<string>();
        //    List<string> Ts_time = new List<string>();

        //    List<string> TTs_Pid = new List<string>();
        //    List<string> TTs_Tid1 = new List<string>();
        //    List<string> TTs_Tid2 = new List<string>();
        //    List<string> TTs_id = new List<string>();
        //    List<string> TTs_time = new List<string>();

        //    string[] str = new string[2] { "Ts", "Tf" };


        //    for (int i = 0; i < 2; i++)
        //    {
        //        foreach (GAMSVariableRecord rec in t.OutDB.GetVariable(str[i]))
        //        {
        //            //if (rec.Level > 0)
        //            //{
        //                //    Debug.WriteLine("+" + rec.Level.ToString());
        //                if (i == 0)
        //                {
        //                    Ts_Pid.Add(rec.Keys[0]);
        //                    Ts_Tid.Add(rec.Keys[1]);
        //                    Ts_id.Add(rec.Keys[2]);
        //                    Ts_time.Add(rec.Level.ToString());
        //                    //Debug.WriteLine(rec.Keys[0] + "," + rec.Keys[1] + "," + rec.Keys[2] + "," + rec.Level);
        //                }
        //                else
        //                {
        //                    TTs_Pid.Add(rec.Keys[0]);
        //                    TTs_Tid1.Add(rec.Keys[1]);
        //                    TTs_Tid2.Add(rec.Keys[2]);
        //                    TTs_id.Add(rec.Keys[3]);
        //                    TTs_time.Add(rec.Level.ToString());
        //                    //Debug.WriteLine(rec.Keys[0] + "," + rec.Keys[1] + "," + rec.Keys[2] + "," + rec.Keys[3] + "," + rec.Level);
        //                }
        //            //}
        //        }
        //    }

        //    List<string>[] resultKeys = new List<string>[9];
        //    resultKeys[0] = Ts_Pid;
        //    resultKeys[1] = Ts_Tid;
        //    resultKeys[2] = Ts_id;
        //    resultKeys[3] = Ts_time;

        //    resultKeys[4] = TTs_Pid;
        //    resultKeys[5] = TTs_Tid1;
        //    resultKeys[6] = TTs_Tid2;
        //    resultKeys[7] = TTs_id;
        //    resultKeys[8] = TTs_time;

        //    return resultKeys;
        //}

        //#region 从Excel读取数据并运行GAMS model
        //public string Run(string sourceFilePath)
        //{
        //    IWorkbook workbook = null;  //新建IWorkbook对象
        //    FileStream fileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
        //    if (sourceFilePath.IndexOf(".xlsx") > 0) // 2007版本
        //    {
        //        workbook = new XSSFWorkbook(fileStream);  //xlsx数据读入workbook
        //        fileStream.Close();
        //    }
        //    else if (sourceFilePath.IndexOf(".xls") > 0) // 2003版本
        //    {
        //        workbook = new HSSFWorkbook(fileStream);  //xls数据读入workbook
        //        fileStream.Close();
        //    }

        //    int sheetNumber = workbook.NumberOfSheets;
        //    string[] sheetName = new string[sheetNumber];
        //    int[] rowCount = new int[sheetNumber];
        //    int[] cellCount = new int[sheetNumber];
        //    ISheet[] sheet = new ISheet[sheetNumber];
        //    string[] setsTemplate = new string[2] { "i", "j" };
        //    string[] parametersTemplate = new string[4] { "PL", "TL", "Tij", "Tjj" };
        //    string[] setsName = new string[2] { "所有维修员元素点", "所有任务元素点" };

        //    GAMSWorkspace ws = new GAMSWorkspace(workspacePath, null, DebugLevel.Off);
        //    GAMSDatabase db = ws.AddDatabase();

        //    GAMSSet[] gSet = new GAMSSet[sheetNumber];
        //    GAMSParameter[] gPar = new GAMSParameter[sheetNumber];

        //    for (int i = 0; i < sheetNumber; i++)
        //    {
        //        sheetName[i] = workbook.GetSheetName(i);
        //        sheet[i] = workbook.GetSheetAt(i);
        //        rowCount[i] = sheet[i].LastRowNum + 1;
        //        cellCount[i] = sheet[i].GetRow(0).LastCellNum;

        //        //Console.WriteLine("rowCount:" + rowCount[i] + " cellCount:" + cellCount[i] + " i:" + i + sheetName[i]);

        //        if (rowCount[i] == 2)
        //        {
        //            gSet[i] = db.AddSet(setsTemplate[i], 1, setsName[i]);
        //            gPar[i] = db.AddParameter(parametersTemplate[i], 1, sheetName[i]);
        //            for (int j = 0; j < cellCount[i]; j++)
        //            {
        //                gSet[i].AddRecord(sheet[i].GetRow(0).GetCell(j).ToString());
        //                gPar[i].AddRecord(sheet[i].GetRow(0).GetCell(j).ToString()).Value = Convert.ToDouble(sheet[i].GetRow(1).GetCell(j).ToString());
        //            }
        //        }
        //        else if (rowCount[i] > 2)
        //        {
        //            gPar[i] = db.AddParameter(parametersTemplate[i], 2, sheetName[i]);
        //            for (int m = 0; m < cellCount[i] - 1; m++)
        //            {
        //                for (int n = 0; n < rowCount[i] - 1; n++)
        //                {
        //                    //Console.WriteLine("m:" + m + " n:" + n + " i:" + i);
        //                    //Console.WriteLine(sheet[i].GetRow(n + 1).GetCell(0).ToString() + " " + sheet[i].GetRow(0).GetCell(m + 1).ToString() + " " + sheet[i].GetRow(n + 1).GetCell(m + 1).ToString());
        //                    gPar[i].AddRecord(sheet[i].GetRow(n + 1).GetCell(0).ToString(), sheet[i].GetRow(0).GetCell(m + 1).ToString()).Value = Convert.ToDouble(sheet[i].GetRow(n + 1).GetCell(m + 1).ToString());
        //                    Debug.Write("('" + sheet[i].GetRow(n + 1).GetCell(0).ToString() + "','" + sheet[i].GetRow(0).GetCell(m + 1).ToString() + "'," + Convert.ToDouble(sheet[i].GetRow(n + 1).GetCell(m + 1).ToString()) + "),\r\n");
        //                }
        //            }
        //            Debug.Write("\r\n");
        //        }
        //        else
        //        {
        //            string errorInfo = "输入excel文件格式有误";
        //            return errorInfo;
        //        }
        //    }

        //    using (GAMSOptions opt = ws.AddOptions())
        //    {
        //        GAMSJob t10 = ws.AddJobFromString(GetModelText());
        //        opt.Defines.Add("gdxincname", db.Name);
        //        opt.AllModelTypes = "xpress";
        //        t10.Run(opt, db);
        //    }
        //    string targetFilePath = workspacePath + @"\test.xls";
        //    return targetFilePath;
        //}
        //#endregion

        private List<string>[] GetResultData(GAMSJob t)
        {
            List<string> Ts_Pid = new List<string>();
            List<string> Ts_id = new List<string>();
            List<string> Ts_time = new List<string>();

            List<string> Tf_Pid = new List<string>();
            List<string> Tf_id = new List<string>();
            List<string> Tf_time = new List<string>();

            List<string> XS_Pid = new List<string>();
            List<string> XS_Tid = new List<string>();
            List<string> XS_id = new List<string>();


            string[] str = new string[3] { "Ts", "Tf", "XS" };

            foreach (GAMSVariableRecord rec in t.OutDB.GetVariable(str[0]))
            {
                Ts_Pid.Add(rec.Keys[0]);
                Ts_id.Add(rec.Keys[1]);
                Ts_time.Add(rec.Level.ToString());
            }
            foreach (GAMSVariableRecord rec in t.OutDB.GetVariable(str[1]))
            {
                Tf_Pid.Add(rec.Keys[0]);
                Tf_id.Add(rec.Keys[1]);
                Tf_time.Add(rec.Level.ToString());
            }
            foreach (GAMSVariableRecord rec in t.OutDB.GetVariable(str[2]))
            {
                int order = Convert.ToInt32(rec.Keys[2]);
                if ((rec.Level == 1)&&(order>0))
                {
                    XS_Pid.Add(rec.Keys[0]);
                    XS_Tid.Add(rec.Keys[1]);
                    XS_id.Add(rec.Keys[2]);
                }
            }

            List<string>[] resultKeys = new List<string>[9];
            resultKeys[0] = Ts_Pid;
            resultKeys[1] = Ts_id;
            resultKeys[2] = Ts_time;

            resultKeys[3] = Tf_Pid;
            resultKeys[4] = Tf_id;
            resultKeys[5] = Tf_time;

            resultKeys[6] = XS_Pid;
            resultKeys[7] = XS_Tid;
            resultKeys[8] = XS_id;

            return resultKeys;
        }

        /// <summary>
        /// Gets the model text.
        /// </summary>
        /// <returns>System.String.</returns>
        private string GetModelText(int n,string time)
        {
            string n0 = "/0 * " + n + "/";
            string n1 = "/";
            string n2 = "/";
            for (int i = 0; i <= n; i++)
            {
                if (i%2==1)
                {
                    n1 = n1 + i + ",";
                }
                else if (i % 2 == 0)
                {
                    n2 = n2 + i + ",";
                }
            }
            n1 += "/";
            n2 += "/";

            n1 = n1.Replace(",/", "/");
            n2 = n2.Replace(",/", "/");

            string H = null, BL = null, BU = null;
            if (time == "8:30")
            {
                H = "570";
                BL = "210";
                BU = "330";
            }
            else if (time == "7:30")
            {
                H = "630";
                BL = "270";
                BU = "450";
            }

            #region model1:没有紧急任务时运行此模型
            string model1 = @"
Sets
             n         time point         " + n0 + @"            
             n1(n)     odd  point         " + n1 + @"
             n2(n)     even point         " + n2 + @"
             j         all task           
             i         所有维修员元素点    
                     
             j2(j)     非紧急任务           
             j3(j)     必须完成任务           

alias(j, jp, jpp);
alias(j2, j2p);

Parameters   PL(i)       维修人员i拥有级别 PL(i)以上技能                      
             TL(j)       任务j所需技能
             Tij(i,j)      i in j     time
             Tjj(j,jp)     j to jp    time   

Scalar       PN        维修人员总数       /5/
             H         总调度时间        /" + H + @"/
             BL                          /" + BL + @"/
             BU                          /" + BU + @"/ 
             Nmax;
Nmax=card(n);           
            



$if not set gdxincname $abort 'no include file name for data file provided'
$gdxin %gdxincname%
$load i j  j2 j3 PL TL Tij  Tjj
$gdxin

Variables
             XS(i,j,n)         n时间段维修人员i在做j任务
             X(i,j,jp,n)       n时间段维修人员i从j到j'任务
             Ts(i,n)
             Tf(i,n)
             XS2(i,j,n2)
             XS1(i,j,n2)
             XS21(i,j,n2)
             XS11(i,j,n2)
             AXS(i,j,n)
             BX(i,j,jp,n)
             BXS(i,j,n)
             cost;

Binary Variables    XS, X;
Positive  Variables Ts,Tf;

Equations

cons_1(j)                       所有任务都必须完成且只完成一次
cons_2(i, n)                    同一时间同一个人最多只能做一个任务
*cons_3(i, j, n)                 修井人员满足技能需求
cons_4(i, j, n)
cons_5(i, jp, n)
cons_6(i,n1)
cons_7(i,n2)
cons_6_1(i,n1)
cons_7_1(i,n2)
cons_8(i)
cons_9(j3)
cons_15(i,n)
cons_15_1(i,n)
cons_15_2(i,n)
cons_AX_1(i,j,n)
cons_AX_2(i,j,n)
cons_AX_3(i,j,n)
cons_21(i,n2)
cons_22(i)
cons_22_1(i,n2)
cons_22_2(i,n2)
cons_22_3(i,n2)
cons_22_4(i,n2)
cons_22_5(i,n2)
cons_25(i)
obj;

cons_1(j)$(ord(j) ne 1 and ord(j) ne 2).. sum((i, n2), XS(i, j, n2))=l=1;
cons_2(i, n).. sum(j, XS(i, j, n)) =l= 1;
*cons_3(i, j, n)..  XS(i, j, n) * (PL(i) - TL(j)) =l= 0;
cons_4(i, j, n)$(ord(n) le Nmax-1)..  XS(i, j, n)-AXS(i,j,n)=e=sum(jp, X(i, j, jp, n+1));
cons_5(i, jp, n)$(ord(n) ge 2 and ord(jp) ne 1)..   sum(j, X(i, j, jp, n-1))=e=XS(i, jp, n)-AXS(i,jp,n-1);
cons_6(i,n1)$(ord(n1) ge 1)..   Tf(i,n1)-Ts(i,n1)-sum((j,jp),Tjj(j,jp)*X(i,j,jp,n1))=l=0;
cons_6_1(i,n1)$(ord(n1) ge 1)..   Tf(i,n1)-Ts(i,n1)-sum((j,jp),Tjj(j,jp)*X(i,j,jp,n1))=g=0;
cons_7(i,n2)$(ord(n2) ge 1 )..        Tf(i,n2)-Ts(i,n2)-sum(j,Tij(i,j)*XS(i,j,n2))=l=0;
cons_7_1(i,n2)$(ord(n2) ge 1 )..        Tf(i,n2)-Ts(i,n2)-sum(j,Tij(i,j)*XS(i,j,n2))=g=0;
cons_8(i).. sum((j,n),X(i,j,'AS00-0',n))=l=1;
cons_9(j3).. sum((i, n2), XS(i, j3, n2))=e=1;
cons_15(i,n)$(ord(n) ge 2).. Tf(i,n-1)=l=Ts(i,n);
cons_15_1(i,n)..  Ts(i,n)=l=H;
cons_15_2(i,n)..  Tf(i,n)=l=H;
cons_25(i)..  sum(n2$(ord(n2) ge 3),XS(i,'AS0-0',n2))=g=1;
cons_22(i)..       BL=g=sum(n2,XS11(i,'AS0-0',n2));
cons_22_1(i,n2)..  XS11(i,'AS0-0',n2)=l=XS(i,'AS0-0',n2)*H  ;
cons_22_2(i,n2)..  XS21(i,'AS0-0',n2)=l=(1-XS(i,'AS0-0',n2))*H  ;
cons_22_3(i,n2)..  XS21(i,'AS0-0',n2)=g=0  ;
cons_22_4(i,n2)..  XS11(i,'AS0-0',n2)=g=0  ;
cons_22_5(i,n2)..  XS21(i,'AS0-0',n2)+ XS11(i,'AS0-0',n2)=e=Ts(i,n2);
cons_21(i,n2)..       XS(i,'AS0-0',n2)*BU=l=Tf(i,n2);
cons_AX_1(i,j,n).. AXS(i,j,n)=l=XS(i,j,n);
cons_AX_2(i,j,n)$(ord(n) le Nmax-1).. AXS(i,j,n)=l=XS(i,j,n+1);
cons_AX_3(i,j,n)$(ord(n) le Nmax-1).. AXS(i,j,n)=g=XS(i,j,n)+XS(i,j,n+1)-1;

obj.. cost =e=(-1)*sum((i,j2,n2),XS(i,j2,n2))+ 0.001*(sum((i,j2,j2p,n1),Tjj(j2,j2p)*X(i,j2,j2p,n1)));

Model test /all/;

*设置初值和终值
XS.fx(i, 'AS00-0', '0')=1 ;
XS.fx(i, j, n1)=0;
*XS.fx(i, 'AS00-0', '18')=1 ;
XS.fx(i, 'AS00-0', n)$(ord(n) eq Nmax)=1;
X.fx(i,j,j,n)=0;
X.fx(i,j,jp,n2)=0;
*TS.l(i,n)=0;
*Tf.l(i,n)=0;
*TS.up(i,n)=500;
*Tf.up(i,n)=500;
option limrow=1000;
option threads=4;
option mip=cplex;
*option minlp=bonmin;
option decimals=0;
option reslim=10000;
option optcr=0.15;
Solve test minimizing cost using mip;

Display  XS.l, X.l,Ts.l,Tf.l;

Display cost.l;
Execute_Unload 'filename.gdx', Ts,Tf;
Execute 'Gdxxrw.exe filename.gdx O = test1.xls var = Ts = Excel spreadsheet!';
Execute 'Gdxxrw.exe filename.gdx O = test2.xls var = Tf = Excel spreadsheet!';
";
            #endregion

            #region model2：存在紧急任务时运行此模型
            string model2 = @"
Sets
             n         time point         " + n0 + @"
             n1(n)     odd  point         " + n1 + @"
             n2(n)     even point         " + n2 + @"
             j         all task           
             i         所有维修员元素点    
             j1(j)     紧急任务           
             j2(j)     非紧急任务           
             j3(j)     必须完成任务           

alias(j, jp, jpp);


Parameters   PL(i)       维修人员i拥有级别 PL(i)以上技能                      
             TL(j)       任务j所需技能
             Tij(i,j)      i in j     time
             Tjj(j,jp)     j to jp    time   

Scalar       PN        维修人员总数       /5/
             H         总调度时间        /" + H + @"/
             BL                          /" + BL + @"/
             BU                          /" + BU + @"/ 
             Nmax;
Nmax=card(n);           
            



$if not set gdxincname $abort 'no include file name for data file provided'
$gdxin %gdxincname%
$load i j j1 j2 j3 PL TL Tij  Tjj
$gdxin

Variables
             XS(i,j,n)         n时间段维修人员i在做j任务
             X(i,j,jp,n)       n时间段维修人员i从j到j'任务
             Ts(i,n)
             Tf(i,n)
             XS2(i,j,n2)
             XS1(i,j,n2)
             XS21(i,j,n2)
             XS11(i,j,n2)
             AXS(i,j,n)
             BX(i,j,jp,n)
             BXS(i,j,n)
             cost;

Binary Variables    XS, X;
Positive  Variables Ts,Tf;

Equations

cons_1(j)                       所有任务都必须完成且只完成一次
cons_2(i, n)                    同一时间同一个人最多只能做一个任务
*cons_3(i, j, n)                 修井人员满足技能需求
cons_4(i, j, n)
cons_5(i, jp, n)
cons_6(i,n1)
cons_7(i,n2)
cons_6_1(i,n1)
cons_7_1(i,n2)
cons_8(i)
cons_9(j3)
cons_15(i,n)
cons_15_1(i,n)
cons_15_2(i,n)
cons_AX_1(i,j,n)
cons_AX_2(i,j,n)
cons_AX_3(i,j,n)
cons_21(i,n2)
cons_22(i)
cons_22_1(i,n2)
cons_22_2(i,n2)
cons_22_3(i,n2)
cons_22_4(i,n2)
cons_22_5(i,n2)
cons_25(i)
obj;

cons_1(j)$(ord(j) ne 1 and ord(j) ne 2).. sum((i, n2), XS(i, j, n2))=l=1;
cons_2(i, n).. sum(j, XS(i, j, n)) =l= 1;
*cons_3(i, j, n)..  XS(i, j, n) * (PL(i) - TL(j)) =l= 0;
cons_4(i, j, n)$(ord(n) le Nmax-1)..  XS(i, j, n)-AXS(i,j,n)=e=sum(jp, X(i, j, jp, n+1));
cons_5(i, jp, n)$(ord(n) ge 2 and ord(jp) ne 1)..   sum(j, X(i, j, jp, n-1))=e=XS(i, jp, n)-AXS(i,jp,n-1);
cons_6(i,n1)$(ord(n1) ge 1)..   Tf(i,n1)-Ts(i,n1)-sum((j,jp),Tjj(j,jp)*X(i,j,jp,n1))=l=0;
cons_6_1(i,n1)$(ord(n1) ge 1)..   Tf(i,n1)-Ts(i,n1)-sum((j,jp),Tjj(j,jp)*X(i,j,jp,n1))=g=0;
cons_7(i,n2)$(ord(n2) ge 1 )..        Tf(i,n2)-Ts(i,n2)-sum(j,Tij(i,j)*XS(i,j,n2))=l=0;
cons_7_1(i,n2)$(ord(n2) ge 1 )..        Tf(i,n2)-Ts(i,n2)-sum(j,Tij(i,j)*XS(i,j,n2))=g=0;
cons_8(i).. sum((j,n),X(i,j,'AS00-0',n))=l=1;
cons_9(j3).. sum((i, n2), XS(i, j3, n2))=e=1;
cons_15(i,n)$(ord(n) ge 2).. Tf(i,n-1)=l=Ts(i,n);
cons_15_1(i,n)..  Ts(i,n)=l=H;
cons_15_2(i,n)..  Tf(i,n)=l=H;
cons_25(i)..  sum(n2$(ord(n2) ge 3),XS(i,'AS0-0',n2))=g=1;
cons_22(i)..       BL=g=sum(n2,XS11(i,'AS0-0',n2));
cons_22_1(i,n2)..  XS11(i,'AS0-0',n2)=l=XS(i,'AS0-0',n2)*H  ;
cons_22_2(i,n2)..  XS21(i,'AS0-0',n2)=l=(1-XS(i,'AS0-0',n2))*H  ;
cons_22_3(i,n2)..  XS21(i,'AS0-0',n2)=g=0  ;
cons_22_4(i,n2)..  XS11(i,'AS0-0',n2)=g=0  ;
cons_22_5(i,n2)..  XS21(i,'AS0-0',n2)+ XS11(i,'AS0-0',n2)=e=Ts(i,n2);
cons_21(i,n2)..       XS(i,'AS0-0',n2)*BU=l=Tf(i,n2);
cons_AX_1(i,j,n).. AXS(i,j,n)=l=XS(i,j,n);
cons_AX_2(i,j,n)$(ord(n) le Nmax-1).. AXS(i,j,n)=l=XS(i,j,n+1);
cons_AX_3(i,j,n)$(ord(n) le Nmax-1).. AXS(i,j,n)=g=XS(i,j,n)+XS(i,j,n+1)-1;

obj.. cost =e=(-1)*sum((i,j2,n2),XS(i,j2,n2))+sum((i,j1,n),ord(n)*XS(i,j1,n))+(sum((i,n2),Ts(i,n2)))*0.001;

Model test /all/;

*设置初值和终值
XS.fx(i, 'AS00-0', '0')=1 ;
XS.fx(i, j, n1)=0;
*XS.fx(i, 'AS00-0', '18')=1 ;
XS.fx(i, 'AS00-0', n)$(ord(n) eq Nmax)=1;
X.fx(i,j,j,n)=0;
X.fx(i,j,jp,n2)=0;
*TS.l(i,n)=0;
*Tf.l(i,n)=0;
*TS.up(i,n)=500;
*Tf.up(i,n)=500;
option limrow=1000;
option threads=4;
option mip=cplex;
*option minlp=bonmin;
option decimals=0;
option reslim=10000;
option optcr=0.15;
Solve test minimizing cost using mip;

Display  XS.l, X.l,Ts.l,Tf.l;

Display cost.l;
Execute_Unload 'filename.gdx', Ts,Tf;
Execute 'Gdxxrw.exe filename.gdx O = test1.xls var = Ts = Excel spreadsheet!';
Execute 'Gdxxrw.exe filename.gdx O = test2.xls var = Tf = Excel spreadsheet!';
";
            #endregion

            //TODO:
            if (!hasPriorityOne)
            {
                return model1;
            }
            else
            {
                return model2;
            }
        }
    }
}
