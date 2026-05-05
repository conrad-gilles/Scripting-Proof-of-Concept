global using Microsoft.CodeAnalysis;
global using Ember.Scripting.Compilation;
global using Ember.Scripting.Execution;
global using Ember.Scripting.Persistence;
global using Ember.Scripting.ScriptingFramework;
global using Ember.Scripting.Manager;
global using Ember.Scripting.Versioning;
global using Ember.Sandbox.ScriptingFrameWork.ScriptTypes;
global using Ember.Scripting;
global using Ember.Simulation;


global using RecentActionResult = ActionResultV3.ActionResult;
global using RecentIConditionScript = GeneratorScriptsGenericSimple.IConditionScript<IGeneratorContextNoInheritance_V5.IGeneratorContext>;
global using RecentIActionScript = GeneratorScriptV4.IActionScript;

global using RecentGeneratorContextFactory = ContextFactoryNameSpace;

global using RecentIGeneratorContext = IGeneratorContextNoInheritance_V5.IGeneratorContext;
global using RecentGeneratorContext = GeneratorContextNoInherVaccineV5.GeneratorContext;


public record RecentTypeHelper
{
    public static List<Type> GetRecentTypes()
    {
        List<Type> returnedDict = [];
        returnedDict.Add(typeof(RecentActionResult));
        returnedDict.Add(typeof(RecentIGeneratorContext));
        returnedDict.Add(typeof(RecentGeneratorContext));
        returnedDict.Add(typeof(RecentIConditionScript));
        returnedDict.Add(typeof(RecentIActionScript));
        return returnedDict;
    }
}