the changes are all visible on Github in the repo, mainly look at the 2 files MultiMethodScriptTests (Line 192-256) and the file EmberInternalFacade (whole file)


I added 2 new ways of calling the additional functions, one way where you dont have to Get() the script first and just insert the name and type into a ember internal facade function.
A call would look like this:

File: MultiMethodScriptTests, Line 197
RecentActionResult ar = (RecentActionResult)await InternalScriptManager.ExecuteScript<IGeneratorActionScript>(scriptDB.ScriptName!, TestHelper.GetContext());

And I added a way to first get the script using the EmberInternalFacade and then call the function on the returned(from the get function) object.
Example File: MultiMethodScriptTests, Line 217

var script = InternalScriptManager.GetScript<IGeneratorActionScript>(scriptDB.ScriptName!); //same as above just using var instead
RecentActionResult ar = (RecentActionResult)await script.ExecuteAsync(context);

What exactly should the "script instance" be, i am guessing a class that implements the IActionScript or IConditionScript for example?
Currently my ScriptFacade is a class that implements the most recent interface versions: 

File: EmberInternalFacade, Line 73
internal class ScriptFacade<ScriptType> : RecentIGeneratorActionScript,
GeneratorScriptsGenericSimple.IGeneratorConditionScript<RecentIGeneratorContext> where ScriptType : IScript

An object of this ScriptFacade class is returned when a developer calls the GetScript<type>(name); method that is located inside the EmberInternalFacade.

File: EmberInternalFacade, Line 67
public ScriptFacade<ScriptType> GetScript<ScriptType>(string name) where ScriptType : IScript
{
    return new ScriptFacade<ScriptType>(_scriptManager, name);
}

I have not really made a solid implementation of the "Check from newest to oldest" Page 3 of the doc, since i am still a bit confused on how I shoudl implement this,
i am confused mainly because we always pass only one parameter to every method (the context), so the only thing that could change over time would be the Method name.
This is also assuming every method must either return a bool or an ActionResult? Or should we allow other return types?
But basically if the only thing that changes is the name, maybe a simple switch like I did on the bottome of the EmberInternalFacade file is enough?