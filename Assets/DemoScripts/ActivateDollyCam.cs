using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ActivateDollyCam : MonoBehaviour
{
  
  CinemachineTrackedDolly dolly;
    // Start is called before the first frame update
    void Start()
    {
    dolly = GetComponent<CinemachineTrackedDolly>();
    dolly.m_AutoDolly.m_Enabled = true; ;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
