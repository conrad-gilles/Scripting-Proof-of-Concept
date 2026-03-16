using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[TestClass]
public class EmberInternalFacadeTests
{
    ISccriptManagerDeleteAfter? ScriptManager;
    EmberInternalFacade? InternalScriptManager;
    EmberMethods? em;
    (LabOrder labOrder, Patient patient, ConsoleLogger logger, DataAccess testDataAccess, Vaccine vaccine) obj;
    DataV1.MockData? data;
    string? ActionResultVersionSpecific;
    string? sourceCodeActionV1 = TestHelper.GetSC().sourceCodeActionV1;
    string? sourceCodeActionV2 = TestHelper.GetSC().sourceCodeActionV2;
    string? sourceCodeActionV3 = TestHelper.GetSC().sourceCodeActionV3;
    string? sourceCodeVaccineAction = TestHelper.GetSC().sourceCodeVaccineAction;
    string? sourceCodePedia = TestHelper.GetSC().sourceCodePedia;
    List<string>? sourceCodes = TestHelper.GetSC(includeCondInList: false).sourceCodes;

    [TestInitialize]
    public
     void Setup()
    {
        ActionResultVersionSpecific = "[Message contains either failure or succes: ] ";
        ScriptManager = EmberMethods.GetNewScriptManagerInstance();
        InternalScriptManager = new EmberInternalFacade(ScriptManager);
        ScriptManager.DeleteAllData();
        em = new EmberMethods(ScriptManager!);
        ActionResultVersionSpecific = "[Message contains either failure or succes: ] ";
        obj = em.ScriptObjects();
        data = new DataV1.MockData(labOrder: obj.labOrder, patient: obj.patient, consoleLogger: obj.logger,
        dataAccess: obj.testDataAccess, vaccine: obj.vaccine);
    }

    [TestMethod]
    public async Task CreateContextByDowngradeTest()
    {
        Guid id;
        ActionResultV3.ActionResult ar;
        string src;

        src = sourceCodeVaccineAction!;
        ScriptManager = EmberMethods.GetNewScriptManagerInstance();

        EmberInternalFacade InternalScriptManager = new EmberInternalFacade(ScriptManager);

        id = await ScriptManager.CreateScript(sourceCodeActionV2!);

        // DataV1.MockData data = new DataV1.MockData(labOrder: obj.labOrder, patient: obj.patient, consoleLogger: obj.logger,
        //                 dataAccess: obj.testDataAccess, vaccine: obj.vaccine);

        DataV2.DataV2 data = new DataV2.DataV2(labOrder: obj.labOrder, patient: obj.patient, consoleLogger: obj.logger,
                                dataAccess: obj.testDataAccess, vaccine: obj.vaccine);


        // ActiveGeneratorContext ctx = new ActiveGeneratorContext(labOrder: obj.labOrder, vaccine: obj.vaccine);

        var ctx = InternalScriptManager.CreateContext(obj.labOrder, obj.vaccine);

        ar = await InternalScriptManager.ExecuteScriptById(id, ctx!);



        // Assert.IsTrue(ar.GetType() == typeof(ActionResultV3.ActionResult));
        Console.WriteLine("Type name: " + ar.GetType().FullName);
        Console.WriteLine("Returned result: " + ar);



        // Assert.IsTrue(false);
    }

    [TestMethod]
    public async Task TestingQueryAndExecutionByNameAndType()
    {
        (string Name, ScriptTypes ScriptType) script;
        script = await ScriptManager!.CreateScriptUsingNameType(sourceCodeActionV1!);

        var ctx = InternalScriptManager!.CreateContext(obj.labOrder, obj.vaccine);
        ActiveActionResult ar = await InternalScriptManager.ExecuteScriptByNameAndType(script.Name, script.ScriptType, ctx);

        Console.WriteLine("Name: " + script.Name + ", ScriptType: " + script.ScriptType);
        Console.WriteLine("Type name: " + ar.GetType().FullName);
        Console.WriteLine("Returned result: " + ar);

        Assert.IsTrue(script.Name == "AddPediatricTestsV2");
        Assert.IsTrue(script.ScriptType == ScriptTypes.GeneratorActionScript);
        Assert.IsTrue(ar.ToString().Contains("[Message contains either failure or succes: ] Pediatric tests added"));

        Exception e = await Assert.ThrowsExceptionAsync<Ember.Scripting.DbHelperException>(async () =>
         {
             script = await ScriptManager!.CreateScriptUsingNameType(sourceCodeActionV1!);
         });
        Console.WriteLine(e.ToString());

        script = await ScriptManager!.CreateScriptUsingNameType(sourceCodeActionV3!);
        ctx = InternalScriptManager!.CreateContext(obj.labOrder, obj.vaccine);
        ar = await InternalScriptManager.ExecuteScriptByNameAndType(script.Name, script.ScriptType, ctx);

        Console.WriteLine("Name: " + script.Name + ", ScriptType: " + script.ScriptType);
        Console.WriteLine("Type name: " + ar.GetType().FullName);
        Console.WriteLine("Returned result: " + ar);

        Assert.IsTrue(script.Name == "AddPediatricTestsV4");
        Assert.IsTrue(script.ScriptType == ScriptTypes.GeneratorActionScript);
        Assert.IsTrue(ar.ToString().Contains("[Message contains either failure or succes: ] Pediatric tests added V3"));
        // Assert.IsTrue(false);
    }

}