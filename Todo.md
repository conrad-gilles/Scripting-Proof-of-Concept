# Todo

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