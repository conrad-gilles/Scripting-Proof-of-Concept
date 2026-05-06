using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Ember.Scripting.Execution;

/// <summary>
/// Responsible for safely loading and executing compiled script (in the form of byte[]).
/// </summary>
/// <param name="logger">The logger instance.</param>
internal class ScriptExecutor(ILogger<ScriptExecutor> logger)
{
    private readonly int _scriptTimeout = ((int)ExecutionTimeGroups.Medium);   // ms of how much time scripts get to execute

    /// <summary>
    /// Loads a compiled script assembly, instantiates its class, and invokes a specified asynchronous method while enforcing size limits and execution timeouts.
    /// </summary>
    /// <param name="compiledScript">The raw byte array of the compiled script.</param>
    /// <param name="genContext">The context object that is passed as an argument to the script's method.</param>
    /// <param name="executionTime">An optional custom timeout in milliseconds. If null, the default medium execution time is applied.</param>
    /// <param name="methodName">The name of the method to execute within the instantiated script class.</param>
    /// <returns>The result object extracted from the dynamically invoked method's returned Task.</returns>
    /// <exception cref="CompiledScriptWasTooLargeException">Thrown when the compiled script size exceeds the 5 MB limit.</exception>
    /// <exception cref="NoClassFoundInScriptFileException">Thrown when no non-abstract class implementing <see cref="IScriptVersion"/> exists in the assembly.</exception>
    /// <exception cref="MoreThanOneClassFoundInScriptExecutionException">Thrown when multiple valid script classes are found, making the target ambiguous.</exception>
    /// <exception cref="CouldNotFindMethodException">Thrown when the requested method name cannot be found or invoked on the script instance.</exception>
    /// <exception cref="ActionScriptTimeoutException">Thrown when the script execution exceeds the allotted timeout and is safely terminated.</exception>
    /// <exception cref="ActionScriptExecutionException">Thrown when a general exception or fault occurs during script invocation.</exception>
    internal async Task<object> RunScriptExecution(byte[] compiledScript, IContext genContext, int? executionTime, string methodName)
    {
        logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(RunScriptExecution), nameof(ScriptExecutor));
        int scriptTimeout = _scriptTimeout;

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
    /// <summary>
    /// Cancellation token used to enforce timeouts and allow scripts to gracefully abort runaway operations.
    /// </summary>
    public static readonly AsyncLocal<CancellationToken> CurrentToken = new();
}
