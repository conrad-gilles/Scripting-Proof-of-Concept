using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;

public class ScriptManagerFacade : IScriptManager
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
    public async Task<Guid> CreateScript(string sourceCode, string? scriptType = null, string userName = "Default", int apiVersion = -1)    //maybe minApiVersion is better?
    {
        int currentApiVersion = await GetRecentApiVersion();
        Guid id = Guid.NewGuid();
        if (apiVersion == -1)
        {
            await db.CreateAndInsertCustomerScript(sourceCode, id, userName);
        }
        else
        {
            await db.CreateAndInsertCustomerScript(sourceCode, id, userName, apiVersion);
        }
        return id;

    }

    // Updates existing script source code and recompiles for all compatible API versions
    public async Task UpdateScript(Guid scriptId, string newSourceCode, string userName = "Default", int apiVersion = -1)
    {
        if (apiVersion == -1)
        {
            apiVersion = await GetRecentApiVersion();
        }
        var customerScript = await db.GetCustomerScript(scriptId);
        var creationDate = customerScript.CreatedAt;
        await db.DeleteCustomerScript(scriptId);    //todo update is still inefficient 
        await db.CreateAndInsertCustomerScript(newSourceCode, scriptId, userName, createdAt: (DateTime)creationDate); //todo unsafe af
        // await RecompileScript(scriptId);    //todo inefficient also untested, ineff because compiles 2 for 1 api v
    }

    // Removes script and all associated compiled caches
    public async Task DeleteScript(Guid scriptId)
    {
        await db.DeleteCustomerScript(scriptId);    //todo check if this also deletes the caches
    }

    // Retrieves script metadata and source code
    public async Task<CustomerScript> GetScript(Guid scriptId, bool includeCaches = false)
    {
        CustomerScript script = await db.GetCustomerScript(scriptId, includeCaches);
        return script;
    }

    // Returns all scripts with optional filtering by type, API version, or creation date
    public async Task<List<CustomerScript>> ListScripts(CustomerScriptFilter filters = null, bool includeCaches = false)
    {
        List<CustomerScript> scripts;
        scripts = await db.GetAllCustomerScripts(includeCaches: includeCaches, filters: filters);
        return scripts;
    }

    #endregion

    #region Compilation Operations

    // Compiles a specific script for a target API version, if target version not specified just takes the recent one
    public async Task CompileScript(Guid scriptId, int targetApiVersion = -1)
    {
        if (targetApiVersion == -1)
        {
            CustomerScript script = await db.GetCustomerScript(scriptId);
            await db.CreateAndInsertCompiledCache(script);
        }
        else
        {
            targetApiVersion = await GetRecentApiVersion();
            CustomerScript script = await db.GetCustomerScript(scriptId);
            await db.CreateAndInsertCompiledCache(script, oldApiV: targetApiVersion);
        }

    }

    // Compiles all compatible scripts for a new API version
    public async Task CompileAllScripts()
    {
        int currentApiVersion = await GetRecentApiVersion();
        await db.CompileAllStoredScripts(currentApiVersion);
    }

    // Recompiles script for all active API versions
    public async Task RecompileScript(Guid scriptId)  //todo maybe get rid of the currentapi and create global var in dbhelper class
    {
        await db.RecompileScript(scriptId, deleteAlso: true);  //todo fix this i need old version dlls or something like that which will need to be passed maybe as fodler path or file path
    }

    /// Performs syntax and interface validation without saving
    public async Task<string> ValidateScript(string sourceCode)
    {
        try
        {
            var validation = compiler.BasicValidationBeforeCompiling(sourceCode);
            string className = validation.className;    //figure out if i should do something with this
            string baseTypeName = validation.baseTypeName;
            int versionInt = validation.versionInt;

            return "Success: ClassName: " + className + ", BaseTypeName: " + baseTypeName + ", VersionInt: " + versionInt;
        }
        catch (Exception e)
        {
            return "Error: " + e.ToString();
        }

    }

    // Retrieves compilation error details
    public async Task<string> GetCompilationErrors(Guid scriptId, int apiVersion = -1)
    {
        try
        {
            CustomerScript script = await GetScript(scriptId);

            // compiler.BasicValidationBeforeCompiling(script.SourceCode);
            if (apiVersion == -1)
            {
                apiVersion = await GetRecentApiVersion();
                compiler.RunCompilation(script.SourceCode);
            }
            else
            {
                MetadataReference[] refs = compiler.GetReferencesForVersion(apiVersion);
                compiler.RunCompilation(script.SourceCode, refs);
            }

            return "Successful Compilation!";
        }
        catch (Exception e)
        {
            return "Failed to compilate script:" + scriptId + " " + e.ToString();
        }
    }

    #endregion

    #region Execution Operations

    // Executes a Generator Action script with provided context, realisitcally not needed 
    public async Task<ActionResultBaseClass> ExecuteActionScript(Guid scriptId, GeneratorContext context, int currentApiVersion = -1)    //lowkey so many errors better to have one 
    {
        try
        {
            if (currentApiVersion == -1)
            {
                currentApiVersion = await GetRecentApiVersion();
            }

            byte[] compiledScript = null;
            try
            {
                var temp = await db.GetCompiledScripCache(scriptId, currentApiVersion);
                compiledScript = temp.AssemblyBytes;
            }
            catch (Exception e)
            {
                Console.WriteLine("Retrieval failed jit comp launched:");
                await CompileScript(scriptId, currentApiVersion);
                //try again, if fails again we catch error outside
                var temp = await db.GetCompiledScripCache(scriptId, currentApiVersion);
                compiledScript = temp.AssemblyBytes;
            }

            //possibly add a null check for compiledScript
            ScriptExecutor executor = new ScriptExecutor(compiledScript, context);
            ActionResultBaseClass result = (ActionResultBaseClass)executor.RunScriptExecution<object>();  //todo maybe better handling than casting although the error will be thrown in the class itself
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw new Exception();
        }

    }

    // Executes a Generator Condition script and returns boolean result, realisitcally not needed 
    public async Task<bool> ExecuteConditionScript(Guid scriptId, GeneratorContext context, int ApiVersion = -1)
    {
        try
        {
            if (ApiVersion == -1)
            {
                ApiVersion = await GetRecentApiVersion();
            }

            byte[] compiledScript = null;
            try
            {
                var temp = await db.GetCompiledScripCache(scriptId, ApiVersion);
                compiledScript = temp.AssemblyBytes;
            }
            catch (Exception e)
            {
                Console.WriteLine("Retrieval failed jit comp launched:");
                await CompileScript(scriptId, await GetRecentApiVersion());
                //try again, if fails again we catch error outside
                var temp = await db.GetCompiledScripCache(scriptId, ApiVersion);
                compiledScript = temp.AssemblyBytes;
            }

            //possibly add a null check for compiledScript
            ScriptExecutor executor = new ScriptExecutor(compiledScript, context);
            bool result = (bool)executor.RunScriptExecution<object>();  //todo maybe better handling than casting although the error will be thrown in the class itself
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw new Exception();
        }
    }

    // Generic execution that detects script type automatically
    public async Task<object> ExecuteScriptById(Guid scriptId, GeneratorContext context, int currentApiVersion = -1)
    {
        try
        {
            if (currentApiVersion == -1)
            {
                currentApiVersion = await GetRecentApiVersion();
            }
            byte[] compiledScript = null;
            try
            {
                var temp = await db.GetCompiledScripCache(scriptId, currentApiVersion);
                compiledScript = temp.AssemblyBytes;
            }
            catch (Exception e)
            {
                Console.WriteLine("Retrieval failed jit comp launched:");
                await CompileScript(scriptId, await GetRecentApiVersion());
                //try again, if fails again we catch error outside
                var temp = await db.GetCompiledScripCache(scriptId, currentApiVersion);
                compiledScript = temp.AssemblyBytes;
            }

            //possibly add a null check for compiledScript
            ScriptExecutor executor = new ScriptExecutor(compiledScript, context);
            object result = executor.RunScriptExecution<object>();  //returns either bool or action result todo maybe add checks if thats the case but normally should be
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw new Exception();
        }
    }

    #endregion

    #region Cache Management

    // Retrieves compiled assembly bytes from cache
    public async Task<byte[]> GetCompiledCache(Guid scriptId, int currentApiVersion = -1)
    {
        if (currentApiVersion == -1)
        {
            currentApiVersion = await GetRecentApiVersion();
        }
        try
        {
            var temp = await db.GetCompiledScripCache(scriptId, currentApiVersion);
            byte[] compiledScript = temp.AssemblyBytes;
            return compiledScript;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw new Exception();
        }

    }

    // Removes all compiled versions of a script
    public async Task ClearScriptCache(Guid scriptId)
    {
        int currentApiVersion = await GetRecentApiVersion();
        for (int i = 0; i <= currentApiVersion; i++)    //this only works if currentApiVersion is the highest
        {
            try
            {
                await db.DeleteScriptCache(scriptId, i);
                Console.WriteLine("Deleted: " + i);
            }   //todo check if doesnt throw something  
            catch (Exception e)
            { Console.WriteLine(e.ToString()); }

        }

    }

    // Removes all compiled caches (maintenance operation)
    public async Task ClearAllCaches()
    {
        await db.DeleteAllCachedScripts();
    }

    // Background job to precompile all compatible scripts
    public async Task PrecompileForApiVersion()
    {
        int currentApiVersion = await GetRecentApiVersion();
        await db.AutomaticCompilationOnVersionUpdate(currentApiVersion);
    }

    #endregion

    #region Version Management

    // Returns list of currently active API versions from Ember instances
    public async Task<List<int>> GetActiveApiVersions() //todo implement
    {
        return await db.GetActiveApiVersions(); //shit implementation not really functional in rl
    }

    public async Task<int> GetRecentApiVersion() //todo implement
    {
        // var versions = await GetActiveApiVersions();
        // int last = versions[versions.Count() - 1];
        // return last;
        // return UsefulMethods.GetRecentApiVersion();
        return 6;
    }

    // Returns which API versions a script is compatible with
    public async Task<int> GetScriptCompatibility(Guid scriptId)
    {
        CustomerScript script = await db.GetCustomerScript(scriptId);
        return script.MinApiVersion;
    }

    // Validates if script can run on target version
    public async Task<bool> CheckVersionCompatibility(Guid scriptId, int targetApiVersion)
    {
        CustomerScript script = await db.GetCustomerScript(scriptId);
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
    public async Task RegisterEmberInstance(Guid instanceId, string emberVersion, int apiVersion)
    {
        // TODO
    }

    #endregion

    #region Duplicate Detection & Cleanup

    // Identifies duplicate scripts based on source code equivalence
    public async Task<(List<Guid> scriptGUIDs, Dictionary<Guid, int> cacheGUIDs)> DetectDuplicates()
    {
        var dupes = await db.DetectDuplicates();
        return (scriptGUIDs: dupes.scriptGUIDs, cacheGUIDs: dupes.cacheGUIDs);
    }

    // Removes duplicate scripts and orphaned caches
    public async Task RemoveDuplicates()
    {
        int currentApiVersion = await GetRecentApiVersion();
        await db.RemoveDuplicates();   //automatically get dupes from function in dbhelper dont have to pass therefore
    }

    // Removes caches without associated scripts
    public async Task CleanupOrphanedCaches()
    {
        int currentApiVersion = await GetRecentApiVersion();
        await db.AutomaticCompilationOnVersionUpdate(currentApiVersion);    //this also does this maybe implement real funcion later
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
        await db.HealthCheck();
    }

    // Extracts className, baseTypeName, and version from script
    public async Task<string> GetScriptMetadata(Guid scriptId)
    {
        CustomerScript script = await db.GetCustomerScript(scriptId, includeCaches: true);
        string str = "Metadata for script: " + script.ToString();
        return str;
    }

    public string GetUserName()
    {
        return UsefulMethods.GetUserName();
    }

    #endregion
}