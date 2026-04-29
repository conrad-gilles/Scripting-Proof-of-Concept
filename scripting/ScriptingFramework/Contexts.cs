namespace Ember.Scripting.ScriptingFramework;

public interface IContext
{

}
public interface IDowngradeableContext : IContext
{
    public IDowngradeableContext Downgrade();
}
public interface IRecentContext : IContext    //maybe should just inherit from IContext? but if its recent it implies theres a pervious version?
{

}


public static class ContextManager
{
    public static IContext CreateByDowngrade(int desiredVersion, IRecentContext recentCtx)
    {
        Dictionary<int, Type> contextVersionMap = ContextVersionScanner.GetClassDictionary(recentCtx);

        if (contextVersionMap.Values.Contains(recentCtx.GetType()) == false)
        {
            throw new NoContextClassDefinedForApiVException("No Context class defined in " + nameof(contextVersionMap) + " for the passed Context Type: " + recentCtx.GetType().FullName);
        }

        Type recentCtxType = recentCtx.GetType();
        Type desiredCtxType = contextVersionMap[desiredVersion];
        IDowngradeableContext context = (IDowngradeableContext)recentCtx;

        int iterations = 0;
        int maxIterations = contextVersionMap.Keys.Count() + 3;

        while (recentCtxType != desiredCtxType && iterations <= maxIterations)
        {
            try
            {
                context = context.Downgrade();
                recentCtxType = context.GetType();
            }
            catch (Exception e)
            {
                throw new DowngradeFailedInEmberException("CreateContextByDowngrade failed in while.", e);
            }
            if (iterations > maxIterations)
            {
                throw new LoopExecutedTooManyTimesException("Somethign went wrong trying to downgrade the context");
            }
            iterations++;
        }
        return context;
    }
}