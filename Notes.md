## To isolate code:
	before .NET 5 APP domains to isolate code but now deprecated

	Process Isolation (Standard): You run the user's script in a separate .exe process. If the 	script crashes or tries to delete files, it only kills its own process, not the main LIMS. 	You communicate via standard input/output or gRPC.

	WebAssembly / WASM (Advanced): This is the modern "gold standard" for 2025/2026 security. 	You compile the user's C# script into WebAssembly (WASM) and run it inside a sandbox (like 	Wasmtime). This makes it mathematically impossible for the script to access files or memory 	you didn't explicitly allow.


## Quick Setup for VS Code (if you choose it)
If you stick with VS Code, run this in your terminal to create the console app structure manually (since you don't have the VS "New Project" wizard):

bash
mkdir RoslynTest
cd RoslynTest
dotnet new console
dotnet add package Microsoft.CodeAnalysis.CSharp.Scripting
code .

## Code into Program.cs and run using:
dotnet run

docker exec -it script_db_container psql -U admin -d script_registry -c "\dt"

docker compose up -d

docker ps

docker exec -it script_db_container bash

psql -U username -d database_name
psql -U admin -d script_registry	//some like this

dotnet add package Npgsql

Host=localhost;Port=5432;Database=script_registry;Username=admin;Password=your_secure_password


Todo:
	- implement in facade GetActiveApiVersions()
	- when executeaction scrip or execute cond script it executes the script even when given script but it fails to return fix this for stability or just remove the option
	- implement observer patter for updates on db
	- maybe ember version check
	- create a Console Application (e.g., compiler.exe) that references the same class library. This tool would take arguments    like compiler.exe --input "script.cs" --output "script.dll"
	- compilation per api/sdk version because multiple versions of ember could be running in the enivronment, that use diffrent versions
	- Implement a Script Runner and Script Manager, or an alternative architecture as deemed appropriate by the student, responsible for selecting and executing the correct compiled script version, instantiating the script context, and invoking the script using the chosen execution model.
	- script timeouts etc, logging, error handling, etc.

	-For Real Execution: If you tried to take these bytes and load them into a real old V4 application, they might crash if V5 introduced breaking changes (like new methods in interfaces). To truly compile for V4, you would need to modify ScriptCompiler.cs to load references from a specific folder (e.g., ./libs/v4/) instead of using typeof(Type).Assembly.Location.

Commands to set up db:
docker-compose up -d

dotnet add package Microsoft.Extensions.Logging.Abstractions
# 1. Build the project
dotnet build -c Release

dotnet publish -c Release -f net9.0 -r win-x64 --self-contained -o ./TempPublish

dotnet run --project sandbox          # runs your app normally
dotnet test sandbox.Tests             # runs only tests

# Microsoft Logger Tutorial:
From lowest severity to highest:
### Trace:
logger.LogTrace("Entered the CompileScript method with scriptId: {Id}", scriptId);

### Debug
logger.LogDebug("Fetched {Count} cached scripts from the database", cacheDict.Count);

### Information
logger.LogInformation("Successfully compiled script {ScriptName}", script.Name);

### Warning
logger.LogWarning("No API version specified, falling back to default version {Version}", currentApiVersion);

### Error
try
{
    // code that throws
}
catch (CompilationFailedException ex)
{
    logger.LogError(ex, "Failed to compile script {ScriptId} due to syntax errors", scriptId);
}

### Critical
logger.LogCritical(ex, "The database connection dropped. ScriptManager cannot continue.");

