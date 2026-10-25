
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Taurus.DiscordRPC.IO;
public static class PipeLocation
{
  private const string DiscordPipePrefix = "discord-ipc-";
  private const int MaximumPipeVariations = 10;
  private static readonly string[] LinuxPackageManagers = new string[2]
  {
    "app/com.discordapp.Discord/",
    "snap.discord/"
  };
  public static IEnumerable<string> GetPipes(int startPipe = 0)
  {
        return !PipeLocation.IsOSUnix() ? Enumerable.Range(startPipe, 10).SelectMany<int, string>(new Func<int, IEnumerable<string>>(PipeLocation.GetWindowsPipes)) : Enumerable.Range(startPipe, 10).SelectMany<int, string>(new Func<int, IEnumerable<string>>(PipeLocation.GetUnixPipes));
  }
  private static IEnumerable<string> GetWindowsPipes(int index)
  {
    yield return $"{"discord-ipc-"}{index}";
  }
  private static IEnumerable<string> GetUnixPipes(int index)
  {
    foreach (string tempDir in PipeLocation.TemporaryDirectories())
    {
      yield return Path.Combine(tempDir, $"{"discord-ipc-"}{index}");
      string[] strArray = PipeLocation.LinuxPackageManagers;
      for (int index1 = 0; index1 < strArray.Length; ++index1)
        yield return Path.Combine(tempDir, strArray[index1], $"{"discord-ipc-"}{index}");
      strArray = (string[]) null;
    }
  }
  private static IEnumerable<string> TemporaryDirectories()
  {
    string environmentVariable1 = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
    if (environmentVariable1 != null)
      yield return environmentVariable1;
    string environmentVariable2 = Environment.GetEnvironmentVariable("TMPDIR");
    if (environmentVariable2 != null)
      yield return environmentVariable2;
    string environmentVariable3 = Environment.GetEnvironmentVariable("TMP");
    if (environmentVariable3 != null)
      yield return environmentVariable3;
    string environmentVariable4 = Environment.GetEnvironmentVariable("TEMP");
    if (environmentVariable4 != null)
      yield return environmentVariable4;
    yield return "/temp";
  }
  private static bool IsOSUnix()
  {
    return Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;
  }
}
