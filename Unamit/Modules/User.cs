using Nancy;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Unamit.Modules
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

          if (!conn.TryExecute("INSERT INTO [User] ([Id], [Password], [Partner]) VALUES (@Id, @Password, @Partner)", new { user.Id, user.Password, user.Partner })) return HttpStatusCode.UnprocessableEntity;
          return user;
        }
      };

      Put["/me"] = _ =>
      {
        string user;
        if (!Utility.LoggedIn(this, out user)) return HttpStatusCode.Unauthorized;

        var newUser = this.TryBind<Models.User>();
        if (newUser == null) return HttpStatusCode.UnprocessableEntity;

        newUser.Id = user;
        newUser.Password = string.IsNullOrEmpty(newUser.Password) ? null : Hash(newUser.Password);

        using (var conn = Utility.Connect())
        {
          if (!conn.TryExecute("UPDATE [User] SET [Password] = ISNULL(@Password, [Password]), [Partner] = @Partner WHERE [Id] = @Id", new { newUser.Id, newUser.Password, newUser.Partner })) return HttpStatusCode.UnprocessableEntity;
          return newUser;
        }
      };

      Get["/me/partner"] = _ =>
      {
        string user;
        if (!Utility.LoggedIn(this, out user)) return HttpStatusCode.Unauthorized;

        using (var conn = Utility.Connect())
        {
          return conn.TryQuery<bool?>(@"
            
            SELECT CASE WHEN u.[Partner] IS NULL THEN NULL ELSE CASE WHEN p.[Id] IS NULL THEN 0 ELSE 1 END END
            FROM [User] u
            LEFT OUTER JOIN [User] p ON p.[Id] = u.[Partner] AND p.[Partner] = u.[Id]
            WHERE u.[Id] = @User
            
          ", new { user }).ToList();
        }
      };

      Get["/me/ratings"] = _ =>
      {
        string user;
        if (!Utility.LoggedIn(this, out user)) return HttpStatusCode.Unauthorized;

        using (var conn = Utility.Connect())
        {
          return conn.TryQuery<Models.Rating>(@"
            
            DECLARE @Partner nvarchar(150)
            
            SELECT @Partner = u.[Partner]
            FROM [User] u JOIN [User] p ON p.[Id] = u.[Partner] AND p.[Partner] = u.[Id]
            WHERE u.[Id] = @User
            
            SELECT DISTINCT [User], [Name], [Value] FROM [Rating] WHERE [User] IN (@User, @Partner)
            
          ", new { user }).ToList();
        }
      };

      Post["/me/ratings"] = _ =>
      {
        string user;
        if (!Utility.LoggedIn(this, out user)) return HttpStatusCode.Unauthorized;

        using (var conn = Utility.Connect())
        {
          var rating = this.TryBind<Models.Rating>();
          if (rating == null) return HttpStatusCode.UnprocessableEntity;

          rating.User = user;
          if (!conn.TryExecute("INSERT INTO [Rating] ([User], [Name], [Value]) VALUES (@User, @Name, @Value)", new { rating.User, rating.Value, rating.Name })) return HttpStatusCode.UnprocessableEntity;
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