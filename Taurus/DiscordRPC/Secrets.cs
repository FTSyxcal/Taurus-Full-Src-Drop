
using System;
using System.Text;
using Taurus.DiscordRPC.Exceptions;
using Valve.Newtonsoft.Json;
namespace Taurus.DiscordRPC;
[Serializable]
public class Secrets
{
  [Obsolete("This feature has been deprecated my Mason in issue #152 on the offical library. Was originally used as a Notify Me feature, it has been replaced with Join / Spectate.", true)]
  [JsonIgnore]
  public string MatchSecret;
  private string _joinSecret;
  public const int SecretLength = 128 ;
  [JsonProperty("join")]
  public string Join
  {
    get => this._joinSecret;
    set
    {
      if (!BaseRichPresence.ValidateString(value, out this._joinSecret, false, 128 ))
        throw new StringOutOfRangeException(128 );
    }
  }
  [Obsolete("Property name is redundant and replaced with Join.")]
  [JsonIgnore]
  public string JoinSecret
  {
    get => this.Join;
    set => this.Join = value;
  }
  [Obsolete("Spectating is no longer supported by Discord.")]
  [JsonIgnore]
  public string SpectateSecret { get; set; }
  public static Encoding Encoding => Encoding.UTF8;
  public static string CreateSecret(Random random, int length = 128 )
  {
    byte[] numArray = length >= 1 && length <= 128 ? new byte[length] : throw new ArgumentOutOfRangeException(nameof (length), "Secret length must be between 1 and 128 characters.");
    random.NextBytes(numArray);
    return Secrets.Encoding.GetString(numArray);
  }
  public static string CreateFriendlySecret(Random random, int length = 128 )
  {
    if (length < 1 || length > 128 )
      throw new ArgumentOutOfRangeException(nameof (length), "Secret length must be between 1 and 128 characters.");
    string str = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    StringBuilder stringBuilder = new StringBuilder();
    for (int index = 0; index < length; ++index)
      stringBuilder.Append(str[random.Next(str.Length)]);
    return stringBuilder.ToString();
  }
}
