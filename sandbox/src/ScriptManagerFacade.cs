using Microsoft.CodeAnalysis.Scripting;

public class ScriptManagerFacade
{
    DbHelper db;
    ScriptCompiler compiler;
    public ScriptManagerFacade()
    {
        db = new DbHelper();
        compiler = new ScriptCompiler();
    }

    #region Script Lifecycle

    /// Validates, compiles, and stores a new script
    public async Task CreateScript(string sourceCode, string scriptType, string userName, int currentApiVersion)    //maybe minApiVersion is better?
    {
        Guid id = Guid.NewGuid();
        await db.CreateAndInsertCustomerScript(sourceCode, id, userName, currentApiVersion);
    }

    // Updates existing script source code and recompiles for all compatible API versions
    public async Task UpdateScript(Guid scriptId, string newSourceCode, string userName, int currentApiVersion)
    {
        var customerScript = await db.GetCustomerScript(scriptId);
        var creationDate = customerScript.CreatedAt;
        await db.DeleteCustomerScript(scriptId);    //todo update is still inefficient 
        await db.CreateAndInsertCustomerScript(newSourceCode, scriptId, userName, currentApiVersion, createdAt: (DateTime)creationDate); //todo unsafe af
    }

    // Removes script and all associated compiled caches
    public async Task DeleteScript(Guid scriptId)
    {
        await db.DeleteCustomerScript(scriptId);    //todo check if this also deletes the caches
    }

    // Retrieves script metadata and source code
    public async Task<CustomerScript> GetScript(Guid scriptId)
    {
        CustomerScript script = await db.GetCustomerScript(scriptId);
        return script;
    }

    // Returns all scripts with optional filtering by type, API version, or creation date
    public async Task<List<CustomerScript>> ListScripts(object? filters = null)
    {
        List<CustomerScript> scripts = await db.GetAllCustomerScripts();    //todo implement filtering
        return scripts;
    }

    #endregion

    #region Compilation Operations

    // Compiles a specific script for a target API version
    public async Task CompileScript(Guid scriptId, int targetApiVersion)
    {
        CustomerScript script = await db.GetCustomerScript(scriptId);
        await db.CreateAndInsertCompiledCache(script, targetApiVersion);
    }

    // Compiles all compatible scripts for a new API version
    public async Task CompileAllScripts(int currentApiVersion)
    {
        await db.CompileAllStoredScripts(currentApiVersion);
    }

    // Recompiles script for all active API versions
    public async Task RecompileScript(Guid scriptId, int currentApiVersion)  //todo maybe get rid of the currentapi and create global var in dbhelper class
    {
        await db.AutomaticCompilationOnVersionUpdate(currentApiVersion);    //todo this is not perfect yet
    }

    /// Performs syntax and interface validation without saving
    public async Task ValidateScript(string sourceCode)
    {
        var validation = compiler.BasicValidationBeforeCompiling(sourceCode);
        string className = validation.className;    //figure out if i should do something with this
        string baseTypeName = validation.baseTypeName;
        int versionInt = validation.versionInt;
    }

    // Retrieves compilation error details
    public async Task<string> GetCompilationErrors(Guid scriptId, int apiVersion)
    {
        try
        {
            CustomerScript script = await GetScript(scriptId);
            // compiler.BasicValidationBeforeCompiling(script.SourceCode);
            compiler.RunCompilation(script.SourceCode);
            return "success";
        }
        catch (Exception e)
        {
            return e.ToString();
        }
    }

    #endregion

    #region Execution Operations

    // Executes a Generator Action script with provided context
    public async Task<ActionResult> ExecuteActionScript(Guid scriptId, int currentApiVersion, GeneratorContext context)
    {
        var temp = await db.GetCompiledScripCache(scriptId, currentApiVersion);
        byte[] compiledScript = temp.AssemblyBytes;

        ScriptExecutor executor = new ScriptExecutor(compiledScript, context);
        ActionResult result = (ActionResult)executor.RunScriptExecution<object>();  //todo maybe better handling than casting although the error will be thrown in the class itself
        return result;
    }

