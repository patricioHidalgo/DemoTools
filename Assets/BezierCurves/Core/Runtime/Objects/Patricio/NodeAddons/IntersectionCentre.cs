using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[System.Serializable]
public class IntersectionCentre : MonoBehaviour
{
  [System.Serializable]
  public struct EntrancesPair
  {
    public Node a;
    public Node b;

    public IntersectionEntrance aEntrance;
    public IntersectionEntrance bEntrance;

    public EntrancesPair(Node a, Node b)
    {
      this.a = a;
      this.b = b;

      aEntrance = this.a.GetComponent<IntersectionEntrance>();
      bEntrance = this.b.GetComponent<IntersectionEntrance>();
    }

    public static bool operator == (EntrancesPair first, EntrancesPair second)
    {
      if (first.a == second.a && first.b == second.b)
        return true;
      if (first.a == second.b && first.b == second.a)
        return true;
      return false;
    }
    public static bool operator != (EntrancesPair first, EntrancesPair second)
    {
      if (first.a == second.a && first.b == second.b)
        return false;
      if (first.a == second.b && first.b == second.a)
        return false;
      return true;
    }

    public void SetTransitable(bool transitable)
    {
      aEntrance.transitable = transitable;
      bEntrance.transitable = transitable;
    }

    public bool GetTransitable()
    {
      return aEntrance.transitable;
    }
  }

  [SerializeField]
  private List<EntrancesPair> pairs = new List<EntrancesPair>(2);

  [SerializeField]
  float switchTimer = 20f;

  private void Start()
  {
    pairs[0].SetTransitable(true);
    pairs[1].SetTransitable(false);

    InvokeRepeating("SwitchTrafficLight", switchTimer, switchTimer);
  }

  public void AddPair(Node a, Node b, IntersectionCentre ic)
  {
    if (!a.GetComponent<IntersectionEntrance>())
    {
      IntersectionEntrance ie = a.gameObject.AddComponent<IntersectionEntrance>();
      ie.centre = ic;
    }

    if (!b.GetComponent<IntersectionEntrance>())
    {
      IntersectionEntrance ie = b.gameObject.AddComponent<IntersectionEntrance>();
      ie.centre = ic;
    }

    EntrancesPair p = new EntrancesPair(a, b);
    if (!ExistPair(p))
    {
      pairs.Add(p);
    }
      
  }

  private bool ExistPair(EntrancesPair p)
  {
    foreach (var pair in pairs)
    {
      if (pair == p)
        return true;
    }
    return false;
  }

  private void SwitchTrafficLight()
  {
    pairs[0].SetTransitable(!pairs[0].GetTransitable());
    pairs[1].SetTransitable(!pairs[1].GetTransitable());
  }
}
