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
    RandomMethods? rm;
    string? ActionResultVersionSpecific;
    string? sourceCodeActionV1;
    string? sourceCodeActionV2;
    string? sourceCodeActionV3;
    string? sourceCodeVaccineAction;
    string? sourceCodePedia;
    LoggerForScripting? logger;
    ServiceCollection? services;
    List<string>? sourceCodes;

    [TestInitialize]
    public
     void Setup()
    {
        ActionResultVersionSpecific = "[Message contains either failure or succes: ] ";  //change this if action result version changes it will thraow cause of message contains
        sourceCodeActionV1 = RandomMethods.CreateStringFromCsFile(
           Path.GetFullPath(Path.Combine(
               AppDomain.CurrentDomain.BaseDirectory,
               "..", "..", "..", "..",
               "sandbox", "src", "Scripts", "ActionScripts", "AddPediatricTestsV1.cs"
           ))
       );
        sourceCodeActionV2 = RandomMethods.CreateStringFromCsFile(
        Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "sandbox", "src", "Scripts", "ActionScripts", "AddPediatricTestsV2.cs"
        ))
    );
        sourceCodeActionV3 = RandomMethods.CreateStringFromCsFile(
       Path.GetFullPath(Path.Combine(
           AppDomain.CurrentDomain.BaseDirectory,
           "..", "..", "..", "..",
           "sandbox", "src", "Scripts", "ActionScripts", "AddPediatricTestsV3.cs"
       ))
   );
        sourceCodeVaccineAction = RandomMethods.CreateStringFromCsFile(
          Path.GetFullPath(Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory,
          "..", "..", "..", "..",
          "sandbox", "src", "Scripts", "ActionScripts", "VaccineScript.cs"
      ))
      );
        sourceCodePedia = RandomMethods.CreateStringFromCsFile(
       Path.GetFullPath(Path.Combine(
           AppDomain.CurrentDomain.BaseDirectory,
                   "..", "..", "..", "..",
                   "sandbox", "src", "Scripts", "ConditionScripts", "PediatricCondition.cs"
               ))
           );
        sourceCodes = [];
        sourceCodes!.Add(sourceCodeActionV1);
        sourceCodes!.Add(sourceCodeActionV2);
        sourceCodes!.Add(sourceCodeActionV3);
        sourceCodes!.Add(sourceCodeVaccineAction);
        // sourceCodes!.Add(sourceCodePedia);
    }


    [TestMethod]
    public async Task RecentEmberVersionTestAsync()
    {
        int v = RandomMethods.GetEmberApiVersion();

        facade = RandomMethods.GetNewScriptManagerInstance(v);
        rm = new RandomMethods(facade);
        await facade!.EnsureDeletedCreated();
        await ExecuteEachScript(facade, rm);
        int apiVersionInsideFacade = await facade.GetRecentApiVersion();
        Assert.IsTrue(apiVersionInsideFacade == v);
        Assert.IsTrue(apiVersionInsideFacade == 6);

        facade = RandomMethods.GetNewScriptManagerInstance(1);
        rm = new RandomMethods(facade);
        await facade!.EnsureDeletedCreated();
        await ExecuteEachScript(facade, rm);
        apiVersionInsideFacade = await facade.GetRecentApiVersion();
        Assert.IsTrue(apiVersionInsideFacade == RandomMethods.GetEmberApiVersion(testingDiffrentVersion: 1));
        Assert.IsTrue(apiVersionInsideFacade == 1);
    }

    public void OldEmberVersionTest()
    {

    }
    public async Task<List<Guid>> SaturateDBAsync(ISccriptManagerDeleteAfter facade, RandomMethods rm)
    {
        List<Guid> ids = [];
        foreach (var item in sourceCodes!)
        {
            Guid id = await facade!.CreateScript(sourceCodeActionV1!);
            ids.Add(id);
        }
        return ids;
    }
    public async Task ExecuteEachScript(ISccriptManagerDeleteAfter facade, RandomMethods rm)
    {
        foreach (var id in await SaturateDBAsync(facade, rm))
        {
            CustomerScript retrievedScript = await facade!.GetScript(id);
            var context = rm!.GetTestingContext<GeneratorContextNoInherVaccine.GeneratorContext>(justForTesting: retrievedScript);
            object resultBeforeUpgrade = await facade.ExecuteScriptById(id, context);   //here somehow figure out how to get the version that is being executed todo

            if (resultBeforeUpgrade is ActionResultBaseClass)
            {
                ActionResultV3NoInheritance result = RandomMethods.UpgradeActionResult(resultBeforeUpgrade);
                string shouldReturn = ActionResultVersionSpecific + "Pediatric tests added";
                Assert.IsInstanceOfType(result, typeof(ActionResultBaseClass));
                Assert.IsInstanceOfType(result, typeof(ActionResultV3NoInheritance));
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
        int v = RandomMethods.GetEmberApiVersion();

        facade = RandomMethods.GetNewScriptManagerInstance(v);
        rm = new RandomMethods(facade);
        await facade!.EnsureDeletedCreated();
        await ExecuteEachScript(facade, rm);
        int apiVersionInsideFacade = await facade.GetRecentApiVersion();

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
        Assert.IsTrue(apiVersionInsideFacade == 6);

        facade = RandomMethods.GetNewScriptManagerInstance(1);
        rm = new RandomMethods(facade);
        await ExecuteEachScript(facade, rm);
        apiVersionInsideFacade = await facade.GetRecentApiVersion();
        Assert.IsTrue(apiVersionInsideFacade == RandomMethods.GetEmberApiVersion(testingDiffrentVersion: 1));
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
        Assert.IsTrue(versionDict2[6].Count() == 4);

    }

    [TestMethod]
    public async Task OldSourceCodeVersionsTest()
    {
        facade = RandomMethods.GetNewScriptManagerInstance(1);
        Guid id = await facade!.CreateScript(sourceCodeActionV1!);

        facade = RandomMethods.GetNewScriptManagerInstance(2);
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
