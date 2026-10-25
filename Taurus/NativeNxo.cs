
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Taurus;
internal sealed class NativeNxo
{
  private readonly NativeNxo.VariablesData Variables = new NativeNxo.VariablesData();
  private readonly NativeNxo.SettingsData Settings = new NativeNxo.SettingsData();
  private readonly NativeNxo.SearchData SearchAndKeyboard = new NativeNxo.SearchData();
  private readonly NativeUiSnapshot snapshot = new NativeUiSnapshot();
  private readonly Dictionary<string, string> bindings = new Dictionary<string, string>();
  private readonly List<GameObject> ObjectSink = new List<GameObject>();
  private readonly List<GameObject> _trackedObjects = new List<GameObject>();
  private readonly StringBuilder _titleBuilder = new StringBuilder();
  private GameObject disconnectButton;
  private GameObject fpsButton;
  private UnityEngine.UI.Text fpsText;
  private bool _drawingSearchBar;
  private bool MenuAnimations;
  private Color pinwheelColor1 = ((Color32)new Color32((byte) 13, byte.MaxValue, byte.MaxValue, byte.MaxValue));
  private Color pinwheelColor2 = Color.white;
  private Font CurrentFont = Font.CreateDynamicFontFromOSFont("Arial", 22);
  private float uiTime;
  private float pinwheelSpeed = 0.5f;
  private Material _cachedPinwheelBase;
  private Material pinwheel;
  private MenuState menuState;
  public static readonly Vector3 MenuSize = new Vector3(0.75f, 0.2875f, 0.45f);
  public static readonly Vector3 BackgroundScale = new Vector3(0.01f, 0.875f, 0.9f);
  public static readonly Vector3 BackgroundPos = new Vector3(0.05f, 0.0f, 0.025f);
  public const float FaceX = 0.07525f;
  public const float IconX = 0.07225f;
  public static readonly Vector3 SearchBarScale = new Vector3(0.01f, 0.8625f, 1f / 16f);
  public static readonly Vector3 SearchBarPos = new Vector3(0.0667f, 0.0f, 0.56f);
  public static readonly Vector3 SearchTextPos = new Vector3(0.054f, 0.0f, 0.2525f);
  public static readonly Vector3 HeaderIconScale = new Vector3(0.039375f, 0.0590625f, 0.0f);
  public const float DisconnectIconY = -0.25f;
  public const float SearchIconY = -0.35f;
  public const float HeaderIconZ = 0.45f;
  public const float FpsWidthFactor = 3.3f;
  public static readonly Vector2 TitleSize = new Vector2(0.128f, 0.0296f);
  public static readonly Vector3 AccentStripScale = new Vector3(1f / 1000f, 0.875f, 0.0045f);
  public static readonly Vector3 AccentStripPos = new Vector3(0.0715f, 0.0f, 0.4f);
  public const float ButtonFullWidth = 0.82f;
  public const float ButtonWidthWithGear = 0.695f;
  public const float ButtonWidthIncremental = 0.555f;
  public const float ButtonHeight = 0.08f;
  public const float ButtonDepth = 0.0075f;
  public const float ButtonTextX = 0.05975f;
  public const float ModLabelX = 0.054f;
  public const float ButtonYWithGear = 0.0625f;
  public const float ButtonBaseZ = 0.335f;
  public const float SubButtonWidth = 0.12f;
  public const float IncrementUpY = -0.3525f;
  public const float IncrementDownY = -0.22f;
  public const float GearY = -0.3525f;
  public const float ButtonTextYRatio = 0.288f;
  public const float CompactButtonY = -0.3525f;
  public const float CompactLabelX = 0.054f;
  public const float CompactLabelY = 0.035f;
  public const float CompactLabelWidth = 0.125f;
  public const float ToggleWidth = 0.15f;
  public const float ToggleY = -0.3375f;
  public const float ToggleHeight = 0.058f;
  public const float NavRowZ = -0.33f;
  public const float ReturnHeight = 0.2925f;
  public const float PageHeight = 0.25f;
  public const float PageLeftY = 0.285f;
  public const float PageRightY = -0.285f;
  public static readonly Vector2 ModTextSize = new Vector2(0.22f, 1f / 64f);
  public static readonly Vector2 IncrementalTextSize = new Vector2(0.15f, 0.0155f);
  public static readonly Vector2 SubTextSize = new Vector2(0.1f, 0.015f);
  public static readonly Vector2 PageTextSize = new Vector2(0.15f, 0.0225f);
  public static readonly Vector2 GearIconSize = new Vector2(0.1f, 0.02f);
  public static readonly Vector3 HomeIconScale = new Vector3(0.015f, 0.015f, 0.0f);
  private GameObject keyboardObj;
  private Material _keyboardBgMaterial;
  private Material _outlineMaterial;
  private readonly List<Material> _keyboardMaterials = new List<Material>();
  private readonly List<GameObject> _keyboardObjects = new List<GameObject>();
  private readonly List<Mesh> _keyboardMeshes = new List<Mesh>();
  private readonly List<(NativeNxo.ColorRole, GameObject, List<GameObject>)> _keyboardColorGroups = new List<(NativeNxo.ColorRole, GameObject, List<GameObject>)>();
  private readonly Dictionary<string, NativeUiKey> _keyColliders = new Dictionary<string, NativeUiKey>();
  private List<Material> MaterialSink => this.snapshot.Materials;
  private List<Mesh> MeshSink => this.snapshot.Meshes;
  private float TextOffset => 0.15f;
  private float IncrementTextOffset => 0.148f;
  private float PageZOffset => -0.14875f;
  private (float y, float z) TitleOffset => (57f / 1000f, 0.2025f);
  public NativeUiSnapshot Build(MenuState state, Transform proxies)
  {
    this.menuState = state;
    this.Variables.uberShader = Shader.Find("GorillaTag/UberShader") ?? ((Component) proxies.Find("Background")).GetComponent<Renderer>().sharedMaterial.shader;
    this.Variables.uiShader = Graphic.defaultGraphicMaterial.shader;
    this.Settings.MenuRoundness = state.Rounded ? 1f : 0.0f;
    this.CreateBaseMenuObject();
    this.CreateMenuBackground();
    this.CreateMenuCanvasAndTitle();
    this.CreateAccentStrip();
    this.SearchAndKeyboard.isSearching = state.Query.Length > 0 || state.KeyboardOpen;
    this.SearchAndKeyboard.inputText = state.Query;
    this.ToggleSearchButton();
    this.AddSearchBar();
    this.AddDisconnectButton();
    this.AddFpsButton();
    this.AddReturnButton();
    this.AddPageButton("<", 0.285f);
    this.AddPageButton(">", -0.285f);
    this.Variables.title.text = this.BuildStaticGradientTitle();
    ((Graphic) this.Variables.title).color = Color.white;
    Transform transform = new GameObject("NativeContent").transform;
    transform.SetParent(this.Variables.canvasObj.transform, false);
    this.snapshot.Content = transform;
    NativeNxo.ButtonHandler.FavoriteMods.Clear();
    for (int row = 0; row < state.VisibleRows; ++row)
    {
      if (state.EntryAt(row) >= 0 || state.SoundNameAt(row) != null)
      {
        string str = ((Component) proxies.Find($"Row{row.ToString()}/Label")).GetComponent<TextMesh>().text.Replace("\n", " ");
        string text = ((Component) proxies.Find($"Row{row.ToString()}/State")).GetComponent<TextMesh>().text;
        bool activeSelf = ((Component) proxies.Find("IncrementPlus" + row.ToString())).gameObject.activeSelf;
        if (state.EntryAt(row) >= 0 && MenuCatalog.Buttons[state.EntryAt(row)].Name == "No Telemetry")
          str = $"<color=#888888>{str} [Disabled]</color>";
        NativeNxo.ButtonHandler.Button button = new NativeNxo.ButtonHandler.Button(str + (activeSelf ? " : " + text : ""), NativeNxo.Category.Other, state.EntryAt(row) >= 0 && MenuCatalog.Buttons[state.EntryAt(row)].Toggle, state.IsOn(state.EntryAt(row)));
        button.incremental = activeSelf;
        button.isCategory = state.EntryAt(row) >= 0 && MenuCatalog.Buttons[state.EntryAt(row)].Target >= 0;
        if (state.IsFavorite(state.EntryAt(row)))
          NativeNxo.ButtonHandler.FavoriteMods.Add(button);
        this.bindings[button.buttonText] = "Row" + row.ToString();
        this.bindings[button.buttonText + "_DOWN"] = "IncrementMinus" + row.ToString();
        this.bindings[button.buttonText + "_UP"] = "IncrementPlus" + row.ToString();
        int count = this.ObjectSink.Count;
        this.AddModButtons((float) row * 0.09f, button);
        for (int index = count; index < this.ObjectSink.Count; ++index)
          this.ObjectSink[index].transform.SetParent(transform, true);
      }
    }
    this.disconnectButton.AddComponent<NativeUiSemanticColor>();
    this.bindings["Disconnect Button"] = "ButtonDisconnect";
    this.bindings["Toggle Search Button"] = "ButtonSearch";
    this.bindings["ReturnButton"] = "ButtonHome";
    this.bindings["<"] = "ButtonPrevious";
    this.bindings[">"] = "ButtonNext";
    foreach (NativeUiClick componentsInChild in this.Variables.menuObj.GetComponentsInChildren<NativeUiClick>(true))
    {
      string key;
      if (this.bindings.TryGetValue(componentsInChild.Name, out key))
        this.snapshot.Controls[key] = ((Component) componentsInChild).transform;
    }
    this.snapshot.Source = this.Variables.menuObj;
    this.snapshot.PhysicalScale = 0.75f;
    this.snapshot.NotificationFrame = this.Variables.background.transform;
    int count1 = this.ObjectSink.Count;
    this.AddModButtonsClassic(0.0f, new NativeNxo.ButtonHandler.Button("Notification", NativeNxo.Category.Other));
    foreach (NativeUiClick componentsInChild in this.Variables.menuObj.GetComponentsInChildren<NativeUiClick>(true))
    {
      if (componentsInChild.Name == "Notification")
        this.snapshot.NotificationTemplate = ((Component) componentsInChild).transform;
    }
    for (int index = count1; index < this.ObjectSink.Count; ++index)
      this.ObjectSink[index].SetActive(false);
    if (state.KeyboardOpen)
      this.BuildNativeKeyboard();
    foreach (Renderer componentsInChild in this.Variables.menuObj.GetComponentsInChildren<Renderer>(true))
    {
      if (((Object) ((Component) componentsInChild).GetComponent<NativeUiThemeRole>() == (Object) null))
        ((Component) componentsInChild).gameObject.AddComponent<NativeUiThemeRole>();
    }
    this.snapshot.HeaderLegacy = this.Variables.title;
    if (state.ColorPickerOpen)
    {
      ((Component) this.snapshot.HomeIcon).gameObject.SetActive(false);
      UnityEngine.UI.Text canvasText = this.CreateCanvasText();
      canvasText.text = state.EditingEnd ? "Done" : "Next";
      ((Graphic) canvasText).rectTransform.sizeDelta = NativeNxo.PageTextSize;
      ((Component) canvasText).transform.localPosition = new Vector3(0.05975f, 0.0f, this.PageZOffset);
      ((Component) canvasText).transform.localRotation = Quaternion.Euler(180f, 90f, 90f);
    }
    this.snapshot.Animate = (Action<float>) (time =>
    {
      this.uiTime = this.menuState.IsOn("Freeze gradient") ? 0.0f : time;
      this.Variables.title.text = this.menuState.IsOn("Freeze gradient") ? this.BuildStaticGradientTitle() : this.BuildAnimatedTitle();
      this.UpdateFpsCounter();
      this.UpdateDisconnectIcon();
    });
    return this.snapshot;
  }
  private void RegisterColorGroup(
    NativeNxo.ColorRole role,
    GameObject baseObj,
    List<GameObject> extraParts)
  {
    Tag(baseObj);
    if (extraParts != null)
    {
      foreach (GameObject extraPart in extraParts)
        Tag(extraPart);
    }
    if (role != NativeNxo.ColorRole.Outline)
      return;
    if (((Object) this.pinwheel == (Object) null))
    {
      this.pinwheel = this.GetPinwheelMaterial();
      this.snapshot.Materials.Add(this.pinwheel);
      this.snapshot.Pinwheels.Add(this.pinwheel);
    }
    baseObj.GetComponent<Renderer>().sharedMaterial = !((Object) this.pinwheel == (Object) null) ? this.pinwheel : throw new InvalidOperationException("Original NXO pinwheel material missing");
    if (extraParts == null)
      return;
    foreach (GameObject extraPart in extraParts)
      extraPart.GetComponent<Renderer>().sharedMaterial = this.pinwheel;
    void Tag(GameObject obj)
    {
      if (((Object) obj == (Object) null))
        return;
      (obj.GetComponent<NativeUiThemeRole>() ?? obj.AddComponent<NativeUiThemeRole>()).Accent = role == NativeNxo.ColorRole.Outline || role == NativeNxo.ColorRole.AccentStrip;
    }
  }
  private void UpdateFpsCounter(bool force = false)
  {
    this.fpsText.text = Application.isPlaying ? "FPS: " + Mathf.RoundToInt(1f / Mathf.Max(Time.smoothDeltaTime, 1f / 1000f)).ToString() : "FPS";
  }
  private GameObject CreatePrimitive(
    PrimitiveType type,
    string name = null,
    Transform parent = null,
    bool keepCollider = false,
    bool triggerCollider = true)
  {
    GameObject primitive = GameObject.CreatePrimitive(type);
    if (name != null)
      ((Object) primitive).name = name;
    Rigidbody component1 = primitive.GetComponent<Rigidbody>();
    if (((Object) component1 != (Object) null))
      NativeUiSnapshot.Destroy((Object) component1);
    Collider component2 = primitive.GetComponent<Collider>();
    if (((Object) component2 != (Object) null))
    {
      if (!keepCollider)
      {
        NativeUiSnapshot.Destroy((Object) component2);
      }
      else
      {
        BoxCollider boxCollider = component2 is BoxCollider ? (BoxCollider) component2 : (BoxCollider) null;
        if (((Object) boxCollider != (Object) null))
          ((Collider) boxCollider).isTrigger = triggerCollider;
      }
    }
    Transform transform1 = primitive.transform;
    object obj = (object) parent;
    if (obj == null)
    {
      GameObject menuObj = this.Variables.menuObj;
      obj = ((Object) menuObj != (Object) null) ? (object) menuObj.transform : (object) (Transform) null;
    }
    Transform transform2 = (Transform) obj;
    transform1.SetParent(transform2, true);
    primitive.transform.localRotation = Quaternion.identity;
    this.ObjectSink.Add(primitive);
    return primitive;
  }
  private GameObject CreateCube(string name = null, Transform parent = null, bool keepCollider = false)
  {
    return this.CreatePrimitive((PrimitiveType) 3, name, parent, keepCollider);
  }
  private GameObject CreateQuad(string name = null, Transform parent = null)
  {
    GameObject primitive = this.CreatePrimitive((PrimitiveType) 5, name, parent);
    ((Collider) primitive.AddComponent<BoxCollider>()).isTrigger = true;
    return primitive;
  }
  private Material TrackMat(Material mat)
  {
    if (((Object) mat != (Object) null))
      this.MaterialSink.Add(mat);
    return mat;
  }
  private void ApplyColor(GameObject obj, Color color)
  {
    Renderer component = obj.GetComponent<Renderer>();
    if (((Object) component == (Object) null))
      return;
    component.sharedMaterial = this.TrackMat(this.CreateColorMaterial(color));
  }
  private void ApplyTexture(GameObject obj, string resourcePath, Color? tint = null)
  {
    Texture2D texture2D = NativeUiResources.LoadNxoTexture(resourcePath);
    if (((Object) texture2D == (Object) null))
      return;
    Material mat = new Material(this.Variables.uiShader)
    {
      mainTexture = (Texture) texture2D
    };
    if (tint.HasValue)
      mat.color = tint.Value;
    obj.GetComponent<Renderer>().sharedMaterial = this.TrackMat(mat);
  }
  private List<GameObject> ApplyOutline(
    GameObject target,
    int requiredMode,
    float zGrow = 0.0125f,
    bool animated = true,
    float roundness = -1f,
    int seg = 5)
  {
    if (this.Settings.OutlineMode < requiredMode)
      return (List<GameObject>) null;
    Color color = animated ? ((Color32)this.Settings.OutlineColor) : Color.Lerp(this.pinwheelColor1, this.pinwheelColor2, 0.5f);
    GameObject outline = this.CreateOutline(target, this.Variables.menuObj.transform, zGrow, color);
    float roundness1 = (double) roundness < 0.0 ? this.Settings.MenuRoundness : roundness;
    List<GameObject> gameObjectList1 = (List<GameObject>) null;
    if ((double) roundness1 > 0.0)
      gameObjectList1 = this.RoundObj(outline, roundness: roundness1, transparentQueue: 2455, seg: seg);
    if (animated)
      this.RegisterColorGroup(NativeNxo.ColorRole.Outline, outline, gameObjectList1);
    else
      this.RegisterColorGroup(NativeNxo.ColorRole.AccentStrip, outline, gameObjectList1);
    List<GameObject> gameObjectList2 = new List<GameObject>(2)
    {
      outline
    };
    if (gameObjectList1 != null)
      gameObjectList2.AddRange((IEnumerable<GameObject>) gameObjectList1);
    return gameObjectList2;
  }
  private void RegisterClickAnim(
    NativeUiClick col,
    GameObject body,
    List<GameObject> rounded,
    List<GameObject> outline,
    params Transform[] uniform)
  {
    if (((Object) col == (Object) null))
      return;
    if (((Object) body != (Object) null))
      col.RegisterBody(body.transform);
    if (rounded != null)
    {
      for (int index = 0; index < rounded.Count; ++index)
      {
        if (((Object) rounded[index] != (Object) null))
          col.RegisterBody(rounded[index].transform);
      }
    }
    if (outline != null)
    {
      for (int index = 0; index < outline.Count; ++index)
      {
        if (((Object) outline[index] != (Object) null))
          col.RegisterBody(outline[index].transform);
      }
    }
    if (uniform == null)
      return;
    for (int index = 0; index < uniform.Length; ++index)
    {
      if (((Object) uniform[index] != (Object) null))
        col.RegisterUniform(uniform[index]);
    }
  }
  private GameObject CreateOutline(GameObject target, Transform parent, float zGrow, Color color)
  {
    GameObject cube = this.CreateCube(parent: parent);
    cube.transform.position = target.transform.position;
    cube.transform.rotation = target.transform.rotation;
    Vector3 localScale = target.transform.localScale;
    Vector3 lossyScale = parent.lossyScale;
    float num = zGrow * (lossyScale.z / lossyScale.y);
    cube.transform.localScale = new Vector3(localScale.x - 1f / 400f, localScale.y + num, localScale.z + zGrow);
    this.ApplyColor(cube, color);
    return cube;
  }
  private void CreateBaseMenuObject()
  {
    this.Variables.menuObj = GameObject.CreatePrimitive((PrimitiveType) 3);
    ((Object) this.Variables.menuObj).name = "menu";
    NativeUiSnapshot.Destroy((Object) this.Variables.menuObj.GetComponent<Rigidbody>());
    NativeUiSnapshot.Destroy((Object) this.Variables.menuObj.GetComponent<BoxCollider>());
    NativeUiSnapshot.Destroy((Object) this.Variables.menuObj.GetComponent<Renderer>());
    this.Variables.menuObj.transform.localScale = NativeNxo.MenuSize;
  }
  private void CreateMenuBackground()
  {
    this.Variables.background = this.CreateCube("menucolor");
    this.Variables.background.transform.localScale = NativeNxo.BackgroundScale;
    this.Variables.background.transform.position = NativeNxo.BackgroundPos;
    this.ApplyColor(this.Variables.background, ((Color32)this.Settings.BackgroundColor));
    this.ApplyOutline(this.Variables.background, 1);
    List<GameObject> extraParts = (List<GameObject>) null;
    if ((double) this.Settings.MenuRoundness > 0.0)
      extraParts = this.RoundObj(this.Variables.background, roundness: this.Settings.MenuRoundness, transparentQueue: 2450);
    this.RegisterColorGroup(NativeNxo.ColorRole.Background, this.Variables.background, extraParts);
  }
  private void CreateMenuCanvasAndTitle()
  {
    this.Variables.canvasObj = new GameObject("canvas");
    this.Variables.canvasObj.transform.parent = this.Variables.menuObj.transform;
    this._trackedObjects.Add(this.Variables.canvasObj);
    Canvas canvas = this.Variables.canvasObj.AddComponent<Canvas>();
    canvas.renderMode = (RenderMode) 2;
    canvas.sortingOrder = 100;
    this.Variables.canvasObj.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 3000f;
    this.Variables.canvasObj.AddComponent<GraphicRaycaster>();
    this.Variables.title = this.CreateCanvasText(4);
    this.Variables.title.alignment = (TextAnchor) 3;
    this.Variables.title.supportRichText = true;
    ((Graphic) this.Variables.title).color = MenuPalette.Left(this.menuState, this.uiTime);
    (float y, float z) titleOffset = this.TitleOffset;
    RectTransform component = ((Component) this.Variables.title).GetComponent<RectTransform>();
    ((Transform) component).localPosition = Vector3.zero;
    ((Transform) component).position = new Vector3(0.054f, titleOffset.y - 0.0065f, titleOffset.z);
    ((Transform) component).localRotation = Quaternion.Euler(180f, 90f, 90f);
    component.sizeDelta = NativeNxo.TitleSize;
  }
  private UnityEngine.UI.Text CreateCanvasText(int fontSize = 3)
  {
    GameObject gameObject = new GameObject();
    gameObject.transform.SetParent(this.Variables.canvasObj.transform, false);
    this.ObjectSink.Add(gameObject);
    UnityEngine.UI.Text canvasText = gameObject.AddComponent<UnityEngine.UI.Text>();
    canvasText.font = this.CurrentFont;
    canvasText.fontStyle = (FontStyle) 0;
    ((Graphic) canvasText).color = Color.white;
    canvasText.fontSize = fontSize;
    canvasText.alignment = (TextAnchor) 4;
    canvasText.resizeTextForBestFit = true;
    canvasText.resizeTextMinSize = 0;
    return canvasText;
  }
  private void CreateAccentStrip()
  {
    if (this.Settings.CurrentAccentStripType == NativeNxo.SettingsData.AccentStripType.Off)
      return;
    GameObject cube1 = this.CreateCube();
    cube1.transform.localScale = NativeNxo.AccentStripScale;
    cube1.transform.localPosition = NativeNxo.AccentStripPos;
    this.ApplyStripColor(cube1, ((Color32)this.Settings.AccentStripColor));
    this.RegisterColorGroup(NativeNxo.ColorRole.AccentStrip, cube1, (List<GameObject>) null);
    if (this.Settings.CurrentAccentStripType != NativeNxo.SettingsData.AccentStripType.Both)
      return;
    GameObject cube2 = this.CreateCube();
    cube2.transform.localScale = NativeNxo.AccentStripScale;
    cube2.transform.localPosition = new Vector3(NativeNxo.AccentStripPos.x, NativeNxo.AccentStripPos.y, -0.27f);
    this.ApplyStripColor(cube2, ((Color32)this.Settings.AccentStripColor2));
    this.RegisterColorGroup(NativeNxo.ColorRole.AccentStrip, cube2, (List<GameObject>) null);
  }
  private void ApplyStripColor(GameObject obj, Color color)
  {
    Renderer component = obj.GetComponent<Renderer>();
    if (((Object) component == (Object) null))
      return;
    Material mat;
    if ((double) this.Settings.MenuOpacity >= 1.0)
    {
      mat = new Material(this.Variables.uberShader)
      {
        color = color
      };
    }
    else
    {
      mat = new Material(this.Variables.uiShader);
      Color color1 = color;
      color1.a = this.Settings.MenuOpacity;
      mat.color = color1;
      mat.renderQueue = 2460;
    }
    component.sharedMaterial = this.TrackMat(mat);
  }
  private void CreateButtonBackdrop(float offset)
  {
    GameObject cube = this.CreateCube("Backdrop");
    cube.transform.localScale = new Vector3(0.0015f, 0.82f, 0.08f);
    cube.transform.localPosition = new Vector3(0.0685f, 0.0f, 0.335f - offset);
    this.ApplyColor(cube, ((Color32)this.Settings.ButtonColor));
    List<GameObject> extraParts = this.RoundObj(cube, roundness: 1.6f, transparentQueue: 2461, seg: 12);
    this.RegisterColorGroup(NativeNxo.ColorRole.Button, cube, extraParts);
  }
  public void AddModButtons(float offset, NativeNxo.ButtonHandler.Button button)
  {
    if (button == null)
      return;
    if (this.Settings.ClassicButtons)
    {
      this.AddModButtonsClassic(offset, button);
    }
    else
    {
      this.CreateButtonBackdrop(offset);
      if (button.incremental)
      {
        this.CreateIncrementalButton("DOWN : " + button.buttonText, button, offset, -0.22f, "<", "_DOWN");
        this.CreateIncrementalButton("UP : " + button.buttonText, button, offset, -0.3525f, ">", "_UP");
      }
      bool flag = !button.showGear;
      GameObject gameObject = (GameObject) null;
      NativeUiClick col = (NativeUiClick) null;
      List<GameObject> gameObjectList = (List<GameObject>) null;
      List<GameObject> outline = (List<GameObject>) null;
      if (!button.incremental)
      {
        float num1 = button.showGear ? 0.695f : 0.15f;
        float num2 = button.showGear ? 1f / 16f : -0.3375f;
        float num3 = button.showGear ? 0.08f : 0.058f;
        gameObject = this.CreateCube("Button: " + button.buttonText, keepCollider: true);
        gameObject.transform.localScale = new Vector3(0.0075f, num1, num3);
        gameObject.transform.localPosition = new Vector3(0.07525f, num2, 0.335f - offset);
        col = gameObject.AddComponent<NativeUiClick>();
        col.Name = button.buttonText;
        this.ApplyColor(gameObject, ((Color32)(button.Enabled ? this.Settings.EnabledButtonColor : this.Settings.ButtonColor)));
        if (button.showGear)
        {
          outline = this.ApplyOutline(gameObject, 2, 0.0065f, false);
          if ((double) this.Settings.MenuRoundness > 0.0)
            gameObjectList = this.RoundObj(gameObject, roundness: this.Settings.MenuRoundness);
        }
        else
        {
          outline = this.ApplyOutline(gameObject, 2, 0.0065f, false, 1.6f, 12);
          gameObjectList = this.RoundObj(gameObject, roundness: 1.6f, transparentQueue: 2462, seg: 12);
        }
        this.RegisterColorGroup(button.Enabled ? NativeNxo.ColorRole.EnabledButton : NativeNxo.ColorRole.Button, gameObject, gameObjectList);
        if (button.showGear)
          this.CreateQuickActionButton(button, offset);
        else
          this.CreateToggleKnob(button, offset, col, gameObject, gameObjectList);
      }
      UnityEngine.UI.Text canvasText = this.CreateCanvasText();
      ((Object) canvasText).name = "Button: Text - " + button.buttonText;
      canvasText.text = NativeNxo.ButtonHandler.FavoriteMods.Contains(button) ? "<color=blue>★</color> " + button.buttonText : button.buttonText;
      if (flag)
        canvasText.alignment = (TextAnchor) 3;
      float num4 = 1.2f;
      ((Component) canvasText).gameObject.transform.localScale = new Vector3(num4, num4, 1f);
      RectTransform component = ((Component) canvasText).GetComponent<RectTransform>();
      float num5 = !flag ? (button.showGear ? 0.015f : 0.0f) : 0.035f;
      ((Transform) component).localPosition = new Vector3(flag ? 0.054f : 0.054f, num5, this.TextOffset - offset / 2.225f);
      ((Transform) component).localRotation = Quaternion.Euler(180f, 90f, 90f);
      Vector2 modTextSize = NativeNxo.ModTextSize;
      component.sizeDelta = new Vector2(flag ? 0.125f : modTextSize.x / num4, modTextSize.y);
      Transform transform = (Transform) null;
      if (!((Object) col != (Object) null))
        return;
      this.RegisterClickAnim(col, gameObject, gameObjectList, outline, flag ? (Transform) null : ((Component) canvasText).transform, transform);
      if (!this.MenuAnimations || !button.isToggle || !button.Enabled || !button.showGear)
        return;
      col.ApplyFactor(0.92f);
    }
  }
  private void CreateToggleKnob(
    NativeNxo.ButtonHandler.Button button,
    float offset,
    NativeUiClick col,
    GameObject pill,
    List<GameObject> pillParts)
  {
    float num1 = 0.0464f;
    float num2 = num1 * (NativeNxo.MenuSize.z / NativeNxo.MenuSize.y);
    float num3 = (float) ((0.15000000596046448 - (double) num2) * 0.5 - 0.0068999999202787876);
    float off = num3 - 0.3375f;
    float on = -0.3375f - num3;
    GameObject cube = this.CreateCube("Knob: " + button.buttonText);
    cube.transform.localScale = new Vector3(0.0075f, num2, num1);
    cube.transform.localPosition = new Vector3(0.07825f, button.Enabled ? on : off, 0.335f - offset);
    Color color = ((Color32)(button.Enabled ? this.Settings.EnabledButtonColor : this.Settings.ButtonColor));
    this.ApplyColor(cube, Color.Lerp(color, Color.white, 0.6f));
    List<GameObject> gameObjectList = this.RoundObj(cube, roundness: 2f, transparentQueue: 2466, seg: 14);
    Transform knob = gameObjectList == null || gameObjectList.Count <= 0 ? cube.transform : gameObjectList[0].transform;
    if (!((Object) col != (Object) null))
      return;
    List<Renderer> parts = new List<Renderer>(2);
    if (pillParts != null)
    {
      foreach (GameObject pillPart in pillParts)
      {
        Renderer component = ((Object) pillPart != (Object) null) ? pillPart.GetComponent<Renderer>() : (Renderer) null;
        if (((Object) component != (Object) null))
          parts.Add(component);
      }
    }
    if (parts.Count == 0 && ((Object) pill != (Object) null))
    {
      Renderer component = pill.GetComponent<Renderer>();
      if (((Object) component != (Object) null))
        parts.Add(component);
    }
    col.SetupPulse(knob, ((Component) knob).GetComponent<Renderer>(), off, on, parts, ((Color32)this.Settings.ButtonColor), ((Color32)this.Settings.EnabledButtonColor));
  }
  private void CreateIncrementalButton(
    string name,
    NativeNxo.ButtonHandler.Button parent,
    float offset,
    float yPos,
    string symbol,
    string suffix)
  {
    GameObject cube = this.CreateCube(name, keepCollider: true);
    cube.transform.localScale = new Vector3(0.0075f, 0.12f, 0.08f);
    cube.transform.localPosition = new Vector3(0.07525f, yPos, 0.335f - offset);
    NativeUiClick col = cube.AddComponent<NativeUiClick>();
    col.Name = new NativeNxo.ButtonHandler.Button(parent.buttonText + suffix, parent.Page).buttonText;
    this.ApplyColor(cube, ((Color32)this.Settings.ButtonColor));
    List<GameObject> outline = this.ApplyOutline(cube, 2, 0.0065f, false);
    List<GameObject> gameObjectList = (List<GameObject>) null;
    if ((double) this.Settings.MenuRoundness > 0.0)
      gameObjectList = this.RoundObj(cube, roundness: this.Settings.MenuRoundness);
    this.RegisterColorGroup(NativeNxo.ColorRole.Button, cube, gameObjectList);
    UnityEngine.UI.Text canvasText = this.CreateCanvasText();
    canvasText.text = symbol;
    ((Component) canvasText).gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
    RectTransform component = ((Component) canvasText).GetComponent<RectTransform>();
    ((Transform) component).localPosition = new Vector3(0.05975f, yPos * 0.288f, this.TextOffset - offset / 2.225f);
    component.sizeDelta = NativeNxo.SubTextSize;
    ((Transform) component).localRotation = Quaternion.Euler(180f, 90f, 90f);
    this.RegisterClickAnim(col, cube, gameObjectList, outline, ((Component) canvasText).transform);
    if (parent != NativeNxo.ButtonHandler._bounceIncParent || suffix == "_UP" != NativeNxo.ButtonHandler._bounceIncUp)
      return;
    NativeNxo.ButtonHandler._bounceIncParent = (NativeNxo.ButtonHandler.Button) null;
    col.PlayBounce();
  }
  public void AddModButtonsClassic(float offset, NativeNxo.ButtonHandler.Button button)
  {
    if (button.incremental)
    {
      this.CreateIncrementalButtonClassic("DOWN : " + button.buttonText, button, offset, 0.3525f, "<", "_DOWN");
      this.CreateIncrementalButtonClassic("UP : " + button.buttonText, button, offset, -0.3525f, ">", "_UP");
    }
    float num1 = !button.incremental ? (button.showGear ? 0.695f : 0.82f) : 0.555f;
    float num2 = !button.showGear || button.incremental ? 0.0f : 1f / 16f;
    GameObject cube = this.CreateCube("Button: " + button.buttonText, keepCollider: true);
    cube.transform.localScale = new Vector3(0.0075f, num1, 0.08f);
    cube.transform.localPosition = new Vector3(0.07525f, num2, 0.335f - offset);
    NativeUiClick col = cube.AddComponent<NativeUiClick>();
    col.Name = button.buttonText;
    this.ApplyColor(cube, ((Color32)(button.Enabled ? this.Settings.EnabledButtonColor : this.Settings.ButtonColor)));
    List<GameObject> outline = this.ApplyOutline(cube, 2, 0.0065f);
    List<GameObject> gameObjectList = (List<GameObject>) null;
    if ((double) this.Settings.MenuRoundness > 0.0)
      gameObjectList = this.RoundObj(cube, roundness: this.Settings.MenuRoundness);
    this.RegisterColorGroup(button.Enabled ? NativeNxo.ColorRole.EnabledButton : NativeNxo.ColorRole.Button, cube, gameObjectList);
    if (button.showGear && !button.incremental)
      this.CreateQuickActionButton(button, offset);
    UnityEngine.UI.Text canvasText1 = this.CreateCanvasText();
    ((Object) canvasText1).name = "Button: Text - " + button.buttonText;
    canvasText1.text = NativeNxo.ButtonHandler.FavoriteMods.Contains(button) ? "<color=blue>★</color> " + button.buttonText : button.buttonText;
    float num3 = 1.2f;
    ((Component) canvasText1).gameObject.transform.localScale = new Vector3(num3, num3, 1f);
    RectTransform component1 = ((Component) canvasText1).GetComponent<RectTransform>();
    ((Transform) component1).localPosition = new Vector3(0.05975f, button.showGear ? 0.015f : 0.0f, this.TextOffset - offset / 2.225f);
    ((Transform) component1).localRotation = Quaternion.Euler(180f, 90f, 90f);
    Vector2 vector2 = button.incremental ? NativeNxo.IncrementalTextSize : NativeNxo.ModTextSize;
    component1.sizeDelta = new Vector2(vector2.x / num3, vector2.y);
    Transform transform = (Transform) null;
    if (NativeNxo.ButtonHandler.IsCategoryButton(button))
    {
      UnityEngine.UI.Text canvasText2 = this.CreateCanvasText();
      ((Object) canvasText2).name = "Button: Arrow - " + button.buttonText;
      canvasText2.text = button.buttonText == "Return" ? "<color=red><</color>" : $"<color=#{ColorUtility.ToHtmlStringRGB(((Color32)this.Settings.EnabledButtonColor))}>></color>";
      canvasText2.alignment = (TextAnchor) 3;
      ((Component) canvasText2).gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
      RectTransform component2 = ((Component) canvasText2).GetComponent<RectTransform>();
      ((Transform) component2).localPosition = new Vector3(0.05975f, button.showGear ? 0.012f : -0.01125f, this.TextOffset - offset / 2.225f);
      ((Transform) component2).localRotation = Quaternion.Euler(180f, 90f, 90f);
      component2.sizeDelta = button.incremental ? NativeNxo.IncrementalTextSize : NativeNxo.ModTextSize;
      transform = ((Component) canvasText2).transform;
    }
    this.RegisterClickAnim(col, cube, gameObjectList, outline, ((Component) canvasText1).transform, transform);
    if (button != NativeNxo.ButtonHandler._bouncePending)
      return;
    NativeNxo.ButtonHandler._bouncePending = (NativeNxo.ButtonHandler.Button) null;
    col.PlayBounce();
  }
  private void CreateIncrementalButtonClassic(
    string name,
    NativeNxo.ButtonHandler.Button parent,
    float offset,
    float yPos,
    string symbol,
    string suffix)
  {
    GameObject cube = this.CreateCube(name, keepCollider: true);
    cube.transform.localScale = new Vector3(0.0075f, 0.12f, 0.08f);
    cube.transform.localPosition = new Vector3(0.07525f, yPos, 0.335f - offset);
    NativeUiClick col = cube.AddComponent<NativeUiClick>();
    col.Name = new NativeNxo.ButtonHandler.Button(parent.buttonText + suffix, parent.Page).buttonText;
    this.ApplyColor(cube, ((Color32)this.Settings.ButtonColor));
    List<GameObject> outline = this.ApplyOutline(cube, 2, 0.0065f);
    List<GameObject> gameObjectList = (List<GameObject>) null;
    if ((double) this.Settings.MenuRoundness > 0.0)
      gameObjectList = this.RoundObj(cube, roundness: this.Settings.MenuRoundness);
    this.RegisterColorGroup(NativeNxo.ColorRole.Button, cube, gameObjectList);
    UnityEngine.UI.Text canvasText = this.CreateCanvasText();
    canvasText.text = symbol;
    ((Component) canvasText).gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
    RectTransform component = ((Component) canvasText).GetComponent<RectTransform>();
    float incrementTextOffset = this.IncrementTextOffset;
    ((Transform) component).localPosition = new Vector3(0.05975f, (double) yPos < 0.0 ? -0.1025f : 0.1015f, incrementTextOffset - offset / 2.225f);
    component.sizeDelta = NativeNxo.SubTextSize;
    ((Transform) component).localRotation = Quaternion.Euler(180f, 90f, 90f);
    this.RegisterClickAnim(col, cube, gameObjectList, outline, ((Component) canvasText).transform);
    if (parent != NativeNxo.ButtonHandler._bounceIncParent || suffix == "_UP" != NativeNxo.ButtonHandler._bounceIncUp)
      return;
    NativeNxo.ButtonHandler._bounceIncParent = (NativeNxo.ButtonHandler.Button) null;
    col.PlayBounce();
  }
  private void CreateQuickActionButton(NativeNxo.ButtonHandler.Button parent, float offset)
  {
    GameObject cube = this.CreateCube("Gear: " + parent.buttonText, keepCollider: true);
    cube.transform.localScale = new Vector3(0.0075f, 0.12f, 0.08f);
    cube.transform.localPosition = new Vector3(0.07525f, -0.3525f, 0.335f - offset);
    NativeUiClick col = cube.AddComponent<NativeUiClick>();
    col.Name = new NativeNxo.ButtonHandler.Button("_GEAR_" + parent.buttonText, parent.Page, action: (Action) (() => { })).buttonText;
    this.ApplyColor(cube, ((Color32)this.Settings.ButtonColor));
    List<GameObject> outline = this.ApplyOutline(cube, 2, 0.0065f, false);
    List<GameObject> gameObjectList = (List<GameObject>) null;
    if ((double) this.Settings.MenuRoundness > 0.0)
      gameObjectList = this.RoundObj(cube, roundness: this.Settings.MenuRoundness);
    this.RegisterColorGroup(NativeNxo.ColorRole.Button, cube, gameObjectList);
    UnityEngine.UI.Text canvasText = this.CreateCanvasText();
    canvasText.text = "⚙";
    ((Component) canvasText).gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
    RectTransform component = ((Component) canvasText).GetComponent<RectTransform>();
    ((Transform) component).localPosition = new Vector3(0.05975f, -0.1055f, this.TextOffset - offset / 2.225f);
    ((Transform) component).localRotation = Quaternion.Euler(180f, 90f, 90f);
    component.sizeDelta = NativeNxo.GearIconSize;
    this.RegisterClickAnim(col, cube, gameObjectList, outline, ((Component) canvasText).transform);
  }
  private void AddPageButton(string direction, float yPos)
  {
    GameObject cube = this.CreateCube(keepCollider: true);
    cube.transform.localScale = new Vector3(0.0075f, 0.25f, 0.08f);
    cube.transform.localPosition = new Vector3(0.07525f, yPos, -0.33f);
    NativeUiClick col = cube.AddComponent<NativeUiClick>();
    col.Name = new NativeNxo.ButtonHandler.Button(direction, NativeNxo.Category.Home).buttonText;
    this.ApplyColor(cube, ((Color32)this.Settings.ButtonColor));
    List<GameObject> outline = this.ApplyOutline(cube, 2, 0.0065f);
    List<GameObject> gameObjectList = (List<GameObject>) null;
    if ((double) this.Settings.MenuRoundness > 0.0)
      gameObjectList = this.RoundObj(cube, roundness: this.Settings.MenuRoundness);
    this.RegisterColorGroup(NativeNxo.ColorRole.Button, cube, gameObjectList);
    UnityEngine.UI.Text canvasText = this.CreateCanvasText();
    canvasText.text = direction;
    ((Component) canvasText).gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
    RectTransform component = ((Component) canvasText).GetComponent<RectTransform>();
    ((Transform) component).localPosition = Vector3.zero;
    component.sizeDelta = NativeNxo.PageTextSize;
    ((Transform) component).localRotation = Quaternion.Euler(180f, 90f, 90f);
    ((Transform) component).localPosition = new Vector3(0.05975f, direction == "<" ? 0.083f : -0.083f, this.PageZOffset);
    this.RegisterClickAnim(col, cube, gameObjectList, outline, ((Component) canvasText).transform);
  }
  public void AddReturnButton()
  {
    this.Variables.BackToStartButton = this.CreateCube(keepCollider: true);
    this.Variables.BackToStartButton.transform.localScale = new Vector3(0.0075f, 0.2925f, 0.08f);
    this.Variables.BackToStartButton.transform.localPosition = new Vector3(0.07525f, 0.0f, -0.33f);
    NativeUiClick col = this.Variables.BackToStartButton.AddComponent<NativeUiClick>();
    col.Name = new NativeNxo.ButtonHandler.Button("ReturnButton", NativeNxo.Category.Home).buttonText;
    this.ApplyColor(this.Variables.BackToStartButton, ((Color32)this.Settings.ButtonColor));
    List<GameObject> outline = this.ApplyOutline(this.Variables.BackToStartButton, 2, 0.0065f);
    List<GameObject> gameObjectList = (List<GameObject>) null;
    if ((double) this.Settings.MenuRoundness > 0.0)
      gameObjectList = this.RoundObj(this.Variables.BackToStartButton, roundness: this.Settings.MenuRoundness);
    this.RegisterColorGroup(NativeNxo.ColorRole.Button, this.Variables.BackToStartButton, gameObjectList);
    GameObject quad = this.CreateQuad(parent: this.Variables.canvasObj.transform);
    quad.transform.localScale = NativeNxo.HomeIconScale;
    quad.transform.localPosition = new Vector3(0.05975f, 0.0f, -0.14875f);
    quad.transform.localRotation = Quaternion.Euler(0.0f, -90f, -90f);
    this.ApplyTexture(quad, "NXO.Resources.homeicon.png");
    this.snapshot.HomeIcon = quad.transform;
    this.RegisterClickAnim(col, this.Variables.BackToStartButton, gameObjectList, outline, quad.transform);
  }
  public void AddDisconnectButton()
  {
    GameObject cube = this.CreateCube("DisconnectBg", keepCollider: true);
    cube.transform.localScale = new Vector3(0.0075f, NativeNxo.HeaderIconScale.x * 2.504f, NativeNxo.HeaderIconScale.x * 1.6f);
    cube.transform.localPosition = new Vector3(0.07525f, -0.2375f, 0.45f);
    NativeUiClick col = cube.AddComponent<NativeUiClick>();
    col.Name = new NativeNxo.ButtonHandler.Button("Disconnect Button", NativeNxo.Category.Home, action: (Action) (() => { })).buttonText;
    this.ApplyColor(cube, ((Color32)this.Settings.ButtonColor));
    List<GameObject> outline = this.ApplyOutline(cube, 2, 0.0065f, false);
    List<GameObject> gameObjectList = (List<GameObject>) null;
    if ((double) this.Settings.MenuRoundness > 0.0)
      gameObjectList = this.RoundObj(cube, roundness: this.Settings.MenuRoundness);
    this.RegisterColorGroup(NativeNxo.ColorRole.Button, cube, gameObjectList);
    this.disconnectButton = this.CreateQuad();
    this.disconnectButton.transform.localRotation = Quaternion.Euler(0.0f, 90f, 0.0f);
    this.disconnectButton.transform.localScale = NativeNxo.HeaderIconScale;
    this.disconnectButton.transform.localPosition = new Vector3(0.0795357153f, -0.2375f, 0.45f);
    this.ApplyTexture(this.disconnectButton, "NXO.Resources.disconnecticon.png", new Color?(Color.white));
    this.UpdateDisconnectIcon();
    this.RegisterClickAnim(col, cube, gameObjectList, outline, this.disconnectButton.transform);
  }
  public void AddFpsButton()
  {
    float num1 = NativeNxo.HeaderIconScale.x * 2.504f;
    float num2 = 0.1125f - num1;
    float num3 = num1 * 3.3f;
    float num4 = (float) ((double) num1 * 0.5 - 0.23749999701976776 + (double) num2 + (double) num3 * 0.5);
    GameObject cube = this.CreateCube("FpsBg");
    cube.transform.localScale = new Vector3(0.0075f, num3, NativeNxo.HeaderIconScale.x * 1.6f);
    cube.transform.localPosition = new Vector3(0.07525f, num4, 0.45f);
    this.ApplyColor(cube, ((Color32)this.Settings.ButtonColor));
    this.ApplyOutline(cube, 2, 0.0065f, false);
    List<GameObject> extraParts = (List<GameObject>) null;
    if ((double) this.Settings.MenuRoundness > 0.0)
      extraParts = this.RoundObj(cube, roundness: this.Settings.MenuRoundness);
    this.RegisterColorGroup(NativeNxo.ColorRole.Button, cube, extraParts);
    this.fpsButton = cube;
    this.fpsText = this.CreateCanvasText();
    ((Object) this.fpsText).name = "FPS Text";
    this.fpsText.alignment = (TextAnchor) 4;
    RectTransform component = ((Component) this.fpsText).GetComponent<RectTransform>();
    ((Transform) component).localPosition = Vector3.zero;
    ((Transform) component).position = new Vector3(0.0795357153f * NativeNxo.MenuSize.x, num4 * NativeNxo.MenuSize.y, 0.45f * NativeNxo.MenuSize.z);
    ((Transform) component).localRotation = Quaternion.Euler(180f, 90f, 90f);
    component.sizeDelta = new Vector2(0.12f, 0.013f);
    this.UpdateFpsCounter(true);
  }
  public void ToggleSearchButton()
  {
    if (((Object) this.Variables.searchButton != (Object) null))
      return;
    GameObject cube = this.CreateCube("SearchBg", keepCollider: true);
    cube.transform.localScale = new Vector3(0.0075f, NativeNxo.HeaderIconScale.x * 2.504f, NativeNxo.HeaderIconScale.x * 1.6f);
    cube.transform.localPosition = new Vector3(0.07525f, -0.35f, 0.45f);
    NativeUiClick col = cube.AddComponent<NativeUiClick>();
    col.Name = new NativeNxo.ButtonHandler.Button("Toggle Search Button", NativeNxo.Category.Home).buttonText;
    this.ApplyColor(cube, ((Color32)(this.SearchAndKeyboard.isSearching ? this.Settings.EnabledButtonColor : this.Settings.ButtonColor)));
    List<GameObject> outline = this.ApplyOutline(cube, 2, 0.0065f, false);
    List<GameObject> gameObjectList = (List<GameObject>) null;
    if ((double) this.Settings.MenuRoundness > 0.0)
      gameObjectList = this.RoundObj(cube, roundness: this.Settings.MenuRoundness);
    this.RegisterColorGroup(this.SearchAndKeyboard.isSearching ? NativeNxo.ColorRole.EnabledButton : NativeNxo.ColorRole.Button, cube, gameObjectList);
    this.Variables.searchButton = this.CreateQuad();
    this.Variables.searchButton.transform.localRotation = Quaternion.Euler(0.0f, 90f, 0.0f);
    this.Variables.searchButton.transform.localScale = NativeNxo.HeaderIconScale;
    this.Variables.searchButton.transform.localPosition = new Vector3(0.0795357153f, -0.35f, 0.45f);
    this.ApplyTexture(this.Variables.searchButton, "NXO.Resources.searchicon.png");
    this.RegisterClickAnim(col, cube, gameObjectList, outline, this.Variables.searchButton.transform);
  }
  public void AddSearchBar()
  {
    if (!this.SearchAndKeyboard.isSearching)
      return;
    this._drawingSearchBar = true;
    this.Variables.TopMenuButton = this.CreateCube(keepCollider: true);
    this.Variables.TopMenuButton.transform.localScale = NativeNxo.SearchBarScale;
    this.Variables.TopMenuButton.transform.localPosition = NativeNxo.SearchBarPos;
    this.ApplyColor(this.Variables.TopMenuButton, ((Color32)this.Settings.BackgroundColor));
    this.ApplyOutline(this.Variables.TopMenuButton, 1);
    List<GameObject> extraParts = (List<GameObject>) null;
    if ((double) this.Settings.MenuRoundness > 0.0)
      extraParts = this.RoundObj(this.Variables.TopMenuButton, roundness: this.Settings.MenuRoundness);
    this.RegisterColorGroup(NativeNxo.ColorRole.Background, this.Variables.TopMenuButton, extraParts);
    this.SearchAndKeyboard.searchText = this.CreateCanvasText();
    this.SearchAndKeyboard.searchText.alignment = (TextAnchor) 4;
    RectTransform component = ((Component) this.SearchAndKeyboard.searchText).GetComponent<RectTransform>();
    component.sizeDelta = new Vector2(0.2f, 0.02f);
    ((Transform) component).localScale = new Vector3(0.9f, 0.9f, 0.9f);
    ((Transform) component).localRotation = Quaternion.Euler(180f, 90f, 90f);
    ((Transform) component).localPosition = NativeNxo.SearchTextPos;
    this.SearchAndKeyboard.searchText.text = !string.IsNullOrEmpty(this.SearchAndKeyboard.inputText) ? this.SearchAndKeyboard.inputText : this.SearchAndKeyboard.placeholderText;
    this._drawingSearchBar = false;
  }
  public List<GameObject> RoundObj(
    GameObject toRound,
    Transform parent = null,
    List<Material> materialList = null,
    float roundness = 0.5f,
    int transparentQueue = 2460,
    int seg = 5)
  {
    List<GameObject> gameObjectList = new List<GameObject>(1);
    if (((Object) toRound == (Object) null))
      return gameObjectList;
    Renderer component = toRound.GetComponent<Renderer>();
    if (((Object) component == (Object) null))
      return gameObjectList;
    if (((Object) parent == (Object) null))
      parent = this.Variables.menuObj.transform;
    if (materialList == null)
      materialList = this.MaterialSink;
    Vector3 localScale = toRound.transform.localScale;
    Vector3 lossyScale = parent.lossyScale;
    float num1 = localScale.x * 0.5f;
    float num2 = localScale.y * 0.5f * lossyScale.y;
    float num3 = localScale.z * 0.5f * lossyScale.z;
    float num4 = Mathf.Min(7f / 400f * roundness, Mathf.Min(num2, num3) * 0.9f);
    float num5 = (num2 - num4) / lossyScale.y;
    float num6 = (num3 - num4) / lossyScale.z;
    float num7 = num4 / lossyScale.y;
    float num8 = num4 / lossyScale.z;
    List<Vector2> vector2List = new List<Vector2>((seg + 1) * 4);
    float[] numArray1 = new float[4]
    {
      num5,
      0.0f - num5,
      0.0f - num5,
      num5
    };
    float[] numArray2 = new float[4]
    {
      num6,
      num6,
      0.0f - num6,
      0.0f - num6
    };
    for (int index1 = 0; index1 < 4; ++index1)
    {
      float num9 = (float) ((double) index1 * 3.1415927410125732 * 0.5);
      float num10 = numArray1[index1];
      float num11 = numArray2[index1];
      for (int index2 = 0; index2 <= seg; ++index2)
      {
        float num12 = Mathf.Lerp(num9, num9 + 1.57079637f, (float) index2 / (float) seg);
        vector2List.Add(new Vector2(num10 + Mathf.Cos(num12) * num7, num11 + Mathf.Sin(num12) * num8));
      }
    }
    int count1 = vector2List.Count;
    List<Vector3> vector3List1 = new List<Vector3>(count1 * 4 + 4);
    List<Vector3> vector3List2 = new List<Vector3>(count1 * 4 + 4);
    List<int> intList = new List<int>(count1 * 12);
    int count2 = vector3List1.Count;
    vector3List1.Add(new Vector3(num1, 0.0f, 0.0f));
    vector3List2.Add(Vector3.right);
    int count3 = vector3List1.Count;
    for (int index = 0; index < count1; ++index)
    {
      vector3List1.Add(new Vector3(num1, vector2List[index].x, vector2List[index].y));
      vector3List2.Add(Vector3.right);
    }
    for (int index = 0; index < count1; ++index)
    {
      intList.Add(count2);
      intList.Add(count3 + (index + 1) % count1);
      intList.Add(count3 + index);
    }
    int count4 = vector3List1.Count;
    vector3List1.Add(new Vector3(0.0f - num1, 0.0f, 0.0f));
    vector3List2.Add(Vector3.left);
    int count5 = vector3List1.Count;
    for (int index = 0; index < count1; ++index)
    {
      vector3List1.Add(new Vector3(0.0f - num1, vector2List[index].x, vector2List[index].y));
      vector3List2.Add(Vector3.left);
    }
    for (int index = 0; index < count1; ++index)
    {
      intList.Add(count4);
      intList.Add(count5 + index);
      intList.Add(count5 + (index + 1) % count1);
    }
    int count6 = vector3List1.Count;
    for (int index = 0; index < count1; ++index)
    {
      Vector3 vector3;
      vector3 = new Vector3(0.0f, vector2List[index].x, vector2List[index].y);
      Vector3 normalized = (vector3).normalized;
      vector3List1.Add(new Vector3(num1, vector2List[index].x, vector2List[index].y));
      vector3List2.Add(normalized);
      vector3List1.Add(new Vector3(0.0f - num1, vector2List[index].x, vector2List[index].y));
      vector3List2.Add(normalized);
    }
    for (int index = 0; index < count1; ++index)
    {
      int num13 = (index + 1) % count1;
      int num14 = count6 + index * 2;
      int num15 = num14 + 1;
      int num16 = count6 + num13 * 2;
      int num17 = num16 + 1;
      intList.Add(num14);
      intList.Add(num16);
      intList.Add(num15);
      intList.Add(num16);
      intList.Add(num17);
      intList.Add(num15);
    }
    int count7 = intList.Count;
    for (int index = 0; index < count7; index += 3)
    {
      intList.Add(intList[index]);
      intList.Add(intList[index + 2]);
      intList.Add(intList[index + 1]);
    }
    Mesh mesh1 = new Mesh();
    ((Object) mesh1).name = "RoundedMesh";
    Mesh mesh2 = mesh1;
    mesh2.SetVertices(vector3List1);
    mesh2.SetNormals(vector3List2);
    mesh2.SetTriangles(intList, 0);
    mesh2.RecalculateBounds();
    this.MeshSink.Add(mesh2);
    GameObject gameObject = new GameObject("rounded_" + ((Object) toRound).name);
    gameObject.transform.SetParent(parent, false);
    gameObject.transform.localPosition = toRound.transform.localPosition;
    gameObject.transform.localRotation = toRound.transform.localRotation;
    gameObject.transform.localScale = Vector3.one;
    gameObject.AddComponent<MeshFilter>().mesh = mesh2;
    Material material;
    if ((double) this.Settings.MenuOpacity >= 1.0)
    {
      material = new Material(this.Variables.uberShader)
      {
        color = component.sharedMaterial.color
      };
    }
    else
    {
      material = new Material(this.Variables.uiShader);
      Color color = component.sharedMaterial.color;
      color.a = this.Settings.MenuOpacity;
      material.color = color;
      material.renderQueue = transparentQueue;
      material.SetInt("_Cull", 0);
    }
    ((Renderer) gameObject.AddComponent<MeshRenderer>()).sharedMaterial = material;
    materialList.Add(material);
    this.ObjectSink.Add(gameObject);
    component.enabled = false;
    gameObjectList.Add(gameObject);
    return gameObjectList;
  }
  public Material CreateColorMaterial(Color color, int renderQueue = 2460)
  {
    if ((double) this.Settings.MenuOpacity >= 1.0)
      return new Material(this.Variables.uberShader)
      {
        color = color
      };
    Material colorMaterial = new Material(this.Variables.uiShader);
    Color color1 = color;
    color1.a = this.Settings.MenuOpacity;
    colorMaterial.color = color1;
    colorMaterial.renderQueue = renderQueue;
    return colorMaterial;
  }
  private string BuildStaticGradientTitle()
  {
    this._titleBuilder.Clear();
    this._titleBuilder.Append("<b>");
    string str = "NXO";
    for (int index = 0; index < str.Length; ++index)
    {
      float num = str.Length > 1 ? (float) index / (float) (str.Length - 1) : 0.0f;
      Color color = Color.Lerp(MenuPalette.Left(this.menuState, this.uiTime), MenuPalette.Right(this.menuState, this.uiTime), num);
      this._titleBuilder.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(color)).Append('>').Append(str[index]).Append("</color>");
    }
    this._titleBuilder.Append("</b>");
    return this._titleBuilder.ToString();
  }
  private string BuildAnimatedTitle()
  {
    this._titleBuilder.Clear();
    this._titleBuilder.Append("<b>");
    string str = "NXO";
    for (int index = 0; index < str.Length; ++index)
    {
      float num = (float) (((double) Mathf.Sin((float) ((double) this.uiTime * (double) this.Settings.TitleAnimSpeed * 2.0 - (double) index * 0.5)) + 1.0) / 2.0);
      Color color = Color.Lerp(MenuPalette.Left(this.menuState, this.uiTime), MenuPalette.Right(this.menuState, this.uiTime), num);
      this._titleBuilder.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(color)).Append('>').Append(str[index]).Append("</color>");
    }
    this._titleBuilder.Append("</b>");
    return this._titleBuilder.ToString();
  }
  public Material GetPinwheelMaterial()
  {
    if (((Object) this._cachedPinwheelBase == (Object) null))
      this._cachedPinwheelBase = ImportedAssetMenu.Load("nxo-pinwheel").LoadAsset<Material>("outline2");
    if (((Object) this._cachedPinwheelBase == (Object) null))
    {
      Debug.LogError((object) "Failed to load pinwheel material");
      return (Material) null;
    }
    Material pinwheelMaterial = new Material(this._cachedPinwheelBase);
    pinwheelMaterial.SetFloat("_Speed", 0.0f - this.pinwheelSpeed);
    pinwheelMaterial.SetColor("_COLOR1", this.pinwheelColor1);
    pinwheelMaterial.SetColor("_COLOR2", this.pinwheelColor2);
    return pinwheelMaterial;
  }
  public void UpdateDisconnectIcon()
  {
    if (((Object) this.disconnectButton == (Object) null))
      return;
    Renderer component = this.disconnectButton.GetComponent<Renderer>();
    if (((Object) component == (Object) null) || ((Object) component.sharedMaterial == (Object) null))
      return;
    component.sharedMaterial.color = ((Color32)(this.menuState.InRoom ? new Color32((byte) 235, (byte) 70, (byte) 60, byte.MaxValue) : new Color32((byte) 70, (byte) 210, (byte) 95, byte.MaxValue)));
  }
  private void RegisterKeyboardColorGroup(
    NativeNxo.ColorRole role,
    GameObject obj,
    List<GameObject> parts)
  {
    this.RegisterColorGroup(role, obj, parts);
  }
  private void BuildNativeKeyboard()
  {
    this.DrawKeyboard();
    this.keyboardObj.transform.SetPositionAndRotation(new Vector3(0.085f, 0.0f, -0.015f), Quaternion.LookRotation(Vector3.left, Vector3.forward));
    this.keyboardObj.transform.SetParent(this.Variables.menuObj.transform, true);
    foreach (KeyValuePair<string, NativeUiKey> keyCollider in this._keyColliders)
      this.snapshot.Controls["Keyboard/Key_" + (keyCollider.Key == "SPACE" ? "Space" : (keyCollider.Key == "BACK" ? "Back" : (keyCollider.Key == "ENTER" ? "Done" : keyCollider.Key)))] = ((Component) keyCollider.Value).transform;
    this.snapshot.Materials.AddRange((IEnumerable<Material>) this._keyboardMaterials);
    this.snapshot.Meshes.AddRange((IEnumerable<Mesh>) this._keyboardMeshes);
  }
  private List<GameObject> RoundKeyboard(GameObject panel, float roundness)
  {
    List<GameObject> gameObjectList = new List<GameObject>(1);
    if (((Object) panel == (Object) null))
      return gameObjectList;
    Renderer component = panel.GetComponent<Renderer>();
    if (((Object) component == (Object) null))
      return gameObjectList;
    Transform transform = panel.transform.parent ?? this.keyboardObj.transform;
    GameObject gameObject = new GameObject("rounded_" + ((Object) panel).name);
    gameObject.transform.SetParent(transform, false);
    gameObject.transform.localPosition = panel.transform.localPosition;
    gameObject.transform.localRotation = panel.transform.localRotation;
    gameObject.transform.localScale = Vector3.one;
    gameObject.layer = panel.layer;
    Mesh mesh = this.KeyboardRoundObj(panel.transform.localScale, roundness);
    this._keyboardMeshes.Add(mesh);
    gameObject.AddComponent<MeshFilter>().mesh = mesh;
    Material material = new Material(component.sharedMaterial);
    material.SetInt("_Cull", 0);
    ((Renderer) gameObject.AddComponent<MeshRenderer>()).sharedMaterial = material;
    this._keyboardMaterials.Add(material);
    this._keyboardObjects.Add(gameObject);
    component.enabled = false;
    gameObjectList.Add(gameObject);
    return gameObjectList;
  }
  private Material CreateMaterial(Color color, int renderQueue)
  {
    return this.CreateColorMaterial(color, renderQueue);
  }
  private Material CreateOutlineMaterial()
  {
    if (this.Settings.OutlineColorMode == NativeNxo.SettingsData.ColorMode.Pinwheel)
    {
      Material pinwheelMaterial = this.GetPinwheelMaterial();
      if (((Object) pinwheelMaterial != (Object) null))
      {
        this._keyboardMaterials.Add(pinwheelMaterial);
        return pinwheelMaterial;
      }
    }
    return this.CreateMaterial(((Color32)this.Settings.OutlineColor), 2455);
  }
  private Material CreateBackgroundMaterial()
  {
    if (this.Settings.BackgroundMode == NativeNxo.SettingsData.ColorMode.Pinwheel)
    {
      Material pinwheelMaterial = this.GetPinwheelMaterial();
      if (((Object) pinwheelMaterial != (Object) null))
      {
        this._keyboardMaterials.Add(pinwheelMaterial);
        return pinwheelMaterial;
      }
    }
    Material material = this.CreateMaterial(((Color32)this.Settings.BackgroundColor), 2450);
    this._keyboardMaterials.Add(material);
    return material;
  }
  private Material CreateButtonMaterial()
  {
    if (this.Settings.ButtonMode == NativeNxo.SettingsData.ColorMode.Pinwheel)
    {
      Material pinwheelMaterial = this.GetPinwheelMaterial();
      if (((Object) pinwheelMaterial != (Object) null))
      {
        this._keyboardMaterials.Add(pinwheelMaterial);
        return pinwheelMaterial;
      }
    }
    Material material = this.CreateMaterial(((Color32)this.Settings.ButtonColor), 2460);
    this._keyboardMaterials.Add(material);
    return material;
  }
  private GameObject CreateKeyboardPrimitive(Transform parent)
  {
    GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
    NativeUiSnapshot.Destroy((Object) primitive.GetComponent<Rigidbody>());
    NativeUiSnapshot.Destroy((Object) primitive.GetComponent<BoxCollider>());
    primitive.layer = LayerMask.NameToLayer("UI");
    primitive.transform.SetParent(parent, false);
    primitive.transform.localRotation = Quaternion.identity;
    this._keyboardObjects.Add(primitive);
    return primitive;
  }
  public void DrawKeyboard()
  {
    if (((Object) this.keyboardObj != (Object) null))
    {
      NativeUiSnapshot.Destroy((Object) this.keyboardObj);
      this.keyboardObj = (GameObject) null;
    }
    foreach (Material keyboardMaterial in this._keyboardMaterials)
    {
      if (((Object) keyboardMaterial != (Object) null))
        NativeUiSnapshot.Destroy((Object) keyboardMaterial);
    }
    this._keyboardMaterials.Clear();
    foreach (GameObject keyboardObject in this._keyboardObjects)
    {
      if (((Object) keyboardObject != (Object) null))
        NativeUiSnapshot.Destroy((Object) keyboardObject);
    }
    this._keyboardObjects.Clear();
    foreach (Mesh keyboardMesh in this._keyboardMeshes)
    {
      if (((Object) keyboardMesh != (Object) null))
        NativeUiSnapshot.Destroy((Object) keyboardMesh);
    }
    this._keyboardMeshes.Clear();
    this._keyboardColorGroups.Clear();
    this._keyColliders.Clear();
    this.keyboardObj = new GameObject("Keyboard");
    if (this.Settings.OutlineMode > 0)
    {
      GameObject keyboardPrimitive = this.CreateKeyboardPrimitive(this.keyboardObj.transform);
      ((Object) keyboardPrimitive).name = "KeyboardOutline";
      keyboardPrimitive.transform.localScale = new Vector3(0.5f, 0.255f, 0.004f);
      keyboardPrimitive.transform.localPosition = new Vector3(0.0f, 1f / 500f, 3f / 500f);
      this._outlineMaterial = this.CreateOutlineMaterial();
      this._keyboardMaterials.Add(this._outlineMaterial);
      keyboardPrimitive.GetComponent<Renderer>().sharedMaterial = this._outlineMaterial;
      List<GameObject> parts = (List<GameObject>) null;
      if ((double) this.Settings.MenuRoundness > 0.0)
        parts = this.RoundKeyboard(keyboardPrimitive, 0.015f * this.Settings.MenuRoundness);
      this.RegisterKeyboardColorGroup(NativeNxo.ColorRole.Outline, keyboardPrimitive, parts);
    }
    GameObject keyboardPrimitive1 = this.CreateKeyboardPrimitive(this.keyboardObj.transform);
    ((Object) keyboardPrimitive1).name = "KeyboardBackground";
    keyboardPrimitive1.transform.localScale = new Vector3(0.485f, 0.24f, 0.005f);
    keyboardPrimitive1.transform.localPosition = new Vector3(0.0f, 1f / 500f, 0.005f);
    this._keyboardBgMaterial = this.CreateBackgroundMaterial();
    keyboardPrimitive1.GetComponent<Renderer>().sharedMaterial = this._keyboardBgMaterial;
    List<GameObject> parts1 = (List<GameObject>) null;
    if ((double) this.Settings.MenuRoundness > 0.0)
      parts1 = this.RoundKeyboard(keyboardPrimitive1, 0.015f * this.Settings.MenuRoundness);
    this.RegisterKeyboardColorGroup(NativeNxo.ColorRole.Background, keyboardPrimitive1, parts1);
    string[] strArray = new string[4]
    {
      "1234567890",
      "QWERTYUIOP",
      "ASDFGHJKL",
      "ZXCVBNM"
    };
    float width1 = 0.04f;
    float num1 = 0.005f;
    float num2 = 0.092f;
    foreach (string str in strArray)
    {
      float num3 = (float) (0.0 - ((double) str.Length * (double) width1 + (double) (str.Length - 1) * (double) num1) / 2.0 + (double) width1 / 2.0);
      foreach (char ch in str)
      {
        this.CreateKey(ch.ToString(), new Vector3(num3, num2, 0.0f), width1);
        num3 += width1 + num1;
      }
      num2 -= width1 + num1;
    }
    float num4 = num2;
    float width2 = width1 * 5f;
    float width3 = width1 * 2.1f;
    float width4 = width1 * 2.1f;
    float num5 = (float) (0.0 - (double) (width2 + num1 + width3 + num1 + width4) / 2.0);
    this.CreateKey("SPACE", new Vector3(num5 + width2 / 2f, num4, 0.0f), width2);
    this.CreateKey("BACK", new Vector3((float) ((double) num5 + (double) width2 + (double) num1 + (double) width3 / 2.0), num4, 0.0f), width3);
    this.CreateKey("ENTER", new Vector3((float) ((double) num5 + (double) width2 + (double) num1 + (double) width3 + (double) num1 + (double) width4 / 2.0), num4, 0.0f), width4);
  }
  private void CreateKey(string key, Vector3 position, float width, float height = 0.04f)
  {
    GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
    ((Collider) primitive.GetComponent<BoxCollider>()).isTrigger = true;
    primitive.layer = LayerMask.NameToLayer("UI");
    primitive.transform.SetParent(this.keyboardObj.transform, false);
    primitive.transform.localScale = new Vector3(width - 1f / 400f, height - 1f / 400f, 0.012f);
    primitive.transform.localPosition = position;
    primitive.transform.localRotation = Quaternion.identity;
    this._keyboardObjects.Add(primitive);
    Rigidbody rigidbody = primitive.AddComponent<Rigidbody>();
    rigidbody.isKinematic = true;
    rigidbody.useGravity = false;
    Material buttonMaterial = this.CreateButtonMaterial();
    primitive.GetComponent<Renderer>().sharedMaterial = buttonMaterial;
    GameObject gameObject = new GameObject("KeyCanvas");
    gameObject.transform.SetParent(primitive.transform, false);
    gameObject.transform.localPosition = new Vector3(0.0f, 0.0f, -0.52f);
    gameObject.transform.localRotation = Quaternion.identity;
    float num1 = width - 1f / 400f;
    float num2 = height - 1f / 400f;
    float num3 = 0.01f;
    gameObject.transform.localScale = new Vector3(num3 / num1 * num2, num3, num3);
    gameObject.layer = LayerMask.NameToLayer("UI");
    this._keyboardObjects.Add(gameObject);
    Canvas canvas = gameObject.AddComponent<Canvas>();
    canvas.renderMode = (RenderMode) 2;
    canvas.sortingOrder = 10;
    UnityEngine.UI.Text text = new GameObject("Label").AddComponent<UnityEngine.UI.Text>();
    ((Component) text).transform.SetParent(gameObject.transform, false);
    text.font = this.CurrentFont;
    text.text = key;
    text.fontSize = 50;
    ((Graphic) text).color = Color.white;
    text.alignment = (TextAnchor) 4;
    text.resizeTextForBestFit = true;
    text.resizeTextMinSize = 10;
    text.resizeTextMaxSize = 50;
    RectTransform component = ((Component) text).GetComponent<RectTransform>();
    component.sizeDelta = new Vector2(200f, 200f);
    ((Transform) component).localPosition = Vector3.zero;
    ((Transform) component).localRotation = Quaternion.identity;
    if (this.Settings.OutlineMode >= 2)
    {
      GameObject keyboardPrimitive = this.CreateKeyboardPrimitive(this.keyboardObj.transform);
      ((Object) keyboardPrimitive).name = "KeyOutline";
      keyboardPrimitive.transform.localScale = new Vector3((float) ((double) width - 1.0 / 400.0 + 0.0040000001899898052), (float) ((double) height - 1.0 / 400.0 + 0.0040000001899898052), 11f / 1000f);
      keyboardPrimitive.transform.localPosition = new Vector3(position.x, position.y, position.z + 0.0005f);
      Material outlineMaterial = this.CreateOutlineMaterial();
      this._keyboardMaterials.Add(outlineMaterial);
      keyboardPrimitive.GetComponent<Renderer>().sharedMaterial = outlineMaterial;
      List<GameObject> parts = (List<GameObject>) null;
      if ((double) this.Settings.MenuRoundness > 0.0)
        parts = this.RoundKeyboard(keyboardPrimitive, 0.007f * this.Settings.MenuRoundness);
      this.RegisterKeyboardColorGroup(NativeNxo.ColorRole.Outline, keyboardPrimitive, parts);
    }
    List<GameObject> parts1 = (List<GameObject>) null;
    if ((double) this.Settings.MenuRoundness > 0.0)
      parts1 = this.RoundKeyboard(primitive, 0.007f * this.Settings.MenuRoundness);
    this.RegisterKeyboardColorGroup(NativeNxo.ColorRole.Button, primitive, parts1);
    NativeUiKey nativeUiKey = primitive.AddComponent<NativeUiKey>();
    nativeUiKey.key = key;
    nativeUiKey.baseScale = primitive.transform.localScale;
    if (parts1 != null && parts1.Count > 0)
      nativeUiKey.roundedHolder = parts1[0];
    this._keyColliders[key] = nativeUiKey;
  }
  private Mesh KeyboardRoundObj(Vector3 localScale, float r, int seg = 5)
  {
    float num1 = localScale.x * 0.5f;
    float num2 = localScale.y * 0.5f;
    float num3 = localScale.z * 0.5f;
    r = Mathf.Clamp(r, 0.0001f, Mathf.Min(num1, num2) * 0.9f);
    float num4 = num1 - r;
    float num5 = num2 - r;
    List<Vector2> vector2List = new List<Vector2>((seg + 1) * 4);
    float[] numArray1 = new float[4]
    {
      num4,
      0.0f - num4,
      0.0f - num4,
      num4
    };
    float[] numArray2 = new float[4]
    {
      num5,
      num5,
      0.0f - num5,
      0.0f - num5
    };
    for (int index1 = 0; index1 < 4; ++index1)
    {
      float num6 = (float) ((double) index1 * 3.1415927410125732 * 0.5);
      float num7 = numArray1[index1];
      float num8 = numArray2[index1];
      for (int index2 = 0; index2 <= seg; ++index2)
      {
        float num9 = Mathf.Lerp(num6, num6 + 1.57079637f, (float) index2 / (float) seg);
        vector2List.Add(new Vector2(num7 + Mathf.Cos(num9) * r, num8 + Mathf.Sin(num9) * r));
      }
    }
    int count1 = vector2List.Count;
    List<Vector3> vector3List1 = new List<Vector3>(count1 * 4 + 4);
    List<Vector3> vector3List2 = new List<Vector3>(count1 * 4 + 4);
    List<int> intList = new List<int>(count1 * 12);
    int count2 = vector3List1.Count;
    vector3List1.Add(new Vector3(0.0f, 0.0f, num3));
    vector3List2.Add(Vector3.forward);
    int count3 = vector3List1.Count;
    for (int index = 0; index < count1; ++index)
    {
      vector3List1.Add(new Vector3(vector2List[index].x, vector2List[index].y, num3));
      vector3List2.Add(Vector3.forward);
    }
    for (int index = 0; index < count1; ++index)
    {
      intList.Add(count2);
      intList.Add(count3 + index);
      intList.Add(count3 + (index + 1) % count1);
    }
    int count4 = vector3List1.Count;
    vector3List1.Add(new Vector3(0.0f, 0.0f, 0.0f - num3));
    vector3List2.Add(Vector3.back);
    int count5 = vector3List1.Count;
    for (int index = 0; index < count1; ++index)
    {
      vector3List1.Add(new Vector3(vector2List[index].x, vector2List[index].y, 0.0f - num3));
      vector3List2.Add(Vector3.back);
    }
    for (int index = 0; index < count1; ++index)
    {
      intList.Add(count4);
      intList.Add(count5 + (index + 1) % count1);
      intList.Add(count5 + index);
    }
    int count6 = vector3List1.Count;
    for (int index = 0; index < count1; ++index)
    {
      Vector3 vector3;
      vector3 = new Vector3(vector2List[index].x, vector2List[index].y, 0.0f);
      Vector3 normalized = (vector3).normalized;
      vector3List1.Add(new Vector3(vector2List[index].x, vector2List[index].y, num3));
      vector3List2.Add(normalized);
      vector3List1.Add(new Vector3(vector2List[index].x, vector2List[index].y, 0.0f - num3));
      vector3List2.Add(normalized);
    }
    for (int index = 0; index < count1; ++index)
    {
      int num10 = (index + 1) % count1;
      int num11 = count6 + index * 2;
      int num12 = num11 + 1;
      int num13 = count6 + num10 * 2;
      int num14 = num13 + 1;
      intList.Add(num11);
      intList.Add(num12);
      intList.Add(num13);
      intList.Add(num13);
      intList.Add(num12);
      intList.Add(num14);
    }
    int count7 = intList.Count;
    for (int index = 0; index < count7; index += 3)
    {
      intList.Add(intList[index]);
      intList.Add(intList[index + 2]);
      intList.Add(intList[index + 1]);
    }
    Mesh mesh = new Mesh();
    ((Object) mesh).name = "KeyboardRoundedMesh";
    mesh.SetVertices(vector3List1);
    mesh.SetNormals(vector3List2);
    mesh.SetTriangles(intList, 0);
    mesh.RecalculateBounds();
    return mesh;
  }
  public enum Category
  {
    Home,
    Other,
  }
  public enum ColorRole
  {
    None,
    Background,
    Button,
    EnabledButton,
    Outline,
    AccentStrip,
  }
  private sealed class VariablesData
  {
    public GameObject menuObj;
    public GameObject background;
    public GameObject canvasObj;
    public GameObject TopMenuButton;
    public GameObject searchButton;
    public GameObject BackToStartButton;
    public UnityEngine.UI.Text title;
    public Shader uberShader = Shader.Find("GorillaTag/UberShader") ?? Shader.Find("ItzGame/Solid");
    public Shader uiShader = Shader.Find("UI/Default");
  }
  private sealed class SettingsData
  {
    public NativeNxo.SettingsData.ColorMode OutlineColorMode = NativeNxo.SettingsData.ColorMode.Pinwheel;
    public NativeNxo.SettingsData.ColorMode BackgroundMode;
    public NativeNxo.SettingsData.ColorMode ButtonMode;
    public Color32 BackgroundColor = new Color32((byte) 15, (byte) 15, (byte) 15, byte.MaxValue);
    public Color32 ButtonColor = new Color32((byte) 40, (byte) 40, (byte) 40, byte.MaxValue);
    public Color32 EnabledButtonColor = new Color32((byte) 80 , (byte) 80 , (byte) 80 , byte.MaxValue);
    public Color32 OutlineColor = new Color32((byte) 0, (byte) 0, (byte) 0, byte.MaxValue);
    public Color32 TitleColor = new Color32((byte) 13, byte.MaxValue, byte.MaxValue, byte.MaxValue);
    public Color32 TitleColor2 = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
    public Color32 AccentStripColor = new Color32((byte) 40, (byte) 40, (byte) 40, byte.MaxValue);
    public Color32 AccentStripColor2 = new Color32((byte) 230, (byte) 230, (byte) 230, byte.MaxValue);
    public int OutlineMode = 1;
    public float MenuRoundness = 1f;
    public float MenuOpacity = 1f;
    public bool ClassicButtons;
    public float TitleAnimSpeed = 1f;
    public NativeNxo.SettingsData.AccentStripType CurrentAccentStripType = NativeNxo.SettingsData.AccentStripType.Both;
    public enum AccentStripType
    {
      Off,
      Top,
      Both,
    }
    public enum ColorMode
    {
      Solid,
      Pinwheel,
    }
  }
  private sealed class SearchData
  {
    public bool isSearching;
    public string inputText;
    public string placeholderText = "Search...";
    public UnityEngine.UI.Text searchText;
  }
  public static class ButtonHandler
  {
    public static readonly List<NativeNxo.ButtonHandler.Button> FavoriteMods = new List<NativeNxo.ButtonHandler.Button>();
    public static NativeNxo.ButtonHandler.Button _bouncePending;
    public static NativeNxo.ButtonHandler.Button _bounceIncParent;
    public static bool _bounceIncUp;
    public static bool IsCategoryButton(NativeNxo.ButtonHandler.Button b) => b.isCategory;
    public sealed class Button
    {
      public string buttonText;
      public NativeNxo.Category Page;
      public bool Enabled;
      public bool isToggle;
      public bool incremental;
      public bool showGear;
      public bool isCategory;
      public Button(
        string text,
        NativeNxo.Category page,
        bool isToggle = false,
        bool isActive = false,
        Action action = null)
      {
        this.buttonText = text;
        this.Page = page;
        this.isToggle = isToggle;
        this.Enabled = isActive;
      }
    }
  }
}
