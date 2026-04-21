using Microsoft.Extensions.Logging;
using Ember.Scripting;
using System.Runtime.InteropServices;
using TypeInfo = Ember.Scripting.Versioning.TypeInfo;
namespace Ember.Scripting.ScriptingFramework;

public abstract class Context : IDowngradeableContext
{
    public abstract Context Downgrade();
}

public interface IContext
{

}

public interface IDowngradeableContext
{
    public Context Downgrade();
}

public static class ContextManager
{
    public static async Task<Context> CreateByDowngrade(int desiredVersion, Context recentCtx)
    {
        Dictionary<int, Type> contextVersionMap = ContextVersionScanner.GetClassDictionary();

        if (contextVersionMap.Values.Contains(recentCtx.GetType()) == false)
        {
            throw new NoContextClassDefinedForApiVException("No Context class defined in " + nameof(contextVersionMap) + " for the passed Context Type.");
        }

        Type recentCtxType = recentCtx.GetType();
        Type desiredCtxType = contextVersionMap[desiredVersion];
        Context context = recentCtx;

        int iterations = 0;
        int maxIterations = ContextVersionScanner.GetClassDictionary().Keys.Count() + 3;

        while (recentCtxType != desiredCtxType && iterations <= maxIterations)
        {
            if (recentCtxType == recentCtx.GetType())
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