using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Node))]
[CanEditMultipleObjects]
public class NodeEditor : Editor
{
  public override void OnInspectorGUI()
  {
    Node[] selectedNodes = Selection.GetFiltered<Node>(SelectionMode.Unfiltered);
    if (selectedNodes.Length > 1)
    {
      if (GUILayout.Button("Create Intersection"))
      {
        NodeNetCreator.mainNet.CreateIntersection(selectedNodes);
      }
    }
  }
}
