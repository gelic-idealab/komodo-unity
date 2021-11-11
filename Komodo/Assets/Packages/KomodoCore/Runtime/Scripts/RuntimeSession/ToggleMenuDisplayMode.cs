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
    public void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING
        WebXRManager.OnXRChange += ToggleMode;
#else 
        WebXRManagerEditorSimulator.OnXRChange += ToggleMode;
#endif
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

        if (UIManager.IsAlive) 
        {
            UIManager.Instance.EnableCursor();
            //TODO: One of the above actually does the job. Which is it?
            
            UIManager.Instance.PlaceMenuOnCurrentHand();

            UIManager.Instance.ConvertMenuToAlwaysExpanded();

            UIManager.Instance.EnableCreateMenu();

            UIManager.Instance.EnableHightCalibrationButtons();
            
            return;
        }
    }


    [ContextMenu("Set to Desktop Mode")]
    public void SetDesktopViewport()
    {
        if (UIManager.IsAlive)
        {
            UIManager.Instance.DisableCursor();
            //TODO: One of the above actually does the job. Which is it?

            UIManager.Instance.menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            UIManager.Instance.ConvertMenuToExpandable(false);
            
            return;
        }
    }
}
