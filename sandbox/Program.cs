using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using EFModeling.EntityProperties.FluentAPI.Required;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Ember.Scripting;
using System.Reflection;
using Serilog.Extensions.Logging;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;


string scriptFolderPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "Scripts"));
string userName = "Gilles";
int currentApiVersion;
// int currentSdkVersion = 1;

try
{

    using (var db = new MyContext())
    {
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    var logger = new LoggerForScripting();
    Log.Logger = logger.SetUpAndGetSeriLogger(); // ADD THIS LINE
    Log.Debug("Sandbox launched.");

    var services = new ServiceCollection();
    services.AddLogging(builder =>
    {
        // Assuming you use Serilog, this forwards standard MS Logging to Serilog
        builder.AddSerilog(dispose: true);
    });

    ScriptingServiceCollectionExtensions.AddEmberScripting(services, EmberMethods.GetReferences(), EmberMethods.GetEmberApiVersion());

    using var provider = services.BuildServiceProvider();
    await MainProgramSwitch(provider);
}
finally
{
    Log.Debug("Sandbox closing.");
    await Log.CloseAndFlushAsync();
}

async Task MainProgramSwitch(IServiceProvider provider)
{
    try
    {
        // ScriptManagerFacade facade = provider.GetRequiredService<ScriptManagerFacade>();
        ISccriptManagerDeleteAfter facade = provider.GetRequiredService<ISccriptManagerDeleteAfter>();
        IScriptManager facade1 = provider.GetRequiredService<IScriptManager>();
        currentApiVersion = facade.GetRunningApiVersion();

        EmberMethods em = new EmberMethods(facade);

        Dictionary<int, Guid> sourceDict = await em.ListAllStoredSourceCodes();
        Dictionary<int, Guid> cacheDict = [];
        Guid scriptId = new Guid();

        await facade.PrecompileForApiVersion();

        bool running = true;
        while (running)
        {
            try
            {
                Console.WriteLine("Enter the function you want to run: ");
                string? userInput = Console.ReadLine();
                switch (userInput)
                {
                    case null:
                    case "":
                    case " ":
                        Console.WriteLine("You entered nothing try again.");
                        break;
                    case "exit":
                        running = false;
                        break;
                    case "source":

                        await em.GetSourceCodeInSwitch();
                        break;
                    case "reset":
                        await facade.EnsureDeletedCreated();
                        await em.ListAllCompiledFromDB();
                        break;
                    case "comp":
                        await em.CompileAllScriptsInFolderAndSaveToDB(scriptFolderPath, userName, currentApiVersion);

                        await em.ListAllCompiledFromDB();
                        break;
                    case "comp source":

                        await facade.CompileAllScripts();
                        break;
                    case "comp mv":
                        scriptId = await em.GetIdInConsoleAsync(fromSrc: true);
                        for (int i = 0; i < 5; i++)
                        {
                            await facade.CompileScript(scriptId, i);
                        }
                        break;
                    case "ls":
                        cacheDict = await em.ListAllCompiledFromDB();
                        break;
                    case "ls source":
                        sourceDict = await em.ListAllStoredSourceCodes();
                        break;
                    case "dupes":
                        await facade.RemoveDuplicates();
                        break;
                    case "deleteCache":
                        await facade.ClearAllCaches();
                        break;

                    #region Script Lifecycle

                    case "CreateScript":
                        string sourceCode = EmberMethods.CreateStringFromCsFile(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "Scripts", "ConditionScripts", "PediatricCondition.cs")));
                        await facade.CreateScript(sourceCode, "Gilles");
                        break;
                    case "CreateScriptWithOld":
                        string sourceCodeOld = EmberMethods.CreateStringFromCsFile(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "Scripts", "ConditionScripts", "PediatricCondition.cs")));
                        await facade.CreateScript(sourceCodeOld, "Gilles", 2);
                        break;

                    case "UpdateScript":
                        Guid idEdit = await em.GetIdInConsoleAsync(fromSrc: true);

                        var customerScript = await facade.GetScript(idEdit);
                        var creationDate = customerScript.CreatedAt;

                        Console.WriteLine("Copy paste your new version file path now:");
                        string? userInput2 = Console.ReadLine();

                        string? str = EmberMethods.CreateStringFromCsFile(userInput2!);

                        //In reality it would be better like this but doesnt work because cant paste too much in console:
                        // Console.WriteLine("Copy paste your new version now:");
                        // string userInput2 = Console.ReadLine();

                        await facade.UpdateScript(idEdit, str, userName);
                        break;

                    case "DeleteScript":
                        scriptId = await em.GetIdInConsoleAsync(fromSrc: true);
                        await facade.DeleteScript(scriptId);
                        break;

                    case "GetScript":
                        scriptId = await em.GetIdInConsoleAsync(fromSrc: true);
                        CustomerScript s = await facade.GetScript(scriptId);
                        Console.WriteLine(s.ToString());
                        break;

                    case "ListScripts":
                        var list = await facade.ListScripts();
                        foreach (var item in list)
                        {
                            Console.WriteLine(item.ToStringShorter());
                        }
                        break;

                    #endregion

                    #region Compilation Operations

                    case "CompileScript":   //todo check
                        scriptId = await em.GetIdInConsoleAsync(fromSrc: true);
                        await facade.CompileScript(scriptId);    //should not compile if already present in db;
                        break;

                    case "CompileAllScripts":
                        await facade.CompileAllScripts();
                        break;

                    case "RecompileScript":
                        scriptId = await em.GetIdInConsoleAsync(fromSrc: true);
                        await facade.RecompileScript(scriptId);
                        break;

                    case "ValidateScript":
                        scriptId = await em.GetIdInConsoleAsync(fromSrc: true);
                        var script = await facade.GetScript(scriptId);
                        string answer = facade.ValidateScript(script.SourceCode!);
                        Console.WriteLine(answer);
                        break;

                    case "GetCompilationErrors":    //todo test by turning off auto check when adding same with above
                        scriptId = await em.GetIdInConsoleAsync(fromSrc: true);
                        string compAnswer = await facade.GetCompilationErrors(scriptId);
                        Console.WriteLine(compAnswer);
                        break;

                    #endregion

                    #region Execution Operations

                    case "ExecuteActionScript":
                        scriptId = await em.GetIdInConsoleAsync(fromSrc: true);
                        ActionResultSF result = await facade.ExecuteActionScript(scriptId, await em.GetTestingContext<GeneratorContextV4.GeneratorContext>());
                        Console.WriteLine(result.ToString());   //you could do whatever with it
                        break;

                    case "ExecuteConditionScript":
                        scriptId = await em.GetIdInConsoleAsync(fromSrc: true);
                        bool resultCond = await facade.ExecuteConditionScript(scriptId, await em.GetTestingContext<GeneratorContextV4.GeneratorContext>());
                        Console.WriteLine(resultCond);
                        break;

                    case "ExecuteScriptById":
                        scriptId = await em.GetIdInConsoleAsync(fromSrc: true);
                        object resultObj = await facade.ExecuteScriptById(scriptId, await em.GetTestingContext<GeneratorContextV4.GeneratorContext>());

                        if (typeof(bool).IsAssignableFrom(resultObj.GetType()))       //good idea to check what type was returne, you could also just check the property from db normally
                        {
                            Console.WriteLine("Script returned a bool: " + resultObj.ToString());
                        }
                        else if (typeof(ActionResultSF).IsAssignableFrom(resultObj.GetType()))
                        {
                            Console.WriteLine("Script returned an ActionResult: " + resultObj.ToString());
                        }
                        else
                        {
                            Console.WriteLine("Script was neither returned a bool nor an ActionResultBaseClass, it was a: " + resultObj.GetType().ToString());
                        }
                        break;

                    #endregion

                    #region Cache Management

                    case "GetCompiledCache":
                        scriptId = await em.GetIdInConsoleAsync();
                        ScriptCompiledCache cache = await facade.GetCompiledCache(scriptId, currentApiVersion);
                        byte[] arr = cache.AssemblyBytes!;
                        Console.WriteLine(arr.ToString());
                        break;

                    case "ClearScriptCache":
                        scriptId = await em.GetIdInConsoleAsync(fromSrc: true);
                        await facade.ClearScriptCache(scriptId);
                        break;

                    case "ClearAllCaches":
                        await facade.ClearAllCaches();
                        break;

                    case "PrecompileForApiVersion":
                        await facade.PrecompileForApiVersion();
                        break;

                    #endregion

                    #region Version Management

                    case "GetActiveApiVersions":
                        var ls = await facade.GetActiveApiVersions();    //todo
                        string vs = "";
                        foreach (var item in ls)
                        {
                            vs = vs + " " + item;
                        }
                        Console.WriteLine("Active Api versions: " + vs);
                        break;

                    case "GetScriptCompatibility":
                        scriptId = await em.GetIdInConsoleAsync(fromSrc: true);
                        int compa = await facade.GetScriptCompatibility(scriptId);
                        Console.WriteLine("Min api version: " + compa);
                        break;

                    case "CheckVersionCompatibility":
                        scriptId = await em.GetIdInConsoleAsync(fromSrc: true);
                        bool check = await facade.CheckVersionCompatibility(scriptId, currentApiVersion);
                        Console.WriteLine("CheckVersionCompatibility with ApiV: " + currentApiVersion + " is " + check);
                        break;

                    case "RegisterEmberInstance":
                        // await facade.RegisterEmberInstance(instanceId, emberVersion, apiVersion);
                        break;

                    #endregion

                    #region Duplicate Detection & Cleanup

                    case "DetectDuplicates":
                        var dupes = await facade.DetectDuplicates();
                        List<Guid> scriptGUIDs = dupes.scriptGUIDs;
                        Dictionary<Guid, int> cacheGUIDs = dupes.cacheGUIDs;
                        string rtrnStr1 = "Script Guids to remove: ";
                        foreach (var item in scriptGUIDs)
                        {
                            rtrnStr1 = rtrnStr1 + " , " + item;
                        }
                        string rtrnStr2 = "Cache Guids to remove: ";
                        foreach (var item in cacheGUIDs)
                        {
                            rtrnStr2 = rtrnStr2 + " , " + item;
                        }
                        Console.WriteLine(rtrnStr1);
                        Console.WriteLine(rtrnStr2);
                        break;

                    case "RemoveDuplicates":
                        await facade.RemoveDuplicates();
                        break;

                    case "CleanupOrphanedCaches":
                        await facade.CleanupOrphanedCaches();
                        break;

                    #endregion

                    #region Monitoring & Diagnostics

                    case "GetScriptExecutionHistory":
                        // await facade.GetScriptExecutionHistory(scriptId);
                        break;

                    case "GetCompilationStatistics":
                        // await facade.GetCompilationStatistics();
                        break;

                    case "HealthCheck":
                        await facade.HealthCheck();
                        break;

                    case "GetScriptMetadata":
                        scriptId = await em.GetIdInConsoleAsync(fromSrc: true);
                        string strr = await facade.GetScriptMetadata(scriptId);
                        Console.WriteLine(strr);
                        break;
                    case "GetUserName":
                        string strrr = facade.GetUserName();
                        Console.WriteLine(strrr);
                        break;

                    #endregion

                    default:
                        sourceDict = await em.ListAllStoredSourceCodes(dontPrint: true);
                        scriptId = sourceDict[Int32.Parse(userInput)];
                        CustomerScript justForTesting = await facade.GetScript(scriptId);
                        object resultObj2 = await facade.ExecuteScriptById(scriptId, await em.GetTestingContext<GeneratorContextNoInherVaccineV5.GeneratorContext>(autoDetectFromScript: justForTesting));

                        if (typeof(bool).IsAssignableFrom(resultObj2.GetType()))       //good idea to check what type was returne, you could also just check the property from db normally
                        {
                            Console.WriteLine("Script returned a bool: " + resultObj2.ToString());
                        }
                        else if (typeof(ActionResultSF).IsAssignableFrom(resultObj2.GetType()))
                        {
                            Console.WriteLine("Script returned an ActionResult: " + resultObj2.ToString());
                        }
                        else
                        {
                            Console.WriteLine("Script was neither returned a bool nor an ActionResult, it was a: " + resultObj2.GetType().ToString());
                        }
                        break;
                }

            }
            catch (DbUpdateException e)
            {
                Console.WriteLine("Database Error: " + e.InnerException?.Message);  //todo check what this really does and if superior to just tostring
                Console.WriteLine("Database Error: " + e.ToString());
            }
            catch (NoFileWithThisClassNameFoundException)
            {
                Console.WriteLine("No file with this Class name found, make sure the file name is the same as the class name.");
            }
            catch (CompilationFailedException)
            {
                Console.WriteLine("Something went wrong when trying to compile your script, but you can try again.");
            }
            catch (ScriptExecutionException)
            {
                Console.WriteLine("Something went wrong when trying to execute your script, but you can try again.");
            }
            // catch (ConditionScriptExecutionException)
            // {
            //     Console.WriteLine("Something went wrong trying to execute your ConditionScript, but you can try again.");
            // }
            // catch (ActionScriptExecutionException)
            // {
            //     Console.WriteLine("Something went wrong trying to execute your ActionScript, but you can try again.");
            // }
            catch (GetScriptPathFromFolderException)
            {
                Console.WriteLine("Something went wrong when trying to get the Script path from the folder, but you can try again.");
            }
            catch (CreateStringFromCsFileException)
            {
                Console.WriteLine("Something went wrong trying to create the string from the .cs file, but you can try again.");
            }
            catch (NoClassFoundInScriptFileException)
            {
                //todo
            }
            catch (MoreThanOneClassFoundInScriptException)
            {
                //todo
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
    }

}

