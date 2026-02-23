public interface IGeneratorConditionScript
{
    Task<bool> EvaluateAsync(IGeneratorReadOnlyContext context);
}
public interface IGeneratorActionScript
{
    Task<ActionResultBaseClass> ExecuteAsync(IGeneratorContext context);
}
public interface IGeneratorConditionScript<TContext> : IGeneratorConditionScript    //todo this needs to get tested still
where TContext : IGeneratorReadOnlyContext
{
    new Task<bool> EvaluateAsync(TContext context);
    Task<bool> IGeneratorConditionScript.EvaluateAsync(IGeneratorReadOnlyContext context)
    {
        return EvaluateAsync((TContext)context);
    }
}
public interface IGeneratorActionScript<TContext> : IGeneratorActionScript
where TContext : IGeneratorContext
{
    new Task<ActionResultBaseClass> ExecuteAsync(TContext context);

    //explicit default implementation for the base interface
    Task<ActionResultBaseClass> IGeneratorActionScript.ExecuteAsync(IGeneratorContext context)
    {
        return ExecuteAsync((TContext)context);
    }
}
public interface IGeneratorActionScript<TContext, TActionResult> : IGeneratorActionScript
where TContext : IGeneratorContext
where TActionResult : ActionResultBaseClass
{
    new Task<TActionResult> ExecuteAsync(TContext context);

    //explicit default implementation for the base interface
    async Task<ActionResultBaseClass> IGeneratorActionScript.ExecuteAsync(IGeneratorContext context)
    {
        return await ExecuteAsync((TContext)context);
    }
}
//todo version of cond script and action script v2 and son on, dont inherit implement empty interface
//upgrade methode vun action res 1 ob 2

