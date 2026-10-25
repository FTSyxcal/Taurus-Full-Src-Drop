
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class ImportedAssetMenu : IDisposable
{
  private static readonly Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>();
  private readonly Transform root;
  private readonly int design;
  private readonly Dictionary<string, Transform> controls = new Dictionary<string, Transform>();
  private readonly Dictionary<Transform, Vector3> scales = new Dictionary<Transform, Vector3>();
  private readonly Dictionary<Transform, float> animated = new Dictionary<Transform, float>();
  private readonly List<Material> createdMaterials = new List<Material>();
  private readonly Transform[] normal;
  private readonly Transform[] incremental;
  private readonly GameObject[] hearts;
  private readonly TMP_Text[] labels;
  private readonly TMP_Text[] incrementLabels;
  private readonly UnityEngine.UI.Text[] sandboxLabels;
  private readonly UnityEngine.UI.Text[,] sandboxArrows;
  private readonly TMP_Text title;
  private readonly UnityEngine.UI.Text sandboxTitle;
  private readonly TMP_Text[] iiLabels;
  private readonly TMP_Text[,] iiArrows;
  private readonly TMP_Text iiTitle;
  private readonly TMP_Text iiPage;
  private readonly TMP_Text iiHomeLabel;
  private readonly Transform iiHomeIcon;
  private readonly Transform keyboard;
  private readonly TMP_Text keyboardQuery;
  private readonly UnityEngine.UI.Text sandboxQuery;
  private readonly ImportedTheme theme;
  private readonly ImportedPicker picker;
  private readonly PortNotification notification;
  private readonly TMP_Text emptyTmp;
  private readonly UnityEngine.UI.Text emptyText;
  private readonly UnityEngine.UI.Text advanceLabel;
  private MenuState state;
  private string signature;
  private bool active;
  public ImportedSurface Surface { get; }
  internal static AssetBundle Load(string name)
  {
    AssetBundle assetBundle;
    if (ImportedAssetMenu.bundles.TryGetValue(name, out assetBundle) && ((Object) assetBundle != (Object) null))
      return assetBundle;
    using (Stream manifestResourceStream = typeof (ImportedAssetMenu).Assembly.GetManifestResourceStream($"Taurus.{name}.bundle"))
    {
      using (MemoryStream destination = new MemoryStream())
      {
        if (manifestResourceStream == null)
          throw new InvalidDataException("Missing original design: " + name);
        manifestResourceStream.CopyTo((Stream) destination);
        assetBundle = AssetBundle.LoadFromMemory(destination.ToArray());
      }
    }
    ImportedAssetMenu.bundles[name] = !((Object) assetBundle == (Object) null) ? assetBundle : throw new InvalidDataException("Could not load original design: " + name);
    return assetBundle;
  }
  public ImportedAssetMenu(Transform root, int design)
  {
    this.root = root;
    this.design = design;
    AssetBundle assetBundle = design < 4 ? ImportedAssetMenu.Load(design == 2 ? "hamburbur" : "monosandbox") : (AssetBundle) null;
    GameObject source = ((Object) assetBundle == (Object) null) ? new GameObject(MenuDesign.Names[design]) : Object.Instantiate<GameObject>(assetBundle.LoadAsset<GameObject>(design == 2 ? "assets/models/hamburburv2.prefab" : "assets/assets/monosandbox/list.prefab"));
    ((Object) source).name = "Original" + MenuDesign.Names[design];
    source.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
    int num1;
    switch (design)
    {
      case 2:
        num1 = 6;
        break;
      case 3:
        num1 = 12;
        break;
      case 4:
        num1 = 9;
        break;
      default:
        num1 = 8;
        break;
    }
    int length = num1;
    this.normal = new Transform[length];
    this.incremental = new Transform[length];
    this.hearts = new GameObject[length];
    this.labels = new TMP_Text[length];
    this.incrementLabels = new TMP_Text[length];
    this.sandboxLabels = new UnityEngine.UI.Text[length];
    this.sandboxArrows = new UnityEngine.UI.Text[length, 2];
    switch (design)
    {
      case 2:
        this.title = ((Component) source.transform.Find("Title")).GetComponent<TMP_Text>();
        ((Component) source.transform.Find("Version")).gameObject.SetActive(false);
        this.controls["ButtonDisconnect"] = source.transform.Find("MiscButtons/Disconnect");
        this.controls["ButtonPrevious"] = source.transform.Find("MiscButtons/LastPage");
        this.controls["ButtonNext"] = source.transform.Find("MiscButtons/NextPage");
        this.controls["ButtonHome"] = source.transform.Find("MiscButtons/Return");
        Transform transform1 = this.controls["ButtonHome"].Find("ReturnImageCanvas");
        this.advanceLabel = new GameObject("PickerAdvance", new Type[3]
        {
          typeof (RectTransform),
          typeof (CanvasRenderer),
          typeof (UnityEngine.UI.Text)
        }).GetComponent<UnityEngine.UI.Text>();
        ((Component) this.advanceLabel).transform.SetParent(transform1, false);
        ((Graphic) this.advanceLabel).rectTransform.sizeDelta = new Vector2(100f, 100f);
        this.advanceLabel.font = Font.CreateDynamicFontFromOSFont("Arial", 32 );
        this.advanceLabel.fontSize = 32 ;
        this.advanceLabel.resizeTextForBestFit = true;
        this.advanceLabel.resizeTextMinSize = 12;
        this.advanceLabel.resizeTextMaxSize = 32 ;
        this.advanceLabel.alignment = (TextAnchor) 4;
        ((Graphic) this.advanceLabel).color = Color.white;
        ((Component) this.advanceLabel).gameObject.SetActive(false);
        Transform transform2 = Object.Instantiate<Transform>(this.controls["ButtonDisconnect"], this.controls["ButtonDisconnect"].parent, false);
        ((Object) transform2).name = "Search";
        transform2.localPosition = new Vector3(0.0f, 0.0f, -0.016f);
        ((Component) transform2).GetComponentInChildren<TMP_Text>().text = "Search";
        this.controls["ButtonSearch"] = transform2;
        for (int index = 0; index < length; ++index)
        {
          Transform transform3 = source.transform.Find("ModButtons/Button" + (index + 1).ToString());
          this.normal[index] = transform3.Find("NormalButton");
          this.incremental[index] = transform3.Find("IncrementalButton");
          this.labels[index] = ((Component) this.normal[index].Find("TMP")).GetComponent<TMP_Text>();
          this.incrementLabels[index] = ((Component) this.incremental[index].Find("TMP")).GetComponent<TMP_Text>();
          this.controls["Row" + index.ToString()] = this.normal[index];
          this.controls["IncrementMinus" + index.ToString()] = this.incremental[index].Find("MinusButton");
          this.controls["IncrementPlus" + index.ToString()] = this.incremental[index].Find("PlusButton");
          this.hearts[index] = Object.Instantiate<GameObject>(((Component) root.Find("Row0/FavoriteIcon")).gameObject, this.normal[index], false);
          this.hearts[index].transform.localPosition = new Vector3(-0.00015f, -0.0095f, 0.0f);
          this.hearts[index].transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 180f);
          this.hearts[index].transform.localScale = new Vector3(0.142857149f, 0.0714285746f, 0.0714285746f);
        }
        break;
      case 3:
        this.sandboxTitle = ((Component) source.transform.Find("Decor/Canvas/Header")).GetComponent<UnityEngine.UI.Text>();
        ((Component) this.sandboxTitle).transform.localScale = (Vector3.one * 0.0012f);
        ((Component) this.sandboxTitle).transform.position = new Vector3(-0.06f, 0.335f, -0.06f);
        ((Graphic) this.sandboxTitle).rectTransform.sizeDelta = new Vector2(600f, 32f);
        this.sandboxTitle.fontSize = 24;
        UnityEngine.UI.Text component1 = ((Component) source.transform.Find("Decor/Canvas/Version")).GetComponent<UnityEngine.UI.Text>();
        ((Component) component1).transform.localScale = (Vector3.one * 1f / 1000f);
        ((Component) component1).transform.position = new Vector3(-0.06f, 0.298f, -0.06f);
        ((Graphic) component1).rectTransform.sizeDelta = new Vector2(650f, 22f);
        component1.fontSize = 16 ;
        component1.alignment = (TextAnchor) 4;
        component1.resizeTextForBestFit = false;
        Transform transform4 = source.transform.Find("Main");
        Transform canvas = transform4.Find("Canvas");
        Font font1 = this.sandboxTitle.font;
        for (int index1 = 0; index1 < length; ++index1)
        {
          GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
          ((Object) primitive).name = "Button" + index1.ToString();
          primitive.transform.SetParent(transform4, false);
          primitive.transform.localScale = new Vector3(0.025f, 0.145f, 0.145f);
          primitive.transform.localPosition = new Vector3(0.02f, (float) -(index1 / 4) * 0.175f, (float) (3 - index1 % 4) * 0.165f);
          this.normal[index1] = primitive.transform;
          this.controls["Row" + index1.ToString()] = primitive.transform;
          this.sandboxLabels[index1] = ImportedAssetMenu.SandboxText(canvas, font1, "ButtonText" + index1.ToString(), (primitive.transform.position + (Vector3.left * 0.015f)), "");
          XyferHeart component2 = new GameObject("Favorite" + index1.ToString(), new Type[3]
          {
            typeof (RectTransform),
            typeof (CanvasRenderer),
            typeof (XyferHeart)
          }).GetComponent<XyferHeart>();
          ((Component) component2).transform.SetParent(canvas, false);
          ((Component) component2).transform.localScale = (Vector3.one * 1f / 1000f);
          component2.rectTransform.sizeDelta = new Vector2(20f, 20f);
          ((Component) component2).transform.position = (primitive.transform.position + new Vector3(-0.019f, 0.05f, -0.05f));
          component2.color = ((Color32)new Color32((byte) 177, (byte) 60, (byte) 83, byte.MaxValue));
          this.hearts[index1] = ((Component) component2).gameObject;
          for (int index2 = 0; index2 < 2; ++index2)
          {
            Transform transform5 = Object.Instantiate<Transform>(primitive.transform, transform4, false);
            ((Object) transform5).name = index2 == 0 ? "Minus" : "Plus";
            transform5.localScale = new Vector3(0.025f, 0.045f, 0.045f);
            transform5.localPosition = (primitive.transform.localPosition + new Vector3(0.0f, -0.045f, index2 == 0 ? 0.045f : -0.045f));
            this.sandboxArrows[index1, index2] = ImportedAssetMenu.SandboxText(canvas, font1, ((Object) transform5).name + index1.ToString(), (transform5.position + (Vector3.left * 0.015f)), index2 == 0 ? "-" : "+");
            transform5.SetParent(canvas, true);
            ((Component) this.sandboxArrows[index1, index2]).transform.SetParent(transform5, true);
            ((Component) this.sandboxArrows[index1, index2]).gameObject.SetActive(false);
            this.controls[(index2 == 0 ? "IncrementMinus" : "IncrementPlus") + index1.ToString()] = transform5;
          }
        }
        string[] strArray1 = new string[5]
        {
          "ButtonHome",
          "ButtonPrevious",
          "ButtonNext",
          "ButtonSearch",
          "DesignSettings"
        };
        string[] strArray2 = new string[5]
        {
          "HOME",
          "<",
          ">",
          "SEARCH",
          "SETTINGS"
        };
        for (int index = 0; index < strArray1.Length; ++index)
        {
          Transform transform6 = GameObject.CreatePrimitive((PrimitiveType) 3).transform;
          ((Object) transform6).name = strArray1[index];
          transform6.SetParent(transform4, false);
          bool flag = index == 1 || index == 2;
          transform6.localScale = new Vector3(0.02f, flag ? 0.055f : 0.07f, flag ? 0.105f : 0.225f);
          transform6.localPosition = flag ? new Vector3(0.0f, -0.48f, index == 1 ? 0.495f : 0.0f) : new Vector3(0.0f, (float) (0.085000000894069672 - (index == 0 ? 0.0 : (double) (index - 2)) * 0.10999999940395355), 0.745f);
          UnityEngine.UI.Text text = ImportedAssetMenu.SandboxText(canvas, font1, strArray1[index], (transform6.position + (Vector3.left * 0.015f)), strArray2[index]);
          transform6.SetParent(canvas, true);
          ((Component) text).transform.SetParent(transform6, true);
          if (flag)
          {
            text.fontSize = 40;
            text.resizeTextMaxSize = 40;
          }
          this.controls[strArray1[index]] = transform6;
        }
        Transform transform7 = GameObject.CreatePrimitive((PrimitiveType) 3).transform;
        ((Object) transform7).name = "Disconnect";
        transform7.SetParent(source.transform, false);
        transform7.localScale = new Vector3(0.02f, 0.07f, 0.65f);
        transform7.localPosition = new Vector3(-0.05f, 0.44f, 0.0f);
        transform7.SetParent(canvas, true);
        ((Component) ImportedAssetMenu.SandboxText(canvas, font1, "Disconnect", (transform7.position + (Vector3.left * 0.015f)), "DISCONNECT")).transform.SetParent(transform7, true);
        this.controls["ButtonDisconnect"] = transform7;
        break;
      case 4:
        Transform transform8 = new GameObject("Decor").transform;
        transform8.SetParent(source.transform, false);
        Transform transform9 = new GameObject("Canvas", new Type[2]
        {
          typeof (RectTransform),
          typeof (Canvas)
        }).transform;
        transform9.SetParent(transform8, false);
        ((Component) transform9).GetComponent<Canvas>().renderMode = (RenderMode) 2;
        Font dynamicFontFromOsFont = Font.CreateDynamicFontFromOSFont("Arial", 32 );
        Color gray = Color.gray;
        this.ClassicCube(source.transform, "Back", new Vector3(0.05f, 0.0f, 0.0f), new Vector3(0.01f, 0.3f, 0.4f), MenuPalette.Unpack(526346U));
        this.sandboxTitle = ImportedAssetMenu.ClassicText(transform9, dynamicFontFromOsFont, "Header", new Vector3(0.064f, 0.0f, 0.175f), new Vector2(0.28f, 0.035f), MenuDesign.Names[design], 23);
        this.sandboxTitle.fontStyle = (FontStyle) 1;
        ImportedAssetMenu.ClassicText(transform9, dynamicFontFromOsFont, "Version", new Vector3(0.064f, 0.0f, -0.208f), new Vector2(0.28f, 0.018f), "", 13);
        for (int index3 = 0; index3 < length; ++index3)
        {
          this.normal[index3] = this.ClassicCube(source.transform, "Button" + index3.ToString(), new Vector3(0.056f, 0.0f, (float) (0.13600000739097595 - (double) index3 * 0.039999999105930328)), new Vector3(0.009f, 0.24f, 0.032f), gray);
          this.controls["Row" + index3.ToString()] = this.normal[index3];
          this.sandboxLabels[index3] = ImportedAssetMenu.ClassicText(transform9, dynamicFontFromOsFont, "ButtonText" + index3.ToString(), (this.normal[index3].position + (Vector3.right * 3f / 500f)), new Vector2(0.224f, 0.029f), "", 18);
          ((Component) this.sandboxLabels[index3]).transform.SetParent(this.normal[index3], true);
          XyferHeart component3 = new GameObject("Favorite" + index3.ToString(), new Type[3]
          {
            typeof (RectTransform),
            typeof (CanvasRenderer),
            typeof (XyferHeart)
          }).GetComponent<XyferHeart>();
          ((Component) component3).transform.SetParent(transform9, false);
          ((Component) component3).transform.localScale = (Vector3.one * 1f / 1000f);
          ((Component) component3).transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.forward);
          component3.rectTransform.sizeDelta = new Vector2(12f, 12f);
          ((Component) component3).transform.position = (this.normal[index3].position + new Vector3(0.007f, 0.104f, 0.0f));
          component3.color = ((Color32)new Color32(byte.MaxValue, (byte) 180, (byte) 213, byte.MaxValue));
          this.hearts[index3] = ((Component) component3).gameObject;
          for (int index4 = 0; index4 < 2; ++index4)
          {
            Transform transform10 = this.ClassicCube(source.transform, $"Increment{index3.ToString()}{index4.ToString()}", (this.normal[index3].position + new Vector3(1f / 500f, index4 == 0 ? -0.076f : -0.105f, 0.0f)), new Vector3(0.01f, 0.025f, 0.028f), MenuPalette.Unpack(3158070U));
            this.sandboxArrows[index3, index4] = ImportedAssetMenu.ClassicText(transform9, dynamicFontFromOsFont, "IncrementLabel", (transform10.position + (Vector3.right * 3f / 500f)), new Vector2(0.022f, 0.025f), index4 == 0 ? "-" : "+", 20);
            ((Component) this.sandboxArrows[index3, index4]).transform.SetParent(transform10, true);
            this.controls[(index4 == 0 ? "IncrementMinus" : "IncrementPlus") + index3.ToString()] = transform10;
          }
        }
        string[] strArray3 = new string[6]
        {
          "ButtonPrevious",
          "ButtonNext",
          "ButtonHome",
          "ButtonSearch",
          "ButtonDisconnect",
          "DesignSettings"
        };
        string[] strArray4 = new string[6]
        {
          "<",
          ">",
          "Home",
          "Search",
          "Disconnect",
          "Settings"
        };
        Vector3[] vector3Array1 = new Vector3[6]
        {
          new Vector3(0.056f, 0.1971f, 0.00252f),
          new Vector3(0.056f, -0.1971f, 0.00252f),
          new Vector3(0.056f, 0.082f, -0.235f),
          new Vector3(0.056f, 0.0f, -0.235f),
          new Vector3(0.056f, 0.0f, 0.245f),
          new Vector3(0.056f, -0.082f, -0.235f)
        };
        for (int index = 0; index < strArray3.Length; ++index)
        {
          Vector3 size = index < 2 ? new Vector3(0.0045f, 0.075f, 0.35745f) : new Vector3(3f / 500f, index == 4 ? 0.3f : 0.078f, 0.035f);
          Transform transform11 = this.ClassicCube(source.transform, strArray3[index], vector3Array1[index], size, index < 2 ? MenuPalette.Unpack(1513245U) : Color.gray);
          ((Component) ImportedAssetMenu.ClassicText(transform9, dynamicFontFromOsFont, strArray3[index], (transform11.position + (Vector3.right * 3f / 500f)), new Vector2(size.y * 0.94f, index < 2 ? 0.06f : 0.031f), strArray4[index], index < 2 ? 40 : 16 )).transform.SetParent(transform11, true);
          this.controls[strArray3[index]] = transform11;
        }
        break;
      default:
        Transform transform12 = new GameObject("Decor").transform;
        transform12.SetParent(source.transform, false);
        Transform transform13 = new GameObject("Canvas", new Type[2]
        {
          typeof (RectTransform),
          typeof (Canvas)
        }).transform;
        transform13.SetParent(transform12, false);
        ((Component) transform13).GetComponent<Canvas>().renderMode = (RenderMode) 2;
        TMP_FontAsset font2 = ImportedAssetMenu.Load("ii").LoadAsset<TMP_FontAsset>("Agency");
        if (((Object) font2 == (Object) null))
          throw new InvalidDataException("Original ii Agency SDF font missing.");
        Color color = MenuPalette.Unpack(11162880U);
        this.ClassicCube(source.transform, "Back", new Vector3(0.05f, 0.0f, 0.0f), new Vector3(0.01f, 0.3f, 0.3825f), new Color(1f, 0.5f, 0.0f, 0.5f));
        this.iiTitle = ImportedAssetMenu.IiText(transform13, font2, "Header", new Vector3(0.064f, 0.0f, 0.165f), new Vector2(0.28f, 0.05f), "ii's <b>Stupid</b> Menu <color=grey>[</color>1<color=grey>]</color>");
        this.iiPage = ImportedAssetMenu.IiText(transform13, font2, "Version", new Vector3(0.064f, 0.0f, 0.135f), new Vector2(0.28f, 0.02f), "Main");
        this.iiLabels = new TMP_Text[length];
        this.iiArrows = new TMP_Text[length, 2];
        for (int index5 = 0; index5 < length; ++index5)
        {
          this.normal[index5] = this.ClassicCube(source.transform, "Button" + index5.ToString(), new Vector3(0.056f, 0.0f, (float) (0.045899998396635056 - (double) index5 * 0.03060000017285347)), new Vector3(0.009f, 0.27f, 0.02448f), color);
          this.incremental[index5] = this.normal[index5];
          this.controls["Row" + index5.ToString()] = this.normal[index5];
          this.iiLabels[index5] = ImportedAssetMenu.IiText(transform13, font2, "ButtonText" + index5.ToString(), new Vector3(0.064f, 0.0f, (float) (0.049460001289844513 - (double) index5 * 0.030768999829888344)), new Vector2(0.2f, 0.024f), "");
          this.hearts[index5] = Object.Instantiate<GameObject>(((Component) root.Find("Row0/FavoriteIcon")).gameObject, this.normal[index5], false);
          this.hearts[index5].transform.localPosition = new Vector3(0.6f, -0.39f, 0.0f);
          this.hearts[index5].transform.localScale = (new Vector3(111.111115f, 3.70370364f, 40.8496742f) * 0.75f);
          for (int index6 = 0; index6 < 2; ++index6)
          {
            Transform transform14 = this.ClassicCube(source.transform, $"Increment{index5.ToString()}{index6.ToString()}", (this.normal[index5].position + new Vector3(1f / 500f, index6 == 0 ? -0.086f : -0.115f, 0.0f)), new Vector3(0.009f, 0.024f, 0.02448f), MenuPalette.Unpack(5581312U));
            this.iiArrows[index5, index6] = ImportedAssetMenu.IiText(transform13, font2, "IncrementLabel", (transform14.position + (Vector3.right * 3f / 500f)), new Vector2(0.022f, 0.022f), index6 == 0 ? "-" : "+");
            this.iiArrows[index5, index6].transform.SetParent(transform14, true);
            this.controls[(index6 == 0 ? "IncrementMinus" : "IncrementPlus") + index5.ToString()] = transform14;
          }
        }
        string[] strArray5 = new string[6]
        {
          "ButtonPrevious",
          "ButtonNext",
          "ButtonHome",
          "ButtonSearch",
          "ButtonDisconnect",
          "DesignSettings"
        };
        string[] strArray6 = new string[6]
        {
          "<",
          ">",
          "Home",
          "Search",
          "Disconnect",
          "Settings"
        };
        Vector3[] vector3Array2 = new Vector3[6]
        {
          new Vector3(0.056f, 0.0f, 0.1071f),
          new Vector3(0.056f, 0.0f, 0.0765f),
          new Vector3(0.056f, 0.135f, -0.22185f),
          new Vector3(0.056f, -0.135f, -0.22185f),
          new Vector3(0.056f, 0.0f, 0.22185f),
          new Vector3(0.056f, 0.0f, -0.22185f)
        };
        for (int kind = 0; kind < strArray5.Length; ++kind)
        {
          Vector3 size = kind < 2 || kind == 4 ? new Vector3(0.009f, 0.27f, 0.02448f) : new Vector3(0.009f, 0.0306f, 0.0306f);
          Transform control = this.ClassicCube(source.transform, strArray5[kind], vector3Array2[kind], size, color);
          this.controls[strArray5[kind]] = control;
          if (kind == 2 || kind == 3 || kind == 5)
          {
            Transform transform15 = this.IiIcon(control, kind);
            if (kind == 2)
            {
              this.iiHomeIcon = transform15;
              this.iiHomeLabel = ImportedAssetMenu.IiText(transform13, font2, "PickerAdvance", (vector3Array2[kind] + (Vector3.right * 0.008f)), new Vector2(0.029f, 0.024f), "Next");
              this.iiHomeLabel.transform.SetParent(control, true);
              ((Component) this.iiHomeLabel).gameObject.SetActive(false);
            }
          }
          else
            ImportedAssetMenu.IiText(transform13, font2, strArray5[kind], (vector3Array2[kind] + (Vector3.right * 0.008f)), new Vector2(0.2f, 0.024f), strArray6[kind]).transform.SetParent(control, true);
        }
        break;
    }
    Vector3 up = design == 3 ? Vector3.up : Vector3.forward;
    Vector3 forward = design >= 4 ? Vector3.left : Vector3.right;
    Vector3 right = design == 3 ? Vector3.back : (design >= 4 ? Vector3.down : Vector3.up);
    Bounds bounds = ImportedSurface.BoundsOf(this.normal[0]);
    foreach (Transform control in this.normal)
      (bounds).Encapsulate(ImportedSurface.BoundsOf(control));
    this.keyboard = new GameObject("DesignKeyboard").transform;
    this.keyboard.SetParent(source.transform, false);
    this.keyboard.SetPositionAndRotation((bounds).center, Quaternion.LookRotation(forward, up));
    Transform keyboard = this.keyboard;
    Vector3 vector3_1 = source.transform.InverseTransformVector(right);
    double magnitude1 = (double) (vector3_1).magnitude;
    Vector3 vector3_2 = source.transform.InverseTransformVector(up);
    double magnitude2 = (double) (vector3_2).magnitude;
    Vector3 vector3_3 = source.transform.InverseTransformVector(forward);
    double magnitude3 = (double) (vector3_3).magnitude;
    Vector3 vector3_4 = new Vector3((float) magnitude1, (float) magnitude2, (float) magnitude3);
    keyboard.localScale = vector3_4;
    this.keyboard.rotation = Quaternion.LookRotation(forward, up);
    float num2 = Vector3.Dot(new Vector3(Mathf.Abs(right.x), Mathf.Abs(right.y), Mathf.Abs(right.z)), (bounds).size);
    float num3 = Vector3.Dot(new Vector3(Mathf.Abs(up.x), Mathf.Abs(up.y), Mathf.Abs(up.z)), (bounds).size);
    Transform transform16 = new GameObject("Canvas", new Type[2]
    {
      typeof (RectTransform),
      typeof (Canvas)
    }).transform;
    transform16.SetParent(this.keyboard, false);
    transform16.localScale = (Vector3.one * 1f / 1000f);
    ((Component) transform16).GetComponent<Canvas>().renderMode = (RenderMode) 2;
    GameObject nativeKeyboard = design == 2 ? assetBundle.LoadAsset<GameObject>("assets/models/keyboard.prefab") : (GameObject) null;
    float num4 = num2 * 0.014f;
    float num5 = num2 / 10f;
    float num6 = num3 / 6f;
    Transform transform17 = this.KeyboardKey(nativeKeyboard, transform16, "Query", "", new Vector2(0.0f, (float) ((double) num3 / 2.0 - (double) num6 / 2.0)), new Vector2(num2, num6 * 0.82f));
    this.keyboardQuery = ((Component) transform17).GetComponentInChildren<TMP_Text>();
    this.sandboxQuery = ((Component) transform17).GetComponentInChildren<UnityEngine.UI.Text>();
    string[] strArray7 = new string[4]
    {
      "1234567890",
      "QWERTYUIOP",
      "ASDFGHJKL",
      "ZXCVBNM"
    };
    for (int index7 = 0; index7 < strArray7.Length; ++index7)
    {
      for (int index8 = 0; index8 < strArray7[index7].Length; ++index8)
      {
        string caption = strArray7[index7][index8].ToString();
        Transform transform18 = this.KeyboardKey(nativeKeyboard, transform16, "Key_" + caption, caption, new Vector2(((float) index8 - (float) (strArray7[index7].Length - 1) / 2f) * num5, (float) ((double) num3 / 2.0 - (double) num6 * ((double) index7 + 1.5))), new Vector2(num5 - num4, num6 * 0.82f));
        this.controls["Keyboard/Key_" + caption] = transform18;
      }
    }
    string[] strArray8 = new string[4]
    {
      "Clear",
      "Space",
      "Back",
      "Done"
    };
    for (int index = 0; index < strArray8.Length; ++index)
      this.controls["Keyboard/Key_" + strArray8[index]] = this.KeyboardKey(nativeKeyboard, transform16, "Key_" + strArray8[index], strArray8[index], new Vector2((float) (((double) index - 1.5) * (double) num2 / 4.0), (float) (-(double) num3 / 2.0 + (double) num6 / 2.0)), new Vector2(num2 / 4f - num4, num6 * 0.82f));
    ((Component) this.keyboard).gameObject.SetActive(false);
    this.notification = new PortNotification(root, source.transform, this.normal[0], ((Object) this.labels[0] != (Object) null) ? (Component) (object) this.labels[0] : (Component) (object) this.sandboxLabels[0], ImportedSurface.BoundsOf(source.transform), forward, up, true);
    this.Surface = new ImportedSurface(root, source, forward, up);
    foreach (Material createdMaterial in this.createdMaterials)
    {
      if (Application.isPlaying)
        Object.Destroy((Object) createdMaterial);
      else
        Object.DestroyImmediate((Object) createdMaterial);
    }
    this.createdMaterials.Clear();
    Bounds content = ImportedSurface.BoundsOf(this.normal[0]);
    foreach (Transform control in this.normal)
      (content).Encapsulate(ImportedSurface.BoundsOf(control));
    switch (design)
    {
      case 2:
        this.emptyTmp = Object.Instantiate<TMP_Text>(this.labels[0], this.labels[0].transform.parent.parent.parent, true);
        ((Object) this.emptyTmp).name = "EmptyMessage";
        this.emptyTmp.transform.position = ((content).center - (forward * 0.02f));
        this.emptyTmp.rectTransform.sizeDelta = new Vector2(600f, 100f);
        this.emptyTmp.alignment = (TextAlignmentOptions) 514;
        break;
      case 5:
        this.emptyTmp = Object.Instantiate<TMP_Text>(this.iiLabels[0], source.transform.Find("Decor/Canvas"), true);
        ((Object) this.emptyTmp).name = "EmptyMessage";
        this.emptyTmp.transform.position = ((content).center - (forward * 0.02f));
        this.emptyTmp.rectTransform.sizeDelta = new Vector2(0.24f, 0.08f);
        break;
      default:
        this.emptyText = Object.Instantiate<UnityEngine.UI.Text>(this.sandboxLabels[0], source.transform.Find("Decor/Canvas"), true);
        ((Object) this.emptyText).name = "EmptyMessage";
        ((Component) this.emptyText).transform.position = ((content).center - (forward * 0.02f));
        ((Graphic) this.emptyText).rectTransform.sizeDelta = new Vector2(design == 3 ? 500f : 240f, 80f);
        this.emptyText.alignment = (TextAnchor) 4;
        break;
    }
    this.theme = new ImportedTheme(source.transform, right, up);
    this.picker = new ImportedPicker(root, source.transform, content, forward, right, up, new Action<Transform, Transform>(this.Surface.Map));
    foreach (KeyValuePair<string, Transform> control in this.controls)
      this.scales[control.Value] = control.Value.localScale;
  }
  private Transform KeyboardKey(
    GameObject nativeKeyboard,
    Transform canvas,
    string name,
    string caption,
    Vector2 position,
    Vector2 size)
  {
    Transform transform1 = new GameObject(name, new Type[3]
    {
      typeof (MeshFilter),
      typeof (MeshRenderer),
      typeof (BoxCollider)
    }).transform;
    transform1.SetParent(this.keyboard, false);
    transform1.localPosition = new Vector3(position.x, position.y, -0.028f);
    Mesh sharedMesh;
    Material[] sharedMaterials;
    if (((Object) nativeKeyboard != (Object) null))
    {
      Transform transform2 = nativeKeyboard.transform.Find("KeyboardBase/TopRow/1");
      sharedMesh = ((Component) transform2).GetComponent<MeshFilter>().sharedMesh;
      sharedMaterials = ((Component) transform2).GetComponent<Renderer>().sharedMaterials;
    }
    else
    {
      sharedMesh = ((Component) this.normal[0]).GetComponent<MeshFilter>().sharedMesh;
      sharedMaterials = ((Component) this.normal[0]).GetComponent<Renderer>().sharedMaterials;
    }
    ((Component) transform1).GetComponent<MeshFilter>().sharedMesh = sharedMesh;
    ((Component) transform1).GetComponent<Renderer>().sharedMaterials = sharedMaterials;
    Bounds bounds1 = sharedMesh.bounds;
    Vector3 size1 = (bounds1).size;
    transform1.localScale = new Vector3(size.x / size1.x, size.y / size1.y, 0.008f / size1.z);
    BoxCollider component1 = ((Component) transform1).GetComponent<BoxCollider>();
    Bounds bounds2 = sharedMesh.bounds;
    Vector3 center = (bounds2).center;
    component1.center = center;
    ((Component) transform1).GetComponent<BoxCollider>().size = size1;
    if (this.design == 2 || this.design == 5)
    {
      TMP_Text tmpText = Object.Instantiate<TMP_Text>(this.design == 5 ? this.iiLabels[0] : this.labels[0], canvas, false);
      ((Object) tmpText).name = name + "Label";
      tmpText.transform.localRotation = Quaternion.identity;
      tmpText.transform.localScale = Vector3.one;
      tmpText.rectTransform.sizeDelta = ((size * 1000f) * 0.85f);
      ((Transform) tmpText.rectTransform).localPosition = new Vector3(position.x * 1000f, position.y * 1000f, -34f);
      tmpText.alignment = (TextAlignmentOptions) 514;
      tmpText.margin = Vector4.zero;
      tmpText.enableAutoSizing = true;
      tmpText.fontSizeMin = this.design == 5 ? 10f : 100f;
      tmpText.fontSizeMax = this.design == 5 ? 2200f : (caption.Length > 1 ? 280f : 340f);
      tmpText.text = caption;
      tmpText.transform.SetParent(transform1, true);
    }
    else
    {
      UnityEngine.UI.Text component2 = new GameObject(name + "Label", new Type[3]
      {
        typeof (RectTransform),
        typeof (CanvasRenderer),
        typeof (UnityEngine.UI.Text)
      }).GetComponent<UnityEngine.UI.Text>();
      ((Component) component2).transform.SetParent(canvas, false);
      ((Transform) ((Graphic) component2).rectTransform).localPosition = new Vector3(position.x * 1000f, position.y * 1000f, -34f);
      ((Graphic) component2).rectTransform.sizeDelta = ((size * 1000f) * 0.9f);
      component2.font = this.sandboxTitle.font;
      component2.fontSize = 27;
      component2.resizeTextForBestFit = true;
      component2.resizeTextMinSize = 8;
      component2.resizeTextMaxSize = 27;
      component2.alignment = (TextAnchor) 4;
      ((Graphic) component2).color = this.design == 3 ? Color.black : Color.white;
      component2.text = caption;
      ((Component) component2).transform.SetParent(transform1, true);
    }
    transform1.SetParent(canvas, true);
    return transform1;
  }
  public static AudioClip ClickSound(MenuState state, string button)
  {
    if (state.Design == 7)
      return ImportedAssetMenu.Load("nxo-click").LoadAsset<AudioClip>("destiny");
    if (state.Design == 2)
      return ImportedAssetMenu.Load("hamburbur").LoadAsset<AudioClip>(button.StartsWith("Key_", StringComparison.Ordinal) ? "assets/sounds/keyboardclick.wav" : "assets/sounds/default.wav");
    if (state.Design != 3)
      return (AudioClip) null;
    bool flag = button == "ButtonHome" || button == "ButtonPrevious" || button == "ButtonNext" || button == "DesignSettings" || button == "ButtonSearch";
    int result;
    if (button.StartsWith("Row", StringComparison.Ordinal) && int.TryParse(button.Substring(3), out result))
    {
      int index = state.EntryAt(result);
      flag = ((flag ? 1 : 0) | (index < 0 ? 0 : (MenuCatalog.Buttons[index].Target >= 0 ? 1 : 0))) != 0;
    }
    return ImportedAssetMenu.Load("monosandbox").LoadAsset<AudioClip>($"assets/assets/monosandbox/audioclip/step{(flag ? "1" : "2")}.ogg");
  }
  private static UnityEngine.UI.Text SandboxText(
    Transform canvas,
    Font font,
    string name,
    Vector3 position,
    string caption)
  {
    UnityEngine.UI.Text component = new GameObject(name, new Type[3]
    {
      typeof (RectTransform),
      typeof (CanvasRenderer),
      typeof (UnityEngine.UI.Text)
    }).GetComponent<UnityEngine.UI.Text>();
    ((Component) component).transform.SetParent(canvas, false);
    ((Component) component).transform.rotation = Quaternion.Euler(0.0f, 90f, 0.0f);
    ((Component) component).transform.localScale = (Vector3.one * 1f / 1000f);
    ((Component) component).transform.position = position;
    ((Graphic) component).rectTransform.sizeDelta = new Vector2(135f, 135f);
    component.font = font;
    component.fontSize = 24;
    component.resizeTextForBestFit = true;
    component.resizeTextMinSize = 12;
    component.resizeTextMaxSize = 24;
    component.alignment = (TextAnchor) 4;
    ((Graphic) component).color = Color.black;
    component.text = caption;
    return component;
  }
  private Transform ClassicCube(
    Transform parent,
    string name,
    Vector3 position,
    Vector3 size,
    Color color)
  {
    Transform transform = GameObject.CreatePrimitive((PrimitiveType) 3).transform;
    ((Object) transform).name = name;
    transform.SetParent(parent.Find("Decor/Canvas") ?? parent, false);
    transform.localPosition = position;
    transform.localScale = size;
    Material material = new Material(((Component) this.root.Find("Background")).GetComponent<Renderer>().sharedMaterial)
    {
      color = color
    };
    this.createdMaterials.Add(material);
    ((Component) transform).GetComponent<Renderer>().sharedMaterial = material;
    return transform;
  }
  private static UnityEngine.UI.Text ClassicText(
    Transform canvas,
    Font font,
    string name,
    Vector3 position,
    Vector2 size,
    string caption,
    int fontSize)
  {
    UnityEngine.UI.Text text = ImportedAssetMenu.SandboxText(canvas, font, name, position, caption);
    ((Component) text).transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.forward);
    ((Graphic) text).rectTransform.sizeDelta = (size * 1000f);
    ((Graphic) text).color = Color.white;
    text.fontSize = fontSize;
    text.resizeTextMaxSize = fontSize;
    text.resizeTextMinSize = 9;
    return text;
  }
  private static TMP_Text IiText(
    Transform parent,
    TMP_FontAsset font,
    string name,
    Vector3 position,
    Vector2 size,
    string caption)
  {
    TMP_Text component = new GameObject(name, new Type[1]
    {
      typeof (TextMeshPro)
    }).GetComponent<TMP_Text>();
    component.transform.SetParent(parent, false);
    component.transform.position = position;
    component.transform.rotation = Quaternion.LookRotation(Vector3.left, Vector3.forward);
    component.rectTransform.sizeDelta = size;
    component.font = font;
    component.text = caption;
    ((Graphic) component).color = MenuPalette.Unpack(16760445U);
    component.fontStyle = (FontStyles) 2;
    component.alignment = (TextAlignmentOptions) 514;
    component.enableAutoSizing = true;
    component.fontSizeMin = 0.1f;
    component.fontSizeMax = 8f;
    return component;
  }
  private Transform IiIcon(Transform control, int kind)
  {
    Transform transform1 = new GameObject("NativeIcon").transform;
    transform1.SetParent(control.parent, false);
    transform1.position = (control.position + (Vector3.right * 0.008f));
    List<Vector2[]> vector2ArrayList = new List<Vector2[]>();
    if (kind == 2)
    {
      vector2ArrayList.Add(new Vector2[3]
      {
        new Vector2(-0.009f, 0.0f),
        new Vector2(0.0f, 0.008f),
        new Vector2(0.009f, 0.0f)
      });
      vector2ArrayList.Add(new Vector2[4]
      {
        new Vector2(-3f / 500f, 1f / 1000f),
        new Vector2(-3f / 500f, -0.008f),
        new Vector2(3f / 500f, -0.008f),
        new Vector2(3f / 500f, 1f / 1000f)
      });
    }
    else
    {
      Vector2[] vector2Array = new Vector2[33];
      for (int index = 0; index < vector2Array.Length; ++index)
      {
        float num1 = (float) ((double) index * 3.1415927410125732 * 2.0 / 32.0);
        float num2 = kind == 3 ? 0.0065f : (index % 4 < 2 ? 0.0085f : 0.0065f);
        vector2Array[index] = (new Vector2(Mathf.Cos(num1), Mathf.Sin(num1)) * num2);
      }
      vector2ArrayList.Add(vector2Array);
      if (kind == 3)
        vector2ArrayList.Add(new Vector2[2]
        {
          new Vector2(0.0045f, -0.0045f),
          new Vector2(0.01f, -0.01f)
        });
    }
    foreach (Vector2[] vector2Array in vector2ArrayList)
    {
      for (int index = 1; index < vector2Array.Length; ++index)
      {
        Vector2 vector2_1 = (vector2Array[index] - vector2Array[index - 1]);
        Vector2 vector2_2 = ((vector2Array[index] + vector2Array[index - 1]) / 2f);
        Transform transform2 = this.ClassicCube(transform1, "Stroke", Vector3.zero, new Vector3(1f / 1000f, (vector2_1).magnitude, 0.0015f), MenuPalette.Unpack(16760445U));
        transform2.localPosition = new Vector3(0.0f, -vector2_2.x, vector2_2.y);
        transform2.localRotation = Quaternion.Euler(Mathf.Atan2(-vector2_1.y, vector2_1.x) * 57.29578f, 0.0f, 0.0f);
      }
    }
    transform1.SetParent(control, true);
    return transform1;
  }
  public void SetActive(bool value)
  {
    this.active = value;
    this.Surface.SetActive(value);
    this.signature = (string) null;
    if (value)
      return;
    ((Component) this.root.Find("DesignSettings")).gameObject.SetActive(false);
  }
  public void Refresh(MenuState state)
  {
    if (!this.active)
      return;
    this.state = state;
    this.Surface.HideOriginals(state);
    foreach (Renderer componentsInChild in ((Component) this.root.Find("Keyboard")).GetComponentsInChildren<Renderer>(true))
      componentsInChild.enabled = false;
    ((Component) this.keyboard).gameObject.SetActive(state.KeyboardOpen);
    string str1 = state.Query.Length == 0 ? "Type to search_" : state.Query + "_";
    if (((Object) this.keyboardQuery != (Object) null))
      this.keyboardQuery.text = str1;
    if (((Object) this.sandboxQuery != (Object) null))
      this.sandboxQuery.text = str1;
    ((Component) this.root.Find("DesignSettings")).gameObject.SetActive(this.design >= 3 && !state.ColorPickerOpen);
    StringBuilder stringBuilder = new StringBuilder();
    string text1 = ((Component) this.root.Find("PageLabel")).GetComponent<TextMesh>().text;
    if (((Object) this.iiTitle != (Object) null))
    {
      this.iiTitle.text = $"ii's <b>Stupid</b> Menu <color=grey>[</color>{(state.Page + 1).ToString()}<color=grey>]</color>";
      this.iiPage.text = MenuCatalog.Categories[state.Category];
      ((Component) this.iiHomeIcon).gameObject.SetActive(!state.ColorPickerOpen);
      ((Component) this.iiHomeLabel).gameObject.SetActive(state.ColorPickerOpen);
      this.iiHomeLabel.text = state.EditingEnd ? "Done" : "Next";
    }
    if (((Object) this.title != (Object) null))
    {
      this.title.text = $"hamburbur\n<size=65%>{text1}</size>";
      this.title.enableAutoSizing = true;
    }
    if (((Object) this.sandboxTitle != (Object) null))
    {
      this.sandboxTitle.text = this.design == 3 ? "MONO SANDBOX MOD" : MenuDesign.Names[this.design];
      ((Component) this.Surface.Source.transform.Find("Decor/Canvas/Version")).GetComponent<UnityEngine.UI.Text>().text = ((Component) this.root.Find("NotificationBackground")).gameObject.activeSelf ? MenuCatalog.Categories[state.Category] : text1;
    }
    stringBuilder.Append(text1).Append(str1).Append(state.KeyboardOpen).Append(state.ColorPickerOpen);
    if (((Object) this.advanceLabel != (Object) null))
    {
      ((Component) this.advanceLabel).gameObject.SetActive(state.ColorPickerOpen);
      this.advanceLabel.text = state.EditingEnd ? "Done" : "Next";
      ((Component) this.controls["ButtonHome"].Find("ReturnImageCanvas/Image")).gameObject.SetActive(!state.ColorPickerOpen);
      stringBuilder.Append(this.advanceLabel.text);
    }
    bool flag1 = state.ResultCount == 0 && !state.KeyboardOpen && !state.ColorPickerOpen;
    if (((Object) this.emptyTmp != (Object) null))
    {
      ((Component) this.emptyTmp).gameObject.SetActive(flag1);
      this.emptyTmp.text = text1;
    }
    if (((Object) this.emptyText != (Object) null))
    {
      ((Component) this.emptyText).gameObject.SetActive(flag1);
      this.emptyText.text = text1;
    }
    for (int row = 0; row < this.normal.Length; ++row)
    {
      Transform transform = this.root.Find("Row" + row.ToString());
      bool activeSelf = ((Component) this.root.Find("IncrementPlus" + row.ToString())).gameObject.activeSelf;
      string str2 = ((Component) transform.Find("Label")).GetComponent<TextMesh>().text.Replace("\n", " ");
      string text2 = ((Component) transform.Find("State")).GetComponent<TextMesh>().text;
      bool flag2 = state.IsFavorite(state.EntryAt(row));
      bool flag3 = state.IsOn(state.EntryAt(row));
      string str3 = text2 == "On" || text2 == "Off" ? "" : text2;
      string str4 = str2 + (str3.Length == 0 ? "" : $"\n<size=65%>{str3}</size>");
      if (this.design == 5)
      {
        ((Component) this.normal[row]).gameObject.SetActive(((Component) transform).gameObject.activeSelf);
        ((Component) this.iiLabels[row]).gameObject.SetActive(((Component) transform).gameObject.activeSelf);
        this.iiLabels[row].text = str4;
        this.iiLabels[row].rectTransform.sizeDelta = new Vector2(activeSelf ? 0.16f : (flag2 ? 0.18f : 0.2f), 0.024f);
        this.iiLabels[row].transform.position = (this.normal[row].position + new Vector3(0.008f, activeSelf ? 0.035f : 0.0f, (float) (0.0035600000992417336 - (double) row * 0.00016900000628083944)));
        this.hearts[row].SetActive(((Component) transform).gameObject.activeSelf & flag2);
        this.theme.SetEnabled(((Component) this.normal[row]).GetComponent<Renderer>(), flag3);
        for (int index = 0; index < 2; ++index)
          ((Component) this.iiArrows[row, index]).gameObject.SetActive(((Component) transform).gameObject.activeSelf & activeSelf);
      }
      else if (((Object) this.labels[row] != (Object) null))
      {
        this.labels[row].text = str4;
        this.labels[row].enableAutoSizing = true;
        this.incrementLabels[row].text = str4;
        this.incrementLabels[row].enableAutoSizing = true;
        ((Component) this.normal[row]).gameObject.SetActive(((Component) transform).gameObject.activeSelf && !activeSelf);
        ((Component) this.incremental[row]).gameObject.SetActive(((Component) transform).gameObject.activeSelf & activeSelf);
        ((Component) this.normal[row]).GetComponent<Renderer>().enabled = !flag3;
        this.hearts[row].SetActive(flag2);
        this.controls["Row" + row.ToString()] = activeSelf ? this.incremental[row] : this.normal[row];
        if (!this.scales.ContainsKey(this.controls["Row" + row.ToString()]))
          this.scales[this.controls["Row" + row.ToString()]] = this.controls["Row" + row.ToString()].localScale;
      }
      else
      {
        ((Component) this.normal[row]).gameObject.SetActive(((Component) transform).gameObject.activeSelf);
        ((Component) this.sandboxLabels[row]).gameObject.SetActive(((Component) transform).gameObject.activeSelf);
        this.sandboxLabels[row].text = str2.ToUpperInvariant() + (str3.Length == 0 ? "" : "\n" + str3);
        if (this.design >= 4)
        {
          ((Graphic) this.sandboxLabels[row]).rectTransform.sizeDelta = new Vector2(activeSelf ? 162f : (flag2 ? 200f : (this.design == 5 ? 250f : 224f)), this.design == 5 ? 22f : 29f);
          ((Component) this.sandboxLabels[row]).transform.position = (this.normal[row].position + new Vector3(3f / 500f, activeSelf ? 0.03f : 0.0f, 0.0f));
        }
        this.hearts[row].SetActive(((Component) transform).gameObject.activeSelf & flag2);
        this.theme.SetEnabled(((Component) this.normal[row]).GetComponent<Renderer>(), flag3);
        for (int index = 0; index < 2; ++index)
          ((Component) this.sandboxArrows[row, index]).gameObject.SetActive(((Component) transform).gameObject.activeSelf & activeSelf);
      }
      stringBuilder.Append(str4).Append(flag2).Append(flag3).Append(((Component) transform).gameObject.activeSelf);
    }
    foreach (KeyValuePair<string, Transform> control in this.controls)
    {
      Transform proxy = this.root.Find(control.Key);
      if (!control.Key.StartsWith("Row"))
        ((Component) control.Value).gameObject.SetActive(((Component) proxy).gameObject.activeSelf);
      if (control.Key == "ButtonHome")
      {
        string text3 = ((Component) this.root.Find("ButtonHome/Label")).GetComponent<TextMesh>().text;
        TMP_Text componentInChildren = ((Component) control.Value).GetComponentInChildren<TMP_Text>(true);
        if (((Object) componentInChildren != (Object) null))
          componentInChildren.text = text3;
        foreach (UnityEngine.UI.Text componentsInChild in this.Surface.Source.GetComponentsInChildren<UnityEngine.UI.Text>(true))
        {
          if (((Object) componentsInChild).name == "ButtonHome")
            componentsInChild.text = text3.ToUpperInvariant();
        }
        stringBuilder.Append(text3);
      }
      this.Surface.Map(proxy, control.Value);
    }
    if (this.picker.Refresh(state))
      this.Surface.Dirty();
    MenuButtonEffects.BindRows(this.Surface.Source.transform, state, (IEnumerable<KeyValuePair<string, Transform>>) this.controls);
    string str5 = stringBuilder.ToString();
    if (this.signature != str5)
    {
      this.signature = str5;
      this.Surface.Dirty();
    }
    this.Tick();
  }
  public void Tick()
  {
    if (!this.active)
      return;
    if (this.notification.Refresh(this.state))
      this.Surface.Dirty();
    if (this.state != null && this.theme.Apply(this.state, Time.unscaledTime))
      this.Surface.Dirty();
    if (this.state != null && MenuFonts.Apply(this.Surface.Source.transform, this.state.FontIndex))
      this.Surface.Dirty();
    foreach (KeyValuePair<string, Transform> control in this.controls)
    {
      float x = this.root.Find(control.Key).localScale.x;
      float num;
      if (!this.animated.TryGetValue(control.Value, out num) || !Mathf.Approximately(num, x))
      {
        control.Value.localScale = (this.scales[control.Value] * x);
        this.animated[control.Value] = x;
        this.Surface.Dirty();
      }
    }
    this.Surface.Tick();
  }
  public void Dispose()
  {
    this.notification.Dispose();
    this.Surface.Dispose();
  }
}
