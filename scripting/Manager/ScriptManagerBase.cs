using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;

namespace Ember.Scripting.Manager;

internal class ScriptManagerBase(
    ScriptRepository _db,
    ScriptCompiler _compiler,
    ScriptExecutor _executor,
    ILogger<ScriptManagerBase> _logger,
    IUserSession _userSession) : IScriptManagerBase, IScriptManagerBaseExtended
{

    #region Script Lifecycle

    public async Task<CustomerScript> CreateScript(string sourceCode, int? apiVersion = null, DateTime? createdAt = null)    //maybe minApiVersion is better?
    {
        _logger.LogDebug("Entered {MethodName} in {ClassName} apiVersion: {apiVersion}.", nameof(CreateScript), nameof(ScriptManagerBase), apiVersion);
        return await _db.CreateAndInsertCustomerScript(sourceCode, oldApiV: apiVersion, createdAt: createdAt);
    }

    // Updates existing script source code
    public async Task UpdateScript(Guid scriptId, string newSourceCode, bool allowFaultySave = false, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(UpdateScript), nameof(ScriptManagerBase), scriptId);
        CustomerScript script = await GetScript(scriptId);
        await _db.UpdateScript(script, newSourceCode, allowFaultySave, apiVersion: apiVersion);
    }

    public async Task UpdateScript<ScriptType>(string name, string newSourceCode, int? apiVersion = null) where ScriptType : IScriptType
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with Name: {name}.", nameof(UpdateScript), nameof(ScriptManagerBase), name);

        Guid scriptId = await GetScriptId<ScriptType>(name);
        await UpdateScript(scriptId, newSourceCode, apiVersion: apiVersion);
    }

    //todo unsafe check if it compiles first before updating, compiling first doesnt work because it would compile old version
    public async Task UpdateScriptAndCompile(Guid scriptId, string newSourceCode, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {scriptId}.", nameof(UpdateScriptAndCompile), nameof(ScriptManagerBase), scriptId);
        await _db.UpdateScriptAndRecompile(scriptId, newSourceCode, apiVersion);
    }

    public async Task UpdateScriptAndCompile<ScriptType>(string name, string newSourceCode, int? apiVersion = null) where ScriptType : IScriptType
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with Name: {name}.", nameof(UpdateScriptAndCompile), nameof(ScriptManagerBase), name);
        Guid scriptId = await GetScriptId<ScriptType>(name);
        await UpdateScriptAndCompile(scriptId, newSourceCode, apiVersion);
    }

    // Removes script and all associated compiled caches
    public async Task DeleteScript(Guid scriptId)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(DeleteScript), nameof(ScriptManagerBase), scriptId);
        await _db.DeleteCustomerScript(scriptId);    //todo check if this also deletes the caches
    }

    public async Task DeleteScript<ScriptType>(string scriptName) where ScriptType : IScriptType
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptName: {scriptName}.", nameof(DeleteScript), nameof(ScriptManagerBase), scriptName);
        Guid id = await GetScriptId<ScriptType>(scriptName);
        await DeleteScript(id);
    }
    // Retrieves script metadata and source code
    public async Task<CustomerScript> GetScript(Guid scriptId, bool includeCaches = false)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetScript), nameof(ScriptManagerBase), scriptId);
        return await _db.GetCustomerScript(scriptId, includeCaches);
    }

    public async Task<CustomerScript> GetScript<ScriptType>(string name, bool includeCaches = false) where ScriptType : IScriptType
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with Name: {name}.", nameof(GetScript), nameof(ScriptManagerBase), name);
        Guid scriptId = await GetScriptId<ScriptType>(name);
        return await GetScript(scriptId);
    }

    // Returns all scripts with optional filtering by type, API version, or creation date
    public async Task<List<CustomerScript>> ListScripts(CustomerScriptFilter? filters = null, bool includeCaches = false)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(ListScripts), nameof(ScriptManagerBase));
        return await _db.GetAllCustomerScripts(includeCaches: includeCaches, filters: filters);
    }

    #endregion

    #region Compilation Operations

    // Compiles a specific script for a target API version, if target version not specified just takes the recent one
    public async Task CompileScript(Guid scriptId, int? targetApiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId} and targetApiVersion: {TargetApiVersion}.", nameof(CompileScript), nameof(ScriptManagerBase), scriptId, targetApiVersion);

        CustomerScript script = await _db.GetCustomerScript(scriptId);
        await _db.CreateAndInsertCompiledScript(script, apiV: targetApiVersion);
    }

    // Compiles all compatible scripts for a new API version
    public async Task CompileAllScripts()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(CompileAllScripts), nameof(ScriptManagerBase));
        await _db.CompileAllStoredScripts();
    }

    // public async Task SaveScriptWithoutCompiling(Guid id, string sourceCode)
    // {
    //     _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(SaveScriptWithoutCompiling), nameof(ScriptManagerFacade));
    //     await _db.SaveScriptWithoutCompiling(id, sourceCode);
    // }

    public async Task CreateScriptWithoutCompiling(Guid id, string sourceCode)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(CreateScriptWithoutCompiling), nameof(ScriptManagerBase));
        await _db.CreateScriptWithoutCompiling(id, sourceCode);
    }

    // Recompiles script for all active API versions
    public async Task RecompileAllCaches(Guid scriptId)  //todo maybe get rid of the currentapi and create global var in dbhelper class
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(RecompileAllCaches), nameof(ScriptManagerBase), scriptId);
        await _db.RecompileScript(scriptId, deleteAlso: true);  //todo fix this i need old version dlls or something like that which will need to be passed maybe as fodler path or file path
    }

    public async Task RecompileCache(Guid scriptId, int apiVersion)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(RecompileCache), nameof(ScriptManagerBase), scriptId);
        await _db.RecompileCache(scriptId, apiVersion);
    }

    // Retrieves compilation error details
    public async Task<string> GetCompilationErrors(Guid scriptId)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetCompilationErrors), nameof(ScriptManagerBase), scriptId);
        return await _db.GetCompilationErrors(scriptId);
    }

    public async Task<EmitResult?> GetCompilationErrors(string sourceCode)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with sourceCode: {sourceCode}.", nameof(GetCompilationErrors), nameof(ScriptManagerBase), sourceCode);
        return await _db.GetCompilationErrors(sourceCode);
    }

    public async Task<bool> TryCompile(string script)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}", nameof(TryCompile), nameof(ScriptManagerBase));
        _compiler.RunCompilation(script);
        return true;
    }

    public ValidationRecord BasicValidationBeforeCompiling(string script)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetCompilationErrors), nameof(ScriptManagerBase));
        return _compiler.BasicValidationBeforeCompiling(script);
    }

    #endregion

    #region Execution Operations

    // Generic execution 
    public async Task<object> ExecuteScript(Guid scriptId, IContext context, string methodName, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(ExecuteScript), nameof(ScriptManagerBase), scriptId);

        byte[]? compiledScript = null;
        int? executionTime = null;
        try
        {
            CompiledScript temp = await _db.GetCompiledScripCache(scriptId, apiVersion);
            executionTime = temp.CustomerScript!.ExecutionTimeInMS;
            compiledScript = temp.AssemblyBytes;
        }
        catch (InvalidOperationException e)
        {
            _logger.LogError("Retrieval failed jit comp launched:" + e.ToString());
            await CompileScript(scriptId, GetRunningApiVersion());
            //try again, if fails again we catch error outside
            CompiledScript script = await _db.GetCompiledScripCache(scriptId, apiVersion);
            compiledScript = script.AssemblyBytes;
            executionTime = script.CustomerScript!.ExecutionTimeInMS;
            // Console.WriteLine("execution time was in 199 set to: " + executionTime);
        }
        object result = await _executor.RunScriptExecution(compiledScript!, (IContext)context, executionTime, methodName);  //returns either bool or action result todo maybe add checks if thats the case but normally should be
        return result;
    }

    public async Task<object> ExecuteScript<ScriptType>(string name, IContext context, string methodName, int? apiVersion = null) where ScriptType : IScriptType
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptName: {ScriptId}.", nameof(ExecuteScript), nameof(ScriptManagerBase), name);
        Guid scriptId = await GetScriptId<ScriptType>(name);
        return await ExecuteScript(scriptId, context, methodName, apiVersion);
    }
    public async Task<object> ExecuteUnfinishedScriptBySourceCode(string sourceCode, IContext context, string methodName, int? apiVersion = null, int? executionTime = null)
    {
        ValidationRecord vali = _compiler.BasicValidationBeforeCompiling(sourceCode);
        if (executionTime == null)
        {
            executionTime = vali.ExecutionTime;
            // Console.WriteLine("execution time was in 217 set to: " + executionTime);
        }
        byte[] comp = _compiler.RunCompilation(sourceCode);
        return await _executor.RunScriptExecution(compiledScript: comp, genContext: (IContext)context, executionTime: executionTime, methodName: methodName);
        // return null //todo
    }
    #endregion

    #region Cache Management

    // Retrieves compiled assembly bytes from cache
    public async Task<CompiledScript> GetCompiledCache(Guid scriptId, int? currentApiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetCompiledCache), nameof(ScriptManagerBase), scriptId);
        return await _db.GetCompiledScripCache(scriptId, currentApiVersion);
    }

    // Removes all compiled versions of a script
    public async Task ClearScriptCache(Guid scriptId)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(ClearScriptCache), nameof(ScriptManagerBase), scriptId);
        await _db.ClearScriptCache(scriptId);
    }

    public async Task DeleteScriptCache(Guid id, int apiVersion)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with ApiVersion: {ScriptId}.", nameof(DeleteScriptCache), nameof(ScriptManagerBase), apiVersion);
        await _db.DeleteScriptCache(id, apiVersion);
    }

    public async Task<List<CompiledScript>> GetAllCompiledScriptCaches()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetAllCompiledScriptCaches), nameof(ScriptManagerBase));
        return await _db.GetAllCompiledScriptCaches();
    }

    // Removes all compiled caches (maintenance operation)
    public async Task ClearAllCaches()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(ClearAllCaches), nameof(ScriptManagerBase));
        await _db.DeleteAllCachedScripts();
    }

    public async Task DeleteAllData()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(DeleteAllData), nameof(ScriptManagerBase));
        await _db.DeleteAllData();
    }
    #endregion

    #region Version Management

    public int GetRunningApiVersion() //todo implement
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetRunningApiVersion), nameof(ScriptManagerBase));
        return _db.GetRecentApiVersion();
    }

    // Returns which API versions a script is compatible with
    public async Task<int> GetScriptCompatibility(Guid scriptId)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetScriptCompatibility), nameof(ScriptManagerBase), scriptId);
        return (await _db.GetCustomerScript(scriptId)).ScriptApiVersion;
    }

    // Validates if script can run on target version
    public async Task<bool> CheckVersionCompatibility(Guid scriptId, int? targetApiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(CheckVersionCompatibility), nameof(ScriptManagerBase), scriptId);
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
        _logger.LogTrace("Entered {MethodName} in {ClassName} with instanceId: {InstanceId}.", nameof(RegisterEmberInstance), nameof(ScriptManagerBase), instanceId);
        // TODO
    }

    #endregion

    #region Monitoring & Diagnostics

    // Returns execution logs and metrics
    public void GetScriptExecutionHistory(Guid scriptId)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetScriptExecutionHistory), nameof(ScriptManagerBase), scriptId);
        // TODO
    }

    // Returns compilation success/failure rates and performance metrics
    public void GetCompilationStatistics()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetCompilationStatistics), nameof(ScriptManagerBase));
        // TODO
    }

    // Validates database connectivity, API version consistency, and system state
    public async Task HealthCheck()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(HealthCheck), nameof(ScriptManagerBase));
        // TODO
        await _db.HealthCheck();
    }

    // Extracts className, baseTypeName, and version from script
    public async Task<string> GetScriptMetadata(Guid scriptId)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetScriptMetadata), nameof(ScriptManagerBase), scriptId);

        CustomerScript script = await _db.GetCustomerScript(scriptId, includeCaches: true);
        string str = "Metadata for script: " + script.ToString();
        return str;
    }

    public async Task<Guid> GetScriptId<ScriptType>(string scriptName) where ScriptType : IScriptType
    {
        return await _db.GetScriptId<ScriptType>(scriptName);
    }

    public async Task<Dictionary<int, List<CompiledScript>>> GetCachesForEachApiVersion()
    {
        return await _db.GetCachesForEachApiVersion();
    }

    public IUserSession GetUserSession()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetUserSession), nameof(ScriptManagerBase));
        return _userSession;
    }

    #endregion
}


