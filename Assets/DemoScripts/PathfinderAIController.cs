using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class PathfinderAIController : BezierAIController
{
  [SerializeField] Node start;
  [SerializeField] Node end;
  // Start is called before the first frame update
  new void Start()
  {
    character = GetComponent<ThirdPersonCharacter>();
    path = VehiclePathfinding.Pathfinder.ShortestPath(NodeNetCreator.mainNet, end, start);
    
    previousSpeeds = new List<float>(speedSmoothFactor);
  }

  // Update is called once per frame
  void Update()
  {
    base.Update();
  }

  protected override void EndCheck()
  {
    if (Vector3.SqrMagnitude(path.LastNode.Pos - target.Pos) < 0.5f)
    {
      if (path.LastNode == path.LastNode)
        finished = true;
    }
  }
}
