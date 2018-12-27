using FluentScheduler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Unamit.Enums;
using Unamit.Utility;

namespace Unamit.Services
{
  public class Babybytes : Registry
  {
    public Babybytes()
    {
      Schedule(Process).ToRunEvery(1).Days().At(3, 0);
    }

    public Dictionary<string, string[]> Mappings = new Dictionary<string, string[]>
    {
      { "", new[] { "nieuwe", "lezerbijdrage", "lezerbijdage", "onbekend", "zelf bedacht", "the bold and the beautyfull", "suzanne", "indianennaam" } },
      { "Nederlandse", new[] {"nederlands", "nederland"} },
      { "Zweedse", new[] { "zweden", "zweeds" } },
      { "Indische", new[] { "indische", "oudindische", "names_indi" } },
      { "Spaanse", new[] { "spaans", "spanje" } },
      { "Scandinavische", new[] { "scandinavisch" } },
      { "Keltische", new[] { "keltisch" } },
      { "Japanse", new[] { "japans", "japan" } },
      { "Italiaanse", new[] { "itali", "italiaans", "italie", "italië" } },
      { "Iraanse", new[] { "iran", "iraans" } },
      { "Hebreeuwse", new[] { "hebreeuws", "names_hebr" } },
      { "Georgische", new[] { "georgisch", "georgië", "georgie" } },
      { "Griekse", new[] { "grieks/frans", "grieks", "griekenland" } },
      { "Friese", new[] { "fries", "friesland" } },
      { "Franse", new[] { "frans", "frankrijk", "names_fren" } },
      { "Arabische", new[] { "engelse-arabische", "arabisch,fries", "arabisch", "arabie", "arabië" } },
      { "Engelse", new[] { "engels", "engeland", "oudengelse" } },
      { "Armeense", new[] { "aramese", "armenie", "armenië" } }
    };

    public void Process()
    {
      var regex = new Regex(@"<div class='nameblock'>\s*<div .*?>(.*?)<\/div>(.*?)<br>(.*?) <span .*? <\/div>", RegexOptions.Compiled);
      var sb = new StringBuilder();

      try
      {
        sb.AppendLine("DECLARE @Table TABLE ([Name] nvarchar(150), [Group] nvarchar(150), [Gender] int)");

        for (var i = 1; i < 2500; i++)
        {
          var found = false;
          var s = Get($"http://www.babybytes.nl/namen/?page={i}");

          foreach (Match m in regex.Matches(s))
          {
            var name = m.Groups[1].Value.Replace("'", "''").Trim();
            var group = m.Groups[2].Value.Replace("'", "''").Trim();
            var gender = m.Groups[3].Value.Trim().ToLower().Contains("meisjesnamen") ? Gender.Female : m.Groups[3].Value.Trim().ToLower().Contains("jongensnamen") ? Gender.Male : Gender.Unisex;

            if (Mappings.Any(x => x.Value.Contains(group.ToLower()))) group = Mappings.First(x => x.Value.Contains(group.ToLower())).Key;
            if (string.IsNullOrEmpty(group)) group = null;

            found = true;
            sb.AppendLine($"INSERT INTO @Table ([Name], [Group], [Gender]) VALUES ('{name}', '{group}', {(int)gender})");
          }

          if (!found) break;
        }

        using (var conn = Database.Connect())
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
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
        throw ex;
      }
    }

    public static string Get(string uri)
    {
      try
      {
        var req = (HttpWebRequest)WebRequest.Create(uri);
        using (var res = (HttpWebResponse)req.GetResponse())
        {
          using (var s = res.GetResponseStream())
          {
            if (s == null) return null;
            using (var r = new StreamReader(s))
            {
              return r.ReadToEnd();
            }
          }
        }
      }
      catch (Exception ex)
      {
#if DEBUG
        throw ex;
#else
        return null;
#endif
      }
    }
  }
}