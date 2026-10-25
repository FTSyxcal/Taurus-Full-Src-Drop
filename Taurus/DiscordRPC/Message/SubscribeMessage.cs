
using Taurus.DiscordRPC.RPC.Payload;
namespace Taurus.DiscordRPC.Message;
public class SubscribeMessage : IMessage
{
  public override MessageType Type => MessageType.Subscribe;
  public EventType Event { get; internal set; }
  internal SubscribeMessage(ServerEvent evt)
  {
    if (evt == ServerEvent.ActivityJoin || evt != ServerEvent.ActivityJoinRequest)
      this.Event = EventType.Join;
    else
      this.Event = EventType.JoinRequest;
  }
}
