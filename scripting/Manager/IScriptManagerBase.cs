namespace Ember.Scripting.Manager;

/// <summary>
/// Defines the fundamental methods to work with the scripting system.
/// </summary>
public interface IScriptManagerBase
{
    /// <summary>
    /// Executes a script identified by its unique ID, automatically detecting and handling its specific script type.
    /// </summary>
    /// <param name="scriptId">The unique identifier of the script to execute.</param>
    /// <param name="context">The execution context or state to be passed into the script's method.</param>
    /// <param name="methodName">The name of the specific method to invoke within the compiled script.</param>
    /// <param name="currentApiVersion">The optional API version to target during execution. If null, the default or most recent version is used.</param>
    /// <returns>A task representing the asynchronous execution, containing the returned object from the script.</returns>
    Task<object> ExecuteScript(Guid scriptId, IContext context, string methodName, int? currentApiVersion = null);

    /// <summary>
    /// Executes a script identified by its name, enforcing a specific script type restriction.
    /// </summary>
    /// <typeparam name="ScriptType">The expected category or type of the script, which must implement <see cref="IScriptType"/>.</typeparam>
    /// <param name="name">The registered name of the script to execute.</param>
    /// <param name="context">The execution context or state to be passed into the script's method.</param>
    /// <param name="methodName">The name of the specific method to invoke within the compiled script.</param>
    /// <param name="currentApiVersion">The optional API version to target during execution. If null, the default or most recent version is used.</param>
    /// <returns>A task representing the asynchronous execution, containing the returned object from the script.</returns>
    Task<object> ExecuteScript<ScriptType>(string name, IContext context, string methodName, int? currentApiVersion = null) where ScriptType : IScriptType;
}


