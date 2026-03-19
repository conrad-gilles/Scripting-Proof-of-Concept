using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.Extensions.Logging;

namespace Ember.Scripting;

internal class ScriptManagerFacade : IScriptManager, IScriptManagerExtended, ISccriptManagerDeleteAfter
{
    /// <inheritdoc cref="IScriptManager.CreateScript(string, string?, string, int)">

    private readonly DbHelper _db;
    private readonly ScriptCompiler _compiler;
    private readonly ScriptExecutor _executor;
    private readonly List<MetadataReference> _references;
    private readonly ILogger<ScriptManagerFacade> _logger;
    private readonly int _recentApiVersion;

    internal ScriptManagerFacade(DbHelper db, ScriptCompiler compiler, ScriptExecutor executor, List<MetadataReference> references, ILogger<ScriptManagerFacade> logger, int recentApiVersion)
    {
        _references = references;
        _db = db;
        _compiler = compiler;
        _executor = executor;
        _logger = logger;
        _recentApiVersion = recentApiVersion;
    }

    #region Script Lifecycle

    public async Task<Guid> CreateScript(string sourceCode, string userName = "Default", int? apiVersion = null, DateTime? createdAt = null, bool checkForDuplicates = false)    //maybe minApiVersion is better?
    {
        _logger.LogDebug("Entered {MethodName} in {ClassName} apiVersion: {apiVersion}.", nameof(CreateScript), nameof(ScriptManagerFacade), apiVersion);

        int currentApiVersion = GetRunningApiVersion();
        Guid id = Guid.NewGuid();

        if (apiVersion == null)
        {
            await _db.CreateAndInsertCustomerScript(sourceCode, id, userName, createdAt: createdAt, checkForDuplicates: checkForDuplicates);
        }
        else
        {
            await _db.CreateAndInsertCustomerScript(sourceCode, id, userName, (int)apiVersion, createdAt: createdAt, checkForDuplicates: checkForDuplicates);
        }
        return id;

    }

    public async Task<ScriptNameType> CreateScriptUsingNameType(string sourceCode, string userName = "Default", int? apiVersion = null, DateTime? createdAt = null, bool checkForDuplicates = false)
    {
        _logger.LogDebug("Entered {MethodName} in {ClassName} apiVersion: {apiVersion}.", nameof(CreateScriptUsingNameType), nameof(ScriptManagerFacade), apiVersion);

        int currentApiVersion = GetRunningApiVersion();
        Guid id = Guid.NewGuid();
        CustomerScript? script = null;

        if (apiVersion == null)
        {
            script = await _db.CreateAndInsertCustomerScript(sourceCode, id, userName, createdAt: createdAt, checkForDuplicates: checkForDuplicates);
        }
        else
        {
            script = await _db.CreateAndInsertCustomerScript(sourceCode, id, userName, (int)apiVersion, createdAt: createdAt, checkForDuplicates: checkForDuplicates);
        }
        ScriptTypes sType;
        switch (script.ScriptType)
        {
            case "IGeneratorActionScript":
                sType = ScriptTypes.GeneratorActionScript;
                break;
            case "IGeneratorConditionScript":
                sType = ScriptTypes.GeneratorConditionScript;
                break;
            default:
                throw new Exception(message: "Could not assign baseTypeName");
        }
        // return (Name: script.ScriptName!, ScriptType: sType);
        return new ScriptNameType
        {
            Name = script.ScriptName!,
            Type = sType
        };
    }

    // Updates existing script source code
    public async Task UpdateScript(Guid scriptId, string newSourceCode, string? userName = null, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(UpdateScript), nameof(ScriptManagerFacade), scriptId);

