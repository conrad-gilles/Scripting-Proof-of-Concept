namespace Ember.Scripting;

public interface IScript
{

}

[MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Condition, type: IGeneratorScriptType.AbstractBaseInSF)]
public interface IConditionScript : IScript
{
    // Task<bool> EvaluateAsync(IContext context);
}

[MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Action, type: IGeneratorScriptType.AbstractBaseInSF)]
public interface IActionScript : IScript
{
    // Task<ActionResultSF> ExecuteAsync(IContext context);
}
public interface IUserSession
{
    Guid Id { get; }
    string UserName { get; }
}