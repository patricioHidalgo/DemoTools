/*PATRICIO HIDALGO SANCHEZ. 2019*/

using System.Linq;
using UnityEngine;
using UnityEditor;
using PathCreationEditor;

[CustomEditor(typeof(NodeNetCreator))]
public class NodeNetEditor : Editor
{
  NodeNetCreator creator;

  [SerializeField]
  Node selectedNode;
  bool transformingNode = false;

  float lastClickTime = 0f;

  Stretch selectedStretch = null;
  Stretch.CurveSide selectedControlPointSide;

  NetDisplaySettings netDisplaySettings;
  Editor netDisplaySettingsEditor;
  bool netDisplaySettingsFoldout;
  GUIStyle labelStyle;

  enum EMirrorMode
  {
    ALL,
    PAIRS,
    NONE
  }


  #region DRAW_METHODS
  //Updated
  private void DrawNodeNet(ref bool elementClicked)
  {
    //Nodes
    for (int i = 0; i < creator.NNodes; i++)
    {
      DrawNode(creator.GetNode(i), ref elementClicked);
      DrawNodeID(i);
    }
    //Stretches
    for (int i = 0; i < creator.NStretches; i++)
    {
      Stretch st = creator.GetStretch(i);
      Vector3[] points = st.GetPoints();

      DrawHandler(st, ref elementClicked);

      DrawStretch(points);
      if (creator.displayNet)
        DrawBezier(points, netDisplaySettings, st.IsIntersection());
      DrawDeleteButton(st);
      if(netDisplaySettings.showArrows)
        DrawArrow(st);
      if (netDisplaySettings.showVertices)
        DrawVertices(st);
    }
  }

  private void DrawStretch(Vector3[] points)
  {
    Handles.DrawLine(points[0], points[1]);
    Handles.DrawLine(points[1], points[2]);
    Handles.DrawLine(points[2], points[3]);
  }

  private static void DrawBezier(Vector3[] points, NetDisplaySettings settings, bool isIntersection)
  {
    Color c = isIntersection ? Color.blue : settings.curveColor;
    Handles.DrawBezier(points[0], points[3], points[1], points[2], c, null, settings.curveThickness);
  }


  private void DrawNode(Node node, ref bool elementClicked)
  {
    float radius = netDisplaySettings.nodeRadius;
    Handles.color = netDisplaySettings.nodeColor;

    if (node == selectedNode && transformingNode)
    {
      EditorGUI.BeginChangeCheck();
      Vector3 newPos = Handles.PositionHandle(node.Pos, Quaternion.identity);

      if (EditorGUI.EndChangeCheck())
      {
        Undo.RecordObject(node.transform, "node moved");
        node.Pos = newPos;
        foreach (Stretch st in creator.GetStretches(node))
        {
          st.OnPathModified();
        }

      }
    }
    else
    {
      if (Handles.Button(node.Pos, Quaternion.identity, radius, radius, Handles.SphereHandleCap))
      {
        //Delete node
        if (Event.current.modifiers == EventModifiers.Control)
        {
          if (selectedNode == node)
            selectedNode = null;
          creator.DeleteNode(node);
        }
        //Select node to move
        else
        {
          if (selectedNode == node)
          {
            float clickTime = Time.time;
            if ((clickTime - lastClickTime) < 0.2f)
            {
              transformingNode = true;
              selectedStretch = null;
            }
          }
          if (selectedNode != null && !transformingNode)
          {
            Undo.RecordObject(creator, "Stretch added");
            Stretch st = creator.CreateStretch(selectedNode, node);
          }
          selectedStretch = null;
          selectedNode = node;
          elementClicked = true;

          lastClickTime = Time.time;
        }
      }
    }
  }

