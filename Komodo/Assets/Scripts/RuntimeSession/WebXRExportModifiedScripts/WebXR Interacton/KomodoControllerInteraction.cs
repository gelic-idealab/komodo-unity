using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using WebXR;
using Unity.Entities;
using Komodo.Utilities;

namespace Komodo.Runtime
{
    [RequireComponent(typeof(WebXRController), typeof(AvatarComponent))]
    public class KomodoControllerInteraction : MonoBehaviour, IUpdatable
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
        private NetworkedGameObject currentNetRegisteredGameObject;
        [ShowOnly] public Transform currentTransform = null;
       // private Transform currentParent = null;
        private WebXRController webXRController;
        private int handEntityType;

        public static KomodoControllerInteraction firstControllerInteraction;
        public static KomodoControllerInteraction secondControllerInteraction;

        [Header("Rigidbody Properties")]
        public float throwForce = 3f;
        private Vector3 oldPos;
        private Vector3 newPos;
        private Vector3 velocity;

        //used for detecting and setting grouping parents into null (may need to make this more performant and enable to detects sending original null parents to nulls after manipulation
        private bool isGroupObject;

        //Reference to the parent of our hands and the transform that will be rotating the player 
       // private Transform handParentForContainerPlacement;

        private EntityManager entityManager;
        void Awake()
        {
            #region Parent Setup for Scalling and Rotation
            //Only construct pivots and tilt parents in the first instance, provide reference to all  
            //other scripts by setting them to static fields.
            if (firstControllerInteraction == null)
            {
                firstControllerInteraction = this;
            }
            else
                secondControllerInteraction = this;

            #endregion
        }
        void Start()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            #region Establish Parent Setup References for Both Hands

            //set references
            (thisTransform, thisAnimCont, thisRigidBody, webXRController) = (transform, gameObject.GetComponent<Animator>(), GetComponent<Rigidbody>(), gameObject.GetComponent<WebXRController>());

            //identify the entity type for network calls
            handEntityType = (int)GetComponent<AvatarComponent>().thisEntityType; //entity_data.current_Entity_Type;

            //set old pos for physics calculations
            oldPos = thisTransform.position;
            #endregion

            //SetControllerVisible(false);
            //SetHandJointsVisible(false);

            //Register this update loop
            GameStateManager.Instance.RegisterUpdatableObject(this);

        }

#if WEBXR_INPUT_PROFILES
    private void HandleProfilesList(Dictionary<string, string> profilesList)
    {
      if (profilesList == null || profilesList.Count == 0)
      {
        return;
      }
      hasProfileList = true;

      if (controllerVisible && useInputProfile)
      {
        SetControllerVisible(true);
      }
    }

    private void LoadInputProfile()
    {
      // Start loading possible profiles for the controller
      var profiles = controller.GetProfiles();
      if (hasProfileList && profiles != null && profiles.Length > 0)
      {
        loadedProfile = profiles[0];
        inputProfileLoader.LoadProfile(profiles, OnProfileLoaded);
      }
    }

    private void OnProfileLoaded(bool success)
    {
      if (success)
      {
        LoadInputModel();
      }
      // Nothing to do if profile didn't load
    }

    private void LoadInputModel()
    {
      inputProfileModel = inputProfileLoader.LoadModelForHand(
                          loadedProfile,
                          (InputProfileLoader.Handedness)controller.hand,
                          HandleModelLoaded);
      if (inputProfileModel != null)
      {
        // Update input state while still loading the model
        UpdateModelInput();
      }
    }

    private void HandleModelLoaded(bool success)
    {
      loadedModel = success;
      if (loadedModel)
      {
        // Set parent only after successful loading, to not interupt loading in case of disabled object
        var inputProfileModelTransform = inputProfileModel.transform;
        inputProfileModelTransform.SetParent(inputProfileModelParent.transform);
        inputProfileModelTransform.localPosition = Vector3.zero;
        inputProfileModelTransform.localRotation = Quaternion.identity;
        inputProfileModelTransform.localScale = Vector3.one;
        if (controllerVisible)
        {
          contactRigidBodies.Clear();
          inputProfileModelParent.SetActive(true);
          foreach (var visual in controllerVisuals)
          {
            visual.SetActive(false);
          }
        }
      }
      else
      {
        Destroy(inputProfileModel.gameObject);
      }
    }

    private void UpdateModelInput()
    {
      for (int i = 0; i < 6; i++)
      {
        SetButtonValue(i);
      }
      for (int i = 0; i < 4; i++)
      {
        SetAxisValue(i);
      }
    }

    private void SetButtonValue(int index)
    {
      inputProfileModel.SetButtonValue(index, controller.GetButtonIndexValue(index));
    }

    private void SetAxisValue(int index)
    {
      inputProfileModel.SetAxisValue(index, controller.GetAxisIndexValue(index));
    }
