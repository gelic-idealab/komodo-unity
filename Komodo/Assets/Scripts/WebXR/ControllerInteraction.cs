using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using WebXR;
using System.Collections;

#region INTERACTION TYPES
public enum INTERACTIONS
{
    LOOK = 0,
    LOOK_END = 1,
    RENDERING = 2,
    NOT_RENDERING = 3,
    GRAB = 4,
    DROP = 5,
    CHANGE_SCENE = 6,
    SLICE_OBJECT = 7,
    LOCK = 8,
    UNLOCK = 9,
    LINE = 10,
    LINE_END = 11
}
#endregion
[RequireComponent(typeof(WebXRController), typeof(Entity_Container))]
public class ControllerInteraction : MonoBehaviour, IUpdatable
{
    #region Controller Unity Events To Add Input From Editor
    [Header("Bottom Trigger BUTTON INVOKE EVENT")]
    public UnityEvent onTriggerButtonDown;
    public UnityEvent onTriggerButtonUp;

    [Header("Side Trigger BUTTON")]
    public UnityEvent onGripButtonDown;
    public UnityEvent onGripButtonUp;

    [Header("A/X BUTTON")]
    public UnityEvent onPrimaryButtonDown;
    public UnityEvent onPrimaryButtonUp;

    [Header("B/Y BUTTON")]
    public UnityEvent onSecondaryButtonDown;
    public UnityEvent onSecondaryButtonUp;

    [Header("Thumbstick BUTTON")]
    public UnityEvent onThumbstickButtonDown;
    public UnityEvent onThumbstickButtonUp;

    [Header("DIRECTIONAL THRESHOLD INVOCATIONS")]
    public UnityEvent onLeftFlick;
    public UnityEvent onRightFlick;
    public UnityEvent onDownFlick;
    public UnityEvent onUpFlick;

    private bool isHorAxisReset;
    private bool isVerAxisReset;
    #endregion

    //this hand field references
    private Transform thisTransform;
    private Animator thisAnimCont;
    private Collider thisCollider;
    private Rigidbody thisRigidBody;
    private bool hasObject;
    private Rigidbody currentRB;
    private Net_Register_GameObject currentNetRegisteredGameObject;
    private Transform currentTransform = null;
    private Transform currentParent = null;
    private WebXRController webXRController;
    private int handEntityType;

    //Static fields to recognize references between hands
    public static Transform curSharedParTransform;
    public static Transform firstObjectGrabbed;
    public static Transform secondObjectGrabbed;
    public static List<GameObject> handTransform;
    public static ControllerInteraction firstControllerInteraction;
    public static ControllerInteraction secondControllerInteraction;
  
    //initial Data when Double Grabbing -scalling and rotation 
    [HideInInspector] public bool isBothHandsHaveObject;
    private bool isInitialDoubleGrab;
    private Quaternion doubleGrabInitialRotation;
    private float doubleGrabinitialDistance;
    private Vector3 doublGrabinitialScale;
    private Vector3 initialOffsetFromHandToGrabbedObject;
    private Quaternion initialPlayerRotation;
    private float initialScaleRatioBasedOnDistance;
    float initialZCoord;
    float initialYCoord;

    [Header("Rigidbody Properties")]
    public float throwForce = 3f;
    private Vector3 oldPos;
    private Vector3 newPos;
    private Vector3 velocity;

    //Hierarchy used to set correct Pivot points for scalling and rotating objects on DoubleGrab
    private static Transform pivotRootTransform;             ///PARENT SCALE PIVOT1 CONTAINER
    private static Transform pivotChildTransform;             ///-CHILD SCALE PIVOT2 CONTAINER
    private static Transform doubleGrabRotationTransform;       //--Child for rotations

    //coordinate system to use to tilt double grand object appropriately: pulling, pushing, hand lift, and hand lower
     public static Transform doubleGrabRotateCoordinate;

    //Reference to the parent of our hands and the transform that will be rotating the player 
    private Transform handParentForContainerPlacement;

