using Microsoft.CodeAnalysis;

namespace Ember.Scripting;

public record ValidationRecord
{
    public required string ClassName { get; init; }
    // public required ScriptTypes BaseTypeName { get; init; }
    public required Type ScriptType { get; init; }

    public required int Version { get; init; }

    public string BaseTypeAsString()
    {
        string sScriptType;
        if (ScriptType == typeof(IGeneratorActionScript))
        {
            sScriptType = nameof(IGeneratorActionScript);
        }
        else if (ScriptType == typeof(IGeneratorConditionScript))
        {
            sScriptType = nameof(IGeneratorConditionScript);
        }
        else
        {
            throw new CouldNotMatchBaseTypeInRecord("Could not match the BaseTypeNAme to a String this should never happen!");
        }

        return sScriptType;
    }
}

public record GetBaseTypeReturn
{
    public required Microsoft.CodeAnalysis.INamedTypeSymbol? BaseType { get; init; }
    public required Microsoft.CodeAnalysis.ITypeSymbol? MyClassSymbol { get; init; }
    public required Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax? MyClass { get; init; }
    public required SyntaxTree Tree { get; init; }
    public required Microsoft.CodeAnalysis.SemanticModel? Model { get; init; }
}

public record ScriptCompilationError(
    string Id,
    string Message,
    int Line,
    int Column,
    int EndLine,
    int EndColumn,
    bool IsError)
{
    public override string ToString()
    {
        return "An error occurred in Line: " + Line + ", Column: " + Column + " Message: " + Message;
    }
}
