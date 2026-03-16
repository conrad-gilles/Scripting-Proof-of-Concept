using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;

[TestClass]
public class ScriptTests
{


    ISccriptManagerDeleteAfter? facade;
    EmberMethods? em;
    string? ActionResultVersionSpecific;
    string? sourceCodeActionV1 = TestHelper.GetSC().sourceCodeActionV1;
    string? sourceCodeActionV2 = TestHelper.GetSC().sourceCodeActionV2;
    string? sourceCodeActionV3 = TestHelper.GetSC().sourceCodeActionV3;
    string? sourceCodeVaccineAction = TestHelper.GetSC().sourceCodeVaccineAction;
    string? sourceCodePedia = TestHelper.GetSC().sourceCodePedia;


    [TestInitialize]
    public
async Task SetupAsync()
    {
        facade = EmberMethods.GetNewScriptManagerInstance();
        em = new EmberMethods(facade);
        await facade.DeleteAllData();

        ActionResultVersionSpecific = "[Message contains either failure or succes: ] ";
    }


    [TestMethod]
    public async Task ActionV1Test()
    {
        // await facade!.EnsureDeletedCreated();
        await facade!.DeleteAllData();
        Guid id = await facade!.CreateScript(sourceCodeActionV1!);
        CustomerScript retrievedScript = await facade.GetScript(id);
        var context = await em!.GetTestingContext<GeneratorContextNoInherVaccineV5.GeneratorContext>(autoDetectFromScript: retrievedScript);
        object resultBeforeUpgrade = await facade.ExecuteScriptById(id, context);
        ActionResultV3.ActionResult result = (ActionResultV3.ActionResult)EmberMethods.UpgradeActionResult(resultBeforeUpgrade);
        string shouldReturn = ActionResultVersionSpecific + "Pediatric tests added";
        Assert.IsInstanceOfType(result, typeof(ActionResultSF));
        Assert.IsInstanceOfType(result, typeof(ActionResultV3.ActionResult));
        Assert.IsTrue(result.ToString().Contains(shouldReturn));

        //todo make negative test
    }
    [TestMethod]
    public async Task ActionV2Test()
    {
        Guid id = await facade!.CreateScript(sourceCodeActionV2!);
        CustomerScript retrievedScript = await facade.GetScript(id);
        var context = await em!.GetTestingContext<GeneratorContextNoInherVaccineV5.GeneratorContext>(autoDetectFromScript: retrievedScript);
        object resultBeforeUpgrade = await facade.ExecuteScriptById(id, context);
        ActionResultV3.ActionResult result = (ActionResultV3.ActionResult)EmberMethods.UpgradeActionResult(resultBeforeUpgrade);
        string shouldReturn = ActionResultVersionSpecific + "Pediatric tests added";
        Assert.IsInstanceOfType(result, typeof(ActionResultSF));
        Assert.IsInstanceOfType(result, typeof(ActionResultV3.ActionResult));
        Assert.IsTrue(result.ToString().Contains(shouldReturn));

        //todo make negative test
    }
    [TestMethod]
    public async Task ActionV3Test()
    {
        Guid id = await facade!.CreateScript(sourceCodeActionV3!);
        CustomerScript retrievedScript = await facade.GetScript(id);
        var context = await em!.GetTestingContext<GeneratorContextNoInherVaccineV5.GeneratorContext>(autoDetectFromScript: retrievedScript);
        object resultBeforeUpgrade = await facade.ExecuteScriptById(id, context);
        ActionResultV3.ActionResult result = (ActionResultV3.ActionResult)EmberMethods.UpgradeActionResult(resultBeforeUpgrade);
        string shouldReturn = ActionResultVersionSpecific + "Pediatric tests added V3";
        Assert.IsInstanceOfType(result, typeof(ActionResultSF));
        Assert.IsInstanceOfType(result, typeof(ActionResultV3.ActionResult));
        Assert.IsTrue(result.ToString().Contains(shouldReturn));

        //todo make negative test
    }
    [TestMethod]
    public async Task VaccineScriptTest()
    {
        Guid id = await facade!.CreateScript(sourceCodeVaccineAction!);
        CustomerScript retrievedScript = await facade.GetScript(id);
        var context = await em!.GetTestingContext<GeneratorContextNoInherVaccineV5.GeneratorContext>(autoDetectFromScript: retrievedScript);
        object result = await facade.ExecuteScriptById(id, context);
        string shouldReturn = ActionResultVersionSpecific + "Polio Vaccine added";
        Assert.IsInstanceOfType(result, typeof(ActionResultSF));
        Assert.IsInstanceOfType(result, typeof(ActionResultV3.ActionResult));
        Assert.IsTrue(result.ToString()!.Contains(shouldReturn));

        //todo make negative test
    }

