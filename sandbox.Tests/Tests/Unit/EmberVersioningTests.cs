using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.SolutionPersistence.Serializer.SlnV12;
using System.Runtime.CompilerServices;

[TestClass]
public class EmberVersioningTests
{


    ISccriptManagerDeleteAfter? facade;
    EmberMethods? em;
    string? ActionResultVersionSpecific;
    string? sourceCodeActionV1 = TestHelper.GetSC().sourceCodeActionV1;
    string? sourceCodeActionV2 = TestHelper.GetSC().sourceCodeActionV2;
    string? sourceCodeActionV3 = TestHelper.GetSC().sourceCodeActionV3;
    string? sourceCodeVaccineAction = TestHelper.GetSC().sourceCodeVaccineAction;
    string? sourceCodePedia = TestHelper.GetSC().sourceCodePedia;
    // LoggerForScripting? logger;
    List<string>? sourceCodes = TestHelper.GetSC(includeCondInList: false).sourceCodes;

    [TestInitialize]
    public
     void Setup()
    {
        int v = EmberMethods.GetEmberApiVersion();
        facade = EmberMethods.GetNewScriptManagerInstance(v);
        em = new EmberMethods(facade);
        ActionResultVersionSpecific = "[Message contains either failure or succes: ] ";  //change this if action result version changes it will thraow cause of message contains
    }


    [TestMethod]
    public async Task RecentEmberVersionTestAsync()
    {
        int v = EmberMethods.GetEmberApiVersion();

        facade = EmberMethods.GetNewScriptManagerInstance(v);
        em = new EmberMethods(facade);
        await facade!.EnsureDeletedCreated();
        await ExecuteEachScript(facade, em);
        int apiVersionInsideFacade = facade.GetRunningApiVersion();
        Assert.IsTrue(apiVersionInsideFacade == v);
        Assert.IsTrue(apiVersionInsideFacade == 5);

        facade = EmberMethods.GetNewScriptManagerInstance(1);
        em = new EmberMethods(facade);
        await facade!.EnsureDeletedCreated();
        await ExecuteEachScript(facade, em);
        apiVersionInsideFacade = facade.GetRunningApiVersion();
        Assert.IsTrue(apiVersionInsideFacade == EmberMethods.GetEmberApiVersion(testingDiffrentVersion: 1));
        Assert.IsTrue(apiVersionInsideFacade == 1);
    }

    public void OldEmberVersionTest()
    {

    }
    public async Task<List<Guid>> SaturateDBAsync(ISccriptManagerDeleteAfter facade, EmberMethods rm)
    {
        List<Guid> ids = [];
        foreach (var item in sourceCodes!)
        {
            Guid id = await facade!.CreateScript(sourceCodeActionV1!);
            ids.Add(id);
        }
        return ids;
    }
    public async Task ExecuteEachScript(ISccriptManagerDeleteAfter facade, EmberMethods em)
    {
        foreach (var id in await SaturateDBAsync(facade, em))
        {
            CustomerScript retrievedScript = await facade!.GetScript(id);
            var context = await em!.GetTestingContext<GeneratorContextNoInherVaccineV5.GeneratorContext>(autoDetectFromScript: retrievedScript);
            object resultBeforeUpgrade = await facade.ExecuteScriptById(id, context);   //here somehow figure out how to get the version that is being executed todo

            if (resultBeforeUpgrade is ActionResultBaseClass)
            {
                ActionResultV3.ActionResult result = (ActionResultV3.ActionResult)EmberMethods.UpgradeActionResult(resultBeforeUpgrade);
                string shouldReturn = ActionResultVersionSpecific + "Pediatric tests added";
                Assert.IsInstanceOfType(result, typeof(ActionResultBaseClass));
                Assert.IsInstanceOfType(result, typeof(ActionResultV3.ActionResult));
                Assert.IsTrue(result.ToString().Contains(shouldReturn));
            }
            else
            {
                object result = await facade.ExecuteScriptById(id, context);
                string shouldReturn = "True";
                Assert.IsInstanceOfType(result, typeof(bool));
                Assert.IsTrue(result.ToString()!.Contains(shouldReturn));
            }
        }
    }

    [TestMethod]
    public async Task GetCachesForEachApiVersionTestsAsync()
    {
        int v = EmberMethods.GetEmberApiVersion();

        facade = EmberMethods.GetNewScriptManagerInstance(v);
        em = new EmberMethods(facade);
        await facade!.EnsureDeletedCreated();
        await ExecuteEachScript(facade, em);
        int apiVersionInsideFacade = facade.GetRunningApiVersion();

        var versionDict = await facade!.GetCachesForEachApiVersion();
        foreach (var item in versionDict)
        {
            Console.WriteLine("Key: " + item.Key);
            foreach (var cache in item.Value)
            {
                Console.WriteLine(cache.CustomerScript!.ScriptName);
            }
        }

        Assert.IsTrue(apiVersionInsideFacade == v);
        Assert.IsTrue(apiVersionInsideFacade == 5);

        facade = EmberMethods.GetNewScriptManagerInstance(1);
        em = new EmberMethods(facade);
        await ExecuteEachScript(facade, em);
        apiVersionInsideFacade = facade.GetRunningApiVersion();
        Assert.IsTrue(apiVersionInsideFacade == EmberMethods.GetEmberApiVersion(testingDiffrentVersion: 1));
        Assert.IsTrue(apiVersionInsideFacade == 1);

        var versionDict2 = await facade!.GetCachesForEachApiVersion();
        foreach (var item in versionDict2)
        {
            Console.WriteLine("Key: " + item.Key);
            foreach (var cache in item.Value)
            {
                Console.WriteLine(cache.CustomerScript!.ScriptName);
            }
        }
        Assert.IsTrue(versionDict2[1].Count() == 4);
        Assert.IsTrue(versionDict2[5].Count() == 4);

    }

