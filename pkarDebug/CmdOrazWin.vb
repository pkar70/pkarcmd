

Public Class OneApp
    Public Property sName As String ' do pokazywania przez debug/cmd app
    Public Property sApp As String  ' dla RemSys
    Public Property sGuid As String ' dla RemSys
    Public Property sExecAlias As String ' dla Exec (cmdline)

    Public Sub New(name As String, app As String, guid As String, exec As String)
        sName = name
        sApp = app
        sGuid = guid
        sExecAlias = exec
    End Sub

End Class

Partial Public Module commonsy

    Public Function GetPkarAppsList() As System.Collections.Generic.List(Of OneApp)
        Dim gAllApps As System.Collections.Generic.List(Of OneApp) = New List(Of OneApp)

        ' DisplayName , App i Package dla RemSys, ExecAlias
	' dla RemSys: z manifest, AppService Name oraz packaging/packagefamilyname
        gAllApps.Add(New OneApp("Calls'Stat", "callsstat", "622PKar.Callsstat_pm8terbg0v8ky", ""))
        gAllApps.Add(New OneApp("Smogmeter", "smogometr", "622PKar.SmogMeter_pm8terbg0v8ky", ""))
        gAllApps.Add(New OneApp("wycofania", "wycofania", "622PKar.Wycofania_pm8terbg0v8ky", "Wycofania"))
        gAllApps.Add(New OneApp("RGB LED", "", "622PKar.RGBbulb_pm8terbg0v8ky", "RGBbulb"))
        gAllApps.Add(New OneApp("Ballots", "ballots", "d6b003b6-d189-4df3-87ab-4a2cc388b0c9_y9hvt3b1p7jrj", "ballots"))
        gAllApps.Add(New OneApp("InstaMon", "instamonitor", "ce4111e6-6488-4c7d-bb3b-fd3e95e26c42_y9hvt3b1p7jrj", "instamonitor"))
        gAllApps.Add(New OneApp("BackupSMS", "backupsms", "622PKar.BackupSMS_pm8terbg0v8ky", "backupsms"))
        gAllApps.Add(New OneApp("DailyItinerary", "DailyItinerary", "622PKar.Dailyitinerary_pm8terbg0v8ky", "DailyItinerary"))
        gAllApps.Add(New OneApp("MijiaThermo", "termo", "622PKar.MijiaThermo_pm8terbg0v8ky", "MijiaThermo"))
        gAllApps.Add(New OneApp("FilteredRSS", "filtered", "622PKar.FilteredRSS_pm8terbg0v8ky", "FilteredRSS"))


        Return gAllApps

    End Function

End Module

'      </uap:VisualElements>
'      <Extensions>
'        <uap:Extension Category="windows.appService">
'          <uap3:AppService Name="com.microsoft.pkar.ballots" SupportsRemoteSystems="true"/>
'        </uap:Extension>
'      </Extensions>
