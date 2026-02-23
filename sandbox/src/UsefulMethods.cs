using System.Threading.Tasks;
using Microsoft.VisualBasic;

public class UsefulMethods
{
    public static int GetRecentApiVersion()
    {
        return 6;
    }
    public static string GetUserName()
    {
        // GeneratorContext ctx = GetTestingContext();
        return "Gilles";
    }
    public static GeneratorContext GetTestingContext<T>() where T : GeneratorContext
    {
        LabOrder labOrder = new LabOrder("1", "Pediatrics");
        Patient patient = new Patient("1", "TestFirst", "TestLast", new DateTime(2010, 6, 1, 7, 47, 0), "M");   //mfu
        ConsoleLogger logger = new ConsoleLogger();
        DataAccess testDataAccess = new DataAccess();

        GeneratorContext ctx = typeof(T) switch
        {
            var t when t == typeof(ReadOnlyContext) => new ReadOnlyContext(labOrder, patient, logger, testDataAccess),
            var t when t == typeof(RWContext) => new RWContext(labOrder, patient, logger, testDataAccess),
            var t when t == typeof(GeneratorContextV2) => new GeneratorContextV2(labOrder, patient, logger, testDataAccess),
            var t when t == typeof(GeneratorContextV3) => new GeneratorContextV3(labOrder, patient, logger, testDataAccess),
            _ => throw new ArgumentException($"Unsupported context type: {typeof(T).Name}")
        };

        return (T)ctx;
    }


    public static string CreateStringFromCsFile(string scriptPath)
    {
        try
        {
            // Display the lines that are read from the file
            string text = File.ReadAllText(scriptPath);
            // Console.WriteLine(text); //cool for debugging
            return text;
        }
        catch (Exception e)
        {
            // Displays the error on the screen.
            Console.WriteLine(e.ToString());
            Console.WriteLine("The file could not be read: probably because typo in classOfScriptToExecute variable:");
            throw new CreateStringFromCsFileException();    // todo correct error handling throw an error
        }
        // Source https://www.tutorialspoint.com/chash-program-to-create-string-from-contents-of-a-file //obv i changed a lot but i still copied
    }
    public static string GetScriptPathFromFolder(string scriptFolderPath, string classOfScriptToExecute)
    {
        try
        {
            string[] files =
                 Directory.GetFiles(scriptFolderPath, "*.cs", SearchOption.AllDirectories);  //todo check for infinite loop  https://msdn.microsoft.com/en-us/library/ms143316(v=vs.110).aspx
            string scriptPath = "";
            for (int i = 0; i < files.Length; i++)
            {
                int start = files[i].Length - 1 - classOfScriptToExecute.Length - 3;    //minus 3 for the.cs extension
                int lenght = classOfScriptToExecute.Length + 1;                     //plus 3 for extension might be unnsessecary
                string subString = files[i].Substring(start, lenght);

                if (subString.Contains(classOfScriptToExecute))
                {
                    scriptPath = files[i];
                    break;  //probably unsafe but good for efficiency? todo
                }
            }
            if (scriptPath == "" || scriptPath == null)
            {
                throw new NoFileWithThisClassNameFoundException();
            }
            return scriptPath;  //todo this method cant fail because scriptPath is always atleast "" and therefore if wrong name is inserted it will still return even if no file was found
        }
        catch (Exception e)
        {
            Console.WriteLine("getScriptPathFromFolder method failed");
            Console.WriteLine(e.ToString());
            throw new GetScriptPathFromFolderException();
        }

    }
    public static async Task<Guid> GetIdInConsoleAsync(bool fromSrc = false)
    {
        Dictionary<int, Guid> cacheDict = [];
        if (fromSrc == false)
        { cacheDict = await RandomMethods.ListAllCompiledFromDB(); }

        else { cacheDict = await RandomMethods.ListAllStoredSourceCodes(); }

        Dictionary<int, Guid> sourceDict = [];
        Console.WriteLine("Enter the number of the script ");
        string userInputForEdit = Console.ReadLine();
        Guid id = cacheDict[Int32.Parse(userInputForEdit)];
        return id;
    }

