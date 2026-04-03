using Ember.Scripting;
using Ember.Simulation;

namespace BlazorUI.Services
{
    public class ScriptManagerAccessor
    {
        public IScriptManagerDeleteAfter Current { get; set; } = default!;
    }
    public class EmberInternalFacadeAccessor
    {
        internal EmberInternalFacade Current { get; set; } = default!;
    }
    public class ScriptRepositoryAccessor
    {
        // internal Scriptreposit Current { get; set; } = default!;
    }

}