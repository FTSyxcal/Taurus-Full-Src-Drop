
using System;
using System.Globalization;
using System.Text;
namespace Taurus;
internal static class TelemetryFormat
{
  public static string Build(
    bool leaving,
    string player,
    string playerId,
    string room,
    string map)
  {
    string str1 = leaving ? "**left room**" : "**joined room**";
    string str2 = $"[{{\"name\":\"Player\",\"value\":{TelemetryFormat.Quote($"{TelemetryFormat.Markdown(player)} ({TelemetryFormat.Markdown(playerId)})")},\"inline\":false}},{{\"name\":\"Room Code\",\"value\":{TelemetryFormat.Quote(TelemetryFormat.Markdown(room))},\"inline\":true}},{{\"name\":\"Map\",\"value\":{TelemetryFormat.Quote(TelemetryFormat.Markdown(map))},\"inline\":true}}]";
    return $"{{\"allowed_mentions\":{{\"parse\":[]}},\"embeds\":[{{\"title\":\"Taurus Paid\",\"color\":{(leaving ? "15158332" : "3066993")},\"description\":{TelemetryFormat.Quote(str1)},\"fields\":{str2},\"timestamp\":{TelemetryFormat.Quote(DateTimeOffset.UtcNow.ToString("o", (IFormatProvider) CultureInfo.InvariantCulture))},\"footer\":{{\"text\":{TelemetryFormat.Quote("Taurus 1.4.0")}}}}}]}}";
  }
  private static string Markdown(string value)
  {
    if (string.IsNullOrWhiteSpace(value))
      return "unknown";
    StringBuilder stringBuilder = new StringBuilder();
    foreach (char c in value.Substring(0, Math.Min(value.Length, 128 )))
    {
      if (char.IsControl(c))
      {
        stringBuilder.Append(' ');
      }
      else
      {
        if ("\\*_~`|[]<>".IndexOf(c) >= 0)
          stringBuilder.Append('\\');
        stringBuilder.Append(c);
      }
    }
    return stringBuilder.ToString();
  }
  private static string Quote(string value)
  {
    StringBuilder stringBuilder = new StringBuilder("\"");
    foreach (char ch in value)
    {
      switch (ch)
      {
        case '"':
        case '\\':
          stringBuilder.Append('\\').Append(ch);
          break;
        default:
          if (ch < ' ')
          {
            stringBuilder.Append("\\u").Append(((int) ch).ToString("x4", (IFormatProvider) CultureInfo.InvariantCulture));
            break;
          }
          stringBuilder.Append(ch);
          break;
      }
    }
    return stringBuilder.Append('"').ToString();
  }
}
