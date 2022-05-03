//#define TESTING_BEFORE_BUILDING

using UnityEngine;
using UnityEngine.EventSystems;
using WebXR;
using Komodo.Utilities;
using System.Collections.Generic;

namespace Komodo.Runtime
{
    /** 
     *  @brief Camera collection to reference for eventsystem interaction
     */
    public class EventSystemManager : SingletonComponent<EventSystemManager>
    {
        public static EventSystemManager Instance
        {
            get { return ((EventSystemManager)_Instance); }
            set { _Instance = value; }
        }

        /** 
         * @brief we use cameras for our lazer selection to use Unity Eventsystem.
        */
         public TriggerEventInputSource inputSource_LeftHand;
         public TriggerEventInputSource inputSource_RighttHand;

        [ShowOnly] public Transform cursor;

        public StandaloneDesktopInputModule desktopStandaloneInput;
        public StandaloneXRInputModule xrStandaloneInput;

        [Header("UI Canvases to set event camera for when switching between desktop and xr modes")]
        public List<Canvas> canvasesToReceiveEvents;

        //Check for null references
        public void Awake()
        {
                //used to set our managers alive state to true to detect if it exist within scene
                var initManager = Instance;

#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING
            WebXRManager.OnXRChange += onXRChange;
#else 
            WebXRManagerEditorSimulator.OnXRChange += onXRChange;
#endif

            if (inputSource_LeftHand == null)
                Debug.LogError("We are missing XR Lefthand camera to use with our eventsystem (EventSystemRayCastCameras.cs", gameObject);

            if (inputSource_RighttHand == null)
                Debug.LogError("We are missing XR RightHand camera to use with our eventsystem (EventSystemRayCastCameras.cs", gameObject);

            if (desktopStandaloneInput == null)
                Debug.LogError("We are missing desktopEventsystem (EventSystemRayCastCameras.cs", gameObject);

            if (xrStandaloneInput == null)
                Debug.LogError("We are missing xREventsystem (EventSystemRayCastCameras.cs", gameObject);
        }

        //public void Start()
        //{
        //    ////check if we have a menu available in our UIManager
        //    //if (UIManager.Instance.menuCanvas != null)
        //    //{
        //    //    //if we have one add it to the last canvas array index
        //    //    canvasesToReceiveEvents[canvasesToReceiveEvents.Length - 1] = UIManager.Instance.menuCanvas;


        //    //}
        //  //  xrStandaloneInput.gameObject.SetActive(false);
        //}

        public WebXRState GetXRCurrentState()
        {
            //to avoid issues with not finding xrstate 
#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING
            return WebXRManager.Instance.XRState;
#else
            return WebXRState.NORMAL;
#endif
        }

        private void onXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {

            if (state == WebXRState.VR)
            {
                SetToXR();
            }
            else if (state == WebXRState.NORMAL)
            {
                SetToDesktop();
            }

        }

        [ContextMenu("Set to Desktop")]
        public void SetToDesktop()
        {
            GetComponent<ToggleMenuDisplayMode>().SetDesktopViewport();
            //turn on and off appropriate eventsystem to handle appropriate input
            desktopStandaloneInput.gameObject.SetActive(true);
            xrStandaloneInput.gameObject.SetActive(false);
        }

        [ContextMenu("Set to XR")]
        public void SetToXR()
        {
            GetComponent<ToggleMenuDisplayMode>().SetVRViewPort();
            desktopStandaloneInput.gameObject.SetActive(false);
            xrStandaloneInput.gameObject.SetActive(true);
        }


        /// <summary>
        /// set our canvas reference event camera to receive proper input source
        /// </summary>
        /// <param name="trigger_Select"> the trigger_select instance to set active</param>
        //public void AddInputSource(TriggerEventInputSource trigger_Select)
        //{
        //    ////set our canvas to receive input from our activated hand
        //    //foreach (var canvas in canvasesToReceiveEvents)
        //    //    canvas.worldCamera = trigger_Select.eventCamera;

        //    //set linerenderer to use for line to UI interactions
        //    xrStandaloneInput.RegisterInputSource(trigger_Select);

        //}

        /// <summary>
        /// Set source to disable and set alternative source on, to switch selection input when alternating butons
        /// </summary>
        /// <param name="inputSource"></param>
        public void RemoveInputSourceAndSendClickAndDownEvent(TriggerEventInputSource inputSource)
        {
            //set click event for our lazer if it is on top of a UI component when disabling
            xrStandaloneInput.SetTriggerForClick();

            //set appropriate trigger hand active
            if (inputSource_LeftHand == inputSource)
            {
                //only change input when other lazer is on, if not keep it within the current hand
                if (!inputSource_RighttHand.gameObject.activeInHierarchy)
                    return;

                //set alternate camera for input
                foreach (var canvas in canvasesToReceiveEvents)
                    canvas.worldCamera = inputSource_RighttHand.eventCamera;

                //set linerenderer to use for line to UI interactions
                xrStandaloneInput.RegisterInputSource(inputSource_RighttHand);

                //remove this input source
                xrStandaloneInput.RemoveInputSource(inputSource);

            }
            else if (inputSource_RighttHand == inputSource)
            {
                //only change input when other lazer is on, if not keep it within the current hand
                if (!inputSource_LeftHand.gameObject.activeInHierarchy)
                    return;

                foreach (var canvas in canvasesToReceiveEvents)
                    canvas.worldCamera = inputSource_LeftHand.eventCamera;



                //set linerenderer to use for line to UI interactions
                xrStandaloneInput.RegisterInputSource(inputSource_LeftHand);

                //remove this input source
                xrStandaloneInput.RemoveInputSource(inputSource);
            }

        }

        public void RemoveInputSourceWithoutClick(TriggerEventInputSource inputSource)
        {
            //set click event for our lazer if it is on top of a UI component when disabling
           // xrStandaloneInput.SetTriggerForClick();

            //set appropriate trigger hand active
            if (inputSource_LeftHand == inputSource)
            {
                //only change input when other lazer is on, if not keep it within the current hand
                if (!inputSource_RighttHand.gameObject.activeInHierarchy)
                    return;

                //set alternate camera for input
                foreach (var canvas in canvasesToReceiveEvents)
                    canvas.worldCamera = inputSource_RighttHand.eventCamera;

                //set linerenderer to use for line to UI interactions
                xrStandaloneInput.RegisterInputSource(inputSource_RighttHand);

                //remove this input source
                xrStandaloneInput.RemoveInputSource(inputSource);

            }
            else if (inputSource_RighttHand == inputSource)
            {
                //only change input when other lazer is on, if not keep it within the current hand
                if (!inputSource_LeftHand.gameObject.activeInHierarchy)
                    return;

                foreach (var canvas in canvasesToReceiveEvents)
                    canvas.worldCamera = inputSource_LeftHand.eventCamera;

                //set linerenderer to use for line to UI interactions
                xrStandaloneInput.RegisterInputSource(inputSource_LeftHand);

                //remove this input source
                xrStandaloneInput.RemoveInputSource(inputSource);
            }

        }

    }
}
