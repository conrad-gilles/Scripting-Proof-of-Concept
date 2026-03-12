# Todo

- pass data in context so when you execute script(id,context) it gives data to the script
- this data needs to be standardized maybe using something like fluentvalidation idk
- CreateContext() in ScriptFactory takes in a few arguments 
- fix basic val still doesnt check if implements correct class
- in ui when compile all scripts with 1 corrupt one more than 1 dont cpompile because it aborts the process
- version generator scripts in scripting done but yet no logic behind it
- create script factory in scriptmanager facade
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