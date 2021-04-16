using UnityEngine.EventSystems;
using UnityEngine;
using WebXR;
using System.Collections;
using Komodo.Utilities;

namespace Komodo.Runtime
{
    public class FreeFlightController : MonoBehaviour
    {
        [Tooltip("Enable/disable rotation control. For use in Unity editor only.")]
        public bool rotationEnabled = true;

        [Tooltip("Enable/disable translation control. For use in Unity editor only.")]
        public bool translationEnabled = true;

        private WebXRDisplayCapabilities capabilities;

        [Tooltip("Mouse sensitivity")]
        public float mouseSensitivity = 1f;

        [Tooltip("Pan Sensitivity sensitivity + middle mouse hold")]
        public float panSensitivity = 0.65f;

        [Tooltip("Straffe Speed")]
        public float straffeSpeed = 5f;

        [Tooltip("Forward and Back Scroll")]
        public float scrollSpeed = 10f;

        [Tooltip("Snap Rotation Angle")]
        public int turningDegrees = 30;

        Quaternion originalRotation;

        private Transform desktopCamera;

        private Transform playspace;

        private float minimumX = -360f;
        private float maximumX = 360f;

        private float minimumY = -90f;
        private float maximumY = 90f;


        //to check on ui over objects to disable mouse drag while clicking buttons
        private StandaloneDesktopInputModule standaloneInputModule_Desktop;

        //used for syncing our XR player position with desktop
        public TeleportPlayer teleportPlayer;

        //set state of desktop
        [ShowOnly] public bool isUpdating = true;


        void Awake()
        {
              

          

#if UNITY_EDITOR
            WebXRManagerEditorSimulator.OnXRChange += onXRChange;
#else 
            WebXRManager.OnXRChange += onXRChange;
#endif
     

            WebXRManager.OnXRCapabilitiesUpdate += onXRCapabilitiesUpdate;
          
        }

        private void WebXRManager_OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator Start()
        {
            //wait for our ui to be set up before we allow user to move around with camera
            isUpdating = false;

            playspace = GameObject.FindWithTag("XRCamera").transform;
            desktopCamera = GameObject.FindWithTag("DesktopCamera").transform;//transform;

            originalRotation = desktopCamera.localRotation;

            if (EventSystemManager.IsAlive)
            {
                //get our desktop eventsystem
                if (!standaloneInputModule_Desktop)
                    standaloneInputModule_Desktop = EventSystemManager.Instance.desktopStandaloneInput;
            }

            if (UIManager.IsAlive)
                yield return new WaitUntil(() => UIManager.Instance.IsReady());

            isUpdating = true;

            //teleportPlayer.BeginPlayerHeightCalibration(left hand? right hand?); //TODO turn back on and ask for handedness 
        }

        public void Update()
        {
            if (!isUpdating) 
            {
                return;
            }

            if (translationEnabled)
            {
                MovePlayerFromInput();
            }

            if (IsMouseInteractingWithMenu()) {
                return;
            }

            if (rotationEnabled && Input.GetMouseButton(0))
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

            SyncXRWithSpectator();
        }

        private void onXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            if (state == WebXRState.VR)
            {
                isUpdating = false;

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

                isUpdating = true;
            }
        }

        private void onXRCapabilitiesUpdate(WebXRDisplayCapabilities vrCapabilities)
        {
            capabilities = vrCapabilities;
            EnableAccordingToPlatform();
        }

        #region Snap Turns and Move Functionality - Buttton event linking funcions (editor UnityEvent accessible)
        Quaternion xQuaternion;
        Quaternion yQuaternion;
    
        public void RotatePlayer(int rotateDirection)
        {
            switch (rotateDirection)
            {

                case 3:
                    //LEFT
                    curRotationX -= turningDegrees;
                    break;

                case 4:
                    //RIGHT
                    curRotationX += turningDegrees;
                    break;

                case 2:
                    //UP
                    curRotationY -= turningDegrees;
                    break;

                case 1:
                    //DOWN
                    curRotationY += turningDegrees;
                    break;
            }

            curRotationX = ClampAngle(curRotationX, minimumY, maximumY);
            curRotationY = ClampAngle(curRotationY, minimumX, maximumX);

            desktopCamera.localRotation = Quaternion.Euler(new Vector3(curRotationX, curRotationY, 0));
        }

