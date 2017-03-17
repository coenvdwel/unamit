using Dapper;
using Nancy;
using Nancy.Security;
using System.Linq;
using Unamit.Utility;

namespace Unamit.Modules
{
  public class User : NancyModule
  {
    public User() : base("/users")
    {
      Post["/"] = _ =>
      {
        using (var conn = Database.Connect())
        {
          var user = this.TryBind<Models.User>();
          if (user == null) return HttpStatusCode.UnprocessableEntity;

          user.Password = Security.Hash(user.Password);

          if (!conn.TryExecute("INSERT INTO [User] ([Id], [Password], [Partner]) VALUES (@Id, @Password, @Partner)", new { user.Id, user.Password, user.Partner })) return HttpStatusCode.UnprocessableEntity;
          return user;
        }
      };

      Get["/me"] = _ =>
      {
        this.RequiresAuthentication();

        using (var conn = Database.Connect())
        {
          return conn.Query(@"
            
            SELECT u.[Id], u.[Partner], CASE WHEN p.[Id] IS NULL THEN 0 ELSE 1 END as Mutual
            FROM [User] u
            LEFT OUTER JOIN [User] p ON p.[Id] = u.[Partner] AND p.[Partner] = u.[Id]
            WHERE u.[Id] = @User

          ", new { User = Context.CurrentUser.UserName }).FirstOrDefault();
        }
      };

      Put["/me"] = _ =>
      {
        this.RequiresAuthentication();

        var newUser = this.TryBind<Models.User>();
        if (newUser == null) return HttpStatusCode.UnprocessableEntity;

        newUser.Id = Context.CurrentUser.UserName;
        newUser.Password = string.IsNullOrEmpty(newUser.Password) ? null : Security.Hash(newUser.Password);
        newUser.Partner = string.IsNullOrEmpty(newUser.Partner) ? null : newUser.Partner;

        using (var conn = Database.Connect())
        {
          if (!conn.TryExecute("UPDATE [User] SET [Password] = ISNULL(@Password, [Password]), [Partner] = @Partner WHERE [Id] = @Id", new { newUser.Id, newUser.Password, newUser.Partner })) return HttpStatusCode.UnprocessableEntity;
          return newUser;
        }
      };

      Get["/me/ratings"] = _ =>
      {
        this.RequiresAuthentication();

        using (var conn = Database.Connect())
        {
          return conn.Query(@"
            
            DECLARE @Partner nvarchar(150)     
            SELECT @Partner = u.[Partner]
            FROM [User] u JOIN [User] p ON p.[Id] = u.[Partner] AND p.[Partner] = u.[Id]
            WHERE u.[Id] = @User
            
            SELECT n.[Id], n.[Gender], r.[Value], r.[PartnerValue]
            FROM [Name] n
            JOIN (
              SELECT ISNULL(u.[Id], p.[Id]) as [Id], u.[Value], p.[Value] as [PartnerValue]
              FROM (SELECT r.[Name] as [Id], r.[Value] FROM [Rating] r WHERE r.[User] = @User) u
              FULL OUTER JOIN (SELECT r.[Name] as [Id], r.[Value] FROM [Rating] r WHERE r.[User] = @Partner) p ON u.[Id] = p.[Id]
              WHERE u.[Value] > 0 OR p.[Value] > 0
            ) r ON r.[Id] = n.[Id]
            ORDER BY (ISNULL(r.[Value], 0) + ISNULL(r.[PartnerValue], 0)) DESC
            
          ", new { User = Context.CurrentUser.UserName }).ToList();
        }
      };

      Post["/me/ratings"] = _ =>
      {
        this.RequiresAuthentication();

        using (var conn = Database.Connect())
        {
          var rating = this.TryBind<Models.Rating>();
          if (rating == null) return HttpStatusCode.UnprocessableEntity;

          rating.User = Context.CurrentUser.UserName;
          if (!conn.TryExecute("INSERT INTO [Rating] ([User], [Name], [Value]) VALUES (@User, @Name, @Value)", new { rating.User, rating.Value, rating.Name })) return HttpStatusCode.UnprocessableEntity;
          return rating;
        }
      };
    }
  }
}