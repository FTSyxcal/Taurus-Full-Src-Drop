
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class ImportedTheme
{
  private readonly List<ImportedTheme.Paint> paints = new List<ImportedTheme.Paint>();
  private readonly Dictionary<Renderer, bool> enabled = new Dictionary<Renderer, bool>();
  private string previousTheme;
  private uint previousLeft;
  private uint previousRight;
  private int previousStep = -1;
  private int revision;
  private int previousRevision = -1;
  private readonly Vector3 origin;
  private readonly Vector3 rightAxis;
  private readonly Vector3 upAxis;
  private readonly Transform source;
  private Matrix4x4 previousPose;
  public ImportedTheme(Transform source, Vector3 right, Vector3 up)
  {
    this.source = source;
    Bounds bounds1 = ImportedSurface.BoundsOf(source);
    float num1 = Mathf.Max(1f / 1000f, Vector3.Dot(ImportedTheme.Abs(right), (bounds1).size));
    float num2 = Mathf.Max(1f / 1000f, Vector3.Dot(ImportedTheme.Abs(up), (bounds1).size));
    this.origin = source.InverseTransformPoint((bounds1).center);
    Matrix4x4 matrix4x4_1 = source.localToWorldMatrix;
    matrix4x4_1 = (matrix4x4_1).transpose;
    this.rightAxis = (matrix4x4_1).MultiplyVector((right / num1));
    Matrix4x4 matrix4x4_2 = source.localToWorldMatrix;
    matrix4x4_2 = (matrix4x4_2).transpose;
    this.upAxis = (matrix4x4_2).MultiplyVector((up / num2));
    foreach (Renderer componentsInChild in ((Component) source).GetComponentsInChildren<Renderer>(true))
    {
      if (!((Object) ((Component) componentsInChild).GetComponent<TMP_Text>() != (Object) null) && !((Object) ((Component) componentsInChild).GetComponent<NativeUiSemanticColor>() != (Object) null) && !((Object) componentsInChild.sharedMaterial == (Object) null) && ImportedTheme.HasColor(componentsInChild.sharedMaterial) && !componentsInChild.sharedMaterial.HasProperty("_COLOR1"))
      {
        Bounds bounds2 = componentsInChild.bounds;
        Vector3 vector3_1 = ((bounds2).center - (bounds1).center);
        foreach (Material sharedMaterial in componentsInChild.sharedMaterials)
        {
          if (((Object) sharedMaterial != (Object) null) && ImportedTheme.HasColor(sharedMaterial))
          {
            NativeUiThemeRole component = ((Component) componentsInChild).GetComponent<NativeUiThemeRole>();
            int num3;
            if ((!((Object) component != (Object) null) || !component.Background) && !((Object) ((Component) componentsInChild).transform == (Object) source) && !(((Object) componentsInChild).name == "Back") && !(((Object) componentsInChild).name == "TopBlock") && !(((Object) componentsInChild).name == "SideBlock"))
            {
              Vector3 vector3_2 = ImportedTheme.Abs(right);
              bounds2 = componentsInChild.bounds;
              Vector3 size1 = (bounds2).size;
              if ((double) Vector3.Dot(vector3_2, size1) >= (double) num1 * 0.800000011920929)
              {
                Vector3 vector3_3 = ImportedTheme.Abs(up);
                bounds2 = componentsInChild.bounds;
                Vector3 size2 = (bounds2).size;
                num3 = (double) Vector3.Dot(vector3_3, size2) >= (double) num2 * 0.699999988079071 ? 1 : 0;
              }
              else
                num3 = 0;
            }
            else
              num3 = 1;
            bool flag = num3 != 0;
            this.paints.Add(new ImportedTheme.Paint()
            {
              Renderer = componentsInChild,
              Material = sharedMaterial,
              PreserveSurface = ((Object) component != (Object) null) && !component.Accent && !component.Background,
              Original = sharedMaterial.color,
              Opacity = sharedMaterial.GetTag("RenderType", false) == "Opaque" ? 1f : sharedMaterial.color.a,
              Ink = componentsInChild.sharedMaterials.Length > 1 && (double) Mathf.Min(sharedMaterial.color.r, Mathf.Min(sharedMaterial.color.g, sharedMaterial.color.b)) > 0.800000011920929,
              Position = new Vector2(Vector3.Dot(vector3_1, right) / num1, Vector3.Dot(vector3_1, up) / num2),
              Accent = ((Object) component != (Object) null) ? component.Accent : ((Object) componentsInChild).name.IndexOf("outline", StringComparison.OrdinalIgnoreCase) >= 0,
              Background = flag
            });
          }
        }
      }
    }
    foreach (Graphic componentsInChild in ((Component) source).GetComponentsInChildren<Graphic>(true))
    {
      if (!((Object) ((Component) componentsInChild).GetComponent<NativeUiSemanticColor>() != (Object) null))
      {
        Type type = componentsInChild.GetType();
        PropertyInfo property1 = type.GetProperty("FillColor");
        PropertyInfo property2 = type.GetProperty("StrokeColor");
        PropertyInfo property3 = type.GetProperty("FillGradient");
        PropertyInfo property4 = type.GetProperty("Fill");
        PropertyInfo property5 = type.GetProperty("GradientHandlePositions");
        Vector3 vector3 = (((Component) componentsInChild).transform.position - (bounds1).center);
        Color color = componentsInChild is TMP_Text tmpText ? ((Graphic) tmpText).color : (property1 == (PropertyInfo) null ? componentsInChild.color : (Color) property1.GetValue((object) componentsInChild));
        bool flag = false;
        if (property1 != (PropertyInfo) null && ((Object) ((Component) componentsInChild).transform.parent != (Object) null))
        {
          foreach (Component component1 in ((Component) componentsInChild).transform.parent)
          {
            Image component2 = component1.GetComponent<Image>();
            if (component2 != null && component2.GetType().Name != "FigmaImage")
              flag = true;
          }
        }
        List<ImportedTheme.Paint> paints = this.paints;
        ImportedTheme.Paint paint = new ImportedTheme.Paint();
        paint.Graphic = componentsInChild;
        paint.Original = color;
        paint.Opacity = !(property3 != (PropertyInfo) null) || !(property4 != (PropertyInfo) null) || Convert.ToInt32(property4.GetValue((object) componentsInChild)) == 0 || !(property3.GetValue((object) componentsInChild) is Gradient gradient) ? color.a : gradient.Evaluate(0.5f).a;
        paint.Vertex = componentsInChild.color;
        paint.IconPlate = flag;
        paint.Stroke = property2 == (PropertyInfo) null ? Color.clear : (Color) property2.GetValue((object) componentsInChild);
        paint.Fill = property1;
        paint.StrokeProperty = property2;
        paint.GradientProperty = property3;
        paint.StyleProperty = property4;
        paint.Style = property4?.GetValue((object) componentsInChild);
        paint.HandlesProperty = property5;
        paint.Handles = property5?.GetValue((object) componentsInChild) as Vector2[];
        paint.Gradient = property3?.GetValue((object) componentsInChild) as Gradient;
        int num4;
        switch (componentsInChild)
        {
          case Text _:
          case TMP_Text _:
            num4 = 1;
            break;
          case Image image:
            if (image.type == null)
            {
              num4 = ((Object) image.sprite != (Object) null) ? 1 : 0;
              break;
            }
            goto default;
          default:
            num4 = 0;
            break;
        }
        paint.Text = num4 != 0;
        paint.Accent = (double) (color).maxColorComponent - (double) Mathf.Min(color.r, Mathf.Min(color.g, color.b)) > 0.11999999731779099;
        paint.Background = ((Object) componentsInChild).name == "Background" && ((Object) ((Component) componentsInChild).transform.parent).name == "Menu" || ((Object) componentsInChild).name == "OriginalUntitledKeyboard" || ((Object) componentsInChild).name == "KeyboardUi" || ((Object) componentsInChild).name == "k" && ((Object) ((Component) componentsInChild).transform.parent).name == "CanvasObject" && (double) Mathf.Abs(componentsInChild.rectTransform.sizeDelta.y - 560f) < 1.0 || ((Object) componentsInChild).name == "f" && ((Object) ((Component) componentsInChild).transform.parent.parent).name == "CanvasObject" && (double) Mathf.Abs(((Component) ((Component) componentsInChild).transform.parent).GetComponent<RectTransform>().sizeDelta.y - 560f) < 1.0;
        paint.Position = new Vector2(Vector3.Dot(vector3, right) / num1, Vector3.Dot(vector3, up) / num2);
        paints.Add(paint);
      }
    }
  }
  private static Vector3 Abs(Vector3 value)
  {
    return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
  }
  private static bool HasColor(Material material)
  {
    return material.HasProperty("_Color") || material.HasProperty("_BaseColor");
  }
  public void SetEnabled(Renderer renderer, bool value)
  {
    bool flag;
    if (this.enabled.TryGetValue(renderer, out flag) && flag == value)
      return;
    this.enabled[renderer] = value;
    ++this.revision;
  }
  public bool Apply(MenuState state, float time)
  {
    bool original = state.Theme == "ItzGame";
    int num1 = (original ? 0 : (state.Theme == "Rainbow" ? 1 : (!state.IsOn("Freeze gradient") ? 1 : 0))) != 0 ? Mathf.FloorToInt(time * 30f) : 0;
    Matrix4x4 localToWorldMatrix = this.source.localToWorldMatrix;
    if (this.previousTheme == state.Theme && (int) this.previousLeft == (int) state.GradientStart && (int) this.previousRight == (int) state.GradientEnd && this.previousStep == num1 && this.previousRevision == this.revision && (original || (localToWorldMatrix == this.previousPose)))
      return false;
    this.previousPose = localToWorldMatrix;
    this.previousTheme = state.Theme;
    this.previousLeft = state.GradientStart;
    this.previousRight = state.GradientEnd;
    this.previousStep = num1;
    this.previousRevision = this.revision;
    Color accent1 = MenuPalette.Left(state, (float) num1 / 30f);
    Color accent2 = MenuPalette.Right(state, (float) num1 / 30f);
    Color background = MenuPalette.Background(state, time);
    float num2 = state.IsOn("Freeze gradient") ? 0.0f : (float) ((double) num1 / 30.0 * 3.1415927410125732 * 2.0) / state.GradientPeriod;
    Matrix4x4 worldToLocalMatrix = this.source.worldToLocalMatrix;
    Matrix4x4 transpose = (worldToLocalMatrix).transpose;
    Vector3 vector3 = (transpose).MultiplyVector(((this.rightAxis * Mathf.Cos(num2)) + (this.upAxis * Mathf.Sin(num2))));
    foreach (ImportedTheme.Paint paint in this.paints)
    {
      if (!((Object) paint.Graphic == (Object) null) || !((Object) paint.Material == (Object) null))
      {
        float num3 = Mathf.Clamp01((float) (0.5 + (double) paint.Position.x * (double) Mathf.Cos(num2) + (double) paint.Position.y * (double) Mathf.Sin(num2)));
        Color accent3 = Color.Lerp(accent1, accent2, num3);
        Color color1 = this.Tint(paint, accent3, background, original);
        if (((Object) paint.Graphic != (Object) null))
        {
          if (paint.Fill != (PropertyInfo) null)
          {
            paint.Graphic.color = original ? paint.Vertex : Color.white;
            paint.Fill.SetValue((object) paint.Graphic, (object) (original ? color1 : Color.white));
            if (paint.StrokeProperty != (PropertyInfo) null)
              paint.StrokeProperty.SetValue((object) paint.Graphic, (object) (original ? paint.Stroke : new Color(accent3.r, accent3.g, accent3.b, paint.Stroke.a)));
            if (paint.GradientProperty != (PropertyInfo) null && paint.StyleProperty != (PropertyInfo) null)
            {
              Color color2 = this.Tint(paint, accent1, background, original);
              Color color3 = this.Tint(paint, accent2, background, original);
              paint.Keys[0] = new GradientColorKey(color2, 0.0f);
              paint.Keys[1] = new GradientColorKey(color3, 1f);
              paint.Alphas[0] = new GradientAlphaKey(color2.a, 0.0f);
              paint.Alphas[1] = new GradientAlphaKey(color3.a, 1f);
              paint.EditedGradient.SetKeys(paint.Keys, paint.Alphas);
              paint.StyleProperty.SetValue((object) paint.Graphic, original ? paint.Style : Enum.ToObject(paint.StyleProperty.PropertyType, 1));
              paint.GradientProperty.SetValue((object) paint.Graphic, original ? (object) paint.Gradient : (object) paint.EditedGradient);
              if (paint.HandlesProperty != (PropertyInfo) null)
              {
                Vector2 vector2 = (new Vector2(Mathf.Cos(num2), Mathf.Sin(num2)) * 0.5f);
                paint.EditedHandles[0] = ((Vector2.one * 0.5f) - vector2);
                paint.EditedHandles[1] = ((Vector2.one * 0.5f) + vector2);
                paint.EditedHandles[2] = ((Vector2.one * 0.5f) + new Vector2(-vector2.y, vector2.x));
                paint.HandlesProperty.SetValue((object) paint.Graphic, original ? (object) paint.Handles : (object) paint.EditedHandles);
              }
            }
            paint.Graphic.SetMaterialDirty();
          }
          else if (paint.Graphic is TMP_Text graphic)
            ((Graphic) graphic).color = color1;
          else
            paint.Graphic.color = color1;
        }
        else
        {
          paint.Material.color = color1;
          if (paint.Material.HasProperty("_UseGradient"))
          {
            paint.Material.SetFloat("_UseGradient", original || paint.Ink || paint.PreserveSurface ? 0.0f : 1f);
            paint.Material.SetColor("_LeftColor", this.Tint(paint, accent1, background, false));
            paint.Material.SetColor("_RightColor", this.Tint(paint, accent2, background, false));
            paint.Material.SetVector("_DesignOrigin", ((Vector4)this.source.TransformPoint(this.origin)));
            paint.Material.SetVector("_DesignAxis", ((Vector4)vector3));
          }
        }
      }
    }
    return true;
  }
  private Color Tint(ImportedTheme.Paint paint, Color accent, Color background, bool original)
  {
    Color color = original || paint.PreserveSurface ? paint.Original : (paint.Text || paint.Ink ? Color.white : (paint.IconPlate ? Color.Lerp(accent, Color.white, 0.6f) : (paint.Accent ? accent : Color.Lerp(background, accent, paint.Background ? 0.07f : 0.23f))));
    bool flag = false;
    if (((!((Object) paint.Renderer != (Object) null) || paint.Ink ? 0 : (this.enabled.TryGetValue(paint.Renderer, out flag) ? 1 : 0)) & (flag ? 1 : 0)) != 0)
      color = Color.Lerp(color, original ? Color.gray : Color.white, original ? 1f : 0.18f);
    color.a = original ? paint.Original.a : (paint.Text ? 1f : paint.Opacity);
    return color;
  }
  private sealed class Paint
  {
    public Renderer Renderer;
    public Material Material;
    public Graphic Graphic;
    public Color Original;
    public Color Stroke;
    public Color Vertex;
    public PropertyInfo Fill;
    public PropertyInfo StrokeProperty;
    public PropertyInfo GradientProperty;
    public PropertyInfo StyleProperty;
    public PropertyInfo HandlesProperty;
    public Gradient Gradient;
    public Gradient EditedGradient = new Gradient();
    public object Style;
    public Vector2[] Handles;
    public readonly GradientColorKey[] Keys = new GradientColorKey[2];
    public readonly GradientAlphaKey[] Alphas = new GradientAlphaKey[2];
    public readonly Vector2[] EditedHandles = new Vector2[3];
    public Vector2 Position;
    public float Opacity;
    public bool Text;
    public bool Accent;
    public bool Background;
    public bool Ink;
    public bool IconPlate;
    public bool PreserveSurface;
  }
}
