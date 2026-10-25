
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Taurus;
internal sealed class NativeRexon
{
  public const string PrefabPath = "assets/morphineroot-vr.prefab";
  public const float OriginalScale = 0.0004f;
  private static int fpsFrame = -1;
  private static float smoothedFps;
  private static float nextFpsUpdate;
  private static string displayedFps = "FPS: 0";
  private static string ReadFps(float time)
  {
    if (NativeRexon.fpsFrame != Time.frameCount)
    {
      float num = 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
      NativeRexon.smoothedFps = NativeRexon.fpsFrame < 0 ? num : (float) ((double) NativeRexon.smoothedFps * 0.949999988079071 + (double) num * 0.05000000074505806);
      NativeRexon.fpsFrame = Time.frameCount;
    }
    if ((double) time >= (double) NativeRexon.nextFpsUpdate)
    {
      NativeRexon.displayedFps = "FPS: " + Mathf.RoundToInt(NativeRexon.smoothedFps).ToString();
      NativeRexon.nextFpsUpdate = time + 0.1f;
    }
    return NativeRexon.displayedFps;
  }
  public NativeUiSnapshot Build(MenuState state, Transform proxies)
  {
    NativeUiSnapshot snapshot = new NativeUiSnapshot()
    {
      Forward = Vector3.forward,
      Up = Vector3.up
    };
    bool flag1 = ((Object) proxies.parent != (Object) null) && ((Object) proxies.parent).name == "ItzGameDesktop";
    GameObject gameObject = Object.Instantiate<GameObject>(ImportedAssetMenu.Load(flag1 ? "rexon-pc" : "rexon").LoadAsset<GameObject>(flag1 ? "assets/morphineroot-pc.prefab" : "assets/morphineroot-vr.prefab"));
    ((Object) gameObject).name = "OriginalRexon";
    gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
    gameObject.transform.localScale = (Vector3.one * 0.0004f);
    snapshot.Source = gameObject;
    gameObject.GetComponent<Canvas>().renderMode = (RenderMode) 2;
    Transform background = gameObject.transform.Find("Background");
    string[] strArray = new string[2]
    {
      "Background",
      "Border"
    };
    foreach (string str in strArray)
    {
      RectTransform rectTransform = (RectTransform) gameObject.transform.Find(str);
      rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 0.0f);
    }
    ((Graphic) ((Component) background).GetComponent<Image>()).color = new Color(0.196f, 0.051f, 0.357f, 1f);
    snapshot.NotificationFrame = background;
    foreach (Button componentsInChild in gameObject.GetComponentsInChildren<Button>(true))
    {
      ((UnityEventBase) componentsInChild.onClick).RemoveAllListeners();
      ((Behaviour) componentsInChild).enabled = false;
    }
    Shader shader = ((TMP_Asset) ((Component) proxies.Find("XyferFont")).GetComponent<TMP_Text>().font).material.shader;
    foreach (TMP_Text componentsInChild in gameObject.GetComponentsInChildren<TMP_Text>(true))
    {
      if (((Object) componentsInChild.font == (Object) null))
        componentsInChild.font = ((Component) proxies.Find("XyferFont")).GetComponent<TMP_Text>().font;
      Material material = new Material(componentsInChild.fontSharedMaterial)
      {
        shader = shader
      };
      snapshot.Materials.Add(material);
      componentsInChild.fontSharedMaterial = material;
      ((Graphic) componentsInChild).raycastTarget = false;
      if (((Object) componentsInChild).name == "Tooltip")
        ((Component) componentsInChild).gameObject.SetActive(false);
    }
    snapshot.Header = ((Component) background.Find("TitlesBack/Title")).GetComponent<TMP_Text>();
    snapshot.Header.text = "Rexon";
    snapshot.Footer = ((Component) background.Find("TitlesBack/BottomTitle")).GetComponent<TMP_Text>();
    snapshot.Footer.text = $"{MenuCatalog.Categories[state.Category]} {(state.Page + 1).ToString()}/{state.PageCount.ToString()}";
    TMP_Text fps = ((Component) background.Find("FatAssHamster/FPSTitle")).GetComponent<TMP_Text>();
    fps.text = Application.isPlaying ? NativeRexon.ReadFps(Time.unscaledTime) : "FPS";
    ((Graphic) ((Component) background.Find("FatAssHamster")).GetComponent<Image>()).color = Color.white;
    snapshot.Update = (Func<float, bool>) (time =>
    {
      if (!Application.isPlaying)
        return false;
      string str = NativeRexon.ReadFps(time);
      if (fps.text == str)
        return false;
      fps.text = str;
      return true;
    });
    RectTransform component1 = new GameObject("NativeContent", new Type[1]
    {
      typeof (RectTransform)
    }).GetComponent<RectTransform>();
    ((Transform) component1).SetParent(background, false);
    RectTransform rectTransform1 = component1;
    Rect rect1 = ((RectTransform) background).rect;
    Vector2 size = (rect1).size;
    rectTransform1.sizeDelta = size;
    snapshot.Content = (Transform) component1;
    Bounds bounds1 = new Bounds();
    for (int row = 0; row < 6; ++row)
    {
      Transform control1 = background.Find("Button" + (row + 1).ToString());
      TMP_Text component2 = ((Component) control1.Find("ButtonText")).GetComponent<TMP_Text>();
      bool activeSelf = ((Component) proxies.Find("IncrementPlus" + row.ToString())).gameObject.activeSelf;
      component2.text = ((Component) proxies.Find($"Row{row.ToString()}/Label")).GetComponent<TextMesh>().text.Replace("\n", " ") + (activeSelf ? " : " + ((Component) proxies.Find($"Row{row.ToString()}/State")).GetComponent<TextMesh>().text : "");
      component2.enableAutoSizing = true;
      component2.fontSizeMin = 12f;
      component2.fontSizeMax = 36f;
      if (state.EntryAt(row) >= 0 && MenuCatalog.Buttons[state.EntryAt(row)].Target >= 0)
      {
        TMP_Text tmpText = component2;
        tmpText.fontStyle = tmpText.fontStyle | (FontStyles) 1;
        component2.fontWeight = (FontWeight) 700;
        if (component2.fontSharedMaterial.HasProperty("_FaceDilate"))
          component2.fontSharedMaterial.SetFloat("_FaceDilate", 0.12f);
      }
      component2.alignment = (TextAlignmentOptions) 514;
      component2.rectTransform.anchoredPosition = Vector2.zero;
      component2.textWrappingMode = (TextWrappingModes) 0;
      component2.overflowMode = (TextOverflowModes) 0;
      Bind(control1, "Row" + row.ToString());
      Canvas.ForceUpdateCanvases();
      Bounds bounds2 = ImportedSurface.BoundsOf(control1);
      if (row == 0)
        bounds1 = bounds2;
      else
        (bounds1).Encapsulate(bounds2);
      control1.SetParent((Transform) component1, true);
      ((Component) control1).gameObject.SetActive(state.EntryAt(row) >= 0 || state.SoundNameAt(row) != null);
      if (state.IsOn(state.EntryAt(row)))
      {
        Image component3 = ((Component) control1).GetComponent<Image>();
        ((Graphic) component3).color = (((Graphic) component3).color * 3f);
      }
      if (activeSelf)
      {
        RectTransform rectTransform2 = component2.rectTransform;
        Rect rect2 = ((RectTransform) control1).rect;
        Vector2 vector2_1 = new Vector2((rect2).width * 0.62f / component2.transform.localScale.x, component2.rectTransform.sizeDelta.y);
        rectTransform2.sizeDelta = vector2_1;
        bool[] flagArray = new bool[2]{ false, true };
        foreach (bool flag2 in flagArray)
        {
          Transform control2 = Object.Instantiate<Transform>(control1, (Transform) component1, true);
          ((Object) control2).name = flag2 ? "IncrementPlus" + row.ToString() : "IncrementMinus" + row.ToString();
          ((Component) control2).gameObject.SetActive(true);
          RectTransform rectTransform3 = (RectTransform) control2;
          RectTransform rectTransform4 = rectTransform3;
          rect2 = ((RectTransform) control1).rect;
          double num1 = (double) (rect2).width * 0.15999999642372131;
          rect2 = ((RectTransform) control1).rect;
          double num2 = (double) (rect2).height * 0.8399999737739563;
          Vector2 vector2_2 = new Vector2((float) num1, (float) num2);
          rectTransform4.sizeDelta = vector2_2;
          control2.position = (((bounds2).center + ((Vector3.right * (bounds2).size.x) * (flag2 ? 0.39f : -0.39f))) - (Vector3.forward * 1f / 1000f));
          TMP_Text component4 = ((Component) control2.Find("ButtonText")).GetComponent<TMP_Text>();
          component4.text = flag2 ? "+" : "-";
          TMP_Text tmpText = component4;
          tmpText.fontStyle = tmpText.fontStyle | (FontStyles) 1;
          component4.rectTransform.anchoredPosition = Vector2.zero;
          RectTransform rectTransform5 = component4.rectTransform;
          rect2 = rectTransform3.rect;
          double num3 = (double) (rect2).width / (double) component4.transform.localScale.x;
          rect2 = rectTransform3.rect;
          double num4 = (double) (rect2).height / (double) component4.transform.localScale.y;
          Vector2 vector2_3 = new Vector2((float) num3, (float) num4);
          rectTransform5.sizeDelta = vector2_3;
          Bind(control2, ((Object) control2).name);
        }
      }
    }
    snapshot.PickerRegion = new Bounds?(bounds1);
    Bind(background.Find("HomeBack"), "ButtonHome");
    Bind(background.Find("SearchBack"), "ButtonSearch");
    Bind(background.Find("LeaveBack"), "ButtonDisconnect");
    Bind(background.Find("Previous"), "ButtonPrevious");
    Bind(background.Find("Forward"), "ButtonNext");
    if (state.ColorPickerOpen)
    {
      Transform transform = background.Find("HomeBack");
      ((Component) transform.Find("HomeButton")).gameObject.SetActive(false);
      TMP_Text tmpText = Object.Instantiate<TMP_Text>(snapshot.Footer, transform, false);
      ((Object) tmpText).name = "PickerAdvance";
      tmpText.text = state.EditingEnd ? "Done" : "Next";
      ((Transform) tmpText.rectTransform).localPosition = new Vector3(0.0f, 0.0f, -1f);
      ((Transform) tmpText.rectTransform).localScale = Vector3.one;
      tmpText.rectTransform.sizeDelta = new Vector2(75f, 65f);
      tmpText.enableAutoSizing = true;
      tmpText.fontSizeMin = 12f;
      tmpText.fontSizeMax = 28f;
      tmpText.alignment = (TextAlignmentOptions) 514;
    }
    if (state.KeyboardOpen & flag1)
      NativeRexon.CreateSearchInputPanel(gameObject.transform, background, state.Query, ((Component) proxies.Find("XyferFont")).GetComponent<TMP_Text>().font);
    return snapshot;
    void Bind(Transform control, string name)
    {
      RectTransform rectTransform = (RectTransform) control;
      BoxCollider boxCollider1 = ((Component) control).GetComponent<BoxCollider>();
      if (((Object) boxCollider1 == (Object) null))
        boxCollider1 = ((Component) control).gameObject.AddComponent<BoxCollider>();
      BoxCollider boxCollider2 = boxCollider1;
      Rect rect1 = rectTransform.rect;
      double width = (double) (rect1).width;
      Rect rect2 = rectTransform.rect;
      double height = (double) (rect2).height;
      Vector3 vector3 = new Vector3((float) width, (float) height, 50f);
      boxCollider2.size = vector3;
      boxCollider1.center = Vector3.zero;
      ((Collider) boxCollider1).isTrigger = true;
      snapshot.Controls[name] = control;
    }
  }
  private static void CreateSearchInputPanel(
    Transform source,
    Transform background,
    string query,
    TMP_FontAsset fallback)
  {
    GameObject gameObject1 = new GameObject("SearchInputPanel", new Type[3]
    {
      typeof (RectTransform),
      typeof (Image),
      typeof (Outline)
    });
    gameObject1.transform.SetParent(source, false);
    RectTransform component1 = gameObject1.GetComponent<RectTransform>();
    Vector2 vector2;
    vector2 = new Vector2(0.5f, 0.5f);
    component1.anchorMax = vector2;
    component1.anchorMin = vector2;
    RectTransform rectTransform = (RectTransform) background;
    component1.anchoredPosition = new Vector2(0.0f, (float) ((double) rectTransform.anchoredPosition.y + (double) rectTransform.sizeDelta.y * (double) ((Transform) rectTransform).localScale.y / 2.0 + 30.0));
    component1.sizeDelta = new Vector2(670f, 50f);
    ((Graphic) gameObject1.GetComponent<Image>()).color = new Color(0.153f, 0.0f, 0.263f, 1f);
    ((Shadow) gameObject1.GetComponent<Outline>()).effectColor = new Color(0.755f, 0.533f, 1f, 1f);
    ((Shadow) gameObject1.GetComponent<Outline>()).effectDistance = new Vector2(2f, -2f);
    GameObject gameObject2 = new GameObject("Text Area", new Type[2]
    {
      typeof (RectTransform),
      typeof (RectMask2D)
    });
    gameObject2.transform.SetParent(gameObject1.transform, false);
    RectTransform component2 = gameObject2.GetComponent<RectTransform>();
    component2.anchorMin = Vector2.zero;
    component2.anchorMax = Vector2.one;
    component2.offsetMin = new Vector2(10f, 5f);
    component2.offsetMax = new Vector2(-10f, -5f);
    GameObject gameObject3 = new GameObject(query.Length == 0 ? "Placeholder" : "Text", new Type[2]
    {
      typeof (RectTransform),
      typeof (TextMeshProUGUI)
    });
    gameObject3.transform.SetParent(gameObject2.transform, false);
    TextMeshProUGUI component3 = gameObject3.GetComponent<TextMeshProUGUI>();
    if (((Object) ((TMP_Text) component3).font == (Object) null))
      ((TMP_Text) component3).font = fallback;
    ((TMP_Text) component3).text = query.Length == 0 ? "Search mods..." : query;
    ((TMP_Text) component3).fontSize = 24f;
    ((TMP_Text) component3).alignment = (TextAlignmentOptions) 4097;
    ((Graphic) component3).color = query.Length == 0 ? new Color(0.5f, 0.4f, 0.6f, 0.6f) : Color.white;
    ((TMP_Text) component3).rectTransform.anchorMin = Vector2.zero;
    ((TMP_Text) component3).rectTransform.anchorMax = Vector2.one;
    ((TMP_Text) component3).rectTransform.offsetMin = ((TMP_Text) component3).rectTransform.offsetMax = Vector2.zero;
  }
}
