using Microsoft.CodeAnalysis.Emit;

namespace Ember.Scripting.Manager;

/// <summary>
/// Provides an extended set of operations for managing the lifecycle, compilation, caching, and execution of customer scripts.
/// </summary>
public interface IScriptManagerBaseExtended : IScriptManagerBase
{

  #region Script Lifecycle

  /// <summary>
  /// Validates, compiles, and adds a new script into the database.
  /// </summary>
  /// <param name="sourceCode">The raw C# source code of the script.</param>
  /// <param name="apiVersion">The specific API version to target. If null, targets the most recent active version.</param>
  /// <param name="createdAt">The optional timestamp for when the script was created.</param>
  /// <returns>A task returning the newly created and validated <see cref="CustomerScript"/> entity.</returns>
  Task<CustomerScript> CreateScript(string sourceCode, int? apiVersion = null, DateTime? createdAt = null);
  /// <summary>
  /// Updates the source code of an existing script identified by its unique ID.
  /// </summary>
  /// <param name="scriptId">The unique identifier of the target script.</param>
  /// <param name="newSourceCode">The updated C# source code.</param>
  /// <param name="allowFaultySave">If true, saves the script even if it contains compilation errors.</param>
  /// <param name="apiVersion">The specific API version to target. If null, uses the default or latest version.</param>
  Task UpdateScript(Guid scriptId, string newSourceCode, bool allowFaultySave = false, int? apiVersion = null);

  /// <summary>
  /// Updates the source code of an existing script and immediately triggers a recompilation.
  /// </summary>
  /// <param name="scriptId">The unique identifier of the target script.</param>
  /// <param name="newSourceCode">The updated C# source code.</param>
  /// <param name="apiVersion">The specific API version to compile against. If null, uses the default version.</param>
  Task UpdateScriptAndCompile(Guid scriptId, string newSourceCode, int? apiVersion = null);

  /// <summary>
  /// Updates the source code of a strongly-typed script identified by its assigned name.
  /// </summary>
  /// <typeparam name="ScriptType">The expected script interface type.</typeparam>
  /// <param name="name">The registered name of the script.</param>
  /// <param name="newSourceCode">The updated C# source code.</param>
  /// <param name="apiVersion">The specific API version to target.</param>
  Task UpdateScript<ScriptType>(string name, string newSourceCode, int? apiVersion = null) where ScriptType : IScriptType;

  /// <summary>
  /// Updates the source code of a strongly-typed script identified by its assigned name and immediately triggers recompilation.
  /// </summary>
  /// <typeparam name="ScriptType">The expected script interface type.</typeparam>
  /// <param name="name">The registered name of the script.</param>
  /// <param name="newSourceCode">The updated C# source code.</param>
  /// <param name="apiVersion">The specific API version to compile against.</param>
  Task UpdateScriptAndCompile<ScriptType>(string name, string newSourceCode, int? apiVersion = null) where ScriptType : IScriptType;

  /// <summary>
  /// Permanently deletes a script and purges all of its associated compiled caches from the database.
  /// </summary>
  /// <param name="scriptId">The unique identifier of the script to remove.</param>
  Task DeleteScript(Guid scriptId);

  /// <summary>
  /// Permanently deletes a strongly-typed script and its associated caches using its registered name.
  /// </summary>
  /// <typeparam name="ScriptType">The expected script interface type.</typeparam>
  /// <param name="scriptName">The registered name of the script to remove.</param>
  Task DeleteScript<ScriptType>(string scriptName) where ScriptType : IScriptType;

  /// <summary>
  /// Retrieves the CustomerScript object of a script.
  /// </summary>
  /// <param name="scriptId">The unique identifier of the script.</param>
  /// <param name="includeCaches">If true, includes the associated <see cref="CompiledScript"/> objects.</param>
  /// <returns>A task returning the requested <see cref="CustomerScript"/>.</returns>
  Task<CustomerScript> GetScript(Guid scriptId, bool includeCaches = false);

  /// <summary>
  ///  Retrieves the CustomerScript object of a script its registered name and its type.
  /// </summary>
  /// <typeparam name="ScriptType">The expected script interface type.</typeparam>
  /// <param name="name">The registered name of the script.</param>
  /// <param name="includeCaches">If true, includes the associated <see cref="CompiledScript"/>objects.</param>
  /// <returns>A task returning the requested <see cref="CustomerScript"/>.</returns>
  Task<CustomerScript> GetScript<ScriptType>(string name, bool includeCaches = false) where ScriptType : IScriptType;

