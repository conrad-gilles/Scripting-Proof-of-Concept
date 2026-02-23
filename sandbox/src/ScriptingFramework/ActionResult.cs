public abstract class ActionResultBaseClass
{

}
public class ActionResult : ActionResultBaseClass
{
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; }
    public string ErrorCode { get; private set; }

    // private constructor to force usage of factory methods
    protected ActionResult(bool isSuccess, string message, string errorCode = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        ErrorCode = errorCode;
    }

    // Factory method for success
    public static ActionResult Success(string message)
    {
        return new ActionResult(true, message);
    }

    // Factory method for failure
    public static ActionResult Failure(string message, string errorCode = "GENERIC_ERROR")
    {
        return new ActionResult(false, message, errorCode);
    }
    public override string ToString()
    {
        return IsSuccess ? $"[Success] {Message}" : $"[Error: {ErrorCode}] {Message}";
    }
}
public class ActionResultV2 : ActionResult
{
    public List<string> LoggedActions { get; private set; }

    private ActionResultV2(bool isSuccess, string message, string errorCode, List<string> loggedActions) : base(isSuccess, message, errorCode)
    {
        LoggedActions = loggedActions;
    }
    public new static ActionResultV2 Success(string message)
    {
        // return new ActionResult(true, message);
        return new ActionResultV2(true, message, null, new List<string>()); //todo check inheritance use base idk
    }
    public void AppendLoggedActions(string action)
    {
        LoggedActions.Add(action);
    }

    public List<string> GetLoggedActions()
    {
        return LoggedActions;
    }

    public static ActionResultV2 UpgradeV1(ActionResult actionResult
        // bool isSuccess, string message, string errorCode
        , List<string> loggedActions
        )
    {
        return new ActionResultV2(actionResult.IsSuccess, actionResult.Message, actionResult.ErrorCode, loggedActions);
        // return new ActionResultV2(isSuccess, message, errorCode, loggedActions);
    }
}

public class ActionResultV3NoInheritance : ActionResultBaseClass
{
    public bool FailedOrNot { get; private set; }
    public string Message { get; private set; }


    protected ActionResultV3NoInheritance(bool failedOrNot, string message)
    {
        FailedOrNot = failedOrNot;
        Message = message;
    }

    // Factory method
    public static ActionResultV3NoInheritance Success(string message)
    {
        return new ActionResultV3NoInheritance(true, message);
    }

    // Factory method
    public static ActionResultV3NoInheritance Failure(string message)
    {
        return new ActionResultV3NoInheritance(false, message);
    }

    public override string ToString()
    {
        return $"[Message contains either failure or succes: ] {Message}";
    }
    public static ActionResultV3NoInheritance UpgradeV2(ActionResultV2 actionResult)
    {
        string tempMessage = "";
        if (actionResult.Message != null)
        {
            tempMessage = tempMessage + actionResult.Message;
        }
        if (actionResult.ErrorCode != null)
        {
            tempMessage = tempMessage + actionResult.ErrorCode;
        }
        return new ActionResultV3NoInheritance(actionResult.IsSuccess, tempMessage);
    }
}

