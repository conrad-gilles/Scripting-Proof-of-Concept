namespace Ember.Scripting.ScriptManager;

public interface IScriptManagerExtended : IScriptManager
{
    #region Script Lifecycle

    /// <summary>
    /// Validates, compiles, and stores a new script
    /// </summary>
    /// <param name="sourceCode"></param>
    /// <param name="apiVersion"></param>
    /// <param name="createdAt"></param>
    /// <returns></returns>
    Task<CustomerScript> CreateScript(string sourceCode, int? apiVersion = null, DateTime? createdAt = null);
    /// <summary>
    /// Updates existing script source code
    /// </summary>
    /// <param name="scriptId"></param>
    /// <param name="newSourceCode"></param>
    /// <param name="apiVersion"></param>
    /// <returns></returns>
    Task UpdateScriptSC(Guid scriptId, string newSourceCode, bool allowFaultySave = false, int? apiVersion = null);

    Task UpdateScriptAndCompile(Guid scriptId, string newSourceCode, int? apiVersion = null);

    Task UpdateScriptNT<ScriptType>(string name, string newSourceCode, int? apiVersion = null) where ScriptType : IScriptType;

    Task UpdateScriptAndCompileNT<ScriptType>(string name, string newSourceCode, int? apiVersion = null) where ScriptType : IScriptType;

    /// <summary>
    /// Removes script and all associated compiled caches
    /// </summary>
    /// <param name="scriptId"></param>
    /// <returns></returns>
    Task DeleteScript(Guid scriptId);

    Task DeleteScriptNT<ScriptType>(string scriptName) where ScriptType : IScriptType;

    /// <summary>
    /// Retrieves script metadata and source code
    /// </summary>
    /// <param name="scriptId"></param>
    /// <param name="includeCaches"></param>
    /// <returns></returns>
    Task<CustomerScript> GetScript(Guid scriptId, bool includeCaches = false);

    Task<CustomerScript> GetScriptNT<ScriptType>(string name, bool includeCaches = false) where ScriptType : IScriptType;

    #endregion
}
