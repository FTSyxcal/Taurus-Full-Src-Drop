
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class ImportedXyfer : IDisposable
{
  private static AssetBundle bundle;
  private static int sequence;
  private readonly Transform root;
  private readonly GameObject source;
  private readonly GameObject screen;
  private readonly Camera camera;
  private readonly RenderTexture texture;
  private readonly Renderer[] nativeRenderers;
  private readonly Dictionary<string, RectTransform> controls = new Dictionary<string, RectTransform>();
  private readonly Dictionary<Transform, float> scales = new Dictionary<Transform, float>();
  private readonly RectTransform[] rows = new RectTransform[7];
  private readonly TMP_Text[] labels = new TMP_Text[7];
  private readonly TMP_Text[] values = new TMP_Text[7];
  private readonly RectTransform[] knobs = new RectTransform[7];
  private readonly Graphic[] hearts = new Graphic[7];
  private readonly RectTransform keyboard;
  private readonly TMP_Text query;
  private readonly float[] knobFrom = new float[7];
  private readonly float[] knobTarget = new float[7];
  private readonly float[] knobStarted = new float[7];
  private readonly bool[] knobReady = new bool[7];
  private readonly int[] knobEntry = new int[7];
  private readonly Bounds bounds;
  private readonly float scale;
  private readonly Mesh mesh;
  private readonly Material material;
  private readonly int createdFrame;
  private bool active;
  private bool renderPending;
  private int settleFrame;
  private int lastRenderFrame = -1;
  private string signature;
  private readonly TMP_Text footer;
  private readonly ImportedTheme theme;
  private readonly ImportedPicker picker;
  private readonly TMP_Text empty;
  private readonly TMP_Text pickerNext;
  private MenuState state;
  public int RenderCount { get; private set; }
  private static AssetBundle OriginalBundle()
  {
    if (((Object) ImportedXyfer.bundle != (Object) null))
      return ImportedXyfer.bundle;
    Assembly assembly = typeof (ImportedXyfer).Assembly;
    using (Stream manifestResourceStream = assembly.GetManifestResourceStream("Taurus.UnityFigmaBridgeRuntime.dll"))
    {
      using (MemoryStream destination = new MemoryStream())
      {
        manifestResourceStream.CopyTo((Stream) destination);
        Assembly.Load(destination.ToArray());
      }
    }
    using (Stream manifestResourceStream = assembly.GetManifestResourceStream("Taurus.xyfer.bundle"))
    {
      using (MemoryStream destination = new MemoryStream())
      {
        manifestResourceStream.CopyTo((Stream) destination);
        ImportedXyfer.bundle = AssetBundle.LoadFromMemory(destination.ToArray());
      }
    }
    return !((Object) ImportedXyfer.bundle == (Object) null) ? ImportedXyfer.bundle : throw new InvalidDataException("Original Xyfer UI bundle could not be loaded.");
  }
  public ImportedXyfer(Transform root)
  {
    ImportedXyfer importedXyfer = this;
    this.root = root;
    this.createdFrame = Time.frameCount;
    this.nativeRenderers = ((Component) root).GetComponentsInChildren<Renderer>(true);
    AssetBundle assetBundle = ImportedXyfer.OriginalBundle();
    Shader shader = assetBundle.LoadAsset<Shader>("packages/com.simonoliver.unityfigma/unityfigmabridge/runtime/resources/shaders/figmaimageshader.shader");
    this.source = Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>("assets/figma/fontmaterialpresets/menuroot.prefab"));
    ((Object) this.source).name = "TaurusOriginalXyfer";
    if (Application.isPlaying)
      Object.DontDestroyOnLoad((Object) this.source);
    this.source.transform.position = new Vector3((float) (10000.0 + (double) ++ImportedXyfer.sequence * 3.0), 0.0f, 0.0f);
    this.source.transform.localScale = (Vector3.one * 1f / 1000f);
    this.source.transform.rotation = Quaternion.identity;
    foreach (Component componentsInChild in this.source.GetComponentsInChildren<Transform>(true))
      componentsInChild.gameObject.layer = 31 ;
    foreach (Graphic componentsInChild in this.source.GetComponentsInChildren<Graphic>(true))
    {
      if (!(componentsInChild.GetType().Name != "FigmaImage"))
        componentsInChild.GetType().GetField("m_DynamicMaterial", BindingFlags.Instance | BindingFlags.NonPublic).SetValue((object) componentsInChild, (object) new Material(shader));
    }
    TMP_FontAsset font = ((Component) root.Find("XyferFont")).GetComponent<TMP_Text>().font;
    foreach (TMP_Text componentsInChild in this.source.GetComponentsInChildren<TMP_Text>(true))
    {
      componentsInChild.font = font;
      componentsInChild.enableAutoSizing = true;
      componentsInChild.fontSizeMin = 12f;
      componentsInChild.fontSizeMax = componentsInChild.fontSize;
    }
    Canvas component1 = this.source.GetComponent<Canvas>();
    component1.renderMode = (RenderMode) 2;
    Canvas.ForceUpdateCanvases();
    bool flag = true;
    Vector3[] vector3Array = new Vector3[4];
    foreach (Graphic componentsInChild in this.source.GetComponentsInChildren<Graphic>(true))
    {
      componentsInChild.rectTransform.GetWorldCorners(vector3Array);
      foreach (Vector3 vector3 in vector3Array)
      {
        if (flag)
        {
          this.bounds = new Bounds(vector3, Vector3.zero);
          flag = false;
        }
        else
          (this.bounds).Encapsulate(vector3);
      }
    }
    this.scale = 0.43f / (this.bounds).size.x;
    float num1 = (this.bounds).size.y * this.scale;
    GameObject gameObject1 = new GameObject("XyferAssetCamera");
    if (Application.isPlaying)
      Object.DontDestroyOnLoad((Object) gameObject1);
    this.camera = gameObject1.AddComponent<Camera>();
    ((Behaviour) this.camera).enabled = false;
    ((Component) this.camera).transform.position = ((this.bounds).center + Vector3.back);
    this.camera.orthographic = true;
    this.camera.orthographicSize = (this.bounds).size.y / 2f;
    this.camera.nearClipPlane = 0.01f;
    this.camera.farClipPlane = 2f;
    this.camera.clearFlags = (CameraClearFlags) 2;
    this.camera.backgroundColor = Color.clear;
    this.camera.cullingMask = int.MinValue;
    MenuRender.Configure(this.camera);
    this.camera.useOcclusionCulling = false;
    this.texture = new RenderTexture(Mathf.CeilToInt(1720f), Mathf.CeilToInt(num1 * 4000f), 24)
    {
      antiAliasing = 4
    };
    this.camera.targetTexture = this.texture;
    this.camera.aspect = (this.bounds).size.x / (this.bounds).size.y;
    component1.worldCamera = this.camera;
    this.screen = new GameObject("OriginalDesignSurface", new Type[2]
    {
      typeof (MeshFilter),
      typeof (MeshRenderer)
    });
    this.screen.layer = ((Component) root).gameObject.layer;
    this.screen.transform.SetParent(root, false);
    this.screen.transform.localPosition = new Vector3(0.075f, 0.0f, 0.0f);
    this.mesh = new Mesh()
    {
      vertices = new Vector3[4]
      {
        new Vector3(0.0f, 0.215f, (float) (-(double) num1 / 2.0)),
        new Vector3(0.0f, -0.215f, (float) (-(double) num1 / 2.0)),
        new Vector3(0.0f, -0.215f, num1 / 2f),
        new Vector3(0.0f, 0.215f, num1 / 2f)
      },
      uv = new Vector2[4]
      {
        Vector2.zero,
        Vector2.right,
        Vector2.one,
        Vector2.up
      },
      triangles = new int[6]{ 0, 2, 1, 0, 3, 2 }
    };
    this.mesh.RecalculateBounds();
    this.mesh.RecalculateNormals();
    this.screen.GetComponent<MeshFilter>().sharedMesh = this.mesh;
    this.material = new Material(((Component) root.Find("ImportedUIReference")).GetComponent<Renderer>().sharedMaterial)
    {
      mainTexture = (Texture) this.texture
    };
    this.screen.GetComponent<Renderer>().sharedMaterial = this.material;
    Transform transform1 = this.source.transform.Find("Xyfer");
    Transform transform2 = transform1.Find("Menu/Buttons");
    for (int index1 = 0; index1 < 7; ++index1)
    {
      this.rows[index1] = transform2.GetChild(index1) as RectTransform;
      this.labels[index1] = ((Component) ((Transform) this.rows[index1]).Find("Text")).GetComponent<TMP_Text>();
      this.labels[index1].rectTransform.anchorMin = this.labels[index1].rectTransform.anchorMax = (Vector2.one * 0.5f);
      this.labels[index1].rectTransform.pivot = new Vector2(0.0f, 0.5f);
      this.labels[index1].rectTransform.sizeDelta = new Vector2(365f, 65f);
      this.labels[index1].rectTransform.anchoredPosition = new Vector2(-225f, 0.0f);
      this.labels[index1].alignment = (TextAlignmentOptions) 4097;
      this.labels[index1].text = "";
      this.values[index1] = Object.Instantiate<TMP_Text>(this.labels[index1], (Transform) this.rows[index1], false);
      ((Object) this.values[index1]).name = "TaurusValue";
      this.values[index1].rectTransform.anchoredPosition = new Vector2(-225f, -20f);
      this.values[index1].rectTransform.sizeDelta = new Vector2(270f, 24f);
      this.values[index1].fontSizeMax = 18f;
      this.knobs[index1] = ((Transform) this.rows[index1]).Find("Checkbox/Circle") as RectTransform;
      GameObject gameObject2 = new GameObject("TaurusFavorite", new Type[3]
      {
        typeof (RectTransform),
        typeof (CanvasRenderer),
        typeof (XyferHeart)
      });
      gameObject2.layer = 31 ;
      gameObject2.transform.SetParent((Transform) this.rows[index1], false);
      this.hearts[index1] = (Graphic) gameObject2.GetComponent<XyferHeart>();
      this.hearts[index1].color = ((Graphic) this.labels[index1]).color;
      this.hearts[index1].raycastTarget = false;
      this.hearts[index1].rectTransform.anchorMin = this.hearts[index1].rectTransform.anchorMax = (Vector2.one * 0.5f);
      this.hearts[index1].rectTransform.anchoredPosition = new Vector2(-210f, 0.0f);
      this.hearts[index1].rectTransform.sizeDelta = new Vector2(22f, 22f);
      RectTransform rectTransform1 = this.hearts[index1].rectTransform;
      ((Transform) rectTransform1).localPosition = (((Transform) rectTransform1).localPosition + Vector3.back);
      this.controls["Row" + index1.ToString()] = this.rows[index1];
      RectTransform rectTransform2 = ((Transform) this.rows[index1]).Find("Checkbox") as RectTransform;
      for (int index2 = 0; index2 < 2; ++index2)
      {
        RectTransform rectTransform3 = Object.Instantiate<RectTransform>(rectTransform2, (Transform) this.rows[index1], false);
        ((Object) rectTransform3).name = index2 == 0 ? "TaurusMinus" : "TaurusPlus";
        ImportedXyfer.Destroy((Object) ((Component) ((Transform) rectTransform3).Find("Circle")).gameObject);
        rectTransform3.sizeDelta = new Vector2(30f, 36f);
        rectTransform3.anchoredPosition = new Vector2(index2 == 0 ? 416f : 456f, -40f);
        RectTransform rectTransform4 = ((Transform) rectTransform3).Find("Background") as RectTransform;
        Vector2 vector2_1;
        Vector2 vector2_2 = vector2_1 = (Vector2.one * 0.5f);
        rectTransform4.pivot = vector2_1;
        Vector2 vector2_3;
        Vector2 vector2_4 = vector2_3 = vector2_2;
        rectTransform4.anchorMax = vector2_3;
        rectTransform4.anchorMin = vector2_4;
        rectTransform4.anchoredPosition = Vector2.zero;
        rectTransform4.sizeDelta = rectTransform3.sizeDelta;
        TMP_Text tmpText = Object.Instantiate<TMP_Text>(this.labels[index1], (Transform) rectTransform3, false);
        tmpText.rectTransform.anchorMin = tmpText.rectTransform.anchorMax = tmpText.rectTransform.pivot = (Vector2.one * 0.5f);
        tmpText.rectTransform.anchoredPosition = Vector2.zero;
        tmpText.rectTransform.sizeDelta = new Vector2(30f, 36f);
        tmpText.alignment = (TextAlignmentOptions) 514;
        tmpText.text = index2 == 0 ? "-" : "+";
        this.controls[(index2 == 0 ? "IncrementMinus" : "IncrementPlus") + index1.ToString()] = rectTransform3;
      }
    }
    this.keyboard = new GameObject("TaurusKeyboard", new Type[1]
    {
      typeof (RectTransform)
    }).GetComponent<RectTransform>();
    ((Component) this.keyboard).gameObject.layer = 31 ;
    ((Transform) this.keyboard).SetParent(transform1.Find("Menu"), false);
    this.keyboard.anchorMin = this.keyboard.anchorMax = this.keyboard.pivot = (Vector2.one * 0.5f);
    this.keyboard.sizeDelta = new Vector2(490f, 620f);
    this.keyboard.anchoredPosition = new Vector2(0.0f, 20f);
    this.query = ((Component) this.OriginalKey("Query", "Search", new Vector2(0.0f, 230f), new Vector2(466f, 70f), false)).GetComponentInChildren<TMP_Text>();
    string[] strArray1 = new string[4]
    {
      "1234567890",
      "QWERTYUIOP",
      "ASDFGHJKL",
      "ZXCVBNM"
    };
    for (int index3 = 0; index3 < strArray1.Length; ++index3)
    {
      for (int index4 = 0; index4 < strArray1[index3].Length; ++index4)
      {
        string text = strArray1[index3][index4].ToString();
        this.OriginalKey("Key_" + text, text, new Vector2((float) (((double) index4 - (double) (strArray1[index3].Length - 1) / 2.0) * 47.0), (float) (120.0 - (double) index3 * 78.0)), new Vector2(41f, 66f), true);
      }
    }
    string[] strArray2 = new string[4]
    {
      "Clear",
      "Space",
      "Back",
      "Done"
    };
    float[] numArray = new float[4]{ 92f, 136f, 92f, 122f };
    float num2 = -233f;
    for (int index = 0; index < strArray2.Length; ++index)
    {
      this.OriginalKey("Key_" + strArray2[index], strArray2[index], new Vector2(num2 + numArray[index] / 2f, -218f), new Vector2(numArray[index], 66f), true);
      num2 += numArray[index] + 8f;
    }
    this.controls["ButtonPrevious"] = transform1.Find("Menu/PageButtons/Left") as RectTransform;
    this.controls["ButtonNext"] = transform1.Find("Menu/PageButtons/Right") as RectTransform;
    this.controls["ButtonHome"] = transform1.Find("TopBar/ButtonsLeft").GetChild(1) as RectTransform;
    this.pickerNext = Object.Instantiate<TMP_Text>(this.labels[0], (Transform) this.controls["ButtonHome"], false);
    ((Object) this.pickerNext).name = "PickerNext";
    this.pickerNext.rectTransform.anchoredPosition = Vector2.zero;
    this.pickerNext.rectTransform.anchorMin = this.pickerNext.rectTransform.anchorMax = this.pickerNext.rectTransform.pivot = (Vector2.one * 0.5f);
    this.pickerNext.rectTransform.sizeDelta = new Vector2(55f, 50f);
    this.pickerNext.alignment = (TextAlignmentOptions) 514;
    this.pickerNext.fontSizeMax = 16f;
    this.empty = Object.Instantiate<TMP_Text>(this.labels[0], transform1.Find("Menu"), false);
    ((Object) this.empty).name = "EmptyMessage";
    this.empty.rectTransform.anchorMin = this.empty.rectTransform.anchorMax = this.empty.rectTransform.pivot = (Vector2.one * 0.5f);
    this.empty.rectTransform.anchoredPosition = new Vector2(0.0f, 20f);
    this.empty.rectTransform.sizeDelta = new Vector2(410f, 80f);
    this.empty.alignment = (TextAlignmentOptions) 514;
    this.empty.fontSizeMax = 26f;
    this.controls["ButtonSearch"] = transform1.Find("TopBar/ButtonsRight").GetChild(1) as RectTransform;
    this.controls["ButtonDisconnect"] = transform1.Find("TopBar/ButtonsRight").GetChild(0) as RectTransform;
    ((Component) transform1.Find("TopBar/Title/Text")).GetComponent<TMP_Text>().text = "Xyfer";
    this.footer = ((Component) transform1.Find("TopBar/Title/Text")).GetComponent<TMP_Text>();
    this.footer.rectTransform.anchorMin = this.footer.rectTransform.anchorMax = this.footer.rectTransform.pivot = (Vector2.one * 0.5f);
    this.footer.rectTransform.anchoredPosition = Vector2.zero;
    this.footer.rectTransform.sizeDelta = new Vector2(177f, 58f);
    this.footer.alignment = (TextAlignmentOptions) 514;
    this.footer.fontSizeMax = 22f;
    this.controls["DesignSettings"] = transform1.Find("TopBar/ButtonsLeft").GetChild(0) as RectTransform;
    foreach (Graphic componentsInChild in this.source.GetComponentsInChildren<Graphic>(true))
    {
      if (componentsInChild.GetType().Name == "FigmaImage")
      {
        FieldInfo field = componentsInChild.GetType().GetField("m_DynamicMaterial", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field.GetValue((object) componentsInChild) == null)
          field.SetValue((object) componentsInChild, (object) new Material(shader));
      }
    }
    Bounds content = ImportedSurface.BoundsOf((Transform) this.rows[0]);
    foreach (RectTransform row in this.rows)
      (content).Encapsulate(ImportedSurface.BoundsOf((Transform) row));
    this.theme = new ImportedTheme(this.source.transform, Vector3.right, Vector3.up);
    this.picker = new ImportedPicker(root, this.source.transform, content, Vector3.forward, Vector3.right, Vector3.up, (Action<Transform, Transform>) ((proxy, control) =>
    {
      Bounds bounds1 = ImportedSurface.BoundsOf(control);
      Transform transform3 = proxy;
      Transform transform4 = root;
      double x1 = (double) (bounds1).center.x;
      Bounds bounds2 = importedXyfer.bounds;
      double x2 = (double) (bounds2).center.x;
      double num3 = -(x1 - x2) * (double) importedXyfer.scale;
      double y1 = (double) (bounds1).center.y;
      Bounds bounds3 = importedXyfer.bounds;
      double y2 = (double) (bounds3).center.y;
      double num4 = (y1 - y2) * (double) importedXyfer.scale;
      Vector3 vector3_1 = new Vector3(0.08f, (float) num3, (float) num4);
      Vector3 vector3_2 = transform4.TransformPoint(vector3_1);
      transform3.position = vector3_2;
      BoxCollider component2 = ((Component) proxy).GetComponent<BoxCollider>();
      component2.center = Vector3.zero;
      component2.size = (new Vector3(0.014f, (bounds1).size.x * importedXyfer.scale, (bounds1).size.y * importedXyfer.scale) / proxy.localScale.x);
    }));
  }
  private RectTransform OriginalKey(
    string name,
    string text,
    Vector2 position,
    Vector2 size,
    bool input)
  {
    RectTransform component = new GameObject(name, new Type[1]
    {
      typeof (RectTransform)
    }).GetComponent<RectTransform>();
    ((Component) component).gameObject.layer = 31 ;
    ((Transform) component).SetParent((Transform) this.keyboard, false);
    component.anchorMin = component.anchorMax = component.pivot = (Vector2.one * 0.5f);
    component.anchoredPosition = position;
    component.sizeDelta = size;
    RectTransform rectTransform = Object.Instantiate<Transform>(((Transform) this.rows[0]).Find("Background"), (Transform) component, false) as RectTransform;
    Vector2 vector2_1;
    Vector2 vector2_2 = vector2_1 = (Vector2.one * 0.5f);
    rectTransform.pivot = vector2_1;
    Vector2 vector2_3;
    Vector2 vector2_4 = vector2_3 = vector2_2;
    rectTransform.anchorMax = vector2_3;
    rectTransform.anchorMin = vector2_4;
    rectTransform.anchoredPosition = Vector2.zero;
    rectTransform.sizeDelta = size;
    TMP_Text tmpText = Object.Instantiate<TMP_Text>(this.labels[0], (Transform) component, false);
    tmpText.rectTransform.anchorMin = tmpText.rectTransform.anchorMax = tmpText.rectTransform.pivot = (Vector2.one * 0.5f);
    tmpText.rectTransform.anchoredPosition = Vector2.zero;
    tmpText.rectTransform.sizeDelta = (size - new Vector2(8f, 8f));
    tmpText.alignment = (TextAlignmentOptions) 514;
    tmpText.fontSizeMax = input ? 24f : 28f;
    tmpText.text = text;
    if (input)
      this.controls["Keyboard/" + name] = component;
    return component;
  }
  public void SetActive(bool value)
  {
    this.active = value;
    this.source.SetActive(value);
    this.screen.SetActive(value);
    if (!value)
    {
      foreach (Renderer nativeRenderer in this.nativeRenderers)
      {
        if (((Object) nativeRenderer != (Object) null))
          nativeRenderer.enabled = true;
      }
      ((Component) this.root.Find("DesignSettings")).gameObject.SetActive(false);
    }
    this.signature = (string) null;
    if (!value)
      return;
    this.renderPending = true;
    this.settleFrame = Time.frameCount + 1;
  }
  private void Position(string name, RectTransform rect)
  {
    Transform transform1 = this.root.Find(name);
    RectTransform rectTransform = rect;
    Rect rect1 = rect.rect;
    Vector3 vector3_1 = ((Vector2)(rect1).center);
    Vector3 vector3_2 = ((Transform) rectTransform).TransformPoint(vector3_1);
    Transform transform2 = transform1;
    Transform root = this.root;
    double x1 = (double) vector3_2.x;
    Bounds bounds1 = this.bounds;
    double x2 = (double) (bounds1).center.x;
    double num1 = -(x1 - x2) * (double) this.scale;
    double y1 = (double) vector3_2.y;
    Bounds bounds2 = this.bounds;
    double y2 = (double) (bounds2).center.y;
    double num2 = (y1 - y2) * (double) this.scale;
    Vector3 vector3_3 = new Vector3(0.08f, (float) num1, (float) num2);
    Vector3 vector3_4 = root.TransformPoint(vector3_3);
    transform2.position = vector3_4;
    ((Component) transform1).GetComponent<BoxCollider>().center = Vector3.zero;
    BoxCollider component = ((Component) transform1).GetComponent<BoxCollider>();
    Rect rect2 = rect.rect;
    double num3 = (double) (rect2).width * (1.0 / 1000.0) * (double) this.scale;
    rect2 = rect.rect;
    double num4 = (double) (rect2).height * (1.0 / 1000.0) * (double) this.scale;
    Vector3 vector3_5 = (new Vector3(0.014f, (float) num3, (float) num4) / transform1.localScale.x);
    component.size = vector3_5;
  }
  public void Refresh(MenuState state)
  {
    if (!this.active)
      return;
    this.state = state;
    foreach (Renderer nativeRenderer in this.nativeRenderers)
    {
      if (((Object) nativeRenderer != (Object) null))
        nativeRenderer.enabled = false;
    }
    ((Component) this.root.Find("DesignSettings")).gameObject.SetActive(true);
    StringBuilder stringBuilder = new StringBuilder();
    this.root.Find("ColorPicker").localPosition = new Vector3(0.03f, 0.0f, 0.025f);
    this.root.Find("Keyboard").localPosition = new Vector3(0.03f, 0.0f, 0.0f);
    ((Component) this.keyboard).gameObject.SetActive(state.KeyboardOpen);
    this.query.text = state.Query.Length == 0 ? "Type to search_" : state.Query + "_";
    stringBuilder.Append(this.query.text).Append(state.KeyboardOpen).Append(state.ColorPickerOpen);
    ((Component) this.empty).gameObject.SetActive(state.ResultCount == 0 && !state.KeyboardOpen && !state.ColorPickerOpen);
    this.empty.text = ((Component) this.root.Find("PageLabel")).GetComponent<TextMesh>().text;
    ((Component) this.pickerNext).gameObject.SetActive(state.ColorPickerOpen);
    this.pickerNext.text = state.EditingEnd ? "Done" : "Next";
    foreach (Image componentsInChild in ((Component) this.controls["ButtonHome"]).GetComponentsInChildren<Image>(true))
    {
      if (componentsInChild.GetType().Name != "FigmaImage")
        ((Behaviour) componentsInChild).enabled = !state.ColorPickerOpen;
    }
    stringBuilder.Append(this.empty.text).Append(this.pickerNext.text);
    for (int row = 0; row < 7; ++row)
    {
      Transform transform = this.root.Find("Row" + row.ToString());
      ((Component) this.rows[row]).gameObject.SetActive(((Component) transform).gameObject.activeSelf);
      this.labels[row].text = ((Component) transform.Find("Label")).GetComponent<TextMesh>().text.Replace("\n", " ");
      MenuButtonEffects.Bind(this.labels[row].transform, (Transform) this.rows[row]);
      this.values[row].text = ((Component) transform.Find("State")).GetComponent<TextMesh>().text;
      bool activeSelf = ((Component) this.root.Find("IncrementPlus" + row.ToString())).gameObject.activeSelf;
      bool flag1 = state.IsFavorite(state.EntryAt(row));
      bool flag2 = activeSelf || this.values[row].text.Length > 0 && this.values[row].text != "On" && this.values[row].text != "Off";
      this.labels[row].rectTransform.anchoredPosition = new Vector2(flag1 ? -182f : -225f, flag2 ? 16f : 0.0f);
      this.labels[row].rectTransform.sizeDelta = new Vector2(flag1 ? 320f : 365f, flag2 ? 40f : 65f);
      ((Component) this.values[row]).gameObject.SetActive(flag2);
      ((Component) this.hearts[row]).gameObject.SetActive(flag1);
      ((Component) ((Transform) this.rows[row]).Find("Checkbox")).gameObject.SetActive(!activeSelf && state.EntryAt(row) >= 0 && MenuCatalog.Buttons[state.EntryAt(row)].Toggle);
      float num = state.IsOn(state.EntryAt(row)) ? 51f : 19f;
      if (!this.knobReady[row] || this.knobEntry[row] != state.EntryAt(row))
      {
        this.knobReady[row] = true;
        this.knobEntry[row] = state.EntryAt(row);
        this.knobFrom[row] = this.knobTarget[row] = num;
        this.knobs[row].anchoredPosition = new Vector2(num, -18f);
      }
      else if ((double) this.knobTarget[row] != (double) num)
      {
        this.knobFrom[row] = this.knobs[row].anchoredPosition.x;
        this.knobTarget[row] = num;
        this.knobStarted[row] = Time.unscaledTime;
      }
      stringBuilder.Append(this.labels[row].text).Append(this.values[row].text).Append(state.IsOn(state.EntryAt(row))).Append(((Component) this.rows[row]).gameObject.activeSelf).Append(flag1);
      for (int index = 0; index < 2; ++index)
        ((Component) this.controls[(index == 0 ? "IncrementMinus" : "IncrementPlus") + row.ToString()]).gameObject.SetActive(activeSelf);
    }
    foreach (KeyValuePair<string, RectTransform> control in this.controls)
    {
      Transform transform = this.root.Find(control.Key);
      ((Component) control.Value).gameObject.SetActive(((Component) transform).gameObject.activeInHierarchy);
      this.Position(control.Key, control.Value);
    }
    for (int index = 0; index < 7; ++index)
    {
      if (((Component) this.root.Find("IncrementPlus" + index.ToString())).gameObject.activeSelf)
      {
        BoxCollider component = ((Component) this.root.Find("Row" + index.ToString())).GetComponent<BoxCollider>();
        float y = component.size.y;
        component.center = new Vector3(0.0f, y * 0.14f, 0.0f);
        component.size = new Vector3(component.size.x, y * 0.72f, component.size.z);
      }
    }
    for (int index = 0; index < 7; ++index)
      ((Component) this.root.Find($"Row{index.ToString()}/FavoriteIcon")).GetComponent<Renderer>().enabled = false;
    this.footer.text = "Xyfer\n" + ((Component) this.root.Find("PageLabel")).GetComponent<TextMesh>().text;
    stringBuilder.Append(this.footer.text);
    if (this.picker.Refresh(state))
      this.renderPending = true;
    string str = stringBuilder.ToString();
    if (!(this.signature != str))
      return;
    this.signature = str;
    this.settleFrame = Time.frameCount + 1;
    this.Render();
  }
  public void Tick(float time)
  {
    if (!this.active)
      return;
    if (!this.texture.IsCreated())
    {
      this.texture.Create();
      this.renderPending = true;
    }
    bool flag = ((this.state == null ? (false ? 1 : 0) : (this.theme.Apply(this.state, time) ? 1 : 0)) | (this.state == null ? 0 : (MenuFonts.Apply(this.source.transform, this.state.FontIndex) ? 1 : 0))) != 0;
    for (int index = 0; index < 7; ++index)
    {
      if (this.knobReady[index])
      {
        float num1 = Mathf.Clamp01((float) (((double) time - (double) this.knobStarted[index]) / 0.18000000715255737));
        float num2 = Mathf.Lerp(this.knobFrom[index], this.knobTarget[index], Mathf.SmoothStep(0.0f, 1f, num1));
        if ((double) num1 >= 1.0)
          this.knobFrom[index] = this.knobTarget[index];
        if (!Mathf.Approximately(this.knobs[index].anchoredPosition.x, num2))
        {
          this.knobs[index].anchoredPosition = new Vector2(num2, -18f);
          flag = true;
        }
      }
    }
    foreach (KeyValuePair<string, RectTransform> control in this.controls)
    {
      float x = this.root.Find(control.Key).localScale.x;
      float num;
      if (!this.scales.TryGetValue((Transform) control.Value, out num) || !Mathf.Approximately(x, num))
      {
        ((Transform) control.Value).localScale = (Vector3.one * x);
        this.scales[(Transform) control.Value] = x;
        flag = true;
      }
    }
    if (!flag && !this.renderPending)
      return;
    this.Render();
  }
  private void Render()
  {
    this.renderPending = true;
    if (!MenuRender.Ready || Application.isPlaying && Time.frameCount <= this.createdFrame + 1 || Application.isPlaying && this.lastRenderFrame == Time.frameCount)
      return;
    Canvas.ForceUpdateCanvases();
    foreach (TMP_Text componentsInChild in this.source.GetComponentsInChildren<TMP_Text>())
    {
      if (componentsInChild.havePropertiesChanged)
        componentsInChild.ForceMeshUpdate(false, false);
    }
    Canvas.ForceUpdateCanvases();
    if (!MenuRender.TryRender(this.camera))
      return;
    this.renderPending = Application.isPlaying && Time.frameCount < this.settleFrame;
    this.lastRenderFrame = Time.frameCount;
    ++this.RenderCount;
  }
  public void Dispose()
  {
    foreach (Graphic componentsInChild in this.source.GetComponentsInChildren<Graphic>(true))
    {
      if (componentsInChild.GetType().Name == "FigmaImage")
      {
        Material material = componentsInChild.GetType().GetField("m_DynamicMaterial", BindingFlags.Instance | BindingFlags.NonPublic).GetValue((object) componentsInChild) as Material;
        if (((Object) material != (Object) null))
          ImportedXyfer.Destroy((Object) material);
      }
    }
    ImportedXyfer.Destroy((Object) this.source);
    ImportedXyfer.Destroy((Object) ((Component) this.camera).gameObject);
    this.texture.Release();
    ImportedXyfer.Destroy((Object) this.texture);
    ImportedXyfer.Destroy((Object) this.screen);
    ImportedXyfer.Destroy((Object) this.mesh);
    ImportedXyfer.Destroy((Object) this.material);
  }
  private static void Destroy(UnityEngine.Object item)
  {
    if (Application.isPlaying)
      Object.Destroy(item);
    else
      Object.DestroyImmediate(item);
  }
}
