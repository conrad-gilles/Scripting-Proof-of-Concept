# Todo List - Scripting Proof-of-Concept

> Extracted from `code_review_full.md`.  
> **P1** = critical correctness  
> **P2** = important improvement  
> **P3** = nice-to-have

---

## P1 - Critical

---

### 1. Fix assembly load leak `[Memory]`
**`ScriptExecutor.cs:38`**

Every call to `RunScriptExecution` loads a compiled assembly into the **default `AssemblyLoadContext`**. Assemblies in the default context can never be unloaded in .NET - each execution permanently leaks RAM. Under sustained load this will eventually cause an `OutOfMemoryException`.

```csharp
// current - leaks forever
Assembly assembly = Assembly.Load(compiledScript);
```

Fix: use a collectible context that is disposed after the call returns.

```csharp
using var context = new AssemblyLoadContext(name: null, isCollectible: true);
Assembly assembly = context.LoadFromStream(new MemoryStream(compiledScript));
// execute ...
// context disposed → assembly eligible for GC
```

---

### 2. Replace exception-as-control-flow cache miss `[Correctness]`
**`ScriptManagerBase.cs:174`**

`GetCompiledScripCache` calls `SingleAsync` internally, which throws `InvalidOperationException` when no row is found. The caller catches that exception to detect a cache miss and trigger JIT compilation. This is dangerous: **any** `InvalidOperationException` thrown anywhere inside the try block - from EF, from the compiler, from a downstream call - will silently trigger an unnecessary recompilation.

```csharp
// current - any InvalidOperationException triggers compile
try
{
    CompiledScript temp = await _db.GetCompiledScripCache(scriptId, apiVersion);
    ...
}
catch (InvalidOperationException)
{
    await CompileScript(scriptId, GetRunningApiVersion());
}
```

Fix: use `SingleOrDefaultAsync` and an explicit null check.

```csharp
CompiledScript? temp = await _db.TryGetCompiledScriptCache(scriptId, apiVersion);
if (temp == null)
{
    await CompileScript(scriptId, GetRunningApiVersion());
    temp = await _db.GetCompiledScripCache(scriptId, apiVersion);
}
```

---

### 3. `TryCompile` never returns `false` `[Correctness]`
**`ScriptManagerBase.cs:144`**

The method signature implies a safe non-throwing path, but the body throws on failure - `false` is unreachable.

```csharp
public async Task<bool> TryCompile(string script)
{
    _compiler.RunCompilation(script);  // throws CompilationFailedException on failure
    return true;                        // false is unreachable
}
```

Callers checking the return value will never see `false`. Fix: either wrap in try/catch and return false, or change the return type to `Task` and let the exception propagate - whichever matches how callers use it.

---

### 4. `RemoveDuplicates` inner check is always true `[Correctness]`
**`ScriptRepository.cs:758`**

```csharp
if (duplicateGuids.Contains(duplicateGuids[i]))  // always true - element is always in its own list
```

`duplicateGuids[i]` is by definition an element of `duplicateGuids`, so this guard never filters anything. The loop deletes every entry in the list, not just the actual duplicates. This bug was flagged in `code_review_student.md` and was not fixed.

---

### 5. Fix Captive Dependency `[Correctness]`
**`ScriptingServiceCollectionExtensions.cs`**

`IScriptManager` is registered as a **Singleton** that wraps `ScriptManagerBase`, which is registered as **Transient**. A Singleton that captures a Transient dependency holds it for the application lifetime - the Transient is effectively promoted to a Singleton, bypassing its intended short lifetime. If `ScriptManagerBase` ever holds scoped resources (e.g. a `DbContext`), this will cause subtle state corruption.

Fix: align lifetimes - register `ScriptManager` / `IScriptManager` as Transient, or make `ScriptManagerBase` a Singleton if its dependencies allow it.

---

## P2 - Important

---

### 6. Cache `UpgradeManager.GetClassDictionary()` `[Performance]`
**`UpgradeManager.cs:71`**

`GetClassDictionary()` scans **all loaded assemblies** via reflection to build a type-to-scanner map. It is called inside `CheckUpgradeResult`, which runs after **every script execution**. The assembly list does not change at runtime, so this scan is repeated work on every call.

```csharp
// fix: build once
private static readonly Lazy<Dictionary<Type, List<ScannerRecord>>> _classDict =
    new(() => BuildClassDictionary());
```

---

### 7. Cache version scanner results `[Performance]`
**`ContextVersionScanner.cs`, `ScriptVersionScanner.cs`**

Both scanners call `AppDomain.CurrentDomain.GetAssemblies()` and walk all types on every invocation, just like `GetClassDictionary` above. Their results are stable after startup. Apply the same `Lazy<T>` or static field caching. Note that these two scanners are nearly identical in structure - a generic base scanner parameterised on the version attribute type would remove the duplication (see also item 13).

