global using Microsoft.CodeAnalysis;
global using Ember.Scripting.Compilation;
global using Ember.Scripting.Execution;
global using Ember.Scripting.Persistence;
global using Ember.Scripting.ScriptingFramework;
global using Ember.Scripting.Manager;
global using Ember.Scripting.Versioning;
global using Ember.Sandbox.ScriptMethods;
global using Ember.Sandbox.ScriptingFrameWork.ScriptTypes;


global using RecentActionResult = ActionResultV3.ActionResult;
global using RecentIContext = IGeneratorContextNoInheritance_V5.IGeneratorContext;
global using RecentContext = GeneratorContextNoInherVaccineV5.GeneratorContext;
// global using RecentIGeneratorConditionScript = GeneratorScriptsGenericSimple.IGeneratorConditionScript<IGeneratorReadOnlyContextV1.IGeneratorContext>;
global using RecentIConditionScript = GeneratorScriptsGenericSimple.IConditionScript<IGeneratorContextNoInheritance_V5.IGeneratorContext>;
global using RecentIActionScript = GeneratorScriptsV4.IActionScript;
global using RecentContextFactory = GeneratorContextNoInherVaccineV5;
// global using ActiveMultiersionActionScript = Mu/

// public interface RecentIScript


public record RecentTypeHelper
{
    public static List<Type> GetRecentTypes()
    {
        List<Type> returnedDict = [];
        returnedDict.Add(typeof(RecentActionResult));
        returnedDict.Add(typeof(RecentIContext));
        returnedDict.Add(typeof(RecentContext));
        returnedDict.Add(typeof(RecentIConditionScript));
        returnedDict.Add(typeof(RecentIActionScript));
        return returnedDict;
    }
}