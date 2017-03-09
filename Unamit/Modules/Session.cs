using Nancy;
using System;
using System.Linq;
using System.Runtime.Caching;

namespace Unamit.Modules
{
  public class Session : NancyModule
  {
    public static MemoryCache Attempts = new MemoryCache("Unamit_Attempts");
    public static MemoryCache Sessions = new MemoryCache("Unamit_Sessions");

    public Session() : base("/sessions")
    {
      Post["/"] = _ =>
      {
        var user = this.TryBind<Models.User>();
        if (user == null) return HttpStatusCode.UnprocessableEntity;

        int attempt;
        Attempts.Set(user.Id, attempt = ((Attempts.Get(user.Id) as int?) ?? 0) + 1, DateTimeOffset.UtcNow.AddMinutes(1));
        if (attempt > 3) return HttpStatusCode.TooManyRequests;

        using (var conn = Utility.Connect())
        {
          var success = conn.TryQuery<Models.User>("SELECT 1 FROM [User] WHERE [Id] = @Id AND [Password] = @Password", new { Id = user.Id, Password = Modules.User.Hash(user.Password) }).Any();
          if (!success) return HttpStatusCode.Unauthorized;

          var session = new Models.Session { Id = Utility.GetId(), User = user.Id, Expires = DateTimeOffset.UtcNow.AddHours(8) };

          Attempts.Remove(user.Id);
          Sessions.Add(session.Id, session.User, session.Expires);

          return session;
        }
      };
    }
  }
}