  /// <summary>
  /// Retrieves a list of scripts matching the specified filtering criteria.
  /// </summary>
  /// <param name="filters">Criteria to filter the returned scripts (e.g., by type, status, or date).</param>
  /// <param name="includeCaches">If true, includes the associated <see cref="CompiledScript"/>objects.</param>
  /// <returns>A task returning a List of matching <see cref="CustomerScript"/> objects.</returns>
  Task<List<CustomerScript>> ListScripts(CustomerScriptFilter filters = null!, bool includeCaches = false);

  #endregion

  #region Compilation Operations

  /// <summary>
  /// Compiles an existing script and stores it to the Database.
  /// </summary>
  /// <param name="scriptId">The unique identifier of the script to compile.</param>
  /// <param name="targetApiVersion">The targeted API version. If null, the most recent version is used.</param>
  Task CompileScript(Guid scriptId, int? targetApiVersion = null);

  /// <summary>
  /// Triggers a batch compilation for all stored scripts inside the database.
  /// </summary>
  Task CompileAllScripts();

  /// <summary>
  /// Adds a script source code to the Database without trying to compile it.
  /// </summary>
  /// <param name="id">The unique identifier to assign to the new script.</param>
  /// <param name="sourceCode">The C# source code.</param>
  Task CreateScriptWithoutCompiling(Guid id, string sourceCode);

  /// <summary>
  /// Recompiles the specified script across all active or compatible API versions and updates its caches.
  /// </summary>
  /// <param name="scriptId">The unique identifier of the script to recompile.</param>
  /// <returns>A task representing the asynchronous batch recompilation operation.</returns>
  Task RecompileAllCaches(Guid scriptId);

  /// <summary>
  /// Forces a recompilation of a specific script for a single designated API version.
  /// </summary>
  /// <param name="scriptId">The unique identifier of the target script.</param>
  /// <param name="apiVersion">The designated API version to compile against.</param>
  Task RecompileCache(Guid scriptId, int apiVersion);

  /// <summary>
  /// Retrieves compilation error diagnostics for a specific script.
  /// </summary>
  /// <param name="scriptId">The unique identifier of the script.</param>
  /// <returns>A task returning a formatted string containing the compilation error details.</returns>
  Task<string> GetCompilationErrors(Guid scriptId);

  /// <summary>
  /// Analyzes <see langword="string"/> source code and returns the emit diagnostics and compilation results without persisting the script.
  /// </summary>
  /// <param name="sourceCode">The C# source code to evaluate.</param>
  /// <returns>A task returning the raw <see cref="EmitResult"/> if compilation diagnostics are generated.</returns>
  Task<EmitResult?> GetCompilationErrors(string sourceCode);

  /// <summary>
  /// Returns <see langword="true"/> <see langword="if"/> it compiles <see langword="and"/> <see langword="false"/> <see langword="if"/> it doesnt.      
  /// </summary>
  /// <param name="script">The raw C# source code.</param>
  Task<bool> TryCompile(string script);

  /// <summary>
  /// Analyzes the script syntax to extract key metadata such as the declared class name, implemented base type, and target context version.
  /// </summary>
  /// <param name="script">The C# source code to validate.</param>
  /// <returns>A <see cref="ValidationRecord"/> containing the parsed metadata.</returns>
  ValidationRecord BasicValidationBeforeCompiling(string script);

  #endregion

  /// <summary>
  /// Compiles and immediately executes a provided string of source, bypassing database persistence.
  /// </summary>
  /// <param name="sourceCode">The raw C# source code to execute.</param>
  /// <param name="context">The context given to the script method.</param>
  /// <param name="methodName">The entry-point method name to invoke.</param>
  /// <returns>A task returning the output result from the executed script method.</returns>
  Task<object> ExecuteUnfinishedScriptBySourceCode(string sourceCode, IContext context, string methodName, int? apiVersion = null, int? executionTime = null);


  #region Cache Management

