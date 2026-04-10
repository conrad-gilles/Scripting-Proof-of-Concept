- get rid of the default methods
- get ric of the IActionScript and so on in the scripting library
- get rid 
- rename the RecentActionResult and RecentConditionresult
- have to allow other returns
- extract Upgrade and downgrade into seperate interfaces so in checkupgradeactionresult you dont need to check specifically for this return type
- extract the above function mayeb into seperate component
remove all occurences of generator in namings
MultiMethod: 
old methods gets changed from string SetX(context) to void SetX(context), we now want the old version to always receive a default string, where to define this behaviour?
return a new Script facade from getScript
context needs to be versioned also that is being inserted

# Todo
- ask if multiple methods also should be allowed for condition scripts? probably yes

- maybe add a context tab to the ui to be able to modify etc
  
- should ScriptBase implement both the Condition and the Action interface or only the Action?

- One could maybe define a hierarchy of the new functions using attributes, so that one could maybe call something like ExecuteAllMethods(), ExecuteHighPriority()...
  
- how to allow to pass nothing, for normal scripts?

- test if the ExecutionTIme is actually respected and that scritps are aborted after said time

### Always check those

- calling basicvalidation after .GetScript call is redundant and should be removed for performance just use script.Name etc

- maybe switch globally in the whole application to always pass the CustomerScript object instead of the id or name

- maybe assign scripts diffrent types which allow for longer execution times maybe add also something similar for critical... (maybe by adding an optional attribute above a script which declares the time )
- always have the README.md snippets up to date
- remove all occurences of throw new Exception 
- ideally 4 references for every specific exception (3 for constructors 1 for atualusage)
- create new exception classes for them and add them to the respective files
- get rid of the generic DBhelper exception etc and either replace them with specific one or remove them if its just catch->throw 

### Main Todos

- maybe make it so can call script oinly by name not both name and type? No sure some downsides

- new way of calling multiple methods within a script, something like this:
  ActiveActionResult ar = await InternalScriptManager!.GetScript<IGeneratorActionScript>("AddPediatrucTestV2").ExecuteAction(ctx); //get db instance

   they need to be defined in the interface
   versioning script?

- make a get script method that return the DB instance and not the the Customer Script instance, why? maybe return byte[]? 

- check if the line for the cancellation token is called in every loop from within the script to avoid scripts from deadlocking:
  Ember.Scripting.ScriptEnvironment.CurrentToken.Value.ThrowIfCancellationRequested();
  idea: count maybe ammount of loops and force to have same amount of check tokens

- add optional attribute to distinguish between scripts that get more execution time than others, let script declare execution time, but set an absolute hardcoded max somewhere
- also what if script gets abortet halfway through? Is there a rollback changes? If no implement!

- get 100% test coverage
- put all GenCOntext in a namespace like maybe Ember.Scripting.GeneratorContextsV5 etc
- make GeneratorContextSF mayeb an interface?
- public async Task<ActiveActionResult> ExecuteScriptByNameAndType(string Name, ScriptTypes scriptType, ActiveGeneratorContext ctx) //maybe make generic
- get rid of the enum and just use typeOf IGeneratorActionScript etc...
- 
- monaco editor when right clicking bugs and is super dark not ui friendly

- improve isduplicate function in db helper to add semantic treee check to see if no script in db has same tree?: this would be unnessecary if - class name enforced as scriptname
- hard coded connection string still need to fix
- create composite primary key from name and type of script (condition action)

- fix basic val still doesnt check if implements correct class  probably with is null? is typeof ?
- in ui when compile all scripts with 1 corrupt one more than 1 dont cpompile because it aborts the process
- make sure render.com deploy works
-  
### Ask about:
- rename GeneratorContexts to PatientContexts or something like that?
- maybe for the future allow for multiple functions in the script, or what i proposed allow for multiple static classes with the standard methods

### Redefine IGeneratorActionScript so it takes maybe generics
- somethign like this
- public interface IGeneratorActionScript< Tcontext,TActionResult >

## Code Review 1 todo:
6) Remove subsequent empty lines.
7) _camelCase for all private fields — eliminates the biggest source of visual confusion 
8) camelCase for all method parameters. You sometimes used Pascal case.
9) The classes like ScriptManger, ScriptExecutor should never contain methods that are only there for testing. (example: ValidateScript and GetCompilationErrors in ScriptManger.cs)

