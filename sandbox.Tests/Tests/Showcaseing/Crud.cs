#pragma warning disable CS0436

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ember.Simulation;

[TestClass]
public class CrudDemos
{
    static IScriptManagerBaseExtended _scriptManagerBase = InitScriptManager().Item2;
    static IScriptManager _scriptManager = InitScriptManager().Item1;
    static EmberMethods em = new EmberMethods(_scriptManagerBase);

    [TestInitialize]
    public async Task SetupAsync()
    {
        await _scriptManagerBase.DeleteAllData();
    }

    public static (IScriptManager, IScriptManagerBaseExtended) InitScriptManager()
    {
        IScriptManager scriptManager;
        IScriptManagerBaseExtended scriptManagerBase;
        ServiceCollection services = new ServiceCollection();

        LoggerForScripting logger = new LoggerForScripting();

        services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddSerilog(dispose: true);
        });
        services.AddSingleton<IUserSession, SandBoxUserSession>();
        services.AddDbContextFactory<ScriptDbContext>();
        ScriptingServiceCollectionExtensions.AddEmberScripting(services, EmberMethods.GetReferences(), EmberMethods.GetEmberApiVersion(), RecentTypeHelper.GetRecentTypes());

        var provider = services.BuildServiceProvider();

        scriptManager = provider.GetRequiredService<IScriptManager>();
        scriptManagerBase = provider.GetRequiredService<IScriptManagerBaseExtended>();

        return (scriptManager, scriptManagerBase);
    }
    [TestMethod]
    public async Task Create()
    {
        string sourceCode = TestHelper.GetSC().sourceCodeActionV1;
        CustomerScript script = await _scriptManagerBase!.CreateScript(sourceCode!);

        Console.WriteLine("Name: " + script.ScriptName);
        Console.WriteLine("Type: " + script.ScriptType);
    }
    [TestMethod]
    public async Task Read()
    {
        await Create();
        CustomerScript script = await _scriptManagerBase.GetScript<IActionScript>("AddPediatricTestsV2");  //
        Console.WriteLine("Name: " + script.ScriptName + ", ScriptType: " + script.ScriptType);
    }
    [TestMethod]
    public async Task Update()
    {
        await Create();
        string newSourceCode = TestHelper.GetSC().sourceCodeActionV3;
        await _scriptManagerBase.UpdateScript<IActionScript>("AddPediatricTestsV2", newSourceCode);
    }

    [TestMethod]
    public async Task Delete()
    {
        await Create();
        await _scriptManagerBase.DeleteScript<IActionScript>("AddPediatricTestsV2");
    }
    [TestMethod]
    public async Task Execute()
    {
        LabOrder labOrder = new LabOrder("1", "Pediatrics");
        Vaccine vaccine = new Vaccine("Polio", 1, DateTime.UtcNow);

        ConsoleLogger logger = new ConsoleLogger();
        DataAccess testDataAccess = new DataAccess();

        string sourceCode = TestHelper.GetSC().sourceCodeActionV1;
        CustomerScript script = await _scriptManagerBase!.CreateScript(sourceCode!);


        var services = new ServiceCollection();

        Ember.Simulation.SandboxServiceCollectionExtensions.AddSandboxServices
        (services, logger, testDataAccess);

        using var provider = services.BuildServiceProvider();

        RecentGeneratorContextFactory.IGeneratorContextFactory factory = provider.GetRequiredService<RecentGeneratorContextFactory.IGeneratorContextFactory>();


        RecentIGeneratorContext ctx = factory.CreateGeneratorContext(labOrder, vaccine);

        RecentActionResult ar = (RecentActionResult)await _scriptManager!.ExecuteScript<IActionScript>
        ("AddPediatricTestsV2", ctx, nameof(RecentIActionScript.ExecuteAsync));

        ActionScript scriptInstance = _scriptManager.GetScript<ActionScript>(script.ScriptName!);

        ar = await scriptInstance.ExecuteAsync(ctx);
        Assert.IsTrue(ar.ToString().Contains("[Message contains either failure or succes: ] Pediatric tests added"));

        //Doesnt compile which is good
        // await Assert.ThrowsExceptionAsync<CouldNotFindMethodException>(async () =>
        // {
        // bool conditionResult = await scriptInstance.EvaluateAsync(ctx);
        // });


        sourceCode = TestHelper.GetSC().sourceCodeMultiMethodScripts;
        script = await _scriptManagerBase!.CreateScript(sourceCode!);

        scriptInstance = _scriptManager.GetScript<ActionScript>(script.ScriptName!);
        ar = await scriptInstance.Execute1(ctx);

        Assert.IsTrue(ar.ToString().Contains("[Message contains either failure or succes: ] ExecuteAction1 was called"));

        string strResult = await scriptInstance.Execute3(ctx);
        Assert.IsTrue(strResult.Contains("successfully returned"));
    }

    string scriptSourceCode = """
        using System;
        using System.Threading.Tasks;
        using System.Collections.Generic;
        using Ember.Scripting;
        using IGeneratorContext_V3;
        using GeneratorScriptV2;

        public class AddPediatricTestsV3 : GeneratorScriptV2.IActionScript
        {
            public async Task<ActionResultV2.ActionResult> Execute1OldName(IGeneratorContext_V3.IGeneratorContext context)
            {
                return ActionResultV2.ActionResult.Success("Successfully returned old Method!");
            }
            public async Task<ActionResultV2.ActionResult> ExecuteMethodThatWasDeleted(IGeneratorContext_V3.IGeneratorContext context)
            {
                return ActionResultV2.ActionResult.Success("Successfully returned deleted Method!");
            }
        }
        """;
    string scriptSourceCodeNew = """
        using System.Threading.Tasks;
        using System;
        using Ember.Scripting;
        using GeneratorScriptV4;

        public class VaccineScript : GeneratorScriptV4.IActionScript
        {
            public async Task<ActionResultV3.ActionResult> Execute2(IGeneratorContextNoInheritance_V5.IGeneratorContext context)
            {
                return ActionResultV3.ActionResult.Success("New Method Example");
            }
        }
        """;

    [TestMethod]
    public async Task RenamedMethodTestAsync()
    {
        ActionScript scriptInstance = await _scriptManager.CreateScript<ActionScript>(scriptSourceCode);

        RecentActionResult ar = await scriptInstance.Execute1(TestHelper.GetContext());

        Assert.IsTrue(ar.ToString().Contains("Successfully returned old Method!"));
    }
    [TestMethod]
    public async Task DeletedOldMethodTestAsync()
    {
        CustomerScript script = await _scriptManagerBase!.CreateScript(scriptSourceCode);
        ActionScript scriptInstance = _scriptManager.GetScript<ActionScript>(script.ScriptName!);
        // RecentActionResult ar = await scriptInstance.ExecuteMethodThatWasDeleted(TestHelper.GetContext());   //no way of calling it wont compile
    }
    [TestMethod]
    public async Task AddedNemMethodTestAsync()
    {
        CustomerScript script = await _scriptManagerBase!.CreateScript(scriptSourceCode);
        ActionScript scriptInstance = _scriptManager.GetScript<ActionScript>(script.ScriptName!);
        await Assert.ThrowsExceptionAsync<CouldNotFindMethodException>(async () =>
        {
            RecentActionResult ar = await scriptInstance.Execute2(TestHelper.GetContext()); //will throw because method doesnt exist in the script
        });
    }
}
