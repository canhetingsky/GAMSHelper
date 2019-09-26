using GAMSHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace GAMSDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime startDT = DateTime.Now;

            //数据库相关信息
            string server;  //数据库地址
            string database;    //数据库名称
            string user;        //数据库用户名
            string pwd;         //数据库密码

            //数据库查询语句
            string command1 = null;
            string command2 = null;
            string command3 = null;
            string command4 = null;
            string time = null;
            string debug = null;  //是否启用日志记录，"LOG"：启用日志记录；"NONE"：不启用日志记录

            string command5 = null;  //查询技能高于某个水平的人员位置的SQL语句
            string command6 = null;  //查询人员位置的SQL语句

            //紧急任务相关信息
            string taskID = null;
            string t_positionID = null;
            //int skillLevel = 0;
            string position_tableName = ""; //井之间距离的表名

            List<string> personID = new List<string>();
            List<string> p_positionID = new List<string>();
            List<int> p_spendTime = new List<int>();
            string choosePersonID = null;

            #region 输入参数解析
            if (args.Length == 0)
            {
                //共有参数
                server = "DESKTOP-36C9L6T";
                database = "lushushu";
                user = "sa";
                pwd = "123";

                //任务调度参数
                //command1 = "select * from IMS_PATROL_PERSON_ON_DUTY;";
                //command2 = "select * from IMS_PATROL_TASK_SKILL ORDER BY TASK_PRIORITY ASC;";
                //command3 = "select * from IMS_PATROL_PERSON_TASK_TIME;";
                //command4 = "select * from IMS_PATROL_TASK_SPEND_TIME;";
                time = "8:30";
                debug = "LOG";

                //紧急任务参数
                command5 = "select PERSON_ID from IMS_PATROL_PERSON_ON_DUTY where SKILL_LEVEL<=2;";
                taskID = "TS0015";
                t_positionID = "";
            }
            else if (args.Length == 6) //任务调度传递6个参数
            {
                server = args[0];
                database = args[1];
                user = args[2];
                pwd = args[3];

                time = args[4];
                debug = args[5];

                string[] lstr = new string[2] { "7:30", "8:30" };
                bool exists = ((IList)lstr).Contains(time);
                if (!exists)
                {
                    Console.WriteLine("工作开始时间有误");
                    return;
                }
            }
            else if (args.Length == 9) //紧急任务处理传递9个参数
            {
                server = args[0];
                database = args[1];
                user = args[2];
                pwd = args[3];

                command5 = args[4];
                command6 = args[5];
                position_tableName = args[6];
                taskID = args[7];
                t_positionID = args[8];
            }
            else
            {
                Console.WriteLine("数据库参数输入格式不正确，共输入了%d个参数", args.Length);
                return;
            }
            #endregion


            if ((debug == null) && (args.Length == 9))  //紧急任务调度
            {
                SQLConnect scon = new SQLConnect(server, database, user, pwd);
                personID = scon.GetHighLevelPersonIDFromSQL(command5);  //得到可以做此任务的人员ID

                //TODO:get person position id
                //...
                List<string>[] listPersonPosition = scon.GetPersonPositionIDFromSQL(command6);
                for (int i = 0; i < personID.Count; i++)
                {
                    for (int j = 0; j < listPersonPosition[0].Count; j++)
                    {
                        if (listPersonPosition[0][j] == personID[i])
                        {
                            p_positionID.Add(listPersonPosition[1][j]);
                            break;
                        }
                    }
                }


                //TODO:get person spend time from person position id to task position id
                //...
                p_spendTime = scon.GetPositionSpendTimeFromSQL(position_tableName, p_positionID, t_positionID);


                //找到到达任务点最快人的ID
                int minTime = p_spendTime[0];
                int index = 0;
                for (int i = 0; i < p_spendTime.Count; i++)
                {

                    if (p_spendTime[i] < minTime)
                    {
                        minTime = p_spendTime[i];
                        index = i;
                    }
                }
                choosePersonID = personID[index];

                List<string>[] personTaskList = scon.GetPersonTaskListFromSQL(choosePersonID);
                int t_index = 0;
                string taskStartTime = null;
                for (int i = 0; i < personTaskList[0].Count; i++)
                {
                    DateTime tstart = Convert.ToDateTime(personTaskList[2][i]);

                    DateTime restStartTime = new DateTime(startDT.Year, startDT.Month, startDT.Day, 12, 0, 0);
                    DateTime restEndTime = new DateTime(startDT.Year, startDT.Month, startDT.Day, 14, 0, 0);

                    TimeSpan td = startDT.Subtract(tstart);
                    double timeInterval = td.TotalMinutes;
                    if ((timeInterval > 0) && (timeInterval < Convert.ToDouble(personTaskList[1][i])))
                    {
                        if (startDT.AddMinutes(minTime) < restStartTime)
                        {
                            taskStartTime = Convert.ToString(startDT);
                        }
                        else
                        {
                            taskStartTime = Convert.ToString(restEndTime);
                        }
                        t_index = i;
                        break;
                    }
                }
                int count = personTaskList[0].Count;
                List<string>[] newPersonTaskList = new List<string>[4] {
                    new List<string>(),
                    new List<string>(),
                    new List<string>(),
                    new List<string>()};
                //TASK_ID,SPEND_TIME,START_TIME,END_TIME

                newPersonTaskList[0].Add(taskID);
                newPersonTaskList[1].Add(minTime.ToString());
                newPersonTaskList[2].Add(taskStartTime);
                newPersonTaskList[3].Add(AddMinutes(taskStartTime, minTime));

                for (int i = 1; i < count - t_index; i++)
                {
                    newPersonTaskList[0].Add(personTaskList[0][t_index + i]);
                    newPersonTaskList[1].Add(personTaskList[1][t_index + i]);
                    newPersonTaskList[2].Add(newPersonTaskList[3][i - 1]);
                    newPersonTaskList[3].Add(AddMinutes(newPersonTaskList[2][i], Convert.ToInt32(newPersonTaskList[1][i])));
                }
                scon.UpdateTaskTimeToSQL(choosePersonID, newPersonTaskList);

            }
            else
            {
                //从数据库获取数据
                SQLConnect scon = new SQLConnect(server, database, user, pwd);
                //List<string>[] listData = scon.GetDataFromSQL(command1, command2, command3, command4);
                List<string>[] dataBaseData = scon.DataPretreatment();

                //设置GAMS运行路径
                string workPath = Directory.GetCurrentDirectory() + @"\GAMS_workPath";
                Console.WriteLine(workPath);

                //运行GAMS模型
                GAMSModel gModel = new GAMSModel(workPath)
                {
                    Model_N = scon.Model_N,
                    Work_Start_Time = time,
                    conn = "server=" + server + ";database=" + database + ";user=" + user + ";pwd=" + pwd
                }; //GAMS运行模型的工作区，会在此文件夹生成相关调试过程文件

                List<string>[] listData = new List<string>[12];
                Array.Copy(dataBaseData, 0, listData, 0, listData.Length);  //listData的前12个数据
                List<string>[] resultData = gModel.Run(listData);   //运行GAMS模型，得到运行结果

                List<string>[] listTask = new List<string>[3];
                Array.Copy(dataBaseData, 11, listTask, 0, listTask.Length); //listData的后3个数据

                List<string>[] associatedTask = RefactorAssociatedTask(listTask);   //这里发现listTask、dataBaseData的值已经改变了，
                                                                                    //可能是 dataBaseData是指向地址的地址，Array.Copy拷贝的是地址，只是一个引用

                //将模型运行的最终结果存进数据库
                string tableName = scon.SendDataToSQL(resultData, associatedTask, time);
                Console.WriteLine("求解完毕,求解结果保存在" + tableName);

                //开启日志记录
                if (debug == "LOG")
                {
                    string fileName = workPath + @"\Log.csv";
                    Log("-----" + startDT.ToString() + "-----", fileName);
                    Log(dataBaseData, fileName);
                    Log(resultData, fileName);
                    Log(associatedTask, fileName);
                }
            }

            {
                DateTime afterDT = DateTime.Now;
                TimeSpan ts = afterDT.Subtract(startDT);
                Console.WriteLine("求解总共花费{0:0.0}min.", ts.TotalMinutes);
            }
        }

        private static List<string>[] RefactorAssociatedTask(List<string>[] listTask)
        {
            List<string> pointName = listTask[1];
            List<string> oldTask = listTask[2];

            foreach (string name in listTask[0])  //按井组号遍历，处理2、3的任务
            {
                int index1 = pointName.IndexOf(name + "-2"); //返回指定元素的第一个匹配项的索引，或者如果此列表中不包含该元素返回-1
                int index2 = pointName.IndexOf(name + "-3");
                if ((index1 != -1) && (index2 != -1))
                {
                    oldTask[index1] = oldTask[index1] + "|" + oldTask[index2];
                    pointName.RemoveAt(index2);
                    oldTask.RemoveAt(index2);
                }
            }

            List<string>[] associatedTask = new List<string>[2];
            associatedTask[0] = pointName;
            associatedTask[1] = oldTask;

            return associatedTask;
        }

        public static string AddMinutes(string t, int interval)
        {
            DateTime ts = Convert.ToDateTime(t);
            DateTime te = ts.AddMinutes(interval);
            string str_te = te.ToString();

            return str_te;
        }

        public static void Log(List<string>[] data,string fileName)
        {
            Logger logger = new Logger();
            foreach (List<string> item in data)
            {
                string txt = String.Join(",",item.ToArray());
                logger.AddLogToTXT(txt, fileName);
            }
            logger.AddLogToTXT("", fileName);
        }
        public static void Log(string str, string fileName)
        {
            Logger logger = new Logger();
            logger.AddLogToTXT(str, fileName);
        }
    }
}