2) Duplicate code

   Duplicate code between BasicValidationBeforeCompiling and GetBaseType
   ScriptCompiler.cs:93-109 and 111-211   
Answer:
I got rid of it by having GetBaseType return a more complex record, it works but a bit overcomplex still. 
3) GetReferencesForOldVersion it should get the references from a folder.

Question: So instead of from current runtime from folder containing the dlls?
2) RemoveDuplicates has a bug. You loop over duplicateGuids and then check if duplicateGuids contains the element at index i; this is always true of course. So you basically delete everyting.

   Do we even need such a method?

Answer:
If we decide that we always must check for duplicates when inserting or updating, this function would be unncessecary.
Also it is hard to test since you would have to circumvent the duplicate check when inserting.

I didnt try to fix the bug yet since i cant really test it right now, if we decide to keep the function i will of course fix and test it.
   
3) Do not use  using (var db = new MyContext())
   Use dependency injection and DbContextFactory. See: https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#use-a-dbcontext-factory

Answer:
Done, but i kept the connection string hardcoded in the MyContext class for simplicity.
6) You still have a bunch of Console.WriteLine statements that need to be removed.
Answer:
I commented them all out in the DbHelper class
1) CheckVersionCompatibility simply checks all script versions.
   This is because it internally calls GetActiveApiVersions which calls GetAllCompiledScriptCaches.
   It does not check if the current script is compatible with the targetApiVersion.

Question: Should it check if a cache of the current ember api version is inside the db? If yes compile so it is compatible?
          Or by compatibility do you mean the current active context is downgradeable to execute the script?
2) Maybe I am missing something but UpdateScript does not actually compile the script it just updates the source code.

Answer:
I removed the part in the description that says it also recompiles. Or should I instead make it also compile after updating?
4) Create a mock service that provides you the user name for GetUserName.

Question: So inject the username by DI, if yes what if user changes can you reinject?
6) In CleanupOrphanedCaches you call AutomaticCompilationOnVersionUpdate. Seems like the wrong method no?

Also is AutomaticCompilationOnVersionUpdate even needed? We will never recompile all our script simply because ember has a new api version. Or I am missing something - which is very likely - so let me know.

Answer: Yes i have not written an implementation for it yet, should I write one? I dont know if it is worth investing time because normally when you delete a script all associated caches should get deleted too.

I believe the Idea behind AutomaticCompilationOnVersionUpdate was that if there are major changes in Ember (for example LabOrder get renamed to LOrder) the already compiled scripts (who will not be refactored) will fail when this recompilation happens, and not when they should be executed and then fail.
(This would cause JIT compilation in a situation that might be time critical.)
This also brings up the question if one should maybe implement a way of automatically refactoring the scripts somehow? 
7) GetCompilationErrors is a method that is only used for testing. It should therefore not be part of the ScriptManager.

Question: Should i keep it in the ScriptManager class and remove it from the interface, currently i am not using it anywhere except tests, but my idea was using it in the UI to pass the errors to the Monaco Editor,
so that it sees which row which column the errors are to have red underlines and other visual aids when writin the scripts.


- Mock Service for Username
    No a simple mock service interface like IUserSession. And a mock implementation like SandBoxUserSession.
	Which simply returns the static string or id of the user.
	When we combine the library with Ember, Ember will provide an actual implementation of IUserSession (or similar).

Fixing the Roslyn Memory Leak
Currently, your ScriptExecutor.cs uses Assembly.Load(compiledScript) to execute dynamic code. In .NET, assemblies loaded directly into the default Application Domain cannot be unloaded, meaning every time a customer script is executed, RAM is consumed permanently, eventually leading to an OutOfMemoryException server crash.

Implementing Strategy D (Multi-Version Caching)
Your RequirementsConfluence.txt document explicitly dictates "Strategy D: Compile on Save, Multi-Version by API Version". Your current ScriptRepository.cs methods only compile the script for a single recent API version. Instead, you need to discover all active Ember instances and pre-compile the script for every unique API version that is greater than or equal to the script's MinApiVersion