using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[TestClass]
public class FaultyScriptsTests
{
    static ISccriptManagerDeleteAfter ScriptManager = EmberMethods.GetNewScriptManagerInstance();
    static EmberInternalFacade InternalScriptManager = new EmberInternalFacade(ScriptManager);
    static EmberMethods em = new EmberMethods(ScriptManager);

    [TestInitialize]
    public async Task SetupAsync()
    {
        await ScriptManager.DeleteAllData();
    }

    [TestMethod]
    public async Task WhileTrueScript()
    {
        await Assert.ThrowsExceptionAsync<Ember.Scripting.ScriptExecutionException>(async () =>
        {
            string sourceCode = TestHelper.GetSC().sourceCodeWhileTrue;
            ScriptNameType scriptRecord = await ScriptManager!.CreateScriptUsingNameType(sourceCode!);

            var obj = TestHelper.ScriptObjects();
            var services = new ServiceCollection();
            Sandbox.SandboxServiceCollectionExtensions.AddSandboxServices
            // (services, obj.labOrder, obj.patient, obj.logger, obj.testDataAccess, obj.vaccine);
            (services, obj.logger, obj.testDataAccess);

            using var provider = services.BuildServiceProvider();
            ActiveContextFactory factory = provider.GetRequiredService<ActiveContextFactory>();
            ActiveGeneratorContext ctx = factory.Create(obj.labOrder, obj.vaccine);
            ActiveActionResult ar = await InternalScriptManager!.ExecuteScriptByNameAndType("WhileTrueScript", ScriptTypes.GeneratorActionScript, ctx);

            Console.WriteLine("Name: " + scriptRecord.Name + ", ScriptType: " + scriptRecord.Type);
            Console.WriteLine("Type name: " + ar.GetType().FullName);
            Console.WriteLine("Returned result: " + ar);
        });
    }

}
