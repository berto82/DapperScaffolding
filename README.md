![Banner](https://i.imgur.com/UNRrpWE.png)

# Dapper Scaffolding

Scaffold Dapper-friendly model classes from an existing database schema.

This CLI tool inspects the database schema using the `DefaultConnection` string in a target project's `appsettings.json` and generates simple POCO model files (VB.NET or C#) into a specified folder inside that project.

**Features**
- Generate one model file per table using provider-specific scaffolding.
- Console UI with progress and status messages.
- Output languages: VB.NET and C#.
- Supports SQL Server and MySQL providers (others declared in the CLI are not implemented).

**Requirements**
- .NET SDK compatible with the solution (use `dotnet --version` to check).
- A target project containing `appsettings.json` with a `DefaultConnection` connection string.

**Quick Start**

-  Install the package globally using the .NET CLI:

```bash
dotnet tool install -g BertoSoftware.DapperScaffolding
```

Run the scaffold command (example):

```bash
dapper-scaffolding scaffold --project YourProject.csproj --provider SqlServer --language CSharp --output Model --delete-folder true
```

Options summary:
- `--project` (`-prj`): Path to the target project file containing `appsettings.json`. (required)
- `--provider` (`-prov`): Database provider. Supported: `SqlServer`, `MySql`. (defaults to `SqlServer`)
- `--language` (`-lang`): Output language. Values: `VBNet`, `CSharp`. (defaults to `CSharp`)
- `--output` (`-out`): Output folder relative to the project directory. (defaults to `Model`)
- `--delete-folder` (`-del`) (boolean flag): (optional) When provided, the target output folder is deleted and recreated before generation. (default: false)

**Configuration**
The tool reads the `DefaultConnection` string from the target project's `appsettings.json`. Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyDb;User Id=sa;Password=YourPassword;"
  }
}
```

If `appsettings.json` is missing or `DefaultConnection` is not present, the tool will exit with a clear error message.

**Supported Providers**
- `SqlServer` (implemented)
- `MySql` (implemented)
- `PostgreSql` (declared, not implemented)
- `Sqlite` (declared, not implemented)
- `Firebird` (declared, not implemented)

**Behavior & Output**
- The tool connects to the database and enumerates tables (via INFORMATION_SCHEMA or provider-specific queries).
- For each table, it applies an embedded SQL template and writes a `.vb` or `.cs` file named `<TableName>.vb`/`<TableName>.cs` into the chosen output folder inside the project directory.
- After generation, include the generated folder in your project (e.g., add a `<Compile Include="Model\\**\\*.vb" />` entry in a  `.vbproj`).

**Example project**

See the example project configuration at [DapperScaffoldingExample/appsettings.json](DapperScaffoldingExample/appsettings.json).

**Where to look in the source**
- CLI and options: [DapperScaffolding/Program.vb](DapperScaffolding/Program.vb)
- Providers implementation: [DapperScaffolding/Providers](DapperScaffolding/Providers)

**License & Links**
- License: MIT
- Repository: https://github.com/berto82/DapperScaffolding
