
namespace Taurus.DiscordRPC.Logging;
public class FileLogger : ILogger
{
  private readonly object filelock;
  public LogLevel Level { get; set; }
  public string File { get; set; }
  public FileLogger(string path)
    : this(path, LogLevel.Info)
  {
  }
  public FileLogger(string path, LogLevel level)
  {
    this.Level = level;
    this.File = path;
    this.filelock = new object();
  }
  public void Trace(string message, params object[] args)
  {
    if (this.Level > LogLevel.Trace)
      return;
    lock (this.filelock)
      System.IO.File.AppendAllText(this.File, "\r\nTRCE: " + (args.Length != 0 ? string.Format(message, args) : message));
  }
  public void Info(string message, params object[] args)
  {
    if (this.Level > LogLevel.Info)
      return;
    lock (this.filelock)
      System.IO.File.AppendAllText(this.File, "\r\nINFO: " + (args.Length != 0 ? string.Format(message, args) : message));
  }
  public void Warning(string message, params object[] args)
  {
    if (this.Level > LogLevel.Warning)
      return;
    lock (this.filelock)
      System.IO.File.AppendAllText(this.File, "\r\nWARN: " + (args.Length != 0 ? string.Format(message, args) : message));
  }
  public void Error(string message, params object[] args)
  {
    if (this.Level > LogLevel.Error)
      return;
    lock (this.filelock)
      System.IO.File.AppendAllText(this.File, "\r\nERR : " + (args.Length != 0 ? string.Format(message, args) : message));
  }
}
