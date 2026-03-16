namespace Ember.Scripting;

[MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Condition, type: IGeneratorScriptType.AbstractBaseInSF)]
public interface IGeneratorConditionScript
{
    // int VersionScript { get; }
    // Task<bool> EvaluateAsync(IGeneratorReadOnlyContext context);
    Task<bool> EvaluateAsync(IGeneratorBaseInterfaceSF context);
}
[MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Action, type: IGeneratorScriptType.AbstractBaseInSF)]
// public interface IGeneratorActionScript<Tcontext,TActionResult>
public interface IGeneratorActionScript
{
    // int VersionSript { get; }
    // Task<ActionResultBaseClass> ExecuteAsync(IGeneratorContext context);
    Task<ActionResultSF> ExecuteAsync(IGeneratorBaseInterfaceSF context);
}

public enum ScriptTypes
{
    GeneratorConditionScript, GeneratorActionScript
}