    [TestMethod]
    public async Task ConditionTest()
    {
        Guid id = await facade!.CreateScript(sourceCodePedia!);
        CustomerScript retrievedScript = await facade.GetScript(id);
        var context = await em!.GetTestingContext<GeneratorContextNoInherVaccineV5.GeneratorContext>(autoDetectFromScript: retrievedScript);
        object result = await facade.ExecuteScriptById(id, context);
        string shouldReturn = "True";
        Assert.IsInstanceOfType(result, typeof(bool));
        Assert.IsTrue(result.ToString()!.Contains(shouldReturn));

        //todo make negative test
    }
    // [TestMethod]
    // public async Task RunAllScripts()
    // {
    //     await facade.ClearAllCaches();
    //     await facade.CompileAllScripts();
    //     var allScripts = await facade.ListScripts();
    //     Assert.IsTrue(allScripts.Count() == 5);
    //     foreach (var item in allScripts)
    //     {
    //         Guid id = item.Id;
    //         CustomerScript retrievedScript = await facade.GetScript(id);
    //         var context = UsefulMethods.GetTestingContext<GeneratorContextNoInherVaccine>(justForTesting: retrievedScript);
    //         object result = await facade.ExecuteScriptById(id, context);
    //         Assert.IsTrue(result != null);
    //         Assert.IsTrue(result is bool || result is ActionResultBaseClass);
    //     }
    //     //todo make negative test
    // }

    [TestMethod]
    public async Task TestCreatedAt()
    {
        // await facade!.EnsureDeletedCreated();
        await facade!.DeleteAllData();
        // await rm!.CompileAllScriptsInFolderAndSaveToDB(path, "Default", await facade.GetRecentApiVersion());

        Guid id = await facade!.CreateScript(sourceCodePedia!);
        CustomerScript retrievedScript = await facade.GetScript(id);
        DateTime? beforeUpdateCA = retrievedScript.CreatedAt;
        DateTime? beforeUpdateMA = retrievedScript.ModifiedAt;

        var context = await em!.GetTestingContext<GeneratorContextNoInherVaccineV5.GeneratorContext>(autoDetectFromScript: retrievedScript);
        object result = await facade.ExecuteScriptById(id, context);

        await Task.Delay(2000);
        await facade!.UpdateScript(retrievedScript.Id, "new source code doesnt matter if wrong shoudl save to db");

        CustomerScript updatedScript = await facade.GetScript(id);
        DateTime? afterUpdateCA = updatedScript.CreatedAt;
        DateTime? afterUpdateMA = updatedScript.ModifiedAt;

        // Assert that CreatedAt remained exactly the same
        Assert.AreEqual(beforeUpdateCA, afterUpdateCA, "CreatedAt should not change on update.");

        // Assert that ModifiedAt actually changed and is now greater than CreatedAt
        Assert.IsTrue(afterUpdateMA > beforeUpdateMA, "ModifiedAt should be updated to a newer time.");
        Assert.IsTrue(afterUpdateMA > afterUpdateCA, "ModifiedAt should be greater than CreatedAt after an edit.");
    }

    // [TestMethod]
    // public async Task BasicValidationTest()
    // {
    //     string scriptString = @"    public class AddPediatricTestsV2 : IGeneratorActionScriptV2
    //                                 {public async Task<ActionResultV2> ExecuteAsync(IGeneratorContext_V2.IGeneratorContext context){}}";

    //     var returnedValidation = facade!.BasicValidationBeforeCompiling(sourceCodeActionV1!);
    //     Assert.IsTrue(returnedValidation.baseTypeName == "");
    //     Assert.IsTrue(returnedValidation.className == "");
    //     Assert.IsTrue(returnedValidation.versionInt == 1);

    //     await Assert.ThrowsExceptionAsync<ValidationBeforeCompilationException>(async () =>
    //      {
    //          var returnedValidation = facade!.BasicValidationBeforeCompiling(scriptString!);
    //      });
    // }


}
