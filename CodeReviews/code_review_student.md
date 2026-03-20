# Code Review & Refactoring Notes

## General Comments

### 1. The `EmberInstances` Class/Table
**Comment:** What is the use of the `EmberInstances` class/table? Is this because it was listed in the confluence page? I don't see a reason for this DB table or class.

**Answer:** I added it because of the confluence page, but I haven't written any real logic to support it yet, so removing it is no issue at all. I think the thought behind it was having some form of record/table to store information about running ember instances in the environment (Test (T), Acceptance (A), and Production (P))? Although if you asked me it would make more sense to just retrieve this data as the program is running by calling a method from the instance itself maybe?

### 2. The `DataProtectionKeys` DbSet
**Comment:** What is the use of `public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;` in `MyContext`?

**Answer:** I added it for the Blazor app to enhance performance (when running the app in a Docker container). Will probably remove or fix it in the future since it is not working currently.

### 3. Naming Conventions
**Comment:** General naming conventions, private props, private fields, etc.

**Answer:** I tried to rename all to fit the conventions.

### 4. Renaming `CompiledScriptCache`
**Comment:** Maybe we should rename `CompiledScriptCache` to simply `CompiledScripts` because "Cache" implies a cache, which it is not.

**Answer:** Renamed both the C# class and the table in PostgreSQL.

### 5. Return Types
**Comment:** In general do not use tuples for return types. Use records or classes to encapsulate the data.

**Answer:** Changed all to records.

### 6. Empty Lines
**Comment:** Remove subsequent empty lines.

**Answer:** *Todo*

### 7. Private Fields Naming
**Comment:** Use `_camelCase` for all private fields — eliminates the biggest source of visual confusion.

**Answer:** *Todo*

### 8. Method Parameters Naming
**Comment:** Use `camelCase` for all method parameters. You sometimes used Pascal case.

**Answer:** *Todo*

### 9. Test-Only Methods
**Comment:** The classes like `ScriptManger`, `ScriptExecutor` should never contain methods that are only there for testing. (example: `ValidateScript` and `GetCompilationErrors` in `ScriptManger.cs`).

**Answer:** *Todo*

### 10. Class & Interface Naming
**Comment:** Better naming of Classes etc. I got confused quickly as some classes/interfaces are name the same or similar.

**Answer:** *Todo*

### 11. Folder Structure
**Comment:** In .NET projects you do not add another `src` folder inside the projects. For example: `Scripting/src` is wrong. All your namespaces/folders simply live in the root of the project.

**Answer:** *Todo*

---

## ScriptExecutor.cs

### 1. Task Timeout Handling
**Comment:** Task is not killed if it exceeds the timeout. You do `resultTask.Wait(ScriptTimeout)`. But then you need to kill the task if it exceeds the timeout. 
*See: [CancellationTokenSource Docs](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtokensource?view=net-10.0)*
Also use `WaitAsync` and not `Wait` (as this can deadlock in async contexts).

**Answer:** Fixed this using the `CancellationTokenSource`, but the creator of the script always has to add the line to check if it was cancelled to each body of a loop in his script. Maybe in the future I can add a function that adds it automatically if it wasn't there. I also made the methods async and added the `WaitAsync`.

---

## ScriptCompiler.cs

### 1. Duplicate References
**Comment:** Every time `RunCompilation` is called you add `StandardRefrencesForAllScripts` to `ReferencesRO`. So if I call it 100 times it will contain the same references 100 times.

**Answer:** I put the `AddRange(StandardRefrencesForAllScripts)` into the constructor which is only being called via DI.

### 2. Duplicate Code
**Comment:** Duplicate code between `BasicValidationBeforeCompiling` and `GetBaseType` (ScriptCompiler.cs:93-109 and 111-211).

**Answer:** I got rid of it by having `GetBaseType` return a more complex record, it works but is a bit overcomplex still.

