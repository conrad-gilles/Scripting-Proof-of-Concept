namespace Ember.Scripting;

public interface IScriptManager
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
    Task<Guid> CreateScript(string sourceCode, string userName = "Default", int apiVersion = -1, DateTime? createdAt = null);

    /// <summary>
    /// Updates existing script source code and recompiles for all compatible API versions
    /// </summary>
    /// <param name="scriptId"></param>
    /// <param name="newSourceCode"></param>
    /// <param name="userName"></param>
    /// <param name="apiVersion"></param>
    /// <returns></returns>
    Task UpdateScript(Guid scriptId, string newSourceCode, string userName = "Default", int apiVersion = -1);

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

    /// <summary>
    /// Returns all scripts with optional filtering by type, API version, or creation date
    /// </summary>
    /// <param name="filters"></param>
    /// <param name="includeCaches"></param>
    /// <returns></returns>
    Task<List<CustomerScript>> ListScripts(CustomerScriptFilter filters = null!, bool includeCaches = false);

    #endregion

    #region Compilation Operations

    /// <summary>
    /// Compiles a specific script for a target API version, if target version not specified just takes the recent one
    /// </summary>
    /// <param name="scriptId"></param>
    /// <param name="targetApiVersion"></param>
    /// <returns></returns>
    Task CompileScript(Guid scriptId, int targetApiVersion = -1);

    /// <summary>
    /// Compiles all compatible scripts for a new API version
    /// </summary>
    /// <returns></returns>
    Task CompileAllScripts();

    /// <summary>
    /// Recompiles script for all active API versions
    /// </summary>
    /// <param name="scriptId"></param>
    /// <returns></returns>
    Task RecompileScript(Guid scriptId);

    /// <summary>
    /// Performs syntax and interface validation without saving
    /// </summary>
    /// <param name="sourceCode"></param>
    /// <returns></returns>
    Task<string> ValidateScript(string sourceCode);

    /// <summary>
    /// Retrieves compilation error details
    /// </summary>
    /// <param name="scriptId"></param>
    /// <param name="apiVersion"></param>
    /// <returns></returns>
    Task<string> GetCompilationErrors(Guid scriptId, int apiVersion = -1);

    #endregion

    #region Execution Operations

    /// <summary>
    /// Executes a Generator Action script with provided context, realisitcally not needed
    /// </summary>
    /// <param name="scriptId"></param>
    /// <param name="context"></param>
    /// <param name="apiVersion"></param>
    /// <returns></returns>
    Task<ActionResultBaseClass> ExecuteActionScript(Guid scriptId, GeneratorContext context, int apiVersion = -1);

    /// <summary>
    /// Executes a Generator Condition script and returns boolean result, realisitcally not needed
    /// </summary>
    /// <param name="scriptId"></param>
    /// <param name="context"></param>
    /// <param name="apiVersion"></param>
    /// <returns></returns>
    Task<bool> ExecuteConditionScript(Guid scriptId, GeneratorContext context, int apiVersion = -1);

    /// <summary>
    /// Generic execution that detects script type automatically
    /// </summary>
    /// <param name="scriptId"></param>
    /// <param name="context"></param>
    /// <param name="currentApiVersion"></param>
    /// <returns></returns>
    Task<object> ExecuteScriptById(Guid scriptId, GeneratorContext context, int currentApiVersion = -1);

    #endregion

    #region Cache Management

    /// <summary>
    /// Retrieves compiled assembly bytes from cache
    /// </summary>
    /// <param name="scriptId"></param>
    /// <param name="currentApiVersion"></param>
    /// <returns></returns>
    Task<byte[]> GetCompiledCache(Guid scriptId, int currentApiVersion = -1);

    /// <summary>
    /// Removes all compiled versions of a script
    /// </summary>
    /// <param name="scriptId"></param>
    /// <returns></returns>
    Task ClearScriptCache(Guid scriptId);

    /// <summary>
    /// Removes all compiled caches (maintenance operation)
    /// </summary>
    /// <returns></returns>
    Task ClearAllCaches();

    /// <summary>
    /// Background job to precompile all compatible scripts
    /// </summary>
    /// <returns></returns>
    Task PrecompileForApiVersion();

    #endregion

    #region Version Management

    /// <summary>
    /// Returns list of currently active API versions from Ember instances
    /// </summary>
    /// <returns></returns>
    Task<List<int>> GetActiveApiVersions();

    Task<int> GetRecentApiVersion();

    /// <summary>
    /// Returns which API versions a script is compatible with
    /// </summary>
    /// <param name="scriptId"></param>
    /// <returns></returns>
    Task<int> GetScriptCompatibility(Guid scriptId);

    /// <summary>
    /// Validates if script can run on target version
    /// </summary>
    /// <param name="scriptId"></param>
    /// <param name="targetApiVersion"></param>
    /// <returns></returns>
    Task<bool> CheckVersionCompatibility(Guid scriptId, int targetApiVersion);

    /// <summary>
    /// Registers a new Ember instance in the system
    /// </summary>
    /// <param name="instanceId"></param>
    /// <param name="emberVersion"></param>
    /// <param name="apiVersion"></param>
    /// <returns></returns>
    Task RegisterEmberInstance(Guid instanceId, string emberVersion, int apiVersion);

    #endregion

    #region Duplicate Detection & Cleanup

    /// <summary>
    /// Identifies duplicate scripts based on source code equivalence
    /// </summary>
    /// <returns></returns>
    Task<(List<Guid> scriptGUIDs, Dictionary<Guid, int> cacheGUIDs)> DetectDuplicates();

    /// <summary>
    /// Removes duplicate scripts and orphaned caches
    /// </summary>
    /// <returns></returns>
    Task RemoveDuplicates();

    /// <summary>
    /// Removes caches without associated scripts
    /// </summary>
    /// <returns></returns>
    Task CleanupOrphanedCaches();

    #endregion

    #region Monitoring & Diagnostics

    /// <summary>
    /// Returns execution logs and metrics
    /// </summary>
    /// <param name="scriptId"></param>
    /// <returns></returns>
    Task GetScriptExecutionHistory(Guid scriptId);

    /// <summary>
    /// Returns compilation success/failure rates and performance metrics
    /// </summary>
    /// <returns></returns>
    Task GetCompilationStatistics();

    /// <summary>
    /// Validates database connectivity, API version consistency, and system state
    /// </summary>
    /// <returns></returns>
    Task HealthCheck();

    /// <summary>
    /// Extracts className, baseTypeName, and version from script
    /// </summary>
    /// <param name="scriptId"></param>
    /// <returns></returns>
    Task<string> GetScriptMetadata(Guid scriptId);

    string GetUserName();

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
