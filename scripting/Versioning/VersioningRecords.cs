namespace Ember.Scripting.Versioning;

public record ScriptMetaDataRecord
{
    public required int Version { get; init; }
    public required Type RetrievedType { get; init; }
    public required string ScriptType { get; init; }
    public required string ContextType { get; set; }
    // public required string ActionResultType { get; set; }
    public required List<MethodRecord> Methods { get; init; }

    public override string ToString()
    {
        return "V" + Version + ", Type: " + ScriptType
         + ", Context: " + ContextType;
        //   + ", AR: " + ActionResultType;
    }
}