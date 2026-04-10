using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ember.Scripting;

public class ScriptTemplate : GeneratorScriptsV3.IActionScript
{
    public async Task<ActionResultV3.ActionResult> ExecuteAsync(IGeneratorContext_V4.IGeneratorContext context)
    {
        //Write your code here

        //Return your ActionResult here:
        return ActionResultV3.ActionResult.Failure("Insert your message here");
    }
}
