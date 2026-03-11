[AttributeUsage(AttributeTargets.Interface)]
public class MetaDataIGeneratorScript : Attribute
{
    public int Version { get; }
    public IGeneratorScriptType Type { get; }
    public IGeneratorScriptReturnType ReturnType { get; }
    public MetaDataIGeneratorScript(int version, IGeneratorScriptReturnType returnType, IGeneratorScriptType type)
    {
        Version = version;
        ReturnType = returnType;
        Type = IGeneratorScriptType.DefaultVersioned;
    }

    public MetaDataIGeneratorScript(int version)
    {
        Version = version;
        ReturnType = IGeneratorScriptReturnType.Action;
        Type = IGeneratorScriptType.DefaultVersioned;
    }
}

public enum IGeneratorScriptType
{
    AbstractBaseInSF,
    GenericSimple,
    Generic,
    DefaultVersioned
}
public enum IGeneratorScriptReturnType
{
    Condition, Action
}