using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Threading.Tasks;

namespace Ember.Scripting;

internal class ScriptExecutor
{
    private int _scriptTimeout = ((int)ExecutionTimeGroups.Medium);   // ms of how much time scripts get to execute
    private readonly ILogger<ScriptExecutor> _logger;
    public ScriptExecutor(ILogger<ScriptExecutor> logger)
    {
        _logger = logger;
    }

    public async Task<T> RunScriptExecution<T>(byte[] compiledScript, GeneratorContextSF genContext, int? executionTime)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(RunScriptExecution), nameof(ScriptExecutor));

        if (compiledScript.Length > 5 * 1024 * 1024) // 5 mb maximum size
        {
            throw new CompiledScriptWasTooLargeException(nameof(RunScriptExecution) + " failed in if (compiledScript.Length > 5 * 1024 * 1024)");
        }
        if (executionTime != null)
        {
            _scriptTimeout = (int)executionTime;
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
        Type[] typeArray = typeArrayList.ToArray();

        if (typeArray.Length == 0)
        {
            throw new NoClassFoundInScriptFileException(nameof(RunScriptExecution) + "failed in if (typeArray.Length == 0)");
        }
        else if (typeArray.Length > 1)
        {
            _logger.LogInformation("more than one class found in script");
            throw new MoreThanOneClassFoundInScriptExecutionException("more than one class found in script");   //to implement more than one name you would need to pass name of class into this class
        }

        Type type = typeArray[0];

        object scriptInstance = Activator.CreateInstance(type)!;     //if null here probably typo in file name somewhere, like pedriatic instead of pediatic :(

        // if (typeof(IGeneratorConditionScript).IsAssignableFrom(type))
        if (typeof(IGeneratorConditionScript).IsAssignableFrom(type))    //checks if type implements the generator specific interface  //check if runs
        {
            var result = await RunConditionScript(type, scriptInstance, genContext);
            return (T)(object)result;
        }
        else if (typeof(IGeneratorActionScript).IsAssignableFrom(type))
        {
            var result = await RunActionScript(type, scriptInstance, genContext);
            return (T)(object)result;
        }
        else
        {
            _logger.LogInformation("Could not run your script because it is neither a ActionScript nor a ConditionScript.");
            throw new ScriptWasNotOfCorrectTypeException("Could not run your script because it is neither a ActionScript nor a ConditionScript.");
        }
    }

    public async Task<bool> RunConditionScript(Type type, object scriptInstance, GeneratorContextSF genContext)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(RunConditionScript), nameof(ScriptExecutor));
        try
        {
            // MethodInfo method = type.GetMethod("EvaluateAsync")!;
            MethodInfo method = type.GetMethod(nameof(IGeneratorConditionScript.EvaluateAsync))!;

            using var cts = new CancellationTokenSource(_scriptTimeout);
            ScriptEnvironment.CurrentToken.Value = cts.Token;

            var resultTask = (Task)method.Invoke(scriptInstance, new object[] { genContext })!;

            try
            {
                await resultTask.WaitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new ConditionScriptTimeoutException(nameof(RunConditionScript) + " script exceeded time limit and was safely terminated.");
            }
            finally
            {
                ScriptEnvironment.CurrentToken.Value = CancellationToken.None;
            }

            var resultProperty = resultTask.GetType().GetProperty("Result");
            var resultValue = resultProperty!.GetValue(resultTask);

            _logger.LogInformation($"Result: {resultValue}");
            return (bool)resultValue!;
        }
        catch (Exception e)
        {
            _logger.LogInformation(e.ToString());
            throw new ConditionScriptExecutionException("RunConditionScrptFailed.", e);
        }

    }
    public async Task<ActionResultSF> RunActionScript(Type type, object scriptInstance, GeneratorContextSF genContext)  //todo pass Method name as string or enum and also pass arguments maybe as List<args>
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(RunActionScript), nameof(ScriptExecutor));
        try
        {
            MethodInfo method = type.GetMethod(nameof(IGeneratorActionScript.ExecuteAsync))!;

            using var cts = new CancellationTokenSource(_scriptTimeout);
            ScriptEnvironment.CurrentToken.Value = cts.Token;

            var resultTask = (Task)method.Invoke(scriptInstance, new object[] { genContext })!;

            try
            {
                await resultTask.WaitAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new ActionScriptTimeoutException(nameof(RunConditionScript) + " script exceeded time limit and was safely terminated.");
            }
            finally
            {
                ScriptEnvironment.CurrentToken.Value = CancellationToken.None;
            }

            var resultProperty = resultTask.GetType().GetProperty("Result");
            var resultValue = resultProperty!.GetValue(resultTask);

            _logger.LogInformation($"Result in 86 sExecuter: {resultValue}");

            return (ActionResultSF)resultValue!;  //this might fail because not baseclass idk, if it does maybe change whole structure to only one function
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            _logger.LogWarning("You might have passed the wrong GeneratorContext class, ex V1 instead of V2");
            throw new ActionScriptExecutionException("You might have passed the wrong GeneratorContext class, ex V1 instead of V2", e);
        }

    }
}


public static class ScriptEnvironment
{
    public static readonly AsyncLocal<CancellationToken> CurrentToken = new();
}
