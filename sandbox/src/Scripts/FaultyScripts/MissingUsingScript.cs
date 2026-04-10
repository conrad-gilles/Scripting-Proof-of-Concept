public class MissingUsingScript : GeneratorScriptsGeneric.IActionScript<IGeneratorContext_V2.IGeneratorContext, ActionResultV1.ActionResult>
{
    public async Task<ActionResultV1.ActionResult> ExecuteAsync(IGeneratorContext_V2.IGeneratorContext context)
    {
        // System.IO.BufferedStream? stream = null;

        return ActionResultV1.ActionResult.Success("This line will never be hit");
    }
}

