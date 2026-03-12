using System.Threading.Tasks;
using System;
using Ember.Scripting;
using GeneratorScriptsV4;

// public class VaccineScript : IGeneratorActionScript<IGeneratorContextNoInheritance_V4.IGeneratorContext, ActionResultV3NoInheritance>
// {

//     public async Task<ActionResultV3NoInheritance> ExecuteAsync(IGeneratorContextNoInheritance_V4.IGeneratorContext context)
//     {
//         string number = context.LabOrder.OrderNumber;
//         if (number == "1")
//         {
//             Console.WriteLine("do somehing");
//         }
//         // context.LabOrder.OrderNumber = "2";  //doesnt work because red only
//         string name = context.Vaccine.GetName();

//         return ActionResultV3NoInheritance.Success(name + " Vaccine added");
//     }
// }

public class VaccineScript : GeneratorScriptsV4.IGeneratorActionScript
{
    public async Task<ActionResultV3.ActionResult> ExecuteAsync(IGeneratorContextNoInheritance_V5.IGeneratorContext context)
    {
        string number = context.LabOrder.OrderNumber;
        if (number == "1")
        {
            Console.WriteLine("do somehing");
        }
        // context.LabOrder.OrderNumber = "2";  //doesnt work because red only
        string name = context.Vaccine.GetName();

        return ActionResultV3.ActionResult.Success(name + " Vaccine added");
    }
}
