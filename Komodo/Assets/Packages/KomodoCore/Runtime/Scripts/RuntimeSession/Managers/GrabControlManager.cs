using Komodo.Runtime;
using Komodo.Utilities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GrabControlManager : SingletonComponent<GrabControlManager>, IUpdatable
{
    public static GrabControlManager Instance
    {
        get { return ((GrabControlManager)_Instance); }
        set { _Instance = value; }
    }
    
    //check for left hand grabbing
   [ShowOnly] public Transform firstObjectGrabbed;
    //get parent if we are switching objects between hands we want to keep track of were to place it back, to avoid hierachy parenting displacement
    [ShowOnly] public Transform originalParentOfFirstHandTransform;


    //do the same as above for the right Hand
    [ShowOnly] public Transform secondObjectGrabbed;
    [ShowOnly] public Transform originalParentOfSecondHandTransform;

    [ShowOnly] public Transform[] handReferences = new Transform[2];//(2, Allocator.Persistent);

    //away to keep track if we are double grabbing a gameobject to call event;
    public UnityEvent onDoubleAssetGrab;
    public UnityEvent onDoubleAssetRelease;

    // public bool isDoubleGrabbing;

    //Fields to rotate object appropriately
    //Hierarchy used to set correct Pivot points for scalling and rotating objects on DoubleGrab
    [ShowOnly] public Transform pivotRootTransform;             ///PARENT SCALE PIVOT1 CONTAINER
    private static Transform pivotChildTransform;             ///-CHILD SCALE PIVOT2 CONTAINER
    [ShowOnly] public Transform doubleGrabRotationTransform;       //--Child for rotations

    [ShowOnly] public Transform handParentForContainerPlacement;

    //coordinate system to use to tilt double grand object appropriately: pulling, pushing, hand lift, and hand lower
    [ShowOnly] public Transform doubleGrabRotateCoordinate;

    //initial Data when Double Grabbing -scalling and rotation 
    [ShowOnly] public bool isInitialDoubleGrab;
    private Quaternion doubleGrabInitialRotation;
    private float doubleGrabinitialDistance;
    private Vector3 doublGrabinitialScale;
    private Vector3 initialOffsetFromHandToGrabbedObject;
    private Quaternion initialPlayerRotation;
    private float initialScaleRatioBasedOnDistance;
    float initialZCoord;
    float initialYCoord;

    public void Awake()
    {
        //used to set our managers alive state to true to detect if it exist within scene
        var initManager = Instance;

        //register our calls
        onDoubleAssetGrab.AddListener(()=>DoubleAssetGrab());
        onDoubleAssetRelease.AddListener(() => DoubleAssetRelease());

        //create hierarchy to rotate double grab objects appropriately
        //create root parent and share it through scripts by setting it to a static field
        pivotRootTransform = new GameObject("PIVOT_ROOT").transform;
        ////place object one level up from hand to avoid getting our hand rotations
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

    

    public void Start()
    {
       


        var player = GameObject.FindWithTag(TagList.player);

        if (player)
        {
           if(player.TryGetComponent(out PlayerReferences pR))
            {
                handReferences[0] = pR.handL;
                handReferences[1] = pR.handR;
            }
        }

        //set references for parent
        handParentForContainerPlacement = pivotRootTransform.parent;
    }

    //this is used for only running our update loop when necessary
    private void DoubleAssetGrab()
    {
        //register our update loop to be called
        if (GameStateManager.IsAlive)
            GameStateManager.Instance.RegisterUpdatableObject(this);
    }

    private void DoubleAssetRelease()
    {
        if (GameStateManager.IsAlive)
            GameStateManager.Instance.DeRegisterUpdatableObject(this);
    }

    public void OnUpdate(float realltime)
    {
        #region DoubleHand Funcionality
        
        //Called every update when grabbing same item
        //if (isDoubleGrabbing)
        //{
            //if (firstObjectGrabbed == null)
            //{
            //    isDoubleGrabbing = false;
            //    return;
            //}
            //set values 
            if (isInitialDoubleGrab == false)
            {
                //inficates to run this only once at start to get initial values to use in update loop
                isInitialDoubleGrab = true;

                //grab values to know how we should start affecting our object 
                doubleGrabinitialDistance = Vector3.Distance(handReferences[0].transform.position, handReferences[1].transform.position);
                doublGrabinitialScale = pivotRootTransform.localScale;
                pivotChildTransform.rotation = handParentForContainerPlacement.rotation;

                //reset values for our container objects that we use to deform and rotate objects
                doubleGrabRotationTransform.rotation = Quaternion.identity;
                pivotRootTransform.localScale = Vector3.one;

                //set reference vector to tilt our grabed object on - left hand looks at right and sets tilt according to movement of origin or lookat target 
                doubleGrabRotateCoordinate.LookAt((handReferences[1].transform.position - handReferences[0].transform.position), Vector3.up);

                //Get the inverse of the initial rotation to use in update loop to avoid moving the object when grabbing   
                doubleGrabInitialRotation = Quaternion.Inverse(doubleGrabRotateCoordinate.rotation * handParentForContainerPlacement.rotation);

                //get rotational difference to be able to offset it apropriately in update loop
                var tiltRotation = doubleGrabInitialRotation * doubleGrabRotateCoordinate.rotation;

                //our initial orientation to use to tilt object, due to the way lookat behavior behaves we have to set x as Z 
                initialZCoord = tiltRotation.eulerAngles.x - doubleGrabRotationTransform.transform.eulerAngles.x;
                initialYCoord = tiltRotation.eulerAngles.y - doubleGrabRotationTransform.transform.eulerAngles.y;

                ////to fix parenting scalling down issue between centerpoint of hands and object
                initialOffsetFromHandToGrabbedObject = firstObjectGrabbed.position - ((handReferences[1].transform.position + handReferences[0].transform.position) / 2);// - handParentForContainerPlacement.position;

                //pick up the rotation of our client to know when to update our offsets from hands to grab object
                initialPlayerRotation = handParentForContainerPlacement.rotation;
                return;
            }

            //a ratio between our current distance divided by our initial distance
            var scaleRatioBasedOnDistance = Vector3.Distance(handReferences[0].transform.position, handReferences[1].transform.position) / doubleGrabinitialDistance;

            if (float.IsNaN(firstObjectGrabbed.localScale.y)) return;

            //we multiply our ratio with our initial scale
            pivotRootTransform.localScale = doublGrabinitialScale * scaleRatioBasedOnDistance;

            //place our grabbed object and second pivot away from the influeces of scale and rotation at first
            firstObjectGrabbed.SetParent(handParentForContainerPlacement, true);
            pivotChildTransform.SetParent(handParentForContainerPlacement, true);

            //SET PIVOT Location through our parents
            pivotRootTransform.position = handReferences[1].transform.position;// secondControllerInteraction.thisTransform.position;
            pivotChildTransform.position = handReferences[0].transform.position;// firstControllerInteraction.thisTransform.position;

            //place position of rotations to be in the center of both hands to rotate according to center point of hands not object center
            doubleGrabRotationTransform.position = ((handReferences[1].transform.position + handReferences[0].transform.position) / 2);

            //set our second pivot as a child of first to have a pivot for each hands
            pivotChildTransform.SetParent(pivotRootTransform, true);

            //set it to parent to modify rotation
            firstObjectGrabbed.SetParent(doubleGrabRotationTransform, true);

            // provides how an object should behave when double grabbing, object looks at one hand point of hand
            doubleGrabRotateCoordinate.LookAt((handReferences[1].transform.position - handReferences[0].transform.position), Vector3.up);

            //offset our current rotation from our initial difference to set
            var lookRot = doubleGrabInitialRotation * doubleGrabRotateCoordinate.rotation;

            //rotate y -> Yaw bring/push objects by pulling or pushing hand towards 
            var quat3 = Quaternion.AngleAxis(FreeFlightController.ClampAngle(lookRot.eulerAngles.y - initialYCoord, -360, 360), doubleGrabRotateCoordinate.up);
            //rotate z -> Roll shift objects right and left by lifting and lowering hands 
            var quat4 = Quaternion.AngleAxis(FreeFlightController.ClampAngle(initialZCoord - lookRot.eulerAngles.x, -360, 360), -doubleGrabRotateCoordinate.right);

            //add our rotatations
            doubleGrabRotationTransform.rotation = quat3 * quat4;// Quaternion.RotateTowards(doubleGrabRotationTransform.rotation, quat3 * quat4,60);// * handParentForContainerPlacement.rotation;

            //check for shifting of our player rotation to adjust our offset to prevent us from accumulating offsets that separates our grabbed object from hand
            if (handParentForContainerPlacement.eulerAngles.y != initialPlayerRotation.eulerAngles.y)
            {
                initialPlayerRotation = handParentForContainerPlacement.rotation;
                initialOffsetFromHandToGrabbedObject = (firstObjectGrabbed.position) - ((handReferences[1].transform.position + handReferences[0].transform.position) / 2);
                initialOffsetFromHandToGrabbedObject /= scaleRatioBasedOnDistance;
            }

            //modify object spacing offset when scalling using ratio between initial scale and currentscale
            firstObjectGrabbed.position = ((handReferences[1].transform.position + handReferences[0].transform.position) / 2) + (initialOffsetFromHandToGrabbedObject * scaleRatioBasedOnDistance);
     //   }
        #endregion
    }
}
