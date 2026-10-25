
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class MenuView : IDisposable
{
  private readonly Transform root;
  private readonly MenuState state;
  private readonly bool interactive;
  private readonly BoxCollider[] buttons;
  private readonly MaterialPropertyBlock colors = new MaterialPropertyBlock();
  private readonly Renderer background;
  private readonly Renderer disconnect;
  private readonly Renderer[] gradients;
  private readonly TextMesh footer;
  private readonly GameObject notificationPanel;
  private string footerText = "";
  private int notificationCategory;
  private bool notificationActive;
  private readonly Dictionary<Transform, float> presses = new Dictionary<Transform, float>();
  private readonly Dictionary<Transform, Vector3> pressSizes = new Dictionary<Transform, Vector3>();
  private readonly Dictionary<Transform, float> hoverAmounts = new Dictionary<Transform, float>();
  private float hoverTime;
  private readonly List<Transform> finished = new List<Transform>();
  private BoxCollider hovered;
  private readonly MenuDesign design;
  private bool favoriteMode;
  public Transform Root => this.root;
  public string HoveredButton
  {
    get
    {
      return !((Object) this.hovered == (Object) null) ? ((Object) this.hovered).name : "";
    }
  }
  public Vector3 DesignSlide(float time) => this.design.SlideOffset(time);
  public MenuView(Transform root, MenuState state, bool interactive = true)
  {
    this.root = root;
    this.state = state;
    this.interactive = interactive;
    string[] strArray = new string[11]
    {
      "PastCategory0",
      "PastCategory1",
      "PastCategory2",
      "PastCategory3",
      "PastCategory4",
      "PastCategory5",
      "PastCategoryPrevious",
      "PastCategoryNext",
      "DesignLogo",
      "ButtonEnabled",
      "ButtonRoomStatus"
    };
    foreach (string str in strArray)
    {
      if (((Object) root.Find(str) == (Object) null))
      {
        Transform transform = Object.Instantiate<Transform>(root.Find("ButtonHome"), root, false);
        ((Object) transform).name = str;
        ((Component) transform).gameObject.SetActive(false);
      }
    }
    this.design = new MenuDesign(root);
    this.buttons = ((Component) root).GetComponentsInChildren<BoxCollider>(true);
    this.background = ((Component) root.Find("Background")).GetComponent<Renderer>();
    this.disconnect = ((Component) root.Find("ButtonDisconnect")).GetComponent<Renderer>();
    this.gradients = new Renderer[3]
    {
      ((Component) root.Find("Frame")).GetComponent<Renderer>(),
      ((Component) root.Find("DisconnectFrame")).GetComponent<Renderer>(),
      ((Component) root.Find("Title")).GetComponent<Renderer>()
    };
    this.footer = ((Component) root.Find("PageLabel")).GetComponent<TextMesh>();
    this.notificationPanel = ((Component) root.Find("NotificationBackground")).gameObject;
    this.Refresh();
  }
  public void Refresh()
  {
    if (this.design.Apply(this.state))
    {
      this.presses.Clear();
      this.pressSizes.Clear();
      this.hoverAmounts.Clear();
    }
    bool flag1 = this.state.Category == 11;
    bool flag2 = flag1 && !this.state.DiscordAccepted;
    bool colorPickerOpen = this.state.ColorPickerOpen;
    ((Component) this.root.Find("DesignLogo")).gameObject.SetActive((this.state.Design == 10 || this.state.Design == 13) && !flag1);
    ((Component) this.root.Find("ButtonEnabled")).gameObject.SetActive(this.state.Design == 12 && !flag1);
    ((Component) this.root.Find("ButtonRoomStatus")).gameObject.SetActive(this.state.Design == 12 && !flag1);
    foreach (Transform transform in this.root)
    {
      if (((Object) transform).name.StartsWith("PastCategory", StringComparison.Ordinal))
        ((Component) transform).gameObject.SetActive(this.state.Design == 8 && !flag1);
    }
    ((Component) this.root.Find("Title")).gameObject.SetActive(!flag1);
    ((Component) this.root.Find("ButtonDisconnect")).gameObject.SetActive(!flag1);
    ((Component) this.root.Find("DisconnectFrame")).gameObject.SetActive(!flag1);
    ((Component) this.root.Find("ButtonPrevious")).gameObject.SetActive(!colorPickerOpen && !flag1 | flag2);
    ((Component) this.root.Find("ButtonNext")).gameObject.SetActive(!colorPickerOpen && !flag1 | flag2);
    if (this.notificationCategory != this.state.Category)
      this.notificationActive = false;
    for (int row = 0; row < 12; ++row)
    {
      Transform transform1 = this.root.Find("Row" + row.ToString());
      int index = this.state.EntryAt(row);
      string name = this.state.SoundNameAt(row);
      ((Component) transform1).gameObject.SetActive(!flag1 && !colorPickerOpen && (!this.state.KeyboardOpen || this.state.Design == 9) && (index >= 0 || name != null));
      bool flag3 = index >= 0 && this.state.Category != 1 && MenuState.IsIncremental(MenuCatalog.Buttons[index]);
      BoxCollider component1 = ((Component) transform1).GetComponent<BoxCollider>();
      ((Collider) component1).enabled = !flag3 || this.favoriteMode || index >= 0 && MenuCatalog.Buttons[index].Name == "Theme Black Red" && this.state.Theme == "Custom";
      component1.center = new Vector3(0.0f, flag3 ? 0.055f : 0.0f, 0.0f);
      component1.size = (new Vector3(0.01f, flag3 ? 0.214f : 0.324f, 0.05f) / transform1.localScale.x);
      string[] strArray = new string[2]{ "Minus", "Plus" };
      foreach (string str in strArray)
      {
        Transform transform2 = this.root.Find($"Increment{str}{row.ToString()}");
        ((Component) transform2).gameObject.SetActive(((Component) transform1).gameObject.activeSelf & flag3);
        ((Collider) ((Component) transform2).GetComponent<BoxCollider>()).enabled = flag3 && (this.favoriteMode || this.state.CanAdjust(MenuCatalog.Buttons[index], str == "Minus" ? -1 : 1));
      }
      if (name != null)
      {
        ((Component) transform1.Find("FavoriteIcon")).gameObject.SetActive(false);
        TextMesh component2 = ((Component) transform1.Find("Label")).GetComponent<TextMesh>();
        component2.text = MenuView.Wrap(name, 24);
        component2.characterSize = 0.0017f;
        component2.color = MenuStyle.Text;
        ((Component) component2).transform.localPosition = new Vector3(3f / 500f, 0.142f, 0.0f);
        TextMesh component3 = ((Component) transform1.Find("State")).GetComponent<TextMesh>();
        ((Component) component3).transform.localPosition = new Vector3(3f / 500f, -0.142f, 0.0f);
        component3.characterSize = 0.0011f;
        component3.color = MenuStyle.Mint;
        component3.text = name == "Soundboard controls" || name == "Parent folder" || name.StartsWith("Folder: ") ? "Open" : (name == this.state.PlayingSound ? "Playing" : "Play");
      }
      else if (index >= 0)
      {
        MenuEntry button = MenuCatalog.Buttons[index];
        TextMesh component4 = ((Component) transform1.Find("Label")).GetComponent<TextMesh>();
        bool flag4 = button.Target < 0;
        bool flag5 = this.state.ShowsAvailable(button);
        component4.text = flag3 ? MenuView.SettingLabel(button.Name) : MenuView.Wrap(this.state.FeatureLabel(button), flag4 ? 18 : 23);
        bool flag6 = this.state.IsFavorite(index);
        ((Component) transform1.Find("FavoriteIcon")).gameObject.SetActive(flag6);
        ((Component) component4).transform.localPosition = new Vector3(3f / 500f, flag6 ? 0.121f : 0.142f, 0.0f);
        component4.characterSize = flag4 || button.Name.Length > 23 ? 0.0017f : 0.0022f;
        component4.color = button.RequiresAntiban ? MenuStyle.Error : Color.white;
        if (button.Target == 18)
          component4.color = flag5 ? MenuStyle.Mint : MenuStyle.Error;
        TextMesh component5 = ((Component) transform1.Find("State")).GetComponent<TextMesh>();
        string str1 = button.Name == "Menu Hand" ? (this.state.RightHanded ? "Right" : "Left") : (button.Name.StartsWith("VR size") ? Mathf.RoundToInt(this.state.VrScale * 100f).ToString() + "%" : (button.Name == "Fly Speed Up" ? this.state.FlySpeed.ToString("0") : (button.Name == "Theme Black Red" ? MenuView.ThemeName(this.state.Theme) : (button.Toggle ? (this.state.IsOn(index) ? "On" : "Off") : ""))));
        if (button.Name == "PC size")
          str1 = Mathf.RoundToInt(this.state.PcScale * 100f).ToString() + "%";
        if (button.Name == "Click sound")
          str1 = MenuClickSounds.Names[this.state.ClickSound];
        if (button.Name == "Design")
          str1 = MenuDesign.Names[this.state.Design];
        if (button.Name == "Font")
          str1 = MenuFonts.Names[this.state.FontIndex];
        if (button.Name == "Theme Black Red" && this.state.Design != 0 && this.state.Theme == "ItzGame")
          str1 = "Design default";
        if (button.Name == "Click volume" || button.Name == "Whoosh volume")
          str1 = Mathf.RoundToInt((float) ((button.Name == "Click volume" ? (double) this.state.ClickVolume : (double) this.state.WhooshVolume) * 100.0)).ToString() + "%";
        if (button.Name == "Gradient speed")
          str1 = this.state.GradientPeriod.ToString("0") + "s / loop";
        if (button.Name == "Local volume" || button.Name == "Mic volume")
          str1 = Mathf.RoundToInt((float) ((button.Name == "Local volume" ? (double) this.state.SoundVolume : (double) this.state.MicVolume) * 100.0)).ToString() + "%";
        ((Component) component5).transform.localPosition = new Vector3(3f / 500f, flag3 ? -0.049f : -0.142f, 0.0f);
        string str2 = !flag5 || button.RequiresAntiban || MenuState.RequiresMaster(button) ? this.state.AvailabilityLabel(button) : "";
        if (str2 == "requires master client")
          str2 = "requires master\nclient";
        component5.text = flag4 || button.RequiresAntiban ? str2 + (!flag5 || str1.Length <= 0 ? "" : (str2.Length > 0 ? "\n" : "") + str1) : "";
        component5.characterSize = 0.0011f;
        component5.color = flag5 ? MenuStyle.Mint : MenuStyle.Error;
      }
    }
    ((Component) this.root.Find("BrowserSurface")).gameObject.SetActive(flag1 && this.state.DiscordAccepted && !this.state.KeyboardOpen);
    ((Component) this.root.Find("BrowserWarning")).gameObject.SetActive(flag2);
    ((Component) this.root.Find("Keyboard")).gameObject.SetActive(this.state.KeyboardOpen && !flag2);
    this.root.Find("Keyboard").localPosition = new Vector3(0.0f, 0.0f, flag1 ? 0.05f : 0.0f);
    foreach (Transform transform in this.root.Find("Keyboard"))
    {
      if (((Object) transform).name == "Key_Tab" || ((Object) transform).name == "Key_Enter" || ((Object) transform).name == "Key_Symbols")
        ((Component) transform).gameObject.SetActive(flag1);
      ((Component) transform.Find("Label")).GetComponent<TextMesh>().text = flag1 ? this.state.DiscordKeyText(((Object) transform).name.Substring(4)) : ((Object) transform).name.Substring(4);
    }
    ((Component) this.root.Find("ButtonSearch")).gameObject.SetActive(!flag1 && !colorPickerOpen);
    ((Component) this.root.Find("ButtonHome")).gameObject.SetActive(!flag2);
    ((Component) this.root.Find("ButtonHome/Label")).GetComponent<TextMesh>().text = colorPickerOpen ? (this.state.EditingEnd ? "Done" : "Next") : "Home";
    TextMesh component = ((Component) this.root.Find("ButtonSearch/Label")).GetComponent<TextMesh>();
    component.text = flag1 ? "Keyboard" : (this.state.Query.Length == 0 ? (this.state.KeyboardOpen ? "Type to search_" : "Search") : this.state.Query + (this.state.KeyboardOpen ? "_" : ""));
    component.color = this.state.KeyboardOpen ? MenuStyle.Text : MenuStyle.Muted;
    bool flag7 = !flag1 && !this.state.KeyboardOpen && this.state.Query.Length == 0;
    ((Component) component).transform.localPosition = new Vector3(3f / 500f, flag7 ? -11f / 1000f : 0.0f, 0.0f);
    Transform transform3 = this.root.Find("ButtonSearch/Icon");
    ((Component) transform3).gameObject.SetActive(flag7);
    Bounds localBounds = ((Component) component).GetComponent<Renderer>().localBounds;
    transform3.localPosition = new Vector3(3f / 500f, (float) ((double) (localBounds).size.x / 2.0 + 0.0040000001899898052), 0.0f);
    string str3;
    if (!colorPickerOpen)
    {
      if (!flag1)
      {
        if (!this.state.KeyboardOpen)
        {
          if (this.state.ResultCount != 0)
            str3 = $"{(this.state.Query.Length == 0 ? MenuCatalog.Categories[this.state.Category] : "Search")}  {(this.state.Page + 1).ToString()} / {this.state.PageCount.ToString()}";
          else
            str3 = this.state.Query.Length != 0 || this.state.Category != 1 ? (this.state.Query.Length != 0 || this.state.Category != 2 ? (this.state.Query.Length != 0 || this.state.Category != 15 && this.state.Category != 17 && this.state.Category != 19 ? "No results" : "No mods yet") : "No favorites") : "No enabled mods";
        }
        else
          str3 = this.state.ResultCount.ToString() + " results";
      }
      else
        str3 = "";
    }
    else
      str3 = this.state.EditingEnd ? "Pick right color, then Done" : "Pick left color, then Next";
    this.footerText = str3;
    ((Collider) ((Component) this.root.Find("ButtonPrevious")).GetComponent<BoxCollider>()).enabled = flag1 || (!this.state.KeyboardOpen || this.state.Design == 9) && this.state.Page > 0;
    ((Collider) ((Component) this.root.Find("ButtonNext")).GetComponent<BoxCollider>()).enabled = flag1 || (!this.state.KeyboardOpen || this.state.Design == 9) && this.state.Page < this.state.PageCount - 1;
    ((Component) this.root.Find("ButtonPrevious/Label")).GetComponent<TextMesh>().text = flag1 ? "BACK" : "<";
    ((Component) this.root.Find("ButtonNext/Label")).GetComponent<TextMesh>().text = flag2 ? "OK" : ">";
    this.design.FormatRows(this.state);
    foreach (BoxCollider button in this.buttons)
      this.Paint(button);
    GradientPicker.Refresh(this.root, this.state);
    this.Animate(Time.unscaledTime);
    this.design.RefreshImported(this.state);
    MenuFonts.Apply(this.root, this.state.FontIndex, this.state.Design == 0);
    this.LayoutSearch();
  }
  private void LayoutSearch()
  {
    if (this.state.Design != 0 || this.state.KeyboardOpen || this.state.Query.Length != 0)
      return;
    Transform parent = this.root.Find("ButtonSearch");
    Transform transform1 = parent.Find("Label");
    Transform transform2 = parent.Find("Icon");
    transform1.localPosition = new Vector3(3f / 500f, 0.0f, 0.0f);
    transform2.localPosition = new Vector3(3f / 500f, 0.0f, 0.0f);
    Bounds bounds1 = MenuView.BoundsInParent(((Component) transform1).GetComponent<Renderer>(), parent);
    Bounds bounds2 = MenuView.BoundsInParent(((Component) transform2).GetComponent<Renderer>(), parent);
    float num1 = (bounds1).max.y + 0.004f - (bounds2).min.y;
    float num2 = (float) (((double) (bounds1).min.y + (double) (bounds2).max.y + (double) num1) * 0.5);
    transform1.localPosition = (transform1.localPosition - (Vector3.up * num2));
    Transform transform3 = transform2;
    transform3.localPosition = (transform3.localPosition + new Vector3(0.0f, num1 - num2, -(bounds2).center.z));
  }
  private static Bounds BoundsInParent(Renderer renderer, Transform parent)
  {
    Bounds localBounds = renderer.localBounds;
    Bounds bounds;
    bounds = new Bounds(parent.InverseTransformPoint(((Component) renderer).transform.TransformPoint((localBounds).center)), Vector3.zero);
    for (int index = 0; index < 8; ++index)
      (bounds).Encapsulate(parent.InverseTransformPoint(((Component) renderer).transform.TransformPoint(((localBounds).center + Vector3.Scale((localBounds).extents, new Vector3((index & 1) == 0 ? -1f : 1f, (index & 2) == 0 ? -1f : 1f, (index & 4) == 0 ? -1f : 1f))))));
    return bounds;
  }
  public void SetFavoriteMode(bool value)
  {
    if (this.favoriteMode == value)
      return;
    this.favoriteMode = value;
    this.Refresh();
  }
  public void SetHover(BoxCollider button)
  {
    BoxCollider hovered = this.hovered;
    this.hovered = button;
    if (((Object) hovered != (Object) null))
      this.Paint(hovered);
    if (!((Object) button != (Object) null))
      return;
    this.Paint(button);
  }
  private static string SettingLabel(string name)
  {
    switch (name)
    {
      case "VR size +":
        return "VR size";
      case "Fly Speed Up":
        return "Fly speed";
      case "Theme Black Red":
        return "Theme";
      default:
        return name;
    }
  }
  private static string ThemeName(string theme)
  {
    if (theme != null)
    {
      switch (theme.Length)
      {
        case 3:
          if (theme == "Ice")
            return "Glacier";
          break;
        case 4:
          switch (theme[3])
          {
            case 'e':
              if (theme == "Fire")
                return "Solar flare";
              break;
            case 'o':
              if (theme == "Mono")
                return "Moonstone";
              break;
            case 't':
              if (theme == "Mint")
                return "Sea glass";
              break;
          }
          break;
        case 5:
          if (theme == "Ocean")
            return "Deep current";
          break;
        case 6:
          switch (theme[0])
          {
            case 'A':
              if (theme == "Aurora")
                return "Northern lights";
              break;
            case 'S':
              if (theme == "Sunset")
                return "Sunset boulevard";
              break;
          }
          break;
        case 7:
          if (theme == "Rainbow")
            return "Prism rainbow";
          break;
        case 8:
          switch (theme[0])
          {
            case 'B':
              if (theme == "BlackRed")
                return "Crimson noir";
              break;
            case 'D':
              if (theme == "DarkGrey")
                return "Graphite";
              break;
            case 'R':
              if (theme == "RoseGold")
                return "Rose quartz";
              break;
          }
          break;
        case 10:
          switch (theme[0])
          {
            case 'P':
              if (theme == "PurpleHaze")
                return "Violet haze";
              break;
            case 'T':
              if (theme == "ToxicGreen")
                return "Radioactive";
              break;
          }
          break;
        case 12:
          if (theme == "MidnightBlue")
            return "Midnight sky";
          break;
      }
    }
    return !(theme == "ItzGame") ? theme : "Taurus";
  }
  private void Paint(BoxCollider button)
  {
    if (((Object) button).name == "Wheel" || ((Object) button).name == "Brightness")
      return;
    this.colors.Clear();
    Color color = ((Object) button).name == "ButtonDisconnect" ? MenuPalette.Background(this.state, Time.unscaledTime) : MenuPalette.Surface(this.state);
    int result;
    if (((Object) button).name.StartsWith("Row") && int.TryParse(((Object) button).name.Substring(3), out result) && this.state.IsOn(this.state.EntryAt(result)))
      color = this.state.Design == 0 ? MenuStyle.ButtonOn : Color.Lerp(color, Color.white, 0.17f);
    if (!((Collider) button).enabled)
      color = Color.Lerp(MenuPalette.Background(this.state, 0.0f), color, 0.6f);
    else if (((Object) button == (Object) this.hovered))
      color = Color.Lerp(color, Color.white, 0.12f);
    this.colors.SetColor("_Color", color);
    ((Component) button).GetComponent<Renderer>().SetPropertyBlock(this.colors);
    if (!(((Object) button).name == "ButtonPrevious") && !(((Object) button).name == "ButtonNext"))
      return;
    ((Component) button).GetComponentInChildren<TextMesh>().color = ((Collider) button).enabled ? MenuStyle.Text : (MenuStyle.Muted * 0.55f);
  }
  public void Animate(float time)
  {
    if (this.state.Design == 0)
      MenuFonts.Animate(this.root, time);
    this.notificationPanel.SetActive(this.notificationActive);
    if (!this.notificationActive)
    {
      this.footer.text = this.state.Category != 11 || !this.state.DiscordAccepted || this.state.KeyboardOpen ? this.footerText : this.state.DiscordStatus;
      this.footer.color = MenuStyle.Muted;
      this.footer.characterSize = 0.0014f;
    }
    this.colors.Clear();
    this.colors.SetColor("_Color", MenuPalette.Background(this.state, time));
    this.background.SetPropertyBlock(this.colors);
    if (((Object) this.hovered != (Object) null) && ((Object) this.hovered).name == "ButtonDisconnect")
      this.colors.SetColor("_Color", Color.Lerp(MenuPalette.Background(this.state, time), Color.white, 0.12f));
    this.disconnect.SetPropertyBlock(this.colors);
    Color color1 = MenuPalette.Left(this.state, time);
    Color color2 = MenuPalette.Right(this.state, time);
    foreach (Renderer gradient in this.gradients)
    {
      gradient.GetPropertyBlock(this.colors);
      this.colors.SetColor("_LeftColor", color1);
      this.colors.SetColor("_RightColor", color2);
      this.colors.SetFloat("_Speed", this.state.IsOn("Freeze gradient") ? 0.0f : 6.28318548f / this.state.GradientPeriod);
      gradient.SetPropertyBlock(this.colors);
    }
    this.finished.Clear();
    float num1 = Mathf.Max(0.0f, time - this.hoverTime);
    this.hoverTime = time;
    foreach (BoxCollider button in this.buttons)
    {
      if (!this.presses.ContainsKey(((Component) button).transform))
      {
        float num2;
        double num3 = this.hoverAmounts.TryGetValue(((Component) button).transform, out num2) ? (double) num2 : 0.0;
        float num4 = Mathf.MoveTowards((float) num3, !((Object) button == (Object) this.hovered) || this.interactive && !((Collider) button).enabled || !((Component) button).gameObject.activeInHierarchy ? 0.0f : 1f, num1 / 0.12f);
        if (!Mathf.Approximately((float) num3, num4))
        {
          float x = ((Component) button).transform.localScale.x;
          float num5 = (float) (1.0 + 0.019999999552965164 * (double) Mathf.SmoothStep(0.0f, 1f, num4));
          BoxCollider boxCollider = button;
          boxCollider.size = (boxCollider.size * x / num5);
          ((Component) button).transform.localScale = (Vector3.one * num5);
          this.hoverAmounts[((Component) button).transform] = num4;
        }
      }
    }
    foreach (KeyValuePair<Transform, float> press in this.presses)
    {
      float elapsed = time - press.Value;
      press.Key.localScale = (Vector3.one * MenuStyle.PressScale(elapsed));
      ((Component) press.Key).GetComponent<BoxCollider>().size = (this.pressSizes[press.Key] / press.Key.localScale.x);
      if ((double) elapsed >= 0.20000000298023224)
        this.finished.Add(press.Key);
    }
    foreach (Transform key in this.finished)
    {
      this.presses.Remove(key);
      this.pressSizes.Remove(key);
    }
    this.design.Animate(this.state, time);
  }
  public void ShowNotification(string message, bool error = false)
  {
    if (string.IsNullOrEmpty(message))
      return;
    message = MenuState.DisplayText(message);
    this.notificationActive = true;
    this.notificationCategory = this.state.Category;
    this.notificationPanel.SetActive(true);
    this.footer.text = message;
    this.footer.color = error ? MenuStyle.Error : MenuStyle.Mint;
    this.footer.characterSize = 0.0014f * Mathf.Min(1f, 32f / (float) message.Length);
    this.design.RefreshImported(this.state);
  }
  public void Pulse(string name)
  {
    foreach (BoxCollider button in this.buttons)
    {
      if (((Object) button).name == name && (!this.interactive || ((Collider) button).enabled) && ((Component) button).gameObject.activeInHierarchy)
      {
        this.presses[((Component) button).transform] = Time.unscaledTime;
        if (!this.pressSizes.ContainsKey(((Component) button).transform))
          this.pressSizes.Add(((Component) button).transform, button.size);
        ((Component) button).transform.localScale = (Vector3.one * MenuStyle.PressScale(0.0f));
        button.size = (this.pressSizes[((Component) button).transform] / ((Component) button).transform.localScale.x);
        break;
      }
    }
  }
  private static string Wrap(string name, int limit)
  {
    if (name.Length <= limit)
      return name;
    int length = name.LastIndexOf(' ', Math.Min(limit, name.Length - 1));
    return length <= 0 ? name : $"{name.Substring(0, length)}\n{name.Substring(length + 1)}";
  }
  public void Dispose()
  {
    foreach (Transform key in this.presses.Keys)
    {
      if (((Object) key != (Object) null))
      {
        key.localScale = Vector3.one;
        ((Component) key).GetComponent<BoxCollider>().size = this.pressSizes[key];
      }
    }
    this.presses.Clear();
    this.pressSizes.Clear();
    this.design.Dispose();
  }
}
