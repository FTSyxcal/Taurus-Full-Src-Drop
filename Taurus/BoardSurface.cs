
using TMPro;
using UnityEngine;
namespace Taurus;
public static class BoardSurface
{
  public static MeshRenderer FindPanel(RectTransform text, MeshRenderer[] candidates)
  {
    MeshRenderer panel = (MeshRenderer) null;
    float num1 = float.MaxValue;
    foreach (MeshRenderer candidate in candidates)
    {
      if (!(((Object) candidate).name == "TaurusBoardBackground") && !((Object) ((Component) candidate).GetComponentInParent<TMP_Text>() != (Object) null) && BoardSurface.Matches(candidate, (Transform) text))
      {
        Bounds bounds = ((Renderer) candidate).bounds;
        float num2 = Mathf.Abs(Vector3.Dot(((bounds).center - ((Transform) text).position), ((Transform) text).forward));
        if ((double) num2 < (double) num1)
        {
          panel = candidate;
          num1 = num2;
        }
      }
    }
    return panel;
  }
  public static Renderer CreateBackdrop(RectTransform text, Material material, MeshRenderer board = null)
  {
    GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 5);
    ((Object) primitive).name = "TaurusBoardBackground";
    Collider component1 = primitive.GetComponent<Collider>();
    component1.enabled = false;
    if (Application.isPlaying)
      Object.Destroy((Object) component1);
    else
      Object.DestroyImmediate((Object) component1);
    primitive.transform.SetParent((Transform) text, false);
    float num1 = 0.0015f / Mathf.Max(0.0001f, Mathf.Abs(((Transform) text).lossyScale.z));
    Bounds bounds1 = default;
    ref Bounds local = ref bounds1;
    Rect rect = text.rect;
    double x = (double) (rect).center.x;
    rect = text.rect;
    double y = (double) (rect).center.y;
    double num2 = (double) num1;
    Vector3 vector3_1 = new Vector3((float) x, (float) y, (float) num2);
    rect = text.rect;
    double width = (double) (rect).width;
    rect = text.rect;
    double height = (double) (rect).height;
    Vector3 vector3_2 = new Vector3((float) width, (float) height, 0.0f);
    bounds1 = new Bounds(vector3_1, vector3_2);
    if (((Object) board != (Object) null))
    {
      Bounds bounds2 = ((Component) board).GetComponent<MeshFilter>().sharedMesh.bounds;
      bounds1 = new Bounds(((Transform) text).InverseTransformPoint(((Component) board).transform.TransformPoint((bounds2).center)), Vector3.zero);
      for (int index = 0; index < 8; ++index)
        (bounds1).Encapsulate(((Transform) text).InverseTransformPoint(((Component) board).transform.TransformPoint(((bounds2).center + Vector3.Scale((bounds2).extents, new Vector3((index & 1) == 0 ? -1f : 1f, (index & 2) == 0 ? -1f : 1f, (index & 4) == 0 ? -1f : 1f))))));
    }
    primitive.transform.localPosition = new Vector3((bounds1).center.x, (bounds1).center.y, Mathf.Max(num1, (bounds1).min.z - num1));
    primitive.transform.localScale = new Vector3((bounds1).size.x * 0.985f, (bounds1).size.y * 0.985f, 1f);
    if (((Object) board != (Object) null))
      primitive.transform.SetParent(((Component) board).transform, true);
    Renderer component2 = primitive.GetComponent<Renderer>();
    component2.sharedMaterial = material;
    return component2;
  }
  public static bool Matches(MeshRenderer renderer, Transform text)
  {
    Mesh sharedMesh = ((Component) renderer).GetComponent<MeshFilter>()?.sharedMesh;
    if (!((Object) sharedMesh == (Object) null))
    {
      Bounds bounds1 = ((Renderer) renderer).bounds;
      Vector3 size1 = (bounds1).size;
      if ((double) (size1).sqrMagnitude <= 128.0)
      {
        Bounds bounds2 = ((Renderer) renderer).bounds;
        if ((double) (bounds2).SqrDistance(text.position) <= 0.25)
        {
          Bounds bounds3 = sharedMesh.bounds;
          Vector3 size2 = (bounds3).size;
          int num1 = (double) size2.x >= (double) size2.y ? 1 : 0;
          if ((double) size2.z < (double) (size2)[num1])
            num1 = 2;
          int num2 = (num1 + 1) % 3;
          int num3 = (num1 + 2) % 3;
          if ((double) (size2)[num2] <= 0.0 || (double) (size2)[num3] <= 0.0 || (double) (size2)[num1] > (double) Mathf.Min((size2)[num2], (size2)[num3]) * 0.10000000149011612)
            return false;
          Vector3 zero = Vector3.zero;
          (zero)[num1] = 1f;
          Matrix4x4 localToWorldMatrix = ((Component) renderer).transform.localToWorldMatrix;
          Matrix4x4 matrix4x4 = (localToWorldMatrix).inverse;
          matrix4x4 = (matrix4x4).transpose;
          Vector3 vector3 = (matrix4x4).MultiplyVector(zero);
          return (double) Mathf.Abs(Vector3.Dot((vector3).normalized, text.forward)) >= 0.89999997615814209;
        }
      }
    }
    return false;
  }
}
