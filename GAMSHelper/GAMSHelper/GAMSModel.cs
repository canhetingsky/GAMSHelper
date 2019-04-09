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
        public int model_n = 0;
        public int Model_N
        {
            get { return model_n; }
            set { model_n = value; }
        }

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

            string[] setsTemplate = new string[2] { "i", "j" };
            string[] setsName = new string[2] { "所有维修员元素点", "所有任务元素点" };
            string[] parametersTemplate = new string[4] { "PL", "TL", "Tij", "Tjj" };
            string[] parametersName = new string[4] { "PLi", "TLj", "Tij", "Tjj" };

            GAMSWorkspace ws;
            //if (Environment.GetCommandLineArgs().Length > 1)
            //    ws = new GAMSWorkspace(systemDirectory: Environment.GetCommandLineArgs()[1]);
            //else
                ws = new GAMSWorkspace(workspacePath, null, DebugLevel.Off);
            GAMSDatabase db = ws.AddDatabase();
            GAMSSet[] gSet = new GAMSSet[sheetNumber];
            GAMSParameter[] gPar = new GAMSParameter[sheetNumber];

            for (int i = 0; i < 2; i++) //前两个表设置
            {
                gSet[i] = db.AddSet(setsTemplate[i], 1, setsName[i]);
                gPar[i] = db.AddParameter(parametersTemplate[i], 1, parametersName[i]);
                for (int j = 0; j < listData[2*i].Count; j++)
                {
                    gSet[i].AddRecord(listData[2 * i][j]);
                    gPar[i].AddRecord(listData[2 * i][j]).Value = Convert.ToDouble(listData[2 * i + 1][j]);
                }
            }
            for (int i = 2; i < 4; i++)
			{
                gPar[i] = db.AddParameter(parametersTemplate[i], 2, parametersName[i]);
                for (int j = 0; j < listData[3*i-2].Count; j++)
                {
                    gPar[i].AddRecord(listData[3*i-2][j], listData[3*i-1][j]).Value = Convert.ToDouble(listData[3*i][j]);
                }
			}

            GAMSJob t = ws.AddJobFromString(GetModelText(model_n));
            using (GAMSOptions opt = ws.AddOptions())
            {
                opt.Defines.Add("gdxincname", db.Name);
                opt.AllModelTypes = "xpress";
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
        private List<string>[] GetResultData(GAMSJob t)
        {
            List<string> Ts_Pid = new List<string>();
            List<string> Ts_Tid = new List<string>();
            List<string> Ts_id = new List<string>();
            List<string> Ts_time = new List<string>();

            List<string> TTs_Pid = new List<string>();
            List<string> TTs_Tid1 = new List<string>();
            List<string> TTs_Tid2 = new List<string>();
            List<string> TTs_id = new List<string>();
            List<string> TTs_time = new List<string>();

            string[] str = new string[2] { "Ts", "TTs" };


            for (int i = 0; i < 2; i++)
            {
                foreach (GAMSVariableRecord rec in t.OutDB.GetVariable(str[i]))
                {
                    //if (rec.Level > 0)
                    //{
                        //    Debug.WriteLine("+" + rec.Level.ToString());
                        if (i == 0)
                        {
                            Ts_Pid.Add(rec.Keys[0]);
                            Ts_Tid.Add(rec.Keys[1]);
                            Ts_id.Add(rec.Keys[2]);
                            Ts_time.Add(rec.Level.ToString());
                            //Debug.WriteLine(rec.Keys[0] + "," + rec.Keys[1] + "," + rec.Keys[2] + "," + rec.Level);
                        }
                        else
                        {
                            TTs_Pid.Add(rec.Keys[0]);
                            TTs_Tid1.Add(rec.Keys[1]);
                            TTs_Tid2.Add(rec.Keys[2]);
                            TTs_id.Add(rec.Keys[3]);
                            TTs_time.Add(rec.Level.ToString());
                            //Debug.WriteLine(rec.Keys[0] + "," + rec.Keys[1] + "," + rec.Keys[2] + "," + rec.Keys[3] + "," + rec.Level);
                        }
                    //}
                }
            }

            List<string>[] resultKeys = new List<string>[9];
            resultKeys[0] = Ts_Pid;
            resultKeys[1] = Ts_Tid;
            resultKeys[2] = Ts_id;
            resultKeys[3] = Ts_time;

            resultKeys[4] = TTs_Pid;
            resultKeys[5] = TTs_Tid1;
            resultKeys[6] = TTs_Tid2;
            resultKeys[7] = TTs_id;
            resultKeys[8] = TTs_time;

            return resultKeys;
        }

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

        /// <summary>
        /// Gets the model text.
        /// </summary>
        /// <returns>System.String.</returns>
        private string GetModelText(int n)
        {
            string n0 = "/0 * " + n + "/";
            string n1 = "/";
            string n2 = "/";
            for (int i = 0; i < n; i++)
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

            string model = @"

Sets
             n         时间点         " + n0 + @"
             n1(n)     奇数           " + n1 + @"
             n2(n)     偶数           " + n2 + @"     
             i         所有维修员元素点 
             j         所有任务元素点

alias(j, jp, jpp);




Parameters   PL(i)          维修人员i拥有级别 PL(i)以上技能
             TL(j)          任务j所需技能        
             Tij(i,j)       time         
             Tjj(j,jp)      time

Scalar       PN        维修人员总数       /5/
             H         总调度时间        /480/
             Nmax;
Nmax=card(n); 

$if not set gdxincname $abort 'no include file name for data file provided'
$gdxin %gdxincname%
$load i j PL TL Tij  Tjj
$gdxin


Variables
             XS(i,j,n)         n时间段维修人员i在做j任务
             X(i,j,jp,n)       n时间段维修人员i从j到j'任务
             Ts(i,j,n)
             Tf(i,j,n)
             TTs(i,j,jp,n)
             TTf(i,j,jp,n)
             AXS(i,j,n)
             BX(i,j,jp,n)
             BXS(i,j,n)
             cost;

Binary Variables    XS, X;
Positive  Variables Ts,Tf,TTs,TTf;

Equations

cons_1(j)                       所有任务都必须完成且只完成一次
cons_2(i, n)                    同一时间同一个人最多只能做一个任务
cons_3(i, j, n)                 修井人员满足技能需求
cons_4(i, j, n)
cons_5(i, jp, n)
cons_6(i,j,jp,n1)
cons_7(i,j,n2)
cons_6_1(i,j,jp,n1)
cons_7_1(i,j,n2)
cons_8(i,jp)
$ontext
cons_9(i,j,n)
cons_10(i,j,n)
cons_11(i,j,jp,n)
cons_12(i,j,jp,n)
*cons_13(i,j,n2)
*cons_14(i,j,jp,n1)
$offtext
cons_15(i,j,jp,n)
cons_16(i,j,jp,n)
*cons_17(i,j,jp,n1)
*cons_18(i,j,n2)
cons_AX_1(i,j,n)
cons_AX_2(i,j,n)
cons_AX_3(i,j,n)
cons_BX_1(i,j,jp,n)
cons_BX_2(i,j,jp,n)
cons_BX_3(i,j,jp,n)
cons_BXS_1(i,j,n)
cons_BXS_2(i,j,n)
cons_BXS_3(i,j,n)
cons_19(j,n)
obj;

*（1）
*cons_1(jp)$(ord(jp) ne 1).. sum((i, j, t), X(i, j, jp, t))=e=1;
cons_1(j).. sum((i, n2), XS(i, j, n2))=g=1;
*（2）
cons_2(i, n).. sum(j, XS(i, j, n)) =l= 1;
*（3）
cons_3(i, j, n)..  XS(i, j, n) * (PL(i) - TL(j)) =l= 0;

*cons_4(i, j, n)$(ord(n) le Nmax-1)..  XS(i, j, n)*(XS(i, j, n)-XS(i, j, n+1))=e=sum(jp, X(i, j, jp, n+1));
cons_4(i, j, n)$(ord(n) le Nmax-1)..  XS(i, j, n)-AXS(i,j,n)=e=sum(jp, X(i, j, jp, n+1));
*cons_5(i, jp, n)$(ord(n) ge 2 and ord(jp) ne 1)..   sum(j, X(i, j, jp, n-1))=e=XS(i, jp, n)*(XS(i, jp, n)-XS(i, jp, n-1));
cons_5(i, jp, n)$(ord(n) ge 2 and ord(jp) ne 1)..   sum(j, X(i, j, jp, n-1))=e=XS(i, jp, n)-AXS(i,jp,n-1);

cons_6(i,j,jp,n1)$(ord(n1) ge 1)..    0=l=TTf(i,j,jp,n1)-TTs(i,j,jp,n1)-Tjj(j,jp)*X(i,j,jp,n1) ;
cons_6_1(i,j,jp,n1)$(ord(n1) ge 1)..  TTf(i,j,jp,n1)-TTs(i,j,jp,n1)-Tjj(j,jp)*X(i,j,jp,n1)=l=BX(i,j,jp,n1);
cons_7(i,j,n2)$(ord(n2) ge 1)..       0=l=Tf(i,j,n2)-Ts(i,j,n2)-Tij(i,j)*XS(i,j,n2);
cons_7_1(i,j,n2)$(ord(n2) ge 1)..     Tf(i,j,n2)-Ts(i,j,n2)-Tij(i,j)*XS(i,j,n2)=l=BXS(i,j,n2);
*cons_8(i).. sum((j,n),X(i,j,'TS0000',n))=l=1;
cons_8(i,jp)$(ord(jp) eq 1).. sum((j,n),X(i,j,jp,n))=l=1;

$ontext
cons_9(i,j,n).. Ts(i,j,n)=g=0;
cons_10(i,j,n).. Tf(i,j,n)=g=0;
cons_11(i,j,jp,n)..  TTs(i,j,jp,n)=g=0;
cons_12(i,j,jp,n)..  TTf(i,j,jp,n)=g=0;
*cons_13(i,j,n2)$(ord(n2) ge 2)..  Tf(i,j,n2)=g=Tf(i,j,n2-2);
*cons_14(i,j,jp,n1)$(ord(n1) ge 2).. TTf(i,j,jp,n1)=g=TTf(i,j,jp,n1-2);
$offtext
cons_15(i,j,jp,n)..Tf(i,j,n-1)=l=TTs(i,j,jp,n)+H*(1-X(i,j,jp,n));
cons_16(i,j,jp,n)..Ts(i,jp,n+1)=g=TTf(i,j,jp,n)-H*(1-X(i,j,jp,n));
*cons_17(i,j,jp,n1).. (1-X(i,j,jp,n1))*TTf(i,j,jp,n1)=l=0;
*cons_18(i,j,n2)..(1-XS(i,j,n2))*Tf(i,j,n2)=l=0;

cons_AX_1(i,j,n).. AXS(i,j,n)=l=XS(i,j,n);
cons_AX_2(i,j,n)$(ord(n) le Nmax-1).. AXS(i,j,n)=l=XS(i,j,n+1);
cons_AX_3(i,j,n)$(ord(n) le Nmax-1).. AXS(i,j,n)=g=XS(i,j,n)+XS(i,j,n+1)-1;
cons_BX_1(i,j,jp,n).. BX(i,j,jp,n)=l=X(i,j,jp,n);
cons_BX_2(i,j,jp,n).. BX(i,j,jp,n)=l=1-X(i,j,jp,n);
cons_BX_3(i,j,jp,n).. BX(i,j,jp,n)=g=0;
cons_BXS_1(i,j,n).. BXS(i,j,n)=l=XS(i,j,n);
cons_BXS_2(i,j,n).. BXS(i,j,n)=l=1-XS(i,j,n);
cons_BXS_3(i,j,n).. BXS(i,j,n)=g=0;
cons_19(j,n)$(ord(j) ne 1).. sum(i,XS(i,j,n))=l=1;


obj.. cost =e= sum((i, j, n2),Tij(i,j)* XS(i, j, n2))+sum((i, j, jp, n1), Tjj(j,jp)*X(i, j, jp, n1));
*obj.. cost =e= sum((i, j, n2)$(ord(j) ne 1), Tf(i,j,n2)-Ts(i,j,n2))+sum((i, j, jp, n1), TTf(i,j,jp,n1)-TTs(i,j,jp,n1));

Model test /all/;

*设置初值和终值
XS.fx(i, j, '0')$(ord(j) eq 1)=1 ;
XS.fx(i, j, n1)=0;
XS.fx(i, j, n)$(ord(j) eq 1 and ord(n) eq Nmax-1)=1 ;
X.fx(i,j,j,n)=0;
X.fx(i,j,jp,n2)=0;


option limrow=1000;
option threads=40;
option mip=cplex;
option reslim=20000;

Solve test minimizing cost using mip;

Display  XS.l, X.l,Ts.l,Tf.l,TTs.l,TTf.l;

Display cost.l;
Execute_Unload 'filename.gdx', Ts,TTs;
Execute 'Gdxxrw.exe filename.gdx O = test1.xls var = Ts = Excel spreadsheet!';
Execute 'Gdxxrw.exe filename.gdx O = test2.xls var = TTs = Excel spreadsheet!';
";
            return model;
        }
    }
}
