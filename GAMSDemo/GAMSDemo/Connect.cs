// ***********************************************************************
// Assembly         : GAMSDemo
// Author           : Administrator
// Created          : 03-18-2019
//
// Last Modified By : Administrator
// Last Modified On : 03-19-2019
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
        /// Gets the data from SQL.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="database">The database.</param>
        /// <param name="user">The user.</param>
        /// <param name="pwd">The password.</param>
        /// <returns>List&lt;System.String&gt;[].</returns>
        public List<string>[] GetDataFromSQL(string server,string database,string user,string pwd)
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
            string conn = "server=" +server+";database="+database+";user="+user+";pwd="+pwd+"";
            myconnect = new SqlConnection(conn);
            myconnect.Open();

            //1-1
            try
            {
                SqlCommand mycomm1=new SqlCommand("select PERSON_ID from IMS_PATROL_PERSON_ON_DUTY;", myconnect);
                SqlDataReader rd1 = mycomm1.ExecuteReader();
                while(rd1.Read())
                {
                    int i = rd1.FieldCount;
                    Pi1.Add(rd1.GetValue(0).ToString());
                }
                rd1.Close();
                //myconnect.Close();
            }
            catch 
            {}
            //myconnect.Open();
            //1-2
            try
            {
                SqlCommand mycomm2 = new SqlCommand("select SKILL_LEVEL from IMS_PATROL_PERSON_ON_DUTY;", myconnect);
                SqlDataReader rd2 = mycomm2.ExecuteReader();
                while (rd2.Read())
                {
                    int i = rd2.FieldCount;
                    Pi2.Add(rd2.GetValue(0).ToString());
                }
                rd2.Close();
                //myconnect.Close();
            }
            catch
            { }
            //myconnect.Open();
            //2-1
            try
            {
                SqlCommand mycomm3 = new SqlCommand("select TASK_ID from IMS_PATROL_TASK_SKILL;", myconnect);
                SqlDataReader rd3 = mycomm3.ExecuteReader();
                while (rd3.Read())
                {
                    int i = rd3.FieldCount;
                    TLi1.Add(rd3.GetValue(0).ToString());
                }
                rd3.Close();
                //myconnect.Close();
            }
            catch
            { }
            //myconnect.Open();
            //2-2
            try
            {
                SqlCommand mycomm4 = new SqlCommand("select SKILL_LEVEL from IMS_PATROL_TASK_SKILL;", myconnect);
                SqlDataReader rd4 = mycomm4.ExecuteReader();
                while (rd4.Read())
                {
                    int i = rd4.FieldCount;
                    TLi2.Add(rd4.GetValue(0).ToString());
                }
                rd4.Close();
                //myconnect.Close();
            }
            catch
            { }
            //myconnect.Open();
            //3-1
            try
            {
                SqlCommand mycomm5 = new SqlCommand("select PERSON_ID from IMS_PATROL_PERSON_TASK_TIME;", myconnect);
                SqlDataReader rd5 = mycomm5.ExecuteReader();
                while (rd5.Read())
                {
                    int i = rd5.FieldCount;
                    Tij1.Add(rd5.GetValue(0).ToString());
                }
                rd5.Close();
                //myconnect.Close();
            }
            catch
            { }
            //3-2
            try
            {
                SqlCommand mycomm6 = new SqlCommand("select TASK_ID from IMS_PATROL_PERSON_TASK_TIME;", myconnect);
                SqlDataReader rd6 = mycomm6.ExecuteReader();
                while (rd6.Read())
                {
                    int i = rd6.FieldCount;
                    Tij2.Add(rd6.GetValue(0).ToString());
                }
                rd6.Close();
                //myconnect.Close();
            }
            catch
            { }
            //3-3
            try
            {
                SqlCommand mycomm7 = new SqlCommand("select SPEND_TIME from IMS_PATROL_PERSON_TASK_TIME;", myconnect);
                SqlDataReader rd7 = mycomm7.ExecuteReader();
                while (rd7.Read())
                {
                    int i = rd7.FieldCount;
                    Tij3.Add(rd7.GetValue(0).ToString());
                }
                rd7.Close();
                //myconnect.Close();
            }
            catch
            { }
            //4-1
            try
            {
                SqlCommand mycomm8 = new SqlCommand("select FROM_TASK_ID from IMS_PATROL_TASK_SPEND_TIME;", myconnect);
                SqlDataReader rd8 = mycomm8.ExecuteReader();
                while (rd8.Read())
                {
                    int i = rd8.FieldCount;
                    Tjj1.Add(rd8.GetValue(0).ToString());
                }
                rd8.Close();
                //myconnect.Close();
            }
            catch
            { }
            //4-2
            try
            {
                SqlCommand mycomm9 = new SqlCommand("select TO_TASK_ID from IMS_PATROL_TASK_SPEND_TIME;", myconnect);
                SqlDataReader rd9 = mycomm9.ExecuteReader();
                while (rd9.Read())
                {
                    int i = rd9.FieldCount;
                    Tjj2.Add(rd9.GetValue(0).ToString());
                }
                rd9.Close();
                //myconnect.Close();
            }
            catch
            { }
            //4-3
            try
            {
                SqlCommand mycomm10 = new SqlCommand("select SPEND_TIME from IMS_PATROL_TASK_SPEND_TIME;", myconnect);
                SqlDataReader rd10 = mycomm10.ExecuteReader();
                while (rd10.Read())
                {
                    int i = rd10.FieldCount;
                    Tjj3.Add(rd10.GetValue(0).ToString());
                }
                rd10.Close();
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

            return listData;
        }

        /// <summary>
        /// Sends the data to SQL.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="database">The database.</param>
        /// <param name="user">The user.</param>
        /// <param name="pwd">The password.</param>
        /// <param name="resultData">The result data.</param>
        /// <returns>System.String.</returns>
        public string SendDataToSQL(string server,string database,string user,string pwd, List<string>[] resultData)
        {
            SqlConnection myconnect;
            string conn = "server=" +server+";database="+database+";user="+user+";pwd="+pwd+"";
            myconnect = new SqlConnection(conn);
            myconnect.Open();

            DateTime currentTime = DateTime.Now;
            string temporarytable = "GAMSresult";
            string tableName = "IMS_PATROL_TASK_TIME";
            int n = 0;
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
            List<string>[] listTask = GetTaskTimeFromSQL(server, database, user, pwd, temporarytable);
            SendTaskTimeToSQL(server, database, user, pwd, tableName, listTask);
            return tableName;
        }

        /// <summary>
        /// Gets the task time from SQL.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="database">The database.</param>
        /// <param name="user">The user.</param>
        /// <param name="pwd">The password.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>List&lt;System.String&gt;[].</returns>
        private List<string>[] GetTaskTimeFromSQL(string server, string database, string user, string pwd, string tableName)
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
        /// <param name="server">The server.</param>
        /// <param name="database">The database.</param>
        /// <param name="user">The user.</param>
        /// <param name="pwd">The password.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="listTask">The list task.</param>
        private void SendTaskTimeToSQL(string server, string database, string user, string pwd, string tableName, List<string>[] listTask)
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
