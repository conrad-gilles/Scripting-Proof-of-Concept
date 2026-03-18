using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.Extensions.Logging;

namespace Ember.Scripting;

internal class ScriptManagerFacade : IScriptManager, IScriptManagerExtended, ISccriptManagerDeleteAfter
{
    /// <inheritdoc cref="IScriptManager.CreateScript(string, string?, string, int)">

    private readonly DbHelper Db;
    private readonly ScriptCompiler Compiler;
    private readonly ScriptExecutor Executor;
    private readonly List<MetadataReference> References;
    private readonly ILogger<ScriptManagerFacade> Logger;
    private readonly int RecentApiVersion;

    internal ScriptManagerFacade(DbHelper db, ScriptCompiler compiler, ScriptExecutor executor, List<MetadataReference> references, ILogger<ScriptManagerFacade> logger, int recentApiVersion)
    {
        References = references;
        Db = db;
        Compiler = compiler;
        Executor = executor;
        Logger = logger;
        RecentApiVersion = recentApiVersion;
    }

    #region Script Lifecycle

    public async Task<Guid> CreateScript(string sourceCode, string userName = "Default", int? apiVersion = null, DateTime? createdAt = null, bool checkForDuplicates = false)    //maybe minApiVersion is better?
    {
        Logger.LogDebug("Entered {MethodName} in {ClassName} apiVersion: {apiVersion}.", nameof(CreateScript), nameof(ScriptManagerFacade), apiVersion);

        int currentApiVersion = GetRunningApiVersion();
        Guid id = Guid.NewGuid();

        if (apiVersion == null)
        {
            await Db.CreateAndInsertCustomerScript(sourceCode, id, userName, createdAt: createdAt, checkForDuplicates: checkForDuplicates);
        }
        else
        {
            await Db.CreateAndInsertCustomerScript(sourceCode, id, userName, (int)apiVersion, createdAt: createdAt, checkForDuplicates: checkForDuplicates);
        }
        return id;

    }

    public async Task<ScriptNameType> CreateScriptUsingNameType(string sourceCode, string userName = "Default", int? apiVersion = null, DateTime? createdAt = null, bool checkForDuplicates = false)
    {
        Logger.LogDebug("Entered {MethodName} in {ClassName} apiVersion: {apiVersion}.", nameof(CreateScriptUsingNameType), nameof(ScriptManagerFacade), apiVersion);

        int currentApiVersion = GetRunningApiVersion();
        Guid id = Guid.NewGuid();
        CustomerScript? script = null;

        if (apiVersion == null)
        {
            script = await Db.CreateAndInsertCustomerScript(sourceCode, id, userName, createdAt: createdAt, checkForDuplicates: checkForDuplicates);
        }
        else
        {
            script = await Db.CreateAndInsertCustomerScript(sourceCode, id, userName, (int)apiVersion, createdAt: createdAt, checkForDuplicates: checkForDuplicates);
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

    // Updates existing script source code and recompiles for all compatible API versions
    public async Task UpdateScript(Guid scriptId, string newSourceCode, string? userName = null, int? apiVersion = null)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(UpdateScript), nameof(ScriptManagerFacade), scriptId);

        await Db.UpdateScript(scriptId, newSourceCode, userName, apiVersion);
    }

    public async Task UpdateScriptNT(string name, ScriptTypes scriptType, string newSourceCode, string? userName = null, int? apiVersion = null)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with Name: {name}.", nameof(UpdateScriptNT), nameof(ScriptManagerFacade), name);

