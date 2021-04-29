//#define TESTING_BEFORE_BUILDING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Events;
using WebXR;

public class WebXRManagerEditorSimulator : MonoBehaviour
{
    public delegate void XRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect);

    public static event XRChange OnXRChange;
    private bool previousIsVRActive = false;

#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING
//do nothing
#else
    void Update () {
        bool isVRActive = XRSettings.isDeviceActive; //tells us whether the device is attached (not necessarily if it is being worn or used.)

        if (isVRActive && !previousIsVRActive) {
            Debug.Log("VR Headset detected.");
            OnXRChange.Invoke(WebXRState.VR, 2, new Rect(), new Rect());
        }

        if (!isVRActive && previousIsVRActive) {
            Debug.Log("VR Headset no longer detected.");
            OnXRChange.Invoke(WebXRState.NORMAL, 1, new Rect(), new Rect());
        }
        
        previousIsVRActive = isVRActive;
    }
#endif
}
