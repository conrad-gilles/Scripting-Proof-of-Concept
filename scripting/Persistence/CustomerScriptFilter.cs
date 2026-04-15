namespace Ember.Scripting.Persistence;

public class CustomerScriptFilter
{
    public string? ScriptName;

    public string? ScriptType;

    public string? SourceCode;

    public int? MinApiVersion;

    public DateTime? CreatedAt;

    public DateTime? ModifiedAt;

    public string? CreatedBy;
    public CustomerScriptFilter(string? scriptName = null, string? scriptType = null, string? sourceCode = null, int? minApiVersion = null,
     DateTime? createdAt = null, DateTime? modifiedAt = null, string? createdBy = null)
    {
        ScriptName = scriptName;
        ScriptType = scriptType;
        SourceCode = sourceCode;
        MinApiVersion = minApiVersion;
        CreatedAt = createdAt;
        ModifiedAt = modifiedAt;
        CreatedBy = createdBy;
    }

}