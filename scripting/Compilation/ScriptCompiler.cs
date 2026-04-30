using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Ember.Scripting.Compilation;

internal class ScriptCompiler
{
    private readonly List<MetadataReference> _allReferences = [];
    private readonly ILogger<ScriptCompiler> _logger;
    private readonly List<Type> _recentTypes;

    private readonly List<MetadataReference> _standardReferencesForAllScripts = [MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                        MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                        MetadataReference.CreateFromFile(typeof(Task<>).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(DateTime).Assembly.Location),

                        MetadataReference.CreateFromFile(typeof(System.Threading.AsyncLocal<>).Assembly.Location),
                        MetadataReference.CreateFromFile(typeof(System.Threading.CancellationToken).Assembly.Location),
                        MetadataReference.CreateFromFile(Assembly.Load("System.Threading").Location),
                        MetadataReference.CreateFromFile(Assembly.Load("System.Threading.Tasks").Location),
                        MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location)
                        ];
    public ScriptCompiler(List<MetadataReference> hostReferences, ILogger<ScriptCompiler> logger, List<Type> recentTypes)
    {
        _allReferences.AddRange(_standardReferencesForAllScripts);
        _allReferences.AddRange(hostReferences);
        _logger = logger;
        _recentTypes = recentTypes;
    }
    internal byte[] RunCompilation(string script)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(RunCompilation), nameof(ScriptCompiler));

        //Takes C# source string and turns it into a "Roslyn parsed syntax tree representation" of a normal C# file
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(script);

        List<MetadataReference>? references = _allReferences;

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
            // FailedToCompile(emitResult);
            throw new CompilationFailedException("Compilation failed.", emitResult);
        }

        byte[] assemblyBytes = ms.ToArray();
        return assemblyBytes;
    }
    internal ValidationRecord BasicValidationBeforeCompiling(string script)//record
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(BasicValidationBeforeCompiling), nameof(ScriptCompiler));

        //Does basic validation such as correct interface usage, or if only one class is in the script file.
        //Also extracts class name, type of the scrip(ex. Action) and verison of interface.
        // Source - https://stackoverflow.com/a/33095466

        if (string.IsNullOrWhiteSpace(script))
        {
            throw new ScriptWasEmptyOrNullException();
        }

        SyntaxTree tree = CSharpSyntaxTree.ParseText(script);   //being called twice also in RunCompilation() might be better for performance to remove twice but also you want to be able to compile without having to parse always
        var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        IEnumerable<MetadataReference>? minimalReferences = [mscorlib];
        var compilation = CSharpCompilation.Create("MyCompilation",
            syntaxTrees: new[] { tree }, references: _allReferences);   // i might be able to use this method to init the refrences also above?
        var model = compilation.GetSemanticModel(tree);
        var classesInTree = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
        if (classesInTree.Count > 1)
        {
            _logger.LogError("More Than one class found in Script string.");
            throw new MoreThanOneClassFoundInScriptException("More Than one class found in Script string. in if (classesInTree.Count() > 1)"); //this might throw even if there is one class in script if compiler adds classes or something like that, so if it does maybe change this if statement
        }
        var myClass = classesInTree.Single();
        var myClassSymbol = model.GetDeclaredSymbol(myClass) as ITypeSymbol;


        try
        {
            INamedTypeSymbol parentSymbol = myClassSymbol!.Interfaces.FirstOrDefault() ?? myClassSymbol!.BaseType!;

            if (parentSymbol.Name == "Object")
            {
                throw new VersionIntNotAssignedException("Version Int was not assigned probably because the foeach loop faliled maybe because the Context name of the script was not in the dictionary.");
            }

            var className = myClassSymbol.ToString();
            int? versionInt = null;

            versionInt = GetVersionInt(parentSymbol);

            if (versionInt == null)
            {
                throw new VersionIntNotAssignedException("Version Int was not assigned probably because the foeach loop faliled maybe because the Context name of the script was not in the dictionary.");
            }
            if (className == null)
            {
                _logger.LogError("BaseTypeName or classname null in BasicValidationBeforeCompiling.");
                throw new ClassNameOrBaseNameNullException("BaseTypeName or classname null in BasicValidationBeforeCompiling. in if (baseTypeName == null || className == null)");
            }
            _logger.LogTrace("Class Name = " + className);
            _logger.LogTrace("ParentSymbol Name = " + parentSymbol);
            _logger.LogTrace("Version Int = " + versionInt);

            Type scriptType = scriptType = CustomerScript.GetScriptType(parentSymbol.ToDisplayString());

            List<MethodRecord> methods = ValidateScriptMethodTypes(tree!, model, parentSymbol);
            // ValiHelper.ValidateOnlyUseRecentTypes(methods, parentSymbol.ToDisplayString(), _recentTypes);    //work in progress
            int? executionTime = GetExecutionTime(tree);
            ValidateLoopsHavingCancellation(tree!);
            ValidateNamespaceUsage(tree!, model!);
            ValidationRecord returnedRecord = new ValidationRecord
            {
                ClassName = className,
                ScriptType = scriptType!,
                Version = (int)versionInt,
                ExecutionTime = executionTime,
                methods = methods,
                ParentSymbol = parentSymbol.ToDisplayString()
            };
            return returnedRecord;
        }

        catch (VersionIntNotAssignedException e)
        {
            throw new ScriptFieldNullException("The script very likely did not implement one of the predefined interfaces.", e);
        }
        catch (ClassNameOrBaseNameNullException e)
        {
            throw new ScriptFieldNullException("The script very likely did not implement one of the predefined interfaces.", e);
        }
    }

    private int GetVersionInt(INamedTypeSymbol baseType)
    {
        List<ScriptMetaDataRecord> scriptRecords = ScriptVersionScanner.GetClassRecords();
        string? contextTypeString = null;

        foreach (var record in scriptRecords)
        {
            if (baseType.ToDisplayString() == record.ScriptType)
            {
                contextTypeString = record.ContextType;
            }
        }

        if (baseType.IsGenericType)
        {
            if (baseType.TypeArguments.Length >= 1)
            {
                ITypeSymbol contextType = baseType.TypeArguments[0];
                contextTypeString = GetFullyQualifiedName(contextType);
            }
        }
        int? ctxVersion = null;
        Dictionary<int, Type> activeContexts = ContextVersionScanner.GetInterfaceDictionary();

        if (contextTypeString == nameof(IContext))
        {
            ctxVersion = activeContexts.Keys.Max();
        }
        else
        {
            foreach (var ctx in activeContexts)
            {

                if (ctx.Value.FullName == contextTypeString)
                {
                    if (ctxVersion != null)
                    {
                        throw new ContextNameOccuredMoreThanOnceException("Context name occured more than once for some reason that should not happen.");
                    }
                    ctxVersion = ctx.Key;
                }
            }
        }
        return (int)ctxVersion!;
    }
    private ScriptMetaDataRecord GetMetaDataRecord(INamedTypeSymbol baseType)
    {
        List<ScriptMetaDataRecord> scriptRecords = ScriptVersionScanner.GetClassRecords();

        string? contextTypeStr = null;
        string? customReturnType = null;

        ScriptMetaDataRecord? foundRecord = null;
        foreach (var record in scriptRecords)
        {
            string definedMethodName = record.RetrievedType.FullName!;

            if (definedMethodName.Contains("`"))
            {
                definedMethodName = definedMethodName.Split('`')[0];
            }
            if (baseType.IsGenericType)
            {
                if (baseType.TypeArguments.Length >= 1)
                {
                    ITypeSymbol contextType = baseType.TypeArguments[0];
                    contextTypeStr = GetFullyQualifiedName(contextType);
                    customReturnType = "bool";
                }
                if (baseType.TypeArguments.Length == 2)
                {
                    ITypeSymbol contextType = baseType.TypeArguments[1];
                    customReturnType = GetFullyQualifiedName(contextType);
                }
            }
            if (baseType.ToDisplayString().Contains(definedMethodName))
            {
                List<MethodRecord> methods = [];
                if (contextTypeStr != null)
                {
                    foreach (var item in record.Methods)
                    {
                        List<ParameterRecord> parameters = [];
                        foreach (var item2 in item.Parameters)
                        {
                            parameters.Add(new ParameterRecord
                            {
                                Name = item2.Name,
                                ReturnType = contextTypeStr!
                            }
                            );
                        }
                        methods.Add(new MethodRecord
                        {
                            Name = item.Name,
                            ReturnType = customReturnType!,
                            Parameters = parameters
                        });
                    }
                    foundRecord = new ScriptMetaDataRecord
                    {
                        Version = record.Version,
                        ContextType = record.ContextType,
                        RetrievedType = record.RetrievedType,
                        ScriptType = record.ScriptType,
                        Methods = methods
                    };
                }
                else
                {
                    foundRecord = record;
                }
            }
        }
        if (foundRecord == null)
        {
            throw new RecordCouldNotBeMatchedException(baseType.ToDisplayString());
        }
        return foundRecord;
    }

    private List<MethodRecord> ValidateScriptMethodTypes(SyntaxTree tree, SemanticModel semanticModel, INamedTypeSymbol baseType)
    {
        List<MethodRecord> justToReturn = [];

        SyntaxNode root = tree.GetRoot();
        IEnumerable<MethodDeclarationSyntax> scriptMethods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

        ScriptMetaDataRecord record = GetMetaDataRecord(baseType);

        foreach (var scriptMethodMDS in scriptMethods)
        {

            if (scriptMethodMDS.Modifiers.Any(SyntaxKind.PrivateKeyword))
            {
                continue;   //skips private methods
            }
            if (scriptMethodMDS.Modifiers.Any(SyntaxKind.InternalKeyword))
            {
                // continue;   //skips internal methods
            }
            if (scriptMethodMDS.Modifiers.Any(SyntaxKind.ProtectedKeyword))
            {
                // continue;   //skips protected methods
            }
            MethodRecord scriptMethod = MethodRecord.GetMethodRecord(semanticModel.GetDeclaredSymbol(scriptMethodMDS)!);
            justToReturn.Add(scriptMethod);

            MethodRecord? foundMethod = null;
            foreach (var definedMeth in record.Methods)
            {
                if (scriptMethod.Name == definedMeth.Name)
                {
                    foundMethod = definedMeth;
                }
            }
            if (foundMethod == null)
            {
                throw new UndefinedMethodException(message: "No new methods allowed that are not predefinded!" + scriptMethod.ToString(), scriptMethod);
            }
            if (scriptMethod.ReturnType != foundMethod.ReturnType)
            {
                throw new WrongReturnTypeException(message: scriptMethod.Name + " has the wrong return Type: " + scriptMethod.ReturnType + ", it should be: " + foundMethod.ReturnType + ".");
            }
            foreach (var scriptParam in scriptMethod.Parameters)
            {
                ParameterRecord? foundParam = null;
                foreach (var definedParam in foundMethod.Parameters)
                {
                    if (definedParam.Name == scriptParam.Name)
                    {
                        foundParam = definedParam;
                    }
                }
                if (foundParam == null)
                {
                    throw new CouldNotFindParameterException(foundMethod.Name + " could not find defined parameter: " + scriptParam.Name);
                }
                if (foundParam!.ReturnType != scriptParam.ReturnType)
                {
                    throw new WrongParameterTypeException(foundMethod.Name + " a parameter had the wrong return type: " + scriptParam.ReturnType + ", it should have been: " + foundParam.ReturnType);
                }
            }
        }
        if (justToReturn.Count() < 1)
        {
            throw new NoMethodInScriptException("Script needs at least one method to be stored.");
        }
        return justToReturn;
    }
    private string GetFullyQualifiedName(ITypeSymbol symbol)
    {
        string name = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (name.StartsWith("global::"))
        {
            return name.Substring("global::".Length);
        }
        return name;
    }
    private int? GetExecutionTime(SyntaxTree syntaxTree)
    {
        int? executionTime = null;

        var syntaxRoot = syntaxTree.GetRoot();
        var classNode = syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();

        var timeAttribute = classNode!.AttributeLists
            .SelectMany(al => al.Attributes)
            .FirstOrDefault(a => a.Name.ToString().Contains(nameof(ExecutionTime)));

        if (timeAttribute?.ArgumentList != null && timeAttribute.ArgumentList.Arguments.Any())
        {
            var firstArgument = timeAttribute.ArgumentList.Arguments.First();

            var argumentFullString = firstArgument.NormalizeWhitespace().ToFullString();
            string enumString = argumentFullString.Split('.').Last();
            executionTime = ExecutionTime.GetDurationFromEnumString(enumString);
        }
        return executionTime;
    }
    //Following method was AI generated 
    private void ValidateLoopsHavingCancellation(SyntaxTree tree)
    {
        var root = tree.GetRoot();

        var loops = root.DescendantNodes().Where(n =>
            n is ForStatementSyntax ||
            n is WhileStatementSyntax ||
            n is DoStatementSyntax ||
            n is ForEachStatementSyntax);

        // Short form: exactly as written in your script
        string shortCall = $"{nameof(ScriptEnvironment)}.{nameof(ScriptEnvironment.CurrentToken)}.Value.{nameof(System.Threading.CancellationToken.ThrowIfCancellationRequested)}";

        // Fully qualified form: in case the user writes the full namespace out
        string fullyQualifiedCall = $"{typeof(ScriptEnvironment).Namespace}.{nameof(ScriptEnvironment)}.{nameof(ScriptEnvironment.CurrentToken)}.Value.{nameof(System.Threading.CancellationToken.ThrowIfCancellationRequested)}";

        foreach (var loop in loops)
        {
            bool hasCancellation = loop.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(inv =>
                {
                    string expressionString = inv.Expression.ToString();
                    return expressionString == shortCall || expressionString == fullyQualifiedCall;
                });

            if (!hasCancellation)
            {
                throw new ConcellationTokenUncheckedException();
            }
        }
    }

    private void ValidateNamespaceUsage(SyntaxTree tree, SemanticModel model)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(ValidateNamespaceUsage), nameof(ScriptCompiler));
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
                    _logger.LogError("Script tried to use a illegal namespace.");
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
                        _logger.LogError("Script tried to use a illegal type.");
                        throw new ForbiddenTypeAccessException($"Usage of forbidden type: {symbol.ToDisplayString()}");
                    }
                }
            }
        }
    }

    internal bool IsTheSameTree(string script1, string script2)
    {
        //This function i quickly generated with AI
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(IsTheSameTree), nameof(ScriptCompiler));
        SyntaxTree tree1 = CSharpSyntaxTree.ParseText(script1);
        SyntaxTree tree2 = CSharpSyntaxTree.ParseText(script2);

        bool areSame = tree1.IsEquivalentTo(tree2, topLevel: false);

        return areSame;
    }

    private List<MetadataReference> GetReferencesForOldVersion(int version, List<string>? customDlls = null, bool loadCurrentRT = true)  //todo fix this
    {
        var stdReferences = _standardReferencesForAllScripts;
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(GetReferencesForOldVersion), nameof(ScriptCompiler));
        var references = new List<MetadataReference>();

        string tempPath = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                         "..", "..", "..", "..", "..", "..")
        );

        string versionPath = Path.Combine(tempPath, "OldVersions", version.ToString());

        if (!Directory.Exists(versionPath))
        {
            _logger.LogError("Could not find references.");
            throw new ReferencesForVersionNotFound($"References for version {version} not found at {versionPath}");
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
            _logger.LogTrace("loadCurrentRT true.");
            dllsToLoad = ["Framework.dll"];
            references.AddRange(stdReferences);
        }
        if (customDlls != null)
        {
            _logger.LogTrace("customDlls was not not null");
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
}

public static class AllowOnlyRecentTypes
{
    public static void ValidateAllowOnlyRecentTypes(string parentSymbol, List<Type> recentTypes)
    {
        if (parentSymbol.Contains("<"))
        {
            parentSymbol = parentSymbol.Split('<')[0];
        }
        bool found = false;
        foreach (var recentType in recentTypes)
        {
            string typeClean = recentType.FullName!;
            if (recentType.FullName!.Contains("`"))
            {
                typeClean = recentType.FullName.Split('`')[0];
            }
            if (parentSymbol == typeClean)
            {
                found = true;
                break;
            }
        }
        if (found == false)
        {
            throw new AllowOnlyRecentTypesException("Script was not a recent type!");
        }
    }
}