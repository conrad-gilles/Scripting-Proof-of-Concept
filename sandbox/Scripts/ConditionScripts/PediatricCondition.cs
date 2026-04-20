

using Ember.Scripting;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using GeneratorScriptsGenericSimple;
public class PediatricCondition : IConditionScript<IGeneratorReadOnlyContextV1.IGeneratorContext>
{

    public async Task<bool> EvaluateAsync(IGeneratorReadOnlyContextV1.IGeneratorContext context)
    {
        if (context.Patient2.DateOfBirth.HasValue)
        {
            var age = DateTime.Now.Year - context.Patient2.DateOfBirth.Value.Year;
            return age < 18 && context.LabOrder.Department == "Pediatrics";
        }
        // var example = context.LabOrder.OrderNumber;
        // context.LabOrder.AddTest("PED-BASIC");
        return false;

        // return true; //just for compiler error
    }
}
