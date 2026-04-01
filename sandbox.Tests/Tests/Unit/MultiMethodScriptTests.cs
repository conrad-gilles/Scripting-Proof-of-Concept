using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ember.Simulation;
using ScriptMethodManager;
using Sandbox;
using Microsoft.CodeAnalysis.Scripting;

[TestClass]
public class MultiMethodScriptTests
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
    public async Task RunMultiMethodScript()
    {
        string sourceCode = TestHelper.GetSC().sourceCodeMultiMethodScripts;

        //define specific method in the methodName parameter
        CustomerScript script = await ScriptManager.CreateScript(sourceCode);
        ActiveActionResult ar = await InternalScriptManager!.ExecuteScript<IGeneratorActionScript>
        (script.ScriptName!, TestHelper.GetContext(), methodName: "ExecuteAction1");

        Console.WriteLine(ar.ToString());
        Assert.IsTrue(ar.ToString().Contains("ExecuteAction1 was called"));

        Exception ex = await Assert.ThrowsExceptionAsync<Ember.Scripting.ActionScriptExecutionException>(async () =>
        {
            ar = await InternalScriptManager!.ExecuteScript<IGeneratorActionScript>
           (script.ScriptName!, TestHelper.GetContext(), methodName: "ExecuteAction2");
        });

        // Can explicitly call default method
        ar = await InternalScriptManager!.ExecuteScript<IGeneratorActionScript>
       (script.ScriptName!, TestHelper.GetContext(), methodName: "ExecuteAsync");

        Console.WriteLine(ar.ToString());
        Assert.IsTrue(ar.ToString().Contains("Default method ExecuteAsync was called"));

        // if no specific method defined fall back to default
        ar = await InternalScriptManager!.ExecuteScript<IGeneratorActionScript>
        (script.ScriptName!, TestHelper.GetContext());

        Console.WriteLine(ar.ToString());
        Assert.IsTrue(ar.ToString().Contains("Default method ExecuteAsync was called"));

        // Assert.IsTrue(false);
    }


    [TestMethod]
    public async Task RunMultiMethodScriptUsingMethodHelper()
    {
        //first we define the script and insert it into the db
        string sourceCode = TestHelper.GetSC().sourceCodeMultiMethodScripts;
        CustomerScript scriptDB = await ScriptManager.CreateScript(sourceCode);

        //we create the ScriptHelper
        MultipleMethodsScriptHelper script = new MultipleMethodsScriptHelper(ScriptManager, InternalScriptManager, scriptDB.ScriptName!);

        //define specific method in the methodName parameter
        ActiveActionResult ar = await script.ExecuteAction1(TestHelper.GetContext());

        Console.WriteLine(ar.ToString());
        Assert.IsTrue(ar.ToString().Contains("ExecuteAction1 was called"));

        Exception ex = await Assert.ThrowsExceptionAsync<Ember.Scripting.ActionScriptExecutionException>(async () =>
        {
            ar = await script.ExecuteAction2(TestHelper.GetContext());
        });

        // Can explicitly call default method
        ar = await script.ExecuteAsync(TestHelper.GetContext());

        Console.WriteLine(ar.ToString());
        Assert.IsTrue(ar.ToString().Contains("Default method ExecuteAsync was called"));

        // Assert.IsTrue(false);
    }

    [TestMethod]
    public async Task NegativeTest()
    {
        string sourceCode = TestHelper.GetSC().sourceCodeMultiMethodScripts;
        CustomerScript scriptDB = await ScriptManager.CreateScript(sourceCode);

        Exception ex = await Assert.ThrowsExceptionAsync<Ember.Scripting.CouldNotFindMethodException>(async () =>
        {
            ActiveActionResult ar = await InternalScriptManager!.ExecuteScript<IGeneratorActionScript>
            (scriptDB.ScriptName!, TestHelper.GetContext(), methodName: "MethodDoesntExist");
        });

        ExceptionHelper.PrintExceptionListToConsole(ex);

        sourceCode = TestHelper.GetSC().sourceCodeActionV3;
        scriptDB = await ScriptManager.CreateScript(sourceCode);

        ex = await Assert.ThrowsExceptionAsync<Ember.Scripting.CouldNotFindMethodException>(async () =>
       {
           ActiveActionResult ar = await InternalScriptManager!.ExecuteScript<IGeneratorActionScript>
               (scriptDB.ScriptName!, TestHelper.GetContext(), methodName: "ExecuteAction1");
       });
        ExceptionHelper.PrintExceptionListToConsole(ex);
        // Assert.IsTrue(false);
    }

    [TestMethod]

    public async Task TestValidationNormalScriptAsync()
    {
        Console.WriteLine("Source Code 1:");
        string sourceCode = TestHelper.GetSC().sourceCodeActionV2;
        CustomerScript scriptDB = await ScriptManager.CreateScript(sourceCode);

        ValidationRecord vr = ScriptManager.BasicValidationBeforeCompiling(sourceCode);

        Console.WriteLine("Source Code 2:");
        sourceCode = TestHelper.GetSC().sourceCodeActionV3;
        scriptDB = await ScriptManager.CreateScript(sourceCode);

        vr = ScriptManager.BasicValidationBeforeCompiling(sourceCode);

    }

    [TestMethod]
    public async Task TestValidationShouldThrowAsync()
    {
        string sourceCode = TestHelper.GetSC().undefinedMethodsScriptPublic;

        UndefinedMethodException ex = await Assert.ThrowsExceptionAsync<UndefinedMethodException>(async () =>
             {
                 ValidationRecord vr = ScriptManager.BasicValidationBeforeCompiling(sourceCode);
             });

        Console.WriteLine(ex.Method);
        Assert.IsTrue(ex.Method!.Name == nameof(PublicScript.SomeUndefindedMethod));
        Assert.IsTrue(ex.Method.ReturnType == "Task`1");

        sourceCode = TestHelper.GetSC().undefinedMethodsScriptPrivate;

        ex = await Assert.ThrowsExceptionAsync<UndefinedMethodException>(async () =>
            {
                ValidationRecord vr = ScriptManager.BasicValidationBeforeCompiling(sourceCode);
            });

        Console.WriteLine(ex.Method);
        Assert.IsTrue(ex.Method!.Name == "SomeUndefindedPrivateMethod");
        Assert.IsTrue(ex.Method.ReturnType == "Task`1");

        sourceCode = TestHelper.GetSC().undefinedMethodsScriptInternal;

        ex = await Assert.ThrowsExceptionAsync<UndefinedMethodException>(async () =>
            {
                ValidationRecord vr = ScriptManager.BasicValidationBeforeCompiling(sourceCode);
            });

        Console.WriteLine(ex.Method);
        Assert.IsTrue(ex.Method!.Name == nameof(InternalScript.SomeUndefindedInternalMethod));
        Assert.IsTrue(ex.Method.ReturnType == "Task`1");

        sourceCode = TestHelper.GetSC().undefinedMethodsScriptStatic;

        ex = await Assert.ThrowsExceptionAsync<UndefinedMethodException>(async () =>
            {
                ValidationRecord vr = ScriptManager.BasicValidationBeforeCompiling(sourceCode);
            });

        Console.WriteLine(ex.Method);
        Assert.IsTrue(ex.Method!.Name == nameof(StaticScript.SomeUndefindedStaticMethod));
        Assert.IsTrue(ex.Method.ReturnType == "Task`1");

        // Assert.IsFalse(true);
    }
}