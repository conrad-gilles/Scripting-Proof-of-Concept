namespace BlazorUI.TotalExecutions;

public class TotalExecutions
{
    private static int _totalExecutions = 0;

    public static int GetTotalExecutions()
    {
        return _totalExecutions;
    }
    public static void IncrementTotalExecutions()
    {
        _totalExecutions++;
    }
}