using Dapper;
using Nancy;
using Nancy.ModelBinding;

namespace Unamit.Services
{
  public class User : NancyModule
  {
    public User() : base("/user")
    {
      Put["/"] = _ =>
      {
        using (var conn = Database.Connect())
        {
          return conn.Execute("INSERT INTO [User] (Id, Password) VALUES (@Id, @Password)", this.Bind<User>());
        }
      };
    }
  }
}