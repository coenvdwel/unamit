using Dapper;
using Nancy;
using System;
using System.Linq;
using System.Runtime.Caching;

namespace Unamit.Services
{
  public class Session : NancyModule
  {
    public static MemoryCache Attempts = new MemoryCache("Unamit_Attempts");
    public static MemoryCache Sessions = new MemoryCache("Unamit_Sessions");

    public Session() : base("/session")
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
          var success = conn.Query<Models.User>("SELECT 1 FROM [User] WHERE Id = @Id AND Password = @Password", new { Id = user.Id, Password = user.Password.Hash() }).Any();
          if (!success) return HttpStatusCode.Unauthorized;

          Attempts.Remove(user.Id);

          var session = new Models.Session { Id = Utility.GetId(), User = user.Id };
          return Sessions.AddOrGetExisting(user.Id, session, DateTimeOffset.UtcNow.AddHours(8)) as Models.Session ?? session;
        }
      };
    }
  }
}