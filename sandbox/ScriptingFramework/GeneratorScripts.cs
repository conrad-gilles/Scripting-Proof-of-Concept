using Ember.Scripting;
using Ember.Sandbox.ScriptMethods;

namespace Ember.Sandbox.ScriptingFrameWork.ScriptTypes
{
    [MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Condition, type: IGeneratorScriptType.AbstractBaseInSF)]
    public interface IConditionScript : IScriptType
    {
        // Task<bool> EvaluateAsync(IContext context);
    }

    [MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Action, type: IGeneratorScriptType.AbstractBaseInSF)]
    public interface IActionScript : IScriptType
    {
        // Task<ActionResultSF> ExecuteAsync(IContext context);
    }
}

namespace GeneratorScriptsGenericSimple
{
    /// <summary>
    /// Simplest implementation of the Condition script probably should make them private in real application to prevent user from using
    /// </summary>
    /// <summary>
    /// Implementation using 1 generic for Context
    /// </summary>
    [MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Condition, type: IGeneratorScriptType.GenericSimple)]
    public interface IConditionScript<TContext> :
      IScriptMethodsCondition  //todo this needs to get tested still
                               // where TContext : IGeneratorReadOnlyContextV1.IGeneratorContext
        where TContext : IContext
    {
        Task<bool> EvaluateAsync(TContext context)
        {
            throw new MethodNotImplementedException(message: nameof(EvaluateAsync) + " was not implemented.");
        }
    }
    [MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Action, type: IGeneratorScriptType.GenericSimple)]
    public interface IActionScript<TContext> :
     IScriptMethodsAction
    where TContext : IGeneratorContext_V2.IGeneratorContext
    {

        Task<ActionResultBase> ExecuteAsync(TContext context)
        {
            throw new MethodNotImplementedException(message: nameof(ExecuteAsync) + " was not implemented.");
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
     IScriptMethodsAction
        where TContext : IContext    //changed from IGeneratorContext
        where TActionResult : ActionResultBase
    {
        Task<TActionResult> ExecuteAsync(TContext context)
        {
            throw new MethodNotImplementedException(message: nameof(ExecuteAsync) + " was not implemented.");
        }
    }
}


/// <summary>
/// Implementation using versioning of Generator scripts good for strict control over what version of context and what version of the return type the user uses
/// </summary>

namespace GeneratorScriptsV2
{
    [MetaDataIGeneratorScript(version: 2, contextVersion: typeof(IGeneratorContext_V3.IGeneratorContext), actionResultVersion: typeof(ActionResultV2.ActionResult))]
    public interface IActionScript :
      IScriptMethodsAction
    {
        Task<ActionResultV2.ActionResult> ExecuteAsync(IGeneratorContext_V3.IGeneratorContext context)
        {
            throw new MethodNotImplementedException(message: nameof(ExecuteAsync) + " was not implemented.");
        }
    }
}
namespace GeneratorScriptsV3
{
    // public interface IGeneratorActionScript : Ember.Scripting.IGeneratorActionScript
    // [MetaDataIGeneratorScript(version: 3)]
    [MetaDataIGeneratorScript(version: 3, contextVersion: typeof(IGeneratorContext_V4.IGeneratorContext), actionResultVersion: typeof(ActionResultV3.ActionResult))]
    public interface IActionScript : IScriptMethodsAction
    {
        Task<ActionResultV3.ActionResult> ExecuteAsync(IGeneratorContext_V4.IGeneratorContext context)
        {
            throw new MethodNotImplementedException(message: nameof(ExecuteAsync) + " was not implemented.");
        }
        Task<ActionResultV3.ActionResult> Execute1(IGeneratorContext_V4.IGeneratorContext context)
        {
            throw new MethodNotImplementedException(message: nameof(Execute1) + " was not implemented.");
        }
        Task<ActionResultV3.ActionResult> Execute2(IGeneratorContext_V4.IGeneratorContext context)
        {
            throw new MethodNotImplementedException(message: nameof(Execute2) + " was not implemented.");
        }
        Task<string> Execute3(IGeneratorContext_V4.IGeneratorContext context)
        {
            throw new MethodNotImplementedException(message: nameof(Execute3) + " was not implemented.");
        }
    }
}
namespace GeneratorScriptsV4
{
    [MetaDataIGeneratorScript(version: 4, contextVersion: typeof(IGeneratorContextNoInheritance_V5.IGeneratorContext), actionResultVersion: typeof(ActionResultV3.ActionResult))]
    public interface IActionScript :
     IScriptMethodsAction
    {
        Task<ActionResultV3.ActionResult> ExecuteAsync(IGeneratorContextNoInheritance_V5.IGeneratorContext context)
        {
            throw new MethodNotImplementedException(message: nameof(ExecuteAsync) + " was not implemented.");
        }
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


