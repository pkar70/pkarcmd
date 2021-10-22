Partial Public Module commonsy

    Public Async Function WykonajLokalnieCommon(oApp As OneApp, sCmd As String) As Task(Of String)

        Dim oAppSrvConn As AppService.AppServiceConnection

        Try
            oAppSrvConn = New AppService.AppServiceConnection
            oAppSrvConn.AppServiceName = "com.microsoft.pkar." & oApp.sApp
            oAppSrvConn.PackageFamilyName = oApp.sGuid
        Catch ex As Exception
            Return "ERROR creating AppServiceConnection = " & ex.Message
        End Try

        Dim oAppSrvStat As AppService.AppServiceConnectionStatus
        Dim bError As Boolean = True
        Try
            oAppSrvStat = Await oAppSrvConn.OpenAsync()
            bError = False
        Catch ex As Exception
            Return "ERROR OpenAsync = " & ex.Message
        End Try
        If bError Then Return "ERROR OpenAsync still nothing"

        If oAppSrvStat <> AppService.AppServiceConnectionStatus.Success Then
            oAppSrvConn.Dispose()
            Return "ERROR conneting to " & oApp.sName & " app:" & vbCrLf & oAppSrvStat.ToString
        End If

        Dim oInputs = New ValueSet
        oInputs.Add("command", sCmd)

        Dim oRemSysResp As AppService.AppServiceResponse
        Try
            oRemSysResp = Await oAppSrvConn.SendMessageAsync(oInputs)
        Catch ex As Exception
            Return "ERROR SendMessageAsync = " & ex.Message
        End Try

        oAppSrvConn.Dispose()

        ' tu jest Failure?
        If oRemSysResp.Status <> AppService.AppServiceResponseStatus.Success Then
            Return "ERROR goRemSysResp.Status <> AppServiceResponseStatus.Success " & oApp.sName & " app:" & vbCrLf & oRemSysResp.Status.ToString
        End If

        If Not oRemSysResp.Message.ContainsKey("status") Then
            Return "ERROR getting result " & oApp.sName & " app: no 'status' key"
        End If

        Dim sResp As String = oRemSysResp.Message("status").ToString()
        If sResp <> "OK" Then
            Return "ERROR from remote: " & sResp
        End If

        Dim sRetVal As String = oRemSysResp.Message("result").ToString()
        Return sRetVal

    End Function

End Module