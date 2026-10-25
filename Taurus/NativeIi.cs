
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Taurus;
internal sealed class NativeIi
{
  private readonly NativeUiSnapshot snapshot = new NativeUiSnapshot();
  private GameObject menu;
  private GameObject menuBackground;
  private GameObject canvasObj;
  private TMP_Text title;
  private TMP_FontAsset activeFont;
  private Material solid;
  private FontStyles activeFontStyle = (FontStyles) 2;
  private bool thinMenu = true;
  private bool incrementalButtons = true;
  private bool swapButtonColors;
  private bool isKeyboardPc;
  private bool hidetitle;
  private int PageSize = 6;
  private int buttonOffset = 2;
  private int pageButtonType = 1;
  private int pageNumber;
  private int LastPage;
  private int arrowType;
  private string[][] arrowTypes = new string[1][]
  {
    new string[2]{ "<", ">" }
  };
  private Material searchMat;
  private Material returnMat;
  private Texture2D searchIcon;
  private Texture2D returnIcon;
  private bool isSearching;
  private bool shouldRound;
  private bool noPageNumber;
  private bool inTextInput;
  private readonly Dictionary<string, Transform> labels = new Dictionary<string, Transform>();
  private readonly Dictionary<string, string> bindings = new Dictionary<string, string>();
  private readonly NativeIi.ExtGradient backgroundColor = new NativeIi.ExtGradient()
  {
    colors = NativeIi.ExtGradient.GetSimpleGradient(((Color32)new Color32(byte.MaxValue, (byte) 128 , (byte) 0, byte.MaxValue)), ((Color32)new Color32(byte.MaxValue, (byte) 102, (byte) 0, byte.MaxValue)))
  };
  private readonly NativeIi.ExtGradient[] buttonColors = new NativeIi.ExtGradient[2]
  {
    new NativeIi.ExtGradient()
    {
      colors = NativeIi.ExtGradient.GetSolidGradient(((Color32)new Color32((byte) 170, (byte) 85, (byte) 0, byte.MaxValue)))
    },
    new NativeIi.ExtGradient()
    {
      colors = NativeIi.ExtGradient.GetSolidGradient(((Color32)new Color32((byte) 85, (byte) 42, (byte) 0, byte.MaxValue)))
    }
  };
  private readonly NativeIi.ExtGradient[] textColors = new NativeIi.ExtGradient[3]
  {
    new NativeIi.ExtGradient()
    {
      colors = NativeIi.ExtGradient.GetSolidGradient(((Color32)new Color32(byte.MaxValue, (byte) 190, (byte) 125, byte.MaxValue)))
    },
    new NativeIi.ExtGradient()
    {
      colors = NativeIi.ExtGradient.GetSolidGradient(((Color32)new Color32(byte.MaxValue, (byte) 190, (byte) 125, byte.MaxValue)))
    },
    new NativeIi.ExtGradient()
    {
      colors = NativeIi.ExtGradient.GetSolidGradient(((Color32)new Color32(byte.MaxValue, (byte) 190, (byte) 125, byte.MaxValue)))
    }
  };
  private float ButtonDistance => 0.8f / (float) (this.PageSize + this.buttonOffset);
  private Color DarkenColor(Color color, float amount) => Color.Lerp(color, Color.black, amount);
  private string FollowMenuSettings(string text) => text;
  private void FollowMenuSettings(TMP_Text text) => text.characterSpacing = -8f;
  private void FollowMenuSettings(MaskableGraphic graphic)
  {
  }
  private void FollowMenuSettings(GameObject obj, bool shouldBeEnabled = true)
  {
    if (!this.shouldRound)
      return;
    this.RoundMenuObject(obj);
  }
  public NativeUiSnapshot Build(MenuState state, Transform proxies)
  {
    this.solid = ((Component) proxies.Find("Background")).GetComponent<Renderer>().sharedMaterial;
    this.activeFont = ImportedAssetMenu.Load("ii-current").LoadAsset<TMP_FontAsset>("Agency");
    if (((Object) this.activeFont == (Object) null))
      throw new InvalidOperationException("Original ii Agency font missing");
    this.pageNumber = state.Page;
    this.LastPage = state.PageCount - 1;
    this.isSearching = state.Query.Length > 0 || state.KeyboardOpen;
    this.shouldRound = state.Rounded;
    this.DrawFrame();
    this.bindings["Disconnect"] = "ButtonDisconnect";
    this.bindings["Search"] = "ButtonSearch";
    this.bindings["Global Return"] = "ButtonHome";
    this.bindings["PreviousPage"] = "ButtonPrevious";
    this.bindings["NextPage"] = "ButtonNext";
    this.AddButton(-0.3f, -1, new NativeIi.ButtonInfo()
    {
      buttonText = "Disconnect"
    });
    this.AddSearchButton();
    this.AddReturnButton(true);
    this.AddPageButtons();
    Transform transform = new GameObject("NativeContent").transform;
    transform.SetParent(this.menu.transform, false);
    this.snapshot.Content = transform;
    for (int index = 0; index < state.VisibleRows; ++index)
    {
      if (state.EntryAt(index) >= 0 || state.SoundNameAt(index) != null)
      {
        string str = ((Component) proxies.Find($"Row{index.ToString()}/Label")).GetComponent<TextMesh>().text.Replace("\n", " ");
        string text = ((Component) proxies.Find($"Row{index.ToString()}/State")).GetComponent<TextMesh>().text;
        bool activeSelf = ((Component) proxies.Find("IncrementPlus" + index.ToString())).gameObject.activeSelf;
        NativeIi.ButtonInfo method = new NativeIi.ButtonInfo()
        {
          buttonText = str + (activeSelf ? " : " + text : ""),
          incremental = activeSelf,
          enabled = state.IsOn(state.EntryAt(index))
        };
        this.bindings[method.buttonText] = "Row" + index.ToString();
        HashSet<Transform> transformSet = new HashSet<Transform>((IEnumerable<Transform>) this.menu.GetComponentsInChildren<Transform>(true));
        this.AddButton((float) (index + this.buttonOffset) * this.ButtonDistance, index, method);
        foreach (Transform componentsInChild in this.menu.GetComponentsInChildren<Transform>(true))
        {
          if (!transformSet.Contains(componentsInChild) && transformSet.Contains(componentsInChild.parent))
            componentsInChild.SetParent(transform, true);
        }
      }
    }
    foreach (NativeIi.NativeIiControl componentsInChild in this.menu.GetComponentsInChildren<NativeIi.NativeIiControl>(true))
    {
      string str;
      if (this.bindings.TryGetValue(componentsInChild.relatedText, out str))
        this.snapshot.Controls[componentsInChild.incremental ? (componentsInChild.positive ? "IncrementPlus" : "IncrementMinus") + str.Substring(3) : str] = ((Component) componentsInChild).transform;
    }
    foreach (NativeIi.NativeIiControl componentsInChild1 in this.menu.GetComponentsInChildren<NativeIi.NativeIiControl>(true))
    {
      NativeUiClick nativeUiClick = ((Component) componentsInChild1).gameObject.AddComponent<NativeUiClick>();
      nativeUiClick.Name = componentsInChild1.relatedText;
      nativeUiClick.RegisterBody(((Component) componentsInChild1).transform);
      foreach (NativeIi.NativeIiClamp componentsInChild2 in this.menu.GetComponentsInChildren<NativeIi.NativeIiClamp>(true))
      {
        if (((Object) componentsInChild2.targetRenderer == (Object) ((Component) componentsInChild1).GetComponent<Renderer>()))
          nativeUiClick.RegisterBody(((Component) componentsInChild2).transform);
      }
      Transform body;
      if (!componentsInChild1.incremental && this.labels.TryGetValue(componentsInChild1.relatedText, out body))
        nativeUiClick.RegisterUniform(body);
    }
    this.snapshot.Source = this.menu;
    this.snapshot.Header = this.title;
    if (state.KeyboardOpen)
      this.BuildKeyboard();
    if (state.ColorPickerOpen)
    {
      Transform control = this.snapshot.Controls["ButtonHome"];
      foreach (Image componentsInChild in this.canvasObj.GetComponentsInChildren<Image>(true))
      {
        if (((Object) ((Graphic) componentsInChild).material == (Object) this.returnMat))
          ((Component) componentsInChild).gameObject.SetActive(false);
      }
      TMP_Text tmpText = Object.Instantiate<TMP_Text>(this.title, this.canvasObj.transform, false);
      ((Component) tmpText).GetComponent<NativeIi.NativeIiInk>().colors = this.textColors[1];
      tmpText.text = state.EditingEnd ? "Done" : "Next";
      tmpText.rectTransform.sizeDelta = new Vector2(0.04f, 0.025f);
      tmpText.transform.position = (control.position + (Vector3.right * 0.008f));
    }
    foreach (NativeIi.NativeIiColor componentsInChild in this.menu.GetComponentsInChildren<NativeIi.NativeIiColor>(true))
    {
      Material material = new Material(this.solid);
      material.color = componentsInChild.colors.GetColor(0);
      ((Component) componentsInChild).GetComponent<Renderer>().sharedMaterial = material;
      this.snapshot.Materials.Add(material);
    }
    foreach (NativeIi.NativeIiInk componentsInChild in this.menu.GetComponentsInChildren<NativeIi.NativeIiInk>(true))
    {
      TMP_Text tmpText;
      if (((Component) componentsInChild).TryGetComponent<TMP_Text>(out tmpText))
      {
        ((Graphic) tmpText).color = componentsInChild.colors.GetColor(0);
      }
      else
      {
        Graphic graphic;
        if (((Component) componentsInChild).TryGetComponent<Graphic>(out graphic))
          graphic.color = componentsInChild.colors.GetColor(0);
      }
    }
    foreach (NativeIi.NativeIiClamp componentsInChild in this.menu.GetComponentsInChildren<NativeIi.NativeIiClamp>(true))
      ((Component) componentsInChild).GetComponent<Renderer>().sharedMaterial = componentsInChild.targetRenderer.sharedMaterial;
    if (((Object) this.searchMat != (Object) null))
      this.snapshot.Materials.Add(this.searchMat);
    if (((Object) this.returnMat != (Object) null))
      this.snapshot.Materials.Add(this.returnMat);
    return this.snapshot;
  }
  private void BuildKeyboard()
  {
    GameObject gameObject = Object.Instantiate<GameObject>(ImportedAssetMenu.Load("ii-current").LoadAsset<GameObject>("VRKeyboard"));
    ((Object) gameObject).name = "OriginalIiKeyboard";
    gameObject.transform.SetParent(this.menu.transform, false);
    Transform transform1 = gameObject.transform.Find("Numbers/1");
    Transform transform2 = gameObject.transform.Find("Numbers/0");
    Transform transform3 = gameObject.transform.Find("Letters/Q") ?? gameObject.transform.Find("Letters/q");
    Transform transform4 = gameObject.transform.Find("Letters/Z") ?? gameObject.transform.Find("Letters/z");
    Vector3 vector3_1 = (transform2.position - transform1.position);
    Vector3 normalized1 = (vector3_1).normalized;
    Vector3 vector3_2 = (transform3.position - transform4.position);
    Vector3 normalized2 = (vector3_2).normalized;
    Vector3 vector3_3 = normalized2;
    Vector3 vector3_4 = Vector3.Cross(normalized1, vector3_3);
    Vector3 normalized3 = (vector3_4).normalized;
    gameObject.transform.rotation = (Quaternion.LookRotation(Vector3.left, Vector3.forward) * Quaternion.Inverse(Quaternion.LookRotation(normalized3, normalized2)));
    Bounds bounds1 = ImportedSurface.BoundsOf(gameObject.transform);
    float y = (bounds1).size.y;
    Transform transform5 = gameObject.transform;
    transform5.localScale = (transform5.localScale * 0.27f / Mathf.Max(y, 1f / 1000f));
    Bounds bounds2 = ImportedSurface.BoundsOf(gameObject.transform);
    Transform transform6 = gameObject.transform;
    transform6.position = (transform6.position + (new Vector3(0.085f, 0.0f, -0.025f) - (bounds2).center));
    foreach (Renderer componentsInChild in gameObject.GetComponentsInChildren<Renderer>(true))
    {
      Material[] materialArray = new Material[componentsInChild.sharedMaterials.Length];
      for (int index = 0; index < materialArray.Length; ++index)
      {
        Material sharedMaterial = componentsInChild.sharedMaterials[index];
        Material material = ((Component) componentsInChild).transform.IsChildOf(gameObject.transform.Find("Canvas")) ? new Material(Graphic.defaultGraphicMaterial) : new Material(this.solid);
        material.color = ((Object) ((Component) componentsInChild).transform).name == "Background" ? this.backgroundColor.GetColor(0) : (((Component) componentsInChild).transform.IsChildOf(gameObject.transform.Find("Canvas")) ? this.textColors[1].GetColor(0) : this.buttonColors[0].GetColor(0));
        material.mainTexture = sharedMaterial.HasProperty("_BaseMap") ? sharedMaterial.GetTexture("_BaseMap") : (sharedMaterial.HasProperty("_MainTex") ? sharedMaterial.mainTexture : (Texture) null);
        materialArray[index] = material;
        this.snapshot.Materials.Add(material);
      }
      componentsInChild.sharedMaterials = materialArray;
    }
    string[] strArray = new string[4]
    {
      "Numbers",
      "Letters",
      "Special",
      "Seperate"
    };
    foreach (string str1 in strArray)
    {
      foreach (Transform transform7 in gameObject.transform.Find(str1))
      {
        string str2 = ((Object) transform7).name.ToUpperInvariant();
        switch (str2)
        {
          case "BACKSPACE":
          case "BACK":
            str2 = "Back";
            break;
          case "ENTER":
          case "RETURN":
            str2 = "Done";
            break;
          case "SPACE":
            str2 = "Space";
            break;
          case "CLEAR":
            str2 = "Clear";
            break;
        }
        if (str2.Length == 1 || str2 == "Back" || str2 == "Done" || str2 == "Space" || str2 == "Clear")
          this.snapshot.Controls["Keyboard/Key_" + str2] = transform7;
      }
    }
  }
  private void DrawFrame()
  {
    this.menu = GameObject.CreatePrimitive((PrimitiveType) 3);
    NativeUiSnapshot.Destroy((Object) this.menu.GetComponent<BoxCollider>());
    NativeUiSnapshot.Destroy((Object) this.menu.GetComponent<Renderer>());
    this.menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.3825f);
    this.menuBackground = GameObject.CreatePrimitive((PrimitiveType) 3);
    NativeUiSnapshot.Destroy((Object) this.menuBackground.GetComponent<BoxCollider>());
    this.menuBackground.transform.parent = this.menu.transform;
    this.menuBackground.transform.localPosition = new Vector3(0.5f, 0.0f, 0.0f);
    this.menuBackground.transform.rotation = Quaternion.identity;
    this.menuBackground.gameObject.AddComponent<NativeUiThemeRole>().Background = true;
    this.menuBackground.transform.localScale = this.thinMenu ? new Vector3(0.1f, 1f, 1f) : new Vector3(0.1f, 1.5f, 1f);
    this.menuBackground.AddComponent<NativeIi.NativeIiColor>().colors = this.backgroundColor;
    this.FollowMenuSettings(this.menuBackground);
    this.canvasObj = new GameObject();
    this.canvasObj.transform.parent = this.menu.transform;
    Canvas canvas = this.canvasObj.AddComponent<Canvas>();
    CanvasScaler canvasScaler = this.canvasObj.AddComponent<CanvasScaler>();
    canvas.renderMode = (RenderMode) 2;
    canvasScaler.dynamicPixelsPerUnit = 2500f;
    this.canvasObj.AddComponent<GraphicRaycaster>();
    this.title = (TMP_Text) new GameObject()
    {
      transform = {
        parent = this.canvasObj.transform
      }
    }.AddComponent<TextMeshPro>();
    this.title.font = this.activeFont;
    this.title.text = "ii's <b>Stupid</b> Menu";
    this.title.text = this.title.text;
    if (!this.noPageNumber)
      this.title.text += $" <color=grey>[</color><color=white>{this.pageNumber + 1}</color><color=grey>]</color>";
    this.title.fontSize = 1f;
    ((Component) this.title).gameObject.AddComponent<NativeIi.NativeIiInk>().colors = this.textColors[0];
    this.title.richText = true;
    this.title.fontStyle = this.activeFontStyle;
    this.title.alignment = (TextAlignmentOptions) 514;
    this.title.enableAutoSizing = true;
    this.title.fontSizeMin = 0.0f;
    RectTransform component = ((Component) this.title).GetComponent<RectTransform>();
    ((Transform) component).localPosition = Vector3.zero;
    component.sizeDelta = new Vector2(0.28f, 0.05f);
    ((Transform) component).localPosition = new Vector3(0.06f, 0.0f, 0.165f);
    ((Transform) component).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
    this.FollowMenuSettings(this.title);
  }
  private void AddButton(float offset, int buttonIndex, NativeIi.ButtonInfo method)
  {
    if (!method.label)
    {
      GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
      if (!this.isKeyboardPc)
        primitive.layer = 2;
      ((Collider) primitive.GetComponent<BoxCollider>()).isTrigger = true;
      primitive.transform.parent = this.menu.transform;
      primitive.transform.rotation = Quaternion.identity;
      primitive.transform.localScale = this.thinMenu ? new Vector3(0.09f, 0.9f, this.ButtonDistance * 0.8f) : new Vector3(0.09f, 1.3f, this.ButtonDistance * 0.8f);
      primitive.transform.localPosition = new Vector3(0.56f, 0.0f, 0.28f - offset);
      NativeIi.NativeIiControl nativeIiControl = primitive.AddComponent<NativeIi.NativeIiControl>();
      nativeIiControl.relatedText = method.buttonText;
      if (this.incrementalButtons && method.incremental)
      {
        Transform transform = primitive.transform;
        transform.localScale = (transform.localScale - new Vector3(0.0f, 0.254f, 0.0f));
        NativeUiSnapshot.Destroy((Object) nativeIiControl);
        this.RenderIncrementalButton(false, offset, buttonIndex, method);
        this.RenderIncrementalButton(true, offset, buttonIndex, method);
      }
      bool flag = this.swapButtonColors && buttonIndex < 0;
      primitive.AddComponent<NativeIi.NativeIiColor>().colors = this.buttonColors[flag ^ method.enabled ? 1 : 0];
      this.FollowMenuSettings(primitive, flag ? method.enabled : !method.enabled);
    }
    TextMeshPro text1 = new GameObject()
    {
      transform = {
        parent = this.canvasObj.transform
      }
    }.AddComponent<TextMeshPro>();
    string text2 = method.overlapText ?? method.buttonText;
    if (method.detected)
      text2 = $"<color=red>{text2}</color>";
    string str = this.FollowMenuSettings(text2);
    ((TMP_Text) text1).font = this.activeFont;
    ((TMP_Text) text1).text = str;
    ((TMP_Text) text1).richText = true;
    ((TMP_Text) text1).fontSize = 1f;
    ((Component) text1).gameObject.AddComponent<NativeIi.NativeIiInk>().colors = this.textColors[!method.enabled ? 1 : 2];
    ((TMP_Text) text1).alignment = (TextAlignmentOptions) 514;
    ((TMP_Text) text1).fontStyle = this.activeFontStyle;
    ((TMP_Text) text1).enableAutoSizing = true;
    ((TMP_Text) text1).fontSizeMin = 0.0f;
    RectTransform component = ((Component) text1).GetComponent<RectTransform>();
    ((Transform) component).localPosition = Vector3.zero;
    component.sizeDelta = new Vector2(!method.incremental || !this.incrementalButtons ? 0.2f : 0.18f, (float) (0.029999999329447746 * ((double) this.ButtonDistance / 0.10000000149011612)));
    ((Transform) component).localPosition = new Vector3(0.064f, 0.0f, (float) (0.11100000143051147 - (double) offset / 2.5999999046325684));
    ((Transform) component).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
    this.FollowMenuSettings((TMP_Text) text1);
    this.labels[method.buttonText] = text1.transform;
  }
  private void AddSearchButton()
  {
    GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
    if (!this.isKeyboardPc)
      primitive.layer = 2;
    ((Collider) primitive.GetComponent<BoxCollider>()).isTrigger = true;
    primitive.transform.parent = this.menu.transform;
    primitive.transform.rotation = Quaternion.identity;
    primitive.transform.localScale = new Vector3(0.09f, 0.102f, 0.08f);
    primitive.transform.localPosition = this.thinMenu ? new Vector3(0.56f, -0.45f, -0.58f) : new Vector3(0.56f, -0.7f, -0.58f);
    primitive.AddComponent<NativeIi.NativeIiControl>().relatedText = "Search";
    primitive.AddComponent<NativeIi.NativeIiColor>().colors = this.buttonColors[!(this.isSearching ^ !this.swapButtonColors) ? 1 : 0];
    this.FollowMenuSettings(primitive, this.isSearching ^ !this.swapButtonColors);
    Image graphic = new GameObject()
    {
      transform = {
        parent = this.canvasObj.transform
      }
    }.AddComponent<Image>();
    if (((Object) this.searchIcon == (Object) null))
      this.searchIcon = NativeUiResources.LoadImage("ii-search.png");
    if (((Object) this.searchMat == (Object) null))
      this.searchMat = new Material(((Graphic) graphic).material);
    ((Graphic) graphic).material = this.searchMat;
    ((Graphic) graphic).material.SetTexture("_MainTex", (Texture) this.searchIcon);
    ((Component) graphic).gameObject.AddComponent<NativeIi.NativeIiInk>().colors = this.textColors[!this.isSearching ? 1 : 2];
    RectTransform component = ((Component) graphic).GetComponent<RectTransform>();
    ((Transform) component).localPosition = Vector3.zero;
    component.sizeDelta = new Vector2(0.03f, 0.03f);
    ((Transform) component).localPosition = this.thinMenu ? new Vector3(0.064f, -0.134615391f, -0.223076925f) : new Vector3(0.064f, -0.209401712f, -0.223076925f);
    ((Transform) component).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
    this.FollowMenuSettings((MaskableGraphic) graphic);
  }
  private void AddReturnButton(bool offcenteredPosition)
  {
    GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
    if (!this.isKeyboardPc)
      primitive.layer = 2;
    ((Collider) primitive.GetComponent<BoxCollider>()).isTrigger = true;
    primitive.transform.parent = this.menu.transform;
    primitive.transform.rotation = Quaternion.identity;
    primitive.transform.localScale = new Vector3(0.09f, 0.102f, 0.08f);
    primitive.transform.localPosition = this.thinMenu ? new Vector3(0.56f, -0.45f, -0.58f) : new Vector3(0.56f, -0.7f, -0.58f);
    if (offcenteredPosition)
    {
      Transform transform = primitive.transform;
      transform.localPosition = (transform.localPosition + new Vector3(0.0f, 0.16f, 0.0f));
    }
    primitive.AddComponent<NativeIi.NativeIiControl>().relatedText = "Global Return";
    NativeIi.NativeIiColor nativeIiColor = primitive.AddComponent<NativeIi.NativeIiColor>();
    NativeIi.ExtGradient buttonColor;
    NativeIi.ExtGradient extGradient = buttonColor = this.buttonColors[this.swapButtonColors ? 1 : 0];
    nativeIiColor.colors = buttonColor;
    nativeIiColor.colors = extGradient;
    this.FollowMenuSettings(primitive, !this.swapButtonColors);
    Image graphic = new GameObject()
    {
      transform = {
        parent = this.canvasObj.transform
      }
    }.AddComponent<Image>();
    if (((Object) this.returnIcon == (Object) null))
      this.returnIcon = NativeUiResources.LoadImage("ii-return.png");
    if (((Object) this.returnMat == (Object) null))
      this.returnMat = new Material(((Graphic) graphic).material);
    ((Graphic) graphic).material = this.returnMat;
    ((Graphic) graphic).material.SetTexture("_MainTex", (Texture) this.returnIcon);
    ((Component) graphic).gameObject.AddComponent<NativeIi.NativeIiInk>().colors = this.textColors[1];
    RectTransform component = ((Component) graphic).GetComponent<RectTransform>();
    ((Transform) component).localPosition = Vector3.zero;
    component.sizeDelta = new Vector2(0.03f, 0.03f);
    ((Transform) component).localPosition = this.thinMenu ? new Vector3(0.064f, -0.134615391f, -0.223076925f) : new Vector3(0.064f, -0.209401712f, -0.223076925f);
    if (offcenteredPosition)
    {
      RectTransform rectTransform = component;
      ((Transform) rectTransform).localPosition = (((Transform) rectTransform).localPosition + new Vector3(0.0f, 0.0475f, 0.0f));
    }
    ((Transform) component).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
    this.FollowMenuSettings((MaskableGraphic) graphic);
  }
  private void RenderIncrementalButton(
    bool increment,
    float offset,
    int buttonIndex,
    NativeIi.ButtonInfo method)
  {
    if (!method.label)
    {
      GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
      if (!this.isKeyboardPc)
        primitive.layer = 2;
      ((Collider) primitive.GetComponent<BoxCollider>()).isTrigger = true;
      primitive.transform.parent = this.menu.transform;
      primitive.transform.rotation = Quaternion.identity;
      primitive.transform.localScale = new Vector3(0.09f, 0.102f, this.ButtonDistance * 0.8f);
      primitive.transform.localPosition = this.thinMenu ? new Vector3(0.56f, 0.399f, 0.28f - offset) : new Vector3(0.56f, 0.599f, 0.28f - offset);
      NativeIi.NativeIiControl nativeIiControl = primitive.AddComponent<NativeIi.NativeIiControl>();
      nativeIiControl.relatedText = method.buttonText;
      nativeIiControl.incremental = true;
      nativeIiControl.positive = increment;
      if (increment)
        primitive.transform.localPosition = new Vector3(primitive.transform.localPosition.x, 0.0f - primitive.transform.localPosition.y, primitive.transform.localPosition.z);
      primitive.AddComponent<NativeIi.NativeIiColor>().colors = this.buttonColors[0];
      this.FollowMenuSettings(primitive);
    }
    this.RenderIncrementalText(increment, offset);
  }
  public void RenderIncrementalText(bool increment, float offset)
  {
    TextMeshPro text = new GameObject()
    {
      transform = {
        parent = this.canvasObj.transform
      }
    }.AddComponent<TextMeshPro>();
    ((TMP_Text) text).font = this.activeFont;
    ((TMP_Text) text).text = increment ? "+" : "-";
    ((TMP_Text) text).richText = true;
    ((TMP_Text) text).fontSize = 1f;
    ((Component) text).gameObject.AddComponent<NativeIi.NativeIiInk>().colors = this.textColors[1];
    ((TMP_Text) text).alignment = (TextAlignmentOptions) 514;
    ((TMP_Text) text).fontStyle = this.activeFontStyle;
    ((TMP_Text) text).enableAutoSizing = true;
    ((TMP_Text) text).fontSizeMin = 0.0f;
    RectTransform component = ((Component) text).GetComponent<RectTransform>();
    ((Transform) component).localPosition = Vector3.zero;
    component.sizeDelta = new Vector2(0.2f, (float) (0.029999999329447746 * ((double) this.ButtonDistance / 0.10000000149011612)));
    ((Transform) component).localPosition = this.thinMenu ? new Vector3(0.064f, increment ? -0.12f : 0.12f, (float) (0.11100000143051147 - (double) offset / 2.5999999046325684)) : new Vector3(0.064f, increment ? -0.18f : 0.18f, (float) (0.11100000143051147 - (double) offset / 2.5999999046325684));
    ((Transform) component).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
    this.FollowMenuSettings((TMP_Text) text);
  }
  private void AddPageButtons()
  {
    NativeIi.ExtGradient buttonColor = this.buttonColors[this.swapButtonColors ? 1 : 0];
    switch (this.pageButtonType == 3 || this.pageButtonType == 4 ? 1 : this.pageButtonType)
    {
      case 1:
        this.CreatePageButtonPair("PreviousPage", "NextPage", new Vector3(0.09f, this.thinMenu ? 0.9f : 1.3f, this.ButtonDistance * 0.8f), new Vector3(0.56f, 0.0f, (float) (0.2800000011920929 - (double) this.ButtonDistance * (double) (this.buttonOffset - 2))), new Vector3(0.56f, 0.0f, (float) (0.2800000011920929 - (double) this.ButtonDistance * (double) (this.buttonOffset - 1))), new Vector3(0.064f, 0.0f, (float) (0.10899999737739563 - (double) this.ButtonDistance * (double) (this.buttonOffset - 2) / 2.5499999523162842)), new Vector3(0.064f, 0.0f, (float) (0.10899999737739563 - (double) this.ButtonDistance * (double) (this.buttonOffset - 1) / 2.5499999523162842)), buttonColor);
        break;
      case 2:
        this.CreatePageButtonPair("PreviousPage", "NextPage", new Vector3(0.09f, 0.2f, 0.9f), new Vector3(0.56f, this.thinMenu ? 0.65f : 0.9f, 0.0f), new Vector3(0.56f, this.thinMenu ? -0.65f : -0.9f, 0.0f), new Vector3(0.064f, this.thinMenu ? 0.195f : 0.267f, 0.0f), new Vector3(0.064f, this.thinMenu ? -0.195f : -0.267f, 0.0f), buttonColor);
        break;
      case 5:
        this.CreatePageButtonPair("PreviousPage", "NextPage", new Vector3(0.09f, this.hidetitle ? 0.1f : 0.3f, 0.05f), new Vector3(0.56f, (float) ((this.thinMenu ? 0.29899999499320984 : 0.49900001287460327) + (this.hidetitle ? 0.10000000149011612 : 0.0)), (float) (0.35499998927116394 + (this.hidetitle ? 0.10000000149011612 : 0.0))), new Vector3(0.56f, (float) ((this.thinMenu ? -0.29899999499320984 : -0.49900001287460327) - (this.hidetitle ? 0.10000000149011612 : 0.0)), (float) (0.35499998927116394 + (this.hidetitle ? 0.10000000149011612 : 0.0))), new Vector3(0.064f, (float) ((this.thinMenu ? 0.090000003576278687 : 0.15000000596046448) + (this.hidetitle ? 0.035000000149011612 : 0.0)), (float) (0.13500000536441803 + (this.hidetitle ? 0.037500001490116119 : 0.0))), new Vector3(0.064f, (float) ((this.thinMenu ? -0.090000003576278687 : -0.15000000596046448) - (this.hidetitle ? 0.035000000149011612 : 0.0)), (float) (0.13500000536441803 + (this.hidetitle ? 0.037500001490116119 : 0.0))), buttonColor);
        break;
      case 6:
        this.CreatePageButtonPair("PreviousPage", "NextPage", new Vector3(0.09f, 0.102f, 0.08f), new Vector3(0.56f, this.thinMenu ? 0.45f : 0.7f, -0.58f), (new Vector3(0.56f, this.thinMenu ? 0.45f : 0.7f, -0.58f) - new Vector3(0.0f, 0.16f, 0.0f)), new Vector3(0.064f, this.thinMenu ? 0.134615391f : 0.209401712f, -0.214814812f), (new Vector3(0.064f, this.thinMenu ? 0.134615391f : 0.209401712f, -0.214814812f) - new Vector3(0.0f, 0.0475f, 0.0f)), buttonColor, new Vector2?(new Vector2(0.03f, 0.03f)));
        break;
    }
  }
  private void CreatePageButtonPair(
    string prevButtonName,
    string nextButtonName,
    Vector3 buttonScale,
    Vector3 prevButtonPos,
    Vector3 nextButtonPos,
    Vector3 prevTextPos,
    Vector3 nextTextPos,
    NativeIi.ExtGradient color,
    Vector2? textSize = null)
  {
    bool interactable = this.LastPage > 0;
    this.AdvancedAddButton(prevButtonName, buttonScale, prevButtonPos, prevTextPos, color, textSize, 0, interactable);
    this.AdvancedAddButton(nextButtonName, buttonScale, nextButtonPos, nextTextPos, color, textSize, 1, interactable);
  }
  private NativeIi.ExtGradient DimGradient(NativeIi.ExtGradient source)
  {
    GradientColorKey[] gradientColorKeyArray = new GradientColorKey[source.colors.Length];
    for (int index = 0; index < gradientColorKeyArray.Length; ++index)
      gradientColorKeyArray[index] = new GradientColorKey(this.DarkenColor(source.colors[index].color, 0.45f), source.colors[index].time);
    return new NativeIi.ExtGradient()
    {
      colors = gradientColorKeyArray
    };
  }
  private GameObject AdvancedAddButton(
    string buttonName,
    Vector3 scale,
    Vector3 position,
    Vector3 textPosition,
    NativeIi.ExtGradient color,
    Vector2? textSize,
    int arrowIndex,
    bool interactable = true)
  {
    GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
    if (!this.inTextInput || !this.isKeyboardPc)
      primitive.layer = 2;
    ((Collider) primitive.GetComponent<BoxCollider>()).isTrigger = true;
    primitive.transform.parent = this.menu.transform;
    primitive.transform.rotation = Quaternion.identity;
    primitive.transform.localScale = scale;
    primitive.transform.localPosition = position;
    NativeIi.NativeIiControl nativeIiControl = primitive.AddComponent<NativeIi.NativeIiControl>();
    nativeIiControl.relatedText = buttonName;
    nativeIiControl.interactable = interactable;
    primitive.AddComponent<NativeIi.NativeIiColor>().colors = interactable ? color : this.DimGradient(color);
    TextMeshPro text = new GameObject()
    {
      transform = {
        parent = this.canvasObj.transform
      }
    }.AddComponent<TextMeshPro>();
    ((TMP_Text) text).font = this.activeFont;
    ((TMP_Text) text).text = this.arrowTypes[this.arrowType][arrowIndex];
    ((TMP_Text) text).fontSize = 1f;
    ((TMP_Text) text).alignment = (TextAlignmentOptions) 514;
    ((TMP_Text) text).enableAutoSizing = true;
    ((TMP_Text) text).fontSizeMin = 0.0f;
    ((Component) text).gameObject.AddComponent<NativeIi.NativeIiInk>().colors = interactable ? this.textColors[1] : this.DimGradient(this.textColors[1]);
    RectTransform component = ((Component) text).GetComponent<RectTransform>();
    component.sizeDelta = textSize ?? new Vector2(0.2f, 0.03f);
    ((Transform) component).localPosition = textPosition;
    ((Transform) component).rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
    this.FollowMenuSettings((TMP_Text) text);
    this.FollowMenuSettings(primitive, !this.swapButtonColors);
    return primitive;
  }
  public void RoundMenuObject(GameObject toRound, float Bevel = 0.02f)
  {
    if (((Object) toRound.transform.parent != (Object) this.menu?.transform))
    {
      this.RoundObject(toRound, Bevel);
    }
    else
    {
      Renderer component1 = toRound.GetComponent<Renderer>();
      GameObject primitive1 = GameObject.CreatePrimitive((PrimitiveType) 3);
      primitive1.GetComponent<Renderer>().enabled = component1.enabled;
      NativeUiSnapshot.Destroy((Object) primitive1.GetComponent<Collider>());
      primitive1.transform.parent = this.menu.transform;
      primitive1.transform.rotation = Quaternion.identity;
      primitive1.transform.localPosition = toRound.transform.localPosition;
      primitive1.transform.localScale = (toRound.transform.localScale + new Vector3(0.0f, Bevel * -2.55f, 0.0f));
      GameObject primitive2 = GameObject.CreatePrimitive((PrimitiveType) 3);
      primitive2.GetComponent<Renderer>().enabled = component1.enabled;
      NativeUiSnapshot.Destroy((Object) primitive2.GetComponent<Collider>());
      primitive2.transform.parent = this.menu.transform;
      primitive2.transform.rotation = Quaternion.identity;
      primitive2.transform.localPosition = toRound.transform.localPosition;
      primitive2.transform.localScale = (toRound.transform.localScale + new Vector3(0.0f, 0.0f, (float) ((0.0 - (double) Bevel) * 2.0)));
      GameObject primitive3 = GameObject.CreatePrimitive((PrimitiveType) 2);
      primitive3.GetComponent<Renderer>().enabled = component1.enabled;
      NativeUiSnapshot.Destroy((Object) primitive3.GetComponent<Collider>());
      primitive3.transform.parent = this.menu.transform;
      primitive3.transform.rotation = (Quaternion.identity * Quaternion.Euler(0.0f, 0.0f, 90f));
      primitive3.transform.localPosition = (toRound.transform.localPosition + new Vector3(0.0f, (float) ((double) toRound.transform.localScale.y / 2.0 - (double) Bevel * 1.2749999761581421), toRound.transform.localScale.z / 2f - Bevel));
      primitive3.transform.localScale = new Vector3(Bevel * 2.55f, toRound.transform.localScale.x / 2f, Bevel * 2f);
      GameObject primitive4 = GameObject.CreatePrimitive((PrimitiveType) 2);
      primitive4.GetComponent<Renderer>().enabled = component1.enabled;
      NativeUiSnapshot.Destroy((Object) primitive4.GetComponent<Collider>());
      primitive4.transform.parent = this.menu.transform;
      primitive4.transform.rotation = (Quaternion.identity * Quaternion.Euler(0.0f, 0.0f, 90f));
      primitive4.transform.localPosition = (toRound.transform.localPosition + new Vector3(0.0f, (float) (0.0 - (double) toRound.transform.localScale.y / 2.0 + (double) Bevel * 1.2749999761581421), toRound.transform.localScale.z / 2f - Bevel));
      primitive4.transform.localScale = new Vector3(Bevel * 2.55f, toRound.transform.localScale.x / 2f, Bevel * 2f);
      GameObject primitive5 = GameObject.CreatePrimitive((PrimitiveType) 2);
      primitive5.GetComponent<Renderer>().enabled = component1.enabled;
      NativeUiSnapshot.Destroy((Object) primitive5.GetComponent<Collider>());
      primitive5.transform.parent = this.menu.transform;
      primitive5.transform.rotation = (Quaternion.identity * Quaternion.Euler(0.0f, 0.0f, 90f));
      primitive5.transform.localPosition = (toRound.transform.localPosition + new Vector3(0.0f, (float) ((double) toRound.transform.localScale.y / 2.0 - (double) Bevel * 1.2749999761581421), (float) (0.0 - (double) toRound.transform.localScale.z / 2.0) + Bevel));
      primitive5.transform.localScale = new Vector3(Bevel * 2.55f, toRound.transform.localScale.x / 2f, Bevel * 2f);
      GameObject primitive6 = GameObject.CreatePrimitive((PrimitiveType) 2);
      primitive6.GetComponent<Renderer>().enabled = component1.enabled;
      NativeUiSnapshot.Destroy((Object) primitive6.GetComponent<Collider>());
      primitive6.transform.parent = this.menu.transform;
      primitive6.transform.rotation = (Quaternion.identity * Quaternion.Euler(0.0f, 0.0f, 90f));
      primitive6.transform.localPosition = (toRound.transform.localPosition + new Vector3(0.0f, (float) (0.0 - (double) toRound.transform.localScale.y / 2.0 + (double) Bevel * 1.2749999761581421), (float) (0.0 - (double) toRound.transform.localScale.z / 2.0) + Bevel));
      primitive6.transform.localScale = new Vector3(Bevel * 2.55f, toRound.transform.localScale.x / 2f, Bevel * 2f);
      GameObject[] gameObjectArray = new GameObject[6]
      {
        primitive1,
        primitive2,
        primitive3,
        primitive4,
        primitive5,
        primitive6
      };
      foreach (GameObject gameObject in gameObjectArray)
        gameObject.AddComponent<NativeIi.NativeIiClamp>().targetRenderer = component1;
      component1.enabled = false;
      NativeIi.NativeIiColor component2 = ((Component) component1).GetComponent<NativeIi.NativeIiColor>();
      if (!((Object)(Object) component2))
        return;
      component2.overrideTransparency = false;
    }
  }
  public void RoundObject(GameObject toRound, float bevel = 0.02f)
  {
    Renderer component1 = toRound.GetComponent<Renderer>();
    if (((Object) component1 == (Object) null))
      return;
    Transform transform = toRound.transform;
    Vector3 localScale = transform.localScale;
    bool enabled = component1.enabled;
    GameObject primitive1 = CreatePrimitive((PrimitiveType) 3, transform, enabled);
    primitive1.transform.localPosition = Vector3.zero;
    primitive1.transform.localRotation = Quaternion.identity;
    primitive1.transform.localScale = new Vector3(localScale.x, localScale.y - bevel * 2f, localScale.z);
    GameObject primitive2 = CreatePrimitive((PrimitiveType) 3, transform, enabled);
    primitive2.transform.localPosition = Vector3.zero;
    primitive2.transform.localRotation = Quaternion.identity;
    primitive2.transform.localScale = new Vector3(localScale.x, localScale.y, localScale.z - bevel * 2f);
    GameObject[] gameObjectArray1 = new GameObject[4];
    Vector3[] vector3Array = new Vector3[4]
    {
      new Vector3(0.0f, localScale.y / 2f - bevel, localScale.z / 2f - bevel),
      new Vector3(0.0f, (float) ((0.0 - (double) localScale.y) / 2.0) + bevel, localScale.z / 2f - bevel),
      new Vector3(0.0f, localScale.y / 2f - bevel, (float) ((0.0 - (double) localScale.z) / 2.0) + bevel),
      new Vector3(0.0f, (float) ((0.0 - (double) localScale.y) / 2.0) + bevel, (float) ((0.0 - (double) localScale.z) / 2.0) + bevel)
    };
    for (int index = 0; index < 4; ++index)
    {
      gameObjectArray1[index] = CreatePrimitive((PrimitiveType) 2, transform, enabled);
      gameObjectArray1[index].transform.localPosition = vector3Array[index];
      gameObjectArray1[index].transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 90f);
      gameObjectArray1[index].transform.localScale = new Vector3(bevel * 2f, localScale.x / 2f, bevel * 2f);
    }
    GameObject[] gameObjectArray2 = new GameObject[6]
    {
      primitive1,
      primitive2,
      gameObjectArray1[0],
      gameObjectArray1[1],
      gameObjectArray1[2],
      gameObjectArray1[3]
    };
    foreach (GameObject gameObject in gameObjectArray2)
      gameObject.AddComponent<NativeIi.NativeIiClamp>().targetRenderer = component1;
    component1.enabled = false;
    NativeIi.NativeIiColor component2 = ((Component) component1).GetComponent<NativeIi.NativeIiColor>();
    if (!((Object) component2 != (Object) null))
      return;
    component2.overrideTransparency = false;
    static GameObject CreatePrimitive(PrimitiveType type, Transform parent, bool flag)
    {
      GameObject primitive = GameObject.CreatePrimitive(type);
      primitive.GetComponent<Renderer>().enabled = flag;
      Collider component = primitive.GetComponent<Collider>();
      if (((Object) component != (Object) null))
        NativeUiSnapshot.Destroy((Object) component);
      primitive.transform.SetParent(parent, false);
      return primitive;
    }
  }
  private sealed class ButtonInfo
  {
    public string buttonText;
    public string overlapText;
    public bool label;
    public bool incremental;
    public bool enabled;
    public bool detected;
  }
  [Serializable]
  private sealed class ExtGradient
  {
    public GradientColorKey[] colors;
    public Color GetColor(int index) => this.colors[index].color;
    public static GradientColorKey[] GetSolidGradient(Color color)
    {
      return new GradientColorKey[2]
      {
        new GradientColorKey(color, 0.0f),
        new GradientColorKey(color, 1f)
      };
    }
    public static GradientColorKey[] GetSimpleGradient(Color a, Color b)
    {
      return new GradientColorKey[3]
      {
        new GradientColorKey(a, 0.0f),
        new GradientColorKey(b, 0.5f),
        new GradientColorKey(a, 1f)
      };
    }
  }
  public sealed class NativeIiControl : MonoBehaviour
  {
    public string relatedText;
    public bool incremental;
    public bool positive;
    public bool interactable;
  }
  private sealed class NativeIiColor : MonoBehaviour
  {
    public NativeIi.ExtGradient colors;
    public bool overrideTransparency;
  }
  private sealed class NativeIiInk : MonoBehaviour
  {
    public NativeIi.ExtGradient colors;
  }
  private sealed class NativeIiClamp : MonoBehaviour
  {
    public Renderer targetRenderer;
  }
}
