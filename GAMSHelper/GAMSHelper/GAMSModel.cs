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
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        public string conn;

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
            //以下参数来源于GAMS模型
            string[] setsTemplate = new string[6] { "i", "j","j1","j2","j3","j4" };
            string[] setsName = new string[6] { "所有维修员元素点", "所有任务元素点","紧急任务","重要任务", "一般任务", "必须完成" };
            string[] parametersTemplate = new string[5] { "PL", "TL", "Tij_lo", "Tij_up", "Tjj" };
            string[] parametersName = new string[5] { "PLi", "TLj", "Tij_lo", "Tij_up", "Tjj" };

            GAMSWorkspace ws;
            ws = new GAMSWorkspace(workspacePath, null, DebugLevel.Off);
            GAMSDatabase db = ws.AddDatabase();
            GAMSSet[] gSet = new GAMSSet[setsTemplate.Length];
            GAMSParameter[] gPar = new GAMSParameter[parametersTemplate.Length];

            //设置Sets变量
            for (int i = 0; i < setsTemplate.Length; i++)
            {
                gSet[i] = db.AddSet(setsTemplate[i], 1, setsName[i]);
            }
            //设置Parameters变量
            for (int i = 0; i < parametersTemplate.Length; i++)
            {
                int dimension = i < 2 ? 1 : 2;  //前两个Parameters是 1 维，后几个是 2 维
                gPar[i] = db.AddParameter(parametersTemplate[i], dimension, parametersName[i]);
            }

            //jSetRecord临时变量，j 的记录值
            List<string> jSetRecord = new List<string>();   //j 的记录值，后面其他 Parameters 会使用
            for (int i = 0; i < listData[2].Count; i++)
            {
                int priority = Convert.ToInt32(listData[10][i]);
                string taskId = listData[2][i];
                if (priority != 3)  //优先级为0、1、2的任务
                {
                    jSetRecord.Add(taskId);
                }
                else    //优先级为3
                {
                    string name = taskId.Split(new char[] { '-' })[0]; //提取出来井组
                    int index = listData[2].IndexOf(name + "-2");
                    if (index == -1)    //没2有3的情况
                    {
                        jSetRecord.Add(taskId);
                    }
                }
            }

            //添加 i、PLi 的记录值
            for (int i = 0; i < listData[0].Count; i++)
            {
                gSet[0].AddRecord(listData[0][i]);  //i
                gPar[0].AddRecord(listData[0][i]).Value = Convert.ToDouble(listData[1][i]); //PLi
            }

            //添加 j、j1、j2、j3、j4 的记录值
            //添加 TLj 的记录值
            foreach (string item in jSetRecord)
            {
                //j
                gSet[1].AddRecord(item);  //j

                //j1、j2、j3、j4
                int priority = Convert.ToInt32(item.Split(new char[] { '-' })[1]);  //提取出优先级
                switch (priority)
                {
                    case 1:
                        hasPriorityOne = true;
                        gSet[2].AddRecord(item);    //j1
                        gSet[5].AddRecord(item);    //j4，优先级为1的
                        break;
                    case 2:
                        gSet[3].AddRecord(item);    //j2
                        gSet[5].AddRecord(item);    //j4，优先级为2的
                        break;
                    case 3:
                        gSet[4].AddRecord(item);  //j3
                        break;
                    default:
                        break;
                }

                //TLj
                if (priority != 2)  //优先级为0、1、3的任务
                {
                    string taskId = item;
                    int index = listData[2].IndexOf(item);
                    string skillLevel = listData[3][index];
                    gPar[1].AddRecord(taskId).Value = Convert.ToDouble(skillLevel); //TLj
                }
                else    //优先级为2的任务
                {
                    string taskId = item;
                    string name = taskId.Split(new char[] { '-' })[0]; //提取出来井组
                    int index1 = listData[2].IndexOf(name + "-2");
                    int index2 = listData[2].IndexOf(name + "-3");
                    if (index2 == -1)   //有2没3
                    {
                        string skillLevel = listData[3][index1];
                        gPar[1].AddRecord(taskId).Value = Convert.ToDouble(skillLevel);
                    }
                    else    //有2有3
                    {
                        int skillLevel = Math.Max(Convert.ToInt32(listData[3][index1]), Convert.ToInt32(listData[3][index2]));
                        gPar[1].AddRecord(taskId).Value = Convert.ToDouble(skillLevel);
                    }
                }
            }

            //添加 Tij_lo 的部分记录值（优先级为0、1、2）、添加 Tij_up 的部分记录值（优先级为0、1）
            for (int i = 0; i < listData[4].Count; i++)
            {
                string taskId = listData[5][i];
                string priority = taskId.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries)[1];   //分隔类似AS1-1，得到优先级
                if (Convert.ToInt32(priority) < 3 ) //0、1、2
                {
                    gPar[2].AddRecord(listData[4][i], listData[5][i]).Value = Convert.ToDouble(listData[6][i]); //Tij_lo 
                }
                if (Convert.ToInt32(priority) < 2) //0、1
                {
                    gPar[3].AddRecord(listData[4][i], listData[5][i]).Value = Convert.ToDouble(listData[6][i]); //Tij_up
                }
            }

            string[] excludePoint = new string[] { "AS0", "AS00" }; //排除的井组
            //添加 Tij_lo 的部分记录值（优先级为2，将原优先级2、3的合并）
            //添加 Tij_up 的部分记录值（优先级为2的），需要将优先级为2、3的进行合并
            for (int i = 0; i < listData[0].Count; i++) //按人员进行遍历
            {
                string personId = listData[0][i];
                for (int j = 0; j < listData[11].Count; j++)    //按井组号遍历
                {
                    string pointName = listData[11][j]; //井组
                    bool exist = ((IList)excludePoint).Contains(pointName);
                    if (!exist)
                    {
                        //人员-任务， 两列不会重复
                        int index1 = listData[5].IndexOf(pointName + "-2"); //返回指定元素的第一个匹配项的索引，或者如果此列表中不包含该元素返回-1
                        int index2 = listData[5].IndexOf(pointName + "-3");
                    
                        if ((index1 == -1) && (index2 != -1))
                        {
                            gPar[2].AddRecord(personId, pointName + "-3").Value = 10; //Tij_lo
                            
                            double spendTime = Convert.ToDouble(listData[6][index2]);
                            gPar[3].AddRecord(personId, pointName + "-3").Value = spendTime;    //Tij_up
                        }
                        else if ((index1 != -1) && (index2 == -1))
                        {
                            double spendTime = Convert.ToDouble(listData[6][index1]);
                            gPar[3].AddRecord(personId, pointName + "-2").Value = spendTime;    //Tij_up
                        }
                        else if ((index1 != -1) && (index2 != -1))
                        {
                            double spendTime = Convert.ToDouble(listData[6][index1]) + Convert.ToDouble(listData[6][index2]);
                            gPar[3].AddRecord(personId, pointName + "-2").Value = spendTime;    //Tij_up
                        }
                    }
                }
            }
            //Tjj设置Parameters的记录值
            foreach (string item1 in jSetRecord)
            {
                string str1 = item1.Split(new char[] { '-' })[0];   //得到井组
                foreach (string item2 in jSetRecord)
                {
                    string str2 = item2.Split(new char[] { '-' })[0];   //得到井组
                    if (str2 == str1)
                    {
                        gPar[4].AddRecord(item1, item2).Value = Convert.ToDouble(0);    //Tjj，同一个井组，花费时间为0
                    }
                    else
                    {
                        SqlConnection myconnect = new SqlConnection(conn);
                        myconnect.Open();
                        string command1 = String.Format("select SPEND_TIME from IMS_PATROL_POINT_SPEND_TIME where FROM_POINT_NAME='{0}' and TO_POINT_NAME='{1}';", str1, str2);
                        SqlCommand mycomm1 = new SqlCommand(command1, myconnect);

                        string spendTime = mycomm1.ExecuteScalar().ToString();
                        gPar[4].AddRecord(item1, item2).Value = Convert.ToDouble(spendTime);    //Tjj

                        myconnect.Close();
                    }
                }
            }
            //模型判断
            int modelType = 0;
            if (hasPriorityOne)    //存在紧急任务
            {
                int j2_count = gSet[3].NumberRecords;
                if (j2_count>0)
                {
                    int j3_count = gSet[4].NumberRecords;
                    modelType = j3_count > 0 ? 2 : 3;
                }
                else
                {
                    modelType = 6;
                }
            }
            else
            {
                int j2_count = gSet[3].NumberRecords;
                int j3_count = gSet[4].NumberRecords;
                if ((j2_count > 0) && (j3_count > 0))
                {
                    modelType = 1;
                }
                else if ((j2_count > 0) && (j3_count == 0))
                {
                    modelType = 4;
                }
                else if ((j2_count == 0) && (j3_count > 0))
                {
                    modelType = 5;
                }
            }

            GAMSJob t = ws.AddJobFromString(GetModelText(model_n, work_start_time, modelType));
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
        private string GetModelText(int n,string time,int type)
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
                H = "/570/";
                BL = "/210/";
                BU = "/330/";
            }
            else if (time == "7:30")
            {
                H = "630";
                BL = "270";
                BU = "450";
            }

            #region model1
            string model1 = @"
