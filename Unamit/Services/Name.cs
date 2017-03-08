using Nancy;
using System.Linq;

namespace Unamit.Services
{
  public class Name : NancyModule
  {
    public Name() : base("/names")
    {
      Get["/"] = _ =>
      {
        string user;
        if (!Utility.LoggedIn(this, out user)) return HttpStatusCode.Unauthorized;

        using (var conn = Utility.Connect())
        {
          var name = conn.TryQuery<Models.Name>(@"
            
            SELECT TOP 1 n.[Id], n.[Gender]
            FROM [Name] n
            LEFT OUTER JOIN [Rating] r ON r.[Name] = n.[Id] AND r.[User] = @User
            WHERE r.[User] IS NULL
            -- todo: order by group score
            
          ", new { User = user }).FirstOrDefault();

          if (name == null) return HttpStatusCode.NoContent;
          return name;
        }
      };

      Post["/"] = _ =>
      {
        if (!Utility.LoggedIn(this)) return HttpStatusCode.Unauthorized;

        using (var conn = Utility.Connect())
        {
          var name = this.TryBind<Models.Name>();
          if (name == null) return HttpStatusCode.UnprocessableEntity;

          if (!conn.TryExecute("INSERT INTO [Name] ([Id], [Gender]) VALUES (@Id, @Gender)", new { Id = name.Id, Gender = name.Gender })) return HttpStatusCode.UnprocessableEntity;
          return name;
        }
      };

      Get["/{name}/groups"] = _ =>
      {
        if (!Utility.LoggedIn(this)) return HttpStatusCode.Unauthorized;

        using (var conn = Utility.Connect())
        {
          var groups = conn.TryQuery<Models.Group>("SELECT [Group] as Id FROM [NameGroups] WHERE [Name] = @Name", new { Name = (string)_.Name }).ToList();

          return groups;
        }
      };

      Post["/{name}/groups"] = _ =>
      {
        if (!Utility.LoggedIn(this)) return HttpStatusCode.Unauthorized;

        using (var conn = Utility.Connect())
        {
          var group = this.TryBind<Models.Group>();
          if (group == null) return HttpStatusCode.UnprocessableEntity;

          var namegroup = new { Name = (string)_.Name, Group = group.Id };
          if (!conn.TryExecute("INSERT INTO [NameGroups] ([Name], [Group]) VALUES (@Name, @Group)", namegroup)) return HttpStatusCode.UnprocessableEntity;
          return namegroup;
        }
      };
    }
  }
}