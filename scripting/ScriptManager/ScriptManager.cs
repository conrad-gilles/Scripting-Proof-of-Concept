using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.Extensions.Logging;

namespace Ember.Scripting;

internal class ScriptManagerFacade : IScriptManager, IScriptManagerExtended, ISccriptManagerDeleteAfter
{
    /// <inheritdoc cref="IScriptManager.CreateScript(string, string?, string, int)">

    private readonly ScriptRepository _db;
    private readonly ScriptCompiler _compiler;
    private readonly ScriptExecutor _executor;
    private readonly List<MetadataReference> _references;
    private readonly ILogger<ScriptManagerFacade> _logger;
    private readonly int _recentApiVersion;

    internal ScriptManagerFacade(ScriptRepository db, ScriptCompiler compiler, ScriptExecutor executor, List<MetadataReference> references, ILogger<ScriptManagerFacade> logger, int recentApiVersion)
    {
        _references = references;
        _db = db;
        _compiler = compiler;
        _executor = executor;
        _logger = logger;
        _recentApiVersion = recentApiVersion;
    }

    #region Script Lifecycle

    public async Task<CustomerScript> CreateScript(string sourceCode, string userName = "Default", int? apiVersion = null, DateTime? createdAt = null, bool checkForDuplicates = true)    //maybe minApiVersion is better?
    {
        _logger.LogDebug("Entered {MethodName} in {ClassName} apiVersion: {apiVersion}.", nameof(CreateScript), nameof(ScriptManagerFacade), apiVersion);

        return await _db.CreateAndInsertCustomerScript(sourceCode, createdBy: userName, oldApiV: apiVersion, createdAt: createdAt, checkForDuplicates: checkForDuplicates);
    }

