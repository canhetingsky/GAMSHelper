using System;
using System.IO;
using System.Collections.Generic;
using GAMSHelper;

namespace GAMSDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime beforeDT = DateTime.Now;

            //数据库查询语句
            string command1;
            string command2;
            string command3;
            string command4;

            string command = null;

            //数据库相关信息
            string server;  //数据库地址
            string database;    //数据库名称
            string user;        //数据库用户名
            string pwd;         //数据库密码
            string debug = "";  //是否启用日志记录，当debug = "LOG"，启用日志记录

            //紧急任务相关信息
            string taskID = "TS0015";
            string t_positionID = "";
            int skillLevel = 2;
            List<string> personID = new List<string>();
            List<string> p_positionID = new List<string>();
            List<int> p_spendTime = new List<int>();
            string choosePersonID = "";

            #region 输入参数解析
            if (args.Length == 0)
            {
                server = "DESKTOP-36C9L6T";
                database = "lushushu";
                user = "sa";
                pwd = "123";

                command1 = "select * from IMS_PATROL_PERSON_ON_DUTY;";
                command2 = "select * from IMS_PATROL_TASK_SKILL;";
                command3 = "select * from IMS_PATROL_PERSON_TASK_TIME;";
                command4 = "select * from IMS_PATROL_TASK_SPEND_TIME;";
            }
            else if (args.Length == 8)
            {
                server = args[0];
                database = args[1];
                user = args[2];
                pwd = args[3];

                command1 = args[4];
                command2 = args[5];
                command3 = args[6];
                command4 = args[7];
            }
            else if(args.Length == 9)
            {
                server = args[0];
                database = args[1];
                user = args[2];
                pwd = args[3];

                command1 = args[4];
                command2 = args[5];
                command3 = args[6];
                command4 = args[7];
                debug    = args[8];
            }
            else
            {
                Console.WriteLine("数据库参数输入格式不正确，共输入了%d个参数", args.Length);
                return;
            }
            #endregion


            if (true)
            {
                SQLConnect scon = new SQLConnect(server, database, user, pwd);
                List<string>[] listData = scon.GetDataFromSQL(command1, command2, command3, command4);

                //得到可以做此任务的人员ID
                for (int i = 0; i < listData[1].Count; i++)
                {
                    int level = Convert.ToInt32(listData[1][i]);
                    if (level <= skillLevel)
                    {
                        personID.Add(listData[0][i]);
                    }
                }

                //TODO:get person position id
                //...
                List<string>[] listPersonPosition = scon.GetPersonPositionIDFromSQL(command);
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
                string position_tableName = "";
                p_spendTime = scon.GetPositionSpendTimeFromSQL(position_tableName, p_positionID, t_positionID);

                
                //找到到达任务点最快人得ID
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


            }
            else
            {
                //从数据库获取数据
                SQLConnect scon = new SQLConnect(server, database, user, pwd);
                List<string>[] listData = scon.GetDataFromSQL(command1, command2, command3, command4);

                //设置GAMS运行路径
                string workPath = Directory.GetCurrentDirectory() + @"\GAMS_workPath";
                Console.WriteLine(workPath);

                //运行GAMS模型
                GAMSModel gModel = new GAMSModel(workPath); //GAMS运行模型的工作区，会在此文件夹生成相关调试过程文件
                                                            //gModel.Model_N = scon.Model_N;
                gModel.Model_N = scon.Model_N;
                List<string>[] resultData = gModel.Run(listData);   //得到GAMS的运行结果

                //将模型运行的最终结果存进数据库
                string tableName = scon.SendDataToSQL(resultData);
                Console.WriteLine("求解完毕,求解结果保存在" + tableName);

                //开启日志记录
                if (debug == "LOG")
                {
                    string fileName = workPath + @"\Log.txt";
                    Logger logger = new Logger();

                    #region 数据库数据进行记录
                    for (int i = 0; i < 2; i++) //前两个表
                    {
                        for (int j = 0; j < listData[2 * i].Count; j++)
                        {
                            logger.AddLogToTXT(listData[2 * i][j] + "     " + listData[2 * i + 1][j], fileName);
                        }
                        logger.AddLogToTXT("", fileName);
                    }
                    for (int i = 2; i < 4; i++) //后两个表
                    {
                        for (int j = 0; j < listData[3 * i - 2].Count; j++)
                        {
                            logger.AddLogToTXT(listData[3 * i - 2][j] + "     " + listData[3 * i - 1][j] + "     " + listData[3 * i][j], fileName);
                        }
                        logger.AddLogToTXT("", fileName);
                    }
                    #endregion

                    #region GAMS运行结果记录
                    for (int i = 0; i < resultData[0].Count; i++)
                    {
                        logger.AddLogToTXT(resultData[0][i] + "     " + resultData[1][i] + "     " + resultData[2][i] + "     " + resultData[3][i], fileName);
                    }
                    logger.AddLogToTXT("", fileName);
                    for (int i = 4; i < resultData[4].Count; i++)
                    {
                        logger.AddLogToTXT(resultData[4][i] + "     " + resultData[5][i] + "     " + resultData[6][i] + "     " + resultData[7][i], fileName);
                    }
                    #endregion

                    logger.AddLogToTXT("-----------------------------------", fileName);
                }
            }
            
            DateTime afterDT = DateTime.Now;
            TimeSpan ts = afterDT.Subtract(beforeDT);
            Console.WriteLine("求解总共花费{0:0.0}min.", ts.TotalMinutes);
        }
    }
}
