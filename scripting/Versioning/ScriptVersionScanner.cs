using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace Ember.Scripting.Versioning;

public static class ScriptVersionScanner
{
    public static Dictionary<int, Type> GetClassDictionary()
    {
        Type baseType = typeof(MetaDataIGeneratorScript);
        return GetBaseTypeDictionary(baseType).Item1;
    }
    public static List<ScriptMetaDataRecord> GetClassRecords()
    {
        Type baseType = typeof(MetaDataIGeneratorScript);
        return GetBaseTypeDictionary(baseType).Item2;
    }
    private static (Dictionary<int, Type>, List<ScriptMetaDataRecord>) GetBaseTypeDictionary(Type baseType)
    {
        Dictionary<int, Type> contextVersionMap = new();
        List<ScriptMetaDataRecord> records = [];

        //The following six lines were ai generated
        var subClasses = AppDomain.CurrentDomain.GetAssemblies()
            // .SelectMany(assembly => assembly.GetTypes())
            .SelectMany(assembly => VersionScannerHelper.GetLoadableTypes(assembly))
            .Where(t => t.IsInterface && t.GetCustomAttribute<MetaDataIGeneratorScript>() != null)
            .GroupBy(t => t.Namespace + "." + t.Name)
            .Select(g => g.First())
            .ToList();

        for (int i = 0; i < subClasses.Count(); i++)
        {
            Type currentType = subClasses[i];

            var metaDataAttribute = currentType.GetCustomAttribute<MetaDataIGeneratorScript>();

            if (metaDataAttribute == null)
            {
                throw new MetaDataAttribueNullSVSException(message: nameof(metaDataAttribute) + " was mull, which means version would have been null.");
            }
            if (metaDataAttribute.Type == IGeneratorScriptType.AbstractBaseInSF
            || metaDataAttribute.Type == IGeneratorScriptType.Generic
            )
            {
                // throw new Exception(message: "Loop tried to instantiate a base type which it should not.");
                continue;
            }
            if (metaDataAttribute.Type == IGeneratorScriptType.GenericSimple)
            {
                if (metaDataAttribute.ReturnType == IGeneratorScriptReturnType.Action)
                {
                    continue;
                }
            }

            int version = metaDataAttribute.Version;
            Type scriptType = currentType;
            Type contextType = metaDataAttribute.ContextVersion;
            Type arType = metaDataAttribute.ActionResultVersion;

            if (contextVersionMap.Values.Contains(currentType))
            {
                throw new TypeMoreThanOnceInAssemblySVSException("Type was more than once in the assembly probably with more than 1 Version property");
            }
            if (contextVersionMap.ContainsKey(version))
            {
                throw new VersionIntMoreThanOnceInAssemblySVSException("Api version int more than once in the assembly should not happen.");
            }

            // Console.WriteLine("Version: " + version + ", CurrentType: " + currentType.Name + " , Type: " + metaDataAttribute.Type + " , ReturnType: " + metaDataAttribute.ReturnType);
            contextVersionMap.Add(version, currentType);
            ScriptMetaDataRecord temp = new ScriptMetaDataRecord
            {
                Version = version,
                ScriptType = currentType.FullName!,
                ContextType = contextType.FullName!,
                ActionResultType = arType.FullName!
            };
            records.Add(temp);

        }

        return (contextVersionMap, records);
    }
}

public record ScriptMetaDataRecord
{
    public required int Version { get; init; }
    public required string ScriptType { get; init; }
    public required string ContextType { get; init; }
    public required string ActionResultType { get; init; }

    public override string ToString()
    {
        return "V" + Version + ", Type: " + ScriptType + ", Context: " + ContextType + ", AR: " + ActionResultType;
    }
}