using System.Threading.Tasks;
using System;
using Ember.Scripting;

public class VaccineScript : IGeneratorActionScript<IGeneratorContextNoInheritance_V4, ActionResultV3NoInheritance>
{

    public async Task<ActionResultV3NoInheritance> ExecuteAsync(IGeneratorContextNoInheritance_V4 context)
    {
        string number = context.LabOrder.OrderNumber;
        if (number == "1")
        {
            Console.WriteLine("do somehing");
        }
        // context.LabOrder.OrderNumber = "2";  //doesnt work because red only
        string name = context.Vaccine.GetName();

        return ActionResultV3NoInheritance.Success(name + " Vaccine added");
    }
}

// public class VaccineScript : IGeneratorActionScriptV4Vaccine
// {
//     public async Task<ActionResultV3NoInheritance> ExecuteAsync(IGeneratorContextNoInheritance context)
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