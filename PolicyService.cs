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
    public class PolicyService
    {
        static string AuthenticationKey;
        static string DataBase;
        static string ConnectionString = ConfigurationManager.ConnectionStrings["CBLReporting"].ConnectionString;
        CBLServiceReference.MessageHeader oMessageHeader;

        public PolicyService()
        {
            AuthenticationKey = ConfigurationManager.AppSettings["AppliedSDKKey"];
            DataBase = ConfigurationManager.AppSettings["AppliedSDKDatabase"];
            oMessageHeader = new CBLServiceReference.MessageHeader();
            oMessageHeader.AuthenticationKey = AuthenticationKey;
            oMessageHeader.DatabaseName = DataBase;
        }

        public void  GetWhoOwnerCode(string PolicyNumber, int ClientID)
        {
            CBLServiceReference.EpicSDK_2017_02Client EpicSDKClient = new CBLServiceReference.EpicSDK_2017_02Client();
            CBLServiceReference.PolicyGetResult oPolicyResult = new CBLServiceReference.PolicyGetResult();
            CBLServiceReference.PolicyFilter oPolicyFilter = new CBLServiceReference.PolicyFilter();
            CBLServiceReference.Policy oPol = new CBLServiceReference.Policy();

            oPolicyFilter.PolicyNumber = PolicyNumber;
            oPolicyFilter.ClientID = ClientID;
            oPolicyResult = EpicSDKClient.Get_Policy(oMessageHeader, oPolicyFilter, 0);
            oPol = oPolicyResult.Policies[0];

            string OwnerCode = oPol.ProfitCenterCode;

        }

        public void GetPolicy(string PolicyNumber, int ClientID, string csvDate)
        {
            CBLServiceReference.EpicSDK_2017_02Client EpicSDKClient = new CBLServiceReference.EpicSDK_2017_02Client();
            CBLServiceReference.PolicyGetResult oPolicyResult = new CBLServiceReference.PolicyGetResult();
            CBLServiceReference.PolicyFilter oPolicyFilter = new CBLServiceReference.PolicyFilter();
            CBLServiceReference.Policy oPol = new CBLServiceReference.Policy();

            CBLServiceReference.LineGetResult1 oLineResult = new CBLServiceReference.LineGetResult1();
            CBLServiceReference.LineFilter oLineFilter = new CBLServiceReference.LineFilter();
            CBLServiceReference.Line1 lne = new CBLServiceReference.Line1();

            oPolicyFilter.PolicyNumber = PolicyNumber;
            oPolicyFilter.ClientID = ClientID;
            oPolicyResult = EpicSDKClient.Get_Policy(oMessageHeader, oPolicyFilter, 0);

            //Console.WriteLine(oPolicyResult.Policies.Count());
            foreach (CBLServiceReference.Policy Pol in oPolicyResult.Policies)
            {
                oLineFilter.PolicyID = Pol.PolicyID;
                oLineResult = EpicSDKClient.Get_Line(oMessageHeader, oLineFilter, 0);
                lne = oLineResult.Lines[0];

                string PolExpDate = Pol.ExpirationDate.ToString("yyyy-MM-dd");
                //if (Pol.PolicyNumber == PolicyNumber && )

                    Console.WriteLine(PolExpDate);
                Console.WriteLine(Pol.PolicyID);
                Console.WriteLine(Pol.PolicyNumber);
                Console.WriteLine(lne.IssuingCompanyLookupCode);
                Console.WriteLine("*-*-*-*");
            }

        }







    }
}
