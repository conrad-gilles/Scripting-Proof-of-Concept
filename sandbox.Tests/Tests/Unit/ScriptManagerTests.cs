using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using sandbox.Tests;
using Sandbox;

namespace FirstTests;

[TestClass]
public class ScriptManagerFacadeTests
{
    private ISccriptManagerDeleteAfter? _facade;
    string? _sourceCodePedia = TestHelper.GetSC().sourceCodePedia;
    private string? _sourceCodeActionV1 = TestHelper.GetSC().sourceCodeActionV1;
    private string? _sourceCodeActionV3 = TestHelper.GetSC().sourceCodeActionV3;
    private EmberMethods? _em;

    [TestInitialize]
    public async Task Setup()
    {
        _facade = EmberMethods.GetNewScriptManagerInstance();
        _em = new EmberMethods(_facade);

        await _facade.DeleteAllData();
        await _facade.ClearAllCaches();
        var existing = await _facade.ListScripts(includeCaches: false);
        foreach (var s in existing)
        {
            await _facade.DeleteScript(s.Id);
        }
    }

    #region Script Lifecycle

    [TestMethod]
    public async Task CreateScriptTest()
    {
        Guid id = (await _facade!.CreateScript(_sourceCodePedia!, "ConditionScriptTest")).Id;
        CustomerScript retrievedScript = await _facade.GetScript(id);

        Assert.AreEqual(retrievedScript.SourceCode, _sourceCodePedia);
    }
    [TestMethod]
    public async Task CreateScriptWithOldTest()
    {

        Guid id = (await _facade!.CreateScript(_sourceCodePedia!, apiVersion: 2)).Id;
        CustomerScript retrievedScript = await _facade.GetScript(id);
        var context = await _em!.GetTestingContext<GeneratorContextNoInherVaccineV5.GeneratorContext>(retrievedScript);
        await _facade.ExecuteScriptById(retrievedScript.Id, context);

        Assert.AreEqual(retrievedScript.SourceCode, _sourceCodePedia);
    }

    [TestMethod]
    public async Task UpdateScriptTest()
    {
        CustomerScript script = await _facade!.CreateScript(_sourceCodePedia!);
        string newSourceCode = _sourceCodePedia + System.Environment.NewLine + @"//new line added for testing";
        // newSourceCode = newSourceCode.Replace("@", "@" + System.Environment.NewLine);
        await _facade.UpdateScriptSC(script.Id, newSourceCode);

        CustomerScript retrievedScript = await _facade.GetScript(script.Id);
        Assert.AreEqual(retrievedScript.SourceCode, newSourceCode);
    }

    [TestMethod]
    public async Task UpdateAndCompileTest()
    {
        CustomerScript script = await _facade!.CreateScript(_sourceCodePedia!);
        string newSourceCode = _sourceCodePedia + System.Environment.NewLine + @"//new line added for testing";
        // newSourceCode = newSourceCode.Replace("@", "@" + System.Environment.NewLine);
        await _facade.UpdateScriptAndCompile(script.Id, newSourceCode);


        CustomerScript retrievedScript = await _facade.GetScript(script.Id);
        Assert.AreEqual(retrievedScript.SourceCode, newSourceCode);

        script = await _facade!.CreateScript(_sourceCodeActionV1!);
        newSourceCode = _sourceCodeActionV1 + System.Environment.NewLine + @"public class WillCauseError{}";

        Exception ex = await Assert.ThrowsExceptionAsync<CompilationOfUpdatedScriptException>(async () =>
        {
            await _facade.UpdateScriptAndCompile(script.Id, newSourceCode);

        });

        Exception innerEx = ExceptionHelper.GetExceptionFromChainReversed(ex, 1);
        Console.WriteLine("Exception: " + innerEx);
        Assert.IsTrue(innerEx.GetType() == typeof(Ember.Scripting.ValidationBeforeCompilationException));

        //source code shoud be unchacnged
        retrievedScript = await _facade.GetScript(script.Id);

        Console.WriteLine("script.SourceCode: ");
        Console.WriteLine(script.SourceCode);
        Console.WriteLine("retrievedScript.SourceCode: ");
        Console.WriteLine(retrievedScript.SourceCode);

        Assert.AreEqual(script.SourceCode, retrievedScript.SourceCode);
    }

