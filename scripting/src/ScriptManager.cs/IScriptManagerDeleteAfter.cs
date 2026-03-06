namespace Ember.Scripting;

public interface ISccriptManagerDeleteAfter : IScriptManagerExtended
{
  Task<List<CustomerScript>> ListScripts(CustomerScriptFilter filters = null!, bool includeCaches = false);



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
  /// Saves a script source code to the DB even if it doesnt compile, later you could call GetCompilationErrors to get errors in it.
  /// </summary>
  /// <returns></returns>
  Task SaveScriptWithoutCompiling(Guid id, string sourceCode);

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
  string ValidateScript(string sourceCode);

  /// <summary>
  /// Retrieves compilation error details
  /// </summary>
  /// <param name="scriptId"></param>
  /// <param name="apiVersion"></param>
  /// <returns></returns>
  Task<string> GetCompilationErrors(Guid scriptId, int? apiVersion = null);

  /// <summary>
  /// Gets the tuple from a script containing Class Name, base type name and the last integer of the declared version in the name
  /// </summary>
  /// <param name="script"></param>
  /// <returns></returns>
  (string className, string baseTypeName, int versionInt) BasicValidationBeforeCompiling(string script);

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

  // /// <summary>
  // /// Generic execution that detects script type automatically
  // /// </summary>
  // /// <param name="scriptId"></param>
  // /// <param name="context"></param>
  // /// <param name="currentApiVersion"></param>
  // /// <returns></returns>
  // Task<object> ExecuteScriptById(Guid scriptId, GeneratorContext context, int currentApiVersion = -1);

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


  Task DeleteScriptCache(Guid id, int ApiVersion);

  /// <summary>
  /// Gets all Compiled Script Caches in the Database
  /// </summary>
  /// <returns></returns>
  Task<List<ScriptCompiledCache>> GetAllCompiledScriptCaches();

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

  /// <summary>
  ///Deletes the entire Database
  /// </summary>
  /// <returns></returns>
  Task EnsureDeletedCreated();

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
  void RegisterEmberInstance(Guid instanceId, string emberVersion, int apiVersion);

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
  void GetScriptExecutionHistory(Guid scriptId);

  /// <summary>
  /// Returns compilation success/failure rates and performance metrics
  /// </summary>
  /// <returns></returns>
  void GetCompilationStatistics();

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

  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  Task<Dictionary<int, List<ScriptCompiledCache>>> GetCachesForEachApiVersion();

  string GetUserName();

  #endregion
}


