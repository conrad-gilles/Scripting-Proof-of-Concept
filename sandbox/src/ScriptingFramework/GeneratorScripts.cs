using Ember.Scripting;

namespace GeneratorScriptsGenericSimple
{
    /// <summary>
    /// Simplest implementation of the Condition script probably should make them private in real application to prevent user from using
    /// </summary>
    /// <summary>
    /// Implementation using 1 generic for Context
    /// </summary>
    [MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Condition, type: IGeneratorScriptType.GenericSimple)]
    public interface IConditionScript<TContext> : Ember.Scripting.IConditionScript, Ember.Scripting.ScriptMethods.IEvaluateAsync    //todo this needs to get tested still
                                                                                                                                    // where TContext : IGeneratorReadOnlyContextV1.IGeneratorContext
        where TContext : IContext
    {
        Task<bool> EvaluateAsync(TContext context);
        // Task<bool> IGeneratorConditionScript.EvaluateAsync(IGeneratorReadOnlyContext context)
        Task<bool> Ember.Scripting.ScriptMethods.IEvaluateAsync.EvaluateAsync(IContext context)
        {
            return EvaluateAsync((TContext)context);
        }
    }
    [MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Action, type: IGeneratorScriptType.GenericSimple)]
    public interface IActionScript<TContext> :
     Ember.Scripting.IActionScript,
     Ember.Scripting.ScriptMethods.IExecuteAsync
    where TContext : IGeneratorContext_V2.IGeneratorContext
    {

        Task<ActionResultSF> ExecuteAsync(TContext context);

        //explicit default implementation for the base interface
        // Task<ActionResultBaseClass> IGeneratorActionScript.ExecuteAsync(IGeneratorContext context)
        Task<ActionResultSF> Ember.Scripting.ScriptMethods.IExecuteAsync.ExecuteAsync(IContext context)
        {
            return ExecuteAsync((TContext)context);
        }
    }
}

namespace GeneratorScriptsGeneric
{
    /// <summary>
    /// Implementation using 2 generics, one for context and one additional for ActionResult
    /// </summary>
    [MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Action, type: IGeneratorScriptType.Generic)]
    public interface IActionScript<TContext, TActionResult> :
     Ember.Scripting.IActionScript,
     Ember.Scripting.ScriptMethods.IExecuteAsync
        where TContext : IContext    //changed from IGeneratorContext
        where TActionResult : ActionResultSF
    {
        Task<TActionResult> ExecuteAsync(TContext context);

        //explicit default implementation for the base interface
        async Task<ActionResultSF> Ember.Scripting.ScriptMethods.IExecuteAsync.ExecuteAsync(IContext context)
        {
            return await ExecuteAsync((TContext)context);
        }
    }
}


/// <summary>
/// Implementation using versioning of Generator scripts good for strict control over what version of context and what version of the return type the user uses
/// </summary>

namespace GeneratorScriptsV2
{
    [MetaDataIGeneratorScript(version: 2)]
    public interface IActionScript : Ember.Scripting.IActionScript, Ember.Scripting.ScriptMethods.IExecuteAsync
    {
        Task<ActionResultV2.ActionResult> ExecuteAsync(IGeneratorContext_V3.IGeneratorContext context);

        Task<ActionResultSF> Ember.Scripting.ScriptMethods.IExecuteAsync.ExecuteAsync(IContext context)     //if bugs maybe put as async and await ExecuteAsync
        {
            return ExecuteAsync(context);
        }
    }
}
namespace GeneratorScriptsV3
{
    [MetaDataIGeneratorScript(version: 3)]
    // public interface IGeneratorActionScript : Ember.Scripting.IGeneratorActionScript
    public interface IActionScript : Ember.Scripting.IActionScript, Ember.Scripting.IScriptMethodsAction
    {
        Task<ActionResultV3.ActionResult> ExecuteAsync(IGeneratorContext_V4.IGeneratorContext context);
        Task<ActionResultV3.ActionResult> Execute1(IGeneratorContext_V4.IGeneratorContext context)
        {
            throw new MethodNotImplementedException(message: nameof(Execute1) + " was not implemented.");
        }
        Task<ActionResultV3.ActionResult> Execute2(IGeneratorContext_V4.IGeneratorContext context)
        {
            throw new MethodNotImplementedException(message: nameof(Execute2) + " was not implemented.");
        }
        Task<string> Execute3(IGeneratorContextNoInheritance_V5.IGeneratorContext context)
        {
            throw new MethodNotImplementedException(message: nameof(Execute3) + " was not implemented.");
        }

        Task<ActionResultSF> Ember.Scripting.ScriptMethods.IExecuteAsync.ExecuteAsync(IContext context)     //if bugs maybe put as async and await ExecuteAsync
        {
            return ExecuteAsync(context);
        }
        Task<ActionResultSF> Ember.Scripting.ScriptMethods.IExecute1.Execute1(IContext context)     //if bugs maybe put as async and await ExecuteAsync
        {
            return Execute1(context);
        }
        Task<ActionResultSF> Ember.Scripting.ScriptMethods.IExecute2.Execute2(IContext context)     //if bugs maybe put as async and await ExecuteAsync
        {
            return Execute2(context);
        }
        Task<string> Ember.Scripting.ScriptMethods.IExecute3.Execute3(IContext context)     //if bugs maybe put as async and await ExecuteAsync
        {
            return Execute3(context);
        }
    }
}
namespace GeneratorScriptsV4
{
    [MetaDataIGeneratorScript(version: 4)]
    public interface IActionScript : Ember.Scripting.IActionScript, Ember.Scripting.IScriptMethodsAction
    {
        Task<ActionResultV3.ActionResult> ExecuteAsync(IGeneratorContextNoInheritance_V5.IGeneratorContext context);
        Task<ActionResultV3.ActionResult> Execute1(IGeneratorContextNoInheritance_V5.IGeneratorContext context)
        {
            throw new MethodNotImplementedException(message: nameof(Execute1) + " was not implemented.");
        }
        Task<ActionResultV3.ActionResult> Execute2(IGeneratorContextNoInheritance_V5.IGeneratorContext context)
        {
            throw new MethodNotImplementedException(message: nameof(Execute2) + " was not implemented.");
        }

        Task<string> Execute3(IGeneratorContextNoInheritance_V5.IGeneratorContext context)
        {
            throw new MethodNotImplementedException(message: nameof(Execute3) + " was not implemented.");
        }

        Task<ActionResultSF> Ember.Scripting.ScriptMethods.IExecuteAsync.ExecuteAsync(IContext context)     //if bugs maybe put as async and await ExecuteAsync
        {
            return ExecuteAsync(context);
        }
        Task<ActionResultSF> Ember.Scripting.ScriptMethods.IExecute1.Execute1(IContext context)     //if bugs maybe put as async and await ExecuteAsync
        {
            return Execute1(context);
        }
        Task<ActionResultSF> Ember.Scripting.ScriptMethods.IExecute2.Execute2(IContext context)     //if bugs maybe put as async and await ExecuteAsync
        {
            return Execute2(context);
        }
        Task<string> Ember.Scripting.ScriptMethods.IExecute3.Execute3(IContext context)     //if bugs maybe put as async and await ExecuteAsync
        {
            return Execute3(context);
        }
    }
}

// public class GeneratorActionBaseClass : IGeneratorActionScript
// {
//     public Task<ActionResultSF> ExecuteAsyncXXX(IGeneratorBaseInterfaceSF context)
//     {
//         throw new NotImplementedException();
//     }

//     public IGeneratorActionScript Upgrade(GeneratorScriptsV3.IGeneratorActionScript script)
//     {

//     }
// }


