using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Events;
using WebXR;

/// <summary>
/// Used to set off states when switing between Desktop and XR 
/// </summary>
/// we dont register our update loop because it only impacts editor
public class StateSwitchDependingOnVR : SingletonComponent<StateSwitchDependingOnVR>
{

    public UnityEvent _inVR_Event;
    public UnityEvent _outOfVR_Event;

    public bool isInVR;
    void Start()
    {
        _outOfVR_Event.Invoke();
        WebXRManager.Instance.OnXRChange += onXRChange;
    }
    private void onXRChange(WebXRState state)
    {

        if (state == WebXRState.ENABLED)
        {
            isInVR = true;
            _inVR_Event.Invoke();
        }
        else
        {
            isInVR = false;
            _outOfVR_Event.Invoke();
        }

    }
  //  public void OnDestroy() => WebXRManager.Instance.OnXRChange -= onXRChange;



#if UNITY_EDITOR || !UNITY_WEBGL
    void Update()
    {
        //only toggle when the device active state doesn't match the internal state
        if (XRSettings.isDeviceActive && !isInVR)
        {
            Debug.Log("Entered Headset.");
            isInVR = true;
            //WebXRManager.Instance.setXrState(WebXRState.ENABLED);
            _inVR_Event.Invoke();
            return;
        }

        if (!XRSettings.isDeviceActive && isInVR)
        {
            Debug.Log("Exited Headset.");
            isInVR = false;
            //WebXRManager.Instance.setXrState(WebXRState.NORMAL);
            _outOfVR_Event.Invoke();
        }
    }

    public void OnUpdate(float realTime)
    {
        //only toggle when the device active state doesn't match the internal state
        if (XRSettings.isDeviceActive && !isInVR)
        {
            Debug.Log("Entered Headset.");
            isInVR = true;
            //WebXRManager.Instance.setXrState(WebXRState.ENABLED);
            _inVR_Event.Invoke();
            return;
        }

        if (!XRSettings.isDeviceActive && isInVR)
        {
            Debug.Log("Exited Headset.");
            isInVR = false;
            //WebXRManager.Instance.setXrState(WebXRState.NORMAL);
            _outOfVR_Event.Invoke();
        }
    }


#endif
}