#endif

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

            #region Hand Input Calls

            float hand_Anim_NormalizedTime = webXRController.GetButton(WebXRController.ButtonTypes.Trigger) ? 1 : webXRController.GetAxis(WebXRController.AxisTypes.Grip);

            //Set anim current state depending on grip and trigger pressure
            thisAnimCont.Play("Take", -1, hand_Anim_NormalizedTime);

            if (webXRController.GetButtonDown(WebXRController.ButtonTypes.Grip))
            {
                onGripButtonDown.Invoke();
                PickUp();


                if (firstControllerInteraction == this)
                    DoubleTapState.Instance.leftHandGripPressed = true;

                if (secondControllerInteraction == this)
                    DoubleTapState.Instance.rightHandGripPressed = true;

                if (DoubleTapState.Instance.leftHandGripPressed == true && DoubleTapState.Instance.rightHandGripPressed == true)
                    DoubleTapState.Instance.OnDoubleGripStateOn?.Invoke();


            }

            if (webXRController.GetButtonUp(WebXRController.ButtonTypes.Grip))
            {
                onGripButtonUp.Invoke();
                Drop();


                if (firstControllerInteraction == this)
                    DoubleTapState.Instance.leftHandGripPressed = false;

                if (secondControllerInteraction == this)
                    DoubleTapState.Instance.rightHandGripPressed = false;

                DoubleTapState.Instance.OnDoubleGripStateOff?.Invoke();
            }

            if (webXRController.GetButtonUp(WebXRController.ButtonTypes.Trigger))
            {
                onTriggerButtonUp.Invoke();

                //set the state of our current controller press
                if(firstControllerInteraction == this)
                   DoubleTapState.Instance.leftHandTriggerPressed = false;

                if (secondControllerInteraction == this)
                    DoubleTapState.Instance.rightHandTriggerPressed = false;


                DoubleTapState.Instance.OnDoubleTriggerStateOff?.Invoke();
                //if (DoubleTapState.Instance.leftHandTrigger == false && DoubleTapState.Instance.rightHandTrigger == false)
                //    DoubleTapState.Instance.OnDoubleGripStateOff?.Invoke();
                //.gripTicks = -1;
            }

            if (webXRController.GetButtonDown(WebXRController.ButtonTypes.Trigger))
            {
                onTriggerButtonDown.Invoke();


                if (firstControllerInteraction == this)
                    DoubleTapState.Instance.leftHandTriggerPressed = true;

                if (secondControllerInteraction == this)
                    DoubleTapState.Instance.rightHandTriggerPressed = true;

                if (DoubleTapState.Instance.leftHandTriggerPressed == true && DoubleTapState.Instance.rightHandTriggerPressed == true)
                    DoubleTapState.Instance.OnDoubleTriggerStateOn?.Invoke();
            }

            //A button - primarybutton
            if (webXRController.GetButtonDown(WebXRController.ButtonTypes.ButtonA))
                onPrimaryButtonDown.Invoke();

            if (webXRController.GetButtonUp(WebXRController.ButtonTypes.ButtonA))
                onPrimaryButtonUp.Invoke();

            if (webXRController.GetButtonDown(WebXRController.ButtonTypes.ButtonB))
                onSecondaryButtonDown.Invoke();

            if (webXRController.GetButtonUp(WebXRController.ButtonTypes.ButtonB))
                onSecondaryButtonUp.Invoke();

            float horAxis = webXRController.GetAxisIndexValue(2);//webXRController.GetAxis("ThumbstickX");
            float verAxis = webXRController.GetAxisIndexValue(3);//webXRController.GetAxis("ThumbstickY");

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

            if (webXRController.GetButtonDown(WebXRController.ButtonTypes.Thumbstick))
                onThumbstickButtonDown.Invoke();

            if (webXRController.GetButtonUp(WebXRController.ButtonTypes.Thumbstick))
                onThumbstickButtonUp.Invoke();
            #endregion

        }
