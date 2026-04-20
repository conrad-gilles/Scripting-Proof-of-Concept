#pragma warning disable CS0436


using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ember.Simulation;
using Sandbox;
using Ember.Scripting.Compilation;


[TestClass]
public class ArchitechtureTests
{
    static IScriptManagerExtended ScriptManager = TestHelper.InitScriptManager();
    static EmberInternalFacade InternalScriptManager = new EmberInternalFacade(ScriptManager);
    static EmberMethods em = new EmberMethods(ScriptManager);

    string sourceCode100 = """
        using System;   //todo this is possible to default in compiler
        using System.Threading.Tasks;
        using System.Collections.Generic;   //todo same for them
        using Ember.Scripting;
        using Ember.Scripting.Execution;
        using GeneratorScriptsGeneric;
        using IGeneratorContext_V2;
        using Ember.Scripting.Compilation;


        [ExecutionTime(ExecutionTimeGroups.Short)]
        public class ExecutionTimeTest : GeneratorScriptsGeneric.IActionScript<IGeneratorContext_V2.IGeneratorContext, ActionResultV1.ActionResult>
        {
        public async Task<ActionResultV1.ActionResult> ExecuteAsync(IGeneratorContext_V2.IGeneratorContext context)    //error is here in Basyns
        {
        while (true)
        {
            ScriptEnvironment.CurrentToken.Value.ThrowIfCancellationRequested();
        }
            return ActionResultV1.ActionResult.Success("Pediatric tests added");
        }
        }
        """;
    string sourceCode500 = """
        using System;   //todo this is possible to default in compiler
        using System.Threading.Tasks;
        using System.Collections.Generic;   //todo same for them
        using Ember.Scripting;
        using GeneratorScriptsGeneric;
        using IGeneratorContext_V2;
        using Ember.Scripting.Compilation;
        using Ember.Scripting.Execution;

        [ExecutionTime(ExecutionTimeGroups.Medium)]
        public class ExecutionTimeTest : GeneratorScriptsGeneric.IActionScript<IGeneratorContext_V2.IGeneratorContext, ActionResultV1.ActionResult>
        {
        public async Task<ActionResultV1.ActionResult> ExecuteAsync(IGeneratorContext_V2.IGeneratorContext context)    //error is here in Basyns
        {
        while (true)
        {
            ScriptEnvironment.CurrentToken.Value.ThrowIfCancellationRequested();
        }
            return ActionResultV1.ActionResult.Success("Pediatric tests added");
        }
        }
        """;
    string sourceCode1000 = """
        using System;   //todo this is possible to default in compiler
        using System.Threading.Tasks;
        using System.Collections.Generic;   //todo same for them
        using Ember.Scripting;
        using GeneratorScriptsGeneric;
        using IGeneratorContext_V2;
        using Ember.Scripting.Compilation;
        using Ember.Scripting.Execution;

        [ExecutionTime(ExecutionTimeGroups.Long)]
        public class ExecutionTimeTest : GeneratorScriptsGeneric.IActionScript<IGeneratorContext_V2.IGeneratorContext, ActionResultV1.ActionResult>
        {
        public async Task<ActionResultV1.ActionResult> ExecuteAsync(IGeneratorContext_V2.IGeneratorContext context)    //error is here in Basyns
        {
        while (true)
        {
            ScriptEnvironment.CurrentToken.Value.ThrowIfCancellationRequested();
        }
            return ActionResultV1.ActionResult.Success("Pediatric tests added");
        }
        }
        """;
    string sourceCode5000 = """
        using System;   //todo this is possible to default in compiler
        using System.Threading.Tasks;
        using System.Collections.Generic;   //todo same for them
        using Ember.Scripting;
        using Ember.Scripting.Compilation;
        using GeneratorScriptsGeneric;
        using IGeneratorContext_V2;
        using Ember.Scripting.Compilation;
        using Ember.Scripting.Execution;

        [ExecutionTime(ExecutionTimeGroups.ExtraLong)]
        public class ExecutionTimeTest : GeneratorScriptsGeneric.IActionScript<IGeneratorContext_V2.IGeneratorContext, ActionResultV1.ActionResult>
        {
        public async Task<ActionResultV1.ActionResult> ExecuteAsync(IGeneratorContext_V2.IGeneratorContext context)    //error is here in Basyns
        {
        while (true)
        {
            ScriptEnvironment.CurrentToken.Value.ThrowIfCancellationRequested();
        }
            return ActionResultV1.ActionResult.Success("Pediatric tests added");
        }
        }
        """;

    [TestInitialize]
    public async Task SetupAsync()
    {
        await ScriptManager.DeleteAllData();
    }
    // [TestMethod]
    public async Task ScriptTypesVerification()
    {
        // List<CustomerScript> scripts = await ScriptManager.ListScripts();

        // ScriptTypes[] enumValues = Enum.GetValues<ScriptTypes>();

        // foreach (var script in scripts)
        // {
        //     ScriptTypes type = script.GetScriptTypeEnum();
        //     if (enumValues.Contains(type) == false)
        //     {
        //         throw new Exception();
        //     }
        // }
    }

