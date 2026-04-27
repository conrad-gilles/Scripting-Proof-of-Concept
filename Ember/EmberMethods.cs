using System.Globalization;
using System.Reflection;
using Ember.Scripting;
using Microsoft.CodeAnalysis;
using Serilog;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TypeInfo = System.Reflection.TypeInfo;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore;
using ContextBases;
using System.Runtime.CompilerServices;

public class EmberMethods
{
    private readonly IScriptManagerExtended _facade;

    public EmberMethods(IScriptManagerExtended facade)
    {
        _facade = facade;
    }
    public static int GetEmberApiVersion(int? testingDiffrentVersion = null)
    {
        Log.Debug("Entered {MethodName} in {ClassName}.", nameof(GetEmberApiVersion), nameof(EmberMethods));
        if (testingDiffrentVersion != null)
        {
            return (int)testingDiffrentVersion;
        }

        return 5;
    }

    public static IScriptManagerExtended GetNewScriptManagerInstance(int? apiVersion = null)
    {
        IScriptManagerExtended facade;
        ServiceCollection services2 = new ServiceCollection();

        LoggerForScripting logger = new LoggerForScripting();
        Log.Debug("New instance.");

        services2 = new ServiceCollection();
        services2.AddLogging(builder =>
        {
            builder.AddSerilog(dispose: true);
        });
        services2.AddSingleton<IUserSession, SandBoxUserSession>();

        services2.AddDbContextFactory<ScriptDbContext>();
        ScriptingServiceCollectionExtensions.AddEmberScripting(services2, EmberMethods.GetReferences(), EmberMethods.GetEmberApiVersion(testingDiffrentVersion: apiVersion), RecentTypeHelper.GetRecentTypes());
        var provider2 = services2.BuildServiceProvider();
        return facade = provider2.GetRequiredService<IScriptManagerExtended>();
    }

    public async Task CompileAllScriptsInFolderAndSaveToDB(string? userName = null, int? currentApiVersion = null)
    {
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName}.", nameof(CompileAllScriptsInFolderAndSaveToDB), nameof(EmberMethods));

        string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts");


        string[] files = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);  //todo check for infinite loop  https://msdn.microsoft.com/en-us/library/ms143316(v=vs.110).aspx

        string successfullyInserted = "";
        string failedInserted = "";
        for (int i = 0; i < files.Length; i++)
        {
            string scriptString = CreateStringFromCsFile(files[i]);
            string vali = "Default Name";
            try
            {
                vali = _facade.BasicValidationBeforeCompiling(scriptString).ClassName;
            }
            catch (System.Exception) { }
            try
            {

                Guid id = Guid.NewGuid();
                Guid randomTestScript2Id = (await _facade.CreateScript(scriptString)).Id;
                successfullyInserted = successfullyInserted + ", " + vali;

            }
            catch
            {
                failedInserted = failedInserted + ", " + vali;
            }

        }
        if (failedInserted != "")
        {
            string message = "Could not insert scripts : " + failedInserted + ", Successfully inserted script: " + successfullyInserted;
            Console.WriteLine(message);
            throw new CompileAllScriptsInFolderException(message: message);
        }
    }

    public static async Task<Dictionary<string, string>> GetScriptTemplates(string? folderPath = null, string? userName = null, int? currentApiVersion = null)
    {
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName}.", nameof(GetScriptTemplates), nameof(EmberMethods));
        if (folderPath == null)
        {
            folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts");
        }

        Dictionary<string, string> returnedDict = [];

        string[] files = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string scriptString = CreateStringFromCsFile(file);
                returnedDict.Add(fileName, scriptString);
            }
            catch { continue; }
        }
        return returnedDict;
    }

    public static List<MetadataReference> GetReferences()
    {
        List<MetadataReference> _references = [
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(IScriptType).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IGeneratorReadOnlyContextV1.IGeneratorContext).Assembly.Location),
                          ];
        return _references;
    }

    public static string CreateStringFromCsFile(string scriptPath)
    {
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName} with path: {ScriptPath}.", nameof(CreateStringFromCsFile), nameof(EmberMethods), scriptPath);
        try
        {
            // Display the lines that are read from the file
            string text = File.ReadAllText(scriptPath);
            return text;
        }
        catch (Exception e)
        {
            // Displays the error on the screen.
            Console.WriteLine(e.ToString());
            Console.WriteLine("The file could not be read: probably because typo in classOfScriptToExecute variable:");
            throw new CreateStringFromCsFileException("The file could not be read: probably because typo in classOfScriptToExecute variable:", e);    // todo correct error handling throw an error
        }
        // Source https://www.tutorialspoint.com/chash-program-to-create-string-from-contents-of-a-file //obv i changed a lot but i still copied
    }
    //The following method statement was AI Generated
    public static List<ScriptCompilationError> ParseCompilationErrors(Microsoft.CodeAnalysis.Emit.EmitResult? emitResult)
    {
        if (emitResult == null || emitResult.Success == true)
        {
            return [];
        }

        List<ScriptCompilationError> compilerErrors = emitResult.Diagnostics
        .Where(d => d.IsWarningAsError || d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
        .Select(d =>
        {
            var hasLocation = d.Location != Location.None;
            var lineSpan = hasLocation ? d.Location.GetLineSpan() : default;

            int startLine = hasLocation ? lineSpan.StartLinePosition.Line + 1 : 1;
            int startCol = hasLocation ? lineSpan.StartLinePosition.Character + 1 : 1;
            int endLine = hasLocation ? lineSpan.EndLinePosition.Line + 1 : 1;
            int endCol = hasLocation ? lineSpan.EndLinePosition.Character + 1 : 1;

            // Force a minimum width of 1 character so Monaco actually draws a squiggly line.
            // For CS0161 (missing return), Roslyn sometimes gives the exact method name location.
            if (startLine == endLine && startCol == endCol)
            {
                endCol = startCol + 1; // Minimum 1 char width
            }

            return new ScriptCompilationError(
                Id: d.Id,
                Message: d.GetMessage(),
                Line: startLine,
                Column: startCol,
                EndLine: endLine,
                EndColumn: endCol,
                IsError: d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error
            );
        }).ToList();

        Log.Error("Compilation failed with {ErrorCount} errors.", compilerErrors.Count);

        string errorString = "Errors: ";
        foreach (var error in compilerErrors)
        {
            errorString = errorString + error.ToString() + ", ";
        }

        // Throw passing the clean DTOs
        // throw new CompilationFailedException("Compilation failed, " + errorString, compilerErrors);
        return compilerErrors;
    }
}
