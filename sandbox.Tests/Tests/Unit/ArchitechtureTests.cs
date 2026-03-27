using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ember.Simulation;

[TestClass]
public class ArchitechtureTests
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
    public async Task ScriptTypesVerification()
    {
        List<CustomerScript> scripts = await ScriptManager.ListScripts();

        ScriptTypes[] enumValues = Enum.GetValues<ScriptTypes>();

        foreach (var script in scripts)
        {
            ScriptTypes type = script.GetScriptTypeEnum();
            if (enumValues.Contains(type) == false)
            {
                throw new Exception();
            }
        }
    }
}