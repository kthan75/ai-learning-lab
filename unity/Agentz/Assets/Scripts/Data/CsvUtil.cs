using System.Collections.Generic;
using System.Text;

/// <summary>
/// Shared CSV helpers used by the mission and agent loaders.
/// </summary>
public static class CsvUtil
{
    /// <summary>
    /// Parses a single CSV line into fields, handling double-quoted fields that
    /// contain commas. A doubled quote ("") inside a quoted field is an escaped quote.
    /// Unquoted fields are trimmed of surrounding whitespace.
    /// </summary>
    public static List<string> ParseLine(string line)
    {
        var fields = new List<string>();
        int i = 0;

        while (i < line.Length)
        {
            if (line[i] == '"')
            {
                // Quoted field
                i++; // skip opening quote
                var sb = new StringBuilder();
                while (i < line.Length)
                {
                    if (line[i] == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            sb.Append('"'); // escaped quote ""
                            i += 2;
                        }
                        else
                        {
                            i++; // closing quote
                            break;
                        }
                    }
                    else
                    {
                        sb.Append(line[i++]);
                    }
                }
                fields.Add(sb.ToString());
                if (i < line.Length && line[i] == ',') i++; // skip comma
            }
            else
            {
                // Unquoted field
                int start = i;
                while (i < line.Length && line[i] != ',') i++;
                fields.Add(line.Substring(start, i - start).Trim());
                if (i < line.Length) i++; // skip comma
            }
        }

        return fields;
    }

    /// <summary>
    /// Wraps a value as a quoted CSV field, doubling any embedded quotes.
    /// Always quotes, which is safe for values containing commas, quotes, or dashes.
    /// </summary>
    public static string Quote(string value)
    {
        value ??= "";
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
