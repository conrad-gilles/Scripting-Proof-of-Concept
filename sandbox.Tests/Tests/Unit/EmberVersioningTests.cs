using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
            var context = await em!.GetTestingContext<GeneratorContextNoInherVaccineV5.GeneratorContext>(justForTesting: retrievedScript);
            object resultBeforeUpgrade = await facade.ExecuteScriptById(id, context);   //here somehow figure out how to get the version that is being executed todo

            if (resultBeforeUpgrade is ActionResultBaseClass)
            {
                ActionResultV3.ActionResultV3NoInheritance result = (ActionResultV3.ActionResultV3NoInheritance)EmberMethods.UpgradeActionResult(resultBeforeUpgrade);
                string shouldReturn = ActionResultVersionSpecific + "Pediatric tests added";
                Assert.IsInstanceOfType(result, typeof(ActionResultBaseClass));
                Assert.IsInstanceOfType(result, typeof(ActionResultV3.ActionResultV3NoInheritance));
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
}
