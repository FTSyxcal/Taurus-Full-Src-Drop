
using System;
namespace Taurus.DiscordRPC.Message;
public abstract class IMessage
{
  private readonly DateTime _timecreated;
  public abstract MessageType Type { get; }
  public DateTime TimeCreated => this._timecreated;
  public IMessage() => this._timecreated = DateTime.Now;
}
