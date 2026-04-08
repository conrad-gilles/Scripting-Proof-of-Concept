using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Logging;
using Npgsql.Internal;
using System.Reflection;

namespace Ember.Scripting;

internal class ScriptCompiler
{
    private readonly List<MetadataReference>? _referencesRO = null;
    private readonly ILogger<ScriptCompiler> _logger;

    private readonly List<MetadataReference> _standardRefrencesForAllScripts = [MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
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
    public ScriptCompiler(List<MetadataReference> referencesRO, ILogger<ScriptCompiler> logger)
    {
        _referencesRO = referencesRO;
        _referencesRO!.AddRange(_standardRefrencesForAllScripts);
        _logger = logger;
    }
    public byte[] RunCompilation(string script, int? apiVersion = null, ValidationRecord? metaData = null) //references param there to enable later on users to define custom references
    {
        _logger.LogTrace("Entered {MethodName} in {ClassName}.", nameof(RunCompilation), nameof(ScriptCompiler));
        if (metaData != null)
        {
            _logger.LogTrace("Trying to compile script:" + metaData.ClassName);
        }
        // _referencesRO!.AddRange(_standardRefrencesForAllScripts);

        List<MetadataReference>? references = [];

        //Takes C# source string and turns it into a "Roslyn parsed syntax tree representation" of a normal C# file
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(script);
        if (_referencesRO != null)
        {
            _logger.LogInformation("References 2 added references in {MethodName}.", nameof(RunCompilation));
            references = _referencesRO;
        }
        if (apiVersion != null)
        {
            _logger.LogInformation("Added custom references in {MethodName}.", nameof(RunCompilation));  //if this works remove the if references is null if stat above
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
            var compilerErrors = emitResult.Diagnostics
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

            // Throw passing the clean DTOs
            throw new CompilationFailedException("Compilation failed.", compilerErrors);
        }

        byte[] assemblyBytes = ms.ToArray();
        return assemblyBytes;
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
        IEnumerable<MetadataReference>? referencesBT = [mscorlib];
        var compilation = CSharpCompilation.Create("MyCompilation",
            syntaxTrees: new[] { tree }, references: referencesBT);   // i might be able to use this method to init the refrences also above?
        var model = compilation.GetSemanticModel(tree);
        var classesInTree = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();
        if (classesInTree.Count() > 1)
        {
            _logger.LogError("More Than one class found in Script string.");
            throw new MoreThanOneClassFoundInScriptException("More Than one class found in Script string. in if (classesInTree.Count() > 1)"); //this might throw even if there is one class in script if compiler adds classes or something like that, so if it does maybe change this if statement
        }
        var myClass = classesInTree.Last();
        var myClassSymbol = model.GetDeclaredSymbol(myClass) as ITypeSymbol;


        try
        {
            var baseTypeName = myClassSymbol!.BaseType!.Name;
            var className = myClassSymbol.ToString();
            var parentSymbol = myClassSymbol!.Interfaces.FirstOrDefault() ?? myClassSymbol.BaseType;
            int? versionInt = null;
            string implementedIntrf = parentSymbol.Name;

            string? contextParameterTypeName = null;

            var executeMethod = myClass!.DescendantNodes()
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
            _logger.LogTrace("Extracted Context Parameter Type: " + contextParameterTypeName);

            Dictionary<int, Type> activeContexts = ContextVersionScanner.GetInterfaceDictionary();
            // Console.WriteLine("Start of dict String:");
            _logger.LogTrace("Start of dict String:");
            foreach (var pair in activeContexts)
            {
                // Console.WriteLine("Key: " + pair.Key + ", Value: " + pair.Value);
                _logger.LogTrace("Key: " + pair.Key + ", Value: " + pair.Value);
            }

            foreach (var ctx in activeContexts)
            {
                string dictTypeFullName = ctx.Value.FullName ?? "";

                if (ctx.Value.FullName == contextParameterTypeName)
                {
                    if (versionInt != null)
                    {
                        throw new ContextNameOccuredMoreThanOnceException("Context name occured more than once for some reason that should not happen.");
                    }
                    versionInt = ctx.Key;
                }
            }

            if (versionInt == null)
            {
                throw new VersionIntNotAssignedException("Version Int was not assigned probably because the foeach loop faliled maybe because the Context name of the script was not in the dictionary.");
            }
            if (baseTypeName == null || className == null)
            {
                _logger.LogError("BaseTypeName or classname null in BasicValidationBeforeCompiling.");
                throw new ClassNameOrBaseNameNullException("BaseTypeName or classname null in BasicValidationBeforeCompiling. in if (baseTypeName == null || className == null)");
            }
            _logger.LogTrace("Class Name = " + className);
            _logger.LogTrace("BaseClass Name = " + baseTypeName);
            _logger.LogTrace("Version Int = " + versionInt);


            // ScriptTypes scriptType;
            Type scriptType;
            switch (baseTypeName)
            {
                case nameof(IGeneratorActionScript):
                    scriptType = typeof(IGeneratorActionScript);
                    break;
                case nameof(IGeneratorConditionScript):
                    scriptType = typeof(IGeneratorConditionScript);
                    break;
                default:
                    throw new CouldNotMatchBaseTypeInCompiler(nameof(baseTypeName) + " was not a valid option!");
            }
            List<MethodRecord> methods = ValidateOnlyInheritedMethodsAndReturn(tree!, model);
            int? executionTime = GetExecutionTime(tree);
            ValidateLoopsHavingCancellation(tree!);
            ValidateNamespaceUsage(tree!, model!);
            ValidationRecord returnedRecord = new ValidationRecord
            {
                ClassName = className,
                ScriptType = scriptType,
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

        Assembly myAssembly = Assembly.GetExecutingAssembly();
        string targetNamespace = "Ember.Scripting.AdditionalMethods";
        IEnumerable<Type> types = myAssembly.GetTypes().Where(type => type.Namespace == targetNamespace && type.IsInterface
                // && !type.Name.StartsWith("<") //last line to exclude compiler generated 
                );
        List<MethodRecord> methodsAssembly = GetMethodsAssembly(types);

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
                    Console.WriteLine("Method from Assembly that wasnt found: " + meth2);
                }
            }
            if (isInside == false)
            {
                if (meth1.Name != nameof(Ember.Scripting.IGeneratorActionScript.ExecuteAsync)
                && meth1.Name != nameof(Ember.Scripting.IGeneratorConditionScript.EvaluateAsync)
                )
                {
                    Console.WriteLine("Method from Roslyn: " + meth1);
                    // Console.WriteLine("Method from Roslyn: "+meth2);
                    throw new UndefinedMethodException(message: "No new methods allowed that are not predefinded!", meth1);
                }
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
            string returnType = methodSymbol.ReturnType.MetadataName;


            List<ParameterRecord> resultParams = [];

            foreach (IParameterSymbol paramSymbol in methodSymbol.Parameters)
            {
                try
                {
                    string paramName = paramSymbol.Name;
                    string paramType = paramSymbol.Type.MetadataName;

                    resultParams.Add(new ParameterRecord { Name = paramName, ReturnType = paramType });
                }
                catch { continue; }
            }
            result.Add(new MethodRecord { Name = methodName, ReturnType = returnType, Parameters = resultParams });
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

    // Todo test Checks if the Cancellation Token is being checked within each Loop if not throws error.
    private void ValidateLoopsHavingCancellation(SyntaxTree tree)
    {
        var root = tree.GetRoot();

        var loops = root.DescendantNodes().Where(n =>
            n is ForStatementSyntax ||
            n is WhileStatementSyntax ||
            n is DoStatementSyntax ||
            n is ForEachStatementSyntax);

        // Result: "Ember.Scripting.ScriptEnvironment.CurrentToken.Value.ThrowIfCancellationRequested"
        string requiredCall = $"{typeof(Ember.Scripting.ScriptEnvironment).Namespace}.{nameof(Ember.Scripting.ScriptEnvironment)}.{nameof(Ember.Scripting.ScriptEnvironment.CurrentToken)}.Value.{nameof(System.Threading.CancellationToken.ThrowIfCancellationRequested)}";

        foreach (var loop in loops)
        {
            bool hasCancellation = loop.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(inv => inv.Expression.ToString() == requiredCall);

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
        var stdReferences = _standardRefrencesForAllScripts;
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


