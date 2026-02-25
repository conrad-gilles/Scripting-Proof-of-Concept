using Ember.Scripting;

/// <summary>
/// Simplest implementation of the Condition script probably should make them private in real application to prevent user from using
/// </summary>
/// <summary>
/// Implementation using 1 generic for Context
/// </summary>
public interface IGeneratorConditionScript<TContext> : IGeneratorConditionScript    //todo this needs to get tested still
where TContext : IGeneratorReadOnlyContext
{
    new Task<bool> EvaluateAsync(TContext context);
    // Task<bool> IGeneratorConditionScript.EvaluateAsync(IGeneratorReadOnlyContext context)
    Task<bool> IGeneratorConditionScript.EvaluateAsync(IGeneratorBaseInterface context)
    {
        return EvaluateAsync((TContext)context);
    }
}
public interface IGeneratorActionScript<TContext> : IGeneratorActionScript
where TContext : IGeneratorContext
{
    new Task<ActionResultBaseClass> ExecuteAsync(TContext context);

    //explicit default implementation for the base interface
    // Task<ActionResultBaseClass> IGeneratorActionScript.ExecuteAsync(IGeneratorContext context)
    Task<ActionResultBaseClass> IGeneratorActionScript.ExecuteAsync(IGeneratorBaseInterface context)
    {
        return ExecuteAsync((TContext)context);
    }
}

/// <summary>
/// Implementation using 2 generics, one for context and one additional for ActionResult
/// </summary>

public interface IGeneratorActionScript<TContext, TActionResult> : IGeneratorActionScript
where TContext : IGeneratorBaseInterface    //changed from IGeneratorContext
where TActionResult : ActionResultBaseClass
{
    new Task<TActionResult> ExecuteAsync(TContext context);

    //explicit default implementation for the base interface
    async Task<ActionResultBaseClass> IGeneratorActionScript.ExecuteAsync(IGeneratorBaseInterface context)
    {
        return await ExecuteAsync((TContext)context);
    }
}

/// <summary>
/// Implementation using versioning of Generator scripts good for string control over what version of context and what version of the return type the user uses
/// </summary>

public interface IGeneratorActionScriptV2 : IGeneratorActionScript
{
    Task<ActionResultV2> ExecuteAsync(IGeneratorContext_V2 context);

    Task<ActionResultBaseClass> IGeneratorActionScript.ExecuteAsync(IGeneratorBaseInterface context)     //if bugs maybe put as async and await ExecuteAsync
    {
        return ExecuteAsync(context);
    }
}
public interface IGeneratorActionScriptV3 : IGeneratorActionScript
{
    Task<ActionResultV3NoInheritance> ExecuteAsync(IGeneratorContext_V3 context);

    Task<ActionResultBaseClass> IGeneratorActionScript.ExecuteAsync(IGeneratorBaseInterface context)     //if bugs maybe put as async and await ExecuteAsync
    {
        return ExecuteAsync(context);
    }
}
public interface IGeneratorActionScriptV4Vaccine : IGeneratorActionScript
{
    Task<ActionResultV3NoInheritance> ExecuteAsync(IGeneratorContextNoInheritance_V4 context);

    Task<ActionResultBaseClass> IGeneratorActionScript.ExecuteAsync(IGeneratorBaseInterface context)     //if bugs maybe put as async and await ExecuteAsync
    {
        return ExecuteAsync(context);
    }
}


