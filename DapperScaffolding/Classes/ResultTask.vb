Public Class ResultTask

    Public Const TASK_SUCCESS As Integer = 0
    Public Const TASK_APPSETTINGNOTFOUND As Integer = 1
    Public Const TASK_CONNECTIONSTRINGNOTFOUND As Integer = 2
    Public Const TASK_EXCEPTION As Integer = 3
    Public Const TASK_PROJECTNOTFOUND As Integer = 4
    Public Const TASK_ARGUMENTSNOTVALID As Integer = 5
    Public Const TASK_NOTIMPLMENTED As Integer = 6
    Public Const TASK_UNEXPECTED As Integer = 99

    Public ReadOnly Property isSuccess As Boolean
    Public ReadOnly Property ErrorMessage As String
    Public ReadOnly Property Exception As Exception
    Public ReadOnly Property ReturnCode As Integer

    Public Sub New(returnCode As Integer, Optional errorMessage As String = "", Optional ex As Exception = Nothing)
        Me.ReturnCode = returnCode
        Me.ErrorMessage = errorMessage
        Me.Exception = ex
        Me.isSuccess = (returnCode = TASK_SUCCESS)
    End Sub

End Class
