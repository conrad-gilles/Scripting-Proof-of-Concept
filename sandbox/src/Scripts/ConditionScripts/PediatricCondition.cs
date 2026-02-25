

using Ember.Scripting;
using System;   //todo this is possible to default in compiler
using System.Threading.Tasks;
using System.Collections.Generic;   //todo same for them
// [ScriptMetadata(MinApiVersion = 2)]
// namespace MyCustomScripts //todo change get rid idk
// {

public class PediatricCondition : IGeneratorConditionScript<IGeneratorReadOnlyContext>
{

    public async Task<bool> EvaluateAsync(IGeneratorReadOnlyContext context)
    {
        if (context.Patient.DateOfBirth.HasValue)
        {
            var age = DateTime.Now.Year - context.Patient.DateOfBirth.Value.Year;
            return age < 18 && context.LabOrder.Department == "Pediatrics";
        }
        // var example = context.LabOrder.OrderNumber;
        // context.LabOrder.AddTest("PED-BASIC");
        return false;

        // return true; //just for compiler error
    }
}
// }
// test change to see update 4