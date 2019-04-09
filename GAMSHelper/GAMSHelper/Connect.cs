// ***********************************************************************
// Assembly         : GAMSDemo
// Author           : Administrator
// Created          : 03-18-2019
//
// Last Modified By : Administrator
// Last Modified On : 03-28-2019
// ***********************************************************************
// <copyright file="Connect.cs" company="Microsoft">
//     Copyright © Microsoft 2019
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;

/// <summary>
/// The Connect namespace.
/// </summary>
namespace Connect
{
    /// <summary>
    /// Class SQLConnect.
    /// </summary>
    public class SQLConnect
    {
        /// <summary>
        /// The server
        /// </summary>
        private string server = null;
        /// <summary>
        /// The database
        /// </summary>
        private string database = null;
        /// <summary>
        /// The user
        /// </summary>
        private string user = null;
        /// <summary>
        /// The password
        /// </summary>
        private string pwd = null;
        private int model_n = 0;

        public int Model_N
        {
            get { return model_n; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLConnect"/> class.
        /// </summary>
        /// <param name="str_server">The str_server.</param>
        /// <param name="str_database">The str_database.</param>
        /// <param name="str_user">The str_user.</param>
        /// <param name="str_pwd">The STR_PWD.</param>
        public SQLConnect(string str_server, string str_database, string str_user, string str_pwd)
        {
            server = str_server;
            database = str_database;
            user = str_user;
            pwd = str_pwd;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SQLConnect"/> class.
        /// </summary>
        ~SQLConnect() { }

        /// <summary>
        /// Gets the data from SQL.
        /// </summary>
        /// <param name="command1">The command1.</param>
        /// <param name="command2">The command2.</param>
        /// <param name="command3">The command3.</param>
        /// <param name="command4">The command4.</param>
        /// <returns>List&lt;System.String&gt;[].</returns>
        public List<string>[] GetDataFromSQL(string command1, string command2, string command3, string command4)
        {
            List<string> Pi1 = new List<string>();
            List<string> Pi2 = new List<string>();
            List<string> TLi1 = new List<string>();
            List<string> TLi2 = new List<string>();
            List<string> Tij1 = new List<string>();
            List<string> Tij2 = new List<string>();
            List<string> Tij3 = new List<string>();
            List<string> Tjj1 = new List<string>();
            List<string> Tjj2 = new List<string>();
            List<string> Tjj3 = new List<string>();

            SqlConnection myconnect;
            //string conn = "server=DESKTOP-36C9L6T;database=lushushu;user=sa;pwd=123";
            string conn = "server=" +server+";database="+database+";user="+user+";pwd="+pwd;
            myconnect = new SqlConnection(conn);
            myconnect.Open();

            //1
            int i1 = 0;
            try
            {
                SqlCommand mycomm1 = new SqlCommand(command1, myconnect);
                SqlDataReader rd1 = mycomm1.ExecuteReader();
                while (rd1.Read())
                {
                    i1++;
                    Pi1.Add(rd1["PERSON_ID"].ToString());
                    Pi2.Add(rd1["SKILL_LEVEL"].ToString());
                }
                rd1.Close();
                //myconnect.Close();
            }
            catch
            { }

            //2
            int i2 = 0;
            try
            {
                SqlCommand mycomm2 = new SqlCommand(command2, myconnect);
                SqlDataReader rd2 = mycomm2.ExecuteReader();
                while (rd2.Read())
                {
                    i2++;
                    TLi1.Add(rd2["TASK_ID"].ToString());
                    TLi2.Add(rd2["SKILL_LEVEL"].ToString());
                }
                rd2.Close();
                //myconnect.Close();
            }
            catch
            { }

            //3
            try
            {
                SqlCommand mycomm3 = new SqlCommand(command3, myconnect);
                SqlDataReader rd3 = mycomm3.ExecuteReader();
                while (rd3.Read())
                {
                    int i = rd3.FieldCount;
                    Tij1.Add(rd3["PERSON_ID"].ToString());
                    Tij2.Add(rd3["TASK_ID"].ToString());
                    Tij3.Add(rd3["SPEND_TIME"].ToString());
                }
                rd3.Close();
                //myconnect.Close();
            }
            catch
            { }

            //4
            try
            {
                SqlCommand mycomm4 = new SqlCommand(command4, myconnect);
                SqlDataReader rd4 = mycomm4.ExecuteReader();
                while (rd4.Read())
                {
                    int i = rd4.FieldCount;
                    Tjj1.Add(rd4["FROM_TASK_ID"].ToString());
                    Tjj2.Add(rd4["TO_TASK_ID"].ToString());
                    Tjj3.Add(rd4["SPEND_TIME"].ToString());
                }
                rd4.Close();
                myconnect.Close();
            }
            catch
            { }

            List<string>[] listData = new List<string>[10];

            listData[0] = Pi1;
            listData[1] = Pi2;
            listData[2] = TLi1;
            listData[3] = TLi2;
            listData[4] = Tij1;
            listData[5] = Tij2;
            listData[6] = Tij3;
            listData[7] = Tjj1;
            listData[8] = Tjj2;
            listData[9] = Tjj3;

            //n=((任务数-1)/人数+1)*2
            if ((i2 - 1) % i1 == 0)
            {
                model_n = ((i2 - 1) / i1 + 1) * 2;
            }
            else
            {
                model_n = ((i2 - 1) / i1 + 2) * 2;
            }
            
            return listData;
        }

        /// <summary>
        /// Sends the data to SQL.
        /// </summary>
        /// <param name="resultData">The result data.</param>
        /// <returns>System.String.</returns>
        public string SendDataToSQL(List<string>[] resultData)
        {
            SqlConnection myconnect;
            string conn = "server=" +server+";database="+database+";user="+user+";pwd="+pwd+"";
            myconnect = new SqlConnection(conn);
            myconnect.Open();

            DateTime currentTime = DateTime.Now;
            string temporarytable = "GAMSresult";
            string tableName = "IMS_PATROL_TASK_TIME";
            int n = 0;

            //中间过程数据库
            try
            {
                //delete table
                try
                {
                    SqlCommand mycomm1 = new SqlCommand("DROP TABLE " + temporarytable, myconnect);
                    mycomm1.ExecuteNonQuery();
                }
                catch { }

                try
                {
                    SqlCommand mycomm2 = new SqlCommand("CREATE TABLE " + temporarytable + " (Pid varchar(10),Tid1 varchar(10) ,Tid2 varchar(10) ,id varchar(10) ,number bigint ,nowdaystime datetime)", myconnect);
                    n = mycomm2.ExecuteNonQuery();
                }
                catch { }

                for (int i = 0; i < resultData[0].Count; i++)
                {
                    DateTime startWorkTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 8, 0, 0);
                    DateTime time = startWorkTime.AddMinutes(Convert.ToInt32(resultData[3][i]) - 1);
                    //Debug.WriteLine(time);

                    SqlCommand mycomm3 = new SqlCommand("insert into "+ temporarytable + "(Pid ,Tid1,id,number,nowdaystime) values('" + resultData[0][i] + "','" + resultData[1][i] + "','" + resultData[2][i] + "','" + Convert.ToInt32(resultData[3][i]) + "','"+ time.ToString() + "');", myconnect);
                    mycomm3.ExecuteNonQuery();
                }
                
                for (int i = 4; i < resultData[4].Count; i++)
                {
                    DateTime startWorkTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 8, 0, 0);
                    DateTime time = startWorkTime.AddMinutes(Convert.ToInt32(resultData[8][i]) - 1);
                    //Debug.WriteLine(time);

                    SqlCommand mycomm4 = new SqlCommand("insert into " + temporarytable + "(Pid ,Tid1,Tid2,id,number,nowdaystime) values('" + resultData[4][i] + "','" + resultData[5][i] + "','" + resultData[6][i] + "','" + resultData[7][i] + "','" + Convert.ToInt32(resultData[8][i]) + "','" + time + "');", myconnect);
                    mycomm4.ExecuteNonQuery();
                }

                myconnect.Close();
            }
            catch (Exception ex)
            {
                //throw ex;
            }

            List<string>[] listTask = GetTaskTimeFromSQL(temporarytable);
            SendTaskTimeToSQL(tableName, listTask);

            return tableName;
        }

        /// <summary>
        /// Gets the task time from SQL.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>List&lt;System.String&gt;[].</returns>
        private List<string>[] GetTaskTimeFromSQL(string tableName)
        {
            List<string>[] listData = new List<string>[6];

            List<string> PERSON_ID = new List<string>();
            List<string> TASK_ID = new List<string>();
            List<string> ORDER_NO = new List<string>();
            List<string> SPEND_TIME = new List<string>();
            List<string> START_TIME = new List<string>();
            List<string> END_TIME = new List<string>();

            SqlConnection myconnect;
            string conn = "server=" + server + ";database=" + database + ";user=" + user + ";pwd=" + pwd + "";
            myconnect = new SqlConnection(conn);
            myconnect.Open();

            string[] pid = new string[5];

            for (int i = 0; i < 5; i++)
            {
                //数据库查询
                pid[i] = "XJ00" + (i+1).ToString("00");

                SqlCommand mycomm = new SqlCommand("SELECT COUNT(*) AS pcount FROM " + tableName + " WHERE number!=0 AND Pid='" + pid[i] + "';", myconnect);
                SqlDataReader rd = mycomm.ExecuteReader();
                int queryNumber = 0;
                if (rd.Read())
                {
                    queryNumber = Convert.ToInt32(rd["pcount"]);
                    rd.Close();
                    if (queryNumber == 0)
                    {
                        continue;
                    }
                }
                
                for (int j = 2; j < queryNumber; j=j+2)
                {
                    PERSON_ID.Add(pid[i]);
                    ORDER_NO.Add(j.ToString());

                    SqlCommand mycomm1 = new SqlCommand("SELECT * FROM " + tableName + " WHERE id="+j.ToString
                        ()+" AND number!=0 AND Pid='" + pid[i] + "' ORDER BY number ASC;", myconnect);
                    SqlDataReader rd1 = mycomm1.ExecuteReader();
                    while (rd1.Read())
                    {
                        TASK_ID.Add(rd1.GetValue(1).ToString());
                        break;
                    }
                    rd1.Close();

                    SqlCommand mycomm2 = new SqlCommand("SELECT * FROM " + tableName + " WHERE id=" + (j - 1).ToString
                        () + " AND number!=0 AND Pid='" + pid[i] + "' ORDER BY number ASC;", myconnect);
                    SqlDataReader rd2 = mycomm2.ExecuteReader();
                    string startTime = null;
                    if (rd2.Read())
                    {
                        startTime = rd2.GetValue(5).ToString();
                    }
                    START_TIME.Add(startTime);
                    rd2.Close();

                    SqlCommand mycomm3 = new SqlCommand("SELECT * FROM " + tableName + " WHERE id=" + (j+1).ToString
                        () + " AND number!=0 AND Pid='" + pid[i] + "' ORDER BY number ASC;", myconnect);
                    SqlDataReader rd3 = mycomm3.ExecuteReader();
                    string endTime = null;
                    while (rd3.Read())
                    {
                        endTime = rd3.GetValue(5).ToString();
                        break;
                    }
                    END_TIME.Add(endTime);
                    rd3.Close();

                    DateTime ts = Convert.ToDateTime(startTime);
                    DateTime te = Convert.ToDateTime(endTime);
                    TimeSpan td = te.Subtract(ts);
                    double spendTime = td.TotalMinutes;

                    SPEND_TIME.Add(spendTime.ToString());
                }
            }
            
            listData[0] = PERSON_ID;
            listData[1] = TASK_ID;
            listData[2] = ORDER_NO;
            listData[3] = SPEND_TIME;
            listData[4] = START_TIME;
            listData[5] = END_TIME;

            return listData;
        }
        /// <summary>
        /// Sends the task time to SQL.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="listTask">The list task.</param>
        private void SendTaskTimeToSQL(string tableName, List<string>[] listTask)
        {
            SqlConnection myconnect;
            string conn = "server=" + server + ";database=" + database + ";user=" + user + ";pwd=" + pwd + "";
            myconnect = new SqlConnection(conn);
            myconnect.Open();

            try
            {
                SqlCommand mycomm = new SqlCommand("truncate table " + tableName + ";", myconnect);
                mycomm.ExecuteNonQuery();

                for (int i = 0; i < listTask[0].Count; i++)
                {
                    SqlCommand mycomm1 = new SqlCommand("insert into " + tableName + "(PERSON_ID ,TASK_ID,ORDER_NO,SPEND_TIME,START_TIME,END_TIME) values('" + listTask[0][i] + "','" + listTask[1][i] + "','" + listTask[2][i] + "','" + listTask[3][i] + "','" + listTask[4][i] + "','" + listTask[5][i] +  "');", myconnect);
                    mycomm1.ExecuteNonQuery();
                }
                
                myconnect.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
