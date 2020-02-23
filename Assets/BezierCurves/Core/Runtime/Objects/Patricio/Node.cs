using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node : MonoBehaviour
{
  public Vector3 Pos
  {
    get
    {
      return transform.position;
    }
    set
    {
      transform.position = value;
    }
  }

  public int id;

  private List<NodeComponent> components = new List<NodeComponent>();

  public T GetNodeComponent<T>()
    where T : NodeComponent
  {
    foreach (NodeComponent nc in components)
    {
      if (nc is T)
        return nc as T;
    }
    return null;
  }

  public T AddNodeComponent<T>()
    where T : NodeComponent, new()
  {
    T comp = gameObject.AddComponent<T>();
    components.Add(comp);
    return comp;
  }

  public override bool Equals(object obj)
  {
    var node = obj as Node;
    return node != null &&
           base.Equals(obj) &&
           Pos.Equals(node.Pos);
  }

  public override int GetHashCode()
  {
    return 991532785 + EqualityComparer<Vector3>.Default.GetHashCode(Pos);
  }

  public static bool operator ==(Node a, Node b)
  {
    if (ReferenceEquals(a, null))
    {
      if (ReferenceEquals(b, null))
        return true;

      return false;
    }
    if (ReferenceEquals(b, null))
    {
      if (ReferenceEquals(a, null))
        return true;

      return false;
    }

    Vector3 aPos = a.Pos;
    Vector3 bPos = b.Pos;

    if (aPos == bPos)
      return true;

    return false;
  }
  public static bool operator !=(Node a, Node b)
  {
    if (ReferenceEquals(a, null))
    {
      if (ReferenceEquals(b, null))
        return false;

      return true;
    }
    if (ReferenceEquals(b, null))
    {
      if (ReferenceEquals(a, null))
        return false;

      return true;
    }

    Vector3 aPos = a.Pos;
    Vector3 bPos = b.Pos;

    if (aPos == bPos)
      return false;

    return true;
  }
}

[System.Serializable]
public abstract class NodeComponent : MonoBehaviour
{
  private Node n;
  protected Node node
  {
    get
    {
      if (n == null)
        n = gameObject.GetComponent<Node>();
      return n;
    }
  }
}