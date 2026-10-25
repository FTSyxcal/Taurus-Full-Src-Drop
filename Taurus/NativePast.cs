
using System;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Taurus;
internal sealed class NativePast
{
  public const string PrefabPath = "assets/### past assets/new folder/new folder/hamburrburpast2 1.prefab";
  public NativeUiSnapshot Build(MenuState state, Transform proxies)
  {
    NativeUiSnapshot snapshot = new NativeUiSnapshot()
    {
      Forward = Vector3.right,
      Up = Vector3.up
    };
    GameObject gameObject = Object.Instantiate<GameObject>(ImportedAssetMenu.Load("past").LoadAsset<GameObject>("assets/### past assets/new folder/new folder/hamburrburpast2 1.prefab"));
    ((Object) gameObject).name = "OriginalPast";
    gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
    snapshot.Source = gameObject;
    foreach (Renderer componentsInChild in gameObject.GetComponentsInChildren<Renderer>(true))
    {
      if (((Object) ((Component) componentsInChild).GetComponent<TMP_Text>() == (Object) null))
      {
        Material[] sharedMaterials = componentsInChild.sharedMaterials;
        for (int index = 0; index < sharedMaterials.Length; ++index)
        {
          sharedMaterials[index] = new Material(sharedMaterials[index]);
          snapshot.Materials.Add(sharedMaterials[index]);
        }
        componentsInChild.sharedMaterials = sharedMaterials;
      }
    }
    Transform root = gameObject.transform;
    foreach (TMP_Text componentsInChild in gameObject.GetComponentsInChildren<TMP_Text>(true))
    {
      if (((Object) componentsInChild.font == (Object) null))
        componentsInChild.font = ((Component) proxies.Find("XyferFont")).GetComponent<TMP_Text>().font;
    }
    snapshot.Header = ((Component) root.Find("Text (TMP) (11)")).GetComponent<TMP_Text>();
    snapshot.Header.text = "Past";
    Transform transform1 = new GameObject("NativeContent").transform;
    transform1.SetParent(root, false);
    snapshot.Content = transform1;
    string[] strArray1 = new string[4]
    {
      "Mod button",
      "Mod button 2",
      "Mod button 3",
      "Mod button 4"
    };
    Transform transform2 = root.Find("- increment");
    Transform transform3 = root.Find("+ increment");
    ((Component) root.Find("increment")).gameObject.SetActive(false);
    ((Component) transform2).gameObject.SetActive(false);
    ((Component) transform3).gameObject.SetActive(false);
    Bounds region = new Bounds();
    for (int row = 0; row < 4; ++row)
    {
      Transform control1 = root.Find(strArray1[row]);
      string str = ((Component) proxies.Find($"Row{row.ToString()}/Label")).GetComponent<TextMesh>().text.Replace("\n", " ");
      bool activeSelf = ((Component) proxies.Find("IncrementPlus" + row.ToString())).gameObject.activeSelf;
      TMP_Text tmpText = Label(control1, str + (activeSelf ? " : " + ((Component) proxies.Find($"Row{row.ToString()}/State")).GetComponent<TextMesh>().text : ""));
      Bounds bounds = ImportedSurface.BoundsOf(control1);
      if (row == 0)
      {
        region = bounds;
      }
      else
      {
        (region).Encapsulate(bounds);
      }
      control1.SetParent(transform1, true);
      snapshot.Controls["Row" + row.ToString()] = control1;
      ((Component) control1).gameObject.SetActive(state.EntryAt(row) >= 0 || state.SoundNameAt(row) != null);
      if (activeSelf)
      {
        bool[] flagArray = new bool[2]{ false, true };
        foreach (bool flag in flagArray)
        {
          Transform control2 = Object.Instantiate<Transform>(flag ? transform3 : transform2, transform1, true);
          ((Component) control2).gameObject.SetActive(true);
          Resize(control2, (bounds).size.z * 0.18f, (bounds).size.y * 0.7f);
          control2.position = (((bounds).center + ((Vector3.forward * (bounds).size.z) * (flag ? 0.38f : -0.38f))) - (Vector3.left * 3f / 1000f));
          Label(control2, flag ? "+" : "-");
          snapshot.Controls[(flag ? "IncrementPlus" : "IncrementMinus") + row.ToString()] = control2;
        }
        RectTransform rectTransform = tmpText.rectTransform;
        rectTransform.sizeDelta = (rectTransform.sizeDelta * new Vector2(0.6f, 1f));
      }
    }
    snapshot.PickerRegion = new Bounds?(region);
    Bind("disconecting", "ButtonDisconnect", "Disconnect");
    Bind("return", "ButtonHome", state.ColorPickerOpen ? (state.EditingEnd ? "Done" : "Next") : "Home");
    Bind("page left", "ButtonPrevious", "<");
    Bind("page right", "ButtonNext", ">");
    Bind("page left Category", "PastCategoryPrevious", "<");
    Bind("page right Category", "PastCategoryNext", ">");
    for (int slot = 0; slot < 6; ++slot)
    {
      int index = state.PastCategoryAt(slot);
      Bind("Category " + (slot + 1).ToString(), "PastCategory" + slot.ToString(), index < 0 ? "" : MenuCatalog.Categories[index]);
      ((Component) snapshot.Controls["PastCategory" + slot.ToString()]).gameObject.SetActive(index >= 0);
    }
    Transform control3 = root.Find("return");
    Transform control4 = Object.Instantiate<Transform>(control3, root, true);
    Bounds bounds1 = ((Renderer) ((Component) control3).GetComponent<MeshRenderer>()).bounds;
    float z1 = (bounds1).size.z;
    Transform transform4 = control3;
    transform4.position = (transform4.position + ((Vector3.forward * z1) * 0.25f));
    Transform transform5 = control4;
    transform5.position = (transform5.position + ((Vector3.back * z1) * 0.25f));
    Transform control5 = control3;
    double width1 = (double) z1 * 0.46000000834465027;
    Bounds bounds2 = ((Renderer) ((Component) control3).GetComponent<MeshRenderer>()).bounds;
    double y1 = (double) (bounds2).size.y;
    Resize(control5, (float) width1, (float) y1);
    Transform control6 = control4;
    double width2 = (double) z1 * 0.46000000834465027;
    bounds2 = ((Renderer) ((Component) control4).GetComponent<MeshRenderer>()).bounds;
    double y2 = (double) (bounds2).size.y;
    Resize(control6, (float) width2, (float) y2);
    Label(control3, state.ColorPickerOpen ? (state.EditingEnd ? "Done" : "Next") : "Home");
    Label(control4, "Search");
    snapshot.Controls["ButtonSearch"] = control4;
    Transform template = null;
    float keyHeight = 0f;
    if (state.KeyboardOpen)
    {
      template = transform1.Find(strArray1[0]);
      string[] strArray2 = new string[4]
      {
        "1234567890",
        "QWERTYUIOP",
        "ASDFGHJKL",
        "ZXCVBNM"
      };
      float z2 = (region).size.z;
      float y3 = (region).size.y;
      float num = z2 / 10.5f;
      keyHeight = y3 / 6f;
      for (int index1 = 0; index1 < strArray2.Length; ++index1)
      {
        for (int index2 = 0; index2 < strArray2[index1].Length; ++index2)
        {
          Key(strArray2[index1][index2].ToString(), ((float) index2 - (float) (strArray2[index1].Length - 1) * 0.5f) * num, (float) ((double) y3 * 0.5 - (double) keyHeight * ((double) index1 + 1.0)), num * 0.85f);
        }
      }
      string[] strArray3 = new string[4]
      {
        "Clear",
        "Space",
        "Back",
        "Done"
      };
      for (int index = 0; index < strArray3.Length; ++index)
      {
        Key(strArray3[index], (float) (((double) index - 1.5) * (double) z2 / 4.0), (float) (-(double) y3 * 0.5 + (double) keyHeight * 0.5), z2 * 0.23f);
      }
    }
    return snapshot;
    void Bind(string findName, string key, string label)
    {
      Transform control = root.Find(findName);
      Label(control, label);
      snapshot.Controls[key] = control;
    }
    void Key(string caption, float x, float y, float width)
    {
      Transform control = Object.Instantiate<Transform>(template, transform1, true);
      ((Component) control).gameObject.SetActive(true);
      Resize(control, width, keyHeight);
      control.position = ((region.center + (Vector3.forward * x)) + (Vector3.up * y));
      Label(control, caption);
      snapshot.Controls["Key_" + caption] = control;
    }
    static void Resize(Transform control, float width, float height)
    {
      Bounds bounds = ((Renderer) ((Component) control).GetComponent<MeshRenderer>()).bounds;
      Vector3 localScale = control.localScale;
      for (int index = 0; index < 3; ++index)
      {
        Vector3 zero = Vector3.zero;
        (zero)[index] = 1f;
        Vector3 vector3 = control.TransformDirection(zero);
        Vector3 normalized = (vector3).normalized;
        ref Vector3 local = ref localScale;
        int num = index;
        (local)[num] = (local)[num] * (float) ((double) Mathf.Abs(normalized.x) + (double) Mathf.Abs(normalized.y) * (double) height / (double) (bounds).size.y + (double) Mathf.Abs(normalized.z) * (double) width / (double) (bounds).size.z);
      }
      control.localScale = localScale;
    }
    static TMP_Text Label(Transform control, string caption)
    {
      TMP_Text componentInChildren = ((Component) control).GetComponentInChildren<TMP_Text>(true);
      if (((Object) componentInChildren == (Object) null))
        throw new InvalidOperationException("Past label missing: " + ((Object) control).name);
      componentInChildren.text = caption;
      componentInChildren.enableAutoSizing = true;
      componentInChildren.fontSizeMin = 0.1f;
      Bounds bounds1 = ((Renderer) ((Component) control).GetComponent<MeshRenderer>()).bounds;
      Vector3 vector3_1 = componentInChildren.transform.TransformVector(Vector3.up);
      float magnitude1 = (vector3_1).magnitude;
      componentInChildren.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
      Quaternion localRotation = componentInChildren.transform.localRotation;
      Transform parent = componentInChildren.transform.parent;
      Transform transform = componentInChildren.transform;
      double num1 = (double) magnitude1;
      Vector3 vector3_2 = parent.TransformVector((localRotation * Vector3.right));
      double magnitude2 = (double) (vector3_2).magnitude;
      double num2 = num1 / magnitude2;
      double num3 = (double) magnitude1;
      vector3_2 = parent.TransformVector((localRotation * Vector3.up));
      double magnitude3 = (double) (vector3_2).magnitude;
      double num4 = num3 / magnitude3;
      double num5 = (double) magnitude1;
      vector3_2 = parent.TransformVector((localRotation * Vector3.forward));
      double magnitude4 = (double) (vector3_2).magnitude;
      double num6 = num5 / magnitude4;
      Vector3 vector3_3 = new Vector3((float) num2, (float) num4, (float) num6);
      transform.localScale = vector3_3;
      componentInChildren.rectTransform.sizeDelta = new Vector2((bounds1).size.z * 0.88f / Mathf.Max(0.0001f, Mathf.Abs(componentInChildren.transform.lossyScale.x)), (bounds1).size.y * 0.8f / Mathf.Max(0.0001f, Mathf.Abs(componentInChildren.transform.lossyScale.y)));
      componentInChildren.transform.position = new Vector3((bounds1).min.x - 0.0008f, (bounds1).center.y, (bounds1).center.z);
      BoxCollider boxCollider = ((Component) control).GetComponent<BoxCollider>();
      if (((Object) boxCollider == (Object) null))
        boxCollider = ((Component) control).gameObject.AddComponent<BoxCollider>();
      Bounds bounds2 = ((Component) control).GetComponent<MeshFilter>().sharedMesh.bounds;
      boxCollider.center = (bounds2).center;
      boxCollider.size = (bounds2).size;
      ((Collider) boxCollider).isTrigger = true;
      return componentInChildren;
    }
  }
}