        Guid scriptId = await GetScriptId(name, scriptType);
        await UpdateScript(scriptId, newSourceCode, userName, apiVersion);
    }

    // Removes script and all associated compiled caches
    public async Task DeleteScript(Guid scriptId)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(DeleteScript), nameof(ScriptManagerFacade), scriptId);

        await Db.DeleteCustomerScript(scriptId);    //todo check if this also deletes the caches
    }

    public async Task DeleteScriptNT(string scriptName, ScriptTypes scriptType)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptName: {scriptName}.", nameof(DeleteScript), nameof(ScriptManagerFacade), scriptName);
        Guid id = await GetScriptId(scriptName, scriptType);
        await DeleteScript(id);
    }
    // Retrieves script metadata and source code
    public async Task<CustomerScript> GetScript(Guid scriptId, bool includeCaches = false)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetScript), nameof(ScriptManagerFacade), scriptId);

        CustomerScript script = await Db.GetCustomerScript(scriptId, includeCaches);
        return script;
    }

    public async Task<CustomerScript> GetScriptNT(string name, ScriptTypes scriptType, bool includeCaches = false)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with Name: {name}.", nameof(GetScriptNT), nameof(ScriptManagerFacade), name);

        Guid scriptId = await GetScriptId(name, scriptType);
        return await GetScript(scriptId);
    }

    // Returns all scripts with optional filtering by type, API version, or creation date
    public async Task<List<CustomerScript>> ListScripts(CustomerScriptFilter? filters = null, bool includeCaches = false)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(ListScripts), nameof(ScriptManagerFacade));

        List<CustomerScript> scripts;
        scripts = await Db.GetAllCustomerScripts(includeCaches: includeCaches, filters: filters);
        return scripts;
    }

    #endregion

    #region Compilation Operations

    // Compiles a specific script for a target API version, if target version not specified just takes the recent one
    public async Task CompileScript(Guid scriptId, int? targetApiVersion = null)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId} and targetApiVersion: {TargetApiVersion}.", nameof(CompileScript), nameof(ScriptManagerFacade), scriptId, targetApiVersion);

        if (targetApiVersion == null)
        {
            targetApiVersion = GetRunningApiVersion();
        }
        CustomerScript script = await Db.GetCustomerScript(scriptId);
        await Db.CreateAndInsertCompiledCache(script, apiV: targetApiVersion);
    }

    // Compiles all compatible scripts for a new API version
    public async Task CompileAllScripts()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(CompileAllScripts), nameof(ScriptManagerFacade));

        int currentApiVersion = GetRunningApiVersion();
        await Db.CompileAllStoredScripts(currentApiVersion);
    }

    public async Task SaveScriptWithoutCompiling(Guid id, string sourceCode)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(SaveScriptWithoutCompiling), nameof(ScriptManagerFacade));
        // Guid id = Guid.NewGuid();
        await Db.SaveScriptWithoutCompiling(id, sourceCode);
    }

    // Recompiles script for all active API versions
    public async Task RecompileAllCaches(Guid scriptId)  //todo maybe get rid of the currentapi and create global var in dbhelper class
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(RecompileAllCaches), nameof(ScriptManagerFacade), scriptId);

        await Db.RecompileScript(scriptId, deleteAlso: true);  //todo fix this i need old version dlls or something like that which will need to be passed maybe as fodler path or file path
    }

    public async Task RecompileCache(Guid scriptId, int apiVersion)
    {
        await Db.RecompileCache(scriptId, apiVersion);
    }

    /// Performs syntax and interface validation without saving
    public string ValidateScript(string sourceCode)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(ValidateScript), nameof(ScriptManagerFacade));
        try
        {
            var validation = Compiler.BasicValidationBeforeCompiling(sourceCode);
            string className = validation.ClassName;    //figure out if i should do something with this
            string baseTypeName = validation.BaseTypeName;
            int versionInt = validation.Version;

            return "Success: ClassName: " + className + ", BaseTypeName: " + baseTypeName + ", VersionInt: " + versionInt;
        }
        catch (Exception e)
        {
            return "Error: " + e.ToString();
            // throw new FacadeException(e.ToString(), e);
        }

    }

    // Retrieves compilation error details
    public async Task<string> GetCompilationErrors(Guid scriptId, int? apiVersion = null)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetCompilationErrors), nameof(ScriptManagerFacade), scriptId);
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
                ScriptCompiledCache cache = await Db.GetCompiledScripCache(scriptId, (int)apiVersion);
                sourceCode = cache.OldSourceCode!;
            }
            try
            {
                metaData = Compiler.BasicValidationBeforeCompiling(sourceCode);
            }
            catch (Exception e)
            {
                Logger.LogError("Validation in GetCompilationErrors failed but will still try to compile." + e.ToString());
            }

            Compiler.RunCompilation(sourceCode, metaData: metaData);

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
        return Compiler.BasicValidationBeforeCompiling(script);
    }

    public INamedTypeSymbol GetBaseType(string script)
    {
        return Compiler.GetBaseType(script);
    }

    #endregion

    #region Execution Operations

    // Executes a Generator Action script with provided context, realisitcally not needed
    public async Task<ActionResultSF> ExecuteActionScript(Guid scriptId, GeneratorContextSF context, int? currentApiVersion = null)    //lowkey so many errors better to have one
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(ExecuteActionScript), nameof(ScriptManagerFacade), scriptId);
        if (currentApiVersion == null)
        {
            currentApiVersion = GetRunningApiVersion();
        }

        byte[]? compiledScript = null;
        try
        {
            var temp = await Db.GetCompiledScripCache(scriptId, (int)currentApiVersion);
            compiledScript = temp.AssemblyBytes;
        }
        catch (Exception e)
        {
            Logger.LogError("Retrieval failed jit comp launched:" + e.ToString());
            await CompileScript(scriptId, currentApiVersion);
            //try again, if fails again we catch error outside
            var temp = await Db.GetCompiledScripCache(scriptId, (int)currentApiVersion);
            compiledScript = temp.AssemblyBytes;
        }

        //possibly add a null check for compiledScript
        // ScriptExecutor executor = new ScriptExecutor();
        ActionResultSF result = (ActionResultSF)Executor.RunScriptExecution<object>(compiledScript!, context);  //todo maybe better handling than casting although the error will be thrown in the class itself
        return result;
    }

    // Executes a Generator Condition script and returns boolean result, realisitcally not needed
    public async Task<bool> ExecuteConditionScript(Guid scriptId, GeneratorContextSF context, int? ApiVersion = null)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(ExecuteConditionScript), nameof(ScriptManagerFacade), scriptId);
        if (ApiVersion == null)
        {
            ApiVersion = GetRunningApiVersion();
        }

        byte[]? compiledScript = null;
        try
        {
            var temp = await Db.GetCompiledScripCache(scriptId, (int)ApiVersion);
            compiledScript = temp.AssemblyBytes;
        }
        catch (Exception e)
        {
            Logger.LogError("Retrieval failed jit comp launched:" + e.ToString());
            await CompileScript(scriptId, GetRunningApiVersion());
            //try again, if fails again we catch error outside
            var temp = await Db.GetCompiledScripCache(scriptId, (int)ApiVersion);
            compiledScript = temp.AssemblyBytes;
        }

        //possibly add a null check for compiledScript
        // ScriptExecutor executor = new ScriptExecutor();
        bool result = (bool)Executor.RunScriptExecution<object>(compiledScript!, context);  //todo maybe better handling than casting although the error will be thrown in the class itself
        return result;
    }

    // Generic execution that detects script type automatically
    public async Task<object> ExecuteScriptById(Guid scriptId, GeneratorContextSF context, int? apiVersion = null)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(ExecuteScriptById), nameof(ScriptManagerFacade), scriptId);
        if (apiVersion == null)
        {
            apiVersion = GetRunningApiVersion();
        }
        byte[]? compiledScript = null;
        try
        {
            var temp = await Db.GetCompiledScripCache(scriptId, (int)apiVersion);
            compiledScript = temp.AssemblyBytes;
        }
        catch (Exception e)
        {
            Logger.LogError("Retrieval failed jit comp launched:" + e.ToString());
            await CompileScript(scriptId, GetRunningApiVersion());
            //try again, if fails again we catch error outside
            var temp = await Db.GetCompiledScripCache(scriptId, (int)apiVersion);
            compiledScript = temp.AssemblyBytes;
        }

        //possibly add a null check for compiledScript
        // ScriptExecutor executor = new ScriptExecutor();
        object result = Executor.RunScriptExecution<object>(compiledScript!, context);  //returns either bool or action result todo maybe add checks if thats the case but normally should be
        return result;
    }

    public async Task<object> ExecuteScriptByNameAndType(string Name, ScriptTypes scriptType, GeneratorContextSF context, int? apiVersion = null)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptName: {ScriptId}.", nameof(ExecuteScriptByNameAndType), nameof(ScriptManagerFacade), Name);
        Guid scriptId = await Db.GetScriptId(Name, scriptType);
        if (apiVersion == null)
        {
            apiVersion = GetRunningApiVersion();
        }
        byte[]? compiledScript = null;
        try
        {
            var temp = await Db.GetCompiledScripCache(scriptId, (int)apiVersion);
            compiledScript = temp.AssemblyBytes;
        }
        catch (Exception e)
        {
            Logger.LogError("Retrieval failed jit comp launched:" + e.ToString());
            await CompileScript(scriptId, GetRunningApiVersion());
            //try again, if fails again we catch error outside
            var temp = await Db.GetCompiledScripCache(scriptId, (int)apiVersion);
            compiledScript = temp.AssemblyBytes;
        }

        //possibly add a null check for compiledScript
        // ScriptExecutor executor = new ScriptExecutor();
        object result = Executor.RunScriptExecution<object>(compiledScript!, context);  //returns either bool or action result todo maybe add checks if thats the case but normally should be
        return result;
    }

    #endregion

    #region Cache Management

    // Retrieves compiled assembly bytes from cache
    // public async Task<byte[]> GetCompiledCache(Guid scriptId, int? currentApiVersion = null)
    public async Task<ScriptCompiledCache> GetCompiledCache(Guid scriptId, int? currentApiVersion = null)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetCompiledCache), nameof(ScriptManagerFacade), scriptId);
        if (currentApiVersion == null)
        {
            currentApiVersion = GetRunningApiVersion();
        }
        var temp = await Db.GetCompiledScripCache(scriptId, (int)currentApiVersion);

        return temp;
    }

    // Removes all compiled versions of a script
    public async Task ClearScriptCache(Guid scriptId)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(ClearScriptCache), nameof(ScriptManagerFacade), scriptId);
        await Db.ClearScriptCache(scriptId);
    }

    public async Task DeleteScriptCache(Guid id, int ApiVersion)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with ApiVersion: {ScriptId}.", nameof(DeleteScriptCache), nameof(ScriptManagerFacade), ApiVersion);
        await Db.DeleteScriptCache(id, ApiVersion);
    }

    public async Task<List<ScriptCompiledCache>> GetAllCompiledScriptCaches()
    {
        return await Db.GetAllCompiledScriptCaches();
    }

    // Removes all compiled caches (maintenance operation)
    public async Task ClearAllCaches()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(ClearAllCaches), nameof(ScriptManagerFacade));

        await Db.DeleteAllCachedScripts();
    }

    // Background job to precompile all compatible scripts
    public async Task PrecompileForApiVersion()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(PrecompileForApiVersion), nameof(ScriptManagerFacade));

        int currentApiVersion = GetRunningApiVersion();
        await Db.AutomaticCompilationOnVersionUpdate(currentApiVersion);
    }

    public async Task DeleteAllData()
    {
        await Db.DeleteAllData();
    }
    #endregion

    #region Version Management

    // Returns list of currently active API versions from Ember instances
    public async Task<List<int>> GetActiveApiVersions() //todo implement
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetActiveApiVersions), nameof(ScriptManagerFacade));

        return await Db.GetActiveApiVersions(); //shit implementation not really functional in rl
    }

    public int GetRunningApiVersion() //todo implement
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetRunningApiVersion), nameof(ScriptManagerFacade));

        return Db.GetRecentApiVersion();
    }

    // Returns which API versions a script is compatible with
    public async Task<int> GetScriptCompatibility(Guid scriptId)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetScriptCompatibility), nameof(ScriptManagerFacade), scriptId);

        CustomerScript script = await Db.GetCustomerScript(scriptId);
        return script.MinApiVersion;
    }

    // Validates if script can run on target version
    public async Task<bool> CheckVersionCompatibility(Guid scriptId, int targetApiVersion)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(CheckVersionCompatibility), nameof(ScriptManagerFacade), scriptId);

        CustomerScript script = await Db.GetCustomerScript(scriptId);
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
        Logger.LogTrace("Entered {MethodName} in {ClassName} with instanceId: {InstanceId}.", nameof(RegisterEmberInstance), nameof(ScriptManagerFacade), instanceId);

        // TODO
    }

    #endregion

    #region Duplicate Detection & Cleanup

    // Identifies duplicate scripts based on source code equivalence
    public async Task<(List<Guid> scriptGUIDs, Dictionary<Guid, int> cacheGUIDs)> DetectDuplicates()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(DetectDuplicates), nameof(ScriptManagerFacade));

        var dupes = await Db.DetectDuplicates();
        return (scriptGUIDs: dupes.scriptGUIDs, cacheGUIDs: dupes.cacheGUIDs);
    }

    // Removes duplicate scripts and orphaned caches
    public async Task RemoveDuplicates()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(RemoveDuplicates), nameof(ScriptManagerFacade));

        int currentApiVersion = GetRunningApiVersion();
        await Db.RemoveDuplicates();   //automatically get dupes from function in dbhelper dont have to pass therefore
    }

    // Removes caches without associated scripts
    public async Task CleanupOrphanedCaches()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(CleanupOrphanedCaches), nameof(ScriptManagerFacade));

        int currentApiVersion = GetRunningApiVersion();
        await Db.AutomaticCompilationOnVersionUpdate(currentApiVersion);    //this also does this maybe implement real funcion later
    }

    #endregion

    #region Monitoring & Diagnostics

    // Returns execution logs and metrics
    public void GetScriptExecutionHistory(Guid scriptId)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetScriptExecutionHistory), nameof(ScriptManagerFacade), scriptId);

        // TODO
    }

    // Returns compilation success/failure rates and performance metrics
    public void GetCompilationStatistics()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetCompilationStatistics), nameof(ScriptManagerFacade));

        // TODO
    }

    // Validates database connectivity, API version consistency, and system state
    public async Task HealthCheck()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(HealthCheck), nameof(ScriptManagerFacade));

        // TODO
        await Db.HealthCheck();
    }

    // Extracts className, baseTypeName, and version from script
    public async Task<string> GetScriptMetadata(Guid scriptId)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetScriptMetadata), nameof(ScriptManagerFacade), scriptId);

        CustomerScript script = await Db.GetCustomerScript(scriptId, includeCaches: true);
        string str = "Metadata for script: " + script.ToString();
        return str;
    }

    public async Task<Guid> GetScriptId(string scriptName, ScriptTypes scriptType)
    {
        return await Db.GetScriptId(scriptName, scriptType);
    }

    public async Task<Dictionary<int, List<ScriptCompiledCache>>> GetCachesForEachApiVersion()
    {
        return await Db.GetCachesForEachApiVersion();
    }

    public string GetUserName()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetUserName), nameof(ScriptManagerFacade));

        // return UsefulMethods.GetUserName();
        return "Gilles";
    }

    #endregion


}


public record ScriptNameType
{
    public required string Name { get; init; }
    public required ScriptTypes Type { get; init; }
}