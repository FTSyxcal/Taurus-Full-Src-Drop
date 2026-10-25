
using Valve.Newtonsoft.Json;
namespace Taurus.DiscordRPC.RPC.Payload;
internal class ClosePayload : IPayload
{
  [JsonProperty("code")]
  public int Code { get; set; }
  [JsonProperty("message")]
  public string Reason { get; set; }
  [JsonConstructor]
  public ClosePayload()
  {
    this.Code = -1;
    this.Reason = "";
  }
}
