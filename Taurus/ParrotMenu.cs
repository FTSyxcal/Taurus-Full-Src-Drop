
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class ParrotMenu : IDisposable
{
  private readonly Transform root;
  private readonly Transform visual;
  private readonly Transform model;
  private readonly Transform keyboard;
  private readonly Renderer[] originals;
  private readonly Dictionary<string, Transform> controls = new Dictionary<string, Transform>();
  private readonly Dictionary<Transform, Vector3> positions = new Dictionary<Transform, Vector3>();
  private readonly Dictionary<Transform, Vector3> scales = new Dictionary<Transform, Vector3>();
  private readonly List<Material> materials = new List<Material>();
  private readonly Transform[] rows = new Transform[6];
  private readonly Transform[] meshLabels = new Transform[6];
  private readonly Text[] captions = new Text[6];
  private readonly Font font;
  private readonly Text query;
  private readonly Text notice;
  private readonly Text advance;
  private readonly Transform noticeBody;
  private MenuState state;
  private bool active;
  public ParrotMenu(Transform root)
  {
    this.root = root;
    this.originals = ((Component) root).GetComponentsInChildren<Renderer>(true);
    GameObject gameObject1 = new GameObject("Parrot3D", new Type[2]
    {
      typeof (RectTransform),
      typeof (Canvas)
    });
    this.visual = gameObject1.transform;
    this.visual.SetParent(root, false);
    gameObject1.GetComponent<Canvas>().renderMode = (RenderMode) 2;
    gameObject1.GetComponent<Canvas>().sortingOrder = 100;
    this.font = Font.CreateDynamicFontFromOSFont("Arial", 32 );
    GameObject gameObject2 = ImportedAssetMenu.Load("parrot").LoadAsset<GameObject>("assets/parrot.client_menu.fbx");
    if (((Object) gameObject2 == (Object) null))
      throw new InvalidOperationException("Original Parrot.Client model missing.");
    Transform transform1 = new GameObject("OriginalParrot").transform;
    transform1.SetParent(this.visual, false);
    this.model = Object.Instantiate<GameObject>(gameObject2).transform;
    this.model.SetParent(transform1, false);
    Material sharedMaterial1 = ((Component) root.Find("Background")).GetComponent<Renderer>().sharedMaterial;
    foreach (Renderer componentsInChild in ((Component) this.model).GetComponentsInChildren<Renderer>(true))
    {
      Material[] materialArray = new Material[componentsInChild.sharedMaterials.Length];
      for (int index = 0; index < materialArray.Length; ++index)
      {
        Material sharedMaterial2 = componentsInChild.sharedMaterials[index];
        Material material1 = new Material(sharedMaterial1);
        ((Object) material1).name = ((Object) sharedMaterial2).name;
        material1.color = sharedMaterial2.HasProperty("_BaseColor") ? sharedMaterial2.GetColor("_BaseColor") : (sharedMaterial2.HasProperty("_Color") ? sharedMaterial2.color : Color.white);
        Material material2 = material1;
        material2.SetFloat("_UseGradient", 0.0f);
        if (sharedMaterial2.HasProperty("_BaseMap"))
          material2.mainTexture = sharedMaterial2.GetTexture("_BaseMap");
        this.materials.Add(material2);
        materialArray[index] = material2;
      }
      componentsInChild.sharedMaterials = materialArray;
    }
    string[] strArray1 = new string[6]
    {
      "Settings",
      "Movement",
      "Room",
      "Safety",
      "Visuals",
      "Overpowered"
    };
    for (int index = 0; index < 6; ++index)
    {
      string str = strArray1[index];
      this.rows[index] = this.Group("Row" + index.ToString(), "Btn_" + str, new string[8]
      {
        "Btn_" + str,
        $"Btn_{str}_Border",
        "Text_" + str,
        "Index_" + str,
        "Arrow_" + str,
        "Chip_" + str,
        $"Chip_{str}_Border",
        "Stripe_" + str
      });
      this.meshLabels[index] = this.rows[index].Find("Text_" + str);
      this.controls["Row" + index.ToString()] = this.rows[index];
    }
    this.controls["ButtonPrevious"] = this.Group("ButtonPrevious", "Btn_Prev", new string[4]
    {
      "Btn_Prev",
      "Btn_Prev_Border",
      "Text_Prev",
      "Icon_Prev"
    });
    this.controls["ButtonNext"] = this.Group("ButtonNext", "Btn_Next", new string[4]
    {
      "Btn_Next",
      "Btn_Next_Border",
      "Text_Next",
      "Icon_Next"
    });
    this.controls["ButtonSearch"] = this.Group("ButtonSearch", "Btn_Search", new string[4]
    {
      "Btn_Search",
      "Btn_Search_Border",
      "Icon_Search_Ring",
      "Icon_Search_Handle"
    });
    this.controls["ButtonHome"] = this.Group("ButtonHome", "Title_Bar", new string[3]
    {
      "Title_Bar",
      "Title_Bar_Border",
      "Title_Text"
    });
    this.controls["DesignLogo"] = this.Group("DesignLogo", "Logo_Diamond", new string[2]
    {
      "Logo_Diamond",
      "Logo_Diamond_Core"
    });
    transform1.localPosition = new Vector3(0.05f, 0.0f, 0.0f);
    transform1.localRotation = Quaternion.LookRotation(Vector3.back, Vector3.right);
    for (int index = 0; index < 6; ++index)
    {
      Vector3 vector3 = this.visual.InverseTransformPoint(this.rows[index].position);
      this.captions[index] = this.Label("Caption" + index.ToString(), (vector3 + (Vector3.right * 0.009f)), new Vector2(0.39f, 0.06f), "", 28);
      this.captions[index].alignment = (TextAnchor) 3;
      ((Component) this.captions[index]).transform.SetParent(this.rows[index], true);
      MenuButtonEffects.Bind(((Component) this.captions[index]).transform, this.rows[index]);
      bool[] flagArray = new bool[2]{ false, true };
      foreach (bool flag in flagArray)
      {
        string str = $"Increment{(flag ? "Plus" : "Minus")}{index.ToString()}";
        Transform transform2 = this.Face(str, (vector3 + new Vector3(3f / 500f, flag ? -0.22f : -0.164f, 0.0f)), new Vector2(0.045f, 0.047f), flag ? "+" : "-");
        transform2.SetParent(this.rows[index], true);
        this.controls[str] = transform2;
      }
    }
    this.advance = this.Label("PickerAdvance", (this.visual.InverseTransformPoint(this.controls["ButtonHome"].position) + (Vector3.right * 0.009f)), new Vector2(0.36f, 0.05f), "Next", 28);
    ((Component) this.advance).transform.SetParent(this.controls["ButtonHome"], true);
    this.keyboard = new GameObject("ParrotKeyboard").transform;
    this.keyboard.SetParent(this.visual, false);
    this.query = this.Label("SearchQuery", new Vector3(0.064f, 0.0f, 0.267f), new Vector2(0.5f, 0.05f), "Search_", 28);
    ((Component) this.query).transform.SetParent(this.keyboard, true);
    string[] strArray2 = new string[4]
    {
      "1234567890",
      "QWERTYUIOP",
      "ASDFGHJKL",
      "ZXCVBNM"
    };
    for (int index1 = 0; index1 < strArray2.Length; ++index1)
    {
      for (int index2 = 0; index2 < strArray2[index1].Length; ++index2)
      {
        string caption = strArray2[index1][index2].ToString();
        this.Key("Key_" + caption, caption, new Vector3(0.052f, (float) (((double) (strArray2[index1].Length - 1) * 0.5 - (double) index2) * 0.052000001072883606), (float) (0.17000000178813934 - (double) index1 * 0.097000002861022949)), new Vector2(0.047f, 0.078f));
      }
    }
    string[] strArray3 = new string[4]
    {
      "Back",
      "Clear",
      "Space",
      "Done"
    };
    for (int index = 0; index < strArray3.Length; ++index)
      this.Key("Key_" + strArray3[index], strArray3[index], new Vector3(0.052f, (float) (0.19499999284744263 - (double) index * 0.12999999523162842), -0.23f), new Vector2(0.12f, 0.07f));
    this.noticeBody = this.Face("NoticeBackground", new Vector3(0.052f, 0.0f, -0.445f), new Vector2(0.52f, 0.05f), "");
    this.notice = ((Component) this.noticeBody).GetComponentInChildren<Text>();
    ((Object) this.notice).name = "NativeNoticeText";
    ((Component) this.notice).gameObject.AddComponent<NativeUiSemanticColor>();
    foreach (KeyValuePair<string, Transform> control in this.controls)
    {
      this.positions[control.Value] = control.Value.localPosition;
      this.scales[control.Value] = control.Value.localScale;
    }
    foreach (Component componentsInChild in ((Component) this.visual).GetComponentsInChildren<Transform>(true))
      componentsInChild.gameObject.layer = ((Component) root).gameObject.layer;
    this.SetActive(false);
  }
  private Transform Group(string name, string face, string[] names)
  {
    Transform transform1 = new GameObject(name).transform;
    transform1.SetParent(this.model, false);
    Transform transform2 = transform1;
    Bounds bounds = ((Component) this.model.Find(face)).GetComponent<Renderer>().bounds;
    Vector3 center = (bounds).center;
    transform2.position = center;
    foreach (string name1 in names)
    {
      Transform transform3 = this.model.Find(name1);
      if (((Object) transform3 != (Object) null))
        transform3.SetParent(transform1, true);
    }
    return transform1;
  }
  private Transform Face(string name, Vector3 position, Vector2 size, string caption)
  {
    Transform transform = new GameObject(name).transform;
    transform.SetParent(this.visual, false);
    transform.position = this.rows[0].position;
    string[] strArray = new string[2]
    {
      "Btn_Settings",
      "Btn_Settings_Border"
    };
    foreach (string str in strArray)
      Object.Instantiate<Transform>(this.rows[0].Find(str), transform, true);
    transform.localPosition = position;
    transform.localScale = new Vector3(1f, size.x / 0.52f, size.y / 0.077f);
    ((Component) this.Label(name + "Caption", (position + (Vector3.right * 0.009f)), new Vector2(size.x * 0.9f, size.y * 0.85f), caption, 28)).transform.SetParent(transform, true);
    return transform;
  }
  private void Key(string name, string caption, Vector3 position, Vector2 size)
  {
    Transform transform = this.Face(name, position, size, caption);
    transform.SetParent(this.keyboard, true);
    this.controls["Keyboard/" + name] = transform;
  }
  private Text Label(string name, Vector3 position, Vector2 size, string caption, int fontSize)
  {
    GameObject gameObject = new GameObject(name, new Type[3]
    {
      typeof (RectTransform),
      typeof (CanvasRenderer),
      typeof (Text)
    });
    gameObject.transform.SetParent(this.visual, false);
    gameObject.transform.localPosition = position;
    gameObject.transform.localRotation = Quaternion.LookRotation(Vector3.left, Vector3.forward);
    gameObject.transform.localScale = (Vector3.one * 1f / 1000f);
    Text component = gameObject.GetComponent<Text>();
    component.font = this.font;
    component.text = caption;
    component.fontStyle = (FontStyle) 1;
    ((Graphic) component).color = MenuPalette.Unpack(14337791U);
    component.fontSize = fontSize;
    component.alignment = (TextAnchor) 4;
    component.supportRichText = true;
    ((Graphic) component).raycastTarget = false;
    ((Graphic) component).rectTransform.sizeDelta = (size * 1000f);
    component.resizeTextForBestFit = true;
    component.resizeTextMinSize = 6;
    component.resizeTextMaxSize = fontSize;
    return component;
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
    bool flag = state.Category == 0 && state.Query.Length == 0 && state.Page == 0;
    ((Component) this.keyboard).gameObject.SetActive(state.KeyboardOpen);
    this.query.text = (state.Query.Length == 0 ? "Search" : state.Query) + "_";
    for (int index = 0; index < 6; ++index)
    {
      Transform transform = this.root.Find("Row" + index.ToString());
      ((Component) this.rows[index]).gameObject.SetActive(((Component) transform).gameObject.activeSelf);
      string str = ((Component) transform.Find("Label")).GetComponent<TextMesh>().text;
      string text = ((Component) transform.Find("State")).GetComponent<TextMesh>().text;
      if (flag)
      {
        if (str == "Security")
          str = "Safety";
        if (str == "Visual")
          str = "Visuals";
      }
      this.captions[index].text = str.ToUpperInvariant() + (text.Length == 0 ? "" : "  " + text.Replace("\n", " "));
      ((Component) this.meshLabels[index]).gameObject.SetActive(flag && state.FontIndex == 1);
      ((Component) this.captions[index]).gameObject.SetActive(!flag || state.FontIndex != 1);
    }
    bool colorPickerOpen = state.ColorPickerOpen;
    ((Component) this.advance).gameObject.SetActive(colorPickerOpen);
    this.advance.text = state.EditingEnd ? "Done" : "Next";
    ((Component) this.controls["ButtonHome"].Find("Title_Text")).gameObject.SetActive(!colorPickerOpen);
    foreach (KeyValuePair<string, Transform> control in this.controls)
    {
      Transform transform = this.root.Find(control.Key);
      if (!((Object) transform == (Object) null))
      {
        if (!control.Key.StartsWith("Row"))
          ((Component) control.Value).gameObject.SetActive(((Component) transform).gameObject.activeSelf);
        string str1;
        if (!control.Key.StartsWith("Row"))
          str1 = (string) null;
        else
          str1 = "Btn_" + new string[6]
          {
            "Settings",
            "Movement",
            "Room",
            "Safety",
            "Visuals",
            "Overpowered"
          }[int.Parse(control.Key.Substring(3))];
        string str2 = str1;
        Renderer renderer = str2 == null ? ((Component) control.Value).GetComponentInChildren<Renderer>() : ((Component) control.Value.Find(str2)).GetComponent<Renderer>();
        if (!((Object) renderer == (Object) null))
        {
          Bounds localBounds = renderer.localBounds;
          Bounds bounds;
          bounds = new Bounds(this.root.InverseTransformPoint(((Component) renderer).transform.TransformPoint((localBounds).center)), Vector3.zero);
          for (int index = 0; index < 8; ++index)
            (bounds).Encapsulate(this.root.InverseTransformPoint(((Component) renderer).transform.TransformPoint(((localBounds).center + Vector3.Scale((localBounds).extents, new Vector3((index & 1) == 0 ? -1f : 1f, (index & 2) == 0 ? -1f : 1f, (index & 4) == 0 ? -1f : 1f))))));
          transform.localPosition = (bounds).center;
          transform.localScale = Vector3.one;
          ((Component) transform).GetComponent<BoxCollider>().center = Vector3.zero;
          ((Component) transform).GetComponent<BoxCollider>().size = ((bounds).size + (Vector3.right * 0.01f));
        }
      }
    }
    ((Component) this.noticeBody).gameObject.SetActive(((Component) this.root.Find("NotificationBackground")).gameObject.activeSelf);
    this.notice.text = ((Component) this.root.Find("PageLabel")).GetComponent<TextMesh>().text;
    GradientPicker.Place(this.root, new Vector3(0.0525f, 0.0f, 0.0f), 1f);
    foreach (Renderer componentsInChild in ((Component) this.root.Find("ColorPicker")).GetComponentsInChildren<Renderer>(true))
      componentsInChild.enabled = colorPickerOpen;
    this.Tick(Time.unscaledTime);
  }
  public void Tick(float time)
  {
    if (!this.active || this.state == null)
      return;
    foreach (KeyValuePair<string, Transform> control in this.controls)
    {
      Transform transform = this.root.Find(control.Key);
      if (!((Object) transform == (Object) null))
      {
        float num = Mathf.Clamp(transform.localScale.x, 0.85f, 1.03f);
        Vector3 scale = this.scales[control.Value];
        control.Value.localScale = (scale * num);
        Vector3 vector3 = control.Value.parent.InverseTransformVector(this.root.right);
        control.Value.localPosition = (this.positions[control.Value] - (vector3 * (float) ((1.0 - (double) num) * 0.00800000037997961)));
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
