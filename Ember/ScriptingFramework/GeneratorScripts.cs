using Ember.Scripting;
using GeneratorScriptsGeneric;
namespace Ember.Sandbox.ScriptingFrameWork.ScriptTypes
{
    public interface IConditionScript : IScriptType
    {

    }

    public interface IActionScript : IScriptType
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
    [MetaDataIScript(version: 1, returnType: IMDScriptReturnType.Condition, type: IMDScriptType.GenericSimple)]
    public interface IConditionScript<TContext>
        : IScriptVersion
        , IConditionScript
        where TContext : IContext
    {
        Task<bool> EvaluateAsync(TContext context)
        {
            throw new MethodNotImplementedException(message: nameof(EvaluateAsync) + " was not implemented.");
        }
    }
    [MetaDataIScript(version: 1, returnType: IMDScriptReturnType.Action, type: IMDScriptType.GenericSimple)]
    public interface IActionScript<TContext>
    : IScriptVersion
    , IActionScript
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
    [MetaDataIScript(version: 1, returnType: IMDScriptReturnType.Action, type: IMDScriptType.Generic)]
    public interface IActionScript<TContext, TActionResult>
        : IScriptVersion
        , IActionScript
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
    [MetaDataIScript(version: 2)]
    public interface IActionScript
    : IScriptVersion
    , Ember.Sandbox.ScriptingFrameWork.ScriptTypes.IActionScript
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
    [MetaDataIScript(version: 3)]
    public interface IActionScript
        : IScriptVersion
        , Ember.Sandbox.ScriptingFrameWork.ScriptTypes.IActionScript
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
    [MetaDataIScript(version: 4)]
    public interface IActionScript
    : IScriptVersion
    , Ember.Sandbox.ScriptingFrameWork.ScriptTypes.IActionScript
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
