using System.Collections.Generic;
using UnityEngine;

namespace VehiclePathfinding
{
  public static class Pathfinder
  {
    static DijkstraNode[] dNodes;

    private class DijkstraNode
    {
      public Node node;
      public DijkstraNode parent;
      public bool visited;
      public float distance;
      public bool startNode;
      public bool endNode;

      private DijkstraNode() { }
      public DijkstraNode(Node node, Node start, Node end)
      {
        this.node = node;
        visited = false;
        startNode = node == start ? true : false;
        endNode = node == end ? true : false;
        distance = startNode ? 0f : float.MaxValue;
        parent = null;
      }
    }

    private static void DikjstraInitialSetup(NodeNetCreator net, Node start, Node end, out DijkstraNode dStart, out DijkstraNode dEnd)
    {
      Node[] nodes = net.GetAllNodes();
      dNodes = new DijkstraNode[nodes.Length];
      dStart = null;
      dEnd = null;
      for (int i = 0; i < nodes.Length; i++)
      {
        dNodes[i] = new DijkstraNode(nodes[i], start, end);
        if (dNodes[i].startNode)
          dStart = dNodes[i];
        if (dNodes[i].endNode)
          dEnd = dNodes[i];
      }
    }

    private static void UpdateDistances(NodeNetCreator net, DijkstraNode current)
    {
      foreach (var dn in dNodes)
      {
        Stretch st = net.GetStretch(current.node, dn.node);
        if (st != null && !dn.visited)
        {
          float possibleNewDistance = current.distance + st.GetLength();
          if (possibleNewDistance < dn.distance)
          {
            dn.distance = possibleNewDistance;
            dn.parent = current;
          }
        }
      }
      current.visited = true;
    }

    private static DijkstraNode SetNewCurrentNode(NodeNetCreator net, DijkstraNode current)
    {
      DijkstraNode newNode = null;
      float minDst = float.MaxValue;
      foreach (var dn in dNodes)
      {
        //Si es vecino y todavia no ha sido visitado
        if(!dn.visited)
        {
          //Si su distancia es menor que la minima hasta ahora
          float dst = dn.distance;
          if (dst < minDst)
          {
            newNode = dn;
            minDst = dst;
          }
        }
      }
      return newNode;
    }

    private static void Dijkstra(NodeNetCreator net, DijkstraNode start, DijkstraNode end)
    {
      DijkstraNode currentNode = start;
      while (currentNode != end)
      {
        UpdateDistances(net, currentNode);
        currentNode = SetNewCurrentNode(net, currentNode);
      }
    }

    public static Path ShortestPath(NodeNetCreator net, Node start, Node end)
    {
      DijkstraNode dStart;
      DijkstraNode dEnd;
      DikjstraInitialSetup(net, start, end, out dStart, out dEnd);
      Dijkstra(net, dStart, dEnd);
      return ConstructPath(net, dStart, dEnd);
    }

    public static Path ShortestPath(NodeNetCreator net, Vector3 startPos, Vector3 endPos)
    {
      Node start = net.GetClosestNode(startPos);
      Node end = net.GetClosestNode(endPos);

      return ShortestPath(net, start, end);
    }

    //Construye un Path y le añade lo llena con los nodos ordenados del primero al destino
    private static Path ConstructPath(NodeNetCreator net, DijkstraNode dStart, DijkstraNode dEnd)
    {
      List<Node> nodesToAdd = new List<Node>();

      DijkstraNode nodeToAdd = dEnd;
      nodesToAdd.Add(nodeToAdd.node);

      do
      {
        nodeToAdd = nodeToAdd.parent;
        if(nodeToAdd != null)
          nodesToAdd.Add(nodeToAdd.node);
      } while (nodeToAdd != dStart);

      return new Path(nodesToAdd, net);
    }
  }
}