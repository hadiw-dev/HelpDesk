using System.Text.RegularExpressions;

namespace HelpDesk.Application.Common.Utils;

/// <summary>
/// Comments encode an @-mention as <c>@[Display Name](userId)</c> (the same convention GitHub/GitLab
/// markdown uses), so the raw comment text carries both a human-readable label and a stable ID the
/// backend can resolve without a separate lookup pass. The frontend renders the token as a highlighted pill.
/// </summary>
public static class MentionParser
{
    private static readonly Regex MentionRegex = new(
        @"@\[[^\]]+\]\(([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\)",
        RegexOptions.Compiled);

    public static IReadOnlyList<Guid> ExtractMentionedUserIds(string content)
    {
        var ids = new List<Guid>();

        foreach (Match match in MentionRegex.Matches(content))
        {
            if (Guid.TryParse(match.Groups[1].Value, out var id) && !ids.Contains(id))
            {
                ids.Add(id);
            }
        }

        return ids;
    }
}