    void Awake()
    {
        #region Parent Setup for Scalling and Rotation
        //Only construct pivots and tilt parents in the first instance, provide reference to all  
        //other scripts by setting them to static fields.
        if (firstControllerInteraction == null)
        {
            firstControllerInteraction = this;

            //create root parent and share it through scripts by setting it to a static field
            pivotRootTransform = new GameObject("PIVOT_ROOT").transform;
            //place object one level up from hand to avoid getting our hand rotations
            pivotRootTransform.parent = transform.parent;

            //construct coordinate system to reference for tilting double grab object 
            doubleGrabRotateCoordinate = new GameObject("DoubleGrabCoordinateForObjectTilt").transform;
            doubleGrabRotateCoordinate.SetParent(transform.root.parent, true);
            doubleGrabRotateCoordinate.localPosition = Vector3.zero;

            //parent used to set secondary hand pivot for scalling objects properly
            pivotChildTransform = new GameObject("Pivot Point 2 Parent").transform;
            pivotChildTransform.SetParent(pivotRootTransform, true);
            pivotChildTransform.localPosition = Vector3.zero;

            //parent used for rotating or doble grab object
            doubleGrabRotationTransform = new GameObject("Rotation Parent_3").transform;
            doubleGrabRotationTransform.SetParent(pivotChildTransform, true);
            doubleGrabRotationTransform.localPosition = Vector3.zero;

        }
        else
            secondControllerInteraction = this;

        #endregion
    }
    void Start()
    {
        #region Establish Parent Setup References for Both Hands
       
       

        //set references for parent
        handParentForContainerPlacement = pivotRootTransform.parent;

        //add both hands
        handTransform = new List<GameObject>(2);
        handTransform.AddRange(GameObject.FindGameObjectsWithTag("Hand"));

        //set references
        (thisTransform, thisAnimCont, thisRigidBody, webXRController) = (transform, gameObject.GetComponent<Animator>(), GetComponent<Rigidbody>(), gameObject.GetComponent<WebXRController>());

        //identify the entity type for network calls
        handEntityType = (int)GetComponent<Entity_Container>().entity_data.current_Entity_Type;

        //set old pos for physics calculations
        oldPos = thisTransform.position;
        #endregion

        //Register this update loop
        GameStateManager.Instance.RegisterUpdatableObject(this);
    }

   
    #region Expression-bodied members For Input 
    float hand_Anim_NormalizedTime => webXRController.GetButton("Trigger") ? 1 : webXRController.GetAxis("Grip");
    bool isPrimaryButtonDown => webXRController.GetButtonDown("PrimaryButton");
    bool isPrimaryButtonUp => webXRController.GetButtonUp("PrimaryButton");
    bool isSecondaryButtonDown => webXRController.GetButtonDown("SecondaryButton");
    bool isSecondaryButtonUp => webXRController.GetButtonUp("SecondaryButton");

    bool isThumbstickButtonDown => webXRController.GetButtonDown("ThumbstickPress");
    bool isThumbstickButtonUp => webXRController.GetButtonUp("ThumbstickPress");
    float horAxis => webXRController.GetAxis("ThumbstickX");
    float verAxis => webXRController.GetAxis("ThumbstickY");
    #endregion

