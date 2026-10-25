
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class MenuFontView : MonoBehaviour
{
  private readonly List<MenuFontView.Label> labels = new List<MenuFontView.Label>();
  private readonly HashSet<Component> captured = new HashSet<Component>();
  private readonly Dictionary<Canvas, float> quality = new Dictionary<Canvas, float>();
  private bool dirtyLabels = true;
  private int current = -1;
  private bool effectsEnabled = true;
  public void Invalidate() => this.dirtyLabels = true;
  public void SetEffects(bool enabled)
  {
    if (this.effectsEnabled == enabled)
      return;
    this.effectsEnabled = enabled;
    if (enabled)
      return;
    foreach (MenuFontView.Label label in this.labels)
    {
      label.Effect?.Dispose();
      label.Effect = (MenuButtonEffects) null;
      label.EffectText = (string) null;
    }
  }
  public bool Apply(int index)
  {
    bool flag1 = false;
    if (this.dirtyLabels)
    {
      this.dirtyLabels = false;
      foreach (TextMesh componentsInChild in ((Component) this).GetComponentsInChildren<TextMesh>(true))
      {
        if (((Object) ((Component) componentsInChild).GetComponentInParent<MenuFontView>() == (Object) this) && this.captured.Add((Component) componentsInChild))
        {
          this.labels.Add(new MenuFontView.Label()
          {
            Mesh = componentsInChild,
            Font = componentsInChild.font,
            Original = ((Component) componentsInChild).GetComponent<Renderer>().sharedMaterial,
            CharacterSize = componentsInChild.characterSize
          });
          flag1 = true;
        }
      }
      foreach (Text componentsInChild in ((Component) this).GetComponentsInChildren<Text>(true))
      {
        if (((Object) ((Component) componentsInChild).GetComponentInParent<MenuFontView>() == (Object) this) && this.captured.Add((Component) componentsInChild))
        {
          this.labels.Add(new MenuFontView.Label()
          {
            Ui = componentsInChild,
            Font = componentsInChild.font,
            BestFit = componentsInChild.resizeTextForBestFit,
            MinSize = componentsInChild.resizeTextMinSize,
            MaxSize = componentsInChild.resizeTextMaxSize,
            FontSize = componentsInChild.fontSize
          });
          flag1 = true;
        }
      }
      foreach (TMP_Text componentsInChild in ((Component) this).GetComponentsInChildren<TMP_Text>(true))
      {
        if (((Object) ((Component) componentsInChild).GetComponentInParent<MenuFontView>() == (Object) this) && this.captured.Add((Component) componentsInChild))
        {
          this.labels.Add(new MenuFontView.Label()
          {
            Tmp = componentsInChild,
            Sdf = componentsInChild.font,
            Original = componentsInChild.fontSharedMaterial
          });
          flag1 = true;
        }
      }
      this.quality.Clear();
      foreach (MenuFontView.Label label in this.labels)
      {
        if (((Object) label.Ui != (Object) null) && ((Object) ((Graphic) label.Ui).canvas != (Object) null))
        {
          Canvas canvas = ((Graphic) label.Ui).canvas;
          CanvasScaler component = ((Component) canvas).GetComponent<CanvasScaler>();
          float num1 = Mathf.Max(new float[4]
          {
            1f,
            canvas.scaleFactor,
            ((Object) component == (Object) null) ? 1f : component.dynamicPixelsPerUnit,
            Mathf.Ceil(4000f * Mathf.Abs(((Component) label.Ui).transform.lossyScale.x))
          });
          float num2;
          if (!this.quality.TryGetValue(canvas, out num2) || (double) num1 > (double) num2)
            this.quality[canvas] = num1;
        }
      }
      foreach (KeyValuePair<Canvas, float> keyValuePair in this.quality)
      {
        CanvasScaler component = ((Component) keyValuePair.Key).GetComponent<CanvasScaler>();
        if (((Object) component != (Object) null))
          component.dynamicPixelsPerUnit = keyValuePair.Value;
        keyValuePair.Key.scaleFactor = keyValuePair.Value;
      }
    }
    if (this.current == index && !flag1)
    {
      bool flag2 = false;
      foreach (MenuFontView.Label label in this.labels)
      {
        if (((Object) label.Mesh != (Object) null) && ((Object) label.Font != (Object) null) && !Mathf.Approximately(label.Mesh.characterSize, label.LastSize))
        {
          label.CharacterSize = label.Mesh.characterSize;
          if (index != 1)
            label.Mesh.characterSize *= (float) label.Font.fontSize / Mathf.Max(1f, (float) label.Mesh.font.fontSize);
          label.LastSize = label.Mesh.characterSize;
          flag2 = true;
        }
      }
      return this.Animate(Time.unscaledTime) | flag2;
    }
    this.current = index;
    Font font1 = (Font) null;
    Font font2 = (Font) null;
    TMP_FontAsset tmpFontAsset = (TMP_FontAsset) null;
    if (index != 1)
    {
      AssetBundle assetBundle = ImportedAssetMenu.Load("menu-fonts");
      font1 = assetBundle.LoadAsset<Font>("assets/fonts/selectable/" + MenuFonts.Files[index].ToLowerInvariant());
      font2 = assetBundle.LoadAsset<Font>("assets/fonts/selectable/ui/" + MenuFonts.Files[index].ToLowerInvariant());
      tmpFontAsset = assetBundle.LoadAsset<TMP_FontAsset>($"assets/fonts/selectable/sdf/font-{index.ToString()}.asset");
      if (((Object) font1 == (Object) null) || ((Object) font2 == (Object) null) || ((Object) tmpFontAsset == (Object) null))
        throw new InvalidOperationException("Missing selectable font: " + MenuFonts.Names[index]);
    }
    foreach (MenuFontView.Label label in this.labels)
    {
      if (((Object) label.Replacement != (Object) null))
        NativeUiSnapshot.Destroy((Object) label.Replacement);
      label.Replacement = (Material) null;
      if (((Object) label.Ui != (Object) null))
      {
        label.Ui.font = index == 1 ? label.Font : font2;
        label.Ui.resizeTextForBestFit = index != 1 || label.BestFit;
        label.Ui.resizeTextMinSize = index == 1 ? label.MinSize : Math.Min(label.MinSize, 1);
        label.Ui.resizeTextMaxSize = index == 1 ? label.MaxSize : (label.BestFit ? label.MaxSize : label.FontSize);
      }
      if (((Object) label.Tmp != (Object) null))
      {
        label.Tmp.font = index == 1 ? label.Sdf : tmpFontAsset;
        if (index == 1)
        {
          label.Tmp.fontSharedMaterial = label.Original;
        }
        else
        {
          label.Replacement = new Material(((TMP_Asset) tmpFontAsset).material);
          string[] strArray = new string[2]
          {
            "_FaceDilate",
            "_OutlineWidth"
          };
          foreach (string str in strArray)
          {
            if (((Object) label.Original != (Object) null) && label.Original.HasProperty(str))
              label.Replacement.SetFloat(str, label.Original.GetFloat(str));
          }
          label.Tmp.fontSharedMaterial = label.Replacement;
        }
        ((Graphic) label.Tmp).SetAllDirty();
      }
      if (((Object) label.Mesh != (Object) null))
      {
        label.Mesh.font = index == 1 ? label.Font : font1;
        if (index == 1)
        {
          ((Component) label.Mesh).GetComponent<Renderer>().sharedMaterial = label.Original;
        }
        else
        {
          label.Replacement = new Material(label.Original)
          {
            mainTexture = font1.material.mainTexture
          };
          ((Component) label.Mesh).GetComponent<Renderer>().sharedMaterial = label.Replacement;
          if (((Object) label.Font != (Object) null))
            label.Mesh.characterSize = label.CharacterSize * (float) label.Font.fontSize / Mathf.Max(1f, (float) font1.fontSize);
        }
        if (index == 1)
          label.Mesh.characterSize = label.CharacterSize;
        label.LastSize = label.Mesh.characterSize;
      }
    }
    this.Animate(Time.unscaledTime);
    return true;
  }
  public bool Animate(float time)
  {
    if (!this.effectsEnabled)
      return false;
    bool flag = false;
    foreach (MenuFontView.Label label1 in this.labels)
    {
      Component component = ((Object) label1.Mesh != (Object) null) ? (Component) (object) label1.Mesh : (((Object) label1.Ui != (Object) null) ? (Component) (object) label1.Ui : (Component) (object) label1.Tmp);
      if (!((Object) component == (Object) null) && component.gameObject.activeInHierarchy)
      {
        string text = ((Object) label1.Mesh != (Object) null) ? label1.Mesh.text : (((Object) label1.Ui != (Object) null) ? label1.Ui.text : (((Object) label1.Tmp != (Object) null) ? label1.Tmp.text : ""));
        if (label1.Effect == null && label1.EffectText != text)
        {
          label1.EffectText = text;
          Transform label2 = ((Object) label1.Mesh != (Object) null) ? ((Component) label1.Mesh).transform : (((Object) label1.Ui != (Object) null) ? ((Component) label1.Ui).transform : (((Object) label1.Tmp != (Object) null) ? label1.Tmp.transform : (Transform) null));
          if (((Object) label2 != (Object) null) && ((Object) ((Component) label2).GetComponent<NativeUiSemanticColor>() == (Object) null) && MenuButtonEffects.Matches(text))
            label1.Effect = new MenuButtonEffects(label2);
        }
        if (label1.Effect != null && label1.Effect.Tick(ref text, time))
        {
          if (((Object) label1.Mesh != (Object) null))
            label1.Mesh.text = text;
          else if (((Object) label1.Ui != (Object) null))
            label1.Ui.text = text;
          else if (((Object) label1.Tmp != (Object) null))
            label1.Tmp.text = text;
          flag = true;
        }
      }
    }
    return flag;
  }
  private void OnDestroy()
  {
    foreach (MenuFontView.Label label in this.labels)
    {
      label.Effect?.Dispose();
      if (((Object) label.Replacement != (Object) null))
        NativeUiSnapshot.Destroy((Object) label.Replacement);
    }
  }
  private sealed class Label
  {
    public TextMesh Mesh;
    public Text Ui;
    public TMP_Text Tmp;
    public Font Font;
    public TMP_FontAsset Sdf;
    public Material Original;
    public Material Replacement;
    public float CharacterSize;
    public float LastSize;
    public bool BestFit;
    public int MinSize;
    public int MaxSize;
    public int FontSize;
    public MenuButtonEffects Effect;
    public string EffectText;
  }
}
