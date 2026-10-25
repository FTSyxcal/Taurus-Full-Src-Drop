
using Taurus.DiscordRPC.Converters;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Linq;
namespace Taurus.DiscordRPC.RPC.Payload;
internal class EventPayload : IPayload
{
  [JsonProperty("data")]
  public JObject Data { get; set; }
  [JsonProperty("evt")]
  [JsonConverter(typeof (EnumSnakeCaseConverter))]
  public ServerEvent? Event { get; set; }
  public EventPayload() => this.Data = (JObject) null;
  public EventPayload(long nonce)
    : base(nonce)
  {
    this.Data = (JObject) null;
  }
  public T GetObject<T>() => this.Data != null ? ((JToken) this.Data).ToObject<T>() : default (T);
  public override string ToString()
  {
    return $"Event {base.ToString()}, Event: {(this.Event.HasValue ? this.Event.ToString() : "N/A")}";
  }
}
