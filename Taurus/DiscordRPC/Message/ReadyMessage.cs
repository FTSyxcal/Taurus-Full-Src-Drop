
using Valve.Newtonsoft.Json;
namespace Taurus.DiscordRPC.Message;
public class ReadyMessage : IMessage
{
  public override MessageType Type => MessageType.Ready;
  [JsonProperty("config")]
  public Configuration Configuration { get; set; }
  [JsonProperty("user")]
  public User User { get; set; }
  [JsonProperty("v")]
  public int Version { get; set; }
}
