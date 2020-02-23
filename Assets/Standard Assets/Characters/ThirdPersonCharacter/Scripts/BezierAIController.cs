using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

[RequireComponent(typeof(ThirdPersonCharacter))]
public class BezierAIController : MonoBehaviour
{
  protected ThirdPersonCharacter character;

  protected Path path;

  protected OrientedPoint closest;
  protected Vector3 p;
  protected OrientedPoint target;

  [Range(1, 20)]
  [SerializeField] protected float minSpeed = 0.5f;
  [Range(1, 20)]
  [SerializeField] protected float maxSpeed = 3f;

  [Range(0, 5)]
  [SerializeField] protected float curvatureInfluenceOnSpeed = 1f;

  [SerializeField] protected int speedSmoothFactor = 50;
  protected List<float> previousSpeeds;

  protected bool finished = false;


  // Start is called before the first frame update
  protected void Start()
  {
    character = GetComponent<ThirdPersonCharacter>();
    path = new Path(NodeNetCreator.mainNet.GetClosestNode(transform.position), NodeNetCreator.mainNet);
    path.AddRamdomNeighbour();
    path.AddRamdomNeighbour();

    previousSpeeds = new List<float>(speedSmoothFactor);
  }


  // Update is called once per frame
  protected void Update()
  {
    closest = path.GetProjectionAndTangent(transform.position);
    p = closest.Pos + closest.Dir * 2;
    target = path.GetProjectionAndTangent(p);

    SetSpeed(Vector3.SqrMagnitude(target.Pos - p));
    character.Move(target.Pos - transform.position, false, false);

    EndCheck();
  }

  protected virtual void EndCheck()
  {
    if (Vector3.SqrMagnitude(path.LastNode.Pos - target.Pos) < 0.5f)
    {
      Node ln = path.LastNode;
      path.AddRamdomNeighbour();
      if (path.LastNode == ln)
        finished = true;
      path.RemoveFirst();
    }
  }

  void SetSpeed(float curvature)
  {
    if (finished)
      character.speed = 0f;
    else
    {
      float speed = Mathf.Max(minSpeed, maxSpeed - curvature * curvatureInfluenceOnSpeed);
      float avgSpeed = GetAverageSpeed(speed);
      character.speed = avgSpeed;
    }
       
  }

  float GetAverageSpeed(float speed)
  {
    previousSpeeds.Add(speed);
    if(previousSpeeds.Count > speedSmoothFactor)
    {
      previousSpeeds.RemoveAt(0);
    }

    float s = 0;
    for (int i = 0; i < previousSpeeds.Count; i++)
    {
      s += previousSpeeds[i];
    }

    return s / previousSpeeds.Count;
  }

  private void OnDrawGizmos()
  {
    if(closest != null)
    {
      Gizmos.DrawSphere(closest.Pos, 0.4f);
      Gizmos.DrawSphere(target.Pos, 0.1f);
      Gizmos.DrawSphere(p, 0.2f);

      Gizmos.DrawLine(closest.Pos, p);
      Gizmos.DrawLine(p, target.Pos);
    }
    
  }
}
