using Ember.Scripting;
namespace ActionResultV1
{
    [MetaDataActionResult(version: 1)]
    public class ActionResult : ActionResultSF
    {
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; }
        public string ErrorCode { get; private set; }

        // private constructor to force usage of factory methods
        protected ActionResult(bool isSuccess, string message, string? errorCode = null)
        {
            IsSuccess = isSuccess;
            Message = message;
            ErrorCode = errorCode!;
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

        // public override ActionResultSF Upgrade(ActionResultSF actionResult)
        // {
        //     throw new NotImplementedException(message: "This can not be implemented because and also should never be called since the verson below this one is abstract.");
        // }

        public override ActionResultSF Upgrade(object actionResult)
        {
            throw new NotImplementedException(message: "This can not be implemented because and also should never be called since the verson below this one is abstract.");
        }
    }
}

namespace ActionResultV2
{
    [MetaDataActionResult(version: 2)]
    public class ActionResult : ActionResultV1.ActionResult
    {
        public List<string> LoggedActions { get; private set; }

        private ActionResult(bool isSuccess, string message, string? errorCode, List<string> loggedActions) : base(isSuccess, message, errorCode!)
        {
            LoggedActions = loggedActions;
        }
        public new static ActionResult Success(string message)
        {
            // return new ActionResult(true, message);
            return new ActionResult(true, message, null, new List<string>()); //todo check inheritance use base idk
        }
        public static ActionResult Failure(string message, List<string> loggedActions, string errorCode = "GENERIC_ERROR")
        {
            return new ActionResult(false, message, errorCode, loggedActions);
        }
        public void AppendLoggedActions(string action)
        {
            LoggedActions.Add(action);
        }

        public List<string> GetLoggedActions()
        {
            return LoggedActions;
        }

        public static ActionResult UpgradeV1(ActionResultV1.ActionResult actionResult
            // bool isSuccess, string message, string errorCode
            , List<string> loggedActions
            )
        {
            return new ActionResult(actionResult.IsSuccess, actionResult.Message, actionResult.ErrorCode, loggedActions);
            // return new ActionResultV2(isSuccess, message, errorCode, loggedActions);
        }

        public override ActionResultSF Upgrade(object actionResult)
        {
            List<string> loggedActions = [];

            if (actionResult is ActionResultV1.ActionResult)
            {
                return UpgradeV1((ActionResultV1.ActionResult)actionResult, loggedActions);
            }
            throw new ArgumentException("Provided action result is not an ActionResult (V1).");
        }
    }
}

namespace ActionResultV3
{
    [MetaDataActionResult(version: 3)]
    public class ActionResult : ActionResultSF
    {
        public bool FailedOrNot { get; private set; }
        public string Message { get; private set; }


        protected ActionResult(bool failedOrNot, string message)
        {
            FailedOrNot = failedOrNot;
            Message = message;
        }

        // Factory method
        public static ActionResult Success(string message)
        {
            return new ActionResult(true, message);
        }

        // Factory method
        public static ActionResult Failure(string message)
        {
            return new ActionResult(false, message);
        }

        public override string ToString()
        {
            return $"[Message contains either failure or succes: ] {Message}";
        }
        public static ActionResult UpgradeV2(ActionResultV2.ActionResult actionResult)    //interface plus downgrade async
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
            return new ActionResult(actionResult.IsSuccess, tempMessage);
        }

        public override ActionResultSF Upgrade(object actionResult)
        {
            if (actionResult is ActionResultV2.ActionResult)
            {
                return UpgradeV2((ActionResultV2.ActionResult)actionResult);
            }
            throw new ArgumentException("Provided action result is not an ActionResultV2.");
        }
    }
}