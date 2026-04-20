using Microsoft.CodeAnalysis;
using Ember.Scripting.Compilation;
using Ember.Scripting.Execution;
using Ember.Scripting.Persistence;

namespace Ember.Scripting.Manager;

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
  Task UpdateScript(Guid scriptId, string newSourceCode, bool allowFaultySave = false, int? apiVersion = null);

  Task UpdateScriptAndCompile(Guid scriptId, string newSourceCode, int? apiVersion = null);

  Task UpdateScript<ScriptType>(string name, string newSourceCode, int? apiVersion = null) where ScriptType : IScriptType;

  Task UpdateScriptAndCompile<ScriptType>(string name, string newSourceCode, int? apiVersion = null) where ScriptType : IScriptType;

  /// <summary>
  /// Removes script and all associated compiled caches
  /// </summary>
  /// <param name="scriptId"></param>
  /// <returns></returns>
  Task DeleteScript(Guid scriptId);

  Task DeleteScript<ScriptType>(string scriptName) where ScriptType : IScriptType;

  /// <summary>
  /// Retrieves script metadata and source code
  /// </summary>
  /// <param name="scriptId"></param>
  /// <param name="includeCaches"></param>
  /// <returns></returns>
  Task<CustomerScript> GetScript(Guid scriptId, bool includeCaches = false);

  Task<CustomerScript> GetScript<ScriptType>(string name, bool includeCaches = false) where ScriptType : IScriptType;

  Task<List<CustomerScript>> ListScripts(CustomerScriptFilter filters = null!, bool includeCaches = false);

  #endregion

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
  // Task SaveScriptWithoutCompiling(Guid id, string sourceCode);

  Task CreateScriptWithoutCompiling(Guid id, string sourceCode);

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
  Task<string> GetCompilationErrors(Guid scriptId);
  Task<List<ScriptCompilationError>> GetCompilationErrors(string sourceCode);

  Task<bool> TryCompile(string script);

  /// <summary>
  /// Gets the tuple from a script containing Class Name, base type name and the last integer of the declared version in the name
  /// </summary>
  /// <param name="script"></param>
  /// <returns></returns>
  ValidationRecord BasicValidationBeforeCompiling(string script);

  #endregion

  Task<object> ExecuteUnfinishedScriptBySourceCode(string sourceCode, Context context, string methodName, int? apiVersion = null, int? executionTime = null);


  #region Cache Management

  /// <summary>
  /// Retrieves compiled assembly bytes from cache
  /// </summary>
  /// <param name="scriptId"></param>
  /// <param name="currentApiVersion"></param>
  /// <returns></returns>
  Task<CompiledScript> GetCompiledCache(Guid scriptId, int? currentApiVersion = null);

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
  Task<List<CompiledScript>> GetAllCompiledScriptCaches();

  /// <summary>
  /// Removes all compiled caches (maintenance operation)
  /// </summary>
  /// <returns></returns>
  Task ClearAllCaches();

  /// <summary>
  ///Deletes the entire Database
  /// </summary>
  /// <returns></returns>
  // Task EnsureDeletedCreated();

  Task DeleteAllData();

  #endregion

  #region Version Management

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

  Task<Guid> GetScriptId<ScriptType>(string scriptName) where ScriptType : IScriptType;

  /// <summary>
  /// 
  /// </summary>
  /// <returns></returns>
  Task<Dictionary<int, List<CompiledScript>>> GetCachesForEachApiVersion();

  IUserSession GetUserSession();

  #endregion
}