    [TestMethod]
    public async Task OldSourceCodeVersionsTest()
    {
        facade = EmberMethods.GetNewScriptManagerInstance(1);
        Guid id = await facade!.CreateScript(sourceCodeActionV1!);

        facade = EmberMethods.GetNewScriptManagerInstance(2);
        await facade.UpdateScript(id, sourceCodeActionV2!);
        await facade.CompileScript(id);

        string sourceCodeAV = (await facade.GetCompiledCache(id)).OldSourceCode!;
        string sourceCodeV1 = (await facade.GetCompiledCache(id, 1)).OldSourceCode!;
        string sourceCodeV2 = (await facade.GetCompiledCache(id, 2)).OldSourceCode!;

        Assert.IsTrue(sourceCodeAV == sourceCodeActionV2);
        Assert.IsTrue(sourceCodeV1 == sourceCodeActionV1);
        Assert.IsTrue(sourceCodeAV == sourceCodeActionV2);
        Assert.IsTrue(sourceCodeAV == sourceCodeV2);
        Assert.IsFalse(sourceCodeAV == sourceCodeV1);
        // Console.WriteLine(sourceCodeAV);
        // Console.WriteLine(sourceCodeV1);
        // Assert.IsTrue(false);
    }

    [TestMethod]
    public void ScriptVersionScannerTest()
    {
        Dictionary<int, Type> contextVersionMap = new()
        {
            {1, typeof(GeneratorScriptsGenericSimple.IGeneratorConditionScript<>)},
            {2, typeof(GeneratorScriptsV2.IGeneratorActionScript)},
            {3, typeof(GeneratorScriptsV3.IGeneratorActionScript)},
            {4, typeof(GeneratorScriptsV4.IGeneratorActionScript)},
        };

        Dictionary<int, Type> retrievedDict = ScriptVersionScanner.GetClassDictionary();
        PrintDictToConsole(retrievedDict);


        CollectionAssert.AreEquivalent(contextVersionMap, retrievedDict);
        // Assert.IsTrue(false);
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
        // Assert.IsTrue(false);
    }

    [TestMethod]
    public void ContextVersionScannerTest()
    {
        Dictionary<int, Type> contextVersionMap = new()
        {
            {1, typeof(ReadOnlyContextV1.GeneratorContext)},
            {2, typeof(RWContextV2.GeneratorContext)},
            {3, typeof(GeneratorContextV3.GeneratorContext)},
            {4, typeof(GeneratorContextV4.GeneratorContext)},
            {5, typeof(GeneratorContextNoInherVaccineV5.GeneratorContext)},
        };

        Dictionary<int, Type> retrievedDict = ContextVersionScanner.GetClassDictionary();
        PrintDictToConsole(retrievedDict);


        CollectionAssert.AreEquivalent(contextVersionMap, retrievedDict);
        // Assert.IsTrue(false);
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
    public async Task CreateUsingDataTestAsync()
    {
        MockData data = new MockData();
        GeneratorContext ctx;
        Guid id;
        id = await facade!.CreateScript(sourceCodeActionV2!);
        var vali = facade.BasicValidationBeforeCompiling(sourceCodeActionV2!);
        int desiredContextVersion = vali.versionInt;

        Console.WriteLine(nameof(desiredContextVersion) + ": " + desiredContextVersion);

        await Assert.ThrowsExceptionAsync<Exception>(async () =>
        {
            ctx = ContextFactory.CreateContext(desiredContextVersion, data);
        });
        ScriptFactory sf = new ScriptFactory(facade);
        var obj = sf.ScriptObjects();

        data = new MockData(labOrder: obj.labOrder, patient: obj.patient, consoleLogger: obj.logger,
        dataAccess: obj.testDataAccess, vaccine: obj.vaccine);

        ctx = ContextFactory.CreateContext(desiredContextVersion, data);
        var result1 = await facade.ExecuteScriptById(id, ctx);

        var result = EmberMethods.UpgradeActionResult(result1);

        string shouldReturn = ActionResultVersionSpecific + "Pediatric tests added";
        Assert.IsInstanceOfType(result, typeof(ActionResultBaseClass));
        Assert.IsInstanceOfType(result, typeof(ActionResultV3.ActionResult));
        Assert.IsTrue(result.ToString()!.Contains(shouldReturn));

        // Assert.IsTrue(false);
    }

    public async Task GetTestingContextTest()
    {
        GeneratorContext ctx;
        Guid id;
        id = await facade!.CreateScript(sourceCodeActionV2!);
        var vali = facade.BasicValidationBeforeCompiling(sourceCodeActionV2!);
        int desiredContextVersion = vali.versionInt;

        await Assert.ThrowsExceptionAsync<Exception>(async () =>
        {
            ctx = await em!.GetTestingContext<GeneratorContextV3.GeneratorContext>();
            var result1 = await facade.ExecuteScriptById(id, ctx);
        });

        ctx = await em!.GetTestingContext<RWContextV2.GeneratorContext>();
        var result1 = await facade.ExecuteScriptById(id, ctx);
        var result = EmberMethods.UpgradeActionResult(result1);

        string shouldReturn = ActionResultVersionSpecific + "Pediatric tests added";
        Assert.IsInstanceOfType(result, typeof(ActionResultBaseClass));
        Assert.IsInstanceOfType(result, typeof(ActionResultV3.ActionResult));
        Assert.IsTrue(result.ToString()!.Contains(shouldReturn));
    }
}
