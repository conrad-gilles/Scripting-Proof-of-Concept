// using System.Reflection;
// using Microsoft.CodeAnalysis;

// public class ReferenceLoader
// {
//     public MetadataReference[] GetReferencesForVersion(int version)
//     {
//         var references = new MetadataReference[]
//              {
//                         MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // System.Private.CoreLib
//                         MetadataReference.CreateFromFile(typeof(Console).Assembly.Location), // System.Console
//                         MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location), // System.Runtime
//                         MetadataReference.CreateFromFile(typeof(Task<>).Assembly.Location), // System.Threading.Tasks
//                         MetadataReference.CreateFromFile(typeof(DateTime).Assembly.Location), // System.DateTime
//                         // References t custom interfaces
//                         MetadataReference.CreateFromFile(typeof(IGeneratorConditionScript).Assembly.Location),    //todo check if runs
//                         MetadataReference.CreateFromFile(typeof(IGeneratorReadOnlyContext).Assembly.Location)
//              };
//         return references;
//     }


//     public MetadataReference[] GetReferencesForVersion2(int version, string[]? customDlls = null)
//     {
//         var references = new List<MetadataReference>();

//         // 1. Define path to the specific version folder
//         string versionPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OldVersions", version.ToString());

//         if (!Directory.Exists(versionPath))
//         {
//             throw new Exception($"References for version {version} not found at {versionPath}");
//         }

//         // 2. Identify the core DLLs your scripts need. 
//         // You likely need your main app executable/dll and standard .NET libs.
//         List<string> dllsToLoad =
//         [
//         "Ember.dll",               // Your application assembly
//         "System.Private.CoreLib.dll", // Core .NET types
//         "System.Runtime.dll",
//         "System.Console.dll",
//         "System.Collections.dll"
//         ];

//         if (customDlls != null)
//         {
//             foreach (var item in customDlls)
//             {
//                 dllsToLoad.Append(item);
//             }
//         }

//         foreach (var dllName in dllsToLoad)
//         {
//             string fullPath = Path.Combine(versionPath, dllName);
//             if (File.Exists(fullPath))
//             {
//                 // Create reference from the file on disk, not the loaded assembly
//                 references.Add(MetadataReference.CreateFromFile(fullPath));
//             }
//         }

//         return references.ToArray();
//     }

// }

