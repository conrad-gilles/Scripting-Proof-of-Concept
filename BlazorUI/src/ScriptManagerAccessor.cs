using Ember.Scripting;
using Ember.Simulation;

namespace BlazorUI.Services
{
    public class ScriptManagerAccessor
    {
        public IScriptManagerBaseExtended Current { get; set; } = default!;
    }
    public class EmberInternalFacadeAccessor
    {
        internal ScriptManager Current { get; set; } = default!;
    }
    public class ScriptRepositoryAccessor
    {
        // internal Scriptreposit Current { get; set; } = default!;
    }

}