    public void OnUpdate(float realTime)
    {
        #region Hand Velocity Information
        //to enable throwing physics objects
        if (currentTransform)
        {
            if (currentRB)
            {
                newPos = thisTransform.position;
                var dif = newPos - oldPos;
                velocity = dif / Time.deltaTime;
                oldPos = newPos;
            }
        }
        #endregion

        #region Hand Input values

        float triggerThreshold = 0.5f;
        float gripThreshold = 0.5f;

#if UNITY_EDITOR

        bool isTriggerButtonDown = webXRController.GetAxis("Trigger") > triggerThreshold;
        bool isTriggerButtonUp = webXRController.GetAxis("Trigger") <= triggerThreshold;
        bool isGripButtonDown = webXRController.GetAxis("Grip") > gripThreshold;

        //is called every frame if not
        bool isGripButtonUp = false;

#elif UNITY_WEBGL

        bool isTriggerButtonDown = webXRController.GetButtonDown("Trigger");
        bool isTriggerButtonUp = webXRController.GetButtonUp("Trigger");
        bool isGripButtonDown = webXRController.GetButtonDown("Grip");
        bool isGripButtonUp = webXRController.GetButtonUp("Grip");
        
#endif
        #endregion

        #region Hand Input Calls
        //Set anim current state depending on grip and trigger pressure
        thisAnimCont.Play("Take", -1, hand_Anim_NormalizedTime);

        if (isGripButtonDown)
        {
            onGripButtonDown.Invoke();
            PickUp();
        }

        if (isGripButtonUp)
        {
            onGripButtonUp.Invoke();
            Drop();
        }

        if (isTriggerButtonUp)
        {
            onTriggerButtonUp.Invoke();
        }

        if (isTriggerButtonDown)
        {
            onTriggerButtonDown.Invoke();
        }

        //A button - primarybutton
        if (isPrimaryButtonDown)
        {
            onPrimaryButtonDown.Invoke();
        }

        if (isPrimaryButtonUp)
        {
            onPrimaryButtonUp.Invoke();
        }

        if (isSecondaryButtonDown)
        {
            onSecondaryButtonDown.Invoke();
        }

        if (isSecondaryButtonUp)
        {
            onSecondaryButtonUp.Invoke();
        }


        //Reset Horizontal Flick
        if (horAxis >= -0.5f && horAxis <= 0.5f)
        {
            isHorAxisReset = true;
        }

        //Left flick
        if (horAxis < -0.5f && isHorAxisReset)
        {
            isHorAxisReset = false;
            onRightFlick.Invoke();
        }

        //Right flick
        if (horAxis > 0.5f && isHorAxisReset)
        {
            isHorAxisReset = false;
            onLeftFlick.Invoke();
        }

        //Reset Vertical Flick
        if (verAxis >= -0.5f && verAxis <= 0.5f)
        {
            isVerAxisReset = true;
        }

        if (verAxis < -0.5f && isVerAxisReset)
        {
            isVerAxisReset = false;
            onDownFlick.Invoke();
        }

        if (verAxis > 0.5f && isVerAxisReset)
        {
            isVerAxisReset = false;
            onUpFlick.Invoke();
        }

        if (isThumbstickButtonDown)
            onThumbstickButtonDown.Invoke();

        if (isThumbstickButtonUp)
            onThumbstickButtonUp.Invoke();
        #endregion

        #region DoubleHand Funcionality

        //Called every update when grabbing same item
        if (isBothHandsHaveObject)
        {
            if (firstObjectGrabbed == null)
            {
                isBothHandsHaveObject = false;
                return;
            }
            //set values 
            if (isInitialDoubleGrab == false)
            {
                //inficates to run this only once at start to get initial values to use in update loop
                isInitialDoubleGrab = true;

                //grab values to know how we should start affecting our object 
                doubleGrabinitialDistance = Vector3.Distance(handTransform[0].transform.position, handTransform[1].transform.position);
                doublGrabinitialScale = pivotRootTransform.localScale;
                pivotChildTransform.rotation = handParentForContainerPlacement.rotation;

                //reset values for our container objects that we use to deform and rotate objects
                doubleGrabRotationTransform.rotation = Quaternion.identity;
                pivotRootTransform.localScale = Vector3.one;

                //set reference vector to tilt our grabed object on - left hand looks at right and sets tilt according to movement of origin or lookat target 
                doubleGrabRotateCoordinate.LookAt((handTransform[1].transform.position - handTransform[0].transform.position), Vector3.up);

                //Get the inverse of the initial rotation to use in update loop to avoid moving the object when grabbing   
                doubleGrabInitialRotation = Quaternion.Inverse(doubleGrabRotateCoordinate.rotation * handParentForContainerPlacement.rotation) ;

                //get rotational difference to be able to offset it apropriately in update loop
                var tiltRotation = doubleGrabInitialRotation * doubleGrabRotateCoordinate.rotation;

                //our initial orientation to use to tilt object, due to the way lookat behavior behaves we have to set x as Z 
                initialZCoord = tiltRotation.eulerAngles.x - doubleGrabRotationTransform.transform.eulerAngles.x;
                initialYCoord = tiltRotation.eulerAngles.y - doubleGrabRotationTransform.transform.eulerAngles.y;

                ////to fix parenting scalling down issue between centerpoint of hands and object
                initialOffsetFromHandToGrabbedObject = firstObjectGrabbed.transform.position - ((handTransform[1].transform.position + handTransform[0].transform.position) / 2);// - handParentForContainerPlacement.position;
    
                //pick up the rotation of our client to know when to update our offsets from hands to grab object
                initialPlayerRotation = handParentForContainerPlacement.rotation;
                return;
            }

            //a ratio between our current distance divided by our initial distance
            var scaleRatioBasedOnDistance = Vector3.Distance(handTransform[0].transform.position, handTransform[1].transform.position) / doubleGrabinitialDistance;

            if (float.IsNaN(firstObjectGrabbed.transform.localScale.y)) return;

            //we multiply our ratio with our initial scale
            pivotRootTransform.localScale = doublGrabinitialScale * scaleRatioBasedOnDistance;

            //place our grabbed object and second pivot away from the influeces of scale and rotation at first
            firstObjectGrabbed.SetParent(handParentForContainerPlacement, true);
            pivotChildTransform.SetParent(handParentForContainerPlacement, true);

            //SET PIVOT Location through our parents
            pivotRootTransform.position = secondControllerInteraction.thisTransform.position;
            pivotChildTransform.position = firstControllerInteraction.thisTransform.position;

            //place position of rotations to be in the center of both hands to rotate according to center point of hands not object center
            doubleGrabRotationTransform.position = ((handTransform[1].transform.position + handTransform[0].transform.position) / 2);

            //set our second pivot as a child of first to have a pivot for each hands
            pivotChildTransform.SetParent(pivotRootTransform, true);

           //set it to parent to modify rotation
            firstObjectGrabbed.SetParent(doubleGrabRotationTransform, true);






     





            // provides how an object should behave when double grabbing, object looks at one hand point of hand
            doubleGrabRotateCoordinate.LookAt((handTransform[1].transform.position - handTransform[0].transform.position), Vector3.up);

            //offset our current rotation from our initial difference to set
            var lookRot = doubleGrabInitialRotation * doubleGrabRotateCoordinate.rotation;

            //rotate y -> Yaw bring/push objects by pulling or pushing hand towards 
            var quat3 = Quaternion.AngleAxis( FreeFlightController.ClampAngle(  lookRot.eulerAngles.y - initialYCoord, -360,360)                , doubleGrabRotateCoordinate.up);
            //rotate z -> Roll shift objects right and left by lifting and lowering hands 
            var quat4 = Quaternion.AngleAxis(FreeFlightController.ClampAngle(initialZCoord - lookRot.eulerAngles.x, -360,360)                  , -doubleGrabRotateCoordinate.right);

            //add our rotatations
            doubleGrabRotationTransform.rotation = quat3 * quat4;// Quaternion.RotateTowards(doubleGrabRotationTransform.rotation, quat3 * quat4,60);// * handParentForContainerPlacement.rotation;

            //check for shifting of our player rotation to adjust our offset to prevent us from accumulating offsets that separates our grabbed object from hand
            if (handParentForContainerPlacement.eulerAngles.y != initialPlayerRotation.eulerAngles.y) 
            {
                initialPlayerRotation = handParentForContainerPlacement.rotation;
                initialOffsetFromHandToGrabbedObject =(firstObjectGrabbed.transform.position  ) - ((handTransform[1].transform.position + handTransform[0].transform.position) / 2);
                initialOffsetFromHandToGrabbedObject /= scaleRatioBasedOnDistance;
            }
             
            //modify object spacing offset when scalling using ratio between initial scale and currentscale
            firstObjectGrabbed.transform.position =  ((handTransform[1].transform.position + handTransform[0].transform.position) / 2) + (initialOffsetFromHandToGrabbedObject * scaleRatioBasedOnDistance);
        }
        #endregion
    }

