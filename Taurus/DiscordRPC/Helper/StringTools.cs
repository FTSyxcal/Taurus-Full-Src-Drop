
using System;
using System.Text;
namespace Taurus.DiscordRPC.Helper;
public static class StringTools
{
  public static string GetNullOrString(this string str)
  {
    return str.Length != 0 && !string.IsNullOrEmpty(str.Trim()) ? str : (string) null;
  }
  public static bool WithinLength(this string str, int bytes)
  {
    return str.WithinLength(bytes, Encoding.UTF8);
  }
  public static bool WithinLength(this string str, int bytes, Encoding encoding)
  {
    return encoding.GetByteCount(str) <= bytes;
  }
  public static string ToCamelCase(this string str)
  {
    if (str == null)
      return (string) null;
    string[] strArray = str.ToLowerInvariant().Split(new string[2]
    {
      "_",
      " "
    }, StringSplitOptions.RemoveEmptyEntries);
    StringBuilder stringBuilder = new StringBuilder();
    foreach (string str1 in strArray)
      stringBuilder.Append(char.ToUpper(str1[0]).ToString() + str1.Substring(1));
    return stringBuilder.ToString();
  }
  public static string ToSnakeCase(this string str)
  {
    if (str == null)
      return (string) null;
    StringBuilder stringBuilder = new StringBuilder();
    for (int index = 0; index < str.Length; ++index)
    {
      if (index > 0 && char.IsUpper(str[index]))
        stringBuilder.Append('_');
      stringBuilder.Append(str[index]);
    }
    return stringBuilder.ToString().ToUpperInvariant();
  }
}
