namespace Ember.Scripting.ScriptingFramework;


public interface IScriptType
{

}
[MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Condition, type: IGeneratorScriptType.AbstractBaseInSF)]
public interface IConditionScript : IScriptType
{
    // Task<bool> EvaluateAsync(IContext context);
}

[MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Action, type: IGeneratorScriptType.AbstractBaseInSF)]
public interface IActionScript : IScriptType
{
    // Task<ActionResultSF> ExecuteAsync(IContext context);
}
public interface IUserSession
{
    Guid Id { get; }
    string UserName { get; }
}