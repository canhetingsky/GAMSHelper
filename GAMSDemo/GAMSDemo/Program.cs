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

            string command4;
            string command1;
            string command3;
            string command2;
            string pwd;
            string user;
            string database;
            string server;
            string debug = "";

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

            SQLConnect scon = new SQLConnect(server, database, user, pwd);
            List<string>[] listData = scon.GetDataFromSQL(command1, command2, command3, command4);

            string workPath = Directory.GetCurrentDirectory() + @"\GAMS_workPath";
            Console.WriteLine(workPath);

            GAMSModel gModel = new GAMSModel(workPath); //GAMS运行模型的工作区，会在此文件夹生成相关调试过程文件
            gModel.Model_N = scon.Model_N;
            List<string>[] resultData = gModel.Run(listData);   //得到GAMS的运行结果

            string tableName = scon.SendDataToSQL(resultData);

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

            DateTime afterDT = DateTime.Now;
            TimeSpan ts = afterDT.Subtract(beforeDT);
            
            Console.WriteLine("求解完毕,求解结果保存在" + tableName);
            Console.WriteLine("求解总共花费{0:0.0}min.", ts.TotalMinutes);
        }
    }
}
