namespace Ember.Scripting;

public interface IScriptManagerExtended : IScriptManager
{
    #region Script Lifecycle

    /// <summary>
    /// Validates, compiles, and stores a new script
    /// </summary>
    /// <param name="sourceCode"></param>
    /// <param name="scriptType"></param>
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

    Task UpdateScriptNT(string name, ScriptTypes scriptType, string newSourceCode, int? apiVersion = null);

    Task UpdateScriptAndCompileNT(string name, ScriptTypes scriptType, string newSourceCode, int? apiVersion = null);

    /// <summary>
    /// Removes script and all associated compiled caches
    /// </summary>
    /// <param name="scriptId"></param>
    /// <returns></returns>
    Task DeleteScript(Guid scriptId);

    Task DeleteScriptNT(string scriptName, ScriptTypes scriptType);

    /// <summary>
    /// Retrieves script metadata and source code
    /// </summary>
    /// <param name="scriptId"></param>
    /// <param name="includeCaches"></param>
    /// <returns></returns>
    Task<CustomerScript> GetScript(Guid scriptId, bool includeCaches = false);

    Task<CustomerScript> GetScriptNT(string name, ScriptTypes scriptType, bool includeCaches = false);

    #endregion
}