  /// <summary>
  /// Retrieves a CompiledScript.
  /// </summary>
  /// <param name="scriptId">The unique identifier of the target script.</param>
  /// <returns>A task returning the <see cref="CompiledScript"/> containing the <see langword="byte"/>[].</returns>
  Task<CompiledScript> GetCompiledCache(Guid scriptId, int? currentApiVersion = null);

  /// <summary>
  /// Deletes all CompiledCaches <see langword="from"/> the Database. 
  /// </summary>
  /// <param name="scriptId">The unique identifier of the script whose caches should be cleared.</param>
  Task ClearScriptCache(Guid scriptId);

  /// <summary>
  /// Deletes a CompiledScript <see langword="from"/> the Database. 
  /// </summary>
  /// <param name="id">The unique identifier of the script.</param>
  /// <param name="apiVersion">The specific API version cache to delete.</param>
  Task DeleteScriptCache(Guid id, int apiVersion);

  /// <summary>
  /// Retrieves all CompiledScript objects currently stored in the database.
  /// </summary>
  /// <returns>A task returning a list of all <see cref="CompiledScript"/> entities.</returns>
  Task<List<CompiledScript>> GetAllCompiledScriptCaches();

  /// <summary>
  /// A maintenance operation that wipes all CompiledScript ojects <see langword="from"/> the Database.
  /// </summary>
  Task ClearAllCaches();

  /// <summary>
  /// Completely deletes all script and cache data from the database. Intended for deep resets or testing environments.
  /// </summary>
  Task DeleteAllData();

  #endregion

  #region Version Management

  /// <summary>
  /// Returns the API version that the current instance <see langword="is"/> running. 
  /// </summary>
  /// <returns>An integer representing the active API version.</returns>
  int GetRunningApiVersion();

  /// <summary>
  /// Returns the minimum API version of a script.
  /// </summary>
  /// <param name="scriptId">The unique identifier of the target script.</param>
  Task<int> GetScriptCompatibility(Guid scriptId);

  /// <summary>
  /// Verifies whether the specified script <see langword="is"/> compiled <see langword="for"/> the target API version.
  /// </summary>
  /// <param name="scriptId">The unique identifier of the target script.</param>
  /// <param name="targetApiVersion">The API version to test against. If null, checks against the current running version.</param>
  /// <returns>A task returning <c>true</c> if compatible; otherwise, <c>false</c>.</returns>
  Task<bool> CheckVersionCompatibility(Guid scriptId, int? targetApiVersion = null);

  /// <summary>
  /// No logic behind yet.
  /// </summary>
  /// <param name="instanceId">The unique identifier of the host instance.</param>
  /// <param name="emberVersion">The application version of the Ember host.</param>
  /// <param name="apiVersion">The supported API version running on this host.</param>
  void RegisterEmberInstance(Guid instanceId, string emberVersion, int apiVersion);

  #endregion

  #region Monitoring & Diagnostics

  /// <summary>
  /// No logic behind yet.
  /// </summary>
  /// <param name="scriptId">The unique identifier of the script.</param>
  void GetScriptExecutionHistory(Guid scriptId);

  /// <summary>
  /// No logic behind yet.
  /// </summary>
  void GetCompilationStatistics();

  /// <summary>
  /// No logic behind yet.
  /// </summary>
  /// <returns>A task representing the asynchronous health validation process.</returns>
  Task HealthCheck();

  /// <summary>
  /// No logic behind yet.
  /// </summary>
  /// <param name="scriptId">The unique identifier of the script to analyze.</param>
  /// <returns>A task returning a string detailing the script's core metadata.</returns>
  Task<string> GetScriptMetadata(Guid scriptId);

  /// <summary>
  /// Returns the GUID of a Script <see langword="by"/> taking its Name and Type.
  /// </summary>
  Task<Guid> GetScriptId<ScriptType>(string scriptName) where ScriptType : IScriptType;

  /// Returns the compiled scripts on each API version. 
  /// </summary>
  /// <returns>A task returning a dictionary mapping API versions to their respective list of compiled caches.</returns>
  Task<Dictionary<int, List<CompiledScript>>> GetCachesForEachApiVersion();

  /// <summary>
  /// Retrieves the session context for the user currently executing or managing scripts.
  /// </summary>
  /// <returns>The active <see cref="IUserSession"/> implementation.</returns>
  IUserSession GetUserSession();

  #endregion
}


