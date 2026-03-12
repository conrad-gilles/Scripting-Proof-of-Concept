using Ember.Scripting;
namespace ActionResultV1
{
    [MetaDataActionResult(version: 1)]
    public class ActionResult : ActionResultBaseClass
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

        public override ActionResultBaseClass Upgrade(ActionResultBaseClass actionResult)
        {
            throw new NotImplementedException(message: "This can not be implemented because and also should never be called since the verson below this one is abstract.");
        }
    }
}

namespace ActionResult_V2
{
    [MetaDataActionResult(version: 2)]
    public class ActionResultV2 : ActionResultV1.ActionResult
    {
        public List<string> LoggedActions { get; private set; }

        private ActionResultV2(bool isSuccess, string message, string? errorCode, List<string> loggedActions) : base(isSuccess, message, errorCode!)
        {
            LoggedActions = loggedActions;
        }
        public new static ActionResultV2 Success(string message)
        {
            // return new ActionResult(true, message);
            return new ActionResultV2(true, message, null, new List<string>()); //todo check inheritance use base idk
        }
        public static ActionResultV2 Failure(string message, List<string> loggedActions, string errorCode = "GENERIC_ERROR")
        {
            return new ActionResultV2(false, message, errorCode, loggedActions);
        }
        public void AppendLoggedActions(string action)
        {
            LoggedActions.Add(action);
        }

        public List<string> GetLoggedActions()
        {
            return LoggedActions;
        }

        public static ActionResultV2 UpgradeV1(ActionResultV1.ActionResult actionResult
            // bool isSuccess, string message, string errorCode
            , List<string> loggedActions
            )
        {
            return new ActionResultV2(actionResult.IsSuccess, actionResult.Message, actionResult.ErrorCode, loggedActions);
            // return new ActionResultV2(isSuccess, message, errorCode, loggedActions);
        }

        public override ActionResultBaseClass Upgrade(ActionResultBaseClass actionResult)
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
        public static ActionResultV3NoInheritance UpgradeV2(ActionResult_V2.ActionResultV2 actionResult)    //interface plus downgrade async
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

        public override ActionResultBaseClass Upgrade(ActionResultBaseClass actionResult)
        {
            if (actionResult is ActionResult_V2.ActionResultV2)
            {
                return UpgradeV2((ActionResult_V2.ActionResultV2)actionResult);
            }
            throw new ArgumentException("Provided action result is not an ActionResultV2.");
        }
    }
}