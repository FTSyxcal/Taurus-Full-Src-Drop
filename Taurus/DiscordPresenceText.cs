
using System;
namespace Taurus;
internal static class DiscordPresenceText
{
  internal static string Details(bool joined, string zone)
  {
    return !joined ? "Not in a room" : "Trolling Kids in " + zone;
  }
  internal static string State(bool joined, int count, int max)
  {
    return !joined ? "best mod menu" : $"{count.ToString()}/{Math.Max(max, 1).ToString()}";
  }
}
