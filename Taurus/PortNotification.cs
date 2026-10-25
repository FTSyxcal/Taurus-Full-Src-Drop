
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class PortNotification : IDisposable
{
  private readonly Transform proxy;
  private readonly GameObject panel;
  private readonly TMP_Text tmp;
  private readonly Text legacy;
  private readonly Color original;
  private string previous;
  private string previousTheme;
  private bool visible;
  private readonly List<Material> materials = new List<Material>();
  public PortNotification(
    Transform proxy,
    Transform source,
    Transform template,
    Component fontTemplate,
    Bounds region,
    Vector3 forward,
    Vector3 up,
    bool below = false)
  {
    this.proxy = proxy;
    this.panel = new GameObject(nameof (PortNotification), new Type[1]
    {
      typeof (Canvas)
    });
    this.panel.SetActive(false);
    this.panel.transform.SetParent(source, false);
    this.panel.GetComponent<Canvas>().renderMode = (RenderMode) 2;
    this.panel.GetComponent<Canvas>().sortingOrder = 200;
    this.panel.GetComponent<Canvas>().scaleFactor = 1f;
    this.panel.transform.rotation = Quaternion.LookRotation(forward, up);
    NativeUiClick component1 = ((Component) template).GetComponent<NativeUiClick>();
    List<Transform> transformList = ((Object) component1 == (Object) null) ? new List<Transform>() : new List<Transform>(component1.Visuals);
    if (transformList.Count == 0)
      transformList.Add(template);
    Material material1 = (Material) null;
    foreach (Graphic componentsInChild in ((Component) template).GetComponentsInChildren<Graphic>(true))
    {
      if (componentsInChild.GetType().Name == "FigmaImage")
        material1 = componentsInChild.GetType().GetField("m_DynamicMaterial", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue((object) componentsInChild) as Material;
    }
    foreach (Transform transform1 in transformList)
    {
      bool flag = false;
      foreach (Transform transform2 in transformList)
      {
        if (((Object) transform2 != (Object) transform1) && transform1.IsChildOf(transform2))
          flag = true;
      }
      if (!flag)
        Object.Instantiate<GameObject>(((Component) transform1).gameObject, this.panel.transform, true).SetActive(true);
    }
    foreach (Graphic componentsInChild in this.panel.GetComponentsInChildren<Graphic>(true))
    {
      if (componentsInChild.GetType().Name == "FigmaImage" && ((Object) material1 != (Object) null))
      {
        Material material2 = new Material(material1);
        this.materials.Add(material2);
        componentsInChild.GetType().GetField("m_DynamicMaterial", BindingFlags.Instance | BindingFlags.NonPublic).SetValue((object) componentsInChild, (object) material2);
      }
    }
    foreach (Collider componentsInChild in this.panel.GetComponentsInChildren<Collider>(true))
      componentsInChild.enabled = false;
    this.tmp = this.panel.GetComponentInChildren<TMP_Text>(true);
    this.legacy = ((Object) this.tmp == (Object) null) ? this.panel.GetComponentInChildren<Text>(true) : (Text) null;
    if (((Object) this.tmp == (Object) null) && ((Object) this.legacy == (Object) null) && ((Object) fontTemplate != (Object) null))
    {
      GameObject gameObject = Object.Instantiate<GameObject>(fontTemplate.gameObject, this.panel.transform, true);
      this.tmp = gameObject.GetComponent<TMP_Text>();
      this.legacy = gameObject.GetComponent<Text>();
      Transform transform = gameObject.transform;
      Bounds bounds = ImportedSurface.BoundsOf(template);
      Vector3 vector3 = ((bounds).center - (forward * 0.005f));
      transform.position = vector3;
    }
    foreach (TMP_Text componentsInChild in this.panel.GetComponentsInChildren<TMP_Text>(true))
      ((Component) componentsInChild).gameObject.SetActive(((Object) componentsInChild == (Object) this.tmp));
    foreach (Text componentsInChild in this.panel.GetComponentsInChildren<Text>(true))
      ((Component) componentsInChild).gameObject.SetActive(((Object) componentsInChild == (Object) this.legacy));
    foreach (Transform componentsInChild in this.panel.GetComponentsInChildren<Transform>(true))
    {
      ((Component) componentsInChild).gameObject.layer = 31 ;
      if (((Object) componentsInChild).name.Contains("Checkbox") || ((Object) componentsInChild).name.Contains("Favorite") || ((Object) componentsInChild).name.Contains("TaurusPlus") || ((Object) componentsInChild).name.Contains("TaurusMinus"))
        ((Component) componentsInChild).gameObject.SetActive(false);
    }
    if (((Object) this.tmp != (Object) null))
    {
      this.original = ((Graphic) this.tmp).color;
      this.tmp.enableAutoSizing = true;
      this.tmp.textWrappingMode = (TextWrappingModes) 1;
      this.tmp.overflowMode = (TextOverflowModes) 1;
      this.tmp.alignment = (TextAlignmentOptions) 514;
      ((Component) this.tmp).gameObject.AddComponent<NativeUiSemanticColor>();
    }
    if (((Object) this.legacy != (Object) null))
    {
      this.original = ((Graphic) this.legacy).color;
      this.legacy.resizeTextForBestFit = true;
      this.legacy.alignment = (TextAnchor) 4;
      this.legacy.horizontalOverflow = (HorizontalWrapMode) 0;
    }
    Canvas.ForceUpdateCanvases();
    Bounds box1 = PortNotification.BodyBounds(this.panel.transform);
    Vector3 vector3_1 = Vector3.Cross(up, forward);
    Vector3 normalized = (vector3_1).normalized;
    float num1 = Width(region, up) * 0.11f;
    float num2 = Mathf.Min(Width(region, normalized) * 0.94f / Mathf.Max(0.0001f, Width(box1, normalized)), num1 / Mathf.Max(0.0001f, Width(box1, up)));
    Transform transform3 = this.panel.transform;
    transform3.localScale = (transform3.localScale * num2);
    Bounds box2 = PortNotification.BodyBounds(this.panel.transform);
    this.panel.transform.localScale = Vector3.Scale(this.panel.transform.localScale, new Vector3(Width(region, normalized) * 0.94f / Mathf.Max(0.0001f, Width(box2, normalized)), 1f, 1f));
    Bounds bounds1 = PortNotification.BodyBounds(this.panel.transform);
    Vector3 vector3_2 = (((region).center - (up * (float) ((double) Width(region, up) * 0.5 + (below ? (double) num1 * 0.64999997615814209 : -(double) num1 * 0.5)))) - (forward * 0.025f));
    Transform transform4 = this.panel.transform;
    transform4.position = (transform4.position + (vector3_2 - (bounds1).center));
    RectTransform rectTransform1 = ((Object) this.tmp != (Object) null) ? this.tmp.rectTransform : (RectTransform) null;
    Vector3 vector3_3;
    if (((Object) rectTransform1 != (Object) null))
    {
      float y = this.tmp.GetPreferredValues("Ag").y;
      float num3 = num1 * 0.65f / Mathf.Max(0.0001f, y);
      ((Transform) rectTransform1).SetParent(this.panel.transform, true);
      ((Transform) rectTransform1).rotation = Quaternion.LookRotation(forward, up);
      RectTransform rectTransform2 = rectTransform1;
      double num4 = (double) num3;
      vector3_3 = this.panel.transform.TransformVector(Vector3.right);
      double magnitude1 = (double) (vector3_3).magnitude;
      double num5 = num4 / magnitude1;
      double num6 = (double) num3;
      vector3_3 = this.panel.transform.TransformVector(Vector3.up);
      double magnitude2 = (double) (vector3_3).magnitude;
      double num7 = num6 / magnitude2;
      double num8 = (double) num3;
      vector3_3 = this.panel.transform.TransformVector(Vector3.forward);
      double magnitude3 = (double) (vector3_3).magnitude;
      double num9 = num8 / magnitude3;
      Vector3 vector3_4 = new Vector3((float) num5, (float) num7, (float) num9);
      ((Transform) rectTransform2).localScale = vector3_4;
      ((Transform) rectTransform1).position = (vector3_2 - (forward * 3f / 500f));
      Bounds box3 = new Bounds();
      bool flag = true;
      foreach (Renderer componentsInChild in this.panel.GetComponentsInChildren<Renderer>(true))
      {
        if (((Object) ((Component) componentsInChild).GetComponent<TMP_Text>() == (Object) null))
        {
          if (flag)
          {
            box3 = componentsInChild.bounds;
            flag = false;
          }
          else
            (box3).Encapsulate(componentsInChild.bounds);
        }
      }
      if (!flag)
        ((Transform) rectTransform1).position = ((box3).center - (forward * (float) ((double) Width(box3, forward) * 0.5 + 1.0 / 1000.0)));
      rectTransform1.sizeDelta = new Vector2(Width(region, normalized) * 0.86f / num3, num1 * 0.8f / num3);
    }
    if (((Object) this.legacy != (Object) null))
    {
      Text legacy = this.legacy;
      ((Component) legacy).gameObject.SetActive(false);
      GameObject gameObject1 = new GameObject("NoticeCanvas", new Type[2]
      {
        typeof (RectTransform),
        typeof (Canvas)
      });
      gameObject1.layer = 31 ;
      gameObject1.transform.SetParent(this.panel.transform, false);
      gameObject1.transform.SetPositionAndRotation((vector3_2 - (forward * 0.012f)), Quaternion.LookRotation(forward, up));
      Quaternion localRotation = gameObject1.transform.localRotation;
      Transform transform5 = gameObject1.transform;
      vector3_3 = this.panel.transform.TransformVector((localRotation * Vector3.right));
      double num10 = 1.0 / 1000.0 / (double) (vector3_3).magnitude;
      vector3_3 = this.panel.transform.TransformVector((localRotation * Vector3.up));
      double num11 = 1.0 / 1000.0 / (double) (vector3_3).magnitude;
      vector3_3 = this.panel.transform.TransformVector((localRotation * Vector3.forward));
      double num12 = 1.0 / 1000.0 / (double) (vector3_3).magnitude;
      Vector3 vector3_5 = new Vector3((float) num10, (float) num11, (float) num12);
      transform5.localScale = vector3_5;
      Canvas component2 = gameObject1.GetComponent<Canvas>();
      component2.renderMode = (RenderMode) 2;
      component2.overrideSorting = true;
      component2.sortingOrder = 201;
      component2.scaleFactor = 1f;
      component2.referencePixelsPerUnit = 100f;
      GameObject gameObject2 = new GameObject("NativeNoticeText", new Type[3]
      {
        typeof (RectTransform),
        typeof (CanvasRenderer),
        typeof (Text)
      });
      gameObject2.layer = 31 ;
      gameObject2.transform.SetParent(gameObject1.transform, false);
      this.legacy = gameObject2.GetComponent<Text>();
      this.legacy.font = legacy.font;
      this.legacy.fontStyle = legacy.fontStyle;
      ((Graphic) this.legacy).color = this.original;
      this.legacy.fontSize = 26;
      this.legacy.resizeTextForBestFit = true;
      this.legacy.resizeTextMinSize = 10;
      this.legacy.resizeTextMaxSize = 26;
      this.legacy.alignment = (TextAnchor) 4;
      this.legacy.horizontalOverflow = (HorizontalWrapMode) 0;
      this.legacy.verticalOverflow = (VerticalWrapMode) 0;
      ((Graphic) this.legacy).raycastTarget = false;
      gameObject2.AddComponent<NativeUiSemanticColor>();
      ((Graphic) this.legacy).rectTransform.sizeDelta = new Vector2((float) ((double) Width(region, normalized) * 0.86000001430511475 / (1.0 / 1000.0)), (float) ((double) num1 * 0.800000011920929 / (1.0 / 1000.0)));
    }
    this.panel.SetActive(false);
    static float Width(Bounds box, Vector3 axis)
    {
      return Vector3.Dot(new Vector3(Mathf.Abs(axis.x), Mathf.Abs(axis.y), Mathf.Abs(axis.z)), (box).size);
    }
  }
  public bool Refresh(MenuState state)
  {
    if (state == null)
      return false;
    bool activeSelf = ((Component) this.proxy.Find("NotificationBackground")).gameObject.activeSelf;
    TextMesh component = ((Component) this.proxy.Find("PageLabel")).GetComponent<TextMesh>();
    string text = component.text;
    if (activeSelf == this.visible && text == this.previous && state.Theme == this.previousTheme)
      return false;
    this.visible = activeSelf;
    this.previous = text;
    this.previousTheme = state.Theme;
    this.panel.SetActive(activeSelf);
    if (((Object) this.tmp != (Object) null))
    {
      this.tmp.text = text;
      ((Graphic) this.tmp).color = (component.color == MenuStyle.Error) ? MenuStyle.Error : this.original;
    }
    if (((Object) this.legacy != (Object) null))
    {
      this.legacy.text = text;
      ((Graphic) this.legacy).color = (component.color == MenuStyle.Error) ? MenuStyle.Error : (state.Theme == "ItzGame" ? this.original : Color.white);
    }
    return true;
  }
  private static Bounds BodyBounds(Transform root)
  {
    Bounds bounds = new Bounds();
    bool flag = true;
    foreach (Renderer componentsInChild in ((Component) root).GetComponentsInChildren<Renderer>(true))
    {
      if (!((Object) ((Component) componentsInChild).GetComponent<TMP_Text>() != (Object) null) && !((Object) ((Component) componentsInChild).GetComponent<TextMesh>() != (Object) null))
      {
        if (flag)
        {
          bounds = componentsInChild.bounds;
          flag = false;
        }
        else
          (bounds).Encapsulate(componentsInChild.bounds);
      }
    }
    foreach (Graphic componentsInChild in ((Component) root).GetComponentsInChildren<Graphic>(true))
    {
      switch (componentsInChild)
      {
        case Text _:
        case TMP_Text _:
          continue;
        default:
          Vector3[] vector3Array = new Vector3[4];
          componentsInChild.rectTransform.GetWorldCorners(vector3Array);
          foreach (Vector3 vector3 in vector3Array)
          {
            if (flag)
            {
              bounds = new Bounds(vector3, Vector3.zero);
              flag = false;
            }
            else
              (bounds).Encapsulate(vector3);
          }
          continue;
      }
    }
    return !flag ? bounds : ImportedSurface.BoundsOf(root);
  }
  public void Dispose()
  {
    foreach (Object material in this.materials)
      NativeUiSnapshot.Destroy(material);
  }
}
