using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;

public class ScriptCompiler
{
    // public string script;

    // public ScriptCompiler(string pScript)
    // {
    //     script = pScript;
    // }

    public byte[] RunCompilation(string script, MetadataReference[]? references = null, int apiVersion = -1) //references param there to enable later on users to define custom references
    {
        try
        {
            //Takes C# source string and turns it into a "Roslyn parsed syntax tree representation" of a normal C# file
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(script);

            //create list of "external assamblies" that the script needs in oder to be able to compile and run, you need to manually add file paths
            // if (references == null)
            if (apiVersion == -1)
            {
                Console.WriteLine("Added default references!");
                references = new MetadataReference[]
                     {
                        MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // System.Private.CoreLib
                        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location), // System.Console
                        MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location), // System.Runtime
                        MetadataReference.CreateFromFile(typeof(Task<>).Assembly.Location), // System.Threading.Tasks
                        MetadataReference.CreateFromFile(typeof(DateTime).Assembly.Location), // System.DateTime
                        // References t custom interfaces
                        MetadataReference.CreateFromFile(typeof(IGeneratorConditionScript).Assembly.Location),    //todo check if runs
                        MetadataReference.CreateFromFile(typeof(IGeneratorReadOnlyContext).Assembly.Location)
                     };
            }
            if (apiVersion != -1)
            {
                Console.WriteLine("Added custom references!");  //if this works remove the if references is null if stat above
                references = GetReferencesForVersion(apiVersion);
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
                // Print why it failed (e.g., missing references)
                foreach (var diag in emitResult.Diagnostics) Console.WriteLine(diag);   //Iterates through the list of compiler messages
                Console.WriteLine("Error in ScriptCompiler RunCompilation method compilation probably failed");
                throw new CompilationFailedException();
            }

            byte[] assemblyBytes = ms.ToArray();
            return assemblyBytes;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            throw new CompilationFailedException();
        }

    }

    public (string className, string baseTypeName, int versionInt) BasicValidationBeforeCompiling(string script)
    {
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
                throw new MoreThanOneClassFoundInScriptException(); //this might throw even if there is one class in script if compiler adds classes or something like that, so if it does maybe change this if statement
            }
            var myClass = classesInTree.Last();
            var myClassSymbol = model.GetDeclaredSymbol(myClass) as ITypeSymbol;
            var baseTypeName = myClassSymbol.BaseType.Name;
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
                Console.WriteLine("BaseTypeName or classname null in BasicValidationBeforeCompiling.");
                throw new ClassNameOrBaseNameNullException();
            }
            // Console.WriteLine("Class Name = " + className);
            // Console.WriteLine("BaseClass Name = " + baseTypeName);
            // Console.WriteLine("Version Int = " + versionInt);

            return (className, baseTypeName, versionInt);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            Console.WriteLine("BasicValidationBeforeCompiling() failed!");
            throw new ValidationBeforeCompilationException();
        }

    }
    public bool IsTheSameTree(string script1, string script2)
    {
        SyntaxTree tree1 = CSharpSyntaxTree.ParseText(script1);
        SyntaxTree tree2 = CSharpSyntaxTree.ParseText(script2);

        // AI: Check if they are structurally identical (ignoring whitespace/comments)
        bool areSame = tree1.IsEquivalentTo(tree2, topLevel: false);

        return areSame;
    }

    public MetadataReference[] GetReferencesForVersion(int version, string[]? customDlls = null, bool loadCurrentRT = true)
    {
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