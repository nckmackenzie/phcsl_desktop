Imports System.Security.Cryptography
Public Class Encryption
    Public Shared Function HashString(str As String) As String
        Return EncryptString(str)
    End Function
    Public Shared Function GenerateSalt() As String
        Using CryptoServiceProvider As New RNGCryptoServiceProvider
            Dim sb As New System.Text.StringBuilder()
            Dim data As Byte() = New Byte() {}
            For i = 0 To 6
                CryptoServiceProvider.GetBytes(data)
                Dim value As String = BitConverter.ToString(data, 0)
                sb.Append(value)
            Next
            Return EncryptString(sb.ToString())
        End Using
    End Function
    Private Shared Function EncryptString(txt As String) As String
        Dim bytes As Byte() = System.Text.Encoding.ASCII.GetBytes(txt)
        Dim Hashed = SHA256.Create().ComputeHash(bytes)
        Return Convert.ToBase64String(Hashed)
    End Function
End Class
