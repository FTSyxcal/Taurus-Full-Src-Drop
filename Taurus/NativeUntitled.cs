
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Taurus;
internal sealed class NativeUntitled
{
  private readonly NativeUntitled.SettingsData Settings = new NativeUntitled.SettingsData();
  private readonly NativeUiSnapshot snapshot = new NativeUiSnapshot();
  private readonly Dictionary<string, string> bindings = new Dictionary<string, string>();
  private readonly List<NativeUntitled.Button> rows = new List<NativeUntitled.Button>();
  private readonly List<Rect> placedRects = new List<Rect>();
  private readonly List<GameObject> hitCubes = new List<GameObject>();
  private string NewBadgeColor = "#7cc46b";
  private static readonly Dictionary<int, Sprite> sprCache = new Dictionary<int, Sprite>();
  private NativeUntitled.Category CurrentCategory;
  private int CurrentPage;
  private int ButtonsPerPage;
  private int toggleStyleIdx;
  private bool menuDelayed;
  private bool insearch;
  private bool inSafetyWarning;
  private bool PCMenuOpen;
  private bool isPCUser;
  private bool gradienttitle;
  private float CANVAS_W = 360f;
  private Color32 TextColor = ((Color32)Color.white);
  private Color32 OutlineColor = new Color32((byte) 120, (byte) 120, (byte) 120, byte.MaxValue);
  private Color32 BgColor1 = new Color32((byte) 6, (byte) 6, (byte) 6, byte.MaxValue);
  private Color32 BgColor2 = new Color32((byte) 14, (byte) 14, (byte) 14, byte.MaxValue);
  private Vector2 piv = (Vector2.one * 0.5f);
  private GameObject MenuObj;
  private GameObject CanvasObject;
  private GameObject DisconnectButton;
  private RectTransform cvRoot;
  private RectTransform contentRoot;
  private TMP_Text titleTMP;
  private TMP_Text pageTMP;
  private TMP_Text fpsTMP;
  private Text fps;
  private NativeUntitled.Button recentButton;
  private NativeUntitled.Button searchButton = new NativeUntitled.Button("SearchButton");
  private object selectedPlayerCate;
  private Texture2D customBackgroundImage;
  private Sprite logoSpr;
  private string fpsText = "";
  private readonly NativeUntitled.SearchData Search = new NativeUntitled.SearchData();
  private bool ShowSearch => this.CurrentCategory == NativeUntitled.Category.Base || this.insearch;
  private bool InPcCondition => false;
  public NativeUiSnapshot Build(MenuState state, Transform proxies)
  {
    NativeUntitled.Category result;
    this.CurrentCategory = state.Category == 0 ? NativeUntitled.Category.Base : (Enum.TryParse<NativeUntitled.Category>(MenuCatalog.Categories[state.Category].Replace(" ", ""), out result) ? result : NativeUntitled.Category.Custom);
    this.CurrentPage = state.Page;
    this.insearch = state.Query.Length > 0 || state.KeyboardOpen;
    this.Search.currenttext = state.Query;
    this.Settings.Rounding = state.Rounded;
    int resultCount = state.ResultCount;
    for (int index = 0; index < resultCount; ++index)
      this.rows.Add(new NativeUntitled.Button(""));
    for (int row = 0; row < state.VisibleRows; ++row)
    {
      int index = state.Page * state.VisibleRows + row;
      if (index < resultCount)
      {
        string str = ((Component) proxies.Find($"Row{row.ToString()}/Label")).GetComponent<TextMesh>().text.Replace("\n", " ");
        string text = ((Component) proxies.Find($"Row{row.ToString()}/State")).GetComponent<TextMesh>().text;
        bool activeSelf = ((Component) proxies.Find("IncrementPlus" + row.ToString())).gameObject.activeSelf;
        int num = state.EntryAt(row) < 0 ? 0 : (MenuCatalog.Buttons[state.EntryAt(row)].Name == "No Telemetry" ? 1 : 0);
        NativeUntitled.Button button = new NativeUntitled.Button((num != 0 ? $"<color=#888888>{str} [Disabled]</color>" : str) + (activeSelf ? " : " + text : ""), state.EntryAt(row) >= 0 && MenuCatalog.Buttons[state.EntryAt(row)].Toggle);
        if (num != 0)
          button.ToolTip = "Telemetry remains enabled";
        button.Enabled = state.IsOn(state.EntryAt(row));
        button.buttonType = activeSelf ? "Arrows" : "";
        this.bindings[button.Name] = "Row" + row.ToString();
        this.bindings[button.Name + "_DOWN"] = "IncrementMinus" + row.ToString();
        this.bindings[button.Name + "_UP"] = "IncrementPlus" + row.ToString();
        this.rows[index] = button;
      }
      else
        break;
    }
    this.bindings["SearchButton"] = "ButtonSearch";
    this.bindings["HomeButton"] = "ButtonHome";
    this.bindings["NextPage"] = "ButtonPrevious";
    this.bindings["PreviousPage"] = "ButtonNext";
    this.bindings["DisconnectButton"] = "ButtonDisconnect";
    this.logoSpr = NativeUiResources.UntitledLogo;
    this.DrawFrame();
    if (state.ColorPickerOpen)
    {
      RectTransform p = this.Card(this.cvRoot, new Vector2(0.0f, -249f), new Vector2(100f, 36f), this.RowFill(new NativeUntitled.Button("")), 10);
      this.Txt(p, state.EditingEnd ? "<b>Done</b>" : "<b>Next</b>", 12f);
      Transform transform;
      if (this.snapshot.Controls.TryGetValue("ButtonHome", out transform))
        ((Component) transform).gameObject.SetActive(false);
      this.snapshot.Controls["ButtonHome"] = (Transform) p;
    }
    this.snapshot.Source = this.MenuObj;
    this.snapshot.Content = (Transform) this.contentRoot;
    if (state.KeyboardOpen)
      NativeUntitledKeyboard.Build(this.snapshot, state.Query, state.Rounded);
    foreach (TMP_Text componentsInChild in this.MenuObj.GetComponentsInChildren<TMP_Text>(true))
    {
      if (((Object) componentsInChild).name == "l")
      {
        componentsInChild.enableAutoSizing = true;
        componentsInChild.fontSizeMin = 8f;
        componentsInChild.fontSizeMax = 15f;
        componentsInChild.enableWordWrapping = true;
        componentsInChild.overflowMode = (TextOverflowModes) 1;
      }
    }
    this.snapshot.Header = this.titleTMP;
    this.snapshot.Footer = this.pageTMP;
    Transform transform1;
    this.snapshot.HomeLabel = this.snapshot.Controls.TryGetValue("ButtonHome", out transform1) ? ((Component) transform1).GetComponentInChildren<TMP_Text>(true) : (TMP_Text) null;
    return this.snapshot;
  }
  private List<NativeUntitled.Button> GetButtonInfoByPage(NativeUntitled.Category category)
  {
    return this.rows;
  }
  private List<NativeUntitled.Button> GetSearchedButtons() => this.rows;
  private void StartEnabled()
  {
  }
  private void WarningThing()
  {
  }
  private void BuildTargetView()
  {
  }
  private void EnsureLogo()
  {
  }
  private void FxReset() => this.placedRects.Clear();
  private void FxFadeIn(RectTransform card)
  {
  }
  private void FxNextRow()
  {
  }
  public void HitBox(RectTransform card, NativeUntitled.Button b)
  {
    string key;
    if (!this.bindings.TryGetValue(b.Name, out key))
      return;
    this.snapshot.Controls[key] = (Transform) card;
  }
  public Vector2 AvoidOverlap(Vector2 pos, Vector2 sz, bool preferDown = true)
  {
    int num = 0;
    while (num++ < 24)
    {
      bool flag = false;
      Rect rect;
      rect = new Rect(pos.x - sz.x * 0.5f, pos.y - sz.y * 0.5f, sz.x, sz.y);
      foreach (Rect placedRect in this.placedRects)
      {
        if ((rect).Overlaps(placedRect))
        {
          if (!preferDown)
          {
            double yMax = (double) (placedRect).yMax;
            double yMin = (double) (rect).yMin;
          }
          else
          {
            double yMin = (double) (placedRect).yMin;
            double yMax = (double) (rect).yMax;
          }
          if (preferDown)
            pos.y -= Mathf.Max(2f, (float) ((double) (rect).yMax - (double) (placedRect).yMin + 4.0));
          else
            pos.y += Mathf.Max(2f, (float) ((double) (placedRect).yMax - (double) (rect).yMin + 4.0));
          flag = true;
          break;
        }
      }
      if (!flag)
        break;
    }
    return pos;
  }
  public void MarkRect(Vector2 pos, Vector2 sz)
  {
    this.placedRects.Add(new Rect(pos.x - sz.x * 0.5f, pos.y - sz.y * 0.5f, sz.x, sz.y));
  }
  public string ToggleSymbol(NativeUntitled.Button b)
  {
    if (b == null || !b.IsToggle)
      return ">";
    bool enabled = b.Enabled;
    string str;
    switch (this.toggleStyleIdx)
    {
      case 0:
        str = enabled ? "●" : "○";
        break;
      case 1:
        str = enabled ? "<color=#7cc46b>[X]</color>" : "<color=#666666>[ ]</color>";
        break;
      case 2:
        str = enabled ? "<size=10><b>ON</b></size>" : "<size=10><color=#666666>OFF</color></size>";
        break;
      case 3:
        str = ">";
        break;
      case 4:
        str = enabled ? "■" : "□";
        break;
      case 5:
        str = enabled ? "<color=#f5c542>*</color>" : "<color=#555555>.</color>";
        break;
      case 6:
        str = enabled ? "<color=#7cc46b><b>+</b></color>" : "<color=#888888><b>-</b></color>";
        break;
      case 7:
        str = enabled ? "<b>›</b>" : "<color=#555555>›</color>";
        break;
      case 8:
        str = enabled ? "<b>1</b>" : "<color=#555555><b>0</b></color>";
        break;
      case 9:
        str = enabled ? "<size=9><color=#7cc46b><b>YES</b></color></size>" : "<size=9><color=#888888>NO</color></size>";
        break;
      case 10:
        str = enabled ? "<color=#7cc46b><b>I</b></color>" : "<color=#888888><b>O</b></color>";
        break;
      case 11:
        str = enabled ? "<color=#7cc46b><b>V</b></color>" : "<color=#aa3030><b>X</b></color>";
        break;
      case 12:
        str = enabled ? "<b>|</b>" : "<color=#555555>-</color>";
        break;
      case 13:
        str = enabled ? "<color=#7cc46b><b>#</b></color>" : "<color=#555555>.</color>";
        break;
      case 14:
        str = enabled ? "<color=#f5c542><b>*</b></color>" : "<color=#555555>-</color>";
        break;
      case 15:
        str = "";
        break;
      default:
        str = enabled ? "●" : "○";
        break;
    }
    return str;
  }
  public Sprite Round(int r)
  {
    Sprite sprite1;
    if (NativeUntitled.sprCache.TryGetValue(r, out sprite1) && ((Object) sprite1 != (Object) null))
      return sprite1;
    int num1 = r * 4;
    int num2 = num1 * 2 + 2;
    Texture2D texture2D = new Texture2D(num2, num2, (TextureFormat) 4, false);
    ((Texture) texture2D).filterMode = (FilterMode) 1;
    ((Texture) texture2D).wrapMode = (TextureWrapMode) 1;
    Color[] colorArray = new Color[num2 * num2];
    float num3 = 1f / 16f;
    for (int index1 = 0; index1 < num2; ++index1)
    {
      for (int index2 = 0; index2 < num2; ++index2)
      {
        if (index2 >= num1 && index2 <= num2 - num1 - 1 && index1 >= num1 && index1 <= num2 - num1 - 1)
        {
          colorArray[index1 * num2 + index2] = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
          float num4 = index2 < num1 ? (float) num1 : (index2 > num2 - num1 - 1 ? (float) (num2 - num1 - 1) : (float) index2);
          float num5 = index1 < num1 ? (float) num1 : (index1 > num2 - num1 - 1 ? (float) (num2 - num1 - 1) : (float) index1);
          int num6 = 0;
          for (int index3 = 0; index3 < 4; ++index3)
          {
            float num7 = (float) ((double) index1 + ((double) index3 + 0.5) / 4.0 - 0.5) - num5;
            for (int index4 = 0; index4 < 4; ++index4)
            {
              double num8 = (double) index2 + ((double) index4 + 0.5) / 4.0 - 0.5 - (double) num4;
              if (num8 * num8 + (double) num7 * (double) num7 <= (double) (num1 * num1))
                ++num6;
            }
          }
          float num9 = (float) num6 * num3;
          colorArray[index1 * num2 + index2] = new Color(1f, 1f, 1f, num9);
        }
      }
    }
    texture2D.SetPixels(colorArray);
    texture2D.Apply();
    Sprite sprite2 = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, (float) num2, (float) num2), new Vector2(0.5f, 0.5f), 400f, 0U, (SpriteMeshType) 0, new Vector4((float) num1, (float) num1, (float) num1, (float) num1));
    NativeUntitled.sprCache[r] = sprite2;
    return sprite2;
  }
  public void LogoIcon(RectTransform parent, float size)
  {
    this.EnsureLogo();
    if (((Object) this.logoSpr == (Object) null))
    {
      ((Graphic) this.Txt(parent, "<b>H</b>", 14f)).color = new Color((float) this.TextColor.r / (float) byte.MaxValue, (float) this.TextColor.g / (float) byte.MaxValue, (float) this.TextColor.b / (float) byte.MaxValue, 1f);
    }
    else
    {
      GameObject gameObject = new GameObject("logo");
      gameObject.transform.SetParent((Transform) parent, false);
      RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
      Vector2 piv;
      Vector2 vector2_1 = piv = this.piv;
      rectTransform.pivot = piv;
      Vector2 vector2_2;
      Vector2 vector2_3 = vector2_2 = vector2_1;
      rectTransform.anchorMax = vector2_2;
      rectTransform.anchorMin = vector2_3;
      rectTransform.anchoredPosition = Vector2.zero;
      rectTransform.sizeDelta = new Vector2(size, size);
      Image image = gameObject.AddComponent<Image>();
      image.sprite = this.logoSpr;
      image.preserveAspect = true;
      ((Graphic) image).color = Color.white;
    }
  }
  public RectTransform Img(RectTransform p, Vector2 pos, Vector2 sz, Color c)
  {
    GameObject gameObject = new GameObject("i");
    gameObject.transform.SetParent((Transform) p, false);
    RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
    Vector2 piv;
    Vector2 vector2_1 = piv = this.piv;
    rectTransform.pivot = piv;
    Vector2 vector2_2;
    Vector2 vector2_3 = vector2_2 = vector2_1;
    rectTransform.anchorMax = vector2_2;
    rectTransform.anchorMin = vector2_3;
    rectTransform.anchoredPosition = pos;
    rectTransform.sizeDelta = sz;
    ((Graphic) gameObject.AddComponent<Image>()).color = c;
    return rectTransform;
  }
  public RectTransform Card(
    RectTransform p,
    Vector2 pos,
    Vector2 sz,
    Color c,
    int r,
    bool outline = true)
  {
    int r1 = !this.Settings.Rounding ? 1 : r;
    GameObject gameObject1 = new GameObject("k");
    gameObject1.transform.SetParent((Transform) p, false);
    RectTransform rectTransform1 = gameObject1.AddComponent<RectTransform>();
    Vector2 vector2_1 = rectTransform1.pivot = this.piv;
    Vector2 vector2_2 = rectTransform1.anchorMax = vector2_1;
    rectTransform1.anchorMin = vector2_2;
    rectTransform1.anchoredPosition = pos;
    rectTransform1.sizeDelta = sz;
    Image image1 = gameObject1.AddComponent<Image>();
    image1.sprite = this.Round(r1);
    image1.type = (Image.Type) 1;
    Color color = outline ? new Color((float) this.OutlineColor.r / (float) byte.MaxValue, (float) this.OutlineColor.g / (float) byte.MaxValue, (float) this.OutlineColor.b / (float) byte.MaxValue, 1f) : c;
    ((Graphic) image1).color = this.Settings.Outline & outline ? color : c;
    GameObject gameObject2 = new GameObject("f");
    gameObject2.transform.SetParent((Transform) rectTransform1, false);
    RectTransform rectTransform2 = gameObject2.AddComponent<RectTransform>();
    rectTransform2.anchorMin = Vector2.zero;
    rectTransform2.anchorMax = Vector2.one;
    rectTransform2.offsetMin = new Vector2(0.5f, 0.5f);
    rectTransform2.offsetMax = new Vector2(-0.5f, -0.5f);
    Image image2 = gameObject2.AddComponent<Image>();
    image2.sprite = this.Round(Mathf.Max(1, r1 - 1));
    image2.type = (Image.Type) 1;
    ((Graphic) image2).color = c;
    return rectTransform1;
  }
  public TMP_Text Txt(RectTransform p, string s, float fs)
  {
    GameObject gameObject = new GameObject("t");
    gameObject.transform.SetParent((Transform) p, false);
    RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
    rectTransform.anchorMin = Vector2.zero;
    rectTransform.anchorMax = Vector2.one;
    rectTransform.offsetMin = Vector2.zero;
    rectTransform.offsetMax = Vector2.zero;
    TextMeshProUGUI textMeshProUgui = gameObject.AddComponent<TextMeshProUGUI>();
    ((TMP_Text) textMeshProUgui).text = s;
    ((TMP_Text) textMeshProUgui).fontSize = fs;
    ((Graphic) textMeshProUgui).color = ((Color32)this.TextColor);
    ((TMP_Text) textMeshProUgui).alignment = (TextAlignmentOptions) 514;
    return (TMP_Text) textMeshProUgui;
  }
  public Color RowFill(NativeUntitled.Button b)
  {
    float num1 = (float) ((double) this.BgColor1.r / (double) byte.MaxValue + 0.054999999701976776);
    float num2 = (float) ((double) this.BgColor1.g / (double) byte.MaxValue + 0.054999999701976776);
    float num3 = (float) ((double) this.BgColor1.b / (double) byte.MaxValue + 0.054999999701976776);
    Color color;
    color = new Color(Mathf.Clamp01(num1), Mathf.Clamp01(num2), Mathf.Clamp01(num3), 1f);
    if (this.menuDelayed && b != null && b.delay)
      return new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, 1f);
    return !b.IsToggle || !b.Enabled ? color : new Color(Mathf.Clamp01(color.r + 0.1f), Mathf.Clamp01(color.g + 0.1f), Mathf.Clamp01(color.b + 0.1f), 1f);
  }
  public void BuildLabel(RectTransform row, string title, string sub, bool dim, bool newBadge = false)
  {
    Color color1;
    color1 = new Color((float) this.TextColor.r / (float) byte.MaxValue, (float) this.TextColor.g / (float) byte.MaxValue, (float) this.TextColor.b / (float) byte.MaxValue, 1f);
    Color color2;
    color2 = new Color(color1.r * 0.55f, color1.g * 0.55f, color1.b * 0.55f, 1f);
    Color color3 = dim ? new Color(color1.r * 0.5f, color1.g * 0.5f, color1.b * 0.5f, 1f) : color1;
    bool flag = !string.IsNullOrEmpty(sub);
    float num = newBadge ? -64f : -34f;
    GameObject gameObject1 = new GameObject("l");
    gameObject1.transform.SetParent((Transform) row, false);
    RectTransform rectTransform1 = gameObject1.AddComponent<RectTransform>();
    rectTransform1.anchorMin = Vector2.zero;
    rectTransform1.anchorMax = Vector2.one;
    rectTransform1.offsetMin = new Vector2(12f, 0.0f);
    rectTransform1.offsetMax = new Vector2(num, 0.0f);
    TextMeshProUGUI textMeshProUgui1 = gameObject1.AddComponent<TextMeshProUGUI>();
    ((TMP_Text) textMeshProUgui1).text = title;
    ((TMP_Text) textMeshProUgui1).enableAutoSizing = true;
    ((TMP_Text) textMeshProUgui1).fontSizeMin = 9f;
    ((TMP_Text) textMeshProUgui1).fontSizeMax = 14f;
    ((TMP_Text) textMeshProUgui1).fontSize = 14f;
    ((Graphic) textMeshProUgui1).color = color3;
    ((TMP_Text) textMeshProUgui1).fontStyle = (FontStyles) 1;
    ((TMP_Text) textMeshProUgui1).alignment = flag ? (TextAlignmentOptions) 257 : (TextAlignmentOptions) 4097;
    ((TMP_Text) textMeshProUgui1).enableWordWrapping = false;
    ((TMP_Text) textMeshProUgui1).overflowMode = (TextOverflowModes) 1;
    if (flag)
      ((TMP_Text) textMeshProUgui1).margin = new Vector4(0.0f, 6f, 0.0f, 0.0f);
    if (!flag)
      return;
    GameObject gameObject2 = new GameObject("s");
    gameObject2.transform.SetParent((Transform) row, false);
    RectTransform rectTransform2 = gameObject2.AddComponent<RectTransform>();
    rectTransform2.anchorMin = Vector2.zero;
    rectTransform2.anchorMax = Vector2.one;
    rectTransform2.offsetMin = new Vector2(12f, 0.0f);
    rectTransform2.offsetMax = new Vector2(num, 0.0f);
    TextMeshProUGUI textMeshProUgui2 = gameObject2.AddComponent<TextMeshProUGUI>();
    ((TMP_Text) textMeshProUgui2).text = sub;
    ((TMP_Text) textMeshProUgui2).enableAutoSizing = true;
    ((TMP_Text) textMeshProUgui2).fontSizeMin = 7f;
    ((TMP_Text) textMeshProUgui2).fontSizeMax = 9f;
    ((TMP_Text) textMeshProUgui2).fontSize = 9f;
    ((Graphic) textMeshProUgui2).color = color2;
    ((TMP_Text) textMeshProUgui2).alignment = (TextAlignmentOptions) 1025;
    ((TMP_Text) textMeshProUgui2).margin = new Vector4(0.0f, 0.0f, 0.0f, 8f);
    ((TMP_Text) textMeshProUgui2).overflowMode = (TextOverflowModes) 1;
    ((TMP_Text) textMeshProUgui2).enableWordWrapping = false;
  }
  public void BuildNewBadge(RectTransform row)
  {
    Color color = new Color();
    if (!ColorUtility.TryParseHtmlString(this.NewBadgeColor, out color))
      color = Color.green;
    GameObject gameObject = new GameObject("nw");
    gameObject.transform.SetParent((Transform) row, false);
    RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
    Vector2 vector2_1;
    vector2_1 = new Vector2(1f, 0.5f);
    rectTransform.pivot = vector2_1;
    Vector2 vector2_2;
    Vector2 vector2_3 = vector2_2 = vector2_1;
    rectTransform.anchorMax = vector2_2;
    rectTransform.anchorMin = vector2_3;
    rectTransform.anchoredPosition = new Vector2(-44f, 0.0f);
    rectTransform.sizeDelta = new Vector2(24f, 56f);
    TextMeshProUGUI textMeshProUgui = gameObject.AddComponent<TextMeshProUGUI>();
    ((TMP_Text) textMeshProUgui).text = "NEW";
    ((TMP_Text) textMeshProUgui).enableAutoSizing = true;
    ((TMP_Text) textMeshProUgui).fontSizeMin = 8f;
    ((TMP_Text) textMeshProUgui).fontSizeMax = 12f;
    ((TMP_Text) textMeshProUgui).fontSize = 12f;
    ((Graphic) textMeshProUgui).color = color;
    ((TMP_Text) textMeshProUgui).fontStyle = (FontStyles) 1;
    ((TMP_Text) textMeshProUgui).alignment = (TextAlignmentOptions) 514;
    ((TMP_Text) textMeshProUgui).enableWordWrapping = false;
  }
  public void BuildChev(RectTransform row, string ch)
  {
    Color color;
    color = new Color((float) this.TextColor.r / (float) byte.MaxValue, (float) this.TextColor.g / (float) byte.MaxValue, (float) this.TextColor.b / (float) byte.MaxValue, 1f);
    GameObject gameObject = new GameObject(nameof (ch));
    gameObject.transform.SetParent((Transform) row, false);
    RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
    Vector2 vector2_1;
    vector2_1 = new Vector2(1f, 0.5f);
    rectTransform.pivot = vector2_1;
    Vector2 vector2_2;
    Vector2 vector2_3 = vector2_2 = vector2_1;
    rectTransform.anchorMax = vector2_2;
    rectTransform.anchorMin = vector2_3;
    rectTransform.anchoredPosition = new Vector2(-12f, 0.0f);
    rectTransform.sizeDelta = new Vector2(28f, 58f);
    TextMeshProUGUI textMeshProUgui = gameObject.AddComponent<TextMeshProUGUI>();
    ((TMP_Text) textMeshProUgui).text = ch;
    ((TMP_Text) textMeshProUgui).enableAutoSizing = true;
    ((TMP_Text) textMeshProUgui).fontSizeMin = 10f;
    ((TMP_Text) textMeshProUgui).fontSizeMax = 18f;
    ((TMP_Text) textMeshProUgui).fontSize = 18f;
    ((Graphic) textMeshProUgui).color = color;
    ((TMP_Text) textMeshProUgui).alignment = (TextAlignmentOptions) 514;
    ((TMP_Text) textMeshProUgui).enableWordWrapping = false;
  }
  public void CreateButton(NativeUntitled.Button identifier, float yPos)
  {
    if (identifier == null || ((Object) this.contentRoot == (Object) null))
      return;
    int num1 = identifier.buttonType == "Arrows" ? 1 : 0;
    bool flag = identifier.PCkeybind != "null" || identifier.VRkeybind != "null";
    bool dim = this.menuDelayed && identifier.delay;
    string name = identifier.Name;
    Color c = this.RowFill(identifier);
    Color color;
    color = new Color((float) this.TextColor.r / (float) byte.MaxValue, (float) this.TextColor.g / (float) byte.MaxValue, (float) this.TextColor.b / (float) byte.MaxValue, 1f);
    float num2 = this.CANVAS_W - 16f;
    float num3 = 4f;
    if (num1 != 0)
    {
      string str = identifier.Name ?? "";
      string title = str;
      string s = str;
      int length = str.LastIndexOf(" : ");
      if (length >= 0)
      {
        title = str.Substring(0, length);
        s = str.Substring(length + 3);
      }
      float num4 = 32f;
      float num5 = 64f;
      float num6 = (float) ((double) num2 - (double) num4 * 2.0 - (double) num5 - (double) num3 * 3.0);
      float num7 = (float) ((0.0 - (double) num2) * 0.5 + (double) num6 * 0.5);
      float num8 = (float) ((double) num7 + (double) num6 * 0.5 + (double) num3 + (double) num4 * 0.5);
      float num9 = (float) ((double) num8 + (double) num4 * 0.5 + (double) num3 + (double) num5 * 0.5);
      float num10 = (float) ((double) num9 + (double) num5 * 0.5 + (double) num3 + (double) num4 * 0.5);
      RectTransform rectTransform1 = this.Card(this.contentRoot, new Vector2(num7, yPos), new Vector2(num6, 60f), c, 10);
      this.BuildLabel(rectTransform1, title, identifier.ToolTip, dim, identifier.isNew);
      if (identifier.isNew)
        this.BuildNewBadge(rectTransform1);
      RectTransform rectTransform2 = this.Card(this.contentRoot, new Vector2(num8, yPos), new Vector2(num4, 60f), c, 10);
      TMP_Text tmpText1 = this.Txt(rectTransform2, "<", 24f);
      ((Graphic) tmpText1).color = color;
      tmpText1.fontStyle = (FontStyles) 1;
      RectTransform rectTransform3 = this.Card(this.contentRoot, new Vector2(num9, yPos), new Vector2(num5, 60f), c, 10);
      TMP_Text tmpText2 = this.Txt(rectTransform3, s, 12f);
      ((Graphic) tmpText2).color = color;
      tmpText2.fontStyle = (FontStyles) 1;
      tmpText2.enableWordWrapping = false;
      tmpText2.overflowMode = (TextOverflowModes) 1;
      tmpText2.enableAutoSizing = true;
      tmpText2.fontSizeMin = 8f;
      tmpText2.fontSizeMax = 12f;
      identifier.arrowactualtext = (object) null;
      RectTransform rectTransform4 = this.Card(this.contentRoot, new Vector2(num10, yPos), new Vector2(num4, 60f), c, 10);
      TMP_Text tmpText3 = this.Txt(rectTransform4, ">", 24f);
      ((Graphic) tmpText3).color = color;
      tmpText3.fontStyle = (FontStyles) 1;
      this.HitBox(rectTransform2, new NativeUntitled.Button(identifier.Name + "_DOWN", action: (Action) (() =>
      {
        Action down = identifier.down;
        if (down == null)
          return;
        down();
      })));
      this.HitBox(rectTransform4, new NativeUntitled.Button(identifier.Name + "_UP", action: (Action) (() =>
      {
        Action up = identifier.up;
        if (up == null)
          return;
        up();
      })));
      this.FxFadeIn(rectTransform1);
      this.FxFadeIn(rectTransform2);
      this.FxFadeIn(rectTransform3);
      this.FxFadeIn(rectTransform4);
      this.FxNextRow();
    }
    else if (flag)
    {
      float num11 = 52f;
      float num12 = num2 - num11 - num3;
      float num13 = (float) ((0.0 - (double) num2) * 0.5 + (double) num12 * 0.5);
      float num14 = (float) ((double) num13 + (double) num12 * 0.5 + (double) num3 + (double) num11 * 0.5);
      RectTransform rectTransform5 = this.Card(this.contentRoot, new Vector2(num13, yPos), new Vector2(num12, 60f), c, 10);
      this.BuildLabel(rectTransform5, name, identifier.ToolTip, dim, identifier.isNew);
      this.BuildChev(rectTransform5, this.ToggleSymbol(identifier));
      if (identifier.isNew)
        this.BuildNewBadge(rectTransform5);
      this.HitBox(rectTransform5, identifier);
      RectTransform rectTransform6 = this.Card(this.contentRoot, new Vector2(num14, yPos), new Vector2(num11, 60f), c, 10);
      TMP_Text tmpText = this.Txt(rectTransform6, "<b>KEY</b>", 11f);
      ((Graphic) tmpText).color = color;
      tmpText.fontStyle = (FontStyles) 1;
      this.HitBox(rectTransform6, new NativeUntitled.Button(identifier.Name + "_EditButton"));
      this.FxFadeIn(rectTransform5);
      this.FxFadeIn(rectTransform6);
      this.FxNextRow();
    }
    else
    {
      RectTransform rectTransform = this.Card(this.contentRoot, new Vector2(0.0f, yPos), new Vector2(num2, 60f), c, 10);
      this.BuildLabel(rectTransform, name, identifier.ToolTip, dim, identifier.isNew);
      this.BuildChev(rectTransform, this.ToggleSymbol(identifier));
      if (identifier.isNew)
        this.BuildNewBadge(rectTransform);
      this.HitBox(rectTransform, identifier);
      this.FxFadeIn(rectTransform);
      this.FxNextRow();
    }
  }
  public void AddDisconnectButton()
  {
    if (((Object) this.cvRoot == (Object) null))
      return;
    float num1 = 140f;
    float num2 = 26f;
    float num3 = 0.0f;
    float num4 = (float) (280.0 + (double) num2 * 0.5 + 10.0);
    float num5 = 0.1f;
    Color c;
    c = new Color(Mathf.Max(num5, (float) this.BgColor2.r / (float) byte.MaxValue), Mathf.Max(num5, (float) this.BgColor2.g / (float) byte.MaxValue), Mathf.Max(num5, (float) this.BgColor2.b / (float) byte.MaxValue), 1f);
    RectTransform rectTransform = this.Card(this.cvRoot, new Vector2(num3, num4), new Vector2(num1, num2), c, 7);
    TMP_Text tmpText = this.Txt(rectTransform, "<b>DISCONNECT</b>", 12f);
    ((Graphic) tmpText).color = new Color((float) this.TextColor.r / (float) byte.MaxValue, (float) this.TextColor.g / (float) byte.MaxValue, (float) this.TextColor.b / (float) byte.MaxValue, 1f);
    tmpText.alignment = (TextAlignmentOptions) 514;
    tmpText.characterSpacing = 1.6f;
    this.HitBox(rectTransform, new NativeUntitled.Button("DisconnectButton", action: (Action) (() => { })));
    this.DisconnectButton = ((Component) rectTransform).gameObject;
  }
  public void AddPageButtons()
  {
    if (((Object) this.cvRoot == (Object) null))
      return;
    float num1 = 0.095f;
    Color c;
    c = new Color(Mathf.Max(num1, (float) ((double) this.BgColor1.r / (double) byte.MaxValue + 0.054999999701976776)), Mathf.Max(num1, (float) ((double) this.BgColor1.g / (double) byte.MaxValue + 0.054999999701976776)), Mathf.Max(num1, (float) ((double) this.BgColor1.b / (double) byte.MaxValue + 0.054999999701976776)), 1f);
    Color color;
    color = new Color((float) this.TextColor.r / (float) byte.MaxValue, (float) this.TextColor.g / (float) byte.MaxValue, (float) this.TextColor.b / (float) byte.MaxValue, 1f);
    float num2 = 36f;
    float num3 = -249f;
    float num4 = 6f;
    float num5 = num2;
    Vector2 pos1;
    pos1 = new Vector2((float) ((0.0 - (double) this.CANVAS_W) * 0.5 + (double) num5 * 0.5 + 6.0), num3);
    Vector2 sz1;
    sz1 = new Vector2(num5, num5);
    bool flag = this.Settings.HomeButtons && this.CurrentCategory != 0;
    Vector2 pos2;
    pos2 = new Vector2((float) ((double) this.CANVAS_W * 0.5 - (double) num5 * 0.5 - 6.0), num3);
    Vector2 sz2;
    sz2 = new Vector2(num5, num5);
    bool inPcCondition = this.InPcCondition;
    if (flag)
      this.MarkRect(pos1, sz1);
    if (inPcCondition)
      this.MarkRect(pos2, sz2);
    string s1 = "<b>‹  PREV</b>";
    string s2 = "<b>NEXT  ›</b>";
    Vector2 pos3;
    Vector2 pos4;
    Vector2 sz3;
    switch (this.Settings.PageSIndex)
    {
      case 1:
        float num6 = 36f;
        float num7 = 426f;
        float num8 = (float) ((double) this.CANVAS_W * 0.5 + (double) num6 * 0.5) + num4;
        pos3 = new Vector2(0.0f - num8, 0.0f);
        pos4 = new Vector2(num8, 0.0f);
        sz3 = new Vector2(num6, num7);
        s1 = "<b>‹</b>";
        s2 = "<b>›</b>";
        break;
      case 3:
        float num9 = 34f;
        float num10 = (float) ((double) this.CANVAS_W * 0.5 + (double) num9 * 0.5) + num4;
        float num11 = (float) (0.0 - (280.0 - (double) num9 * 0.5 - 4.0));
        pos3 = new Vector2(0.0f - num10, num11);
        pos4 = new Vector2(num10, num11);
        sz3 = new Vector2(num9, num9);
        s1 = "<b>‹</b>";
        s2 = "<b>›</b>";
        break;
      default:
        float num12 = 100f;
        pos3 = new Vector2((float) ((0.0 - (double) num12) * 0.5 - 4.0), num3);
        pos4 = new Vector2((float) ((double) num12 * 0.5 + 4.0), num3);
        sz3 = new Vector2(num12, num2);
        break;
    }
    Vector2 pos5 = this.AvoidOverlap(pos3, sz3, false);
    this.MarkRect(pos5, sz3);
    Vector2 pos6 = this.AvoidOverlap(pos4, sz3, false);
    this.MarkRect(pos6, sz3);
    RectTransform rectTransform1 = this.Card(this.cvRoot, pos5, sz3, c, 10);
    TMP_Text tmpText1 = this.Txt(rectTransform1, s1, 12f);
    ((Graphic) tmpText1).color = color;
    tmpText1.alignment = (TextAlignmentOptions) 514;
    tmpText1.enableAutoSizing = true;
    tmpText1.fontSizeMin = 9f;
    tmpText1.fontSizeMax = 14f;
    this.HitBox(rectTransform1, new NativeUntitled.Button("NextPage"));
    RectTransform rectTransform2 = this.Card(this.cvRoot, pos6, sz3, c, 10);
    TMP_Text tmpText2 = this.Txt(rectTransform2, s2, 12f);
    ((Graphic) tmpText2).color = color;
    tmpText2.alignment = (TextAlignmentOptions) 514;
    tmpText2.enableAutoSizing = true;
    tmpText2.fontSizeMin = 9f;
    tmpText2.fontSizeMax = 14f;
    this.HitBox(rectTransform2, new NativeUntitled.Button("PreviousPage"));
    if (flag)
    {
      RectTransform rectTransform3 = this.Card(this.cvRoot, pos1, sz1, c, 10);
      this.LogoIcon(rectTransform3, num5 - 10f);
      this.HitBox(rectTransform3, new NativeUntitled.Button("HomeButton"));
    }
    if (!inPcCondition)
      return;
    RectTransform rectTransform4 = this.Card(this.cvRoot, pos2, sz2, c, 10);
    ((Graphic) this.Txt(rectTransform4, "<b>R</b>", 14f)).color = color;
    this.HitBox(rectTransform4, new NativeUntitled.Button("Rotate"));
  }
  public void AddSearchRow(float y)
  {
    if (((Object) this.contentRoot == (Object) null))
      return;
    float num1 = (float) ((double) this.BgColor1.r / (double) byte.MaxValue + 0.054999999701976776);
    float num2 = (float) ((double) this.BgColor1.g / (double) byte.MaxValue + 0.054999999701976776);
    float num3 = (float) ((double) this.BgColor1.b / (double) byte.MaxValue + 0.054999999701976776);
    Color c;
    c = new Color(Mathf.Clamp01(num1), Mathf.Clamp01(num2), Mathf.Clamp01(num3), 1f);
    Color color;
    color = new Color((float) this.TextColor.r / (float) byte.MaxValue, (float) this.TextColor.g / (float) byte.MaxValue, (float) this.TextColor.b / (float) byte.MaxValue, 1f);
    RectTransform rectTransform = this.Card(this.contentRoot, new Vector2(0.0f, y), new Vector2(this.CANVAS_W - 16f, 60f), c, 10);
    TMP_Text tmpText = this.Txt(rectTransform, !this.insearch || string.IsNullOrEmpty(this.Search.currenttext) ? "<color=#7a7a7a><i>search…</i></color>" : this.Search.currenttext, 16f);
    tmpText.alignment = (TextAlignmentOptions) 513;
    ((Graphic) tmpText).color = color;
    tmpText.margin = new Vector4(16f, 0.0f, 16f, 0.0f);
    this.HitBox(rectTransform, this.searchButton);
    this.FxFadeIn(rectTransform);
    this.FxNextRow();
  }
  public string BuildCrumbTitle()
  {
    string str1 = this.menuDelayed ? "Untitled FREE" : "Untitled PAID";
    if (this.CurrentCategory == NativeUntitled.Category.Base)
      return str1;
    string str2 = this.CurrentCategory.ToString();
    return $"{str1} <color=#7a7a7a>·</color>  <size=15>{str2}</size>";
  }
  public void DrawFrame()
  {
    if (this.CurrentCategory == NativeUntitled.Category.Enabled)
      this.StartEnabled();
    try
    {
      this.hitCubes.Clear();
      this.FxReset();
      this.MenuObj = new GameObject("menuobj");
      this.MenuObj.transform.localScale = (Vector3.one * this.Settings.MenuScale);
      this.CanvasObject = new GameObject("CanvasObject");
      this.CanvasObject.transform.SetParent(this.MenuObj.transform, false);
      this.CanvasObject.transform.localPosition = new Vector3(0.07f, 0.01f, -0.04f);
      this.CanvasObject.transform.localRotation = Quaternion.Euler(180f, 90f, 90f);
      this.CanvasObject.transform.localScale = (Vector3.one * 0.0008f);
      this.CanvasObject.AddComponent<Canvas>().renderMode = (RenderMode) 2;
      this.CanvasObject.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 2500f;
      this.CanvasObject.AddComponent<GraphicRaycaster>();
      this.cvRoot = this.CanvasObject.GetComponent<RectTransform>();
      this.cvRoot.sizeDelta = new Vector2(this.CANVAS_W, 560f);
      float num1 = 0.04f;
      Color c1;
      c1 = new Color(Mathf.Max(num1, (float) this.BgColor1.r / (float) byte.MaxValue), Mathf.Max(num1, (float) this.BgColor1.g / (float) byte.MaxValue), Mathf.Max(num1, (float) this.BgColor1.b / (float) byte.MaxValue), 1f);
      Color c2;
      c2 = new Color(c1.r + 0.14f, c1.g + 0.14f, c1.b + 0.14f, 0.85f);
      Color color1;
      color1 = new Color((float) this.TextColor.r / (float) byte.MaxValue, (float) this.TextColor.g / (float) byte.MaxValue, (float) this.TextColor.b / (float) byte.MaxValue, 1f);
      Color color2;
      color2 = new Color(color1.r * 0.55f, color1.g * 0.55f, color1.b * 0.55f, 1f);
      this.Card(this.cvRoot, Vector2.zero, new Vector2(this.CANVAS_W, 560f), c1, 16 );
      if (((Object) this.customBackgroundImage != (Object) null))
      {
        GameObject gameObject = new GameObject("bgimg");
        gameObject.transform.SetParent((Transform) this.cvRoot, false);
        RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = new Vector2(8f, 8f);
        rectTransform.offsetMax = new Vector2(-8f, -8f);
        Image image = gameObject.AddComponent<Image>();
        image.sprite = Sprite.Create(this.customBackgroundImage, new Rect(0.0f, 0.0f, (float) ((Texture) this.customBackgroundImage).width, (float) ((Texture) this.customBackgroundImage).height), this.piv);
        ((Graphic) image).color = new Color(1f, 1f, 1f, 0.9f);
      }
      float num2 = 256f;
      this.titleTMP = this.Txt(this.Img(this.cvRoot, new Vector2(0.0f, num2), new Vector2(this.CANVAS_W, 48f), new Color(0.0f, 0.0f, 0.0f, 0.0f)), $"<b>{this.BuildCrumbTitle()}</b>", 20f);
      ((Graphic) this.titleTMP).color = color1;
      this.titleTMP.alignment = (TextAlignmentOptions) 514;
      this.titleTMP.fontStyle = (FontStyles) 1;
      if (this.gradienttitle)
      {
        this.titleTMP.enableVertexGradient = true;
        this.titleTMP.colorGradient = new VertexGradient(color1, color1, color2, color2);
      }
      this.Img(this.cvRoot, new Vector2(0.0f, num2 - 24f), new Vector2(this.CANVAS_W - 36f, 1f), c2);
      float num3 = -249f;
      this.Img(this.cvRoot, new Vector2(0.0f, num3 + 31f), new Vector2(this.CANVAS_W - 36f, 1f), c2);
      this.pageTMP = this.Txt(this.Img(this.cvRoot, new Vector2(0.0f, (float) ((double) num2 - 24.0 - 10.0)), new Vector2(this.CANVAS_W - 40f, 14f), new Color(0.0f, 0.0f, 0.0f, 0.0f)), "", 11f);
      this.pageTMP.alignment = (TextAlignmentOptions) 516;
      ((Graphic) this.pageTMP).color = color2;
      this.pageTMP.margin = new Vector4(0.0f, 0.0f, 8f, 0.0f);
      if (this.Settings.Counter)
      {
        this.fpsTMP = this.Txt(this.Img(this.cvRoot, new Vector2(0.0f, (float) ((double) num2 - 24.0 - 10.0)), new Vector2(this.CANVAS_W - 40f, 14f), new Color(0.0f, 0.0f, 0.0f, 0.0f)), this.fpsText, 11f);
        this.fpsTMP.alignment = (TextAlignmentOptions) 513;
        ((Graphic) this.fpsTMP).color = color2;
        this.fpsTMP.margin = new Vector4(8f, 0.0f, 0.0f, 0.0f);
      }
      else
        this.fpsTMP = (TMP_Text) null;
      this.fps = (Text) null;
      float num4 = 428f;
      GameObject gameObject1 = new GameObject("c");
      gameObject1.transform.SetParent((Transform) this.cvRoot, false);
      this.contentRoot = gameObject1.AddComponent<RectTransform>();
      this.contentRoot.anchorMin = this.contentRoot.anchorMax = this.contentRoot.pivot = this.piv;
      this.contentRoot.anchoredPosition = new Vector2(0.0f, -10f);
      this.contentRoot.sizeDelta = new Vector2(this.CANVAS_W, num4);
      if (this.inSafetyWarning)
      {
        if (((Object) this.titleTMP != (Object) null))
          this.titleTMP.text = "<b><color=#ff5050>WARNING</color></b>";
        if (((Object) this.fpsTMP != (Object) null))
          this.fpsTMP.text = "";
        else
          this.WarningThing();
      }
      else
      {
        List<NativeUntitled.Button> source = this.insearch ? this.GetSearchedButtons() : this.GetButtonInfoByPage(this.CurrentCategory);
        bool flag = this.ShowSearch || this.insearch;
        int count = Mathf.Max(1, (int) (((double) num4 - 8.0) / 64.0));
        if (flag)
          count = Mathf.Max(1, count - 1);
        this.ButtonsPerPage = count;
        int num5 = Mathf.Max(1, (source.Count + count - 1) / count);
        if (this.CurrentPage >= num5)
          this.CurrentPage = num5 - 1;
        if (this.CurrentPage < 0)
          this.CurrentPage = 0;
        List<NativeUntitled.Button> list = source.Skip<NativeUntitled.Button>(this.CurrentPage * count).Take<NativeUntitled.Button>(count).ToList<NativeUntitled.Button>();
        this.pageTMP.text = num5 <= 1 ? "" : $"page {this.CurrentPage + 1}/{num5}";
        float y = (float) ((double) this.contentRoot.sizeDelta.y * 0.5 - 31.0 - 2.0);
        int num6 = 0;
        if (flag)
        {
          this.AddSearchRow(y);
          num6 = 1;
        }
        for (int index = 0; index < list.Count; ++index)
        {
          float yPos = y - (float) (index + num6) * 64f;
          this.CreateButton(list[index], yPos);
        }
        if (this.Settings.PageButtons || this.PCMenuOpen || this.isPCUser)
          this.AddPageButtons();
        if (this.Settings.DisconnectButton)
          this.AddDisconnectButton();
        if (this.Settings.Tooltips && this.recentButton != null && !string.IsNullOrEmpty(this.recentButton.ToolTip) && this.recentButton.Enabled)
        {
          GameObject gameObject2 = new GameObject("toolobj");
          gameObject2.transform.SetParent(this.CanvasObject.transform, false);
          RectTransform rectTransform = gameObject2.AddComponent<RectTransform>();
          Vector2 piv;
          Vector2 vector2_1 = piv = this.piv;
          rectTransform.pivot = piv;
          Vector2 vector2_2;
          Vector2 vector2_3 = vector2_2 = vector2_1;
          rectTransform.anchorMax = vector2_2;
          rectTransform.anchorMin = vector2_3;
          rectTransform.anchoredPosition = new Vector2(0.0f, num3 - 18f);
          rectTransform.sizeDelta = new Vector2(this.CANVAS_W - 24f, 14f);
          TextMeshProUGUI textMeshProUgui = gameObject2.AddComponent<TextMeshProUGUI>();
          ((TMP_Text) textMeshProUgui).text = $"<i>{this.recentButton.ToolTip}</i>";
          ((TMP_Text) textMeshProUgui).fontSize = 11f;
          ((Graphic) textMeshProUgui).color = new Color(color1.r, color1.g, color1.b, 0.7f);
          ((TMP_Text) textMeshProUgui).alignment = (TextAlignmentOptions) 514;
        }
        if (this.selectedPlayerCate == null)
          return;
        this.BuildTargetView();
      }
    }
    catch (Exception ex)
    {
      throw new InvalidOperationException("Native Untitled UI failed", ex);
    }
  }
  public enum Category
  {
    Base,
    Settings,
    Players,
    Vrrig,
    Movement,
    Visuals,
    OP,
    Master,
    Global,
    Mode,
    Beta,
    Custom,
    Credits,
    MenuSettings,
    VRRigSettings,
    MovementSettings,
    RoomStats,
    Fun,
    GunSettings,
    SafetySettings,
    Enabled,
    MenuEditor,
    Soundboard,
    ButtonSettings,
    Horror,
    Helpful,
    Projectiles,
    VisualSettings,
    Favorites,
    Infection,
    SuperInfection,
    Paintbrawl,
    Guardian,
    GamemodeChangers,
    Utilities,
    Detected,
    Catalog,
    KeySaver,
  }
  public sealed class Button
  {
    public string Name;
    public string ToolTip = "";
    public string buttonType = "";
    public string PCkeybind = "null";
    public string VRkeybind = "null";
    public bool IsToggle;
    public bool Enabled;
    public bool delay;
    public bool isNew;
    public Action down;
    public Action up;
    public object arrowactualtext;
    public Button(string name, bool IsToggle = false, Action action = null)
    {
      this.Name = name;
      this.IsToggle = IsToggle;
    }
  }
  private sealed class SettingsData
  {
    public float MenuScale = 1f;
    public bool Rounding;
    public bool Outline;
    public bool Counter;
    public bool HomeButtons = true;
    public bool PageButtons = true;
    public bool DisconnectButton = true;
    public bool Tooltips;
    public int PageSIndex = 1;
  }
  private sealed class SearchData
  {
    public string currenttext;
  }
}
