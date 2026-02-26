using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Ember.Scripting;

public class ScriptCompiler
{
    private readonly MetadataReference[]? References2 = null;
    private readonly ILogger<ScriptCompiler> Logger;
    public ScriptCompiler(MetadataReference[] references2, ILogger<ScriptCompiler> logger)
    {
        References2 = references2;
        Logger = logger;
    }
    public byte[] RunCompilation(string script, MetadataReference[]? references = null, int apiVersion = -1, (string className, string baseTypeName, int versionInt)? metaData = null) //references param there to enable later on users to define custom references
    {
        try
        {
            Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(RunCompilation), nameof(ScriptCompiler));
            if (metaData != null)
            {
                Logger.LogTrace("Trying to compile script:" + metaData.Value.className);
            }

            //Takes C# source string and turns it into a "Roslyn parsed syntax tree representation" of a normal C# file
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(script);

            //create list of "external assamblies" that the script needs in oder to be able to compile and run, you need to manually add file paths
            // if (references == null)
            if (apiVersion == -1)
            {
                Logger.LogInformation("Added default references in {MethodName}.", nameof(RunCompilation));
                references = new MetadataReference[]
                     {
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // System.Private.CoreLib
                        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location), // System.Console
                        MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location), // System.Runtime
                        MetadataReference.CreateFromFile(typeof(Task<>).Assembly.Location), // System.Threading.Tasks
                        MetadataReference.CreateFromFile(typeof(DateTime).Assembly.Location), // System.DateTime
                        // References t custom interfaces
                        MetadataReference.CreateFromFile(typeof(IGeneratorConditionScript).Assembly.Location),
                        // MetadataReference.CreateFromFile(typeof(IGeneratorReadOnlyContext).Assembly.Location)   //try removing if works good i guess but still need to pass from sandbox
                     };
            }
            if (apiVersion != -1)
            {
                Logger.LogInformation("Added custom references in {MethodName}.", nameof(RunCompilation));  //if this works remove the if references is null if stat above
                references = GetReferencesForVersion(apiVersion);
            }
            if (References2 != null)
            {
                Logger.LogInformation("References 2 added references in {MethodName}.", nameof(RunCompilation));
                references = References2;
            }

            //this initiates the process of the compilation (does not produce bytes yet), it binds the source code with the refrences and the config options
            CSharpCompilation compilation = CSharpCompilation.Create(
                "MyDynamicAssembly",
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            //actual build phase, success emits raw bytes of a .dll file
            using var ms = new MemoryStream();
            var emitResult = compilation.Emit(ms);  //this line is the line that uses a lot of resources and time

            if (!emitResult.Success)
            {
                foreach (var diag in emitResult.Diagnostics)
                {
                    Logger.LogInformation(diag.ToString());
                }
                ;   //Iterates through the list of compiler messages
                Logger.LogError("Error in ScriptCompiler RunCompilation method compilation probably failed");
                throw new CompilationFailedException();
            }

            byte[] assemblyBytes = ms.ToArray();
            return assemblyBytes;
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            throw new CompilationFailedException();
        }

    }

    public (string className, string baseTypeName, int versionInt) BasicValidationBeforeCompiling(string script)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(BasicValidationBeforeCompiling), nameof(ScriptCompiler));
        try
        {

            //Does basic validation such as correct interface usage, or if only one class is in the script file.
            //Also extracts class name, type of the scrip(ex. Action) and verison of interface.

            //Gets base class name like IGeneratorScript and so on
            // Source - https://stackoverflow.com/a/33095466

            SyntaxTree tree = CSharpSyntaxTree.ParseText(script);   //being called twice also in RunCompilation() might be better for performance to remove twice but also you want to be able to compile without having to parse always
            var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("MyCompilation",
                syntaxTrees: new[] { tree }, references: new[] { Mscorlib });   // i might be able to use this method to init the refrences also above?
            var model = compilation.GetSemanticModel(tree);
            var classesInTree = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
            if (classesInTree.Count() > 1)
            {
                Logger.LogError("More Than one class found in Script string.");
                throw new MoreThanOneClassFoundInScriptException(); //this might throw even if there is one class in script if compiler adds classes or something like that, so if it does maybe change this if statement
            }
            var myClass = classesInTree.Last();
            var myClassSymbol = model.GetDeclaredSymbol(myClass) as ITypeSymbol;
            var baseTypeName = myClassSymbol!.BaseType!.Name;
            var className = myClassSymbol.ToString();
            int versionInt;

            if (myClassSymbol.BaseType?.TypeArguments.Length > 0)
            {
                var generatorContextType = myClassSymbol.BaseType.TypeArguments[0];
                var versionName = generatorContextType.Name;
                if (char.IsDigit(versionName[versionName.Length - 1]))
                {
                    versionInt = int.Parse(versionName[versionName.Length - 1].ToString());
                }
                else
                {
                    versionInt = 1;
                }
            }
            else
            {
                versionInt = 1;
            }

            if (baseTypeName == null || className == null)
            {
                Logger.LogError("BaseTypeName or classname null in BasicValidationBeforeCompiling.");
                throw new ClassNameOrBaseNameNullException();
            }
            Logger.LogTrace("Class Name = " + className);
            Logger.LogTrace("BaseClass Name = " + baseTypeName);
            Logger.LogTrace("Version Int = " + versionInt);

            ValidateNamespaceUsage(tree, model);
            return (className, baseTypeName, versionInt);
        }
        catch (Exception e)
        {
            Logger.LogError("BasicValidation failed:" + e.ToString());
            throw new ValidationBeforeCompilationException();
        }

    }
    private void ValidateNamespaceUsage(SyntaxTree tree, SemanticModel model)    //todo check
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(ValidateNamespaceUsage), nameof(ScriptCompiler));
        HashSet<string> illegalNamespaces = new() //no duplicates unlike list
            {
                "System.IO",
                "System.Net",
                "System.Net.Http",
                "System.Reflection",
                "System.Diagnostics",
                "System.Runtime.InteropServices",

                "System.Threading.Thread",
                "System.Threading.ThreadPool",
                "System.Threading.Timer",
                "System.Threading.Mutex",
                "System.Threading.Semaphore",
                "System.Threading.SemaphoreSlim",
            };
        var usings = tree.GetRoot()
            .DescendantNodes()
            .OfType<UsingDirectiveSyntax>()
            .Select(s => s.Name!.ToString());

        IEnumerable<IdentifierNameSyntax> identifiers = tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>();

        foreach (var illegal in illegalNamespaces)
        {
            foreach (var usingItem in usings)
            {
                if (illegal == usingItem || usingItem.StartsWith(illegal + "."))  //to prevent usage of for example System.IO and then System.IO.Compression
                {
                    Logger.LogError("Script tried to use a illegal namespace.");
                    throw new ForbiddenNamespaceException($"Script uses forbidden namespace: {illegal}");
                }
            }
            foreach (var id in identifiers) //to prevent system.io.file stuff like that
            {
                var symbol = model.GetSymbolInfo(id).Symbol;
                if (symbol != null)
                {
                    var nameSpaceEx = symbol.ContainingNamespace?.ToString();
                    if (nameSpaceEx != null && (illegal == nameSpaceEx || nameSpaceEx.StartsWith(illegal + ".")))
                    {
                        Logger.LogError("Script tried to use a illegal type.");
                        throw new ForbiddenNamespaceException($"Usage of forbidden type: {symbol.ToDisplayString()}");
                    }
                }
            }
        }
    }

    public bool IsTheSameTree(string script1, string script2)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(IsTheSameTree), nameof(ScriptCompiler));
        SyntaxTree tree1 = CSharpSyntaxTree.ParseText(script1);
        SyntaxTree tree2 = CSharpSyntaxTree.ParseText(script2);

        // AI: Check if they are structurally identical (ignoring whitespace/comments)
        bool areSame = tree1.IsEquivalentTo(tree2, topLevel: false);

        return areSame;
    }

    public MetadataReference[] GetReferencesForVersion(int version, string[]? customDlls = null, bool loadCurrentRT = true)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetReferencesForVersion), nameof(ScriptCompiler));
        var references = new List<MetadataReference>();

        // string versionPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OldVersions", version.ToString());
        // string tempPath = @"C:\Users\Gilles\Desktop\UNI\Semester 6\Code";
        string tempPath = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                         "..", "..", "..", "..", "..", "..")
        );

        string versionPath = Path.Combine(tempPath, "OldVersions", version.ToString());

        if (!Directory.Exists(versionPath))
        {
            Logger.LogError("Could not find references.");
            throw new Exception($"References for version {version} not found at {versionPath}");
        }

        List<string> dllsToLoad =
        [
        "Ember.dll",               // application assembly
        "System.Private.CoreLib.dll",
        "System.Runtime.dll",
        "System.Console.dll",
        "System.Collections.dll"
        ];

        if (loadCurrentRT)
        {
            Logger.LogTrace("loadCurrentRT true.");
            dllsToLoad = ["Ember.dll"];
            var stdRefs = new MetadataReference[]
            {
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // System.Private.CoreLib
                        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location), // System.Console
                        MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location), // System.Runtime
                        MetadataReference.CreateFromFile(typeof(Task<>).Assembly.Location), // System.Threading.Tasks
                        MetadataReference.CreateFromFile(typeof(DateTime).Assembly.Location), // System.DateTime
            };
            foreach (var item in stdRefs)
            {
                references.Add(item);
            }
        }
        if (customDlls != null)
        {
            Logger.LogTrace("customDlls was not not null");
            foreach (var item in customDlls)
            {
                dllsToLoad.Append(item);
            }
        }

        foreach (var dllName in dllsToLoad)
        {
            string fullPath = Path.Combine(versionPath, dllName);
            if (File.Exists(fullPath))
            {
                // Create reference from the file on disk, not the loaded assembly
                references.Add(MetadataReference.CreateFromFile(fullPath));
            }
        }

        return references.ToArray();
    }
}
