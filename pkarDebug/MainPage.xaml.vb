

partial Public NotInheritable Class MainPage
    Inherits Page

    Private gAllApps As System.Collections.Generic.List(Of OneApp) = Nothing
    Private gAllDevs As Collection(Of Windows.System.RemoteSystems.RemoteSystem)

    Public gWatcher As Windows.System.RemoteSystems.RemoteSystemWatcher
    Private goTimer As DispatcherTimer = New DispatcherTimer


    Private Sub InitAppList()
        gAllApps = GetPkarAppsList()
    End Sub

    Private Sub FillCmdCombo()
        uiStandardCmd.Items.Clear()
        ' dodawanie kolejnych komend
        uiStandardCmd.Items.Add("ping")
        uiStandardCmd.Items.Add("ver")
        uiStandardCmd.Items.Add("localdir")
        uiStandardCmd.Items.Add("appdir")
        uiStandardCmd.Items.Add("installeddate")
        uiStandardCmd.Items.Add("help")
        uiStandardCmd.Items.Add("debug vars")
        uiStandardCmd.Items.Add("debug triggers")
        uiStandardCmd.Items.Add("debug triggers unregister")
        uiStandardCmd.Items.Add("debug toasts")
        uiStandardCmd.Items.Add("debug memsize")
        uiStandardCmd.Items.Add("debug rungc")
        uiStandardCmd.Items.Add("debug crashmsg")
        uiStandardCmd.Items.Add("debug crashmsg clear")
        uiStandardCmd.Items.Add("debug " & mMyCmd1)

        uiStandardCmd.Items.Add("lib unregistertriggers")
        uiStandardCmd.Items.Add("lib isfamilymobile")
        uiStandardCmd.Items.Add("lib isfamilydesktop")
        uiStandardCmd.Items.Add("lib netisipavailable")
        uiStandardCmd.Items.Add("lib netiscellinet")
        uiStandardCmd.Items.Add("lib gethostname")
        uiStandardCmd.Items.Add("lib isthismoje")
        uiStandardCmd.Items.Add("lib istriggersregistered")

        uiStandardCmd.Items.Add("lib " & mMyCmd1)
        uiStandardCmd.Items.Add("lib " & mMyCmd1 & " 1")
        uiStandardCmd.Items.Add("lib " & mMyCmd1 & " 0")

    End Sub

    Private Async Function FillDevices() As Task

        If Not IsFamilyDesktop() Then
            uiListDevices.Visibility = Visibility.Collapsed
            uiListDevices.Width = 1
            Return
        End If

        Dim oAccStat As Windows.System.RemoteSystems.RemoteSystemAccessStatus =
            Await Windows.System.RemoteSystems.RemoteSystem.RequestAccessAsync()

        If oAccStat <> Windows.System.RemoteSystems.RemoteSystemAccessStatus.Allowed Then
            Await DialogBoxAsync("No permission")
            Return
        End If

        gAllDevs = New Collection(Of Windows.System.RemoteSystems.RemoteSystem)
        gWatcher = Windows.System.RemoteSystems.RemoteSystem.CreateWatcher()
        AddHandler gWatcher.RemoteSystemAdded, AddressOf remsys_Added
        AddHandler gWatcher.RemoteSystemRemoved, AddressOf remsys_Remove
        'AddHandler gWatcher.RemoteSystemUpdated, AddressOf remsys_Update
        gWatcher.Start()

        goTimer.Interval = New TimeSpan(0, 0, 15)
        AddHandler goTimer.Tick, AddressOf TimerTick
        goTimer.Start()

    End Function

    Private Sub TimerTick(sender As Object, e As Object)
        goTimer.Stop()
        gWatcher.Stop()
    End Sub

