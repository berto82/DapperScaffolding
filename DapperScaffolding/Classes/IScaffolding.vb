Imports System.Data

Public Interface IScaffolding

    Function Convert(cn As IDbConnection, path As String, lang As String, [namespace] As String, deleteFolder As Boolean) As ResultTask

End Interface
