namespace Ember.Simulation;

internal class ScriptFacade<ScriptType> where ScriptType : IScriptType
{
    private EmberInternalFacade _emberScriptManager;
    private IScriptManagerExtended _scriptManager;
    private string _scriptName;

    public ScriptFacade(IScriptManagerExtended scriptManager, string scriptName)
    {
        _scriptManager = scriptManager;
        _emberScriptManager = new EmberInternalFacade(scriptManager);
        _scriptName = scriptName;
    }
    public async Task<bool> EvaluateAsync(RecentIContext context)
    {
        string methodName = nameof(RecentIConditionScript.EvaluateAsync);
        return (bool)await _emberScriptManager.ExecuteScript<ScriptType>(_scriptName, (RecentContext)context, methodName);
    }
    public async Task<RecentActionResult> ExecuteAsync(RecentIContext context)
    {
        string methodName = nameof(GeneratorScriptsV4.IActionScript.ExecuteAsync);
        return (RecentActionResult)await _emberScriptManager.ExecuteScript<ScriptType>(_scriptName, (RecentContext)context, methodName);
    }

    public async Task<RecentActionResult> Execute1(RecentIContext context)
    {
        string methodName = nameof(GeneratorScriptsV4.IActionScript.Execute1);
        CustomerScript script = await _scriptManager.GetScript<ScriptType>(_scriptName);   // this is being called twice, also in ExecuteScript i can move it down but then i also need to move the old MethodName check down into ExecuteScript 
        if (script.ScriptApiVersion == 10)
        {
            methodName = "ExecuteOldName1";
        }
        return (RecentActionResult)await _emberScriptManager.ExecuteScript<IActionScript>(_scriptName, (RecentContext)context, methodName);
    }
    public async Task<RecentActionResult> Execute2(RecentIContext context)
    {
        string methodName = nameof(GeneratorScriptsV4.IActionScript.Execute2);
        return (RecentActionResult)await _emberScriptManager.ExecuteScript<ScriptType>(_scriptName, (RecentContext)context, methodName);
    }
    public async Task<string> Execute3(RecentIContext context)
    {
        string methodName = nameof(GeneratorScriptsV4.IActionScript.Execute3);
        return (string)await _emberScriptManager.ExecuteScript<ScriptType>(_scriptName, (RecentContext)context, methodName);
    }
}