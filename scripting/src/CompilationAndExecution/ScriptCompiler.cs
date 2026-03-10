using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Ember.Scripting;

internal class ScriptCompiler
{
    private readonly List<MetadataReference>? References2 = null;
    private readonly ILogger<ScriptCompiler> Logger;

    private readonly List<MetadataReference> StandardRefrencesForAllScripts = [MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                        MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                        MetadataReference.CreateFromFile(typeof(Task<>).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(DateTime).Assembly.Location),
                        ];
    public ScriptCompiler(List<MetadataReference> references2, ILogger<ScriptCompiler> logger)
    {
        References2 = references2;
        Logger = logger;
    }
    public byte[] RunCompilation(string script, int? apiVersion = null, (string className, string baseTypeName, int versionInt)? metaData = null) //references param there to enable later on users to define custom references
    {
        try
        {
            References2!.AddRange(StandardRefrencesForAllScripts);

            List<MetadataReference>? references = [];

            Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(RunCompilation), nameof(ScriptCompiler));
            if (metaData != null)
            {
                Logger.LogTrace("Trying to compile script:" + metaData.Value.className);
            }

            //Takes C# source string and turns it into a "Roslyn parsed syntax tree representation" of a normal C# file
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(script);
            if (References2 != null)
            {
                Logger.LogInformation("References 2 added references in {MethodName}.", nameof(RunCompilation));
                references = References2;
            }
            if (apiVersion != null)
            {
                Logger.LogInformation("Added custom references in {MethodName}.", nameof(RunCompilation));  //if this works remove the if references is null if stat above
                references = GetReferencesForOldVersion((int)apiVersion);
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

            //The following if statement was AI Generated
            if (!emitResult.Success)
            {

                string? errors = string.Join(Environment.NewLine, emitResult.Diagnostics
                .Where(d => d.IsWarningAsError || d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
                // .Select(d => $"{d.Id}: {d.GetMessage()}"));
                .Select(d => $"Line {d.Location.GetLineSpan().StartLinePosition.Line + 1}, Col {d.Location.GetLineSpan().StartLinePosition.Character + 1}: {d.Id} - {d.GetMessage()}"));


                foreach (var diag in emitResult.Diagnostics)
                {
                    Logger.LogInformation(diag.ToString());
                }
                ;   //Iterates through the list of compiler messages
                Logger.LogError("Error in ScriptCompiler RunCompilation method compilation probably failed in if (!emitResult.Success)");
                throw new CompilationFailedException("Error in ScriptCompiler RunCompilation method compilation probably failed in if (!emitResult.Success) errors: " + errors);
            }

            byte[] assemblyBytes = ms.ToArray();
            return assemblyBytes;
        }
        catch (Exception e)
        {
            Logger.LogError(e.ToString());
            throw new CompilationFailedException("RunCompilation failed exception caught.", e);
        }

    }

    public INamedTypeSymbol GetBaseType(string script)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(script);   //being called twice also in RunCompilation() might be better for performance to remove twice but also you want to be able to compile without having to parse always
        var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        var compilation = CSharpCompilation.Create("MyCompilation",
            syntaxTrees: new[] { tree }, references: new[] { Mscorlib });   // i might be able to use this method to init the refrences also above?
        var model = compilation.GetSemanticModel(tree);
        var classesInTree = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
        if (classesInTree.Count() > 1)
        {
            Logger.LogError("More Than one class found in Script string.");
            throw new MoreThanOneClassFoundInScriptException("More Than one class found in Script string. in if (classesInTree.Count() > 1)"); //this might throw even if there is one class in script if compiler adds classes or something like that, so if it does maybe change this if statement
        }
        var myClass = classesInTree.Last();
        var myClassSymbol = model.GetDeclaredSymbol(myClass) as ITypeSymbol;
        var baseType = myClassSymbol!.BaseType!;
        return baseType;
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
                throw new MoreThanOneClassFoundInScriptException("More Than one class found in Script string. in if (classesInTree.Count() > 1)"); //this might throw even if there is one class in script if compiler adds classes or something like that, so if it does maybe change this if statement
            }
            var myClass = classesInTree.Last();
            var myClassSymbol = model.GetDeclaredSymbol(myClass) as ITypeSymbol;
            var baseTypeName = myClassSymbol!.BaseType!.Name;
            var className = myClassSymbol.ToString();
            var parentSymbol = myClassSymbol!.Interfaces.FirstOrDefault() ?? myClassSymbol.BaseType;
            int? versionInt = null;
            string implementedIntrf = parentSymbol.Name;

            string? contextParameterTypeName = null;

