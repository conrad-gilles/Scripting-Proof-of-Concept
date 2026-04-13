namespace Ember.Scripting
{
    using Ember.Scripting.ScriptMethods;

    public interface IScriptMethodsAction : IExecuteAsync, IExecute1, IExecute2, IExecute3
    {

    }
    public interface IScriptMethodscondition : IEvaluateAsync
    {

    }
}

namespace Ember.Scripting.ScriptMethods

{
    public interface IEvaluateAsync : IScript
    {
        Task<bool> EvaluateAsync(IContext context);
    }

    public interface IExecuteAsync : IScript
    {
        Task<ActionResultSF> ExecuteAsync(IContext context);
    }

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




