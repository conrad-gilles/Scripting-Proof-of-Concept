using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[TestClass]
public class CrudDemos
{
    static ISccriptManagerDeleteAfter ScriptManager = InitScriptManager();
    static EmberInternalFacade InternalScriptManager = new EmberInternalFacade(ScriptManager);
    static EmberMethods em = new EmberMethods(ScriptManager);

    [TestInitialize]
    public async Task SetupAsync()
    {
        //this block is ecessary at least once in the code everytime you modify the init.sql else the db wont be initialized somehow
        using (var db = new EFModeling.EntityProperties.FluentAPI.Required.MyContext())
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }
        await ScriptManager.DeleteAllData();
    }

    public static ISccriptManagerDeleteAfter InitScriptManager()
    {
        ISccriptManagerDeleteAfter facade;
        ServiceCollection services = new ServiceCollection();

        LoggerForScripting logger = new LoggerForScripting();

        services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddSerilog(dispose: true);
        });
        services.AddDbContextFactory<EFModeling.EntityProperties.FluentAPI.Required.MyContext>();
        ScriptingServiceCollectionExtensions.AddEmberScripting(services, EmberMethods.GetReferences(), EmberMethods.GetEmberApiVersion());

        var provider = services.BuildServiceProvider();

        return facade = provider.GetRequiredService<ISccriptManagerDeleteAfter>();
    }
    [TestMethod]
    public async Task Create()
    {
        string sourceCode = TestHelper.GetSC().sourceCodeActionV1;
        ScriptNameType scriptRecord = await ScriptManager!.CreateScriptUsingNameType(sourceCode!);
    }
    [TestMethod]
    public async Task Read()
    {
        await Create();
        CustomerScript script = await ScriptManager.GetScriptNT("AddPediatricTestsV2", ScriptTypes.GeneratorActionScript);
        Console.WriteLine("Name: " + script.ScriptName + ", ScriptType: " + script.ScriptType);
    }
    [TestMethod]
    public async Task Update()
    {
        await Create();
        string newSourceCode = TestHelper.GetSC().sourceCodeActionV3;
        await ScriptManager.UpdateScriptNT("AddPediatricTestsV2", ScriptTypes.GeneratorActionScript, newSourceCode);
    }
    [TestMethod]
    public async Task Delete()
    {
        await Create();
        await ScriptManager.DeleteScriptNT("AddPediatricTestsV2", ScriptTypes.GeneratorActionScript);
    }
    [TestMethod]
    public async Task Execute()
    {
        LabOrder labOrder = new LabOrder("1", "Pediatrics");
        Patient patient = new Patient("1", "TestFirst", "TestLast", new DateTime(2010, 6, 1, 7, 47, 0), "M");   //mfu
        ConsoleLogger logger = new ConsoleLogger();
        DataAccess testDataAccess = new DataAccess();
        Vaccine vaccine = new Vaccine("Polio", 1, DateTime.UtcNow);

        await Create();

        var services = new ServiceCollection();
        Sandbox.SandboxServiceCollectionExtensions.AddSandboxData
        (services, labOrder, patient, logger, testDataAccess, vaccine);
        using var provider = services.BuildServiceProvider();
        ActiveGeneratorContext ctx = (ActiveGeneratorContext)ActiveContextFactory.Create(provider);
        ActiveActionResult ar = await InternalScriptManager!.ExecuteScriptByNameAndType("AddPediatricTestsV2", ScriptTypes.GeneratorActionScript, ctx);
    }
}
