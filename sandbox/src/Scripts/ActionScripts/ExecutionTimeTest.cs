using System;   //todo this is possible to default in compiler
using System.Threading.Tasks;
using System.Collections.Generic;   //todo same for them
using Ember.Scripting;
using GeneratorScriptsGeneric;
using IGeneratorContext_V2;

[ExecutionTime(ExecutionTimeGroups.Long)]
public class ExecutionTimeTest : GeneratorScriptsGeneric.IGeneratorActionScript<IGeneratorContext_V2.IGeneratorContext, ActionResultV1.ActionResult>
{
    public async Task<ActionResultV1.ActionResult> ExecuteAsync(IGeneratorContext_V2.IGeneratorContext context)    //error is here in Basyns
    {
        return ActionResultV1.ActionResult.Success("Pediatric tests added");
    }
}