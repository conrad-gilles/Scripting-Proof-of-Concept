#pragma warning disable CS0436

using Ember.Scripting.Compilation;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ember.Simulation;
// using ScriptMethodManager;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[TestClass]
public class MultiMethodScriptTests
{
    static IScriptManagerBaseExtended ScriptManager = TestHelper.InitScriptManager();
    static ScriptManager InternalScriptManager = new ScriptManager(ScriptManager);
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
        RecentActionResult ar = (RecentActionResult)await InternalScriptManager!.ExecuteScript<IActionScriptBase>
        (script.ScriptName!, TestHelper.GetContext(), methodName: "Execute1");

        Console.WriteLine(ar.ToString());
        Assert.IsTrue(ar.ToString().Contains("ExecuteAction1 was called"));

        Exception ex = await Assert.ThrowsExceptionAsync<ActionScriptExecutionException>(async () =>
        {
            ar = (RecentActionResult)await InternalScriptManager!.ExecuteScript<IActionScriptBase>
           (script.ScriptName!, TestHelper.GetContext(), methodName: "Execute2");
        });

        // Can explicitly call default method
        ar = (RecentActionResult)await InternalScriptManager!.ExecuteScript<IActionScriptBase>
       (script.ScriptName!, TestHelper.GetContext(), methodName: "ExecuteAsync");

        Console.WriteLine(ar.ToString());
        Assert.IsTrue(ar.ToString().Contains("Default method ExecuteAsync was called"));

        // if no specific method defined fall back to default
        ar = (RecentActionResult)await InternalScriptManager!.ExecuteScript<IActionScriptBase>
        (script.ScriptName!, TestHelper.GetContext(), nameof(RecentIActionScript.ExecuteAsync));

        Console.WriteLine(ar.ToString());
        Assert.IsTrue(ar.ToString().Contains("Default method ExecuteAsync was called"));

