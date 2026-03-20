using System.Net.Http.Headers;
using System.Reflection;

namespace Ember.Scripting;

public static class ScriptVersionScanner
{
    public static Dictionary<int, Type> GetClassDictionary()
    {
        Type baseType = typeof(Ember.Scripting.MetaDataIGeneratorScript);
        return GetBaseTypeDictionary(baseType);
    }

    private static Dictionary<int, Type> GetBaseTypeDictionary(Type baseType)
    {
        Dictionary<int, Type> contextVersionMap = new();

        //The following six lines were ai generated
        var subClasses = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
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
                throw new Exception(message: "MetdaDataAttribute was mull, which means version would have been null.");
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

            if (contextVersionMap.Values.Contains(currentType))
            {
                throw new Exception("Type was more than once in the assembly probably with more than 1 Version property");
            }
            if (contextVersionMap.ContainsKey(version))
            {
                throw new Exception("Api version int more than once in the assembly should not happen.");
            }

            // Console.WriteLine("Version: " + version + ", CurrentType: " + currentType.Name + " , Type: " + metaDataAttribute.Type + " , ReturnType: " + metaDataAttribute.ReturnType);
            contextVersionMap.Add(version, currentType);
        }
        return contextVersionMap;
    }
}