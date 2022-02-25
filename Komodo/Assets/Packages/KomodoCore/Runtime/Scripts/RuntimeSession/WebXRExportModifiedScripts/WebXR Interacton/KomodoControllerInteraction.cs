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
        private const float handColliderRadius = 0.1f;

        public HandSelector handSelector;

        public TeleportPlayer teleportPlayer;

        [Header("Trigger Button")]

        [HideInInspector]
        public UnityEvent onTriggerButtonDown;

        [HideInInspector]
        public UnityEvent onTriggerButtonUp;

        [Header("Grip Button")]

        [HideInInspector]
        public UnityEvent onGripButtonDown;

        [HideInInspector]
        public UnityEvent onGripButtonUp;

        [Header("A / X Button")]
        [HideInInspector]
        public UnityEvent onPrimaryButtonDown;

        [HideInInspector]
        public UnityEvent onPrimaryButtonUp;

        [Header("B / Y Button")]
        public UnityEvent onSecondaryButtonDown;

        [HideInInspector]
        public UnityEvent onSecondaryButtonUp;

        [Header("Thumbstick Button")]
        public UnityEvent onThumbstickButtonDown;

        public UnityEvent onThumbstickButtonUp;

        [Header("Thumbstick Flick")]
        [HideInInspector]
        public UnityEvent onRightFlick;

        [HideInInspector]
        public UnityEvent onLeftFlick;

        [HideInInspector]
        public UnityEvent onDownFlick;

        [HideInInspector]
        public UnityEvent onUpFlick;

        private bool isHorAxisReset;

        private bool isVerAxisReset;

        //this hand field references
        private Transform thisHandTransform;

        private Animator thisHandAnimator;

        private bool hasObject;

        private Rigidbody currentGrabbedObjectRigidBody;

        private NetworkedGameObject currentGrabbedNetObject;

        [ShowOnly] public Transform hoveredObjectTransform = null;

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

        // TODO (Brandon): refactor this file into...
        // * GrabManager
        // * PhysicsManager
        // * StretchManager (update existing)
        // ... and then create LeftHand and RightHand components, which require ...
        // * Animator
        // * AvatarComponent

        void Awake ()
        {
            if (!handSelector)
            {
                Debug.LogError("You must set a handSelector on KomodoControllerInteraction", gameObject);
            }

            if (!teleportPlayer)
            {
                Debug.LogError("You must set a teleportPlayer on KomodoControllerInteraction", gameObject);
            }

            // TODO(Brandon, 2/24/22) -- Refactor this so it uses KomodoEventManager
            onTriggerButtonDown.AddListener(StartSelector);

            // TODO(Brandon, 2/24/22) -- Refactor this so it uses KomodoEventManager
            onTriggerButtonUp.AddListener(EndSelector);

            // TODO(Brandon, 2/24/22) -- Refactor this so it uses KomodoEventManager
            onTriggerButtonDown.AddListener(SendTriggerDownToControllerState);

            // TODO(Brandon, 2/24/22) -- Refactor this so it uses KomodoEventManager
            onTriggerButtonUp.AddListener(SendTriggerUpToControllerState);

            // TODO(Brandon, 2/24/22) -- Refactor this so it uses KomodoEventManager
            onGripButtonDown.AddListener(StartGrab);

            // TODO(Brandon, 2/24/22) -- Refactor this so it uses KomodoEventManager
            onGripButtonUp.AddListener(EndGrab);

            // TODO(Brandon, 2/24/22) -- Refactor this so it uses KomodoEventManager
            onGripButtonDown.AddListener(SendGripButtonDownToControllerState);

            // TODO(Brandon, 2/24/22) -- Refactor this so it uses KomodoEventManager
            onGripButtonUp.AddListener(SendGripButtonUpToControllerState);

            // TODO(Brandon, 2/24/22) -- Refactor this so it uses KomodoEventManager
            onLeftFlick.AddListener(teleportPlayer.SnapTurnLeft);

            // TODO(Brandon, 2/24/22) -- Refactor this so it uses KomodoEventManager
            onRightFlick.AddListener(teleportPlayer.SnapTurnRight);

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

        private void SetUpHands ()
        {
            thisHandTransform = transform;

            thisHandAnimator = gameObject.GetComponent<Animator>();

            webXRController = gameObject.GetComponent<WebXRController>();

            //identify the entity type for network calls
            handEntityType = (int) GetComponent<AvatarComponent>().thisEntityType;

            //set old pos for physics calculations
            oldPos = thisHandTransform.position;

            //SetControllerVisible(false);

            //SetHandJointsVisible(false);
        }

#if WEBXR_INPUT_PROFILES
        private void ControllerProfilesList(Dictionary<string, string> profilesList)
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
            UpdatePhysicsParameters();

            UpdateHandAnimationState();

            ProcessGripInput();

            ProcessTriggerInput();

            ProcessButtonsInput();

            ProcessThumbstickInput();
        }

        private void ProcessThumbstickInput()
        {
            float horAxis = webXRController.GetAxisIndexValue(2); //webXRController.GetAxis("ThumbstickX");

            float verAxis = webXRController.GetAxisIndexValue(3); //webXRController.GetAxis("ThumbstickY");

            //Reset Horizontal Flick
            if (horAxis >= -0.5f && horAxis <= 0.5f)
            {
                isHorAxisReset = true;
            }

            //Left flick
            if (horAxis < -0.5f && isHorAxisReset)
            {
                isHorAxisReset = false;

                onLeftFlick.Invoke();
            }

            //Right flick
            if (horAxis > 0.5f && isHorAxisReset)
            {
                isHorAxisReset = false;

                onRightFlick.Invoke();
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
        }

        private void ProcessButtonsInput()
        {
            // Primary Button
            if (webXRController.GetButtonDown(WebXRController.ButtonTypes.ButtonA))
            {
                onPrimaryButtonDown.Invoke();
            }

            if (webXRController.GetButtonUp(WebXRController.ButtonTypes.ButtonA))
            {
                onPrimaryButtonUp.Invoke();
            }

            // Secondary Button
            if (webXRController.GetButtonDown(WebXRController.ButtonTypes.ButtonB))
            {
                onSecondaryButtonDown.Invoke();
            }

            if (webXRController.GetButtonUp(WebXRController.ButtonTypes.ButtonB))
            {
                onSecondaryButtonUp.Invoke();
            }
        }

        private void ProcessTriggerInput()
        {
            if (webXRController.GetButtonUp(WebXRController.ButtonTypes.Trigger))
            {
                onTriggerButtonUp.Invoke();
            }

            if (webXRController.GetButtonDown(WebXRController.ButtonTypes.Trigger))
            {
                onTriggerButtonDown.Invoke();
            }
        }

        // TODO(Brandon, 2/24/22) -- Rename DoubleTapState in the project to ControllerState
        // TODO(Brandon, 2/24/22) -- Refactor this so it uses KomodoEventManager
        private void SendTriggerUpToControllerState ()
        {
            if (firstControllerOfStretchGesture == this)
            {
                DoubleTapState.Instance.leftHandTriggerPressed = false;
            }

            if (secondControllerOfStretchGesture == this)
            {
                DoubleTapState.Instance.rightHandTriggerPressed = false;
            }

            DoubleTapState.Instance.OnDoubleTriggerStateOff?.Invoke();
        }

        // TODO(Brandon, 2/24/22) -- Rename DoubleTapState in the project to ControllerState
        // TODO(Brandon, 2/24/22) -- Refactor this so it uses KomodoEventManager
        private void SendTriggerDownToControllerState ()
        {
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

        // TODO(Brandon, 2/24/22) -- Rename DoubleTapState in the project to ControllerState
        // TODO(Brandon, 2/24/22) -- Refactor this so it uses KomodoEventManager
        private void SendGripButtonUpToControllerState ()
        {
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

        // TODO(Brandon, 2/24/22) -- Rename DoubleTapState in the project to ControllerState
        // TODO(Brandon, 2/24/22) -- Refactor this so it uses KomodoEventManager
        private void SendGripButtonDownToControllerState ()
        {
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

        private void ProcessGripInput()
        {
            if (webXRController.GetButtonDown(WebXRController.ButtonTypes.Grip))
            {
                onGripButtonDown.Invoke();
            }

            if (webXRController.GetButtonUp(WebXRController.ButtonTypes.Grip))
            {
                onGripButtonUp.Invoke();
            }
        }

        private void UpdateHandAnimationState()
        {
            float handAnimationNormalizedTime = webXRController.GetButton(WebXRController.ButtonTypes.Trigger) ? 1 : webXRController.GetAxis(WebXRController.AxisTypes.Grip);

            //Set anim current state depending on grip and trigger pressure
            thisHandAnimator.Play("Take", -1, handAnimationNormalizedTime);
        }

        private void UpdatePhysicsParameters()
        {
            if (!hoveredObjectTransform || !currentGrabbedObjectRigidBody)
            {
                return;
            }

            newPos = thisHandTransform.position;

            var dif = newPos - oldPos;

            velocity = dif / Time.deltaTime;

            oldPos = newPos;
        }

#if WEBXR_INPUT_PROFILES
        private void ControllerProfilesList(Dictionary<string, string> profilesList)
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

        [ContextMenu("Test Start Selector")]
        public void StartSelector()
        {
            handSelector.gameObject.SetActive(true);
        }

        [ContextMenu("Test Stop Selector")]
        public void EndSelector()
        {
            handSelector.gameObject.SetActive(false);
        }

        [ContextMenu("Test Left Flick")]
        public void TestLeftFlick ()
        {
            onRightFlick.Invoke();
        }

        [ContextMenu("Test Right Flick")]
        public void TestRightFlick ()
        {
            onLeftFlick.Invoke();
        }

        [ContextMenu("Test Up Flick")]
        public void TestUpFlick ()
        {
            onUpFlick.Invoke();
        }

        [ContextMenu("Test Down Flick")]
        public void TestDownFlick ()
        {
            onDownFlick.Invoke();
        }

        [ContextMenu("Test Start Grab")]
        public void StartGrab()
        {
            if (hasObject)
            {
                return;
            }

            hoveredObjectTransform = FindHoveredObjectTransform();

            if (!hoveredObjectTransform)
            {
                return;
            }

            hasObject = true;

            if (currentGrabbedNetObject)
            {
                SendInteractionStartGrab();
            }

            InitializePhysicsParameters();

            InitializeStretchParameters();

            //check if first hand has the same object as the second hand 
            if (StretchManager.Instance.firstObjectGrabbed == hoveredObjectTransform && StretchManager.Instance.secondObjectGrabbed == hoveredObjectTransform)
            {
                StartStretch();

                return;
            }

            hoveredObjectTransform.SetParent(thisHandTransform, true);
        }

        private void InitializePhysicsParameters()
        {
            if (!currentGrabbedObjectRigidBody)
            {
                return;
            }

            currentGrabbedObjectRigidBody.isKinematic = true;
        }

        private Transform FindHoveredObjectTransform()
        {
            Collider[] colls = Physics.OverlapSphere(thisHandTransform.position, handColliderRadius);

            return GetNearestUnlockedNetObject(colls);
        }

        private void SendInteractionStartGrab()
        {
            var entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(currentGrabbedNetObject.Entity).entityID;

            NetworkUpdateHandler.Instance.SendSyncInteractionMessage(new Interaction
            {
                sourceEntity_id = int.Parse(NetworkUpdateHandler.Instance.client_id.ToString() + handEntityType.ToString()),

                targetEntity_id = entityID,

                interactionType = (int)INTERACTIONS.GRAB,
            });

            MainClientUpdater.Instance.AddUpdatable(currentGrabbedNetObject);

            entityManager.AddComponentData(currentGrabbedNetObject.Entity, new SendNetworkUpdateTag { });
        }

        private void InitializeStretchParameters()
        {
            if (firstControllerOfStretchGesture == this && StretchManager.Instance.firstObjectGrabbed == null)
            {
                StretchManager.Instance.firstObjectGrabbed = hoveredObjectTransform;
            }
            //check second hand if it has object
            else if (secondControllerOfStretchGesture == this && StretchManager.Instance.secondObjectGrabbed == null)
            {
                StretchManager.Instance.secondObjectGrabbed = hoveredObjectTransform;
            }
        }

        private void StartStretch()
        {
            StretchManager.Instance.onStretchStart.Invoke();

            //share our origin parent if it is null
            var firstObject = StretchManager.Instance.originalParentOfFirstHandTransform;

            var secondObject = StretchManager.Instance.originalParentOfSecondHandTransform;

            //share our parent since we are grabbing the same parent
            if (firstObject)
            {
                StretchManager.Instance.originalParentOfSecondHandTransform = firstObject;
            }

            if (secondObject)
            {
                StretchManager.Instance.originalParentOfFirstHandTransform = secondObject;
            }

            //SET WHOLE OBJECT PIVOT TO BE POSITION OF FIRST HAND THAT GRABBED OBJECT, ALLOWING FOR EXPANDING FROM FIRST HAND
            if (firstControllerOfStretchGesture == this)
            {
                StretchManager.Instance.endpoint1.position = secondControllerOfStretchGesture.thisHandTransform.position;
            }
            else if (secondControllerOfStretchGesture == this)
            {
                StretchManager.Instance.endpoint1.position = firstControllerOfStretchGesture.thisHandTransform.position;
            }

            //RESET AND SET PIVOT PARENT
            StretchManager.Instance.endpoint1.transform.localScale = Vector3.one;

            StretchManager.Instance.firstObjectGrabbed.SetParent(StretchManager.Instance.endpoint1, true);
        }

        [ContextMenu("Test End Grab")]
        public void EndGrab()
        {
            if (!hasObject)
            {
                return;
            }

            //Both objects of each hands are present
            if (StretchManager.Instance.firstObjectGrabbed && StretchManager.Instance.secondObjectGrabbed)
            {
                if (StretchManager.Instance.firstObjectGrabbed == StretchManager.Instance.secondObjectGrabbed)
                {
                    EndStretch();
                }
                else
                {
                    EndGrabForOneObjectInOneHand();
                }

                ResetStretchParameters();

                hoveredObjectTransform = null;

                hasObject = false;

                return;
            }

            //We only have one object in our hands, check to remove appropriate object from whichever hand
            if (StretchManager.Instance.firstObjectGrabbed == null || StretchManager.Instance.secondObjectGrabbed == null)
            {
                RestoreStretchParentsIfNeeded();

                ThrowPhysicsObject();

                //only drop when object is the last thing to drop
                if (!currentGrabbedNetObject)
                {
                    ResetStretchParameters();

                    hoveredObjectTransform = null;

                    hasObject = false;

                    return;
                }

                var netIDComp = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(currentGrabbedNetObject.Entity);

                SendInteractionEndGrab(netIDComp);

                SendPhysicsEndGrab(netIDComp);
            }

            ResetStretchParameters();

            hoveredObjectTransform = null;

            hasObject = false;
        }

        private void SendInteractionEndGrab(NetworkEntityIdentificationComponentData netIDComp)
        {
            var entityID = netIDComp.entityID;

            NetworkUpdateHandler.Instance.SendSyncInteractionMessage(new Interaction
            {
                sourceEntity_id = int.Parse(NetworkUpdateHandler.Instance.client_id.ToString() + handEntityType.ToString()),

                targetEntity_id = entityID,

                interactionType = (int)INTERACTIONS.DROP,
            });

            MainClientUpdater.Instance.RemoveUpdatable(currentGrabbedNetObject);

            if (!entityManager.HasComponent<SendNetworkUpdateTag>(currentGrabbedNetObject.Entity))
            {
                Debug.LogWarning("Tried to remove SendNetworkUpdateTag from netObject, but the tag was not found.");

                return;
            }

            entityManager.RemoveComponent<SendNetworkUpdateTag>(currentGrabbedNetObject.Entity);
        }

        // TODO -- compare to SendInteractionEndGrab and see if we need to perform those actions as well.
        private void SendInteractionEndGrabAlternateVersion()
        {
            int entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(currentGrabbedNetObject.Entity).entityID;

            NetworkUpdateHandler.Instance.SendSyncInteractionMessage(new Interaction
            {
                sourceEntity_id = int.Parse(NetworkUpdateHandler.Instance.client_id.ToString() + handEntityType.ToString()),

                targetEntity_id = entityID,

                interactionType = (int)INTERACTIONS.DROP,
            });
        }

        private void EndStretch()
        {
            if (secondControllerOfStretchGesture == this)
            {
                //reattach to other hand
                StretchManager.Instance.secondObjectGrabbed.SetParent(firstControllerOfStretchGesture.thisHandTransform, true);
            }
            else if (firstControllerOfStretchGesture == this)
            {
                StretchManager.Instance.firstObjectGrabbed.SetParent(secondControllerOfStretchGesture.thisHandTransform, true);
            }

            StretchManager.Instance.onStretchEnd.Invoke();
        }

        private void EndGrabForOneObjectInOneHand()
        {
            if (secondControllerOfStretchGesture == this)
            {
                StretchManager.Instance.secondObjectGrabbed.SetParent(StretchManager.Instance.originalParentOfSecondHandTransform, true);
            }
            else if (firstControllerOfStretchGesture == this)
            {
                StretchManager.Instance.firstObjectGrabbed.SetParent(StretchManager.Instance.originalParentOfFirstHandTransform, true);
            }

            ThrowPhysicsObject();
        }

        private static void RestoreStretchParentsIfNeeded()
        {
            if (StretchManager.Instance.firstObjectGrabbed)
            {
                StretchManager.Instance.firstObjectGrabbed.SetParent(StretchManager.Instance.originalParentOfFirstHandTransform, true);
            }

            if (StretchManager.Instance.secondObjectGrabbed)
            {
                StretchManager.Instance.secondObjectGrabbed.SetParent(StretchManager.Instance.originalParentOfSecondHandTransform, true);
            }
        }

        private void SendPhysicsEndGrab(NetworkEntityIdentificationComponentData netIDComp)
                {
            //if droping a physics object update it for all.
            if (!currentGrabbedObjectRigidBody)
            {
                return;
            }

            if (!NetworkedPhysicsManager.Instance.physics_networkedEntities.Contains(currentGrabbedNetObject))
            {
                NetworkedPhysicsManager.Instance.physics_networkedEntities.Add(currentGrabbedNetObject);
            }

            if (Entity_Type.physicsObject != netIDComp.current_Entity_Type)
            {
                return;
            }

            if (entityManager.HasComponent<SendNetworkUpdateTag>(currentGrabbedNetObject.Entity))
            {
                return;
            }

            entityManager.AddComponent<SendNetworkUpdateTag>(currentGrabbedNetObject.Entity);
        }

        private void ResetStretchParameters ()
        {
            if (secondControllerOfStretchGesture == this)
            {
                StretchManager.Instance.secondObjectGrabbed = null;
            }
            else
            {
                StretchManager.Instance.firstObjectGrabbed = null;
            }

            StretchManager.Instance.didStartStretching = false;
        }

        public void ThrowPhysicsObject()
        {
            if (!currentGrabbedObjectRigidBody)
            {
                return;
            }

            currentGrabbedObjectRigidBody.isKinematic = false;

            currentGrabbedObjectRigidBody.AddForce(velocity * throwForce, ForceMode.Impulse);
        }

        //pick up our closest collider and obtain its references
        private Transform GetNearestUnlockedNetObject(Collider[] colliders)
        {
            float minDistance = float.MaxValue;

            float distance;

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

                distance = (col.ClosestPoint(thisHandTransform.position) - thisHandTransform.position).sqrMagnitude;

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

            currentGrabbedObjectRigidBody = null;

            currentGrabbedNetObject = null;

            SetNearestParentWhenChangingGrabbingHand(nearestTransform);

            return GetUnlockedNetworkedObjectTransformIfExists(nearestTransform);
        }

        private Transform GetUnlockedNetworkedObjectTransformIfExists(Collider nearestTransform)
        {
            if (nearestTransform.TryGetComponent(out NetworkedGameObject netObj))
            {
                if (entityManager.HasComponent<TransformLockTag>(netObj.Entity))
                {
                    // TODO(Brandon) -- instead of returning null here, keep searching for the next rigid body. Otherwise, we trap smaller, unlocked objects inside larger, locked objects.

                    return null;
                }

                currentGrabbedNetObject = netObj;

                InitializeNetworkedPhysicsObjectIfNeeded();
            }

            return nearestTransform.transform;
        }

        private void SetNearestParentWhenChangingGrabbingHand(Collider nearestTransform)
        {
            Transform nearestParent = nearestTransform.transform.parent;

            //set shared parent to reference when changing hands - set this ref when someone is picking up first object and
            //whenever someone has on object on left hand then grabs that same object with the right hand, releases right hand to grab new object
            //with the left hand grab this new object - however, the shared parent is still the left
            //set last object to be picked up as the shared parent

            if (!nearestParent)
            {
                return;
            }

            if (nearestParent == firstControllerOfStretchGesture.thisHandTransform || nearestParent == secondControllerOfStretchGesture.thisHandTransform || nearestParent == StretchManager.Instance.midpoint || nearestParent == StretchManager.Instance.endpoint1 || StretchManager.Instance.stretchParent == nearestParent)
            {
                return;
            }

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

        private void InitializeNetworkedPhysicsObjectIfNeeded()
        {
            Entity_Type netObjectType = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(currentGrabbedNetObject.Entity).current_Entity_Type;

            if (netObjectType != Entity_Type.physicsObject)
            {
                return;
            }

            currentGrabbedObjectRigidBody = currentGrabbedNetObject.GetComponent<Rigidbody>();

            if (currentGrabbedObjectRigidBody == null)
            {
                Debug.LogWarning("No Rigid body on physics object Entity Type");
            }
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
            if (hoveredObjectTransform && currentGrabbedNetObject)
            {
                SendInteractionEndGrabAlternateVersion();
            }

            if (GameStateManager.IsAlive)
            {
                GameStateManager.Instance.DeRegisterUpdatableObject(this);
            }
        }
    }
}
