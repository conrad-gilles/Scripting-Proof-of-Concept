namespace Ember.Scripting
{
    using Ember.Scripting.ScriptMethods;

    public interface IScriptMethodsAction : IExecute1, IExecute2, IExecute3
    {

    }
    public interface IScriptMethodscondition
    {

    }
}

namespace Ember.Scripting.ScriptMethods
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




