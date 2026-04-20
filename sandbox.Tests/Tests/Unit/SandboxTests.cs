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

        ContextManagement sf;
        _facade = EmberMethods.GetNewScriptManagerInstance(1);
        sf = new ContextManagement(_facade);
        Dictionary<int, Type> retrievedDict = ContextVersionScanner.GetClassDictionary();
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

    [TestMethod]
    public void ActionResultVersionScannerTest()
    {
        Dictionary<int, Type> contextVersionMap = new()
        {
            {1, typeof(ActionResultV1.ActionResult)},
            {2, typeof(ActionResultV2.ActionResult)},
            {3, typeof(ActionResultV3.ActionResult)},
        };

        Dictionary<int, Type> retrievedDict = ActionResultVersionScanner.GetClassDictionary();
        PrintDictToConsole(retrievedDict);

        CollectionAssert.AreEquivalent(contextVersionMap, retrievedDict);
        // Assert.IsFalse(true);
    }
    [TestMethod]
    public void BasicValidationTestUsingGetDictionary()
    {
        _facade = EmberMethods.GetNewScriptManagerInstance();
        ValidationRecord valResult = _facade.BasicValidationBeforeCompiling(_sourceCodeActionV2!);

        Console.WriteLine(nameof(valResult.ScriptType) + " : " + valResult.ScriptType);
        Console.WriteLine(nameof(valResult.ClassName) + " : " + valResult.ClassName);
        Console.WriteLine(nameof(valResult.Version) + " : " + valResult.Version);

        Assert.IsTrue(valResult.ScriptType == typeof(IActionScript));
        Assert.IsTrue(valResult.ClassName == "AddPediatricTestsV3");
        Assert.IsTrue(valResult.Version == 3);

        valResult = _facade.BasicValidationBeforeCompiling(_sourceCodeActionV1!);

        Console.WriteLine(nameof(valResult.ScriptType) + " : " + valResult.ScriptType);
        Console.WriteLine(nameof(valResult.ClassName) + " : " + valResult.ClassName);
        Console.WriteLine(nameof(valResult.Version) + " : " + valResult.Version);

        Assert.IsTrue(valResult.ScriptType == typeof(IActionScript));
        Assert.IsTrue(valResult.ClassName == "AddPediatricTestsV2");
        Assert.IsTrue(valResult.Version == 2);

        valResult = _facade.BasicValidationBeforeCompiling(_sourceCodeActionV3!);

        Console.WriteLine(nameof(valResult.ScriptType) + " : " + valResult.ScriptType);
        Console.WriteLine(nameof(valResult.ClassName) + " : " + valResult.ClassName);
        Console.WriteLine(nameof(valResult.Version) + " : " + valResult.Version);

        Assert.IsTrue(valResult.ScriptType == typeof(IActionScript));
        Assert.IsTrue(valResult.ClassName == "AddPediatricTestsV4");
        Assert.IsTrue(valResult.Version == 4);

        valResult = _facade.BasicValidationBeforeCompiling(_sourceCodeVaccineAction!);

        Console.WriteLine(nameof(valResult.ScriptType) + " : " + valResult.ScriptType);
        Console.WriteLine(nameof(valResult.ClassName) + " : " + valResult.ClassName);
        Console.WriteLine(nameof(valResult.Version) + " : " + valResult.Version);

        Assert.IsTrue(valResult.ScriptType == typeof(IActionScript));
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

        RecentContext data = new RecentContext(labOrder: _obj!.labOrder, vaccine: _obj.vaccine);

        CustomerScript script;
        ContextManagement sf;
        ValidationRecord valResult;
        object result;
        ActionResultV3.ActionResult ar;
        string src;

        src = _sourceCodeVaccineAction!;
        _facade = EmberMethods.GetNewScriptManagerInstance();
        valResult = _facade.BasicValidationBeforeCompiling(src!);
        script = (await _facade.CreateScript(src!));

        sf = new ContextManagement(_facade);
        Context ctx = await sf.CreateByDowngrade(script.ScriptApiVersion, data!);
        Console.WriteLine("Type name: " + ctx.GetType().FullName);

        result = await _facade.ExecuteScript(script.Id, ctx, nameof(IExecuteAsync.ExecuteAsync));
        ar = (ActionResultV3.ActionResult)EmberMethods.UpgradeActionResult(result);
        Console.WriteLine(ar.ToString());
        Assert.IsInstanceOfType(ar, typeof(ActionResultV3.ActionResult));

        src = _sourceCodeActionV3!;
        valResult = _facade.BasicValidationBeforeCompiling(src!);
        script = (await _facade.CreateScript(src!));

        sf = new ContextManagement(_facade);
        ctx = await sf.CreateByDowngrade(script.ScriptApiVersion, data!);
        Console.WriteLine("Type name: " + ctx.GetType().FullName);

        result = await _facade.ExecuteScript(script.Id, ctx, nameof(IExecuteAsync.ExecuteAsync));
        ar = (ActionResultV3.ActionResult)EmberMethods.UpgradeActionResult(result);
        Console.WriteLine(ar.ToString());
        Assert.IsInstanceOfType(ar, typeof(ActionResultV3.ActionResult));

        src = _sourceCodeActionV1!;
        valResult = _facade.BasicValidationBeforeCompiling(src!);
        script = (await _facade.CreateScript(src!));

        sf = new ContextManagement(_facade);
        ctx = await sf.CreateByDowngrade(script.ScriptApiVersion, data!);
        Console.WriteLine("Type name: " + ctx.GetType().FullName);

        result = await _facade.ExecuteScript(script.Id, ctx, nameof(IExecuteAsync.ExecuteAsync));
        ar = (ActionResultV3.ActionResult)EmberMethods.UpgradeActionResult(result);
        Console.WriteLine(ar.ToString());
        Assert.IsInstanceOfType(ar, typeof(ActionResultV3.ActionResult));

        src = _sourceCodePedia!;
        valResult = _facade.BasicValidationBeforeCompiling(src!);
        script = (await _facade.CreateScript(src!));

        sf = new ContextManagement(_facade);
        ctx = await sf.CreateByDowngrade(script.ScriptApiVersion, data!);
        Console.WriteLine("Type name: " + ctx.GetType().FullName);

        result = await _facade.ExecuteScript(script.Id, ctx, nameof(IEvaluateAsync.EvaluateAsync));
        // ar = EmberMethods.UpgradeActionResult(result);
        Console.WriteLine(result.ToString());

        Assert.IsInstanceOfType(result, typeof(bool));


        ////////////////////////////////////////////
        ctx = await sf.CreateByDowngrade(script.ScriptApiVersion, data!);
        result = await _facade.ExecuteScript(script.Id, ctx, nameof(IEvaluateAsync.EvaluateAsync));


        // Assert.IsTrue(false);
    }
}