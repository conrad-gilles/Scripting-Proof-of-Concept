using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Ember.Scripting.Execution;

internal class ScriptExecutor(ILogger<ScriptExecutor> logger)
{
    private readonly int _scriptTimeout = ((int)ExecutionTimeGroups.Medium);   // ms of how much time scripts get to execute
    private readonly int maxScriptLenght = 5 * 1024 * 1024;     // 5 mb maximum size

    internal async Task<object> RunScriptExecution(byte[] compiledScript, IContext genContext, int? executionTime, string methodName)
    {
        logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(RunScriptExecution), nameof(ScriptExecutor));
        int scriptTimeout = _scriptTimeout;

        if (compiledScript.Length > maxScriptLenght)
        {
            throw new CompiledScriptWasTooLargeException(nameof(RunScriptExecution) + " failed in if (compiledScript.Length > 5 * 1024 * 1024)");
        }
        if (executionTime != null)
        {
            scriptTimeout = executionTime.Value;
            logger.LogTrace("excecutionTime was null set to: " + scriptTimeout);
        }

        Assembly assembly = Assembly.Load(compiledScript);

        Type type;
        try
        {
            type = assembly.GetTypes()
                .Single(t => t.IsClass && !t.IsAbstract && typeof(IScriptVersion).IsAssignableFrom(t));
        }
        catch (InvalidOperationException)
        {
            var matches = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(IScriptVersion).IsAssignableFrom(t))
                .ToList();

            if (matches.Count == 0)
            {
                throw new NoClassFoundInScriptFileException(nameof(RunScriptExecution) + "failed in if (typeArray.Length == 0)");
            }

            logger.LogInformation("More than one class found in script");
            throw new MoreThanOneClassFoundInScriptExecutionException("More than one class found in script");
        }

        object scriptInstance = Activator.CreateInstance(type)!;

        try
        {
            MethodInfo method;
            method = type.GetMethod(methodName)!;

            using var cts = new CancellationTokenSource(scriptTimeout);
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

            logger.LogInformation($"Result in 86 sExecuter: {resultValue}");

            return resultValue!;
        }
        catch (CouldNotFindMethodException e)
        {
            logger.LogError(e.ToString());
            logger.LogWarning("You might have passed the wrong GeneratorContext class, ex V1 instead of V2");
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e.ToString());
            logger.LogWarning("You might have passed the wrong GeneratorContext class, ex V1 instead of V2");
            throw new ActionScriptExecutionException("Something went wrong when trying to exexute the script. Message: " + e.Message, e);
        }
    }
}

public static class ScriptEnvironment
{
    public static readonly AsyncLocal<CancellationToken> CurrentToken = new();
}
