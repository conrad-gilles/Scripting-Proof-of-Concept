#pragma warning disable CS0436

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.ComponentModel.DataAnnotations;
using Sandbox;
using Ember.Scripting.Compilation;

using Ember.Simulation;
using Newtonsoft.Json.Serialization;

[TestClass]
public class SanboxTests
{
    private IScriptManagerExtended? _facade;
    private EmberMethods? _em;
    private ObjectsRecord? _obj;
    // DataV1.MockData? data;
    // private DataV2.DataV2? _data;
    private string? _sourceCodeActionV1 = TestHelper.GetSC().sourceCodeActionV1;
    private string? _sourceCodeActionV2 = TestHelper.GetSC().sourceCodeActionV2;
    private string? _sourceCodeActionV3 = TestHelper.GetSC().sourceCodeActionV3;
    private string? _sourceCodeVaccineAction = TestHelper.GetSC().sourceCodeVaccineAction;
    private string? _sourceCodePedia = TestHelper.GetSC().sourceCodePedia;
    private List<string>? _sourceCodes = TestHelper.GetSC(includeCondInList: false).sourceCodes;

    [TestInitialize]
    public
     async Task Setup()
    {
        _facade = EmberMethods.GetNewScriptManagerInstance();
        _em = new EmberMethods(_facade!);
        await _facade.DeleteAllData();
        _obj = TestHelper.ScriptObjects();
        // data = new DataV1.MockData(labOrder: obj.labOrder, patient: obj.patient, consoleLogger: obj.logger,
        // dataAccess: obj.testDataAccess, vaccine: obj.vaccine);
        // _data = new DataV2.DataV2(labOrder: _obj.labOrder, patient: _obj.patient, consoleLogger: _obj.logger,
        //    dataAccess: _obj.testDataAccess, vaccine: _obj.vaccine);
    }
    // [TestMethod]
    // public async Task CreateContextTest()
    // {
    //     Sandbox.ContextManagementDemos sf;
    //     Context ctx;

    //     _facade = EmberMethods.GetNewScriptManagerInstance(1);
    //     sf = new Sandbox.ContextManagementDemos(_facade);
    //     ctx = sf.CreateContextForApiV(_data!);

    //     Assert.IsTrue(ctx.GetType() == typeof(ReadOnlyContextV1.GeneratorContext));

    //     _facade = EmberMethods.GetNewScriptManagerInstance(2);
    //     sf = new Sandbox.ContextManagementDemos(_facade);
    //     ctx = sf.CreateContextForApiV(_data!);

    //     Assert.IsTrue(ctx.GetType() == typeof(RWContextV2.GeneratorContext));

    //     _facade = EmberMethods.GetNewScriptManagerInstance(6);
    //     sf = new Sandbox.ContextManagementDemos(_facade);

    //     Exception ex = await Assert.ThrowsExceptionAsync<Exception>(async () =>  //todo check this one
    //      {
    //          ctx = sf.CreateContextForApiV(_data!);
    //      });
    //     // Assert.IsTrue(ex.Message.Contains("No Context class defined in") && ex.Message.Contains("for the passed API version."));
    //     Assert.IsTrue(ex.Message.Contains("The version was not found in the Dictionary"));

    //     Console.WriteLine(ex.Message);
    //     var ls = ExceptionHelper.GetExceptionList(ex);
    //     ExceptionHelper.PrintExceptionListToConsole(ex);

    //     // Assert.IsTrue(false);
    // }

    [TestMethod]
    public void GetDictionaryTest()
    {
        Dictionary<int, Type> contextVersionMap = new()
        {
            {1, typeof(ReadOnlyContextV1.GeneratorContext)},
            {2, typeof(RWContextV2.GeneratorContext)},
            {3, typeof(GeneratorContextV3.GeneratorContext)},
            {4, typeof(GeneratorContextV4.GeneratorContext)},
            {5, typeof(GeneratorContextNoInherVaccineV5.GeneratorContext)},
        };

        _facade = EmberMethods.GetNewScriptManagerInstance(1);
        RecentIGeneratorContext ctx = new RecentGeneratorContext(null!, null!);
        Dictionary<int, Type> retrievedDict = ContextVersionScanner.GetClassDictionary(ctx);
        retrievedDict.Reverse();
        PrintDictToConsole(contextVersionMap);
        PrintDictToConsole(retrievedDict);
        CollectionAssert.AreEquivalent(contextVersionMap, retrievedDict);
    }

