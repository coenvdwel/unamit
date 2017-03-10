using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;

namespace Unamit.Utility
{
  public static class Security
  {
    public static MemoryCache Sessions = new MemoryCache("Unamit_Sessions");
    public static MemoryCache Attempts = new MemoryCache("Unamit_Attempts");

    public static Models.Session Session(string user)
    {
      var session = new Models.Session
      {
        Id = Guid.NewGuid().ToString().Replace("-", ""),
        User = user,
        Expires = DateTimeOffset.UtcNow.AddHours(8)
      };

      Sessions.Add(session.Id, session.User, session.Expires);
      Attempts.Remove(session.User);
      return session;
    }

    public static bool Limited(string user)
    {
      int attempt;
      Attempts.Set(user, attempt = ((Attempts.Get(user) as int?) ?? 0) + 1, DateTimeOffset.UtcNow.AddMinutes(1));
      return (attempt > 3);
    }

    public static string Hash(string s, int i = 777)
    {
      if (i <= 0) return s;

      var hash = new StringBuilder();
      foreach (var b in new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(s), 0, Encoding.UTF8.GetByteCount(s))) hash.Append(b.ToString("x2"));
      return Hash(hash.ToString(), --i);
    }
  }

  public class User : IUserIdentity
  {
    public string UserName { get; }
    public IEnumerable<string> Claims { get; }

    public User(string auth)
    {
      UserName = Security.Sessions.Get(auth) as string;
    }
  }
}