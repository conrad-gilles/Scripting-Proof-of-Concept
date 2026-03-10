# Todo
- version generator scripts in scripting
- wrap generator scripts also in namespaces so the name can be the same
- create script factory in scriptmanager facade
- pass data in context so when you execute script(id,context) it gives data to the script
- this data needs to be standardized maybe using something like fluentvalidation idk
- fix basic val still doesnt check if implements correct class
- in ui when compile all scripts with 1 corrupt one more than 1 dont cpompile because it aborts the process
- upgrade action result -> interface plus downgrade async
- create context downgrade that is automatically called from ember, developer in ember should not know care about what most recent version is
- namespace of most recent version
- issue with this would be that old scripts when recompiled would bug because using would stay and would not get auto refactored when a new namespace of recent version gets updated and but the implementation of this namespace could have changed which could break the new recompield version of the script
- CreateContext() in ScriptFactory takes in a few arguments 
- maybe refactor basic val so he gets the version int from parameter outside where from maybe sandbox the version int is determined using the ScriptFactory
- other option is injecting scriptfactory somehow and then determining it inside the class lib without having to ive access to the entire sandbox!
- change verion abstract int to annotation 

# Developers in Ember simply do this:
 
var context = await RoutingDecisionSpecialCaseScriptFactory.CreateContext(...);
var result = await ScriptManager.ExecuteScript(Scripts.RoutingDecisionSpecialCaseScript, context);
 
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