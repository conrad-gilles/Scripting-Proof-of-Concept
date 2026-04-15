using Microsoft.CodeAnalysis;

namespace Ember.Scripting.ScriptManager;

public record DuplicateRecord
{
    public required List<Guid> scriptGUIDs { get; init; }
    public required Dictionary<Guid, int> cacheGUIDs { get; init; }
}