
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Taurus;
public sealed class RexonKeyboard : IDisposable
{
  private readonly MenuState state;
  private readonly Action changed;
  internal readonly Action<bool> Feedback;
  private readonly Material materialTemplate;
  private readonly TMP_FontAsset fallbackFont;
  private readonly List<Material> materials = new List<Material>();
  private Transform leftHand;
  private Transform rightHand;
  internal SphereCollider LeftHandPointer;
  internal SphereCollider RightHandPointer;
  private const float KeySize = 0.03f;
  private const float KeySpacing = 0.005f;
  private readonly string[][] KeyRows = new string[4][]
  {
    new string[10]
    {
      "1",
      "2",
      "3",
      "4",
      "5",
      "6",
      "7",
      "8",
      "9",
      "0"
    },
    new string[10]
    {
      "Q",
      "W",
      "E",
      "R",
      "T",
      "Y",
      "U",
      "I",
      "O",
      "P"
    },
    new string[9]{ "A", "S", "D", "F", "G", "H", "J", "K", "L" },
    new string[7]{ "Z", "X", "C", "V", "B", "N", "M" }
  };
  private GameObject KeyboardRoot;
  private GameObject LeftPointer;
  private GameObject RightPointer;
  private TextMeshPro DisplayText;
  private string InputText = string.Empty;
  private bool ShiftEnabled;
  internal bool Spawned;
  public Transform Root
  {
    get
    {
      return !((Object) this.KeyboardRoot == (Object) null) ? this.KeyboardRoot.transform : (Transform) null;
    }
  }
  public RexonKeyboard(
    MenuState state,
    Material material,
    TMP_FontAsset font,
    Action changed,
    Action<bool> feedback)
  {
    this.state = state;
    this.materialTemplate = material;
    this.fallbackFont = font;
    this.changed = changed;
    this.Feedback = feedback;
  }
  private Material NewMaterial()
  {
    Material material = new Material(this.materialTemplate);
    Shader shader = Shader.Find("GorillaTag/UberShader");
    if (((Object) shader != (Object) null))
      material.shader = shader;
    this.materials.Add(material);
    return material;
  }
  public void Dispose()
  {
    this.Close();
    foreach (Object material in this.materials)
      NativeUiSnapshot.Destroy(material);
    this.materials.Clear();
  }
  public bool IsSpawned => this.Spawned;
  public bool IsKeyboardPointer(Collider collider)
  {
    if (!this.Spawned || ((Object) collider == (Object) null))
      return false;
    GameObject gameObject = ((Component) collider).gameObject;
    if (((Object) this.LeftPointer != (Object) null) && ((Object) gameObject == (Object) this.LeftPointer))
      return true;
    return ((Object) this.RightPointer != (Object) null) && ((Object) gameObject == (Object) this.RightPointer);
  }
  public bool IsLeftPointer(Collider collider)
  {
    return this.Spawned && ((Object) collider != (Object) null) && ((Object) this.LeftPointer != (Object) null) && ((Object) ((Component) collider).gameObject == (Object) this.LeftPointer);
  }
  public void Spawn(Transform playerBody, Transform leftHand, Transform rightHand)
  {
    if (this.Spawned)
      return;
    this.Spawned = true;
    this.InputText = string.Empty;
    this.ShiftEnabled = false;
    float num1 = 0.035f;
    this.KeyboardRoot = new GameObject("RexonVRKeyboard");
    if (Application.isPlaying)
      Object.DontDestroyOnLoad((Object) this.KeyboardRoot);
    this.leftHand = leftHand;
    this.rightHand = rightHand;
    Vector3 forward = playerBody.forward;
    forward.y = 0.0f;
    (forward).Normalize();
    this.KeyboardRoot.transform.position = ((playerBody.position + (forward * 0.4f)) - (Vector3.up * 0.15f));
    this.KeyboardRoot.transform.rotation = (Quaternion.LookRotation(forward, Vector3.up) * Quaternion.Euler(55f, 0.0f, 0.0f));
    this.CreatePanel("KeyboardBackground", new Vector3(0.0f, 0.01f, -1f / 500f), new Vector3(num1 * 11f, num1 * 7.5f, 1f / 500f), new Color(0.08f, 0.0f, 0.14f, 0.95f));
    float num2 = num1 * 3f;
    for (int index1 = 0; index1 < this.KeyRows.Length; ++index1)
    {
      string[] keyRow = this.KeyRows[index1];
      float num3 = num2 - (float) index1 * num1;
      float num4 = (float) ((double) -keyRow.Length * (double) num1 / 2.0 + 0.014999999664723873);
      for (int index2 = 0; index2 < keyRow.Length; ++index2)
        this.CreateKey(keyRow[index2], new Vector3(num4 + (float) index2 * num1, num3, 0.0f), 0.03f);
    }
    float num5 = num2 - 4f * num1;
    this.CreateKey("Shift", new Vector3((float) (-(double) num1 * 3.0), num5, 0.0f), 0.03f, 2f);
    this.CreateKey("Space", new Vector3(0.0f, num5, 0.0f), 0.03f, 3f);
    this.CreateKey("Del", new Vector3(num1 * 2.5f, num5, 0.0f), 0.03f, 1.5f);
    this.CreateKey("Done", new Vector3(num1 * 4.2f, num5, 0.0f), 0.03f, 1.5f);
    GameObject panel = this.CreatePanel("DisplayPanel", new Vector3(0.0f, num2 + num1 * 1.8f, 0.0f), new Vector3(num1 * 10f, num1 * 0.9f, 3f / 1000f), new Color(0.05f, 0.0f, 0.1f, 1f));
    GameObject gameObject = new GameObject("InputDisplay");
    gameObject.transform.SetParent(panel.transform, false);
    gameObject.transform.localPosition = new Vector3(0.0f, 0.0f, -0.6f);
    gameObject.transform.localRotation = Quaternion.identity;
    gameObject.transform.localScale = new Vector3(0.3f, 1f, 1f);
    this.DisplayText = gameObject.AddComponent<TextMeshPro>();
    if (((Object) ((TMP_Text) this.DisplayText).font == (Object) null))
      ((TMP_Text) this.DisplayText).font = this.fallbackFont;
    ((TMP_Text) this.DisplayText).fontSize = 3f;
    ((Graphic) this.DisplayText).color = Color.white;
    ((TMP_Text) this.DisplayText).alignment = (TextAlignmentOptions) 514;
    ((TMP_Text) this.DisplayText).text = "|";
    ((TMP_Text) this.DisplayText).rectTransform.sizeDelta = new Vector2(10f, 1f);
    this.CreateHandPointers();
    this.UpdateDisplay();
  }
  private GameObject CreatePanel(
    string name,
    Vector3 localPosition,
    Vector3 localScale,
    Color color)
  {
    GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
    ((Object) primitive).name = name;
    primitive.transform.SetParent(this.KeyboardRoot.transform, false);
    primitive.transform.localPosition = localPosition;
    primitive.transform.localScale = localScale;
    ((Collider) primitive.GetComponent<BoxCollider>()).enabled = false;
    NativeUiSnapshot.Destroy((Object) primitive.GetComponent<BoxCollider>());
    Renderer component = primitive.GetComponent<Renderer>();
    component.sharedMaterial = this.NewMaterial();
    component.sharedMaterial.color = color;
    return primitive;
  }
  private void CreateKey(string keyValue, Vector3 localPosition, float keySize, float width = 1f)
  {
    GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
    ((Object) primitive).name = keyValue;
    primitive.transform.SetParent(this.KeyboardRoot.transform, false);
    primitive.transform.localPosition = localPosition;
    primitive.transform.localScale = new Vector3(keySize * width, keySize, keySize * 0.3f);
    BoxCollider component1 = primitive.GetComponent<BoxCollider>();
    ((Collider) component1).isTrigger = true;
    component1.size = new Vector3(1f, 1f, 3f);
    Renderer component2 = primitive.GetComponent<Renderer>();
    component2.sharedMaterial = this.NewMaterial();
    component2.sharedMaterial.color = new Color(0.15f, 0.0f, 0.26f, 1f);
    GameObject gameObject = new GameObject("Label");
    gameObject.transform.SetParent(primitive.transform, false);
    gameObject.transform.localPosition = new Vector3(0.0f, 0.0f, -0.6f);
    gameObject.transform.localRotation = Quaternion.identity;
    gameObject.transform.localScale = new Vector3(1f / width, 1f, 1f);
    TextMeshPro textMeshPro = gameObject.AddComponent<TextMeshPro>();
    if (((Object) ((TMP_Text) textMeshPro).font == (Object) null))
      ((TMP_Text) textMeshPro).font = this.fallbackFont;
    ((TMP_Text) textMeshPro).fontSize = 2.5f;
    ((Graphic) textMeshPro).color = Color.white;
    ((TMP_Text) textMeshPro).alignment = (TextAlignmentOptions) 514;
    ((TMP_Text) textMeshPro).text = keyValue;
    ((TMP_Text) textMeshPro).rectTransform.sizeDelta = new Vector2(2f, 1f);
    RexonKeyboardKey rexonKeyboardKey = primitive.AddComponent<RexonKeyboardKey>();
    rexonKeyboardKey.KeyValue = keyValue;
    rexonKeyboardKey.Owner = this;
  }
  private void CreateHandPointers()
  {
    SphereCollider collider1;
    this.LeftPointer = this.CreateHandPointer(this.leftHand, out collider1);
    SphereCollider collider2;
    this.RightPointer = this.CreateHandPointer(this.rightHand, out collider2);
    this.LeftHandPointer = collider1;
    this.RightHandPointer = collider2;
  }
  private GameObject CreateHandPointer(Transform hand, out SphereCollider collider)
  {
    GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 0);
    primitive.transform.parent = hand;
    primitive.transform.localPosition = new Vector3(0.0f, -0.1f, 0.0f);
    primitive.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
    NativeUiSnapshot.Destroy((Object) primitive.GetComponent<Rigidbody>());
    primitive.GetComponent<Renderer>().sharedMaterial = this.NewMaterial();
    primitive.GetComponent<Renderer>().sharedMaterial.color = new Color(0.75f, 0.53f, 1f, 0.8f);
    collider = primitive.GetComponent<SphereCollider>();
    ((Collider) collider).isTrigger = true;
    return primitive;
  }
  public void Close()
  {
    if (!this.Spawned)
      return;
    this.Spawned = false;
    this.DestroyIfPresent(this.LeftPointer);
    this.DestroyIfPresent(this.RightPointer);
    this.DestroyIfPresent(this.KeyboardRoot);
    this.LeftPointer = (GameObject) null;
    this.RightPointer = (GameObject) null;
    this.KeyboardRoot = (GameObject) null;
    this.DisplayText = (TextMeshPro) null;
  }
  private void DestroyIfPresent(GameObject gameObject)
  {
    if (!((Object) gameObject != (Object) null))
      return;
    NativeUiSnapshot.Destroy((Object) gameObject);
  }
  public void HandleKeyPress(string keyValue)
  {
    switch (keyValue)
    {
      case "Space":
        this.InputText += " ";
        break;
      case "Del":
        if (this.InputText.Length > 0)
        {
          this.InputText = this.InputText.Substring(0, this.InputText.Length - 1);
          break;
        }
        break;
      case "Shift":
        this.ShiftEnabled = !this.ShiftEnabled;
        break;
      case "Done":
        this.state.Type("Done");
        this.changed();
        this.Close();
        return;
      default:
        if (keyValue.Length == 1)
        {
          this.InputText += this.ShiftEnabled ? keyValue.ToUpper() : keyValue.ToLower();
          this.ShiftEnabled = false;
          break;
        }
        break;
    }
    this.UpdateDisplay();
    this.state.SetRexonSearchQuery(this.InputText);
    this.changed();
  }
  private void UpdateDisplay()
  {
    if (!((Object) this.DisplayText != (Object) null))
      return;
    ((TMP_Text) this.DisplayText).text = this.InputText + (Time.frameCount / 30 % 2 == 0 ? "|" : " ");
  }
  public void RefreshDisplay()
  {
    if (!this.Spawned || !((Object) this.KeyboardRoot != (Object) null))
      return;
    MenuFonts.Apply(this.KeyboardRoot.transform, this.state.FontIndex);
    this.UpdateDisplay();
  }
}
