using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class VirtualCameraControl : MonoBehaviour
{
    private static CinemachineVirtualCamera[] virtualCameras;
    void Start()
    {
        virtualCameras = GetComponentsInChildren<CinemachineVirtualCamera>();
    }
    public static void FocusOnCamera(int cameraIndex)
    {
        if(virtualCameras == null || cameraIndex >= virtualCameras.Length) return;
        
        foreach(CinemachineVirtualCamera cvc in virtualCameras) cvc.Priority = 0;
        virtualCameras[cameraIndex].Priority = 1;
    }

}
