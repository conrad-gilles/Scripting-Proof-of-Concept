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
    static IScriptManagerDeleteAfter ScriptManager = TestHelper.InitScriptManager();
    static EmberInternalFacade InternalScriptManager = new EmberInternalFacade(ScriptManager);
    static EmberMethods em = new EmberMethods(ScriptManager);

    [TestInitialize]
    public async Task SetupAsync()
    {
        await ScriptManager.DeleteAllData();
    }
    // [TestMethod]
    public async Task ScriptTypesVerification()
    {
        // List<CustomerScript> scripts = await ScriptManager.ListScripts();

        // ScriptTypes[] enumValues = Enum.GetValues<ScriptTypes>();

        // foreach (var script in scripts)
        // {
        //     ScriptTypes type = script.GetScriptTypeEnum();
        //     if (enumValues.Contains(type) == false)
        //     {
        //         throw new Exception();
        //     }
        // }
    }

    [TestMethod]
    public async Task ExecutionTimesEnumValuesVerification()
    {
        List<CustomerScript> scripts = await ScriptManager.ListScripts();

        ExecutionTimeGroups[] enumValues = Enum.GetValues<ExecutionTimeGroups>();

        foreach (var item in enumValues)
        {
            if (((int)item) > ExecutionTime.MaximumDuration)
            {
                throw new Exception("enum value too big");
            }
            if (((int)item) < ExecutionTime.MinimumDuration)
            {
                throw new Exception("enum value too small");
            }
        }

        foreach (var script in scripts)
        {
            if (script.ExecutionTimeInMS > ExecutionTime.MaximumDuration)
            {
                throw new Exception("duration too big");
            }
            if (script.ExecutionTimeInMS < ExecutionTime.MinimumDuration)
            {
                throw new Exception("duration too small");
            }
        }


        string sourceCode = """
        using System;   //todo this is possible to default in compiler
        using System.Threading.Tasks;
        using System.Collections.Generic;   //todo same for them
        using Ember.Scripting;
        using GeneratorScriptsGeneric;
        using IGeneratorContext_V2;

        [ExecutionTime(ExecutionTimeGroups.Short)]
        public class ExecutionTimeTest : GeneratorScriptsGeneric.IGeneratorActionScript<IGeneratorContext_V2.IGeneratorContext, ActionResultV1.ActionResult>
        {
        public async Task<ActionResultV1.ActionResult> ExecuteAsync(IGeneratorContext_V2.IGeneratorContext context)    //error is here in Basyns
        {
            return ActionResultV1.ActionResult.Success("Pediatric tests added");
        }
        }
        """;

        int expectedMS = 100;
        int? realMS = ScriptManager.BasicValidationBeforeCompiling(sourceCode).ExecutionTime;
        Assert.IsTrue(expectedMS == realMS);

        sourceCode = """
        using System;   //todo this is possible to default in compiler
        using System.Threading.Tasks;
        using System.Collections.Generic;   //todo same for them
        using Ember.Scripting;
        using GeneratorScriptsGeneric;
        using IGeneratorContext_V2;

        [ExecutionTime(ExecutionTimeGroups.Medium)]
        public class ExecutionTimeTest : GeneratorScriptsGeneric.IGeneratorActionScript<IGeneratorContext_V2.IGeneratorContext, ActionResultV1.ActionResult>
        {
        public async Task<ActionResultV1.ActionResult> ExecuteAsync(IGeneratorContext_V2.IGeneratorContext context)    //error is here in Basyns
        {
            return ActionResultV1.ActionResult.Success("Pediatric tests added");
        }
        }
        """;

        expectedMS = 500;  //shows how the maximum is respected and not overstepped
        realMS = ScriptManager.BasicValidationBeforeCompiling(sourceCode).ExecutionTime;
        Assert.IsTrue(expectedMS == realMS);

        sourceCode = """
        using System;   //todo this is possible to default in compiler
        using System.Threading.Tasks;
        using System.Collections.Generic;   //todo same for them
        using Ember.Scripting;
        using GeneratorScriptsGeneric;
        using IGeneratorContext_V2;

        [ExecutionTime(ExecutionTimeGroups.Long)]
        public class ExecutionTimeTest : GeneratorScriptsGeneric.IGeneratorActionScript<IGeneratorContext_V2.IGeneratorContext, ActionResultV1.ActionResult>
        {
        public async Task<ActionResultV1.ActionResult> ExecuteAsync(IGeneratorContext_V2.IGeneratorContext context)    //error is here in Basyns
        {
            return ActionResultV1.ActionResult.Success("Pediatric tests added");
        }
        }
        """;

        expectedMS = 1000;
        realMS = ScriptManager.BasicValidationBeforeCompiling(sourceCode).ExecutionTime;
        Assert.IsTrue(expectedMS == realMS);

        sourceCode = """
        using System;   //todo this is possible to default in compiler
        using System.Threading.Tasks;
        using System.Collections.Generic;   //todo same for them
        using Ember.Scripting;
        using GeneratorScriptsGeneric;
        using IGeneratorContext_V2;

        [ExecutionTime(ExecutionTimeGroups.ExtraLong)]
        public class ExecutionTimeTest : GeneratorScriptsGeneric.IGeneratorActionScript<IGeneratorContext_V2.IGeneratorContext, ActionResultV1.ActionResult>
        {
        public async Task<ActionResultV1.ActionResult> ExecuteAsync(IGeneratorContext_V2.IGeneratorContext context)    //error is here in Basyns
        {
            return ActionResultV1.ActionResult.Success("Pediatric tests added");
        }
        }
        """;

        expectedMS = 5000;
        realMS = ScriptManager.BasicValidationBeforeCompiling(sourceCode).ExecutionTime;
        Assert.IsTrue(expectedMS == realMS);
    }
}