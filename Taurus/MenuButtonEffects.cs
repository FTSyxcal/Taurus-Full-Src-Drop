
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class MenuButtonEffects : IDisposable
{
  private static readonly Regex Tags = new Regex("<[^>]*>");
  private static readonly Regex Colors = new Regex("</?color(?:=[^>]*)?>", RegexOptions.IgnoreCase);
  private static readonly Regex Spaces = new Regex("\\s+");
  private readonly Transform label;
  private readonly GameObject[] blocks = new GameObject[8];
  private readonly Vector2[] positions = new Vector2[8];
  private readonly Vector2[] velocities = new Vector2[8];
  private readonly Renderer[] renderers = new Renderer[8];
  private readonly MaterialPropertyBlock paint = new MaterialPropertyBlock();
  private readonly Material material;
  private readonly Transform background;
  private readonly bool paintBackground;
  private readonly Dictionary<Renderer, MaterialPropertyBlock> savedPaint = new Dictionary<Renderer, MaterialPropertyBlock>();
  private Graphic backgroundGraphic;
  private Color graphicColor;
  private Color fillColor;
  private PropertyInfo fill;
  private float size;
  private float previousTime = -1f;
  private Rect region;
  private int previousStep = -1;
  private string original = "";
  private string output = "";
  private bool active;
  public MenuButtonEffects(Transform label)
  {
    this.label = label;
    this.background = ((Component) label).GetComponent<MenuEffectAnchor>()?.Target ?? label.parent;
    MenuEffectAnchor component = ((Component) label).GetComponent<MenuEffectAnchor>();
    this.paintBackground = component == null || component.PaintBackground;
    foreach (Renderer componentsInChild in ((Component) this.background).GetComponentsInChildren<Renderer>(true))
    {
      if (!((Object) ((Component) componentsInChild).GetComponent<TMP_Text>() != (Object) null) && !((Object) ((Component) componentsInChild).GetComponent<TextMesh>() != (Object) null))
      {
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        componentsInChild.GetPropertyBlock(materialPropertyBlock);
        this.savedPaint[componentsInChild] = materialPropertyBlock;
      }
    }
    foreach (Graphic componentsInChild in ((Component) this.background).GetComponentsInChildren<Graphic>(true))
    {
      if (!(componentsInChild is UnityEngine.UI.Text) && !(componentsInChild is TMP_Text))
      {
        if (!((Object) this.backgroundGraphic == (Object) null))
        {
          Rect rect1 = componentsInChild.rectTransform.rect;
          Vector2 size = (rect1).size;
          double sqrMagnitude1 = (double) (size).sqrMagnitude;
          Rect rect2 = this.backgroundGraphic.rectTransform.rect;
          size = (rect2).size;
          double sqrMagnitude2 = (double) (size).sqrMagnitude;
          if (sqrMagnitude1 <= sqrMagnitude2)
            continue;
        }
        this.backgroundGraphic = componentsInChild;
      }
    }
    if (((Object) this.backgroundGraphic != (Object) null))
    {
      this.graphicColor = this.backgroundGraphic.color;
      this.fill = this.backgroundGraphic.GetType().GetProperty("FillColor");
      if (this.fill != (PropertyInfo) null)
        this.fillColor = (Color) this.fill.GetValue((object) this.backgroundGraphic);
    }
    Shader shader = Shader.Find("ItzGame/Solid") ?? Shader.Find("Unlit/Color");
    if (!((Object) shader != (Object) null))
      return;
    this.material = new Material(shader)
    {
      color = Color.white
    };
  }
  public static void Bind(Transform label, Transform target, bool paintBackground = true)
  {
    MenuEffectAnchor menuEffectAnchor = ((Component) label).GetComponent<MenuEffectAnchor>() ?? ((Component) label).gameObject.AddComponent<MenuEffectAnchor>();
    menuEffectAnchor.Target = target;
    menuEffectAnchor.PaintBackground = paintBackground;
  }
  public static void BindRows(
    Transform source,
    MenuState state,
    IEnumerable<KeyValuePair<string, Transform>> controls)
  {
    foreach (KeyValuePair<string, Transform> control in controls)
    {
      int result;
      if (control.Key.StartsWith("Row", StringComparison.Ordinal) && int.TryParse(control.Key.Substring(3), out result))
      {
        int index = state.EntryAt(result);
        if (index >= 0)
        {
          string name = MenuCatalog.Buttons[index].Name;
          if (name.StartsWith("Crash", StringComparison.Ordinal) || name.StartsWith("Kick", StringComparison.Ordinal))
          {
            foreach (UnityEngine.UI.Text componentsInChild in ((Component) source).GetComponentsInChildren<UnityEngine.UI.Text>(true))
            {
              if (MenuButtonEffects.Plain(componentsInChild.text).StartsWith(name, StringComparison.OrdinalIgnoreCase))
                MenuButtonEffects.Bind(((Component) componentsInChild).transform, control.Value);
            }
            foreach (TMP_Text componentsInChild in ((Component) source).GetComponentsInChildren<TMP_Text>(true))
            {
              if (MenuButtonEffects.Plain(componentsInChild.text).StartsWith(name, StringComparison.OrdinalIgnoreCase))
                MenuButtonEffects.Bind(componentsInChild.transform, control.Value);
            }
          }
        }
      }
    }
  }
  private static string Plain(string text)
  {
    return MenuButtonEffects.Spaces.Replace(MenuButtonEffects.Tags.Replace(text, ""), " ").Trim().TrimStart('★', ' ');
  }
  public static bool Matches(string text)
  {
    string str = MenuButtonEffects.Plain(text);
    foreach (MenuEntry button in MenuCatalog.Buttons)
    {
      if (MenuState.IsFeature(button) && (button.Name.StartsWith("Crash", StringComparison.Ordinal) || button.Name.StartsWith("Kick", StringComparison.Ordinal)) && str.StartsWith(button.Name, StringComparison.OrdinalIgnoreCase) && (str.Length == button.Name.Length || !char.IsLetterOrDigit(str[button.Name.Length])))
        return true;
    }
    return false;
  }
  public bool Tick(ref string text, float time)
  {
    if (text != this.output)
    {
      this.original = MenuButtonEffects.Colors.Replace(text, "");
      this.active = MenuButtonEffects.Matches(this.original);
      if (!this.active)
        this.RestoreBackground();
      this.previousStep = -1;
      foreach (GameObject block in this.blocks)
      {
        if (((Object) block != (Object) null))
          block.SetActive(this.active);
      }
    }
    if (!this.active)
    {
      this.output = text;
      return false;
    }
    int num1 = Mathf.FloorToInt(time * 30f);
    if (num1 == this.previousStep)
      return false;
    this.previousStep = num1;
    float hue = Mathf.Repeat(time * 0.16f, 1f);
    StringBuilder stringBuilder = new StringBuilder();
    int num2 = 0;
    for (int index = 0; index < this.original.Length; ++index)
    {
      if (this.original[index] == '<')
      {
        int num3 = this.original.IndexOf('>', index);
        if (num3 >= 0)
        {
          stringBuilder.Append(this.original, index, num3 - index + 1);
          index = num3;
          continue;
        }
      }
      if (char.IsWhiteSpace(this.original[index]))
        stringBuilder.Append(this.original[index]);
      else
        stringBuilder.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(Color.HSVToRGB(Mathf.Repeat(hue + (float) num2++ * 0.06f, 1f), 0.8f, 1f))).Append('>').Append(this.original[index]).Append("</color>");
    }
    this.output = text = stringBuilder.ToString();
    if (this.paintBackground)
      this.PaintBackground(hue);
    this.AnimateBlocks(time, hue);
    return true;
  }
  private void AnimateBlocks(float time, float hue)
  {
    if (((Object) this.material == (Object) null))
      return;
    if (this.label is RectTransform label)
    {
      this.region = label.rect;
    }
    else
    {
      Bounds localBounds = ((Component) this.label).GetComponent<Renderer>().localBounds;
      this.region = new Rect((localBounds).min.x, (localBounds).min.y, (localBounds).size.x, (localBounds).size.y);
    }
    Renderer component1 = ((Component) this.background).GetComponent<Renderer>();
    if (((Object) component1 != (Object) null))
    {
      Bounds localBounds = component1.localBounds;
      Bounds bounds;
      bounds = new Bounds(this.label.InverseTransformPoint(((Component) component1).transform.TransformPoint((localBounds).center)), Vector3.zero);
      for (int index = 0; index < 8; ++index)
        (bounds).Encapsulate(this.label.InverseTransformPoint(((Component) component1).transform.TransformPoint(((localBounds).center + Vector3.Scale((localBounds).extents, new Vector3((index & 1) == 0 ? -1f : 1f, (index & 2) == 0 ? -1f : 1f, (index & 4) == 0 ? -1f : 1f))))));
      this.region = new Rect((bounds).min.x + (bounds).size.x * 0.05f, (bounds).min.y + (bounds).size.y * 0.12f, (bounds).size.x * 0.9f, (bounds).size.y * 0.76f);
    }
    this.size = Mathf.Min((this.region).width * 0.03f, (this.region).height * 0.15f);
    if ((double) this.size <= 0.0)
      return;
    float num1 = (double) this.previousTime < 0.0 ? 0.0f : Mathf.Clamp(time - this.previousTime, 0.0f, 0.05f);
    this.previousTime = time;
    for (int index1 = 0; index1 < this.blocks.Length; ++index1)
    {
      if (((Object) this.blocks[index1] == (Object) null))
      {
        GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
        ((Object) primitive).name = "RainbowPixel";
        primitive.layer = ((Component) this.label).gameObject.layer;
        Collider component2 = primitive.GetComponent<Collider>();
        component2.enabled = false;
        NativeUiSnapshot.Destroy((Object) component2);
        primitive.transform.SetParent(this.label, false);
        this.renderers[index1] = primitive.GetComponent<Renderer>();
        this.renderers[index1].sharedMaterial = this.material;
        this.blocks[index1] = primitive;
        this.positions[index1] = new Vector2(Mathf.Lerp((this.region).xMin + this.size, (this.region).xMax - this.size, ((float) index1 + 0.5f) / (float) this.blocks.Length), (this.region).yMax - this.size * (float) (1 + index1 % 3));
        this.velocities[index1] = new Vector2((float) ((index1 % 2 == 0 ? 1.0 : -1.0) * (double) (this.region).width * 0.14000000059604645), (float) (-(double) (this.region).height * 0.30000001192092896));
      }
      ref Vector2 local1 = ref this.velocities[index1];
      local1 = (local1 + (Vector2.down * (this.region).height * 1.5f * num1));
      ref Vector2 local2 = ref this.positions[index1];
      local2 = (local2 + (this.velocities[index1] * num1));
      if ((double) this.positions[index1].x < (double) (this.region).xMin + (double) this.size || (double) this.positions[index1].x > (double) (this.region).xMax - (double) this.size)
      {
        this.positions[index1].x = Mathf.Clamp(this.positions[index1].x, (this.region).xMin + this.size, (this.region).xMax - this.size);
        this.velocities[index1].x *= -1f;
      }
      if ((double) this.positions[index1].y < (double) (this.region).yMin + (double) this.size)
      {
        this.positions[index1].y = (this.region).yMin + this.size;
        this.velocities[index1].y = (this.region).height * (float) (1.1000000238418579 + (double) index1 * 0.039999999105930328);
      }
      if ((double) this.positions[index1].y > (double) (this.region).yMax - (double) this.size)
      {
        this.positions[index1].y = (this.region).yMax - this.size;
        this.velocities[index1].y = -Mathf.Abs(this.velocities[index1].y);
      }
      for (int index2 = 0; index2 < index1; ++index2)
      {
        Vector2 vector2_1 = (this.positions[index1] - this.positions[index2]);
        double magnitude = (double) (vector2_1).magnitude;
        if ((double) Mathf.Abs(vector2_1.x) < (double) this.size * 1.0499999523162842 && (double) Mathf.Abs(vector2_1.y) < (double) this.size * 1.0499999523162842)
        {
          Vector2 vector2_2 = (double) Mathf.Abs(vector2_1.x) > (double) Mathf.Abs(vector2_1.y) ? new Vector2((double) vector2_1.x < 0.0 ? -1f : 1f, 0.0f) : new Vector2(0.0f, (double) vector2_1.y < 0.0 ? -1f : 1f);
          Vector2 vector2_3 = (vector2_2 * (float) (((double) this.size * 1.0499999523162842 - (double) Mathf.Abs(Vector2.Dot(vector2_1, vector2_2))) * 0.5));
          ref Vector2 local3 = ref this.positions[index1];
          local3 = (local3 + vector2_3);
          ref Vector2 local4 = ref this.positions[index2];
          local4 = (local4 - vector2_3);
          float num2 = Vector2.Dot((this.velocities[index1] - this.velocities[index2]), vector2_2);
          if ((double) num2 < 0.0)
          {
            ref Vector2 local5 = ref this.velocities[index1];
            local5 = (local5 - (vector2_2 * num2));
            ref Vector2 local6 = ref this.velocities[index2];
            local6 = (local6 + (vector2_2 * num2));
          }
        }
      }
      this.blocks[index1].transform.localPosition = new Vector3(this.positions[index1].x, this.positions[index1].y, this.size * 0.05f);
      this.blocks[index1].transform.localScale = new Vector3(this.size, this.size, this.size * 0.03f);
      this.paint.SetColor("_Color", Color.HSVToRGB(Mathf.Repeat(hue + (float) index1 / (float) this.blocks.Length, 1f), 0.8f, 1f));
      this.renderers[index1].SetPropertyBlock(this.paint);
    }
  }
  private void PaintBackground(float hue)
  {
    Color rgb1 = Color.HSVToRGB(hue, 0.75f, 0.22f);
    Color rgb2 = Color.HSVToRGB(Mathf.Repeat(hue + 0.35f, 1f), 0.75f, 0.22f);
    foreach (KeyValuePair<Renderer, MaterialPropertyBlock> keyValuePair in this.savedPaint)
    {
      if (!((Object) keyValuePair.Key == (Object) null) && (!((Object) keyValuePair.Key.sharedMaterial.mainTexture != (Object) null) || !(((Object) keyValuePair.Key.sharedMaterial.mainTexture).name == "tel-aviv.png")))
      {
        keyValuePair.Key.GetPropertyBlock(this.paint);
        this.paint.SetColor("_Color", rgb1);
        this.paint.SetColor("_BaseColor", rgb1);
        this.paint.SetFloat("_UseGradient", 1f);
        this.paint.SetColor("_LeftColor", rgb1);
        this.paint.SetColor("_RightColor", rgb2);
        MaterialPropertyBlock paint1 = this.paint;
        Bounds bounds1 = keyValuePair.Key.bounds;
        Vector4 vector4_1 = ((Vector4)(bounds1).center);
        paint1.SetVector("_DesignOrigin", vector4_1);
        MaterialPropertyBlock paint2 = this.paint;
        Vector3 right = this.label.right;
        Bounds bounds2 = keyValuePair.Key.bounds;
        Vector3 size = (bounds2).size;
        double num = (double) Mathf.Max(1f / 1000f, (size).magnitude);
        Vector4 vector4_2 = ((Vector4)(right / (float) num));
        paint2.SetVector("_DesignAxis", vector4_2);
        keyValuePair.Key.SetPropertyBlock(this.paint);
      }
    }
    if (!((Object) this.backgroundGraphic != (Object) null))
      return;
    if (this.fill != (PropertyInfo) null)
    {
      this.fill.SetValue((object) this.backgroundGraphic, (object) rgb1);
      this.backgroundGraphic.color = Color.white;
    }
    else
      this.backgroundGraphic.color = rgb1;
  }
  private void RestoreBackground()
  {
    foreach (KeyValuePair<Renderer, MaterialPropertyBlock> keyValuePair in this.savedPaint)
    {
      if (((Object) keyValuePair.Key != (Object) null))
        keyValuePair.Key.SetPropertyBlock(keyValuePair.Value);
    }
    if (!((Object) this.backgroundGraphic != (Object) null))
      return;
    this.backgroundGraphic.color = this.graphicColor;
    if (!(this.fill != (PropertyInfo) null))
      return;
    this.fill.SetValue((object) this.backgroundGraphic, (object) this.fillColor);
  }
  public void Dispose()
  {
    this.RestoreBackground();
    foreach (GameObject block in this.blocks)
    {
      if (((Object) block != (Object) null))
        NativeUiSnapshot.Destroy((Object) block);
    }
    if (!((Object) this.material != (Object) null))
      return;
    NativeUiSnapshot.Destroy((Object) this.material);
  }
}
