using Ember.Scripting;
namespace Ember.Simulation;

// public abstract class EmberScriptBase : Ember.Scripting.ScriptBase
// {

// }

public interface IEmberScriptBase : Ember.Scripting.IGeneratorActionScript, IExecuteAction1
{

}

// [MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Action, type: IGeneratorScriptType.AbstractBaseInSF)]
public interface IExecuteAction1 : IScript
{
    Task<ActionResultSF> ExecuteAction1(IGeneratorBaseInterfaceSF context)
    {
        throw new NotImplementedException();
    }
}

