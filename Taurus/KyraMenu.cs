
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class KyraMenu : IDisposable
{
  public const float Unit = 0.0003147023f;
  private readonly Transform root;
  private readonly Transform visual;
  private readonly Func<string, Vector3, float, int, Mesh> shape;
  private readonly Renderer[] originals;
  private readonly Dictionary<string, Transform> controls = new Dictionary<string, Transform>();
  private readonly List<KyraMenu.Control> bindings = new List<KyraMenu.Control>();
  private readonly List<Material> materials = new List<Material>();
  private readonly List<Mesh> icons = new List<Mesh>();
  private readonly Text[] labels = new Text[6];
  private readonly Text[] details = new Text[6];
  private readonly Transform[] knobs = new Transform[6];
  private readonly Transform[] tracks = new Transform[6];
  private readonly Transform[] contents = new Transform[6];
  private readonly Vector3[] knobOff = new Vector3[6];
  private readonly Vector3[] labelPositions = new Vector3[6];
  private readonly Material[] trackMaterials = new Material[6];
  private readonly bool[] toggleTargets = new bool[6];
  private readonly float[] toggles = new float[6];
  private readonly MenuFontView fontView;
  private readonly GameObject noticeProxy;
  private readonly TextMesh sourceNotice;
  private readonly Material solid;
  private readonly Font font;
  private Transform keyboard;
  private Text query;
  private Text homeText;
  private Text notice;
  private Transform homeIcon;
  private Transform noticeBody;
  private MenuState state;
  private bool active;
  private float lastTime;
  private float pageAt;
  private float slideDirection = 1f;
  private bool sliding;
  private const float SlideDuration = 0.16f;
  private const float SlideDistance = 8f;
  private static readonly Color TrackOff = MenuPalette.Unpack(2368548U );
  private static readonly Color TrackOn = MenuPalette.Unpack(27270U);
  private int page = -1;
  private int category = -1;
  public Transform Visual => this.visual;
  public KyraMenu(Transform root, Func<string, Vector3, float, int, Mesh> shape)
  {
    this.root = root;
    this.shape = shape;
    this.originals = ((Component) root).GetComponentsInChildren<Renderer>(true);
    this.solid = ((Component) root.Find("Background")).GetComponent<Renderer>().sharedMaterial;
    this.font = Font.CreateDynamicFontFromOSFont("Arial", 38);
    GameObject gameObject = new GameObject("Kyra3D", new Type[2]
    {
      typeof (RectTransform),
      typeof (Canvas)
    });
    gameObject.transform.SetParent(root, false);
    this.visual = gameObject.transform;
    gameObject.GetComponent<Canvas>().renderMode = (RenderMode) 2;
    gameObject.GetComponent<Canvas>().sortingOrder = 100;
    this.Body("Frame", 39f, 53f, 665f, 789f, 29f, 0.04f, 0.018f, 1052688U );
    this.Body("Inner", 52f, 67f, 638f, 762f, 26f, 0.051f, 0.008f, 1052688U );
    this.Body("SidebarFrame", 725f, 119f, 137f, 658f, 29f, 0.04f, 0.018f, 1052688U );
    this.Body("SidebarInner", 735f, 130f, 116f, 634f, 23f, 0.051f, 0.008f, 1052688U );
    for (int index = 0; index < 6; ++index)
    {
      float y = (float) (67.0 + (double) index * 112.0);
      Transform transform = this.Body("Row" + index.ToString(), 52f, y, 638f, 97f, index == 0 ? 26f : 0.0f, 0.056f, 0.008f, 1315860U , 3);
      this.controls["Row" + index.ToString()] = transform;
      this.labels[index] = this.Label("Caption" + index.ToString(), 73f, y, 488f, 97f, 38, (TextAnchor) 3);
      ((Component) this.labels[index]).transform.SetParent(transform, true);
      MenuButtonEffects.Bind(((Component) this.labels[index]).transform, transform, false);
      KyraMenu.OnFace(((Component) this.labels[index]).transform, transform);
      this.details[index] = this.Label("Detail" + index.ToString(), 73f, y + 55f, 480f, 32f, 17, (TextAnchor) 3);
      ((Component) this.details[index]).transform.SetParent(transform, true);
      KyraMenu.OnFace(((Component) this.details[index]).transform, transform);
      this.tracks[index] = this.Body("Track" + index.ToString(), 578f, y + 26f, 90f, 46f, 23f, 0.063f, 0.004f, 2368548U );
      this.tracks[index].SetParent(transform, true);
      this.knobs[index] = this.Body("Knob" + index.ToString(), 584f, y + 33f, 33f, 33f, 16.5f, 0.067f, 0.004f, 14277081U );
      this.knobs[index].SetParent(transform, true);
      this.contents[index] = new GameObject("PageContent").transform;
      this.contents[index].SetParent(transform, false);
      ((Component) this.labels[index]).transform.SetParent(this.contents[index], true);
      ((Component) this.details[index]).transform.SetParent(this.contents[index], true);
      this.tracks[index].SetParent(this.contents[index], true);
      this.knobs[index].SetParent(this.contents[index], true);
      this.knobOff[index] = this.knobs[index].localPosition;
      this.labelPositions[index] = ((Component) this.labels[index]).transform.localPosition;
      this.trackMaterials[index] = ((Component) this.tracks[index]).GetComponent<Renderer>().sharedMaterial;
    }
    string[] strArray = new string[5]
    {
      "DesignLogo",
      "ButtonHome",
      "DesignSettings",
      "ButtonSearch",
      "ButtonDisconnect"
    };
    Rect[] rectArray = new Rect[5]
    {
      new Rect(758f, 155f, 68f, 60f),
      new Rect(759f, 281f, 67f, 69f),
      new Rect(759f, 410f, 68f, 73f),
      new Rect(758f, 542f, 69f, 65f),
      new Rect(760f, 671f, 64f, 65f)
    };
    for (int index = 0; index < strArray.Length; ++index)
    {
      Transform transform1 = this.Body(strArray[index], 735f, (float) (130.0 + (double) index * 129.0), 116f, 117f, index == 0 || index == 4 ? 23f : 0.0f, 0.056f, 0.008f, 1315860U , index == 0 ? 3 : 12);
      this.controls[strArray[index]] = transform1;
      Transform transform2 = this.Icon(strArray[index] + "Icon", rectArray[index], index == 0, 0.058f);
      transform2.SetParent(transform1, true);
      if (index == 1)
        this.homeIcon = transform2;
    }
    this.controls["ButtonPrevious"] = this.Body("ButtonPrevious", 52f, 739f, 314f, 90f, 26f, 0.056f, 0.008f, 1315860U , 8);
    this.controls["ButtonNext"] = this.Body("ButtonNext", 374f, 739f, 314f, 90f, 26f, 0.056f, 0.008f, 1315860U , 4);
    this.Icon("PreviousIcon", new Rect(198f, 769f, 19f, 30f), false, 0.058f).SetParent(this.controls["ButtonPrevious"], true);
    this.Icon("NextIcon", new Rect(517f, 768f, 25f, 30f), false, 0.058f).SetParent(this.controls["ButtonNext"], true);
    this.homeText = this.Label("PickerAdvance", 735f, 259f, 116f, 117f, 25, (TextAnchor) 4);
    ((Component) this.homeText).gameObject.SetActive(false);
    ((Component) this.homeText).transform.SetParent(this.controls["ButtonHome"], true);
    KyraMenu.OnFace(((Component) this.homeText).transform, this.controls["ButtonHome"]);
    this.noticeBody = this.Body("Notification", 52f, 850f, 638f, 45f, 14f, 0.056f, 0.008f, 1315860U );
    this.notice = this.Label("NativeNoticeText", 70f, 850f, 602f, 45f, 26, (TextAnchor) 4);
    ((Component) this.notice).gameObject.AddComponent<NativeUiSemanticColor>();
    ((Component) this.notice).transform.SetParent(this.noticeBody, true);
    KyraMenu.OnFace(((Component) this.notice).transform, this.noticeBody);
    this.noticeProxy = ((Component) root.Find("NotificationBackground")).gameObject;
    this.sourceNotice = ((Component) root.Find("PageLabel")).GetComponent<TextMesh>();
    this.BuildKeyboard();
    foreach (KeyValuePair<string, Transform> control in this.controls)
      this.CacheControl(control.Key, control.Value);
    this.fontView = ((Component) this.visual).gameObject.AddComponent<MenuFontView>();
    foreach (Component componentsInChild in ((Component) this.visual).GetComponentsInChildren<Transform>(true))
      componentsInChild.gameObject.layer = ((Component) root).gameObject.layer;
    this.SetActive(false);
  }
  private static Vector3 Point(float x, float y, float depth)
  {
    return new Vector3(depth, (float) ((458.5 - (double) x) * 0.00031470230896957219), (float) ((448.5 - (double) y) * 0.00031470230896957219));
  }
  private Transform Body(
    string name,
    float x,
    float y,
    float width,
    float height,
    float radius,
    float depth,
    float thickness,
    uint color,
    int cornerMask = 15)
  {
    if (name.StartsWith("Row", StringComparison.Ordinal) || name.StartsWith("Button", StringComparison.Ordinal) || name.StartsWith("Key_", StringComparison.Ordinal) || name.StartsWith("Increment", StringComparison.Ordinal) || name.StartsWith("Track", StringComparison.Ordinal) || name.StartsWith("Knob", StringComparison.Ordinal) || name == "DesignLogo" || name == "DesignSettings")
    {
      thickness *= 0.5f;
      depth = (float) (0.0560000017285347 + ((double) depth - 0.0560000017285347) * 0.5);
    }
    GameObject gameObject = new GameObject(name, new Type[2]
    {
      typeof (MeshFilter),
      typeof (MeshRenderer)
    });
    gameObject.transform.SetParent(this.visual, false);
    gameObject.transform.localPosition = KyraMenu.Point(x + width / 2f, y + height / 2f, depth);
    gameObject.GetComponent<MeshFilter>().sharedMesh = this.shape("Kyra/" + name, new Vector3(thickness * 0.7f, width * 0.0003147023f, height * 0.0003147023f), radius * 0.0003147023f, cornerMask);
    Material material = new Material(this.solid)
    {
      color = MenuPalette.Unpack(color)
    };
    material.SetFloat("_UseGradient", 0.0f);
    this.materials.Add(material);
    gameObject.GetComponent<Renderer>().sharedMaterial = material;
    return gameObject.transform;
  }
  private Text Label(
    string name,
    float x,
    float y,
    float width,
    float height,
    int size,
    TextAnchor alignment)
  {
    GameObject gameObject = new GameObject(name, new Type[3]
    {
      typeof (RectTransform),
      typeof (CanvasRenderer),
      typeof (Text)
    });
    gameObject.transform.SetParent(this.visual, false);
    gameObject.transform.localPosition = KyraMenu.Point(x + width / 2f, y + height / 2f, 0.058f);
    gameObject.transform.localRotation = Quaternion.LookRotation(Vector3.left, Vector3.forward);
    gameObject.transform.localScale = (Vector3.one * 0.0003147023f);
    Text component = gameObject.GetComponent<Text>();
    ((Graphic) component).rectTransform.sizeDelta = new Vector2(width, height);
    component.font = this.font;
    component.fontSize = size;
    ((Graphic) component).color = Color.white;
    component.alignment = alignment;
    component.supportRichText = true;
    ((Graphic) component).raycastTarget = false;
    component.resizeTextForBestFit = true;
    component.resizeTextMinSize = 12;
    component.resizeTextMaxSize = size;
    return component;
  }
  private static void OnFace(Transform text, Transform body)
  {
    Vector3 localPosition = text.localPosition;
    ref Vector3 local = ref localPosition;
    Bounds localBounds = ((Component) body).GetComponent<Renderer>().localBounds;
    double num = (double) (localBounds).max.x + 0.00050000002374872565;
    local.x = (float) num;
    text.localPosition = localPosition;
  }
  private Transform Icon(string name, Rect rect, bool cyan, float depth)
  {
    Texture2D image = NativeUiResources.LoadImage("kyra-reference.png");
    List<Vector3> vector3List = new List<Vector3>();
    List<int> intList = new List<int>();
    for (int yMin = (int) (rect).yMin; (double) yMin < (double) (rect).yMax; ++yMin)
    {
      for (int xMin = (int) (rect).xMin; (double) xMin < (double) (rect).xMax; ++xMin)
      {
        if (Filled(xMin, yMin))
        {
          int x = xMin;
          while ((double) (xMin + 1) < (double) (rect).xMax && Filled(xMin + 1, yMin))
            ++xMin;
          int count = vector3List.Count;
          float[] numArray1 = new float[2]
          {
            depth,
            depth - 0.0012f
          };
          foreach (float depth1 in numArray1)
          {
            vector3List.Add(KyraMenu.Point((float) x, (float) yMin, depth1));
            vector3List.Add(KyraMenu.Point((float) (xMin + 1), (float) yMin, depth1));
            vector3List.Add(KyraMenu.Point((float) (xMin + 1), (float) (yMin + 1), depth1));
            vector3List.Add(KyraMenu.Point((float) x, (float) (yMin + 1), depth1));
          }
          int[] numArray2 = new int[36]
          {
            0,
            1,
            2,
            0,
            2,
            3,
            4,
            6,
            5,
            4,
            7,
            6,
            0,
            1,
            5,
            0,
            5,
            4,
            1,
            2,
            6,
            1,
            6,
            5,
            2,
            3,
            7,
            2,
            7,
            6,
            3,
            0,
            4,
            3,
            4,
            7
          };
          foreach (int num in numArray2)
            intList.Add(count + num);
        }
      }
    }
    Mesh mesh1 = new Mesh();
    ((Object) mesh1).name = "Kyra/" + name;
    mesh1.vertices = vector3List.ToArray();
    mesh1.triangles = intList.ToArray();
    Mesh mesh2 = mesh1;
    mesh2.RecalculateNormals();
    mesh2.RecalculateBounds();
    this.icons.Add(mesh2);
    GameObject gameObject = new GameObject(name, new Type[2]
    {
      typeof (MeshFilter),
      typeof (MeshRenderer)
    });
    gameObject.transform.SetParent(this.visual, false);
    gameObject.GetComponent<MeshFilter>().sharedMesh = mesh2;
    Material material = new Material(this.solid)
    {
      color = cyan ? MenuPalette.Unpack(47080U) : Color.white
    };
    this.materials.Add(material);
    gameObject.GetComponent<Renderer>().sharedMaterial = material;
    return gameObject.transform;
    bool Filled(int x, int y)
    {
      Color32 color32 = ((Color32)image.GetPixel(x, ((Texture) image).height - 1 - y));
      return !cyan ? color32.r > (byte) 110 && color32.g > (byte) 110 && color32.b > (byte) 110 : color32.b > (byte) 120 && color32.g > (byte) 80 && color32.r < (byte) 80 ;
    }
  }
  private void BuildKeyboard()
  {
    this.keyboard = new GameObject("KyraKeyboard").transform;
    this.keyboard.SetParent(this.visual, false);
    this.query = this.Label("SearchQuery", 75f, 90f, 590f, 80f, 32 , (TextAnchor) 3);
    ((Component) this.query).transform.SetParent(this.keyboard, true);
    string[] strArray = new string[4]
    {
      "1234567890",
      "QWERTYUIOP",
      "ASDFGHJKL",
      "ZXCVBNM"
    };
    for (int index1 = 0; index1 < strArray.Length; ++index1)
    {
      for (int index2 = 0; index2 < strArray[index1].Length; ++index2)
      {
        string caption = strArray[index1][index2].ToString();
        float x = (float) (75 + (10 - strArray[index1].Length) * 29 + index2 * 58);
        float y = (float) (200 + index1 * 105);
        this.Key("Key_" + caption, caption, x, y, 50f, 84f);
      }
    }
    this.Key("Key_Back", "Back", 75f, 640f, 133f, 80f);
    this.Key("Key_Clear", "Clear", 218f, 640f, 133f, 80f);
    this.Key("Key_Space", "Space", 361f, 640f, 155f, 80f);
    this.Key("Key_Done", "Done", 526f, 640f, 133f, 80f);
    ((Component) this.keyboard).gameObject.SetActive(false);
  }
  private void Key(string name, string caption, float x, float y, float width, float height)
  {
    Transform body = this.Body(name, x, y, width, height, 10f, 0.058f, 0.009f, 2368548U );
    body.SetParent(this.keyboard, true);
    this.controls["Keyboard/" + name] = body;
    Text text = this.Label(name + "Caption", x, y, width, height, caption.Length == 1 ? 32 : 24, (TextAnchor) 4);
    text.text = caption;
    ((Component) text).transform.SetParent(body, true);
    KyraMenu.OnFace(((Component) text).transform, body);
  }
  public void SetActive(bool value)
  {
    this.active = value;
    ((Component) this.visual).gameObject.SetActive(value);
    foreach (Renderer original in this.originals)
    {
      if (((Object) original != (Object) null))
        original.enabled = !value;
    }
    if (!value)
      return;
    foreach (Renderer componentsInChild in ((Component) this.visual).GetComponentsInChildren<Renderer>(true))
      componentsInChild.enabled = true;
  }
  public void Refresh(MenuState state)
  {
    if (!this.active)
      return;
    this.state = state;
    ((Component) this.root.Find("DesignSettings")).gameObject.SetActive(true);
    if (this.page != state.Page || this.category != state.Category)
    {
      this.slideDirection = this.category != state.Category || this.page < 0 || state.Page >= this.page ? 1f : -1f;
      this.sliding = this.page >= 0 && !state.KeyboardOpen && !state.ColorPickerOpen;
      this.page = state.Page;
      this.category = state.Category;
      this.pageAt = Time.unscaledTime;
    }
    foreach (Transform content in this.contents)
      content.localPosition = Vector3.zero;
    ((Component) this.keyboard).gameObject.SetActive(state.KeyboardOpen);
    this.query.text = (state.Query.Length == 0 ? "Search" : state.Query) + "_";
    for (int row = 0; row < 6; ++row)
    {
      Transform control = this.controls["Row" + row.ToString()];
      int index = state.EntryAt(row);
      this.toggleTargets[row] = state.IsOn(index);
      ((Component) control).gameObject.SetActive(((Component) this.root.Find("Row" + row.ToString())).gameObject.activeSelf);
      string str1 = ((Component) this.root.Find($"Row{row.ToString()}/Label")).GetComponent<TextMesh>().text.Replace("\n", " ");
      this.labels[row].text = str1;
      string text1 = ((Component) this.root.Find($"Row{row.ToString()}/State")).GetComponent<TextMesh>().text;
      bool activeSelf = ((Component) this.root.Find("IncrementPlus" + row.ToString())).gameObject.activeSelf;
      ((Component) this.labels[row]).transform.localPosition = (this.labelPositions[row] + (Vector3.forward * (activeSelf ? 0.00456318352f : 0.0f)));
      this.labels[row].alignment = (TextAnchor) 3;
      ((Graphic) this.labels[row]).rectTransform.sizeDelta = new Vector2(activeSelf ? 448f : 488f, 97f);
      this.details[row].text = activeSelf ? text1 : "";
      ((Component) this.tracks[row]).gameObject.SetActive(!activeSelf);
      ((Component) this.knobs[row]).gameObject.SetActive(!activeSelf);
      if (activeSelf)
      {
        bool[] flagArray = new bool[2]{ false, true };
        foreach (bool flag in flagArray)
        {
          string str2 = $"Increment{(flag ? "Plus" : "Minus")}{row.ToString()}";
          if (!this.controls.ContainsKey(str2))
          {
            Transform body = this.Body(str2, flag ? 620f : 565f, (float) (100 + row * 112 ), 47f, 47f, 12f, 0.064f, 0.005f, 2368548U );
            body.SetParent(this.contents[row], true);
            this.controls[str2] = body;
            this.CacheControl(str2, body);
            Text text2 = this.Label(str2 + "Caption", flag ? 620f : 565f, (float) (100 + row * 112 ), 47f, 47f, 28, (TextAnchor) 4);
            text2.text = flag ? "+" : "-";
            ((Component) text2).transform.SetParent(body, true);
            KyraMenu.OnFace(((Component) text2).transform, body);
            MenuFonts.Invalidate(this.visual);
          }
        }
      }
    }
    foreach (KyraMenu.Control binding in this.bindings)
    {
      if (!binding.Row)
        ((Component) binding.Visual).gameObject.SetActive(((Component) binding.Proxy).gameObject.activeSelf);
      binding.Proxy.localPosition = this.root.InverseTransformPoint(binding.Visual.TransformPoint((binding.Bounds).center));
      binding.Proxy.localScale = Vector3.one;
      binding.Collider.center = Vector3.zero;
      binding.Collider.size = ((Vector3.Scale((binding.Bounds).size, binding.Visual.lossyScale) / Mathf.Max(0.0001f, this.root.lossyScale.x)) + (Vector3.right * 0.01f));
    }
    ((Component) this.homeIcon).gameObject.SetActive(!state.ColorPickerOpen);
    ((Component) this.homeText).gameObject.SetActive(state.ColorPickerOpen);
    this.homeText.text = state.EditingEnd ? "Done" : "Next";
    GradientPicker.Place(this.root, KyraMenu.Point(371f, 447.5f, 0.055f), 0.5f);
    foreach (Renderer componentsInChild in ((Component) this.root.Find("ColorPicker")).GetComponentsInChildren<Renderer>(true))
      componentsInChild.enabled = state.ColorPickerOpen;
    this.fontView.Apply(state.FontIndex);
    this.Tick(Time.unscaledTime);
  }
  private void CacheControl(string name, Transform body)
  {
    Transform transform = this.root.Find(name);
    if (((Object) transform == (Object) null))
      return;
    this.bindings.Add(new KyraMenu.Control()
    {
      Visual = body,
      Proxy = transform,
      Collider = ((Component) transform).GetComponent<BoxCollider>(),
      Position = body.localPosition,
      Bounds = ((Component) body).GetComponent<Renderer>().localBounds,
      Row = name.StartsWith("Row", StringComparison.Ordinal),
      FollowsContent = name.StartsWith("Increment", StringComparison.Ordinal)
    });
  }
  public void Tick(float time)
  {
    if (!this.active || this.state == null)
      return;
    float num1 = Mathf.Clamp(time - this.lastTime, 0.0f, 0.05f);
    this.lastTime = time;
    bool sliding = this.sliding;
    for (int index = 0; index < 6; ++index)
    {
      float num2 = Mathf.MoveTowards(this.toggles[index], (float) (this.toggleTargets[index] ? 1 : 0), num1 / 0.16f);
      if ((double) num2 != (double) this.toggles[index])
      {
        this.toggles[index] = num2;
        this.knobs[index].localPosition = (this.knobOff[index] + (Vector3.down * 0.0138469012f * Mathf.SmoothStep(0.0f, 1f, num2)));
        this.trackMaterials[index].color = Color.Lerp(KyraMenu.TrackOff, KyraMenu.TrackOn, num2);
      }
      if (this.sliding)
      {
        float num3 = 1f - Mathf.Clamp01((float) (((double) time - (double) this.pageAt) / 0.15999999642372131));
        this.contents[index].localPosition = (Vector3.down * (float) ((double) this.slideDirection * 8.0 * 0.00031470230896957219) * num3 * num3 * num3);
      }
    }
    if (this.sliding && (double) time - (double) this.pageAt >= 0.15999999642372131)
      this.sliding = false;
    foreach (KyraMenu.Control binding in this.bindings)
    {
      if (((Component) binding.Visual).gameObject.activeInHierarchy)
      {
        if (sliding && binding.FollowsContent)
          binding.Proxy.localPosition = this.root.InverseTransformPoint(binding.Visual.TransformPoint((binding.Bounds).center));
        float num4 = Mathf.Clamp(binding.Proxy.localScale.x, 0.85f, 1.03f);
        if ((double) num4 != (double) binding.Scale)
        {
          binding.Scale = num4;
          binding.Visual.localScale = (Vector3.one * num4);
          binding.Visual.localPosition = (binding.Position - (Vector3.right * (float) ((1.0 - (double) num4) * 0.012000000104308128)));
        }
      }
    }
    bool activeSelf = this.noticeProxy.activeSelf;
    if (((Component) this.noticeBody).gameObject.activeSelf != activeSelf)
      ((Component) this.noticeBody).gameObject.SetActive(activeSelf);
    if (activeSelf)
    {
      if (this.notice.text != this.sourceNotice.text)
        this.notice.text = this.sourceNotice.text;
      if ((((Graphic) this.notice).color != this.sourceNotice.color))
        ((Graphic) this.notice).color = this.sourceNotice.color;
    }
    this.fontView.Animate(time);
  }
  public void Dispose()
  {
    this.SetActive(false);
    NativeUiSnapshot.Destroy((Object) ((Component) this.visual).gameObject);
    foreach (Object material in this.materials)
      NativeUiSnapshot.Destroy(material);
    foreach (Object icon in this.icons)
      NativeUiSnapshot.Destroy(icon);
    NativeUiSnapshot.Destroy((Object) this.font);
  }
  private sealed class Control
  {
    public Transform Visual;
    public Transform Proxy;
    public BoxCollider Collider;
    public Vector3 Position;
    public Bounds Bounds;
    public bool Row;
    public bool FollowsContent;
    public float Scale = float.NaN;
  }
}