Sets
             n         time point            " + n0 + @"
             n1(n)     odd  point            " + n1 + @" 
             n2(n)     even point            " + n2 + @" 
             j         all task            
             i         所有维修员元素点    
             
             j2(j)     重要任务     
             j3(j)     一般任务        
             j4(j)     必须完成 


alias(j, jp, jpp);
alias(j2, j2p);

Scalar       PN        维修人员总数       /5/
             H         总调度时间        " + H + @"
             BL                          " + BL + @"
             BU                          " + BU + @"
             Nmax;
Nmax=card(n);

Parameters   PL(i)          维修人员i拥有级别 PL(i)以上技能
             TL(j)          任务j所需技能
             Tij_lo(i,j)           i in j     time        
             Tij_up(i,j)           i in j     time
             Tjj(j,jp)     j to jp    time
;



$if not set gdxincname $abort 'no include file name for data file provided'
$gdxin %gdxincname%
$load i j  j2  j3 j4 PL TL Tjj  Tij_lo  Tij_up  
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

cons_1(j)
cons_2(i, n)                    同一时间同一个人最多只能做一个任务
*cons_3(i, j, n)                 修井人员满足技能需求
cons_4(i, j, n)
cons_5(i, jp, n)
cons_6(i,n1)
cons_7(i,n2)
cons_6_1(i,n1)
cons_7_1(i,n2)
cons_8(i)
cons_15(i,n)
cons_15_1(i,n)
cons_15_2(i,n)
cons_AX_1(i,j,n)
cons_AX_2(i,j,n)
cons_AX_3(i,j,n)
cons_21(i,n2)
cons_22(i)
cons_22_1(i,j,n2)
cons_22_2(i,j,n2)
cons_22_3(i,j,n2)
cons_22_4(i,j,n2)
cons_22_5(i,j,n2)
cons_25(i)
cons_9(j4)
cons_8_1(i)
obj;
cons_9(j4).. sum((i, n2), XS(i, j4, n2))=e=1;
cons_1(j)$(ord(j) ne 1 and ord(j) ne 2).. sum((i, n2), XS(i, j, n2))=l=1;
cons_2(i, n).. sum(j, XS(i, j, n)) =l= 1;
*cons_3(i, j, n)..  XS(i, j, n) * (PL(i) - TL(j)) =l= 0;
cons_4(i, j, n)$(ord(n) le Nmax-1)..  XS(i, j, n)-AXS(i,j,n)=e=sum(jp, X(i, j, jp, n+1));
cons_5(i, jp, n)$(ord(n) ge 2 and ord(jp) ne 1)..   sum(j, X(i, j, jp, n-1))=e=XS(i, jp, n)-AXS(i,jp,n-1);
cons_6(i,n1)$(ord(n1) ge 1)..   Tf(i,n1)-Ts(i,n1)-sum((j,jp),Tjj(j,jp)*X(i,j,jp,n1))=l=0;
cons_6_1(i,n1)$(ord(n1) ge 1)..   Tf(i,n1)-Ts(i,n1)-sum((j,jp),Tjj(j,jp)*X(i,j,jp,n1))=g=0;
cons_7(i,n2)$(ord(n2) ge 1 )..        Tf(i,n2)-Ts(i,n2)=g=sum(j,Tij_lo(i,j)*XS(i,j,n2));
cons_7_1(i,n2)$(ord(n2) ge 1 )..        Tf(i,n2)-Ts(i,n2)=l=sum(j,Tij_up(i,j)*XS(i,j,n2));
cons_8(i).. sum((j,n),X(i,j,'AS00-0',n))=l=1;
cons_8_1(i).. sum((j,n),X(i,j,'AS0-0',n))=l=1;
cons_15(i,n)$(ord(n) ge 2).. Tf(i,n-1)=l=Ts(i,n);
cons_15_1(i,n)..  Ts(i,n)=l=H;
cons_15_2(i,n)..  Tf(i,n)=l=H;
cons_25(i)..  sum(n2$(ord(n2) ge 2),XS(i,'AS0-0',n2))=g=1;
cons_21(i,n2)..       XS(i,'AS0-0',n2)*BU=l=Tf(i,n2);
cons_22(i)..    BL=g=sum(n2,XS11(i,'AS0-0',n2));
cons_22_1(i,j,n2)..  XS11(i,'AS0-0',n2)=l=XS(i,'AS0-0',n2)*H  ;
cons_22_2(i,j,n2)..  XS21(i,'AS0-0',n2)=l=(1-XS(i,'AS0-0',n2))*H  ;
cons_22_3(i,j,n2)..  XS21(i,'AS0-0',n2)=g=0  ;
cons_22_4(i,j,n2)..  XS11(i,'AS0-0',n2)=g=0  ;
cons_22_5(i,j,n2)..  XS21(i,'AS0-0',n2)+ XS11(i,'AS0-0',n2)=e=Ts(i,n2);
cons_AX_1(i,j,n).. AXS(i,j,n)=l=XS(i,j,n);
cons_AX_2(i,j,n)$(ord(n) le Nmax-1).. AXS(i,j,n)=l=XS(i,j,n+1);
cons_AX_3(i,j,n)$(ord(n) le Nmax-1).. AXS(i,j,n)=g=XS(i,j,n)+XS(i,j,n+1)-1;
*obj.. cost =e=(-0.001)*sum((i,n),Tf(i,n)-Ts(i,n))+sum((i,j1,n),ord(n)*XS(i,j1,n));
obj.. cost =e=(-0.001)*sum((i,n),Tf(i,n)-Ts(i,n))+(-1)*sum((i,j,n2),XS(i,j,n2)); 
Model test /all/;
*设置初值和终值
XS.fx(i, 'AS00-0', '0')=1 ;
XS.fx(i, j, n1)=0;
*XS.fx(i, 'TS0000', '18')=1 ;
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
option optcr=0.2;
Solve test minimizing cost using mip;
Display  XS.l, X.l,Ts.l,Tf.l;
Display cost.l;
Execute_Unload 'filename.gdx', Ts,Tf;
Execute 'Gdxxrw.exe filename.gdx O = test1.xls var = Ts = Excel spreadsheet!';
Execute 'Gdxxrw.exe filename.gdx O = test2.xls var = Tf = Excel spreadsheet!';
";
            #endregion

            #region model2
            string model2 = @"
