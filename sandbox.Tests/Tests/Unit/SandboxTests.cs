using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[TestClass]
public class SanboxTests
{
    ISccriptManagerDeleteAfter? facade;
    // EmberMethods? em;
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
    }
    [TestMethod]
    public async Task CreateContextTest()
    {
        ScriptFactory sf;
        GeneratorContext ctx;

        facade = EmberMethods.GetNewScriptManagerInstance(1);
        sf = new ScriptFactory(facade);
        ctx = sf.CreateContextForApiV();

        Assert.IsTrue(ctx.GetType() == typeof(ReadOnlyContextV1.GeneratorContext));

        facade = EmberMethods.GetNewScriptManagerInstance(2);
        sf = new ScriptFactory(facade);
        ctx = sf.CreateContextForApiV();

        Assert.IsTrue(ctx.GetType() == typeof(RWContextV2.GeneratorContext));

        facade = EmberMethods.GetNewScriptManagerInstance(6);
        sf = new ScriptFactory(facade);

        Exception ex = await Assert.ThrowsExceptionAsync<Exception>(async () =>  //todo check this one
         {
             ctx = sf.CreateContextForApiV();
         });
        // Assert.IsTrue(ex.Message.Contains("No Context class defined in") && ex.Message.Contains("for the passed API version."));
        Assert.IsTrue(ex.Message.Contains("The version was not found in the Dictionary"));

        Console.WriteLine(ex.Message);
        var ls = ExceptionHelper.GetExceptionList(ex);
        ExceptionHelper.PrintExceptionListToConsole(ex);

        // Assert.IsTrue(false);
    }

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

        ScriptFactory sf;
        facade = EmberMethods.GetNewScriptManagerInstance(1);
        sf = new ScriptFactory(facade);
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
        facade = EmberMethods.GetNewScriptManagerInstance();
        (string className, string baseTypeName, int versionInt) valResult = facade.BasicValidationBeforeCompiling(sourceCodeActionV2!);

        Console.WriteLine(nameof(valResult.baseTypeName) + " : " + valResult.baseTypeName);
        Console.WriteLine(nameof(valResult.className) + " : " + valResult.className);
        Console.WriteLine(nameof(valResult.versionInt) + " : " + valResult.versionInt);

        Assert.IsTrue(valResult.baseTypeName == "IGeneratorActionScript");
        Assert.IsTrue(valResult.className == "AddPediatricTestsV3");
        Assert.IsTrue(valResult.versionInt == 3);

        valResult = facade.BasicValidationBeforeCompiling(sourceCodeActionV1!);

        Console.WriteLine(nameof(valResult.baseTypeName) + " : " + valResult.baseTypeName);
        Console.WriteLine(nameof(valResult.className) + " : " + valResult.className);
        Console.WriteLine(nameof(valResult.versionInt) + " : " + valResult.versionInt);

        Assert.IsTrue(valResult.baseTypeName == "IGeneratorActionScript");
        Assert.IsTrue(valResult.className == "AddPediatricTestsV2");
        Assert.IsTrue(valResult.versionInt == 2);

        valResult = facade.BasicValidationBeforeCompiling(sourceCodeActionV3!);

        Console.WriteLine(nameof(valResult.baseTypeName) + " : " + valResult.baseTypeName);
        Console.WriteLine(nameof(valResult.className) + " : " + valResult.className);
        Console.WriteLine(nameof(valResult.versionInt) + " : " + valResult.versionInt);

        Assert.IsTrue(valResult.baseTypeName == "IGeneratorActionScript");
        Assert.IsTrue(valResult.className == "AddPediatricTestsV4");
        Assert.IsTrue(valResult.versionInt == 4);

        valResult = facade.BasicValidationBeforeCompiling(sourceCodeVaccineAction!);

        Console.WriteLine(nameof(valResult.baseTypeName) + " : " + valResult.baseTypeName);
        Console.WriteLine(nameof(valResult.className) + " : " + valResult.className);
        Console.WriteLine(nameof(valResult.versionInt) + " : " + valResult.versionInt);

        Assert.IsTrue(valResult.baseTypeName == "IGeneratorActionScript");
        Assert.IsTrue(valResult.className == "VaccineScript");
        Assert.IsTrue(valResult.versionInt == 5);

        valResult = facade.BasicValidationBeforeCompiling(sourceCodePedia!);

        Console.WriteLine(nameof(valResult.baseTypeName) + " : " + valResult.baseTypeName);
        Console.WriteLine(nameof(valResult.className) + " : " + valResult.className);
        Console.WriteLine(nameof(valResult.versionInt) + " : " + valResult.versionInt);

        Assert.IsTrue(valResult.baseTypeName == "IGeneratorConditionScript");
        Assert.IsTrue(valResult.className == "PediatricCondition");
        Assert.IsTrue(valResult.versionInt == 1);

        // Assert.IsTrue(false);

    }

    [TestMethod]
    public async Task CreateContextForApiTest()
    {
        Guid id;
        ScriptFactory sf;
        (string className, string baseTypeName, int versionInt) valResult;

        facade = EmberMethods.GetNewScriptManagerInstance();
        valResult = facade.BasicValidationBeforeCompiling(sourceCodeActionV2!);
        id = await facade.CreateScript(sourceCodeActionV2!);

        sf = new ScriptFactory(facade);
        await facade.ExecuteScriptById(id, sf.CreateContextForApiV(valResult.versionInt));

        for (int i = 0; i < 6; i++)
        {
            facade = EmberMethods.GetNewScriptManagerInstance(i);
            valResult = facade.BasicValidationBeforeCompiling(sourceCodeActionV2!);
            id = await facade.CreateScript(sourceCodeActionV2!);

            sf = new ScriptFactory(facade);
            await facade.ExecuteScriptById(id, sf.CreateContextForApiV(valResult.versionInt));
        }

        for (int i = 0; i < 6; i++)
        {
            facade = EmberMethods.GetNewScriptManagerInstance(i);
            valResult = facade.BasicValidationBeforeCompiling(sourceCodeVaccineAction!);
            id = await facade.CreateScript(sourceCodeVaccineAction!);

            sf = new ScriptFactory(facade);
            await facade.ExecuteScriptById(id, sf.CreateContextForApiV(valResult.versionInt));
        }
        for (int i = 0; i < 6; i++)
        {
            facade = EmberMethods.GetNewScriptManagerInstance(i);
            valResult = facade.BasicValidationBeforeCompiling(sourceCodePedia!);
            id = await facade.CreateScript(sourceCodePedia!);

            sf = new ScriptFactory(facade);
            await facade.ExecuteScriptById(id, sf.CreateContextForApiV(valResult.versionInt));
        }
    }

    [TestMethod]
    public async Task CreateContextByDowngradeTest()
    {
        Guid id;
        ScriptFactory sf;
        (string className, string baseTypeName, int versionInt) valResult;
        object result;
        ActionResultV3.ActionResult ar;
        string src;

        src = sourceCodeVaccineAction!;
        facade = EmberMethods.GetNewScriptManagerInstance();
        valResult = facade.BasicValidationBeforeCompiling(src!);
        id = await facade.CreateScript(src!);

        sf = new ScriptFactory(facade);
        result = await facade.ExecuteScriptById(id, await sf.CreateContextByDowngrade(src!));
        ar = (ActionResultV3.ActionResult)EmberMethods.UpgradeActionResult(result);
        Console.WriteLine(ar.ToString());
        Assert.IsInstanceOfType(ar, typeof(ActionResultV3.ActionResult));

        src = sourceCodeActionV3!;
        valResult = facade.BasicValidationBeforeCompiling(src!);
        id = await facade.CreateScript(src!);

        sf = new ScriptFactory(facade);
        result = await facade.ExecuteScriptById(id, await sf.CreateContextByDowngrade(src!));
        ar = (ActionResultV3.ActionResult)EmberMethods.UpgradeActionResult(result);
        Console.WriteLine(ar.ToString());
        Assert.IsInstanceOfType(ar, typeof(ActionResultV3.ActionResult));

        src = sourceCodeActionV1!;
        valResult = facade.BasicValidationBeforeCompiling(src!);
        id = await facade.CreateScript(src!);

        sf = new ScriptFactory(facade);
        result = await facade.ExecuteScriptById(id, await sf.CreateContextByDowngrade(src!));
        ar = (ActionResultV3.ActionResult)EmberMethods.UpgradeActionResult(result);
        Console.WriteLine(ar.ToString());
        Assert.IsInstanceOfType(ar, typeof(ActionResultV3.ActionResult));

        src = sourceCodePedia!;
        valResult = facade.BasicValidationBeforeCompiling(src!);
        id = await facade.CreateScript(src!);

        sf = new ScriptFactory(facade);
        result = await facade.ExecuteScriptById(id, await sf.CreateContextByDowngrade(src!));
        // ar = EmberMethods.UpgradeActionResult(result);
        Console.WriteLine(result.ToString());

        Assert.IsInstanceOfType(result, typeof(bool));

        // Assert.IsTrue(false);
    }
}