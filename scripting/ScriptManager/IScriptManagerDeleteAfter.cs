using Microsoft.CodeAnalysis;

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
  Task CompileScript(Guid scriptId, int? targetApiVersion = null);

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

  Task CreateScriptWithoutCompiling(Guid id, string sourceCode, string? userName = null);

  /// <summary>
  /// Recompiles script for all active API versions
  /// </summary>
  /// <param name="scriptId"></param>
  /// <returns></returns>
  Task RecompileAllCaches(Guid scriptId);

  Task RecompileCache(Guid scriptId, int apiVersion);

  /// <summary>
  /// Retrieves compilation error details
  /// </summary>
  /// <param name="scriptId"></param>
  /// <param name="apiVersion"></param>
  /// <returns></returns>
  Task<string> GetCompilationErrors(Guid scriptId, int? apiVersion = null);

  Task<bool> ThrowCompilationErrors(string script);

  /// <summary>
  /// Gets the tuple from a script containing Class Name, base type name and the last integer of the declared version in the name
  /// </summary>
  /// <param name="script"></param>
  /// <returns></returns>
  ValidationRecord BasicValidationBeforeCompiling(string script);

  INamedTypeSymbol GetBaseType(string script);

  #endregion

  #region Cache Management

  /// <summary>
  /// Retrieves compiled assembly bytes from cache
  /// </summary>
  /// <param name="scriptId"></param>
  /// <param name="currentApiVersion"></param>
  /// <returns></returns>
  Task<CompiledScripts> GetCompiledCache(Guid scriptId, int? currentApiVersion = null);

  /// <summary>
  /// Removes all compiled versions of a script
  /// </summary>
  /// <param name="scriptId"></param>
  /// <returns></returns>
  Task ClearScriptCache(Guid scriptId);


  Task DeleteScriptCache(Guid id, int apiVersion);

  /// <summary>
  /// Gets all Compiled Script Caches in the Database
  /// </summary>
  /// <returns></returns>
  Task<List<CompiledScripts>> GetAllCompiledScriptCaches();

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
  // Task EnsureDeletedCreated();

  Task DeleteAllData();

  #endregion

  #region Version Management

  /// <summary>
  /// Returns list of currently active API versions from Ember instances
  /// </summary>
  /// <returns></returns>
  Task<List<int>> GetActiveApiVersions();

  int GetRunningApiVersion();

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
  Task<bool> CheckVersionCompatibility(Guid scriptId, int? targetApiVersion = null);

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
  Task<DuplicateRecord> DetectDuplicates();

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

  Task<Guid> GetScriptId(string scriptName, ScriptTypes scriptType);

  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  Task<Dictionary<int, List<CompiledScripts>>> GetCachesForEachApiVersion();

  string GetUserName();

  #endregion
}