    public void PrintDictToConsole(Dictionary<int, Type> dict)
    {
        Console.WriteLine("Start of dict String:");
        foreach (var pair in dict)
        {
            Console.WriteLine("Key: " + pair.Key + ", Value: " + pair.Value);
        }
    }


    //the following three tests were partally ai generated
    [TestMethod]
    public void UpgradeCustomReturn_UpgradesFromV1ToLatest()
    {
        ActionResultV1.ActionResult v1Failure = ActionResultV1.ActionResult.Failure("it didnt work");
        ActionResultV1.ActionResult v1Success = ActionResultV1.ActionResult.Success("ye it worked");

        ActionResultV3.ActionResult upgradedFailure = (ActionResultV3.ActionResult)UpgradeManager.UpgradeReturnValue(v1Failure);
        ActionResultV3.ActionResult upgradedSuccess = (ActionResultV3.ActionResult)UpgradeManager.UpgradeReturnValue(v1Success);

        Assert.IsInstanceOfType(upgradedFailure, typeof(ActionResultV3.ActionResult));
        Assert.IsInstanceOfType(upgradedSuccess, typeof(ActionResultV3.ActionResult));

        ActionResultV3.ActionResult v3Failure = (ActionResultV3.ActionResult)upgradedFailure;
        ActionResultV3.ActionResult v3Success = (ActionResultV3.ActionResult)upgradedSuccess;

        Assert.IsFalse(v3Failure.FailedOrNot);
        Assert.IsTrue(v3Success.FailedOrNot);
    }

    [TestMethod]
    public void UpgradeCustomReturn_UpgradesFromV2ToLatest()
    {
        ActionResultV2.ActionResult v2 = ActionResultV2.ActionResult.Failure(
            "something broke", new List<string> { "step1", "step2" });

        ActionResultV3.ActionResult upgraded = (ActionResultV3.ActionResult)UpgradeManager.UpgradeReturnValue(v2);

        Assert.IsInstanceOfType(upgraded, typeof(ActionResultV3.ActionResult));
        ActionResultV3.ActionResult v3 = (ActionResultV3.ActionResult)upgraded;
        Assert.IsFalse(v3.FailedOrNot);
        StringAssert.Contains(v3.Message, "something broke");
    }

    [TestMethod]
    public void UpgradeCustomReturn_AlreadyLatest_ReturnsSameType()
    {
        ActionResultV3.ActionResult v3 = ActionResultV3.ActionResult.Success("already latest");

        ActionResultV3.ActionResult result = (ActionResultV3.ActionResult)UpgradeManager.UpgradeReturnValue(v3);

        Assert.IsInstanceOfType(result, typeof(ActionResultV3.ActionResult));
        Assert.AreEqual(v3, result);
    }
    [TestMethod]
    public void BasicValidationTestUsingGetDictionary()
    {
        _facade = EmberMethods.GetNewScriptManagerInstance();
        ValidationRecord valResult = _facade.BasicValidationBeforeCompiling(_sourceCodeActionV2!);

        Console.WriteLine(nameof(valResult.ScriptType) + " : " + valResult.ScriptType);
        Console.WriteLine(nameof(valResult.ClassName) + " : " + valResult.ClassName);
        Console.WriteLine(nameof(valResult.Version) + " : " + valResult.Version);

        Assert.IsTrue(valResult.ScriptType == typeof(IActionScriptBase));
        Assert.IsTrue(valResult.ClassName == "AddPediatricTestsV3");
        Assert.IsTrue(valResult.Version == 3);

        valResult = _facade.BasicValidationBeforeCompiling(_sourceCodeActionV1!);

        Console.WriteLine(nameof(valResult.ScriptType) + " : " + valResult.ScriptType);
        Console.WriteLine(nameof(valResult.ClassName) + " : " + valResult.ClassName);
        Console.WriteLine(nameof(valResult.Version) + " : " + valResult.Version);

        Assert.IsTrue(valResult.ScriptType == typeof(IActionScriptBase));
        Assert.IsTrue(valResult.ClassName == "AddPediatricTestsV2");
        Assert.IsTrue(valResult.Version == 2);

        valResult = _facade.BasicValidationBeforeCompiling(_sourceCodeActionV3!);

        Console.WriteLine(nameof(valResult.ScriptType) + " : " + valResult.ScriptType);
        Console.WriteLine(nameof(valResult.ClassName) + " : " + valResult.ClassName);
        Console.WriteLine(nameof(valResult.Version) + " : " + valResult.Version);

        Assert.IsTrue(valResult.ScriptType == typeof(IActionScriptBase));
        Assert.IsTrue(valResult.ClassName == "AddPediatricTestsV4");
        Assert.IsTrue(valResult.Version == 4);

        valResult = _facade.BasicValidationBeforeCompiling(_sourceCodeVaccineAction!);

        Console.WriteLine(nameof(valResult.ScriptType) + " : " + valResult.ScriptType);
        Console.WriteLine(nameof(valResult.ClassName) + " : " + valResult.ClassName);
        Console.WriteLine(nameof(valResult.Version) + " : " + valResult.Version);

        Assert.IsTrue(valResult.ScriptType == typeof(IActionScriptBase));
        Assert.IsTrue(valResult.ClassName == "VaccineScript");
        Assert.IsTrue(valResult.Version == 5);

        valResult = _facade.BasicValidationBeforeCompiling(_sourceCodePedia!);

        Console.WriteLine(nameof(valResult.ScriptType) + " : " + valResult.ScriptType);
        Console.WriteLine(nameof(valResult.ClassName) + " : " + valResult.ClassName);
        Console.WriteLine(nameof(valResult.Version) + " : " + valResult.Version);

        Assert.IsTrue(valResult.ScriptType == typeof(IConditionScript));
        Assert.IsTrue(valResult.ClassName == "PediatricCondition");
        Assert.IsTrue(valResult.Version == 1);

        // Assert.IsTrue(false);

    }

