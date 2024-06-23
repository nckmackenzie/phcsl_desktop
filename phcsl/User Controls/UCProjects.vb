Public Class UCProjects
    Private Shared _instance As UCProjects
    Public Shared ReadOnly Property Instance As UCProjects
        Get
            If _instance Is Nothing Then _instance = New UCProjects()
            Return _instance
        End Get
    End Property
End Class
