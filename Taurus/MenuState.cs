
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Taurus;
public sealed class MenuState
{
  public static readonly string[] Entries = Array.ConvertAll<MenuEntry, string>(MenuCatalog.Buttons, (Converter<MenuEntry, string>) (entry => entry.Name));
  public const int PageSize = 5;
  public const int MaxRows = 12;
  private string[] soundNames = new string[0];
  private bool roomConnected;
  private bool roomPublic;
  private readonly HashSet<int> enabled = new HashSet<int>();
  private readonly HashSet<int> favorites = new HashSet<int>();
  private readonly List<int> matches = new List<int>();
  private readonly Dictionary<string, Action<MenuEntry>> handlers = new Dictionary<string, Action<MenuEntry>>((IEqualityComparer<string>) StringComparer.Ordinal);
  private readonly Dictionary<string, string> testStatuses = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.Ordinal);
  private readonly HashSet<string> remotelyDisabled = new HashSet<string>((IEqualityComparer<string>) StringComparer.Ordinal);
  private bool rexonSearch;
  private int rexonCategory;
  private int rexonPage;
  public int VisibleRows
  {
    get
    {
      if (this.Design == 11)
        return 8;
      if (this.Design == 8)
        return 4;
      if (this.Design != 6)
      {
        if (this.Design == 1 || this.Design == 7)
          return 7;
        if (this.Design == 2 || this.Design == 5 || this.Design == 9 || this.Design == 10 || this.Design == 12 || this.Design == 13)
          return 6;
        if (this.Design == 3)
          return 12;
        return this.Design != 4 ? 5 : 9;
      }
      return this.Category != 0 && this.Query.Length <= 0 && !this.KeyboardOpen ? 6 : 5;
    }
  }
  public int PastCategoryPage { get; private set; }
  public int PastCategoryAt(int slot)
  {
    int num = this.PastCategoryPage * 6 + slot;
    if (num >= 11)
      ++num;
    return num >= MenuCatalog.Categories.Length ? -1 : num;
  }
  public void MovePastCategories(int direction)
  {
    this.PastCategoryPage = Math.Max(0, Math.Min(this.PastCategoryPage + direction, (MenuCatalog.Categories.Length - 2) / 6));
  }
  public int Page { get; private set; }
  public string Query { get; private set; } = "";
  public bool KeyboardOpen { get; private set; }
  public bool DiscordAccepted { get; private set; }
  public int DiscordKeyboardMode { get; private set; }
  public string DiscordStatus { get; set; } = "Loading Discord...";
  public int Category { get; private set; }
  public bool RightHanded { get; set; }
  public bool Rounded { get; set; } = true;
  public string Theme { get; set; } = "ItzGame";
  public float FlySpeed { get; set; } = 12f;
  public float VrScale { get; set; } = 0.65f;
  public float SoundVolume { get; private set; } = 0.5f;
  public float MicVolume { get; private set; } = 0.65f;
  public float PcScale { get; private set; } = 1.3f;
  public float ClickVolume { get; private set; } = 0.7f;
  public int ClickSound { get; private set; }
  public int Design { get; private set; }
  public int FontIndex { get; private set; }
  public float WhooshVolume { get; private set; } = 0.2f;
  public float GradientPeriod { get; private set; } = 20f;
  public float AntiReportDistance { get; private set; } = 15f;
  public uint GradientStart { get; private set; } = 16730955;
  public uint GradientEnd { get; private set; } = 16751423;
  public bool ColorPickerOpen { get; private set; }
  public bool EditingEnd { get; private set; }
  public float PickerHue { get; private set; }
  public float PickerSaturation { get; private set; }
  public float PickerValue { get; private set; }
  public bool BrowsingSounds { get; private set; }
  public string PlayingSound { get; set; } = "";
  public string Notice { get; private set; } = "";
  public bool PrivateModdedRoom { get; private set; }
  public bool InRoom => this.roomConnected;
  public bool IsMasterClient { get; private set; }
  public int ResultCount => this.matches.Count;
  public int PageCount
  {
    get => Math.Max(1, (this.matches.Count + this.VisibleRows - 1) / this.VisibleRows);
  }
  public bool HasActiveGun
  {
    get
    {
      foreach (int index in this.enabled)
      {
        if (MenuCatalog.Buttons[index].Name == "Gun pointer" || MenuCatalog.Buttons[index].Name.Contains(" Gun"))
          return true;
      }
      return false;
    }
  }
  public MenuState()
  {
    foreach (MenuEntry button in MenuCatalog.Buttons)
    {
      if (button.Target < 0 && (button.Group == 9 || button.Group == 23))
        this.RegisterHandler(button.Name, (Action<MenuEntry>) (action => { }));
    }
    this.RegisterHandler("Local volume", (Action<MenuEntry>) (entry => this.SoundVolume = Math.Min(1f, (float) Math.Round((double) this.SoundVolume + 0.05000000074505806, 2))));
    this.RegisterHandler("Local volume -", (Action<MenuEntry>) (entry => this.SoundVolume = Math.Max(0.0f, (float) Math.Round((double) this.SoundVolume - 0.05000000074505806, 2))));
    this.RegisterHandler("Mic volume", (Action<MenuEntry>) (entry => this.MicVolume = Math.Min(1f, (float) Math.Round((double) this.MicVolume + 0.05000000074505806, 2))));
    this.RegisterHandler("Mic volume -", (Action<MenuEntry>) (entry => this.MicVolume = Math.Max(0.0f, (float) Math.Round((double) this.MicVolume - 0.05000000074505806, 2))));
    this.RegisterHandler("PC size", (Action<MenuEntry>) (entry => this.PcScale = Math.Min(1.8f, (float) Math.Round((double) this.PcScale + 0.05000000074505806, 2))));
    this.RegisterHandler("PC size -", (Action<MenuEntry>) (entry => this.PcScale = Math.Max(0.8f, (float) Math.Round((double) this.PcScale - 0.05000000074505806, 2))));
    this.RegisterHandler("Reset PC size", (Action<MenuEntry>) (entry => this.PcScale = 1.3f));
    this.RegisterHandler("Click volume", (Action<MenuEntry>) (entry => this.ClickVolume = Math.Min(1f, (float) Math.Round((double) this.ClickVolume + 0.05000000074505806, 2))));
    this.RegisterHandler("Click volume -", (Action<MenuEntry>) (entry => this.ClickVolume = Math.Max(0.0f, (float) Math.Round((double) this.ClickVolume - 0.05000000074505806, 2))));
    this.RegisterHandler("Click sound", (Action<MenuEntry>) (entry => this.ClickSound = (this.ClickSound + 1) % MenuClickSounds.Names.Length));
    this.RegisterHandler("Click sound -", (Action<MenuEntry>) (entry => this.ClickSound = (this.ClickSound + MenuClickSounds.Names.Length - 1) % MenuClickSounds.Names.Length));
    for (int index = 0; index < MenuDesign.Names.Length; ++index)
    {
      int selected = index;
      string name = "Use " + MenuDesign.Names[index];
      if (MenuCatalog.Find(name) >= 0)
        this.RegisterHandler(name, (Action<MenuEntry>) (entry => this.SelectDesign(selected)));
    }
    this.RegisterHandler(nameof (Design), (Action<MenuEntry>) (entry => this.MoveDesign(1)));
    this.RegisterHandler("Design -", (Action<MenuEntry>) (entry => this.MoveDesign(-1)));
    this.RegisterHandler("Font", (Action<MenuEntry>) (entry => this.FontIndex = (this.FontIndex + 1) % MenuFonts.Names.Length));
    this.RegisterHandler("Font -", (Action<MenuEntry>) (entry => this.FontIndex = (this.FontIndex + MenuFonts.Names.Length - 1) % MenuFonts.Names.Length));
    this.RegisterHandler("Whoosh volume", (Action<MenuEntry>) (entry => this.WhooshVolume = Math.Min(1f, (float) Math.Round((double) this.WhooshVolume + 0.05000000074505806, 2))));
    this.RegisterHandler("Whoosh volume -", (Action<MenuEntry>) (entry => this.WhooshVolume = Math.Max(0.0f, (float) Math.Round((double) this.WhooshVolume - 0.05000000074505806, 2))));
    this.RegisterHandler("Start color", (Action<MenuEntry>) (entry => this.OpenColorPicker(false)));
    this.RegisterHandler("End color", (Action<MenuEntry>) (entry => this.OpenColorPicker(true)));
    this.RegisterHandler("Custom color", (Action<MenuEntry>) (entry => this.OpenColorPicker(false)));
    this.RegisterHandler("Swap gradient", (Action<MenuEntry>) (entry =>
    {
      this.CopyTheme();
      uint gradientStart = this.GradientStart;
      this.GradientStart = this.GradientEnd;
      this.GradientEnd = gradientStart;
    }));
    this.RegisterHandler("Reset gradient", (Action<MenuEntry>) (entry =>
    {
      this.GradientStart = 16730955U;
      this.GradientEnd = 16751423U;
      this.GradientPeriod = 20f;
      this.Theme = "ItzGame";
      this.SetEnabled("Freeze gradient", false);
    }));
    this.RegisterHandler("Gradient speed", (Action<MenuEntry>) (entry => this.GradientPeriod = Math.Max(5f, this.GradientPeriod - 5f)));
    this.RegisterHandler("Gradient speed -", (Action<MenuEntry>) (entry => this.GradientPeriod = Math.Min(60f, this.GradientPeriod + 5f)));
    this.RegisterHandler("Freeze gradient", (Action<MenuEntry>) (entry => { }));
    this.RegisterHandler("Antiban Check", (Action<MenuEntry>) (entry => this.Notice = this.PrivateModdedRoom ? "Antiban active" : (!this.roomConnected ? "needs antiban: No room joined" : (this.roomPublic ? "needs antiban: Public room" : "needs antiban"))));
    this.RegisterHandler("Anti Report Distance", (Action<MenuEntry>) (entry =>
    {
      ++this.AntiReportDistance;
      if ((double) this.AntiReportDistance > 30.0)
        this.AntiReportDistance = 5f;
      this.Notice = $"Anti Report Distance: {this.AntiReportDistance.ToString("F0")}m";
    }));
    this.RegisterHandler("Experimental Anti Player Reports", (Action<MenuEntry>) (entry => this.Notice = this.IsOn(entry.Name) ? "Now blocks all reports." : "Report blocking off."));
    this.RegisterHandler("Menu Hand", (Action<MenuEntry>) (entry => this.RightHanded = !this.RightHanded));
    this.RegisterHandler("Rounded Corners", (Action<MenuEntry>) (entry => this.Rounded = this.IsOn(entry.Name)));
    this.RegisterHandler("Fly Speed Up", (Action<MenuEntry>) (entry => this.FlySpeed = Math.Min(100f, this.FlySpeed + 2f)));
    this.RegisterHandler("Fly Speed Down", (Action<MenuEntry>) (entry => this.FlySpeed = Math.Max(2f, this.FlySpeed - 2f)));
    this.RegisterHandler("Reset theme", (Action<MenuEntry>) (entry =>
    {
      this.Theme = "ItzGame";
      this.GradientPeriod = 20f;
      this.SetEnabled("Freeze gradient", false);
    }));
    this.RegisterHandler("VR size +", (Action<MenuEntry>) (entry => this.VrScale = Math.Min(1.4f, (float) Math.Round((double) this.VrScale * 100.0 + 5.0) / 100f)));
    this.RegisterHandler("VR size -", (Action<MenuEntry>) (entry => this.VrScale = Math.Max(0.35f, (float) Math.Round((double) this.VrScale * 100.0 - 5.0) / 100f)));
    this.RegisterHandler("Reset VR size", (Action<MenuEntry>) (entry => this.VrScale = 0.65f));
    string[] strArray = new string[6]
    {
      "Theme Black Red",
      "Theme Dark Grey",
      "Theme Midnight Blue",
      "Theme Toxic Green",
      "Theme Purple Haze",
      "Theme Rainbow"
    };
    foreach (string name in strArray)
      this.RegisterHandler(name, (Action<MenuEntry>) (entry => this.Theme = entry.Name.Substring(6).Replace(" ", "")));
    this.SetEnabled("Rounded Corners", true);
    this.Filter();
  }
  public bool IsOn(string name) => this.enabled.Contains(MenuCatalog.Find(name));
  public bool IsOn(int index) => this.enabled.Contains(index);
  public bool IsFavorite(int index) => this.favorites.Contains(index);
  public static bool CanFavorite(MenuEntry entry) => MenuCatalog.CanFavorite(entry);
  public bool ToggleFavorite(int index)
  {
    if (index < 0 || index >= MenuCatalog.Buttons.Length || !MenuState.CanFavorite(MenuCatalog.Buttons[index]))
      return false;
    bool flag = this.favorites.Add(index);
    if (!flag)
      this.favorites.Remove(index);
    int page = this.Page;
    this.Filter();
    this.Page = Math.Min(page, this.PageCount - 1);
    this.Notice = MenuCatalog.Buttons[index].Name + (flag ? " favorited" : " unfavorited");
    return true;
  }
  public static bool IsIncremental(MenuEntry entry)
  {
    if (entry == null)
      return false;
    return entry.Name == "VR size +" || entry.Name == "Fly Speed Up" || entry.Name == "Theme Black Red" || entry.Name == "Local volume" || entry.Name == "Mic volume" || entry.Name == "PC size" || entry.Name == "Click volume" || entry.Name == "Whoosh volume" || entry.Name == "Gradient speed" || entry.Name == "Click sound" || entry.Name == "Design" || entry.Name == "Font";
  }
  public bool CanAdjust(MenuEntry entry, int direction)
  {
    if (direction == 0 || !MenuState.IsIncremental(entry) || !this.ShowsAvailable(entry))
      return false;
    string name = entry.Name;
    if (name != null)
    {
      switch (name.Length)
      {
        case 7:
          if (name == "PC size")
            return direction >= 0 ? (double) this.PcScale < 1.7999999523162842 : (double) this.PcScale > 0.800000011920929;
          break;
        case 9:
          if (name == "VR size +")
            return direction >= 0 ? (double) this.VrScale < 1.3999999761581421 : (double) this.VrScale > 0.34999999403953552;
          break;
        case 10:
          if (name == "Mic volume")
            return direction >= 0 ? (double) this.MicVolume < 1.0 : (double) this.MicVolume > 0.0;
          break;
        case 12:
          switch (name[0])
          {
            case 'C':
              if (name == "Click volume")
                return direction >= 0 ? (double) this.ClickVolume < 1.0 : (double) this.ClickVolume > 0.0;
              break;
            case 'F':
              if (name == "Fly Speed Up")
                return direction >= 0 ? (double) this.FlySpeed < 100.0 : (double) this.FlySpeed > 2.0;
              break;
            case 'L':
              if (name == "Local volume")
                return direction >= 0 ? (double) this.SoundVolume < 1.0 : (double) this.SoundVolume > 0.0;
              break;
          }
          break;
        case 13:
          if (name == "Whoosh volume")
            return direction >= 0 ? (double) this.WhooshVolume < 1.0 : (double) this.WhooshVolume > 0.0;
          break;
        case 14:
          if (name == "Gradient speed")
            return direction >= 0 ? (double) this.GradientPeriod > 5.0 : (double) this.GradientPeriod < 60.0;
          break;
      }
    }
    return true;
  }
  public MenuEntry Adjust(int row, int direction)
  {
    int index = this.EntryAt(row);
    if (index < 0 || !this.CanAdjust(MenuCatalog.Buttons[index], direction))
      return (MenuEntry) null;
    MenuEntry button = MenuCatalog.Buttons[index];
    if (button.Name == "Theme Black Red")
    {
      int num = Math.Max(0, Array.IndexOf<string>(MenuPacket.Themes, this.Theme));
      this.Theme = MenuPacket.Themes[(num + (direction < 0 ? -1 : 1) + MenuPacket.Themes.Length) % MenuPacket.Themes.Length];
      if (this.Theme == "Custom")
        this.OpenColorPicker(false);
      else
        this.CloseColorPicker();
      this.Notice = "";
      return button;
    }
    string name = button.Name == "VR size +" ? (direction < 0 ? "VR size -" : "VR size +") : (button.Name == "Fly Speed Up" ? (direction < 0 ? "Fly Speed Down" : "Fly Speed Up") : button.Name);
    if ((button.Name == "Local volume" || button.Name == "Mic volume" || button.Name == "PC size" || button.Name == "Click volume" || button.Name == "Whoosh volume" || button.Name == "Gradient speed" || button.Name == "Click sound" || button.Name == "Design" || button.Name == "Font") && direction < 0)
      name += " -";
    return this.Activate(MenuCatalog.Find(name));
  }
  public void ApplyRemote(MenuPacket packet)
  {
    this.enabled.Clear();
    this.favorites.Clear();
    this.handlers.Clear();
    this.Theme = MenuPacket.Themes[(int) packet.Theme];
    this.ClickSound = (int) packet.ClickSound;
    this.ClickVolume = packet.ClickVolume;
    this.Design = (int) packet.Design;
    this.FontIndex = (int) packet.Font;
    this.roomConnected = packet.InRoom;
    this.RightHanded = packet.RightHanded;
    this.Rounded = packet.Rounded;
    this.FlySpeed = packet.FlySpeed;
    this.VrScale = packet.VrScale;
    this.GradientStart = packet.GradientStart;
    this.GradientEnd = packet.GradientEnd;
    this.GradientPeriod = packet.GradientPeriod;
    this.PrivateModdedRoom = packet.PrivateModded;
    this.IsMasterClient = packet.Master;
    for (int index = 0; index < MenuCatalog.Buttons.Length; ++index)
    {
      if (MenuPacket.Bit(packet.Available, index) && MenuCatalog.Buttons[index].Target < 0)
        this.handlers[MenuCatalog.Buttons[index].Name] = (Action<MenuEntry>) (entry => { });
      if (MenuPacket.Bit(packet.Enabled, index))
        this.enabled.Add(index);
      if (MenuPacket.Bit(packet.Favorites, index))
        this.favorites.Add(index);
    }
    this.SelectCategory((int) packet.Category);
    this.Page = Math.Min((int) packet.Page, this.PageCount - 1);
    this.ColorPickerOpen = packet.ColorPickerOpen;
    this.EditingEnd = packet.EditingEnd;
    if (!this.ColorPickerOpen)
      return;
    float num1;
    float num2;
    float num3;
    Color.RGBToHSV(MenuPalette.Unpack(this.EditingEnd ? this.GradientEnd : this.GradientStart), out num1, out num2, out num3);
    this.PickerHue = num1;
    this.PickerSaturation = num2;
    this.PickerValue = num3;
  }
  public void RegisterHandler(string name, Action<MenuEntry> handler)
  {
    int index = MenuCatalog.Find(name);
    if (index < 0 || MenuCatalog.Buttons[index].Target >= 0)
      throw new ArgumentException("Unknown mod name: " + name, nameof (name));
    Dictionary<string, Action<MenuEntry>> handlers = this.handlers;
    string key = name;
    handlers[key] = handler ?? throw new ArgumentNullException(nameof (handler));
  }
  public bool HasCode(MenuEntry entry)
  {
    if (entry == null)
      return false;
    return entry.Target >= 0 || this.handlers.ContainsKey(entry.Name);
  }
  public static bool RequiresMaster(MenuEntry entry)
  {
    return entry.Target < 0 && entry.Name.EndsWith("(M)", StringComparison.Ordinal);
  }
  public bool ShowsAvailable(MenuEntry entry)
  {
    if (!this.HasCode(entry) || this.remotelyDisabled.Contains(entry.Name) || MenuState.RequiresMaster(entry) && !this.IsMasterClient)
      return false;
    return !entry.RequiresAntiban || this.PrivateModdedRoom;
  }
  public static bool IsFeature(MenuEntry entry)
  {
    return entry != null && entry.Target < 0 && entry.Group != 3 && entry.Group != 14 && entry.Group != 22 && entry.Group != 9 && entry.Group != 23 && entry.Group != 10;
  }
  public void SetTestStatus(string name, string status)
  {
    int index = MenuCatalog.Find(name);
    if (index < 0 || !MenuState.IsFeature(MenuCatalog.Buttons[index]) || status != "W" && status != "W?" && status != "NW")
      throw new ArgumentException("Invalid feature status: " + name);
    this.testStatuses[name] = status;
  }
  public string FeatureLabel(MenuEntry entry)
  {
    string str;
    return MenuState.DisplayText(entry.Name) + (!MenuState.IsFeature(entry) || !entry.RequiresAntiban ? "" : $" ({(!this.HasCode(entry) ? "NW" : (this.testStatuses.TryGetValue(entry.Name, out str) ? str : "W?"))})");
  }
  public void ApplyDisabledFeatures(IEnumerable<string> names)
  {
    this.remotelyDisabled.Clear();
    foreach (string name in names)
    {
      int index = MenuCatalog.Find(name);
      if (index >= 0 && MenuState.IsFeature(MenuCatalog.Buttons[index]))
        this.remotelyDisabled.Add(name);
    }
    foreach (string str in this.remotelyDisabled)
    {
      if (this.IsOn(str))
      {
        MenuEntry button = MenuCatalog.Buttons[MenuCatalog.Find(str)];
        this.SetEnabled(str, false);
        Action<MenuEntry> action;
        if (this.handlers.TryGetValue(str, out action))
        {
          try
          {
            action(button);
          }
          catch (Exception ex)
          {
            this.Notice = $"Cleanup failed: {str}: {ex.Message}";
          }
        }
      }
    }
  }
  public static string DisplayText(string text)
  {
    return text.Replace("Modded", "Antiban").Replace("modded", "antiban").Replace("MODDED", "ANTIBAN");
  }
  public string AvailabilityLabel(MenuEntry entry)
  {
    if (this.remotelyDisabled.Contains(entry.Name))
      return "Remotely disabled";
    if (MenuState.RequiresMaster(entry) && !this.IsMasterClient)
      return "requires master client";
    if (entry.RequiresAntiban && !this.PrivateModdedRoom)
      return "needs antiban";
    return !this.ShowsAvailable(entry) ? "Unavailable" : "Available";
  }
  public string UnavailableReason(MenuEntry entry)
  {
    string str = this.HasCode(entry) ? "" : "No implementation";
    if (this.remotelyDisabled.Contains(entry.Name))
      str = "Disabled by the Taurus safety policy";
    if (MenuState.RequiresMaster(entry) && !this.IsMasterClient)
      str = $"{str}{(str.Length == 0 ? "" : "; ")}Master client required";
    if (entry.RequiresAntiban && !this.PrivateModdedRoom)
      str = $"{str}{(str.Length == 0 ? "" : "; ")}Antiban required";
    return str;
  }
  public bool UpdateRoom(bool inRoom, bool isPublic, string gameMode, bool isMasterClient = false)
  {
    bool flag1 = inRoom && !isPublic && gameMode != null && gameMode.IndexOf("MODDED_", StringComparison.OrdinalIgnoreCase) >= 0;
    bool flag2 = inRoom & isMasterClient;
    if (this.PrivateModdedRoom == flag1 && this.roomConnected == inRoom && this.roomPublic == isPublic && this.IsMasterClient == flag2)
      return false;
    this.PrivateModdedRoom = flag1;
    this.IsMasterClient = flag2;
    this.roomConnected = inRoom;
    this.roomPublic = isPublic;
    this.Notice = "";
    for (int index = 0; index < MenuCatalog.Buttons.Length; ++index)
    {
      MenuEntry button = MenuCatalog.Buttons[index];
      if (this.enabled.Contains(index) && !this.ShowsAvailable(button))
      {
        this.SetEnabled(button.Name, false);
        Action<MenuEntry> action;
        if (this.handlers.TryGetValue(button.Name, out action))
        {
          try
          {
            action(button);
          }
          catch (Exception ex)
          {
            this.Notice = $"Cleanup failed: {button.Name}: {ex.Message}";
          }
        }
      }
    }
    return true;
  }
  public void SetEnabled(string name, bool value)
  {
    int index = MenuCatalog.Find(name);
    if (index < 0 || !MenuCatalog.Buttons[index].Toggle || value && !this.ShowsAvailable(MenuCatalog.Buttons[index]))
      return;
    if (value)
    {
      if (name == "Low Gravity" || name == "Zero Gravity" || name == "High Gravity")
      {
        string[] strArray = new string[3]
        {
          "Low Gravity",
          "Zero Gravity",
          "High Gravity"
        };
        foreach (string name1 in strArray)
          this.enabled.Remove(MenuCatalog.Find(name1));
      }
      if (name.StartsWith("Alien Stuff Remover"))
      {
        string[] strArray = new string[2]
        {
          "Alien Stuff Remover (No Colliders)",
          "Alien Stuff Remover (With Colliders)"
        };
        foreach (string name2 in strArray)
          this.enabled.Remove(MenuCatalog.Find(name2));
      }
      this.enabled.Add(index);
    }
    else
      this.enabled.Remove(index);
    if (this.Category != 1 || this.Query.Length != 0)
      return;
    int page = this.Page;
    this.Filter();
    this.Page = Math.Min(page, this.PageCount - 1);
  }
  public MenuEntry Activate(int index)
  {
    if (index < 0 || index >= MenuCatalog.Buttons.Length)
      return (MenuEntry) null;
    MenuEntry button = MenuCatalog.Buttons[index];
    this.Notice = "";
    if (button.Target >= 0)
    {
      this.SelectCategory(button.Target);
      return (MenuEntry) null;
    }
    if (!this.ShowsAvailable(button))
    {
      this.Notice = !button.RequiresAntiban || this.PrivateModdedRoom ? $"{this.AvailabilityLabel(button)}: {this.UnavailableReason(button)}" : "needs antiban";
      return (MenuEntry) null;
    }
    if (button.Name == "Theme Black Red" && this.Theme == "Custom")
    {
      this.OpenColorPicker(false);
      return button;
    }
    if (button.Toggle)
      this.SetEnabled(button.Name, !this.IsOn(index));
    try
    {
      this.handlers[button.Name](button);
    }
    catch
    {
      if (button.Toggle)
        this.SetEnabled(button.Name, false);
      throw;
    }
    return button;
  }
  public MenuSaveData Capture()
  {
    List<string> stringList1 = new List<string>();
    List<string> stringList2 = new List<string>();
    for (int index = 0; index < MenuCatalog.Buttons.Length; ++index)
    {
      if (this.enabled.Contains(index))
        stringList1.Add(MenuCatalog.Buttons[index].Name);
    }
    for (int index = 0; index < MenuCatalog.Buttons.Length; ++index)
    {
      if (this.favorites.Contains(index))
        stringList2.Add(MenuCatalog.Buttons[index].Name);
    }
    return new MenuSaveData()
    {
      version = 1,
      enabledMods = stringList1.ToArray(),
      favoriteMods = stringList2.ToArray(),
      theme = this.Theme,
      design = MenuDesign.Names[this.Design],
      font = MenuFonts.Names[this.FontIndex],
      rightHanded = this.RightHanded,
      rounded = this.Rounded,
      flySpeed = this.FlySpeed,
      vrScale = this.VrScale,
      discordAccepted = this.DiscordAccepted,
      soundVolume = this.SoundVolume,
      micVolume = this.MicVolume,
      pcScale = this.PcScale,
      clickVolume = this.ClickVolume,
      clickSound = MenuClickSounds.Names[this.ClickSound],
      whooshVolume = this.WhooshVolume,
      gradientStart = this.GradientStart,
      gradientEnd = this.GradientEnd,
      gradientPeriod = this.GradientPeriod
    };
  }
  public void Restore(MenuSaveData data)
  {
    if (data == null || data.version != 1)
      throw new InvalidDataException("Unsupported menu save version.");
    this.enabled.Clear();
    this.favorites.Clear();
    if (data.favoriteMods != null)
    {
      foreach (string favoriteMod in data.favoriteMods)
      {
        int index = MenuCatalog.Find(favoriteMod);
        if (index >= 0 && MenuState.CanFavorite(MenuCatalog.Buttons[index]))
          this.favorites.Add(index);
      }
    }
    if (data.enabledMods != null)
    {
      foreach (string enabledMod in data.enabledMods)
      {
        if (!string.IsNullOrEmpty(enabledMod))
          this.SetEnabled(enabledMod, true);
      }
    }
    this.Theme = Array.IndexOf<string>(MenuPacket.Themes, data.theme) >= 0 ? data.theme : "ItzGame";
    this.Design = Math.Max(0, Array.IndexOf<string>(MenuDesign.Names, data.design == "Tel Aviv" ? "Tel Aviv Client" : data.design));
    this.FontIndex = Math.Max(0, Array.IndexOf<string>(MenuFonts.Names, data.font));
    this.RightHanded = data.rightHanded;
    this.DiscordAccepted = data.discordAccepted;
    this.Rounded = data.rounded;
    this.SetEnabled("Rounded Corners", this.Rounded);
    this.FlySpeed = float.IsNaN(data.flySpeed) || float.IsInfinity(data.flySpeed) || (double) data.flySpeed < 2.0 ? 12f : Math.Min(100f, data.flySpeed);
    this.VrScale = float.IsNaN(data.vrScale) || float.IsInfinity(data.vrScale) || (double) data.vrScale < 0.34999999403953552 || (double) data.vrScale > 1.3999999761581421 ? 0.65f : data.vrScale;
    this.SoundVolume = float.IsNaN(data.soundVolume) || float.IsInfinity(data.soundVolume) ? 0.5f : Math.Max(0.0f, Math.Min(1f, data.soundVolume));
    this.MicVolume = float.IsNaN(data.micVolume) || float.IsInfinity(data.micVolume) ? 0.65f : Math.Max(0.0f, Math.Min(1f, data.micVolume));
    this.PcScale = MenuState.SafeSetting(data.pcScale, 0.8f, 1.8f, 1.3f);
    this.ClickVolume = MenuState.SafeSetting(data.clickVolume, 0.0f, 1f, 0.7f);
    this.ClickSound = Math.Max(0, Array.IndexOf<string>(MenuClickSounds.Names, data.clickSound));
    this.WhooshVolume = MenuState.SafeSetting(data.whooshVolume, 0.0f, 1f, 0.2f);
    this.GradientStart = data.gradientStart <= 16777215U ? data.gradientStart : 16730955U;
    this.GradientEnd = data.gradientEnd <= 16777215U ? data.gradientEnd : 16751423U;
    this.GradientPeriod = MenuState.SafeSetting(data.gradientPeriod, 5f, 60f, 20f);
    this.Home();
  }
  public void SelectCategory(int category)
  {
    if (category == 11)
      category = 0;
    if (category < 0 || category >= MenuCatalog.Categories.Length)
      return;
    this.Category = category;
    this.BrowsingSounds = false;
    this.ColorPickerOpen = false;
    this.Query = "";
    this.KeyboardOpen = false;
    this.rexonSearch = false;
    this.Filter();
  }
  public int EntryAt(int row)
  {
    int index = this.Page * this.VisibleRows + row;
    return row < 0 || row >= this.VisibleRows || index >= this.matches.Count || this.matches[index] < 0 ? -1 : this.matches[index];
  }
  public string SoundNameAt(int row)
  {
    int index = this.Page * this.VisibleRows + row;
    return this.Category != 12 || !this.BrowsingSounds || row < 0 || row >= this.VisibleRows || index >= this.matches.Count || this.matches[index] >= -1 ? (string) null : this.soundNames[-this.matches[index] - 2];
  }
  public int SoundIndexAt(int row)
  {
    int index = this.Page * this.VisibleRows + row;
    return this.SoundNameAt(row) != null ? -this.matches[index] - 2 : -1;
  }
  public void SetSounds(string[] names)
  {
    this.soundNames = names ?? new string[0];
    if (this.Category != 12 || !this.BrowsingSounds)
      return;
    this.Filter();
  }
  public void BrowseSounds(bool browse)
  {
    this.Category = 12;
    this.BrowsingSounds = browse;
    this.KeyboardOpen = false;
    this.Query = "";
    this.Filter();
  }
  public void MovePage(int direction)
  {
    this.Page = Math.Max(0, Math.Min(this.Page + direction, this.PageCount - 1));
  }
  private void MoveDesign(int direction)
  {
    int num = Array.IndexOf<int>(MenuPacket.DesignOrder, this.Design);
    this.SelectDesign(MenuPacket.DesignOrder[(num + direction + MenuPacket.DesignOrder.Length) % MenuPacket.DesignOrder.Length]);
  }
  private void SelectDesign(int design)
  {
    this.Design = (design + MenuDesign.Names.Length) % MenuDesign.Names.Length;
    if (this.Design >= 10 && this.FontIndex == 0)
      this.FontIndex = 1;
    if (this.Design == 6 || this.Design == 7)
    {
      this.Rounded = this.Design == 7;
      this.SetEnabled("Rounded Corners", this.Rounded);
    }
    this.Page = Math.Min(this.Page, this.PageCount - 1);
    if (this.Design < 10 && this.Category != 0)
      return;
    this.Filter();
  }
  private static float SafeSetting(float value, float min, float max, float fallback)
  {
    return float.IsNaN(value) || float.IsInfinity(value) || (double) value < (double) min || (double) value > (double) max ? fallback : value;
  }
  private void CopyTheme()
  {
    if (this.Theme == "Custom")
      return;
    this.GradientStart = MenuPalette.Pack(MenuPalette.Left(this, 0.0f));
    this.GradientEnd = MenuPalette.Pack(MenuPalette.Right(this, 0.0f));
    this.Theme = "Custom";
  }
  private void OpenColorPicker(bool end)
  {
    if (this.Design == 9)
    {
      this.Notice = "Rexon keeps its original theme";
    }
    else
    {
      this.CopyTheme();
      this.SelectCategory(3);
      this.EditingEnd = end;
      float num1;
      float num2;
      float num3;
      Color.RGBToHSV(MenuPalette.Unpack(end ? this.GradientEnd : this.GradientStart), out num1, out num2, out num3);
      this.PickerHue = num1;
      this.PickerSaturation = num2;
      this.PickerValue = num3;
      this.ColorPickerOpen = true;
    }
  }
  public void SetGradientColor(uint color)
  {
    if (this.EditingEnd)
      this.GradientEnd = color & 16777215U ;
    else
      this.GradientStart = color & 16777215U ;
    this.Theme = "Custom";
  }
  public void CloseColorPicker() => this.ColorPickerOpen = false;
  public void AdvanceColorPicker()
  {
    if (this.EditingEnd)
      this.CloseColorPicker();
    else
      this.OpenColorPicker(true);
  }
  public bool SetGradientHSV(float hue, float saturation, float value)
  {
    if ((double) Math.Abs(this.PickerHue - hue) + (double) Math.Abs(this.PickerSaturation - saturation) + (double) Math.Abs(this.PickerValue - value) < 1.0 / 1000.0)
      return false;
    this.PickerHue = hue;
    this.PickerSaturation = saturation;
    this.PickerValue = value;
    this.SetGradientColor(MenuPalette.Pack(Color.HSVToRGB(hue, saturation, value)));
    return true;
  }
  public void Home()
  {
    this.rexonSearch = false;
    this.Query = "";
    this.KeyboardOpen = false;
    this.Category = 0;
    this.BrowsingSounds = false;
    this.ColorPickerOpen = false;
    this.Filter();
  }
  public void OpenSearch()
  {
    if (this.Design == 9 && !this.KeyboardOpen)
    {
      this.rexonSearch = true;
      this.rexonCategory = this.Category;
      this.rexonPage = this.Page;
      this.Query = "";
    }
    this.KeyboardOpen = true;
    if (!this.rexonSearch)
      return;
    this.Filter();
  }
  public void SetRexonSearchQuery(string query)
  {
    this.Query = query ?? "";
    this.Filter();
  }
  public void AcceptDiscord() => this.DiscordAccepted = true;
  public void ToggleDiscordCase()
  {
    this.DiscordKeyboardMode = this.DiscordKeyboardMode != 1 ? 1 : 0;
  }
  public void ToggleDiscordSymbols()
  {
    this.DiscordKeyboardMode = this.DiscordKeyboardMode == 2 ? 0 : 2;
  }
  public string DiscordKeyText(string key)
  {
    switch (key)
    {
      case "Clear":
        return this.DiscordKeyboardMode != 1 ? "ABC" : "abc";
      case "Symbols":
        return this.DiscordKeyboardMode != 2 ? "@#" : "abc";
      default:
        if (key.Length != 1)
          return key;
        if (this.DiscordKeyboardMode < 2)
          return this.DiscordKeyboardMode != 0 ? key.ToUpperInvariant() : key.ToLowerInvariant();
        int startIndex = "1234567890QWERTYUIOPASDFGHJKLZXCVBNM".IndexOf(key, StringComparison.Ordinal);
        return startIndex >= 0 ? "!@#$%^&*()-_=+[]{}\\|;:'\",.<>?/`~@._-".Substring(startIndex, 1) : key;
    }
  }
  public void Type(string key)
  {
    switch (key)
    {
      case "Done":
        this.KeyboardOpen = false;
        if (!this.rexonSearch)
          return;
        this.rexonSearch = false;
        this.Query = "";
        this.Category = this.rexonCategory;
        this.Filter();
        this.Page = Math.Min(this.rexonPage, this.PageCount - 1);
        return;
      case "Back":
        this.Query = this.Query.Length == 0 ? "" : this.Query.Substring(0, this.Query.Length - 1);
        break;
      case "Clear":
        this.Query = "";
        break;
      case "Space":
        if (this.Design == 9 || this.Query.Length < 24)
        {
          this.Query += " ";
          break;
        }
        break;
      default:
        if (key.Length == 1 && char.IsLetterOrDigit(key[0]) && (this.Design == 9 || this.Query.Length < 24))
        {
          this.Query += this.Design == 9 ? key : key.ToLowerInvariant();
          break;
        }
        break;
    }
    this.Filter();
  }
  private void Filter()
  {
    this.matches.Clear();
    this.Notice = "";
    if (this.Category == 12 && this.BrowsingSounds)
    {
      for (int index = 0; index < this.soundNames.Length; ++index)
      {
        if (this.Query.Length == 0 || this.soundNames[index].IndexOf(this.Query, StringComparison.OrdinalIgnoreCase) >= 0)
          this.matches.Add(-index - 2);
      }
      this.Page = 0;
    }
    else
    {
      for (int index = 0; index < MenuState.Entries.Length; ++index)
      {
        MenuEntry button = MenuCatalog.Buttons[index];
        if (button.Group != 14 && (!(button.Name == "Fly Speed Down") && !(button.Name == "VR size -") && !button.Name.EndsWith("volume -", StringComparison.Ordinal) && !(button.Name == "PC size -") && !(button.Name == "Gradient speed -") && !(button.Name == "Click sound -") && !(button.Name == "Design -") && !(button.Name == "Font -") || this.Category == 2 && this.favorites.Contains(index)) && (this.Query.Length != 0 || this.Category == 2 || !button.Name.StartsWith("Theme ", StringComparison.Ordinal) || !(button.Name != "Theme Black Red")))
        {
          bool flag = this.rexonSearch && this.KeyboardOpen && button.Target < 0 && button.Group != 0 && (this.Query.Length == 0 || MenuCatalog.Categories[button.Group].IndexOf(this.Query, StringComparison.OrdinalIgnoreCase) >= 0 || MenuState.DisplayText(button.Name).IndexOf(this.Query, StringComparison.OrdinalIgnoreCase) >= 0);
          if ((!this.rexonSearch || !this.KeyboardOpen ? (this.Query.Length == 0 ? (this.Category == 1 ? (this.enabled.Contains(index) ? 1 : 0) : (this.Category == 2 ? (this.favorites.Contains(index) ? 1 : 0) : (button.Group == this.Category ? 1 : 0))) : (!(button.Name != "Return to Main") ? 0 : (MenuState.DisplayText(button.Name).IndexOf(this.Query, StringComparison.OrdinalIgnoreCase) >= 0 ? 1 : 0))) : (flag ? 1 : 0)) != 0)
            this.matches.Add(index);
        }
      }
      if (this.Category == 18 && this.Query.Length == 0)
      {
        string[] strArray = new string[8]
        {
          "Crash Gun",
          "Crash All",
          "Kick Gun",
          "Kick All",
          "Freeze Gun",
          "Freeze All",
          "Ghost Gun",
          "Ghost All"
        };
        List<int> collection = new List<int>((IEnumerable<int>) this.matches);
        this.matches.Clear();
        foreach (string name in strArray)
        {
          int num = MenuCatalog.Find(name);
          if (collection.Remove(num))
            this.matches.Add(num);
        }
        this.matches.AddRange((IEnumerable<int>) collection);
      }
      if (this.Category == 22 && this.Query.Length == 0)
      {
        List<int> collection = new List<int>((IEnumerable<int>) this.matches);
        this.matches.Clear();
        foreach (int index in MenuPacket.DesignOrder)
        {
          int num = MenuCatalog.Find("Use " + MenuDesign.Names[index]);
          if (collection.Remove(num))
            this.matches.Add(num);
        }
        this.matches.AddRange((IEnumerable<int>) collection);
      }
      if (this.Design == 13 && this.Category == 0 && this.Query.Length == 0)
      {
        string[] strArray = new string[6]
        {
          "Settings",
          "Movement",
          "Room",
          "Security",
          "Visual",
          "Overpowered"
        };
        List<int> collection = new List<int>((IEnumerable<int>) this.matches);
        this.matches.Clear();
        foreach (string name in strArray)
        {
          int num = MenuCatalog.Find(name);
          if (collection.Remove(num))
            this.matches.Add(num);
        }
        this.matches.AddRange((IEnumerable<int>) collection);
      }
      if (this.Design == 12 && this.Category == 0 && this.Query.Length == 0)
      {
        string[] strArray = new string[6]
        {
          "Settings",
          "Room",
          "Rig Mods",
          "Advantage",
          "Movement",
          "Misc"
        };
        List<int> collection = new List<int>((IEnumerable<int>) this.matches);
        this.matches.Clear();
        foreach (string name in strArray)
        {
          int num = MenuCatalog.Find(name);
          if (collection.Remove(num))
            this.matches.Add(num);
        }
        this.matches.AddRange((IEnumerable<int>) collection);
      }
      this.Page = 0;
    }
  }
}
