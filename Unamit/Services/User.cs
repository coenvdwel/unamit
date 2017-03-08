using Nancy;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Unamit.Services
{
  public class User : NancyModule
  {
    public User() : base("/users")
    {
      Post["/"] = _ =>
      {
        using (var conn = Utility.Connect())
        {
          var user = this.TryBind<Models.User>();
          if (user == null) return HttpStatusCode.UnprocessableEntity;

          user.Password = Hash(user.Password);

          if (!conn.TryExecute("INSERT INTO [User] ([Id], [Password]) VALUES (@Id, @Password)", new { Id = user.Id, Password = user.Password })) return HttpStatusCode.UnprocessableEntity;
          return user;
        }
      };

      Get["/ratings"] = _ =>
      {
        string user;
        if (!Utility.LoggedIn(this, out user)) return HttpStatusCode.Unauthorized;

        using (var conn = Utility.Connect())
        {
          return conn.TryQuery<Models.Rating>("SELECT DISTINCT [User], [Name], [Value] FROM [Rating] WHERE [User] = @User", new { User = user }).ToList();
        }
      };

      Post["/ratings"] = _ =>
      {
        string user;
        if (!Utility.LoggedIn(this, out user)) return HttpStatusCode.Unauthorized;

        using (var conn = Utility.Connect())
        {
          var rating = this.TryBind<Models.Rating>();
          if (rating == null) return HttpStatusCode.UnprocessableEntity;

          rating.User = user;
          if (!conn.TryExecute("INSERT INTO [Rating] ([User], [Name], [Value]) VALUES (@User, @Name, @Value)", new { User = rating.User, Value = rating.Value, Name = rating.Name })) return HttpStatusCode.UnprocessableEntity;
          return rating;
        }
      };
    }

    public static string Hash(string s, int i = 777)
    {
      if (i <= 0) return s;

      var hash = new StringBuilder();
      foreach (var b in new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(s), 0, Encoding.UTF8.GetByteCount(s))) hash.Append(b.ToString("x2"));
      return Hash(hash.ToString(), --i);
    }
  }
}