---

### 8. Avoid parsing the syntax tree twice `[Performance]`
**`ScriptCompiler.cs:50, 107`**

`BasicValidationBeforeCompiling` and `RunCompilation` are always called together, and both call `CSharpSyntaxTree.ParseText(script)` independently. Roslyn parsing is not free - it allocates a full syntax tree. The `ValidationRecord` returned from `BasicValidationBeforeCompiling` could carry the `SyntaxTree`, allowing `RunCompilation` to reuse it instead of re-parsing.

---

### 9. Remove or implement stub interface methods `[Design]`
**`ScriptManagerBase.cs:281–303`**

Three methods sit on the public `IScriptManagerBaseExtended` interface with empty bodies:

```csharp
public void RegisterEmberInstance(...) { /* TODO */ }
public void GetScriptExecutionHistory(...) { /* TODO */ }
public void GetCompilationStatistics() { /* TODO */ }
```

Empty stubs on a public interface mislead callers into thinking the feature exists. Either implement them before the defence, or remove them from the interface and mark the concrete methods `private` until ready.

---

### 10. Move `IScriptManager` to its own file `[Design]`
**`ScriptManager.cs`**

`IScriptManager` is declared in the same file as `ScriptManager`. C# convention is one public type per file. Moving the interface to `IScriptManager.cs` makes it easier to find and signals clearly that it is a stable contract, not an implementation detail.

---

### 11. `ScriptFacades.cs` creates `ScriptManager` via `new` `[Design]`
**`ScriptFacades.cs:11`**

```csharp
public ActionScript(IScriptManagerBaseExtended scriptManager, string scriptName)
{
    _emberScriptManager = new ScriptManager(scriptManager);  // bypasses DI
}
```

`ScriptManager` is being newed up inside a facade that is itself created by `ScriptManager.GetScript`. Every `GetScript` call therefore creates a second `ScriptManager`, triggering another full `AppDomain` type-map reflection scan. The facade should receive `IScriptManager` as an injected dependency rather than constructing one internally.

---

### 12. Replace hard casts in context downgrade with `as` + null check `[Design]`
**`GeneratorContexts.cs:139`**

```csharp
ILabOrderInterface labOrderV1 = (ILabOrderInterface)LabOrder;  // throws InvalidCastException on mismatch
Patient patientV1 = (Patient)Patient;
```

The `catch (Exception e)` below these casts swallows the `InvalidCastException` and rethrows with a generic message, losing the specific type information that would identify *which* cast failed. Using `as` with an explicit null check preserves the diagnostic detail:

```csharp
var labOrderV1 = LabOrder as ILabOrderInterface
    ?? throw new DowngradeException("LabOrder does not implement ILabOrderInterface");
```

---

### 13. Genericise `ContextVersionScanner` and `ScriptVersionScanner` `[Design]`
**`ContextVersionScanner.cs`, `ScriptVersionScanner.cs`**

Both scanners implement almost identical assembly-scan logic, differing only in the attribute type they look for. A generic base class parameterised on the attribute type would eliminate the structural duplication and make adding future scanner types trivial. This is also noted in `Todo.md`.

---

### 14. Add null check before force-unwrap in `GetVersionInt` `[Correctness]`
**`ScriptCompiler.cs:222`**

```csharp
return (int)ctxVersion!;  // NullReferenceException if no context matched
```

If no matching context version is found, `ctxVersion` is null and this line throws `NullReferenceException` rather than the intended `VersionIntNotAssignedException`. The null check that should guard this is missing:

```csharp
if (ctxVersion == null)
    throw new VersionIntNotAssignedException("No context version found for ...");
return ctxVersion.Value;
```

---

### 15. Un-comment or remove duplicate detection throws in `ScriptVersionScanner` `[Correctness]`
**`ScriptVersionScanner.cs:105`**

```csharp
if (records.Any(r => r.RetrievedType == currentType))
{
    // throw new TypeMoreThanOnceInAssemblySVSException(...)
}
var existingVersionRecord = records.FirstOrDefault(r => r.Version == version);
if (existingVersionRecord != null)
{
    // throw new VersionIntMoreThanOnceInAssemblySVSException(...)
}
```

Both duplicate-detection throws are commented out. If two interfaces share the same version attribute, the scanner silently proceeds with the duplicate - the version map is corrupt and the error surfaces later in a confusing way. These should either throw or at minimum log a warning.

---

### 16. `AllowOnlyRecentTypes` is commented out - decide and act `[Correctness]`
**`ScriptCompiler.cs:153`**

```csharp
// ValiHelper.ValidateOnlyUseRecentTypes(methods, parentSymbol.ToDisplayString(), _recentTypes);
```

