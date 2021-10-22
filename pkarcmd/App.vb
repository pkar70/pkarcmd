Public Class App
    Private Shared gAllApps As System.Collections.Generic.List(Of OneApp) = Nothing

    Private Shared Sub InitAppList()
        gAllApps = GetPkarAppsList()
    End Sub

    Private Shared gbDebug As Boolean = False

    Public Shared Sub Main(args As String())
        gbDebug = GetSettingsBool("debugmode")

        If args.Length = 0 Then
            ShowHelp()
        Else
            InitAppList()
            TryAnyCommands(args)
        End If
        '//Console.WriteLine("Press a key to continue: ");
        '//Console.ReadLine();
    End Sub

#Region "library"
    Public Shared Function GetSettingsBool(sName As String, Optional iDefault As Boolean = False) As Boolean
        Dim sTmp As Boolean

        sTmp = iDefault
        With Windows.Storage.ApplicationData.Current
            If .RoamingSettings.Values.ContainsKey(sName) Then
                sTmp = CBool(.RoamingSettings.Values(sName).ToString)
            End If
            If .LocalSettings.Values.ContainsKey(sName) Then
                sTmp = CBool(.LocalSettings.Values(sName).ToString)
            End If
        End With

        Return sTmp

    End Function
    Private Shared Sub DebugOut(sMsg As String)
        If gbDebug Then Console.WriteLine("DEBUG: " & sMsg)
        Diagnostics.Debug.WriteLine("sMsg")
    End Sub
#End Region


    Private Shared Async Sub TryAnyCommands(args As String())
        If Not Await TryCmdCommands(args) Then Await TryAppCommands(args)
    End Sub

    Private Shared Async Function TryCmdCommands(args As String()) As Task(Of Boolean)

        '//list - lista znanych app
        If args(0).ToLower() = "list" Then

            Console.WriteLine("App known to me:")
            Console.WriteLine("")
            Console.WriteLine("      name      |    hardname    |              guid                ")
            Console.WriteLine("----------------|----------------|----------------------------------")
            For Each oApp As OneApp In gAllApps
                Console.WriteLine("{0,-16}|{1,-16}|{2,-48}", oApp.sName, oApp.sApp, oApp.sGuid)
            Next
            Return True
        End If

        ' //scan - podaje ktore app sa dostępne
        If args(0).ToLower() = "scan" Then
            ' // *TODO* args[1] jako "device" na ktorym trzeba sprawdzac

            Console.WriteLine("Checking installed app...")
            Console.WriteLine("")
            For Each oApp As OneApp In gAllApps
                If Await TryConnectToApp(oApp.sApp, oApp.sGuid) Then
                    Console.WriteLine("{0,16} - found", oApp.sName)
                End If
            Next
            Return True

            End If

            ' //devices - podaje jakie device sa mu znane
            If args(0).ToLower() = "devices" Then
                Return True
            End If

            Return False
    End Function

    Private Shared Function onlyOneApp(sAppName As String) As Boolean
        Dim iCnt As Integer = 0
        Dim sRozne As String = ""

        For Each oApp As OneApp In gAllApps
            If oApp.sName.ToLower().Contains(sAppName) Then
                sRozne = sRozne & " | " & oApp.sName
                iCnt += 1
            End If
        Next
        If iCnt = 1 Then Return True

        If iCnt < 1 Then
            Console.Error.WriteLine("ERROR: unknown app?")
            Return False
        End If

        ' // > 1
        Console.Error.WriteLine("ERROR: ambigous app name")
        Console.Error.WriteLine("Matches: " & sRozne)
        Return False
    End Function


    Private Shared Async Function TryAppCommands(args As String()) As Task
        If Not onlyOneApp(args(0).ToLower()) Then Return

        For Each oApp As OneApp In gAllApps
            If oApp.sName.ToLower().Contains(args(0).ToLower()) Then

                ' // utworzymy sobie cmdline (jednym ciurkiem), pomijając samą app
                Dim sCmdLine As String = ""
                For iLp As Integer = 1 To args.Length - 1
                    sCmdLine = sCmdLine & " " & args(iLp)
                Next

                sCmdLine = sCmdLine.Trim()

                Console.WriteLine("Sending '" & sCmdLine & "' to " + oApp.sName)
                Dim sResp As String = Await WykonajLokalnieCommon(oApp, sCmdLine)
                Console.WriteLine(sResp)
                Return
            End If
        Next

        Console.WriteLine("dziwacznosc!")
        ' // dziwacznosc! nie powinno sie zdarzyc, bo wczesniej testujemy

    End Function


    Private Shared Sub ShowHelp()
        Console.WriteLine("pkarcmd: command line front end for PKAR UWP apps")
        Console.WriteLine("version 0.5, 2021.04.28")
    End Sub

    Private Shared Function CreateConnection(sApp As String, sPack As String) As Windows.ApplicationModel.AppService.AppServiceConnection
        Dim oAppSrvConn As Windows.ApplicationModel.AppService.AppServiceConnection = New Windows.ApplicationModel.AppService.AppServiceConnection
        oAppSrvConn.AppServiceName = "com.microsoft.pkar." & sApp
        oAppSrvConn.PackageFamilyName = sPack
        Return oAppSrvConn
    End Function

    Private Shared Async Function TryConnectToApp(sApp As String, sPack As String) As Task(Of Boolean)
        Dim bRet As Boolean = False

        Dim oAppSrvConn As Windows.ApplicationModel.AppService.AppServiceConnection = CreateConnection(sApp, sPack)
        Dim oAppSrvStat As Windows.ApplicationModel.AppService.AppServiceConnectionStatus = Await oAppSrvConn.OpenAsync()

        bRet = (oAppSrvStat = Windows.ApplicationModel.AppService.AppServiceConnectionStatus.Success)
        oAppSrvConn.Dispose()
        Return bRet
    End Function





End Class
