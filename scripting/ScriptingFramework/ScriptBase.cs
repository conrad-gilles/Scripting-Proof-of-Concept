namespace Ember.Scripting
{
    using Ember.Scripting.ScriptMethods;

    public interface IScriptMethod
    {

    }

    public interface IScriptMethodsAction : IExecuteAsync, IExecute1, IExecute2, IExecute3
    {

    }
    public interface IScriptMethodsCondition : IEvaluateAsync
    {

    }
}

namespace Ember.Scripting.ScriptMethods

{
    public interface IEvaluateAsync : IScriptMethod
    {
        Task<bool> EvaluateAsync(IContext context)
        {
            throw new MethodNotImplementedException(message: "Method: " + nameof(EvaluateAsync) + " was not implemented.");
        }
    }

    public interface IExecuteAsync : IScriptMethod
    {
        Task<ActionResultSF> ExecuteAsync(IContext context)
        {
            throw new MethodNotImplementedException(message: "Method: " + nameof(ExecuteAsync) + " was not implemented.");
        }
    }

    public interface IExecute1 : IScriptMethod
    {
        Task<ActionResultSF> Execute1(IContext context)    //type of construcotr changes per version
        {
            throw new MethodNotImplementedException(message: "Method: " + nameof(Execute1) + " was not implemented.");
        }
    }

    public interface IExecute2 : IScriptMethod
    {
        Task<ActionResultSF> Execute2(IContext context)
        {
            throw new MethodNotImplementedException(message: "Method: " + nameof(Execute2) + " was not implemented.");
        }
    }
    public interface IExecute3 : IScriptMethod
    {
        Task<string> Execute3(IContext context)
        {
            throw new MethodNotImplementedException(message: "Method: " + nameof(Execute3) + " was not implemented.");
        }
    }
}




