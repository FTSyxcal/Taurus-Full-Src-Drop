
using Taurus.DiscordRPC.RPC.Payload;
namespace Taurus.DiscordRPC.Message;
public class UnsubscribeMessage : IMessage
{
  public override MessageType Type => MessageType.Unsubscribe;
  public EventType Event { get; internal set; }
  internal UnsubscribeMessage(ServerEvent evt)
  {
    if (evt == ServerEvent.ActivityJoin || evt != ServerEvent.ActivityJoinRequest)
      this.Event = EventType.Join;
    else
      this.Event = EventType.JoinRequest;
  }
}
