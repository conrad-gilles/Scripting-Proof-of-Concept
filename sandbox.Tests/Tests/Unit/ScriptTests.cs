using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;

[TestClass]
public class ScriptTests
{


    ScriptManagerFacade? facade;
    RandomMethods? rm;
    string? ActionResultVersionSpecific;
    string? sourceCodeActionV1;
    string? sourceCodeActionV2;
    string? sourceCodeActionV3;
    string? sourceCodeVaccineAction;
    string? sourceCodePedia;


    [TestInitialize]
    public async Task Setup()
    {
        // var logger = new LoggerForScripting();
        // // var microsoftLogger = logger.GetMicrosoftLogger<ScriptManagerFacade>();
        // ScriptCompiler compiler = new ScriptCompiler(RandomMethods.GetReferences(), logger.GetMicrosoftLogger<ScriptCompiler>());
        // // compiler = new ScriptCompiler(UsefulMethods.GetReferences());
        // DbHelper db = new DbHelper(compiler, RandomMethods.GetReferences(), logger.GetMicrosoftLogger<DbHelper>());
        // ScriptExecutor executor = new ScriptExecutor(logger.GetMicrosoftLogger<ScriptExecutor>());

        // facade = new ScriptManagerFacade(db, compiler, executor, RandomMethods.GetReferences(), logger.GetMicrosoftLogger<ScriptManagerFacade>());

        var logger = new LoggerForScripting();
        Log.Debug("Sandbox launched.");

        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            // Assuming you use Serilog, this forwards standard MS Logging to Serilog
            builder.AddSerilog(dispose: true);
        });

        ScriptingServiceCollectionExtensions.AddEmberScripting(services, RandomMethods.GetReferences());

        using var provider = services.BuildServiceProvider();

        facade = provider.GetRequiredService<ScriptManagerFacade>();
        rm = new RandomMethods(facade);

        ActionResultVersionSpecific = "[Message contains either failure or succes: ] ";  //change this if action result version changes it will thraow cause of message contains
        sourceCodeActionV1 = rm.CreateStringFromCsFile(
           Path.GetFullPath(Path.Combine(
               AppDomain.CurrentDomain.BaseDirectory,
               "..", "..", "..", "..",
               "sandbox", "src", "Scripts", "ActionScripts", "AddPediatricTestsV1.cs"
           ))
       );
        sourceCodeActionV2 = rm.CreateStringFromCsFile(
        Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "sandbox", "src", "Scripts", "ActionScripts", "AddPediatricTestsV2.cs"
        ))
    );
        sourceCodeActionV3 = rm.CreateStringFromCsFile(
       Path.GetFullPath(Path.Combine(
           AppDomain.CurrentDomain.BaseDirectory,
           "..", "..", "..", "..",
           "sandbox", "src", "Scripts", "ActionScripts", "AddPediatricTestsV3.cs"
       ))
   );
        sourceCodeVaccineAction = rm.CreateStringFromCsFile(
          Path.GetFullPath(Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory,
          "..", "..", "..", "..",
          "sandbox", "src", "Scripts", "ActionScripts", "VaccineScript.cs"
      ))
      );
        sourceCodePedia = rm.CreateStringFromCsFile(
       Path.GetFullPath(Path.Combine(
           AppDomain.CurrentDomain.BaseDirectory,
                   "..", "..", "..", "..",
                   "sandbox", "src", "Scripts", "ConditionScripts", "PediatricCondition.cs"
               ))
           );
    }


    [TestMethod]
    public async Task ActionV1Test()
    {
        Guid id = await facade!.CreateScript(sourceCodeActionV1!);
        CustomerScript retrievedScript = await facade.GetScript(id);
        var context = rm!.GetTestingContext<GeneratorContextNoInherVaccine>(justForTesting: retrievedScript);
        object resultBeforeUpgrade = await facade.ExecuteScriptById(id, context);
        ActionResultV3NoInheritance result = RandomMethods.UpgradeActionResult(resultBeforeUpgrade);
        string shouldReturn = ActionResultVersionSpecific + "Pediatric tests added";
        Assert.IsInstanceOfType(result, typeof(ActionResultBaseClass));
        Assert.IsInstanceOfType(result, typeof(ActionResultV3NoInheritance));
        Assert.IsTrue(result.ToString().Contains(shouldReturn));

        //todo make negative test
    }
    [TestMethod]
    public async Task ActionV2Test()
    {
        Guid id = await facade!.CreateScript(sourceCodeActionV2!);
        CustomerScript retrievedScript = await facade.GetScript(id);
        var context = rm!.GetTestingContext<GeneratorContextNoInherVaccine>(justForTesting: retrievedScript);
        object resultBeforeUpgrade = await facade.ExecuteScriptById(id, context);
        ActionResultV3NoInheritance result = RandomMethods.UpgradeActionResult(resultBeforeUpgrade);
        string shouldReturn = ActionResultVersionSpecific + "Pediatric tests added";
        Assert.IsInstanceOfType(result, typeof(ActionResultBaseClass));
        Assert.IsInstanceOfType(result, typeof(ActionResultV3NoInheritance));
        Assert.IsTrue(result.ToString().Contains(shouldReturn));

        //todo make negative test
    }
    [TestMethod]
    public async Task ActionV3Test()
    {
        Guid id = await facade!.CreateScript(sourceCodeActionV3!);
        CustomerScript retrievedScript = await facade.GetScript(id);
        var context = rm!.GetTestingContext<GeneratorContextNoInherVaccine>(justForTesting: retrievedScript);
        object resultBeforeUpgrade = await facade.ExecuteScriptById(id, context);
        ActionResultV3NoInheritance result = RandomMethods.UpgradeActionResult(resultBeforeUpgrade);
        string shouldReturn = ActionResultVersionSpecific + "Pediatric tests added V3";
        Assert.IsInstanceOfType(result, typeof(ActionResultBaseClass));
        Assert.IsInstanceOfType(result, typeof(ActionResultV3NoInheritance));
        Assert.IsTrue(result.ToString().Contains(shouldReturn));

        //todo make negative test
    }
    [TestMethod]
    public async Task VaccineScriptTest()
    {
        Guid id = await facade!.CreateScript(sourceCodeVaccineAction!);
        CustomerScript retrievedScript = await facade.GetScript(id);
        var context = rm!.GetTestingContext<GeneratorContextNoInherVaccine>(justForTesting: retrievedScript);
        object result = await facade.ExecuteScriptById(id, context);
        string shouldReturn = ActionResultVersionSpecific + "Polio Vaccine added";
        Assert.IsInstanceOfType(result, typeof(ActionResultBaseClass));
        Assert.IsInstanceOfType(result, typeof(ActionResultV3NoInheritance));
        Assert.IsTrue(result.ToString()!.Contains(shouldReturn));

        //todo make negative test
    }

    [TestMethod]
    public async Task ConditionTest()
    {
        Guid id = await facade!.CreateScript(sourceCodePedia!);
        CustomerScript retrievedScript = await facade.GetScript(id);
        var context = rm!.GetTestingContext<GeneratorContextNoInherVaccine>(justForTesting: retrievedScript);
        object result = await facade.ExecuteScriptById(id, context);
        string shouldReturn = "True";
        Assert.IsInstanceOfType(result, typeof(bool));
        Assert.IsTrue(result.ToString()!.Contains(shouldReturn));

        //todo make negative test
    }
    // [TestMethod]
    // public async Task RunAllScripts()
    // {
    //     await facade.ClearAllCaches();
    //     await facade.CompileAllScripts();
    //     var allScripts = await facade.ListScripts();
    //     Assert.IsTrue(allScripts.Count() == 5);
    //     foreach (var item in allScripts)
    //     {
    //         Guid id = item.Id;
    //         CustomerScript retrievedScript = await facade.GetScript(id);
    //         var context = UsefulMethods.GetTestingContext<GeneratorContextNoInherVaccine>(justForTesting: retrievedScript);
    //         object result = await facade.ExecuteScriptById(id, context);
    //         Assert.IsTrue(result != null);
    //         Assert.IsTrue(result is bool || result is ActionResultBaseClass);
    //     }
    //     //todo make negative test
    // }
}
