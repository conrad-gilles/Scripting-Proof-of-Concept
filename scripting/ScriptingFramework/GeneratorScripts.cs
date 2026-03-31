namespace Ember.Scripting;

// public abstract class ScriptBase : IGeneratorActionScript
// {
//     public virtual Task<ActionResultSF> ExecuteAsync(IGeneratorBaseInterfaceSF context)
//     {
//         throw new NotImplementedException();
//     }
// }

// public interface IScriptBase : IGeneratorActionScript
// {

// }

public interface IScript
{

}

[MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Condition, type: IGeneratorScriptType.AbstractBaseInSF)]
public interface IGeneratorConditionScript : IScript
{
    Task<bool> EvaluateAsync(IGeneratorBaseInterfaceSF context);
}
[MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Action, type: IGeneratorScriptType.AbstractBaseInSF)]
public interface IGeneratorActionScript : IScript
{
    Task<ActionResultSF> ExecuteAsync(IGeneratorBaseInterfaceSF context);
}
public interface IUserSession
{
    Guid Id { get; }
    string UserName { get; }
}