### 3. Old Version References
**Comment:** `GetReferencesForOldVersion` should get the references from a folder.

**Question:** So instead of from current runtime, from a folder containing the DLLs?

### 4. `ValidateScript` Purpose
**Comment:** What purpose does `ValidateScript` offer? We do have `BasicValidationBeforeCompiling`. Is it only for testing? If so, you can use `Assert.ThrowsException`. We should not have methods in our classes that serve only unit tests.

**Answer:** I got rid of it and replaced all occurrences of it with `BasicValidationBeforeCompiling`.

---

## DbHelper.cs

### 1. `CreateAndInsertCustomerScript` Parameters
**Comment:** `alsoCompAndSave` parameter is never used. `checkForDuplicates` is always set to true in line 281. So an outside caller can never skip the duplicate check. Another question is: does it even make sense to ever skip the duplicate check? Not really, or do you have a use case?

**Answer:** Got rid of the unused parameter. I only used the `checkForDuplicates` flag for testing, in a real-world scenario you would always check, I believe.

### 2. `RemoveDuplicates` Bug
**Comment:** `RemoveDuplicates` has a bug. You loop over `duplicateGuids` and then check if `duplicateGuids` contains the element at index `i`; this is always true of course. So you basically delete everything. Do we even need such a method?

**Answer:** If we decide that we always must check for duplicates when inserting or updating, this function would be unnecessary. Also it is hard to test since you would have to circumvent the duplicate check when inserting. I didn't try to fix the bug yet since I can't really test it right now, if we decide to keep the function I will of course fix and test it.

