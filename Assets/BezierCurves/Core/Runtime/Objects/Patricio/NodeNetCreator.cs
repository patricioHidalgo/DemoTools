/*PATRICIO HIDALGO SANCHEZ. 2019*/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using PathCreation;
using PathCreation.Utility;

[System.Serializable]
public class NodeNetCreator : MonoBehaviour
{
  public bool displayNet = true;

  [HideInInspector]
  public List<Stretch> stretches = new List<Stretch>();
  //[HideInInspector]
  //public List<Node> nodes = new List<Node>(); 

  #region GETTERS
  public int NStretches
  {
    get
    {
      return stretches.Count;
    }
  }

  public int NNodes
  {
    get
    {
      return transform.childCount;
    }
  } 

  /// <summary>
  /// Searches for a <see cref="Stretch"/> connecting <see cref="Node"/> a and  <see cref="Node"/> b.
  /// </summary>
  /// <param name="a"></param>
  /// <param name="b"></param>
  /// <returns></returns>
  public Stretch GetStretch(Node a, Node b)
  {
    Stretch temp = new Stretch(IndexOfNode(a), IndexOfNode(b));

    foreach (Stretch s in stretches)
    {
      if (s.Equals(temp))
        return s;
    }

    return null;
  }

  public Stretch GetStretch(int index) { return stretches[index]; }

  public Node GetNode(int index)
  {
    if (index >= NNodes)
      return null;

    return transform.GetChild(index).GetComponent<Node>();
  }

  //public void RemakeNodesList()
  //{
  //  nodes = new List<Node>(transform.childCount);
  //  for (int c = 0; c < transform.childCount; c++)
  //  {
  //    nodes.Add(transform.GetChild(c).GetComponent<Node>());
  //  }
  //}

  public Node[] GetAllNodes()
  {
    Node[] nodes = new Node[NNodes];
    for (int n = 0; n < NNodes; n++)
    {
      nodes[n] = transform.GetChild(n).GetComponent<Node>();
    }
    return nodes.ToArray();
  }

  public bool AreNeighbours(Node a, Node b)
  {
    Stretch st = GetStretch(a, b);
    
    return st != null;
  }

  public Node[] GetAllNeighbours(Node node)
  {
    Node[] neighbours;
    Stretch[] sts = GetStretches(node);
    neighbours = new Node[sts.Length];
    for(int i = 0; i < sts.Length; i++)
    {
      neighbours[i] = sts[i].GetOppositeNode(node);
    }

    return neighbours;
  }

  public Stretch[] GetStretches(Node n)
  {
    return (from st in stretches
            where st.Contains(n)
            select st).ToArray();
  }

  public Stretch GetClosestStretch(Vector3 worldPos)
  {
    Debug.Log(Time.timeSinceLevelLoad);
    Stretch closestStretch = null;
    float minDistance = float.MaxValue;

    float dst = 0f;
    Stretch st = null;
    for (int i = 0; i < stretches.Count; i++)
    {
      st = GetStretch(i);

      dst = CubicBezierUtility.EstimateDistanceToCurve(st.GetPoints(), worldPos);

      if(dst < minDistance)
      {
        minDistance = dst;
        closestStretch = st;
      }
    }

    return closestStretch;
  }

  public Vector3 GetClosestPoint(Vector3 worldPos, bool includeIntersections = true)
  {
    float minDst = float.MaxValue;
    Vector3 closestPoint = Vector3.zero;

    Stretch st = null;
    for (int i = 0; i < stretches.Count; i++)
    {
      st = GetStretch(i);

      Vector3 p = st.GetClosestPointOnStretch(worldPos);
      float dst = (worldPos - p).sqrMagnitude;
      if (dst < minDst)
      {
        if (!includeIntersections)
        {
          if (!st.IsIntersection())
          {
            minDst = dst;
            closestPoint = p;
          }
        }
        else
        {
          minDst = dst;
          closestPoint = p;
        }
      }
    }

    return closestPoint;
  }

  public Vector3 GetClosestTangent(Vector3 worldPos)
  {
    Stretch st = GetClosestStretch(worldPos);
    return st.GetTangentAtClosestPoint(worldPos);
  }

  public Node GetClosestNode(Vector3 worldPos)
  {
    float minDistance = float.MaxValue;
    Node closestNode = null;

    for (int n = 0; n < NNodes; n++)
    {
      Node node = GetNode(n);
      float d = (node.Pos - worldPos).sqrMagnitude;
      if (d < minDistance)
      {
        closestNode = node;
        minDistance = d;
      }
    }

    return closestNode;
  }

