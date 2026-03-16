using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[TestClass]
public class EmberInternalFacadeTests
{
    ISccriptManagerDeleteAfter? facade;
    EmberMethods? em;
    (LabOrder labOrder, Patient patient, ConsoleLogger logger, DataAccess testDataAccess, Vaccine vaccine) obj;
    DataV1.MockData? data;
    string? ActionResultVersionSpecific;
    string? sourceCodeActionV1 = TestHelper.GetSC().sourceCodeActionV1;
    string? sourceCodeActionV2 = TestHelper.GetSC().sourceCodeActionV2;
    string? sourceCodeActionV3 = TestHelper.GetSC().sourceCodeActionV3;
    string? sourceCodeVaccineAction = TestHelper.GetSC().sourceCodeVaccineAction;
    string? sourceCodePedia = TestHelper.GetSC().sourceCodePedia;
    List<string>? sourceCodes = TestHelper.GetSC(includeCondInList: false).sourceCodes;

    [TestInitialize]
    public
     void Setup()
    {
        ActionResultVersionSpecific = "[Message contains either failure or succes: ] ";
        em = new EmberMethods(facade!);
        ActionResultVersionSpecific = "[Message contains either failure or succes: ] ";
        obj = em.ScriptObjects();
        data = new DataV1.MockData(labOrder: obj.labOrder, patient: obj.patient, consoleLogger: obj.logger,
        dataAccess: obj.testDataAccess, vaccine: obj.vaccine);
    }

    [TestMethod]
    public async Task CreateContextByDowngradeTest()
    {
        Guid id;
        ActionResultV3.ActionResult ar;
        string src;

        src = sourceCodeVaccineAction!;
        facade = EmberMethods.GetNewScriptManagerInstance();

        EmberInternalFacade eif = new EmberInternalFacade(facade);

        id = await facade.CreateScript(sourceCodeActionV2!);

        // DataV1.MockData data = new DataV1.MockData(labOrder: obj.labOrder, patient: obj.patient, consoleLogger: obj.logger,
        //                 dataAccess: obj.testDataAccess, vaccine: obj.vaccine);

        DataV2.DataV2 data = new DataV2.DataV2(labOrder: obj.labOrder, patient: obj.patient, consoleLogger: obj.logger,
                                dataAccess: obj.testDataAccess, vaccine: obj.vaccine);


        // ActiveGeneratorContext ctx = new ActiveGeneratorContext(labOrder: obj.labOrder, vaccine: obj.vaccine);

        var ctx = eif.CreateContext(obj.labOrder, obj.vaccine);

        ar = await eif.ExecuteScriptById(id, ctx!);



        // Assert.IsTrue(ar.GetType() == typeof(ActionResultV3.ActionResult));
        Console.WriteLine("Type name: " + ar.GetType().FullName);
        Console.WriteLine("Returned result: " + ar);

        // Assert.IsTrue(false);
    }
}