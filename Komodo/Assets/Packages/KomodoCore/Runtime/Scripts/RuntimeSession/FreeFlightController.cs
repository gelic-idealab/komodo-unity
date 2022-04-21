//#define TESTING_BEFORE_BUILDING

using UnityEngine.EventSystems;
using UnityEngine;
using WebXR;
using System.Collections;
using Komodo.Utilities;


namespace Komodo.Runtime
{
    public class FreeFlightController : MonoBehaviour, IUpdatable
    {
        
        /** 
         * @brief Enable/disable rotation control. For use in Unity editor only.
        */
        [Tooltip("Enable/disable rotation control. For use in Unity editor only.")]
        public bool rotationEnabled = true;

        private WebXRDisplayCapabilities capabilities;

        /** 
         * @brief Rotation sensitivity
        */
        [Tooltip("Rotation sensitivity")]
        public float rotationSensitivity = 3f; 
        public bool naturalRotationDirection = true;

        /** 
         * @brief Enable/disable translation control. For use in Unity editor only
        */
        [Tooltip("Enable/disable translation control. For use in Unity editor only.")]
        public bool translationEnabled = true; 
        
        /** 
         * @brief Pan Sensitivity sensitivity + middle mouse hold
        */
        [Tooltip("Pan Sensitivity sensitivity + middle mouse hold")]
        public float panSensitivity = 0.1f; 
        public bool naturalPanDirection = true;

        /** 
         * @brief Strafe Speed
        */
        [Tooltip("Strafe Speed")]
        public float strafeSpeed = 5f;

        public bool naturalStrafeDirection = true;

        /** 
         * @brief Forward and Backwards Hyperspeed Scroll
        */
        [Tooltip("Forward and Backwards Hyperspeed Scroll")]
        public float scrollSpeed = 10f; 
        public bool naturalScrollDirection = true;

        /** 
         * @brief Snap Rotation Angle; the snap turn is set to 30 degree in this case. 
        */
        [Tooltip("Snap Rotation Angle")]
        public int turningDegrees = 30;

        Quaternion originalRotation;

        /** 
         * @brief  the floorIndicator game object shows up when holding right-click in spectator/PC mode; it is a
         * purple cylinder. It serves the teleportation fucntionality (in spectator/PC mode).
        */
        public GameObject floorIndicator;

        [SerializeField] private Camera spectatorCamera;

        /** 
         * @brief  the teleportationIndicator game object shows up when holding right-click in spectator/PC mode; it 
         * is a curved line. It serves the teleportation fucntionality (in spectator/PC mode).
        */
        [Tooltip("Hierarchy: Spectator Camera -> TeleportationLine")]
        [SerializeField] private GameObject teleportationIndicator;

        /** 
         * @brief  This is the position that the floorIndicator should be;
        */
        Vector3 targetPosition; 


        private Transform desktopCamera;

        private Transform playspace;

        private float minimumX = -360f;
        private float maximumX = 360f;

        private float minimumY = -90f;
        private float maximumY = 90f;


        //to check on ui over objects to disable mouse drag while clicking buttons
        private StandaloneDesktopInputModule standaloneInputModule_Desktop;

        /** 
         * @brief Used for syncing our XR player position with desktop
         */
        [Tooltip("Hierarchy: PlayerSet Prefab")]
        public TeleportPlayer teleportPlayer;

        void Awake()
        {
#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING
            WebXRManager.OnXRChange += onXRChange;
#else 
            WebXRManagerEditorSimulator.OnXRChange += onXRChange;
#endif
            WebXRManager.OnXRCapabilitiesUpdate += onXRCapabilitiesUpdate;
        }


        /** 
         * @brief This is a coroutine that waits for our UI to be setup before we allow user to move around with camera.
         * It first gets our references for the player we are moving and the player's xr camera and desktop camera. It will 
         * then get the desktop eventsystem, wait until the UIManager is ready, and starts the freeflightcontroller.
        */
        public IEnumerator Start()
        {

            //get our references for the player we are moving and its xrcamera and desktopcamera
            TryGetPlayerReferences();

            playspace = GameObject.FindWithTag(TagList.xrCamera).transform;
            desktopCamera = GameObject.FindWithTag(TagList.desktopCamera).transform;//transform;

            originalRotation = desktopCamera.localRotation;

            if (EventSystemManager.IsAlive)
            {
                //get our desktop eventsystem
                if (!standaloneInputModule_Desktop)
                    standaloneInputModule_Desktop = EventSystemManager.Instance.desktopStandaloneInput;
            }

            if (UIManager.IsAlive)
                yield return new WaitUntil(() => UIManager.Instance.IsReady());

            //start using our freflightcontroller after we finish loading UI
            GameStateManager.Instance.RegisterUpdatableObject(this);

            //teleportPlayer.BeginPlayerHeightCalibration(left hand? right hand?); //TODO turn back on and ask for handedness 
        }

