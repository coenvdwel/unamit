using Nancy;
using System.Linq;

namespace Unamit.Modules
{
  public class Group : NancyModule
  {
    public Group() : base("/groups")
    {
      Post["/"] = _ =>
      {
        if (!Utility.LoggedIn(this)) return HttpStatusCode.Unauthorized;

        using (var conn = Utility.Connect())
        {
          var group = this.TryBind<Models.Group>();
          if (group == null) return HttpStatusCode.UnprocessableEntity;

          if (!conn.TryExecute("INSERT INTO [Group] (Id) VALUES (@Id)", new { Id = group.Id })) return HttpStatusCode.UnprocessableEntity;
          return group;
        }
      };

      Get["/{group}/names"] = _ =>
      {
        if (!Utility.LoggedIn(this)) return HttpStatusCode.Unauthorized;

        using (var conn = Utility.Connect())
        {
          var names = conn.TryQuery<Models.Name>(@"
            
            SELECT DISTINCT n.Id, n.Gender
            FROM Name n
            JOIN NameGroups ng ON ng.Name = n.Id
            WHERE ng.[Group] = @Group
            
          ", new { Group = (string)_.Group }).ToList();

          return names;
        }
      };

      Post["/{group}/names"] = _ =>
      {
        if (!Utility.LoggedIn(this)) return HttpStatusCode.Unauthorized;

        using (var conn = Utility.Connect())
        {
          var name = this.TryBind<Models.Name>();
          if (name == null) return HttpStatusCode.UnprocessableEntity;

          var namegroup = new { Name = name.Id, Group = (string)_.Group };
          if (!conn.TryExecute("INSERT INTO [NameGroups] ([Name], [Group]) VALUES (@Name, @Group)", namegroup)) return HttpStatusCode.UnprocessableEntity;
          return namegroup;
        }
      };
    }
  }
}