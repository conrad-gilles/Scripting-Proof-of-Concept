// using EFModeling.EntityProperties.FluentAPI.Required;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Ember.Scripting;
using Microsoft.Extensions.Logging;
using Microsoft.CodeAnalysis.Scripting;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Ember.Scripting.Compilation;

namespace Ember.Scripting.Persistence;

internal class ScriptRepository
{
    private readonly List<MetadataReference> _references;
    private readonly ScriptCompiler _compiler;
    private readonly ILogger<ScriptRepository> _logger;
    private readonly int _recentApiVersion;
    private readonly IDbContextFactory<ScriptDbContext> _contextFactory;
    private readonly IUserSession _userSession;
    public ScriptRepository(ScriptCompiler compiler, List<MetadataReference> references, ILogger<ScriptRepository> logger, int recentApiVersion,
    IDbContextFactory<ScriptDbContext> contextFactory, IUserSession userSession)
    {
        _references = references;
        _compiler = compiler;
        _logger = logger;
        _recentApiVersion = recentApiVersion;
        _contextFactory = contextFactory;
        _userSession = userSession;
    }
    public async Task DeleteAllData()
    {

        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(DeleteAllData), nameof(ScriptRepository));

        using (var db = await _contextFactory.CreateDbContextAsync())
        {
            await db.ScriptCompiledCaches.ExecuteDeleteAsync();
            await db.CustomerScripts.ExecuteDeleteAsync();
        }


    }
    public async Task<List<CustomerScript>> GetAllCustomerScripts(bool includeCaches = false, CustomerScriptFilter? filters = null)
    {

        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetAllCustomerScripts), nameof(ScriptRepository));

        if (filters != null)
        {
            using (var db = await _contextFactory.CreateDbContextAsync())
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
            using (var db = await _contextFactory.CreateDbContextAsync())
            {
                var scripts = await db.CustomerScripts.ToListAsync();
                return scripts;
            }
        }
        else
        {
            using (var db = await _contextFactory.CreateDbContextAsync())
            {
                var scripts = await db.CustomerScripts.Include(s => s.CompiledCaches).ToListAsync();
                return scripts;
            }
        }



    }
    public async Task<List<CompiledScript>> GetAllCompiledScriptCaches(bool includeScripts = true)
    {

        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetAllCompiledScriptCaches), nameof(ScriptRepository));
        if (!includeScripts)
        {
            using (var db = await _contextFactory.CreateDbContextAsync())
            {
                var caches = await db.ScriptCompiledCaches
                .ToListAsync();
                return caches;
            }
        }
        using (var db = await _contextFactory.CreateDbContextAsync())
        {
            var caches = await db.ScriptCompiledCaches
            .Include(c => c.CustomerScript)
            .ToListAsync();
            return caches;
        }


    }
    public int GetRecentApiVersion()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetRecentApiVersion), nameof(ScriptRepository));
        return _recentApiVersion;
    }
    public async Task ClearScriptCache(Guid scriptId)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(ClearScriptCache), nameof(ScriptRepository), scriptId);

        int currentApiVersion = GetRecentApiVersion();
        int highestV = currentApiVersion;
        foreach (var item in await GetActiveApiVersions())
        {
            if (item > highestV)
            {
                throw new ClearScriptCacheException(nameof(ClearScriptCache) + " failed in " + nameof(ScriptRepository) + " because for some reason the passed Recent Version is not the highest in the db.");
            }
        }
        for (int i = 0; i <= currentApiVersion; i++)    //checks above if current APi is highest
        {

            await DeleteScriptCache(scriptId, i);
            _logger.LogInformation("Deleted: " + i);
            // Console.WriteLine("Deleted: " + i);
        }

    }
    public async Task RecompileScript(Guid scriptId, bool deleteAlso = false)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(RecompileScript), nameof(ScriptRepository), scriptId);

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
                    await CreateAndInsertCompiledScript(script, itemN.ApiVersion);
                    _logger.LogInformation("Mock recompilation of old V" + itemN.ApiVersion);
                    // Console.WriteLine("Mock recompilation of old V" + itemN.ApiVersion);
                }
                catch (Exception e) { _logger.LogError("Compilation of old version failed" + e.ToString()); }

            }
            if (itemN.ApiVersion == currentApiVersion)  //this is only temporary until the if statement above works
            {
                await CreateAndInsertCompiledScript(script);
                _logger.LogInformation("Real recompilation of new V" + itemN.ApiVersion + " , normal if twice");
                // Console.WriteLine("Real recompilation of new V" + itemN.ApiVersion + " , normal if twice");
                break;
            }
        }


    }
    public async Task RecompileCache(Guid scriptId, int apiVersion)
    {
        await DeleteScriptCache(scriptId, apiVersion);
        CustomerScript script = await GetCustomerScript(scriptId);
        await CreateAndInsertCompiledScript(script, apiVersion);
    }
    public async Task<string> GetCompilationErrors(Guid scriptId, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(GetCompilationErrors), nameof(ScriptRepository), scriptId);

        try
        {
            CustomerScript script;
            string sourceCode;

            if (apiVersion == null)
            {
                script = await GetCustomerScript(scriptId);
                sourceCode = script.SourceCode!;
            }
            else
            {
                CompiledScript cache = await GetCompiledScripCache(scriptId, (int)apiVersion);
                sourceCode = cache.OldSourceCode!;
            }
            try
            {
                _compiler.BasicValidationBeforeCompiling(sourceCode);
            }
            catch (Exception e)
            {
                _logger.LogError("Validation in GetCompilationErrors failed but will still try to compile." + e.ToString());
            }

            _compiler.RunCompilation(sourceCode);

            return "Successful Compilation!";
        }
        catch (Exception e)
        {
            // return "Failed to compile script:" + scriptId + " " + e.ToString();
            return e.ToString();
            // throw new FacadeException(e.ToString(), e);
        }
    }
    public async Task<List<ScriptCompilationError>> GetCompilationErrors(string sourceCode)
    {
        ValidationRecord? metaData = null;
        try
        {
            try
            {
                metaData = _compiler.BasicValidationBeforeCompiling(sourceCode);
            }
            catch (Exception e)
            {
                _logger.LogError("Validation in GetCompilationErrors failed but will still try to compile." + e.ToString());
            }
            _compiler.RunCompilation(sourceCode);
            // throw new NoErrorsInScriptException("No error was in the Source Code.");
            List<ScriptCompilationError> returnList = [];
            return returnList;
        }
        catch (CompilationFailedException e)
        {
            return e.Errors;
        }
    }

    public async Task AutomaticCompilationOnVersionUpdate(int currentApiVersion)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with currentApiVersion: {ApiVersion}.", nameof(AutomaticCompilationOnVersionUpdate), nameof(ScriptRepository), currentApiVersion);

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
                    _logger.LogInformation("Deleted an old Script Cache!");
                    // Console.WriteLine("Deleted an old Script Cache!");
                }
            }

            if (currentImplementation == false)
            {
                await CreateAndInsertCompiledScript(item);
            }
        }
    }
    public async Task<CustomerScript> CreateAndInsertCustomerScript(string scriptString, Guid? randomGUID = null, int? oldApiV = null, DateTime? createdAt = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {ScriptId}.", nameof(CreateAndInsertCustomerScript), nameof(ScriptRepository), randomGUID);
        if (oldApiV == null)
        {
            oldApiV = GetRecentApiVersion();
        }
        if (randomGUID == null)
        {
            randomGUID = Guid.NewGuid();
        }
        using (var db = await _contextFactory.CreateDbContextAsync())
        {
            if (createdAt == null)
            {
                createdAt = DateTime.UtcNow;
            }
            var validationRecord = _compiler.BasicValidationBeforeCompiling(scriptString);



            string scriptType;
            if (validationRecord.ScriptType == typeof(IActionScript))
            {
                scriptType = nameof(IActionScript);
            }
            else if (validationRecord.ScriptType == typeof(IConditionScript))
            {
                scriptType = nameof(IConditionScript);
            }
            else
            {
                throw new CouldNotMatchBaseTypeInPersistence("No valid type");
            }

            CustomerScript script = new CustomerScript
            {
                Id = (Guid)randomGUID,
                ScriptName = validationRecord.ClassName,
                ScriptType = scriptType,
                SourceCode = scriptString,
                MinApiVersion = validationRecord.Version,
                CreatedAt = createdAt,
                ModifiedAt = DateTime.UtcNow,
                CreatedBy = _userSession.UserName,
                ExecutionTimeInMS = validationRecord.ExecutionTime
            };
            bool checkForDuplicates = true; //never set this to false!
            if (checkForDuplicates)
            {
                checkForDuplicates = await IsDuplicateScript(script);  //uncomment this if you dont want to check for duplicate existing in db when inserting
            }

            if (checkForDuplicates == false)
            {
                db.CustomerScripts.Add(script);
                await db.SaveChangesAsync();

                await CreateAndInsertCompiledScript(script, oldApiV);
                return script;
            }
            else
            {
                throw new CreateAndInsertCustomerScriptException(nameof(CreateAndInsertCustomerScript) + " failed in " + nameof(ScriptRepository) + " because a duplicate script was already in the database!");
            }
        }
    }
    public async Task CreateAndInsertCompiledScript(CustomerScript script, int? apiV = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {ScriptId}.", nameof(CreateAndInsertCompiledScript), nameof(ScriptRepository), script.Id);

        using (var db = await _contextFactory.CreateDbContextAsync())
        {
            // int currentApiVersion;
            if (apiV == null)
            {
                apiV = GetRecentApiVersion();
            }

            var getTupleFromVal = _compiler.BasicValidationBeforeCompiling(script.SourceCode!);

            if (await db.ScriptCompiledCaches.AnyAsync(c => c.ScriptId == script.Id && c.ApiVersion == apiV))
            {
                _logger.LogInformation("Skipping insert of: " + getTupleFromVal.ClassName + " because it already exists already exists.");
                // Console.WriteLine("Skipping insert of: " + getTupleFromVal.ClassName + " because it already exists already exists.");
                throw new CreateAndInsertCompiledScriptException(nameof(CreateAndInsertCompiledScript) + " failed in " + nameof(ScriptRepository) + "more details: " + "Skipping insert of: " + getTupleFromVal.ClassName + " because it already exists already exists."); ;
                // return;
            }
            else
            {
                byte[] tempComp;
                tempComp = _compiler.RunCompilation(script.SourceCode!);

                CompiledScript tempCache = new CompiledScript
                {
                    ScriptId = script.Id,
                    ApiVersion = (int)apiV,
                    AssemblyBytes = tempComp,
                    CompilationDate = DateTime.UtcNow,
                    CompilationSuccess = true,
                    CompilationErrors = "",
                    OldSourceCode = script.SourceCode,
                    CustomerScript = script
                };
                await InsertScriptCompiledCache(tempCache);
                await db.SaveChangesAsync();
            }
        }
    }
    public async Task DeleteCustomerScript(Guid id)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {ScriptId}.", nameof(DeleteCustomerScript), nameof(ScriptRepository), id);

        using (var db = await _contextFactory.CreateDbContextAsync())
        {
            CustomerScript temp = await GetCustomerScript(id);
            db.Remove(temp);
            await db.SaveChangesAsync();
        }
    }
    public async Task DeleteScriptCache(Guid id, int apiVersion)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {ScriptId} and ApiVersion: {ApiVersion}.", nameof(DeleteScriptCache), nameof(ScriptRepository), id, apiVersion);

        using (var db = await _contextFactory.CreateDbContextAsync())
        {
            try
            {
                CompiledScript temp = await GetCompiledScripCache(id, apiVersion);
                db.Remove(temp);
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                // Console.WriteLine(e.ToString());
                _logger.LogError(e.ToString());
                // Console.WriteLine("Could not execute DeleteScriptCache");
                _logger.LogInformation("Could not execute DeleteScriptCache");
            }
        }
    }
    public async Task UpdateScript(CustomerScript script, string newSourceCode, bool allowFaultySave = false, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(UpdateScript), nameof(ScriptRepository), script.Id);

        using (var db = await _contextFactory.CreateDbContextAsync())
        {
            CustomerScript? existingScript = await db.CustomerScripts.FindAsync(script.Id);

            if (existingScript == null)
            {
                _logger.LogDebug("Somethign went wrong retrieving your script, we could not find it.");
                throw new UpdateScriptException("Could not find the script what needs to be updated.");
            }

            if (allowFaultySave == false)
            {
                ValidationRecord validationRecord = _compiler.BasicValidationBeforeCompiling(newSourceCode);

                if (script.ScriptName != validationRecord.ClassName)
                {
                    existingScript.ScriptName = validationRecord.ClassName;
                }
                if (script.MinApiVersion != validationRecord.Version)
                {
                    existingScript.MinApiVersion = validationRecord.Version;
                }
                if (script.GetScriptType() != validationRecord.ScriptType)
                {
                    existingScript.ScriptType = validationRecord.BaseTypeAsString();
                }
            }

            existingScript.SourceCode = newSourceCode;
            existingScript.ModifiedAt = DateTime.UtcNow;

            existingScript.CreatedBy = _userSession.UserName;

            await db.SaveChangesAsync();
        }
    }
    // Goal is to compile the updated script first before insertion to prevent having to undo a insertion of a non working script
    public async Task UpdateScriptAndRecompile(Guid scriptId, string newSourceCode, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with scriptId: {ScriptId}.", nameof(UpdateScriptAndRecompile), nameof(ScriptRepository), scriptId);

        if (apiVersion == null)
        {
            apiVersion = GetRecentApiVersion();
        }

        using (var db = await _contextFactory.CreateDbContextAsync())
        {
            try
            {
                CustomerScript script = await GetCustomerScript(scriptId);
                var validationRecord = _compiler.BasicValidationBeforeCompiling(newSourceCode);

                if (await db.ScriptCompiledCaches.AnyAsync(c => c.ScriptId == script.Id && c.ApiVersion == apiVersion))
                {
                    await DeleteScriptCache(scriptId, (int)apiVersion);
                }
                byte[] compilation;
                compilation = _compiler.RunCompilation(newSourceCode);

                CompiledScript cache = new CompiledScript
                {
                    ScriptId = script.Id,
                    ApiVersion = (int)apiVersion,
                    AssemblyBytes = compilation,
                    CompilationDate = DateTime.UtcNow,
                    CompilationSuccess = true,
                    CompilationErrors = "",
                    // OldSourceCode = script.SourceCode
                    OldSourceCode = newSourceCode

                };
                await InsertScriptCompiledCache(cache);
                await db.SaveChangesAsync();
                await UpdateScript(script, newSourceCode, apiVersion: apiVersion);
            }
            catch (Exception e)
            {
                throw new CompilationOfUpdatedScriptException(message: "Failed to compile the updated script. Script source code was not updated.", e);
            }
        }
    }
    public async Task InsertScriptCompiledCache(CompiledScript scriptCompiledCache)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(InsertScriptCompiledCache), nameof(ScriptRepository));

        using (var db = await _contextFactory.CreateDbContextAsync())
        {
            scriptCompiledCache.CustomerScript = null!;

            db.ScriptCompiledCaches.Add(scriptCompiledCache);
            await db.SaveChangesAsync();
        }
    }

    public async Task<CompiledScript> GetCompiledScripCache(Guid id, int? apiVersion = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {ScriptId}.", nameof(GetCompiledScripCache), nameof(ScriptRepository), id);

        if (apiVersion == null)
        {
            apiVersion = GetRecentApiVersion();
        }
        using (var db = await _contextFactory.CreateDbContextAsync())
        {
            var cache = await db.ScriptCompiledCaches.Include(c => c.CustomerScript).SingleAsync(b => b.ScriptId == id && b.ApiVersion == apiVersion);
            return cache;
        }
    }
    public async Task<CustomerScript> GetCustomerScript(Guid id, bool includeCaches = false)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with ID: {ScriptId}.", nameof(GetCustomerScript), nameof(ScriptRepository), id);

        if (includeCaches == true)
        {
            using (var db = await _contextFactory.CreateDbContextAsync())
            {
                var script = await db.CustomerScripts.Include(s => s.CompiledCaches).SingleAsync(b => b.Id == id);
                return script;
            }
        }
        else
        {
            using (var db = await _contextFactory.CreateDbContextAsync())
            {
                var script = await db.CustomerScripts.SingleAsync(b => b.Id == id);
                return script;
            }
        }
    }

    public async Task<Guid> GetScriptId<ScriptType>(string scriptName)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName} with Name: {ScriptId}.", nameof(GetCustomerScript), nameof(ScriptRepository), scriptName);

        string sScriptType;
        if (typeof(ScriptType) == typeof(IActionScript))
        {
            sScriptType = nameof(IActionScript);
        }
        else if (typeof(ScriptType) == typeof(IConditionScript))
        {
            sScriptType = nameof(IConditionScript);
        }
        else
        {
            throw new GetScriptIdException(message: "Could not convert Script type enum to string");
        }

        // string sScriptType;
        // switch (scriptType)
        // {
        //     case ScriptTypes.GeneratorActionScript:
        //         // sScriptType = "IGeneratorActionScript";
        //         sScriptType = nameof(IGeneratorActionScript);
        //         break;
        //     case ScriptTypes.GeneratorConditionScript:
        //         sScriptType = nameof(IGeneratorConditionScript);
        //         break;
        //     default:
        //         throw new GetScriptIdException(message: "Could not convert Script type enum to string");
        // }
        using (var db = await _contextFactory.CreateDbContextAsync())
        {
            var script = await db.CustomerScripts.SingleAsync(b => b.ScriptName == scriptName && b.ScriptType == sScriptType);
            return script.Id;
        }
    }
    public async Task CompileAllStoredScripts()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(CompileAllStoredScripts), nameof(ScriptRepository));

        using (var db = await _contextFactory.CreateDbContextAsync())
        {
            List<CustomerScript> allScriptSC = await GetAllCustomerScripts();
            for (int i = 0; i < allScriptSC.Count(); i++)
            {
                await CreateAndInsertCompiledScript(allScriptSC[i]);
                await db.SaveChangesAsync();
            }
        }
    }

    public async Task SaveScriptWithoutCompiling(Guid id, string sourceCode)
    {
        using (var db = await _contextFactory.CreateDbContextAsync())
        {
            CustomerScript? existingScript = await db.CustomerScripts.FindAsync(id);

            if (existingScript != null)
            {
                existingScript.SourceCode = sourceCode;
                existingScript.ModifiedAt = DateTime.UtcNow;


                existingScript.CreatedBy = _userSession.UserName;

            }
            else
            {
                // CustomerScript testScript = new CustomerScript
                // {
                //     Id = id,
                //     ScriptName = null,
                //     ScriptType = null,
                //     SourceCode = sourceCode,
                //     MinApiVersion = GetRecentApiVersion(),
                //     CreatedAt = DateTime.UtcNow,
                //     ModifiedAt = DateTime.UtcNow,
                //     CreatedBy = null
                // };
                // db.CustomerScripts.Add(testScript);
                throw new SaveScriptWithoutCompilingException(message: "Script did not exist, therefore could not save.");

            }
            await db.SaveChangesAsync();
        }
    }
    public async Task CreateScriptWithoutCompiling(Guid id, string sourceCode, string? userName = null)
    {
        using (var db = await _contextFactory.CreateDbContextAsync())
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
                CreatedBy = userName,
                ExecutionTimeInMS = null
            };
            db.CustomerScripts.Add(testScript);
            await db.SaveChangesAsync();
        }
    }
    public async Task DeleteAllCachedScripts()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(DeleteAllCachedScripts), nameof(ScriptRepository));

        using (var db = await _contextFactory.CreateDbContextAsync())
        {
            await db.ScriptCompiledCaches.ExecuteDeleteAsync();
        }
    }
    public async Task<bool> IsDuplicateScript(CustomerScript script)    //todo make more efficient using ef core
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(IsDuplicateScript), nameof(ScriptRepository));
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
    public async Task<DuplicateListDbH> DetectDuplicates()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(DetectDuplicates), nameof(ScriptRepository));

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
                    || _compiler.IsTheSameTree(allScripts[i].SourceCode!, allScripts[j].SourceCode!)
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
        List<CompiledScript> allCaches = await GetAllCompiledScriptCaches();
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
                    _logger.LogError("Could not add cache to DB." + e.ToString());
                    throw new DetectDuplicatesException("Could not add cache to DB.", e);
                }

            }
        }
        // return (scriptGUIDs: duplicateGuids, cacheGUIDs: cachesToDelete);
        return new DuplicateListDbH
        {
            duplicateGuids = duplicateGuids,
            cachesToDelete = cachesToDelete
        };
    }
    public async Task<List<int>> GetActiveApiVersions()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetActiveApiVersions), nameof(ScriptRepository));

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
    public async Task RemoveDuplicates(List<Guid>? duplicateGuids = null, Dictionary<Guid, int>? cachesWithoutScript = null)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(RemoveDuplicates), nameof(ScriptRepository));

        duplicateGuids = (await DetectDuplicates()).duplicateGuids;
        cachesWithoutScript = (await DetectDuplicates()).cachesToDelete;

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
                    _logger.LogError("Error when deleting index: " + i + " the GUID: " + duplicateGuids[i] + ". Full Exception: " + e.ToString());
                }
            }
        }
        foreach (var item in cachesWithoutScript.Keys)
        {
            try { await DeleteScriptCache(item, cachesWithoutScript[item]); }
            catch (Exception e)
            {
                _logger.LogError("Failed to delete Script Cache." + e.ToString());
                throw new RemoveDuplicatesException("Failed to delete Script Cache.");
            }
        }
    }
    //Ai generated
    public async Task HealthCheck() //todo verify
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(HealthCheck), nameof(ScriptRepository));

        // ScriptCompiler compiler = new ScriptCompiler(References);

        //checking if can connect to db
        using (var context = await _contextFactory.CreateDbContextAsync())
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
        if (_compiler == null)
        {
            throw new InvalidOperationException("HealthCheck Failed: ScriptCompiler is not correctly initialized.");
        }
    }

    public async Task<Dictionary<int, List<CompiledScript>>> GetCachesForEachApiVersion()
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetActiveApiVersions), nameof(ScriptRepository));

        var caches = await GetAllCompiledScriptCaches();
        Dictionary<int, List<CompiledScript>> returnedDict = [];

        List<int> ls = [];
        foreach (var cache in caches)
        {
            if (returnedDict.Keys.Contains(cache.ApiVersion) == false)
            {
                List<CompiledScript> newList = [];
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

