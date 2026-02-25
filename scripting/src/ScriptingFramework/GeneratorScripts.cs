namespace Ember.Scripting;

public interface IGeneratorConditionScript
{
    // Task<bool> EvaluateAsync(IGeneratorReadOnlyContext context);
    Task<bool> EvaluateAsync(IGeneratorBaseInterface context);
}
public interface IGeneratorActionScript
{
    // Task<ActionResultBaseClass> ExecuteAsync(IGeneratorContext context);
    Task<ActionResultBaseClass> ExecuteAsync(IGeneratorBaseInterface context);
}