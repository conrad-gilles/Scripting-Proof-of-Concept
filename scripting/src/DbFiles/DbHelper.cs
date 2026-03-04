using EFModeling.EntityProperties.FluentAPI.Required;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Ember.Scripting;
using Microsoft.Extensions.Logging;
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
    public async Task EnsureDeletedCreated()    //todo delete this for obvious safety reasons before production
    {
        try
        {
            Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(EnsureDeletedCreated), nameof(DbHelper));

            using (var db = new MyContext())
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();
            }
        }

        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            throw new DbHelperException(nameof(EnsureDeletedCreated) + " failed in " + nameof(DbHelper), e);
        }
    }
    public async Task<List<CustomerScript>> GetAllCustomerScripts(bool includeCaches = false, CustomerScriptFilter? filters = null)
    {
        try
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
        catch (Exception e)
        {
            throw new DbHelperException(nameof(GetAllCustomerScripts) + " failed in " + nameof(DbHelper), e);
        }

    }
    public async Task<List<ScriptCompiledCache>> GetAllCompiledScriptCaches(bool includeScripts = true)
    {
        try
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
        catch (Exception e)
        {
            throw new DbHelperException(nameof(GetAllCompiledScriptCaches) + " failed in " + nameof(DbHelper), e);
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
            try
            {
                await DeleteScriptCache(scriptId, i);
                Logger.LogInformation("Deleted: " + i);
                Console.WriteLine("Deleted: " + i);
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                throw new DbHelperException(nameof(ClearScriptCache) + " failed", e);
            }

        }

    }
    public async Task RecompileScript(Guid scriptId, bool deleteAlso = false)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(RecompileScript), nameof(DbHelper), scriptId);

        try
        {
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
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            throw new DbHelperException(nameof(RecompileScript) + " failed in " + nameof(DbHelper), e);
        }
    }
    public async Task AutomaticCompilationOnVersionUpdate(int currentApiVersion)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with currentApiVersion: {ApiVersion}.", nameof(AutomaticCompilationOnVersionUpdate), nameof(DbHelper), currentApiVersion);

        try
        {
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
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            throw new DbHelperException(nameof(AutomaticCompilationOnVersionUpdate) + " failed in " + nameof(DbHelper), e);
        }

    }
    public async Task<CustomerScript> CreateAndInsertCustomerScript(string scriptString, Guid randomGUID, string createdBy, int oldApiV = -1, DateTime? createdAt = null)
    //todo make sure compiling happens after verification of isDuplicate
    {
        try
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

                bool testBool = false;
                // testBool = await IsDuplicateScript(randomTestScript2);  //uncomment this if you dont want to check for duplicate existing in db when inserting


                if (testBool == false)
                {
                    byte[] tempComp;
                    if (oldApiV != -1)
                    {
                        tempComp = Compiler.RunCompilation(scriptString, apiVersion: oldApiV, metaData: getTupleFromVal);
                        currentApiVersion = oldApiV;
                    }
                    else
                    {
                        currentApiVersion = GetRecentApiVersion();
                        tempComp = Compiler.RunCompilation(scriptString, metaData: getTupleFromVal);
                    }

                    randomTestScript2.CompiledCaches.Add(new ScriptCompiledCache
                    {
                        ScriptId = randomGUID,
                        ApiVersion = currentApiVersion,
                        AssemblyBytes = tempComp,
                        CompilationDate = DateTime.UtcNow,
                        CompilationSuccess = true,
                        CompilationErrors = "",
                        OldSourceCode = scriptString    //todo
                    });
                    db.CustomerScripts.Add(randomTestScript2);
                    await db.SaveChangesAsync();
                    return randomTestScript2;
                }
                else
                {
                    // return randomTestScript2;
                    throw new DbHelperException(nameof(CreateAndInsertCustomerScript) + " failed in " + nameof(DbHelper) + " because a duplicate script was already in the database!");
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            throw;
            // throw new DbHelperException(nameof(CreateAndInsertCustomerScript) + " failed in " + nameof(DbHelper), e);
        }
    }
    public async Task CreateAndInsertCompiledCache(CustomerScript script, int oldApiV = -1)
    {
        try
        {
            Logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {ScriptId}.", nameof(CreateAndInsertCompiledCache), nameof(DbHelper), script.Id);

            using (var db = new MyContext())
            {
                int currentApiVersion = GetRecentApiVersion();

                var getTupleFromVal = Compiler.BasicValidationBeforeCompiling(script.SourceCode!);

                if (await db.ScriptCompiledCaches.AnyAsync(c => c.ScriptId == script.Id && c.ApiVersion == currentApiVersion))
                {
                    Logger.LogInformation("Skipping insert of: " + getTupleFromVal.className + " because it already exists already exists.");
                    Console.WriteLine("Skipping insert of: " + getTupleFromVal.className + " because it already exists already exists.");
                    return;
                }
                else
                {
                    byte[] tempComp;
                    if (oldApiV != -1)
                    {
                        // List<MetadataReference> refs = Compiler.GetReferencesForVersion(oldApiV, References);
                        tempComp = Compiler.RunCompilation(script.SourceCode!, metaData: getTupleFromVal);
                        currentApiVersion = oldApiV;
                    }
                    else
                    {
                        tempComp = Compiler.RunCompilation(script.SourceCode!, metaData: getTupleFromVal);
                    }
                    ScriptCompiledCache tempCache = new ScriptCompiledCache
                    {
                        ScriptId = script.Id,
                        ApiVersion = currentApiVersion,
                        AssemblyBytes = tempComp,
                        CompilationDate = DateTime.UtcNow,
                        CompilationSuccess = true,
                        CompilationErrors = "",
                        OldSourceCode = script.SourceCode   //todo
                    };
                    await InsertScriptCompiledCache(tempCache);
                    await db.SaveChangesAsync();
                }
            }
        }
        catch (Exception e)
        {
            throw new DbHelperException(nameof(CreateAndInsertCompiledCache) + " failed in " + nameof(DbHelper), e);
        }

    }
    public async Task DeleteCustomerScript(Guid id)
    {
        try
        {
            Logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {ScriptId}.", nameof(DeleteCustomerScript), nameof(DbHelper), id);

            using (var db = new MyContext())
            {
                CustomerScript temp = await GetCustomerScript(id);
                db.Remove(temp);
                await db.SaveChangesAsync();
            }
        }
        catch (Exception e)
        {
            throw new DbHelperException(nameof(DeleteCustomerScript) + " failed in " + nameof(DbHelper), e);
        }
    }
    public async Task DeleteScriptCache(Guid id, int ApiVersion)
    {
        try
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
                    Console.WriteLine("Could not execute DeleteScriptCache");
                }
            }
        }
        catch (Exception e)
        {
            throw new DbHelperException(nameof(DeleteScriptCache) + " failed in " + nameof(DbHelper), e);
        }
    }
    public void UpdateCustomerScript(CustomerScript customerScript)
    {
        try
        {
            Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(UpdateCustomerScript), nameof(DbHelper));

            using (var db = new MyContext())
            {
                //todo
            }
        }
        catch (Exception e)
        {
            throw new DbHelperException(nameof(UpdateCustomerScript) + " failed in " + nameof(DbHelper), e);
        }
    }
    public async Task InsertScriptCompiledCache(ScriptCompiledCache scriptCompiledCache)
    {
        try
        {
            Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(InsertScriptCompiledCache), nameof(DbHelper));

            using (var db = new MyContext())
            {
                scriptCompiledCache.CustomerScript = null!;

                db.ScriptCompiledCaches.Add(scriptCompiledCache);
                await db.SaveChangesAsync();
            }
        }
        catch (Exception e)
        {
            throw new DbHelperException(nameof(InsertScriptCompiledCache) + " failed in " + nameof(DbHelper), e);
        }
    }

    public async Task<ScriptCompiledCache> GetCompiledScripCache(Guid id, int ApiVersion)
    {
        try
        {
            Logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {ScriptId}.", nameof(GetCompiledScripCache), nameof(DbHelper), id);

            using (var db = new MyContext())
            {
                var cache = await db.ScriptCompiledCaches.SingleAsync(b => b.ScriptId == id && b.ApiVersion == ApiVersion);
                return cache;
            }
        }
        catch (Exception e)
        {
            throw new DbHelperException(nameof(GetCompiledScripCache) + " failed in " + nameof(DbHelper), e);
        }
    }
    public async Task<CustomerScript> GetCustomerScript(Guid id, bool includeCaches = false)
    {
        try
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
        catch (Exception e)
        {
            throw new DbHelperException(nameof(GetCustomerScript) + " failed in " + nameof(DbHelper), e);
        }
    }


    public async Task CompileAllStoredScripts(int currentApiVersion)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(CompileAllStoredScripts), nameof(DbHelper));

        try
        {
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
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            throw new DbHelperException(nameof(CompileAllStoredScripts) + " failed in " + nameof(DbHelper), e);
        }
    }
    public async Task DeleteAllCachedScripts()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(DeleteAllCachedScripts), nameof(DbHelper));

        try
        {
            using (var db = new MyContext())
            {
                await db.ScriptCompiledCaches.ExecuteDeleteAsync();
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            throw new DbHelperException(nameof(DeleteAllCachedScripts) + " failed in " + nameof(DbHelper), e);
        }
    }
    public async Task<bool> IsDuplicateScript(CustomerScript script)    //todo make more efficient using ef core
    {
        try
        {
            Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(IsDuplicateScript), nameof(DbHelper));

            List<CustomerScript> allScripts = await GetAllCustomerScripts(includeCaches: true);

            foreach (var item in allScripts)
            {
                if (item.Id == script.Id
                    || item.ScriptName == script.ScriptName
                    || item.SourceCode == script.SourceCode
                    || Compiler.IsTheSameTree(item.SourceCode!, script.SourceCode!))
                {
                    return true;
                }
            }
            return false;
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            throw new DbHelperException(nameof(IsDuplicateScript) + " failed in " + nameof(DbHelper), e);
        }
    }
    public async Task<(List<Guid> scriptGUIDs, Dictionary<Guid, int> cacheGUIDs)> DetectDuplicates()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(DetectDuplicates), nameof(DbHelper));

        try
        {
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
                        Logger.LogError(e.ToString());
                        throw new Exception();
                    }

                }
            }
            return (scriptGUIDs: duplicateGuids, cacheGUIDs: cachesToDelete);
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            throw new DbHelperException(nameof(DetectDuplicates) + " failed in " + nameof(DbHelper), e);
        }
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

        try
        {
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
                        Logger.LogError(e.ToString());
                        Logger.LogError("Error when deleting index: " + i + " the GUID: " + duplicateGuids[i]);
                    }
                }
            }
            foreach (var item in cachesWithoutScript.Keys)
            {
                try { await DeleteScriptCache(item, cachesWithoutScript[item]); }
                catch (Exception e)
                {
                    Logger.LogError(e.ToString());
                    throw new Exception();
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            throw new DbHelperException(nameof(GetActiveApiVersions) + " failed in " + nameof(DbHelper), e);
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
}
