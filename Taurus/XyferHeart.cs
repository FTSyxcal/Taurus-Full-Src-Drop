
using UnityEngine;
using UnityEngine.UI;
namespace Taurus;
public sealed class XyferHeart : Graphic
{
  protected override void OnPopulateMesh(VertexHelper vertices)
  {
    vertices.Clear();
    Rect rect1 = this.rectTransform.rect;
    Vector2 center = (rect1).center;
    vertices.AddVert(((Vector2)center), ((Color32)this.color), ((Vector4)Vector2.zero));
    for (int index = 0; index < 48 ; ++index)
    {
      float num1 = (float) ((double) index * 3.1415927410125732 * 2.0 / 48.0);
      float num2 = (float) (16.0 * (double) Mathf.Pow(Mathf.Sin(num1), 3f) / 32.0);
      float num3 = (float) ((13.0 * (double) Mathf.Cos(num1) - 5.0 * (double) Mathf.Cos(2f * num1) - 2.0 * (double) Mathf.Cos(3f * num1) - (double) Mathf.Cos(4f * num1) + 2.0) / 32.0);
      VertexHelper vertexHelper = vertices;
      Vector2 vector2_1 = center;
      double num4 = (double) num2;
      Rect rect2 = this.rectTransform.rect;
      double width = (double) (rect2).width;
      double num5 = num4 * width;
      double num6 = (double) num3;
      Rect rect3 = this.rectTransform.rect;
      double height = (double) (rect3).height;
      double num7 = num6 * height;
      Vector2 vector2_2 = new Vector2((float) num5, (float) num7);
      Vector3 vector3 = ((Vector2)(vector2_1 + vector2_2));
      Color32 color32 = ((Color32)this.color);
      Vector4 vector4 = ((Vector4)Vector2.zero);
      vertexHelper.AddVert(vector3, color32, vector4);
      vertices.AddTriangle(0, index + 1, (index + 1) % 48 + 1);
    }
  }
}
