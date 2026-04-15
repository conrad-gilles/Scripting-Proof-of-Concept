global using Microsoft.CodeAnalysis;
global using Ember.Scripting.Compilation;
global using Ember.Scripting.Execution;
global using Ember.Scripting.Persistence;
global using Ember.Scripting.ScriptingFramework;
global using Ember.Scripting.ScriptManager;
global using Ember.Scripting.Versioning;
global using Ember.Scripting.ScriptingFramework.ScriptMethods;


global using RecentActionResult = ActionResultV3.ActionResult;
global using RecentIContext = IGeneratorContextNoInheritance_V5.IGeneratorContext;
global using RecentContext = GeneratorContextNoInherVaccineV5.GeneratorContext;
// global using RecentIGeneratorConditionScript = GeneratorScriptsGenericSimple.IGeneratorConditionScript<IGeneratorReadOnlyContextV1.IGeneratorContext>;
global using RecentIConditionScript = GeneratorScriptsGenericSimple.IConditionScript<IGeneratorContextNoInheritance_V5.IGeneratorContext>;
global using RecentIActionScript = GeneratorScriptsV4.IActionScript;
global using RecentDataClass = DataV2.DataV2;
global using RecentContextFactory = GeneratorContextNoInherVaccineV5;
// global using ActiveMultiersionActionScript = Mu/

// public interface RecentIScript


public record RecentTypeHelper
{
    // public readonly string TypeName;
    // public readonly string Recent;
    public static Dictionary<string, string> GetRecentTypeDictionary()
    {
        Dictionary<string, string> returnedDict = [];
        returnedDict.Add(typeof(RecentActionResult).Name, typeof(RecentActionResult).FullName!);
        returnedDict.Add(typeof(RecentIContext).Name, typeof(RecentIContext).FullName!);
        returnedDict.Add(typeof(RecentContext).Name, typeof(RecentContext).FullName!);
        returnedDict.Add(typeof(RecentIConditionScript).Name, typeof(RecentIConditionScript).FullName!);
        returnedDict.Add(typeof(RecentIActionScript).Name, typeof(RecentIActionScript).FullName!);
        // returnedDict.Add(typeof(RecentDataClass).Name, typeof(RecentDataClass).FullName!);
        // returnedDict.Add(typeof(RecentContextFactory.IGeneratorContextFactory).Name, typeof(RecentContextFactory.IGeneratorContextFactory).FullName!);
        return returnedDict;
    }
    public static List<Type> GetRecentTypes()
    {
        List<Type> returnedDict = [];
        returnedDict.Add(typeof(RecentActionResult));
        returnedDict.Add(typeof(RecentIContext));
        returnedDict.Add(typeof(RecentContext));
        returnedDict.Add(typeof(RecentIConditionScript));
        returnedDict.Add(typeof(RecentIActionScript));
        // returnedDict.Add(typeof(RecentDataClass));
        // returnedDict.Add(typeof(RecentContextFactory.ContextFactory));
        return returnedDict;
    }
}