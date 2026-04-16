#pragma warning disable CS0436

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ember.Simulation;
using Sandbox;

[TestClass]
public class CrudDemos
{
    static IScriptManagerDeleteAfter ScriptManager = InitScriptManager();
    static EmberInternalFacade InternalScriptManager = new EmberInternalFacade(ScriptManager);
    static EmberMethods em = new EmberMethods(ScriptManager);

    [TestInitialize]
    public async Task SetupAsync()
    {
        // //this block is ecessary at least once in the code everytime you modify the init.sql else the db wont be initialized somehow
        // using (var db = new EFModeling.EntityProperties.FluentAPI.Required.ScriptDbContext())
        // {
        //     await db.Database.EnsureDeletedAsync();
        //     await db.Database.EnsureCreatedAsync();
        // }
        await ScriptManager.DeleteAllData();
    }

    public static IScriptManagerDeleteAfter InitScriptManager()
    {
        IScriptManagerDeleteAfter scriptManager;
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

        return scriptManager = provider.GetRequiredService<IScriptManagerDeleteAfter>();
    }
    [TestMethod]
    public async Task Create()
    {
        string sourceCode = TestHelper.GetSC().sourceCodeActionV1;
        CustomerScript script = await ScriptManager!.CreateScript(sourceCode!);

        Console.WriteLine("Name: " + script.ScriptName);
        Console.WriteLine("Type: " + script.ScriptType);
    }
    [TestMethod]
    public async Task Read()
    {
        await Create();
        CustomerScript script = await ScriptManager.GetScriptNT<IActionScript>("AddPediatricTestsV2");  //
        Console.WriteLine("Name: " + script.ScriptName + ", ScriptType: " + script.ScriptType);
    }
    [TestMethod]
    public async Task Update()
    {
        await Create();
        string newSourceCode = TestHelper.GetSC().sourceCodeActionV3;
        await ScriptManager.UpdateScriptNT<IActionScript>("AddPediatricTestsV2", newSourceCode);
    }

    [TestMethod]
    public async Task Delete()
    {
        await Create();
        await ScriptManager.DeleteScriptNT<IActionScript>("AddPediatricTestsV2");
    }
    [TestMethod]
    public async Task Execute()
    {
        LabOrder labOrder = new LabOrder("1", "Pediatrics");
        Vaccine vaccine = new Vaccine("Polio", 1, DateTime.UtcNow);

        ConsoleLogger logger = new ConsoleLogger();
        DataAccess testDataAccess = new DataAccess();

        await Create();

        var services = new ServiceCollection();

        Ember.Simulation.SandboxServiceCollectionExtensions.AddSandboxServices
        (services, logger, testDataAccess);

        using var provider = services.BuildServiceProvider();

        RecentContextFactory.IGeneratorContextFactory factory = provider.GetRequiredService<RecentContextFactory.IGeneratorContextFactory>();


        RecentContext ctx = factory.Create(labOrder, vaccine);

        RecentActionResult ar = (RecentActionResult)await InternalScriptManager!.ExecuteScript<IActionScript>
        ("AddPediatricTestsV2", ctx, nameof(IExecuteAsync.ExecuteAsync));


        // ar = (ActiveActionResult)await InternalScriptManager.Execute1<IGeneratorActionScript>
        // ("AddPediatricTestsV2", ctx);

        // ar = (ActiveActionResult)await InternalScriptManager.Execute2<IGeneratorActionScript>
        // ("AddPediatricTestsV2", ctx);
    }
}
