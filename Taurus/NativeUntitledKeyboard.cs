
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class NativeUntitledKeyboard : MonoBehaviour
{
  private NativeUiSnapshot snapshot;
  private TMP_Text display;
  private TMP_Text placeholder;
  private Sprite roundedsprite;
  private string currenttext;
  private Action<string> oninput;
  private static readonly Color ACCENT = Color.white;
  private static readonly Color BG = new Color(0.04f, 0.04f, 0.04f, 1f);
  private static readonly Color PANEL = new Color(0.078f, 0.078f, 0.078f, 1f);
  private static readonly Color KEY_BG = new Color(0.078f, 0.078f, 0.078f, 1f);
  private static readonly Color KEY_BORDER = new Color(1f, 1f, 1f, 0.06f);
  private static readonly Color KEY_HOVER = new Color(0.22f, 0.22f, 0.22f, 1f);
  private static readonly Color TXT = Color.white;
  private static readonly Color TXT_DIM = new Color(0.55f, 0.55f, 0.55f, 1f);
  internal static void Build(NativeUiSnapshot snapshot, string query, bool rounded)
  {
    GameObject gameObject1 = new GameObject("OriginalUntitledKeyboard", new Type[2]
    {
      typeof (RectTransform),
      typeof (Canvas)
    });
    gameObject1.GetComponent<Canvas>().renderMode = (RenderMode) 2;
    gameObject1.transform.SetPositionAndRotation(new Vector3(0.085f, 0.01f, -0.048f), Quaternion.Euler(180f, 90f, 90f));
    gameObject1.transform.localScale = (Vector3.one * 0.000405301922f);
    gameObject1.transform.SetParent(snapshot.Source.transform, true);
    GameObject gameObject2 = new GameObject("KeyboardUi", new Type[1]
    {
      typeof (RectTransform)
    });
    gameObject2.transform.SetParent(gameObject1.transform, false);
    NativeUntitledKeyboard untitledKeyboard = gameObject2.AddComponent<NativeUntitledKeyboard>();
    untitledKeyboard.snapshot = snapshot;
    untitledKeyboard.currenttext = query;
    untitledKeyboard.roundedsprite = new NativeUntitled().Round(rounded ? 10 : 0);
    untitledKeyboard.buildui();
  }
  private void RegisterKey(string key, Transform control)
  {
    string str;
    switch (key)
    {
      case "clear":
        str = "Clear";
        break;
      case "space":
        str = "Space";
        break;
      case "←":
        str = "Back";
        break;
      default:
        str = key.ToUpperInvariant();
        break;
    }
    this.snapshot.Controls["Keyboard/Key_" + str] = control;
  }
  private void buildui()
  {
    float num1 = 679f;
    float num2 = 470f;
    Image image = ((Component) this).gameObject.AddComponent<Image>();
    ((Graphic) image).color = NativeUntitledKeyboard.BG;
    image.sprite = this.roundedsprite;
    image.type = (Image.Type) 1;
    image.pixelsPerUnitMultiplier = 3f;
    ((Component) this).GetComponent<RectTransform>().sizeDelta = new Vector2(num1, num2);
    float num3 = num2 / 2f;
    GameObject gameObject1 = this.createblock(((Component) this).transform, "header", NativeUntitledKeyboard.PANEL, new Vector2(0.0f, num3 - 22f), new Vector2(num1, 44f));
    this.createstroke(((Component) this).transform, "Stroke", NativeUntitledKeyboard.ACCENT, new Vector2((float) ((0.0 - (double) num1) / 2.0 + 2.0), num3 - 22f), new Vector2(4f, 44f));
    this.createtext(gameObject1.transform, "untitled / search", 22, (FontStyles) 1, NativeUntitledKeyboard.TXT, new Vector2((float) ((0.0 - (double) num1) / 2.0 + 28.0 + 90.0), 0.0f), new Vector2(360f, 28f), (TextAlignmentOptions) 513);
    this.createtext(gameObject1.transform, "Tap search to close", 14, (FontStyles) 0, NativeUntitledKeyboard.TXT_DIM, new Vector2((float) ((double) num1 / 2.0 - 28.0 - 60.0), 0.0f), new Vector2(140f, 20f), (TextAlignmentOptions) 516);
    this.createblock(((Component) this).transform, "Rule", new Color(1f, 1f, 1f, 0.1f), new Vector2(0.0f, (float) ((double) num3 - 44.0 - 1.0)), new Vector2(num1 - 56f, 1f));
    float num4 = (float) ((double) num3 - 44.0 - 50.0);
    GameObject gameObject2 = this.createblock(((Component) this).transform, "Field", NativeUntitledKeyboard.PANEL, new Vector2(0.0f, num4), new Vector2(623f, 64f));
    this.createtext(gameObject2.transform, "/", 30, (FontStyles) 1, NativeUntitledKeyboard.ACCENT, new Vector2(-285.5f, 0.0f), new Vector2(28f, 40f), (TextAlignmentOptions) 514);
    this.snapshot.Controls["ButtonSearch"] = gameObject2.transform;
    this.placeholder = this.createtext(gameObject2.transform, "search mods, settings, players...", 22, (FontStyles) 0, NativeUntitledKeyboard.TXT_DIM, new Vector2(-241.5f, 0.0f), new Vector2(523f, 40f), (TextAlignmentOptions) 513);
    this.display = this.createtext(gameObject2.transform, this.currenttext, 26, (FontStyles) 1, NativeUntitledKeyboard.TXT, new Vector2(-241.5f, 0.0f), new Vector2(523f, 40f), (TextAlignmentOptions) 513);
    this.createblock(((Component) this).transform, "Caret", new Color(NativeUntitledKeyboard.ACCENT.r, NativeUntitledKeyboard.ACCENT.g, NativeUntitledKeyboard.ACCENT.b, 0.55f), new Vector2(0.0f, num4 - 36f), new Vector2(619f, 2f));
    this.updateplaceholdervisibility();
    string[] strArray = new string[4]
    {
      "1234567890",
      "qwertyuiop",
      "asdfghjkl",
      "zxcvbnm"
    };
    float num5 = num4 - 100f;
    for (int index1 = 0; index1 < strArray.Length; ++index1)
    {
      string str = strArray[index1];
      float num6 = (float) ((0.0 - ((double) str.Length * 63.0 - 7.0)) / 2.0 + 28.0);
      for (int index2 = 0; index2 < str.Length; ++index2)
      {
        char ch = str[index2];
        string capture;
        this.createkey(capture = ch.ToString(), new Vector2(num6 + (float) index2 * 63f, num5 - (float) index1 * 63f), new Vector2(56f, 56f), (Action) (() => this.typechar(capture)), false);
      }
    }
    float num7 = (float) ((double) num5 - 252.0 - 4.0);
    float num8 = 110f;
    float num9 = (float) (623.0 - (double) num8 * 2.0 - 14.0);
    float num10 = (float) ((double) num8 / 2.0 - 311.5);
    float num11 = (float) (311.5 - (double) num8 / 2.0);
    this.createkey("clear", new Vector2(num10, num7), new Vector2(num8, 56f), (Action) (() =>
    {
      this.currenttext = "";
      this.updatedisplay();
    }), false);
    this.createkey("space", new Vector2(0.0f, num7), new Vector2(num9, 56f), (Action) (() => this.typechar(" ")), false);
    this.createkey("←", new Vector2(num11, num7), new Vector2(num8, 56f), new Action(this.backspace), true);
  }
  private void createstroke(Transform parent, string nm, Color c, Vector2 pos, Vector2 size)
  {
    GameObject gameObject = new GameObject(nm);
    gameObject.transform.SetParent(parent, false);
    ((Graphic) gameObject.AddComponent<Image>()).color = c;
    RectTransform component = gameObject.GetComponent<RectTransform>();
    component.sizeDelta = size;
    component.anchoredPosition = pos;
  }
  private void updateplaceholdervisibility()
  {
    if (!((Object) this.placeholder != (Object) null))
      return;
    ((Behaviour) this.placeholder).enabled = string.IsNullOrEmpty(this.currenttext);
  }
  private void createkey(string label, Vector2 pos, Vector2 size, Action click, bool accent)
  {
    Color color1 = accent ? new Color(NativeUntitledKeyboard.ACCENT.r, NativeUntitledKeyboard.ACCENT.g, NativeUntitledKeyboard.ACCENT.b, 0.85f) : NativeUntitledKeyboard.KEY_BG;
    if (!accent)
    {
      Color keyHover = NativeUntitledKeyboard.KEY_HOVER;
    }
    else
    {
      Color color2 = new Color(NativeUntitledKeyboard.ACCENT.r, NativeUntitledKeyboard.ACCENT.g, NativeUntitledKeyboard.ACCENT.b, 1f);
    }
    Color color3 = accent ? NativeUntitledKeyboard.TXT : NativeUntitledKeyboard.TXT;
    GameObject gameObject1 = this.createblock(((Component) this).transform, "key" + label, color1, pos, size);
    if (!accent)
    {
      GameObject gameObject2 = new GameObject("border");
      gameObject2.transform.SetParent(gameObject1.transform, false);
      Image image1 = gameObject2.AddComponent<Image>();
      image1.sprite = this.roundedsprite;
      image1.type = (Image.Type) 1;
      ((Graphic) image1).color = NativeUntitledKeyboard.KEY_BORDER;
      image1.pixelsPerUnitMultiplier = 3f;
      RectTransform component1 = gameObject2.GetComponent<RectTransform>();
      component1.anchorMin = Vector2.zero;
      component1.anchorMax = Vector2.one;
      component1.offsetMin = Vector2.zero;
      component1.offsetMax = Vector2.zero;
      ((Graphic) image1).raycastTarget = false;
      ((MaskableGraphic) image1).maskable = false;
      GameObject gameObject3 = new GameObject("inner");
      gameObject3.transform.SetParent(gameObject2.transform, false);
      Image image2 = gameObject3.AddComponent<Image>();
      image2.sprite = this.roundedsprite;
      image2.type = (Image.Type) 1;
      ((Graphic) image2).color = color1;
      image2.pixelsPerUnitMultiplier = 3f;
      ((Graphic) image2).raycastTarget = false;
      RectTransform component2 = gameObject3.GetComponent<RectTransform>();
      component2.anchorMin = Vector2.zero;
      component2.anchorMax = Vector2.one;
      component2.offsetMin = new Vector2(1f, 1f);
      component2.offsetMax = new Vector2(-1f, -1f);
    }
    Button button = gameObject1.AddComponent<Button>();
    ((Selectable) button).transition = (Selectable.Transition) 0;
        ((UnityEvent) button.onClick).AddListener(new UnityAction(() => click()));
    BoxCollider boxCollider = gameObject1.AddComponent<BoxCollider>();
    ((Collider) boxCollider).isTrigger = true;
    boxCollider.size = new Vector3(size.x, size.y, 14f);
    this.RegisterKey(label, gameObject1.transform);
    int size1 = label.Length == 1 ? 26 : 20;
    FontStyles style = label == "←" ? (FontStyles) 1 : (FontStyles) 1;
    this.createtext(gameObject1.transform, label, size1, style, color3, Vector2.zero, size, (TextAlignmentOptions) 514);
  }
  private void typechar(string c)
  {
    if (this.currenttext.Length >= 32 )
      return;
    this.currenttext += c;
    this.updatedisplay();
  }
  private void backspace()
  {
    if (this.currenttext.Length <= 0)
      return;
    this.currenttext = this.currenttext.Substring(0, this.currenttext.Length - 1);
    this.updatedisplay();
  }
  private void updatedisplay()
  {
    this.display.text = this.currenttext;
    this.updateplaceholdervisibility();
    Action<string> oninput = this.oninput;
    if (oninput == null)
      return;
    oninput(this.currenttext);
  }
  private GameObject createblock(
    Transform parent,
    string name,
    Color color,
    Vector2 pos,
    Vector2 size)
  {
    GameObject gameObject = new GameObject(name);
    gameObject.transform.SetParent(parent, false);
    Image image = gameObject.AddComponent<Image>();
    image.sprite = this.roundedsprite;
    image.type = (Image.Type) 1;
    ((Graphic) image).color = color;
    image.pixelsPerUnitMultiplier = 3f;
    RectTransform component = gameObject.GetComponent<RectTransform>();
    component.sizeDelta = size;
    component.anchoredPosition = pos;
    ((Transform) component).localPosition = new Vector3(((Transform) component).localPosition.x, ((Transform) component).localPosition.y, 0.0f);
    return gameObject;
  }
  private TMP_Text createtext(
    Transform parent,
    string text,
    int size,
    FontStyles style,
    Color color,
    Vector2 pos,
    Vector2 dim,
    TextAlignmentOptions align)
  {
    GameObject gameObject = new GameObject(nameof (text));
    gameObject.transform.SetParent(parent, false);
    RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
    rectTransform.sizeDelta = dim;
    rectTransform.anchoredPosition = pos;
    TextMeshProUGUI textMeshProUgui = gameObject.AddComponent<TextMeshProUGUI>();
    ((TMP_Text) textMeshProUgui).text = text;
    ((TMP_Text) textMeshProUgui).fontSize = (float) size;
    ((Graphic) textMeshProUgui).color = color;
    ((TMP_Text) textMeshProUgui).alignment = align;
    ((TMP_Text) textMeshProUgui).fontStyle = style;
    ((TMP_Text) textMeshProUgui).richText = true;
    ((TMP_Text) textMeshProUgui).enableWordWrapping = false;
    ((TMP_Text) textMeshProUgui).overflowMode = (TextOverflowModes) 1;
    ((Graphic) textMeshProUgui).raycastTarget = false;
    ((Transform) rectTransform).localPosition = new Vector3(((Transform) rectTransform).localPosition.x, ((Transform) rectTransform).localPosition.y, -1f);
    rectTransform.pivot = new Vector2((align & (TextAlignmentOptions) 4) != 0 ? 1f : ((align & (TextAlignmentOptions) 1) != 0 ? 0.0f : 0.5f), 0.5f);
    rectTransform.anchoredPosition = pos;
    return (TMP_Text) textMeshProUgui;
  }
  private Sprite makeroundedsprite()
  {
    int num1 = 64 ;
    int num2 = 16 ;
    Texture2D texture2D = new Texture2D(num1, num1, (TextureFormat) 5, false);
    ((Texture) texture2D).filterMode = (FilterMode) 1;
    Color[] colorArray = new Color[num1 * num1];
    for (int index1 = 0; index1 < num1; ++index1)
    {
      for (int index2 = 0; index2 < num1; ++index2)
      {
        bool flag1 = index2 < num2;
        bool flag2 = index2 >= num1 - num2;
        bool flag3 = index1 < num2;
        bool flag4 = index1 >= num1 - num2;
        if (flag1 | flag2 && flag3 | flag4)
        {
          int num3 = flag1 ? num2 : num1 - num2 - 1;
          int num4 = flag3 ? num2 : num1 - num2 - 1;
          float num5 = Vector2.Distance(new Vector2((float) index2, (float) index1), new Vector2((float) num3, (float) num4));
          colorArray[index1 * num1 + index2] = (double) num5 > (double) (num2 - 1) ? ((double) num5 <= (double) num2 ? new Color(1f, 1f, 1f, (float) (1.0 - ((double) num5 - (double) (num2 - 1)))) : Color.clear) : Color.white;
        }
        else
          colorArray[index1 * num1 + index2] = Color.white;
      }
    }
    texture2D.SetPixels(colorArray);
    texture2D.Apply();
    return Sprite.Create(texture2D, new Rect(0.0f, 0.0f, (float) num1, (float) num1), new Vector2(0.5f, 0.5f), 100f, 0U, (SpriteMeshType) 0, new Vector4((float) num2, (float) num2, (float) num2, (float) num2));
  }
}
