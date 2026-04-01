using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ember.Scripting;
using GeneratorScriptsV3;
using IGeneratorContext_V4;

public class InternalScript : GeneratorScriptsV3.IGeneratorActionScript
{
    public async Task<ActionResultV3.ActionResult> ExecuteAsync(IGeneratorContext_V4.IGeneratorContext context)
    {
        return ActionResultV3.ActionResult.Success("Default method ExecuteAsync was called");
    }

    public async Task<ActionResultSF> ExecuteAction1(IGeneratorBaseInterfaceSF context)
    {
        return ActionResultV3.ActionResult.Success("ExecuteAction1 was called");
    }
    public async Task<ActionResultSF> ExecuteAction2(IGeneratorBaseInterfaceSF context)
    {
        throw new MethodNotImplementedException(message: "error was thrown");
    }

    internal async Task<String> SomeUndefindedInternalMethod(CustomerScript script)
    {
        return "";
    }
}
