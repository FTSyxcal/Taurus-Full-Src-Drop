
using UnityEngine;
namespace Taurus;
public static class MenuFonts
{
  public static readonly string[] Names = MenuPacket.Fonts;
  public static readonly string[] Files = new string[9]
  {
    "Minecraft Regular.otf",
    "",
    "Gilroy-Black.ttf",
    "Gogga_Thorax.otf",
    "gorilla-tag-title.ttf",
    "impact.ttf",
    "montserrat.bold.ttf",
    "Montserrat-Black.ttf",
    "CardboardCutout.otf"
  };
  public static bool Apply(Transform root, int index, bool effects = true)
  {
    MenuFontView menuFontView = ((Component) root).GetComponent<MenuFontView>();
    if (((Object) menuFontView == (Object) null))
      menuFontView = ((Component) root).gameObject.AddComponent<MenuFontView>();
    menuFontView.SetEffects(effects);
    return menuFontView.Apply(index);
  }
  public static void Animate(Transform root, float time)
  {
    ((Component) root).GetComponent<MenuFontView>()?.Animate(time);
  }
  public static void Invalidate(Transform root)
  {
    ((Component) root).GetComponent<MenuFontView>()?.Invalidate();
  }
}
