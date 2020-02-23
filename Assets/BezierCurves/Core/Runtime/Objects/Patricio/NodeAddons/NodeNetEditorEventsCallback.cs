using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using PathCreation.Utility;

public static class NodeNetEditorEventsCallback
{
  public static void OnNodeCreated(Node n)
  {

  } 

  public static void OnStretchCreated(Stretch st)
  {
    //EvaluateIntersection(st.AnchorA);
    //EvaluateIntersection(st.AnchorB);
  }

  private static void EvaluateIntersection(Node n)
  {
    if (n.GetNodeComponent<IntersectionEntrance>() == null)
    {
      Node[] neighbours = NodeNetCreator.mainNet.GetAllNeighbours(n);

      List<Node> intersectionMembers = new List<Node>(neighbours.Length);
      if (neighbours.Length >= 3)
      {
        //Por cada vecino
        for (int i = 0; i < neighbours.Length; i++)
        {
          Node[] nNeighbours = NodeNetCreator.mainNet.GetAllNeighbours(neighbours[i]);
          int commonNeighbours = 0;
          for (int j = 0; j < nNeighbours.Length; j++)
          {
            if (NodeNetCreator.mainNet.AreNeighbours(n, nNeighbours[j]))
            {
              commonNeighbours++;
            }
          }
          if (commonNeighbours == 2)
            intersectionMembers.Add(neighbours[i]);
        }
        if (intersectionMembers.Count == 3)
        {
          intersectionMembers.Add(n);
          Stretch[] stretches = NodeNetCreator.mainNet.GetAllStretchesBetweenNodes(intersectionMembers.ToArray());
          Stretch[] straightStretches = FindCrossSectionsInIntersection(stretches);
          IntersectionEntrance a = straightStretches[0].AnchorA.AddNodeComponent<IntersectionEntrance>();
          IntersectionEntrance b = straightStretches[0].AnchorB.AddNodeComponent<IntersectionEntrance>();
          IntersectionEntrance c = straightStretches[1].AnchorA.AddNodeComponent<IntersectionEntrance>();
          IntersectionEntrance d = straightStretches[1].AnchorB.AddNodeComponent<IntersectionEntrance>();
          a.transitable = b.transitable = true;
          c.transitable = d.transitable = false;
        }
      }
    }    
  }

  private static Stretch[] FindCrossSectionsInIntersection(Stretch[] stretches)
  {
    Stretch[] sts = new Stretch[2];
    if(stretches.Length == 6)
    {
      stretches.OrderBy(stretch => CubicBezierUtility.AproximateAmountOfCurvature(stretch.GetPoints()));
      sts[0] = stretches[0];
      sts[1] = stretches[1];
    }
    return sts;
  }
}
