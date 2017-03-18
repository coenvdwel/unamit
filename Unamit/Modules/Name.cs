using Dapper;
using Nancy;
using Nancy.Security;
using System;
using System.Linq;
using Unamit.Enums;
using Unamit.Utility;

namespace Unamit.Modules
{
  public class Name : NancyModule
  {
    public Name() : base("/names")
    {
      this.RequiresAuthentication();

      Get["/"] = _ =>
      {
        using (var conn = Database.Connect())
        {
          var gender = (int)Request.Query["Gender"].TryParse<int>((int)Gender.None);
          var exclude = ((string)Request.Query["Exclude[]"].TryParse<string>("")).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

          return conn.Query(@"
            
            DECLARE @Partner nvarchar(150)
            
            SELECT @Partner = u.[Partner]
            FROM [User] u JOIN [User] p ON p.[Id] = u.[Partner] AND p.[Partner] = u.[Id]
            WHERE u.[Id] = @User
            
            SELECT TOP(5) n.[Id], n.[Gender]
            FROM [Name] n
            LEFT OUTER JOIN [Rating] r ON r.[Name] = n.[Id] AND r.[User] = @User
            LEFT OUTER JOIN [Rating] pr ON pr.[Name] = n.[Id] AND pr.[User] = @Partner AND pr.[Value] > 0
            LEFT OUTER JOIN [NameGroups] ng ON ng.[Name] = n.[Id]
            LEFT OUTER JOIN
            (
	            SELECT sng.[Group], SUM(sr.Value) as [Score]
	            FROM [Rating] sr
	            JOIN [NameGroups] sng ON sng.[Name] = sr.[Name]
	            WHERE sr.[User] IN (@User, @Partner)
	            GROUP BY sng.[Group]
            ) gs ON gs.[Group] = ng.[Group]
            WHERE r.[Value] IS NULL AND ((n.[Gender] & @gender) = @gender) AND n.[Id] NOT IN @exclude
            GROUP BY n.[Id], n.[Gender]
            ORDER BY MAX(pr.Value) DESC, MAX(ISNULL(gs.[Score], 0)) DESC, NEWID()
            
          ", new { User = Context.CurrentUser.UserName, gender, exclude }).ToList();
        }
      };

      Get["/{name}"] = _ =>
      {
        using (var conn = Database.Connect())
        {
          return conn.Query(@"
            
            SELECT n.[Id], n.[Gender]
            FROM [Name] n
            WHERE n.[Id] = @Name
            
          ", new { Name = _.name.Value.ToString() }).FirstOrDefault();
        }
      };
    }
  }
}