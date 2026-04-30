namespace Ember.Scripting.Persistence;

internal record DuplicateListDbH
{
    internal required List<Guid> duplicateGuids { get; init; }
    internal required Dictionary<Guid, int> cachesToDelete { get; init; }
}