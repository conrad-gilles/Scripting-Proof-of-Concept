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
    private readonly IUserSession _userSession;

    internal ScriptManagerFacade(ScriptRepository db, ScriptCompiler compiler, ScriptExecutor executor, List<MetadataReference> references,
    ILogger<ScriptManagerFacade> logger, int recentApiVersion, IUserSession userSession)
    {
        _references = references;
        _db = db;
        _compiler = compiler;
        _executor = executor;
        _logger = logger;
        _recentApiVersion = recentApiVersion;
        _userSession = userSession;
    }

    #region Script Lifecycle

    public async Task<CustomerScript> CreateScript(string sourceCode, int? apiVersion = null, DateTime? createdAt = null)    //maybe minApiVersion is better?
    {
        _logger.LogDebug("Entered {MethodName} in {ClassName} apiVersion: {apiVersion}.", nameof(CreateScript), nameof(ScriptManagerFacade), apiVersion);
        return await _db.CreateAndInsertCustomerScript(sourceCode, oldApiV: apiVersion, createdAt: createdAt);
    }

    // Updates existing script source code
    public async Task UpdateScriptSC(Guid scriptId, string newSourceCode, bool allowFaultySave = false, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(UpdateScriptSC), nameof(ScriptManagerFacade), scriptId);
        CustomerScript script = await GetScript(scriptId);
        await _db.UpdateScript(script, newSourceCode, allowFaultySave, apiVersion: apiVersion);
    }

    public async Task UpdateScriptNT<ScriptType>(string name, string newSourceCode, int? apiVersion = null) where ScriptType : IScript
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with Name: {name}.", nameof(UpdateScriptNT), nameof(ScriptManagerFacade), name);

        Guid scriptId = await GetScriptId<ScriptType>(name);
        await UpdateScriptSC(scriptId, newSourceCode, apiVersion: apiVersion);
    }

    //todo unsafe check if it compiles first before updating, compiling first doesnt work because it would compile old version
    public async Task UpdateScriptAndCompile(Guid scriptId, string newSourceCode, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {scriptId}.", nameof(UpdateScriptAndCompile), nameof(ScriptManagerFacade), scriptId);
        await _db.UpdateScriptAndRecompile(scriptId, newSourceCode, apiVersion);
    }

    public async Task UpdateScriptAndCompileNT<ScriptType>(string name, string newSourceCode, int? apiVersion = null) where ScriptType : IScript
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with Name: {name}.", nameof(UpdateScriptAndCompileNT), nameof(ScriptManagerFacade), name);
        Guid scriptId = await GetScriptId<ScriptType>(name);
        await UpdateScriptAndCompile(scriptId, newSourceCode, apiVersion);
    }

    // Removes script and all associated compiled caches
    public async Task DeleteScript(Guid scriptId)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(DeleteScript), nameof(ScriptManagerFacade), scriptId);
        await _db.DeleteCustomerScript(scriptId);    //todo check if this also deletes the caches
    }

    public async Task DeleteScriptNT<ScriptType>(string scriptName) where ScriptType : IScript
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptName: {scriptName}.", nameof(DeleteScriptNT), nameof(ScriptManagerFacade), scriptName);
        Guid id = await GetScriptId<ScriptType>(scriptName);
        await DeleteScript(id);
    }
    // Retrieves script metadata and source code
    public async Task<CustomerScript> GetScript(Guid scriptId, bool includeCaches = false)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetScript), nameof(ScriptManagerFacade), scriptId);
        return await _db.GetCustomerScript(scriptId, includeCaches);
    }

    public async Task<CustomerScript> GetScriptNT<ScriptType>(string name, bool includeCaches = false) where ScriptType : IScript
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with Name: {name}.", nameof(GetScriptNT), nameof(ScriptManagerFacade), name);
        Guid scriptId = await GetScriptId<ScriptType>(name);
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
        await _db.CreateAndInsertCompiledScript(script, apiV: targetApiVersion);
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

    public async Task CreateScriptWithoutCompiling(Guid id, string sourceCode)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(CreateScriptWithoutCompiling), nameof(ScriptManagerFacade));
        await _db.CreateScriptWithoutCompiling(id, sourceCode);
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

    #endregion

    #region Execution Operations

    // Generic execution 
    public async Task<object> ExecuteScriptById(Guid scriptId, GeneratorContextSF context, int? apiVersion = null, string? methodName = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(ExecuteScriptById), nameof(ScriptManagerFacade), scriptId);

        byte[]? compiledScript = null;
        int? executionTime = null;
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
            CompiledScripts script = await _db.GetCompiledScripCache(scriptId, apiVersion);
            compiledScript = script.AssemblyBytes;
            executionTime = script.CustomerScript!.ExecutionTimeInMS;
        }
        object result = await _executor.RunScriptExecution<object>(compiledScript!, context, executionTime, methodName);  //returns either bool or action result todo maybe add checks if thats the case but normally should be
        return result;
    }

    public async Task<object> ExecuteScriptByNameAndType<ScriptType>(string name, GeneratorContextSF context, int? apiVersion = null, string? methodName = null) where ScriptType : IScript
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptName: {ScriptId}.", nameof(ExecuteScriptByNameAndType), nameof(ScriptManagerFacade), name);
        Guid scriptId = await GetScriptId<ScriptType>(name);
        return await ExecuteScriptById(scriptId, context, apiVersion, methodName);
    }
    public async Task<object> ExecuteUnfinishedScriptBySourceCode(string sourceCode, GeneratorContextSF context, int? apiVersion = null, string? methodName = null)
    {
        _compiler.BasicValidationBeforeCompiling(sourceCode);
        byte[] comp = _compiler.RunCompilation(sourceCode);
        return await _executor.RunScriptExecution<object>(compiledScript: comp, genContext: context, executionTime: null, methodName: methodName);
        // return null //todo
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

    public async Task DeleteAllData()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(DeleteAllData), nameof(ScriptManagerFacade));
        await _db.DeleteAllData();
    }
    #endregion

    #region Version Management

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

    public async Task<Guid> GetScriptId<ScriptType>(string scriptName) where ScriptType : IScript
    {
        return await _db.GetScriptId<ScriptType>(scriptName);
    }

    public async Task<Dictionary<int, List<CompiledScripts>>> GetCachesForEachApiVersion()
    {
        return await _db.GetCachesForEachApiVersion();
    }

    public IUserSession GetUserName()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(_userSession.UserName), nameof(ScriptManagerFacade));
        return _userSession;
    }

    #endregion
}


