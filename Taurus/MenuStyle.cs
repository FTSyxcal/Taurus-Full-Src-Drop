
using UnityEngine;
namespace Taurus;
public static class MenuStyle
{
  public static readonly Color Background = ((Color32)new Color32((byte) 18, (byte) 21, (byte) 27, byte.MaxValue));
  public static readonly Color Surface = ((Color32)new Color32((byte) 30, (byte) 35, (byte) 44, byte.MaxValue));
  public static readonly Color Border = ((Color32)new Color32((byte) 49, (byte) 57, (byte) 69, byte.MaxValue));
  public static readonly Color Text = ((Color32)new Color32((byte) 235, (byte) 240 , (byte) 247, byte.MaxValue));
  public static readonly Color Muted = ((Color32)new Color32((byte) 165, (byte) 176 , (byte) 193, byte.MaxValue));
  public static readonly Color Blue = ((Color32)new Color32((byte) 109, (byte) 154, (byte) 251, byte.MaxValue));
  public static readonly Color Mint = ((Color32)new Color32((byte) 92, (byte) 207, (byte) 186, byte.MaxValue));
  public static readonly Color AccentStart = ((Color32)new Color32(byte.MaxValue, (byte) 75, (byte) 75, byte.MaxValue));
  public static readonly Color AccentEnd = ((Color32)new Color32(byte.MaxValue, (byte) 155, (byte) 63 , byte.MaxValue));
  public static readonly Color Error = ((Color32)new Color32(byte.MaxValue, (byte) 64 , (byte) 64 , byte.MaxValue));
  public static readonly Color ButtonOff = new Color(0.23f, 0.23f, 0.25f);
  public static readonly Color ButtonOn = new Color(0.35f, 0.35f, 0.38f);
  public const float PopDuration = 0.32f;
  public const int PixelDensity = 4000;
  public const float DesktopScale = 1.3f;
  public const float VrScale = 0.65f;
  public const float CloseDuration = 0.2f;
  public const float PressDuration = 0.2f;
  public const float DiscordWidth = 0.324f;
  public const float DiscordHeight = 0.45f;
  public const float DiscordCorner = 0.014f;
  public const float DiscordVrScale = 1.35f;
  public const float MonoSandboxVrScale = 1.5f;
  public const float ShibaDarkVrScale = 1.25f;
  public const float HamburburVrScale = 1.1f;
  public static float DesignVrScale(int design, int category)
  {
    if (category == 11)
      return 1.35f;
    switch (design)
    {
      case 2:
        return 1.1f;
      case 3:
        return 1.5f;
      case 4:
        return 1.25f;
      case 12:
        return 0.615384638f;
      case 13:
        return 0.4615385f;
      default:
        return design < 5 ? 1f : 1.53846157f;
    }
  }
  public static float CloseScale(float elapsed)
  {
    return 1f - Mathf.SmoothStep(0.0f, 1f, Mathf.Clamp01(elapsed / 0.2f));
  }
  public static float PressScale(float elapsed)
  {
    float num = Mathf.Clamp01(elapsed / 0.2f);
    return (double) num >= 0.25 ? Mathf.Lerp(0.91f, 1f, 1f - Mathf.Pow((float) ((1.0 - (double) num) / 0.75), 3f)) : Mathf.Lerp(1f, 0.91f, Mathf.SmoothStep(0.0f, 1f, num * 4f));
  }
  public static float OpenScale(float elapsed)
  {
    float num = Mathf.Clamp01(elapsed / 0.32f) - 1f;
    return (float) (0.64999997615814209 + 0.34999999403953552 * (1.0 + 2.7015800476074219 * (double) num * (double) num * (double) num + 1.7015800476074219 * (double) num * (double) num));
  }
}