#if WEBXR_INPUT_PROFILES
    private void HandleProfilesList(Dictionary<string, string> profilesList)
    {
      if (profilesList == null || profilesList.Count == 0)
      {
        return;
      }
      hasProfileList = true;

      if (controllerVisible && useInputProfile)
      {
        SetControllerVisible(true);
      }
    }

    private void LoadInputProfile()
    {
      // Start loading possible profiles for the controller
      var profiles = controller.GetProfiles();
      if (hasProfileList && profiles != null && profiles.Length > 0)
      {
        loadedProfile = profiles[0];
        inputProfileLoader.LoadProfile(profiles, OnProfileLoaded);
      }
    }

    private void OnProfileLoaded(bool success)
    {
      if (success)
      {
        LoadInputModel();
      }
      // Nothing to do if profile didn't load
    }

    private void LoadInputModel()
    {
      inputProfileModel = inputProfileLoader.LoadModelForHand(
                          loadedProfile,
                          (InputProfileLoader.Handedness)controller.hand,
                          HandleModelLoaded);
      if (inputProfileModel != null)
      {
        // Update input state while still loading the model
        UpdateModelInput();
      }
    }

    private void HandleModelLoaded(bool success)
    {
      loadedModel = success;
      if (loadedModel)
      {
        // Set parent only after successful loading, to not interupt loading in case of disabled object
        var inputProfileModelTransform = inputProfileModel.transform;
        inputProfileModelTransform.SetParent(inputProfileModelParent.transform);
        inputProfileModelTransform.localPosition = Vector3.zero;
        inputProfileModelTransform.localRotation = Quaternion.identity;
        inputProfileModelTransform.localScale = Vector3.one;
        if (controllerVisible)
        {
          contactRigidBodies.Clear();
          inputProfileModelParent.SetActive(true);
          foreach (var visual in controllerVisuals)
          {
            visual.SetActive(false);
          }
        }
      }
      else
      {
        Destroy(inputProfileModel.gameObject);
      }
    }

    private void UpdateModelInput()
    {
      for (int i = 0; i < 6; i++)
      {
        SetButtonValue(i);
      }
      for (int i = 0; i < 4; i++)
      {
        SetAxisValue(i);
      }
    }

    private void SetButtonValue(int index)
    {
      inputProfileModel.SetButtonValue(index, controller.GetButtonIndexValue(index));
    }

    private void SetAxisValue(int index)
    {
      inputProfileModel.SetAxisValue(index, controller.GetAxisIndexValue(index));
    }
