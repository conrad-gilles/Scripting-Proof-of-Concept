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
        logger = new LoggerForScripting();
        Log.Debug("Sandbox launched.");

        services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            // Assuming you use Serilog, this forwards standard MS Logging to Serilog
            builder.AddSerilog(dispose: true);
        });

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
        ScriptingServiceCollectionExtensions.AddEmberScripting(services!, RandomMethods.GetReferences(), v);

        using var provider = services!.BuildServiceProvider();

        facade = provider.GetRequiredService<ISccriptManagerDeleteAfter>();
        rm = new RandomMethods(facade);
        await facade!.EnsureDeletedCreated();
        await ExecuteEachScript(facade, rm);
        int apiVersionInsideFacade = await facade.GetRecentApiVersion();
        Assert.IsTrue(apiVersionInsideFacade == v);
        Assert.IsTrue(apiVersionInsideFacade == 6);

        ServiceCollection services2 = new ServiceCollection();

        logger = new LoggerForScripting();
        Log.Debug("Sandbox launched.");

        services2 = new ServiceCollection();
        services2.AddLogging(builder =>
        {
            builder.AddSerilog(dispose: true);
        });

        ScriptingServiceCollectionExtensions.AddEmberScripting(services2, RandomMethods.GetReferences(), RandomMethods.GetEmberApiVersion(testingDiffrentVersion: 1));

        using var provider2 = services2.BuildServiceProvider();

        facade = provider2.GetRequiredService<ISccriptManagerDeleteAfter>();
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
        ScriptingServiceCollectionExtensions.AddEmberScripting(services!, RandomMethods.GetReferences(), v);

        using var provider = services!.BuildServiceProvider();

        facade = provider.GetRequiredService<ISccriptManagerDeleteAfter>();
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

        ServiceCollection services2 = new ServiceCollection();

        logger = new LoggerForScripting();
        Log.Debug("Sandbox launched.");

        services2 = new ServiceCollection();
        services2.AddLogging(builder =>
        {
            builder.AddSerilog(dispose: true);
        });

        ScriptingServiceCollectionExtensions.AddEmberScripting(services2, RandomMethods.GetReferences(), RandomMethods.GetEmberApiVersion(testingDiffrentVersion: 1));

        using var provider2 = services2.BuildServiceProvider();

        facade = provider2.GetRequiredService<ISccriptManagerDeleteAfter>();
        rm = new RandomMethods(facade);
        // await facade!.EnsureDeletedCreated();
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
}