  private void DrawHandler(Stretch st, ref bool elementClicked)
  {
    float buttonRadius = netDisplaySettings.handlerRadius;

    Handles.color = netDisplaySettings.handlerColor;

    if (selectedStretch == st)
      MoveHandler(st, selectedControlPointSide, ref elementClicked);
    else
    {
      if (Handles.Button(st.ControlA, Quaternion.identity, buttonRadius, buttonRadius, Handles.SphereHandleCap))
      {
        selectedStretch = st;
        selectedControlPointSide = Stretch.CurveSide.A;
        selectedNode = null;
        transformingNode = false;
        elementClicked = true;
      }
      if (Handles.Button(st.ControlB, Quaternion.identity, buttonRadius, buttonRadius, Handles.SphereHandleCap))
      {
        selectedStretch = st;
        selectedControlPointSide = Stretch.CurveSide.B;
        selectedNode = null;
        transformingNode = false;
        elementClicked = true;
      }
    }
  }

  private void MoveHandler(Stretch st, Stretch.CurveSide side, ref bool elementClicked)
  {
    float buttonRadius = netDisplaySettings.handlerRadius;
    Handles.color = netDisplaySettings.handlerColor;

    Vector3 handlerInitialPos;
    Quaternion handlerRot = Quaternion.identity;
    Vector3 newPos;

    switch (side)
    {
      case Stretch.CurveSide.A:
        handlerInitialPos = st.ControlA;
        handlerRot = Quaternion.LookRotation(Camera.current.transform.position - handlerInitialPos);
        EditorGUI.BeginChangeCheck();
        newPos = Handles.PositionHandle(handlerInitialPos, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
          st.OnPathModified();
          Undo.RecordObject(creator, "Move ControlA");
          EditorUtility.SetDirty(creator);

          Vector3 oldPos = st.ControlA;
          st.ControlA = newPos;
          if(Event.current.shift)
            DoMirrorPairs(st, st.AnchorA, oldPos, newPos);
        }
        if (Handles.Button(st.ControlB, Quaternion.identity, buttonRadius, buttonRadius, Handles.SphereHandleCap))
        {
          selectedControlPointSide = Stretch.CurveSide.B;
          selectedNode = null;
          transformingNode = false;
          elementClicked = true;
        }
        break;
      case Stretch.CurveSide.B:
        handlerInitialPos = st.ControlB;
        handlerRot = Quaternion.LookRotation(Camera.current.transform.position - handlerInitialPos);
        EditorGUI.BeginChangeCheck();
        newPos = Handles.PositionHandle(handlerInitialPos, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
          st.OnPathModified();
          Undo.RecordObject(creator, "Move ControlB");
          EditorUtility.SetDirty(creator);

          Vector3 oldPos = st.ControlB;
          st.ControlB = newPos;
          if (Event.current.shift)
            DoMirrorPairs(st, st.AnchorB, oldPos, newPos);
        }
        if (Handles.Button(st.ControlA, Quaternion.identity, buttonRadius, buttonRadius, Handles.SphereHandleCap))
        {
          selectedControlPointSide = Stretch.CurveSide.A;
          selectedNode = null;
          transformingNode = false;
          elementClicked = true;
        }
        break;
    }

    Handles.DrawLine(st.ControlA, st.AnchorA.Pos);
    Handles.DrawLine(st.ControlB, st.AnchorB.Pos);
    SceneView.RepaintAll();
  }

  private void DrawDeleteButton(Stretch st)
  {
    Handles.color = netDisplaySettings.deleteStretchColor;
    float radius = netDisplaySettings.deleteSretchRadius;
    Vector3 pos = st.MidPoint;

    if (Handles.Button(pos, Quaternion.identity, radius, radius / 3, Handles.SphereHandleCap))
    {
      Undo.RegisterCompleteObjectUndo(creator, "Deleted Stretch");
      creator.DeleteStretch(st);
    }
  }

  private void DrawArrow(Stretch st)
  {
    Handles.color = netDisplaySettings.arrowColor;
    Handles.ConeHandleCap(0, st.GetPoint(0.5f) + Vector3.up, Quaternion.LookRotation(st.GetTangent(0.5f)), netDisplaySettings.arrowSize, EventType.Repaint);
  }

