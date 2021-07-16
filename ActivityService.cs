using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace EpicIntegrator
{
    public class ActivityService
    {
        static string AuthenticationKey;
        static string DataBase;
        static string ConnectionString = ConfigurationManager.ConnectionStrings["CBLReporting"].ConnectionString;
        CBLServiceReference.MessageHeader oMessageHeader;


        public ActivityService()
        {
            AuthenticationKey = ConfigurationManager.AppSettings["AppliedSDKKey"];
            DataBase = ConfigurationManager.AppSettings["AppliedSDKDatabase"];
            oMessageHeader = new CBLServiceReference.MessageHeader();
            oMessageHeader.AuthenticationKey = AuthenticationKey;
            oMessageHeader.DatabaseName = DataBase;
            
        }


        public int InsertActivity(int PolicyID, int ClientID)
        {
            CBLServiceReference.EpicSDK_2017_02Client EpicSDKClient = new CBLServiceReference.EpicSDK_2017_02Client();
            CBLServiceReference.ActivityGetResult oActResult = new CBLServiceReference.ActivityGetResult();
            CBLServiceReference.ActivityFilter oActFilter = new CBLServiceReference.ActivityFilter();
            CBLServiceReference.Activity act = new CBLServiceReference.Activity();
         
            CBLServiceReference.Detail detail = new CBLServiceReference.Detail();
            DateTime NowDate = DateTime.Now;
            //Console.WriteLine(NowDate);

            act.AccountID = ClientID;
            act.AccountTypeCode = "CUST";
            act.AssociatedToID = PolicyID;
            act.AssociatedToType = "Policy";
            act.ActivityCode = "ALRT";
            act.Priority = "Urgent";
            act.Description = "WAWAN1 PHOM Urgent Action Required Wawanesa Property GRC Removal";
            detail.FollowUpStartDate = NowDate;
            //act.WhoOwnerCode = "aa";
            //act.StatusOption = CBLServiceReference.Activity.StatusOption.Open;
            //act.DetailValue.FollowUpStartDate = detail.FollowUpStartDate;
            act.DetailValue = detail;
            int ActID = EpicSDKClient.Insert_Activity(oMessageHeader, act);

            //oActFilter.ActivityID = ActivityID;
            //oActResult = EpicSDKClient.Get_Activity(oMessageHeader, oActFilter, 0);
            //act = oActResult.Activities[0];


            //Console.WriteLine("Account ID: "+act.AccountID);
            //Console.WriteLine("Account Type Code: " + act.AccountTypeCode);
            //Console.WriteLine("Associated to ID: " + act.AssociatedToID);
            //Console.WriteLine("Associated to Type: " + act.AssociatedToType);
            //Console.WriteLine("Activity Code: " + act.ActivityCode);
            //Console.WriteLine("Priority: " + act.Priority);
            //Console.WriteLine("Description: " + act.Description);
            //Console.WriteLine("WhoOwnerCode: " + act.WhoOwnerCode);
            //Console.WriteLine("Status Option: " + act.StatusOption);
            //Console.WriteLine("Follow Up Start Date: " + act.DetailValue.FollowUpStartDate);

            Console.WriteLine("Activity Inserted: " + ActID);
            
            

            return ActID;
        }

    }

    public class ActivityVars
    {
        public int UniqActivity { get; set; }


    }

}
