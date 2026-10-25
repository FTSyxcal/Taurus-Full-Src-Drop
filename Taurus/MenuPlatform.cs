
using UnityEngine;
namespace Taurus;
public static class MenuPlatform
{
  public static readonly Vector3 Size = new Vector3(0.025f, 0.3f, 0.4f);
  public static GameObject Create(GameObject menuPrefab, Vector3 position, Quaternion rotation)
  {
    GameObject gameObject1 = new GameObject("ItzGamePlatform");
    gameObject1.transform.SetPositionAndRotation(position, rotation);
    GameObject gameObject2 = Object.Instantiate<GameObject>(((Component) menuPrefab.transform.Find("Background")).gameObject, gameObject1.transform, false);
    ((Object) gameObject2).name = "Background";
    gameObject2.transform.localPosition = Vector3.zero;
    Bounds bounds = gameObject2.GetComponent<MeshFilter>().sharedMesh.bounds;
    gameObject2.transform.localScale = new Vector3(MenuPlatform.Size.x / (bounds).size.x, MenuPlatform.Size.y / (bounds).size.y, MenuPlatform.Size.z / (bounds).size.z);
    BoxCollider boxCollider = gameObject2.AddComponent<BoxCollider>();
    boxCollider.size = (bounds).size;
    boxCollider.center = (bounds).center;
    for (int index = 0; index < 2; ++index)
    {
      GameObject gameObject3 = Object.Instantiate<GameObject>(((Component) menuPrefab.transform.Find("Frame")).gameObject, gameObject1.transform, false);
      ((Object) gameObject3).name = "Outline" + index.ToString();
      gameObject3.transform.localPosition = new Vector3((float) ((index == 0 ? 1.0 : -1.0) * ((double) MenuPlatform.Size.x / 2.0 + 1.0 / 1000.0)), 0.0f, 0.0f);
      gameObject3.transform.localRotation = Quaternion.Euler(0.0f, (float) index * 180f, 0.0f);
      gameObject3.transform.localScale = new Vector3(1f, (float) (((double) MenuPlatform.Size.y + 1.0 / 500.0) / 0.37000000476837158), (float) (((double) MenuPlatform.Size.z + 1.0 / 500.0) / 0.550000011920929));
    }
    return gameObject1;
  }
  public static void Paint(
    GameObject platform,
    MenuState state,
    float time,
    MaterialPropertyBlock colors)
  {
    colors.Clear();
    colors.SetColor("_Color", MenuPalette.Background(state, time));
    ((Component) platform.transform.Find("Background")).GetComponent<Renderer>().SetPropertyBlock(colors);
    colors.Clear();
    colors.SetColor("_LeftColor", MenuPalette.Left(state, time));
    colors.SetColor("_RightColor", MenuPalette.Right(state, time));
    colors.SetFloat("_Speed", state.IsOn("Freeze gradient") ? 0.0f : 6.28318548f / state.GradientPeriod);
    for (int index = 0; index < 2; ++index)
      ((Component) platform.transform.Find("Outline" + index.ToString())).GetComponent<Renderer>().SetPropertyBlock(colors);
  }
  public static void Paint(
    GameObject platform,
    string theme,
    float time,
    MaterialPropertyBlock colors)
  {
    colors.Clear();
    colors.SetColor("_Color", MenuPalette.Background(theme, time));
    ((Component) platform.transform.Find("Background")).GetComponent<Renderer>().SetPropertyBlock(colors);
    colors.Clear();
    colors.SetColor("_LeftColor", MenuPalette.Left(theme, time));
    colors.SetColor("_RightColor", MenuPalette.Right(theme, time));
    for (int index = 0; index < 2; ++index)
      ((Component) platform.transform.Find("Outline" + index.ToString())).GetComponent<Renderer>().SetPropertyBlock(colors);
  }
}
