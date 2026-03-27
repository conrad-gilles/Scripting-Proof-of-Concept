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
    private ISccriptManagerDeleteAfter? _scriptManager;
    string? _sourceCodePedia = TestHelper.GetSC().sourceCodePedia;
    private string? _sourceCodeActionV1 = TestHelper.GetSC().sourceCodeActionV1;
    private string? _sourceCodeActionV3 = TestHelper.GetSC().sourceCodeActionV3;
    private EmberMethods? _em;

    [TestInitialize]
    public async Task Setup()
    {
        _scriptManager = EmberMethods.GetNewScriptManagerInstance();
        _em = new EmberMethods(_scriptManager);

        await _scriptManager.DeleteAllData();
        await _scriptManager.ClearAllCaches();
        var existing = await _scriptManager.ListScripts(includeCaches: false);
        foreach (var s in existing)
        {
            await _scriptManager.DeleteScript(s.Id);
        }
    }

    #region Script Lifecycle

    [TestMethod]
    public async Task CreateScriptTest()
    {
        Guid id = (await _scriptManager!.CreateScript(_sourceCodePedia!)).Id;
        CustomerScript retrievedScript = await _scriptManager.GetScript(id);

        Assert.AreEqual(retrievedScript.SourceCode, _sourceCodePedia);
    }
    [TestMethod]
    public async Task CreateScriptWithOldTest()
    {

        Guid id = (await _scriptManager!.CreateScript(_sourceCodePedia!, apiVersion: 2)).Id;
        CustomerScript retrievedScript = await _scriptManager.GetScript(id);
        var context = await _em!.GetTestingContext<GeneratorContextNoInherVaccineV5.GeneratorContext>(retrievedScript);
        await _scriptManager.ExecuteScriptById(retrievedScript.Id, context);

        Assert.AreEqual(retrievedScript.SourceCode, _sourceCodePedia);
    }

    [TestMethod]
    public async Task UpdateScriptTest()
    {
        CustomerScript script = await _scriptManager!.CreateScript(_sourceCodePedia!);
        string newSourceCode = _sourceCodePedia + System.Environment.NewLine + @"//new line added for testing";
        // newSourceCode = newSourceCode.Replace("@", "@" + System.Environment.NewLine);
        await _scriptManager.UpdateScriptSC(script.Id, newSourceCode);

        CustomerScript retrievedScript = await _scriptManager.GetScript(script.Id);
        Assert.AreEqual(retrievedScript.SourceCode, newSourceCode);
    }

    [TestMethod]
    public async Task UpdateAndCompileTest()
    {
        CustomerScript script = await _scriptManager!.CreateScript(_sourceCodePedia!);
        string newSourceCode = _sourceCodePedia + System.Environment.NewLine + @"//new line added for testing";
        // newSourceCode = newSourceCode.Replace("@", "@" + System.Environment.NewLine);
        await _scriptManager.UpdateScriptAndCompile(script.Id, newSourceCode);


        CustomerScript retrievedScript = await _scriptManager.GetScript(script.Id);

        Console.WriteLine("retrievedScript.SourceCode: ");
        Console.WriteLine(retrievedScript.SourceCode);
        Console.WriteLine("newSourceCode: ");
        Console.WriteLine(newSourceCode);

        Assert.AreEqual(retrievedScript.SourceCode, newSourceCode);

        script = await _scriptManager!.CreateScript(_sourceCodeActionV1!);
        newSourceCode = _sourceCodeActionV1 + System.Environment.NewLine + @"public class WillCauseError{}";

        Exception ex = await Assert.ThrowsExceptionAsync<CompilationOfUpdatedScriptException>(async () =>
        {
            await _scriptManager.UpdateScriptAndCompile(script.Id, newSourceCode);

        });

        Exception innerEx = ExceptionHelper.GetExceptionFromChainReversed(ex, 1);
        Console.WriteLine("Exception: " + innerEx);
        // Assert.IsTrue(innerEx.GetType() == typeof(Ember.Scripting.ValidationBeforeCompilationException));

        //source code shoud be unchacnged
        retrievedScript = await _scriptManager.GetScript(script.Id);

        Console.WriteLine("script.SourceCode: ");
        Console.WriteLine(script.SourceCode);
        Console.WriteLine("retrievedScript.SourceCode: ");
        Console.WriteLine(retrievedScript.SourceCode);

        Assert.AreEqual(script.SourceCode, retrievedScript.SourceCode);

        await _scriptManager.UpdateScriptAndCompile(script.Id, TestHelper.GetSC().sourceCodeWhileTrue);
        retrievedScript = await _scriptManager.GetScript(script.Id);
        Console.WriteLine(nameof(retrievedScript.ScriptName) + ": " + retrievedScript.ScriptName);
        Console.WriteLine(nameof(WhileTrueScript) + ": too compare");
        Assert.IsTrue(retrievedScript.ScriptName == nameof(WhileTrueScript));

        await _scriptManager.UpdateScriptAndCompile(script.Id, TestHelper.GetSC().sourceCodePedia);
        retrievedScript = await _scriptManager.GetScript(script.Id);
        Assert.IsTrue(retrievedScript.ScriptName == nameof(PediatricCondition));
        Assert.IsTrue(retrievedScript.GetScriptType() == typeof(IGeneratorConditionScript));
    }

    [TestMethod]
    public async Task DeleteScriptTest()
    {
        Guid id = (await _scriptManager!.CreateScript(_sourceCodePedia!)).Id;
        await _scriptManager.DeleteScript(id);


        await Assert.ThrowsExceptionAsync<System.InvalidOperationException>(async () =>    //todo check that this implements the correct exception
        {
            CustomerScript script = await _scriptManager.GetScript(id);
        });
    }

    [TestMethod]
    public async Task GetScriptTest()
    {
        Guid id = (await _scriptManager!.CreateScript(_sourceCodePedia!)).Id;
        CustomerScript script = await _scriptManager.GetScript(id);

        Assert.IsNotNull(script);
        Assert.AreEqual(id, script.Id);
        Assert.AreEqual(_sourceCodePedia, script.SourceCode);
    }

    [TestMethod]
    public async Task ListScriptsTest()
    {
        await _scriptManager!.ClearAllCaches();
        Guid id1 = (await _scriptManager.CreateScript(_sourceCodePedia!)).Id;
        Guid id2 = (await _scriptManager.CreateScript(_sourceCodeActionV1!)).Id;
        List<CustomerScript> scripts = await _scriptManager.ListScripts(includeCaches: true);
        Assert.IsNotNull(scripts);
        Assert.IsTrue(scripts.Count == 2);
    }
    [TestMethod]
    public async Task ListScriptsTestWithFilter()
    {
        string scriptFolderPathFull = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
                "sandbox", "src", "Scripts"));
        string scriptFolderPathAction = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
                "sandbox", "src", "Scripts", "ActionScripts"));
        string scriptFolderPathCondition = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
        "sandbox", "src", "Scripts", "ConditionScripts"));
        // await facade!.EnsureDeletedCreated();
        await _scriptManager!.DeleteAllData();
        // await _em!.CompileAllScriptsInFolderAndSaveToDB(scriptFolderPath, "Gilles", EmberMethods.GetEmberApiVersion());
        await _em!.CompileAllScriptsInFolderAndSaveToDB(scriptFolderPathAction, "Gilles", EmberMethods.GetEmberApiVersion());
        await _em!.CompileAllScriptsInFolderAndSaveToDB(scriptFolderPathCondition, "Gilles", EmberMethods.GetEmberApiVersion());

        List<CustomerScript> scripts = await _scriptManager!.ListScripts(includeCaches: true);
        Assert.IsNotNull(scripts);
        Assert.IsTrue(scripts.Count == 5);

        CustomerScriptFilter filters = new CustomerScriptFilter(scriptName: "VaccineScript");   //todo fix this oine thorws errors
        List<CustomerScript> scriptsFiltered = await _scriptManager.ListScripts(includeCaches: true, filters: filters);
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
        Guid id = (await _scriptManager!.CreateScript(_sourceCodePedia!)).Id;

        await _scriptManager.ClearScriptCache(id);
        var before = await _scriptManager.GetScript(id, includeCaches: true);
        Assert.AreEqual(0, before.CompiledCaches.Count);

        await _scriptManager.CompileScript(id);

        var after = await _scriptManager.GetScript(id, includeCaches: true);
        Assert.IsTrue(after.CompiledCaches.Count == 1);
        Assert.IsTrue(after.CompiledCaches.Any(c => c.AssemblyBytes != null && c.AssemblyBytes.Length >= 1));   //todo check why this fails when ==1 but i think its correct
    }

    [TestMethod]
    public async Task CompileAllScriptsTest()
    {
        Guid id1 = (await _scriptManager!.CreateScript(_sourceCodePedia!)).Id;
        Guid id2 = (await _scriptManager.CreateScript(_sourceCodeActionV1!)).Id;

        await _scriptManager.ClearAllCaches();

        var beforeS1 = await _scriptManager.GetScript(id1, includeCaches: true);
        var beforeS2 = await _scriptManager.GetScript(id2, includeCaches: true);
        Assert.AreEqual(0, beforeS1.CompiledCaches.Count);
        Assert.AreEqual(0, beforeS2.CompiledCaches.Count);

        await _scriptManager.CompileAllScripts();

        var afterS1 = await _scriptManager.GetScript(id1, includeCaches: true);
        var afterS2 = await _scriptManager.GetScript(id2, includeCaches: true);
        Assert.IsTrue(afterS1.CompiledCaches.Count == 1);
        Assert.IsTrue(afterS2.CompiledCaches.Count == 1);
    }

    [TestMethod]
    public async Task RecompileScriptTest()
    {
        Guid id = (await _scriptManager!.CreateScript(_sourceCodePedia!)).Id;

        await _scriptManager.RecompileAllCaches(id);

        var script = await _scriptManager.GetScript(id, includeCaches: true);
        Assert.IsTrue(script.CompiledCaches.Count == 1);
        Assert.IsTrue(script.CompiledCaches.Any(c => c.AssemblyBytes != null && c.AssemblyBytes.Length > 0));
    }

    [TestMethod]
    public async Task ValidateScriptTest()
    {
        ValidationRecord result = _scriptManager!.BasicValidationBeforeCompiling(_sourceCodePedia!);

        // Assert.IsTrue(result.StartsWith("Success:", StringComparison.OrdinalIgnoreCase));
        // Assert.IsTrue(result.Contains("PediatricCondition"));
        await Assert.ThrowsExceptionAsync<System.InvalidOperationException>(async () =>
        {
            ValidationRecord result2 = _scriptManager.BasicValidationBeforeCompiling("wrong input test could be whatever");
        });

        // Assert.IsTrue(result2.StartsWith("Error:", StringComparison.OrdinalIgnoreCase));
        // Assert.IsTrue(result2.Contains("Exception"));
    }

    [TestMethod]
    public async Task GetCompilationErrorsTest()
    {
        Guid id = (await _scriptManager!.CreateScript(_sourceCodePedia!)).Id;

        string result = await _scriptManager.GetCompilationErrors(id);
        Assert.IsTrue(result.Contains("Successful Compilation!"));

        await Assert.ThrowsExceptionAsync<System.InvalidOperationException>(async () =>    //before was basicvalidation exception todo
        {
            Guid id2 = (await _scriptManager.CreateScript("wrong input test could be whatever")).Id;
            // string result2 = await facade.GetCompilationErrors(id2);
            string result2 = await _scriptManager.GetCompilationErrors(id2);
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


    // [TestMethod]
    // public async Task GetCompilationErrorsTestNegative()
    // {
    //     Guid id2 = Guid.NewGuid();
    //     await _scriptManager!.CreateScriptWithoutCompiling(id2, "wrong input test could be whatever");
    //     // string result2 = await facade.GetCompilationErrors(id2);
    //     string result2 = await _scriptManager.GetCompilationErrors(id2);
    //     Console.WriteLine("Start of Result:");
    //     Console.WriteLine(result2);
    //     Console.WriteLine("End of Result.");

    //     string desiredOutput = """
    //     Error in ScriptCompiler RunCompilation method compilation probably failed in if (!emitResult.Success) errors: Line 1, Col 13: CS1003 - Syntax error, ',' expected
    //     Line 1, Col 35: CS1002 - ; expected
    //     Line 1, Col 1: CS8805 - Program using top-level statements must be an executable.
    //     Line 1, Col 1: CS0246 - The type or namespace name 'wrong' could not be found (are you missing a using directive or an assembly reference?)
    //     """;


    //     Assert.IsTrue(result2.Contains(desiredOutput));

    //     Guid id3 = Guid.NewGuid();
    //     await _scriptManager.CreateScriptWithoutCompiling(id3, "public class Test : IFakeInterface{}");
    //     // string result2 = await facade.GetCompilationErrors(id2);
    //     string result3 = await _scriptManager.GetCompilationErrors(id3);
    //     Console.WriteLine("Start of Result:");
    //     Console.WriteLine(result3);
    //     Console.WriteLine("End of Result.");
    //     string desiredOutput3 = """
    //     Error in ScriptCompiler RunCompilation method compilation probably failed in if (!emitResult.Success) errors: Line 1, Col 21: CS0246 - The type or namespace name 'IFakeInterface' could not be found (are you missing a using directive or an assembly reference?)
    //     """;

    //     Assert.IsTrue(result3.Contains(desiredOutput3));
    // }

    [TestMethod]
    public async Task GetCompilationErrorsCleaned()
    {
        Guid id = Guid.NewGuid();
        string sourceCode = "wrong input test could be whatever";
        List<ScriptCompilationError> result1 = await _scriptManager!.GetCompilationErrors(sourceCode);

        var expectedErrorIds = new List<string> { "CS1003", "CS1002", "CS8805", "CS0246" };

        List<string> result2 = [];
        foreach (var item in result1)
        {
            result2.Add(item.Id);
        }

        CollectionAssert.IsSubsetOf(expectedErrorIds, result2);

        id = Guid.NewGuid();
        sourceCode = TestHelper.GetSC().sourceCodeActionV2;
        // Exception ex = await Assert.ThrowsExceptionAsync<NoErrorsInScriptException>(async () =>
        // {
        result1 = await _scriptManager!.GetCompilationErrors(sourceCode);
        // });
        List<ScriptCompilationError> expected = [];
        CollectionAssert.IsSubsetOf(expected, result1);
    }

    #endregion

    #region Execution Operations

    [TestMethod]
    public async Task ExecuteActionScriptTest()
    {
        Guid id = (await _scriptManager!.CreateScript(_sourceCodeActionV1!)).Id;
        var testingContext = await _em!.GetTestingContext<GeneratorContextV4.GeneratorContext>();

        ActionResultSF result = (ActionResultSF)await _scriptManager.ExecuteScriptById(id, testingContext);

        Assert.IsNotNull(result);
        // Assert.IsTrue(result.IsSuccess);
        // Assert.IsTrue(result.Message.Contains("Pediatric"));
        result = (ActionResultV3.ActionResult)EmberMethods.UpgradeActionResult(result);
        Assert.IsInstanceOfType(result, typeof(ActionResultV3.ActionResult));

        Guid id2 = (await _scriptManager.CreateScript(_sourceCodePedia!)).Id;
        await Assert.ThrowsExceptionAsync<System.InvalidCastException>(async () =>
        {
            ActionResultSF result2 = (ActionResultSF)await _scriptManager.ExecuteScriptById(id2, testingContext);
        });

    }

    [TestMethod]
    public async Task ExecuteConditionScriptTest()
    {
        Guid id = (await _scriptManager!.CreateScript(_sourceCodePedia!)).Id;
        var testingContext = await _em!.GetTestingContext<GeneratorContextV4.GeneratorContext>();

        bool result = (bool)await _scriptManager.ExecuteScriptById(id, testingContext);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.GetType().ToString() == "System.Boolean");

        Guid id2 = (await _scriptManager.CreateScript(_sourceCodeActionV1!)).Id;
        await Assert.ThrowsExceptionAsync<System.InvalidCastException>(async () =>
        {
            bool result2 = (bool)await _scriptManager.ExecuteScriptById(id2, testingContext);
        });
    }

    [TestMethod]
    public async Task ExecuteScriptByIdTest()
    {
        var context = await _em!.GetTestingContext<GeneratorContextV4.GeneratorContext>();

        Guid condId = (await _scriptManager!.CreateScript(_sourceCodePedia!)).Id;
        object condResult = await _scriptManager.ExecuteScriptById(condId, context);
        Assert.IsInstanceOfType(condResult, typeof(bool));
        Assert.AreEqual(true, (bool)condResult);

        Guid actId = (await _scriptManager.CreateScript(_sourceCodeActionV1!)).Id;
        object actResult = await _scriptManager.ExecuteScriptById(actId, context);
        Assert.IsInstanceOfType(actResult, typeof(ActionResultSF));
        // Assert.IsTrue(((ActionResultBaseClass)actResult).IsSuccess);
    }

    #endregion

    #region Cache Management

    [TestMethod]
    public async Task GetCompiledCacheTest()
    {
        Guid id = (await _scriptManager!.CreateScript(_sourceCodePedia!)).Id;

        CompiledScripts cache = await _scriptManager.GetCompiledCache(id);
        byte[] cacheAB = cache.AssemblyBytes!;

        Assert.IsNotNull(cacheAB);
        Assert.IsTrue(cacheAB.Length > 0);
    }

    [TestMethod]
    public async Task ClearScriptCacheTest()
    {
        Guid id = (await _scriptManager!.CreateScript(_sourceCodePedia!)).Id;

        CompiledScripts cache = await _scriptManager.GetCompiledCache(id);
        byte[] cacheAB = cache.AssemblyBytes!;
        Assert.IsTrue(cacheAB.Length > 0);

        await _scriptManager.ClearScriptCache(id);

        var script = await _scriptManager.GetScript(id, includeCaches: true);
        Assert.AreEqual(0, script.CompiledCaches.Count);

        await Assert.ThrowsExceptionAsync<System.InvalidOperationException>(async () =>
        {
            await _scriptManager.GetCompiledCache(id);
        });
    }

    [TestMethod]
    public async Task ClearAllCachesTest()
    {
        Guid id = (await _scriptManager!.CreateScript(_sourceCodePedia!)).Id;
        Guid id2 = (await _scriptManager.CreateScript(_sourceCodeActionV1!)).Id;

        await _scriptManager.ClearAllCaches();

        var caches = await _scriptManager.GetAllCompiledScriptCaches();
        Assert.AreEqual(0, caches.Count);
    }

    // [TestMethod]
    // public async Task PrecompileForApiVersionTest()
    // {
    //     Guid id = (await _scriptManager!.CreateScript(_sourceCodePedia!)).Id;

    //     await _scriptManager.ClearAllCaches();
    //     var before = await _scriptManager.GetScript(id, includeCaches: true);
    //     Assert.AreEqual(0, before.CompiledCaches.Count);

    //     await _scriptManager.PrecompileForApiVersion();

    //     var after = await _scriptManager.GetScript(id, includeCaches: true);
    //     Assert.IsTrue(after.CompiledCaches.Count >= 1);
    // }

    #endregion

    #region Version Management

    // [TestMethod]
    // public async Task GetActiveApiVersionsTest()
    // {
    //     Guid id = (await _scriptManager!.CreateScript(_sourceCodePedia!)).Id;
    //     int currentVersion = _scriptManager.GetRunningApiVersion();

    //     List<int> versions = await _scriptManager.GetActiveApiVersions();

    //     Assert.IsNotNull(versions);
    //     Assert.IsTrue(versions.Count >= 1);
    //     Assert.IsTrue(versions.Contains(currentVersion));
    // }

    [TestMethod]
    public void GetRecentApiVersionTest()
    {
        int v = _scriptManager!.GetRunningApiVersion();
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
        Guid id = (await _scriptManager!.CreateScript(_sourceCodePedia!)).Id;
        int currentVersion = _scriptManager.GetRunningApiVersion();

        bool hasRecent = await _scriptManager.CheckVersionCompatibility(id, currentVersion);

        Assert.IsTrue(hasRecent);

        hasRecent = await _scriptManager.CheckVersionCompatibility(id, currentVersion - 1);

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


    // [TestMethod]
    // public async Task RemoveDuplicatesTest()
    // {
    //     if (TestConfig.DuplicatesAllowed)
    //     {
    //         Guid id = (await _scriptManager!.CreateScript(_sourceCodePedia!)).Id;
    //         Guid id2 = (await _scriptManager.CreateScript(_sourceCodePedia!)).Id;

    //         await _scriptManager.RemoveDuplicates();

    //         var scripts = await _scriptManager.ListScripts();
    //         Assert.AreEqual(1, scripts.Count);
    //     }

    // }

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
        await _scriptManager!.HealthCheck();
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task GetScriptMetadataTest()
    {
        Guid id = (await _scriptManager!.CreateScript(_sourceCodePedia!)).Id;

        string metaData = await _scriptManager.GetScriptMetadata(id);

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
