namespace Ember.Scripting.ScriptingFramework;

public abstract class Context : IDowngradeableContext       //remove use interfr
{
    public abstract IContext Downgrade();
}

public interface IContext
{

}

public interface IDowngradeableContext
{
    public IContext Downgrade();
}

public static class ContextManager
{
    public static async Task<IContext> CreateByDowngrade(int desiredVersion, IContext recentCtx)
    {
        Dictionary<int, Type> contextVersionMap = ContextVersionScanner.GetClassDictionary();

        if (contextVersionMap.Values.Contains(recentCtx.GetType()) == false)
        {
            throw new NoContextClassDefinedForApiVException("No Context class defined in " + nameof(contextVersionMap) + " for the passed Context Type: " + recentCtx.GetType().FullName);
        }

        Type recentCtxType = recentCtx.GetType();
        Type desiredCtxType = contextVersionMap[desiredVersion];
        Context context = (Context)recentCtx;

        int iterations = 0;
        int maxIterations = ContextVersionScanner.GetClassDictionary().Keys.Count() + 3;

        while (recentCtxType != desiredCtxType && iterations <= maxIterations)
        {
            if (recentCtxType == recentCtx.GetType())
            {
                try
                {
                    context = (Context)context.Downgrade();
                    recentCtxType = context.GetType();
                }
                catch (Exception e)
                {
                    throw new DowngradeFailedInEmberException("CreateContextByDowngrade failed in while.", e);
                }
            }
            if (iterations > maxIterations)
            {
                throw new LoopExecutedTooManyTimesException("Somethign went wrong trying to downgrade the context");
            }
            iterations++;
        }
        return (IContext)context;
    }
}