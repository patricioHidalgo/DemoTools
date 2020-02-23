//using System.Collections.Generic;
//using UnityEngine;

//namespace PathCreation {

//    public class PathCreator : MonoBehaviour {

//        /// This class stores data for the path editor, and provides accessors to get the current vertex and bezier path.
//        /// Attach to a GameObject to create a new path editor.

//        public event System.Action pathUpdated;

//        [SerializeField, HideInInspector]
//        NodeNetCreator nodeNetCreator;
//        [SerializeField, HideInInspector]
//        PathCreatorData editorData;
//        [SerializeField, HideInInspector]
//        bool initialized;

//        GlobalDisplaySettings globalEditorDisplaySettings;

//        // Vertex path created from the current bezier path
//        public VertexPath path {
//            get {
//                if (!initialized) {
//                    InitializeEditorData (false);
//                }
//                return editorData.vertexPath;
//            }
//        }

//        //NodeNetCreator this PathCreator belongs to
//        public NodeNetCreator NetCreator
//        {
//            get
//            {
//                if(!nodeNetCreator)
//                {
//                    nodeNetCreator = transform.GetComponentInParent<NodeNetCreator>();
//                    if (!nodeNetCreator)
//                    {
//                        Debug.LogError("The GameObject: " + gameObject.name + " contains a PathCreator but is not child of a NodeNetCreator. This could lead to errors");
//                        return null;
//                    }
//                }
//                return nodeNetCreator;
//            }
//            set
//            {
//                if (value != null)
//                    nodeNetCreator = value;
//                else
//                {
//                    Debug.LogError("The PathCreator in GameObject: " + gameObject.name + " is getting assigned an invalid value. Setting " + gameObject.name + " inactive.");
//                    gameObject.SetActive(false);
//                }
//            }
//        }

//        // The bezier path created in the editor
//        public BezierPath bezierPath {
//            get {
//                if (!initialized) {
//                    InitializeEditorData (false);
//                }
//                return editorData.bezierPath;
//            }
//            set {
//                if (!initialized) {
//                    InitializeEditorData (false);
//                }
//                editorData.bezierPath = value;
//            }
//        }


//        ~PathCreator()
//        {
//            Debug.Log("Destroyed: " + this.gameObject.name);
//        }

//        #region Internal methods

//        /// Used by the path editor to initialise some data
//        public void InitializeEditorData (bool in2DMode) {
//            if (editorData == null) {
//                editorData = new PathCreatorData ();
//            }
//            editorData.bezierOrVertexPathModified -= OnPathUpdated;
//            editorData.bezierOrVertexPathModified += OnPathUpdated;

//            int id;
//            int.TryParse(name.Substring(5, name.Length - 5), out id);

//            editorData.Initialize (NetCreator, id, transform.position, in2DMode);
//            initialized = true;
//        }

//        public PathCreatorData EditorData {
//            get {
//                return editorData;
//            }

//        }

//        void OnPathUpdated () {
//            pathUpdated?.Invoke();
//        }

//        public void DeleteEditorData()
//        {
//            bezierPath.EraseData();
//        }

//#if UNITY_EDITOR

//        // Draw the path when path objected is not selected (if enabled in settings)
//        void OnDrawGizmos () {

//            if (path != null) {

//                if (globalEditorDisplaySettings == null) {
//                    globalEditorDisplaySettings = GlobalDisplaySettings.Load ();
//                }

//                if (globalEditorDisplaySettings.alwaysDrawPath) {

//                    // Only draw path gizmo if the path object is not selected
//                    // (editor script is resposible for drawing when selected)
//                    GameObject selectedObj = UnityEditor.Selection.activeGameObject;
//                    if (selectedObj != gameObject) {
//                        Gizmos.color = globalEditorDisplaySettings.bezierPath;

//                        for (int i = 0; i < path.NumVertices; i++) {
//                            int nextI = i + 1;
//                            if (nextI >= path.NumVertices) {
//                                if (path.isClosedLoop) {
//                                    nextI %= path.NumVertices;
//                                } else {
//                                    break;
//                                }
//                            }
//                            Gizmos.DrawLine (path.vertices[i], path.vertices[nextI]);
//                        }
//                    }
//                }
//            }
//        }
//#endif

//        #endregion
//    }
//}