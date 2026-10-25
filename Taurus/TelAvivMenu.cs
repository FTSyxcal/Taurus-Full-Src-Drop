
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class TelAvivMenu : IDisposable
{
  private readonly Transform root;
  private readonly Transform visual;
  private readonly Transform keyboard;
  private readonly Renderer[] originals;
  private readonly Dictionary<string, Transform> controls = new Dictionary<string, Transform>();
  private readonly Dictionary<string, Vector3> positions = new Dictionary<string, Vector3>();
  private readonly Dictionary<string, Vector3> scales = new Dictionary<string, Vector3>();
  private readonly List<Material> materials = new List<Material>();
  private readonly List<Mesh> meshes = new List<Mesh>();
  private readonly UnityEngine.UI.Text[] labels = new UnityEngine.UI.Text[8];
  private readonly Material solid;
  private readonly Font font;
  private readonly Renderer background;
  private readonly UnityEngine.UI.Text title;
  private readonly UnityEngine.UI.Text fps;
  private readonly UnityEngine.UI.Text query;
  private readonly UnityEngine.UI.Text notice;
  private readonly Transform noticeBody;
  private bool active;
  private MenuState state;
  private float nextFps;
  public Transform Visual => this.visual;
  public TelAvivMenu(Transform root)
  {
    this.root = root;
    this.originals = ((Component) root).GetComponentsInChildren<Renderer>(true);
    this.solid = ((Component) root.Find("Background")).GetComponent<Renderer>().sharedMaterial;
    this.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
    GameObject gameObject = new GameObject("TelAviv3D", new Type[2]
    {
      typeof (RectTransform),
      typeof (Canvas)
    });
    gameObject.transform.SetParent(root, false);
    this.visual = gameObject.transform;
    gameObject.GetComponent<Canvas>().renderMode = (RenderMode) 2;
    gameObject.GetComponent<Canvas>().sortingOrder = 100;
    this.background = ((Component) this.Cube("Background", new Vector3(0.05f, 0.0f, 0.0f), new Vector3(0.01f, 0.3f, 0.3825f), true)).GetComponent<Renderer>();
    this.background.sharedMaterial.mainTexture = (Texture) NativeUiResources.LoadImage("tel-aviv-flag.png");
    Mesh sharedMesh = ((Component) this.background).GetComponent<MeshFilter>().sharedMesh;
    Vector2[] uv = sharedMesh.uv;
    for (int index = 0; index < uv.Length; ++index)
      uv[index].y = 1f - uv[index].y;
    sharedMesh.uv = uv;
    this.title = this.Text("Title", new Vector3(0.061f, 0.0f, 0.165f), new Vector2(0.28f, 0.05f), "Tel Aviv Client [1]", 32 );
    this.fps = this.Text("FPS", new Vector3(0.061f, 0.0f, 0.135f), new Vector2(0.28f, 0.02f), "FPS", 17);
    for (int index = 0; index < 8; ++index)
    {
      Transform target = this.Cube("Row" + index.ToString(), new Vector3(0.056f, 0.0f, (float) ((0.2800000011920929 - (double) index * 0.10000000149011612) * 0.38249999284744263)), new Vector3(0.009f, 0.27f, 0.0306f), true);
      this.controls["Row" + index.ToString()] = target;
      this.labels[index] = this.Text("Caption" + index.ToString(), new Vector3(0.064f, 0.0f, (float) (0.11100000143051147 - (double) index * 0.10000000149011612 / 2.5999999046325684)), new Vector2(0.2f, 0.03f), "", 19);
      ((Component) this.labels[index]).transform.SetParent(target, true);
      MenuButtonEffects.Bind(((Component) this.labels[index]).transform, target);
    }
    this.Button("ButtonDisconnect", new Vector3(0.056f, 0.0f, 0.2295f), new Vector3(0.009f, 0.27f, 0.0306f), "Disconnect");
    this.Button("ButtonPrevious", new Vector3(0.056f, 0.195f, 0.0f), new Vector3(0.009f, 0.06f, 0.34425f), "<");
    this.Button("ButtonNext", new Vector3(0.056f, -0.195f, 0.0f), new Vector3(0.009f, 0.06f, 0.34425f), ">");
    this.Button("ButtonHome", new Vector3(0.056f, 0.0f, -0.22f), new Vector3(0.009f, 0.12f, 0.03f), "Home");
    this.Button("DesignSettings", new Vector3(0.056f, 57f / 500f, 0.165f), new Vector3(0.009f, 0.045f, 0.03f), "Settings");
    this.Button("ButtonSearch", new Vector3(0.056f, -57f / 500f, 0.165f), new Vector3(0.009f, 0.045f, 0.03f), "Search");
    this.noticeBody = this.Cube("NoticeBackground", new Vector3(0.056f, 0.0f, -0.263f), new Vector3(0.009f, 0.27f, 0.035f), true);
    this.notice = this.Text("NativeNoticeText", new Vector3(0.064f, 0.0f, -0.263f), new Vector2(0.25f, 0.031f), "", 18);
    ((Component) this.notice).gameObject.AddComponent<NativeUiSemanticColor>();
    this.keyboard = new GameObject("TelAvivKeyboard").transform;
    this.keyboard.SetParent(this.visual, false);
    this.query = this.Text("SearchQuery", new Vector3(0.064f, 0.0f, 0.11f), new Vector2(0.25f, 0.04f), "Search_", 22);
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
        this.Key("Key_" + caption, caption, new Vector3(0.056f, (float) (((double) (strArray[index1].Length - 1) * 0.5 - (double) index2) * 0.026000000536441803), (float) (0.0560000017285347 - (double) index1 * 0.054000001400709152)), new Vector3(0.009f, 23f / 1000f, 0.042f));
      }
    }
    this.Key("Key_Back", "Back", new Vector3(0.056f, 0.098f, -0.17f), new Vector3(0.009f, 0.06f, 0.037f));
    this.Key("Key_Clear", "Clear", new Vector3(0.056f, 0.032f, -0.17f), new Vector3(0.009f, 0.06f, 0.037f));
    this.Key("Key_Space", "Space", new Vector3(0.056f, -0.034f, -0.17f), new Vector3(0.009f, 0.06f, 0.037f));
    this.Key("Key_Done", "Done", new Vector3(0.056f, -0.1f, -0.17f), new Vector3(0.009f, 0.06f, 0.037f));
    for (int index = 0; index < 8; ++index)
    {
      bool[] flagArray = new bool[2]{ false, true };
      foreach (bool flag in flagArray)
      {
        string str = $"Increment{(flag ? "Plus" : "Minus")}{index.ToString()}";
        this.Button(str, new Vector3(0.065f, flag ? -0.119f : -0.081f, (float) ((0.2800000011920929 - (double) index * 0.10000000149011612) * 0.38249999284744263)), new Vector3(0.005f, 0.032f, 0.024f), flag ? "+" : "-");
        this.controls[str].SetParent(this.controls["Row" + index.ToString()], true);
      }
    }
    foreach (KeyValuePair<string, Transform> control in this.controls)
    {
      this.positions[control.Key] = control.Value.localPosition;
      this.scales[control.Key] = control.Value.localScale;
    }
    foreach (Component componentsInChild in ((Component) this.visual).GetComponentsInChildren<Transform>(true))
      componentsInChild.gameObject.layer = ((Component) root).gameObject.layer;
    this.SetActive(false);
  }
  private Transform Cube(string name, Vector3 position, Vector3 size, bool image)
  {
    GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
    ((Object) primitive).name = name;
    Collider component = primitive.GetComponent<Collider>();
    component.enabled = false;
    NativeUiSnapshot.Destroy((Object) component);
    primitive.transform.SetParent(this.visual, false);
    primitive.transform.localPosition = position;
    primitive.transform.localScale = size;
    Material material = new Material(this.solid)
    {
      color = Color.white
    };
    material.SetFloat("_UseGradient", 0.0f);
    if (image)
      material.mainTexture = (Texture) NativeUiResources.LoadImage("tel-aviv.png");
    if (image)
    {
      Mesh mesh = Object.Instantiate<Mesh>(primitive.GetComponent<MeshFilter>().sharedMesh);
      Vector2[] uv = mesh.uv;
      Vector3[] vertices = mesh.vertices;
      Vector3[] normals = mesh.normals;
      for (int index = 0; index < uv.Length; ++index)
        uv[index] = (double) Mathf.Abs(normals[index].x) > 0.89999997615814209 ? new Vector2(0.5f - vertices[index].y, 0.5f + vertices[index].z) : ((double) Mathf.Abs(normals[index].y) > 0.89999997615814209 ? new Vector2(0.5f + vertices[index].x, 0.5f + vertices[index].z) : new Vector2(0.5f - vertices[index].y, 0.5f + vertices[index].x));
      mesh.uv = uv;
      this.meshes.Add(mesh);
      primitive.GetComponent<MeshFilter>().sharedMesh = mesh;
    }
    this.materials.Add(material);
    primitive.GetComponent<Renderer>().sharedMaterial = material;
    return primitive.transform;
  }
  private UnityEngine.UI.Text Text(
    string name,
    Vector3 position,
    Vector2 size,
    string caption,
    int fontSize)
  {
    GameObject gameObject = new GameObject(name, new Type[3]
    {
      typeof (RectTransform),
      typeof (CanvasRenderer),
      typeof (UnityEngine.UI.Text)
    });
    gameObject.transform.SetParent(this.visual, false);
    gameObject.transform.localPosition = position;
    gameObject.transform.localRotation = Quaternion.LookRotation(Vector3.left, Vector3.forward);
    gameObject.transform.localScale = (Vector3.one * 1f / 1000f);
    UnityEngine.UI.Text component = gameObject.GetComponent<UnityEngine.UI.Text>();
    component.font = this.font;
    component.text = caption;
    component.fontSize = fontSize;
    component.fontStyle = (FontStyle) 2;
    ((Graphic) component).color = Color.white;
    component.supportRichText = true;
    component.alignment = (TextAnchor) 4;
    ((Graphic) component).raycastTarget = false;
    ((Graphic) component).rectTransform.sizeDelta = (size * 1000f);
    component.resizeTextForBestFit = true;
    component.resizeTextMinSize = 6;
    component.resizeTextMaxSize = fontSize;
    Shadow shadow = gameObject.AddComponent<Shadow>();
    shadow.effectColor = Color.black;
    shadow.effectDistance = new Vector2(1f, -1f);
    return component;
  }
  private void Button(string name, Vector3 position, Vector3 size, string caption)
  {
    Transform transform = this.Cube(name, position, size, true);
    this.controls[name] = transform;
    ((Component) this.Text(name + "Caption", (position + (Vector3.right * 0.008f)), new Vector2(size.y * 0.9f, Mathf.Min(size.z * 0.9f, 0.03f)), caption, caption.Length == 1 ? 28 : 18)).transform.SetParent(transform, true);
  }
  private void Key(string name, string caption, Vector3 position, Vector3 size)
  {
    this.Button(name, position, size, caption);
    Transform control = this.controls[name];
    this.controls.Remove(name);
    this.controls["Keyboard/" + name] = control;
    control.SetParent(this.keyboard, true);
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
    this.title.text = $"Tel Aviv Client <color=grey>[</color>{(state.Page + 1).ToString()}<color=grey>]</color>";
    ((Component) this.keyboard).gameObject.SetActive(state.KeyboardOpen);
    this.query.text = (state.Query.Length == 0 ? "Search" : state.Query) + "_";
    for (int index = 0; index < 8; ++index)
    {
      Transform transform = this.root.Find("Row" + index.ToString());
      ((Component) this.controls["Row" + index.ToString()]).gameObject.SetActive(((Component) transform).gameObject.activeSelf);
      string text = ((Component) transform.Find("State")).GetComponent<TextMesh>().text;
      this.labels[index].text = ((Component) transform.Find("Label")).GetComponent<TextMesh>().text.Replace("\n", " ") + (text.Length == 0 ? "" : "  " + text.Replace("\n", " "));
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
        ((Component) transform).GetComponent<BoxCollider>().size = ((Vector3.Scale((localBounds).size, control.Value.lossyScale) / Mathf.Max(0.0001f, this.root.lossyScale.x)) + (Vector3.right * 0.01f));
      }
    }
    bool activeSelf = ((Component) this.root.Find("NotificationBackground")).gameObject.activeSelf;
    ((Component) this.noticeBody).gameObject.SetActive(activeSelf);
    ((Component) this.notice).gameObject.SetActive(activeSelf);
    this.notice.text = ((Component) this.root.Find("PageLabel")).GetComponent<TextMesh>().text;
    GradientPicker.Place(this.root, new Vector3(57f / 1000f, 0.0f, 0.0f), 0.8f);
    foreach (Renderer componentsInChild in ((Component) this.root.Find("ColorPicker")).GetComponentsInChildren<Renderer>(true))
      componentsInChild.enabled = state.ColorPickerOpen;
    this.Tick(Time.unscaledTime);
  }
  public void Tick(float time)
  {
    if (!this.active || this.state == null)
      return;
    this.background.sharedMaterial.color = Color.white;
    if ((double) time >= (double) this.nextFps)
    {
      this.nextFps = time + 0.1f;
      this.fps.text = Application.isPlaying ? "FPS: " + Mathf.RoundToInt(1f / Mathf.Max(1f / 1000f, Time.smoothDeltaTime)).ToString() : "FPS";
    }
    foreach (KeyValuePair<string, Transform> control in this.controls)
    {
      Transform transform = this.root.Find(control.Key);
      if (!((Object) transform == (Object) null))
      {
        float num = Mathf.Clamp(transform.localScale.x, 0.85f, 1.03f);
        control.Value.localScale = (this.scales[control.Key] * num);
        Vector3 position = this.positions[control.Key];
        control.Value.localPosition = (position - (Vector3.right * (float) ((1.0 - (double) num) * 0.0099999997764825821)));
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
    foreach (Object mesh in this.meshes)
      NativeUiSnapshot.Destroy(mesh);
    NativeUiSnapshot.Destroy((Object) this.font);
  }
}
