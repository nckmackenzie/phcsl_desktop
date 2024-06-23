Imports System.Data.SqlClient

Module AppModule
    Friend SMS_API_KEY = "c54a2317968a2bfe7626e73eaac5eeef6f8cd424b46568334a00eecf63ce5092"
    Friend SMS_USER_NAME = "pceahousing"
    Friend SMS_SENDER = "PCEAHOUSING"
    Friend connection As SqlConnection
    Friend cmd As SqlCommand
    Friend sql As String
    Friend da As SqlDataAdapter
    Friend rd As SqlDataReader
    Friend found As Boolean
    Friend connstr As String = Nothing
    '  Friend connstr As String = "Data Source=localhost\SQLEXPRESS;Initial Catalog=housingTest;User ID=sa;Password=NA-b$H12;"
    Friend RecordCount As Integer
    Friend bindingscr As BindingSource
    Friend ds As DataSet
    ' Friend criteria As String
    Friend ProcessID As String
    Friend errmsg As String
    Friend dt As System.Data.DataTable
    Friend IsFind As Boolean = False
    Friend params As New List(Of SqlParameter)
    Friend ref As String
    'Friend UserID As String
    Friend UserName As String
    Friend LogedUserID As Int16
    ' Friend MemberNo As Int16
    Friend BackupLocation As String
    Friend LocationSet As Boolean
    Friend SearchString As String
    Friend hash = "bym@ck"
    Public Sub AddParams(ParameterName As String, value As Object)
        '//add parameter and set their values sub
        Dim newparam As New SqlParameter(ParameterName, value)
        params.Add(newparam)
    End Sub
End Module
