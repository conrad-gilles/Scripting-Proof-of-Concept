using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Ember.Scripting;

namespace Ember.Simulation
{
    public class ContextManagement
    {
        private readonly IScriptManagerExtended _scriptManager;
        public ContextManagement(IScriptManagerExtended scriptManager)
        {
            _scriptManager = scriptManager;
        }

        public async Task<Context> CreateByDowngrade(int desiredVersion, RecentContext ctx)
        {
            int apiV = _scriptManager.GetRunningApiVersion();

            Type recentType;
            Dictionary<int, Type> contextVersionMap = ContextVersionScanner.GetClassDictionary();

            if (contextVersionMap.Keys.Contains(apiV) == false)
            {
                throw new NoContextClassDefinedForApiVException("No Context class defined in " + nameof(contextVersionMap) + " for the passed API version.");
                // might be better to instead return latest version?
                // recentType = contextVersionMap.Last().Value;
            }
            recentType = contextVersionMap[apiV];
            Type desiredType = contextVersionMap[desiredVersion];
            Context context = ctx;
            int iterations = 0;
            int maxIterations = ContextVersionScanner.GetClassDictionary().Keys.Count() + 3;

            while (recentType != desiredType && iterations <= maxIterations)
            {
                if (recentType == typeof(GeneratorContextNoInherVaccineV5.GeneratorContext))
                {
                    try
                    {
                        context = context.Downgrade();
                        recentType = context.GetType();
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
}