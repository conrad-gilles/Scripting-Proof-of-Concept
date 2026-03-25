using Ember.Scripting;

namespace Sandbox;

public class SandBoxUserSession : IUserSession
{
    public Guid Id => new Guid("11111111-2222-3333-4444-555555555555");
    public string UserName => "SandboxTestUser";
}