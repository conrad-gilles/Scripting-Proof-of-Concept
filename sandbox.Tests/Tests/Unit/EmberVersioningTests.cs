using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.SolutionPersistence.Serializer.SlnV12;
using System.Runtime.CompilerServices;
using Ember.Simulation;

[TestClass]
public class EmberVersioningTests
{


    private IScriptManagerDeleteAfter? _facade;
    private EmberMethods? _em;
    private DataV2.DataV2? _data;
    private string? _actionResultVersionSpecific;
    private string? _sourceCodeActionV1 = TestHelper.GetSC().sourceCodeActionV1;
    private string? _sourceCodeActionV2 = TestHelper.GetSC().sourceCodeActionV2;
    private string? _sourceCodeActionV3 = TestHelper.GetSC().sourceCodeActionV3;
    private string? _sourceCodeVaccineAction = TestHelper.GetSC().sourceCodeVaccineAction;
    private string? _sourceCodePedia = TestHelper.GetSC().sourceCodePedia;
    // private LoggerForScripting? logger;
    private List<string>? _sourceCodes = TestHelper.GetSC(includeCondInList: false).sourceCodes;

    [TestInitialize]
    public
     async Task Setup()
    {
        int v = EmberMethods.GetEmberApiVersion();
        _facade = EmberMethods.GetNewScriptManagerInstance(v);
        _em = new EmberMethods(_facade);
        await _facade.DeleteAllData();
        _actionResultVersionSpecific = "[Message contains either failure or succes: ] ";  //change this if action result version changes it will thraow cause of message contains
        var obj = TestHelper.ScriptObjects();
        _data = new DataV2.DataV2(labOrder: obj.labOrder, patient: obj.patient, consoleLogger: obj.logger,
                                dataAccess: obj.testDataAccess, vaccine: obj.vaccine);
    }


    [TestMethod]
    public async Task RecentEmberVersionTestAsync()
    {
        int v = EmberMethods.GetEmberApiVersion();

        _facade = EmberMethods.GetNewScriptManagerInstance(v);
        _em = new EmberMethods(_facade);
        // await facade!.EnsureDeletedCreated();
        await _facade!.DeleteAllData();
        await ExecuteEachScript(_facade, _em);
        int apiVersionInsideFacade = _facade.GetRunningApiVersion();
        Assert.IsTrue(apiVersionInsideFacade == v);
        Assert.IsTrue(apiVersionInsideFacade == 5);

        _facade = EmberMethods.GetNewScriptManagerInstance(1);
        _em = new EmberMethods(_facade);
        // await facade!.EnsureDeletedCreated();
        await _facade!.DeleteAllData();
        await ExecuteEachScript(_facade, _em);
        apiVersionInsideFacade = _facade.GetRunningApiVersion();
        Assert.IsTrue(apiVersionInsideFacade == EmberMethods.GetEmberApiVersion(testingDiffrentVersion: 1));
        Assert.IsTrue(apiVersionInsideFacade == 1);
    }

