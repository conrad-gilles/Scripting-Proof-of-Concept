using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Ember.Scripting;
using Microsoft.CodeAnalysis;
using System.Reflection;

public class UsefulMethods
{
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
    public static GeneratorContext GetTestingContext<T>(CustomerScript? justForTesting = null) where T : GeneratorContext
    {
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
                ScriptCompiler compiler = new ScriptCompiler(refs, microsoftLogger);
                string implementedInterface = compiler.BasicValidationBeforeCompiling(justForTesting.SourceCode).baseTypeName;
                switch (implementedInterface)
                {
                    case "IGeneratorActionScript":
                        // ctx = new RWContext(labOrder, patient, logger, testDataAccess);
                        int v = compiler.BasicValidationBeforeCompiling(justForTesting.SourceCode).versionInt;
                        ctx = v switch
                        {
                            1 => new RWContext(labOrder, patient, logger, testDataAccess),
                            2 => new GeneratorContextV2(labOrder, patient, logger, testDataAccess),
                            3 => new GeneratorContextV3(labOrder, patient, logger, testDataAccess),
                            4 => new GeneratorContextNoInherVaccine(labOrder, vaccine),
                            // _ => new RWContext(labOrder, patient, logger, testDataAccess)
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
