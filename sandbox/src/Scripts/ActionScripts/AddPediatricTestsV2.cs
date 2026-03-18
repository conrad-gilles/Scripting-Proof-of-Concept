using System;   //todo this is possible to default in compiler
using System.Threading.Tasks;
using System.Collections.Generic;   //todo same for them
using Ember.Scripting;
using IGeneratorContext_V3;
using GeneratorScriptsV2;
// public class AddPediatricTestsV2 : IGeneratorActionScript<IGeneratorContext_V2, ActionResultV2>
// {
//     // public async Task<ActionResult> ExecuteAsync(IGeneratorContext context)
//     public async Task<ActionResultV2> ExecuteAsync(IGeneratorContext_V2 context)
//     {
//         context.Logger.Info("Adding pediatric standard tests");

//         // Check if test already exists (might be auto-added by business rule)
//         if (!context.LabOrder.HasTest("PED-BASIC"))
//         {
//             context.LabOrder.AddTest("PED-BASIC");
//         }

//         context.LabOrder.SetCustomField("ProcessedByGenerator", true);

//         double versionExample = context.LabOrder.RandomNewDouble;    //example of newer version
//         Console.WriteLine(versionExample);

//         // string exampleV3 = context.LabOrder.randomNewFunctionInV3("Gilles");
//         // Console.WriteLine("Return also works if here:" + exampleV3);

//         return ActionResultV2.Success("Pediatric tests added");
//         // return null; //just for compiler
//     }
// }

public class AddPediatricTestsV3 : GeneratorScriptsV2.IGeneratorActionScript
{
    // public async Task<ActionResult> ExecuteAsync(IGeneratorContext context)
    public async Task<ActionResultV2.ActionResult> ExecuteAsync(IGeneratorContext_V3.IGeneratorContext context)
    {
        context.Logger2.Info("Adding pediatric standard tests");

        // Check if test already exists (might be auto-added by business rule)
        if (!context.LabOrder.HasTest("PED-BASIC"))
        {
            context.LabOrder.AddTest("PED-BASIC");
        }

        context.LabOrder.SetCustomField("ProcessedByGenerator", true);

        double versionExample = context.LabOrder.RandomNewDouble;    //example of newer version
        Console.WriteLine(versionExample);

        // string exampleV3 = context.LabOrder.randomNewFunctionInV3("Gilles");
        // Console.WriteLine("Return also works if here:" + exampleV3);

        return ActionResultV2.ActionResult.Success("Pediatric tests added");
        // return null; //just for compiler
    }
}
