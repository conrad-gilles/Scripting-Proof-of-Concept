using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Ember.Scripting.Versioning;

public static class UpgradeManager
{
    public static IUpgradeableReturnValue UpgradeCustomReturn(object resultValue)
    {
        IUpgradeableReturnValue current = (IUpgradeableReturnValue)resultValue;

        MetaDataCustomReturn? metaDataAttribute = current
            .GetType()
            .GetTypeInfo()
            .GetCustomAttribute<MetaDataCustomReturn>();

        if (metaDataAttribute == null)
        {
            throw new Exception("MetaDataCustomReturn attribute was null.");
        }

        Dictionary<Type, List<ScannerRecord>> dict = GetClassDictionary();

        List<ScannerRecord>? foundList = null;
        foreach (var abstractBase in dict)
        {
            if (abstractBase.Key.IsAssignableFrom(current.GetType()))
            {
                foundList = abstractBase.Value;
                break;
            }
        }

        if (foundList == null || foundList.Count == 0)
        {
            throw new Exception($"No version list found for type {current.GetType().FullName}.");
        }

        List<ScannerRecord> ordered = foundList.OrderBy(r => r.Version).ToList();
        ScannerRecord maxRecord = ordered.Last();

        while (current.GetType() != maxRecord.CustomReturnType)
        {
            int currentVersion = current
                .GetType()
                .GetTypeInfo()
                .GetCustomAttribute<MetaDataCustomReturn>()!
                .Version;

            ScannerRecord? nextRecord = ordered.FirstOrDefault(r => r.Version > currentVersion);

            if (nextRecord == null)
            {
                throw new Exception($"No next version found after version {currentVersion} for {current.GetType().FullName}.");
            }

            IUpgradeableReturnValue uninitializedNext = (IUpgradeableReturnValue)RuntimeHelpers.GetUninitializedObject(nextRecord.CustomReturnType);

            try
            {
                current = (IUpgradeableReturnValue)uninitializedNext.Upgrade(current);
            }
            catch (TargetInvocationException ex)
            {
                throw new Exception(
                    $"Failed to upgrade from {current.GetType().Name} to {nextRecord.CustomReturnType.Name}.",
                    ex.InnerException);
            }
        }
        return current;
    }

    public static Dictionary<Type, List<ScannerRecord>> GetClassDictionary()
    {
        Type baseType = typeof(CustomReturnType);

        Dictionary<Type, List<ScannerRecord>> returnDict = new();

        List<Type> abstractSubClasses = AppDomain.CurrentDomain.GetAssemblies()
                   .SelectMany(assembly => assembly.GetTypes())
                   .Where(t => t.IsClass && t.IsAbstract && t.IsSubclassOf(baseType))
                   .ToList();

        for (int i = 0; i < abstractSubClasses.Count(); i++)
        {
            List<Type> subClasses = AppDomain.CurrentDomain.GetAssemblies()
                               .SelectMany(assembly => assembly.GetTypes())
                               .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(abstractSubClasses[i]))
                               .ToList();

            List<ScannerRecord> subTypes = [];

            for (int k = 0; k < subClasses.Count(); k++)
            {

                Type currentType = subClasses[k];
                object uninitializedContext = System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(currentType);

                System.Reflection.TypeInfo typeInfo = uninitializedContext.GetType().GetTypeInfo();

                MetaDataCustomReturn? metaDataAttribute = typeInfo.GetCustomAttribute<MetaDataCustomReturn>();

                if (metaDataAttribute == null)
                {
                    throw new ActionResultVSNullException(message: "MetdaDataAttribute was mull, which means version would have been null.");
                }
                if (returnDict.ContainsKey(abstractSubClasses[i]))
                {
                    throw new VersionIntMoreThanOnceInAssemblyARVSException("Api version int more than once in the assembly should not happen.");
                }
                // contextVersionMap.Add(version, currentType);
                ScannerRecord tempRecord = new ScannerRecord
                {
                    // AbstractBase = abstractSubClasses[i],
                    Version = metaDataAttribute.Version,
                    CustomReturnType = currentType
                };
                subTypes.Add(tempRecord);
            }
            returnDict.Add(abstractSubClasses[i], subTypes);
        }
        return returnDict;
    }
}

public record ScannerRecord
{
    // public required Type AbstractBase { get; init; }
    public required int Version { get; init; }
    public required Type CustomReturnType { get; init; }

    public override string ToString()
    {
        return
        //  nameof(AbstractBase) + ": " + AbstractBase.FullName + 
        ", Version: " + Version + ", CustomReturnType: " + CustomReturnType;
    }
}