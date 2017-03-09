using Nancy;
using System.Linq;

namespace Unamit.Modules
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
            
            DECLARE @Partner nvarchar(150)
            DECLARE @GroupScores table ([Group] nvarchar(150), Score int)

            SELECT @Partner = u.[Partner]
            FROM [User] u JOIN [User] p ON p.[Id] = u.[Partner] AND p.[Partner] = u.[Id]
            WHERE u.[Id] = @User

            INSERT INTO @GroupScores SELECT ng.[Group], SUM(r.Value) as [Score]
            FROM [Rating] r JOIN [NameGroups] ng ON ng.[Name] = r.[Name]
            WHERE r.[User] IN (@User, @Partner)
            GROUP BY ng.[Group]

            SELECT TOP(1) n.[Id], n.[Gender]
            FROM [Name] n
            LEFT OUTER JOIN [Rating] r ON r.[Name] = n.[Id] AND r.[User] = @User
            LEFT OUTER JOIN [Rating] pr ON pr.[Name] = n.[Id] AND pr.[User] = @Partner AND pr.[Value] > 0
            LEFT OUTER JOIN [NameGroups] ng ON ng.[Name] = n.[Id]
            LEFT OUTER JOIN @GroupScores gs ON gs.[Group] = ng.[Group]
            WHERE r.[Value] IS NULL -- exclude those you already rated
            ORDER BY pr.Value DESC, ISNULL(gs.[Score], 0) DESC
            
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