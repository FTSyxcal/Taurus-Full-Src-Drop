
using UnityEngine;
namespace Taurus;
public static class MenuPalette
{
  private static readonly uint[] DesignLeft = new uint[14]
  {
    16730955U,
    9639446U,
    2827867U,
    16777215U ,
    11731174U,
    16744448U,
    16777215U ,
    917503U ,
    15188605U,
    12617983U,
    47080U,
    24319U,
    12399668U,
    11035903U
  };
  private static readonly uint[] DesignRight = new uint[14]
  {
    16751423U,
    9639446U,
    656947U,
    8421504U ,
    4412927U,
    16737792U,
    9211020U ,
    16777215U ,
    15919836U,
    12617983U,
    47080U,
    24319U,
    12399668U,
    11035903U
  };
  private static readonly uint[] DesignBackground = new uint[14]
  {
    1185051U,
    1513239U ,
    2827867U,
    1250330U,
    526346U,
    16744448U,
    657930U,
    986895U,
    2368289U,
    3280219U,
    1052688U ,
    1513239U ,
    1769472U ,
    131331U
  };
  private static readonly uint[] DesignSurface = new uint[14]
  {
    3881792U,
    1513239U ,
    656947U,
    16777215U ,
    8421504U ,
    11162880U,
    1315860U ,
    2631720U ,
    14075043U,
    2555971U,
    1315860U ,
    3289650U ,
    2821390U,
    328202U
  };
  public static Color Unpack(uint rgb)
  {
    return ((Color32)new Color32((byte) (rgb >> 16 ), (byte) (rgb >> 8), (byte) rgb, byte.MaxValue));
  }
  public static uint Pack(Color color)
  {
    Color32 color32 = ((Color32)color);
    return (uint) ((int) color32.r << 16 | (int) color32.g << 8) | (uint) color32.b;
  }
  public static Color Left(MenuState state, float time)
  {
    if (state.Design == 9)
      return MenuPalette.Unpack(MenuPalette.DesignLeft[9]);
    if (state.Theme == "Custom")
      return MenuPalette.Unpack(state.GradientStart);
    return !(state.Theme == "ItzGame") ? MenuPalette.Left(state.Theme, time) : MenuPalette.Unpack(MenuPalette.DesignLeft[state.Design]);
  }
  public static Color Right(MenuState state, float time)
  {
    if (state.Design == 9)
      return MenuPalette.Unpack(MenuPalette.DesignRight[9]);
    if (state.Theme == "Custom")
      return MenuPalette.Unpack(state.GradientEnd);
    return !(state.Theme == "ItzGame") ? MenuPalette.Right(state.Theme, time) : MenuPalette.Unpack(MenuPalette.DesignRight[state.Design]);
  }
  public static Color Background(MenuState state, float time)
  {
    return state.Design != 9 && !(state.Theme == "ItzGame") ? MenuPalette.Background(state.Theme, time) : MenuPalette.Unpack(MenuPalette.DesignBackground[state.Design]);
  }
  public static Color Surface(MenuState state)
  {
    if (state.Design == 0)
      return MenuStyle.ButtonOff;
    return state.Design != 9 && !(state.Theme == "ItzGame") ? Color.Lerp(MenuPalette.Background(state, 0.0f), MenuPalette.Left(state, 0.0f), 0.12f) : MenuPalette.Unpack(MenuPalette.DesignSurface[state.Design]);
  }
  public static Color Background(string theme, float time)
  {
    switch (theme)
    {
      case "BlackRed":
        return ((Color32)new Color32((byte) 25, (byte) 19, (byte) 23, byte.MaxValue));
      case "MidnightBlue":
        return ((Color32)new Color32((byte) 17, (byte) 23, (byte) 36, byte.MaxValue));
      case "ToxicGreen":
        return ((Color32)new Color32((byte) 16 , (byte) 28, (byte) 24, byte.MaxValue));
      case "PurpleHaze":
        return ((Color32)new Color32((byte) 25, (byte) 20, (byte) 35, byte.MaxValue));
      case "DarkGrey":
        return ((Color32)new Color32((byte) 22, (byte) 25, (byte) 31 , byte.MaxValue));
      default:
        return MenuStyle.Background;
    }
  }
  public static Color Left(string theme, float time)
  {
    if (theme != null)
    {
      switch (theme.Length)
      {
        case 3:
          if (theme == "Ice")
            return MenuPalette.Unpack(10875900U);
          break;
        case 4:
          switch (theme[3])
          {
            case 'e':
              if (theme == "Fire")
                return MenuPalette.Unpack(16725037U);
              break;
            case 'o':
              if (theme == "Mono")
                return MenuPalette.Unpack(10265519U);
              break;
            case 't':
              if (theme == "Mint")
                return MenuPalette.Unpack(7183099U);
              break;
          }
          break;
        case 5:
          if (theme == "Ocean")
            return MenuPalette.Unpack(3718648U);
          break;
        case 6:
          switch (theme[0])
          {
            case 'A':
              if (theme == "Aurora")
                return MenuPalette.Unpack(3462041U);
              break;
            case 'S':
              if (theme == "Sunset")
                return MenuPalette.Unpack(16739211U);
              break;
          }
          break;
        case 7:
          if (theme == "Rainbow")
            return Color.HSVToRGB(Mathf.Repeat(time / 2f, 1f), 0.85f, 1f);
          break;
        case 8:
          switch (theme[0])
          {
            case 'B':
              if (theme == "BlackRed")
                return ((Color32)new Color32((byte) 251, (byte) 113, (byte) 133, byte.MaxValue));
              break;
            case 'D':
              if (theme == "DarkGrey")
                return ((Color32)new Color32((byte) 180, (byte) 190, (byte) 205, byte.MaxValue));
              break;
            case 'R':
              if (theme == "RoseGold")
                return MenuPalette.Unpack(16361656U);
              break;
          }
          break;
        case 10:
          switch (theme[0])
          {
            case 'P':
              if (theme == "PurpleHaze")
                return ((Color32)new Color32((byte) 167, (byte) 139, (byte) 250, byte.MaxValue));
              break;
            case 'T':
              if (theme == "ToxicGreen")
                return ((Color32)new Color32((byte) 110, (byte) 231, (byte) 183, byte.MaxValue));
              break;
          }
          break;
        case 12:
          if (theme == "MidnightBlue")
            return MenuStyle.Blue;
          break;
      }
    }
    return MenuStyle.AccentStart;
  }
  public static Color Right(string theme, float time)
  {
    if (theme != null)
    {
      switch (theme.Length)
      {
        case 3:
          if (theme == "Ice")
            return MenuPalette.Unpack(11979263U);
          break;
        case 4:
          switch (theme[3])
          {
            case 'e':
              if (theme == "Fire")
                return MenuPalette.Unpack(16761665U);
              break;
            case 'o':
              if (theme == "Mono")
                return MenuPalette.Unpack(15857145U);
              break;
            case 't':
              if (theme == "Mint")
                return MenuPalette.Unpack(6082490U);
              break;
          }
          break;
        case 5:
          if (theme == "Ocean")
            return MenuPalette.Unpack(6514417U);
          break;
        case 6:
          switch (theme[0])
          {
            case 'A':
              if (theme == "Aurora")
                return MenuPalette.Unpack(10980346U);
              break;
            case 'S':
              if (theme == "Sunset")
                return MenuPalette.Unpack(16760169U);
              break;
          }
          break;
        case 7:
          if (theme == "Rainbow")
            return Color.HSVToRGB(Mathf.Repeat((float) ((double) time / 2.0 + 0.18000000715255737), 1f), 0.85f, 1f);
          break;
        case 8:
          switch (theme[0])
          {
            case 'B':
              if (theme == "BlackRed")
                return ((Color32)new Color32((byte) 239, (byte) 68, (byte) 68, byte.MaxValue));
              break;
            case 'D':
              if (theme == "DarkGrey")
                return ((Color32)new Color32((byte) 230, (byte) 234, (byte) 240 , byte.MaxValue));
              break;
            case 'R':
              if (theme == "RoseGold")
                return MenuPalette.Unpack(16176026U);
              break;
          }
          break;
        case 10:
          switch (theme[0])
          {
            case 'P':
              if (theme == "PurpleHaze")
                return ((Color32)new Color32((byte) 232, (byte) 121, (byte) 249, byte.MaxValue));
              break;
            case 'T':
              if (theme == "ToxicGreen")
                return ((Color32)new Color32((byte) 163, (byte) 230, (byte) 53, byte.MaxValue));
              break;
          }
          break;
        case 12:
          if (theme == "MidnightBlue")
            return ((Color32)new Color32((byte) 129, (byte) 140, (byte) 248, byte.MaxValue));
          break;
      }
    }
    return MenuStyle.AccentEnd;
  }
}