Sets
             n         time point            " + n0 + @"
             n1(n)     odd  point            " + n1 + @" 
             n2(n)     even point            " + n2 + @" 
             j         all task            
             i         所有维修员元素点    
             j1(j)     紧急任务            
             j2(j)     重要任务     
             j3(j)     一般任务        
             j4(j)     必须完成 


alias(j, jp, jpp);
alias(j2, j2p);

Scalar       PN        维修人员总数       /5/
             H         总调度时间        " + H + @"
             BL                          " + BL + @"
             BU                          " + BU + @"
             Nmax;
Nmax=card(n);

Parameters   PL(i)          维修人员i拥有级别 PL(i)以上技能
             TL(j)          任务j所需技能
             Tij_lo(i,j)           i in j     time        
             Tij_up(i,j)           i in j     time
             Tjj(j,jp)     j to jp    time
;



$if not set gdxincname $abort 'no include file name for data file provided'
$gdxin %gdxincname%
$load i j j1 j2  j3 j4 PL TL Tjj  Tij_lo  Tij_up  
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

cons_1(j)
cons_2(i, n)                    同一时间同一个人最多只能做一个任务
*cons_3(i, j, n)                 修井人员满足技能需求
cons_4(i, j, n)
cons_5(i, jp, n)
cons_6(i,n1)
cons_7(i,n2)
cons_6_1(i,n1)
cons_7_1(i,n2)
cons_8(i)
cons_15(i,n)
cons_15_1(i,n)
cons_15_2(i,n)
cons_AX_1(i,j,n)
cons_AX_2(i,j,n)
cons_AX_3(i,j,n)
cons_21(i,n2)
cons_22(i)
cons_22_1(i,j,n2)
cons_22_2(i,j,n2)
cons_22_3(i,j,n2)
cons_22_4(i,j,n2)
cons_22_5(i,j,n2)
cons_25(i)
cons_9(j4)
cons_8_1(i)
obj;
cons_9(j4).. sum((i, n2), XS(i, j4, n2))=e=1;
cons_1(j)$(ord(j) ne 1 and ord(j) ne 2).. sum((i, n2), XS(i, j, n2))=l=1;
cons_2(i, n).. sum(j, XS(i, j, n)) =l= 1;
*cons_3(i, j, n)..  XS(i, j, n) * (PL(i) - TL(j)) =l= 0;
cons_4(i, j, n)$(ord(n) le Nmax-1)..  XS(i, j, n)-AXS(i,j,n)=e=sum(jp, X(i, j, jp, n+1));
cons_5(i, jp, n)$(ord(n) ge 2 and ord(jp) ne 1)..   sum(j, X(i, j, jp, n-1))=e=XS(i, jp, n)-AXS(i,jp,n-1);
cons_6(i,n1)$(ord(n1) ge 1)..   Tf(i,n1)-Ts(i,n1)-sum((j,jp),Tjj(j,jp)*X(i,j,jp,n1))=l=0;
cons_6_1(i,n1)$(ord(n1) ge 1)..   Tf(i,n1)-Ts(i,n1)-sum((j,jp),Tjj(j,jp)*X(i,j,jp,n1))=g=0;
cons_7(i,n2)$(ord(n2) ge 1 )..        Tf(i,n2)-Ts(i,n2)=g=sum(j,Tij_lo(i,j)*XS(i,j,n2));
cons_7_1(i,n2)$(ord(n2) ge 1 )..        Tf(i,n2)-Ts(i,n2)=l=sum(j,Tij_up(i,j)*XS(i,j,n2));
cons_8(i).. sum((j,n),X(i,j,'AS00-0',n))=l=1;
cons_8_1(i).. sum((j,n),X(i,j,'AS0-0',n))=l=1;
cons_15(i,n)$(ord(n) ge 2).. Tf(i,n-1)=l=Ts(i,n);
cons_15_1(i,n)..  Ts(i,n)=l=H;
cons_15_2(i,n)..  Tf(i,n)=l=H;
cons_25(i)..  sum(n2$(ord(n2) ge 2),XS(i,'AS0-0',n2))=g=1;
cons_21(i,n2)..       XS(i,'AS0-0',n2)*BU=l=Tf(i,n2);
cons_22(i)..    BL=g=sum(n2,XS11(i,'AS0-0',n2));
cons_22_1(i,j,n2)..  XS11(i,'AS0-0',n2)=l=XS(i,'AS0-0',n2)*H  ;
cons_22_2(i,j,n2)..  XS21(i,'AS0-0',n2)=l=(1-XS(i,'AS0-0',n2))*H  ;
cons_22_3(i,j,n2)..  XS21(i,'AS0-0',n2)=g=0  ;
cons_22_4(i,j,n2)..  XS11(i,'AS0-0',n2)=g=0  ;
cons_22_5(i,j,n2)..  XS21(i,'AS0-0',n2)+ XS11(i,'AS0-0',n2)=e=Ts(i,n2);
cons_AX_1(i,j,n).. AXS(i,j,n)=l=XS(i,j,n);
cons_AX_2(i,j,n)$(ord(n) le Nmax-1).. AXS(i,j,n)=l=XS(i,j,n+1);
cons_AX_3(i,j,n)$(ord(n) le Nmax-1).. AXS(i,j,n)=g=XS(i,j,n)+XS(i,j,n+1)-1;
*obj.. cost =e=(-0.001)*sum((i,n),Tf(i,n)-Ts(i,n))+sum((i,j1,n),ord(n)*XS(i,j1,n));
obj.. cost =e=(-0.01)*sum((i,n),Tf(i,n)-Ts(i,n))+10*sum((i,j1,n),ord(n)*XS(i,j1,n))+(-1)*sum((i,j,n2),XS(i,j,n2)); 
Model test /all/;
*设置初值和终值
XS.fx(i, 'AS00-0', '0')=1 ;
XS.fx(i, j, n1)=0;
*XS.fx(i, 'TS0000', '18')=1 ;
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
option optcr=0.2;
Solve test minimizing cost using mip;
Display  XS.l, X.l,Ts.l,Tf.l;
Display cost.l;
Execute_Unload 'filename.gdx', Ts,Tf;
Execute 'Gdxxrw.exe filename.gdx O = test1.xls var = Ts = Excel spreadsheet!';
Execute 'Gdxxrw.exe filename.gdx O = test2.xls var = Tf = Excel spreadsheet!';
";
            #endregion

            #region model3
            string model3 = @"
