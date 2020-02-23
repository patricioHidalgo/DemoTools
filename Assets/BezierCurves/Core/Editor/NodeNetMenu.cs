using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeNetMenu : Editor
{
  [MenuItem("NodeNet/New NodeNet")]
  private static void NewNodeNet()
  {
    if (FindObjectOfType<NodeNetCreator>() == null)
    {
      GameObject nodeNet = new GameObject("NodeNet");
      nodeNet.transform.position = Vector3.zero;
      nodeNet.transform.rotation = Quaternion.identity;
      nodeNet.transform.localScale = Vector3.one;
      nodeNet.AddComponent<NodeNetCreator>();
      NodeNetCreator.mainNet = nodeNet.GetComponent<NodeNetCreator>();    
    }
    else
      Debug.Log("There is a NodeNet object in this scene");

    if (NodeNetCreator.mainNet == null)
      NodeNetCreator.mainNet = FindObjectOfType<NodeNetCreator>();
    Selection.activeGameObject = NodeNetCreator.mainNet.gameObject;
  }
}