        // Assert.IsTrue(false);
    }


    // [TestMethod]
    // public async Task RunMultiMethodScriptUsingMethodHelper()
    // {
    //     //first we define the script and insert it into the db
    //     string sourceCode = TestHelper.GetSC().sourceCodeMultiMethodScripts;
    //     CustomerScript scriptDB = await ScriptManager.CreateScript(sourceCode);

    //     //we create the ScriptHelper
    //     MultipleMethodsScriptHelper script = new MultipleMethodsScriptHelper(ScriptManager, InternalScriptManager, scriptDB.ScriptName!);

    //     //define specific method in the methodName parameter
    //     RecentActionResult ar = await script.Execute1(TestHelper.GetContext());

    //     Console.WriteLine(ar.ToString());
    //     Assert.IsTrue(ar.ToString().Contains("ExecuteAction1 was called"));

    //     Exception ex = await Assert.ThrowsExceptionAsync<ActionScriptExecutionException>(async () =>
    //     {
    //         ar = await script.Execute2(TestHelper.GetContext());
    //     });

    //     // Can explicitly call default method
    //     ar = await script.ExecuteAsync(TestHelper.GetContext());

    //     Console.WriteLine(ar.ToString());
    //     Assert.IsTrue(ar.ToString().Contains("Default method ExecuteAsync was called"));

    //     // Assert.IsTrue(false);
    // }

    [TestMethod]
    public async Task NegativeTest()
    {
        string sourceCode = TestHelper.GetSC().sourceCodeMultiMethodScripts;
        CustomerScript scriptDB = await ScriptManager.CreateScript(sourceCode);

        Exception ex = await Assert.ThrowsExceptionAsync<CouldNotFindMethodException>(async () =>
        {
            RecentActionResult ar = (RecentActionResult)await InternalScriptManager!.ExecuteScript<IActionScriptBase>
            (scriptDB.ScriptName!, TestHelper.GetContext(), methodName: "MethodDoesntExist");
        });

        ExceptionHelper.PrintExceptionListToConsole(ex);

        sourceCode = TestHelper.GetSC().sourceCodeActionV3;
        scriptDB = await ScriptManager.CreateScript(sourceCode);

        ex = await Assert.ThrowsExceptionAsync<CouldNotFindMethodException>(async () =>
       {
           RecentActionResult ar = (RecentActionResult)await InternalScriptManager!.ExecuteScript<IActionScriptBase>
               (scriptDB.ScriptName!, TestHelper.GetContext(), methodName: "ExecuteAction1");
       });
        ExceptionHelper.PrintExceptionListToConsole(ex);
        // Assert.IsTrue(false);

        //Pseudo
        // IGeneratorActionScript script = ScriptManager.GetScript<IGeneratorActionScript>("name");
        // script.Execute1(context);
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

        UndefinedMethodException ex = new UndefinedMethodException();

        ex = await Assert.ThrowsExceptionAsync<UndefinedMethodException>(async () =>
             {
                 ValidationRecord vr2 = ScriptManager.BasicValidationBeforeCompiling(sourceCode);
             });

        //returns: Ember.Scripting.Compilation.UndefinedMethodException: No new methods allowed that are not predefinded!MethodRecord: Name: SomeUndefindedMethod, ReturnType: Task`1, Parameters: System.Collections.Generic.List`1[Ember.Scripting.Compilation.ParameterRecord]

        Console.WriteLine(ex.Method);
        Assert.IsTrue(ex.Method!.Name == nameof(PublicScript.SomeUndefindedMethod));
        Console.WriteLine(ex.Method.ReturnType);
        Assert.IsTrue(ex.Method.ReturnType == "string");

        sourceCode = TestHelper.GetSC().undefinedMethodsScriptPrivate;

        // ex = await Assert.ThrowsExceptionAsync<UndefinedMethodException>(async () =>
        //     {
        ValidationRecord vr = ScriptManager.BasicValidationBeforeCompiling(sourceCode);
        // });

        // Console.WriteLine(ex.Method);
        // Assert.IsTrue(ex.Method!.Name == "SomeUndefindedPrivateMethod");
        // Assert.IsTrue(ex.Method.ReturnType == "Task`1");

        sourceCode = TestHelper.GetSC().undefinedMethodsScriptInternal;

        ex = await Assert.ThrowsExceptionAsync<UndefinedMethodException>(async () =>
            {
                ValidationRecord vr2 = ScriptManager.BasicValidationBeforeCompiling(sourceCode);
            });

        //returns: Ember.Scripting.Compilation.UndefinedMethodException: No new methods allowed that are not predefinded!MethodRecord: Name: SomeUndefindedInternalMethod, ReturnType: Task`1, Parameters: System.Collections.Generic.List`1[Ember.Scripting.Compilation.ParameterRecord]

        Console.WriteLine(ex.Method);
        Assert.IsTrue(ex.Method!.Name == nameof(InternalScript.SomeUndefindedInternalMethod));
        Console.WriteLine(ex.Method.ReturnType);
        Assert.IsTrue(ex.Method.ReturnType == "string");

        sourceCode = TestHelper.GetSC().undefinedMethodsScriptStatic;

        ex = await Assert.ThrowsExceptionAsync<UndefinedMethodException>(async () =>
            {
                ValidationRecord vr3 = ScriptManager.BasicValidationBeforeCompiling(sourceCode);
            });

        // returns: Ember.Scripting.Compilation.UndefinedMethodException: No new methods allowed that are not predefinded!MethodRecord: Name: SomeUndefindedStaticMethod, ReturnType: Task`1, Parameters: System.Collections.Generic.List`1[Ember.Scripting.Compilation.ParameterRecord] 

        Console.WriteLine(ex.Method);
        Assert.IsTrue(ex.Method!.Name == nameof(StaticScript.SomeUndefindedStaticMethod));
        Assert.IsTrue(ex.Method.ReturnType == "string");

        // Assert.IsFalse(true);
    }

    [TestMethod]
    public async Task MultiMethodsInEmberInternalFacadeTestAsync()
    {
        string sourceCode = TestHelper.GetSC().sourceCodeMultiMethodScripts;
        CustomerScript scriptDB = await ScriptManager.CreateScript(sourceCode);

        // RecentActionResult ar = (RecentActionResult)await InternalScriptManager.ExecuteScript<IGeneratorActionScript>(scriptDB.ScriptName!, TestHelper.GetContext());
        var script = InternalScriptManager.GetScript<ActionScript>(scriptDB.ScriptName!);
        RecentActionResult ar = await script.ExecuteAsync(TestHelper.GetContext());
        Assert.IsTrue(ar.ToString().Contains("Default method ExecuteAsync was called"));

        // ar = (RecentActionResult)await InternalScriptManager.Execute1<IGeneratorActionScript>(scriptDB.ScriptName!, TestHelper.GetContext());
        script = InternalScriptManager.GetScript<ActionScript>(scriptDB.ScriptName!);
        ar = await script.Execute1(TestHelper.GetContext());
        Assert.IsTrue(ar.ToString().Contains("ExecuteAction1 was called"));

        Exception ex = await Assert.ThrowsExceptionAsync<ActionScriptExecutionException>(async () =>
     {
         //  ar = (RecentActionResult)await InternalScriptManager.Execute2<IGeneratorActionScript>(scriptDB.ScriptName!, TestHelper.GetContext());
         ar = await script.Execute2(TestHelper.GetContext());
     });
    }

    [TestMethod]
    public async Task GetScriptAndExecuteSpecificMethodTest()
    {
        //Setting up the script anc context and inserting it into the DB
        string sourceCode = TestHelper.GetSC().sourceCodeMultiMethodScripts;
        var context = TestHelper.GetContext();
        // CompilationFailedException ex5 = await Assert.ThrowsExceptionAsync<CompilationFailedException>(async () =>
        //          {
        //              CustomerScript scriptDB = await ScriptManager.CreateScript(sourceCode);
        //          });
        // foreach (var item in ex5.Errors)
        // {
        //     Console.WriteLine(item);
        // }
        CustomerScript scriptDB = await ScriptManager.CreateScript(sourceCode);

        //Getting the Script
        var script = InternalScriptManager.GetScript<ActionScript>(scriptDB.ScriptName!);

        //Executing the first function
        RecentActionResult ar = (RecentActionResult)await script.ExecuteAsync(context);
        Assert.IsTrue(ar.ToString().Contains("Default method ExecuteAsync was called"));

        //Executing the second function
        ar = (RecentActionResult)await script.Execute1(context);
        Assert.IsTrue(ar.ToString().Contains("ExecuteAction1 was called"));

        Exception ex = await Assert.ThrowsExceptionAsync<ActionScriptExecutionException>(async () =>
        {
            //Executing the third function (normal that it throws an exception)
            ar = (RecentActionResult)await script.Execute2(context);
        });

        //Setting up a new script that does not implement the extra Methods
        sourceCode = TestHelper.GetSC().sourceCodeVaccineAction;
        scriptDB = await ScriptManager.CreateScript(sourceCode);

        script = InternalScriptManager.GetScript<ActionScript>(scriptDB.ScriptName!);

        //Executing the first function that exists
        ar = (RecentActionResult)await script.ExecuteAsync(context);
        Assert.IsTrue(ar.ToString().Contains("Vaccine added"));

        //Trying to execute a function that is not implemented in the Script
        ex = await Assert.ThrowsExceptionAsync<CouldNotFindMethodException>(async () =>
       {
           ar = (RecentActionResult)await script.Execute1(context);
       });

        //Trying to call a condition method on an action script wont even compile
        // ex = await Assert.ThrowsExceptionAsync<System.InvalidCastException>(async () =>
        // {
        // var result1 = await script.EvaluateAsync(context);   //uncomment
        // });

        //Setting up a condition Script
        sourceCode = TestHelper.GetSC().sourceCodePedia;
        scriptDB = await ScriptManager.CreateScript(sourceCode);
        ConditionScript condScript = InternalScriptManager.GetScript<ConditionScript>(scriptDB.ScriptName!);

        //Executing the evaluate function
        bool result = await condScript.EvaluateAsync(context);

        Console.WriteLine("Result: " + result);
        Assert.IsTrue(result);

        //Trying to execute an action method on an Condition Script wont compile
        // result=await condScript.Execute1(context);   //uncomment
    }

    // [TestMethod]
    // public void NegativeTest2()
    // {

    // }

    [TestMethod]
    public async Task DiffrentReturnTest()
    {
        string sourceCode = TestHelper.GetSC().sourceCodeMultiMethodScripts;
        var context = TestHelper.GetContext();
        CustomerScript scriptDB = await ScriptManager.CreateScript(sourceCode);

        //Getting the Script
        var script = InternalScriptManager.GetScript<ActionScript>(scriptDB.ScriptName!);

        //Executing the first function
        RecentActionResult ar = (RecentActionResult)await script.ExecuteAsync(context);
        Assert.IsTrue(ar.ToString().Contains("Default method ExecuteAsync was called"));
    }
}