using EFModeling.EntityProperties.FluentAPI.Required;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Ember.Scripting;
using Microsoft.Extensions.Logging;
namespace Ember.Scripting;

internal class DbHelper
{
    private readonly MetadataReference[] References;
    private readonly ScriptCompiler Compiler;
    private readonly ILogger<DbHelper> Logger;
    public DbHelper(ScriptCompiler compiler, MetadataReference[] references, ILogger<DbHelper> logger)
    {
        References = references;
        Compiler = compiler;
        Logger = logger;
        // Compiler = new ScriptCompiler(References);
    }
    public async Task EnsureDeletedCreated()    //todo delete this for obvious safety reasons before production
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(EnsureDeletedCreated), nameof(DbHelper));

        using (var db = new MyContext())
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
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
                // Console.WriteLine("Db Helper getAllCustomerScripts executed!");
                return scripts;
            }
        }
        else
        {
            using (var db = new MyContext())
            {
                var scripts = await db.CustomerScripts.Include(s => s.CompiledCaches).ToListAsync();
                // Console.WriteLine("Db Helper getAllCustomerScripts executed!");
                return scripts;
            }
        }


    }
    public async Task<List<ScriptCompiledCache>> GetAllCompiledScriptCaches()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetAllCompiledScriptCaches), nameof(DbHelper));

        using (var db = new MyContext())
        {
            var caches = await db.ScriptCompiledCaches
            .Include(c => c.CustomerScript) //added might be bad for performance
            .ToListAsync();
            // Console.WriteLine("Db Helper getAllCompiledScriptCaches executed!");
            return caches;
        }
    }
    public async Task<int> GetRecentApiVersion() //todo implement
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetRecentApiVersion), nameof(DbHelper));

        return 6;
    }
    public async Task ClearScriptCache(Guid scriptId)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(ClearScriptCache), nameof(DbHelper), scriptId);

        int currentApiVersion = await GetRecentApiVersion();
        for (int i = 0; i <= currentApiVersion; i++)    //this only works if currentApiVersion is the highest
        {
            try
            {
                await DeleteScriptCache(scriptId, i);
                Logger.LogInformation("Deleted: " + i);
                Console.WriteLine("Deleted: " + i);
            }   //todo check if doesnt throw something
            catch (Exception e)
            { Logger.LogError(e.ToString()); }

        }

    }
    public async Task RecompileScript(Guid scriptId, bool deleteAlso = false)  //todo maybe get rid of the currentapi and create global var in dbhelper class
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(RecompileScript), nameof(DbHelper), scriptId);

        try
        {
            int currentApiVersion = await GetRecentApiVersion();
            CustomerScript script = await GetCustomerScript(scriptId, includeCaches: true);  //maybe this is inefficient better to pass object maybe

            int start = script.MinApiVersion;
            int count = currentApiVersion - start + 1;  //check if correct
            int[] versions = Enumerable.Range(start, count).ToArray();
            if (deleteAlso)
            {
                await ClearScriptCache(scriptId);
            }
            foreach (var itemN in script.CompiledCaches)
            {
                if (versions.Contains(itemN.ApiVersion) == false)
                {
                    try
                    { // await DeleteScriptCache(scriptId, itemN.ApiVersion);
                        await CreateAndInsertCompiledCache(script, itemN.ApiVersion); //todo error check this
                        Logger.LogInformation("Mock recompilation of old V" + itemN.ApiVersion);
                        Console.WriteLine("Mock recompilation of old V" + itemN.ApiVersion);
                    }
                    catch (Exception e) { Logger.LogError("Compilation of old version failed" + e.ToString()); }

                }
                if (itemN.ApiVersion == currentApiVersion)  //this is only temporary until the if statement above works
                {
                    // await DeleteScriptCache(scriptId, currentApiVersion);
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
            throw new Exception();
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
                    int count = currentApiVersion - start + 1;  //check if correct

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
            throw new Exception();
        }

    }
    public async Task<CustomerScript> CreateAndInsertCustomerScript(string scriptString, Guid randomGUID, string createdBy, int oldApiV = -1, DateTime? createdAt = null)
    //todo make sure compiling happens after verification of isDuplicate
    //todo remove currentapi
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {ScriptId}.", nameof(CreateAndInsertCustomerScript), nameof(DbHelper), randomGUID);


        using (var db = new MyContext())
        {
            int currentApiVersion = await GetRecentApiVersion();
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

            bool testBool = false;      // try ensure compilat not added twice
            // testBool = await IsDuplicateScript(randomTestScript2);  //uncomment this if you dont want to check for duplicate existing in db when inserting


            if (testBool == false)
            {
                byte[] tempComp;
                if (oldApiV != -1)
                {
                    // Console.WriteLine("trying to add refs in dbhelper");
                    // MetadataReference[] refs = compiler.GetReferencesForVersion(oldApiV);
                    // Console.WriteLine("Added refs in DbHelper!");
                    tempComp = Compiler.RunCompilation(scriptString, apiVersion: oldApiV, metaData: getTupleFromVal);
                    // Console.WriteLine("Comp ran in db helper with refs!");
                    currentApiVersion = oldApiV;
                }
                else
                {
                    currentApiVersion = await GetRecentApiVersion();
                    tempComp = Compiler.RunCompilation(scriptString, metaData: getTupleFromVal);
                }

                randomTestScript2.CompiledCaches.Add(new ScriptCompiledCache
                {
                    ScriptId = randomGUID,
                    ApiVersion = currentApiVersion,    //todo figure out how to do this
                    AssemblyBytes = tempComp,
                    CompilationDate = DateTime.UtcNow,
                    CompilationSuccess = true,
                    CompilationErrors = ""
                });
                db.CustomerScripts.Add(randomTestScript2);
                await db.SaveChangesAsync();
                // Console.WriteLine("Db Helper CreateAndInsertCustomerScript executed!");
                return randomTestScript2;
            }
            else
            {
                return randomTestScript2;   //todo maybe throw error idk
            }
        }
    }
    public async Task CreateAndInsertCompiledCache(CustomerScript script, int oldApiV = -1)   //could change to take Guid instead
    //remove currenetAPivers, by adding version detection in .compile function in compiler, so you just give version, myabe detection?
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {ScriptId}.", nameof(CreateAndInsertCompiledCache), nameof(DbHelper), script.Id);

        using (var db = new MyContext())
        {
            int currentApiVersion = await GetRecentApiVersion();
            // ScriptCompiler compiler = new ScriptCompiler(References);
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
                    MetadataReference[] refs = Compiler.GetReferencesForVersion(oldApiV);
                    tempComp = Compiler.RunCompilation(script.SourceCode!, refs, metaData: getTupleFromVal);
                    currentApiVersion = oldApiV;
                }
                else
                {
                    tempComp = Compiler.RunCompilation(script.SourceCode!, metaData: getTupleFromVal);
                }
                // byte[] tempComp = compiler.RunCompilation(script.SourceCode);
                ScriptCompiledCache tempCache = new ScriptCompiledCache
                {
                    ScriptId = script.Id,
                    ApiVersion = currentApiVersion,
                    AssemblyBytes = tempComp,
                    CompilationDate = DateTime.UtcNow,
                    CompilationSuccess = true,
                    CompilationErrors = ""
                };
                // Console.WriteLine("API Version Compiled Cache: " + getTupleFromVal.versionInt);
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
            // Console.WriteLine("Db Helper deleteCustomerScript executed!");
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
                // Console.WriteLine("Db Helper deleteScriptCache executed!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("Could not execute DeleteScriptCache");
            }

        }
    }
    public async Task UpdateCustomerScript(CustomerScript customerScript)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(UpdateCustomerScript), nameof(DbHelper));

        using (var db = new MyContext())
        {
            //todo
        }
    }
    public async Task InsertScriptCompiledCache(ScriptCompiledCache scriptCompiledCache)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(InsertScriptCompiledCache), nameof(DbHelper));

        using (var db = new MyContext())
        {
            // Option 1: If scriptCompiledCache.ScriptId is already set correctly
            // Just add it directly. EF Core will see it has a foreign key and insert it.
            // Ensure scriptCompiledCache.CustomerScript is NULL to avoid EF trying to re-insert the parent.
            scriptCompiledCache.CustomerScript = null!;

            db.ScriptCompiledCaches.Add(scriptCompiledCache);
            await db.SaveChangesAsync();
            // Console.WriteLine("Db Helper insertScriptCompiledCache executed!");
        }

    }

    public async Task<ScriptCompiledCache> GetCompiledScripCache(Guid id, int ApiVersion)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {ScriptId}.", nameof(GetCompiledScripCache), nameof(DbHelper), id);

        using (var db = new MyContext())
        {
            // Console.WriteLine("GetCompiledScriptCacheReached");
            var cache = await db.ScriptCompiledCaches.SingleAsync(b => b.ScriptId == id && b.ApiVersion == ApiVersion);
            // Console.WriteLine("Db Helper GetCompiledScriptCache executed!");
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
                // Console.WriteLine("Db Helper GetCustomerScript executed!");
                return script;
            }
        }
        else
        {
            using (var db = new MyContext())
            {

                var script = await db.CustomerScripts.SingleAsync(b => b.Id == id);
                // Console.WriteLine("Db Helper GetCustomerScript executed!");
                return script;
            }
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
                    // Console.WriteLine("DbHelper CompileAllStoredScripts Executed!");
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
        }
    }
    public async Task DeleteAllCachedScripts()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(DeleteAllCachedScripts), nameof(DbHelper));

        try
        {
            using (var db = new MyContext())
            {
                List<ScriptCompiledCache> allCaches = await GetAllCompiledScriptCaches();
                for (int i = 0; i < allCaches.Count(); i++) //this could lead to a bottleneck on large datasets
                {
                    db.Remove(allCaches[i]);
                }
                await db.SaveChangesAsync();
                // Console.WriteLine("Db Helper DeleteAllCachedScripts executed!");
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
        }
    }
    public async Task<bool> IsDuplicateScript(CustomerScript script)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(IsDuplicateScript), nameof(DbHelper));

        List<CustomerScript> allScripts = await GetAllCustomerScripts(includeCaches: true);
        // ScriptCompiler compiler = new ScriptCompiler(References);
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
    public async Task<(List<Guid> scriptGUIDs, Dictionary<Guid, int> cacheGUIDs)> DetectDuplicates()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(DetectDuplicates), nameof(DbHelper));

        // bool isDuplicate = false;
        // if (script == null)
        // {

        // }
        try
        {
            List<CustomerScript> allScripts = await GetAllCustomerScripts(includeCaches: true);
            List<Guid> duplicateGuids = [];
            // ScriptCompiler compiler = new ScriptCompiler(References);
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
            throw new Exception();
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
    public async Task RemoveDuplicates()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(RemoveDuplicates), nameof(DbHelper));

        try
        {
            List<Guid> duplicateGuids = (await DetectDuplicates()).scriptGUIDs;
            Dictionary<Guid, int> cachesWithoutScript = (await DetectDuplicates()).cacheGUIDs;

            for (int i = 0; i < duplicateGuids.Count(); i++)
            {
                // Console.WriteLine("if 4 reached");
                if (duplicateGuids.Contains(duplicateGuids[i]))
                {
                    try
                    {
                        await DeleteCustomerScript(duplicateGuids[i]);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError(e.ToString());
                        Logger.LogError("Error when deleting index: " + i + " the GUID: " + duplicateGuids[i]);   //todo fix why it throws this error all the time but still works?
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
            throw new Exception();
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
            int currentApiVersion = await GetRecentApiVersion();
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