    float speed = 60; // speed in degrees per second var turning: boolean = false; // this is true while turning
    bool turning = false;
    IEnumerator TurnAngle(float angle)
    {
        if (!turning)
        { // don't start another rotation while this one is running 
            turning = true; // tells that this rotation is running 
            var degrees = Mathf.Abs(angle); // calculate degrees to rotate 
            var dest = Quaternion.Euler(0, angle, 0) * doubleGrabRotationTransform.rotation;
            while (degrees > 0)
            {
                var dt = speed * Time.deltaTime; // rotate dt degrees this frame 
                doubleGrabRotationTransform.rotation = Quaternion.RotateTowards(doubleGrabRotationTransform.rotation, dest, dt);
                degrees -= dt; // subtract the angle rotated this frame 
                yield return null; // let Unity free till the next frame 
            }
            turning = false; // tells that rotation finished 

        }
    }

    IEnumerator Turn180()
    {
        while (turning) yield return null;
        TurnAngle(90);
        while (turning) yield return null;
        TurnAngle(90);
    }
    //public void OnDrawGizmos()
    //{
    //   // centerHandTransform.position = handTransform[0].transform.TransformPoint(Vector3.left * 1.5f);
    //    if (!isBothHandsHaveObject)
    //        return;
    //    Ray ray = new Ray(handTransform[0].transform.TransformPoint(Vector3.left * 3), Quaternion.LookRotation(handTransform[1].transform.position - (handTransform[0].transform.position + handTransform[0].transform.TransformPoint(Vector3.left)), Vector3.up) * Vector3.forward);
    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawRay(ray);

