using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu]
public class NetDisplaySettings : ScriptableObject
{
  public GUIStyle guiStyle = new GUIStyle();

  [Space(10)]

  [Header("Info to display")]
  public bool showNodeIds = true;
  public bool showStretchesIds = true;
  public bool showVertices = false;
  public bool showLengths = false;
  public bool showArrows = false;

  [Space(10)]

  [Header("Display Options")]
  public float nodeRadius = 1f;
  public Color nodeColor = Color.green;
  [Space(5)]
  public float handlerRadius = 1f;
  public Color handlerColor = Color.white;
  [Space(5)]
  public float deleteSretchRadius = 1f;
  public Color deleteStretchColor = Color.red;
  [Space(5)]
  public float curveThickness = 1.5f;
  public Color curveColor = Color.green;
  [Space(5)]
  public float arrowSize = 1f;
  public Color arrowColor = Color.black;

#if UNITY_EDITOR
  public static NetDisplaySettings Load()
  {
    string[] guids = UnityEditor.AssetDatabase.FindAssets("t:NetDisplaySettings");
    if (guids.Length == 0)
    {
      Debug.LogWarning("Could not find DisplaySettings asset. Will use default settings instead.");
      return CreateInstance<NetDisplaySettings>();
    }
    else
    {
      string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
      return UnityEditor.AssetDatabase.LoadAssetAtPath<NetDisplaySettings>(path);
    }
  }
#endif
}
