Imports System

Module Program

    Sub Main(args As String())
        If args.Length = 0 Then
            ShowHelp()
        Else
            InitAppList()
            TryAnyCommands(args)
        End If

    End Sub

    Private gAllApps As System.Collections.Generic.List(Of OneApp) = Nothing

    Private Sub InitAppList()
        gAllApps = GetPkarAppsList()
    End Sub

    Private Async Function TryAnyCommands(args As String()) As Task
        If Not Await TryCmdCommands(args) Then Await TryAppCommands(args)
    End Function

#Disable Warning BC42356 ' This async method lacks 'Await' operators and so will run synchronously
    Private Async Function TryCmdCommands(args As String()) As Task(Of Boolean)
#Enable Warning BC42356 ' This async method lacks 'Await' operators and so will run synchronously

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

        ' //scan - podaje ktore app sa dostêpne
        If args(0).ToLower() = "scan" Then
            ' // *TODO* args[1] jako "device" na ktorym trzeba sprawdzac

            Console.WriteLine("Checking installed app...")
            Console.WriteLine("")
            For Each oApp As OneApp In gAllApps
                If TryConnectToApp(oApp.sApp, oApp.sGuid) Then
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
    Private Function onlyOneApp(sAppName As String) As Boolean
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
    Private Async Function TryAppCommands(args As String()) As Task
        If Not onlyOneApp(args(0).ToLower()) Then Return

        For Each oApp As OneApp In gAllApps
            If oApp.sName.ToLower().Contains(args(0).ToLower()) Then

                If oApp.sGuid = "" Then
                    Console.Error.WriteLine("ERROR: no Guid/Package for app?")
                    Return
                End If

                If oApp.sExecAlias = "" Then
                    Console.Error.WriteLine("ERROR: no ExecAlias for app?")
                    Return
                End If

                ' // utworzymy sobie cmdline (jednym ciurkiem), pomijaj¹c sam¹ app
                Dim sCmdLine As String = ""
                    For iLp As Integer = 1 To args.Length - 1
                        sCmdLine = sCmdLine & " " & args(iLp)
                    Next

                    sCmdLine = sCmdLine.Trim()
                Await WykonajLokalnieCommon(oApp, sCmdLine)
                Return
                End If
        Next

        Console.Error.WriteLine("ERROR: dziwacznosc! Za drugim przebiegiem nie mam app?")
        ' // dziwacznosc! nie powinno sie zdarzyc, bo wczesniej testujemy

    End Function
    Private Sub ShowHelp()
        Console.WriteLine("pkcmd: command line front end for PKAR UWP apps")
        Console.WriteLine("version 0.5, 2021.05.02")
    End Sub

    Private Function TryConnectToApp(sApp As String, sPack As String) As Boolean
        Dim bRet As Boolean = False

        Dim sFolder As String = "C:\Users\pkar\AppData\Local\Packages\" & sPack
        If IO.Directory.Exists(sFolder) Then Return True
        Return False
    End Function
    Public Async Function WykonajLokalnieCommon(oApp As OneApp, sCmd As String) As Task(Of Boolean)

        Dim sFolder As String = "C:\Users\pkar\AppData\Local\Packages\" & oApp.sGuid
        If Not IO.Directory.Exists(sFolder) Then
            Console.Error.WriteLine("App " & oApp.sApp & " is not installed?")
            Return False
        End If

        sFolder = sFolder & "\TempState\"
        If Not IO.Directory.Exists(sFolder) Then
            Console.Error.WriteLine("App " & oApp.sApp & " hasn't TempState folder?")
            Return False
        End If

        Dim sFileLock As String = sFolder & "\" & "cmdline.lock"
        Dim sFileStdErr As String = sFolder & "\" & "stderr.txt"
        Dim sFileStdOut As String = sFolder & "\" & "stdout.txt"

        If IO.File.Exists(sFileLock) Then
            Console.Error.WriteLine("There is cmdline.lock file in app!")
            Console.Error.WriteLine("File: " & sFileLock)
            Return False
        End If

        ' ktory kawalek bedzie nazw¹ app? oApp.sName, sApp, czy extract z sGuid?
        If Process.Start(oApp.sExecAlias, sCmd) Is Nothing Then
            Console.Error.WriteLine("ERROR in Process.Start")
        End If

        Await Task.Delay(2000)

        If IO.File.Exists(sFileLock) Then
            Console.Write("Waiting for lockfile")
            For iLp As Integer = 0 To 10
                Console.Write(".")
                If Not IO.File.Exists(sFileLock) Then Exit For
                ' poczekaj sekunde
            Next
        End If

        If IO.File.Exists(sFileLock) Then
            Console.Error.WriteLine("Too long waiting for app, cmdline.lock still present!")
            Return False
        End If

        If IO.File.Exists(sFileStdErr) Then
            Dim sTxt As String = IO.File.ReadAllText(sFileStdErr)
            Console.Error.WriteLine(sTxt)
            Return False
        End If

        If Not IO.File.Exists(sFileStdOut) Then
            Console.Error.WriteLine("ERROR: no stdout file!")
            Return False
        Else
            Dim sTxt As String = IO.File.ReadAllText(sFileStdOut)
            Console.WriteLine(sTxt)
        End If

        Return True
    End Function

End Module
