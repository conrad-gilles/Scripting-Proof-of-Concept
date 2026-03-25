namespace Ember.Scripting;

[MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Condition, type: IGeneratorScriptType.AbstractBaseInSF)]
public interface IGeneratorConditionScript
{
    Task<bool> EvaluateAsync(IGeneratorBaseInterfaceSF context);
}
[MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Action, type: IGeneratorScriptType.AbstractBaseInSF)]
public interface IGeneratorActionScript
{
    Task<ActionResultSF> ExecuteAsync(IGeneratorBaseInterfaceSF context);
}
public enum ScriptTypes
{
    GeneratorConditionScript, GeneratorActionScript
}
public interface IUserSession
{
    Guid Id { get; }
    string UserName { get; }
}