  public Node GetRandomNeighbour(Node targetNode, Node previous)
  {
    Node neighbour = null;
    Stretch[] stretches = GetStretches(targetNode);

    if (stretches.Length == 0)
      return null;
    if (stretches.Length == 1 && previous != null)
      return null;

    while (neighbour == null)
    {
      int index = Random.Range(0, stretches.Length);
      Node n = stretches[index].GetOppositeNode(targetNode);

      if (previous == null)
      {
        neighbour = n;
      }
      else
      {
        if (previous != n && !AreNeighbours(n, previous))
          neighbour = n;
      }
    }

    return neighbour;
  }

  //private bool IsSuitableNeighbour(Node n, Node targetNode, Node previous)
  //{
  //  if (previous == null)
  //  {
  //    return true;
  //  }
  //  else if(previous != n && !AreNeighbours(n, previous))
  //  {   
  //      return true;
  //  }
  //  return false;
  //}

  //public Node GetRandomNeighbour(Node targetNode, Node previous)
  //{
  //  Node[] suitableNeighbours = (from n in GetAllNeighbours(targetNode)
  //                               where IsSuitableNeighbour(n, targetNode, previous)
  //                               select n).ToArray();

  //  return suitableNeighbours[Random.Range(0, suitableNeighbours.Length)];
  //}

  public Stretch[] GetAllStretchesBetweenNodes(Node[] nodes)
  {
    if (nodes == null || nodes.Length < 2)
      return null;

    int l = nodes.Length;

    List<Stretch> stretchesFound = new List<Stretch>(l*(l-1)/2);

    for (int i = 0; i < l; i++)
    {
      for (int j = i + 1; j < l; j++)
      {
        Stretch st = GetStretch(nodes[i], nodes[j]);
        if (st != null)
          stretchesFound.Add(st);
      }
    }

    return stretchesFound.ToArray();
  }
  #endregion

  #region CREATE&DELETE
  public Stretch CreateStretch(Node a, Node b)
  {
    if (a == null || b == null)
    {
      Debug.LogError("Trying to create a Stretch with one or two null gameobjects.");
      return null;
    }

    if (a == b)
    {
      return null;
    }

    if (GetStretch(a, b) == null)
    {
      Stretch st = new Stretch(IndexOfNode(a), IndexOfNode(b));
      stretches.Add(st);
      NodeNetEditorEventsCallback.OnStretchCreated(st);
      return st;
    }

    return null;
  }

  public Node CreateNode(Vector3 pos)
  {
    GameObject nodeObject = new GameObject("Node_" + NNodes);
    nodeObject.transform.parent = this.transform;
    Node n = nodeObject.AddComponent<Node>();
    n.Pos = pos;

    return n;
  }

  public void DeleteStretch(Stretch st)
  {
    stretches.Remove(st);
  }

  public void DeleteNode(Node n)
  {
    int index = IndexOfNode(n);
    Undo.RegisterCompleteObjectUndo(this, "stretches");
    stretches.RemoveAll(st => st.Contains(n));
    Undo.DestroyObjectImmediate(n.gameObject);  
    for (int i = 0; i < stretches.Count; i++)
    {
      stretches[i].NotifyDeleted(index);
    }
  }

  public void CreateIntersection(Node[] selectedNodes)
  {
    if (selectedNodes.Length == 4)
    {
      foreach (var n in selectedNodes)
      {
        IntersectionCentre i = n.gameObject.GetComponent<IntersectionCentre>();
        if (i != null)
          DestroyImmediate(i);
        IntersectionEntrance ie = n.gameObject.GetComponent<IntersectionEntrance>();
        if (ie != null)
          DestroyImmediate(ie);
      }

      IntersectionCentre ic = selectedNodes[0].gameObject.AddComponent<IntersectionCentre>();
      foreach (var n in selectedNodes)
      {
        Node farestNode = null;
        float maxDistance = float.MinValue;

        foreach (var nn in selectedNodes)
        {
          if (n != nn)
          {
            float d = Vector3.Distance(n.transform.position, nn.transform.position);
            if (d > maxDistance)
            {
              maxDistance = d;
              farestNode = nn;
            }
          }
        }

        ic.AddPair(n, farestNode, ic);
      }
    }
  }
  #endregion

  public int IndexOfNode(Node n)
  {
    for (int i = 0; i < NNodes; i++)
    {
      if (transform.GetChild(i).GetComponent<Node>().Equals(n))
        return i;
    }
    return -1;
  }

  public void DeleteAllNodes()
  {
    for (int n = NNodes - 1; n < -1; n++)
    {
      DestroyImmediate(transform.GetChild(n).gameObject);
    }
  }

  #region DATA_SAVE&LOAD
  public void LoadFromXml()
  {
    XmlDeserialization xml = new XmlDeserialization(this);
  }

  public void SaveToXml()
  {
    XmlSerialization xml = new XmlSerialization(this);
  }
  #endregion

  #region MONOBEHAVIOUR
  public static NodeNetCreator mainNet;
  private void Awake()
  {
    mainNet = this;
  }
  #endregion
}