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
namespace GAMSHelper
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
        private int model_n = 0;    //模型中的n（n1、n2）值

        public Patrol patrol = new Patrol();

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
        /// Gets the GAMS Model's data from SQL.
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

            //string conn = "server=DESKTOP-36C9L6T;database=lushushu;user=sa;pwd=123";
            string conn = "server=" +server+";database="+database+";user="+user+";pwd="+pwd;
            SqlConnection myconnect;
            myconnect = new SqlConnection(conn);
            myconnect.Open();

            //1
            try
            {
                SqlCommand mycomm1 = new SqlCommand(command1, myconnect);
                SqlDataReader rd1 = mycomm1.ExecuteReader();
                while (rd1.Read())
                {
                    string personId = rd1["PERSON_ID"].ToString();
                    string skillLevel = rd1["SKILL_LEVEL"].ToString();
                    patrol.person_id.Add(personId);
                    patrol.p_skill_level.Add(skillLevel);
                    Pi1.Add(personId);
                    Pi2.Add(skillLevel);
                }
                rd1.Close();
                //myconnect.Close();
            }
            catch(Exception ex)
            {
                throw ex;
            }

            //2
            try
            {
                SqlCommand mycomm2 = new SqlCommand(command2, myconnect);
                SqlDataReader rd2 = mycomm2.ExecuteReader();
                while (rd2.Read())
                {
                    string taskId = rd2["TASK_ID"].ToString();
                    string skillLevel = rd2["SKILL_LEVEL"].ToString();
                    patrol.task_id.Add(taskId);
                    patrol.t_skill_level.Add(skillLevel);
                    TLi1.Add(taskId);
                    TLi2.Add(skillLevel);
                }
                rd2.Close();
                //myconnect.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

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
            catch (Exception ex)
            {
                throw ex;
            }

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
            catch (Exception ex)
            {
                throw ex;
            }

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

            int i1 = patrol.person_id.Count;  //人数
            int i2 = patrol.task_id.Count;    //任务数
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

        public List<string> GetHighLevelPersonIDFromSQL(string command1)
        {
            List<string> personID = new List<string>();

            string conn = "server=" + server + ";database=" + database + ";user=" + user + ";pwd=" + pwd;
            SqlConnection myconnect;
            myconnect = new SqlConnection(conn);
            myconnect.Open();

            try
            {
                SqlCommand mycomm = new SqlCommand(command1, myconnect);
                SqlDataReader rd = mycomm.ExecuteReader();
                while (rd.Read())
                {
                    personID.Add(rd["PERSON_ID"].ToString());
                }
                rd.Close();
                myconnect.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return personID;
        }

        public List<string>[] GetPersonPositionIDFromSQL(string command2)
        {
            List<string> personID = new List<string>();
            List<string> positionID = new List<string>();

            string conn = "server=" + server + ";database=" + database + ";user=" + user + ";pwd=" + pwd;
            SqlConnection myconnect;
            myconnect = new SqlConnection(conn);
            myconnect.Open();

            try
            {
                SqlCommand mycomm = new SqlCommand(command2, myconnect);
                SqlDataReader rd = mycomm.ExecuteReader();
                while (rd.Read())
                {
                    personID.Add(rd["PERSON_ID"].ToString());
                    positionID.Add(rd["POSITION_ID"].ToString());
                }
                rd.Close();
                myconnect.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            List<string>[] listData = new List<string>[2];
            listData[0] = personID;
            listData[1] = positionID;

            return listData;
        }

        public List<int> GetPositionSpendTimeFromSQL(string tableName, List<string> startPosition, string endPosition)
        {
            List<int> spendTime = new List<int>();

            string conn = "server=" + server + ";database=" + database + ";user=" + user + ";pwd=" + pwd;
            SqlConnection myconnect;
            myconnect = new SqlConnection(conn);
            myconnect.Open();

            try
            {
                for (int i = 0; i < startPosition.Count; i++)
                {
                    string command = "SELECT * FROM " + tableName + " WHERE FROM_ID='" + startPosition[i] + "' AND TO_ID='" + endPosition + "';";
                    SqlCommand mycomm = new SqlCommand(command, myconnect);
                    SqlDataReader rd = mycomm.ExecuteReader();
                    while (rd.Read())
                    {
                        spendTime.Add(Convert.ToInt32(rd["SPEND_TIME"]));
                    }
                    rd.Close();
                }
                myconnect.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return spendTime;
        }

        public List<string>[] GetPersonTaskListFromSQL(string personID)
        {
            List<string> taskID = new List<string>();
            List<string> spendTime = new List<string>();
            List<string> startTime = new List<string>();

            SqlConnection myconnect;
            string conn = "server=" + server + ";database=" + database + ";user=" + user + ";pwd=" + pwd + "";
            myconnect = new SqlConnection(conn);
            myconnect.Open();

            string tableName = "IMS_PATROL_TASK_TIME";
            SqlCommand mycomm = new SqlCommand("SELECT * FROM " + tableName + " WHERE PERSON_ID='" + personID + "' ORDER BY ORDER_NO ASC;", myconnect);
            SqlDataReader rd = mycomm.ExecuteReader();
            while (rd.Read())
            {
                taskID.Add(rd["TASK_ID"].ToString());
                spendTime.Add(rd["SPEND_TIME"].ToString());
                startTime.Add(rd["START_TIME"].ToString());
            }
            myconnect.Close();

            List<string>[] p_taskList = new List<string>[3];
            p_taskList[0] = taskID;
            p_taskList[1] = spendTime;
            p_taskList[2] = startTime;

            return p_taskList;
        }

        public void UpdateTaskTimeToSQL(string personID, List<string>[] newPersonTaskList)
        {
            SqlConnection myconnect;
            string conn = "server=" + server + ";database=" + database + ";user=" + user + ";pwd=" + pwd + "";
            myconnect = new SqlConnection(conn);
            myconnect.Open();

            string tableName = "IMS_PATROL_TASK_TIME";
            try
            {
                SqlCommand mycomm1 = new SqlCommand("insert into " + tableName + "(PERSON_ID,TASK_ID,ORDER_NO,SPEND_TIME,START_TIME,END_TIME) values('" + personID + "','" + newPersonTaskList[0][0] + "',99," + Convert.ToInt32(newPersonTaskList[1][0]) + ",'" + newPersonTaskList[2][0] + "','" + newPersonTaskList[3][0] + "');", myconnect);
                mycomm1.ExecuteNonQuery();

                for (int i = 1; i < newPersonTaskList[0].Count; i++)
                {
                    SqlCommand mycomm2 = new SqlCommand("UPDATE " + tableName + " SET START_TIME = '" + newPersonTaskList[2][i] + "', END_TIME = '" + newPersonTaskList[3][i] + "'WHERE PERSON_ID = '" + personID + "' AND TASK_ID = '" + newPersonTaskList[0][i] + "';", myconnect);
                    mycomm2.ExecuteNonQuery();
                }
                myconnect.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
                    SqlCommand mycomm2 = new SqlCommand("CREATE TABLE " + temporarytable + " (Pid varchar(32),Tid1 varchar(32) ,Tid2 varchar(32) ,id varchar(10) ,number bigint ,nowdaystime datetime)", myconnect);
                    n = mycomm2.ExecuteNonQuery();
                }
                catch { }

                for (int i = 0; i < resultData[0].Count; i++)
                {
                    double d_level = Convert.ToDouble(resultData[3][i]);
                    int level = Convert.ToInt32(d_level);

                    DateTime startWorkTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 8, 0, 0);
                    DateTime time = startWorkTime.AddMinutes(level - 1);

                    SqlCommand mycomm3 = new SqlCommand("insert into " + temporarytable + "(Pid ,Tid1,id,number,nowdaystime) values('" + resultData[0][i] + "','" + resultData[1][i] + "','" + resultData[2][i] + "','" + level.ToString() + "','" + time.ToString() + "');", myconnect);
                    mycomm3.ExecuteNonQuery();
                }
                
                for (int i = 4; i < resultData[4].Count; i++)
                {
                    double d_level = Convert.ToDouble(resultData[8][i]);
                    int level = Convert.ToInt32(d_level);

                    DateTime startWorkTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 8, 0, 0);
                    DateTime time = startWorkTime.AddMinutes(level - 1);

                    SqlCommand mycomm4 = new SqlCommand("insert into " + temporarytable + "(Pid ,Tid1,Tid2,id,number,nowdaystime) values('" + resultData[4][i] + "','" + resultData[5][i] + "','" + resultData[6][i] + "','" + resultData[7][i] + "','" + level.ToString() + "','" + time.ToString() + "');", myconnect);
                    mycomm4.ExecuteNonQuery();
                }
               
                myconnect.Close();
            }
            catch (Exception ex)
            {
                throw ex;
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

            int personNum = patrol.person_id.Count;
            string[] pid = new string[personNum];

            for (int i = 0; i < personNum; i++)
            {
                //数据库查询
                //pid[i] = "XJ00" + (i+1).ToString("00");
                pid[i] = patrol.person_id[i];

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
