using Ember.Scripting;
namespace Ember.Sandbox.ScriptingFrameWork.ScriptTypes
{
    [MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Condition, type: IGeneratorScriptType.AbstractBaseInSF)]
    public interface IConditionScript : IScriptType     //Do not rename
    {

    }

    [MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Action, type: IGeneratorScriptType.AbstractBaseInSF)]
    public interface IActionScript : IScriptType    //Do not rename
    {

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
    public interface IConditionScript<TContext>
        : IScriptVersion
        where TContext : IContext
    {
        Task<bool> EvaluateAsync(TContext context)
        {
            throw new MethodNotImplementedException(message: nameof(EvaluateAsync) + " was not implemented.");
        }
    }
    [MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Action, type: IGeneratorScriptType.GenericSimple)]
    public interface IActionScript<TContext>
    : IScriptVersion
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
    public interface IActionScript<TContext, TActionResult>
        : IScriptVersion
        where TContext : IContext
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
    [MetaDataIGeneratorScript(version: 2)]
    public interface IActionScript
    : IScriptVersion
    {
        Task<ActionResultV2.ActionResult> ExecuteAsync(IGeneratorContext_V3.IGeneratorContext context)
        {
            throw new MethodNotImplementedException(message: nameof(ExecuteAsync) + " was not implemented.");
        }
        Task<ActionResultV2.ActionResult> Execute1OldName(IGeneratorContext_V3.IGeneratorContext context)
        {
            throw new MethodNotImplementedException(message: nameof(Execute1OldName) + " was not implemented.");
        }
        Task<ActionResultV2.ActionResult> ExecuteMethodThatWasDeleted(IGeneratorContext_V3.IGeneratorContext context)
        {
            throw new MethodNotImplementedException(message: nameof(Execute1OldName) + " was not implemented.");
        }
    }
}
namespace GeneratorScriptsV3
{
    [MetaDataIGeneratorScript(version: 3)]
    public interface IActionScript
        : IScriptVersion
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
    [MetaDataIGeneratorScript(version: 4)]
    public interface IActionScript
    : IScriptVersion
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
