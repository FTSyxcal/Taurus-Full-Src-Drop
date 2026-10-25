
using Taurus.DiscordRPC.RPC.Payload;
namespace Taurus.DiscordRPC.RPC.Commands;
internal class SubscribeCommand : ICommand
{
  public ServerEvent Event { get; set; }
  public bool IsUnsubscribe { get; set; }
  public IPayload PreparePayload(long nonce)
  {
    EventPayload eventPayload = new EventPayload(nonce);
    eventPayload.Command = this.IsUnsubscribe ? Command.Unsubscribe : Command.Subscribe;
    eventPayload.Event = new ServerEvent?(this.Event);
    return (IPayload) eventPayload;
  }
}
