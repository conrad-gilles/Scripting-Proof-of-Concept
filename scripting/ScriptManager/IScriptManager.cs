namespace Ember.Scripting.ScriptManager;

public interface IScriptManager
{
    /// <summary>
    /// Generic execution that detects script type automatically
    /// </summary>
    /// <param name="scriptId"></param>
    /// <param name="context"></param>
    /// <param name="currentApiVersion"></param>
    /// <returns></returns>
    Task<object> ExecuteScriptById(Guid scriptId, Context context, string methodName, int? currentApiVersion = null);

    Task<object> ExecuteScriptByNameAndType<ScriptType>(string name, Context context, string methodName, int? currentApiVersion = null) where ScriptType : IScriptType;
}