            var executeMethod = myClass.DescendantNodes()
                                       .OfType<MethodDeclarationSyntax>()
                                       .FirstOrDefault(m => m.Identifier.Text == "ExecuteAsync" || m.Identifier.Text == "EvaluateAsync");

            if (executeMethod != null)
            {
                var firstParameter = executeMethod.ParameterList.Parameters.FirstOrDefault();
                if (firstParameter != null && firstParameter.Type != null)
                {
                    contextParameterTypeName = firstParameter.Type.ToString();
                }
            }

            // Console.WriteLine("Extracted Context Parameter Type: " + contextParameterTypeName);
            Logger.LogTrace("Extracted Context Parameter Type: " + contextParameterTypeName);

            Dictionary<int, Type> activeContexts = ContextVersionScanner.GetInterfaceDictionary();
            // Console.WriteLine("Start of dict String:");
            Logger.LogTrace("Start of dict String:");
            foreach (var pair in activeContexts)
            {
                // Console.WriteLine("Key: " + pair.Key + ", Value: " + pair.Value);
                Logger.LogTrace("Key: " + pair.Key + ", Value: " + pair.Value);
            }

            foreach (var ctx in activeContexts)
            {
                string dictTypeFullName = ctx.Value.FullName ?? "";

                if (ctx.Value.FullName == contextParameterTypeName)
                {
                    if (versionInt != null)
                    {
                        throw new Exception("Context name occured more than once for some reason that should not happen.");
                    }
                    versionInt = ctx.Key;
                }
            }

            if (versionInt == null)
            {
                throw new Exception("Version int was null in BasicValidation");
            }

            if (versionInt == null)
            {
                throw new ValidationBeforeCompilationException("Version Int was not assigned probably because the foeach loop faliled maybe because the Context name of the script was not in the dictionary.");
            }
            if (baseTypeName == null || className == null)
            {
                Logger.LogError("BaseTypeName or classname null in BasicValidationBeforeCompiling.");
                throw new ClassNameOrBaseNameNullException("BaseTypeName or classname null in BasicValidationBeforeCompiling. in if (baseTypeName == null || className == null)");
            }
            Logger.LogTrace("Class Name = " + className);
            Logger.LogTrace("BaseClass Name = " + baseTypeName);
            Logger.LogTrace("Version Int = " + versionInt);

            ValidateNamespaceUsage(tree, model);
            return (className, baseTypeName, (int)versionInt!);
        }
        catch (Exception e)
        {
            Logger.LogError("BasicValidation failed:" + e.ToString());
            throw new ValidationBeforeCompilationException(e.ToString(), e);
        }

    }
    private void ValidateNamespaceUsage(SyntaxTree tree, SemanticModel model)
    {
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(ValidateNamespaceUsage), nameof(ScriptCompiler));
        List<string> illegalNamespaces = [

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
            ];
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
        //This function i quickly generated with AI
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(IsTheSameTree), nameof(ScriptCompiler));
        SyntaxTree tree1 = CSharpSyntaxTree.ParseText(script1);
        SyntaxTree tree2 = CSharpSyntaxTree.ParseText(script2);

        bool areSame = tree1.IsEquivalentTo(tree2, topLevel: false);

        return areSame;
    }

    public List<MetadataReference> GetReferencesForOldVersion(int version, List<string>? customDlls = null, bool loadCurrentRT = true)  //todo fix this
    {
        var stdReferences = StandardRefrencesForAllScripts;
        Logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetReferencesForOldVersion), nameof(ScriptCompiler));
        var references = new List<MetadataReference>();

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
        "Framework.dll",               // application assembly
        "System.Private.CoreLib.dll",
        "System.Runtime.dll",
        "System.Console.dll",
        "System.Collections.dll"
        ];

        if (loadCurrentRT)
        {
            Logger.LogTrace("loadCurrentRT true.");
            dllsToLoad = ["Framework.dll"];
            references.AddRange(stdReferences);
        }
        if (customDlls != null)
        {
            Logger.LogTrace("customDlls was not not null");
            foreach (var item in customDlls)
            {
                dllsToLoad.Add(item);
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

        return references;
    }
    // static void RemoveReferencesByAssemblyName(List<MetadataReference> refs, params string[] simpleNames)
    // {
    //     refs.RemoveAll(r =>
    //     {
    //         if (r is not PortableExecutableReference pe || string.IsNullOrWhiteSpace(pe.FilePath))
    //             return false;

    //         try
    //         {
    //             var an = AssemblyName.GetAssemblyName(pe.FilePath);
    //             return simpleNames.Any(n => string.Equals(n, an.Name, StringComparison.OrdinalIgnoreCase));
    //         }
    //         catch
    //         {
    //             return false; // ignore non-file refs
    //         }
    //     });
    // }
}
