using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ember.Simulation;

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
        Exception ex = await Assert.ThrowsExceptionAsync<Ember.Scripting.ValidationBeforeCompilationException>(async () =>
        {
            CustomerScript script = await ScriptManager.CreateScript(sourceCode);
        });
    }
    [TestMethod]
    public async Task MissingUsingTest()
    {
        string sourceCode = TestHelper.GetSC().sourceCodeMissingUsing;
        Exception ex = await Assert.ThrowsExceptionAsync<Ember.Scripting.CompilationFailedException>(async () =>
        {
            CustomerScript script = await ScriptManager.CreateScript(sourceCode);
        });
    }
    [TestMethod]
    public async Task PreventUsageTest()
    {
        string sourceCode = TestHelper.GetSC().sourceCodePreventUsage;
        Exception ex = await Assert.ThrowsExceptionAsync<Ember.Scripting.ValidationBeforeCompilationException>(async () =>
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
            ActiveActionResult ar = await InternalScriptManager!.ExecuteScript("WhileTrueScript", ScriptTypes.GeneratorActionScript, ctx);

            Console.WriteLine("Name: " + script.ScriptName + ", ScriptType: " + script.ScriptType);
            Console.WriteLine("Type name: " + ar.GetType().FullName);
            Console.WriteLine("Returned result: " + ar);
        });
    }
}
