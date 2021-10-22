using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAAPS = Windows.ApplicationModel.AppService;

namespace pkarcmd
{
    public class OneApp
    {
        public string sName;
        public string sApp;
        public string sGuid;

        public OneApp(string name, string app, string guid)
        {
            sName = name;
            sApp = app;
            sGuid = guid;
        }

    }

    public static class commonsy
    {
        public static System.Collections.Generic.List<OneApp> GetPkarAppsList()
        {
            System.Collections.Generic.List<OneApp> gAllApps = new List<OneApp>();

            gAllApps.Add(new OneApp("Calls'Stat", "callsstat", "622PKar.Callsstat_pm8terbg0v8ky"));
            gAllApps.Add(new OneApp("Smogmeter", "smogometr", "622PKar.SmogMeter_pm8terbg0v8ky"));

            return gAllApps;
        }

        public static async Task<string> WykonajLokalnieCommon(OneApp oApp , string sCmd )
        {
            Console.WriteLine("WykonajLokalnieCommon");
            WAAPS.AppServiceConnection oAppSrvConn = new WAAPS.AppServiceConnection();
            Console.WriteLine("WykonajLokalnieCommon1");
            oAppSrvConn.AppServiceName = "com.microsoft.pkar." + oApp.sApp;
            Console.WriteLine("WykonajLokalnieCommon2");
            oAppSrvConn.PackageFamilyName = oApp.sGuid;
            Console.WriteLine("WykonajLokalnieCommon3");

            WAAPS.AppServiceConnectionStatus oAppSrvStat = await oAppSrvConn.OpenAsync();
            Console.WriteLine("WykonajLokalnieCommon4");
            Console.WriteLine(oAppSrvStat.ToString());

            if (oAppSrvStat != WAAPS.AppServiceConnectionStatus.Success)
            {
                oAppSrvConn.Dispose();
                return "ERROR conneting to " + oApp.sName + " app:\n" + oAppSrvStat.ToString();
            }

            Windows.Foundation.Collections.ValueSet oInputs = new Windows.Foundation.Collections.ValueSet();
            oInputs.Add("command", sCmd);

            WAAPS.AppServiceResponse oRemSysResp = await oAppSrvConn.SendMessageAsync(oInputs);
            Console.WriteLine("sendłem!");

            oAppSrvConn.Dispose();

            if (oRemSysResp.Status != WAAPS.AppServiceResponseStatus.Success)
            {
                return "ERROR goRemSysResp.Status <> AppServiceResponseStatus.Success " + oApp.sName + " app:=n" + oRemSysResp.Status.ToString();
            }

            if (!oRemSysResp.Message.ContainsKey("status"))
            {
                return "ERROR getting result " + oApp.sName + " app: no 'status' key";
            }

            Console.WriteLine("biore status");
            string sResp = oRemSysResp.Message["status"].ToString();
            if (sResp != "OK")
            {
                return "ERROR from remote: " + sResp;
            }

            Console.WriteLine("biore result");
            string sRetVal = oRemSysResp.Message["result"].ToString();
            return sRetVal;

        }


    }

}


