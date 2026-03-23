using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[TestClass]
public class EmberInternalFacadeTests
{
    private ISccriptManagerDeleteAfter? _scriptManager;
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

        id = await _scriptManager.CreateScript(_sourceCodeActionV2!);

        // DataV1.MockData data = new DataV1.MockData(labOrder: obj.labOrder, patient: obj.patient, consoleLogger: obj.logger,
        //                 dataAccess: obj.testDataAccess, vaccine: obj.vaccine);

        DataV2.DataV2 data = new DataV2.DataV2(labOrder: _obj.labOrder, patient: _obj.patient, consoleLogger: _obj.logger,
                                dataAccess: _obj.testDataAccess, vaccine: _obj.vaccine);


        // ActiveGeneratorContext ctx = new ActiveGeneratorContext(labOrder: obj.labOrder, vaccine: obj.vaccine);

        var ctx = InternalScriptManager.CreateContext(_obj.labOrder, _obj.vaccine);

        ar = await InternalScriptManager.ExecuteScriptById(id, ctx!);



        // Assert.IsTrue(ar.GetType() == typeof(ActionResultV3.ActionResult));
        Console.WriteLine("Type name: " + ar.GetType().FullName);
        Console.WriteLine("Returned result: " + ar);



        // Assert.IsTrue(false);
    }

    [TestMethod]
    public async Task TestingQueryAndExecutionByNameAndType()
    {
        ScriptNameType scriptTuple;
        scriptTuple = await _scriptManager!.CreateScriptUsingNameType(_sourceCodeActionV1!);

        var ctx = _internalScriptManager!.CreateContext(_obj.labOrder, _obj.vaccine);
        ActiveActionResult ar = await _internalScriptManager.ExecuteScriptByNameAndType(scriptTuple.Name, scriptTuple.Type, ctx);

        Console.WriteLine("Name: " + scriptTuple.Name + ", ScriptType: " + scriptTuple.Type);
        Console.WriteLine("Type name: " + ar.GetType().FullName);
        Console.WriteLine("Returned result: " + ar);

        Assert.IsTrue(scriptTuple.Name == "AddPediatricTestsV2");
        Assert.IsTrue(scriptTuple.Type == ScriptTypes.GeneratorActionScript);
        Assert.IsTrue(ar.ToString().Contains("[Message contains either failure or succes: ] Pediatric tests added"));

        Exception e = await Assert.ThrowsExceptionAsync<Ember.Scripting.ScriptRepositoryException>(async () =>
         {
             scriptTuple = await _scriptManager!.CreateScriptUsingNameType(_sourceCodeActionV1!);
         });
        Console.WriteLine(e.ToString());

        scriptTuple = await _scriptManager!.CreateScriptUsingNameType(_sourceCodeActionV3!);
        ctx = _internalScriptManager!.CreateContext(_obj.labOrder, _obj.vaccine);
        ar = await _internalScriptManager.ExecuteScriptByNameAndType(scriptTuple.Name, scriptTuple.Type, ctx);

        Console.WriteLine("Name: " + scriptTuple.Name + ", ScriptType: " + scriptTuple.Type);
        Console.WriteLine("Type name: " + ar.GetType().FullName);
        Console.WriteLine("Returned result: " + ar);

        Assert.IsTrue(scriptTuple.Name == "AddPediatricTestsV4");
        Assert.IsTrue(scriptTuple.Type == ScriptTypes.GeneratorActionScript);
        Assert.IsTrue(ar.ToString().Contains("[Message contains either failure or succes: ] Pediatric tests added V3"));
        // Assert.IsTrue(false);
    }

    [TestMethod]
    public async Task TestContextFactoryDI()
    {
        ScriptNameType scriptTuple;
        scriptTuple = await _scriptManager!.CreateScriptUsingNameType(_sourceCodeActionV1!);

        var objs = TestHelper.ScriptObjects();
        var services = new ServiceCollection();
        Sandbox.SandboxServiceCollectionExtensions.AddSandboxServices
        (services, _obj!.logger, _obj.testDataAccess);
        using var provider = services.BuildServiceProvider();

        ActiveContextFactory.IGeneratorContextFactory factory = provider.GetRequiredService<ActiveContextFactory.IGeneratorContextFactory>();
        ActiveGeneratorContext ctx = factory.Create(_obj.labOrder, _obj.vaccine);

        ActiveActionResult ar = await _internalScriptManager!.ExecuteScriptByNameAndType(scriptTuple.Name, scriptTuple.Type, ctx);

        Console.WriteLine("Name: " + scriptTuple.Name + ", ScriptType: " + scriptTuple.Type);
        Console.WriteLine("Type name: " + ar.GetType().FullName);
        Console.WriteLine("Returned result: " + ar);

        Assert.IsTrue(scriptTuple.Name == "AddPediatricTestsV2");
        Assert.IsTrue(scriptTuple.Type == ScriptTypes.GeneratorActionScript);
        Assert.IsTrue(ar.ToString().Contains("[Message contains either failure or succes: ] Pediatric tests added"));

        // Assert.IsTrue(false);
    }
}