    // [TestMethod]
    // public async Task CreateContextForApiTest()
    // {
    //     Guid id;
    //     Sandbox.ContextManagementDemos sf;
    //     ValidationRecord valResult;
    //     // string methodName = TestHelper.GetMethodNameAction();

    //     _facade = EmberMethods.GetNewScriptManagerInstance();

    //     valResult = _facade.BasicValidationBeforeCompiling(_sourceCodeActionV1!);
    //     id = (await _facade.CreateScript(_sourceCodeActionV1!)).Id;

    //     sf = new Sandbox.ContextManagementDemos(_facade);


    //     await _facade.ExecuteScriptById(id, sf.CreateContextForApiV(_data!, valResult.Version), nameof(IExecuteAsync.ExecuteAsync));
    //     valResult = _facade.BasicValidationBeforeCompiling(_sourceCodeActionV2!);

    //     id = (await _facade.CreateScript(_sourceCodeActionV2!)).Id;
    //     Console.WriteLine("Class dict start i: " + ContextVersionScanner.GetClassDictionary().Keys.Min() + ", Calss dict max: " + ContextVersionScanner.GetClassDictionary().Keys.Max());
    //     for (int i = 1; i < ContextVersionScanner.GetClassDictionary().Count(); i++)
    //     {

    //         _facade = EmberMethods.GetNewScriptManagerInstance(i);
    //         await _facade.CompileScript(id);
    //         // valResult = facade.BasicValidationBeforeCompiling(sourceCodeActionV2!);
    //         // id = await facade.CreateScript(sourceCodeActionV2!);

    //         sf = new Sandbox.ContextManagementDemos(_facade);
    //         // await facade.ExecuteScriptById(id, sf.CreateContextForApiV(data!, valResult.versionInt));
    //         Console.WriteLine("Running API version: " + _facade.GetRunningApiVersion());
    //         await _facade.ExecuteScriptById(id, sf.CreateContextForApiV(_data!, valResult.Version), nameof(IExecuteAsync.ExecuteAsync));
    //     }

    //     valResult = _facade.BasicValidationBeforeCompiling(_sourceCodeVaccineAction!);
    //     id = (await _facade.CreateScript(_sourceCodeVaccineAction!)).Id;
    //     for (int i = 1; i < ContextVersionScanner.GetClassDictionary().Count(); i++)
    //     {
    //         _facade = EmberMethods.GetNewScriptManagerInstance(i);
    //         sf = new Sandbox.ContextManagementDemos(_facade);
    //         await _facade.ExecuteScriptById(id, sf.CreateContextForApiV(_data!, valResult.Version), nameof(IExecuteAsync.ExecuteAsync));
    //     }

