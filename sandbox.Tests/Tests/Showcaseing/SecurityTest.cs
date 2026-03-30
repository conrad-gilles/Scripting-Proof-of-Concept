using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ember.Simulation;
using Sandbox;

[TestClass]
public class SecurityTests
{
    static ISccriptManagerDeleteAfter ScriptManager = TestHelper.InitScriptManager();
    static EmberInternalFacade InternalScriptManager = new EmberInternalFacade(ScriptManager);
    static EmberMethods em = new EmberMethods(ScriptManager);

    [TestInitialize]
    public async Task SetupAsync()
    {
        await ScriptManager.DeleteAllData();
    }
    [TestMethod]
    public async Task IllegalUsingTest()
    {
        string sourceCode = TestHelper.GetSC().sourceCodeIllegalUsings;
        Exception ex = await Assert.ThrowsExceptionAsync<Ember.Scripting.ForbiddenNamespaceException>(async () =>
        {
            CustomerScript script = await ScriptManager.CreateScript(sourceCode);
        });
    }
    [TestMethod]
    public async Task MissingUsingTest()
    {
        string sourceCode = TestHelper.GetSC().sourceCodeMissingUsing;
        Ember.Scripting.CompilationFailedException ex = await Assert.ThrowsExceptionAsync<Ember.Scripting.CompilationFailedException>(async () =>
        {
            CustomerScript script = await ScriptManager.CreateScript(sourceCode);
        });

        ExceptionHelper.PrintExceptionListToConsole(ex);

        foreach (var item in ex.Errors)
        {
            Console.WriteLine(item.ToString());
        }
    }
    [TestMethod]
    public async Task PreventUsageTest()
    {
        string sourceCode = TestHelper.GetSC().sourceCodePreventUsage;
        Exception ex = await Assert.ThrowsExceptionAsync<Ember.Scripting.ForbiddenTypeAccessException>(async () =>
        {
            CustomerScript script = await ScriptManager.CreateScript(sourceCode);
        });
    }
    [TestMethod]
    public async Task WhileTrueScript()
    {
        await Assert.ThrowsExceptionAsync<Ember.Scripting.ActionScriptExecutionException>(async () =>
        {
            string sourceCode = TestHelper.GetSC().sourceCodeWhileTrue;
            // ScriptNameType scriptRecord = await ScriptManager!.CreateScriptUsingNameType(sourceCode!);
            CustomerScript script = await ScriptManager!.CreateScript(sourceCode!);

            var obj = TestHelper.ScriptObjects();
            var services = new ServiceCollection();
            Ember.Simulation.SandboxServiceCollectionExtensions.AddSandboxServices
            // (services, obj.labOrder, obj.patient, obj.logger, obj.testDataAccess, obj.vaccine);
            (services, obj.logger, obj.testDataAccess);

            using var provider = services.BuildServiceProvider();
            ActiveContextFactory.IGeneratorContextFactory factory = provider.GetRequiredService<ActiveContextFactory.IGeneratorContextFactory>();
            ActiveGeneratorContext ctx = factory.Create(obj.labOrder, obj.vaccine);
            ActiveActionResult ar = await InternalScriptManager!.ExecuteScript<IGeneratorActionScript>("WhileTrueScript", ctx);

            Console.WriteLine("Name: " + script.ScriptName + ", ScriptType: " + script.ScriptType);
            Console.WriteLine("Type name: " + ar.GetType().FullName);
            Console.WriteLine("Returned result: " + ar);
        });
    }

