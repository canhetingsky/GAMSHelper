using System;
using System.IO;
using System.Collections.Generic;
using GAMSHelper;
using Connect;

namespace GAMSDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            string server = null;
            string database = null;
            string user = null;
            string pwd = null;
            if (args.Length == 0)
            {
                server = "DESKTOP-36C9L6T";
                database = "lushushu";
                user = "sa";
                pwd = "123";
            }
            else if (args.Length == 4)
            {
                server = args[0];
                database = args[1];
                user = args[2];
                pwd = args[3];
                Console.WriteLine(server+ database+ user+ pwd);
            }
            else
                return;
            
            SQLConnect scon = new SQLConnect();
            List<string>[] listData = scon.GetDataFromSQL(server, database, user, pwd);

            #region 打印来自数据库的数据
            //for (int i = 0; i < 2; i++) //前两个表
            //{
            //    for (int j = 0; j < listData[2 * i].Count; j++)
            //    {
            //        Console.WriteLine(listData[2 * i][j] + "    " + listData[2 * i + 1][j]);
            //    }
            //    Console.WriteLine(" ");
            //}
            //for (int i = 2; i < 4; i++)
            //{
            //    for (int j = 0; j < listData[3 * i - 2].Count; j++)
            //    {
            //        Console.WriteLine(listData[3 * i - 2][j] + "    " + listData[3 * i - 1][j] + "    " + listData[3 * i][j]);
            //    }
            //    Console.WriteLine(" ");
            //}
            #endregion

            string workPath = Directory.GetCurrentDirectory()+ @"\GAMS_workPath";
            Console.WriteLine(workPath);

            GAMSModel gModel = new GAMSModel(workPath); //GAMS运行模型的工作区，会在此文件夹生成相关调试过程文件
            List<string>[] resultData = gModel.Run(listData);   //得到GAMS的运行结果
            string tableName = scon.SendDataToSQL(server, database, user, pwd, resultData);
            
            Console.WriteLine("求解完毕,求解结果保存在" + tableName);
        }
    }
}
