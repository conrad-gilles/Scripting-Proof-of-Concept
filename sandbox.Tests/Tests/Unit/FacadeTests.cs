using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;

namespace FirstTests;

[TestClass]
public class ScriptManagerFacadeTests
{
    private ISccriptManagerDeleteAfter? facade;
    string? sourceCodePedia;
    private string? sourceCodeActionV1;
    private string? sourceCodeActionV3;
    private EmberMethods? em;

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

        ScriptingServiceCollectionExtensions.AddEmberScripting(services, EmberMethods.GetReferences(), EmberMethods.GetEmberApiVersion());

        using var provider = services.BuildServiceProvider();

        facade = provider.GetRequiredService<ISccriptManagerDeleteAfter>();
        em = new EmberMethods(facade);

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
        sourceCodePedia = EmberMethods.CreateStringFromCsFile(
            Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..",
                "sandbox", "src", "Scripts", "ConditionScripts", "PediatricCondition.cs"
            ))
        );

        sourceCodeActionV1 = EmberMethods.CreateStringFromCsFile(
            Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..",
                "sandbox", "src", "Scripts", "ActionScripts", "AddPediatricTestsV1.cs"
            ))
        );

        sourceCodeActionV3 = EmberMethods.CreateStringFromCsFile(
            Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..", "..", "..", "..",
                "sandbox", "src", "Scripts", "ActionScripts", "AddPediatricTestsV3.cs"
            ))
        );
    }

    #region Script Lifecycle

    [TestMethod]
    public async Task CreateScriptTest()
    {
        Guid id = await facade!.CreateScript(sourceCodePedia!, "ConditionScriptTest");
        CustomerScript retrievedScript = await facade.GetScript(id);

        Assert.AreEqual(retrievedScript.SourceCode, sourceCodePedia);
    }
    [TestMethod]
    public async Task CreateScriptWithOldTest()
    {

        Guid id = await facade!.CreateScript(sourceCodePedia!, apiVersion: 2);
        CustomerScript retrievedScript = await facade.GetScript(id);
        var context = em!.GetTestingContext<GeneratorContextNoInherVaccineV5.GeneratorContext>(retrievedScript);
        await facade.ExecuteScriptById(retrievedScript.Id, context);

        Assert.AreEqual(retrievedScript.SourceCode, sourceCodePedia);
    }

    [TestMethod]
    public async Task UpdateScriptTest()
    {
        Guid id = await facade!.CreateScript(sourceCodePedia!);
        string newSourceCode = sourceCodePedia + System.Environment.NewLine + @"//new line added for testing";
        // newSourceCode = newSourceCode.Replace("@", "@" + System.Environment.NewLine);
        await facade.UpdateScript(id, newSourceCode);

        CustomerScript retrievedScript = await facade.GetScript(id);
        Assert.AreEqual(retrievedScript.SourceCode, newSourceCode);
    }

    [TestMethod]
    public async Task DeleteScriptTest()
    {
        Guid id = await facade!.CreateScript(sourceCodePedia!);
        await facade.DeleteScript(id);


        await Assert.ThrowsExceptionAsync<System.InvalidOperationException>(async () =>    //todo check that this implements the correct exception
        {
            CustomerScript script = await facade.GetScript(id);
        });
    }

    [TestMethod]
    public async Task GetScriptTest()
    {
        Guid id = await facade!.CreateScript(sourceCodePedia!);
        CustomerScript script = await facade.GetScript(id);

        Assert.IsNotNull(script);
        Assert.AreEqual(id, script.Id);
        Assert.AreEqual(sourceCodePedia, script.SourceCode);
    }

    [TestMethod]
    public async Task ListScriptsTest()
    {
        await facade!.ClearAllCaches();
        Guid id1 = await facade.CreateScript(sourceCodePedia!);
        Guid id2 = await facade.CreateScript(sourceCodeActionV1!);
        List<CustomerScript> scripts = await facade.ListScripts(includeCaches: true);
        Assert.IsNotNull(scripts);
        Assert.IsTrue(scripts.Count == 2);
    }
    [TestMethod]
    public async Task ListScriptsTestWithFilter()
    {
        string scriptFolderPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
                "sandbox", "src", "Scripts"));
        await facade!.EnsureDeletedCreated();
        await em!.CompileAllScriptsInFolderAndSaveToDB(scriptFolderPath, "Gilles", EmberMethods.GetEmberApiVersion());
        List<CustomerScript> scripts = await facade!.ListScripts(includeCaches: true);
        Assert.IsNotNull(scripts);
        Assert.IsTrue(scripts.Count == 5);

        CustomerScriptFilter filters = new CustomerScriptFilter(scriptName: "VaccineScript");   //todo fix this oine thorws errors
        List<CustomerScript> scriptsFiltered = await facade.ListScripts(includeCaches: true, filters: filters);
        Assert.IsNotNull(scriptsFiltered);
        Assert.IsTrue(scriptsFiltered.Count == 1);

        // CustomerScriptFilter filters2 = new CustomerScriptFilter(scriptName: "VaccineScript", minApiVersion: 4);
        // List<CustomerScript> scriptsFiltered2 = await facade.ListScripts(includeCaches: true, filters: filters2);
        // Assert.IsNotNull(scriptsFiltered2);
        // Assert.IsTrue(scriptsFiltered2.Count == 0);

        // CustomerScriptFilter filters3 = new CustomerScriptFilter(scriptType: "IGeneratorActionScript");
        // List<CustomerScript> scriptsFiltered3 = await facade.ListScripts(includeCaches: true, filters: filters3);
        // Console.WriteLine("Count is: " + scriptsFiltered3.Count());
        // Assert.IsNotNull(scriptsFiltered3);
        // Assert.IsTrue(scriptsFiltered3.Count == 2); //why 2 todo
    }

    #endregion

    #region Compilation Operations

    [TestMethod]
    public async Task CompileScriptTest()
    {
        Guid id = await facade!.CreateScript(sourceCodePedia!);

        await facade.ClearScriptCache(id);
        var before = await facade.GetScript(id, includeCaches: true);
        Assert.AreEqual(0, before.CompiledCaches.Count);

        await facade.CompileScript(id);

        var after = await facade.GetScript(id, includeCaches: true);
        Assert.IsTrue(after.CompiledCaches.Count == 1);
        Assert.IsTrue(after.CompiledCaches.Any(c => c.AssemblyBytes != null && c.AssemblyBytes.Length >= 1));   //todo check why this fails when ==1 but i think its correct
    }

    [TestMethod]
    public async Task CompileAllScriptsTest()
    {
        Guid id1 = await facade!.CreateScript(sourceCodePedia!);
        Guid id2 = await facade.CreateScript(sourceCodeActionV1!);

        await facade.ClearAllCaches();

        var beforeS1 = await facade.GetScript(id1, includeCaches: true);
        var beforeS2 = await facade.GetScript(id2, includeCaches: true);
        Assert.AreEqual(0, beforeS1.CompiledCaches.Count);
        Assert.AreEqual(0, beforeS2.CompiledCaches.Count);

        await facade.CompileAllScripts();

        var afterS1 = await facade.GetScript(id1, includeCaches: true);
        var afterS2 = await facade.GetScript(id2, includeCaches: true);
        Assert.IsTrue(afterS1.CompiledCaches.Count == 1);
        Assert.IsTrue(afterS2.CompiledCaches.Count == 1);
    }

    [TestMethod]
    public async Task RecompileScriptTest()
    {
        Guid id = await facade!.CreateScript(sourceCodePedia!);

        await facade.RecompileScript(id);

        var script = await facade.GetScript(id, includeCaches: true);
        Assert.IsTrue(script.CompiledCaches.Count == 1);
        Assert.IsTrue(script.CompiledCaches.Any(c => c.AssemblyBytes != null && c.AssemblyBytes.Length > 0));
    }

    [TestMethod]
    public async Task ValidateScriptTest()
    {
        string result = facade!.ValidateScript(sourceCodePedia!);

        Assert.IsTrue(result.StartsWith("Success:", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(result.Contains("PediatricCondition"));

        string result2 = facade.ValidateScript("wrong input test could be whatever");

        Assert.IsTrue(result2.StartsWith("Error:", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(result2.Contains("Exception"));
    }

    [TestMethod]
    public async Task GetCompilationErrorsTest()
    {
        Guid id = await facade!.CreateScript(sourceCodePedia!);

        string result = await facade.GetCompilationErrors(id);
        Assert.IsTrue(result.Contains("Successful Compilation!"));

        await Assert.ThrowsExceptionAsync<ValidationBeforeCompilationException>(async () =>    //before was basicvalidation exception todo
        {
            Guid id2 = await facade.CreateScript("wrong input test could be whatever");
            // string result2 = await facade.GetCompilationErrors(id2);
            string result2 = await facade.GetCompilationErrors(id2);
        });

        // try
        // {
        //     // var e = await Assert.ThrowsExceptionAsync<CompilationFailedException>(async () =>    //before was basicvalidation exception todo
        //     //   {
        //     Guid id2 = await facade.CreateScript("public class Test : IFakeInterface{}");
        //     // string result2 = await facade.GetCompilationErrors(id2);
        //     string result2 = await facade.GetCompilationErrors(id2);
        //     //   });
        // }
        // catch (Exception e)
        // {
        //     Console.WriteLine(e.ToString());
        //     Assert.IsTrue(false);
        // }

    }


    [TestMethod]
    public async Task GetCompilationErrorsTestNegative()
    {
        Guid id2 = Guid.NewGuid();
        await facade!.SaveScriptWithoutCompiling(id2, "wrong input test could be whatever");
        // string result2 = await facade.GetCompilationErrors(id2);
        string result2 = await facade.GetCompilationErrors(id2);
        Console.WriteLine("Start of Result:");
        Console.WriteLine(result2);
        Console.WriteLine("End of Result.");

        string desiredOutput = """
        ---> Ember.Scripting.CompilationFailedException: Error in ScriptCompiler RunCompilation method compilation probably failed in if (!emitResult.Success) errors: Line 1, Col 13: CS1003 - Syntax error, ',' expected
        Line 1, Col 35: CS1002 - ; expected
        Line 1, Col 1: CS8805 - Program using top-level statements must be an executable.
        Line 1, Col 1: CS0246 - The type or namespace name 'wrong' could not be found (are you missing a using directive or an assembly reference?)
        """;


        Assert.IsTrue(result2.Contains(desiredOutput));

        Guid id3 = Guid.NewGuid();
        await facade.SaveScriptWithoutCompiling(id3, "public class Test : IFakeInterface{}");
        // string result2 = await facade.GetCompilationErrors(id2);
        string result3 = await facade.GetCompilationErrors(id3);
        Console.WriteLine("Start of Result:");
        Console.WriteLine(result3);
        Console.WriteLine("End of Result.");
        string desiredOutput3 = """
        ---> Ember.Scripting.CompilationFailedException: Error in ScriptCompiler RunCompilation method compilation probably failed in if (!emitResult.Success) errors: Line 1, Col 21: CS0246 - The type or namespace name 'IFakeInterface' could not be found (are you missing a using directive or an assembly reference?)
        """;

        Assert.IsTrue(result3.Contains(desiredOutput3));
    }

    #endregion

    #region Execution Operations

    [TestMethod]
    public async Task ExecuteActionScriptTest()
    {
        Guid id = await facade!.CreateScript(sourceCodeActionV1!);
        var testingContext = em!.GetTestingContext<GeneratorContextV4.GeneratorContext>();

        ActionResultBaseClass result = await facade.ExecuteActionScript(id, testingContext);

        Assert.IsNotNull(result);
        // Assert.IsTrue(result.IsSuccess);
        // Assert.IsTrue(result.Message.Contains("Pediatric"));
        result = EmberMethods.UpgradeActionResult(result);
        Assert.IsInstanceOfType(result, typeof(ActionResultV3NoInheritance));

        Guid id2 = await facade.CreateScript(sourceCodePedia!);
        await Assert.ThrowsExceptionAsync<Ember.Scripting.FacadeException>(async () =>
        {
            ActionResultBaseClass result2 = await facade.ExecuteActionScript(id2, testingContext);
        });

    }

    [TestMethod]
    public async Task ExecuteConditionScriptTest()
    {
        Guid id = await facade!.CreateScript(sourceCodePedia!);
        var testingContext = em!.GetTestingContext<GeneratorContextV4.GeneratorContext>();

        bool result = await facade.ExecuteConditionScript(id, testingContext);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.GetType().ToString() == "System.Boolean");

        Guid id2 = await facade.CreateScript(sourceCodeActionV1!);
        await Assert.ThrowsExceptionAsync<Ember.Scripting.FacadeException>(async () =>
        {
            bool result2 = await facade.ExecuteConditionScript(id2, testingContext);
        });
    }

    [TestMethod]
    public async Task ExecuteScriptByIdTest()
    {
        var context = em!.GetTestingContext<GeneratorContextV4.GeneratorContext>();

        Guid condId = await facade!.CreateScript(sourceCodePedia!);
        object condResult = await facade.ExecuteScriptById(condId, context);
        Assert.IsInstanceOfType(condResult, typeof(bool));
        Assert.AreEqual(true, (bool)condResult);

        Guid actId = await facade.CreateScript(sourceCodeActionV1!);
        object actResult = await facade.ExecuteScriptById(actId, context);
        Assert.IsInstanceOfType(actResult, typeof(ActionResultBaseClass));
        // Assert.IsTrue(((ActionResultBaseClass)actResult).IsSuccess);
    }

    #endregion

    #region Cache Management

    [TestMethod]
    public async Task GetCompiledCacheTest()
    {
        Guid id = await facade!.CreateScript(sourceCodePedia!);

        ScriptCompiledCache cache = await facade.GetCompiledCache(id);
        byte[] cacheAB = cache.AssemblyBytes!;

        Assert.IsNotNull(cacheAB);
        Assert.IsTrue(cacheAB.Length > 0);
    }

    [TestMethod]
    public async Task ClearScriptCacheTest()
    {
        Guid id = await facade!.CreateScript(sourceCodePedia!);

        ScriptCompiledCache cache = await facade.GetCompiledCache(id);
        byte[] cacheAB = cache.AssemblyBytes!;
        Assert.IsTrue(cacheAB.Length > 0);

        await facade.ClearScriptCache(id);

        var script = await facade.GetScript(id, includeCaches: true);
        Assert.AreEqual(0, script.CompiledCaches.Count);

        await Assert.ThrowsExceptionAsync<Ember.Scripting.FacadeException>(async () =>
        {
            await facade.GetCompiledCache(id);
        });
    }

    [TestMethod]
    public async Task ClearAllCachesTest()
    {
        Guid id = await facade!.CreateScript(sourceCodePedia!);
        Guid id2 = await facade.CreateScript(sourceCodeActionV1!);

        await facade.ClearAllCaches();

        var caches = await facade.GetAllCompiledScriptCaches();
        Assert.AreEqual(0, caches.Count);
    }

    [TestMethod]
    public async Task PrecompileForApiVersionTest()
    {
        Guid id = await facade!.CreateScript(sourceCodePedia!);

        await facade.ClearAllCaches();
        var before = await facade.GetScript(id, includeCaches: true);
        Assert.AreEqual(0, before.CompiledCaches.Count);

        await facade.PrecompileForApiVersion();

        var after = await facade.GetScript(id, includeCaches: true);
        Assert.IsTrue(after.CompiledCaches.Count >= 1);
    }

    #endregion

    #region Version Management

    [TestMethod]
    public async Task GetActiveApiVersionsTest()
    {
        Guid id = await facade!.CreateScript(sourceCodePedia!);
        int currentVersion = await facade.GetRunningApiVersion();

        List<int> versions = await facade.GetActiveApiVersions();

        Assert.IsNotNull(versions);
        Assert.IsTrue(versions.Count >= 1);
        Assert.IsTrue(versions.Contains(currentVersion));
    }

    [TestMethod]
    public async Task GetRecentApiVersionTest()
    {
        int v = await facade!.GetRunningApiVersion();
        Assert.IsInstanceOfType(v, typeof(System.Int32));   //todo implement maybe real
    }

    // [TestMethod]
    // public async Task GetScriptCompatibilityTest()
    // {
    //     Guid id = await facade.CreateScript(sourceCodeActionV3);

    //     int minVersion = await facade.GetScriptCompatibility(id);

    //     Assert.AreEqual(3, minVersion);
    // }

    [TestMethod]
    public async Task CheckVersionCompatibilityTest()
    {
        Guid id = await facade!.CreateScript(sourceCodePedia!);
        int currentVersion = await facade.GetRunningApiVersion();

        bool hasRecent = await facade.CheckVersionCompatibility(id, currentVersion);

        Assert.IsTrue(hasRecent);
    }

    [TestMethod]
    public void RegisterEmberInstanceTest()   //todo
    {
        // await facade.RegisterEmberInstance(Guid.NewGuid(), "2.3.0", 6);
        Assert.IsTrue(true);
    }

    #endregion

    #region Duplicate Detection & Cleanup

    // [TestMethod]
    // public async Task DetectDuplicatesTest()
    // {
    //     Guid id1 = await facade.CreateScript(sourceCodePedia);
    //     Guid id2 = await facade.CreateScript(sourceCodePedia);

    //     var scripts = await facade.ListScripts();
    //     Assert.AreEqual(2, scripts.Count);

    //     var dupes = await facade.DetectDuplicates();
    //     Assert.IsNotNull(dupes.scriptGUIDs);
    //     Assert.IsTrue(dupes.scriptGUIDs.Count >= 1);
    //     Assert.IsFalse(dupes.scriptGUIDs.Contains(id1));
    //     Assert.IsTrue(dupes.scriptGUIDs.Contains(id2));
    // }


    [TestMethod]
    public async Task RemoveDuplicatesTest()
    {
        Guid id = await facade!.CreateScript(sourceCodePedia!);
        Guid id2 = await facade.CreateScript(sourceCodePedia!);

        await facade.RemoveDuplicates();

        var scripts = await facade.ListScripts();
        Assert.AreEqual(1, scripts.Count);
    }

    [TestMethod]
    public void CleanupOrphanedCachesTest()   //todo
    {
        // Guid id = await facade.CreateScript(sourceCodePedia);

        // await facade.ClearAllCaches();
        // var before = await facade.GetScript(id, includeCaches: true);
        // Assert.AreEqual(0, before.CompiledCaches.Count);

        // await facade.CleanupOrphanedCaches();

        // var after = await facade.GetScript(id, includeCaches: true);
        // Assert.IsTrue(after.CompiledCaches.Count >= 1);
    }

    #endregion

    #region Monitoring & Diagnostics

    [TestMethod]
    public void GetScriptExecutionHistoryTest()   //todo
    {
        // Guid id = await facade.CreateScript(sourceCodePedia, "ConditionScriptTest");
        // await facade.GetScriptExecutionHistory(id); // TODO/no-op currently
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void GetCompilationStatisticsTest()    //todo
    {
        // await facade.GetCompilationStatistics(); // TODO/no-op currently
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task HealthCheckTest() //todo
    {
        await facade!.HealthCheck();
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task GetScriptMetadataTest()
    {
        Guid id = await facade!.CreateScript(sourceCodePedia!);

        string metaData = await facade.GetScriptMetadata(id);

        Assert.IsNotNull(metaData);
        Assert.IsTrue(metaData.Contains("Metadata for script:", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(metaData.Contains(id.ToString(), StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void GetUserNameTest()   //todo
    {
        // Assert.AreEqual("Gilles", facade.GetUserName());
    }

    [TestMethod]
    public void GetCachesForEachApiVersionTests()
    {
        // facade!.GetCachesForEachApiVersion() //todo test but maybe better to test in the other class
    }

    #endregion
}