    [TestMethod]
    public async Task ExecutionTimesEnumValuesVerification()
    {
        List<CustomerScript> scripts = await ScriptManager.ListScripts();

        ExecutionTimeGroups[] enumValues = Enum.GetValues<ExecutionTimeGroups>();

        foreach (var item in enumValues)
        {
            if (((int)item) > ExecutionTime.MaximumDuration)
            {
                throw new Exception("enum value too big");
            }
            if (((int)item) < ExecutionTime.MinimumDuration)
            {
                throw new Exception("enum value too small");
            }
        }

        foreach (var script in scripts)
        {
            if (script.ExecutionTimeInMS > ExecutionTime.MaximumDuration)
            {
                throw new Exception("duration too big");
            }
            if (script.ExecutionTimeInMS < ExecutionTime.MinimumDuration)
            {
                throw new Exception("duration too small");
            }
        }

        int expectedMS = 100;
        int? realMS = ScriptManager.BasicValidationBeforeCompiling(sourceCode100).ExecutionTime;
        Assert.IsTrue(expectedMS == realMS);



        expectedMS = 500;  //shows how the maximum is respected and not overstepped
        realMS = ScriptManager.BasicValidationBeforeCompiling(sourceCode500).ExecutionTime;
        Assert.IsTrue(expectedMS == realMS);



        expectedMS = 1000;
        realMS = ScriptManager.BasicValidationBeforeCompiling(sourceCode1000).ExecutionTime;
        Assert.IsTrue(expectedMS == realMS);



        expectedMS = 5000;
        realMS = ScriptManager.BasicValidationBeforeCompiling(sourceCode5000).ExecutionTime;
        Assert.IsTrue(expectedMS == realMS);
    }

    [TestMethod]
    public async Task CheckTimeout()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        Exception e = await Assert.ThrowsExceptionAsync<ActionScriptExecutionException>(async () =>
        {
            await InternalScriptManager.ExecuteUnfinishedScriptBySourceCode(sourceCode5000, TestHelper.GetContext(), nameof(IExecuteAsync.ExecuteAsync));
        });
        stopwatch.Stop();
        long elapsedMs = stopwatch.ElapsedMilliseconds;
        ExceptionHelper.PrintExceptionListToConsole(e);
        Console.WriteLine("execution took: " + elapsedMs);
        int realMS = (int)ScriptManager.BasicValidationBeforeCompiling(sourceCode5000).ExecutionTime;
        Console.WriteLine("exoected: " + realMS);

        Assert.IsTrue(elapsedMs > 4999 && elapsedMs < 7500);

        stopwatch = System.Diagnostics.Stopwatch.StartNew();
        e = await Assert.ThrowsExceptionAsync<ActionScriptExecutionException>(async () =>
       {
           await InternalScriptManager.ExecuteUnfinishedScriptBySourceCode(sourceCode100, TestHelper.GetContext(), nameof(IExecuteAsync.ExecuteAsync));
       });
        stopwatch.Stop();

        elapsedMs = stopwatch.ElapsedMilliseconds;
        Console.WriteLine("execution took: " + elapsedMs);
        Assert.IsTrue(elapsedMs > 99 && elapsedMs < 400);

        stopwatch = System.Diagnostics.Stopwatch.StartNew();
        e = await Assert.ThrowsExceptionAsync<ActionScriptExecutionException>(async () =>
       {
           await InternalScriptManager.ExecuteUnfinishedScriptBySourceCode(sourceCode500, TestHelper.GetContext(), nameof(IExecuteAsync.ExecuteAsync));
       });
        stopwatch.Stop();

        elapsedMs = stopwatch.ElapsedMilliseconds;
        Console.WriteLine("execution took: " + elapsedMs);
        Assert.IsTrue(elapsedMs > 499 && elapsedMs < 750);

        stopwatch = System.Diagnostics.Stopwatch.StartNew();
        e = await Assert.ThrowsExceptionAsync<ActionScriptExecutionException>(async () =>
       {
           await InternalScriptManager.ExecuteUnfinishedScriptBySourceCode(sourceCode1000, TestHelper.GetContext(), nameof(IExecuteAsync.ExecuteAsync));
       });
        stopwatch.Stop();

        elapsedMs = stopwatch.ElapsedMilliseconds;
        Console.WriteLine("execution took: " + elapsedMs);
        Assert.IsTrue(elapsedMs > 900 && elapsedMs < 1200);


        stopwatch = System.Diagnostics.Stopwatch.StartNew();
        e = await Assert.ThrowsExceptionAsync<ActionScriptExecutionException>(async () =>
       {
           await InternalScriptManager.ExecuteUnfinishedScriptBySourceCode(sourceCode5000, TestHelper.GetContext(), nameof(IExecuteAsync.ExecuteAsync));
       });
        stopwatch.Stop();

        elapsedMs = stopwatch.ElapsedMilliseconds;
        Console.WriteLine("execution took: " + elapsedMs);
        Assert.IsTrue(elapsedMs > 4900 && elapsedMs < 5100);
    }

    [TestMethod]
    public void TestRecentUsingTOString()
    {
        Console.WriteLine("Recent ActionResult: " + typeof(RecentActionResult).FullName);

        foreach (var item in RecentTypeHelper.GetRecentTypes())
        {
            Console.WriteLine("Type: " + item.Name + ", FullName: " + item.FullName);
        }

        // Assert.IsTrue(false);
    }
}