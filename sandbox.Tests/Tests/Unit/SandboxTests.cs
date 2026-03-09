using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ember.Scripting;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

[TestClass]
public class SanboxTests
{


    ISccriptManagerDeleteAfter? facade;
    EmberMethods? em;
    string? ActionResultVersionSpecific;
    string? sourceCodeActionV1;
    string? sourceCodeActionV2;
    string? sourceCodeActionV3;
    string? sourceCodeVaccineAction;
    string? sourceCodePedia;
    List<string>? sourceCodes;

    [TestInitialize]
    public
     void Setup()
    {
        ActionResultVersionSpecific = "[Message contains either failure or succes: ] ";  //change this if action result version changes it will thraow cause of message contains
        sourceCodeActionV1 = EmberMethods.CreateStringFromCsFile(
           Path.GetFullPath(Path.Combine(
               AppDomain.CurrentDomain.BaseDirectory,
               "..", "..", "..", "..",
               "sandbox", "src", "Scripts", "ActionScripts", "AddPediatricTestsV1.cs"
           ))
       );
        sourceCodeActionV2 = EmberMethods.CreateStringFromCsFile(
        Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..",
            "sandbox", "src", "Scripts", "ActionScripts", "AddPediatricTestsV2.cs"
        ))
    );
        sourceCodeActionV3 = EmberMethods.CreateStringFromCsFile(
       Path.GetFullPath(Path.Combine(
           AppDomain.CurrentDomain.BaseDirectory,
           "..", "..", "..", "..",
           "sandbox", "src", "Scripts", "ActionScripts", "AddPediatricTestsV3.cs"
       ))
   );
        sourceCodeVaccineAction = EmberMethods.CreateStringFromCsFile(
          Path.GetFullPath(Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory,
          "..", "..", "..", "..",
          "sandbox", "src", "Scripts", "ActionScripts", "VaccineScript.cs"
      ))
      );
        sourceCodePedia = EmberMethods.CreateStringFromCsFile(
       Path.GetFullPath(Path.Combine(
           AppDomain.CurrentDomain.BaseDirectory,
                   "..", "..", "..", "..",
                   "sandbox", "src", "Scripts", "ConditionScripts", "PediatricCondition.cs"
               ))
           );
        sourceCodes = [];
        sourceCodes!.Add(sourceCodeActionV1);
        sourceCodes!.Add(sourceCodeActionV2);
        sourceCodes!.Add(sourceCodeActionV3);
        sourceCodes!.Add(sourceCodeVaccineAction);
        // sourceCodes!.Add(sourceCodePedia);
    }
    [TestMethod]
    public async Task CreateContextText()
    {
        ScriptFactory sf;
        GeneratorContext ctx;

        facade = EmberMethods.GetNewScriptManagerInstance(1);
        sf = new ScriptFactory(facade);
        ctx = await sf.CreateContext();

        Assert.IsTrue(ctx.GetType() == typeof(ReadOnlyContext.GeneratorContext));

        facade = EmberMethods.GetNewScriptManagerInstance(2);
        sf = new ScriptFactory(facade);
        ctx = await sf.CreateContext();

        Assert.IsTrue(ctx.GetType() == typeof(RWContext.GeneratorContext));

        facade = EmberMethods.GetNewScriptManagerInstance();
        sf = new ScriptFactory(facade);

        Exception ex = await Assert.ThrowsExceptionAsync<Exception>(async () =>
         {
             ctx = await sf.CreateContext();
         });
        Assert.IsTrue(ex.Message.Contains("No Context class defined in") && ex.Message.Contains("for the passed API version."));
    }

    [TestMethod]
    public async Task GetDictionaryTest()
    {
        Dictionary<int, Type> contextVersionMap = new()
        {
            // {0, typeof(GeneratorContext)},
            {1, typeof(ReadOnlyContext.GeneratorContext)},
            {2, typeof(RWContext.GeneratorContext)},
            {3, typeof(GeneratorContextV2.GeneratorContext)},
            {4, typeof(GeneratorContextV3.GeneratorContext)},
            {5, typeof(GeneratorContextNoInherVaccine.GeneratorContext)},
        };

        ScriptFactory sf;
        facade = EmberMethods.GetNewScriptManagerInstance(1);
        sf = new ScriptFactory(facade);
        Dictionary<int, Type> retrievedDict = sf.GetDictionary();
        retrievedDict.Reverse();
        PrintDictToConsole(contextVersionMap);
        PrintDictToConsole(retrievedDict);
        CollectionAssert.AreEquivalent(contextVersionMap, retrievedDict);
    }

    public void PrintDictToConsole(Dictionary<int, Type> dict)
    {
        Console.WriteLine("Start of dict String:");
        foreach (var pair in dict)
        {
            Console.WriteLine("Key: " + pair.Key + ", Value: " + pair.Value);
        }
    }
}