using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using EFModeling.EntityProperties.FluentAPI.Required;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.EntityFrameworkCore;

class MainProgram
{
    // private static string scriptFolderPath = @"C:\Users\Gilles\Desktop\UNI\Semester 6\Code\Codebase\labsolutionlu-ember-scripting-fb966c220f60\sandbox\src\Scripts";
    private static string scriptFolderPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "Scripts"));
    // private static string scriptFolderPath = @"C:\Users\Gilles\Desktop\ScriptsForProject\V2";    //out of scope to test errors
    private static string userName = "Gilles";

    // private static int currentApiVersion = 6;   //get rid of this and name todo
    private static int currentApiVersion;   //set below
    private static int currentSdkVersion = 1;
    static async Task Main(string[] args)
    {
        // await new DbHelper().ensureDeletedCreated();       //only for testing
        await MainProgramSwitch();
        // await RandomMethods.MainProgramSwitchAsync(scriptFolderPath);
    }

    public static async Task MainProgramSwitch()
    {
        try
        {
            var db = new DbHelper();

            // await RandomMethods.CompileAllScriptsInFolderAndSaveToDB(scriptFolderPath);   //main precompilation act
            // Dictionary<int, Guid> cacheDict = await RandomMethods.ListAllCompiledFromDB();
            Dictionary<int, Guid> sourceDict = await RandomMethods.ListAllStoredSourceCodes();
            // Dictionary<int, Guid> sourceDict = [];
            Dictionary<int, Guid> cacheDict = [];
            Guid scriptId = new Guid();
            ScriptManagerFacade facade = new ScriptManagerFacade();
            currentApiVersion = await facade.GetRecentApiVersion(); //needs to be above autocomp else error

            await db.AutomaticCompilationOnVersionUpdate(currentApiVersion);    //todo make this "background job that is run automatically periodically"


            bool running = true;
            while (running)
            {
                try
                {
                    Console.WriteLine("Enter the function you want to run: ");
                    string userInput = Console.ReadLine();
                    // Switch statement to test ScriptManagerFacade functions
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

                            await RandomMethods.GetSourceCodeInSwitch();
                            break;
                        case "reset":
                            await new DbHelper().EnsureDeletedCreated();
                            // cacheDict = 
                            await RandomMethods.ListAllCompiledFromDB();
                            break;
                        case "comp":    //doesnt compile if isDuplicate is eneblaed bc scripts are already in db so no cache is created maybe todo
                            await RandomMethods.CompileAllScriptsInFolderAndSaveToDB(scriptFolderPath, userName, currentApiVersion);   //main precompilation act
                                                                                                                                       // cacheDict = 
                            await RandomMethods.ListAllCompiledFromDB();
                            break;
                        case "comp source":
                            await new DbHelper().CompileAllStoredScripts(currentApiVersion);
                            break;
                        case "comp mv": //no functioality just for testing
                            scriptId = await UsefulMethods.GetIdInConsoleAsync(fromSrc: true);
                            for (int i = 0; i < 5; i++)
                            {
                                await facade.CompileScript(scriptId, i);
                            }
                            break;
                        case "ls":
                            cacheDict = await RandomMethods.ListAllCompiledFromDB();
                            break;
                        case "ls source":
                            sourceDict = await RandomMethods.ListAllStoredSourceCodes();
                            break;
                        case "dupes":
                            await new DbHelper().RemoveDuplicates();
                            break;
                        case "deleteCache":
                            await new DbHelper().DeleteAllCachedScripts();
                            break;

                        #region Script Lifecycle

                        case "CreateScript":
                            // string sourceCode = UsefulMethods.CreateStringFromCsFile(@"C:\Users\Gilles\Desktop\UNI\Semester 6\Code\Codebase\labsolutionlu-ember-scripting-fb966c220f60\sandbox\src\Scripts\ConditionScripts\PediatricCondition.cs");
                            string sourceCode = UsefulMethods.CreateStringFromCsFile(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "Scripts", "ConditionScripts", "PediatricCondition.cs")));
                            await facade.CreateScript(sourceCode, "ConditionScriptTest", "Gilles");
                            break;
                        case "CreateScriptWithOld":
                            // string sourceCodeOld = UsefulMethods.CreateStringFromCsFile(@"C:\Users\Gilles\Desktop\UNI\Semester 6\Code\Codebase\labsolutionlu-ember-scripting-fb966c220f60\sandbox\src\Scripts\ConditionScripts\PediatricCondition.cs");
                            string sourceCodeOld = UsefulMethods.CreateStringFromCsFile(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "Scripts", "ConditionScripts", "PediatricCondition.cs")));
                            await facade.CreateScript(sourceCodeOld, "ConditionScriptTest", "Gilles", 1);
                            break;

                        case "UpdateScript":
                            Guid idEdit = await UsefulMethods.GetIdInConsoleAsync(fromSrc: true);

                            var customerScript = await db.GetCustomerScript(idEdit);
                            var creationDate = customerScript.CreatedAt;

                            Console.WriteLine("Copy paste your new version file path now:");
                            string userInput2 = Console.ReadLine();

                            string str = UsefulMethods.CreateStringFromCsFile(userInput2);

                            //In reality it would be better like this but doesnt work because cant paste too much in console:
                            // Console.WriteLine("Copy paste your new version now:");
                            // string userInput2 = Console.ReadLine();

                            await facade.UpdateScript(idEdit, str, userName);
                            break;

                        case "DeleteScript":
                            scriptId = await UsefulMethods.GetIdInConsoleAsync(fromSrc: true);
                            await facade.DeleteScript(scriptId);
                            break;

                        case "GetScript":
                            scriptId = await UsefulMethods.GetIdInConsoleAsync(fromSrc: true);
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
                            scriptId = await UsefulMethods.GetIdInConsoleAsync(fromSrc: true);
                            await facade.CompileScript(scriptId);    //should not compile if already present in db;
                            break;

                        case "CompileAllScripts":
                            await facade.CompileAllScripts();
                            break;

                        case "RecompileScript":
                            scriptId = await UsefulMethods.GetIdInConsoleAsync(fromSrc: true);
                            await facade.RecompileScript(scriptId);
                            break;

                        case "ValidateScript":
                            scriptId = await UsefulMethods.GetIdInConsoleAsync(fromSrc: true);
                            var script = await db.GetCustomerScript(scriptId);
                            string answer = await facade.ValidateScript(script.SourceCode);
                            Console.WriteLine(answer);
                            break;

                        case "GetCompilationErrors":    //todo test by turning off auto check when adding same with above
                            scriptId = await UsefulMethods.GetIdInConsoleAsync(fromSrc: true);
                            string compAnswer = await facade.GetCompilationErrors(scriptId);
                            Console.WriteLine(compAnswer);
                            break;

                        #endregion

                        #region Execution Operations

                        case "ExecuteActionScript":
                            scriptId = await UsefulMethods.GetIdInConsoleAsync(fromSrc: true);
                            ActionResult result = await facade.ExecuteActionScript(scriptId, UsefulMethods.GetTestingContext());
                            Console.WriteLine(result.ToString());   //you could do whatever with it
                            break;

                        case "ExecuteConditionScript":
                            scriptId = await UsefulMethods.GetIdInConsoleAsync(fromSrc: true);
                            bool resultCond = await facade.ExecuteConditionScript(scriptId, UsefulMethods.GetTestingContext());
                            Console.WriteLine(resultCond);
                            break;

                        case "ExecuteScriptById":
                            scriptId = await UsefulMethods.GetIdInConsoleAsync(fromSrc: true);
                            object resultObj = await facade.ExecuteScriptById(scriptId, UsefulMethods.GetTestingContext());

                            if (typeof(bool).IsAssignableFrom(resultObj.GetType()))       //good idea to check what type was returne, you could also just check the property from db normally
                            {
                                Console.WriteLine("Script returned a bool: " + resultObj.ToString());
                            }
                            else if (typeof(ActionResult).IsAssignableFrom(resultObj.GetType()))
                            {
                                Console.WriteLine("Script returned an ActionResult: " + resultObj.ToString());
                            }
                            else
                            {
                                Console.WriteLine("Script was neither returned a bool nor an ActionResult, it was a: " + resultObj.GetType().ToString());
                            }
                            break;

                        #endregion

                        #region Cache Management

                        case "GetCompiledCache":
                            scriptId = await UsefulMethods.GetIdInConsoleAsync();
                            byte[] arr = await facade.GetCompiledCache(scriptId, currentApiVersion);
                            Console.WriteLine(arr.ToString());
                            break;

                        case "ClearScriptCache":
                            scriptId = await UsefulMethods.GetIdInConsoleAsync(fromSrc: true);
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
                            scriptId = await UsefulMethods.GetIdInConsoleAsync(fromSrc: true);
                            int compa = await facade.GetScriptCompatibility(scriptId);
                            Console.WriteLine("Min api version: " + compa);
                            break;

                        case "CheckVersionCompatibility":
                            scriptId = await UsefulMethods.GetIdInConsoleAsync(fromSrc: true);
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
                            // await facade.HealthCheck();
                            break;

                        case "GetScriptMetadata":
                            scriptId = await UsefulMethods.GetIdInConsoleAsync(fromSrc: true);
                            string strr = await facade.GetScriptMetadata(scriptId);
                            Console.WriteLine(strr);
                            break;
                        case "GetUserName":
                            string strrr = facade.GetUserName();
                            Console.WriteLine(strrr);
                            break;

                        #endregion

                        default:
                            Console.WriteLine("Unknown command");
                            break;
                    }

                }
                catch (DbUpdateException e)
                {
                    Console.WriteLine("Database Error: " + e.InnerException?.Message);  //todo check what this really does and if superior to just tostring
                    Console.WriteLine("Database Error: " + e.ToString());
                }
                catch (NoFileWithThisClassNameFoundException e)
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
                catch (ConditionScriptExecutionException)
                {
                    Console.WriteLine("Something went wrong trying to execute your ConditionScript, but you can try again.");
                }
                catch (ActionScriptExecutionException)
                {
                    Console.WriteLine("Something went wrong trying to execute your ActionScript, but you can try again.");
                }
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
}