  private void DrawVertices(Stretch st)
  {
    Handles.color = Color.black;

    for (int v = 0; v < st.Vertices.Length; v++)
    {
      Vector3 vertex = st.Vertices[v];
      Handles.SphereHandleCap(1, vertex, Quaternion.identity, 0.2f, EventType.Repaint);
    }
  }
  private void DrawNetDisplaySettingsInspector()
  {
    using (var check = new EditorGUI.ChangeCheckScope())
    {
      if (creator.displayNet)
      {
        netDisplaySettingsFoldout = EditorGUILayout.InspectorTitlebar(netDisplaySettingsFoldout, netDisplaySettings);
        if (netDisplaySettingsFoldout)
        {
          CreateCachedEditor(netDisplaySettings, null, ref netDisplaySettingsEditor);
          netDisplaySettingsEditor.OnInspectorGUI();
        }
        if (check.changed)
        {
          UpdateNetDisplaySettings();
          SceneView.RepaintAll();
        }
      }
    }
  }

  private void DrawNodeID(int index)
  {
    Node n = creator.GetNode(index);
    if(n != null)
      Handles.Label(n.Pos + Camera.current.transform.up * 2, index.ToString(), netDisplaySettings.guiStyle);
  }
  #endregion

  #region UNITY_EDITOR
  private void OnEnable()
  {
    int n = FindObjectsOfType<NodeNetCreator>().Length;
    if (n > 1)
      DestroyImmediate(target);

    LoadNetDisplaySettings();

    creator = (NodeNetCreator)target;
    NodeNetCreator.mainNet = creator;

    Undo.undoRedoPerformed += OnUndoNodeDelete;

    selectedNode = null;
  }
  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();

    labelStyle.normal.textColor = netDisplaySettings.guiStyle.normal.textColor;

    GUILayout.Space(20);

    if (GUILayout.Button("Stick Nodes to Ground"))
    {
      StickNodesToGround();
    }

    if (GUILayout.Button("Recalculate Vertices"))
    {
      for (int i = 0; i < creator.NStretches; i++)
      {
        Stretch st = creator.GetStretch(i);
        st.OnPathModified();
      }
    }

    if(GUILayout.Button("Reassign Nodes"))
    {
      for (int i = 0; i < creator.NNodes; i++)
      {
        Node n = creator.GetNode(i);
        n.id = i;
        n.transform.name = "NODE_" + i;
      }
    }

    if(GUILayout.Button("Print curvature percentages"))
    {
      for (int i = 0; i < creator.NStretches; i++)
      {
        Stretch st = creator.GetStretch(i);
        Debug.Log("Stretch: " + st.Name + ": " + PathCreation.Utility.CubicBezierUtility.AproximateAmountOfCurvature(st.GetPoints()));
      }
    }

    GUILayout.Space(20);

    if(GUILayout.Button("Load from xml file"))
    {
      creator.LoadFromXml();
    }
    if (GUILayout.Button("Save to xml file"))
    {
      creator.SaveToXml();
    }

    GUILayout.Space(20);

    if(selectedNode != null)
    {
    }

