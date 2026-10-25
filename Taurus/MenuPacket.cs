
using System;
using System.IO;
namespace Taurus;
public sealed class MenuPacket
{
  public const string Version = "1.4.0";
  public static readonly string[] Themes = new string[16 ]
  {
    "ItzGame",
    "BlackRed",
    "DarkGrey",
    "MidnightBlue",
    "ToxicGreen",
    "PurpleHaze",
    "Rainbow",
    "Sunset",
    "Ocean",
    "Aurora",
    "RoseGold",
    "Ice",
    "Fire",
    "Mint",
    "Mono",
    "Custom"
  };
  public static readonly string[] Designs = new string[14]
  {
    "Taurus",
    "Xyfer",
    "hamburbur",
    "MonoSandbox",
    "ShibaGT Dark",
    "ii's Stupid Menu",
    "Untitled Menu",
    "NXO",
    "Past",
    "Rexon",
    "Kyra",
    "Tel Aviv Client",
    "Fizzo Paid",
    "Parrot.Client"
  };
  public static readonly int[] DesignOrder = new int[14]
  {
    0,
    1,
    10,
    9,
    2,
    12,
    13,
    7,
    6,
    5,
    11,
    4,
    3,
    8
  };
  public static readonly string[] Fonts = new string[9]
  {
    "Minecraft",
    "Original",
    "Gilroy Black",
    "Gogga Thorax",
    "Gorilla Tag Title",
    "Impact",
    "Montserrat Bold",
    "Montserrat Black",
    "Cardboard Cutout"
  };
  public static readonly int MaskSize = (MenuCatalog.Buttons.Length + 7) / 8;
  public static readonly int Length = 91 + MenuPacket.MaskSize * 3;
  public bool Visible;
  public bool RightHanded;
  public bool Rounded;
  public bool PrivateModded;
  public bool Master;
  public bool PrivateInput;
  public bool InRoom;
  public bool ColorPickerOpen;
  public bool EditingEnd;
  public bool PointerVisible;
  public byte Category;
  public byte Theme;
  public byte Press;
  public byte Hover;
  public byte ClickSound;
  public byte Design;
  public byte Font;
  public ushort Page;
  public float FlySpeed;
  public float VrScale;
  public float Scale;
  public uint GradientStart = 16730955;
  public uint GradientEnd = 16751423;
  public float GradientPeriod = 20f;
  public float[] Placement = new float[6];
  public float[] PointerPlacement = new float[3];
  public float ClickVolume = 0.7f;
  public byte[] Enabled = new byte[MenuPacket.MaskSize];
  public byte[] Available = new byte[MenuPacket.MaskSize];
  public byte[] Favorites = new byte[MenuPacket.MaskSize];
  public int PressSerial;
  public int Timestamp;
  private static readonly uint Catalog = MenuPacket.CatalogHash();
  public static bool Bit(byte[] mask, int index)
  {
    return ((uint) mask[index / 8] & (uint) (1 << index % 8)) > 0U;
  }
  public static void Set(byte[] mask, int index) => mask[index / 8] |= (byte) (1 << index % 8);
  private static uint CatalogHash()
  {
    uint num = 2166136261;
    foreach (MenuEntry button in MenuCatalog.Buttons)
    {
      foreach (char ch in button.Name)
        num = (uint) (((int) num ^ (int) ch) * 16777619);
      num = (uint) ((((int) num ^ button.Group) * 16777619 ^ button.Target) * 16777619);
    }
    foreach (string name in MenuClickSounds.Names)
    {
      foreach (char ch in name)
        num = (uint) (((int) num ^ (int) ch) * 16777619);
    }
    foreach (string design in MenuPacket.Designs)
    {
      foreach (char ch in design)
        num = (uint) (((int) num ^ (int) ch) * 16777619);
    }
    return num;
  }
  public byte[] Encode()
  {
    using (MemoryStream output = new MemoryStream(MenuPacket.Length))
    {
      using (BinaryWriter binaryWriter = new BinaryWriter((Stream) output))
      {
        binaryWriter.Write(1095914836U);
        binaryWriter.Write(MenuPacket.Catalog);
        binaryWriter.Write((byte) ((this.Visible ? 1 : 0) | (this.RightHanded ? 2 : 0) | (this.Rounded ? 4 : 0) | (this.PrivateModded ? 8 : 0) | (this.Master ? 16 : 0) | (this.PrivateInput ? 32 : 0) | (this.InRoom ? 64 : 0)));
        binaryWriter.Write(this.Category);
        binaryWriter.Write(this.Page);
        binaryWriter.Write(this.Theme);
        binaryWriter.Write(this.FlySpeed);
        binaryWriter.Write(this.VrScale);
        binaryWriter.Write(this.Scale);
        foreach (float num in this.Placement)
          binaryWriter.Write(num);
        binaryWriter.Write(this.Enabled);
        binaryWriter.Write(this.Available);
        binaryWriter.Write(this.Favorites);
        binaryWriter.Write(this.GradientStart);
        binaryWriter.Write(this.GradientEnd);
        binaryWriter.Write(this.GradientPeriod);
        binaryWriter.Write(this.PressSerial);
        binaryWriter.Write(this.Press);
        binaryWriter.Write(this.Hover);
        binaryWriter.Write(this.ClickSound);
        binaryWriter.Write(this.Design);
        binaryWriter.Write(this.Font);
        binaryWriter.Write((byte) ((this.ColorPickerOpen ? 1 : 0) | (this.EditingEnd ? 2 : 0) | (this.PointerVisible ? 4 : 0)));
        foreach (float num in this.PointerPlacement)
          binaryWriter.Write(num);
        binaryWriter.Write(this.ClickVolume);
        binaryWriter.Write(this.Timestamp);
        return output.ToArray();
      }
    }
  }
  public static bool TryDecode(byte[] bytes, int now, out MenuPacket packet)
  {
    packet = (MenuPacket) null;
    if (bytes == null || bytes.Length != MenuPacket.Length)
      return false;
    using (BinaryReader binaryReader = new BinaryReader((Stream) new MemoryStream(bytes, false)))
    {
      if (binaryReader.ReadUInt32() != 1095914836U || (int) binaryReader.ReadUInt32() != (int) MenuPacket.Catalog)
        return false;
      byte num1 = binaryReader.ReadByte();
      if (num1 > (byte) 127 )
        return false;
      MenuPacket menuPacket = new MenuPacket()
      {
        Visible = ((uint) num1 & 1U) > 0U,
        RightHanded = ((uint) num1 & 2U) > 0U,
        Rounded = ((uint) num1 & 4U) > 0U,
        PrivateModded = ((uint) num1 & 8U) > 0U,
        Master = ((uint) num1 & 16U ) > 0U,
        PrivateInput = ((uint) num1 & 32U ) > 0U,
        InRoom = ((uint) num1 & 64U ) > 0U,
        Category = binaryReader.ReadByte(),
        Page = binaryReader.ReadUInt16(),
        Theme = binaryReader.ReadByte(),
        FlySpeed = binaryReader.ReadSingle(),
        VrScale = binaryReader.ReadSingle(),
        Scale = binaryReader.ReadSingle()
      };
      for (int index = 0; index < 6; ++index)
        menuPacket.Placement[index] = binaryReader.ReadSingle();
      menuPacket.Enabled = binaryReader.ReadBytes(MenuPacket.MaskSize);
      menuPacket.Available = binaryReader.ReadBytes(MenuPacket.MaskSize);
      menuPacket.Favorites = binaryReader.ReadBytes(MenuPacket.MaskSize);
      menuPacket.GradientStart = binaryReader.ReadUInt32();
      menuPacket.GradientEnd = binaryReader.ReadUInt32();
      menuPacket.GradientPeriod = binaryReader.ReadSingle();
      if (menuPacket.GradientStart > 16777215U || menuPacket.GradientEnd > 16777215U || !MenuPacket.Range(menuPacket.GradientPeriod, 5f, 60f))
        return false;
      menuPacket.PressSerial = binaryReader.ReadInt32();
      menuPacket.Press = binaryReader.ReadByte();
      menuPacket.Hover = binaryReader.ReadByte();
      menuPacket.ClickSound = binaryReader.ReadByte();
      if ((int) menuPacket.ClickSound >= MenuClickSounds.Names.Length)
        return false;
      menuPacket.Design = binaryReader.ReadByte();
      if ((int) menuPacket.Design >= MenuPacket.Designs.Length)
        return false;
      menuPacket.Font = binaryReader.ReadByte();
      if ((int) menuPacket.Font >= MenuPacket.Fonts.Length)
        return false;
      byte num2 = binaryReader.ReadByte();
      if (num2 > (byte) 7)
        return false;
      menuPacket.ColorPickerOpen = ((uint) num2 & 1U) > 0U;
      menuPacket.EditingEnd = ((uint) num2 & 2U) > 0U;
      menuPacket.PointerVisible = ((uint) num2 & 4U) > 0U;
      if (menuPacket.EditingEnd && !menuPacket.ColorPickerOpen)
        return false;
      for (int index = 0; index < 3; ++index)
      {
        menuPacket.PointerPlacement[index] = binaryReader.ReadSingle();
        if (!MenuPacket.Range(menuPacket.PointerPlacement[index], -2f, 2f))
          return false;
      }
      menuPacket.ClickVolume = binaryReader.ReadSingle();
      if (!MenuPacket.Range(menuPacket.ClickVolume, 0.0f, 1f))
        return false;
      menuPacket.Timestamp = binaryReader.ReadInt32();
      int num3 = now - menuPacket.Timestamp;
      if ((int) menuPacket.Category >= MenuCatalog.Categories.Length || (int) menuPacket.Page >= (MenuCatalog.Buttons.Length + 4) / 5 || (int) menuPacket.Theme >= MenuPacket.Themes.Length || menuPacket.Press > (byte) 46 || menuPacket.Hover > (byte) 45 || num3 < -2000 || num3 > 6000 || !MenuPacket.Range(menuPacket.FlySpeed, 2f, 100f) || !MenuPacket.Range(menuPacket.VrScale, 0.35f, 1.4f) || !MenuPacket.Range(menuPacket.Scale, 0.25f, 3f))
        return false;
      for (int index = 0; index < 6; ++index)
      {
        if (!MenuPacket.Range(menuPacket.Placement[index], index < 3 ? -2f : -3600f, index < 3 ? 2f : 3600f))
          return false;
      }
      for (int index = 0; index < MenuPacket.MaskSize * 8; ++index)
      {
        if (index >= MenuCatalog.Buttons.Length && (MenuPacket.Bit(menuPacket.Enabled, index) || MenuPacket.Bit(menuPacket.Available, index) || MenuPacket.Bit(menuPacket.Favorites, index)) || index < MenuCatalog.Buttons.Length && MenuPacket.Bit(menuPacket.Favorites, index) && !MenuCatalog.CanFavorite(MenuCatalog.Buttons[index]) || index < MenuCatalog.Buttons.Length && MenuPacket.Bit(menuPacket.Enabled, index) && (!MenuPacket.Bit(menuPacket.Available, index) || !MenuCatalog.Buttons[index].Toggle))
          return false;
      }
      packet = menuPacket;
      return true;
    }
  }
  private static bool Range(float value, float min, float max)
  {
    return !float.IsNaN(value) && !float.IsInfinity(value) && (double) value >= (double) min && (double) value <= (double) max;
  }
  public static bool SameView(MenuPacket a, MenuPacket b)
  {
    if (a == null || b == null || (int) a.Category != (int) b.Category || (int) a.Page != (int) b.Page || (int) a.Theme != (int) b.Theme || (int) a.Design != (int) b.Design || (int) a.Font != (int) b.Font || a.Rounded != b.Rounded || a.PrivateInput != b.PrivateInput || a.PrivateModded != b.PrivateModded || a.Master != b.Master || a.InRoom != b.InRoom || a.ColorPickerOpen != b.ColorPickerOpen || a.EditingEnd != b.EditingEnd || (int) a.GradientStart != (int) b.GradientStart || (int) a.GradientEnd != (int) b.GradientEnd || (double) a.GradientPeriod != (double) b.GradientPeriod || (double) a.FlySpeed != (double) b.FlySpeed || (double) a.VrScale != (double) b.VrScale || (int) a.ClickSound != (int) b.ClickSound || (double) a.ClickVolume != (double) b.ClickVolume)
      return false;
    for (int index = 0; index < MenuPacket.MaskSize; ++index)
    {
      if ((int) a.Enabled[index] != (int) b.Enabled[index] || (int) a.Available[index] != (int) b.Available[index] || (int) a.Favorites[index] != (int) b.Favorites[index])
        return false;
    }
    return true;
  }
  public static bool SameDisplay(byte[] first, byte[] second)
  {
    if (first == null || second == null || first.Length != MenuPacket.Length || second.Length != MenuPacket.Length)
      return false;
    for (int index = 0; index < MenuPacket.Length - 4; ++index)
    {
      if ((int) first[index] != (int) second[index])
        return false;
    }
    return true;
  }
  public static byte PressId(string name)
  {
    if (name != null && name.StartsWith("Key_", StringComparison.Ordinal))
      return 46;
    switch (name)
    {
      case "DesignLogo":
        return 43;
      case "ButtonEnabled":
        return 44;
      case "ButtonRoomStatus":
        return 45;
      default:
        for (int index = 7; index < 12; ++index)
        {
          if (name == "Row" + index.ToString())
            return (byte) (28 + index - 7);
          if (name == "IncrementMinus" + index.ToString())
            return (byte) (33 + index - 7);
          if (name == "IncrementPlus" + index.ToString())
            return (byte) (38 + index - 7);
        }
        if (name == "DesignSettings")
          return 27;
        for (int index = 5; index < 7; ++index)
        {
          if (name == "Row" + index.ToString())
            return (byte) (21 + index - 5);
          if (name == "IncrementMinus" + index.ToString())
            return (byte) (23 + index - 5);
          if (name == "IncrementPlus" + index.ToString())
            return (byte) (25 + index - 5);
        }
        for (int index = 0; index < 5; ++index)
        {
          if (name == "Row" + index.ToString())
            return (byte) (index + 1);
          if (name == "IncrementMinus" + index.ToString())
            return (byte) (index + 6);
          if (name == "IncrementPlus" + index.ToString())
            return (byte) (index + 11);
        }
        int num;
        switch (name)
        {
          case "ButtonHome":
            num = 16 ;
            break;
          case "ButtonPrevious":
            num = 17;
            break;
          case "ButtonNext":
            num = 18;
            break;
          case "ButtonSearch":
            num = 19;
            break;
          case "ButtonDisconnect":
            num = 20;
            break;
          default:
            num = 0;
            break;
        }
        return (byte) num;
    }
  }
  public static string PressName(byte id)
  {
    switch (id)
    {
      case 43:
        return "DesignLogo";
      case 44:
        return "ButtonEnabled";
      case 45:
        return "ButtonRoomStatus";
      case 46:
        return "Key_Typed";
      default:
        if (id >= (byte) 28 && id <= (byte) 32 )
          return "Row" + ((int) id - 21).ToString();
        if (id >= (byte) 33 && id <= (byte) 37)
          return "IncrementMinus" + ((int) id - 26).ToString();
        if (id >= (byte) 38 && id <= (byte) 42)
          return "IncrementPlus" + ((int) id - 31 ).ToString();
        if (id == (byte) 27)
          return "DesignSettings";
        if (id >= (byte) 21 && id <= (byte) 22)
          return "Row" + ((int) id - 16 ).ToString();
        if (id >= (byte) 23 && id <= (byte) 24)
          return "IncrementMinus" + ((int) id - 18).ToString();
        if (id >= (byte) 25 && id <= (byte) 26)
          return "IncrementPlus" + ((int) id - 20).ToString();
        if (id >= (byte) 1 && id <= (byte) 5)
          return "Row" + ((int) id - 1).ToString();
        if (id >= (byte) 6 && id <= (byte) 10)
          return "IncrementMinus" + ((int) id - 6).ToString();
        if (id >= (byte) 11 && id <= (byte) 15)
          return "IncrementPlus" + ((int) id - 11).ToString();
        switch (id)
        {
          case 16 :
            return "ButtonHome";
          case 17:
            return "ButtonPrevious";
          case 18:
            return "ButtonNext";
          case 19:
            return "ButtonSearch";
          case 20:
            return "ButtonDisconnect";
          default:
            return "";
        }
    }
  }
}
