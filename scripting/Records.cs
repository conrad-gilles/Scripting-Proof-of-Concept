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

