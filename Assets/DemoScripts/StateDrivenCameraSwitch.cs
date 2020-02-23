using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateDrivenCameraSwitch : MonoBehaviour
{
  [SerializeField] Animator anim;

  private void OnTriggerEnter(Collider other)
  {
    anim.SetInteger("CamIndex", anim.GetInteger("CamIndex") + 1);
  }
}