    public void OldEmberVersionTest()
    {

    }
    public async Task<List<Guid>> SaturateDBAsync(IScriptManagerDeleteAfter facade, EmberMethods rm)
    {
        List<Guid> ids = [];
        foreach (var item in _sourceCodes!)
        {
            Guid id = (await facade!.CreateScript(item!)).Id;
            ids.Add(id);
        }
        return ids;
    }
    public async Task ExecuteEachScript(IScriptManagerDeleteAfter facade, EmberMethods em)
    {
        foreach (var id in await SaturateDBAsync(facade, em))
        {
            CustomerScript retrievedScript = await facade!.GetScript(id);
            var context = await em!.GetTestingContext<GeneratorContextNoInherVaccineV5.GeneratorContext>(autoDetectFromScript: retrievedScript);
            object resultBeforeUpgrade = await facade.ExecuteScriptById(id, context);   //here somehow figure out how to get the version that is being executed todo

            if (resultBeforeUpgrade is ActionResultSF)
            {
                ActionResultV3.ActionResult result = (ActionResultV3.ActionResult)EmberMethods.UpgradeActionResult(resultBeforeUpgrade);
                string shouldReturn = _actionResultVersionSpecific + "Pediatric tests added";
                Assert.IsInstanceOfType(result, typeof(ActionResultSF));
                Assert.IsInstanceOfType(result, typeof(ActionResultV3.ActionResult));
                Console.WriteLine(result.ToString());
                Assert.IsTrue(result.ToString().Contains(shouldReturn)
                || result.ToString().Contains(shouldReturn)
                || result.ToString().Contains("Polio Vaccine added")
                );
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

        _facade = EmberMethods.GetNewScriptManagerInstance(v);
        _em = new EmberMethods(_facade);
        // await facade!.EnsureDeletedCreated();
        await _facade!.DeleteAllData();
        await ExecuteEachScript(_facade, _em);
        int apiVersionInsideFacade = _facade.GetRunningApiVersion();

        var versionDict = await _facade!.GetCachesForEachApiVersion();
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

        _facade = EmberMethods.GetNewScriptManagerInstance(1);
        _em = new EmberMethods(_facade);
        await _facade.DeleteAllData();
        await ExecuteEachScript(_facade, _em);
        apiVersionInsideFacade = _facade.GetRunningApiVersion();
        Assert.IsTrue(apiVersionInsideFacade == EmberMethods.GetEmberApiVersion(testingDiffrentVersion: 1));
        Assert.IsTrue(apiVersionInsideFacade == 1);

        var versionDict2 = await _facade!.GetCachesForEachApiVersion();
        foreach (var item in versionDict2)
        {
            Console.WriteLine("Key: " + item.Key);
            foreach (var cache in item.Value)
            {
                Console.WriteLine(cache.CustomerScript!.ScriptName);
            }
        }
        Assert.IsTrue(versionDict2[1].Count() == 4);
        Console.WriteLine("Count: " + versionDict.Keys.Count());
        // Assert.IsTrue(versionDict2[4].Count() == 4);

    }

    [TestMethod]
    public async Task OldSourceCodeVersionsTest()
    {
        _facade = EmberMethods.GetNewScriptManagerInstance(1);
        Guid id = (await _facade!.CreateScript(_sourceCodeActionV1!)).Id;

        _facade = EmberMethods.GetNewScriptManagerInstance(2);
        await _facade.UpdateScriptSC(id, _sourceCodeActionV2!);
        await _facade.CompileScript(id);

        string sourceCodeAV = (await _facade.GetCompiledCache(id)).OldSourceCode!;
        string sourceCodeV1 = (await _facade.GetCompiledCache(id, 1)).OldSourceCode!;
        string sourceCodeV2 = (await _facade.GetCompiledCache(id, 2)).OldSourceCode!;

        Assert.IsTrue(sourceCodeAV == _sourceCodeActionV2);
        Assert.IsTrue(sourceCodeV1 == _sourceCodeActionV1);
        Assert.IsTrue(sourceCodeAV == _sourceCodeActionV2);
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
            {1, typeof(GeneratorScriptsGenericSimple.IConditionScript<>)},
            {2, typeof(GeneratorScriptsV2.IActionScript)},
            {3, typeof(GeneratorScriptsV3.IActionScript)},
            {4, typeof(GeneratorScriptsV4.IActionScript)},
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
        _data = new DataV2.DataV2();
        Ember.Scripting.Context ctx;  //refactor to ContextSF
        Guid id;
        id = (await _facade!.CreateScript(_sourceCodeActionV2!)).Id;
        var vali = _facade.BasicValidationBeforeCompiling(_sourceCodeActionV2!);
        int desiredContextVersion = vali.Version;

        Console.WriteLine(nameof(desiredContextVersion) + ": " + desiredContextVersion);

        await Assert.ThrowsExceptionAsync<Exception>(async () =>
        {
            ctx = Sandbox.ContextManagementDemos.CreateUsingData(desiredContextVersion, _data!);
        });
        ContextManagement sf = new ContextManagement(_facade);
        var obj = TestHelper.ScriptObjects();

        _data = new DataV2.DataV2(labOrder: obj.labOrder, patient: obj.patient, consoleLogger: obj.logger,
                                dataAccess: obj.testDataAccess, vaccine: obj.vaccine);

        ctx = Sandbox.ContextManagementDemos.CreateUsingData(desiredContextVersion, _data!);
        var result1 = await _facade.ExecuteScriptById(id, ctx);

        var result = EmberMethods.UpgradeActionResult(result1);

        // GeneratorContext ctx=GeneratorContextFactory.Create(,data);  
        // ActionResult result= await facade.executescript<GeneratorContex,ActionResult>(ctx);
        // ActionResult result= await facade.executescript<GeneratorActionScript>(ctx);

        string shouldReturn = _actionResultVersionSpecific + "Pediatric tests added";
        Assert.IsInstanceOfType(result, typeof(ActionResultSF));
        Assert.IsInstanceOfType(result, typeof(ActionResultV3.ActionResult));
        Assert.IsInstanceOfType(result, typeof(RecentActionResult));
        Assert.IsTrue(result.ToString()!.Contains(shouldReturn));

        // Assert.IsTrue(false);
    }

    public async Task GetTestingContextTest()
    {
        Ember.Scripting.Context ctx;
        Guid id;
        id = (await _facade!.CreateScript(_sourceCodeActionV2!)).Id;
        var vali = _facade.BasicValidationBeforeCompiling(_sourceCodeActionV2!);
        int desiredContextVersion = vali.Version;

        await Assert.ThrowsExceptionAsync<Exception>(async () =>
        {
            ctx = await _em!.GetTestingContext<GeneratorContextV3.GeneratorContext>();
            var result1 = await _facade.ExecuteScriptById(id, ctx);
        });

        ctx = await _em!.GetTestingContext<RWContextV2.GeneratorContext>();
        var result1 = await _facade.ExecuteScriptById(id, ctx);
        var result = EmberMethods.UpgradeActionResult(result1);

        string shouldReturn = _actionResultVersionSpecific + "Pediatric tests added";
        Assert.IsInstanceOfType(result, typeof(ActionResultSF));
        Assert.IsInstanceOfType(result, typeof(ActionResultV3.ActionResult));
        Assert.IsTrue(result.ToString()!.Contains(shouldReturn));
    }
}
