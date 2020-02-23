using UnityEngine;

public class OrientedPoint
{
  public Vector3 Pos;
  public Quaternion QDir;

  public Vector3 Dir
  {
    get
    {
      return QDir * Vector3.forward;
    }
  }
  public Vector3 Right
  {
    get
    {
      return QDir * Vector3.right;
    }
  }

  public OrientedPoint(Vector3 pos, Vector3 dir)
  {
    Pos = pos;
    this.QDir = Quaternion.LookRotation(dir);
  }
  public OrientedPoint(Vector3 pos, Quaternion dir)
  {
    Pos = pos;
    this.QDir = dir;
  }
}
