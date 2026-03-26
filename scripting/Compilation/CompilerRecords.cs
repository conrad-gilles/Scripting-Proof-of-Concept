using Microsoft.CodeAnalysis;

namespace Ember.Scripting;

public record ValidationRecord
{
    public required string ClassName { get; init; }
    public required ScriptTypes BaseTypeName { get; init; }
    public required int Version { get; init; }

    public string BaseTypeAsString()
    {
        switch (BaseTypeName)
        {
            case ScriptTypes.GeneratorActionScript:
                return nameof(IGeneratorActionScript);
            case ScriptTypes.GeneratorConditionScript:
                return nameof(IGeneratorConditionScript);
            default:
                throw new CouldNotMatchBaseTypeInRecord("Could not match the BaseTypeNAme to a String this should never happen!");
        }
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
    bool IsError);
