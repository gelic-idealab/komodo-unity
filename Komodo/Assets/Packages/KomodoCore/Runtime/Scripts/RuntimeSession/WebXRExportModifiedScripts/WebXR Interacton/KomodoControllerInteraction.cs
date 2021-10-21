//#define TESTING_BEFORE_BUILDING

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
        [Header("Trigger Button")]

        public UnityEvent onTriggerButtonDown;

        public UnityEvent onTriggerButtonUp;

        [Header("Grip Button")]

        public UnityEvent onGripButtonDown;

        public UnityEvent onGripButtonUp;

        [Header("A / X Button")]
        public UnityEvent onPrimaryButtonDown;

        public UnityEvent onPrimaryButtonUp;

        [Header("B / Y Button")]
        public UnityEvent onSecondaryButtonDown;

        public UnityEvent onSecondaryButtonUp;

        [Header("Thumbstick Button")]
        public UnityEvent onThumbstickButtonDown;

        public UnityEvent onThumbstickButtonUp;

        [Header("Thumbstick Flick")]
        public UnityEvent onLeftFlick;

        public UnityEvent onRightFlick;

        public UnityEvent onDownFlick;

        public UnityEvent onUpFlick;

        private bool isHorAxisReset;

        private bool isVerAxisReset;

        //this hand field references
        private Transform thisTransform;

        private Animator thisAnimCont;

        private Collider thisCollider;

        private Rigidbody thisRigidBody;

        private bool hasObject;

        private Rigidbody currentRB;

        private NetworkedGameObject currentNetObject;

        [ShowOnly] public Transform currentTransform = null;

        private WebXRController webXRController;

        private int handEntityType;

        public static KomodoControllerInteraction firstControllerOfStretchGesture;

        public static KomodoControllerInteraction secondControllerOfStretchGesture;

        [Header("Rigidbody Properties")]
        public float throwForce = 3f;

        private Vector3 oldPos;

        private Vector3 newPos;

        private Vector3 velocity;

        private EntityManager entityManager;

        void Awake()
        {
            if (firstControllerOfStretchGesture == null)
            {
                firstControllerOfStretchGesture = this;
            }
            else
            {
                secondControllerOfStretchGesture = this;
            }
        }

        void Start ()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            SetUpHands();

            //Register the OnUpdate Loop
            GameStateManager.Instance.RegisterUpdatableObject(this);
        }

        void SetUpHands ()
        {
            thisTransform = transform;

            thisAnimCont = gameObject.GetComponent<Animator>();

            thisRigidBody = GetComponent<Rigidbody>();

            webXRController = gameObject.GetComponent<WebXRController>();

            //identify the entity type for network calls
            handEntityType = (int)GetComponent<AvatarComponent>().thisEntityType;

            //set old pos for physics calculations
            oldPos = thisTransform.position;

            //SetControllerVisible(false);

            //SetHandJointsVisible(false);
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
                                    OnControllerModelLoaded);

            if (inputProfileModel != null)
            {
                // Update input state while still loading the model
                UpdateModelInput();
            }
        }

        private void OnControllerModelLoaded(bool success)
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
            if (currentTransform && currentRB)
            {
                newPos = thisTransform.position;

                var dif = newPos - oldPos;

                velocity = dif / Time.deltaTime;

                oldPos = newPos;
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

                if (firstControllerOfStretchGesture == this)
                {
                    DoubleTapState.Instance.leftHandGripPressed = true;
                }

                if (secondControllerOfStretchGesture == this)
                {
                    DoubleTapState.Instance.rightHandGripPressed = true;
                }

                if (DoubleTapState.Instance.leftHandGripPressed && DoubleTapState.Instance.rightHandGripPressed)
                {
                    DoubleTapState.Instance.OnDoubleGripStateOn?.Invoke();
                }
            }

            if (webXRController.GetButtonUp(WebXRController.ButtonTypes.Grip))
            {
                onGripButtonUp.Invoke();

                Drop();

                if (firstControllerOfStretchGesture == this)
                {
                    DoubleTapState.Instance.leftHandGripPressed = false;
                }

                if (secondControllerOfStretchGesture == this)
                {
                    DoubleTapState.Instance.rightHandGripPressed = false;
                }

                DoubleTapState.Instance.OnDoubleGripStateOff?.Invoke();
            }

            if (webXRController.GetButtonUp(WebXRController.ButtonTypes.Trigger))
            {
                onTriggerButtonUp.Invoke();

                //set the state of our current controller press
                if(firstControllerOfStretchGesture == this)
                {
                    DoubleTapState.Instance.leftHandTriggerPressed = false;
                }

                if (secondControllerOfStretchGesture == this)
                {
                    DoubleTapState.Instance.rightHandTriggerPressed = false;
                }

                DoubleTapState.Instance.OnDoubleTriggerStateOff?.Invoke();
                //if (DoubleTapState.Instance.leftHandTrigger == false && DoubleTapState.Instance.rightHandTrigger == false)
                //    DoubleTapState.Instance.OnDoubleGripStateOff?.Invoke();
                //.gripTicks = -1;
            }

            if (webXRController.GetButtonDown(WebXRController.ButtonTypes.Trigger))
            {
                onTriggerButtonDown.Invoke();

                if (firstControllerOfStretchGesture == this)
                {
                    DoubleTapState.Instance.leftHandTriggerPressed = true;
                }

                if (secondControllerOfStretchGesture == this)
                {
                    DoubleTapState.Instance.rightHandTriggerPressed = true;
                }

                if (DoubleTapState.Instance.leftHandTriggerPressed && DoubleTapState.Instance.rightHandTriggerPressed)
                {
                    DoubleTapState.Instance.OnDoubleTriggerStateOn?.Invoke();
                }
            }

            //A button - primarybutton
            if (webXRController.GetButtonDown(WebXRController.ButtonTypes.ButtonA))
            {
                onPrimaryButtonDown.Invoke();
            }

            if (webXRController.GetButtonUp(WebXRController.ButtonTypes.ButtonA))
            {
                onPrimaryButtonUp.Invoke();
            }

            if (webXRController.GetButtonDown(WebXRController.ButtonTypes.ButtonB))
            {
                onSecondaryButtonDown.Invoke();
            }

            if (webXRController.GetButtonUp(WebXRController.ButtonTypes.ButtonB))
            {
                onSecondaryButtonUp.Invoke();
            }

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
            {
                onThumbstickButtonDown.Invoke();
            }

            if (webXRController.GetButtonUp(WebXRController.ButtonTypes.Thumbstick))
            {
                onThumbstickButtonUp.Invoke();
            }
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
                            OnControllerModelLoaded);
        if (inputProfileModel != null)
        {
            // Update input state while still loading the model
            UpdateModelInput();
        }
        }

        private void OnControllerModelLoaded(bool success)
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

                if (currentNetObject)
                {
                    var entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(currentNetObject.Entity).entityID;
                    //don't grab objects that are being grabbed by others avoid disqalifying user's second hand
                    NetworkUpdateHandler.Instance.SendSyncInteractionMessage(new Interaction
                    {
                        sourceEntity_id = int.Parse(NetworkUpdateHandler.Instance.client_id.ToString() + handEntityType.ToString()),
                        targetEntity_id = entityID,
                        interactionType = (int)INTERACTIONS.GRAB,
                    });

                    MainClientUpdater.Instance.AddUpdatable(currentNetObject);

                    entityManager.AddComponentData(currentNetObject.Entity, new SendNetworkUpdateTag { });
                }

                if (currentRB)
                    currentRB.isKinematic = true;

                if (firstControllerOfStretchGesture == this && StretchManager.Instance.firstObjectGrabbed == null)
                {
                    StretchManager.Instance.firstObjectGrabbed = currentTransform;
                }
                //check second hand if it has object
                else if (secondControllerOfStretchGesture == this && StretchManager.Instance.secondObjectGrabbed == null)
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
                    if (firstControllerOfStretchGesture == this)
                    {
                      StretchManager.Instance.endpoint1.position = secondControllerOfStretchGesture.thisTransform.position;


                    }
                    else if (secondControllerOfStretchGesture == this)
                    {
                        StretchManager.Instance.endpoint1.position = firstControllerOfStretchGesture.thisTransform.position;
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
                        if (secondControllerOfStretchGesture == this)
                        {
                            //reattach to other hand
                            StretchManager.Instance.secondObjectGrabbed.SetParent(firstControllerOfStretchGesture.thisTransform, true);

                        }
                        else if (firstControllerOfStretchGesture == this)
                        {
                            StretchManager.Instance.firstObjectGrabbed.SetParent(secondControllerOfStretchGesture.thisTransform, true);
                        }

                        StretchManager.Instance.onStretchEnd.Invoke();
                    }
                    //if different object release appropriate object from hand
                    else
                    {
                        if (secondControllerOfStretchGesture == this)
                        {
                            StretchManager.Instance.secondObjectGrabbed.SetParent(StretchManager.Instance.originalParentOfSecondHandTransform, true);
                        }
                        else if (firstControllerOfStretchGesture == this)
                        {
                            StretchManager.Instance.firstObjectGrabbed.SetParent(StretchManager.Instance.originalParentOfFirstHandTransform, true);
                        }

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
                    if (currentNetObject)
                    {
                        var netIDComp = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(currentNetObject.Entity);
                        var entityID = netIDComp.entityID;
                        NetworkUpdateHandler.Instance.SendSyncInteractionMessage(new Interaction
                        {
                            sourceEntity_id = int.Parse(NetworkUpdateHandler.Instance.client_id.ToString() + handEntityType.ToString()),

                            targetEntity_id = entityID,

                            interactionType = (int)INTERACTIONS.DROP,
                        });

                        MainClientUpdater.Instance.RemoveUpdatable(currentNetObject);

                        if (entityManager.HasComponent<SendNetworkUpdateTag>(currentNetObject.Entity))
                        {
                            entityManager.RemoveComponent<SendNetworkUpdateTag>(currentNetObject.Entity);
                        }

                        //if droping a physics object update it for all.
                        if (currentRB)
                        {
                            if (!NetworkedPhysicsManager.Instance.physics_networkedEntities.Contains(currentNetObject))
                                NetworkedPhysicsManager.Instance.physics_networkedEntities.Add(currentNetObject);

                            if (Entity_Type.physicsObject == netIDComp.current_Entity_Type)
                            {
                                if (!entityManager.HasComponent<SendNetworkUpdateTag>(currentNetObject.Entity))
                                {
                                    entityManager.AddComponent<SendNetworkUpdateTag>(currentNetObject.Entity);
                                }
                            }
                        }
                    }
                }

                if (secondControllerOfStretchGesture == this)
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
                if (!col.CompareTag(TagList.interactable))
                {
                    continue;
                }

                if (!col.gameObject.activeInHierarchy)
                {
                    continue;
                }

                distance = (col.ClosestPoint(thisTransform.position) - thisTransform.position).sqrMagnitude;

                if (distance > 0.01f)
                {
                    continue;
                }

                if (distance < minDistance)
                {
                    minDistance = distance;

                    nearestTransform = col;
                }
            }

            if (nearestTransform == null)
            {
                return null;
            }

            currentRB = null;

            currentNetObject = null;

            Transform nearPar = null;

            //set shared parent to reference when changing hands - set this ref when someone is picking up first object and
            //whenever someone has on object on left hand then grabs that same object with the right hand, releases right hand to grab new object
            //with the left hand grab this new object - however, the shared parent is still the left

            //set last object to be picked up as the shared parent

            nearPar = nearestTransform.transform.parent;

            if (nearPar)
            {
                if (nearPar != firstControllerOfStretchGesture.thisTransform && nearPar != secondControllerOfStretchGesture.thisTransform && nearPar != StretchManager.Instance.midpoint && nearPar != StretchManager.Instance.endpoint1 && StretchManager.Instance.stretchParent != nearPar)
                {
                    var parent = nearestTransform.transform.parent;

                    if (firstControllerOfStretchGesture == this)
                    {
                        StretchManager.Instance.originalParentOfFirstHandTransform = parent;
                    }

                    if (secondControllerOfStretchGesture == this)
                    {
                        StretchManager.Instance.originalParentOfSecondHandTransform = parent;
                    }
                }
            }

            //var netObj = nearestTransform.GetComponent<NetworkAssociatedGameObject>();
            if (nearestTransform.TryGetComponent(out NetworkedGameObject netObj))
            {
                if (entityManager.HasComponent<TransformLockTag>(netObj.Entity))
                {
                    return null;
                }

                currentNetObject = netObj;

                Entity_Type netObjectType = default;

                netObjectType = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(currentNetObject.Entity).current_Entity_Type;

                if (netObjectType == Entity_Type.physicsObject)
                {
                    currentRB = currentNetObject.GetComponent<Rigidbody>();

                    if (currentRB == null)
                    {
                        Debug.LogWarning("No Rigid body on physics object Entity Type");
                    }
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
                if (currentNetObject)
                {
                    int entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(currentNetObject.Entity).entityID;

                    NetworkUpdateHandler.Instance.SendSyncInteractionMessage(new Interaction
                    {
                        sourceEntity_id = int.Parse(NetworkUpdateHandler.Instance.client_id.ToString() + handEntityType.ToString()),

                        targetEntity_id = entityID,

                        interactionType = (int)INTERACTIONS.DROP,
                    });
                }
            }

            if (GameStateManager.IsAlive)
            {
                GameStateManager.Instance.DeRegisterUpdatableObject(this);
            }
        }
    }
}
