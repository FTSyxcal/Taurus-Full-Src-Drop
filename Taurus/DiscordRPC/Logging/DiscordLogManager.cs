
using BepInEx.Logging;
namespace Taurus.DiscordRPC.Logging;
public class DiscordLogManager : ILogger
{
  private static ManualLogSource _log = new ManualLogSource("DiscordRPC");
  public LogLevel Level { get; set; }
  static DiscordLogManager() => Logger.Sources.Add((ILogSource) DiscordLogManager._log);
  public void Trace(string message, params object[] args)
  {
    if (this.Level > LogLevel.Trace)
      return;
    DiscordLogManager._log.LogDebug(args.Length != 0 ? (object) string.Format(message, args) : (object) message);
  }
  public void Info(string message, params object[] args)
  {
    if (this.Level > LogLevel.Info)
      return;
    DiscordLogManager._log.LogInfo(args.Length != 0 ? (object) string.Format(message, args) : (object) message);
  }
  public void Warning(string message, params object[] args)
  {
    if (this.Level > LogLevel.Warning)
      return;
    DiscordLogManager._log.LogWarning(args.Length != 0 ? (object) string.Format(message, args) : (object) message);
  }
  public void Error(string message, params object[] args)
  {
    if (this.Level > LogLevel.Error)
      return;
    DiscordLogManager._log.LogError(args.Length != 0 ? (object) string.Format(message, args) : (object) message);
  }
}
