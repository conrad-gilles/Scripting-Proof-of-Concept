namespace Ember.Scripting
{
    using Ember.Scripting.AdditionalMethods;

    public interface IMultiMethodBase : Ember.Scripting.IGeneratorActionScript, IExecuteAction1, IExecuteAction2
    {

    }
}

namespace Ember.Scripting.AdditionalMethods
{
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
}




