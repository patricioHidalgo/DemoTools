using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[DisallowMultipleComponent]
public class IntersectionEntrance : NodeComponent
{
  public bool transitable;

  public IntersectionCentre centre;

  private static float timer = 25f;

  private void Switch() { transitable = !transitable; }

  private void OnDrawGizmos()
  {
    Gizmos.color = transitable ? Color.green : Color.red;
    Gizmos.DrawSphere(node.Pos, 0.5f);
  }
}