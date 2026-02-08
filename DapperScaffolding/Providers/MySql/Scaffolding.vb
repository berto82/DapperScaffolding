Imports System.Data
Imports System.IO
Imports System.Reflection
Imports Dapper
Imports Spectre.Console

Namespace Providers.MySql

    Public Class Scaffolding
        Implements IScaffolding

        Public Function Convert(cn As IDbConnection, path As String, lang As String, [namespace] As String, deleteFolder As Boolean) As ResultTask Implements IScaffolding.Convert
            Try

                If IO.Directory.Exists(path) = False Then
                    IO.Directory.CreateDirectory(path)
                ElseIf deleteFolder = True Then
                    AnsiConsole.MarkupLine($"{Emoji.Known.Broom} Cleaning up the existing folder")
                    IO.Directory.Delete(path, True)
                    IO.Directory.CreateDirectory(path)
                End If

                If cn.State = ConnectionState.Closed Then
                    cn.Open()
                End If

                Dim lstInformationSchema As List(Of InformationSchema) = cn.Query(Of InformationSchema)("SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @p0", New With {.p0 = cn.Database}).ToList

                Dim rowCount As Integer = 1

                AnsiConsole.Progress().
                    Columns(
                    New SpinnerColumn() With {.Spinner = Spinner.Known.Default},
                    New TaskDescriptionColumn(),
                    New ProgressBarColumn(),
                    New PercentageColumn(),
                    New ElapsedTimeColumn()).
                    Start(Sub(ctx)
                              Dim pTask As ProgressTask = ctx.AddTask("Processing tables...")

                              For Each informationSchema In lstInformationSchema
                                  Dim status As Integer = CInt((rowCount / lstInformationSchema.Count) * 100)
                                  pTask.Value = status

                                  Dim filename As String = $"{path}\{informationSchema.TABLE_NAME}.{If(lang = "VB", "vb", "cs")}"

                                  Dim ass As Assembly = Assembly.GetExecutingAssembly
                                  Dim resourceName As String = $"BertoSoftware.MySql.TableToModel{lang}.sql"
                                  Dim query As String

                                  Using stream As Stream = ass.GetManifestResourceStream(resourceName)
                                      Using reader As StreamReader = New StreamReader(stream)
                                          query = reader.ReadToEnd
                                      End Using
                                  End Using

                                  query = query.Replace("%DATABASE%", cn.Database)
                                  query = query.Replace("%TABLENAME%", informationSchema.TABLE_NAME)
                                  query = query.Replace("%NAMESPACE%", [namespace])

                                  Dim result = cn.Query(Of String)(query)

                                  If deleteFolder = True Then
                                      If IO.Directory.Exists(path) Then
                                          IO.Directory.Delete(path)
                                      End If
                                  End If

                                  IO.File.WriteAllText(filename, result(0))

                                  rowCount += 1
                              Next
                          End Sub)

                If cn.State = ConnectionState.Open Then
                    cn.Close()
                End If

                Return New ResultTask With {.isSuccess = True}

            Catch ex As Exception
                Return New ResultTask With {.isSuccess = False, .ErrorMessage = ex.Message, .Exception = ex}
            End Try
        End Function
    End Class

End Namespace