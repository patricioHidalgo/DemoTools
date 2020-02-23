using UnityEngine;

public static class Vector3Extension
{

  public static Vector3 RotatePointAroundAxisPivot(this Vector3 point, Vector3 pivot, Vector3 axis, float angle)
  {
    Vector3 dir = point - pivot; // get point direction relative to pivot
    dir = Quaternion.AngleAxis(angle, axis) * dir; // rotate it
    point = dir + pivot; // calculate rotated point
    return point; // return it
  }

  /// <summary>
  /// Reflects point A with point B
  /// </summary>
  /// <param name="a"></param>
  /// <param name="b"></param>
  /// <returns></returns>
  public static Vector3 ReflectWithPoint(this Vector3 a, Vector3 b)
  {
    return b - (a - b);
  }
}
