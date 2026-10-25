
namespace Taurus.DiscordRPC.Message;
public class PresenceMessage : IMessage
{
  public override MessageType Type => MessageType.PresenceUpdate;
  public BaseRichPresence Presence { get; internal set; }
  public string Name { get; internal set; }
  public string ApplicationID { get; internal set; }
  internal PresenceMessage()
    : this((RichPresenceResponse) null)
  {
  }
  internal PresenceMessage(RichPresenceResponse rpr)
  {
    if (rpr == null)
    {
      this.Presence = (BaseRichPresence) null;
      this.Name = "No Rich Presence";
      this.ApplicationID = "";
    }
    else
    {
      this.Presence = (BaseRichPresence) rpr;
      this.Name = rpr.Name;
      this.ApplicationID = rpr.ClientID;
    }
  }
}
