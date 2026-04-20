#pragma warning disable CS0436


using Ember.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

public class TestHelper
{
    static string? sourceCodeActionV1;
    static string? sourceCodeActionV2;
    static string? sourceCodeActionV3;
    static string? sourceCodeVaccineAction;
    static string? sourceCodePedia;
    static List<string>? sourceCodes;
    static string? sourceCodeWhileTrue;
    static string? sourceCodeWhileTrueUnsafe;
    static string? sourceCodeIllegalUsings;
    static string? sourceCodeMissingUsing;
    static string? sourceCodePreventUsage;
    static string? sourceCodeMultipleClasses;
    public static TestHelperRecord GetSC(bool includeCondInList = true)
    {
        sourceCodeActionV1 = EmberMethods.CreateStringFromCsFile(
                   Path.GetFullPath(Path.Combine(
                       AppDomain.CurrentDomain.BaseDirectory,
                       "..", "..", "..", "..",
                       "sandbox", "Scripts", "ActionScripts", "AddPediatricTestsV1.cs"
                   ))
               );
        sourceCodeActionV2 = EmberMethods.CreateStringFromCsFile(
        Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "sandbox", "Scripts", "ActionScripts", "AddPediatricTestsV2.cs"
        ))
    );
        sourceCodeActionV3 = EmberMethods.CreateStringFromCsFile(
       Path.GetFullPath(Path.Combine(
           AppDomain.CurrentDomain.BaseDirectory,
           "..", "..", "..", "..",
           "sandbox", "Scripts", "ActionScripts", "AddPediatricTestsV3.cs"
       ))
   );
        sourceCodeVaccineAction = EmberMethods.CreateStringFromCsFile(
          Path.GetFullPath(Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory,
          "..", "..", "..", "..",
          "sandbox", "Scripts", "ActionScripts", "VaccineScript.cs"
      ))
      );
        sourceCodePedia = EmberMethods.CreateStringFromCsFile(
       Path.GetFullPath(Path.Combine(
           AppDomain.CurrentDomain.BaseDirectory,
                   "..", "..", "..", "..",
                   "sandbox", "Scripts", "ConditionScripts", "PediatricCondition.cs"
               ))
           );
        sourceCodeWhileTrue = EmberMethods.CreateStringFromCsFile(
      Path.GetFullPath(Path.Combine(
      AppDomain.CurrentDomain.BaseDirectory,
      "..", "..", "..", "..",
      "sandbox", "Scripts", "FaultyScripts", "WhileTrueScript.cs"
  ))
  );
        sourceCodeIllegalUsings = EmberMethods.CreateStringFromCsFile(
            Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "sandbox", "Scripts", "FaultyScripts", "IllegalUsingScript.cs"
        ))
        );
        sourceCodeMissingUsing = EmberMethods.CreateStringFromCsFile(
            Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "sandbox", "Scripts", "FaultyScripts", "MissingUsingScript.cs"
        ))
        );
        sourceCodePreventUsage = EmberMethods.CreateStringFromCsFile(
            Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "sandbox", "Scripts", "FaultyScripts", "PreventUsageScript.cs"
        ))
        );

        sourceCodeWhileTrueUnsafe = EmberMethods.CreateStringFromCsFile(
            Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "sandbox", "Scripts", "FaultyScripts", "WhileTrueUnchecked.cs"
        ))
        );
        sourceCodeMultipleClasses = EmberMethods.CreateStringFromCsFile(
                    Path.GetFullPath(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "..", "..", "..", "..",
                    "sandbox", "Scripts", "FaultyScripts", "MultipleClassesScript.cs"
                ))
                );
        string sourceCodeExcecutionTimeTest = EmberMethods.CreateStringFromCsFile(
                Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..",
                "sandbox", "Scripts", "ActionScripts", "ExecutionTimeTest.cs"
            ))
            );
        string sourceCodeMultiMethodScripts = EmberMethods.CreateStringFromCsFile(
            Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "sandbox", "Scripts", "ActionScripts", "MultipleMethodsScript.cs"
        ))
        );
        string undefinedMethodsScriptPublic = EmberMethods.CreateStringFromCsFile(
            Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "sandbox", "Scripts", "FaultyScripts", "UndefinedMethods", "PublicScript.cs"
        ))
        );
        string undefinedMethodsScriptPrivate = EmberMethods.CreateStringFromCsFile(
            Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "sandbox", "Scripts", "FaultyScripts", "UndefinedMethods", "PrivateScript.cs"
        ))
        );
        string undefinedMethodsScriptInternal = EmberMethods.CreateStringFromCsFile(
            Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "sandbox", "Scripts", "FaultyScripts", "UndefinedMethods", "InternalScript.cs"
        ))
        );
        string undefinedMethodsStatic = EmberMethods.CreateStringFromCsFile(
            Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "sandbox", "Scripts", "FaultyScripts", "UndefinedMethods", "StaticScript.cs"
        ))
        );
        sourceCodes = [];
        sourceCodes!.Add(sourceCodeActionV1);
        sourceCodes!.Add(sourceCodeActionV2);
        sourceCodes!.Add(sourceCodeActionV3);
        sourceCodes!.Add(sourceCodeVaccineAction);
        if (includeCondInList)
        {
            sourceCodes!.Add(sourceCodePedia);
        }

        return new TestHelperRecord
        {
            sourceCodeActionV1 = sourceCodeActionV1,
            sourceCodeActionV2 = sourceCodeActionV2,
            sourceCodeActionV3 = sourceCodeActionV3,
            sourceCodePedia = sourceCodePedia,
            sourceCodes = sourceCodes,
            sourceCodeVaccineAction = sourceCodeVaccineAction,
            sourceCodeWhileTrue = sourceCodeWhileTrue,
            sourceCodeWhileTrueUnsafe = sourceCodeWhileTrueUnsafe,
            sourceCodeIllegalUsings = sourceCodeIllegalUsings,
            sourceCodeMissingUsing = sourceCodeMissingUsing,
            sourceCodePreventUsage = sourceCodePreventUsage,
            sourceCodeMultipleClasses = sourceCodeMultipleClasses,
            sourceCodeExecutionTimeTest = sourceCodeExcecutionTimeTest,
            sourceCodeMultiMethodScripts = sourceCodeMultiMethodScripts,
            undefinedMethodsScriptPublic = undefinedMethodsScriptPublic,
            undefinedMethodsScriptInternal = undefinedMethodsScriptInternal,
            undefinedMethodsScriptPrivate = undefinedMethodsScriptPrivate,
            undefinedMethodsScriptStatic = undefinedMethodsStatic
        };
    }

    internal static ObjectsRecord ScriptObjects()
    {
        LabOrder labOrder = new LabOrder("1", "Pediatrics");
        Patient patient = new Patient("1", "TestFirst", "TestLast", new DateTime(2010, 6, 1, 7, 47, 0), "M");   //mfu
        ConsoleLogger logger = new ConsoleLogger();
        DataAccess testDataAccess = new DataAccess();
        Vaccine vaccine = new Vaccine("Polio", 1, DateTime.UtcNow);

        // return (labOrder, patient, logger, testDataAccess, vaccine);
        return new ObjectsRecord
        {
            labOrder = labOrder,
            logger = logger,
            patient = patient,
            testDataAccess = testDataAccess,
            vaccine = vaccine
        };
    }
    public static IScriptManagerExtended InitScriptManager()
    {
        IScriptManagerExtended scriptManager;
        ServiceCollection services = new ServiceCollection();

        LoggerForScripting logger = new LoggerForScripting();

        services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddSerilog(dispose: true);
        });
        services.AddSingleton<IUserSession, Sandbox.SandBoxUserSession>();

        services.AddDbContextFactory<ScriptDbContext>();
        ScriptingServiceCollectionExtensions.AddEmberScripting(services, EmberMethods.GetReferences(), EmberMethods.GetEmberApiVersion(), RecentTypeHelper.GetRecentTypes());

        var provider = services.BuildServiceProvider();

        return scriptManager = provider.GetRequiredService<IScriptManagerExtended>();
    }
    public static RecentContext GetContext()
    {
        LabOrder labOrder = new LabOrder("1", "Pediatrics");
        Vaccine vaccine = new Vaccine("Polio", 1, DateTime.UtcNow);

        ConsoleLogger logger = new ConsoleLogger();
        DataAccess testDataAccess = new DataAccess();



        var services = new ServiceCollection();

        Ember.Simulation.SandboxServiceCollectionExtensions.AddSandboxServices
        (services, logger, testDataAccess);

        using var provider = services.BuildServiceProvider();

        RecentContextFactory.IGeneratorContextFactory factory = provider.GetRequiredService<RecentContextFactory.IGeneratorContextFactory>();


        RecentContext ctx = factory.Create(labOrder, vaccine);

        return ctx;
    }

    public static string GetMethodNameAction()
    {
        return nameof(IScriptMethodsAction.ExecuteAsync);
    }
    public static string GetMethodNameCond()
    {
        return nameof(IScriptMethodsCondition.EvaluateAsync);
    }

}

