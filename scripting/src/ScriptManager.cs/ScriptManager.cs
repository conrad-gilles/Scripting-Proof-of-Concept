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

    public async Task<Guid> CreateScript(string sourceCode, string userName = "Default", int apiVersion = -1, DateTime? createdAt = null)    //maybe minApiVersion is better?
    {
        Logger.LogDebug("Entered {MethodName} in {ClassName} apiVersion: {apiVersion}.", nameof(CreateScript), nameof(ScriptManagerFacade), apiVersion);

        int currentApiVersion = await GetRecentApiVersion();
        Guid id = Guid.NewGuid();

        if (apiVersion == -1)
        {
            await Db.CreateAndInsertCustomerScript(sourceCode, id, userName, createdAt: createdAt);
        }
        else
        {
            await Db.CreateAndInsertCustomerScript(sourceCode, id, userName, apiVersion, createdAt: createdAt);
        }
        return id;

    }

    // Updates existing script source code and recompiles for all compatible API versions
    public async Task UpdateScript(Guid scriptId, string newSourceCode, string userName = "Default", int apiVersion = -1)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(UpdateScript), nameof(ScriptManagerFacade), scriptId);

        if (apiVersion == -1)
        {
            apiVersion = await GetRecentApiVersion();
        }
        var customerScript = await Db.GetCustomerScript(scriptId);
        var creationDate = customerScript.CreatedAt;
        await Db.DeleteCustomerScript(scriptId);    //todo update is still inefficient
        await Db.CreateAndInsertCustomerScript(newSourceCode, scriptId, userName, createdAt: (DateTime)creationDate!); //todo unsafe af
        // await RecompileScript(scriptId);    //todo inefficient also untested, ineff because compiles 2 for 1 api v
    }

    // Removes script and all associated compiled caches
    public async Task DeleteScript(Guid scriptId)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(DeleteScript), nameof(ScriptManagerFacade), scriptId);

        await Db.DeleteCustomerScript(scriptId);    //todo check if this also deletes the caches
    }

    // Retrieves script metadata and source code
    public async Task<CustomerScript> GetScript(Guid scriptId, bool includeCaches = false)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetScript), nameof(ScriptManagerFacade), scriptId);

        CustomerScript script = await Db.GetCustomerScript(scriptId, includeCaches);
        return script;
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
    public async Task CompileScript(Guid scriptId, int targetApiVersion = -1)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId} and targetApiVersion: {TargetApiVersion}.", nameof(CompileScript), nameof(ScriptManagerFacade), scriptId, targetApiVersion);

        if (targetApiVersion == -1)
        {
            CustomerScript script = await Db.GetCustomerScript(scriptId);
            await Db.CreateAndInsertCompiledCache(script);
        }
        else
        {
            targetApiVersion = await GetRecentApiVersion();
            CustomerScript script = await Db.GetCustomerScript(scriptId);
            await Db.CreateAndInsertCompiledCache(script, oldApiV: targetApiVersion);
        }

    }

    // Compiles all compatible scripts for a new API version
    public async Task CompileAllScripts()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(CompileAllScripts), nameof(ScriptManagerFacade));

        int currentApiVersion = await GetRecentApiVersion();
        await Db.CompileAllStoredScripts(currentApiVersion);
    }

    // Recompiles script for all active API versions
    public async Task RecompileScript(Guid scriptId)  //todo maybe get rid of the currentapi and create global var in dbhelper class
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(RecompileScript), nameof(ScriptManagerFacade), scriptId);

        await Db.RecompileScript(scriptId, deleteAlso: true);  //todo fix this i need old version dlls or something like that which will need to be passed maybe as fodler path or file path
    }

    /// Performs syntax and interface validation without saving
    public string ValidateScript(string sourceCode)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(ValidateScript), nameof(ScriptManagerFacade));
        try
        {
            var validation = Compiler.BasicValidationBeforeCompiling(sourceCode);
            string className = validation.className;    //figure out if i should do something with this
            string baseTypeName = validation.baseTypeName;
            int versionInt = validation.versionInt;

            return "Success: ClassName: " + className + ", BaseTypeName: " + baseTypeName + ", VersionInt: " + versionInt;
        }
        catch (Exception e)
        {
            return "Error: " + e.ToString();
            // throw new FacadeException(e.ToString(), e);
        }

    }

    // Retrieves compilation error details
    public async Task<string> GetCompilationErrors(Guid scriptId, int apiVersion = -1)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetCompilationErrors), nameof(ScriptManagerFacade), scriptId);
        try
        {
            (string className, string baseTypeName, int versionInt)? metaData = null;
            CustomerScript script = await GetScript(scriptId);
            try
            {
                metaData = Compiler.BasicValidationBeforeCompiling(script.SourceCode!);
            }
            catch (Exception e)
            {
                Logger.LogError("Validation in GetCompilationErrors failed but will still try to compile." + e.ToString());
            }

            if (apiVersion == -1)
            {
                apiVersion = await GetRecentApiVersion();
                Compiler.RunCompilation(script.SourceCode!, metaData: metaData);
            }
            else
            {
                Compiler.RunCompilation(script.SourceCode!, metaData: metaData);
            }

            return "Successful Compilation!";

        }
        catch (Exception e)
        {
            return "Failed to compilate script:" + scriptId + " " + e.ToString();
            // throw new FacadeException(e.ToString(), e);
        }
    }

    public (string className, string baseTypeName, int versionInt) BasicValidationBeforeCompiling(string script)
    {
        return Compiler.BasicValidationBeforeCompiling(script);
    }

    #endregion

    #region Execution Operations

    // Executes a Generator Action script with provided context, realisitcally not needed
    public async Task<ActionResultBaseClass> ExecuteActionScript(Guid scriptId, GeneratorContext context, int currentApiVersion = -1)    //lowkey so many errors better to have one
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(ExecuteActionScript), nameof(ScriptManagerFacade), scriptId);
        try
        {
            if (currentApiVersion == -1)
            {
                currentApiVersion = await GetRecentApiVersion();
            }

            byte[]? compiledScript = null;
            try
            {
                var temp = await Db.GetCompiledScripCache(scriptId, currentApiVersion);
                compiledScript = temp.AssemblyBytes;
            }
            catch (Exception e)
            {
                Logger.LogError("Retrieval failed jit comp launched:" + e.ToString());
                await CompileScript(scriptId, currentApiVersion);
                //try again, if fails again we catch error outside
                var temp = await Db.GetCompiledScripCache(scriptId, currentApiVersion);
                compiledScript = temp.AssemblyBytes;
            }

            //possibly add a null check for compiledScript
            // ScriptExecutor executor = new ScriptExecutor();
            ActionResultBaseClass result = (ActionResultBaseClass)Executor.RunScriptExecution<object>(compiledScript!, context);  //todo maybe better handling than casting although the error will be thrown in the class itself
            return result;
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            throw new FacadeException(e.ToString(), e);
        }

    }

    // Executes a Generator Condition script and returns boolean result, realisitcally not needed
    public async Task<bool> ExecuteConditionScript(Guid scriptId, GeneratorContext context, int ApiVersion = -1)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(ExecuteConditionScript), nameof(ScriptManagerFacade), scriptId);
        try
        {
            if (ApiVersion == -1)
            {
                ApiVersion = await GetRecentApiVersion();
            }

            byte[]? compiledScript = null;
            try
            {
                var temp = await Db.GetCompiledScripCache(scriptId, ApiVersion);
                compiledScript = temp.AssemblyBytes;
            }
            catch (Exception e)
            {
                Logger.LogError("Retrieval failed jit comp launched:" + e.ToString());
                await CompileScript(scriptId, await GetRecentApiVersion());
                //try again, if fails again we catch error outside
                var temp = await Db.GetCompiledScripCache(scriptId, ApiVersion);
                compiledScript = temp.AssemblyBytes;
            }

            //possibly add a null check for compiledScript
            // ScriptExecutor executor = new ScriptExecutor();
            bool result = (bool)Executor.RunScriptExecution<object>(compiledScript!, context);  //todo maybe better handling than casting although the error will be thrown in the class itself
            return result;
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            throw new FacadeException(e.ToString(), e);
        }
    }

    // Generic execution that detects script type automatically
    public async Task<object> ExecuteScriptById(Guid scriptId, GeneratorContext context, int? apiVersion = null)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(ExecuteScriptById), nameof(ScriptManagerFacade), scriptId);
        try
        {
            if (apiVersion == null)
            {
                apiVersion = await GetRecentApiVersion();
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
                await CompileScript(scriptId, await GetRecentApiVersion());
                //try again, if fails again we catch error outside
                var temp = await Db.GetCompiledScripCache(scriptId, (int)apiVersion);
                compiledScript = temp.AssemblyBytes;
            }

            //possibly add a null check for compiledScript
            // ScriptExecutor executor = new ScriptExecutor();
            object result = Executor.RunScriptExecution<object>(compiledScript!, context);  //returns either bool or action result todo maybe add checks if thats the case but normally should be
            return result;
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            throw;
        }
    }

    #endregion

    #region Cache Management

    // Retrieves compiled assembly bytes from cache
    public async Task<byte[]> GetCompiledCache(Guid scriptId, int currentApiVersion = -1)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetCompiledCache), nameof(ScriptManagerFacade), scriptId);
        if (currentApiVersion == -1)
        {
            currentApiVersion = await GetRecentApiVersion();
        }
        try
        {
            var temp = await Db.GetCompiledScripCache(scriptId, currentApiVersion);
            byte[]? compiledScript = temp.AssemblyBytes;
            return compiledScript!;
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            throw new FacadeException(e.ToString(), e);
        }

    }

    // Removes all compiled versions of a script
    public async Task ClearScriptCache(Guid scriptId)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(ClearScriptCache), nameof(ScriptManagerFacade), scriptId);
        await Db.ClearScriptCache(scriptId);
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

        int currentApiVersion = await GetRecentApiVersion();
        await Db.AutomaticCompilationOnVersionUpdate(currentApiVersion);
    }

    public async Task EnsureDeletedCreated()
    {
        await Db.EnsureDeletedCreated();
    }
    #endregion

    #region Version Management

    // Returns list of currently active API versions from Ember instances
    public async Task<List<int>> GetActiveApiVersions() //todo implement
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetActiveApiVersions), nameof(ScriptManagerFacade));

        return await Db.GetActiveApiVersions(); //shit implementation not really functional in rl
    }

    public async Task<int> GetRecentApiVersion() //todo implement
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetRecentApiVersion), nameof(ScriptManagerFacade));

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

        int currentApiVersion = await GetRecentApiVersion();
        await Db.RemoveDuplicates();   //automatically get dupes from function in dbhelper dont have to pass therefore
    }

    // Removes caches without associated scripts
    public async Task CleanupOrphanedCaches()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(CleanupOrphanedCaches), nameof(ScriptManagerFacade));

        int currentApiVersion = await GetRecentApiVersion();
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

    public string GetUserName()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetUserName), nameof(ScriptManagerFacade));

        // return UsefulMethods.GetUserName();
        return "Gilles";
    }

    #endregion


}
