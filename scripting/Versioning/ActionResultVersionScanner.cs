using System.Net.Http.Headers;
using System.Reflection;

namespace Ember.Scripting;

public static class ActionResultVersionScanner
{
    public static Dictionary<int, Type> GetClassDictionary()
    {
        Type baseType = typeof(Ember.Scripting.ActionResultSF);
        return GetBaseTypeDictionary(baseType);
    }

    private static Dictionary<int, Type> GetBaseTypeDictionary(Type baseType)
    {
        Dictionary<int, Type> contextVersionMap = new();
        var subClasses = AppDomain.CurrentDomain.GetAssemblies()
                   .SelectMany(assembly => assembly.GetTypes())
                   .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(baseType))
                   .ToList();

        for (int i = 0; i < subClasses.Count(); i++)
        {
            Type currentType = subClasses[i];
            var uninitializedContext = (Ember.Scripting.ActionResultSF)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(currentType);

            System.Reflection.TypeInfo typeInfo = uninitializedContext.GetType().GetTypeInfo();

            var metaDataAttribute = typeInfo.GetCustomAttribute<MetaDataActionResult>();
            if (metaDataAttribute == null)
            {
                throw new ActionResultVSNullException(message: "MetdaDataAttribute was mull, which means version would have been null.");
            }
            int version = metaDataAttribute.Version;

            if (contextVersionMap.Values.Contains(currentType))
            {
                throw new TypeMoreThanOnceInAssemblyARVSException("Type was more than once in the assembly probably with more than 1 Version property");
            }
            if (contextVersionMap.ContainsKey(version))
            {
                throw new VersionIntMoreThanOnceInAssemblyARVSException("Api version int more than once in the assembly should not happen.");
            }
            contextVersionMap.Add(version, currentType);
        }
        return contextVersionMap;
    }
}

public static class VersionScannerHelper
{
    //Ai generated to fix a bug when running bUnit tests
    public static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (System.Reflection.ReflectionTypeLoadException e)
        {
            // e.Types contains the types that were successfully loaded, 
            // but it may also contain nulls for the ones that failed.
            return e.Types.Where(t => t != null)!;
        }
    }
}