Sets
             n         time point            " + n0 + @"
             n1(n)     odd  point            " + n1 + @" 
             n2(n)     even point            " + n2 + @" 
             j         all task            
             i         所有维修员元素点    
             j1(j)     紧急任务            
             j2(j)     重要任务     
                     
             j4(j)     必须完成 


alias(j, jp, jpp);
alias(j2, j2p);

Scalar       PN        维修人员总数       /5/
             H         总调度时间        " + H + @"
             BL                          " + BL + @"
             BU                          " + BU + @"
             Nmax;
Nmax=card(n);

Parameters   PL(i)          维修人员i拥有级别 PL(i)以上技能
             TL(j)          任务j所需技能
             Tij_lo(i,j)           i in j     time        
             Tij_up(i,j)           i in j     time
             Tjj(j,jp)     j to jp    time
;



$if not set gdxincname $abort 'no include file name for data file provided'
$gdxin %gdxincname%
$load i j j1 j2   j4 PL TL Tjj  Tij_lo  Tij_up  
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

cons_1(j)
cons_2(i, n)                    同一时间同一个人最多只能做一个任务
*cons_3(i, j, n)                 修井人员满足技能需求
cons_4(i, j, n)
cons_5(i, jp, n)
cons_6(i,n1)
cons_7(i,n2)
cons_6_1(i,n1)
cons_7_1(i,n2)
cons_8(i)
cons_15(i,n)
cons_15_1(i,n)
cons_15_2(i,n)
cons_AX_1(i,j,n)
cons_AX_2(i,j,n)
cons_AX_3(i,j,n)
cons_21(i,n2)
cons_22(i)
cons_22_1(i,j,n2)
cons_22_2(i,j,n2)
cons_22_3(i,j,n2)
cons_22_4(i,j,n2)
cons_22_5(i,j,n2)
cons_25(i)
cons_9(j4)
cons_8_1(i)
obj;
cons_9(j4).. sum((i, n2), XS(i, j4, n2))=e=1;
cons_1(j)$(ord(j) ne 1 and ord(j) ne 2).. sum((i, n2), XS(i, j, n2))=l=1;
cons_2(i, n).. sum(j, XS(i, j, n)) =l= 1;
*cons_3(i, j, n)..  XS(i, j, n) * (PL(i) - TL(j)) =l= 0;
cons_4(i, j, n)$(ord(n) le Nmax-1)..  XS(i, j, n)-AXS(i,j,n)=e=sum(jp, X(i, j, jp, n+1));
cons_5(i, jp, n)$(ord(n) ge 2 and ord(jp) ne 1)..   sum(j, X(i, j, jp, n-1))=e=XS(i, jp, n)-AXS(i,jp,n-1);
cons_6(i,n1)$(ord(n1) ge 1)..   Tf(i,n1)-Ts(i,n1)-sum((j,jp),Tjj(j,jp)*X(i,j,jp,n1))=l=0;
cons_6_1(i,n1)$(ord(n1) ge 1)..   Tf(i,n1)-Ts(i,n1)-sum((j,jp),Tjj(j,jp)*X(i,j,jp,n1))=g=0;
cons_7(i,n2)$(ord(n2) ge 1 )..        Tf(i,n2)-Ts(i,n2)=g=sum(j,Tij_lo(i,j)*XS(i,j,n2));
cons_7_1(i,n2)$(ord(n2) ge 1 )..        Tf(i,n2)-Ts(i,n2)=l=sum(j,Tij_up(i,j)*XS(i,j,n2));
cons_8(i).. sum((j,n),X(i,j,'AS00-0',n))=l=1;
cons_8_1(i).. sum((j,n),X(i,j,'AS0-0',n))=l=1;
cons_15(i,n)$(ord(n) ge 2).. Tf(i,n-1)=l=Ts(i,n);
cons_15_1(i,n)..  Ts(i,n)=l=H;
cons_15_2(i,n)..  Tf(i,n)=l=H;
cons_25(i)..  sum(n2$(ord(n2) ge 2),XS(i,'AS0-0',n2))=g=1;
cons_21(i,n2)..       XS(i,'AS0-0',n2)*BU=l=Tf(i,n2);
cons_22(i)..    BL=g=sum(n2,XS11(i,'AS0-0',n2));
cons_22_1(i,j,n2)..  XS11(i,'AS0-0',n2)=l=XS(i,'AS0-0',n2)*H  ;
cons_22_2(i,j,n2)..  XS21(i,'AS0-0',n2)=l=(1-XS(i,'AS0-0',n2))*H  ;
cons_22_3(i,j,n2)..  XS21(i,'AS0-0',n2)=g=0  ;
cons_22_4(i,j,n2)..  XS11(i,'AS0-0',n2)=g=0  ;
cons_22_5(i,j,n2)..  XS21(i,'AS0-0',n2)+ XS11(i,'AS0-0',n2)=e=Ts(i,n2);
cons_AX_1(i,j,n).. AXS(i,j,n)=l=XS(i,j,n);
cons_AX_2(i,j,n)$(ord(n) le Nmax-1).. AXS(i,j,n)=l=XS(i,j,n+1);
cons_AX_3(i,j,n)$(ord(n) le Nmax-1).. AXS(i,j,n)=g=XS(i,j,n)+XS(i,j,n+1)-1;
*obj.. cost =e=(-0.001)*sum((i,n),Tf(i,n)-Ts(i,n))+sum((i,j1,n),ord(n)*XS(i,j1,n));
obj.. cost =e=(-0.01)*sum((i,n),Tf(i,n)-Ts(i,n))+10*sum((i,j1,n),ord(n)*XS(i,j1,n)); 
Model test /all/;
*设置初值和终值
XS.fx(i, 'AS00-0', '0')=1 ;
XS.fx(i, j, n1)=0;
*XS.fx(i, 'TS0000', '18')=1 ;
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
option optcr=0.2;
Solve test minimizing cost using mip;
Display  XS.l, X.l,Ts.l,Tf.l;
Display cost.l;
Execute_Unload 'filename.gdx', Ts,Tf;
Execute 'Gdxxrw.exe filename.gdx O = test1.xls var = Ts = Excel spreadsheet!';
Execute 'Gdxxrw.exe filename.gdx O = test2.xls var = Tf = Excel spreadsheet!';
";
            #endregion

            #region model4
            string model4 = @"