    // public static async Task FacadeTestingSwitch(int currentApiVersion, string userName, string scriptFolderPath)
    // {

    //     var db = new DbHelper();

    //     // await RandomMethods.CompileAllScriptsInFolderAndSaveToDB(scriptFolderPath);   //main precompilation act
    //     // Dictionary<int, Guid> cacheDict = await RandomMethods.ListAllCompiledFromDB();
    //     Dictionary<int, Guid> sourceDict = await RandomMethods.ListAllStoredSourceCodes();
    //     // Dictionary<int, Guid> sourceDict = [];
    //     Dictionary<int, Guid> cacheDict = [];
    //     Guid scriptId = new Guid();

    //     await db.AutomaticCompilationOnVersionUpdate(currentApiVersion);    //todo make this "background job that is run automatically periodically"

    //     bool running = true;
    //     ScriptManagerFacade facade = new ScriptManagerFacade();
    //     while (running)
    //     {
    //         try
    //         {
    //             Console.WriteLine("Enter the function you want to run: ");
    //             string userInput = Console.ReadLine();
    //             // Switch statement to test ScriptManagerFacade functions
    //             switch (userInput)
    //             {
    //                 case null:
    //                 case "":
    //                 case " ":
    //                     Console.WriteLine("You entered nothing try again.");
    //                     break;
    //                 case "exit":
    //                     running = false;
    //                     break;
    //                 case "source":

    //                     await RandomMethods.GetSourceCodeInSwitch();
    //                     break;
    //                 case "reset":
    //                     await new DbHelper().EnsureDeletedCreated();
    //                     // cacheDict = 
    //                     await RandomMethods.ListAllCompiledFromDB();
    //                     break;
    //                 case "comp":    //doesnt compile if isDuplicate is eneblaed bc scripts are already in db so no cache is created maybe todo
    //                     await RandomMethods.CompileAllScriptsInFolderAndSaveToDB(scriptFolderPath, userName, currentApiVersion);   //main precompilation act
    //                     // cacheDict = 
    //                     await RandomMethods.ListAllCompiledFromDB();
    //                     break;
    //                 case "comp source":
    //                     await new DbHelper().CompileAllStoredScripts(currentApiVersion);
    //                     break;
    //                 case "comp mv":
    //                     scriptId = await GetIdInConsoleAsync(fromSrc: true);
    //                     for (int i = 0; i < 5; i++)
    //                     {
    //                         await facade.CompileScript(scriptId, i);
    //                     }
    //                     break;
    //                 case "ls":
    //                     cacheDict = await RandomMethods.ListAllCompiledFromDB();
    //                     break;
    //                 case "ls source":
    //                     sourceDict = await RandomMethods.ListAllStoredSourceCodes();
    //                     break;
    //                 case "dupes":
    //                     await new DbHelper().RemoveDuplicates();
    //                     break;
    //                 case "deleteCache":
    //                     await new DbHelper().DeleteAllCachedScripts();
    //                     break;

    //                 #region Script Lifecycle

    //                 case "CreateScript":
    //                     string sourceCode = CreateStringFromCsFile(@"C:\Users\Gilles\Desktop\UNI\Semester 6\Testing\testing2\labsolutionlu-ember-scripting-fb966c220f60\sandbox\src\Scripts\ConditionScripts\PediatricCondition.cs");
    //                     await facade.CreateScript(sourceCode, "ConditionScriptTest", "Gilles");
    //                     break;

    //                 case "UpdateScript":
    //                     Guid idEdit = await GetIdInConsoleAsync(fromSrc: true);

    //                     var customerScript = await db.GetCustomerScript(idEdit);
    //                     var creationDate = customerScript.CreatedAt;

