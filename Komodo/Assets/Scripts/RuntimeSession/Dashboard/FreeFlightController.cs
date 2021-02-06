using UnityEngine.EventSystems;
using UnityEngine;
using WebXR;
using System.Collections;

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

    private Transform thisTransform;
    private Transform vrPlayer;

    private float minimumX = -360f;
    private float maximumX = 360f;

    private float minimumY = -90f;
    private float maximumY = 90f;


    //to check on ui over objects to disable mouse drag while clicking buttons
    private StandaloneInputModule_Desktop standaloneInputModule_Desktop;

    //used for syncing our XR player position with desktop
    public TeleportPlayer teleportPlayer;

    //set state of desktop
    [ShowOnly]public bool isUpdating = true;


    void Awake()
    {
        //vr_Cameras_parent
        if(!vrPlayer)
        vrPlayer = GameObject.FindWithTag("XRCamera").transform;

        //desktop_Camera
        thisTransform = transform;
        WebXRManager.OnXRChange += onXRChange;

        //get our desktop eventsystem
        if(!standaloneInputModule_Desktop) 
            standaloneInputModule_Desktop = EventSystemManager.Instance.desktopStandaloneInput;

        WebXRManager.OnXRCapabilitiesUpdate += onXRCapabilitiesUpdate;// onVRCapabilitiesUpdate;
        originalRotation = thisTransform.localRotation;
    }

    private void WebXRManager_OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator Start()
    {
        //wait for our ui to setup before we allow user to move around with camera
        isUpdating = false;
        yield return new WaitUntil(() => UIManager.Instance.IsReady());
        isUpdating = true;

        teleportPlayer.UpdatePlayerHeight(teleportPlayer.cameraOffset.cameraYOffset);// defaultPlayerInitialHeight);
    }

    private void onXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
    {
        if (state == WebXRState.VR)
        {
            isUpdating = false;

            //Reset the XR rotation of our VR Cameras to 

            //w allignment x for VR to avoid leaving weird rotations from desktop mode
            curRotationX = 0f;
            var result = Quaternion.Euler(new Vector3(0, curRotationY, 0));

            teleportPlayer.SetXRPayerDesktopPlayerRotation(result );

        }
        else if(state == WebXRState.NORMAL)
        {

            //commented to avoid setting rotation back on which causes rotational issues when switching cameras
            //  EnableAccordingToPlatform();

            //set desktop camera the same as the xr camera on xr exit
            curRotationX = 0f;
            thisTransform.position = vrPlayer.position;
            thisTransform.localRotation = Quaternion.Euler(new Vector3(0, curRotationY, 0));
            teleportPlayer.SetXRPlayerPositionAndLocalRotation(thisTransform.position,  thisTransform.localRotation );//vrPlayer.localRotation;
            isUpdating = true;
        }
    }

    private void onXRCapabilitiesUpdate(WebXRDisplayCapabilities vrCapabilities)
    {
        capabilities = vrCapabilities;
        EnableAccordingToPlatform();
    }

    #region Snap Turns and Move Funcionality - Buttton event linking funcions (editor UnityEvent accessible)
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

        thisTransform.localRotation = Quaternion.Euler(new Vector3(curRotationX, curRotationY, 0));
    }

    public void RotateXR_Player(int rotateDirection)
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

        teleportPlayer.SetXRPayerDesktopPlayerRotation(result);
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

        thisTransform.localRotation = Quaternion.Euler(new Vector3(curRotationX, curRotationY, 0));
    }


    public void MovePlayer(int moveDirection)
    {
        switch (moveDirection)
        {

            case 1:

                float x = 1;
                var movement = new Vector3(x, 0, 0);
                movement = thisTransform.TransformDirection(movement);
                thisTransform.position += movement;
                break;

            case 2:

                x = -1;
                movement = new Vector3(x, 0, 0);
                movement = thisTransform.TransformDirection(movement);
                thisTransform.position += movement;
                break;

            case 3:

                float z = 1;// Input.GetAxis("Vertical") * Time.deltaTime * straffeSpeed;
                movement = new Vector3(0, 0, z);
                movement = thisTransform.TransformDirection(movement);
                thisTransform.position += movement;
                break;

            case 4:

                z = -1;// Input.GetAxis("Vertical") * Time.deltaTime * straffeSpeed;
                movement = new Vector3(0, 0, z);
                movement = thisTransform.TransformDirection(movement);
                thisTransform.position += movement;
                break;
        }
    }

    #endregion

    public void Update()
    {
        if (!isUpdating)
            return;

        //move player
        if (translationEnabled)
        {
            var accumulatedImpactMul = Time.deltaTime * straffeSpeed;
            float x = Input.GetAxis("Horizontal") * accumulatedImpactMul;
            float z = Input.GetAxis("Vertical") * accumulatedImpactMul;

            if (Input.GetKey(KeyCode.Q)) RotatePlayerWithDelta(2);
            if (Input.GetKey(KeyCode.E)) RotatePlayerWithDelta(1);
            if (Input.GetKey(KeyCode.Alpha2)) RotatePlayerWithDelta(3);
            if (Input.GetKey(KeyCode.Alpha3)) RotatePlayerWithDelta(4);


            var movement = new Vector3(x, 0, z);
            movement = thisTransform.TransformDirection(movement);
            thisTransform.position += movement;
        }

        //if event system picks up button selection -> skip mouse drag events
        if (EventSystem.current != null)
            if (EventSystem.current.IsPointerOverGameObject())
            {
                if (standaloneInputModule_Desktop.GetCurrentFocusedObject_Desktop())
                {
                    if (standaloneInputModule_Desktop.GetCurrentFocusedObject_Desktop().layer == LayerMask.NameToLayer("UI"))
                        return;
                }
            }

        //rotate view by holding left mouse click
        if (rotationEnabled && Input.GetMouseButton(0))
        {
            curRotationY += Input.GetAxis("Mouse X") * mouseSensitivity;
            curRotationX -= Input.GetAxis("Mouse Y") * mouseSensitivity;

            curRotationX = ClampAngle(curRotationX, minimumY, maximumY);
            curRotationY = ClampAngle(curRotationY, minimumX, maximumX);

            thisTransform.localRotation = Quaternion.Euler(new Vector3(curRotationX, curRotationY, 0));
        }

        //pan
        if (Input.GetMouseButton(2))
        {
            var x = Input.GetAxis("Mouse X") * panSensitivity;
            var y = Input.GetAxis("Mouse Y") * panSensitivity;

            thisTransform.position += thisTransform.TransformDirection(new Vector3(x, y));
        }

        //moves desktop player forward depending on scrollwheel
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
            thisTransform.position += thisTransform.TransformDirection(scrollSpeed * new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel")));//thisTransform.TransformPoint(scrollSpeed * new Vector3(0, 0, -Input.GetAxis("Mouse ScrollWheel")));


        //synchronize xr camera with desktop camera transforms
        teleportPlayer.SetXRPlayerPositionAndLocalRotation( thisTransform.position, thisTransform.localRotation );
    }

    /// Enables rotation and translation control for desktop environments.
    /// For mobile environments, it enables rotation or translation according to
    /// the device capabilities.
    void EnableAccordingToPlatform()
    {
        rotationEnabled = translationEnabled = !capabilities.canPresentVR;//.supportsImmersiveVR;
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f)
            angle += 360f;
        if (angle > 360f)
            angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

}
