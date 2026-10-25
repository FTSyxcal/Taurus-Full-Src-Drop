
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Taurus;
internal static class NativeUiResources
{
  private static readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
  private static Sprite logo;
  public static Sprite UntitledLogo
  {
    get
    {
      return !((Object) NativeUiResources.logo != (Object) null) ? (NativeUiResources.logo = Sprite.Create(NativeUiResources.Load("untitled-logo.png"), new Rect(0.0f, 0.0f, (float) ((Texture) NativeUiResources.Load("untitled-logo.png")).width, (float) ((Texture) NativeUiResources.Load("untitled-logo.png")).height), (Vector2.one * 0.5f), 100f)) : NativeUiResources.logo;
    }
  }
  public static Texture2D LoadNxoTexture(string name)
  {
    return NativeUiResources.Load(name.Substring("NXO.Resources.".Length));
  }
  public static Texture2D LoadImage(string name) => NativeUiResources.Load(name);
  private static Texture2D Load(string name)
  {
    Texture2D texture2D1;
    if (NativeUiResources.textures.TryGetValue(name, out texture2D1))
      return texture2D1;
    byte[] array;
    using (Stream manifestResourceStream = typeof (NativeUiResources).Assembly.GetManifestResourceStream("Taurus.NativeUi." + name))
    {
      using (MemoryStream destination = new MemoryStream())
      {
        if (manifestResourceStream == null)
          throw new InvalidDataException("Missing native UI texture: " + name);
        manifestResourceStream.CopyTo((Stream) destination);
        array = destination.ToArray();
      }
    }
    Texture2D texture2D2 = new Texture2D(2, 2);
    ((Object) texture2D2).name = name;
    Texture2D texture2D3 = texture2D2;
    NativeUiResources.textures[name] = ImageConversion.LoadImage(texture2D3, array) ? texture2D3 : throw new InvalidDataException("Invalid native UI texture: " + name);
    return texture2D3;
  }
}