    // Updates existing script source code
    public async Task UpdateScriptSC(Guid scriptId, string newSourceCode, bool allowFaultySave = false, string? userName = null, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(UpdateScriptSC), nameof(ScriptManagerFacade), scriptId);
        CustomerScript script = await GetScript(scriptId);
        await _db.UpdateScript(script, newSourceCode, allowFaultySave, userName: userName, apiVersion: apiVersion);
    }

    public async Task UpdateScriptNT(string name, ScriptTypes scriptType, string newSourceCode, string? userName = null, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with Name: {name}.", nameof(UpdateScriptNT), nameof(ScriptManagerFacade), name);

        Guid scriptId = await GetScriptId(name, scriptType);
        await UpdateScriptSC(scriptId, newSourceCode, userName: userName, apiVersion: apiVersion);
    }

    //todo unsafe check if it compiles first before updating, compiling first doesnt work because it would compile old version
    public async Task UpdateScriptAndCompile(Guid scriptId, string newSourceCode, string? userName = null, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {scriptId}.", nameof(UpdateScriptAndCompile), nameof(ScriptManagerFacade), scriptId);
        await _db.UpdateScriptAndRecompile(scriptId, newSourceCode, userName, apiVersion);
    }

    public async Task UpdateScriptAndCompileNT(string name, ScriptTypes scriptType, string newSourceCode, string? userName = null, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with Name: {name}.", nameof(UpdateScriptAndCompileNT), nameof(ScriptManagerFacade), name);
        Guid scriptId = await GetScriptId(name, scriptType);
        await UpdateScriptAndCompile(scriptId, newSourceCode, userName, apiVersion);
    }

    // Removes script and all associated compiled caches
    public async Task DeleteScript(Guid scriptId)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(DeleteScript), nameof(ScriptManagerFacade), scriptId);
        await _db.DeleteCustomerScript(scriptId);    //todo check if this also deletes the caches
    }

    public async Task DeleteScriptNT(string scriptName, ScriptTypes scriptType)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptName: {scriptName}.", nameof(DeleteScriptNT), nameof(ScriptManagerFacade), scriptName);
        Guid id = await GetScriptId(scriptName, scriptType);
        await DeleteScript(id);
    }
    // Retrieves script metadata and source code
    public async Task<CustomerScript> GetScript(Guid scriptId, bool includeCaches = false)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetScript), nameof(ScriptManagerFacade), scriptId);
        return await _db.GetCustomerScript(scriptId, includeCaches);
    }

    public async Task<CustomerScript> GetScriptNT(string name, ScriptTypes scriptType, bool includeCaches = false)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with Name: {name}.", nameof(GetScriptNT), nameof(ScriptManagerFacade), name);
        Guid scriptId = await GetScriptId(name, scriptType);
        return await GetScript(scriptId);
    }

    // Returns all scripts with optional filtering by type, API version, or creation date
    public async Task<List<CustomerScript>> ListScripts(CustomerScriptFilter? filters = null, bool includeCaches = false)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(ListScripts), nameof(ScriptManagerFacade));
        return await _db.GetAllCustomerScripts(includeCaches: includeCaches, filters: filters);
    }

    #endregion

    #region Compilation Operations

    // Compiles a specific script for a target API version, if target version not specified just takes the recent one
    public async Task CompileScript(Guid scriptId, int? targetApiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId} and targetApiVersion: {TargetApiVersion}.", nameof(CompileScript), nameof(ScriptManagerFacade), scriptId, targetApiVersion);

        CustomerScript script = await _db.GetCustomerScript(scriptId);
        await _db.CreateAndInsertCompiledCache(script, apiV: targetApiVersion);
    }

    // Compiles all compatible scripts for a new API version
    public async Task CompileAllScripts()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(CompileAllScripts), nameof(ScriptManagerFacade));
        await _db.CompileAllStoredScripts();
    }

    // public async Task SaveScriptWithoutCompiling(Guid id, string sourceCode)
    // {
    //     _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(SaveScriptWithoutCompiling), nameof(ScriptManagerFacade));
    //     await _db.SaveScriptWithoutCompiling(id, sourceCode);
    // }

    public async Task CreateScriptWithoutCompiling(Guid id, string sourceCode, string? userName = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(CreateScriptWithoutCompiling), nameof(ScriptManagerFacade));
        await _db.CreateScriptWithoutCompiling(id, sourceCode, userName);
    }

    // Recompiles script for all active API versions
    public async Task RecompileAllCaches(Guid scriptId)  //todo maybe get rid of the currentapi and create global var in dbhelper class
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(RecompileAllCaches), nameof(ScriptManagerFacade), scriptId);
        await _db.RecompileScript(scriptId, deleteAlso: true);  //todo fix this i need old version dlls or something like that which will need to be passed maybe as fodler path or file path
    }

    public async Task RecompileCache(Guid scriptId, int apiVersion)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(RecompileCache), nameof(ScriptManagerFacade), scriptId);
        await _db.RecompileCache(scriptId, apiVersion);
    }

    // Retrieves compilation error details
    public async Task<string> GetCompilationErrors(Guid scriptId, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetCompilationErrors), nameof(ScriptManagerFacade), scriptId);
        return await _db.GetCompilationErrors(scriptId, apiVersion);
    }

    public async Task<List<ScriptCompilationError>> GetCompilationErrors(string sourceCode, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with sourceCode: {sourceCode}.", nameof(GetCompilationErrors), nameof(ScriptManagerFacade), sourceCode);
        return await _db.GetCompilationErrors(sourceCode, apiVersion);
    }

    public async Task<bool> ThrowCompilationErrors(string script)
    {
        _compiler.RunCompilation(script);
        return true;
    }

    public ValidationRecord BasicValidationBeforeCompiling(string script)
    {
        return _compiler.BasicValidationBeforeCompiling(script);
    }

    public INamedTypeSymbol GetBaseType(string script)
    {
        return _compiler.GetBaseType(script).BaseType!;
    }

    #endregion

    #region Execution Operations

    // Generic execution 
    public async Task<object> ExecuteScriptById(Guid scriptId, GeneratorContextSF context, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(ExecuteScriptById), nameof(ScriptManagerFacade), scriptId);

        byte[]? compiledScript = null;
        try
        {
            var temp = await _db.GetCompiledScripCache(scriptId, apiVersion);
            compiledScript = temp.AssemblyBytes;
        }
        catch (Exception e)
        {
            _logger.LogError("Retrieval failed jit comp launched:" + e.ToString());
            await CompileScript(scriptId, GetRunningApiVersion());
            //try again, if fails again we catch error outside
            compiledScript = (await _db.GetCompiledScripCache(scriptId, apiVersion)).AssemblyBytes;
        }
        object result = await _executor.RunScriptExecution<object>(compiledScript!, context);  //returns either bool or action result todo maybe add checks if thats the case but normally should be
        return result;
    }

    public async Task<object> ExecuteScriptByNameAndType(string name, ScriptTypes scriptType, GeneratorContextSF context, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptName: {ScriptId}.", nameof(ExecuteScriptByNameAndType), nameof(ScriptManagerFacade), name);
        Guid scriptId = await _db.GetScriptId(name, scriptType);
        return await ExecuteScriptById(scriptId, context, apiVersion);
    }

    #endregion

    #region Cache Management

    // Retrieves compiled assembly bytes from cache
    public async Task<CompiledScripts> GetCompiledCache(Guid scriptId, int? currentApiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetCompiledCache), nameof(ScriptManagerFacade), scriptId);
        return await _db.GetCompiledScripCache(scriptId, currentApiVersion);
    }

    // Removes all compiled versions of a script
    public async Task ClearScriptCache(Guid scriptId)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(ClearScriptCache), nameof(ScriptManagerFacade), scriptId);
        await _db.ClearScriptCache(scriptId);
    }

    public async Task DeleteScriptCache(Guid id, int apiVersion)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with ApiVersion: {ScriptId}.", nameof(DeleteScriptCache), nameof(ScriptManagerFacade), apiVersion);
        await _db.DeleteScriptCache(id, apiVersion);
    }

    public async Task<List<CompiledScripts>> GetAllCompiledScriptCaches()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetAllCompiledScriptCaches), nameof(ScriptManagerFacade));
        return await _db.GetAllCompiledScriptCaches();
    }

    // Removes all compiled caches (maintenance operation)
    public async Task ClearAllCaches()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(ClearAllCaches), nameof(ScriptManagerFacade));
        await _db.DeleteAllCachedScripts();
    }

    // Background job to precompile all compatible scripts
    public async Task PrecompileForApiVersion()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(PrecompileForApiVersion), nameof(ScriptManagerFacade));
        int currentApiVersion = GetRunningApiVersion();
        await _db.AutomaticCompilationOnVersionUpdate(currentApiVersion);
    }

    public async Task DeleteAllData()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(DeleteAllData), nameof(ScriptManagerFacade));
        await _db.DeleteAllData();
    }
    #endregion

    #region Version Management

    // Returns list of currently active API versions from Ember instances
    public async Task<List<int>> GetActiveApiVersions() //todo implement
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetActiveApiVersions), nameof(ScriptManagerFacade));
        return await _db.GetActiveApiVersions();
    }

    public int GetRunningApiVersion() //todo implement
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetRunningApiVersion), nameof(ScriptManagerFacade));
        return _db.GetRecentApiVersion();
    }

    // Returns which API versions a script is compatible with
    public async Task<int> GetScriptCompatibility(Guid scriptId)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetScriptCompatibility), nameof(ScriptManagerFacade), scriptId);
        return (await _db.GetCustomerScript(scriptId)).MinApiVersion;
    }

    // Validates if script can run on target version
    public async Task<bool> CheckVersionCompatibility(Guid scriptId, int? targetApiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(CheckVersionCompatibility), nameof(ScriptManagerFacade), scriptId);
        try
        {
            await _db.GetCompiledScripCache(scriptId, targetApiVersion);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Registers a new Ember instance in the system
    public void RegisterEmberInstance(Guid instanceId, string emberVersion, int apiVersion)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with instanceId: {InstanceId}.", nameof(RegisterEmberInstance), nameof(ScriptManagerFacade), instanceId);
        // TODO
    }

    #endregion

    #region Duplicate Detection & Cleanup

    // Identifies duplicate scripts based on source code equivalence
    public async Task<DuplicateRecord> DetectDuplicates()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(DetectDuplicates), nameof(ScriptManagerFacade));

        var dupes = await _db.DetectDuplicates();
        return new DuplicateRecord
        {
            cacheGUIDs = dupes.cachesToDelete,
            scriptGUIDs = dupes.duplicateGuids
        };
    }

    // Removes duplicate scripts and orphaned caches
    public async Task RemoveDuplicates()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(RemoveDuplicates), nameof(ScriptManagerFacade));
        await _db.RemoveDuplicates();
    }

    // Removes caches without associated scripts
    public async Task CleanupOrphanedCaches()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(CleanupOrphanedCaches), nameof(ScriptManagerFacade));

        // int currentApiVersion = GetRunningApiVersion();
        // await _db.AutomaticCompilationOnVersionUpdate(currentApiVersion);    //this also does this maybe implement real funcion later

        //Todo make implementation
    }

    #endregion

    #region Monitoring & Diagnostics

    // Returns execution logs and metrics
    public void GetScriptExecutionHistory(Guid scriptId)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetScriptExecutionHistory), nameof(ScriptManagerFacade), scriptId);
        // TODO
    }

    // Returns compilation success/failure rates and performance metrics
    public void GetCompilationStatistics()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetCompilationStatistics), nameof(ScriptManagerFacade));
        // TODO
    }

    // Validates database connectivity, API version consistency, and system state
    public async Task HealthCheck()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(HealthCheck), nameof(ScriptManagerFacade));
        // TODO
        await _db.HealthCheck();
    }

    // Extracts className, baseTypeName, and version from script
    public async Task<string> GetScriptMetadata(Guid scriptId)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetScriptMetadata), nameof(ScriptManagerFacade), scriptId);

        CustomerScript script = await _db.GetCustomerScript(scriptId, includeCaches: true);
        string str = "Metadata for script: " + script.ToString();
        return str;
    }

    public async Task<Guid> GetScriptId(string scriptName, ScriptTypes scriptType)
    {
        return await _db.GetScriptId(scriptName, scriptType);
    }

    public async Task<Dictionary<int, List<CompiledScripts>>> GetCachesForEachApiVersion()
    {
        return await _db.GetCachesForEachApiVersion();
    }

    public string GetUserName()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetUserName), nameof(ScriptManagerFacade));
        return "Gilles";
    }

    #endregion
}