Sets
             n         time point            " + n0 + @"
             n1(n)     odd  point            " + n1 + @" 
             n2(n)     even point            " + n2 + @" 
             j         all task            
             i         所有维修员元素点    
                        
             j2(j)     重要任务     
             j4(j)      重要任务  
              


alias(j, jp, jpp);
alias(j2, j2p);

Scalar       PN        维修人员总数       /5/
             H         总调度时间        " + H + @"
             BL                          " + BL + @"
             BU                          " + BU + @"
             Nmax;
Nmax=card(n);

Parameters   PL(i)          维修人员i拥有级别 PL(i)以上技能
             TL(j)          任务j所需技能
             Tij_lo(i,j)           i in j     time        
             Tij_up(i,j)           i in j     time
             Tjj(j,jp)     j to jp    time
;



$if not set gdxincname $abort 'no include file name for data file provided'
$gdxin %gdxincname%
$load i j  j2  j4  PL TL Tjj  Tij_lo  Tij_up  
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

cons_1(j)
cons_2(i, n)                    同一时间同一个人最多只能做一个任务
*cons_3(i, j, n)                 修井人员满足技能需求
cons_4(i, j, n)
cons_5(i, jp, n)
cons_6(i,n1)
cons_7(i,n2)
cons_6_1(i,n1)
cons_7_1(i,n2)
cons_8(i)
cons_15(i,n)
cons_15_1(i,n)
cons_15_2(i,n)
cons_AX_1(i,j,n)
cons_AX_2(i,j,n)
cons_AX_3(i,j,n)
cons_21(i,n2)
cons_22(i)
cons_22_1(i,j,n2)
cons_22_2(i,j,n2)
cons_22_3(i,j,n2)
cons_22_4(i,j,n2)
cons_22_5(i,j,n2)
cons_25(i)
*cons_9(j2)
cons_8_1(i)
obj;
*cons_9(j2).. sum((i, n2), XS(i, j2, n2))=e=1;
cons_1(j)$(ord(j) ne 1 and ord(j) ne 2).. sum((i, n2), XS(i, j, n2))=l=1;
cons_2(i, n).. sum(j, XS(i, j, n)) =l= 1;
*cons_3(i, j, n)..  XS(i, j, n) * (PL(i) - TL(j)) =l= 0;
cons_4(i, j, n)$(ord(n) le Nmax-1)..  XS(i, j, n)-AXS(i,j,n)=e=sum(jp, X(i, j, jp, n+1));
cons_5(i, jp, n)$(ord(n) ge 2 and ord(jp) ne 1)..   sum(j, X(i, j, jp, n-1))=e=XS(i, jp, n)-AXS(i,jp,n-1);
cons_6(i,n1)$(ord(n1) ge 1)..   Tf(i,n1)-Ts(i,n1)-sum((j,jp),Tjj(j,jp)*X(i,j,jp,n1))=l=0;
cons_6_1(i,n1)$(ord(n1) ge 1)..   Tf(i,n1)-Ts(i,n1)-sum((j,jp),Tjj(j,jp)*X(i,j,jp,n1))=g=0;
cons_7(i,n2)$(ord(n2) ge 1 )..        Tf(i,n2)-Ts(i,n2)=g=sum(j,Tij_lo(i,j)*XS(i,j,n2));
cons_7_1(i,n2)$(ord(n2) ge 1 )..        Tf(i,n2)-Ts(i,n2)=l=sum(j,Tij_up(i,j)*XS(i,j,n2));
cons_8(i).. sum((j,n),X(i,j,'AS00-0',n))=l=1;
cons_8_1(i).. sum((j,n),X(i,j,'AS0-0',n))=l=1;
cons_15(i,n)$(ord(n) ge 2).. Tf(i,n-1)=l=Ts(i,n);
cons_15_1(i,n)..  Ts(i,n)=l=H;
cons_15_2(i,n)..  Tf(i,n)=l=H;
cons_25(i)..  sum(n2$(ord(n2) ge 2),XS(i,'AS0-0',n2))=g=1;
cons_21(i,n2)..       XS(i,'AS0-0',n2)*BU=l=Tf(i,n2);
cons_22(i)..    BL=g=sum(n2,XS11(i,'AS0-0',n2));
cons_22_1(i,j,n2)..  XS11(i,'AS0-0',n2)=l=XS(i,'AS0-0',n2)*H  ;
cons_22_2(i,j,n2)..  XS21(i,'AS0-0',n2)=l=(1-XS(i,'AS0-0',n2))*H  ;
cons_22_3(i,j,n2)..  XS21(i,'AS0-0',n2)=g=0  ;
cons_22_4(i,j,n2)..  XS11(i,'AS0-0',n2)=g=0  ;
cons_22_5(i,j,n2)..  XS21(i,'AS0-0',n2)+ XS11(i,'AS0-0',n2)=e=Ts(i,n2);
cons_AX_1(i,j,n).. AXS(i,j,n)=l=XS(i,j,n);
cons_AX_2(i,j,n)$(ord(n) le Nmax-1).. AXS(i,j,n)=l=XS(i,j,n+1);
cons_AX_3(i,j,n)$(ord(n) le Nmax-1).. AXS(i,j,n)=g=XS(i,j,n)+XS(i,j,n+1)-1;
*obj.. cost =e=(-0.001)*sum((i,n),Tf(i,n)-Ts(i,n))+sum((i,j1,n),ord(n)*XS(i,j1,n));
obj.. cost =e=(-0.01)*sum((i,n2),Tf(i,n2)-Ts(i,n2)); 
Model test /all/;
*设置初值和终值
XS.fx(i, 'AS00-0', '0')=1 ;
XS.fx(i, j, n1)=0;
*XS.fx(i, 'TS0000', '18')=1 ;
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
option optcr=0.2;
Solve test minimizing cost using mip;
Display  XS.l, X.l,Ts.l,Tf.l;
Display cost.l;
Execute_Unload 'filename.gdx', Ts,Tf;
Execute 'Gdxxrw.exe filename.gdx O = test1.xls var = Ts = Excel spreadsheet!';
Execute 'Gdxxrw.exe filename.gdx O = test2.xls var = Tf = Excel spreadsheet!';
";
            #endregion

            #region model5
            string model5 = @"
