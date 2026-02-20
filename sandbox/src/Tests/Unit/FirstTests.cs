using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirstTests;

[TestClass]
public class UnitTest1
{
    ScriptManagerFacade facade = new ScriptManagerFacade();
    [TestMethod]
    public async Task CreateScriptTest()
    {
        string sourceCode = UsefulMethods.CreateStringFromCsFile(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "src", "Scripts", "ConditionScripts", "PediatricCondition.cs")));
        Guid id = await facade.CreateScript(sourceCode, "ConditionScriptTest", "Gilles");
        CustomerScript retrievedScript = await facade.GetScript(id);

        Assert.AreEqual(retrievedScript.SourceCode, sourceCode);
    }
}