    [TestMethod]
    public async Task DeleteScriptTest()
    {
        Guid id = (await _facade!.CreateScript(_sourceCodePedia!)).Id;
        await _facade.DeleteScript(id);


        await Assert.ThrowsExceptionAsync<System.InvalidOperationException>(async () =>    //todo check that this implements the correct exception
        {
            CustomerScript script = await _facade.GetScript(id);
        });
    }

    [TestMethod]
    public async Task GetScriptTest()
    {
        Guid id = (await _facade!.CreateScript(_sourceCodePedia!)).Id;
        CustomerScript script = await _facade.GetScript(id);

        Assert.IsNotNull(script);
        Assert.AreEqual(id, script.Id);
        Assert.AreEqual(_sourceCodePedia, script.SourceCode);
    }

    [TestMethod]
    public async Task ListScriptsTest()
    {
        await _facade!.ClearAllCaches();
        Guid id1 = (await _facade.CreateScript(_sourceCodePedia!)).Id;
        Guid id2 = (await _facade.CreateScript(_sourceCodeActionV1!)).Id;
        List<CustomerScript> scripts = await _facade.ListScripts(includeCaches: true);
        Assert.IsNotNull(scripts);
        Assert.IsTrue(scripts.Count == 2);
    }
    [TestMethod]
    public async Task ListScriptsTestWithFilter()
    {
        string scriptFolderPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
                "sandbox", "src", "Scripts"));
        // await facade!.EnsureDeletedCreated();
        await _facade!.DeleteAllData();
        await _em!.CompileAllScriptsInFolderAndSaveToDB(scriptFolderPath, "Gilles", EmberMethods.GetEmberApiVersion());
        List<CustomerScript> scripts = await _facade!.ListScripts(includeCaches: true);
        Assert.IsNotNull(scripts);
        Assert.IsTrue(scripts.Count == 6);

        CustomerScriptFilter filters = new CustomerScriptFilter(scriptName: "VaccineScript");   //todo fix this oine thorws errors
        List<CustomerScript> scriptsFiltered = await _facade.ListScripts(includeCaches: true, filters: filters);
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
        Guid id = (await _facade!.CreateScript(_sourceCodePedia!)).Id;

        await _facade.ClearScriptCache(id);
        var before = await _facade.GetScript(id, includeCaches: true);
        Assert.AreEqual(0, before.CompiledCaches.Count);

        await _facade.CompileScript(id);

        var after = await _facade.GetScript(id, includeCaches: true);
        Assert.IsTrue(after.CompiledCaches.Count == 1);
        Assert.IsTrue(after.CompiledCaches.Any(c => c.AssemblyBytes != null && c.AssemblyBytes.Length >= 1));   //todo check why this fails when ==1 but i think its correct
    }

    [TestMethod]
    public async Task CompileAllScriptsTest()
    {
        Guid id1 = (await _facade!.CreateScript(_sourceCodePedia!)).Id;
        Guid id2 = (await _facade.CreateScript(_sourceCodeActionV1!)).Id;

        await _facade.ClearAllCaches();

        var beforeS1 = await _facade.GetScript(id1, includeCaches: true);
        var beforeS2 = await _facade.GetScript(id2, includeCaches: true);
        Assert.AreEqual(0, beforeS1.CompiledCaches.Count);
        Assert.AreEqual(0, beforeS2.CompiledCaches.Count);

        await _facade.CompileAllScripts();

        var afterS1 = await _facade.GetScript(id1, includeCaches: true);
        var afterS2 = await _facade.GetScript(id2, includeCaches: true);
        Assert.IsTrue(afterS1.CompiledCaches.Count == 1);
        Assert.IsTrue(afterS2.CompiledCaches.Count == 1);
    }

    [TestMethod]
    public async Task RecompileScriptTest()
    {
        Guid id = (await _facade!.CreateScript(_sourceCodePedia!)).Id;

        await _facade.RecompileAllCaches(id);

        var script = await _facade.GetScript(id, includeCaches: true);
        Assert.IsTrue(script.CompiledCaches.Count == 1);
        Assert.IsTrue(script.CompiledCaches.Any(c => c.AssemblyBytes != null && c.AssemblyBytes.Length > 0));
    }

    [TestMethod]
    public async Task ValidateScriptTest()
    {
        ValidationRecord result = _facade!.BasicValidationBeforeCompiling(_sourceCodePedia!);

        // Assert.IsTrue(result.StartsWith("Success:", StringComparison.OrdinalIgnoreCase));
        // Assert.IsTrue(result.Contains("PediatricCondition"));
        await Assert.ThrowsExceptionAsync<ValidationBeforeCompilationException>(async () =>
        {
            ValidationRecord result2 = _facade.BasicValidationBeforeCompiling("wrong input test could be whatever");
        });

        // Assert.IsTrue(result2.StartsWith("Error:", StringComparison.OrdinalIgnoreCase));
        // Assert.IsTrue(result2.Contains("Exception"));
    }

    [TestMethod]
    public async Task GetCompilationErrorsTest()
    {
        Guid id = (await _facade!.CreateScript(_sourceCodePedia!)).Id;

        string result = await _facade.GetCompilationErrors(id);
        Assert.IsTrue(result.Contains("Successful Compilation!"));

        await Assert.ThrowsExceptionAsync<ValidationBeforeCompilationException>(async () =>    //before was basicvalidation exception todo
        {
            Guid id2 = (await _facade.CreateScript("wrong input test could be whatever")).Id;
            // string result2 = await facade.GetCompilationErrors(id2);
            string result2 = await _facade.GetCompilationErrors(id2);
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
        await _facade!.CreateScriptWithoutCompiling(id2, "wrong input test could be whatever");
        // string result2 = await facade.GetCompilationErrors(id2);
        string result2 = await _facade.GetCompilationErrors(id2);
        Console.WriteLine("Start of Result:");
        Console.WriteLine(result2);
        Console.WriteLine("End of Result.");

        string desiredOutput = """
        Error in ScriptCompiler RunCompilation method compilation probably failed in if (!emitResult.Success) errors: Line 1, Col 13: CS1003 - Syntax error, ',' expected
        Line 1, Col 35: CS1002 - ; expected
        Line 1, Col 1: CS8805 - Program using top-level statements must be an executable.
        Line 1, Col 1: CS0246 - The type or namespace name 'wrong' could not be found (are you missing a using directive or an assembly reference?)
        """;


        Assert.IsTrue(result2.Contains(desiredOutput));

        Guid id3 = Guid.NewGuid();
        await _facade.CreateScriptWithoutCompiling(id3, "public class Test : IFakeInterface{}");
        // string result2 = await facade.GetCompilationErrors(id2);
        string result3 = await _facade.GetCompilationErrors(id3);
        Console.WriteLine("Start of Result:");
        Console.WriteLine(result3);
        Console.WriteLine("End of Result.");
        string desiredOutput3 = """
        Error in ScriptCompiler RunCompilation method compilation probably failed in if (!emitResult.Success) errors: Line 1, Col 21: CS0246 - The type or namespace name 'IFakeInterface' could not be found (are you missing a using directive or an assembly reference?)
        """;

        Assert.IsTrue(result3.Contains(desiredOutput3));
    }

    #endregion

    #region Execution Operations

    [TestMethod]
    public async Task ExecuteActionScriptTest()
    {
        Guid id = (await _facade!.CreateScript(_sourceCodeActionV1!)).Id;
        var testingContext = await _em!.GetTestingContext<GeneratorContextV4.GeneratorContext>();

        ActionResultSF result = await _facade.ExecuteActionScript(id, testingContext);

        Assert.IsNotNull(result);
        // Assert.IsTrue(result.IsSuccess);
        // Assert.IsTrue(result.Message.Contains("Pediatric"));
        result = (ActionResultV3.ActionResult)EmberMethods.UpgradeActionResult(result);
        Assert.IsInstanceOfType(result, typeof(ActionResultV3.ActionResult));

        Guid id2 = (await _facade.CreateScript(_sourceCodePedia!)).Id;
        await Assert.ThrowsExceptionAsync<System.InvalidCastException>(async () =>
        {
            ActionResultSF result2 = await _facade.ExecuteActionScript(id2, testingContext);
        });

    }

    [TestMethod]
    public async Task ExecuteConditionScriptTest()
    {
        Guid id = (await _facade!.CreateScript(_sourceCodePedia!)).Id;
        var testingContext = await _em!.GetTestingContext<GeneratorContextV4.GeneratorContext>();

        bool result = await _facade.ExecuteConditionScript(id, testingContext);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.GetType().ToString() == "System.Boolean");

        Guid id2 = (await _facade.CreateScript(_sourceCodeActionV1!)).Id;
        await Assert.ThrowsExceptionAsync<System.InvalidCastException>(async () =>
        {
            bool result2 = await _facade.ExecuteConditionScript(id2, testingContext);
        });
    }

    [TestMethod]
    public async Task ExecuteScriptByIdTest()
    {
        var context = await _em!.GetTestingContext<GeneratorContextV4.GeneratorContext>();

        Guid condId = (await _facade!.CreateScript(_sourceCodePedia!)).Id;
        object condResult = await _facade.ExecuteScriptById(condId, context);
        Assert.IsInstanceOfType(condResult, typeof(bool));
        Assert.AreEqual(true, (bool)condResult);

        Guid actId = (await _facade.CreateScript(_sourceCodeActionV1!)).Id;
        object actResult = await _facade.ExecuteScriptById(actId, context);
        Assert.IsInstanceOfType(actResult, typeof(ActionResultSF));
        // Assert.IsTrue(((ActionResultBaseClass)actResult).IsSuccess);
    }

    #endregion

    #region Cache Management

    [TestMethod]
    public async Task GetCompiledCacheTest()
    {
        Guid id = (await _facade!.CreateScript(_sourceCodePedia!)).Id;

        CompiledScripts cache = await _facade.GetCompiledCache(id);
        byte[] cacheAB = cache.AssemblyBytes!;

        Assert.IsNotNull(cacheAB);
        Assert.IsTrue(cacheAB.Length > 0);
    }

    [TestMethod]
    public async Task ClearScriptCacheTest()
    {
        Guid id = (await _facade!.CreateScript(_sourceCodePedia!)).Id;

        CompiledScripts cache = await _facade.GetCompiledCache(id);
        byte[] cacheAB = cache.AssemblyBytes!;
        Assert.IsTrue(cacheAB.Length > 0);

        await _facade.ClearScriptCache(id);

        var script = await _facade.GetScript(id, includeCaches: true);
        Assert.AreEqual(0, script.CompiledCaches.Count);

        await Assert.ThrowsExceptionAsync<System.InvalidOperationException>(async () =>
        {
            await _facade.GetCompiledCache(id);
        });
    }

    [TestMethod]
    public async Task ClearAllCachesTest()
    {
        Guid id = (await _facade!.CreateScript(_sourceCodePedia!)).Id;
        Guid id2 = (await _facade.CreateScript(_sourceCodeActionV1!)).Id;

        await _facade.ClearAllCaches();

        var caches = await _facade.GetAllCompiledScriptCaches();
        Assert.AreEqual(0, caches.Count);
    }

    [TestMethod]
    public async Task PrecompileForApiVersionTest()
    {
        Guid id = (await _facade!.CreateScript(_sourceCodePedia!)).Id;

        await _facade.ClearAllCaches();
        var before = await _facade.GetScript(id, includeCaches: true);
        Assert.AreEqual(0, before.CompiledCaches.Count);

        await _facade.PrecompileForApiVersion();

        var after = await _facade.GetScript(id, includeCaches: true);
        Assert.IsTrue(after.CompiledCaches.Count >= 1);
    }

    #endregion

    #region Version Management

    [TestMethod]
    public async Task GetActiveApiVersionsTest()
    {
        Guid id = (await _facade!.CreateScript(_sourceCodePedia!)).Id;
        int currentVersion = _facade.GetRunningApiVersion();

        List<int> versions = await _facade.GetActiveApiVersions();

        Assert.IsNotNull(versions);
        Assert.IsTrue(versions.Count >= 1);
        Assert.IsTrue(versions.Contains(currentVersion));
    }

    [TestMethod]
    public void GetRecentApiVersionTest()
    {
        int v = _facade!.GetRunningApiVersion();
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
        Guid id = (await _facade!.CreateScript(_sourceCodePedia!)).Id;
        int currentVersion = _facade.GetRunningApiVersion();

        bool hasRecent = await _facade.CheckVersionCompatibility(id, currentVersion);

        Assert.IsTrue(hasRecent);

        hasRecent = await _facade.CheckVersionCompatibility(id, currentVersion - 1);

        Assert.IsFalse(hasRecent);
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
        if (TestConfig.DuplicatesAllowed)
        {
            Guid id = (await _facade!.CreateScript(_sourceCodePedia!)).Id;
            Guid id2 = (await _facade.CreateScript(_sourceCodePedia!)).Id;

            await _facade.RemoveDuplicates();

            var scripts = await _facade.ListScripts();
            Assert.AreEqual(1, scripts.Count);
        }

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
        await _facade!.HealthCheck();
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task GetScriptMetadataTest()
    {
        Guid id = (await _facade!.CreateScript(_sourceCodePedia!)).Id;

        string metaData = await _facade.GetScriptMetadata(id);

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