If this check is intentional security (preventing scripts from implementing old interface versions), it should be uncommented and tested. If it was abandoned, the dead `AllowOnlyRecentTypes` class and the `_recentTypes` field should be removed. Leaving it commented in the middle of validation logic is the worst of both options.

---

### 17. Simplify `GetAllCustomerScripts` - three near-identical code paths `[Code quality]`
**`ScriptRepository.cs:38`**

The method has three separate `if/else` branches that each build a slightly different EF query. The filter path already handles `includeCaches`, making the non-filter paths below it redundant. All three can be collapsed into one composable query:

```csharp
IQueryable<CustomerScript> query = db.CustomerScripts;
if (includeCaches) query = query.Include(s => s.CompiledCaches);
if (filters?.ScriptName != null) query = query.Where(s => s.ScriptName == filters.ScriptName);
// add further filter conditions ...
return await query.ToListAsync();
```

---

### 18. Fix `RemoveDuplicates` double `DetectDuplicates()` call `[Code quality]`
**`ScriptRepository.cs:754`**

```csharp
duplicateGuids       = (await DetectDuplicates()).duplicateGuids;
cachesWithoutScript  = (await DetectDuplicates()).cachesToDelete;
```

Two full database round-trips execute the same query. Store the result once:

```csharp
var result = await DetectDuplicates();
duplicateGuids      = result.duplicateGuids;
cachesWithoutScript = result.cachesToDelete;
```

---

### 19. Remove redundant `SaveChangesAsync` in `CompileAllStoredScripts` `[Code quality]`
**`ScriptRepository.cs:577`**

```csharp
using (var db = ...)
{
    for (...)
    {
        await CreateAndInsertCompiledScript(allScriptSC[i]);
        await db.SaveChangesAsync();  // no-op - this db context was never written to
    }
}
```

`CreateAndInsertCompiledScript` creates its own `DbContext` internally and calls `SaveChangesAsync` there. The outer context `db` is never modified, so the outer `SaveChangesAsync` is a wasted round-trip that misleads the reader.

---

### 20. Rename reused `checkForDuplicates` variable `[Code quality]`
**`ScriptRepository.cs:328`**

```csharp
bool checkForDuplicates = true;             // means "should we check?"
if (checkForDuplicates)
    checkForDuplicates = await IsDuplicateScript(script);  // now means "is it a duplicate?"
if (checkForDuplicates == false) { ... }    // "not a duplicate → insert"
```

The variable is reused to mean two different things in the same scope, making the intent hard to follow. Replace with:

```csharp
bool isDuplicate = await IsDuplicateScript(script);
if (!isDuplicate) { ... }
```

---

### 21. Fix `ClearScriptCache` - query existing versions instead of iterating 0..N `[Code quality]`
**`ScriptRepository.cs:145`**

```csharp
for (int i = 0; i <= currentApiVersion; i++)
{
    await DeleteScriptCache(scriptId, i);  // most of these version slots don't exist
}
```

This fires one `DELETE` attempt per integer from 0 to the current API version, most of which will find nothing. `DeleteScriptCache` silently swallows the resulting exceptions, so it works - but it's O(apiVersion) unnecessary DB calls. Fix: query the cache versions that actually exist for this script and delete only those.

---

### 22. Remove stale line-number in log message `[Code quality]`
**`ScriptExecutor.cs:96`**

```csharp
logger.LogInformation($"Result in 86 sExecuter: {resultValue}");
```

`"86 sExecuter"` is a hardcoded line number left over from debugging. It is already wrong and will drift further with every edit. Remove it.

---

### 23. Fix redundant third condition in `ExecutionTime.GetSafeDuration` `[Code quality]`
**`CompilerRecords.cs:67`**

```csharp
if (result != MinimumDuration && result != MaximumDuration)
{
    result = time;  // result is already equal to time at this point
}
```

After the two clamp checks above, `result` is either `MinimumDuration`, `MaximumDuration`, or still the original `time`. The third branch reassigns `result = time` when `result` is already `time` - it is a no-op that confuses the reader. Remove it.

---

### 24. Fix test interdependency in `Crud.cs` `[Tests]`
**`Crud.cs:63`**

```csharp
[TestMethod]
public async Task Read()
{
    await Create();  // calls another test method directly
```

`Read`, `Update`, `Delete`, and `Execute` all call `Create()` to set up their precondition. If `Create` has a bug, every downstream test fails for the wrong reason - a cascade of false failures. Extract the shared setup into a private helper or a `[TestInitialize]` method and call that instead.

---

### 25. Fix double `InitScriptManager()` call at class-load time `[Tests]`
**`Crud.cs:14`**

