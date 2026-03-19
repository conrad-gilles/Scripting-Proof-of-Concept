using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[TestClass]
public class EmberInternalFacadeTests
{
    private ISccriptManagerDeleteAfter? _ScriptManager;
    private EmberInternalFacade? _InternalScriptManager;
    private EmberMethods? _em;
    private (LabOrder labOrder, Patient patient, ConsoleLogger logger, DataAccess testDataAccess, Vaccine vaccine) _obj;
    private DataV1.MockData? _data;
    private string? ActionResultVersionSpecific;
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
        ActionResultVersionSpecific = "[Message contains either failure or succes: ] ";
        _ScriptManager = EmberMethods.GetNewScriptManagerInstance();
        _InternalScriptManager = new EmberInternalFacade(_ScriptManager);
        _ScriptManager.DeleteAllData();
        _em = new EmberMethods(_ScriptManager!);
        ActionResultVersionSpecific = "[Message contains either failure or succes: ] ";
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
        _ScriptManager = EmberMethods.GetNewScriptManagerInstance();

        EmberInternalFacade InternalScriptManager = new EmberInternalFacade(_ScriptManager);

        id = await _ScriptManager.CreateScript(_sourceCodeActionV2!);

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
        scriptTuple = await _ScriptManager!.CreateScriptUsingNameType(_sourceCodeActionV1!);

        var ctx = _InternalScriptManager!.CreateContext(_obj.labOrder, _obj.vaccine);
        ActiveActionResult ar = await _InternalScriptManager.ExecuteScriptByNameAndType(scriptTuple.Name, scriptTuple.Type, ctx);

        Console.WriteLine("Name: " + scriptTuple.Name + ", ScriptType: " + scriptTuple.Type);
        Console.WriteLine("Type name: " + ar.GetType().FullName);
        Console.WriteLine("Returned result: " + ar);

        Assert.IsTrue(scriptTuple.Name == "AddPediatricTestsV2");
        Assert.IsTrue(scriptTuple.Type == ScriptTypes.GeneratorActionScript);
        Assert.IsTrue(ar.ToString().Contains("[Message contains either failure or succes: ] Pediatric tests added"));

        Exception e = await Assert.ThrowsExceptionAsync<Ember.Scripting.DbHelperException>(async () =>
         {
             scriptTuple = await _ScriptManager!.CreateScriptUsingNameType(_sourceCodeActionV1!);
         });
        Console.WriteLine(e.ToString());

        scriptTuple = await _ScriptManager!.CreateScriptUsingNameType(_sourceCodeActionV3!);
        ctx = _InternalScriptManager!.CreateContext(_obj.labOrder, _obj.vaccine);
        ar = await _InternalScriptManager.ExecuteScriptByNameAndType(scriptTuple.Name, scriptTuple.Type, ctx);

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
        scriptTuple = await _ScriptManager!.CreateScriptUsingNameType(_sourceCodeActionV1!);

        var objs = TestHelper.ScriptObjects();
        var services = new ServiceCollection();
        Sandbox.ScriptingServiceCollectionExtensions.AddSandboxData
        (services, _obj.labOrder, _obj.patient, _obj.logger, _obj.testDataAccess, _obj.vaccine);
        using var provider = services.BuildServiceProvider();
        ActiveGeneratorContext ctx = (ActiveGeneratorContext)ActiveContextFactory.Create(provider);
        ActiveActionResult ar = await _InternalScriptManager!.ExecuteScriptByNameAndType(scriptTuple.Name, scriptTuple.Type, ctx);

        Console.WriteLine("Name: " + scriptTuple.Name + ", ScriptType: " + scriptTuple.Type);
        Console.WriteLine("Type name: " + ar.GetType().FullName);
        Console.WriteLine("Returned result: " + ar);

        Assert.IsTrue(scriptTuple.Name == "AddPediatricTestsV2");
        Assert.IsTrue(scriptTuple.Type == ScriptTypes.GeneratorActionScript);
        Assert.IsTrue(ar.ToString().Contains("[Message contains either failure or succes: ] Pediatric tests added"));

        // Assert.IsTrue(false);
    }
}