Sets
             n         time point            " + n0 + @"
             n1(n)     odd  point            " + n1 + @" 
             n2(n)     even point            " + n2 + @" 
             j         all task            
             i         所有维修员元素点    
                  
             j3(j)     一般任务        
              


alias(j, jp, jpp);
*alias(j2, j2p);

Scalar       PN        维修人员总数       /5/
             H         总调度时间        " + H + @"
             BL                          " + BL + @"
             BU                          " + BU + @"
             Nmax;
Nmax=card(n);

Parameters   PL(i)          维修人员i拥有级别 PL(i)以上技能
             TL(j)          任务j所需技能
             Tij_lo(i,j)           i in j     time        
             Tij_up(i,j)           i in j     time
             Tjj(j,jp)     j to jp    time
;



$if not set gdxincname $abort 'no include file name for data file provided'
$gdxin %gdxincname%
$load i j  j3  PL TL Tjj  Tij_lo  Tij_up  
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

cons_1(j)
cons_2(i, n)                    同一时间同一个人最多只能做一个任务
*cons_3(i, j, n)                 修井人员满足技能需求
cons_4(i, j, n)
cons_5(i, jp, n)
cons_6(i,n1)
cons_7(i,n2)
cons_6_1(i,n1)
cons_7_1(i,n2)
cons_8(i)
cons_15(i,n)
cons_15_1(i,n)
cons_15_2(i,n)
cons_AX_1(i,j,n)
cons_AX_2(i,j,n)
cons_AX_3(i,j,n)
cons_21(i,n2)
cons_22(i)
cons_22_1(i,j,n2)
cons_22_2(i,j,n2)
cons_22_3(i,j,n2)
cons_22_4(i,j,n2)
cons_22_5(i,j,n2)
cons_25(i)
*cons_9(j4)
cons_8_1(i)
obj;
*cons_9(j4).. sum((i, n2), XS(i, j4, n2))=e=1;
cons_1(j)$(ord(j) ne 1 and ord(j) ne 2).. sum((i, n2), XS(i, j, n2))=l=1;
cons_2(i, n).. sum(j, XS(i, j, n)) =l= 1;
*cons_3(i, j, n)..  XS(i, j, n) * (PL(i) - TL(j)) =l= 0;
cons_4(i, j, n)$(ord(n) le Nmax-1)..  XS(i, j, n)-AXS(i,j,n)=e=sum(jp, X(i, j, jp, n+1));
cons_5(i, jp, n)$(ord(n) ge 2 and ord(jp) ne 1)..   sum(j, X(i, j, jp, n-1))=e=XS(i, jp, n)-AXS(i,jp,n-1);
cons_6(i,n1)$(ord(n1) ge 1)..   Tf(i,n1)-Ts(i,n1)-sum((j,jp),Tjj(j,jp)*X(i,j,jp,n1))=l=0;
cons_6_1(i,n1)$(ord(n1) ge 1)..   Tf(i,n1)-Ts(i,n1)-sum((j,jp),Tjj(j,jp)*X(i,j,jp,n1))=g=0;
cons_7(i,n2)$(ord(n2) ge 1 )..        Tf(i,n2)-Ts(i,n2)=g=sum(j,Tij_lo(i,j)*XS(i,j,n2));
cons_7_1(i,n2)$(ord(n2) ge 1 )..        Tf(i,n2)-Ts(i,n2)=l=sum(j,Tij_up(i,j)*XS(i,j,n2));
cons_8(i).. sum((j,n),X(i,j,'AS00-0',n))=l=1;
cons_8_1(i).. sum((j,n),X(i,j,'AS0-0',n))=l=1;
cons_15(i,n)$(ord(n) ge 2).. Tf(i,n-1)=l=Ts(i,n);
cons_15_1(i,n)..  Ts(i,n)=l=H;
cons_15_2(i,n)..  Tf(i,n)=l=H;
cons_25(i)..  sum(n2$(ord(n2) ge 2),XS(i,'AS0-0',n2))=g=1;
cons_21(i,n2)..       XS(i,'AS0-0',n2)*BU=l=Tf(i,n2);
cons_22(i)..    BL=g=sum(n2,XS11(i,'AS0-0',n2));
cons_22_1(i,j,n2)..  XS11(i,'AS0-0',n2)=l=XS(i,'AS0-0',n2)*H  ;
cons_22_2(i,j,n2)..  XS21(i,'AS0-0',n2)=l=(1-XS(i,'AS0-0',n2))*H  ;
cons_22_3(i,j,n2)..  XS21(i,'AS0-0',n2)=g=0  ;
cons_22_4(i,j,n2)..  XS11(i,'AS0-0',n2)=g=0  ;
cons_22_5(i,j,n2)..  XS21(i,'AS0-0',n2)+ XS11(i,'AS0-0',n2)=e=Ts(i,n2);
cons_AX_1(i,j,n).. AXS(i,j,n)=l=XS(i,j,n);
cons_AX_2(i,j,n)$(ord(n) le Nmax-1).. AXS(i,j,n)=l=XS(i,j,n+1);
cons_AX_3(i,j,n)$(ord(n) le Nmax-1).. AXS(i,j,n)=g=XS(i,j,n)+XS(i,j,n+1)-1;
*obj.. cost =e=(-0.001)*sum((i,n),Tf(i,n)-Ts(i,n))+sum((i,j1,n),ord(n)*XS(i,j1,n));
obj.. cost =e=(-0.001)*sum((i,n2),Tf(i,n2)-Ts(i,n2))+(-1)*sum((i,j,n2),XS(i,j,n2)); 
Model test /all/;
*设置初值和终值
XS.fx(i, 'AS00-0', '0')=1 ;
XS.fx(i, j, n1)=0;
*XS.fx(i, 'TS0000', '18')=1 ;
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
option optcr=0.2;
Solve test minimizing cost using mip;
Display  XS.l, X.l,Ts.l,Tf.l;
Display cost.l;
Execute_Unload 'filename.gdx', Ts,Tf;
Execute 'Gdxxrw.exe filename.gdx O = test1.xls var = Ts = Excel spreadsheet!';
Execute 'Gdxxrw.exe filename.gdx O = test2.xls var = Tf = Excel spreadsheet!';
";
            #endregion

            #region model6
            string model6 = @"
