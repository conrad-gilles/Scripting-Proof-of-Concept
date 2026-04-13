namespace Ember.Scripting
{
    using Ember.Scripting.ScriptMethods;

    public interface IScriptMethodsAction : IExecuteAsync, IExecute1, IExecute2, IExecute3
    {

    }
    public interface IScriptMethodsCondition : IEvaluateAsync
    {

    }
}

namespace Ember.Scripting.ScriptMethods

{
    public interface IEvaluateAsync : IScript
    {
        Task<bool> EvaluateAsync(IContext context)
        {
            throw new MethodNotImplementedException(message: "Method: " + nameof(EvaluateAsync) + " was not implemented.");
        }
    }

    public interface IExecuteAsync : IScript
    {
        Task<ActionResultSF> ExecuteAsync(IContext context)
        {
            throw new MethodNotImplementedException(message: "Method: " + nameof(ExecuteAsync) + " was not implemented.");
        }
    }

    public interface IExecute1 : IScript
    {
        Task<ActionResultSF> Execute1(IContext context)    //type of construcotr changes per version
        {
            throw new MethodNotImplementedException(message: "Method: " + nameof(Execute1) + " was not implemented.");
        }
    }

    public interface IExecute2 : IScript
    {
        Task<ActionResultSF> Execute2(IContext context)
        {
            throw new MethodNotImplementedException(message: "Method: " + nameof(Execute2) + " was not implemented.");
        }
    }
    public interface IExecute3 : IScript
    {
        Task<string> Execute3(IContext context)
        {
            throw new MethodNotImplementedException(message: "Method: " + nameof(Execute3) + " was not implemented.");
        }
    }
}




