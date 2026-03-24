using System;   //todo this is possible to default in compiler
using System.Threading.Tasks;
using System.Collections.Generic;   //todo same for them
using Ember.Scripting;
using GeneratorScriptsGeneric;
using IGeneratorContext_V2;

using static System.Threading.Mutex; //this using is banned and will prevent the script from passing BasicValidation and therefore compiling
public class IllegalUsingScript : GeneratorScriptsGeneric.IGeneratorActionScript<IGeneratorContext_V2.IGeneratorContext, ActionResultV1.ActionResult>
{
    public async Task<ActionResultV1.ActionResult> ExecuteAsync(IGeneratorContext_V2.IGeneratorContext context)
    {
        // System.IO.BufferedStream? stream = null;

        return ActionResultV1.ActionResult.Success("This line will never be hit");
    }
}

