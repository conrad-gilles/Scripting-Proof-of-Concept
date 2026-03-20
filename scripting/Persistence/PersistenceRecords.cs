namespace Ember.Scripting;

public record DuplicateListDbH
{
    public required List<Guid> duplicateGuids { get; init; }
    public required Dictionary<Guid, int> cachesToDelete { get; init; }
}