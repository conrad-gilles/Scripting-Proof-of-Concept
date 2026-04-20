using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;


namespace Ember.Scripting.Versioning;

public static class ScriptVersionScanner
{
    // public static Dictionary<int, Type> GetClassDictionary()
    // {
    //     Type baseType = typeof(MetaDataIGeneratorScript);
    //     return GetBaseTypeDictionary(baseType).Item1;
    // }
    public static List<ScriptMetaDataRecord> GetClassRecords()
    {
        Type baseType = typeof(MetaDataIGeneratorScript);
        return GetBaseTypeDictionary(baseType);
    }
    private static List<ScriptMetaDataRecord> GetBaseTypeDictionary(Type baseType)
    {
        Dictionary<int, Type> contextVersionMap = new();
        List<ScriptMetaDataRecord> records = [];

        //The following six lines were ai generated
        List<Type> subClasses = AppDomain.CurrentDomain.GetAssemblies()
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

            // Extract method information using standard Reflection
            List<MethodInfo> methodInfos = currentType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .ToList();

            List<MethodRecord> methodRecords = new List<MethodRecord>();

            foreach (var m in methodInfos)
            {
                // Note: For explicit interface implementations, m.Name contains dots (e.g., "IExecuteAsync.ExecuteAsync")
                // We split by '.' and take the last part so it cleanly matches Roslyn's parsed method name.
                string methodName = m.Name.Split('.').Last();
                string returnType = m.ReturnType.Name;
                ParameterInfo[] parameters = m.GetParameters();

                List<ParameterRecord> resultParams = new List<ParameterRecord>();
                foreach (ParameterInfo param in parameters)
                {
                    try
                    {
                        string paramName = param.Name!;
                        string paramType = param.ParameterType.Name;
                        resultParams.Add(new ParameterRecord { Name = paramName, ReturnType = paramType });
                    }
                    catch
                    {
                        continue;
                    }
                }
                methodRecords.Add(new MethodRecord { Name = methodName, ReturnType = returnType, Parameters = resultParams });
            }

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
                RetrievedType = currentType,
                ScriptType = currentType.FullName!,
                ContextType = contextType.FullName!,
                ActionResultType = arType.FullName!,
                Methods = methodRecords
            };
            records.Add(temp);

        }

        return records;
    }
}

public record ScriptMetaDataRecord
{
    public required int Version { get; init; }
    public required Type RetrievedType { get; init; }
    public required string ScriptType { get; init; }
    public required string ContextType { get; set; }
    public required string ActionResultType { get; set; }
    public required List<MethodRecord> Methods { get; init; }

    public override string ToString()
    {
        return "V" + Version + ", Type: " + ScriptType + ", Context: " + ContextType + ", AR: " + ActionResultType;
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