#Region "lista RemoteSystem"

    Private Async Sub remsys_Update(sender As Windows.System.RemoteSystems.RemoteSystemWatcher,
                              oArgs As Windows.System.RemoteSystems.RemoteSystemUpdatedEventArgs)

        Debug.WriteLine("  update: " & oArgs.RemoteSystem.DisplayName)
        ' gAllDevs.Remove(gAllDevs.First(Function(aa) aa.Id = oArgs.RemoteSystem.Id))
        Await remsys_SprawdzAdd(oArgs.RemoteSystem, True)

    End Sub

    Private Sub remsys_Remove(sender As Windows.System.RemoteSystems.RemoteSystemWatcher,
                              oArgs As Windows.System.RemoteSystems.RemoteSystemRemovedEventArgs)

        gAllDevs.Remove(gAllDevs.First(Function(aa) aa.Id = oArgs.RemoteSystemId))
        UpdateList()
    End Sub
    Private Async Sub remsys_Added(sender As Windows.System.RemoteSystems.RemoteSystemWatcher,
                                   oargs As Windows.System.RemoteSystems.RemoteSystemAddedEventArgs)

        Debug.WriteLine("  found: " & oargs.RemoteSystem.DisplayName)
        Await remsys_SprawdzAdd(oargs.RemoteSystem, True)

        ' oargs.RemoteSystem ' DisplayName Id IsAvailableByProximity  IsAvailableBySpatialProximity 
    End Sub

    Private Async Function remsys_SprawdzAdd(oRemSys As Windows.System.RemoteSystems.RemoteSystem, bAdd As Boolean) As Task(Of Boolean)
        If oRemSys.Status <> Windows.System.RemoteSystems.RemoteSystemStatus.Available Then
            DebugOut("ale unavailable")
            Return False
        End If

        If Not Await oRemSys.GetCapabilitySupportedAsync(Windows.System.RemoteSystems.KnownRemoteSystemCapabilities.AppService) Then
            DebugOut("ale nie ma dla niego AppService")
            Return False
        End If

        DebugOut("adding...")
        gAllDevs.Add(oRemSys)
        UpdateList()

        Return True
    End Function


    Private Async Sub UpdateList()
        Await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, AddressOf UpdateListUI)
    End Sub

    Private Sub UpdateListUI()
        uiListDevices.ItemsSource = From c In gAllDevs
        ' nie moze byc samo mDevList, bo wtedy tylko jedno pokazuje!
    End Sub
#End Region

    Private Sub FillApps()
        InitAppList()
        uiListApps.ItemsSource = from c in gAllApps order by c.sName
    End Sub

    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        GetAppVers(Nothing)
        ProgRingInit(True, False)
        FillCmdCombo()
        FillApps()
        FillDevices()
    End Sub

    Private Sub uiStandardCmd_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles uiStandardCmd.SelectionChanged
        If e?.AddedItems Is Nothing Then Return
        If e.AddedItems.Count < 1 Then Return

        uiCommand.Text = e.AddedItems.Item(0)
    End Sub

    Private Sub uiRunCmd_Click(sender As Object, e As RoutedEventArgs)
        Dim oApp As OneApp = uiListApps.SelectedItem
        If oApp Is Nothing Then
            DialogBox("ERROR: you must select app!")
            Return
        End If

        Dim sCmd As String = uiCommand.Text
        If sCmd.Length < 3 Then
            DialogBox("ERROR: command is too short")
            Return
        End If
        uiCommand.Text = ""

        Dim oDev As Windows.System.RemoteSystems.RemoteSystem = uiListDevices.SelectedItem

        ProgRingShow(True)
        If oDev Is Nothing Then
            WykonajLokalnie(oApp, sCmd)
        Else
            WykonajZdalnie(oDev, oApp, sCmd)
        End If
        ProgRingShow(False)

    End Sub

    Private Async Sub WykonajLokalnie(oApp As OneApp, sCmd As String)
        '            Public Shared Async Function AccessRemoteSystem(sApp As String, sPack As String, sCmd As String, sResOk As String, sRetVar As String, bAllowEmpty As Boolean, iMode As Integer) As Task(Of String)

        Dim sData As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        uiOutputHistory.Text &= sData & ": sending " & sCmd & " to " & oApp.sName & vbCrLf

        uiOutputHistory.Text &= Await WykonajLokalnieCommon(oApp, sCmd) & vbCrLf

    End Sub
    Private Sub WykonajZdalnie(oDev As Windows.System.RemoteSystems.RemoteSystem, oApp As OneApp, sCmd As String)
        uiOutputHistory.Text &= "Sending " & sCmd & " to " & oApp.sName & "@" & oDev.DisplayName & vbCrLf

        Throw New NotImplementedException()
    End Sub
End Class