    DrawNetDisplaySettingsInspector();
  }
  public void OnSceneGUI()
  {
    Event e = Event.current;
    bool elementClicked = false;
    if (creator.displayNet)
    {
      #region ProcessClickEvent
      DrawNodeNet(ref elementClicked);

      if (e.type == EventType.MouseDown && e.button == 0 && !elementClicked)
      {
        if(!e.shift)
          selectedNode = null;
        transformingNode = false;
        selectedStretch = null;

        if (e.shift)
        {
          Vector3 mouseClickPos = MouseUtility.MouseToFloorRaycast();

          Undo.RecordObject(creator, "stretch created");
          Node newNode = creator.CreateNode(mouseClickPos);

          if (selectedNode != null)
          {
            Undo.RecordObject(creator, "stretch created");
            creator.CreateStretch(selectedNode, newNode);
          }

          Undo.RegisterCreatedObjectUndo(newNode.gameObject, "node created" + newNode.name);
          selectedNode = newNode;
        }
        else
          selectedNode = null;
      }

      else if (e.type == EventType.Layout)
      {
        HandleUtility.AddDefaultControl(0);
      }
      #endregion

      if (selectedNode != null && !transformingNode)
      {
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        Vector3 rayEnd = mouseRay.GetPoint(Camera.current.nearClipPlane);
        Handles.DrawLine(selectedNode.Pos, rayEnd);
        SceneView.RepaintAll();
      }
    }
  }
  public void OnDisable()
  {
    Undo.undoRedoPerformed -= OnUndoNodeDelete;
  }
  #endregion

  #region SETTINGS_MANAGEMENT
  void LoadNetDisplaySettings()
  {
    netDisplaySettings = NetDisplaySettings.Load();
    labelStyle = netDisplaySettings.guiStyle;
  }
  void UpdateNetDisplaySettings()
  {
    var nds = netDisplaySettings;
    if (labelStyle == null)
      labelStyle = new GUIStyle(nds.guiStyle);
  }
  #endregion

  private void StickNodesToGround()
  {
    Vector3 rayStartOffset = new Vector3(0, 2, 0);
    for (int i = 0; i < creator.NNodes; i++)
    {
      Node n = creator.GetNode(i);
      Ray nodeRay = new Ray(n.Pos + rayStartOffset, Vector3.down);

      RaycastHit hit;

      if (Physics.Raycast(nodeRay, out hit, Mathf.Infinity))
      {
        n.Pos = hit.point;
      }
    }
  }

  private void OnUndoNodeDelete()
  {
    for (int i = 0; i < creator.NStretches; i++)
    {
      Stretch st = creator.GetStretch(i);
      st.OnPathModified();
    }
  }

  //private void DoMirror(Stretch st, Node n, Vector3 previousPos, Vector3 newPos)
  //{
  //  Vector3 a = previousPos - n.transform.position;
  //  Vector3 b = newPos - n.transform.position;
  //  Vector3 axis = Vector3.Cross(a, b);
  //  float deltaAngle = Vector3.SignedAngle(a, b, axis);
  //  foreach (Stretch s in creator.GetStretches(n))
  //  {
  //    if (s != st)
  //    {
  //      switch (s.IsAnchorA(n))
  //      {
  //        //Is ControlA
  //        case true:
  //          s.ControlA = Vector3Extension.RotatePointAroundAxisPivot(s.ControlA, n.transform.position, axis, deltaAngle);
  //          break;
  //        //Is ControlB
  //        case false:
  //          s.ControlB = Vector3Extension.RotatePointAroundAxisPivot(s.ControlB, n.transform.position, axis, deltaAngle);
  //          break;
  //        default:
  //          Debug.Assert(true);
  //          break;
  //      }
  //      s.OnPathModified();
  //    }
  //  }
  //}

  private void DoMirrorPairs(Stretch st, Node n, Vector3 previousPos, Vector3 newPos)
  {
    Stretch[] stretches = creator.GetStretches(n);
    if (stretches.Length == 2)
    {
      //Vector3 a = previousPos - n.transform.position;
      //Vector3 b = newPos - n.transform.position;
      //Vector3 axis = Vector3.Cross(a, b);
      //float deltaAngle = Vector3.SignedAngle(a, b, axis);

      for (int i = 0; i < stretches.Length; i++)
      {
        Stretch s = stretches[i];

        if (s != st)
        {
          if (s.IsAnchorA(n))
          {
            s.ControlA = Vector3Extension.ReflectWithPoint(newPos, n.Pos);
          }
          else
          {
            s.ControlB = Vector3Extension.ReflectWithPoint(newPos, n.Pos);
          }
        }
      }
    }
  }
}
