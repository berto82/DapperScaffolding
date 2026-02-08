Imports System.CommandLine
Imports System.Reflection
Imports Figgle.Fonts
Imports Microsoft.Data.SqlClient
Imports Microsoft.Extensions.Configuration
Imports Spectre.Console

Module Program

    ''' <summary>
    ''' The ProviderType enum defines the supported database providers for scaffolding Dapper code.
    ''' </summary>
    Enum ProviderType
        SqlServer
        MySql
        PostgreSql
        Sqlite
        Firebird
    End Enum

    ''' <summary>
    ''' The LanguageType enum defines the supported programming languages for the generated Dapper code.
    ''' </summary>
    Enum LanguageType
        VBNet
        CSharp
    End Enum

    Dim sqlScaffolder As Providers.SQLServer.Scaffolding
    Dim mySqlScaffolder As Providers.MySql.Scaffolding

    Sub Main(args As String())

        AnsiConsole.MarkupLine($"[gray]{FiggleFonts.Standard.Render("Dapper Scaffolding")}[/]")
        AnsiConsole.WriteLine()
        AnsiConsole.MarkupLine($"License: [green]MIT[/] - Version: [green]{Assembly.GetExecutingAssembly.GetName().Version}[/]")
        AnsiConsole.MarkupLine("Take a look on my GitHub: [blue]https://github.com/berto82/DapperScaffolding[/]")
        AnsiConsole.WriteLine()

        Dim root As New RootCommand("Scaffold Dapper code from a database schema.")

        Dim command As New Command("scaffold", "Scaffold Dapper code from a database schema.")

        Dim projectOption As New [Option](Of String)("--project", {"-prj"}) With {
            .Description = "The path to the project where the generated code will be placed.",
            .Required = True
             }

        Dim providerOption As New [Option](Of ProviderType)("--provider", {"-prov"}) With {
            .Description = "The database provider (e.g., SqlServer, MySql, PostgreSql).",
            .Required = True,
            .DefaultValueFactory = Function() ProviderType.SqlServer
             }

        Dim languageOption As New [Option](Of LanguageType)("--language", {"-lang"}) With {
            .Description = "The programming language for the generated code (e.g., VB.NET, C#).",
            .Required = True,
            .DefaultValueFactory = Function() LanguageType.CSharp
             }

        Dim outputOption As New [Option](Of String)("--output", {"-out"}) With {
                .Description = "The output directory for the generated code files.",
                .Required = True,
                .DefaultValueFactory = Function() "Model"
                 }

        Dim deleteFolderOption As New [Option](Of Boolean)("--delete-folder", {"-del"}) With {
                .Description = "Delete the output folder before generating new code.",
                .Required = False,
                .DefaultValueFactory = Function() False
                 }

        root.Subcommands.Add(command)

        command.Options.Add(projectOption)
        command.Options.Add(providerOption)
        command.Options.Add(languageOption)
        command.Options.Add(outputOption)
        command.Options.Add(deleteFolderOption)

        command.Validators.Add(Sub(result)
                                   Dim proj = result.GetValue(projectOption)

                                   If String.IsNullOrWhiteSpace(proj) Then
                                       result.AddError("The --project option is required.")
                                   End If

                               End Sub)

        command.SetAction(Sub(parseActionResult As ParseResult)
                              AnsiConsole.MarkupLine($"{Emoji.Known.Rocket} Scaffolding dapper code")

                              Dim project As String = parseActionResult.GetValue(projectOption)
                              Dim provider As ProviderType = parseActionResult.GetValue(providerOption)
                              Dim lang As LanguageType = parseActionResult.GetValue(languageOption)
                              Dim output As String = parseActionResult.GetValue(outputOption)
                              Dim deleteFolder As Boolean = parseActionResult.GetValue(deleteFolderOption)

                              Dim panel As New Panel($"[yellow]Project:[/] {project}{vbCrLf}[yellow]Provider:[/] {provider}{vbCrLf}[yellow]Language:[/] {lang}{vbCrLf}[yellow]Output Folder:[/] {output}{vbCrLf}[yellow]Delete Folder:[/] {deleteFolder}") With {
                                  .Border = BoxBorder.Rounded,
                                  .Header = New PanelHeader("Configuration parameters", Justify.Center)
                              }

                              AnsiConsole.Write(panel)

                              Dim resultTask As ResultTask = StartScaffolding(project, provider, lang, output, deleteFolder)

                              If resultTask.isSuccess Then
                                  AnsiConsole.MarkupLine($"{Emoji.Known.DogFace} Models generated successfully!")
                                  AnsiConsole.MarkupLine($"{Emoji.Known.Bell} Remember to add the generated folder into your project!")
                                  AnsiConsole.WriteLine()
                                  AnsiConsole.MarkupLine($"{Emoji.Known.PartyPopper} [green]Done. Enjoy![/]")
                              Else
                                  AnsiConsole.MarkupLine($"{Emoji.Known.AngryFace} [red]An error occurred during scaffolding. Check the error messages above.[/]")
                                  AnsiConsole.MarkupLine($"{Emoji.Known.PileOfPoo} [red]{resultTask.ErrorMessage}[/]")
                              End If

                          End Sub)

        Dim parseResult As ParseResult = root.Parse(args)
        parseResult.Invoke()

    End Sub

    Private Function StartScaffolding(project As String, provider As ProviderType, lang As LanguageType, targetFolder As String, deleteFolder As Boolean) As ResultTask

        Dim projectPath As String = New IO.FileInfo(project).DirectoryName

        Dim resultTask As ResultTask = Nothing

        If IO.File.Exists($"{projectPath}\appsettings.json") Then
            AnsiConsole.MarkupLine($"{Emoji.Known.Bone} Configuration file was founded")

            Dim cnString As String = New ConfigurationBuilder().SetBasePath(projectPath).AddJsonFile("appsettings.json").Build.GetConnectionString("DefaultConnection")

            If cnString Is Nothing Then
                Return New ResultTask With {.isSuccess = False, .ErrorMessage = "Connection string 'DefaultConnection' is not found in appsettings.json"}
            End If

            AnsiConsole.MarkupLine($"{Emoji.Known.Eye}  A valid connection string was found")

            Select Case provider
                Case ProviderType.SqlServer
                    Dim sqlcn As New SqlConnection(cnString)

                    sqlScaffolder = New Providers.SQLServer.Scaffolding()
                    resultTask = sqlScaffolder.Convert(sqlcn, IO.Path.Combine(projectPath, targetFolder), If(lang = LanguageType.VBNet, "VB", "CS"), targetFolder, deleteFolder)

                    Return resultTask
                Case ProviderType.MySql
                    Dim sqlcn As New MySqlConnector.MySqlConnection(cnString)

                    mySqlScaffolder = New Providers.MySql.Scaffolding
                    resultTask = mySqlScaffolder.Convert(sqlcn, IO.Path.Combine(projectPath, targetFolder), If(lang = LanguageType.VBNet, "VB", "CS"), targetFolder, deleteFolder)

                    Return resultTask
                Case Else
                    Throw New NotImplementedException("Only SQL Server provider is implemented at this time.")
            End Select
        Else
            Return New ResultTask With {.isSuccess = False, .ErrorMessage = "appsetting.json configuration file is not found"}
        End If

    End Function

End Module
