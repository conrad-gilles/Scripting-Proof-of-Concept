namespace Ember.Scripting.Versioning;

[AttributeUsage(AttributeTargets.Interface)]
public class MetaDataIGeneratorScript : Attribute
{
    public int Version { get; }
    public IGeneratorScriptType Type { get; }
    public IGeneratorScriptReturnType ReturnType { get; }
    public Type ActionResultVersion { get; } //todo maybe pass type string idk
    public Type ContextVersion { get; }
    public MetaDataIGeneratorScript(int version, IGeneratorScriptReturnType returnType, IGeneratorScriptType type)
    {
        Version = version;
        ReturnType = returnType;
        Type = type;
        ContextVersion = typeof(IContext);
        ActionResultVersion = typeof(CustomReturnType);
        if (ReturnType == IGeneratorScriptReturnType.Condition)
        {
            ActionResultVersion = typeof(bool);
        }
    }
    public MetaDataIGeneratorScript(int version, IGeneratorScriptReturnType returnType, IGeneratorScriptType type, Type contextVersion, Type actionResultVersion)
    {
        Version = version;
        ReturnType = returnType;
        Type = type;
        ContextVersion = contextVersion;
        ActionResultVersion = actionResultVersion;
        if (ReturnType == IGeneratorScriptReturnType.Condition)
        {
            ActionResultVersion = typeof(bool);
        }
    }

    public MetaDataIGeneratorScript(int version)
    {
        Version = version;
        ReturnType = IGeneratorScriptReturnType.Action;
        Type = IGeneratorScriptType.DefaultVersioned;
        ContextVersion = typeof(IContext);
        ActionResultVersion = typeof(CustomReturnType);
        if (ReturnType == IGeneratorScriptReturnType.Condition)
        {
            ActionResultVersion = typeof(bool);
        }
    }
    public MetaDataIGeneratorScript(int version, Type contextVersion, Type actionResultVersion)
    {
        Version = version;
        ReturnType = IGeneratorScriptReturnType.Action;
        Type = IGeneratorScriptType.DefaultVersioned;
        ContextVersion = contextVersion;
        ActionResultVersion = actionResultVersion;
        if (ReturnType == IGeneratorScriptReturnType.Condition)
        {
            ActionResultVersion = typeof(bool);
        }
    }
    //     private int GetVersionFromContext()
    //     {
    // retu
    //     }
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

[AttributeUsage(AttributeTargets.Class)]
public class MetaDataActionResult : Attribute
{
    public int Version { get; }
    public TypeInfo Type { get; }

    public MetaDataActionResult(int version)
    {
        Version = version;
        Type = TypeInfo.DefaultVersioned;
    }

    public MetaDataActionResult(int version, TypeInfo type)
    {
        Version = version;
        Type = type;
    }

}

public enum TypeInfo
{
    AbstractBaseInSF,
    DefaultVersioned
}


[AttributeUsage(AttributeTargets.Class)]
public class MetaDataGeneratorClass : Attribute
{
    public int Version { get; }
    public TypeInfo Type { get; }

    public MetaDataGeneratorClass(int version)
    {
        Version = version;
        Type = TypeInfo.DefaultVersioned;
    }

    public MetaDataGeneratorClass(int version, TypeInfo type)
    {
        Version = version;
        Type = type;
    }
}

[AttributeUsage(AttributeTargets.Interface)]
public class MetaDataIGeneratorIntrfc : Attribute
{
    public int Version { get; }
    public TypeInfo Type { get; }

    public MetaDataIGeneratorIntrfc(int version)
    {
        Version = version;
        Type = TypeInfo.DefaultVersioned;
    }

    public MetaDataIGeneratorIntrfc(int version, TypeInfo type)
    {
        Version = version;
        Type = type;
    }
}
