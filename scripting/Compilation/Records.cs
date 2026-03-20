using Microsoft.CodeAnalysis;

namespace Ember.Scripting;

public record DuplicateListDbH
{
    public required List<Guid> duplicateGuids { get; init; }
    public required Dictionary<Guid, int> cachesToDelete { get; init; }
}

public record ScriptNameType
{
    public required string Name { get; init; }
    public required ScriptTypes Type { get; init; }
}

public record DuplicateRecord
{
    public required List<Guid> scriptGUIDs { get; init; }
    public required Dictionary<Guid, int> cacheGUIDs { get; init; }
}

public record ValidationRecord
{
    public required string ClassName { get; init; }
    public required string BaseTypeName { get; init; }
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