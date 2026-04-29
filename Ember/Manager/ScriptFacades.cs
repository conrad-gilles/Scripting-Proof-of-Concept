namespace Ember.Simulation;


internal class ConditionScript : RecentScriptFacade, RecentIConditionScript
{
    private InternalManager _emberScriptManager;
    private string _scriptName;

    public ConditionScript(IScriptManagerExtended scriptManager, string scriptName)
    {
        _emberScriptManager = new InternalManager(scriptManager);
        _scriptName = scriptName;
    }
    public async Task<bool> EvaluateAsync(RecentIGeneratorContext context)
    {
        string methodName = nameof(RecentIConditionScript.EvaluateAsync);
        return (bool)await _emberScriptManager.ExecuteScript<IConditionScript>(_scriptName, context, methodName);

    }
}

internal class ActionScript : RecentScriptFacade, RecentIActionScript
{
    private InternalManager _emberScriptManager;
    private IScriptManagerExtended _scriptManager;
    private string _scriptName;

    public ActionScript(IScriptManagerExtended scriptManager, string scriptName)
    {
        _scriptManager = scriptManager;
        _emberScriptManager = new InternalManager(scriptManager);
        _scriptName = scriptName;
    }
    public async Task<RecentActionResult> ExecuteAsync(RecentIGeneratorContext context)
    {
        string methodName = nameof(RecentIActionScript.ExecuteAsync);
        return (RecentActionResult)await _emberScriptManager.ExecuteScript<IActionScriptBase>(_scriptName, context, methodName);
    }

    public async Task<RecentActionResult> Execute1(RecentIGeneratorContext context)
    {
        string methodName = nameof(RecentIActionScript.Execute1);
        CustomerScript script = await _scriptManager.GetScript<IActionScriptBase>(_scriptName);   // this is being called twice, also in ExecuteScript i can move it down but then i also need to move the old MethodName check down into ExecuteScript 
        if (script.ScriptApiVersion == 3)
        {
            methodName = "Execute1OldName";
        }
        return (RecentActionResult)await _emberScriptManager.ExecuteScript<IActionScriptBase>(_scriptName, context, methodName);
    }
    public async Task<RecentActionResult> Execute2(RecentIGeneratorContext context)
    {
        string methodName = nameof(RecentIActionScript.Execute2);
        return (RecentActionResult)await _emberScriptManager.ExecuteScript<IActionScriptBase>(_scriptName, context, methodName);
    }
    public async Task<string> Execute3(RecentIGeneratorContext context)
    {
        string methodName = nameof(RecentIActionScript.Execute3);
        return (string)await _emberScriptManager.ExecuteScript<IActionScriptBase>(_scriptName, context, methodName);
    }
}