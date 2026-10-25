
using System;
using Valve.Newtonsoft.Json;
namespace Taurus.DiscordRPC;
public class User
{
  [JsonProperty("id")]
  public ulong ID { get; private set; }
  [JsonProperty("username")]
  public string Username { get; private set; }
  [JsonProperty("discriminator")]
  [Obsolete("Discord no longer uses discriminators.")]
  public int Discriminator { get; private set; }
  [JsonProperty("global_name")]
  public string DisplayName { get; private set; }
  [JsonProperty("avatar")]
  public string Avatar { get; private set; }
  public bool IsAvatarAnimated => this.Avatar != null && this.Avatar.StartsWith("a_");
  [JsonProperty("avatar_decoration_data")]
  public User.AvatarDecorationData? AvatarDecoration { get; private set; }
  [JsonProperty("bot")]
  public bool Bot { get; private set; }
  [JsonProperty("flags")]
  public User.Flag Flags { get; private set; }
  [JsonProperty("premium_type")]
  public User.PremiumType Premium { get; private set; }
  public string CdnEndpoint { get; private set; }
  internal User() => this.CdnEndpoint = "cdn.discordapp.com";
  internal void SetConfiguration(Configuration configuration)
  {
    this.CdnEndpoint = configuration.CdnHost;
  }
  public string GetAvatarURL() => this.GetAvatarURL(User.AvatarFormat.PNG, User.AvatarSize.x128);
  public string GetAvatarURL(User.AvatarFormat format)
  {
    return this.GetAvatarURL(format, User.AvatarSize.x128);
  }
  public string GetAvatarURL(User.AvatarFormat format, User.AvatarSize size)
  {
    string str = $"/avatars/{this.ID}/{this.Avatar}";
    if (string.IsNullOrEmpty(this.Avatar))
    {
      if (format != User.AvatarFormat.PNG)
        throw new BadImageFormatException($"The user has no avatar and the requested format {format.ToString()} is not supported. (Only supports PNG).");
      int num = (int) ((this.ID >> 22) % 6UL);
      if (this.Discriminator > 0)
        num = this.Discriminator % 5;
      str = $"/embed/avatars/{num}";
    }
    return $"https://{this.CdnEndpoint}{str}{this.GetAvatarExtension(format)}?size={(int) size}&animated=true";
  }
  public string GetAvatarDecorationURL() => this.GetAvatarDecorationURL(User.AvatarFormat.PNG);
  public string GetAvatarDecorationURL(User.AvatarFormat format)
  {
    return !this.AvatarDecoration.HasValue ? (string) null : $"https://{this.CdnEndpoint}{"/avatar-decoration-presets/" + this.AvatarDecoration.Value.Asset}{this.GetAvatarExtension(format)}";
  }
  public string GetAvatarExtension(User.AvatarFormat format)
  {
    return "." + format.ToString().ToLowerInvariant();
  }
  public override string ToString()
  {
    if (!string.IsNullOrEmpty(this.DisplayName))
      return this.DisplayName;
    return this.Discriminator != 0 ? $"{this.Username}#{this.Discriminator.ToString("D4")}" : this.Username;
  }
  public enum AvatarFormat
  {
    PNG,
    JPEG,
    WebP,
    GIF,
  }
  public enum AvatarSize
  {
    x16 = 16,
    x32 = 32,
    x64 = 64,
    x128 = 128,
    x256 = 256,
    x512 = 512,
    x1024 = 1024,
    x2048 = 2048,
  }
  public struct AvatarDecorationData
  {
    [JsonProperty("asset")]
    public string Asset { get; private set; }
    [JsonProperty("skuId")]
    public string SKU { get; private set; }
  }
  [System.Flags]
  public enum Flag
  {
    None = 0,
    Employee = 1,
    Partner = 2,
    HypeSquad = 4,
    BugHunter = 8,
    HouseBravery = 64,
    HouseBrilliance = 128,
    HouseBalance = 256,
    EarlySupporter = 512,
    TeamUser = 1024,
    BugHunterLevel2 = 16384,
    VerifiedBot = 65536,
    VerifiedDeveloper = 131072,
    CertifiedModerator = 262144,
    BotHttpInteractions = 524288,
    ActiveDeveloper = 4194304,
  }
  public enum PremiumType
  {
    None,
    NitroClassic,
    Nitro,
    NitroBasic,
  }
}
