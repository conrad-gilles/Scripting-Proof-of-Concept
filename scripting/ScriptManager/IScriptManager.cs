namespace Ember.Scripting;

public interface IScriptManager
{
    /// <summary>
    /// Generic execution that detects script type automatically
    /// </summary>
    /// <param name="scriptId"></param>
    /// <param name="context"></param>
    /// <param name="currentApiVersion"></param>
    /// <returns></returns>
    Task<object> ExecuteScriptById(Guid scriptId, Context context, int? currentApiVersion = null, string? methodName = null);

    Task<object> ExecuteScriptByNameAndType<ScriptType>(string name, Context context, int? currentApiVersion = null, string? methodName = null) where ScriptType : IScriptType;
}


