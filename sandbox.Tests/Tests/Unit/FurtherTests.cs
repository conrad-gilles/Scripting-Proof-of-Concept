using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class FurtherTests
{
    ScriptManagerFacade facade = new ScriptManagerFacade();
    UsefulMethods usefulMethods = new UsefulMethods();
    string sourceCodeVaccineAction = UsefulMethods.CreateStringFromCsFile(
        Path.GetFullPath(Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "..", "..", "..", "..",
        "sandbox", "src", "Scripts", "ActionScripts", "VaccineScript.cs"
    ))
    );

    [TestMethod]
    public async Task VaccineScriptTest()
    {
        Guid id = await facade.CreateScript(sourceCodeVaccineAction);
        CustomerScript retrievedScript = await facade.GetScript(id);
        var context = UsefulMethods.GetTestingContext<GeneratorContextNoInherVaccine>(justForTesting: retrievedScript);
        object result = await facade.ExecuteScriptById(id, context);
        string shouldReturn = "[Message contains either failure or succes: ] Polio Vaccine added";
        Assert.IsInstanceOfType(result, typeof(ActionResultBaseClass));
        Assert.IsInstanceOfType(result, typeof(ActionResultV3NoInheritance));
        Assert.IsTrue(result.ToString().Contains(shouldReturn));

        //todo make negative test
    }
}