    //                     Console.WriteLine("Copy paste your new version file path now:");
    //                     string userInput2 = Console.ReadLine();

    //                     string str = UsefulMethods.CreateStringFromCsFile(userInput2);

    //                     //In reality it would be better like this but doesnt work because cant paste too much in console:
    //                     // Console.WriteLine("Copy paste your new version now:");
    //                     // string userInput2 = Console.ReadLine();

    //                     await facade.UpdateScript(idEdit, str, userName);
    //                     break;

    //                 case "DeleteScript":
    //                     scriptId = await GetIdInConsoleAsync(fromSrc: true);
    //                     await facade.DeleteScript(scriptId);
    //                     break;

    //                 case "GetScript":
    //                     scriptId = await GetIdInConsoleAsync(fromSrc: true);
    //                     CustomerScript s = await facade.GetScript(scriptId);
    //                     Console.WriteLine(s.ToString());
    //                     break;

    //                 case "ListScripts":
    //                     var list = await facade.ListScripts();
    //                     foreach (var item in list)
    //                     {
    //                         Console.WriteLine(item.ToStringShorter());
    //                     }
    //                     break;

    //                 #endregion

    //                 #region Compilation Operations

    //                 case "CompileScript":   //todo check
    //                     scriptId = await GetIdInConsoleAsync(fromSrc: true);
    //                     await facade.CompileScript(scriptId, currentApiVersion);    //should not compile if already present in db;
    //                     break;

    //                 case "CompileAllScripts":
    //                     await facade.CompileAllScripts();
    //                     break;

    //                 case "RecompileScript":
    //                     scriptId = await GetIdInConsoleAsync(fromSrc: true);
    //                     await facade.RecompileScript(scriptId);
    //                     break;

    //                 case "ValidateScript":
    //                     scriptId = await GetIdInConsoleAsync(fromSrc: true);
    //                     var script = await db.GetCustomerScript(scriptId);
    //                     string answer = await facade.ValidateScript(script.SourceCode);
    //                     Console.WriteLine(answer);
    //                     break;

    //                 case "GetCompilationErrors":    //todo test by turning off auto check when adding same with above
    //                     scriptId = await GetIdInConsoleAsync(fromSrc: true);
    //                     string compAnswer = await facade.GetCompilationErrors(scriptId, currentApiVersion);
    //                     Console.WriteLine(compAnswer);
    //                     break;

    //                 #endregion

    //                 #region Execution Operations

    //                 case "ExecuteActionScript":
    //                     scriptId = await GetIdInConsoleAsync(fromSrc: true);
    //                     ActionResult result = await facade.ExecuteActionScript(scriptId, GetTestingContext());
    //                     Console.WriteLine(result.ToString());   //you could do whatever with it
    //                     break;

    //                 case "ExecuteConditionScript":
    //                     scriptId = await GetIdInConsoleAsync(fromSrc: true);
    //                     bool resultCond = await facade.ExecuteConditionScript(scriptId, GetTestingContext());
    //                     Console.WriteLine(resultCond);
    //                     break;

    //                 case "ExecuteScriptById":
    //                     scriptId = await GetIdInConsoleAsync(fromSrc: true);
    //                     object resultObj = await facade.ExecuteScriptById(scriptId, GetTestingContext());

    //                     if (typeof(bool).IsAssignableFrom(resultObj.GetType()))       //good idea to check what type was returne, you could also just check the property from db normally
    //                     {
    //                         Console.WriteLine("Script returned a bool: " + resultObj.ToString());
    //                     }
    //                     else if (typeof(ActionResult).IsAssignableFrom(resultObj.GetType()))
    //                     {
    //                         Console.WriteLine("Script returned an ActionResult: " + resultObj.ToString());
    //                     }
    //                     else
    //                     {
    //                         Console.WriteLine("Script was neither returned a bool nor an ActionResult, it was a: " + resultObj.GetType().ToString());
    //                     }
    //                     break;

