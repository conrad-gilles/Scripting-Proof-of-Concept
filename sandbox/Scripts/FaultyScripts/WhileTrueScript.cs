using System;   //todo this is possible to default in compiler
using System.Threading.Tasks;
using System.Collections.Generic;   //todo same for them
using Ember.Scripting;
using Ember.Scripting.Execution;
using GeneratorScriptsV3;
using IGeneratorContext_V4;

public class WhileTrueScript : GeneratorScriptsV3.IActionScript
{
    public async Task<ActionResultV3.ActionResult> ExecuteAsync(IGeneratorContext_V4.IGeneratorContext context)
    {
        int i = 0;
        while (i >= 0)
        {
            ScriptEnvironment.CurrentToken.Value.ThrowIfCancellationRequested();
            // Console.WriteLine("Infinite Loop, iteration N." + i);
            i++;
            if (i <= 0) //for integer overflow
            {
                i = 1;
            }
        }
        return ActionResultV3.ActionResult.Success("Infinite Loop script returned (should never happen!)");
    }
}
