using Nancy;
using Unamit.Utility;

namespace Unamit.Modules
{
  public class Session : NancyModule
  {
    public Session() : base("/sessions")
    {
      Post["/"] = _ =>
      {
        var user = this.TryBind<Models.User>();
        if (user == null) return HttpStatusCode.UnprocessableEntity;

        if (Security.Limited(user.Id)) return HttpStatusCode.TooManyRequests;

        using (var conn = Database.Connect())
        {
          if (!conn.TryScalar("SELECT COUNT(*) FROM [User] WHERE [Id] = @Id AND [Password] = @Password", new { Id = user.Id, Password = Security.Hash(user.Password) }, 1)) return HttpStatusCode.Unauthorized;
          return Security.Session(user.Id);
        }
      };
    }
  }
}