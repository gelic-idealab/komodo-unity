//#define TESTING_BEFORE_BUILDING

using System;
using UnityEngine;
using WebXR;
using UnityEngine.UI;
using Komodo.Runtime;

/// <summary>
/// Switches menu between XR mode and screen mode.
/// </summary>
public class ToggleMenuDisplayMode : MonoBehaviour
{
    //[Header("Menu Canvas To Move")]
    //public Canvas menuCanvas;

    //public HoverCursor cursor;

    //used to turn off our background image of our UI according to what mode one is in -> allows for ghost cursos when pointing at UI without turning on laser

    //Get references for our UI
    private UIManager uiManager;
    public void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING
        WebXRManager.OnXRChange += ToggleMode;
#else 
        WebXRManagerEditorSimulator.OnXRChange += ToggleMode;
#endif
        uiManager = UIManager.Instance;
    }

    private void ToggleMode(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
    {
        if (state == WebXRState.VR)
        {
            SetVRViewPort();
        }
        else if(state == WebXRState.NORMAL)
        {
            SetDesktopViewport();
        }
    }

    [ContextMenu("Set to VR Mode")]
    public void SetVRViewPort() {

        if (!UIManager.IsAlive)
        {
            Debug.LogWarning("UIManager is not alive; called by SetVRViewPort()");
            return;

        }
        uiManager.EnableCursor();
        //TODO: One of the above actually does the job. Which is it?

        uiManager.PlaceMenuOnCurrentHand();

        uiManager.ConvertMenuToAlwaysExpanded();

        uiManager.EnableCreateMenu(true);

        uiManager.HeightCalibrationButtonsSettings(true);

        uiManager.EnableInstructorMenuButton(false);

        uiManager.EnableIgnoreLayoutForVRmode(false);

    }

    [ContextMenu("Set to Desktop Mode")]
    public void SetDesktopViewport()
    {
        if (!UIManager.IsAlive)
        {
           Debug.LogWarning("UIManager is not alive; called by SetDesktopViewport()");

           return;
        }

        uiManager.SwitchMenuToDesktopMode();
    }
}
