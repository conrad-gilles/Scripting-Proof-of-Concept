using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ember.Scripting;
using GeneratorScriptsV3;
using IGeneratorContext_V4;

public class PublicScript : GeneratorScriptsV3.IActionScript
{
    public async Task<ActionResultV3.ActionResult> ExecuteAsync(IGeneratorContext_V4.IGeneratorContext context)
    {
        return ActionResultV3.ActionResult.Success("Default method ExecuteAsync was called");
    }

    public async Task<ActionResultV3.ActionResult> Execute1(IGeneratorContext_V4.IGeneratorContext context)
    {
        return ActionResultV3.ActionResult.Success("ExecuteAction1 was called");
    }
    public async Task<ActionResultV3.ActionResult> Execute2(IGeneratorContext_V4.IGeneratorContext context)
    {
        throw new MethodNotImplementedException(message: "error was thrown");
    }

    public async Task<String> SomeUndefindedMethod(CustomerScript script)
    {
        return "";
    }
}
