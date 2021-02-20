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
    [Header("Menu Canvas To Move")]
    public Canvas menuCanvas;

    public HoverCursor cursor;

    private ToggleExpandability menuExpandability;

    //used to turn off our background image of our UI according to what mode one is in -> allows for ghost cursos when pointing at UI without turning on laser
    private Image cursorImage;

    //Get references for our UI
    public void Awake()
    {

#if UNITY_EDITOR
        WebXRManagerEditorSimulator.OnXRChange += onXRChange;
#else 
        WebXRManager.OnXRChange += onXRChange;
#endif

        //get our component used to customise our UI depending on Desktop vs XR
        menuExpandability = menuCanvas.GetComponent<ToggleExpandability>();

        if (menuExpandability == null)
        {
            Debug.LogError("No ToggleExtendability component found on UI (Swithch_UI_Placement.cs)");
        }

        cursorImage = menuCanvas.GetComponent<Image>();

        if (cursorImage == null) 
        {
            Debug.LogError("No Image component found on UI (Swithch_UI_Placement.cs)");
        }
        
        if (cursor == null) {
            throw new System.Exception("You must set a HoverCursor");
        }
    }

    private void onXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
    {
        if (state == WebXRState.VR)
        {
            SetVRViewPort();
            cursor.EnableHoverCursor();
        }
        else if(state == WebXRState.NORMAL)
        {
            SetDesktopViewport();
            cursor.DisableHoverCursor();
        }
    }

    [ContextMenu("Set to VR Mode")]
    public void SetVRViewPort()
    {   //todo(Brandon): refactor this so that the world camera can also get set for the right hand.

        UIManager.Instance.PlaceMenuOnCurrentHand();

        menuCanvas.renderMode = RenderMode.WorldSpace;

        menuExpandability.ConvertToAlwaysExpanded();
        
        //use ghost cursor on the menu in XR mode
        cursorImage.enabled = true;
    }


    [ContextMenu("Set to Desktop Mode")]
    public void SetDesktopViewport()
    {
        menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        menuExpandability.ConvertToExpandable(false);

        cursorImage.enabled = false;
    }
}
