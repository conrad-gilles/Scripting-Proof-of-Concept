using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;

[TestClass]
public class ScriptTests
{


    private IScriptManagerDeleteAfter? _facade;
    private EmberMethods? _em;
    private string? _actionResultVersionSpecific;
    private string? _sourceCodeActionV1 = TestHelper.GetSC().sourceCodeActionV1;
    private string? _sourceCodeActionV2 = TestHelper.GetSC().sourceCodeActionV2;
    private string? _sourceCodeActionV3 = TestHelper.GetSC().sourceCodeActionV3;
    private string? _sourceCodeVaccineAction = TestHelper.GetSC().sourceCodeVaccineAction;
    private string? _sourceCodePedia = TestHelper.GetSC().sourceCodePedia;


    [TestInitialize]
    public
async Task SetupAsync()
    {
        _facade = EmberMethods.GetNewScriptManagerInstance();
        _em = new EmberMethods(_facade);
        await _facade.DeleteAllData();

        _actionResultVersionSpecific = "[Message contains either failure or succes: ] ";
    }


    [TestMethod]
    public async Task ActionV1Test()
    {
        // await facade!.EnsureDeletedCreated();
        await _facade!.DeleteAllData();
        Guid id = (await _facade!.CreateScript(_sourceCodeActionV1!)).Id;
        CustomerScript retrievedScript = await _facade.GetScript(id);
        var context = await _em!.GetTestingContext<GeneratorContextNoInherVaccineV5.GeneratorContext>(autoDetectFromScript: retrievedScript);
        object resultBeforeUpgrade = await _facade.ExecuteScriptById(id, context);
        ActionResultV3.ActionResult result = (ActionResultV3.ActionResult)EmberMethods.UpgradeActionResult(resultBeforeUpgrade);
        string shouldReturn = _actionResultVersionSpecific + "Pediatric tests added";
        Assert.IsInstanceOfType(result, typeof(ActionResultSF));
        Assert.IsInstanceOfType(result, typeof(ActionResultV3.ActionResult));
        Assert.IsTrue(result.ToString().Contains(shouldReturn));

        //todo make negative test
    }
    [TestMethod]
    public async Task ActionV2Test()
    {
        Guid id = (await _facade!.CreateScript(_sourceCodeActionV2!)).Id;
        CustomerScript retrievedScript = await _facade.GetScript(id);
        var context = await _em!.GetTestingContext<GeneratorContextNoInherVaccineV5.GeneratorContext>(autoDetectFromScript: retrievedScript);
        object resultBeforeUpgrade = await _facade.ExecuteScriptById(id, context);
        ActionResultV3.ActionResult result = (ActionResultV3.ActionResult)EmberMethods.UpgradeActionResult(resultBeforeUpgrade);
        string shouldReturn = _actionResultVersionSpecific + "Pediatric tests added";
        Assert.IsInstanceOfType(result, typeof(ActionResultSF));
        Assert.IsInstanceOfType(result, typeof(ActionResultV3.ActionResult));
        Assert.IsTrue(result.ToString().Contains(shouldReturn));

        //todo make negative test
    }
    [TestMethod]
    public async Task ActionV3Test()
    {
        Guid id = (await _facade!.CreateScript(_sourceCodeActionV3!)).Id;
        CustomerScript retrievedScript = await _facade.GetScript(id);
        var context = await _em!.GetTestingContext<GeneratorContextNoInherVaccineV5.GeneratorContext>(autoDetectFromScript: retrievedScript);
        object resultBeforeUpgrade = await _facade.ExecuteScriptById(id, context);
        ActionResultV3.ActionResult result = (ActionResultV3.ActionResult)EmberMethods.UpgradeActionResult(resultBeforeUpgrade);
        string shouldReturn = _actionResultVersionSpecific + "Pediatric tests added V3";
        Assert.IsInstanceOfType(result, typeof(ActionResultSF));
        Assert.IsInstanceOfType(result, typeof(ActionResultV3.ActionResult));
        Assert.IsTrue(result.ToString().Contains(shouldReturn));

        //todo make negative test
    }
    [TestMethod]
    public async Task VaccineScriptTest()
    {
        Guid id = (await _facade!.CreateScript(_sourceCodeVaccineAction!)).Id;
        CustomerScript retrievedScript = await _facade.GetScript(id);
        var context = await _em!.GetTestingContext<GeneratorContextNoInherVaccineV5.GeneratorContext>(autoDetectFromScript: retrievedScript);
        object result = await _facade.ExecuteScriptById(id, context);
        string shouldReturn = _actionResultVersionSpecific + "Polio Vaccine added";
        Assert.IsInstanceOfType(result, typeof(ActionResultSF));
        Assert.IsInstanceOfType(result, typeof(ActionResultV3.ActionResult));
        Assert.IsTrue(result.ToString()!.Contains(shouldReturn));

        //todo make negative test
    }

    [TestMethod]
    public async Task ConditionTest()
    {
        Guid id = (await _facade!.CreateScript(_sourceCodePedia!)).Id;
        CustomerScript retrievedScript = await _facade.GetScript(id);
        var context = await _em!.GetTestingContext<GeneratorContextNoInherVaccineV5.GeneratorContext>(autoDetectFromScript: retrievedScript);
        object result = await _facade.ExecuteScriptById(id, context);
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
        await _facade!.DeleteAllData();
        // await rm!.CompileAllScriptsInFolderAndSaveToDB(path, "Default", await facade.GetRecentApiVersion());

        Guid id = (await _facade!.CreateScript(_sourceCodePedia!)).Id;
        CustomerScript retrievedScript = await _facade.GetScript(id);
        DateTime? beforeUpdateCA = retrievedScript.CreatedAt;
        DateTime? beforeUpdateMA = retrievedScript.ModifiedAt;

        var context = await _em!.GetTestingContext<GeneratorContextNoInherVaccineV5.GeneratorContext>(autoDetectFromScript: retrievedScript);
        object result = await _facade.ExecuteScriptById(id, context);

        await Task.Delay(2000);
        await _facade!.UpdateScriptSC(retrievedScript.Id, "new source code doesnt matter if wrong shoudl save to db", allowFaultySave: true);

        CustomerScript updatedScript = await _facade.GetScript(id);
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
