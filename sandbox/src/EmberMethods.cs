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

public class EmberMethods
{
    private readonly ISccriptManagerDeleteAfter Facade;

    public EmberMethods(ISccriptManagerDeleteAfter facade)
    {
        Facade = facade;
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

    public static ISccriptManagerDeleteAfter GetNewScriptManagerInstance(int? apiVersion = null)
    {
        ISccriptManagerDeleteAfter facade;
        ServiceCollection services2 = new ServiceCollection();

        LoggerForScripting logger = new LoggerForScripting();
        // Log.Logger = logger.SetUpAndGetSeriLogger(); // ADD THIS LINE
        Log.Debug("New instance.");

        services2 = new ServiceCollection();
        services2.AddLogging(builder =>
        {
            builder.AddSerilog(dispose: true);
        });

        ScriptingServiceCollectionExtensions.AddEmberScripting(services2, EmberMethods.GetReferences(), EmberMethods.GetEmberApiVersion(testingDiffrentVersion: apiVersion));

        using var provider2 = services2.BuildServiceProvider();

        return facade = provider2.GetRequiredService<ISccriptManagerDeleteAfter>();
    }
    public async Task<Dictionary<int, Guid>> ListAllCompiledFromDB()
    {
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName}.", nameof(ListAllCompiledFromDB), nameof(EmberMethods));
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
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName}.", nameof(ListAllStoredSourceCodes), nameof(EmberMethods));
        // var db = new DbHelper(UsefulMethods.GetReferences());
        List<CustomerScript> sourceCodes = await Facade.ListScripts(includeCaches: true);

