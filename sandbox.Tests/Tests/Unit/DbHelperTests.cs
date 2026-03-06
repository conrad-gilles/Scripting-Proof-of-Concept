using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;

namespace FirstTests;

[TestClass]
public class DbHelperTests
{
    private ISccriptManagerDeleteAfter? facade;
    string? sourceCodePedia;
    private string? sourceCodeActionV1;
    private string? sourceCodeActionV3;
    private RandomMethods? rm;

    [TestInitialize]
    public async Task Setup()
    {

        var logger = new LoggerForScripting();
        Log.Debug("Sandbox launched.");

        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            // Assuming you use Serilog, this forwards standard MS Logging to Serilog
            builder.AddSerilog(dispose: true);
        });

        ScriptingServiceCollectionExtensions.AddEmberScripting(services, RandomMethods.GetReferences(), RandomMethods.GetEmberApiVersion());

        using var provider = services.BuildServiceProvider();

        facade = provider.GetRequiredService<ISccriptManagerDeleteAfter>();
        rm = new RandomMethods(facade);

        // var logger = new LoggerForScripting();
        // // var microsoftLogger = logger.GetMicrosoftLogger<ScriptManagerFacade>();
        // ScriptCompiler compiler3 = new ScriptCompiler(RandomMethods.GetReferences(), logger.GetMicrosoftLogger<ScriptCompiler>());
        // ScriptExecutor exec3 = new ScriptExecutor(logger.GetMicrosoftLogger<ScriptExecutor>());
        // db = new DbHelper(compiler3, RandomMethods.GetReferences(), logger.GetMicrosoftLogger<DbHelper>());
        // rm = new RandomMethods(db);

        // facade = new ScriptManagerFacade(db, compiler3, exec3, RandomMethods.GetReferences(), logger.GetMicrosoftLogger<ScriptManagerFacade>());

        // Clear all data between tests without drop/recreate
        await facade.ClearAllCaches();
        var existing = await facade.ListScripts(includeCaches: false);
        foreach (var s in existing)
            await facade.DeleteScript(s.Id);

        // Load source files
        sourceCodePedia = RandomMethods.CreateStringFromCsFile(
            Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..",
                "sandbox", "src", "Scripts", "ConditionScripts", "PediatricCondition.cs"
            ))
        );

        sourceCodeActionV1 = RandomMethods.CreateStringFromCsFile(
            Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..",
                "sandbox", "src", "Scripts", "ActionScripts", "AddPediatricTestsV1.cs"
            ))
        );

        sourceCodeActionV3 = RandomMethods.CreateStringFromCsFile(
            Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..",
                "sandbox", "src", "Scripts", "ActionScripts", "AddPediatricTestsV3.cs"
            ))
        );
    }

    [TestMethod]
    public async Task CreateAndInsertCompiledCacheTest()
    {
        Guid id = await facade!.CreateScript(sourceCodePedia!);

        await facade.ClearScriptCache(id);
        var before = await facade.GetScript(id, includeCaches: true);
        Assert.AreEqual(0, before.CompiledCaches.Count);


        await facade.CompileScript(id);

        var after = await facade.GetScript(id, includeCaches: true);
        var getCache = await facade.GetCompiledCache(id, RandomMethods.GetEmberApiVersion());
        Assert.IsTrue(after.CompiledCaches.Count == 1);
        Assert.IsTrue(getCache != null && (getCache.ApiVersion == RandomMethods.GetEmberApiVersion()));
        Assert.IsTrue(after.CompiledCaches.Any(c => c.AssemblyBytes != null && c.AssemblyBytes.Length >= 1));

        var e = await Assert.ThrowsExceptionAsync<Ember.Scripting.DbHelperException>(async () =>
         {
             await facade.CompileScript(id, RandomMethods.GetEmberApiVersion());
             var after3 = await facade.GetScript(id, includeCaches: true);
             Assert.IsTrue(after3.CompiledCaches.Count == 1);
             Assert.IsTrue(after3.CompiledCaches.Any(c => c.AssemblyBytes != null && c.AssemblyBytes.Length >= 1));
         });
        Console.WriteLine(e.ToString());

        // Assert.IsFalse(true);
        int oldV = 3;
        await facade.CompileScript(id, oldV);

        var after2 = await facade.GetScript(id, includeCaches: true);
        ScriptCompiledCache cache = await facade.GetCompiledCache(id, oldV);
        Assert.IsTrue(after2.CompiledCaches.Count == 2);
        Console.WriteLine("Api Version = " + cache.ApiVersion);
        Assert.IsTrue(cache != null && (cache.ApiVersion == oldV));
        Assert.IsTrue(after2.CompiledCaches.Any(c => c.AssemblyBytes != null && c.AssemblyBytes.Length >= 1));
    }

    // [TestMethod]
    // public async Task OldSourceCodeVersionsTest()
    // {
    //     Guid id = await facade!.CreateScript(sourceCodePedia!);
    //     await facade.ClearScriptCache(id);


    // }

}
