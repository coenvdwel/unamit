using FluentScheduler;
using System.Linq;
using System.Text;

namespace Unamit.Services
{
  public class Groups : Registry
  {
    public Groups()
    {
      Schedule(Process).ToRunEvery(1).Hours().At(30);
    }

    public void Process()
    {
      using (var conn = Utility.Connect())
      {
        var sb = new StringBuilder();
        var cs = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
          .ToCharArray()
          .Select(x => x.ToString())
          .Concat(new[] { "Oe", "Ie", "Ey", "Ke", "Tje" })
          .ToList();

        // start with
        foreach (var c in cs)
        {
          sb.AppendLine($@"
            
            INSERT [Group] ([Id]) SELECT g.[Id] FROM (SELECT 'Namen die beginnen met een {c}' as [Id]) g LEFT OUTER JOIN [Group] x ON x.[Id] = g.[Id] WHERE x.[Id] IS NULL
            
            INSERT [NameGroups] ([Name], [Group])
            SELECT n.[Id], n.[Group] FROM (SELECT [Id], 'Namen die beginnen met een {c}' as [Group] FROM [Name] WHERE [Id] LIKE '{c}%') n
            LEFT OUTER JOIN [NameGroups] x ON x.[Name] = n.[Id] AND x.[Group] = n.[Group] WHERE x.[Name] IS NULL
          ");
        }

        // end with
        foreach (var c in cs)
        {
          sb.AppendLine($@"
            
            INSERT [Group] ([Id]) SELECT g.[Id] FROM (SELECT 'Namen die eindigen met een {c}' as [Id]) g LEFT OUTER JOIN [Group] x ON x.[Id] = g.[Id] WHERE x.[Id] IS NULL
            
            INSERT [NameGroups] ([Name], [Group])
            SELECT n.[Id], n.[Group] FROM (SELECT [Id], 'Namen die eindigen met een {c}' as [Group] FROM [Name] WHERE [Id] LIKE '%{c}') n
            LEFT OUTER JOIN [NameGroups] x ON x.[Name] = n.[Id] AND x.[Group] = n.[Group] WHERE x.[Name] IS NULL
          ");
        }

        // short names
        sb.AppendLine(@"
          
          INSERT [Group] ([Id]) SELECT g.[Id] FROM (SELECT 'Korte namen' as [Id]) g LEFT OUTER JOIN [Group] x ON x.[Id] = g.[Id] WHERE x.[Id] IS NULL
          
          INSERT [NameGroups] ([Name], [Group])
          SELECT n.[Id], n.[Group] FROM (SELECT [Id], 'Korte namen' as [Group] FROM [Name] WHERE LEN([Id]) < 5) n
          LEFT OUTER JOIN [NameGroups] x ON x.[Name] = n.[Id] AND x.[Group] = n.[Group] WHERE x.[Name] IS NULL
        ");

        // long names
        sb.AppendLine(@"
          
          INSERT [Group] ([Id]) SELECT g.[Id] FROM (SELECT 'Lange namen' as [Id]) g LEFT OUTER JOIN [Group] x ON x.[Id] = g.[Id] WHERE x.[Id] IS NULL
          
          INSERT [NameGroups] ([Name], [Group])
          SELECT n.[Id], n.[Group] FROM (SELECT [Id], 'Lange namen' as [Group] FROM [Name] WHERE LEN([Id]) > 7) n
          LEFT OUTER JOIN [NameGroups] x ON x.[Name] = n.[Id] AND x.[Group] = n.[Group] WHERE x.[Name] IS NULL
        ");

        conn.TryExecute(sb.ToString());
      }
    }
  }
}