        Dictionary<int, Guid> sourceDict = new Dictionary<int, Guid>();
        for (int i = 0; i < sourceCodes.Count; i++)
        {
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
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName}.", nameof(CompileAllScriptsInFolderAndSaveToDB), nameof(EmberMethods));

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
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(EditScriptInSwitch), nameof(EmberMethods), id);
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
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName}.", nameof(GetSourceCodeInSwitch), nameof(EmberMethods));
        Console.WriteLine("Enter the the script you want to read: ");
        string userInput = Console.ReadLine()!;
        if (userInput == null || userInput == "")
        {

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
    public static ActionResultSF UpgradeActionResult(object resultValue)   //todo change to base class return and cast in tests and so on
    {
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName}.", nameof(UpgradeActionResult), nameof(EmberMethods));

        ActionResultSF currentActionResult = (ActionResultSF)resultValue;

        TypeInfo typeInfo = currentActionResult.GetType().GetTypeInfo();
        var attrs = typeInfo.GetCustomAttributes();
        var metaDataAttribute = typeInfo.GetCustomAttribute<MetaDataActionResult>();

        if (metaDataAttribute == null)
        {
            throw new Exception(message: "MetadataAttribute was null why i cant tell you.");
        }

        int metaDatatVersionAtt = metaDataAttribute.Version;

        int iterations = 0;
        var dict = ActionResultVersionScanner.GetClassDictionary();
        int maxIterations = dict.Keys.Count() + 1;

        Console.WriteLine("max iterations: " + maxIterations);

        Type maxVersionType = dict[dict.Keys.Max()];

        while (currentActionResult.GetType() != maxVersionType && iterations <= maxIterations)
        {
            metaDatatVersionAtt++;
            var nextVersionType = dict[metaDatatVersionAtt];

            var uninitializedNextVersion =
            (ActionResultSF)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(nextVersionType);

            try
            {
                currentActionResult = uninitializedNextVersion.Upgrade(currentActionResult);
            }
            catch (TargetInvocationException ex)
            {
                throw new Exception($"Failed to upgrade {maxVersionType.Name} to {nextVersionType.Name}.", ex.InnerException);
            }

            if (iterations > maxIterations)
            {
                throw new Exception("Somethign went wrong trying to upgrade the ActionResult");
            }
            iterations++;
        }
        return currentActionResult;
    }
    public async Task<Guid> GetIdInConsoleAsync(bool fromSrc = false)
    {
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName}.", nameof(GetIdInConsoleAsync), nameof(EmberMethods));
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
    private static List<MetadataReference> references = [
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // System.Private.CoreLib
                        MetadataReference.CreateFromFile(typeof(IGeneratorConditionScript).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IGeneratorReadOnlyContextV1.IGeneratorContext).Assembly.Location),//try removing if works good i guess but still need to pass from sandbox];
                          ];
    public static List<MetadataReference> GetReferences()
    {
        return references;
    }

    public static string GetUserName()
    {
        // GeneratorContext ctx = GetTestingContext();
        return "Gilles";
    }
    public async Task<Ember.Scripting.GeneratorContextSF> GetTestingContext<T>(CustomerScript? autoDetectFromScript = null) where T : Ember.Scripting.GeneratorContextSF
    {
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName}.", nameof(GetTestingContext), nameof(EmberMethods));
        try
        {
            ContextFactory sf = new ContextFactory(Facade);
            Ember.Scripting.GeneratorContextSF ctx;
            var objs = sf.ScriptObjects();
            if (autoDetectFromScript == null)
            {
                var dict = ContextVersionScanner.GetClassDictionary();
                int? v = null;

                foreach (KeyValuePair<int, Type> kvp in dict)
                {
                    if (kvp.Value == typeof(T))
                    {
                        if (v != null)
                        {
                            throw new Exception(message: "version int should be null here else it was assigned twice which is weird.");
                        }
                        v = kvp.Key;
                    }
                }
                if (v == null)
                {
                    throw new Exception(message: "Version int was null, this should not happen.");
                }
                ctx = sf.CreateContextForApiV(GetTestData(), v);
            }
            else
            {

                int v = Facade.BasicValidationBeforeCompiling(autoDetectFromScript.SourceCode!).versionInt;
                ctx = sf.CreateContextForApiV(GetTestData(), v);

            }
            return ctx;
        }
        catch (Exception e)
        {
            Console.WriteLine("GetTestingContext failed");
            Console.WriteLine(e.ToString());
            throw new Exception();
        }

    }


    public static string CreateStringFromCsFile(string scriptPath)
    {
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName} with path: {ScriptPath}.", nameof(CreateStringFromCsFile), nameof(EmberMethods), scriptPath);
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
            throw new CreateStringFromCsFileException("The file could not be read: probably because typo in classOfScriptToExecute variable:", e);    // todo correct error handling throw an error
        }
        // Source https://www.tutorialspoint.com/chash-program-to-create-string-from-contents-of-a-file //obv i changed a lot but i still copied
    }
    public string GetScriptPathFromFolder(string scriptFolderPath, string classOfScriptToExecute)
    {
        Serilog.Log.Verbose("Entered {MethodName} in {ClassName}.", nameof(GetScriptPathFromFolder), nameof(EmberMethods));
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
                throw new NoFileWithThisClassNameFoundException("GetScriptPathFromFolder failed no scriptPath in If statement");
            }
            return scriptPath;  //todo this method cant fail because scriptPath is always atleast "" and therefore if wrong name is inserted it will still return even if no file was found
        }
        catch (Exception e)
        {
            Console.WriteLine("getScriptPathFromFolder method failed");
            Console.WriteLine(e.ToString());
            throw new GetScriptPathFromFolderException("getScriptPathFromFolder method failed", e);
        }

    }
    internal (LabOrder labOrder, Patient patient, ConsoleLogger logger, DataAccess testDataAccess,
    Vaccine vaccine) ScriptObjects()
    {
        LabOrder labOrder = new LabOrder("1", "Pediatrics");
        Patient patient = new Patient("1", "TestFirst", "TestLast", new DateTime(2010, 6, 1, 7, 47, 0), "M");   //mfu
        ConsoleLogger logger = new ConsoleLogger();
        DataAccess testDataAccess = new DataAccess();
        Vaccine vaccine = new Vaccine("Polio", 1, DateTime.UtcNow);

        return (labOrder, patient, logger, testDataAccess, vaccine);
    }

    internal ActiveDataClass GetTestData()
    {
        var obj = ScriptObjects();
        return new ActiveDataClass(labOrder: obj.labOrder, patient: obj.patient, consoleLogger: obj.logger,
          dataAccess: obj.testDataAccess, vaccine: obj.vaccine);
    }
}
