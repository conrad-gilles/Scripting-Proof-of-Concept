using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Ember.Scripting;

namespace Ember.Simulation
{
    public class ContextManagement
    {
        private readonly ISccriptManagerDeleteAfter _scriptManager;
        public ContextManagement(ISccriptManagerDeleteAfter scriptManager)
        {
            _scriptManager = scriptManager;
        }

        public async Task<GeneratorContextSF> CreateByDowngrade(Guid id, ActiveGeneratorContext ctx)
        {
            CustomerScript script = await _scriptManager.GetScript(id);
            if (script.SourceCode == null)
            {
                throw new Exception();
            }
            var vali = _scriptManager.BasicValidationBeforeCompiling(script.SourceCode!);
            int? apiV = null;
            if (apiV == null)
            {
                apiV = _scriptManager.GetRunningApiVersion();
            }
            Type recentType;
            Dictionary<int, Type> contextVersionMap = ContextVersionScanner.GetClassDictionary();

            if (contextVersionMap.Keys.Contains((int)apiV) == false)
            {
                throw new Exception("No Context class defined in " + nameof(contextVersionMap) + " for the passed API version.");
                // might be better to instead return latest version?
                // recentType = contextVersionMap.Last().Value;
            }
            recentType = contextVersionMap[(int)apiV];
            Type desiredType = contextVersionMap[vali.Version];
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
                        throw new Exception("CreateContextByDowngrade failed in while.", e);
                    }
                }
                if (iterations > maxIterations)
                {
                    throw new Exception("Somethign went wrong trying to downgrade the context");
                }
                iterations++;
            }
            // return (ActiveGeneratorContext)context;
            return (GeneratorContextSF)context;
        }
    }
}