### 3. DbContext Instantiation
**Comment:** Do not use `using (var db = new MyContext())`. Use dependency injection and `DbContextFactory`. 
*See: [DbContext Configuration](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#use-a-dbcontext-factory)*

**Answer:** Done, but I kept the connection string hardcoded in the `MyContext` class for simplicity.

### 4. Magic Strings
**Comment:** In method `GetScriptId` do not use magic strings. Use `nameof(IGeneratorActionScript)`.

**Answer:** Done.

### 5. `SaveScriptWithoutCompiling`
**Comment:** `SaveScriptWithoutCompiling` throws an exception in case the script cannot be found instead of creating a new instance in the `else` case.

**Answer:** Done, and created a new method below it for `CreateScriptWithoutCompiling`.

### 6. Console Logging
**Comment:** You still have a bunch of `Console.WriteLine` statements that need to be removed.

**Answer:** I commented them all out in the `DbHelper` class.

---

## ScriptManager.cs

### 1. `CheckVersionCompatibility`
**Comment:** `CheckVersionCompatibility` simply checks all script versions. This is because it internally calls `GetActiveApiVersions` which calls `GetAllCompiledScriptCaches`. It does not check if the current script is compatible with the `targetApiVersion`.

**Question:** Should it check if a cache of the current Ember API version is inside the DB? If yes, compile so it is compatible? Or by compatibility do you mean the current active context is downgradeable to execute the script?

### 2. `UpdateScript` Compilation
**Comment:** Maybe I am missing something but `UpdateScript` does not actually compile the script it just updates the source code.

**Answer:** I removed the part in the description that says it also recompiles. Or should I instead make it also compile after updating?

### 3. Magic Strings
**Comment:** In method `CreateScriptUsingNameType` do not use magic strings. Use `nameof(IGeneratorActionScript)`.

**Answer:** Done.

### 4. Mock Service for Username
**Comment:** Create a mock service that provides you the user name for `GetUserName`.

**Question:** So inject the username by DI, if yes what if user changes can you reinject?

### 5. `GetActiveApiVersions`
**Comment:** What is the use case for `GetActiveApiVersions`?

**Answer:** No real use case right now but could be informational in the future? But maybe I should just remove it for now.

### 6. Compilation on Version Update
**Comment:** In `CleanupOrphanedCaches` you call `AutomaticCompilationOnVersionUpdate`. Seems like the wrong method no? Also is `AutomaticCompilationOnVersionUpdate` even needed? We will never recompile all our scripts simply because Ember has a new API version. Or am I missing something?

**Answer:** Yes I have not written an implementation for it yet, should I write one? I don't know if it is worth investing time because normally when you delete a script all associated caches should get deleted too. I believe the idea behind `AutomaticCompilationOnVersionUpdate` was that if there are major changes in Ember, the already compiled scripts will fail on recompilation rather than failing at execution when it might be time-critical. This also brings up the question of whether one should implement a way of automatically refactoring the scripts somehow.

### 7. `GetCompilationErrors` Location
**Comment:** `GetCompilationErrors` is a method that is only used for testing. It should therefore not be part of the `ScriptManager`.

**Question:** Should I keep it in the `ScriptManager` class and remove it from the interface? Currently I am not using it anywhere except tests, but my idea was using it in the UI to pass the errors to the Monaco Editor so that it sees which row and column the errors are to have red underlines and visual aids.

---

## ContextFactories.cs

**Comment:** A factory should be registered via DI. So the create method does not need to be passed an `IServiceProvider`. The factory class can simply get the relevant services from via the constructor. 

You also mixed DI-services with actual parameters. For example `LabOrder` and `Patient` are POCOs not services. They are parameters that need to be provided by the developer when they use the factory. On the other hand `ConsoleLogger` and `DataAccess` are services that need to be resolved from DI via the constructor. 

There is no need for an `IContextFactory` interface. This is because each factory defines its own parameters that are different from the factory of another context. 

You can have an interface per factory like `IGeneratorContextFactory`. As this allows for unit test mocking etc.

**Example:**
```csharp
public class ContextFactory(ConsoleLogger consoleLogger, DataAccess dataAccess) : IGeneratorContextFactory
{
    public GeneratorContextSF Create(LabOrder labOrder, Patient patient)
    {
        return new GeneratorContext(labOrder, patient, consoleLogger, dataAccess);
    }
}

public interface IGeneratorContextFactory
{
    GeneratorContextSF Create(LabOrder labOrder, Patient patient);
}
```

## Restructuring of File Structure Proposal

**Comment:** The scripting library structuring is a bit off at this point. 

*   Rename `DbFiles` -> `Persistence`
    *   `MyContext` -> `ScriptDbContext`
    *   `DbHelper` -> `ScriptRepository`
*   `CompilationAndExecution/` should be separated into `Compilation/` and `Execution/`. They share no code and they have different concerns/purposes.
*   Extract `ValidationRecord` into its own file inside the `Compilation` folder. Same story with `ValidationRecord`.
*   Folder `ScriptManager.cs/` should be renamed to simply `ScriptManager/`.

So something like this:

```text
scripting/
├─ Compilation/
│  ├─ IScriptCompiler.cs
│  ├─ ScriptCompiler.cs
│  ├─ ValidationRecord.cs
│  ├─ CompilationExceptions.cs
├─ Execution/
│  ├─ IScriptExecutor.cs
│  ├─ ScriptExecutor.cs 
│  ├─ ExecutionExceptions.cs
├─ Versioning/
│  ├─ VersionAttributes.cs
│  ├─ ContextVersionScanner.cs
│  ├─ ActionResultVersionScanner.cs
│  ├─ ScriptVersionScanner.cs 
├─ Persistence/
│  ├─ IScriptRepository.cs
│  ├─ ScriptRepository.cs
│  ├─ ScriptDbContext.cs
│  ├─ CustomerScript.cs
│  ├─ CustomerScriptFilter.cs
│  ├─ ScriptCompiledCache.cs  // Maybe rename to CompiledScript.cs
│  ├─ EmberInstance.cs
│  ├─ PersistenceExceptions.cs
└─ ScriptManager/             // Renamed from ScriptManager.cs folder
```