    //    Ray ray2 = new Ray(handTransform[0].transform.TransformPoint(Vector3.left * 3), Quaternion.LookRotation(handTransform[1].transform.position - (handTransform[0].transform.position + handTransform[0].transform.TransformPoint(Vector3.left)), Vector3.up) * Vector3.up);
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawRay(ray2);

    //    Ray ray3 = new Ray(handTransform[0].transform.TransformPoint(Vector3.left * 3), Quaternion.LookRotation(handTransform[1].transform.position - (handTransform[0].transform.position + handTransform[0].transform.TransformPoint(Vector3.left)), Vector3.up) * Vector3.right);
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawRay(ray3);
    //}

    [ContextMenu("PICK UP")]
    public void PickUp()
    {
        //Physics.SyncTransforms();
        if (!hasObject)
        {
            currentTransform = null;

            Collider[] colls = Physics.OverlapSphere(thisTransform.position, 0.1f);
            currentTransform = GetNearestRigidBody(colls);


            if (!currentTransform)
                return;

            hasObject = true;

            if (currentNetRegisteredGameObject)
            {
                //don't grab objects that are being grabbed by others avoid disqalifying user's second hand
                NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
                {
                    sourceEntity_id = int.Parse(ClientSpawnManager.Instance.mainPlayer_head.clientID.ToString() + handEntityType.ToString()),
                    targetEntity_id = currentNetRegisteredGameObject.entity_data.entityID,
                    interactionType = (int)INTERACTIONS.GRAB,
                });

                MainClientUpdater.Instance.PlaceInNetworkUpdateList(currentNetRegisteredGameObject);
            }

            if (currentRB)
                currentRB.isKinematic = true;

            if (firstControllerInteraction == this && firstObjectGrabbed == null)
            {
                firstObjectGrabbed = currentTransform;
            }
            //check second hand if it has object
            else if (secondControllerInteraction == this && secondObjectGrabbed == null)
            {
                secondObjectGrabbed = currentTransform;
            }

            //check if first hand has the same object as the second hand 
            if (firstObjectGrabbed == currentTransform && secondObjectGrabbed == currentTransform)
            {
                isBothHandsHaveObject = true;

                //SET WHOLE OBJECT PIVOT TO BE POSITION OF FIRST HAND THAT GRABBED OBJECT, ALLOWING FOR EXPANDING FROM FIRST HAND
                if (firstControllerInteraction == this)
                {
                    pivotRootTransform.position = secondControllerInteraction.thisTransform.position;
                }
                else if (secondControllerInteraction == this)
                {
                    pivotRootTransform.position = firstControllerInteraction.thisTransform.position;
                }

                //RESET AND SET PIVOT PARENT
                pivotRootTransform.transform.localScale = Vector3.one;
                firstObjectGrabbed.SetParent(pivotRootTransform, true);

                return;
            }

            //set parent to be this hand
            currentTransform.SetParent(thisTransform, true);

        }

    }

    [ContextMenu("DROP")]
    public void Drop()
    {
        if (hasObject)
        {

            //Both objects of each hands are present
            if (firstObjectGrabbed && secondObjectGrabbed)
            {
                //if same object double grab release setup
                if (firstObjectGrabbed == secondObjectGrabbed)
                {
                    if (secondControllerInteraction == this)
                    {
                        //reattach to other hand
                        secondObjectGrabbed.SetParent(firstControllerInteraction.thisTransform, true);

                    }
                    else if (firstControllerInteraction == this)
                    {
                        firstObjectGrabbed.SetParent(secondControllerInteraction.thisTransform, true);
                    }

                    //remove double grab scale updates
                    firstControllerInteraction.isBothHandsHaveObject = false;
                    secondControllerInteraction.isBothHandsHaveObject = false;

                }
                //if different object release appropriate object from hand
                else
                {
                    if (secondControllerInteraction == this)
                    {
                        //reatach object to its past parent
                        if (curSharedParTransform)
                            secondObjectGrabbed.SetParent(curSharedParTransform, true);
                        if (currentParent)
                            secondObjectGrabbed.SetParent(currentParent, true);
                    }
                    else if (firstControllerInteraction == this)
                    {
                      
                        if (curSharedParTransform)
                            firstObjectGrabbed.SetParent(curSharedParTransform, true);

                        if (currentParent)
                            firstObjectGrabbed.SetParent(currentParent, true);
                    }

                    //set physics 
                    ReleaseRigidBody();
                }

            }
            //We only have one object in our hands, check to remove appropriate object from whichever hand
            else if (firstObjectGrabbed == null || secondObjectGrabbed == null)
            {

                if(firstObjectGrabbed)
                {
                    if (curSharedParTransform)
                        firstObjectGrabbed.SetParent(curSharedParTransform, true);

                    if (currentParent) 
                        firstObjectGrabbed.SetParent(currentParent, true);
                }


               if (secondObjectGrabbed)
                {
                    //since we switch objects between hands we need a shararedparent static field to know what parent to place object in when releasing.
                    if (curSharedParTransform)
                        secondObjectGrabbed.SetParent(curSharedParTransform, true);

                    if (currentParent) 
                        secondObjectGrabbed.SetParent(currentParent, true);

                }
                ReleaseRigidBody();

                //only drop when object is the last thing to drop
                if (currentNetRegisteredGameObject)
                {
                    NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
                    {
                        sourceEntity_id = int.Parse(ClientSpawnManager.Instance.mainPlayer_head.clientID.ToString() + handEntityType.ToString()),
                        targetEntity_id = currentNetRegisteredGameObject.entity_data.entityID,
                        interactionType = (int)INTERACTIONS.DROP,
                    });


                    MainClientUpdater.Instance.RemoveFromInNetworkUpdateList(currentNetRegisteredGameObject);

                    //if droping a physics object update it for all.
                    if (currentRB)
                        if (!MainClientUpdater.Instance.physics_entityContainers_InNetwork_OutputList.Contains(currentNetRegisteredGameObject))
                            MainClientUpdater.Instance.physics_entityContainers_InNetwork_OutputList.Add(currentNetRegisteredGameObject);

                }

            }

            if (secondControllerInteraction == this)
                secondObjectGrabbed = null;
            else
                firstObjectGrabbed = null;

            //to reset information for double grab
            isInitialDoubleGrab = false;
            currentTransform = null;
            hasObject = false;
        }
    }

    //to allow for physics behavior when releasing object
    public void ReleaseRigidBody()
    {
        if (currentRB)
        {
            currentRB.isKinematic = false;
            currentRB.AddForce(velocity * throwForce, ForceMode.Impulse);
        }
    }

    //pick up our closest collider and obtain its references
    private Transform GetNearestRigidBody(Collider[] colliders)
    {
        float minDistance = float.MaxValue;
        float distance = 0.0f;
        List<Transform> transformToRemove = new List<Transform>();
        Collider nearestTransform = null;

        foreach (Collider col in colliders)
        {

            if (!col.CompareTag("Interactable"))
                continue;

            if (!col.gameObject.activeInHierarchy)
                continue;

            distance = (col.ClosestPoint(thisTransform.position) - thisTransform.position).sqrMagnitude; // (contactBody.position - thisTransform.position).sqrMagnitude;

            if (distance > 0.01f)
                continue;

            //   Debug.Log("pick up is called");
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestTransform = col;
            }
        }
        // didnt find nearest collider return null
        if (nearestTransform == null)
            return null;

        currentRB = null;
        currentParent = null;
        currentNetRegisteredGameObject = null;

        Transform nearPar = null;

        //set shared parent to reference when changing hands - set this ref when someone is picking up first object and//
        //whenever someone has on object on left hand then grabs that same object with the right hand, releases right hand to grab new object
        //with the left hand grab this new object - however, the shared parent is still the left

        //set last object to be picked up as the shared parent

        nearPar = nearestTransform.transform.parent;

        if (nearPar)
            if (nearPar != firstControllerInteraction.thisTransform && nearPar != secondControllerInteraction.thisTransform && nearPar != doubleGrabRotationTransform && nearPar != pivotRootTransform && handParentForContainerPlacement != nearPar)
            {
                curSharedParTransform = nearestTransform.transform.parent;
                currentParent = nearestTransform.transform.parent;
            }

        var netObj = nearestTransform.GetComponent<Net_Register_GameObject>();

        if (netObj)
        {
            //return null if the object is considered grabbed or locked
            if (netObj.entity_data.isCurrentlyGrabbed)
                return null;

            currentNetRegisteredGameObject = netObj;

            if (currentNetRegisteredGameObject.entity_data.current_Entity_Type == Entity_Type.physicsObject)
            {
                currentRB = currentNetRegisteredGameObject.GetComponent<Rigidbody>();

                if (currentRB == null)
                    Debug.LogWarning("No Rigid body on physics object Entity Type");
            }
        }
        return nearestTransform.transform;
    }

    public void OnDisable()
    {
        //send call to release object as last call
        if (currentTransform)
        {
            if (currentNetRegisteredGameObject)
            {
                NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
                {
                    sourceEntity_id = int.Parse(ClientSpawnManager.Instance.mainPlayer_head.clientID.ToString() + handEntityType.ToString()),
                    targetEntity_id = currentNetRegisteredGameObject.entity_data.entityID,
                    interactionType = (int)INTERACTIONS.DROP,
                });
            }
        }

        if (GameStateManager.IsAlive)
            GameStateManager.Instance.DeRegisterUpdatableObject(this);
    }

}

