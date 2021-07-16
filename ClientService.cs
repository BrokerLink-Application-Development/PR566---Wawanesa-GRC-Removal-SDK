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
    public class ClientService
    {
        static string AuthenticationKey;
        static string DataBase;
        static string ConnectionString = ConfigurationManager.ConnectionStrings["CBLReporting"].ConnectionString;
        CBLServiceReference.MessageHeader oMessageHeader;

        public ClientService()
        {
            AuthenticationKey = ConfigurationManager.AppSettings["AppliedSDKKey"];
            DataBase = ConfigurationManager.AppSettings["AppliedSDKDatabase"];
            oMessageHeader = new CBLServiceReference.MessageHeader();
            oMessageHeader.AuthenticationKey = AuthenticationKey;
            oMessageHeader.DatabaseName = DataBase;

        }

        public int GetClient(string PolNumber)
        {
            CBLServiceReference.EpicSDK_2017_02Client EpicSDKClient = new CBLServiceReference.EpicSDK_2017_02Client();
            CBLServiceReference.ClientFilter oClientFilter = new CBLServiceReference.ClientFilter();
            CBLServiceReference.ClientGetResult oClientResult = new CBLServiceReference.ClientGetResult();
            CBLServiceReference.Client oClient = new CBLServiceReference.Client();

            oClientFilter.PolicyNumber = PolNumber;
            oClientResult = EpicSDKClient.Get_Client(oMessageHeader, oClientFilter, 0);
            oClient = oClientResult.Clients[0];

            int UniqClientID = oClient.ClientID;
            //Console.WriteLine(oClient.ClientLookupCode);

            return UniqClientID;
                       
        }


    }
}