Sets
             n         time point            " + n0 + @"
             n1(n)     odd  point            " + n1 + @" 
             n2(n)     even point            " + n2 + @" 
             j         all task            
             i         所有维修员元素点    
             j1(j)     紧急任务    
             j3(j)     一般任务        
              


alias(j, jp, jpp);
*alias(j2, j2p);

Scalar       PN        维修人员总数       /5/
             H         总调度时间        " + H + @"
             BL                          " + BL + @"
             BU                          " + BU + @"
             Nmax;
Nmax=card(n);

Parameters   PL(i)          维修人员i拥有级别 PL(i)以上技能
             TL(j)          任务j所需技能
             Tij_lo(i,j)           i in j     time        
             Tij_up(i,j)           i in j     time
             Tjj(j,jp)     j to jp    time
;



$if not set gdxincname $abort 'no include file name for data file provided'
$gdxin %gdxincname%
$load i j j1   j3  PL TL Tjj  Tij_lo  Tij_up  
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

cons_1(j)
cons_2(i, n)                    同一时间同一个人最多只能做一个任务
*cons_3(i, j, n)                 修井人员满足技能需求
cons_4(i, j, n)
cons_5(i, jp, n)
cons_6(i,n1)
cons_7(i,n2)
cons_6_1(i,n1)
cons_7_1(i,n2)
cons_8(i)
cons_15(i,n)
cons_15_1(i,n)
cons_15_2(i,n)
cons_AX_1(i,j,n)
cons_AX_2(i,j,n)
cons_AX_3(i,j,n)
cons_21(i,n2)
cons_22(i)
cons_22_1(i,j,n2)
cons_22_2(i,j,n2)
cons_22_3(i,j,n2)
cons_22_4(i,j,n2)
cons_22_5(i,j,n2)
cons_25(i)
cons_9(j1)
cons_8_1(i)
obj;
cons_9(j1).. sum((i, n2), XS(i, j1, n2))=e=1;
cons_1(j)$(ord(j) ne 1 and ord(j) ne 2).. sum((i, n2), XS(i, j, n2))=l=1;
cons_2(i, n).. sum(j, XS(i, j, n)) =l= 1;
*cons_3(i, j, n)..  XS(i, j, n) * (PL(i) - TL(j)) =l= 0;
cons_4(i, j, n)$(ord(n) le Nmax-1)..  XS(i, j, n)-AXS(i,j,n)=e=sum(jp, X(i, j, jp, n+1));
cons_5(i, jp, n)$(ord(n) ge 2 and ord(jp) ne 1)..   sum(j, X(i, j, jp, n-1))=e=XS(i, jp, n)-AXS(i,jp,n-1);
cons_6(i,n1)$(ord(n1) ge 1)..   Tf(i,n1)-Ts(i,n1)-sum((j,jp),Tjj(j,jp)*X(i,j,jp,n1))=l=0;
cons_6_1(i,n1)$(ord(n1) ge 1)..   Tf(i,n1)-Ts(i,n1)-sum((j,jp),Tjj(j,jp)*X(i,j,jp,n1))=g=0;
cons_7(i,n2)$(ord(n2) ge 1 )..        Tf(i,n2)-Ts(i,n2)=g=sum(j,Tij_lo(i,j)*XS(i,j,n2));
cons_7_1(i,n2)$(ord(n2) ge 1 )..        Tf(i,n2)-Ts(i,n2)=l=sum(j,Tij_up(i,j)*XS(i,j,n2));
cons_8(i).. sum((j,n),X(i,j,'AS00-0',n))=l=1;
cons_8_1(i).. sum((j,n),X(i,j,'AS0-0',n))=l=1;
cons_15(i,n)$(ord(n) ge 2).. Tf(i,n-1)=l=Ts(i,n);
cons_15_1(i,n)..  Ts(i,n)=l=H;
cons_15_2(i,n)..  Tf(i,n)=l=H;
cons_25(i)..  sum(n2$(ord(n2) ge 2),XS(i,'AS0-0',n2))=g=1;
cons_21(i,n2)..       XS(i,'AS0-0',n2)*BU=l=Tf(i,n2);
cons_22(i)..    BL=g=sum(n2,XS11(i,'AS0-0',n2));
cons_22_1(i,j,n2)..  XS11(i,'AS0-0',n2)=l=XS(i,'AS0-0',n2)*H  ;
cons_22_2(i,j,n2)..  XS21(i,'AS0-0',n2)=l=(1-XS(i,'AS0-0',n2))*H  ;
cons_22_3(i,j,n2)..  XS21(i,'AS0-0',n2)=g=0  ;
cons_22_4(i,j,n2)..  XS11(i,'AS0-0',n2)=g=0  ;
cons_22_5(i,j,n2)..  XS21(i,'AS0-0',n2)+ XS11(i,'AS0-0',n2)=e=Ts(i,n2);
cons_AX_1(i,j,n).. AXS(i,j,n)=l=XS(i,j,n);
cons_AX_2(i,j,n)$(ord(n) le Nmax-1).. AXS(i,j,n)=l=XS(i,j,n+1);
cons_AX_3(i,j,n)$(ord(n) le Nmax-1).. AXS(i,j,n)=g=XS(i,j,n)+XS(i,j,n+1)-1;
*obj.. cost =e=(-0.001)*sum((i,n),Tf(i,n)-Ts(i,n))+sum((i,j1,n),ord(n)*XS(i,j1,n));
obj.. cost =e=(-0.001)*sum((i,n2),Tf(i,n2)-Ts(i,n2))+(-1)*sum((i,j,n2),XS(i,j,n2))+sum((i,j1,n),ord(n)*XS(i,j1,n)); 
Model test /all/;
*设置初值和终值
XS.fx(i, 'AS00-0', '0')=1 ;
XS.fx(i, j, n1)=0;
*XS.fx(i, 'TS0000', '18')=1 ;
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
option optcr=0.2;
Solve test minimizing cost using mip;
Display  XS.l, X.l,Ts.l,Tf.l;
Display cost.l;
Execute_Unload 'filename.gdx', Ts,Tf;
Execute 'Gdxxrw.exe filename.gdx O = test1.xls var = Ts = Excel spreadsheet!';
Execute 'Gdxxrw.exe filename.gdx O = test2.xls var = Tf = Excel spreadsheet!';
";
            #endregion

            Console.WriteLine("选择模型：" + type);
            switch (type)
            {
                case 1:
                    return model1;
                case 2:
                    return model2;
                case 3:
                    return model3;
                case 4:
                    return model4;
                case 5:
                    return model5;
                case 6:
                    return model6;
                default:
                    return "";
            }
        }
    }
}