        await _db.UpdateScript(scriptId, newSourceCode, userName, apiVersion);
    }

    public async Task UpdateScriptNT(string name, ScriptTypes scriptType, string newSourceCode, string? userName = null, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with Name: {name}.", nameof(UpdateScriptNT), nameof(ScriptManagerFacade), name);

        Guid scriptId = await GetScriptId(name, scriptType);
        await UpdateScript(scriptId, newSourceCode, userName, apiVersion);
    }

    // Removes script and all associated compiled caches
    public async Task DeleteScript(Guid scriptId)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(DeleteScript), nameof(ScriptManagerFacade), scriptId);

        await _db.DeleteCustomerScript(scriptId);    //todo check if this also deletes the caches
    }

    public async Task DeleteScriptNT(string scriptName, ScriptTypes scriptType)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptName: {scriptName}.", nameof(DeleteScript), nameof(ScriptManagerFacade), scriptName);
        Guid id = await GetScriptId(scriptName, scriptType);
        await DeleteScript(id);
    }
    // Retrieves script metadata and source code
    public async Task<CustomerScript> GetScript(Guid scriptId, bool includeCaches = false)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetScript), nameof(ScriptManagerFacade), scriptId);

        CustomerScript script = await _db.GetCustomerScript(scriptId, includeCaches);
        return script;
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

        List<CustomerScript> scripts;
        scripts = await _db.GetAllCustomerScripts(includeCaches: includeCaches, filters: filters);
        return scripts;
    }

    #endregion

    #region Compilation Operations

    // Compiles a specific script for a target API version, if target version not specified just takes the recent one
    public async Task CompileScript(Guid scriptId, int? targetApiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId} and targetApiVersion: {TargetApiVersion}.", nameof(CompileScript), nameof(ScriptManagerFacade), scriptId, targetApiVersion);

        if (targetApiVersion == null)
        {
            targetApiVersion = GetRunningApiVersion();
        }
        CustomerScript script = await _db.GetCustomerScript(scriptId);
        await _db.CreateAndInsertCompiledCache(script, apiV: targetApiVersion);
    }

    // Compiles all compatible scripts for a new API version
    public async Task CompileAllScripts()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(CompileAllScripts), nameof(ScriptManagerFacade));

        int currentApiVersion = GetRunningApiVersion();
        await _db.CompileAllStoredScripts(currentApiVersion);
    }

    public async Task SaveScriptWithoutCompiling(Guid id, string sourceCode)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(SaveScriptWithoutCompiling), nameof(ScriptManagerFacade));
        // Guid id = Guid.NewGuid();
        await _db.SaveScriptWithoutCompiling(id, sourceCode);
    }

    public async Task CreateScriptWithoutCompiling(Guid id, string sourceCode, string? userName = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(CreateScriptWithoutCompiling), nameof(ScriptManagerFacade));
        // Guid id = Guid.NewGuid();
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
        await _db.RecompileCache(scriptId, apiVersion);
    }

    // Retrieves compilation error details
    public async Task<string> GetCompilationErrors(Guid scriptId, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetCompilationErrors), nameof(ScriptManagerFacade), scriptId);
        try
        {
            ValidationRecord? metaData = null;
            CustomerScript script;
            string sourceCode;

            if (apiVersion == null)
            {
                script = await GetScript(scriptId);
                sourceCode = script.SourceCode!;
            }
            else
            {
                CompiledScripts cache = await _db.GetCompiledScripCache(scriptId, (int)apiVersion);
                sourceCode = cache.OldSourceCode!;
            }
            try
            {
                metaData = _compiler.BasicValidationBeforeCompiling(sourceCode);
            }
            catch (Exception e)
            {
                _logger.LogError("Validation in GetCompilationErrors failed but will still try to compile." + e.ToString());
            }

            _compiler.RunCompilation(sourceCode, metaData: metaData);

            return "Successful Compilation!";
        }
        catch (Exception e)
        {
            return "Failed to compile script:" + scriptId + " " + e.ToString();
            // throw new FacadeException(e.ToString(), e);
        }
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

    // Executes a Generator Action script with provided context, realisitcally not needed
    public async Task<ActionResultSF> ExecuteActionScript(Guid scriptId, GeneratorContextSF context, int? currentApiVersion = null)    //lowkey so many errors better to have one
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(ExecuteActionScript), nameof(ScriptManagerFacade), scriptId);
        if (currentApiVersion == null)
        {
            currentApiVersion = GetRunningApiVersion();
        }

        byte[]? compiledScript = null;
        try
        {
            var temp = await _db.GetCompiledScripCache(scriptId, (int)currentApiVersion);
            compiledScript = temp.AssemblyBytes;
        }
        catch (Exception e)
        {
            _logger.LogError("Retrieval failed jit comp launched:" + e.ToString());
            await CompileScript(scriptId, currentApiVersion);
            //try again, if fails again we catch error outside
            var temp = await _db.GetCompiledScripCache(scriptId, (int)currentApiVersion);
            compiledScript = temp.AssemblyBytes;
        }

        //possibly add a null check for compiledScript
        // ScriptExecutor executor = new ScriptExecutor();
        ActionResultSF result = (ActionResultSF)await _executor.RunScriptExecution<object>(compiledScript!, context);  //todo maybe better handling than casting although the error will be thrown in the class itself
        return result;
    }

    // Executes a Generator Condition script and returns boolean result, realisitcally not needed
    public async Task<bool> ExecuteConditionScript(Guid scriptId, GeneratorContextSF context, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(ExecuteConditionScript), nameof(ScriptManagerFacade), scriptId);
        if (apiVersion == null)
        {
            apiVersion = GetRunningApiVersion();
        }

        byte[]? compiledScript = null;
        try
        {
            var temp = await _db.GetCompiledScripCache(scriptId, (int)apiVersion);
            compiledScript = temp.AssemblyBytes;
        }
        catch (Exception e)
        {
            _logger.LogError("Retrieval failed jit comp launched:" + e.ToString());
            await CompileScript(scriptId, GetRunningApiVersion());
            //try again, if fails again we catch error outside
            var temp = await _db.GetCompiledScripCache(scriptId, (int)apiVersion);
            compiledScript = temp.AssemblyBytes;
        }

        //possibly add a null check for compiledScript
        // ScriptExecutor executor = new ScriptExecutor();
        bool result = (bool)await _executor.RunScriptExecution<object>(compiledScript!, context);  //todo maybe better handling than casting although the error will be thrown in the class itself
        return result;
    }

    // Generic execution that detects script type automatically
    public async Task<object> ExecuteScriptById(Guid scriptId, GeneratorContextSF context, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(ExecuteScriptById), nameof(ScriptManagerFacade), scriptId);
        if (apiVersion == null)
        {
            apiVersion = GetRunningApiVersion();
        }
        byte[]? compiledScript = null;
        try
        {
            var temp = await _db.GetCompiledScripCache(scriptId, (int)apiVersion);
            compiledScript = temp.AssemblyBytes;
        }
        catch (Exception e)
        {
            _logger.LogError("Retrieval failed jit comp launched:" + e.ToString());
            await CompileScript(scriptId, GetRunningApiVersion());
            //try again, if fails again we catch error outside
            var temp = await _db.GetCompiledScripCache(scriptId, (int)apiVersion);
            compiledScript = temp.AssemblyBytes;
        }

        //possibly add a null check for compiledScript
        // ScriptExecutor executor = new ScriptExecutor();
        object result = await _executor.RunScriptExecution<object>(compiledScript!, context);  //returns either bool or action result todo maybe add checks if thats the case but normally should be
        return result;
    }

    public async Task<object> ExecuteScriptByNameAndType(string name, ScriptTypes scriptType, GeneratorContextSF context, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptName: {ScriptId}.", nameof(ExecuteScriptByNameAndType), nameof(ScriptManagerFacade), name);
        Guid scriptId = await _db.GetScriptId(name, scriptType);
        if (apiVersion == null)
        {
            apiVersion = GetRunningApiVersion();
        }
        byte[]? compiledScript = null;
        try
        {
            var temp = await _db.GetCompiledScripCache(scriptId, (int)apiVersion);
            compiledScript = temp.AssemblyBytes;
        }
        catch (Exception e)
        {
            _logger.LogError("Retrieval failed jit comp launched:" + e.ToString());
            await CompileScript(scriptId, GetRunningApiVersion());
            //try again, if fails again we catch error outside
            var temp = await _db.GetCompiledScripCache(scriptId, (int)apiVersion);
            compiledScript = temp.AssemblyBytes;
        }

        //possibly add a null check for compiledScript
        // ScriptExecutor executor = new ScriptExecutor();
        object result = await _executor.RunScriptExecution<object>(compiledScript!, context);  //returns either bool or action result todo maybe add checks if thats the case but normally should be
        return result;
    }

    #endregion

    #region Cache Management

    // Retrieves compiled assembly bytes from cache
    // public async Task<byte[]> GetCompiledCache(Guid scriptId, int? currentApiVersion = null)
    public async Task<CompiledScripts> GetCompiledCache(Guid scriptId, int? currentApiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetCompiledCache), nameof(ScriptManagerFacade), scriptId);
        if (currentApiVersion == null)
        {
            currentApiVersion = GetRunningApiVersion();
        }
        var temp = await _db.GetCompiledScripCache(scriptId, (int)currentApiVersion);

        return temp;
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
        await _db.DeleteAllData();
    }
    #endregion

    #region Version Management

    // Returns list of currently active API versions from Ember instances
    public async Task<List<int>> GetActiveApiVersions() //todo implement
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetActiveApiVersions), nameof(ScriptManagerFacade));

        return await _db.GetActiveApiVersions(); //shit implementation not really functional in rl
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

        CustomerScript script = await _db.GetCustomerScript(scriptId);
        return script.MinApiVersion;
    }

    // Validates if script can run on target version
    public async Task<bool> CheckVersionCompatibility(Guid scriptId, int targetApiVersion)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(CheckVersionCompatibility), nameof(ScriptManagerFacade), scriptId);

        CustomerScript script = await _db.GetCustomerScript(scriptId);
        // int minV = script.MinApiVersion;
        int minV = targetApiVersion;
        var ls = await GetActiveApiVersions();
        if (ls.Contains(minV))
        {
            return true;
        }
        return false;
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
        // return (scriptGUIDs: dupes.scriptGUIDs, cacheGUIDs: dupes.cacheGUIDs);
    }

    // Removes duplicate scripts and orphaned caches
    public async Task RemoveDuplicates()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(RemoveDuplicates), nameof(ScriptManagerFacade));

        int currentApiVersion = GetRunningApiVersion();
        await _db.RemoveDuplicates();   //automatically get dupes from function in dbhelper dont have to pass therefore
    }

    // Removes caches without associated scripts
    public async Task CleanupOrphanedCaches()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(CleanupOrphanedCaches), nameof(ScriptManagerFacade));

        int currentApiVersion = GetRunningApiVersion();
        await _db.AutomaticCompilationOnVersionUpdate(currentApiVersion);    //this also does this maybe implement real funcion later
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

        // return UsefulMethods.GetUserName();
        return "Gilles";
    }

    #endregion


}


