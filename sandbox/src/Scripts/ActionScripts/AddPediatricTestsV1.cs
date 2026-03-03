using System;   //todo this is possible to default in compiler
using System.Threading.Tasks;
using System.Collections.Generic;   //todo same for them
using Ember.Scripting;
// using static System.Threading.Mutex; //if uncomment it wont run which is good
public class AddPediatricTestsV1 : IGeneratorActionScript<IGeneratorContext_V1.IGeneratorContext, ActionResult>
{
    public async Task<ActionResult> ExecuteAsync(IGeneratorContext_V1.IGeneratorContext context)    //error is here in Basyns
    {
        context.Logger.Info("Adding pediatric standard tests");

        // Check if test already exists (might be auto-added by business rule)
        if (!context.LabOrder.HasTest("PED-BASIC"))
        {
            context.LabOrder.AddTest("PED-BASIC");
        }
        // System.IO.BufferedStream? stream = null;
        // double versionExample = context.LabOrder.RandomNewDouble;    //example of newer version
        // Console.WriteLine(versionExample);
        context.LabOrder.SetCustomField("ProcessedByGenerator", true);
        return ActionResult.Success("Pediatric tests added");   //todo check what happens if you return this object without calling the function on it
        // return null; //just for compiler
    }
}

