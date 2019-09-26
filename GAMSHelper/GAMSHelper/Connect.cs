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
        /// <summary>
        /// The model n
        /// </summary>
        private int model_n = 0;    //模型中的n（n1、n2）值

        /// <summary>
        /// The patrol
        /// </summary>
        public Patrol patrol = new Patrol();

        /// <summary>
        /// Gets the model n.
        /// </summary>
        /// <value>The model n.</value>
        public int Model_N
        {
            get { return model_n; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLConnect" /> class.
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
        /// Finalizes an instance of the <see cref="SQLConnect" /> class.
        /// </summary>
        ~SQLConnect() { }

        /// <summary>
        /// Datas the pretreatment.
        /// </summary>
        /// <returns>List&lt;System.String&gt;[].</returns>
        public List<string>[] DataPretreatment()
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

            List<string> TLi3 = new List<string>(); //添加任务优先级

            List<string> name = new List<string>(); //所有的井组，不含重复元素
            List<string> pointName = new List<string>(); //任务所属的井组，构建的新的任务号
            List<string> oldTask = new List<string>(); //任务间以 | 隔开
            
            string conn = "server=" + server + ";database=" + database + ";user=" + user + ";pwd=" + pwd;
            SqlConnection myconnect;
            myconnect = new SqlConnection(conn);
            myconnect.Open();

            //1
            try
            {
                string command1 = "select * from IMS_PATROL_PERSON_ON_DUTY;";
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
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //2
            try
            {
                string command1 = "select POINT_NAME from IMS_PATROL_TASK_SKILL ORDER BY TASK_PRIORITY;";
                List<string> pName = new List<string>();
                SqlCommand mycomm1 = new SqlCommand(command1, myconnect);
                SqlDataReader rd1 = mycomm1.ExecuteReader();
                while (rd1.Read())
                {
                    pName.Add(rd1["POINT_NAME"].ToString());   //查找所有的井组，里面有重复的元素，需处理
                }
                rd1.Close();

                foreach (string pn in pName)
                {
                    if (!name.Contains(pn)) //剔除重复元素
                        name.Add(pn);
                }

                foreach (string item in name)   //遍历井组
                {
                    for (int i = 0; i < 4; i++) //遍历井组的优先级
                    {
                        string command2 = String.Format("select * from IMS_PATROL_TASK_SKILL where POINT_NAME='{0}' and TASK_PRIORITY={1};", item, i);
                        SqlCommand mycomm2 = new SqlCommand(command2, myconnect);
                        SqlDataReader rd2 = mycomm2.ExecuteReader();

                        List<int> level = new List<int>();
                        List<string> task = new List<string>();
                        
                        while (rd2.Read())
                        {
                            task.Add(rd2["TASK_ID"].ToString());
                            level.Add(Convert.ToInt32(rd2["SKILL_LEVEL"]));
                        }
                        rd2.Close();

                        if (level.Count > 0)    //此井组的相应优先级任务是否存在
                        {
                            string newTaskID = String.Format("{0}-{1}", item, i);
                            TLi1.Add(newTaskID); 
                            TLi3.Add(i.ToString());

                            level.Sort();   //升序
                            level.Reverse();//反转
                            TLi2.Add(level[0].ToString());
                            
                            pointName.Add(newTaskID);
                            string str = String.Join("|", task.ToArray());
                            oldTask.Add(str);

                            patrol.task_id.Add(String.Format("{0}-{1}", item, i));
                            patrol.t_skill_level.Add(level[0].ToString());
                            patrol.t_priority.Add(i.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //3
            try
            {
                for (int k = 0; k < oldTask.Count; k++)
                {
                    string[] ot = oldTask[k].Split(new char[]{ '|'}, StringSplitOptions.RemoveEmptyEntries);
                    //拼接查询指令
                    foreach (string pid in Pi1)
                    {
                        string command1 = String.Format("select SPEND_TIME from IMS_PATROL_PERSON_TASK_TIME where PERSON_ID='{0}'", pid);
                        command1 += String.Format(" and (TASK_ID='{0}'",ot[0]);
                        if (ot.Length > 1)
                        {
                            for (int i = 1; i < ot.Length; i++)
                            {
                                command1 += String.Format(" or TASK_ID='{0}'", ot[i]);
                            }
                        }
                        command1 += ");";
                        int spt = 0;
                        SqlCommand mycomm1 = new SqlCommand(command1, myconnect);
                        SqlDataReader rd1 = mycomm1.ExecuteReader();
                        while (rd1.Read())
                        {
                            spt += Convert.ToInt32(rd1["SPEND_TIME"]);
                            int timeMax = 999999;
                            spt = spt >= timeMax ? timeMax : spt;
                        }
                        rd1.Close();

                        Tij1.Add(pid);
                        Tij2.Add(TLi1[k]);
                        Tij3.Add(spt.ToString());
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

            //4
            try
            {
                for (int i = 0; i < TLi1.Count; i++)
                {
                    string str1 = TLi1[i].Split(new char[] { '-' })[0];
                    for (int j = 0; j < TLi1.Count; j++)
                    {

                        Tjj1.Add(TLi1[i]);
                        Tjj2.Add(TLi1[j]);

                        string str2 = TLi1[j].Split(new char[] { '-' })[0];
                        if (str2 == str1)
                        {
                            Tjj3.Add("0");
                        }
                        else
                        {
                            string command1 = String.Format("select SPEND_TIME from IMS_PATROL_POINT_SPEND_TIME where FROM_POINT_NAME='{0}' and TO_POINT_NAME='{1}';", str1, str2);
                            SqlCommand mycomm1 = new SqlCommand(command1, myconnect);

                            SqlDataReader rd1 = mycomm1.ExecuteReader();
                            while (rd1.Read())
                            {
                                Tjj3.Add(rd1["SPEND_TIME"].ToString());
                                break;
                            }
                            rd1.Close();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            myconnect.Close();

            int count = 14;
            List<string>[] listData = new List<string>[count];

            listData[0] = Pi1;  //值班人员表：人员ID
            listData[1] = Pi2;  //值班人员表：技能等级

            listData[2] = TLi1; //合并后的任务ID
            listData[3] = TLi2; //合并后的所需人员最低技能等级

            listData[4] = Tij1; //人员ID
            listData[5] = Tij2; //合并后的任务ID
            listData[6] = Tij3; //完成任务的时间

            listData[7] = Tjj1; //合并后的任务ID
            listData[8] = Tjj2; //合并后的任务ID
            listData[9] = Tjj3; //合并后的任务ID

            listData[10] = TLi3; //合并后的任务优先级

            listData[11] = name;    //所有井组（无重复）
            listData[12] = pointName;//新的任务号
            listData[13] = oldTask;  //合并任务号对应的原来的任务，任务间以 | 分隔

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

            List<string> TLi3 = new List<string>(); //添加任务优先级

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
                    string priority = rd2["TASK_PRIORITY"].ToString();
                    patrol.task_id.Add(taskId);
                    patrol.t_skill_level.Add(skillLevel);
                    patrol.t_priority.Add(priority);
                    TLi1.Add(taskId);
                    TLi2.Add(skillLevel);
                    TLi3.Add(priority);
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

            int count = 11;
            List<string>[] listData = new List<string>[count];

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
            listData[10] = TLi3; //任务的优先级

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

        /// <summary>
        /// Gets the high level person identifier from SQL.
        /// </summary>
        /// <param name="command1">The command1.</param>
        /// <returns>List&lt;System.String&gt;.</returns>
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

        /// <summary>
        /// Gets the person position identifier from SQL.
        /// </summary>
        /// <param name="command2">The command2.</param>
        /// <returns>List&lt;System.String&gt;[].</returns>
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

        /// <summary>
        /// Gets the position spend time from SQL.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="startPosition">The start position.</param>
        /// <param name="endPosition">The end position.</param>
        /// <returns>List&lt;System.Int32&gt;.</returns>
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

        /// <summary>
        /// Gets the person task list from SQL.
        /// </summary>
        /// <param name="personID">The person identifier.</param>
        /// <returns>List&lt;System.String&gt;[].</returns>
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

        /// <summary>
        /// Updates the task time to SQL.
        /// </summary>
        /// <param name="personID">The person identifier.</param>
        /// <param name="newPersonTaskList">The new person task list.</param>
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
        /// <param name="associatedTask">The associated task.</param>
        /// <param name="time">The time.</param>
        /// <returns>System.String.</returns>
        //public string SendDataToSQL(List<string>[] resultData)
        //{
        //    SqlConnection myconnect;
        //    string conn = "server=" +server+";database="+database+";user="+user+";pwd="+pwd+"";
        //    myconnect = new SqlConnection(conn);
        //    myconnect.Open();

        //    DateTime currentTime = DateTime.Now;
        //    string temporarytable = "GAMSresult";
        //    string tableName = "IMS_PATROL_TASK_TIME";
        //    int n = 0;

        //    //中间过程数据库
        //    try
        //    {
        //        //delete table
        //        try
        //        {
        //            SqlCommand mycomm1 = new SqlCommand("DROP TABLE " + temporarytable, myconnect);
        //            mycomm1.ExecuteNonQuery();
        //        }
        //        catch { }

        //        try
        //        {
        //            SqlCommand mycomm2 = new SqlCommand("CREATE TABLE " + temporarytable + " (Pid varchar(32),Tid1 varchar(32) ,Tid2 varchar(32) ,id varchar(10) ,number bigint ,nowdaystime datetime)", myconnect);
        //            n = mycomm2.ExecuteNonQuery();
        //        }
        //        catch { }

        //        for (int i = 0; i < resultData[0].Count; i++)
        //        {
        //            double d_level = Convert.ToDouble(resultData[3][i]);
        //            int level = Convert.ToInt32(d_level);

        //            DateTime startWorkTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 8, 0, 0);
        //            DateTime time = startWorkTime.AddMinutes(level - 1);

        //            SqlCommand mycomm3 = new SqlCommand("insert into " + temporarytable + "(Pid ,Tid1,id,number,nowdaystime) values('" + resultData[0][i] + "','" + resultData[1][i] + "','" + resultData[2][i] + "','" + level.ToString() + "','" + time.ToString() + "');", myconnect);
        //            mycomm3.ExecuteNonQuery();
        //        }

        //        for (int i = 4; i < resultData[4].Count; i++)
        //        {
        //            double d_level = Convert.ToDouble(resultData[8][i]);
        //            int level = Convert.ToInt32(d_level);

        //            DateTime startWorkTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 8, 0, 0);
        //            DateTime time = startWorkTime.AddMinutes(level - 1);

        //            SqlCommand mycomm4 = new SqlCommand("insert into " + temporarytable + "(Pid ,Tid1,Tid2,id,number,nowdaystime) values('" + resultData[4][i] + "','" + resultData[5][i] + "','" + resultData[6][i] + "','" + resultData[7][i] + "','" + level.ToString() + "','" + time.ToString() + "');", myconnect);
        //            mycomm4.ExecuteNonQuery();
        //        }

        //        myconnect.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //    List<string>[] listTask = GetTaskTimeFromSQL(temporarytable);
        //    SendTaskTimeToSQL(tableName, listTask);

        //    return tableName;
        //}

        public string SendDataToSQL(List<string>[] resultData, List<string>[] associatedTask, string time)
        {
            SqlConnection myconnect;
            string conn = "server=" + server + ";database=" + database + ";user=" + user + ";pwd=" + pwd + "";
            myconnect = new SqlConnection(conn);
            myconnect.Open();

            DateTime currentTime = DateTime.Now;
            string temporarytable = "GAMSresult";

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
                    SqlCommand mycomm2 = new SqlCommand("CREATE TABLE " + temporarytable + " (Pid varchar(32),number bigint ,time1 datetime ,time2 datetime)", myconnect);
                    mycomm2.ExecuteNonQuery();
                }
                catch { }

                for (int i = 0; i < resultData[0].Count; i++)
                {
                    //int level1 = Convert.ToInt32(resultData[2][i]);
                    //int level2 = Convert.ToInt32(resultData[5][i]);
                    //求解结果里面的时间（level）可能有小数
                    int level1 = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(resultData[2][i]))); //Ts_time
                    int level2 = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(resultData[5][i]))); //Tf_time

                    //起始时间
                    string[] t = time.Split(':');
                    int HH = Convert.ToInt32(t[0]);
                    int MM = Convert.ToInt32(t[1]);
                    DateTime startWorkTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, HH, MM, 0);

                    DateTime time1 = startWorkTime.AddMinutes(level1 - 1);
                    DateTime time2 = startWorkTime.AddMinutes(level2 - 1);
                    
                    SqlCommand mycomm3 = new SqlCommand("insert into " + temporarytable + "(Pid,number,time1,time2) values('" + resultData[0][i] + "'," + Convert.ToInt32(resultData[1][i]) + ",'" + time1.ToString() + "','" + time2.ToString() + "');", myconnect);
                    mycomm3.ExecuteNonQuery();
                }
                myconnect.Close();
        }
            catch (Exception ex)
            {
                throw ex;
            }

            string tableName = "IMS_PATROL_TASK_TIME";  //最终数据库
            List<string>[] XS = new List<string>[3];
            XS[0] = resultData[6];
            XS[1] = resultData[7];
            XS[2] = resultData[8];
            List<string>[] listTask = GetTaskTimeFromSQL(temporarytable,XS);
            SendTaskTimeToSQL(tableName, listTask, associatedTask);

            return tableName;
        }

        /// <summary>
        /// Gets the task time from SQL.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="XS">The xs.</param>
        /// <returns>List&lt;System.String&gt;[].</returns>
        private List<string>[] GetTaskTimeFromSQL(string tableName,List<string>[] XS)
        {
            List<string> PERSON_ID = new List<string>();
            List<string> TASK_ID = new List<string>();
            List<string> ORDER_NO = new List<string>();
            List<string> SPEND_TIME = new List<string>();
            List<string> START_TIME = new List<string>();
            List<string> BETWEEN_TIME = new List<string>();
            List<string> END_TIME = new List<string>();

            for (int i = 0; i < XS[0].Count; i++)
            {
                PERSON_ID.Add(XS[0][i]);
                TASK_ID.Add(XS[1][i]);
                ORDER_NO.Add(XS[2][i]);

                string Pid = XS[0][i];
                int order = Convert.ToInt32(XS[2][i]);
                string[] time = GetStartAndEndTimeFromSQL(tableName, Pid, order);   //从 GAMSresult 表获取任务起止时间

                string startTime = time[0];
                string betweenTime = time[1];
                string endTime = time[2];
                DateTime ts = Convert.ToDateTime(startTime);
                DateTime te = Convert.ToDateTime(endTime);
                TimeSpan td = te.Subtract(ts);
                double spendTime = td.TotalMinutes;

                SPEND_TIME.Add(spendTime.ToString());
                START_TIME.Add(startTime);
                BETWEEN_TIME.Add(betweenTime);
                END_TIME.Add(endTime);
            }

            List<string>[] listData = new List<string>[7];
            listData[0] = PERSON_ID;
            listData[1] = TASK_ID;
            listData[2] = ORDER_NO;
            listData[3] = SPEND_TIME;
            listData[4] = START_TIME;
            listData[5] = BETWEEN_TIME;
            listData[6] = END_TIME;

            return listData;
        }

        /// <summary>
        /// Gets the start and end time from SQL.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="Pid">The pid.</param>
        /// <param name="order">The order.</param>
        /// <returns>System.String[].</returns>
        private string[] GetStartAndEndTimeFromSQL(string tableName,string Pid, int order)
        {
            string[] time = new string[3];

            SqlConnection myconnect;
            string conn = "server=" + server + ";database=" + database + ";user=" + user + ";pwd=" + pwd + "";
            myconnect = new SqlConnection(conn);
            myconnect.Open();

            try
            {
                SqlCommand mycomm1 = new SqlCommand("SELECT time1 FROM " + tableName + " WHERE number=" + (order - 1) + " AND Pid='" + Pid + "';", myconnect);
                time[0] = mycomm1.ExecuteScalar().ToString();
                //SqlDataReader rd1 = mycomm1.ExecuteReader();
                //while (rd1.Read())
                //{
                //    time[0] = rd1["time1"].ToString();
                //    break;
                //}
                //rd1.Close();

                SqlCommand mycomm2 = new SqlCommand("SELECT * FROM " + tableName + " WHERE number=" + order + " AND Pid='" + Pid + "';", myconnect);
                SqlDataReader rd2 = mycomm2.ExecuteReader();
                while (rd2.Read())
                {
                    time[1] = rd2["time1"].ToString();
                    time[2] = rd2["time2"].ToString();
                    break;
                }
                rd2.Close();

                myconnect.Close();
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return time;
        }

        /// <summary>
        /// Gets the task time from SQL.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>List&lt;System.String&gt;[].</returns>
        //private List<string>[] GetTaskTimeFromSQL(string tableName)
        //{
        //    List<string>[] listData = new List<string>[6];

        //    List<string> PERSON_ID = new List<string>();
        //    List<string> TASK_ID = new List<string>();
        //    List<string> ORDER_NO = new List<string>();
        //    List<string> SPEND_TIME = new List<string>();
        //    List<string> START_TIME = new List<string>();
        //    List<string> END_TIME = new List<string>();

        //    SqlConnection myconnect;
        //    string conn = "server=" + server + ";database=" + database + ";user=" + user + ";pwd=" + pwd + "";
        //    myconnect = new SqlConnection(conn);
        //    myconnect.Open();

        //    int personNum = patrol.person_id.Count;
        //    string[] pid = new string[personNum];

        //    for (int i = 0; i < personNum; i++)
        //    {
        //        //数据库查询
        //        //pid[i] = "XJ00" + (i+1).ToString("00");
        //        pid[i] = patrol.person_id[i];

        //        SqlCommand mycomm = new SqlCommand("SELECT COUNT(*) AS pcount FROM " + tableName + " WHERE number!=0 AND Pid='" + pid[i] + "';", myconnect);
        //        SqlDataReader rd = mycomm.ExecuteReader();
        //        int queryNumber = 0;
        //        if (rd.Read())
        //        {
        //            queryNumber = Convert.ToInt32(rd["pcount"]);
        //            rd.Close();
        //            if (queryNumber == 0)
        //            {
        //                continue;
        //            }
        //        }
                
        //        for (int j = 2; j < queryNumber; j=j+2)
        //        {
        //            PERSON_ID.Add(pid[i]);
        //            ORDER_NO.Add(j.ToString());

        //            SqlCommand mycomm1 = new SqlCommand("SELECT * FROM " + tableName + " WHERE id="+j.ToString
        //                ()+" AND number!=0 AND Pid='" + pid[i] + "' ORDER BY number ASC;", myconnect);
        //            SqlDataReader rd1 = mycomm1.ExecuteReader();
        //            while (rd1.Read())
        //            {
        //                TASK_ID.Add(rd1.GetValue(1).ToString());
        //                break;
        //            }
        //            rd1.Close();

        //            SqlCommand mycomm2 = new SqlCommand("SELECT * FROM " + tableName + " WHERE id=" + (j - 1).ToString
        //                () + " AND number!=0 AND Pid='" + pid[i] + "' ORDER BY number ASC;", myconnect);
        //            SqlDataReader rd2 = mycomm2.ExecuteReader();
        //            string startTime = null;
        //            if (rd2.Read())
        //            {
        //                startTime = rd2.GetValue(5).ToString();
        //            }
        //            START_TIME.Add(startTime);
        //            rd2.Close();

        //            SqlCommand mycomm3 = new SqlCommand("SELECT * FROM " + tableName + " WHERE id=" + (j+1).ToString
        //                () + " AND number!=0 AND Pid='" + pid[i] + "' ORDER BY number ASC;", myconnect);
        //            SqlDataReader rd3 = mycomm3.ExecuteReader();
        //            string endTime = null;
        //            while (rd3.Read())
        //            {
        //                endTime = rd3.GetValue(5).ToString();
        //                break;
        //            }
        //            END_TIME.Add(endTime);
        //            rd3.Close();

        //            DateTime ts = Convert.ToDateTime(startTime);
        //            DateTime te = Convert.ToDateTime(endTime);
        //            TimeSpan td = te.Subtract(ts);
        //            double spendTime = td.TotalMinutes;

        //            SPEND_TIME.Add(spendTime.ToString());
        //        }
        //    }
            
        //    listData[0] = PERSON_ID;
        //    listData[1] = TASK_ID;
        //    listData[2] = ORDER_NO;
        //    listData[3] = SPEND_TIME;
        //    listData[4] = START_TIME;
        //    listData[5] = END_TIME;

        //    return listData;
        //}

        /// <summary>
        /// Sends the task time to SQL.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="listTask">The list task.</param>
        /// <param name="associatedTask">The associated task.</param>
        private void SendTaskTimeToSQL(string tableName, List<string>[] listTask, List<String>[] associatedTask)
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
                    //原始任务分割插入数据库
                    int index = associatedTask[0].FindIndex(item => item.Equals(listTask[1][i]));
                    string oldTask = associatedTask[1][index];
                    string[] ot = oldTask.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                    string lastTaskId = null;
                    string startTime;
                    string endTime = listTask[6][i];    //任务倒着插进去
                    for (int j = 0; j < ot.Length; j++)
                    {
                        string item = ot[j];
                        string spendTime = GetPersonTaskTime(listTask[0][i], item);  //从数据库查询花费时间
                        startTime = ReduceMinutes(endTime, Convert.ToInt32(spendTime));

                        DateTime t1 = Convert.ToDateTime(startTime);
                        DateTime t2 = Convert.ToDateTime(listTask[5][i]);
                        int compNum = DateTime.Compare(t1, t2);
                        //t1 < t2
                        if (compNum < 0)
                        {
                            break;
                        }
                        else
                        {
                            string str1 = String.Format("insert into {0}(PERSON_ID ,TASK_ID,ORDER_NO,SPEND_TIME,START_TIME,END_TIME) values('{1}','{2}','{3}','{4}','{5}','{6}');", tableName, listTask[0][i], item, listTask[2][i], spendTime, startTime, endTime);
                            SqlCommand mycomm1 = new SqlCommand(str1, myconnect);
                            mycomm1.ExecuteNonQuery();
                            endTime = startTime;    //上一次的结束时间是本次的开始时间
                            lastTaskId = item;  //最后一次插入的任务，起始时间需要修改
                        }
                    }

                    if (lastTaskId != null)
                    {
                        //修改最后一个任务的起始时间为任务的迁移时间
                        string str2 = String.Format("update {0} set START_TIME = '{1}' where PERSON_ID = '{2}' and TASK_ID = '{3}'; ", tableName, listTask[4][i], listTask[0][i], lastTaskId);
                        SqlCommand mycomm2 = new SqlCommand(str2, myconnect);
                        mycomm2.ExecuteNonQuery();
                    }
                }
                myconnect.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Gets the person task time.
        /// </summary>
        /// <param name="personID">The person identifier.</param>
        /// <param name="taskID">The task identifier.</param>
        /// <returns>System.String.</returns>
        private string GetPersonTaskTime(string personID, string taskID)
        {
            string conn = "server=" + server + ";database=" + database + ";user=" + user + ";pwd=" + pwd;
            SqlConnection myconnect = new SqlConnection(conn);
            myconnect.Open();
            string spendTime = null;
            try
            {
                string command = String.Format("select SPEND_TIME from IMS_PATROL_PERSON_TASK_TIME where PERSON_ID='{0}' and TASK_ID='{1}';", personID, taskID);
                SqlCommand mycomm = new SqlCommand(command, myconnect);
                spendTime = mycomm.ExecuteScalar().ToString();
                myconnect.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return spendTime;
        }

        /// <summary>
        /// Reduces the minutes.
        /// </summary>
        /// <param name="t">The time string.</param>
        /// <param name="interval">The interval.</param>
        /// <returns>System.String.</returns>
        private string ReduceMinutes(string t, int interval)
        {
            DateTime ts = Convert.ToDateTime(t);
            DateTime te = ts.AddMinutes(-interval);
            string str_te = te.ToString();

            return str_te;
        }
    }
}