```csharp
static IScriptManagerBaseExtended _scriptManagerBase = InitScriptManager().Item2;
static IScriptManager _scriptManager = InitScriptManager().Item1;
```

`InitScriptManager()` is called **twice**, creating two separate service providers and two separate database connections (or two in-memory databases). Initialize once:

```csharp
static (IScriptManager sm, IScriptManagerBaseExtended smb) = InitScriptManager();
static IScriptManager _scriptManager = sm;
static IScriptManagerBaseExtended _scriptManagerBase = smb;
```

---

## P3 - Nice-to-have

---

### 27. Document loop-cancellation bypass as a known limitation `[Security]`
**`ScriptCompiler.cs:400`**

The loop cancellation check looks for the literal string `ScriptEnvironment.CurrentToken.Value.ThrowIfCancellationRequested` in the loop body - it can be trivially bypassed by putting the infinite loop in a helper method. The check is marked as AI-generated. It is worth noting in the thesis that this prevents *accidental* infinite loops by well-intentioned developers, not adversarial bypass.

---

### 28. Replace hardcoded method rename with a configurable rename table `[Design]`
**`ScriptFacades.cs:44`**

```csharp
if (script.ScriptApiVersion == 3)
{
    methodName = "Execute1OldName";
}
```

Method renames between API versions are hardcoded with a magic version number. This does not scale beyond a single rename. A proper solution would store rename mappings as part of the script metadata or in the versioning attributes so the framework can look them up rather than hardcoding them.

---

### 29. Clarify `ActionResultV3.FailedOrNot` naming convention `[Design]`
**`ActionResult.cs:111`**

V1 and V2 use `IsSuccess = true` to indicate success. V3 uses `FailedOrNot = true` - also meaning success, but with a name that implies failure. The inverted convention between versions is easy to misread. Also, when upgrading V2 → V3, the `ErrorCode` field is concatenated into the message string and its semantic distinction is lost; this should be documented as intentional if V3 genuinely drops error codes.

---

### 30. Consolidate per-version file namespaces in Ember `[Design]`
**`Ember/ScriptingFramework/`**

Each context version lives in its own file-level namespace (`IGeneratorReadOnlyContextV1`, `IGeneratorContext_V2`, etc.). The intent is to prevent accidental cross-version coupling, but the side effect is that every type must be referenced by its file-namespace qualifier, and the version chain is very hard to follow. A single `Ember.Scripting.Contexts` namespace with explicitly named version types (e.g. `IGeneratorContextV4`) would be easier to navigate without sacrificing isolation.

---

### 31. Move connection string to environment variables / `appsettings.json` `[Code quality]`
**`ScriptDbContext.cs`**

The database connection string is hardcoded. This must be externalised (environment variable or secrets manager) before the project is deployed anywhere beyond a developer machine.

---

### 32. Replace string-concat log calls with structured logging `[Code quality]`
**Throughout**

```csharp
_logger.LogError("Error: " + e.ToString());  // current
_logger.LogError(e, "Error occurred");        // correct
```

Passing the exception as the first argument preserves the full stack trace in structured log sinks (Seq, Application Insights, etc.) and avoids unnecessary string allocation on every log call.

---

### 33. Remove remaining `Console.WriteLine` calls `[Code quality]`
**Throughout**

Several methods contain `Console.WriteLine` calls (many already commented out). These should be removed entirely - in production code diagnostic output belongs in the `ILogger` pipeline, not stdout.

---

### 34. Fix identifier typos `[Code quality]`
**Multiple files**

| Typo | Correct |
|---|---|
| `ConcellationTokenUncheckedException` | `CancellationTokenUncheckedException` |
| `maxScriptLenght` | `maxScriptLength` |
| `foeach` (in exception message) | `foreach` |

---

### 35. `ParentSymbol` should be a property, not a public field `[Code quality]`
**`CompilerRecords.cs:14`**

```csharp
public string ParentSymbol = "Default";  // public field on a record
```

Public fields on records bypass property semantics (no change notification, no interface implementation, no future backing logic). Change to `public string ParentSymbol { get; init; } = "Default";`.

---

### 36. Rename `methods` field to `Methods` (Pascal case) `[Code quality]`
**`CompilerRecords.cs:13`**

```csharp
public List<MethodRecord>? methods  // violates C# naming conventions for public members
```

Public members must be PascalCase. Rename to `Methods`.

---

### 37. Delete `GetReferencesForOldVersion` `[Code quality]`
**`ScriptCompiler.cs:503`**

The method is never called anywhere in the codebase. Its premise is also unnecessary in this design: old versioned types (`GeneratorContextV1` through `GeneratorContextVN`) are never deleted from the Ember assembly, so the current runtime references already contain all the types any script version needs. The method was likely an artefact of an earlier design before the "never delete old versions" policy was established.
