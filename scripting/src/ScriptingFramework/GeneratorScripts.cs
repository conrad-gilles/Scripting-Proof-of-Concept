namespace Ember.Scripting;

public interface IGeneratorConditionScript
{
    // int VersionScript { get; }
    // Task<bool> EvaluateAsync(IGeneratorReadOnlyContext context);
    Task<bool> EvaluateAsync(IGeneratorBaseInterface context);
}
public interface IGeneratorActionScript
{
    // int VersionSript { get; }
    // Task<ActionResultBaseClass> ExecuteAsync(IGeneratorContext context);
    Task<ActionResultBaseClass> ExecuteAsync(IGeneratorBaseInterface context);
}