using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ember.Simulation;
using Microsoft.CodeAnalysis.Scripting;

[TestClass]
public class EmberInternalFacadeTests
{
    private IScriptManagerDeleteAfter? _scriptManager;
    private EmberInternalFacade? _internalScriptManager;
    private EmberMethods? _em;
    private ObjectsRecord? _obj;
    private DataV1.MockData? _data;
    private string? _sourceCodeActionV1 = TestHelper.GetSC().sourceCodeActionV1;
    private string? _sourceCodeActionV2 = TestHelper.GetSC().sourceCodeActionV2;
    private string? _sourceCodeActionV3 = TestHelper.GetSC().sourceCodeActionV3;
    private string? _sourceCodeVaccineAction = TestHelper.GetSC().sourceCodeVaccineAction;
    private string? _sourceCodePedia = TestHelper.GetSC().sourceCodePedia;
    private List<string>? _sourceCodes = TestHelper.GetSC(includeCondInList: false).sourceCodes;

    [TestInitialize]
    public
     void Setup()
    {
        _scriptManager = EmberMethods.GetNewScriptManagerInstance();
        _internalScriptManager = new EmberInternalFacade(_scriptManager);
        _scriptManager.DeleteAllData();
        _em = new EmberMethods(_scriptManager!);
        _obj = TestHelper.ScriptObjects();
        _data = new DataV1.MockData(labOrder: _obj.labOrder, patient: _obj.patient, consoleLogger: _obj.logger,
        dataAccess: _obj.testDataAccess, vaccine: _obj.vaccine);
    }

    [TestMethod]
    public async Task CreateContextByDowngradeTest()
    {
        Guid id;
        ActionResultV3.ActionResult ar;
        string src;

        src = _sourceCodeVaccineAction!;
        _scriptManager = EmberMethods.GetNewScriptManagerInstance();

        EmberInternalFacade InternalScriptManager = new EmberInternalFacade(_scriptManager);

        id = (await _scriptManager.CreateScript(_sourceCodeActionV2!)).Id;

        // DataV1.MockData data = new DataV1.MockData(labOrder: obj.labOrder, patient: obj.patient, consoleLogger: obj.logger,
        //                 dataAccess: obj.testDataAccess, vaccine: obj.vaccine);

        DataV2.DataV2 data = new DataV2.DataV2(labOrder: _obj.labOrder, patient: _obj.patient, consoleLogger: _obj.logger,
                                dataAccess: _obj.testDataAccess, vaccine: _obj.vaccine);


        // ActiveGeneratorContext ctx = new ActiveGeneratorContext(labOrder: obj.labOrder, vaccine: obj.vaccine);

        // var ctx = InternalScriptManager.CreateContext(_obj.labOrder, _obj.vaccine);
        var ctx = TestHelper.GetContext();

        ar = (RecentActionResult)await InternalScriptManager.ExecuteScript(id, ctx!, "Default");



        // Assert.IsTrue(ar.GetType() == typeof(ActionResultV3.ActionResult));
        Console.WriteLine("Type name: " + ar.GetType().FullName);
        Console.WriteLine("Returned result: " + ar);



        // Assert.IsTrue(false);
    }

    [TestMethod]
    public async Task TestingQueryAndExecutionByNameAndType()
    {
        CustomerScript script;
        script = await _scriptManager!.CreateScript(_sourceCodeActionV1!);

        // var ctx = _internalScriptManager!.CreateContext(_obj!.labOrder, _obj.vaccine);
        var ctx = TestHelper.GetContext();
        // Type scriptType=script.GetScriptType();
        RecentActionResult ar = (RecentActionResult)await _internalScriptManager.ExecuteScript<IActionScript>(script.ScriptName!, ctx, "Default");

        Console.WriteLine("Name: " + script.ScriptName + ", ScriptType: " + script.GetScriptType().Name);
        Console.WriteLine("Type name: " + ar.GetType().FullName);
        Console.WriteLine("Returned result: " + ar);

        Assert.IsTrue(script.ScriptName == "AddPediatricTestsV2");
        Assert.IsTrue(script.GetScriptType() == typeof(IActionScript));
        Assert.IsTrue(ar.ToString().Contains("[Message contains either failure or succes: ] Pediatric tests added"));

        Exception e = await Assert.ThrowsExceptionAsync<Ember.Scripting.CreateAndInsertCustomerScriptException>(async () =>
         {
             script = await _scriptManager!.CreateScript(_sourceCodeActionV1!);
         });
        Console.WriteLine(e.ToString());

        script = await _scriptManager!.CreateScript(_sourceCodeActionV3!);
        // ctx = _internalScriptManager!.CreateContext(_obj.labOrder, _obj.vaccine);
        ctx = TestHelper.GetContext();
        ar = (RecentActionResult)await _internalScriptManager.ExecuteScript<IActionScript>(script.ScriptName!, ctx, "Default");

        Console.WriteLine("Name: " + script.ScriptName + ", ScriptType: " + script.GetScriptType());
        Console.WriteLine("Type name: " + ar.GetType().FullName);
        Console.WriteLine("Returned result: " + ar);

        Assert.IsTrue(script.ScriptName == "AddPediatricTestsV4");
        Assert.IsTrue(script.GetScriptType() == typeof(IActionScript));
        Assert.IsTrue(ar.ToString().Contains("[Message contains either failure or succes: ] Pediatric tests added V3"));
        // Assert.IsTrue(false);
    }

    [TestMethod]
    public async Task TestContextFactoryDI()
    {
        CustomerScript scrip;
        scrip = await _scriptManager!.CreateScript(_sourceCodeActionV1!);

        var objs = TestHelper.ScriptObjects();
        var services = new ServiceCollection();
        Ember.Simulation.SandboxServiceCollectionExtensions.AddSandboxServices
        (services, _obj!.logger, _obj.testDataAccess);
        using var provider = services.BuildServiceProvider();

        RecentContextFactory.IGeneratorContextFactory factory = provider.GetRequiredService<RecentContextFactory.IGeneratorContextFactory>();
        RecentContext ctx = factory.Create(_obj.labOrder, _obj.vaccine);

        RecentActionResult ar = (RecentActionResult)await _internalScriptManager!.ExecuteScript<IActionScript>(scrip.ScriptName!, ctx, "Default");

        Console.WriteLine("Name: " + scrip.ScriptName! + ", ScriptType: " + scrip.GetScriptType());
        Console.WriteLine("Type name: " + ar.GetType().FullName);
        Console.WriteLine("Returned result: " + ar);

        Assert.IsTrue(scrip.ScriptName == "AddPediatricTestsV2");
        Assert.IsTrue(scrip.GetScriptType() == typeof(IActionScript));
        Assert.IsTrue(ar.ToString().Contains("[Message contains either failure or succes: ] Pediatric tests added"));

        // Assert.IsTrue(false);
    }
}