public record TestHelperRecord
{
    public required string sourceCodePedia { get; init; }
    public required string sourceCodeActionV1 { get; init; }
    public required string sourceCodeActionV2 { get; init; }
    public required string sourceCodeActionV3 { get; init; }
    public required string sourceCodeVaccineAction { get; init; }
    public required List<string> sourceCodes { get; init; }
    public required string sourceCodeWhileTrue { get; init; }
    public required string sourceCodeWhileTrueUnsafe { get; init; }

    public required string sourceCodeIllegalUsings { get; init; }
    public required string sourceCodeMissingUsing { get; init; }
    public required string sourceCodePreventUsage { get; init; }
    public required string sourceCodeMultipleClasses { get; init; }
    public required string sourceCodeExecutionTimeTest { get; init; }
    public required string sourceCodeMultiMethodScripts { get; init; }
    public required string undefinedMethodsScriptPublic { get; init; }
    public required string undefinedMethodsScriptPrivate { get; init; }
    public required string undefinedMethodsScriptInternal { get; init; }
    public required string undefinedMethodsScriptStatic { get; init; }

}
internal record ObjectsRecord
{
    public required LabOrder labOrder { get; init; }
    public required Patient patient { get; init; }
    public required ConsoleLogger logger { get; init; }
    public required DataAccess testDataAccess { get; init; }
    public required Vaccine vaccine { get; init; }
}