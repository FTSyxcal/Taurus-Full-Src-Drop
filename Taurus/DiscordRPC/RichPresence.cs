
using Valve.Newtonsoft.Json;
namespace Taurus.DiscordRPC;
public sealed class RichPresence : BaseRichPresence
{
  [JsonProperty("buttons")]
  public Button[] Buttons { get; set; }
  public bool HasButtons() => this.Buttons != null && this.Buttons.Length != 0;
  public RichPresence WithState(string state)
  {
    this.State = state;
    return this;
  }
  public RichPresence WithStateUrl(string stateUrl)
  {
    this.StateUrl = stateUrl;
    return this;
  }
  public RichPresence WithDetails(string details)
  {
    this.Details = details;
    return this;
  }
  public RichPresence WithDetailsUrl(string detailsUrl)
  {
    this.DetailsUrl = detailsUrl;
    return this;
  }
  public RichPresence WithType(ActivityType type)
  {
    this.Type = type;
    return this;
  }
  public RichPresence WithStatusDisplay(StatusDisplayType statusDisplay)
  {
    this.StatusDisplay = statusDisplay;
    return this;
  }
  public RichPresence WithTimestamps(Timestamps timestamps)
  {
    this.Timestamps = timestamps;
    return this;
  }
  public RichPresence WithAssets(Assets assets)
  {
    this.Assets = assets;
    return this;
  }
  public RichPresence WithParty(Party party)
  {
    this.Party = party;
    return this;
  }
  public RichPresence WithSecrets(Secrets secrets)
  {
    this.Secrets = secrets;
    return this;
  }
  public RichPresence WithButtons(Button topButton, Button bottomButton = null)
  {
    RichPresence richPresence = this;
    Button[] buttonArray1;
    if (topButton == null || bottomButton == null)
    {
      Button[] buttonArray2;
      if (topButton != null || bottomButton != null)
        buttonArray2 = new Button[1]
        {
          topButton ?? bottomButton
        };
      else
        buttonArray2 = (Button[]) null;
      buttonArray1 = buttonArray2;
    }
    else
      buttonArray1 = new Button[2]
      {
        topButton,
        bottomButton
      };
    richPresence.Buttons = buttonArray1;
    return this;
  }
  public RichPresence Clone()
  {
    RichPresence richPresence = new RichPresence();
    richPresence.State = this._state != null ? this._state.Clone() as string : (string) null;
    richPresence.StateUrl = this._stateUrl != null ? this._stateUrl.Clone() as string : (string) null;
    richPresence.Details = this._details != null ? this._details.Clone() as string : (string) null;
    richPresence.DetailsUrl = this._detailsUrl != null ? this._detailsUrl.Clone() as string : (string) null;
    richPresence.Type = this.Type;
    richPresence.StatusDisplay = this.StatusDisplay;
    richPresence.Buttons = !this.HasButtons() ? (Button[]) null : this.Buttons.Clone() as Button[];
    Secrets secrets;
    if (this.HasSecrets())
      secrets = new Secrets()
      {
        Join = this.Secrets.Join != null ? this.Secrets.Join.Clone() as string : (string) null,
        SpectateSecret = this.Secrets.SpectateSecret != null ? this.Secrets.SpectateSecret.Clone() as string : (string) null
      };
    else
      secrets = (Secrets) null;
    richPresence.Secrets = secrets;
    Timestamps timestamps;
    if (this.HasTimestamps())
      timestamps = new Timestamps()
      {
        Start = this.Timestamps.Start,
        End = this.Timestamps.End
      };
    else
      timestamps = (Timestamps) null;
    richPresence.Timestamps = timestamps;
    Assets assets;
    if (this.HasAssets())
      assets = new Assets()
      {
        LargeImageKey = this.Assets.LargeImageKey != null ? this.Assets.LargeImageKey.Clone() as string : (string) null,
        LargeImageText = this.Assets.LargeImageText != null ? this.Assets.LargeImageText.Clone() as string : (string) null,
        LargeImageUrl = this.Assets.LargeImageUrl != null ? this.Assets.LargeImageUrl.Clone() as string : (string) null,
        SmallImageKey = this.Assets.SmallImageKey != null ? this.Assets.SmallImageKey.Clone() as string : (string) null,
        SmallImageText = this.Assets.SmallImageText != null ? this.Assets.SmallImageText.Clone() as string : (string) null,
        SmallImageUrl = this.Assets.SmallImageUrl != null ? this.Assets.SmallImageUrl.Clone() as string : (string) null
      };
    else
      assets = (Assets) null;
    richPresence.Assets = assets;
    Party party;
    if (this.HasParty())
      party = new Party()
      {
        ID = this.Party.ID,
        Size = this.Party.Size,
        Max = this.Party.Max,
        Privacy = this.Party.Privacy
      };
    else
      party = (Party) null;
    richPresence.Party = party;
    return richPresence;
  }
  internal RichPresence Merge(BaseRichPresence presence)
  {
    this._state = presence.State;
    this._stateUrl = presence.StateUrl;
    this._details = presence.Details;
    this._detailsUrl = presence.DetailsUrl;
    this.Type = presence.Type;
    this.StatusDisplay = presence.StatusDisplay;
    this.Party = presence.Party;
    this.Timestamps = presence.Timestamps;
    this.Secrets = presence.Secrets;
    if (presence.HasAssets())
    {
      if (!this.HasAssets())
        this.Assets = presence.Assets;
      else
        this.Assets.Merge(presence.Assets);
    }
    else
      this.Assets = (Assets) null;
    return this;
  }
  internal override bool Matches(RichPresence other)
  {
    if (!base.Matches(other) || this.Buttons == null ^ other.Buttons == null)
      return false;
    if (this.Buttons != null)
    {
      if (this.Buttons.Length != other.Buttons.Length)
        return false;
      for (int index = 0; index < this.Buttons.Length; ++index)
      {
        Button button1 = this.Buttons[index];
        Button button2 = other.Buttons[index];
        if (button1.Label != button2.Label || button1.Url != button2.Url)
          return false;
      }
    }
    return true;
  }
  public static implicit operator bool(RichPresence presesnce) => presesnce != null;
}
