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
    public interface IGeneratorConditionScript<TContext> : IGeneratorConditionScript    //todo this needs to get tested still
    where TContext : IGeneratorReadOnlyContextV1.IGeneratorContext
    {
        Task<bool> EvaluateAsync(TContext context);
        // Task<bool> IGeneratorConditionScript.EvaluateAsync(IGeneratorReadOnlyContext context)
        Task<bool> IGeneratorConditionScript.EvaluateAsync(IGeneratorBaseInterfaceSF context)
        {
            return EvaluateAsync((TContext)context);
        }
    }
    [MetaDataIGeneratorScript(version: 1, returnType: IGeneratorScriptReturnType.Action, type: IGeneratorScriptType.GenericSimple)]
    public interface IGeneratorActionScript<TContext> : Ember.Simulation.IEmberScriptBase
    where TContext : IGeneratorContext_V2.IGeneratorContext
    {

        Task<ActionResultSF> ExecuteAsync(TContext context);

        //explicit default implementation for the base interface
        // Task<ActionResultBaseClass> IGeneratorActionScript.ExecuteAsync(IGeneratorContext context)
        Task<ActionResultSF> IGeneratorActionScript.ExecuteAsync(IGeneratorBaseInterfaceSF context)
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
    public interface IGeneratorActionScript<TContext, TActionResult> : Ember.Simulation.IEmberScriptBase
        where TContext : IGeneratorBaseInterfaceSF    //changed from IGeneratorContext
        where TActionResult : ActionResultSF
    {
        Task<TActionResult> ExecuteAsync(TContext context);

        //explicit default implementation for the base interface
        async Task<ActionResultSF> IGeneratorActionScript.ExecuteAsync(IGeneratorBaseInterfaceSF context)
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
    public interface IGeneratorActionScript : Ember.Simulation.IEmberScriptBase
    {
        Task<ActionResultV2.ActionResult> ExecuteAsync(IGeneratorContext_V3.IGeneratorContext context);

        Task<ActionResultSF> Ember.Scripting.IGeneratorActionScript.ExecuteAsync(IGeneratorBaseInterfaceSF context)     //if bugs maybe put as async and await ExecuteAsync
        {
            return ExecuteAsync(context);
        }
    }
}
namespace GeneratorScriptsV3
{
    [MetaDataIGeneratorScript(version: 3)]
    // public interface IGeneratorActionScript : Ember.Scripting.IGeneratorActionScript
    public interface IGeneratorActionScript : Ember.Simulation.IEmberScriptBase
    {
        Task<ActionResultV3.ActionResult> ExecuteAsync(IGeneratorContext_V4.IGeneratorContext context);

        Task<ActionResultSF> Ember.Scripting.IGeneratorActionScript.ExecuteAsync(IGeneratorBaseInterfaceSF context)     //if bugs maybe put as async and await ExecuteAsync
        {
            return ExecuteAsync(context);
        }
    }
}
namespace GeneratorScriptsV4
{
    [MetaDataIGeneratorScript(version: 4)]
    public interface IGeneratorActionScript : Ember.Simulation.IEmberScriptBase
    {
        Task<ActionResultV3.ActionResult> ExecuteAsync(IGeneratorContextNoInheritance_V5.IGeneratorContext context);

        Task<ActionResultSF> Ember.Scripting.IGeneratorActionScript.ExecuteAsync(IGeneratorBaseInterfaceSF context)     //if bugs maybe put as async and await ExecuteAsync
        {
            return ExecuteAsync(context);
        }
    }
}