    //     valResult = _facade.BasicValidationBeforeCompiling(_sourceCodePedia!);
    //     id = (await _facade.CreateScript(_sourceCodePedia!)).Id;
    //     for (int i = 1; i < ContextVersionScanner.GetClassDictionary().Count(); i++)
    //     {
    //         _facade = EmberMethods.GetNewScriptManagerInstance(i);
    //         sf = new Sandbox.ContextManagementDemos(_facade);
    //         await _facade.ExecuteScriptById(id, sf.CreateContextForApiV(_data!, valResult.Version), nameof(IEvaluateAsync.EvaluateAsync));
    //     }
    // }

    [TestMethod]
    public async Task CreateContextByDowngradeTest()
    {

        RecentGeneratorContext data = new RecentGeneratorContext(labOrder: _obj!.labOrder, vaccine: _obj.vaccine);

        CustomerScript script;
        ValidationRecord valResult;
        object result;
        ActionResultV3.ActionResult ar;
        string src;

        src = _sourceCodeVaccineAction!;
        _facade = EmberMethods.GetNewScriptManagerInstance();
        valResult = _facade.BasicValidationBeforeCompiling(src!);
        script = (await _facade.CreateScript(src!));

        IContext ctx = (IContext)await ContextManager.CreateByDowngrade(script.ScriptApiVersion, data!);
        Console.WriteLine("Type name: " + ctx.GetType().FullName);

        result = await _facade.ExecuteScript(script.Id, (IContext)ctx, nameof(RecentIActionScript.ExecuteAsync));
        ar = (ActionResultV3.ActionResult)UpgradeManager.UpgradeReturnValue(result);
        Console.WriteLine(ar.ToString());
        Assert.IsInstanceOfType(ar, typeof(ActionResultV3.ActionResult));

        src = _sourceCodeActionV3!;
        valResult = _facade.BasicValidationBeforeCompiling(src!);
        script = (await _facade.CreateScript(src!));

        ctx = (IContext)await ContextManager.CreateByDowngrade(script.ScriptApiVersion, data!);
        Console.WriteLine("Type name: " + ctx.GetType().FullName);

        result = await _facade.ExecuteScript(script.Id, (IContext)ctx, nameof(RecentIActionScript.ExecuteAsync));
        ar = (ActionResultV3.ActionResult)UpgradeManager.UpgradeReturnValue(result);
        Console.WriteLine(ar.ToString());
        Assert.IsInstanceOfType(ar, typeof(ActionResultV3.ActionResult));

        src = _sourceCodeActionV1!;
        valResult = _facade.BasicValidationBeforeCompiling(src!);
        script = (await _facade.CreateScript(src!));

        ctx = (IContext)await ContextManager.CreateByDowngrade(script.ScriptApiVersion, data!);
        Console.WriteLine("Type name: " + ctx.GetType().FullName);

        result = await _facade.ExecuteScript(script.Id, (IContext)ctx, nameof(RecentIActionScript.ExecuteAsync));
        ar = (ActionResultV3.ActionResult)UpgradeManager.UpgradeReturnValue(result);
        Console.WriteLine(ar.ToString());
        Assert.IsInstanceOfType(ar, typeof(ActionResultV3.ActionResult));

        src = _sourceCodePedia!;
        valResult = _facade.BasicValidationBeforeCompiling(src!);
        script = (await _facade.CreateScript(src!));

        ctx = (IContext)await ContextManager.CreateByDowngrade(script.ScriptApiVersion, data!);
        Console.WriteLine("Type name: " + ctx.GetType().FullName);

        result = await _facade.ExecuteScript(script.Id, (IContext)ctx, nameof(RecentIConditionScript.EvaluateAsync));
        // ar = EmberMethods.UpgradeActionResult(result);
        Console.WriteLine(result.ToString());

        Assert.IsInstanceOfType(result, typeof(bool));


        ////////////////////////////////////////////
        ctx = (IContext)await ContextManager.CreateByDowngrade(script.ScriptApiVersion, data!);
        result = await _facade.ExecuteScript(script.Id, (IContext)ctx, nameof(RecentIConditionScript.EvaluateAsync));


        // Assert.IsTrue(false);
    }
}