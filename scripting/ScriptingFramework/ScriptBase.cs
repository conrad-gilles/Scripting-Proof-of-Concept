namespace Ember.Scripting
{
    using Ember.Scripting.AdditionalMethods;

    public interface IMultiMethodBase : IExecute1, IExecute2
    {

    }
}

namespace Ember.Scripting.AdditionalMethods
{
    public interface IExecute1 : IScript
    {
        Task<ActionResultSF> Execute1(IContext context);    //type of construcotr changes per version
        // {
        //     throw new MethodNotImplementedException();
        // }
    }

    public interface IExecute2 : IScript
    {
        Task<ActionResultSF> Execute2(IContext context);
        // {
        //     throw new MethodNotImplementedException();
        // }
    }
    public interface IExecute3 : IScript
    {
        Task<string> Execute3(IContext context);
        // {
        //     throw new MethodNotImplementedException();
        // }
    }
}




