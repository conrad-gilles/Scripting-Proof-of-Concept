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

    public static TestHelperRecord GetSC(bool includeCondInList = true)
    {
        sourceCodeActionV1 = EmberMethods.CreateStringFromCsFile(
                   Path.GetFullPath(Path.Combine(
                       AppDomain.CurrentDomain.BaseDirectory,
                       "..", "..", "..", "..",
                       "sandbox", "src", "Scripts", "ActionScripts", "AddPediatricTestsV1.cs"
                   ))
               );
        sourceCodeActionV2 = EmberMethods.CreateStringFromCsFile(
        Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "sandbox", "src", "Scripts", "ActionScripts", "AddPediatricTestsV2.cs"
        ))
    );
        sourceCodeActionV3 = EmberMethods.CreateStringFromCsFile(
       Path.GetFullPath(Path.Combine(
           AppDomain.CurrentDomain.BaseDirectory,
           "..", "..", "..", "..",
           "sandbox", "src", "Scripts", "ActionScripts", "AddPediatricTestsV3.cs"
       ))
   );
        sourceCodeVaccineAction = EmberMethods.CreateStringFromCsFile(
          Path.GetFullPath(Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory,
          "..", "..", "..", "..",
          "sandbox", "src", "Scripts", "ActionScripts", "VaccineScript.cs"
      ))
      );
        sourceCodePedia = EmberMethods.CreateStringFromCsFile(
       Path.GetFullPath(Path.Combine(
           AppDomain.CurrentDomain.BaseDirectory,
                   "..", "..", "..", "..",
                   "sandbox", "src", "Scripts", "ConditionScripts", "PediatricCondition.cs"
               ))
           );
        sourceCodeWhileTrue = EmberMethods.CreateStringFromCsFile(
      Path.GetFullPath(Path.Combine(
      AppDomain.CurrentDomain.BaseDirectory,
      "..", "..", "..", "..",
      "sandbox", "src", "Scripts", "FaultyScripts", "WhileTrueScript.cs"
  ))
  );
        sourceCodeIllegalUsings = EmberMethods.CreateStringFromCsFile(
            Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "sandbox", "src", "Scripts", "FaultyScripts", "IllegalUsingScript.cs"
        ))
        );
        sourceCodeMissingUsing = EmberMethods.CreateStringFromCsFile(
            Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "sandbox", "src", "Scripts", "FaultyScripts", "MissingUsingScript.cs"
        ))
        );
        sourceCodePreventUsage = EmberMethods.CreateStringFromCsFile(
            Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "sandbox", "src", "Scripts", "FaultyScripts", "PreventUsageScript.cs"
        ))
        );

        sourceCodeWhileTrueUnsafe = """
            using System;   //todo this is possible to default in compiler
            using System.Threading.Tasks;
            using System.Collections.Generic;   //todo same for them
            using Ember.Scripting;
            using GeneratorScriptsV3;
            using IGeneratorContext_V4;

            public class WhileTrueScript : GeneratorScriptsV3.IGeneratorActionScript
            {
                public async Task<ActionResultV3.ActionResult> ExecuteAsync(IGeneratorContext_V4.IGeneratorContext context)
                {
                    int i = 0;
                    while (i >= 0)
                    {
                        //Ember.Scripting.ScriptEnvironment.CurrentToken.Value.ThrowIfCancellationRequested();
                        // Console.WriteLine("Infinite Loop, iteration N." + i);
                        i++;
                        if (i <= 0) //for integer overflow
                        {
                            i = 1;
                        }
                    }
                return ActionResultV3.ActionResult.Success("Infinite Loop script returned (should never happen!)");
                }
            }
        """;

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
    public static ISccriptManagerDeleteAfter InitScriptManager()
    {
        ISccriptManagerDeleteAfter scriptManager;
        ServiceCollection services = new ServiceCollection();

        LoggerForScripting logger = new LoggerForScripting();

        services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddSerilog(dispose: true);
        });
        services.AddSingleton<IUserSession, Sandbox.SandBoxUserSession>();

        services.AddDbContextFactory<EFModeling.EntityProperties.FluentAPI.Required.ScriptDbContext>();
        ScriptingServiceCollectionExtensions.AddEmberScripting(services, EmberMethods.GetReferences(), EmberMethods.GetEmberApiVersion());

        var provider = services.BuildServiceProvider();

        return scriptManager = provider.GetRequiredService<ISccriptManagerDeleteAfter>();
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
}
internal record ObjectsRecord
{
    public required LabOrder labOrder { get; init; }
    public required Patient patient { get; init; }
    public required ConsoleLogger logger { get; init; }
    public required DataAccess testDataAccess { get; init; }
    public required Vaccine vaccine { get; init; }
}