        /**
         * @brief This function updates player's movements and performs various functionalities based on the keys the player inputs. 
         * 
         * @param deltaTime the realtime since the program starts.
         * 
         */
        public void OnUpdate(float deltaTime)
        {
            if (translationEnabled)
            {
                MovePlayerFromInput();
            }
            
            //This prevents the mouse from rotating when interacting with UI.
            //This is why dragging on the scene and hovering to the menu will suddenly stop camera rotation.
            if (IsMouseInteractingWithMenu()) {
                return;
            }

            if ((rotationEnabled && Input.GetMouseButton(0)) || (rotationEnabled && Input.GetMouseButton(1)))
            {
                RotatePlayerFromInput();
            }

            if (Input.GetMouseButton(2))
            {
                PanPlayerFromInput();
            }

            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                HyperspeedPanPlayerFromInput();   
            }

            ShowTeleportationIndicator();

            MousePositionToTeleportationIndicator();

            SyncXRWithSpectator();
        }

        /** 
         * @brief onXRChange method adjusts player's camera position and rotation when switching between WebXR and desktop mode.
         * 
         * @param state 
         * @param viewsCount
         * @param leftRect
         * @param rightRect
         * 
        */
        private void onXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            if (state == WebXRState.VR)
            {
                GameStateManager.Instance.DeRegisterUpdatableObject(this);
                //isUpdating = false;

                //Reset the XR rotation of our VR Cameras to avoid leaving weird rotations from desktop mode
                curRotationX = 0f;

                var result = Quaternion.Euler(new Vector3(0, curRotationY, 0));

                teleportPlayer.SetXRAndSpectatorRotation(result);

            }
            else if(state == WebXRState.NORMAL)
            {
                //commented to avoid setting rotation back on which causes rotational issues when switching cameras
                //  EnableAccordingToPlatform();

                //set desktop camera the same as the xr camera on xr exit
                curRotationX = 0f;

                desktopCamera.position = playspace.position;

                desktopCamera.localRotation = Quaternion.Euler(new Vector3(0, curRotationY, 0));

                SyncXRWithSpectator();

                GameStateManager.Instance.RegisterUpdatableObject(this);
               // isUpdating = true;
            }
        }

        /// <summary>
        /// Get a reference for a playerset to move by assinging playspace and desktopCamera variables.
        /// </summary>
        public void TryGetPlayerReferences()
        {
            var player = GameObject.FindWithTag(TagList.player);

            if (!player)
                Debug.Log("player not found for FreeFlightController.cs");
            else
            {
               if(player.TryGetComponent(out TeleportPlayer tP))
                {
                    teleportPlayer = tP;
                }
                else
                {
                    Debug.Log("no TeleportPlayer script found for player in FreeFlightController.cs");
                }
            }

            playspace = GameObject.FindWithTag(TagList.xrCamera).transform;
            desktopCamera = GameObject.FindWithTag(TagList.desktopCamera).transform;//transform;

            if(!playspace)
                Debug.Log("no XRCamera tagged object found in FreeFlightController.cs");

            if (!desktopCamera)
                Debug.Log("no desktopCamera tagged object found in FreeFlightController.cs");
        }

        /** 
         * @brief  This checks if the player has the eligibility of using webXR.
         * 
         * @param vrCapabilities vrCapabilities is an object with booleans values contained in it. For more info, check out WebXRDisplayCapabilities.
        */
        private void onXRCapabilitiesUpdate(WebXRDisplayCapabilities vrCapabilities)
        {
            capabilities = vrCapabilities;
            EnableAccordingToPlatform();
        }

        #region Snap Turns and Move Functionality - Button event linking functions (editor UnityEvent accessible)
        Quaternion xQuaternion;
        Quaternion yQuaternion;
     
        float curRotationX = 0f;
        float curRotationY = 0f;
        public void RotatePlayerWithDelta(int rotateDirection)
        {
            var delta = Time.deltaTime * strafeSpeed;

            switch (rotateDirection)
            {
                case 3:
                    //LEFT
                    curRotationX -= 45F * delta;
                    break;

                case 4:
                    //RIGHT
                    curRotationX += 45F * delta;
                    break;

                case 2:
                    //UP
                    curRotationY -= 45 * delta;
                    break;

                case 1:
                    //DOWN
                    curRotationY += 45 * delta;
                    break;
            }
            curRotationX = ClampAngle(curRotationX, minimumY, maximumY);
            curRotationY = ClampAngle(curRotationY, minimumX, maximumX);

            desktopCamera.localRotation = Quaternion.Euler(new Vector3(curRotationX, curRotationY, 0));
        }


        public void MovePlayerFromInput() {
            var accumulatedImpactMul = Time.deltaTime * strafeSpeed;
            float x = Input.GetAxis("Horizontal") * accumulatedImpactMul;
            float z = Input.GetAxis("Vertical") * accumulatedImpactMul;

            if (Input.GetKey(KeyCode.Q)) RotatePlayerWithDelta(2);
            if (Input.GetKey(KeyCode.E)) RotatePlayerWithDelta(1);
            if (Input.GetKey(KeyCode.Alpha2)) RotatePlayerWithDelta(3);
            if (Input.GetKey(KeyCode.Alpha3)) RotatePlayerWithDelta(4);


            var movement = new Vector3(x, 0, z);
            movement = desktopCamera.TransformDirection(movement) * (naturalStrafeDirection ? -1 : 1);
            desktopCamera.position += movement;
        }

        /** 
         * @brief RotatePlayerFromInput gets values from mouse motion and transaltes to rotation around either the Y axis or the X axis.
         * 
        */
        public void RotatePlayerFromInput() {
            curRotationY += Input.GetAxis("Mouse X") * rotationSensitivity * (naturalRotationDirection ? -1 : 1); //horizontal mouse motion translates to rotation around the Y axis.
            curRotationX -= Input.GetAxis("Mouse Y") * rotationSensitivity * (naturalRotationDirection ? -1 : 1); // vertical mouse motion translates to rotation around the X axis.

            curRotationX = ClampAngle(curRotationX, minimumY, maximumY);
            curRotationY = ClampAngle(curRotationY, minimumX, maximumX);

            desktopCamera.localRotation = Quaternion.Euler(new Vector3(curRotationX, curRotationY, 0));
        }

        /** 
         * @brief When holding down mouse scrollwheel, PanPlayerFromInput() will be activated. This allows players to hover their camera angle. 
        */
        public void PanPlayerFromInput() {
            var x = Input.GetAxis("Mouse X") * panSensitivity * (naturalPanDirection ? -1 : 1);
            var y = Input.GetAxis("Mouse Y") * panSensitivity * (naturalPanDirection ? -1 : 1);

            desktopCamera.position += desktopCamera.TransformDirection(new Vector3(x, y));
        }
        

        /** 
         * @brief HyperseedPanPlayerFromInput() allows players to control their position through mouse scrollwheel. 
        */
        public void HyperspeedPanPlayerFromInput() {
            desktopCamera.position += desktopCamera.TransformDirection(scrollSpeed * new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel") * (naturalScrollDirection ? -1 : 1)));
        }

        public bool IsMouseInteractingWithMenu() {

            if (EventSystem.current != null)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    if (standaloneInputModule_Desktop.GetCurrentFocusedObject_Desktop())
                    {
                        if (standaloneInputModule_Desktop.GetCurrentFocusedObject_Desktop().layer == LayerMask.NameToLayer("UI"))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /** 
         * @brief synchronize XR camera with desktop camera transforms
        */
        public void SyncXRWithSpectator() {
            teleportPlayer.SetXRPlayerPositionAndLocalRotation(desktopCamera.position, desktopCamera.localRotation);
        }

        #endregion

        /// <Summary>
        /// Enables rotation and translation control for desktop environments.
        /// For mobile environments, it enables rotation or translation according to
        /// the device capabilities.
        /// <Summary>
        void EnableAccordingToPlatform()
        {
            rotationEnabled = translationEnabled = !capabilities.canPresentVR;
        }

        /** 
         * @brief This functions returns a value that is between -360 and 360 degree for angle adjustment
         * 
         * @param angle the angle that the player camera (X or Y) is currently at. 
         * @param min  -360
         * @param max  360
        */
        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360f) 
            {
                angle += 360f;
            }
            if (angle > 360f) 
            {
                angle -= 360f;
            }
            return Mathf.Clamp(angle, min, max);
        }

        /// <Summary> 
        /// Show teleportation indicator while holding right click.
        /// </Summary>
        public void ShowTeleportationIndicator()
        {  
            if (Input.GetMouseButtonDown(1)) {

                teleportationIndicator.SetActive(true);

            } else if (Input.GetMouseButtonUp(1)) {

                teleportationIndicator.SetActive(false);
            }
        }

        /// <Summary>
        /// This function turns mouse position in an xy coordinate into a ray.
        /// The RaycastHit will hit something in the scene and becomes the z coordinate of the mouse's position.
        /// It will then assign mouse's position to floorIndicator.
        /// </Summary>
        public void MousePositionToTeleportationIndicator() 
        {
              Ray ray = spectatorCamera.ScreenPointToRay(Input.mousePosition);
              RaycastHit hit;

              if (Physics.Raycast(ray, out hit)) 
              {
                  targetPosition = hit.point;
                  floorIndicator.transform.position = targetPosition;
              }
        }
    }
}
