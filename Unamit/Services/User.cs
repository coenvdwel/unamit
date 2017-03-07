using Dapper;
using Nancy;

namespace Unamit.Services
{
  public class User : NancyModule
  {
    public User() : base("/user")
    {
      Post["/"] = _ =>
      {
        using (var conn = Utility.Connect())
        {
          var user = this.TryBind<Models.User>();
          if (user == null) return HttpStatusCode.UnprocessableEntity;

          return new { Success = conn.Execute("INSERT INTO [User] (Id, Password) VALUES (@Id, @Password)", new { Id = user.Id, Password = user.Password.Hash() }) == 1 };
        }
      };
    }
  }
}