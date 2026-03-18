namespace Ember.Scripting;

public interface IScriptManagerExtended : IScriptManager
{
    #region Script Lifecycle

    /// <summary>
    /// Validates, compiles, and stores a new script
    /// </summary>
    /// <param name="sourceCode"></param>
    /// <param name="scriptType"></param>
    /// <param name="userName"></param>
    /// <param name="apiVersion"></param>
    /// <param name="createdAt"></param>
    /// <returns></returns>
    Task<Guid> CreateScript(string sourceCode, string userName = "Default", int? apiVersion = null, DateTime? createdAt = null, bool checkForDuplicates = false);

    Task<ScriptNameType> CreateScriptUsingNameType(string sourceCode, string userName = "Default", int? apiVersion = null, DateTime? createdAt = null, bool checkForDuplicates = false);

    /// <summary>
    /// Updates existing script source code and recompiles for all compatible API versions
    /// </summary>
    /// <param name="scriptId"></param>
    /// <param name="newSourceCode"></param>
    /// <param name="userName"></param>
    /// <param name="apiVersion"></param>
    /// <returns></returns>
    Task UpdateScript(Guid scriptId, string newSourceCode, string? userName = null, int? apiVersion = null);

    /// <summary>
    /// Removes script and all associated compiled caches
    /// </summary>
    /// <param name="scriptId"></param>
    /// <returns></returns>
    Task DeleteScript(Guid scriptId);

    /// <summary>
    /// Retrieves script metadata and source code
    /// </summary>
    /// <param name="scriptId"></param>
    /// <param name="includeCaches"></param>
    /// <returns></returns>
    Task<CustomerScript> GetScript(Guid scriptId, bool includeCaches = false);

    #endregion
}

public class CustomerScriptFilter
{
    public string? ScriptName;

    public string? ScriptType;

    public string? SourceCode;

    public int? MinApiVersion;

    public DateTime? CreatedAt;

    public DateTime? ModifiedAt;

    public string? CreatedBy;
    public CustomerScriptFilter(string? scriptName = null, string? scriptType = null, string? sourceCode = null, int? minApiVersion = null,
     DateTime? createdAt = null, DateTime? modifiedAt = null, string? createdBy = null)
    {
        ScriptName = scriptName;
        ScriptType = scriptType;
        SourceCode = sourceCode;
        MinApiVersion = minApiVersion;
        CreatedAt = createdAt;
        ModifiedAt = modifiedAt;
        CreatedBy = createdBy;
    }

}
