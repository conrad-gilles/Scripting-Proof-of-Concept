using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Threading.Tasks;

namespace Ember.Scripting;

internal class ScriptExecutor
{
    // private readonly byte[] compiledScript;
    // private readonly GeneratorContext genContext;
    private readonly static int ScriptTimeout = 5000;
    // private static readonly TimeSpan ScriptTimeout = TimeSpan.FromSeconds(5);
    private readonly ILogger<ScriptExecutor> Logger;
    public ScriptExecutor(ILogger<ScriptExecutor> logger)
    {
        Logger = logger;
    }

    public T RunScriptExecution<T>(byte[] compiledScript, GeneratorContext genContext)
    // public void RunScriptExecution()
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(RunScriptExecution), nameof(ScriptExecutor));
        try
        {
            if (compiledScript.Length > 5 * 1024 * 1024) // 5 mb maximum size
            {
                throw new ScriptExecutionException();
            }

            Assembly assembly = Assembly.Load(compiledScript);

            Type[] unfilteredTypeArray = assembly.GetTypes();   //even though there can be only one class defined in the script file, the compiler adds classes making the array.lenght over 1 which is unsafe so it is better to filer based on our predefined classes for scripts
            List<Type> typeArrayList = [];
            for (int i = 0; i < unfilteredTypeArray.Length; i++)
            {
                if (typeof(IGeneratorActionScript).IsAssignableFrom(unfilteredTypeArray[i]))
                {
                    typeArrayList.Add(unfilteredTypeArray[i]);
                }
                if (typeof(IGeneratorConditionScript).IsAssignableFrom(unfilteredTypeArray[i]))
                {
                    typeArrayList.Add(unfilteredTypeArray[i]);
                }
            }
            Type[] typeArray = typeArrayList.ToArray(); //todo why did i make an array here put back into list

            if (typeArray.Length == 0)
            {
                throw new NoClassFoundInScriptFileException();
            }
            else if (typeArray.Length > 1)
            {
                Logger.LogInformation("more than one class found in script");
                throw new MoreThanOneClassFoundInScriptException();   //to implement more than one name you would need to pass name of class into this class
            }

            Type type = typeArray[0];

            object scriptInstance = Activator.CreateInstance(type)!;     //if null here probably typo in file name somewhere, like pedriatic instead of pediatic :(


            // if (typeof(IGeneratorConditionScript).IsAssignableFrom(type))
            if (typeof(IGeneratorConditionScript).IsAssignableFrom(type))    //checks if type implements the generator specific interface  //check if runs
            {
                var result = RunConditionScript(type, scriptInstance, genContext);
                return (T)(object)result;
            }
            else if (typeof(IGeneratorActionScript).IsAssignableFrom(type))
            {
                var result = RunActionScript(type, scriptInstance, genContext);
                return (T)(object)result;
            }
            else
            {
                Logger.LogInformation("Could not run your script because it is neither a ActionScript nor a ConditionScript.");
                throw new ScriptExecutionException();
            }
        }
        catch (Exception e)
        {
            Logger.LogInformation("Something went wrong when trying to execute your code, here are some details:");
            // Logger.LogError(e.Message);
            // Logger.LogError(e.StackTrace);
            Logger.LogError(e.ToString());
            throw new ScriptExecutionException();
        }

    }
    public bool RunConditionScript(Type type, object scriptInstance, GeneratorContext genContext)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(RunConditionScript), nameof(ScriptExecutor));
        try
        {
            MethodInfo method = type.GetMethod("EvaluateAsync")!;
            // 2. Invoke method on the instance, passing context as a parameter
            // Note: EvaluateAsync returns a Task<bool>, so 'result' will be a Task
            var resultTask = (Task)method.Invoke(scriptInstance, new object[] { genContext })!;

            // // 3. Wait for the task to complete and get the result
            // resultTask.Wait();

            // Wait with timeout — throws OperationCanceledException if exceeded
            // if (!resultTask.Wait((int)ScriptTimeout.TotalMilliseconds))
            if (!resultTask.Wait(ScriptTimeout))
            {
                throw new ScriptTimeoutException("Condition script exceeded time limit.");
            }

            // access the "Result" property of the Task
            var resultProperty = resultTask.GetType().GetProperty("Result");
            var resultValue = resultProperty!.GetValue(resultTask);

            Logger.LogInformation($"Result: {resultValue}");    //todo probably unnessesary but good for debugging
            return (bool)resultValue!; // todo very important error handling
        }
        catch (Exception e)
        {
            Logger.LogInformation(e.ToString());
            throw new ConditionScriptExecutionException();
        }

    }
    public ActionResultBaseClass RunActionScript(Type type, object scriptInstance, GeneratorContext genContext)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(RunActionScript), nameof(ScriptExecutor));
        try
        {
            MethodInfo method = type.GetMethod("ExecuteAsync")!; //todo
                                                                 // 2. Invoke method on the instance, passing context as a parameter
                                                                 // Note: EvaluateAsync returns a Task<bool>, so 'result' will be a Task
                                                                 //todo implement check for old versions of GeneratorContext class because GeneratorContextV2 might not be compatible


            var resultTask = (Task)method.Invoke(scriptInstance, new object[] { genContext })!;

            // 3. Wait for the task to complete and get the result
            // resultTask.Wait();

            if (!resultTask.Wait(ScriptTimeout))
            {
                throw new ScriptTimeoutException("Action script exceeded time limit.");
            }

            // access the "Result" property of the Task
            var resultProperty = resultTask.GetType().GetProperty("Result");
            var resultValue = resultProperty!.GetValue(resultTask);

            Logger.LogInformation($"Result in 86 sExecuter: {resultValue}");    //todo probably unnessesary but good for debugging
            // return (ActionResult)resultValue;
            // return UpgradeActionResult(resultValue);
            return (ActionResultBaseClass)resultValue!;  //this might fail because not baseclass idk, if it does maybe change whole structure to only one function
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            Logger.LogWarning("You might have passed the wrong GeneratorContext class, ex V1 instead of V2");
            throw new ActionScriptExecutionException();
        }

    }
}
