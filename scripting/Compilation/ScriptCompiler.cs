using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.Internal;
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
    public byte[] RunCompilation(string script)
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(RunCompilation), nameof(ScriptCompiler));

        //Takes C# source string and turns it into a "Roslyn parsed syntax tree representation" of a normal C# file
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(script);

        List<MetadataReference>? references = [];
        references = _allReferences;

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
            FailedToCompile(emitResult);
        }

        byte[] assemblyBytes = ms.ToArray();
        return assemblyBytes;
    }

    //The following method statement was AI Generated
    private void FailedToCompile(Microsoft.CodeAnalysis.Emit.EmitResult emitResult)
    {

        List<ScriptCompilationError> compilerErrors = emitResult.Diagnostics
.Where(d => d.IsWarningAsError || d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
.Select(d =>
{
    var hasLocation = d.Location != Location.None;
    var lineSpan = hasLocation ? d.Location.GetLineSpan() : default;

    int startLine = hasLocation ? lineSpan.StartLinePosition.Line + 1 : 1;
    int startCol = hasLocation ? lineSpan.StartLinePosition.Character + 1 : 1;
    int endLine = hasLocation ? lineSpan.EndLinePosition.Line + 1 : 1;
    int endCol = hasLocation ? lineSpan.EndLinePosition.Character + 1 : 1;

    // Force a minimum width of 1 character so Monaco actually draws a squiggly line.
    // For CS0161 (missing return), Roslyn sometimes gives the exact method name location.
    if (startLine == endLine && startCol == endCol)
    {
        endCol = startCol + 1; // Minimum 1 char width
    }

    return new ScriptCompilationError(
        Id: d.Id,
        Message: d.GetMessage(),
        Line: startLine,
        Column: startCol,
        EndLine: endLine,
        EndColumn: endCol,
        IsError: d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error
    );
}).ToList();

        _logger.LogError("Compilation failed with {ErrorCount} errors.", compilerErrors.Count);

        string errorString = "Errors: ";
        foreach (var error in compilerErrors)
        {
            errorString = errorString + error.ToString() + ", ";
        }

        // Throw passing the clean DTOs
        throw new CompilationFailedException("Compilation failed, " + errorString, compilerErrors);
    }
    public ValidationRecord BasicValidationBeforeCompiling(string script)//record
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
            // syntaxTrees: new[] { tree }, references: minimalReferences);   // i might be able to use this method to init the refrences also above?
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
            // INamedTypeSymbol baseType = myClassSymbol!.BaseType!;
            INamedTypeSymbol parentSymbol = myClassSymbol!.Interfaces.FirstOrDefault() ?? myClassSymbol!.BaseType!;

            if (parentSymbol.Name == "Object")
            {
                throw new VersionIntNotAssignedException("Version Int was not assigned probably because the foeach loop faliled maybe because the Context name of the script was not in the dictionary.");
            }

            var className = myClassSymbol.ToString();
            // var parentSymbol = myClassSymbol!.Interfaces.FirstOrDefault() ?? myClassSymbol.BaseType;
            int? versionInt = null;

            // versionInt = GetMetaDataRecord(parentSymbol).ContextVersion;
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

            Type scriptType = scriptType = CustomerScript.GetScriptType(parentSymbol.Name);

            List<MethodRecord> methods = ValidateOnlyInheritedMethodsAndReturn(tree!, model);
            ValidateScriptMethodTypes(tree!, model, parentSymbol);
            int? executionTime = GetExecutionTime(tree);
            ValidateLoopsHavingCancellation(tree!);
            ValidateNamespaceUsage(tree!, model!);
            ValidationRecord returnedRecord = new ValidationRecord
            {
                ClassName = className,
                ScriptType = scriptType!,
                Version = (int)versionInt,
                ExecutionTime = executionTime,
                methods = methods
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
        var scriptRecords = ScriptVersionScanner.GetClassRecords();
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
        return (int)ctxVersion!;
    }
    private ScriptMetaDataRecord GetMetaDataRecord(INamedTypeSymbol baseType)
    {
        List<ScriptMetaDataRecord> scriptRecords = ScriptVersionScanner.GetClassRecords();
        string? contextTypeStr = null;
        string? arTypeString = null;
        ScriptMetaDataRecord? foundRecord = null;

        foreach (var record in scriptRecords)
        {
            string definedMethodName = record.RetrievedType.FullName!;
            if (definedMethodName.Contains("`"))
            {
                definedMethodName = definedMethodName.Split('`')[0];
            }
            // Console.WriteLine("definedMethodName: " + definedMethodName);
            if (baseType.IsGenericType)
            {
                if (baseType.TypeArguments.Length >= 1)
                {
                    ITypeSymbol contextType = baseType.TypeArguments[0];
                    contextTypeStr = GetFullyQualifiedName(contextType);
                }
                if (baseType.TypeArguments.Length == 2)
                {
                    ITypeSymbol contextType = baseType.TypeArguments[1];
                    arTypeString = GetFullyQualifiedName(contextType);
                }
            }
            if (baseType.ToDisplayString().Contains(definedMethodName))
            {
                foundRecord = record;
                if (contextTypeStr != null)
                {
                    foundRecord.ContextType = contextTypeStr!;
                }
                if (arTypeString != null)
                {
                    foundRecord.ActionResultType = arTypeString!;
                }
            }
        }
        if (foundRecord == null)
        {
            throw new Exception(baseType.ToDisplayString());
        }
        return foundRecord;
    }

    public void ValidateScriptMethodTypes(SyntaxTree tree, SemanticModel semanticModel, INamedTypeSymbol baseType)
    {
        if (baseType.IsGenericType == false)
        {
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

                MethodRecord? foundMethod = null;
                foreach (var definedMeth in record.Methods)
                {
                    if (scriptMethod.Name == definedMeth.Name)
                    {
                        if (definedMeth.Parameters[0].Name != nameof(IContext))
                        {
                            foundMethod = definedMeth;
                        }

                    }
                }
                if (foundMethod == null)
                {
                    throw new CouldNotFindMethodTypeException("no method found, scriptMethod: " + scriptMethod.Name);
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
        }

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
    public int? GetExecutionTime(SyntaxTree syntaxTree)
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

    private List<MethodRecord> ValidateOnlyInheritedMethodsAndReturn(SyntaxTree tree, SemanticModel semanticModel)
    {
        List<MethodRecord> result = [];

        SyntaxNode root = tree.GetRoot();
        IEnumerable<MethodDeclarationSyntax> methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
        List<MethodRecord> methodsRoslyn = GetMethodsRoslyn(methods, semanticModel);

        IEnumerable<Type> validScriptMethods = AppDomain.CurrentDomain.GetAssemblies()  //get rid of this, bug: passes methods that are implemented in for example diffrent IGeneratorScript_V4.GenScript
                   .SelectMany(a => a.GetTypes())
                   .Where(t => t.IsInterface
                            && typeof(IScriptMethod).IsAssignableFrom(t)
                            && t != typeof(IScriptMethod)).ToList();


        List<MethodRecord> methodsAssembly = GetMethodsAssembly(validScriptMethods);

        foreach (var meth1 in methodsRoslyn)
        {
            bool isInside = false;
            foreach (var meth2 in methodsAssembly)
            {
                if (MethodRecord.IsTheSame(meth1, meth2))
                {
                    isInside = true;
                    result.Add(meth1);
                }
                else
                {
                    // Console.WriteLine("Method from Assembly that wasnt found: " + meth2);
                }
            }
            if (isInside == false)
            {
                // Console.WriteLine("Method from Roslyn: " + meth1);
                // Console.WriteLine("Method from Roslyn: "+meth2);
                throw new UndefinedMethodException(message: "No new methods allowed that are not predefinded!" + meth1, meth1);
            }
        }
        return result;
    }

    private List<MethodRecord> GetMethodsRoslyn(IEnumerable<MethodDeclarationSyntax> methods, SemanticModel semanticModel)
    {
        List<MethodRecord> result = [];

        foreach (var method in methods)
        {
            if (method.Modifiers.Any(SyntaxKind.PrivateKeyword))
            {
                continue;   //skips private methods
            }
            if (method.Modifiers.Any(SyntaxKind.InternalKeyword))
            {
                // continue;   //skips internal methods
            }
            if (method.Modifiers.Any(SyntaxKind.ProtectedKeyword))
            {
                // continue;   //skips protected methods
            }

            IMethodSymbol methodSymbol = semanticModel.GetDeclaredSymbol(method)!;

            string methodName = methodSymbol.Name;
            ITypeSymbol returnType = methodSymbol.ReturnType;

            // int iterations = 0;
            // while (returnType is INamedTypeSymbol namedTypeSymbol && namedTypeSymbol.IsGenericType && iterations < 100)
            // {
            //     // Match against the open generic type definition of Task<T>
            //     if (namedTypeSymbol.OriginalDefinition.ToDisplayString() == "System.Threading.Tasks.Task<TResult>")
            //     {
            //         // Extract the inner type T
            //         returnType = namedTypeSymbol.TypeArguments[0];
            //     }
            // }
            // if (iterations > 98)
            // {
            //     throw new Exception();
            // }
            string returnTypeString = returnType.MetadataName;
            List<ParameterRecord> resultParams = [];

            foreach (IParameterSymbol paramSymbol in methodSymbol.Parameters)
            {
                // try
                // {
                string paramName = paramSymbol.Name;
                string paramType = paramSymbol.Type.MetadataName;

                resultParams.Add(new ParameterRecord { Name = paramName, ReturnType = paramType });
                // }
                // catch { continue; }
            }
            result.Add(new MethodRecord { Name = methodName, ReturnType = returnTypeString, Parameters = resultParams });
        }

        return result;
    }
    private List<MethodRecord> GetMethodsAssembly(IEnumerable<Type> types)
    {
        List<MethodRecord> result = [];


        BindingFlags flags =
                            BindingFlags.NonPublic |
                            BindingFlags.Public |
                            BindingFlags.Instance |
                            BindingFlags.Static;
        foreach (var t in types)
        {
            MethodInfo[] methodsInType = t.GetMethods(flags);
            foreach (var m in methodsInType)
            {
                string methodName = m.Name;
                string returnType = m.ReturnType.Name;
                ParameterInfo[] parameters = m.GetParameters();

                List<ParameterRecord> resultParams = [];
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
                result.Add(new MethodRecord { Name = methodName, ReturnType = returnType, Parameters = resultParams });
            }
        }

        return result;
    }

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

    public bool IsTheSameTree(string script1, string script2)
    {
        //This function i quickly generated with AI
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(IsTheSameTree), nameof(ScriptCompiler));
        SyntaxTree tree1 = CSharpSyntaxTree.ParseText(script1);
        SyntaxTree tree2 = CSharpSyntaxTree.ParseText(script2);

        bool areSame = tree1.IsEquivalentTo(tree2, topLevel: false);

        return areSame;
    }

    public List<MetadataReference> GetReferencesForOldVersion(int version, List<string>? customDlls = null, bool loadCurrentRT = true)  //todo fix this
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