        public void RotateXRPlayer(int rotateDirection)
        {
            switch (rotateDirection)
            {
                case 3:
                    //LEFT
                    curRotationX -= turningDegrees;
                    break;

                case 4:
                    //RIGHT
                    curRotationX += turningDegrees;
                    break;

                case 2:
                    //UP
                    curRotationY -= turningDegrees;
                    break;

                case 1:
                    //DOWN
                    curRotationY += turningDegrees;
                    break;
            }
            curRotationX = ClampAngle(curRotationX, minimumY, maximumY);
            curRotationY = ClampAngle(curRotationY, minimumX, maximumX);

            var result = Quaternion.Euler(new Vector3(curRotationX, curRotationY, 0));

            teleportPlayer.SetXRAndSpectatorRotation(result);
        }

        float curRotationX = 0f;
        float curRotationY = 0f;
        public void RotatePlayerWithDelta(int rotateDirection)
        {
            var delta = Time.deltaTime * straffeSpeed;

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

        public void MovePlayer(int moveDirection)
        {
            switch (moveDirection)
            {

                case 1:

                    float x = 1;
                    var movement = new Vector3(x, 0, 0);
                    movement = desktopCamera.TransformDirection(movement);
                    desktopCamera.position += movement;
                    break;

                case 2:

                    x = -1;
                    movement = new Vector3(x, 0, 0);
                    movement = desktopCamera.TransformDirection(movement);
                    desktopCamera.position += movement;
                    break;

                case 3:

                    float z = 1;
                    movement = new Vector3(0, 0, z);
                    movement = desktopCamera.TransformDirection(movement);
                    desktopCamera.position += movement;
                    break;

                case 4:

                    z = -1;
                    movement = new Vector3(0, 0, z);
                    movement = desktopCamera.TransformDirection(movement);
                    desktopCamera.position += movement;
                    break;
            }
        }

        public void MovePlayerFromInput() {
            var accumulatedImpactMul = Time.deltaTime * straffeSpeed;
            float x = Input.GetAxis("Horizontal") * accumulatedImpactMul;
            float z = Input.GetAxis("Vertical") * accumulatedImpactMul;

            if (Input.GetKey(KeyCode.Q)) RotatePlayerWithDelta(2);
            if (Input.GetKey(KeyCode.E)) RotatePlayerWithDelta(1);
            if (Input.GetKey(KeyCode.Alpha2)) RotatePlayerWithDelta(3);
            if (Input.GetKey(KeyCode.Alpha3)) RotatePlayerWithDelta(4);


            var movement = new Vector3(x, 0, z);
            movement = desktopCamera.TransformDirection(movement);
            desktopCamera.position += movement;
        }

        public void RotatePlayerFromInput() {
            curRotationY += Input.GetAxis("Mouse X") * mouseSensitivity;
            curRotationX -= Input.GetAxis("Mouse Y") * mouseSensitivity;

            curRotationX = ClampAngle(curRotationX, minimumY, maximumY);
            curRotationY = ClampAngle(curRotationY, minimumX, maximumX);

            desktopCamera.localRotation = Quaternion.Euler(new Vector3(curRotationX, curRotationY, 0));
        }

        public void PanPlayerFromInput() {
            var x = Input.GetAxis("Mouse X") * panSensitivity;
            var y = Input.GetAxis("Mouse Y") * panSensitivity;

            desktopCamera.position += desktopCamera.TransformDirection(new Vector3(x, y));
        }

        public void HyperspeedPanPlayerFromInput() {
            desktopCamera.position += desktopCamera.TransformDirection(scrollSpeed * new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel")));
        }

        public bool IsMouseInteractingWithMenu() {

            if (EventSystem.current != null)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    if (standaloneInputModule_Desktop.GetCurrentFocusedObject_Desktop())
                    {
                        if (standaloneInputModule_Desktop.GetCurrentFocusedObject_Desktop().layer == LayerMask.NameToLayer("UI"))
                            return true;
                    }
                }
            }

            return false;
        }

        public void SyncXRWithSpectator() {
            //synchronize xr camera with desktop camera transforms
            teleportPlayer.SetXRPlayerPositionAndLocalRotation(desktopCamera.position, desktopCamera.localRotation);
        }

        #endregion


        /// Enables rotation and translation control for desktop environments.
        /// For mobile environments, it enables rotation or translation according to
        /// the device capabilities.
        void EnableAccordingToPlatform()
        {
            rotationEnabled = translationEnabled = !capabilities.canPresentVR;
        }

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

    }
}