    // Executes a Generator Condition script and returns boolean result
    public async Task<bool> ExecuteConditionScript(Guid scriptId, GeneratorContext context, int currentApiVersion)
    {
        var temp = await db.GetCompiledScripCache(scriptId, currentApiVersion);
        byte[] compiledScript = temp.AssemblyBytes;

        ScriptExecutor executor = new ScriptExecutor(compiledScript, context);
        bool result = (bool)executor.RunScriptExecution<object>();  //todo maybe better handling than casting although the error will be thrown in the class itself
        return result;
    }

    // Generic execution that detects script type automatically
    public async Task<object> ExecuteScriptById(Guid scriptId, GeneratorContext context, int currentApiVersion)
    {
        var temp = await db.GetCompiledScripCache(scriptId, currentApiVersion);
        byte[] compiledScript = temp.AssemblyBytes;

        ScriptExecutor executor = new ScriptExecutor(compiledScript, context);
        object result = executor.RunScriptExecution<object>();  //returns either bool or action result todo maybe add checks if thats the case but normally should be
        return result;
    }

    #endregion

    #region Cache Management

    // Retrieves compiled assembly bytes from cache
    public async Task<byte[]> GetCompiledCache(Guid scriptId, int currentApiVersion)
    {
        var temp = await db.GetCompiledScripCache(scriptId, currentApiVersion);
        byte[] compiledScript = temp.AssemblyBytes;
        return compiledScript;
    }

    // Removes all compiled versions of a script
    public async Task ClearScriptCache(Guid scriptId, int currentApiVersion)
    {
        for (int i = 0; i < currentApiVersion + 1; i++)
        {
            await db.DeleteScriptCache(scriptId, i);    //todo check if doesnt throw something
        }

    }

    // Removes all compiled caches (maintenance operation)
    public async Task ClearAllCaches()
    {
        await db.DeleteAllCachedScripts();
    }

    // Background job to precompile all compatible scripts
    public async Task PrecompileForApiVersion(int apiVersion)
    {
        // await db.AutomaticCompilationOnVersionUpdate();
    }

    #endregion

    #region Version Management

    // Returns list of currently active API versions from Ember instances
    public async Task<List<int>> GetActiveApiVersions()
    {
        // TODO
        return null;
    }

    // Returns which API versions a script is compatible with
    public async Task GetScriptCompatibility(Guid scriptId)
    {
        // TODO
    }

    // Validates if script can run on target version
    public async Task<bool> CheckVersionCompatibility(Guid scriptId, int targetApiVersion)
    {
        // TODO
        return false;
    }

    // Registers a new Ember instance in the system
    public async Task RegisterEmberInstance(Guid instanceId, string emberVersion, int apiVersion)
    {
        // TODO
    }

    #endregion

    #region Duplicate Detection & Cleanup

    // Identifies duplicate scripts based on source code equivalence
    public async Task DetectDuplicates()
    {
        // TODO
    }

    // Removes duplicate scripts and orphaned caches
    public async Task RemoveDuplicates()
    {
        // TODO
    }

    // Removes caches without associated scripts
    public async Task CleanupOrphanedCaches()
    {
        // TODO
    }

    #endregion

    #region Monitoring & Diagnostics

    // Returns execution logs and metrics
    public async Task GetScriptExecutionHistory(Guid scriptId)
    {
        // TODO
    }

    // Returns compilation success/failure rates and performance metrics
    public async Task GetCompilationStatistics()
    {
        // TODO
    }

    // Validates database connectivity, API version consistency, and system state
    public async Task HealthCheck()
    {
        // TODO
    }

    // Extracts className, baseTypeName, and version from script
    public async Task GetScriptMetadata(Guid scriptId)
    {
        // TODO
    }

    #endregion
}
