namespace Ember.Scripting
{
    using Ember.Scripting.AdditionalMethods;

    public interface IMultiMethodBase : IExecute1, IExecute2
    {

    }
}

namespace Ember.Scripting.AdditionalMethods
{
    // [MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Action, type: IGeneratorScriptType.AbstractBaseInSF)]
    public interface IExecute1 : IScript
    {
        Task<ActionResultSF> Execute1(IGeneratorBaseInterfaceSF context);    //type of construcotr changes per version
        // {
        //     throw new MethodNotImplementedException();
        // }
    }

    public interface IExecute2 : IScript
    {
        Task<ActionResultSF> Execute2(IGeneratorBaseInterfaceSF context);
        // {
        //     throw new MethodNotImplementedException();
        // }
    }
    public interface IExecute3 : IScript
    {
        Task<string> Execute3(IGeneratorBaseInterfaceSF context);
        // {
        //     throw new MethodNotImplementedException();
        // }
    }
}




