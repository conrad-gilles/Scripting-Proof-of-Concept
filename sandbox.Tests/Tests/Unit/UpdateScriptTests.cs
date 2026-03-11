using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[TestClass]
public class UpdateScriptTests
{


    // ISccriptManagerDeleteAfter? facade;
    // EmberMethods? rm;
    string? ActionResultVersionSpecific;
    string? sourceCodeActionV1 = TestHelper.GetSC().sourceCodeActionV1;
    string? sourceCodeActionV2 = TestHelper.GetSC().sourceCodeActionV2;
    string? sourceCodeActionV3 = TestHelper.GetSC().sourceCodeActionV3;
    string? sourceCodeVaccineAction = TestHelper.GetSC().sourceCodeVaccineAction;
    string? sourceCodePedia = TestHelper.GetSC().sourceCodePedia;
    LoggerForScripting? logger;
    ServiceCollection? services;
    // List<string>? sourceCodes = TestHelper.GetSC(includeCondInList: false).sourceCodes;

    [TestInitialize]
    public
     void Setup()
    {
        logger = new LoggerForScripting();
        Log.Debug("Sandbox launched.");

        services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            // Assuming you use Serilog, this forwards standard MS Logging to Serilog
            builder.AddSerilog(dispose: true);
        });

        ActionResultVersionSpecific = "[Message contains either failure or succes: ] ";
    }

    [TestMethod]
    public void TestCreatedAt()
    {

    }
}