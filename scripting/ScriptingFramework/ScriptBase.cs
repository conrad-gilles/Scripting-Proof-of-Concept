// using Ember.Scripting;
// namespace Ember.Simulation;

namespace Ember.Scripting;

// public abstract class EmberScriptBase : Ember.Scripting.ScriptBase
// {

// }

public interface IMultiMethodBase : Ember.Scripting.IGeneratorActionScript, IExecuteAction1, IExecuteAction2
{

}

// [MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Action, type: IGeneratorScriptType.AbstractBaseInSF)]
public interface IExecuteAction1 : IScript
{
    Task<ActionResultSF> ExecuteAction1(IGeneratorBaseInterfaceSF context)
    {
        throw new MethodNotImplementedException();
    }
}

public interface IExecuteAction2 : IScript
{
    Task<ActionResultSF> ExecuteAction2(IGeneratorBaseInterfaceSF context)
    {
        throw new MethodNotImplementedException();
    }
}

public interface IExecuteAction3WithArgs : IScript
{
    Task<ActionResultSF> ExecuteAction3(IGeneratorBaseInterfaceSF context)
    {
        throw new MethodNotImplementedException();
    }
}


public class MethodNotImplementedException : ScriptingFrameworkException
{

    public MethodNotImplementedException() : base() { }
    public MethodNotImplementedException(string message) : base(message) { }
    public MethodNotImplementedException(string message, Exception innerException) : base(message, innerException) { }
}
