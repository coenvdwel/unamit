using FluentScheduler;
using System;
using System.Text;
using System.Text.RegularExpressions;
using Unamit.Enums;

namespace Unamit.Services
{
  public class Babybytes : Registry
  {
    public Babybytes()
    {
      Schedule(Process).ToRunEvery(1).Days().At(3, 0);
    }

    public void Process()
    {
      const string start = "<ul class='names_list'>";
      const string end = "</ul>";

      var regex = new Regex(@"<span .*?>(.*?)<\/span><\/a> (.*?) (jongensnaam|meisjesnaam|gemengdenaam).", RegexOptions.Compiled);
      var sb = new StringBuilder();

      try
      {
        sb.AppendLine("DECLARE @Table TABLE ([Name] nvarchar(150), [Group] nvarchar(150), [Gender] int)");

        for (var i = 1; i < 2500; i++)
        {
          var found = false;
          var s = Utility.Get($"http://www.babybytes.nl/namen/?page={i}");

          s = s.Substring(s.IndexOf(start) + start.Length);
          s = s.Substring(0, s.IndexOf(end));

          foreach (Match m in regex.Matches(s))
          {
            var name = m.Groups[1].Value.Replace("'", "''");
            var group = m.Groups[2].Value.Replace("'", "''");
            var gender = m.Groups[3].Value.ToLower() == "meisjesnaam" ? Gender.Female : m.Groups[3].Value.ToLower() == "jongensnaam" ? Gender.Male : Gender.Unisex;

            if (group.ToLower() == "nieuwe" || group.ToLower() == "lezerbijdrage") group = null;

            found = true;
            sb.AppendLine($"INSERT INTO @Table ([Name], [Group], [Gender]) VALUES ('{name}', '{group}', {(int)gender})");
          }

          if (i % 10 == 0) Finish(sb); // debug
          if (!found) break;
        }

        Finish(sb);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
        throw ex;
      }
    }

    public void Finish(StringBuilder sb)
    {
      using (var conn = Utility.Connect())
      {
        conn.TryExecute(sb.ToString() + @"
          
          DELETE t FROM @Table t
          JOIN (SELECT x.[Name], MAX(x.[Gender]) as [Gender] FROM @Table x GROUP BY x.[Name]) x ON x.[Name] = t.[Name] AND t.[Gender] < x.[Gender]
          
          INSERT [Name] ([Id], [Gender]) SELECT DISTINCT t.[Name], t.[Gender] FROM @Table t
          LEFT OUTER JOIN [Name] s ON s.[Id] = t.[Name] WHERE s.[Id] IS NULL
          
          INSERT [Group] ([Id]) SELECT DISTINCT t.[Group] FROM @Table t
          LEFT OUTER JOIN [Group] s ON s.[Id] = t.[Group] WHERE s.[Id] IS NULL AND t.[Group] IS NOT NULL AND t.[Group] <> ''
          
          INSERT [NameGroups] ([Name], [Group]) SELECT DISTINCT t.[Name], t.[Group] FROM @Table t
          LEFT OUTER JOIN [NameGroups] s ON s.[Name] = t.[Name] AND s.[Group] = t.[Group] WHERE s.[Name] IS NULL AND t.[Group] IS NOT NULL AND t.[Group] <> ''
          
        ");
      }

      sb.Clear();
      sb.AppendLine("DECLARE @Table TABLE ([Name] nvarchar(150), [Group] nvarchar(150), [Gender] int)");
    }
  }
}