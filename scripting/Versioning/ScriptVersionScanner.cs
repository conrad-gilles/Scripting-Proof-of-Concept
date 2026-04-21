using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;


namespace Ember.Scripting.Versioning;

public static class ScriptVersionScanner
{
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
            Type contextType = typeof(IContext);

            List<MethodInfo> methodInfos = currentType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .ToList();

            List<MethodRecord> methodRecords = new List<MethodRecord>();

            foreach (var m in methodInfos)
            {
                string methodName = m.Name.Split('.').Last();
                Type retType = m.ReturnType;
                if (retType.IsGenericType)
                {
                    retType = retType.GetGenericArguments()[0];
                }
                string returnType = GetCSharpTypeName(retType);
                ParameterInfo[] parameters = m.GetParameters();

                List<ParameterRecord> resultParams = new List<ParameterRecord>();
                foreach (ParameterInfo param in parameters)
                {
                    try
                    {
                        string paramName = param.Name!;
                        Type pType = param.ParameterType;

                        if (pType.IsGenericType)
                        {
                            pType = pType.GetGenericArguments()[0];
                        }
                        if (typeof(IContext).IsAssignableFrom(pType))
                        {
                            contextType = pType;
                        }
                        string paramType = GetCSharpTypeName(pType);
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

            if (contextType == typeof(IContext))
            {
                // throw new NoContextDefinedException();  // currently throws exception if there is no method with atleast 1 context as a parameter, i am now just checking in the compiler and setting version to 1
            }
            ScriptMetaDataRecord temp = new ScriptMetaDataRecord
            {
                Version = version,
                RetrievedType = currentType,
                ScriptType = currentType.FullName!,
                ContextType = contextType.FullName!,
                Methods = methodRecords
            };
            records.Add(temp);

        }

        return records;
    }
    // following dictionary and method were Ai generated
    // Cache the dictionary so it isn't recreated on every method call
    private static readonly Dictionary<Type, string> CSharpKeywords = new()
    {
        { typeof(void), "void" },
        { typeof(object), "object" },
        { typeof(string), "string" },
        { typeof(bool), "bool" },
        { typeof(byte), "byte" },
        { typeof(sbyte), "sbyte" },
        { typeof(char), "char" },
        { typeof(short), "short" },
        { typeof(ushort), "ushort" },
        { typeof(int), "int" },
        { typeof(uint), "uint" },
        { typeof(long), "long" },
        { typeof(ulong), "ulong" },
        { typeof(float), "float" },
        { typeof(double), "double" },
        { typeof(decimal), "decimal" },
        { typeof(IntPtr), "nint" },   // C# 9+ native int
        { typeof(UIntPtr), "nuint" }  // C# 9+ native unsigned int
    };

    private static string GetCSharpTypeName(Type type)
    {
        // 1. Handle Arrays (e.g., string[] instead of System.String[])
        if (type.IsArray)
        {
            Type elementType = type.GetElementType()!;
            int rank = type.GetArrayRank();
            string commas = new string(',', rank - 1);
            return $"{GetCSharpTypeName(elementType)}[{commas}]";
        }

        // 2. Handle Nullable Value Types (e.g., int? instead of System.Nullable`1)
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            Type underlyingType = Nullable.GetUnderlyingType(type)!;
            return $"{GetCSharpTypeName(underlyingType)}?";
        }

        // 3. Handle C# built-in alias keywords
        if (CSharpKeywords.TryGetValue(type, out string? keyword))
        {
            return keyword;
        }

        // 4. Fallback for custom objects, structs, or enums (e.g., ActionResultV3.ActionResult)
        return type.FullName ?? type.Name;
    }
}

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

