using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Ember.Scripting;

namespace Ember.Simulation
{
    public class ContextManagement
    {
        private readonly IScriptManagerDeleteAfter _scriptManager;
        public ContextManagement(IScriptManagerDeleteAfter scriptManager)
        {
            _scriptManager = scriptManager;
        }

        public async Task<GeneratorContextSF> CreateByDowngrade(int desiredVersion, RecentGeneratorContext ctx)
        {
            // if (sourceCode == null)
            // {
            //     throw new SourceCodeNullWhenDowngradeException();
            // }
            // var vali = _scriptManager.BasicValidationBeforeCompiling(sourceCode!);
            int? apiV = null;
            if (apiV == null)
            {
                apiV = _scriptManager.GetRunningApiVersion();
            }
            Type recentType;
            Dictionary<int, Type> contextVersionMap = ContextVersionScanner.GetClassDictionary();

            if (contextVersionMap.Keys.Contains((int)apiV) == false)
            {
                throw new NoContextClassDefinedForApiVException("No Context class defined in " + nameof(contextVersionMap) + " for the passed API version.");
                // might be better to instead return latest version?
                // recentType = contextVersionMap.Last().Value;
            }
            recentType = contextVersionMap[(int)apiV];
            // Type desiredType = contextVersionMap[vali.Version];
            Type desiredType = contextVersionMap[desiredVersion];
            // Ember.Scripting.GeneratorContextSF context = CreateContextForApiV(data);
            Ember.Scripting.GeneratorContextSF context = ctx;
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
            // return (ActiveGeneratorContext)context;
            return (GeneratorContextSF)context;
        }
    }
}