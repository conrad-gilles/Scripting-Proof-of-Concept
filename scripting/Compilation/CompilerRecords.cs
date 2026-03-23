using Microsoft.CodeAnalysis;

namespace Ember.Scripting;

public record ValidationRecord
{
    public required string ClassName { get; init; }
    public required ScriptTypes BaseTypeName { get; init; }
    public required int Version { get; init; }
}

public record GetBaseTypeReturn
{
    public required Microsoft.CodeAnalysis.INamedTypeSymbol? BaseType { get; init; }
    public required Microsoft.CodeAnalysis.ITypeSymbol? MyClassSymbol { get; init; }
    public required Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax? MyClass { get; init; }
    public required SyntaxTree Tree { get; init; }
    public required Microsoft.CodeAnalysis.SemanticModel? Model { get; init; }
}