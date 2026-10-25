
using Valve.Newtonsoft.Json;
namespace Taurus.DiscordRPC.Message;
public class JoinRequestMessage : IMessage
{
  public override MessageType Type => MessageType.JoinRequest;
  [JsonProperty("user")]
  public User User { get; internal set; }
}
