using System.Reflection;
using System.Runtime.CompilerServices;

namespace Ember.Scripting.Versioning;

public static class ContextVersionScanner
{
    // Moved exactly as it was from ScriptFactory
    public static Dictionary<int, Type> GetClassDictionary()
    {
        Type baseType = typeof(Context);
        return GetBaseTypeDictionary(baseType);
    }

    public static Dictionary<int, Type> GetInterfaceDictionary()
    {
        Type baseType = typeof(IContext);
        return GetBaseTypeDictionaryIntrfc(baseType);
    }

    private static Dictionary<int, Type> GetBaseTypeDictionary(Type baseType)
    {
        Dictionary<int, Type> contextVersionMap = new();
        //the following 4 lines were ai generated
        var subClasses = AppDomain.CurrentDomain.GetAssemblies()
                    //    .SelectMany(assembly => assembly.GetTypes())
                    .SelectMany(assembly => VersionScannerHelper.GetLoadableTypes(assembly))
                   .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(baseType))
                   .ToList();

        for (int i = 0; i < subClasses.Count(); i++)
        {
            Type currentType = subClasses[i];
            var uninitializedContext = (IContext)RuntimeHelpers.GetUninitializedObject(currentType);

            System.Reflection.TypeInfo typeInfo = uninitializedContext.GetType().GetTypeInfo();

            var metaDataAttribute = typeInfo.GetCustomAttribute<MetaDataGeneratorClass>();
            if (metaDataAttribute == null)
            {
                throw new MetaDataAttribueNullCVSCException(message: "MetdaDataAttribute was mull, which means version would have been null.");
            }
            int version = metaDataAttribute.Version;

            if (contextVersionMap.Values.Contains(currentType))
            {
                throw new TypeMoreThanOnceInAssemblyCVSCException("Type was more than once in the assembly probably with more than 1 Version property");
            }
            if (contextVersionMap.ContainsKey(version))
            {
                throw new VersionIntMoreThanOnceInAssemblyCVSCException("Api version int more than once in the assembly should not happen.");
            }
            contextVersionMap.Add(version, currentType);
        }
        return contextVersionMap;
    }

    private static Dictionary<int, Type> GetBaseTypeDictionaryIntrfc(Type baseType)
    {
        Dictionary<int, Type> contextVersionMap = new();

        // var subClasses = AppDomain.CurrentDomain.GetAssemblies()
        //     .SelectMany(assembly => assembly.GetTypes())
        //     .Where(t => t.IsInterface && baseType.IsAssignableFrom(t) && t != baseType)
        //     .ToList();

        //AiGenerated Linq queries
        var subClasses = AppDomain.CurrentDomain.GetAssemblies()
           .SelectMany(assembly => VersionScannerHelper.GetLoadableTypes(assembly))
           .Where(t => t.IsInterface && baseType.IsAssignableFrom(t) && t != baseType)
           .ToList();

        for (int i = 0; i < subClasses.Count(); i++)
        {
            Type currentType = subClasses[i];

            var versionAttr = currentType.GetCustomAttribute<MetaDataIGeneratorIntrfc>();
            if (versionAttr == null)
            {
                throw new MetaDataAttribueNullCVSIException(message: nameof(versionAttr) + " was null, you probably forgot to put an attribute defining the version of the IGeneratorContext.");
            }
            int version = versionAttr.Version;

            if (contextVersionMap.Values.Contains(currentType))
            {
                throw new TypeMoreThanOnceInAssemblyCVSIException("Type was more than once in the assembly probably with more than 1 Version property");
            }
            if (contextVersionMap.ContainsKey(version))
            {
                throw new VersionIntMoreThanOnceInAssemblyCVSIException("Api version int more than once in the assembly should not happen.");
            }
            contextVersionMap.Add(version, currentType);
        }

        return contextVersionMap;
    }


}
