using System.Globalization;
using System.Reflection;
using Ember.Scripting;
using Microsoft.CodeAnalysis;
public class RandomMethods
{
    private readonly ISccriptManagerDeleteAfter Facade;

    public RandomMethods(ISccriptManagerDeleteAfter facade)
    {
        Facade = facade;
    }
    public async Task<Dictionary<int, Guid>> ListAllCompiledFromDB()
    {
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName}.", nameof(ListAllCompiledFromDB), nameof(RandomMethods));
        // var db = new DbHelper(UsefulMethods.GetReferences());
        List<ScriptCompiledCache> caches = await Facade.GetAllCompiledScriptCaches();

        Dictionary<int, Guid> cacheDict = new Dictionary<int, Guid>();
        for (int i = 0; i < caches.Count(); i++)
        {
            Console.WriteLine(i + 1 + ". " + caches[i].ToString());
            cacheDict.Add(i + 1, caches[i].ScriptId);
        }
        if (cacheDict.Count() == 0)
        {
            Console.WriteLine("Cache Dictionary is Empty!");
        }
        return cacheDict;
    }

    public async Task<Dictionary<int, Guid>> ListAllStoredSourceCodes(bool dontPrint = false)
    {
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName}.", nameof(ListAllStoredSourceCodes), nameof(RandomMethods));
        // var db = new DbHelper(UsefulMethods.GetReferences());
        List<CustomerScript> sourceCodes = await Facade.ListScripts(includeCaches: true);

        Dictionary<int, Guid> sourceDict = new Dictionary<int, Guid>();
        for (int i = 0; i < sourceCodes.Count; i++)
        {
            // Console.WriteLine(i + 1 + ". " + sourceCodes[i].ToString());
            // Console.WriteLine(i + 1 + ". " + sourceCodes[i].ToStringShorter());
            string str = (i + 1).ToString() + ". Name: " + sourceCodes[i].ScriptName + ", Created by: " + sourceCodes[i].CreatedBy
            + ", Created at: " + sourceCodes[i].CreatedAt + ", MinApiVersion: " + sourceCodes[i].MinApiVersion + ", Modified at: " + sourceCodes[i].ModifiedAt
            + ", Compiled count [" + sourceCodes[i].CompiledCaches.Count() + "]";
            sourceDict.Add(i + 1, sourceCodes[i].Id);
            if (dontPrint == false) { Console.WriteLine(str); }
        }
        if (sourceDict.Count() == 0)
        {
            Console.WriteLine("Script Source Code repo is Empty!");
        }
        return sourceDict;
    }

    public async Task CompileAllScriptsInFolderAndSaveToDB(string folderPath, string userName, int currentApiVersion)
    {
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName}.", nameof(CompileAllScriptsInFolderAndSaveToDB), nameof(RandomMethods));

        string[] files = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);  //todo check for infinite loop  https://msdn.microsoft.com/en-us/library/ms143316(v=vs.110).aspx
        for (int i = 0; i < files.Length; i++)
        {

            try
            {
                // var db = new DbHelper(UsefulMethods.GetReferences());
                string scriptString = CreateStringFromCsFile(files[i]);
                Guid id = Guid.NewGuid();
                Guid randomTestScript2Id = await Facade.CreateScript(scriptString);
                // Console.WriteLine(randomTestScript2.ScriptName + "Added script N" + i + ". to both tables.");

            }
            catch (CompilationFailedException e)    //so if one fails not all get cancelled
            {
                Console.WriteLine(e.ToString());
            }
            catch (ValidationBeforeCompilationException e)
            {
                Console.WriteLine(e.ToString());
            }

        }
    }



    public async Task EditScriptInSwitch(Guid id, string userName, int currentApiVersion)
    {
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(EditScriptInSwitch), nameof(RandomMethods), id);
        // var db = new DbHelper(UsefulMethods.GetReferences());
        var customerScript = await Facade.GetScript(id);
        var creationDate = customerScript.CreatedAt;
        Console.WriteLine("Here is the old version of the script source code:");
        Console.WriteLine(customerScript.SourceCode);

        Console.WriteLine("Copy paste your new version file path now:");
        string userInput2 = Console.ReadLine()!;

        string str = CreateStringFromCsFile(userInput2!);
        await Facade.DeleteScript(id);

        //In reality it would be better like this but doesnt work because cant paste too much in console:
        // Console.WriteLine("Copy paste your new version now:");
        // string userInput2 = Console.ReadLine();

        await Facade.CreateScript(str, userName, createdAt: (DateTime)creationDate!); //todo unsafe af


    }
    public async Task GetSourceCodeInSwitch()
    {
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName}.", nameof(GetSourceCodeInSwitch), nameof(RandomMethods));
        Console.WriteLine("Enter the the script you want to read: ");
        string userInput = Console.ReadLine()!;
        // var db = new DbHelper(UsefulMethods.GetReferences());
        if (userInput == null || userInput == "")
        {
            // string userInput = Console.ReadLine();

            List<CustomerScript> customerScripts = await Facade.ListScripts();
            for (int i = 0; i < customerScripts.Count; i++)
            {
                Console.WriteLine(customerScripts[i]);
            }
        }
        else
        {
            var listAllCompiledFromDB = await ListAllCompiledFromDB();
            Guid idEdit = listAllCompiledFromDB[Int32.Parse(userInput)];
            CustomerScript scr = await Facade.GetScript(idEdit);
            Console.WriteLine(scr.SourceCode);
        }

    }
    public static ActionResultV3NoInheritance UpgradeActionResult(object resultValue)
    {
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName}.", nameof(UpgradeActionResult), nameof(RandomMethods));

        // var facade = new ScriptManagerFacade(UsefulMethods.GetReferences());
        // var newestVersion = await facade.GetRecentApiVersion();
        object finalActionResult = resultValue;
        int loopBreaker = 0;   //I am assuming not 1000 versions will be written                // will probably fail in real application todo fix mabe with reflection i heard?
        while (finalActionResult is not ActionResultV3NoInheritance && loopBreaker < 1000)    //could fail if loaded from diffrent assembly should probably replace the is statements with something like get type.name
        {
            loopBreaker++;
            // if (finalActionResult is ActionResultV2 v2Script)
            if (finalActionResult.GetType().Name == "ActionResultV2")
            {
                try
                {
                    ActionResultV2 v2Script2 = (ActionResultV2)finalActionResult;
                    finalActionResult = ActionResultV3NoInheritance.UpgradeV2(v2Script2);
                }
                catch (Exception e)
                {
                    Serilog.Log.Error(e.ToString());
                }
            }
            // else if (finalActionResult is ActionResult v1Script)
            else if (finalActionResult.GetType().Name == "ActionResult")
            {
                try
                {
                    ActionResult v1Script2 = (ActionResult)finalActionResult;
                    List<string> loggedActions = [];
                    finalActionResult = ActionResultV2.UpgradeV1(v1Script2, loggedActions);
                }
                catch (Exception e)
                {
                    Serilog.Log.Error(e.ToString());
                }
            }
        }
        // if (finalActionResult is ActionResultV3NoInheritance v3Script)
        if (finalActionResult.GetType().Name == "ActionResultV3NoInheritance")
        {
            ActionResultV3NoInheritance v3Script2 = (ActionResultV3NoInheritance)finalActionResult;
            return (ActionResultV3NoInheritance)v3Script2;
        }
        else
        {
            throw new Exception(message: "UpgradeActionResult in ScriptExecutor failed.");
        }
    }
    public async Task<Guid> GetIdInConsoleAsync(bool fromSrc = false)
    {
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName}.", nameof(GetIdInConsoleAsync), nameof(RandomMethods));
        Dictionary<int, Guid> cacheDict = [];
        if (fromSrc == false)
        { cacheDict = await ListAllCompiledFromDB(); }

        else { cacheDict = await ListAllStoredSourceCodes(); }

        Dictionary<int, Guid> sourceDict = [];
        Console.WriteLine("Enter the number of the script ");
        string userInputForEdit = Console.ReadLine()!;
        Guid id = cacheDict[Int32.Parse(userInputForEdit)];
        return id;
    }
    private static MetadataReference[] references = new MetadataReference[]
                 {
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // System.Private.CoreLib
                        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location), // System.Console
                        MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location), // System.Runtime
                        MetadataReference.CreateFromFile(typeof(Task<>).Assembly.Location), // System.Threading.Tasks
                        MetadataReference.CreateFromFile(typeof(DateTime).Assembly.Location), // System.DateTime
                        // References t custom interfaces
                        MetadataReference.CreateFromFile(typeof(IGeneratorConditionScript).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(IGeneratorReadOnlyContext).Assembly.Location)   //try removing if works good i guess but still need to pass from sandbox
                 };
    public static MetadataReference[] GetReferences()
    {
        return references;
    }
    public static int GetRecentApiVersion()
    {
        return 6;
    }
    public static string GetUserName()
    {
        // GeneratorContext ctx = GetTestingContext();
        return "Gilles";
    }
    public GeneratorContext GetTestingContext<T>(CustomerScript? justForTesting = null) where T : GeneratorContext
    {
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName}.", nameof(GetTestingContext), nameof(RandomMethods));
        try
        {
            LabOrder labOrder = new LabOrder("1", "Pediatrics");
            Patient patient = new Patient("1", "TestFirst", "TestLast", new DateTime(2010, 6, 1, 7, 47, 0), "M");   //mfu
            ConsoleLogger logger = new ConsoleLogger();
            DataAccess testDataAccess = new DataAccess();
            Vaccine vaccine = new Vaccine("Polio", 1, DateTime.UtcNow);

            GeneratorContext ctx = typeof(T) switch
            {
                var t when t == typeof(ReadOnlyContext) => new ReadOnlyContext(labOrder, patient, logger, testDataAccess),
                var t when t == typeof(RWContext) => new RWContext(labOrder, patient, logger, testDataAccess),
                var t when t == typeof(GeneratorContextV2) => new GeneratorContextV2(labOrder, patient, logger, testDataAccess),
                var t when t == typeof(GeneratorContextV3) => new GeneratorContextV3(labOrder, patient, logger, testDataAccess),
                var t when t == typeof(GeneratorContextNoInherVaccine) => new GeneratorContextNoInherVaccine(labOrder, vaccine),
                _ => throw new ArgumentException($"Unsupported context type: {typeof(T).Name}")
            };
            if (justForTesting != null) //this is ofc just for testing purposes in the real application you would never automatically distribute the context because it is unsafe you want to be able to control who gets which context precisely
            {
                var microsoftLogger = new LoggerForScripting().GetMicrosoftLogger<ScriptManagerFacade>();
                var refs = GetReferences();
                // ScriptCompiler compiler = new ScriptCompiler(refs, new LoggerForScripting().GetMicrosoftLogger<ScriptCompiler>());
                string implementedInterface = Facade.BasicValidationBeforeCompiling(justForTesting.SourceCode!).baseTypeName;
                switch (implementedInterface)
                {
                    case "IGeneratorActionScript":
                        // ctx = new RWContext(labOrder, patient, logger, testDataAccess);
                        int v = Facade.BasicValidationBeforeCompiling(justForTesting.SourceCode!).versionInt;
                        ctx = v switch
                        {
                            1 => new RWContext(labOrder, patient, logger, testDataAccess),
                            2 => new GeneratorContextV2(labOrder, patient, logger, testDataAccess),
                            3 => new GeneratorContextV3(labOrder, patient, logger, testDataAccess),
                            4 => new GeneratorContextNoInherVaccine(labOrder, vaccine),
                            _ => throw new NotImplementedException(),
                        };
                        break;
                    case "IGeneratorActionScriptV2":
                        ctx = new GeneratorContextV2(labOrder, patient, logger, testDataAccess);
                        break;
                    case "IGeneratorActionScriptV3":
                        ctx = new GeneratorContextV3(labOrder, patient, logger, testDataAccess);
                        break;
                    case "IGeneratorActionScriptV4Vaccine":
                        ctx = new GeneratorContextNoInherVaccine(labOrder, vaccine);
                        break;
                    case "IGeneratorConditionScript":
                        ctx = new ReadOnlyContext(labOrder, patient, logger, testDataAccess);
                        break;
                    default:
                        Console.WriteLine("Error in testing switch");
                        throw new Exception();
                }
            }

            // return (T)ctx;
            return ctx;
        }
        catch (Exception e)
        {
            Console.WriteLine("GetTestingContext failed");
            Console.WriteLine(e.ToString());
            throw new Exception();
        }

    }


    public string CreateStringFromCsFile(string scriptPath)
    {
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName} with path: {ScriptPath}.", nameof(CreateStringFromCsFile), nameof(RandomMethods), scriptPath);
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
    public string GetScriptPathFromFolder(string scriptFolderPath, string classOfScriptToExecute)
    {
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName}.", nameof(GetScriptPathFromFolder), nameof(RandomMethods));
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
    // public static async Task<Guid> GetIdInConsoleAsync(bool fromSrc = false)
    // {
    //     Dictionary<int, Guid> cacheDict = [];
    //     if (fromSrc == false)
    //     { cacheDict = await RandomMethods.ListAllCompiledFromDB(); }

    //     else { cacheDict = await RandomMethods.ListAllStoredSourceCodes(); }

    //     Dictionary<int, Guid> sourceDict = [];
    //     Console.WriteLine("Enter the number of the script ");
    //     string userInputForEdit = Console.ReadLine();
    //     Guid id = cacheDict[Int32.Parse(userInputForEdit)];
    //     return id;
    // }

}
