using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Threading.Tasks;
using Ember.Scripting.Compilation;

namespace Ember.Scripting.Execution;

internal class ScriptExecutor
{
    private int _scriptTimeout = ((int)ExecutionTimeGroups.Medium);   // ms of how much time scripts get to execute
    private readonly ILogger<ScriptExecutor> _logger;
    public ScriptExecutor(ILogger<ScriptExecutor> logger)
    {
        _logger = logger;
    }

    public async Task<T> RunScriptExecution<T>(byte[] compiledScript, Context genContext, int? executionTime, string methodName)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(RunScriptExecution), nameof(ScriptExecutor));

        if (compiledScript.Length > 5 * 1024 * 1024) // 5 mb maximum size
        {
            throw new CompiledScriptWasTooLargeException(nameof(RunScriptExecution) + " failed in if (compiledScript.Length > 5 * 1024 * 1024)");
        }
        if (executionTime != null)
        {
            _scriptTimeout = (int)executionTime;
            Console.WriteLine("excecutionTime was null set to: " + _scriptTimeout);
        }

        Assembly assembly = Assembly.Load(compiledScript);

        Type[] unfilteredTypeArray = assembly.GetTypes();   //even though there can be only one class defined in the script file, the compiler adds classes making the array.lenght over 1 which is unsafe so it is better to filer based on our predefined classes for scripts
        List<Type> typeArrayList = [];
        for (int i = 0; i < unfilteredTypeArray.Length; i++)
        {
            if (typeof(IScriptMethod).IsAssignableFrom(unfilteredTypeArray[i]))
            {
                typeArrayList.Add(unfilteredTypeArray[i]);
            }
            // if (typeof(IScriptMethodsCondition).IsAssignableFrom(unfilteredTypeArray[i]))
            // {
            //     typeArrayList.Add(unfilteredTypeArray[i]);
            // }
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

        object scriptInstance = Activator.CreateInstance(type)!;

        try
        {
            MethodInfo method;
            method = type.GetMethod(methodName)!;

            using var cts = new CancellationTokenSource(_scriptTimeout);
            ScriptEnvironment.CurrentToken.Value = cts.Token;
            System.Threading.Tasks.Task? resultTask;
            try
            {
                resultTask = (Task)method.Invoke(scriptInstance, new object[] { genContext })!;
            }
            catch (NullReferenceException ex)
            {
                throw new CouldNotFindMethodException(message: "", innerException: ex);
            }

            try
            {
                await resultTask.WaitAsync(cts.Token);
            }
            catch (OperationCanceledException ex)
            {
                throw new ActionScriptTimeoutException(nameof(RunScriptExecution) + " script exceeded time limit and was safely terminated.", ex);
            }
            finally
            {
                ScriptEnvironment.CurrentToken.Value = CancellationToken.None;
            }

            var resultProperty = resultTask.GetType().GetProperty("Result");
            var resultValue = resultProperty!.GetValue(resultTask);

            _logger.LogInformation($"Result in 86 sExecuter: {resultValue}");

            return (T)resultValue!;
        }

        catch (Exception e)
        {
            _logger.LogError(e.ToString());
            _logger.LogWarning("You might have passed the wrong GeneratorContext class, ex V1 instead of V2");
            if (e.GetType() != typeof(CouldNotFindMethodException))
            {
                throw new ActionScriptExecutionException("You might have passed the wrong GeneratorContext class, ex V1 instead of V2", e);
            }
            else
            {
                throw;
            }
        }
    }
}

public static class ScriptEnvironment
{
    public static readonly AsyncLocal<CancellationToken> CurrentToken = new();
}
