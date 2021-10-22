using System;

// This example code shows how you could implement the required main function for a 
// Console UWP Application. You can replace all the code inside Main with your own custom code.

// You should also change the Alias value in the AppExecutionAlias Extension in the 
// Package.appxmanifest to a value that you define. To edit this file manually, right-click
// it in Solution Explorer and select View Code, or open it with the XML Editor.

namespace pkarcmd
{
    class Program
    {

        static bool gbDebug = false;
        public static System.Collections.Generic.List<OneApp> gAllApps = null;

        private static  void InitAppList()
        {
            gAllApps = commonsy.GetPkarAppsList();
        }

        static void Main(string[] args)
        {
            gbDebug = GetSettingsBool("debugmode");

            if (args.Length == 0)
            {
                ShowHelp();
            }
            else
            {
                InitAppList();
                TryAnyCommands(args);
            }
            //Console.WriteLine("Press a key to continue: ");
            //Console.ReadLine();
        }

        #region "library"
        public static bool GetSettingsBool(string sName, bool iDefault = false)
        {
            bool sTmp;

            sTmp = iDefault;
            {
                var withBlock = Windows.Storage.ApplicationData.Current;
                if (withBlock.RoamingSettings.Values.ContainsKey(sName))
                    sTmp = System.Convert.ToBoolean(withBlock.RoamingSettings.Values[sName].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                if (withBlock.LocalSettings.Values.ContainsKey(sName))
                    sTmp = System.Convert.ToBoolean(withBlock.LocalSettings.Values[sName].ToString(), System.Globalization.CultureInfo.InvariantCulture);
            }

            return sTmp;
        }

        private static void DebugOut(string sMsg)
        {
            if(gbDebug)
                Console.WriteLine("DEBUG: " + sMsg);
            System.Diagnostics.Debug.WriteLine("sMsg");

        }
        #endregion 


        private static async void TryAnyCommands(string[] args)
        {
            if(! await TryCmdCommands(args))
                    await TryAppCommands(args);
        }

        private static async System.Threading.Tasks.Task<bool> TryCmdCommands(string[] args)
        {

            //list - lista znanych app
            if (args[0].ToLower() == "list")
            {
                Console.WriteLine("App known to me:");
                Console.WriteLine("");
                Console.WriteLine("      name      |    hardname    |              guid                ");
                Console.WriteLine("----------------|----------------|----------------------------------");
                foreach (OneApp oApp in gAllApps)
                {
                    Console.WriteLine("{0,-16}|{1,-16}|{2,-48}", oApp.sName, oApp.sApp, oApp.sGuid);
                }
                return true;
            }

            //scan - podaje ktore app sa dostępne
            if (args[0].ToLower() == "scan")
            {
                // *TODO* args[1] jako "device" na ktorym trzeba sprawdzac

                Console.WriteLine("Checking installed app...");
                Console.WriteLine("");
                foreach (OneApp oApp in gAllApps)
                {
                    if (await TryConnectToApp(oApp.sApp, oApp.sGuid))
                    {
                        Console.WriteLine("{0,16} - found", oApp.sName);
                    }
                }
                return true;

            }
            //devices - podaje jakie device sa mu znane
            if (args[0].ToLower() == "devices")
            {
                return true;
            }

            return false;
        }

        private static bool onlyOneApp(string sAppName)
        {
            int iCnt = 0;
            string sRozne = "";

            foreach (OneApp oApp in gAllApps)
            {
                if (oApp.sName.ToLower().Contains(sAppName))
                {
                    sRozne = sRozne + " | " + oApp.sName;
                    iCnt++;
                }
            }

            if (iCnt == 1) return true;

            if(iCnt<1)
            {
                Console.WriteLine("ERROR: unknown app?");
                return false;
            }

            // > 1
            Console.WriteLine("ERROR: ambigous app name");
            Console.WriteLine("Matches: " + sRozne);
            return false;
        }

        private static async System.Threading.Tasks.Task TryAppCommands(string[] args)
        {
            if (!onlyOneApp(args[0].ToLower())) return;

            foreach (OneApp oApp in gAllApps)
            {
                if(oApp.sName.ToLower().Contains(args[0].ToLower()))
                {
                    // utworzymy sobie cmdline (jednym ciurkiem), pomijając samą app
                    string sCmdLine = "";
                    for (int iLp = 1; iLp < args.Length; iLp++)
                        sCmdLine = sCmdLine + " " + args[iLp];
                    sCmdLine = sCmdLine.Trim();

                    Console.WriteLine("Sending '" + sCmdLine + "' to " + oApp.sName);
                    string sResp = await commonsy.WykonajLokalnieCommon(oApp, sCmdLine);
                    Console.WriteLine(sResp);
                    return;
                }
            }

            Console.WriteLine("dziwacznosc!");
            // dziwacznosc! nie powinno sie zdarzyc, bo wczesniej testujemy

        }

        private static void ShowHelp()
        {
            Console.WriteLine("pkarcmd: command line front end for PKAR UWP apps");
            Console.WriteLine("version 0.5, 2021.04.28");
        }

        private static Windows.ApplicationModel.AppService.AppServiceConnection CreateConnection(string sApp, string sPack)
        {
            var oAppSrvConn = new Windows.ApplicationModel.AppService.AppServiceConnection();
            oAppSrvConn.AppServiceName = "com.microsoft.pkar." + sApp;
            oAppSrvConn.PackageFamilyName = sPack;
            return oAppSrvConn;
        }

        private static async System.Threading.Tasks.Task<bool> TryConnectToApp(string sApp, string sPack)
        {
            bool bRet = false;

            var oAppSrvConn = CreateConnection(sApp, sPack);
            Windows.ApplicationModel.AppService.AppServiceConnectionStatus oAppSrvStat = await oAppSrvConn.OpenAsync();

            bRet = (oAppSrvStat == Windows.ApplicationModel.AppService.AppServiceConnectionStatus.Success);
                oAppSrvConn.Dispose();
            return bRet;
        }



    }



}
