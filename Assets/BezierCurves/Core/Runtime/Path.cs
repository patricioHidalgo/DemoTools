using PathCreation.Utility;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
  public List<Node> nodes = new List<Node>();
  private List<bool> forward = new List<bool>();
  private NodeNetCreator net;

  #region PROPERTIES
  private float _totalLength;
  public float TotalLength
  {
    get
    {
      _totalLength = 0f;
      for (int i = 0; i < NStretches; i++)
      {
        _totalLength += GetNStretch(i).GetLength();
      }
      return _totalLength;
    }
  }

  public Node LastNode
  {
    get
    {
      return nodes[nodes.Count - 1];
    }
  }

  public int NStretches
  {
    get
    {
      return nodes.Count - 1;
    }
  }
  #endregion

  #region CONSTRUCTORS
  public Path(List<Node> nodes, NodeNetCreator net)
  {
    this.nodes = nodes;
    this.net = net;
    forward = new List<bool>(nodes.Count - 1);
    for (int i = 0; i < nodes.Count - 1; i++)
    {
      Node a = nodes[i];
      Node b = nodes[i + 1];

      Stretch st = net.GetStretch(a, b);
      forward.Add(st.AnchorA == a);
    }
  }
  public Path(Node initialNode, NodeNetCreator net)
  {
    this.net = net;
    nodes.Add(initialNode);
    forward = new List<bool>();
  }
  public Path(Stretch st, Vector3 carForward, Vector3 carPos, NodeNetCreator net)
  {
    this.net = net;
    Vector3 carToA = (st.AnchorA.Pos - carPos).normalized;
    Vector3 carToB = (st.AnchorB.Pos - carPos).normalized;

    Node first = null;
    Node second = null;

    if (Vector3.Dot(carForward, carToA) > Vector3.Dot(carForward, carToB))
    {
      first = st.AnchorB;
      second = st.AnchorA;
    }
    else
    {
      first = st.AnchorA;
      second = st.AnchorB;
    }

    nodes.Add(first);
    AddNode(second);
  }
  #endregion

  #region GETTERS
  public Stretch GetNStretch(int n)
  {
    if (nodes.Count < 2)
      Debug.LogError("This path has not a single stretch");
    if (n > nodes.Count - 1)
    {
      Debug.LogError("This path doesnt have a " + n + " stretch");
      return null;
    }
    return net.GetStretch(nodes[n], nodes[n + 1]);
  }

  public bool IsStretchNForward(int n)
  {
    return forward[n];
  }

  public OrientedPoint GetOrientedPointAtDistance(float d)
  {
    int i = GetStretchAtDistance(ref d);
    Stretch st = GetNStretch(i);
    OrientedPoint p = st.GetOrientedPointAtDistance(d);

    if (!IsStretchNForward(i))
      p = new OrientedPoint(p.Pos, -p.Dir);

    return p;
  }

  //public Vector3 ProjectOntoPath(Vector3 worldPos)
  //{
  //  Vector3 point = Vector3.zero;
  //  float minDst = float.MaxValue;
  //  //Stretch st = null;
  //  //float t = ClosestPointT(worldPos, out st);

  //  for (int i = 0; i < nodes.Count - 1; i++)
  //  {
  //    Stretch st = GetNStretch(i);
  //    Vector3 closestPoint = st.GetClosestPointOnStretch(worldPos);
  //    float dst = Vector3.SqrMagnitude(closestPoint - worldPos);
  //    if (dst < minDst)
  //    {
  //      point = closestPoint;
  //      minDst = dst;
  //    }
  //  }
  //  return point;
  //  //return st.GetPoint(t);
  //}

  public float GetClosestTValue(Vector3 worldPos, ref int stretchIndex)
  {
    float closestPoint = 0f;
    float minDst = float.MaxValue;
    Stretch st;
    int stIndex = 0;
    for (int i = 0; i < nodes.Count - 1; i++)
    {
      st = GetNStretch(i);
      float cp = st.ClosestPointT(worldPos);
      float dst = Vector3.SqrMagnitude(worldPos - st.GetPoint(cp));
      if (dst < minDst)
      {
        stIndex = i;
        closestPoint = cp;
        minDst = dst;
      }
      else
        break;
    }

    return closestPoint;
  }

  public OrientedPoint GetProjectionAndTangent(Vector3 worldPos, ref int st, ref int index)
  {
    int initialStretch = st;
    OrientedPoint orientedPoint = new OrientedPoint(Vector3.zero, Quaternion.identity);
    float minDst = float.MaxValue;

    for (int i = st; i < NStretches; i++)
    {
      Stretch s = GetNStretch(i);
      bool fwd = IsStretchNForward(i);

      if (index == -1)
      {
        if (fwd)
          index = 0;
        else
          index = s.Vertices.Length - 1;
      }

      int temp = index;
      if (st != initialStretch)
        temp = fwd ? 0 : s.Vertices.Length - 1;

      OrientedPoint op = s.GetClosestOrientedPoint(worldPos, fwd, ref temp);
      float dst = Vector3.SqrMagnitude(op.Pos - worldPos);
      if (dst < minDst)
      {
        orientedPoint = op;
        minDst = dst;
        st = i;
        index = temp;
      }
      else
        break;
    }
    return orientedPoint;
  }

  public OrientedPoint GetProjectionAndTangent(Vector3 worldPos)
  {
    OrientedPoint orientedPoint = new OrientedPoint(Vector3.zero, Quaternion.identity);
    float minDst = float.MaxValue;

    for (int i = 0; i < NStretches; i++)
    {
      Stretch s = GetNStretch(i);
      bool fwd = IsStretchNForward(i);

      OrientedPoint op = s.GetClosestOrientedPoint(worldPos, fwd);
      float dst = Vector3.SqrMagnitude(op.Pos - worldPos);
      if (dst < minDst)
      {
        orientedPoint = op;
        minDst = dst;
      }
      else
        break;
    }
    return orientedPoint;
  }
  #endregion

  #region MODIFIERS
  public void AddNode(Node node)
  {
    Node last = LastNode;
    if (nodes.Count == 0)
    {
      nodes.Add(node);
    }
    else
    {
      if (net.GetStretch(node, last) != null)
      {
        nodes.Add(node);
        forward.Add(net.GetStretch(last, node).IsAnchorA(last));
      }
    }
  }

  public void AddRamdomNeighbour()
  {
    Node n = null;
    if (nodes.Count >= 2)
      n = net.GetRandomNeighbour(LastNode, nodes[nodes.Count - 2]);
    else
      n = net.GetRandomNeighbour(LastNode, null);

    if (n != null)
      AddNode(n);
  }

  public void RemoveFirst()
  {
    nodes.RemoveAt(0);
    forward.RemoveAt(0);
  }
  #endregion

  #region INTERNAL
  /// <summary>
  /// Receive an absolute distance along the Path. Returns the Stretch where it lands and
  /// adjust the distance relative to the start of that Stretch. Also reverses d 
  /// depending on the Forward value of the Stretch
  /// </summary>
  /// <param name="d"></param>
  /// <returns></returns>
  private int GetStretchAtDistance(ref float d)
  {
    d = Mathf.Clamp(d, 0f, TotalLength);
    float previousStretchesTotalLength = 0f;

    for (int i = 0; i < NStretches; i++)
    {
      Stretch st = GetNStretch(i);
      if (st.GetLength() >= d)
      {
        d -= previousStretchesTotalLength;
        if (!IsStretchNForward(i))
          d = st.GetLength() - d;
        return i;
      }
      else
      {
        previousStretchesTotalLength += st.GetLength();
      }
    }

    return (NStretches - 1);
  }
  #endregion

  public void DrawPath()
  {
    Node previous = nodes[0];
    Node current;
    for (int i = 1; i < nodes.Count; i++)
    {
      current = nodes[i];
      Debug.DrawLine(previous.transform.position, current.transform.position, Color.green);
      previous = current;
    }
  }
}