
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Linq;
namespace Taurus.DiscordRPC.RPC.Payload;
internal class ArgumentPayload : IPayload
{
  [JsonProperty("args")]
  public JObject Arguments { get; set; }
  public ArgumentPayload() => this.Arguments = (JObject) null;
  public ArgumentPayload(long nonce)
    : base(nonce)
  {
    this.Arguments = (JObject) null;
  }
  public ArgumentPayload(object args, long nonce)
    : base(nonce)
  {
    this.SetObject(args);
  }
  public void SetObject(object obj) => this.Arguments = JObject.FromObject(obj);
  public T GetObject<T>() => ((JToken) this.Arguments).ToObject<T>();
  public override string ToString() => "Argument " + base.ToString();
}
