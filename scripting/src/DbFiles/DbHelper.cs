using EFModeling.EntityProperties.FluentAPI.Required;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Ember.Scripting;
using Microsoft.Extensions.Logging;
using Microsoft.CodeAnalysis.Scripting;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace Ember.Scripting;

internal class DbHelper
{
    private readonly List<MetadataReference> References;
    private readonly ScriptCompiler Compiler;
    private readonly ILogger<DbHelper> Logger;
    private readonly int RecentApiVersion;
    public DbHelper(ScriptCompiler compiler, List<MetadataReference> references, ILogger<DbHelper> logger, int recentApiVersion)
    {
        References = references;
        Compiler = compiler;
        Logger = logger;
        RecentApiVersion = recentApiVersion;
    }
    // public async Task EnsureDeletedCreated()    //todo delete this for obvious safety reasons before production
    // {
    //     try
    //     {
    //         Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(EnsureDeletedCreated), nameof(DbHelper));

    //         using (var db = new MyContext())
    //         {
    //             await db.Database.EnsureDeletedAsync();
    //             await db.Database.EnsureCreatedAsync();
    //         }
    //     }

    //     catch (Exception e)
    //     {
    //         Logger.LogError(e.ToString());
    //         throw new DbHelperException(nameof(EnsureDeletedCreated) + " failed in " + nameof(DbHelper), e);
    //     }
    // }
    public async Task DeleteAllData()
    {

        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(DeleteAllData), nameof(DbHelper));

