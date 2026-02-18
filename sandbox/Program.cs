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
    // private static string scriptFolderPath = @"C:\Users\Gilles\Desktop\UNI\Semester 6\Testing\testing2\labsolutionlu-ember-scripting-fb966c220f60\sandbox\src\Scripts";
    private static string scriptFolderPath = @"C:\Users\Gilles\Desktop\ScriptsForProject\V2";    //out of scope to test errors
    private static string userName = "Gilles";

    private static int currentApiVersion = 6;
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
            // Dictionary<int, Guid> sourceDict = await RandomMethods.ListAllStoredSourceCodes();
            // Dictionary<int, Guid> sourceDict = [];

            await db.AutomaticCompilationOnVersionUpdate(currentApiVersion);    //todo make this "background job that is run automatically periodically"

            bool running = true;
            while (running)
            {
                try
                {
                    await UsefulMethods.FacadeTestingSwitch(currentApiVersion, userName, scriptFolderPath);
                    break;

                    // Console.WriteLine("Enter the id of the script you want to run: ");
                    // string userInput = Console.ReadLine();

                    // switch (userInput)
                    // {
                    //     case null:
                    //     case "":
                    //     case " ":
                    //         Console.WriteLine("You entered nothing try again.");
                    //         break;
                    //     case "exit":
                    //         running = false;
                    //         break;
                    //     case "source":

                    //         await RandomMethods.GetSourceCodeInSwitch();
                    //         break;
                    //     case "reset":
                    //         await new DbHelper().EnsureDeletedCreated();
                    //         cacheDict = await RandomMethods.ListAllCompiledFromDB();
                    //         break;
                    //     case "edit":
                    //         Console.WriteLine("Enter the id of the script you want to edit: ");
                    //         string userInputForEdit = Console.ReadLine();
                    //         Guid idEdit = cacheDict[Int32.Parse(userInputForEdit)];
                    //         await RandomMethods.EditScriptInSwitch(idEdit, userName, currentApiVersion);

                    //         break;
                    //     case "comp":    //doesnt compile if isDuplicate is eneblaed bc scripts are already in db so no cache is created maybe todo
                    //         await RandomMethods.CompileAllScriptsInFolderAndSaveToDB(scriptFolderPath, userName, currentApiVersion);   //main precompilation act
                    //         cacheDict = await RandomMethods.ListAllCompiledFromDB();
                    //         break;
                    //     case "comp source":
                    //         await new DbHelper().CompileAllStoredScripts(currentApiVersion);
                    //         break;
                    //     case "ls":
                    //         cacheDict = await RandomMethods.ListAllCompiledFromDB();
                    //         break;
                    //     case "ls source":
                    //         sourceDict = await RandomMethods.ListAllStoredSourceCodes();
                    //         break;
                    //     case "dupes":
                    //         await new DbHelper().RemoveDuplicates(currentApiVersion);
                    //         break;
                    //     case "deleteCache":
                    //         await new DbHelper().DeleteAllCachedScripts();
                    //         break;
                    //     case "facade":
                    //         await UsefulMethods.FacadeTestingSwitch(currentApiVersion, userName, scriptFolderPath);
                    //         running = false;
                    //         break;
                    //     default:
                    //         if (userInput.Length > 5)
                    //         {
                    //             var compiledScript = await db.GetCompiledScripCache(Guid.Parse(userInput), currentApiVersion);
                    //             new ScriptExecutor(compiledScript.AssemblyBytes, UsefulMethods.GetTestingContext()).RunScriptExecution<object>();
                    //         }
                    //         else
                    //         {
                    //             Guid id = cacheDict[Int32.Parse(userInput)];
                    //             var compiledScript = await db.GetCompiledScripCache(id, currentApiVersion);
                    //             new ScriptExecutor(compiledScript.AssemblyBytes, UsefulMethods.GetTestingContext()).RunScriptExecution<object>();
                    //         }
                    //         break;
                    // }
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

