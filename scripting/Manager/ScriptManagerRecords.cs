using Microsoft.CodeAnalysis;

namespace Ember.Scripting.Manager;

public record DuplicateRecord
{
    public required List<Guid> ScriptGUIDs { get; init; }
    public required Dictionary<Guid, int> CacheGUIDs { get; init; }
}