#endif

        [ContextMenu("PICK UP")]
        public void PickUp()
        {
            if (!hasObject)
            {
                #region OOD (Object Oriented) Funcionality PickUp GameObject

                currentTransform = null;

                //check we are doing a custom pick up of an object instead of checking environment for pickedup objects
                //if (!customPickup)
                //{
                Collider[] colls = Physics.OverlapSphere(thisTransform.position, 0.1f);
                currentTransform = GetNearestRigidBody(colls);
                //}
                //else
                //    currentTransform = customPickup;

                if (!currentTransform)
                    return;

                hasObject = true;

                if (currentNetRegisteredGameObject)
                {
                    var entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(currentNetRegisteredGameObject.Entity).entityID;
                    //don't grab objects that are being grabbed by others avoid disqalifying user's second hand
                    NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
                    {
                        sourceEntity_id = int.Parse(NetworkUpdateHandler.Instance.client_id.ToString() + handEntityType.ToString()),
                        targetEntity_id = entityID,
                        interactionType = (int)INTERACTIONS.GRAB,
                    });

                    MainClientUpdater.Instance.PlaceInNetworkUpdateList(currentNetRegisteredGameObject);

                    entityManager.AddComponentData(currentNetRegisteredGameObject.Entity, new SendNetworkUpdateTag { });
                }

                if (currentRB)
                    currentRB.isKinematic = true;

                if (firstControllerInteraction == this && StretchManager.Instance.firstObjectGrabbed == null)
                {
                    StretchManager.Instance.firstObjectGrabbed = currentTransform;
                }
                //check second hand if it has object
                else if (secondControllerInteraction == this && StretchManager.Instance.secondObjectGrabbed == null)
                {
                    StretchManager.Instance.secondObjectGrabbed = currentTransform;
                }

                //check if first hand has the same object as the second hand 
                if (StretchManager.Instance.firstObjectGrabbed == currentTransform && StretchManager.Instance.secondObjectGrabbed == currentTransform)
                {
                    // GrabControlManager.Instance.isDoubleGrabbing = true;
                    StretchManager.Instance.onStretchStart.Invoke();

                     //share our origin parent if it is null
                     var FirstObject = StretchManager.Instance.originalParentOfFirstHandTransform;

                    var SecondObject = StretchManager.Instance.originalParentOfSecondHandTransform;

                    //share our parent since we are grabbing the same parent
                    if (FirstObject)
                        StretchManager.Instance.originalParentOfSecondHandTransform = FirstObject;

                    if (SecondObject)
                        StretchManager.Instance.originalParentOfFirstHandTransform = SecondObject;



                    //SET WHOLE OBJECT PIVOT TO BE POSITION OF FIRST HAND THAT GRABBED OBJECT, ALLOWING FOR EXPANDING FROM FIRST HAND
                    if (firstControllerInteraction == this)
                    {
                      StretchManager.Instance.endpoint1.position = secondControllerInteraction.thisTransform.position;


                    }
                    else if (secondControllerInteraction == this)
                    {
                        StretchManager.Instance.endpoint1.position = firstControllerInteraction.thisTransform.position;
                    }

                    //RESET AND SET PIVOT PARENT
                    StretchManager.Instance.endpoint1.transform.localScale = Vector3.one;
                    StretchManager.Instance.firstObjectGrabbed.SetParent(StretchManager.Instance.endpoint1, true);

                    return;
                }

                //set parent to be this hand
                currentTransform.SetParent(thisTransform, true);
            }
            #endregion
        }




        [ContextMenu("DROP")]
        public void Drop()
        {
            if (hasObject)
            {

                //Both objects of each hands are present
                if (StretchManager.Instance.firstObjectGrabbed && StretchManager.Instance.secondObjectGrabbed)
                {
                    //if same object double grab release setup
                    if (StretchManager.Instance.firstObjectGrabbed == StretchManager.Instance.secondObjectGrabbed)
                    {
                        if (secondControllerInteraction == this)
                        {
                            //reattach to other hand
                            StretchManager.Instance.secondObjectGrabbed.SetParent(firstControllerInteraction.thisTransform, true);

                        }
                        else if (firstControllerInteraction == this)
                        {
                            StretchManager.Instance.firstObjectGrabbed.SetParent(secondControllerInteraction.thisTransform, true);
                        }

                        //remove double grab scale updates
                        //   GrabControlManager.Instance.isDoubleGrabbing = false;
                        StretchManager.Instance.onStretchEnd.Invoke();
                        //firstControllerInteraction.isBothHandsHaveObject = false;
                        //secondControllerInteraction.isBothHandsHaveObject = false;

                    }
                    //if different object release appropriate object from hand
                    else
                    {
                        if (secondControllerInteraction == this)
                        {

                            //reatach object to its past parent
                            //if (curSharedParTransform)
                            //    GrabControlManager.Instance.secondObjectGrabbed.SetParent(curSharedParTransform, true);
                            //if (currentParent)
                            //    GrabControlManager.Instance.secondObjectGrabbed.SetParent(currentParent, true);
                            StretchManager.Instance.secondObjectGrabbed.SetParent(StretchManager.Instance.originalParentOfSecondHandTransform, true);
                        }
                        else if (firstControllerInteraction == this)
                        {

                            //if (curSharedParTransform)
                            //    GrabControlManager.Instance.firstObjectGrabbed.SetParent(curSharedParTransform, true);

                            //if (currentParent)
                            //    GrabControlManager.Instance.firstObjectGrabbed.SetParent(currentParent, true);
                            StretchManager.Instance.firstObjectGrabbed.SetParent(StretchManager.Instance.originalParentOfFirstHandTransform, true);
                        }

                        //set physics 
                        ReleaseRigidBody();
                    }

                }
                //We only have one object in our hands, check to remove appropriate object from whichever hand
                else if (StretchManager.Instance.firstObjectGrabbed == null || StretchManager.Instance.secondObjectGrabbed == null)
                {

                    if (StretchManager.Instance.firstObjectGrabbed)
                    {
                        StretchManager.Instance.firstObjectGrabbed.SetParent(StretchManager.Instance.originalParentOfFirstHandTransform, true);
                    }


                    if (StretchManager.Instance.secondObjectGrabbed)
                    {
                        StretchManager.Instance.secondObjectGrabbed.SetParent(StretchManager.Instance.originalParentOfSecondHandTransform, true);
                    }
                    ReleaseRigidBody();

                    //only drop when object is the last thing to drop
                    if (currentNetRegisteredGameObject)
                    {
                        var netIDComp = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(currentNetRegisteredGameObject.Entity);
                        var entityID = netIDComp.entityID;
                        NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
                        {
                            sourceEntity_id = int.Parse(NetworkUpdateHandler.Instance.client_id.ToString() + handEntityType.ToString()),
                            targetEntity_id = entityID,
                            interactionType = (int)INTERACTIONS.DROP,
                        });


                        MainClientUpdater.Instance.RemoveFromInNetworkUpdateList(currentNetRegisteredGameObject);

                        if (entityManager.HasComponent<SendNetworkUpdateTag>(currentNetRegisteredGameObject.Entity))
                            entityManager.RemoveComponent<SendNetworkUpdateTag>(currentNetRegisteredGameObject.Entity);


                        //if droping a physics object update it for all.
                        if (currentRB)
                        {
                            if (!MainClientUpdater.Instance.physics_entityContainers_InNetwork_OutputList.Contains(currentNetRegisteredGameObject))
                                MainClientUpdater.Instance.physics_entityContainers_InNetwork_OutputList.Add(currentNetRegisteredGameObject);

                            if (Entity_Type.physicsObject == netIDComp.current_Entity_Type)
                                if (!entityManager.HasComponent<SendNetworkUpdateTag>(currentNetRegisteredGameObject.Entity))
                                    entityManager.AddComponent<SendNetworkUpdateTag>(currentNetRegisteredGameObject.Entity);

                        }

                    }

                }

                if (secondControllerInteraction == this)
                {
                    StretchManager.Instance.secondObjectGrabbed = null;
                 //   GrabControlManager.Instance.originalParentOfSecondHandTransform = null;
                }
                else
                {
                    StretchManager.Instance.firstObjectGrabbed = null;
                   // GrabControlManager.Instance.originalParentOfFirstHandTransform = null;
                }
                //to reset information for double grab
                StretchManager.Instance.didStartStretching = false;
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
         //   currentParent = null;
            currentNetRegisteredGameObject = null;

            Transform nearPar = null;

            //set shared parent to reference when changing hands - set this ref when someone is picking up first object and//
            //whenever someone has on object on left hand then grabs that same object with the right hand, releases right hand to grab new object
            //with the left hand grab this new object - however, the shared parent is still the left

            //set last object to be picked up as the shared parent

            nearPar = nearestTransform.transform.parent;

            if (nearPar)
                if (nearPar != firstControllerInteraction.thisTransform && nearPar != secondControllerInteraction.thisTransform && nearPar != StretchManager.Instance.midpoint && nearPar != StretchManager.Instance.endpoint1 && StretchManager.Instance.stretchParent != nearPar)
                {
                    var parent = nearestTransform.transform.parent;

                    if (firstControllerInteraction == this)
                        StretchManager.Instance.originalParentOfFirstHandTransform = parent;

                    if (secondControllerInteraction == this)
                        StretchManager.Instance.originalParentOfSecondHandTransform = parent;

                }

            //to detect when we have a linked group and set its parenting to null (may need to reconfigure this to be simpler
            if (nearestTransform.TryGetComponent(out LinkedGroup lg))
                isGroupObject = true;
            else
                isGroupObject = false;



            //var netObj = nearestTransform.GetComponent<NetworkAssociatedGameObject>();
            if (nearestTransform.TryGetComponent(out NetworkedGameObject netObj))
            {
                if (entityManager.HasComponent<TransformLockTag>(netObj.Entity))
                    return null;

                currentNetRegisteredGameObject = netObj;

                Entity_Type netObjectType = default;
                netObjectType = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(currentNetRegisteredGameObject.Entity).current_Entity_Type;

                if (netObjectType == Entity_Type.physicsObject)
                {
                    currentRB = currentNetRegisteredGameObject.GetComponent<Rigidbody>();

                    if (currentRB == null)
                        Debug.LogWarning("No Rigid body on physics object Entity Type");
                }
            }
            return nearestTransform.transform;
        }

        public void OnEnable()
        {
            //webXRController.OnControllerActive += SetControllerVisible;
            //webXRController.OnHandActive += SetHandJointsVisible;
            //webXRController.OnHandUpdate += OnHandUpdate;
        }
        public void OnDisable()
        {
            //webXRController.OnControllerActive -= SetControllerVisible;
            //webXRController.OnHandActive -= SetHandJointsVisible;
            //webXRController.OnHandUpdate -= OnHandUpdate;



            //send call to release object as last call
            if (currentTransform)
            {
                if (currentNetRegisteredGameObject)
                {
                    int entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(currentNetRegisteredGameObject.Entity).entityID;

                    NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
                    {
                        sourceEntity_id = int.Parse(NetworkUpdateHandler.Instance.client_id.ToString() + handEntityType.ToString()),
                        targetEntity_id = entityID,
                        interactionType = (int)INTERACTIONS.DROP,
                    });
                }
            }

            if (GameStateManager.IsAlive)
                GameStateManager.Instance.DeRegisterUpdatableObject(this);
        }

    }
}
