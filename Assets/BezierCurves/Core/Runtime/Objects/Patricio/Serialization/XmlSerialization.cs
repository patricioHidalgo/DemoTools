using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;

public class XmlDeserialization
{
  public XmlDeserialization(/*string path, */NodeNetCreator net)
  {
    string path = EditorUtility.OpenFilePanel("Open Nodenet xml", Application.dataPath, "xml");

    if(File.Exists(path))
    {
      XmlDocument doc = new XmlDocument();
      FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

      doc.Load(fs);

      //"net" node
      XmlNode netNode = doc.ChildNodes[1];

      XmlNode nodesNode = netNode.FirstChild;

      XmlNode stretchesNode = netNode.LastChild;

      int nNodes = nodesNode.ChildNodes.Count;
      int nStretches = stretchesNode.ChildNodes.Count;

      net.DeleteAllNodes();

      List<Stretch> stretches = new List<Stretch>(nStretches);

      //Nodes
      for (int n = 0; n < nNodes; n++)
      {
        XmlNode node = nodesNode.ChildNodes[n];

        float x = float.Parse(node.Attributes["x"].Value);
        float y = float.Parse(node.Attributes["y"].Value);
        float z = float.Parse(node.Attributes["z"].Value);

        net.CreateNode(new Vector3(x, y, z));
      }

      //Stretches
      for (int s = 0; s < nStretches; s++)
      {
        XmlNode stretch = stretchesNode.ChildNodes[s];

        int anchorA = int.Parse(stretch.Attributes["a"].Value);
        int anchorB = int.Parse(stretch.Attributes["b"].Value);

        Vector3 controlARelativePos;
        Vector3 controlBRelativePos;

        //ControlA
        {
          XmlNode controlA = stretch.ChildNodes[0];

          float x = float.Parse(controlA.Attributes["x"].Value);
          float y = float.Parse(controlA.Attributes["y"].Value);
          float z = float.Parse(controlA.Attributes["z"].Value);

          controlARelativePos = new Vector3(x, y, z);
        }
        //ControlB
        {
          XmlNode controlB = stretch.ChildNodes[1];

          float x = float.Parse(controlB.Attributes["x"].Value);
          float y = float.Parse(controlB.Attributes["y"].Value);
          float z = float.Parse(controlB.Attributes["z"].Value);

          controlBRelativePos = new Vector3(x, y, z);
        }

        stretches.Add(new Stretch(anchorA, anchorB, controlARelativePos, controlBRelativePos));
      }

      net.stretches = stretches;

      fs.Close();
    }    
  }
}

public class XmlSerialization
{
  public XmlSerialization(NodeNetCreator net)
  {
    string path = EditorUtility.SaveFilePanel("Save Nodenet xml", Application.dataPath, "NewNodenet", "xml");
    XmlDocument doc = new XmlDocument();
    XmlElement root;
    XmlNode netNode;

    if (File.Exists(path))
    {    
      FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

      doc.Load(fs);

      root = doc.DocumentElement;
      root.RemoveAll();

      netNode = doc.ChildNodes[1];

      fs.Close();
    }
    else
    {
      XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
      doc.AppendChild(xmlDeclaration);

      netNode = doc.CreateElement(string.Empty, "net", string.Empty);
      doc.AppendChild(netNode);
    }

    XmlElement nodesNode = doc.CreateElement(string.Empty, "nodes", string.Empty);
    netNode.AppendChild(nodesNode);

    //Nodes
    for (int n = 0; n < net.NNodes; n++)
    {
      Node node = net.GetNode(n);
      Vector3 nodePos = node.Pos;
      XmlElement nodeNode = doc.CreateElement(string.Empty, "node", string.Empty);
      {
        nodeNode.SetAttribute("x", nodePos.x.ToString());
        nodeNode.SetAttribute("y", nodePos.y.ToString());
        nodeNode.SetAttribute("z", nodePos.z.ToString());
      }
      nodesNode.AppendChild(nodeNode);
    }



    XmlElement stretchesNode = doc.CreateElement(string.Empty, "stretches", string.Empty);
    netNode.AppendChild(stretchesNode);

    //Stretches
    for (int s = 0; s < net.NStretches; s++)
    {
      Stretch st = net.GetStretch(s);
      int anchorAIndex = st.anchorAIndex;
      int anchorBIndex = st.anchorBIndex;

      XmlElement stretchNode = doc.CreateElement(string.Empty, "stretch", string.Empty);
      {
        stretchNode.SetAttribute("a", anchorAIndex.ToString());
        stretchNode.SetAttribute("b", anchorBIndex.ToString());

        Vector3 controlA = st.ControlA;
        XmlElement controlANode = doc.CreateElement(string.Empty, "controlA", string.Empty);
        {
          controlANode.SetAttribute("x", controlA.x.ToString());
          controlANode.SetAttribute("y", controlA.y.ToString());
          controlANode.SetAttribute("z", controlA.z.ToString());
        }
        Vector3 controlB = st.ControlB;
        XmlElement controlBNode = doc.CreateElement(string.Empty, "controlB", string.Empty);
        {
          controlBNode.SetAttribute("x", controlB.x.ToString());
          controlBNode.SetAttribute("y", controlB.y.ToString());
          controlBNode.SetAttribute("z", controlB.z.ToString());
        }

        stretchNode.AppendChild(controlANode);
        stretchNode.AppendChild(controlBNode);

        stretchesNode.AppendChild(stretchNode);
      }
      
    }

    doc.Save(path);
    AssetDatabase.Refresh();
  }
}
