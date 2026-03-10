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
        ctx = await sf.CreateContextForApiV();

        Assert.IsTrue(ctx.GetType() == typeof(ReadOnlyContextV1.GeneratorContext));

        facade = EmberMethods.GetNewScriptManagerInstance(2);
        sf = new ScriptFactory(facade);
        ctx = await sf.CreateContextForApiV();

        Assert.IsTrue(ctx.GetType() == typeof(RWContextV2.GeneratorContext));

        facade = EmberMethods.GetNewScriptManagerInstance();
        sf = new ScriptFactory(facade);

        Exception ex = await Assert.ThrowsExceptionAsync<Exception>(async () =>
         {
             ctx = await sf.CreateContextForApiV();
         });
        Assert.IsTrue(ex.Message.Contains("No Context class defined in") && ex.Message.Contains("for the passed API version."));
    }

    [TestMethod]
    public async Task GetDictionaryTest()
    {
        Dictionary<int, Type> contextVersionMap = new()
        {
            // {0, typeof(GeneratorContext)},
            {1, typeof(ReadOnlyContextV1.GeneratorContext)},
            {2, typeof(RWContextV2.GeneratorContext)},
            {3, typeof(GeneratorContextV3.GeneratorContext)},
            {4, typeof(GeneratorContextV4.GeneratorContext)},
            {5, typeof(GeneratorContextNoInherVaccineV5.GeneratorContext)},
        };

        ScriptFactory sf;
        facade = EmberMethods.GetNewScriptManagerInstance(1);
        sf = new ScriptFactory(facade);
        Dictionary<int, Type> retrievedDict = ContextVersionScanner.GetClassDictionary();
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

    [TestMethod]
    public async Task BasicValidationTestUsingGetDictionary()
    {
        facade = EmberMethods.GetNewScriptManagerInstance();
        (string className, string baseTypeName, int versionInt) valResult = facade.BasicValidationBeforeCompiling(sourceCodeActionV2!);

        Console.WriteLine(nameof(valResult.baseTypeName) + " : " + valResult.baseTypeName);
        Console.WriteLine(nameof(valResult.className) + " : " + valResult.className);
        Console.WriteLine(nameof(valResult.versionInt) + " : " + valResult.versionInt);

        Assert.IsTrue(valResult.baseTypeName == "IGeneratorActionScriptV2");
        Assert.IsTrue(valResult.className == "AddPediatricTestsV3");
        Assert.IsTrue(valResult.versionInt == 3);

        valResult = facade.BasicValidationBeforeCompiling(sourceCodeActionV1!);

        Console.WriteLine(nameof(valResult.baseTypeName) + " : " + valResult.baseTypeName);
        Console.WriteLine(nameof(valResult.className) + " : " + valResult.className);
        Console.WriteLine(nameof(valResult.versionInt) + " : " + valResult.versionInt);

        Assert.IsTrue(valResult.baseTypeName == "IGeneratorActionScript");
        Assert.IsTrue(valResult.className == "AddPediatricTestsV2");
        Assert.IsTrue(valResult.versionInt == 2);

        valResult = facade.BasicValidationBeforeCompiling(sourceCodeActionV3!);

        Console.WriteLine(nameof(valResult.baseTypeName) + " : " + valResult.baseTypeName);
        Console.WriteLine(nameof(valResult.className) + " : " + valResult.className);
        Console.WriteLine(nameof(valResult.versionInt) + " : " + valResult.versionInt);

        Assert.IsTrue(valResult.baseTypeName == "IGeneratorActionScriptV3");
        Assert.IsTrue(valResult.className == "AddPediatricTestsV4");
        Assert.IsTrue(valResult.versionInt == 4);

        valResult = facade.BasicValidationBeforeCompiling(sourceCodeVaccineAction!);

        Console.WriteLine(nameof(valResult.baseTypeName) + " : " + valResult.baseTypeName);
        Console.WriteLine(nameof(valResult.className) + " : " + valResult.className);
        Console.WriteLine(nameof(valResult.versionInt) + " : " + valResult.versionInt);

        Assert.IsTrue(valResult.baseTypeName == "IGeneratorActionScriptV4Vaccine");
        Assert.IsTrue(valResult.className == "VaccineScript");
        Assert.IsTrue(valResult.versionInt == 5);

        valResult = facade.BasicValidationBeforeCompiling(sourceCodePedia!);

        Console.WriteLine(nameof(valResult.baseTypeName) + " : " + valResult.baseTypeName);
        Console.WriteLine(nameof(valResult.className) + " : " + valResult.className);
        Console.WriteLine(nameof(valResult.versionInt) + " : " + valResult.versionInt);

        Assert.IsTrue(valResult.baseTypeName == "IGeneratorConditionScript");
        Assert.IsTrue(valResult.className == "PediatricCondition");
        Assert.IsTrue(valResult.versionInt == 1);

        // Assert.IsTrue(false);

    }

    [TestMethod]
    public async Task CreateContextForApiTest()
    {
        Guid id;
        ScriptFactory sf;
        (string className, string baseTypeName, int versionInt) valResult;

        facade = EmberMethods.GetNewScriptManagerInstance();
        valResult = facade.BasicValidationBeforeCompiling(sourceCodeActionV2!);
        id = await facade.CreateScript(sourceCodeActionV2!);

        sf = new ScriptFactory(facade);
        await facade.ExecuteScriptById(id, await sf.CreateContextForApiV(valResult.versionInt));

        for (int i = 0; i < 6; i++)
        {
            facade = EmberMethods.GetNewScriptManagerInstance(i);
            valResult = facade.BasicValidationBeforeCompiling(sourceCodeActionV2!);
            id = await facade.CreateScript(sourceCodeActionV2!);

            sf = new ScriptFactory(facade);
            await facade.ExecuteScriptById(id, await sf.CreateContextForApiV(valResult.versionInt));
        }

        for (int i = 0; i < 6; i++)
        {
            facade = EmberMethods.GetNewScriptManagerInstance(i);
            valResult = facade.BasicValidationBeforeCompiling(sourceCodeVaccineAction!);
            id = await facade.CreateScript(sourceCodeVaccineAction!);

            sf = new ScriptFactory(facade);
            await facade.ExecuteScriptById(id, await sf.CreateContextForApiV(valResult.versionInt));
        }
        for (int i = 0; i < 6; i++)
        {
            facade = EmberMethods.GetNewScriptManagerInstance(i);
            valResult = facade.BasicValidationBeforeCompiling(sourceCodePedia!);
            id = await facade.CreateScript(sourceCodePedia!);

            sf = new ScriptFactory(facade);
            await facade.ExecuteScriptById(id, await sf.CreateContextForApiV(valResult.versionInt));
        }
    }
}