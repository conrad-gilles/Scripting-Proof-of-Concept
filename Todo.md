# Todo

- maybe assign scripts diffrent types which allow for longer execution times maybe add also something similar for critical...

- always have the README.md snippets up to date

- maybe make a SecurityTest.cs and write test to check if malicious scripts pass

- remove all occurences of throw new Exception
- ideally 4 references for every specific exception (3 for constructors 1 for atualusage)
- create new exception classes for them and add them to the respective files
- get rid of the generic DBhelper exception etc and either replace them with specific one or remove them if its just catch->throw
- 
- remove all occurences of data
- rename GeneratorContexts to PatientContexts or something like that?
- maybe for the future allow for multiple functions in the script, or what i proposed allow for multiple static classes with the standard methods
- only 1 factory that creates the most recent context get rid of the others or maybe put them in a Testing namespace?
- put all GenCOntext in a namespace like maybe Ember.Scripting.GeneratorContextsV5 etc
- make GeneratorContextSF mayeb an interface?
- make new project next to sandbox to put in what will actually go in ember

- write test for checkversioncompatibility
- SaveScriptWithoudCompiling now throwserror so maybe create a function to create a script without compiling?
- write buggy scripts containing maybe stuff like while true...
- monaco editor when right clicking bugs and is super dark not ui friendly
- store v1 of action scripts maybe in hitory not in diffrent file
- create factories and cleanup

- public async Task<ActiveActionResult> ExecuteScriptByNameAndType(string Name, ScriptTypes scriptType, ActiveGeneratorContext ctx) //maybe make generic
- make basicVal return record or class
- add update cache method to db helper and then also to  scriptmanager
- improve isduplicate function in db helper to add semantic treee check to see if no script in db has same tree?: this would be unnessecary if - class name enforced as scriptname
- hard coded connection string still need to fix
- create composite primary key from name and type of script (condition action)
- create diffrent data classes for each api version, then you need to pass this data in create below;
- create() needs to somehow be type safe so it throws compile time error
- maybe diffrent create factories for each context version
- GeneratorContext ctx=GeneratorContextFactory.Create(data);    no id fro script;
- ExecuteById(id,context)   executes script, calls a facade of facade, in ember that upgradesaction result, and also auto downgrades in previes ctx was not right one.  

- define global using that is somethign like this: using Project = PC.MyCompany.Project;
- this using will only be used by ember and not by customer scripts because those cant be auto refactored

- make sure that when executing script, the passed context is the correct one, error should be at compile time not run time
- remove hardcoded paths using ctrl alt f C:\Users\Gilles\Desktop\UNI\Semester 6\Code\Codebase\Scripting-Proof-of-Concept\sandbox\src\Scripts
- pass data in context so when you execute script(id,context) it gives data to the script //maybe add it to facade instead but idk
- this data needs to be standardized maybe using something like fluentvalidation idk
- CreateContext() in ScriptFactory takes in a few arguments 
- fix basic val still doesnt check if implements correct class  probably with is null? is typeof ?
- in ui when compile all scripts with 1 corrupt one more than 1 dont cpompile because it aborts the process
- make sure azure works

# Developers in Ember simply do this:
 
- var context = await RoutingDecisionSpecialCaseScriptFactory.CreateContext(...);
- var result = await ScriptManager.ExecuteScript(Scripts.RoutingDecisionSpecialCaseScript, context);
 
### This has the following implications:
  - The context is always of the newest version
  - Customers may not have upgraded yet, so Script context must be downgradable
    Similar idea to how results are upgradable
### So if we have ember with API Version 6 running:
  - Ember passes V6 context to ScriptManager
  - ScriptManager checks what version is running
  - ScriptManager downgrades the context until it is of the version defined at the customer
  - ScriptManager executes the script and provides the correctly versioned context
  - ScriptManager upgrades the result returned from the Script to the version of the API Version (so V6)

### How Context should be instantiated and execute called
- GeneratorContext ctx=GeneratorContextFactory.Create("Laborder");
- ActionResult result= await facade.executescript< GeneratorContex,ActionResult >(ctx);
- ActionResult result= await facade.executescript< GeneratorActionScript >(ctx);

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
