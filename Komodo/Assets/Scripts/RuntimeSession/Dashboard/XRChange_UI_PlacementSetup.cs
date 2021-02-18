using System;
using UnityEngine;
using WebXR;
using UnityEngine.UI;

namespace Komodo.Runtime
{
    /// <summary>
    /// Used to set our UI placenemnt and settings depending on being in Desktop or XR mode.
    /// </summary>
    public class XRChange_UI_PlacementSetup : MonoBehaviour
    {
        //default values to use to move our UI to be comfortable for usage with hand attachment
        public Vector3 scaleToChangeTo = new Vector3(0.001f, 0.001f, 0.001f);
        public Vector3 rotationToChangeTo = new Vector3(-30, 180, 180);
        public Vector3 positionToChangeTo;


        [Header("UI Canvas To Move From Desktop to XR Input")]
        public Canvas selectionCanvas;

        private ToggleExpandability uiToggleExtensibilityComponent;

        //used to turn off our background image of our UI according to what mode one is in -> allows for ghost cursos when pointing at UI without turning on laser
        private Image uiImage;

        //Get references for our UI
        public void Awake()
        {
            WebXRManager.OnXRChange += onXRChange;

            //get our component used to customise our UI depending on Desktop vs XR
            uiToggleExtensibilityComponent = selectionCanvas.GetComponent<ToggleExpandability>();

            if (uiToggleExtensibilityComponent == null)
                Debug.LogError("No ToggleExtendability component found on UI (Swithch_UI_Placement.cs)");

            uiImage = selectionCanvas.GetComponent<Image>();

            if (uiImage == null)
                Debug.LogError("No Image component found on UI (Swithch_UI_Placement.cs)");

        }

        //listen to Desktop and XR change
        //public void Start()
        //{
        //   WebXRManager.Instance.setXrState(WebXRState.ENABLED);
        //}

        //set settings for our UI depending on Desktop vs XR modes
        private void onXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {

            if (state == WebXRState.VR)
                SetVRViewPort();
            else if (state == WebXRState.NORMAL)
                SetDesktopViewport();
        }

        public void SetVRViewPort()
        {   //todo(Brandon): refactor this so that the world camera can also get set for the right hand.
            var canvasTransform = selectionCanvas.GetComponent<RectTransform>();

            if (canvasTransform == null)
            {
                throw new Exception("selection canvas must have a RectTransform component");
            }

            canvasTransform.localRotation = Quaternion.Euler(rotationToChangeTo); //0, 180, 180 //UI > Rect Trans > Rotation -123, -0.75, 0.16
            canvasTransform.localScale = scaleToChangeTo;
            canvasTransform.anchoredPosition3D = positionToChangeTo; //new Vector3(0.0f,-0.35f,0f); //UI > R T > Position 0.25, -0.15, 0.1
            selectionCanvas.renderMode = RenderMode.WorldSpace;
            canvasTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500); // sizeDelta.x =  500;

            //set our left lazer camera to be used with input (UI events)
            selectionCanvas.worldCamera = EventSystemManager.Instance.inputSource_LeftHand.eventCamera;

            uiToggleExtensibilityComponent.ConvertToAlwaysExpanded();

            //use ghost cursor ui plane only in XR
            uiImage.enabled = true;
        }

        public void SetDesktopViewport()
        {
            selectionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            uiToggleExtensibilityComponent.ConvertToExpandable(false);

            uiImage.enabled = false;
        }
    }
}