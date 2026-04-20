using System;   //todo this is possible to default in compiler
using System.Threading.Tasks;
using System.Collections.Generic;   //todo same for them
using Ember.Scripting;
using GeneratorScriptsGeneric;
using IGeneratorContext_V2;
public class Script1 : GeneratorScriptsGeneric.IActionScript<IGeneratorContext_V2.IGeneratorContext, ActionResultV1.ActionResult>
{
    public async Task<ActionResultV1.ActionResult> ExecuteAsync(IGeneratorContext_V2.IGeneratorContext context)
    {
        return ActionResultV1.ActionResult.Success("This line will never be hit");
    }
}

public class Script2 : GeneratorScriptsGeneric.IActionScript<IGeneratorContext_V2.IGeneratorContext, ActionResultV1.ActionResult>
{
    public async Task<ActionResultV1.ActionResult> ExecuteAsync(IGeneratorContext_V2.IGeneratorContext context)
    {
        return ActionResultV1.ActionResult.Success("This line will never be hit");
    }
}