        using (var db = new MyContext())
        {
            await db.ScriptCompiledCaches.ExecuteDeleteAsync();
            await db.CustomerScripts.ExecuteDeleteAsync();
        }


    }
    public async Task<List<CustomerScript>> GetAllCustomerScripts(bool includeCaches = false, CustomerScriptFilter? filters = null)
    {

        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetAllCustomerScripts), nameof(DbHelper));

        if (filters != null)
        {
            using (var db = new MyContext())
            {
                IQueryable<CustomerScript> query = db.CustomerScripts;
                if (includeCaches)
                {
                    query = query.Include(s => s.CompiledCaches);
                }
                if (filters.ScriptName != null && filters.ScriptName != "")
                {
                    query = query.Where(s => s.ScriptName == filters.ScriptName);
                }
                if (filters.ScriptType != null && filters.ScriptType != "")
                {
                    query = query.Where(s => s.ScriptType == filters.ScriptType);
                }
                if (filters.SourceCode != null && filters.SourceCode != "")
                {
                    query = query.Where(s => s.SourceCode == filters.SourceCode);
                }
                if (filters.MinApiVersion != null)
                {
                    query = query.Where(s => s.MinApiVersion == filters.MinApiVersion);
                }
                if (filters.CreatedAt != null)
                {
                    query = query.Where(s => s.CreatedAt == filters.CreatedAt);
                }
                if (filters.ModifiedAt != null)
                {
                    query = query.Where(s => s.ModifiedAt == filters.ModifiedAt);
                }
                if (filters.CreatedBy != null && filters.CreatedBy != "")
                {
                    query = query.Where(s => s.CreatedBy == filters.CreatedBy);
                }

                return await query.ToListAsync();
            }
        }
        if (includeCaches == false)
        {
            using (var db = new MyContext())
            {
                var scripts = await db.CustomerScripts.ToListAsync();
                return scripts;
            }
        }
        else
        {
            using (var db = new MyContext())
            {
                var scripts = await db.CustomerScripts.Include(s => s.CompiledCaches).ToListAsync();
                return scripts;
            }
        }



    }
    public async Task<List<ScriptCompiledCache>> GetAllCompiledScriptCaches(bool includeScripts = true)
    {

        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetAllCompiledScriptCaches), nameof(DbHelper));
        if (!includeScripts)
        {
            using (var db = new MyContext())
            {
                var caches = await db.ScriptCompiledCaches
                .ToListAsync();
                return caches;
            }
        }
        using (var db = new MyContext())
        {
            var caches = await db.ScriptCompiledCaches
            .Include(c => c.CustomerScript)
            .ToListAsync();
            return caches;
        }


    }
    public int GetRecentApiVersion()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetRecentApiVersion), nameof(DbHelper));
        return RecentApiVersion;
    }
    public async Task ClearScriptCache(Guid scriptId)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(ClearScriptCache), nameof(DbHelper), scriptId);

        int currentApiVersion = GetRecentApiVersion();
        int highestV = currentApiVersion;
        foreach (var item in await GetActiveApiVersions())
        {
            if (item > highestV)
            {
                throw new DbHelperException(nameof(ClearScriptCache) + " failed in " + nameof(DbHelper) + " because for some reason the passed Recent Version is not the highest in the db.");
            }
        }
        for (int i = 0; i <= currentApiVersion; i++)    //checks above if current APi is highest
        {

            await DeleteScriptCache(scriptId, i);
            Logger.LogInformation("Deleted: " + i);
            Console.WriteLine("Deleted: " + i);



        }

    }
    public async Task RecompileScript(Guid scriptId, bool deleteAlso = false)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(RecompileScript), nameof(DbHelper), scriptId);

        int currentApiVersion = GetRecentApiVersion();
        CustomerScript script = await GetCustomerScript(scriptId, includeCaches: true);

        int start = script.MinApiVersion;
        int count = currentApiVersion - start + 1;
        int[] versions = Enumerable.Range(start, count).ToArray();
        if (deleteAlso)
        {
            await ClearScriptCache(scriptId);
        }
        foreach (var itemN in script.CompiledCaches)    //todo fix this once old version implemented
        {
            if (versions.Contains(itemN.ApiVersion) == false)
            {
                try
                {
                    await CreateAndInsertCompiledCache(script, itemN.ApiVersion);
                    Logger.LogInformation("Mock recompilation of old V" + itemN.ApiVersion);
                    Console.WriteLine("Mock recompilation of old V" + itemN.ApiVersion);
                }
                catch (Exception e) { Logger.LogError("Compilation of old version failed" + e.ToString()); }

            }
            if (itemN.ApiVersion == currentApiVersion)  //this is only temporary until the if statement above works
            {
                await CreateAndInsertCompiledCache(script);
                Logger.LogInformation("Real recompilation of new V" + itemN.ApiVersion + " , normal if twice");
                Console.WriteLine("Real recompilation of new V" + itemN.ApiVersion + " , normal if twice");
                break;
            }
        }


    }
    public async Task RecompileCache(Guid scriptId, int apiVersion)
    {
        await DeleteScriptCache(scriptId, apiVersion);
        CustomerScript script = await GetCustomerScript(scriptId);
        await CreateAndInsertCompiledCache(script, apiVersion);
    }
    public async Task AutomaticCompilationOnVersionUpdate(int currentApiVersion)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with currentApiVersion: {ApiVersion}.", nameof(AutomaticCompilationOnVersionUpdate), nameof(DbHelper), currentApiVersion);

        foreach (var item in await GetAllCustomerScripts(includeCaches: true))
        {
            bool currentImplementation = false;
            foreach (var itemN in item.CompiledCaches)
            {
                int start = item.MinApiVersion;
                int count = currentApiVersion - start + 1;

                int[] versions = Enumerable.Range(start, count).ToArray();
                if (versions.Contains(itemN.ApiVersion) == false)
                {
                    // await CreateAndInsertCompiledCache(item, currentApiVersion); //todo create a method to compile to all older versions also
                }

                if (itemN.ApiVersion == currentApiVersion)
                {
                    currentImplementation = true;
                }
                if (itemN.ApiVersion < item.MinApiVersion)
                {
                    await DeleteScriptCache(itemN.ScriptId, currentApiVersion);
                    Logger.LogInformation("Deleted an old Script Cache!");
                    Console.WriteLine("Deleted an old Script Cache!");
                }
            }

            if (currentImplementation == false)
            {
                await CreateAndInsertCompiledCache(item);
            }
        }
    }
    public async Task<CustomerScript> CreateAndInsertCustomerScript(string scriptString, Guid randomGUID, string createdBy, int? oldApiV = null, DateTime? createdAt = null, bool alsoCompAndSave = false, bool checkForDuplicates = true)
    //todo make sure compiling happens after verification of isDuplicate
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {ScriptId}.", nameof(CreateAndInsertCustomerScript), nameof(DbHelper), randomGUID);

        using (var db = new MyContext())
        {
            int currentApiVersion = GetRecentApiVersion();
            if (createdAt == null)
            {
                createdAt = DateTime.UtcNow;
            }
            // ScriptCompiler compiler = new ScriptCompiler(References);
            var getTupleFromVal = Compiler.BasicValidationBeforeCompiling(scriptString);

            CustomerScript randomTestScript2 = new CustomerScript
            {
                Id = randomGUID,
                ScriptName = getTupleFromVal.className,
                ScriptType = getTupleFromVal.baseTypeName,
                SourceCode = scriptString,
                MinApiVersion = getTupleFromVal.versionInt,
                CreatedAt = createdAt,
                ModifiedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
            checkForDuplicates = true;
            if (checkForDuplicates)
            {
                checkForDuplicates = await IsDuplicateScript(randomTestScript2);  //uncomment this if you dont want to check for duplicate existing in db when inserting
            }

            if (checkForDuplicates == false)
            {
                db.CustomerScripts.Add(randomTestScript2);
                await db.SaveChangesAsync();

                await CreateAndInsertCompiledCache(randomTestScript2, oldApiV);
                return randomTestScript2;
            }
            else
            {
                throw new DbHelperException(nameof(CreateAndInsertCustomerScript) + " failed in " + nameof(DbHelper) + " because a duplicate script was already in the database!");
            }
        }
    }
    public async Task CreateAndInsertCompiledCache(CustomerScript script, int? apiV = null)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {ScriptId}.", nameof(CreateAndInsertCompiledCache), nameof(DbHelper), script.Id);

        using (var db = new MyContext())
        {
            // int currentApiVersion;
            if (apiV == null)
            {
                apiV = GetRecentApiVersion();
            }

            var getTupleFromVal = Compiler.BasicValidationBeforeCompiling(script.SourceCode!);

            if (await db.ScriptCompiledCaches.AnyAsync(c => c.ScriptId == script.Id && c.ApiVersion == apiV))
            {
                Logger.LogInformation("Skipping insert of: " + getTupleFromVal.className + " because it already exists already exists.");
                Console.WriteLine("Skipping insert of: " + getTupleFromVal.className + " because it already exists already exists.");
                throw new DbHelperException(nameof(CreateAndInsertCompiledCache) + " failed in " + nameof(DbHelper) + "more details: " + "Skipping insert of: " + getTupleFromVal.className + " because it already exists already exists."); ;
                // return;
            }
            else
            {
                byte[] tempComp;
                tempComp = Compiler.RunCompilation(script.SourceCode!, metaData: getTupleFromVal);

                ScriptCompiledCache tempCache = new ScriptCompiledCache
                {
                    ScriptId = script.Id,
                    ApiVersion = (int)apiV,
                    AssemblyBytes = tempComp,
                    CompilationDate = DateTime.UtcNow,
                    CompilationSuccess = true,
                    CompilationErrors = "",
                    OldSourceCode = script.SourceCode
                };
                await InsertScriptCompiledCache(tempCache);
                await db.SaveChangesAsync();
            }
        }
    }
    public async Task DeleteCustomerScript(Guid id)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {ScriptId}.", nameof(DeleteCustomerScript), nameof(DbHelper), id);

        using (var db = new MyContext())
        {
            CustomerScript temp = await GetCustomerScript(id);
            db.Remove(temp);
            await db.SaveChangesAsync();
        }
    }
    public async Task DeleteScriptCache(Guid id, int ApiVersion)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {ScriptId} and ApiVersion: {ApiVersion}.", nameof(DeleteScriptCache), nameof(DbHelper), id, ApiVersion);

        using (var db = new MyContext())
        {
            try
            {
                ScriptCompiledCache temp = await GetCompiledScripCache(id, ApiVersion);
                db.Remove(temp);
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Logger.LogError(e.ToString());
                Console.WriteLine("Could not execute DeleteScriptCache");
                Logger.LogInformation("Could not execute DeleteScriptCache");
            }
        }
    }
    public async Task UpdateScript(Guid scriptId, string newSourceCode, string? userName = null, int? apiVersion = null)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(UpdateScript), nameof(DbHelper), scriptId);
        if (userName == null)
        {
            userName = "Default";
        }

        using (var db = new MyContext())
        {
            CustomerScript? existingScript = await db.CustomerScripts.FindAsync(scriptId);

            if (existingScript != null)
            {
                existingScript.SourceCode = newSourceCode;
                existingScript.ModifiedAt = DateTime.UtcNow;

                if (userName != null)
                {
                    existingScript.CreatedBy = userName;
                }
            }
            else
            {
                Logger.LogDebug("Somethign went wrong retrieving your script, we could not find it.");
                throw new DbHelperException("Could not find the script what needs to be updated.");
            }

            await db.SaveChangesAsync();
        }
    }
    public async Task InsertScriptCompiledCache(ScriptCompiledCache scriptCompiledCache)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(InsertScriptCompiledCache), nameof(DbHelper));

        using (var db = new MyContext())
        {
            scriptCompiledCache.CustomerScript = null!;

            db.ScriptCompiledCaches.Add(scriptCompiledCache);
            await db.SaveChangesAsync();
        }
    }

    public async Task<ScriptCompiledCache> GetCompiledScripCache(Guid id, int ApiVersion)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {ScriptId}.", nameof(GetCompiledScripCache), nameof(DbHelper), id);

        using (var db = new MyContext())
        {
            var cache = await db.ScriptCompiledCaches.SingleAsync(b => b.ScriptId == id && b.ApiVersion == ApiVersion);
            return cache;
        }
    }
    public async Task<CustomerScript> GetCustomerScript(Guid id, bool includeCaches = false)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {ScriptId}.", nameof(GetCustomerScript), nameof(DbHelper), id);

        if (includeCaches == true)
        {
            using (var db = new MyContext())
            {

                var script = await db.CustomerScripts.Include(s => s.CompiledCaches).SingleAsync(b => b.Id == id);
                return script;
            }
        }
        else
        {
            using (var db = new MyContext())
            {
                var script = await db.CustomerScripts.SingleAsync(b => b.Id == id);
                return script;
            }
        }
    }

    public async Task<Guid> GetScriptId(string scriptName, ScriptTypes scriptType)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with Name: {ScriptId}.", nameof(GetCustomerScript), nameof(DbHelper), scriptName);
        string sScriptType;
        switch (scriptType)
        {
            case ScriptTypes.GeneratorActionScript:
                sScriptType = "IGeneratorActionScript";
                break;
            case ScriptTypes.GeneratorConditionScript:
                sScriptType = "IGeneratorConditionScript";
                break;
            default:
                throw new DbHelperException(message: "Could not convert Script type enum to string");
        }
        using (var db = new MyContext())
        {
            var script = await db.CustomerScripts.SingleAsync(b => b.ScriptName == scriptName && b.ScriptType == sScriptType);
            return script.Id;
        }
    }
    public async Task CompileAllStoredScripts(int currentApiVersion)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(CompileAllStoredScripts), nameof(DbHelper));

        using (var db = new MyContext())
        {
            List<CustomerScript> allScriptSC = await GetAllCustomerScripts();
            for (int i = 0; i < allScriptSC.Count(); i++)
            {
                await CreateAndInsertCompiledCache(allScriptSC[i]);
                await db.SaveChangesAsync();
            }
        }
    }

    public async Task SaveScriptWithoutCompiling(Guid id, string sourceCode, string? userName = null)
    {
        using (var db = new MyContext())
        {
            CustomerScript? existingScript = await db.CustomerScripts.FindAsync(id);

            if (existingScript != null)
            {
                existingScript.SourceCode = sourceCode;
                existingScript.ModifiedAt = DateTime.UtcNow;

                if (userName != null)
                {
                    existingScript.CreatedBy = userName;
                }
            }
            else
            {
                CustomerScript testScript = new CustomerScript
                {
                    Id = id,
                    ScriptName = null,
                    ScriptType = null,
                    SourceCode = sourceCode,
                    MinApiVersion = GetRecentApiVersion(),
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow,
                    CreatedBy = null
                };
                db.CustomerScripts.Add(testScript);

            }
            await db.SaveChangesAsync();
        }
    }
    public async Task DeleteAllCachedScripts()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(DeleteAllCachedScripts), nameof(DbHelper));

        using (var db = new MyContext())
        {
            await db.ScriptCompiledCaches.ExecuteDeleteAsync();
        }
    }
    public async Task<bool> IsDuplicateScript(CustomerScript script)    //todo make more efficient using ef core
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(IsDuplicateScript), nameof(DbHelper));
        // CustomerScriptFilter filter = new CustomerScriptFilter(scriptName: script.ScriptName, sourceCode: script.SourceCode);
        CustomerScriptFilter filter = new CustomerScriptFilter(scriptName: script.ScriptName, scriptType: script.ScriptType);
        List<CustomerScript> allScripts = await GetAllCustomerScripts(filters: filter);



        if (allScripts.Count() == 0)
        {
            return false;
        }
        return true;
        // foreach (var item in allScripts)
        // {
        //     if (Compiler.IsTheSameTree(item.SourceCode!, script.SourceCode!))
        //     {
        //         return true;
        //     }
        // }
        // return false;   //but name is duplicate! only source code is unique, also this could be an issue if the name of the script is not taken from the source code as it could lead to a script having a diffrent name but the same source code as another this would lead to this algorithm not finginh the duplicate script
    }
    public async Task<(List<Guid> scriptGUIDs, Dictionary<Guid, int> cacheGUIDs)> DetectDuplicates()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(DetectDuplicates), nameof(DbHelper));

        List<CustomerScript> allScripts = await GetAllCustomerScripts(includeCaches: true);
        List<Guid> duplicateGuids = [];
        for (int i = 0; i < allScripts.Count(); i++)
        {
            for (int j = 0; j < allScripts.Count(); j++)
            {
                if (i != j && j > i)
                {
                    if (allScripts[i].Id == allScripts[j].Id
                    || allScripts[i].ScriptName == allScripts[j].ScriptName
                    || allScripts[i].SourceCode == allScripts[j].SourceCode
                    || Compiler.IsTheSameTree(allScripts[i].SourceCode!, allScripts[j].SourceCode!)
                    )
                    {
                        if (duplicateGuids.Contains(allScripts[j].Id) == false)
                        {
                            duplicateGuids.Add(allScripts[j].Id);
                        }
                    }
                }
            }
        }
        List<ScriptCompiledCache> allCaches = await GetAllCompiledScriptCaches();
        Dictionary<Guid, int> cachesToDelete = [];
        for (int i = 0; i < allCaches.Count(); i++)
        {
            Guid cacheID = allCaches[i].ScriptId;
            bool found = false;
            for (int j = 0; j < allScripts.Count(); j++)
            {
                if (allScripts[j].Id == cacheID)
                {
                    found = true;
                }
            }
            if (found == false)
            {
                try
                {
                    cachesToDelete.Add(cacheID, allCaches[i].ApiVersion);        //also add the version
                                                                                 // await DeleteScriptCache(cacheID);   //deletes if there is a cache without a script attached to it
                }
                catch (Exception e)
                {
                    Logger.LogError("Could not add cache to DB." + e.ToString());
                    throw new DbHelperException("Could not add cache to DB.", e);
                }

            }
        }
        return (scriptGUIDs: duplicateGuids, cacheGUIDs: cachesToDelete);
    }
    public async Task<List<int>> GetActiveApiVersions()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetActiveApiVersions), nameof(DbHelper));

        var caches = await GetAllCompiledScriptCaches();
        List<int> ls = [];
        foreach (var item in caches)
        {
            if (ls.Contains(item.ApiVersion) == false)
            {
                ls.Add(item.ApiVersion);
            }
        }
        return ls;
    }
    public async Task<int> GetMostRecentApiVersionInDb()
    {
        List<int> versions = await GetActiveApiVersions();
        return versions.Max();
    }
    public async Task RemoveDuplicates()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(RemoveDuplicates), nameof(DbHelper));

        List<Guid> duplicateGuids = (await DetectDuplicates()).scriptGUIDs;
        Dictionary<Guid, int> cachesWithoutScript = (await DetectDuplicates()).cacheGUIDs;

        for (int i = 0; i < duplicateGuids.Count(); i++)
        {
            if (duplicateGuids.Contains(duplicateGuids[i]))
            {
                try
                {
                    await DeleteCustomerScript(duplicateGuids[i]);
                }
                catch (Exception e)
                {
                    Logger.LogError("Error when deleting index: " + i + " the GUID: " + duplicateGuids[i] + ". Full Exception: " + e.ToString());
                }
            }
        }
        foreach (var item in cachesWithoutScript.Keys)
        {
            try { await DeleteScriptCache(item, cachesWithoutScript[item]); }
            catch (Exception e)
            {
                Logger.LogError("Failed to delete Script Cache." + e.ToString());
                throw new DbHelperException("Failed to delete Script Cache.");
            }
        }
    }
    //Ai generated
    public async Task HealthCheck() //todo verify
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(HealthCheck), nameof(DbHelper));

        // ScriptCompiler compiler = new ScriptCompiler(References);

        //checking if can connect to db
        using (var context = new MyContext())
        {
            bool canConnect = await context.Database.CanConnectAsync();
            if (!canConnect)
            {
                throw new InvalidOperationException("HealthCheck Failed: Unable to connect to the database.");
            }
            //verify if db tables exist
            try
            {
                await context.EmberInstances.FirstOrDefaultAsync(); //if it fail it will throw
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"HealthCheck Failed: System state invalid. Unable to query critical tables (EmberInstances). Error: {ex.Message}", ex);
            }
        }

        try
        {
            // Ensure the current runtime version is valid (positive integer)
            int currentApiVersion = GetRecentApiVersion();
            if (currentApiVersion <= 0)
            {
                throw new InvalidOperationException($"HealthCheck Failed: Invalid Current API Version configured ({currentApiVersion}).");
            }

            // Ensure we can retrieve active API versions from the database (Consistency check)
            var activeVersions = await GetActiveApiVersions();
            if (activeVersions == null)
            {
                throw new InvalidOperationException("HealthCheck Failed: Active API versions list is null.");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"HealthCheck Failed: API Version consistency check encountered an error. {ex.Message}", ex);
        }

        // 4. Validate Compiler State
        if (Compiler == null)
        {
            throw new InvalidOperationException("HealthCheck Failed: ScriptCompiler is not correctly initialized.");
        }
    }

    public async Task<Dictionary<int, List<ScriptCompiledCache>>> GetCachesForEachApiVersion()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetActiveApiVersions), nameof(DbHelper));

        var caches = await GetAllCompiledScriptCaches();
        Dictionary<int, List<ScriptCompiledCache>> returnedDict = [];

        List<int> ls = [];
        foreach (var cache in caches)
        {
            if (returnedDict.Keys.Contains(cache.ApiVersion) == false)
            {
                List<ScriptCompiledCache> newList = [];
                returnedDict.Add(cache.ApiVersion, newList);
                returnedDict[cache.ApiVersion].Add(cache);
            }
            else
            {
                // List<ScriptCompiledCache> retrievedList = returnedDict[item.ApiVersion];
                // retrievedList.Add(item);
                returnedDict[cache.ApiVersion].Add(cache);
            }

        }
        return returnedDict;
    }
}