    //                 #endregion

    //                 #region Cache Management

    //                 case "GetCompiledCache":
    //                     scriptId = await GetIdInConsoleAsync();
    //                     byte[] arr = await facade.GetCompiledCache(scriptId, currentApiVersion);
    //                     Console.WriteLine(arr.ToString());
    //                     break;

    //                 case "ClearScriptCache":
    //                     scriptId = await GetIdInConsoleAsync(fromSrc: true);
    //                     await facade.ClearScriptCache(scriptId);
    //                     break;

    //                 case "ClearAllCaches":
    //                     await facade.ClearAllCaches();
    //                     break;

    //                 case "PrecompileForApiVersion":
    //                     await facade.PrecompileForApiVersion();
    //                     break;

    //                 #endregion

    //                 #region Version Management

    //                 case "GetActiveApiVersions":
    //                     var ls = await facade.GetActiveApiVersions();    //todo
    //                     string vs = "";
    //                     foreach (var item in ls)
    //                     {
    //                         vs = vs + " " + item;
    //                     }
    //                     Console.WriteLine("Active Api versions: " + vs);
    //                     break;

    //                 case "GetScriptCompatibility":
    //                     scriptId = await GetIdInConsoleAsync(fromSrc: true);
    //                     int compa = await facade.GetScriptCompatibility(scriptId);
    //                     Console.WriteLine("Min api version: " + compa);
    //                     break;

    //                 case "CheckVersionCompatibility":
    //                     scriptId = await GetIdInConsoleAsync(fromSrc: true);
    //                     bool check = await facade.CheckVersionCompatibility(scriptId, currentApiVersion);
    //                     Console.WriteLine("CheckVersionCompatibility with ApiV: " + currentApiVersion + " is " + check);
    //                     break;

    //                 case "RegisterEmberInstance":
    //                     // await facade.RegisterEmberInstance(instanceId, emberVersion, apiVersion);
    //                     break;

    //                 #endregion

    //                 #region Duplicate Detection & Cleanup

    //                 case "DetectDuplicates":
    //                     var dupes = await facade.DetectDuplicates();
    //                     List<Guid> scriptGUIDs = dupes.scriptGUIDs;
    //                     Dictionary<Guid, int> cacheGUIDs = dupes.cacheGUIDs;
    //                     string rtrnStr1 = "Script Guids to remove: ";
    //                     foreach (var item in scriptGUIDs)
    //                     {
    //                         rtrnStr1 = rtrnStr1 + " , " + item;
    //                     }
    //                     string rtrnStr2 = "Cache Guids to remove: ";
    //                     foreach (var item in cacheGUIDs)
    //                     {
    //                         rtrnStr2 = rtrnStr2 + " , " + item;
    //                     }
    //                     Console.WriteLine(rtrnStr1);
    //                     Console.WriteLine(rtrnStr2);
    //                     break;

    //                 case "RemoveDuplicates":
    //                     await facade.RemoveDuplicates();
    //                     break;

    //                 case "CleanupOrphanedCaches":
    //                     await facade.CleanupOrphanedCaches();
    //                     break;

    //                 #endregion

    //                 #region Monitoring & Diagnostics

    //                 case "GetScriptExecutionHistory":
    //                     // await facade.GetScriptExecutionHistory(scriptId);
    //                     break;

    //                 case "GetCompilationStatistics":
    //                     // await facade.GetCompilationStatistics();
    //                     break;

    //                 case "HealthCheck":
    //                     // await facade.HealthCheck();
    //                     break;

    //                 case "GetScriptMetadata":
    //                     // await facade.GetScriptMetadata(scriptId);
    //                     break;

    //                 #endregion

    //                 default:
    //                     Console.WriteLine("Unknown command");
    //                     break;
    //             }

    //         }

    //         catch (Exception e)
    //         {
    //             Console.WriteLine(e.Message);
    //         }
    //     }

    // }
}