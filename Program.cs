using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Data.SqlClient;
using System.Threading;
using System.Data;

namespace EpicIntegrator
{
    class Program
    {

        static string ConnectionStrings = ConfigurationManager.ConnectionStrings["CBLReporting"].ConnectionString;
        public static SqlConnection conns = new SqlConnection(ConnectionStrings);
        public static string DBtable = "[CBL_Reporting].[dbo].[wawanesaGRCRemovalList]";

        static void Main(string[] args)
        {
            //ActToDB();

            InsertActivities();

            //TestActivity();
        }

        static void ActToDB()
        {
            
            List<Dictionary<string, string>> ActivitiesInsertList = new List<Dictionary<string, string>>();
            string ActClosePath = @"C:\Users\Abhishek\Documents\PR566\"; //Final Check
            string ActCloseFilePath = ActClosePath + "ActivitiesToInsert.csv";
            string SQLFilePath = ActClosePath + "SQLAddActivity.txt";

            string SQLText = System.IO.File.ReadAllText(SQLFilePath);

            // CSV to List of Dicts
            //I have created a list of dicts from the csv, so that it can be converted to JSON, if required, in future
            string[] lines = System.IO.File.ReadAllLines(ActCloseFilePath);
            foreach (string line in lines)
            {
                Dictionary<string, string> tempDict = new Dictionary<string, string>();
                string[] columns = line.Split(',');
                tempDict.Add("PolicyNumber", columns[0]);
                tempDict.Add("ExpiryDate", (DateTime.Parse(columns[1])).ToString("yyyy-MM-dd"));
                ActivitiesInsertList.Add(tempDict);
            }

            // List of Dicts to DB Table

            Console.WriteLine("CONNS STATE " + conns.State);
            if (conns.State == ConnectionState.Closed)
            {
                conns.Open();
            }
            Console.WriteLine("CONNS STATE NEW " + conns.State);
            System.Threading.Thread.Sleep(2000);
            int DbCounter = 0;
            foreach (Dictionary<string, string> item in ActivitiesInsertList)
            {
                string PolNumber = item["PolicyNumber"];
                string ExpDate = item["ExpiryDate"];
                using (SqlCommand commandOne = conns.CreateCommand())
                {

                    string sqltwo = string.Format(SQLText, DBtable);
                    commandOne.CommandText = sqltwo;

                    commandOne.Parameters.AddWithValue("@PolNum", PolNumber);
                    commandOne.Parameters.AddWithValue("@PolExp", ExpDate);
                    commandOne.ExecuteNonQuery();
                }
                DbCounter++;

            }
            conns.Close();
            Console.WriteLine("Policies inserted to DB : " + DbCounter);
            Console.WriteLine("CONNS STATE " + conns.State);
            Console.WriteLine("*-*-*");
            Console.ReadKey();

        }
        
       
        static void TestActivity()
        {
            EpicIntegrator.ActivityService acs = new EpicIntegrator.ActivityService();
            //acs.InsertActivity(61629254);
            Console.ReadKey();

        }


        static void InsertActivities()
        {
            string ErrorFilePath = @"C:\Users\Abhishek\Documents\PR566\ActCloseErrorLog_";
            string ErrorList = "";
            string err1 = "";
            int ErrorCount = 0;

            List<Tuple<int, int, int>> PolsList = new List<Tuple<int, int, int>>();
                       
            EpicIntegrator.ActivityService acs = new EpicIntegrator.ActivityService();
            EpicIntegrator.ClientService ccs = new EpicIntegrator.ClientService();
            EpicIntegrator.PolicyService pcs = new EpicIntegrator.PolicyService();

            var ActCloseStart = DateTime.Now;
            Console.WriteLine("Code Started: " + ActCloseStart);

            // Read data from DB
            Console.WriteLine("CONNS STATE " + conns.State);
            if (conns.State == ConnectionState.Closed)
            {
                conns.Open();
            }
            Console.WriteLine("CONNS STATE NEW " + conns.State);
            System.Threading.Thread.Sleep(2000);

            using (SqlCommand commandZero = conns.CreateCommand())
            {
                string sqlRead = string.Format("SELECT SNo, UniqPolicy, ClientID FROM {0} where ActivityCreatedFlag = 0;", DBtable);
                commandZero.CommandText = sqlRead;
                SqlDataReader rdr = commandZero.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        int SNo = Convert.ToInt32(rdr["SNo"].ToString());
                        int PolID = Convert.ToInt32(rdr["UniqPolicy"].ToString());
                        int ClientID = Convert.ToInt32(rdr["ClientID"].ToString());
                        var PolicyResult = Tuple.Create<int, int, int>(SNo, PolID, ClientID);
                        PolsList.Add(PolicyResult);
                    }
                }
                else
                {
                    Console.WriteLine("No policies to read in SQL");
                }
                rdr.Close();
            }

            Console.WriteLine("Read {0} Policies from DB", PolsList.Count());
            
            // Insert Activity
            foreach (Tuple<int, int, int> pol in PolsList)
            {                
                int ActSNo = pol.Item1;
                int ActPolID = pol.Item2;
                int ActClientID = pol.Item3;
                try
                {
                    int ActID = acs.InsertActivity(ActPolID, ActClientID);

                    using (SqlCommand commandOne = conns.CreateCommand())
                    {
                        string sqltwo = string.Format("update {0} set ActivityCreated = GETDATE(), ActivityCreatedFlag = 1, ActivityID = @ActID WHERE UniqPolicy = @PolID and ClientID = @ClientID and SNo = @SNo;", DBtable);
                        commandOne.CommandText = sqltwo;
                        commandOne.Parameters.AddWithValue("@ActID", ActID);
                        commandOne.Parameters.AddWithValue("@PolID", ActPolID);
                        commandOne.Parameters.AddWithValue("@ClientID", ActClientID);
                        commandOne.Parameters.AddWithValue("@SNo", ActSNo);
                        commandOne.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    
                    err1 = ActPolID + " | " + e;
                    ErrorList = ErrorList + err1 + System.Environment.NewLine;
                    Console.WriteLine(err1);
                    ErrorCount = ErrorCount++;
                    using (SqlCommand commandTwo = conns.CreateCommand())
                    {
                        string sqlthree = string.Format("update {0} set Error = @e WHERE UniqPolicy = @PolID and ClientID = @ClientID and SNo = @SNo;", DBtable);
                        commandTwo.CommandText = sqlthree;
                        commandTwo.Parameters.AddWithValue("@e", e);
                        commandTwo.ExecuteNonQuery();
                    }
                }
                
                Console.WriteLine("DB updated for SNo: " + ActSNo);
                Console.WriteLine("*-*-*");
            }


            conns.Close();

            if (ErrorCount > 0)
            {
                string[] err2 = new string[] { err1 };
                string ErrorPathFull = ErrorFilePath + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                File.WriteAllLines(ErrorPathFull, err2);
            }
            

            var ActCloseEnd = DateTime.Now;
            Console.WriteLine("Code Ended: " + ActCloseEnd);

            Console.ReadKey();
        }
    }
}

