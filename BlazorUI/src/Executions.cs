namespace BlazorUI.TotalExecutions;

public class TotalExecutions
{
    private static int totalExecutions = 0;

    public static int GetTotalExecutions()
    {
        return totalExecutions;
    }
    public static void IncrementTotalExecutions()
    {
        totalExecutions++;
    }
}