    [TestMethod]
    public void BasicValidationBeforeCompilingTest()
    {
        string sourceCode = "";

        // Exception when Source code is empty
        Exception ex = Assert.ThrowsException<Ember.Scripting.ScriptWasEmptyOrNullException>(() =>
        {
            ScriptManager.BasicValidationBeforeCompiling(sourceCode);
        });
        ExceptionHelper.PrintExceptionListToConsole(ex);

        sourceCode = "public class ShouldFail{}";

        //Exception when Script doesnt inherit from the predefined classes      
        ex = Assert.ThrowsException<Ember.Scripting.ScriptFieldNullException>(() =>
       {
           ScriptManager.BasicValidationBeforeCompiling(sourceCode);
       });
        Assert.IsTrue(ExceptionHelper.GetExceptionFromChainReversed(ex, 0).GetType() == typeof(VersionIntNotAssignedException));
        ExceptionHelper.PrintExceptionListToConsole(ex);

        sourceCode = TestHelper.GetSC().sourceCodeIllegalUsings;

        //Exception when the scripts has a using at the top that is defined ass illegal, source code: file=IllegalUsingScript.cs
        ex = Assert.ThrowsException<Ember.Scripting.ForbiddenNamespaceException>(() =>
               {
                   ScriptManager.BasicValidationBeforeCompiling(sourceCode);
               });
        ExceptionHelper.PrintExceptionListToConsole(ex);

        sourceCode = TestHelper.GetSC().sourceCodePreventUsage;

        //Exception when there is a forbidden Type access like System.IO.BufferedStream? stream for example, source code: file=PreventUsageScript.cs
        ex = Assert.ThrowsException<Ember.Scripting.ForbiddenTypeAccessException>(() =>
               {
                   ScriptManager.BasicValidationBeforeCompiling(sourceCode);
               });
        ExceptionHelper.PrintExceptionListToConsole(ex);

        sourceCode = TestHelper.GetSC().sourceCodeWhileTrueUnsafe;

        //Exception when there is a loop that does not contain a check for the cancellation token, source code: file=WhileTrueScript.cs
        ex = Assert.ThrowsException<Ember.Scripting.ConcellationTokenUncheckedException>(() =>
           {
               ScriptManager.BasicValidationBeforeCompiling(sourceCode);
           });
        ExceptionHelper.PrintExceptionListToConsole(ex);


        sourceCode = TestHelper.GetSC().sourceCodeMultipleClasses;

        //Exception when there is a more than one class (script) defined in a file (or db entry), source code: file=MultipleClassesScript.cs
        ex = Assert.ThrowsException<Ember.Scripting.MoreThanOneClassFoundInScriptException>(() =>
           {
               ScriptManager.BasicValidationBeforeCompiling(sourceCode);
           });
        ExceptionHelper.PrintExceptionListToConsole(ex);

        // sourceCode = TestHelper.GetSC().sourceCodeMissingUsing;  //todo doesnt work in validation yet only fails when trying to compile

        // ex = Assert.ThrowsException<Ember.Scripting.ValidationBeforeCompilationException>(() =>
        //        {
        //            ScriptManager.BasicValidationBeforeCompiling(sourceCode);
        //        });
        // ExceptionHelper.PrintExceptionListToConsole(ex);
    }
    [TestMethod]
    public void ExecutionTimeoutTest()
    {
        string sourceCode = TestHelper.GetSC().sourceCodeExecutionTimeTest;
        int expectedMS = 1000;
        int? realMS = ScriptManager.BasicValidationBeforeCompiling(sourceCode).ExecutionTime;

        Console.WriteLine("Source code:" + sourceCode);
        Console.WriteLine("Real MS: " + realMS);

        Console.WriteLine("Test name: " + nameof(ExecutionTimeGroups.Short));
        Assert.IsTrue(expectedMS == realMS);

        sourceCode = """
        using System;   //todo this is possible to default in compiler
        using System.Threading.Tasks;
        using System.Collections.Generic;   //todo same for them
        using Ember.Scripting;
        using GeneratorScriptsGeneric;
        using IGeneratorContext_V2;

        // [ExecutionTime(ExecutionTimeGroups.Long)]
        [ExecutionTime(1333)]
        public class ExecutionTimeTest : GeneratorScriptsGeneric.IGeneratorActionScript<IGeneratorContext_V2.IGeneratorContext, ActionResultV1.ActionResult>
        {
        public async Task<ActionResultV1.ActionResult> ExecuteAsync(IGeneratorContext_V2.IGeneratorContext context)    //error is here in Basyns
        {
            return ActionResultV1.ActionResult.Success("Pediatric tests added");
        }
        }
        """;

        expectedMS = 1333;
        realMS = ScriptManager.BasicValidationBeforeCompiling(sourceCode).ExecutionTime;
        Assert.IsTrue(expectedMS == realMS);

        sourceCode = """
        using System;   //todo this is possible to default in compiler
        using System.Threading.Tasks;
        using System.Collections.Generic;   //todo same for them
        using Ember.Scripting;
        using GeneratorScriptsGeneric;
        using IGeneratorContext_V2;

        [ExecutionTime(6000)]
        public class ExecutionTimeTest : GeneratorScriptsGeneric.IGeneratorActionScript<IGeneratorContext_V2.IGeneratorContext, ActionResultV1.ActionResult>
        {
        public async Task<ActionResultV1.ActionResult> ExecuteAsync(IGeneratorContext_V2.IGeneratorContext context)    //error is here in Basyns
        {
            return ActionResultV1.ActionResult.Success("Pediatric tests added");
        }
        }
        """;

        expectedMS = 5000;  //shows how the maximum is respected and not overstepped
        realMS = ScriptManager.BasicValidationBeforeCompiling(sourceCode).ExecutionTime;
        Assert.IsTrue(expectedMS == realMS);

        sourceCode = """
        using System;   //todo this is possible to default in compiler
        using System.Threading.Tasks;
        using System.Collections.Generic;   //todo same for them
        using Ember.Scripting;
        using GeneratorScriptsGeneric;
        using IGeneratorContext_V2;

        [ExecutionTime(1)]
        public class ExecutionTimeTest : GeneratorScriptsGeneric.IGeneratorActionScript<IGeneratorContext_V2.IGeneratorContext, ActionResultV1.ActionResult>
        {
        public async Task<ActionResultV1.ActionResult> ExecuteAsync(IGeneratorContext_V2.IGeneratorContext context)    //error is here in Basyns
        {
            return ActionResultV1.ActionResult.Success("Pediatric tests added");
        }
        }
        """;

        expectedMS = 100;  //shows how the minimum is respected and not overstepped
        realMS = ScriptManager.BasicValidationBeforeCompiling(sourceCode).ExecutionTime;
        Assert.IsTrue(expectedMS == realMS);
    }

}
