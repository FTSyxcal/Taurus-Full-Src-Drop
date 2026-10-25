
using Taurus.DiscordRPC.Converters;
using Valve.Newtonsoft.Json;
namespace Taurus.DiscordRPC.RPC.Payload;
internal abstract class IPayload
{
  [JsonProperty("cmd")]
  [JsonConverter(typeof (EnumSnakeCaseConverter))]
  public Command Command { get; set; }
  [JsonProperty("nonce")]
  public string Nonce { get; set; }
  protected IPayload()
  {
  }
  protected IPayload(long nonce) => this.Nonce = nonce.ToString();
  public override string ToString() => $"Payload || Command: {this.Command}, Nonce: {this.Nonce}";
}
