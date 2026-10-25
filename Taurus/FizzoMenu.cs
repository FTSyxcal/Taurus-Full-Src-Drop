
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class FizzoMenu : IDisposable
{
  public const float Unit = 0.000825825846f;
  private readonly Transform root;
  private readonly Transform visual;
  private readonly Transform keyboard;
  private readonly Func<string, Vector3, float, Mesh> shape;
  private readonly Renderer[] originals;
  private readonly Dictionary<string, Transform> controls = new Dictionary<string, Transform>();
  private readonly Dictionary<string, Vector3> positions = new Dictionary<string, Vector3>();
  private readonly List<Material> materials = new List<Material>();
  private readonly Text[] labels = new Text[6];
  private readonly Text[] details = new Text[6];
  private readonly List<Transform> dots = new List<Transform>();
  private readonly Material solid;
  private readonly Font font;
  private readonly Text search;
  private readonly Text activeLabel;
  private readonly Text status;
  private readonly Text statusDetail;
  private readonly Text query;
  private readonly Text notice;
  private readonly Text pickerNext;
  private readonly Renderer statusDot;
  private readonly Transform noticeBody;
  private readonly ImportedTheme theme;
  private bool active;
  private MenuState state;
  private int page = -1;
  private int category = -1;
  private float pageAt;
  public Transform Visual => this.visual;
  public FizzoMenu(Transform root, Func<string, Vector3, float, Mesh> shape)
  {
    this.root = root;
    this.shape = shape;
    this.originals = ((Component) root).GetComponentsInChildren<Renderer>(true);
    this.solid = ((Component) root.Find("Background")).GetComponent<Renderer>().sharedMaterial;
    this.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
    GameObject gameObject = new GameObject("Fizzo3D", new Type[2]
    {
      typeof (RectTransform),
      typeof (Canvas)
    });
    gameObject.transform.SetParent(root, false);
    this.visual = gameObject.transform;
    gameObject.GetComponent<Canvas>().renderMode = (RenderMode) 2;
    gameObject.GetComponent<Canvas>().sortingOrder = 100;
    this.Body("Frame", 23f, 15f, 441f, 666f, 42f, 0.04f, 0.014f, 1769472U );
    this.controls["ButtonHome"] = this.Body("Header", 23f, 15f, 441f, 65f, 32f, 0.049f, 0.004f, 2228224U );
    this.Body("Avatar", 39f, 25f, 38f, 38f, 19f, 0.052f, 1f / 500f, 2228224U );
    Text text = this.Label("Title", 100f, 24f, 290f, 39f, 22, 16047838U, (TextAnchor) 4);
    text.text = "Fizzo Paid";
    text.fontStyle = (FontStyle) 1;
    ((Component) text).transform.SetParent(this.controls["ButtonHome"], true);
    this.pickerNext = this.Label("PickerAdvance", 380f, 29f, 60f, 30f, 12, 16047838U, (TextAnchor) 4);
    ((Component) this.pickerNext).transform.SetParent(this.controls["ButtonHome"], true);
    ((Component) this.pickerNext).gameObject.SetActive(false);
    this.controls["ButtonSearch"] = this.Body("ButtonSearch", 24f, 79f, 439f, 44f, 0.0f, 0.052f, 0.004f, 1769472U );
    this.Body("SearchDivider", 24f, 123f, 440f, 1f, 0.0f, 0.055f, 1f / 1000f, 4919312U);
    this.Label("SearchArrow", 39f, 79f, 18f, 44f, 18, 9918035U, (TextAnchor) 4).text = ">";
    this.search = this.Label("SearchCaption", 71f, 80f, 288f, 42f, 11, 7951958U, (TextAnchor) 3);
    this.search.text = "Search mods...";
    this.Body("KeyboardPill", 374f, 86f, 46f, 27f, 13f, 0.055f, 3f / 1000f, 3214605U);
    this.Label("KeyboardPillCaption", 374f, 86f, 46f, 27f, 10, 13610411U, (TextAnchor) 4).text = "KB";
    this.controls["ButtonEnabled"] = this.Body("ButtonEnabled", 24f, 125f, 439f, 44f, 0.0f, 0.052f, 0.004f, 3083533U);
    this.Label("ActiveArrow", 38f, 125f, 24f, 44f, 13, 9918035U, (TextAnchor) 4).text = ">>";
    this.activeLabel = this.Label("ActiveCaption", 80f, 125f, 230f, 44f, 14, 13610411U, (TextAnchor) 4);
    this.activeLabel.text = "Active Mods";
    this.Body("ActiveDivider", 24f, 172f, 440f, 1f, 0.0f, 0.055f, 1f / 1000f, 4919312U);
    for (int index = 0; index < 6; ++index)
    {
      float y = (float) (187 + index * 64 );
      Transform target = this.Body("Row" + index.ToString(), 39f, y, 410f, 52f, 14f, 0.053f, 0.004f, 2821390U);
      this.controls["Row" + index.ToString()] = target;
      this.labels[index] = this.Label("Caption" + index.ToString(), 56f, y, 340f, 52f, 15, 13610411U, (TextAnchor) 3);
      ((Component) this.labels[index]).transform.SetParent(target, true);
      MenuButtonEffects.Bind(((Component) this.labels[index]).transform, target);
      this.details[index] = this.Label("Detail" + index.ToString(), 57f, y + 30f, 290f, 20f, 10, 12160397U, (TextAnchor) 3);
      ((Component) this.details[index]).transform.SetParent(target, true);
      ((Component) this.Label("Arrow" + index.ToString(), 411f, y, 26f, 52f, 24, 9918035U, (TextAnchor) 4)).transform.SetParent(target, true);
    }
    this.Body("Navigation", 24f, 558f, 440f, 65f, 0.0f, 0.052f, 0.004f, 1572864U );
    this.Body("NavigationDivider", 24f, 557f, 440f, 1f, 0.0f, 0.055f, 1f / 1000f, 4919312U);
    this.Body("FooterDivider", 24f, 623f, 440f, 1f, 0.0f, 0.055f, 1f / 1000f, 4919312U);
    this.controls["ButtonPrevious"] = this.Body("ButtonPrevious", 121f, 569f, 42f, 42f, 21f, 0.055f, 0.004f, 5312272U);
    this.controls["ButtonNext"] = this.Body("ButtonNext", 327f, 569f, 42f, 42f, 21f, 0.055f, 0.004f, 5312272U);
    this.Label("PreviousCaption", 121f, 569f, 42f, 42f, 24, 16047838U, (TextAnchor) 4).text = "<";
    this.Label("NextCaption", 327f, 569f, 42f, 42f, 24, 16047838U, (TextAnchor) 4).text = ">";
    this.controls["ButtonRoomStatus"] = this.Body("ButtonRoomStatus", 24f, 625f, 440f, 55f, 27f, 0.052f, 0.004f, 851968U );
    this.statusDot = ((Component) this.Body("StatusDot", 39f, 647f, 11f, 11f, 5.5f, 0.056f, 1f / 500f, 14959691U)).GetComponent<Renderer>();
    ((Component) this.statusDot).gameObject.AddComponent<NativeUiSemanticColor>();
    this.status = this.Label("ConnectionStatus", 61f, 633f, 340f, 23f, 14, 13610411U, (TextAnchor) 3);
    this.status.fontStyle = (FontStyle) 1;
    this.statusDetail = this.Label("ConnectionDetail", 61f, 658f, 340f, 19f, 10, 7951958U, (TextAnchor) 3);
    ((Component) this.status).gameObject.AddComponent<NativeUiSemanticColor>();
    ((Component) this.statusDetail).gameObject.AddComponent<NativeUiSemanticColor>();
    ((Component) this.status).transform.SetParent(this.controls["ButtonRoomStatus"], true);
    ((Component) this.statusDetail).transform.SetParent(this.controls["ButtonRoomStatus"], true);
    ((Component) this.statusDot).transform.SetParent(this.controls["ButtonRoomStatus"], true);
    this.noticeBody = this.Body("Notification", 39f, 702f, 410f, 40f, 12f, 0.053f, 0.004f, 2821390U);
    this.notice = this.Label("NativeNoticeText", 51f, 702f, 386f, 40f, 15, 16047838U, (TextAnchor) 4);
    ((Component) this.notice).gameObject.AddComponent<NativeUiSemanticColor>();
    this.keyboard = new GameObject("FizzoKeyboard").transform;
    this.keyboard.SetParent(this.visual, false);
    this.query = this.Label("SearchQuery", 57f, 185f, 368f, 40f, 17, 13610411U, (TextAnchor) 3);
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
        this.Key("Key_" + caption, caption, (float) (51.0 + (double) (10 - strArray[index1].Length) * 19.5) + (float) (index2 * 39), (float) (242 + index1 * 64 ), 34f, 52f);
      }
    }
    this.Key("Key_Back", "Back", 51f, 501f, 82f, 43f);
    this.Key("Key_Clear", "Clear", 143f, 501f, 82f, 43f);
    this.Key("Key_Space", "Space", 235f, 501f, 92f, 43f);
    this.Key("Key_Done", "Done", 337f, 501f, 98f, 43f);
    foreach (KeyValuePair<string, Transform> control in this.controls)
      this.positions[control.Key] = control.Value.localPosition;
    foreach (Component componentsInChild in ((Component) this.visual).GetComponentsInChildren<Transform>(true))
      componentsInChild.gameObject.layer = ((Component) root).gameObject.layer;
    this.theme = new ImportedTheme(this.visual, Vector3.down, Vector3.forward);
    this.SetActive(false);
  }
  private static Vector3 Point(float x, float y, float depth)
  {
    return new Vector3(depth, (float) ((244.0 - (double) x) * 0.00082582584582269192), (float) ((348.0 - (double) y) * 0.00082582584582269192));
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
    uint color)
  {
    GameObject gameObject = new GameObject(name, new Type[2]
    {
      typeof (MeshFilter),
      typeof (MeshRenderer)
    });
    gameObject.transform.SetParent(this.visual, false);
    gameObject.transform.localPosition = FizzoMenu.Point(x + width / 2f, y + height / 2f, depth);
    gameObject.GetComponent<MeshFilter>().sharedMesh = this.shape("Fizzo/" + name, new Vector3(thickness, width * 0.000825825846f, height * 0.000825825846f), radius * 0.000825825846f);
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
    uint color,
    TextAnchor alignment)
  {
    GameObject gameObject = new GameObject(name, new Type[3]
    {
      typeof (RectTransform),
      typeof (CanvasRenderer),
      typeof (Text)
    });
    gameObject.transform.SetParent(this.visual, false);
    gameObject.transform.localPosition = FizzoMenu.Point(x + width / 2f, y + height / 2f, 0.058f);
    gameObject.transform.localRotation = Quaternion.LookRotation(Vector3.left, Vector3.forward);
    gameObject.transform.localScale = (Vector3.one * 0.000825825846f);
    Text component = gameObject.GetComponent<Text>();
    component.font = this.font;
    component.fontSize = size;
    ((Graphic) component).color = MenuPalette.Unpack(color);
    component.alignment = alignment;
    component.supportRichText = true;
    ((Graphic) component).raycastTarget = false;
    ((Graphic) component).rectTransform.sizeDelta = new Vector2(width, height);
    component.resizeTextForBestFit = true;
    component.resizeTextMinSize = 6;
    component.resizeTextMaxSize = size;
    return component;
  }
  private void Key(string name, string caption, float x, float y, float width, float height)
  {
    Transform transform = this.Body(name, x, y, width, height, 10f, 0.053f, 0.005f, 2821390U);
    transform.SetParent(this.keyboard, true);
    this.controls["Keyboard/" + name] = transform;
    Text text = this.Label(name + "Caption", x, y, width, height, caption.Length == 1 ? 19 : 13, 13610411U, (TextAnchor) 4);
    text.text = caption;
    ((Component) text).transform.SetParent(transform, true);
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
    ((Component) this.root.Find("DesignSettings")).gameObject.SetActive(false);
    if (this.page != state.Page || this.category != state.Category)
    {
      this.page = state.Page;
      this.category = state.Category;
      this.pageAt = Time.unscaledTime;
    }
    this.search.text = state.Query.Length == 0 ? "Search mods..." : state.Query;
    this.query.text = (state.Query.Length == 0 ? "Search mods..." : state.Query) + "_";
    ((Component) this.keyboard).gameObject.SetActive(state.KeyboardOpen);
    ((Component) this.pickerNext).gameObject.SetActive(state.ColorPickerOpen);
    this.pickerNext.text = state.EditingEnd ? "Done" : "Next";
    for (int row = 0; row < 6; ++row)
    {
      Transform transform1 = this.root.Find("Row" + row.ToString());
      Transform control = this.controls["Row" + row.ToString()];
      ((Component) control).gameObject.SetActive(((Component) transform1).gameObject.activeSelf);
      int index = state.EntryAt(row);
      string str1 = ((Component) transform1.Find("Label")).GetComponent<TextMesh>().text.Replace("\n", " ");
      this.labels[row].text = str1;
      bool activeSelf = ((Component) this.root.Find("IncrementPlus" + row.ToString())).gameObject.activeSelf;
      this.details[row].text = state.Category == 0 ? "" : ((Component) transform1.Find("State")).GetComponent<TextMesh>().text.Replace("\n", " ");
      ((Component) control.Find("Arrow" + row.ToString())).GetComponent<Text>().text = activeSelf ? "" : (index < 0 || !MenuCatalog.Buttons[index].Toggle ? ">" : (state.IsOn(index) ? "●" : "○"));
      bool[] flagArray = new bool[2]{ false, true };
      foreach (bool flag in flagArray)
      {
        string str2 = $"Increment{(flag ? "Plus" : "Minus")}{row.ToString()}";
        if (activeSelf && !this.controls.ContainsKey(str2))
        {
          Transform transform2 = this.Body(str2, flag ? 411f : 377f, (float) (197 + row * 64 ), 29f, 29f, 10f, 0.056f, 3f / 1000f, 5312272U);
          transform2.SetParent(control, true);
          this.controls[str2] = transform2;
          this.positions[str2] = transform2.localPosition;
          Text text = this.Label(str2 + "Caption", flag ? 411f : 377f, (float) (197 + row * 64 ), 29f, 29f, 18, 16047838U, (TextAnchor) 4);
          text.text = flag ? "+" : "-";
          ((Component) text).transform.SetParent(transform2, true);
          MenuFonts.Invalidate(this.visual);
        }
      }
    }
    foreach (KeyValuePair<string, Transform> control in this.controls)
    {
      Transform transform = this.root.Find(control.Key);
      if (!((Object) transform == (Object) null))
      {
        if (!control.Key.StartsWith("Row"))
          ((Component) control.Value).gameObject.SetActive(((Component) transform).gameObject.activeSelf);
        Bounds localBounds = ((Component) control.Value).GetComponent<Renderer>().localBounds;
        transform.localPosition = this.root.InverseTransformPoint(control.Value.TransformPoint((localBounds).center));
        transform.localScale = Vector3.one;
        ((Component) transform).GetComponent<BoxCollider>().center = Vector3.zero;
        ((Component) transform).GetComponent<BoxCollider>().size = ((Vector3.Scale((localBounds).size, control.Value.lossyScale) / Mathf.Max(0.0001f, this.root.lossyScale.x)) + (Vector3.right * 0.008f));
      }
    }
    int num = Mathf.Max(4, state.PageCount);
    while (this.dots.Count < num)
      this.dots.Add(this.Body("Dot" + this.dots.Count.ToString(), 0.0f, 0.0f, 9f, 9f, 4.5f, 0.056f, 1f / 500f, 7413021U));
    for (int index = 0; index < this.dots.Count; ++index)
    {
      ((Component) this.dots[index]).gameObject.SetActive(index < num);
      this.dots[index].localPosition = FizzoMenu.Point((float) (244.0 + ((double) index - (double) (num - 1) * 0.5) * (double) Mathf.Min(15f, 140f / (float) (num - 1))), 590f, 0.056f);
    }
    GradientPicker.Place(this.root, FizzoMenu.Point(244f, 365f, 0.049f), 0.85f);
    foreach (Renderer componentsInChild in ((Component) this.root.Find("ColorPicker")).GetComponentsInChildren<Renderer>(true))
      componentsInChild.enabled = state.ColorPickerOpen;
    bool activeSelf1 = ((Component) this.root.Find("NotificationBackground")).gameObject.activeSelf;
    ((Component) this.noticeBody).gameObject.SetActive(activeSelf1);
    ((Component) this.notice).gameObject.SetActive(activeSelf1);
    this.notice.text = ((Component) this.root.Find("PageLabel")).GetComponent<TextMesh>().text;
    this.Tick(Time.unscaledTime);
  }
  public void Tick(float time)
  {
    if (!this.active || this.state == null)
      return;
    this.theme.Apply(this.state, time);
    string str1 = this.state.InRoom ? "Online" : "Offline";
    string str2 = this.state.InRoom ? "Connected to a room" : "Not connected to a room";
    if (this.status.text != str1)
      this.status.text = str1;
    if (this.statusDetail.text != str2)
      this.statusDetail.text = str2;
    this.statusDot.sharedMaterial.color = MenuPalette.Unpack(this.state.InRoom ? 3459192U : 14959691U);
    for (int index = 0; index < this.dots.Count; ++index)
    {
      bool flag = index == this.state.Page;
      this.dots[index].localScale = (Vector3.one * (flag ? 1.25f : 1f));
      ((Component) this.dots[index]).GetComponent<Renderer>().sharedMaterial.color = MenuPalette.Unpack(flag ? 13773351U : 7413021U);
    }
    foreach (KeyValuePair<string, Transform> control in this.controls)
    {
      Transform transform = this.root.Find(control.Key);
      if (!((Object) transform == (Object) null))
      {
        float num = Mathf.Clamp(transform.localScale.x, 0.85f, 1.03f);
        control.Value.localScale = (Vector3.one * num);
        control.Value.localPosition = (this.positions[control.Key] - (Vector3.right * (float) ((1.0 - (double) num) * 0.00800000037997961)));
      }
    }
    MenuFonts.Apply(this.visual, this.state.FontIndex);
  }
  public void Dispose()
  {
    this.SetActive(false);
    NativeUiSnapshot.Destroy((Object) ((Component) this.visual).gameObject);
    foreach (Object material in this.materials)
      NativeUiSnapshot.Destroy(material);
    NativeUiSnapshot.Destroy((Object) this.font);
  }
}
