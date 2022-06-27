//#define TESTING_BEFORE_BUILDING

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using WebXR;
using Unity.Entities;
using Komodo.Utilities;

namespace Komodo.Runtime
{
    /// <summary>
    /// This class wires up Unity events with different buttons on VR controllers. This is a huge file, and I suggest you to refactor it into different files if possible.
    /// </summary>
    [RequireComponent(typeof(WebXRController), typeof(AvatarComponent))]
    public class KomodoControllerInteraction : MonoBehaviour, IUpdatable
    {
        /// <summary>
        /// The pre-defined default collider radius for users' hands.
        /// </summary>
        private const float handColliderRadius = 0.1f;

        /// <summary>
        /// A UnityEvent for pressing the trigger button on in VR controller down.
        /// </summary>
        [Header("Trigger Button")]
        public UnityEvent onTriggerButtonDown;

        /// <summary>
        /// A UnityEvent for releasing the trigger button on in VR controller.
        /// </summary>
        public UnityEvent onTriggerButtonUp;

        /// <summary>
        /// A UnityEvent for pressing the grip button on in VR controller down.
        /// </summary>
        [Header("Grip Button")]
        public UnityEvent onGripButtonDown;

        /// <summary>
        /// A UnityEvent for releasing the grip button on in VR controller.
        /// </summary>
        public UnityEvent onGripButtonUp;

        /// <summary>
        /// A UnityEvent for pressing the primary button (A / X button) on in VR controller down.
        /// </summary>
        [Header("A / X Button")]
        public UnityEvent onPrimaryButtonDown;

        /// <summary>
        /// A UnityEvent for releasing the primary button (A / X button) on in VR controller.
        /// </summary>
        public UnityEvent onPrimaryButtonUp;

        /// <summary>
        /// A UnityEvent for pressing the primary button (B / Y button) on in VR controller down.
        /// </summary>
        [Header("B / Y Button")]
        public UnityEvent onSecondaryButtonDown;
        
        /// <summary>
        /// A UnityEvent for releasing the primary button (B / Y button) on in VR controller.
        /// </summary>
        public UnityEvent onSecondaryButtonUp;

        /// <summary>
        /// A UnityEvent for pressing the thumbstick down on in VR controller.
        /// </summary>
        [Header("Thumbstick Button")]
        public UnityEvent onThumbstickButtonDown;

        /// <summary>
        /// A UnityEvent for releasing the thumbstick button on VR controller.
        /// </summary>
        public UnityEvent onThumbstickButtonUp;


        /// <summary>
        /// A UnityEvent for left click with thumbstick.
        /// </summary>
        [Header("Thumbstick Flick")]
        public UnityEvent onLeftFlick;

        /// <summary>
        /// A UnityEvent for right click with thumbstick.
        /// </summary>
        public UnityEvent onRightFlick;

        /// <summary>
        /// A UnityEvent for down click with thumbstick.
        /// </summary>
        public UnityEvent onDownFlick;

        /// <summary>
        /// A UnityEvent for up click with thumbstick.
        /// </summary>
        public UnityEvent onUpFlick;

        /// <summary>
        /// A boolean value for whether horizontal axis is reset or not.
        /// </summary>
        private bool isHorAxisReset;

        /// <summary>
        /// A boolean value for whether vertical axis is reset or not.
        /// </summary>
        private bool isVerAxisReset;

        /// <summary>
        /// A user's hand reference.
        /// </summary>
        private Transform thisHandTransform;

        /// <summary>
        /// Animator for user's hand.
        /// </summary>
        private Animator thisHandAnimator;

        /// <summary>
        /// Whether the user has an object in his/her hand currently.
        /// </summary>
        private bool hasObject;

        /// <summary>
        /// Ridgidbody of the current grabbed object.
        /// </summary>
        private Rigidbody currentGrabbedObjectRigidBody;

        /// <summary>
        /// Networked type game object for the current grabbed object.
        /// </summary>
        private NetworkedGameObject currentGrabbedNetObject;

        [ShowOnly] public Transform hoveredObjectTransform = null;

        private WebXRController webXRController;

        /// <summary>
        /// Hand entity type; left hand is 1 and right hand is 2. Refer to <c>EntityType.cs</c>.
        /// </summary>
        private int handEntityType;

        /// <summary>
        /// The first controller.
        /// </summary>
        public static KomodoControllerInteraction firstControllerOfStretchGesture;

        /// <summary>
        /// The second controller.
        /// </summary>
        public static KomodoControllerInteraction secondControllerOfStretchGesture;

        /// <summary>
        /// Default force for any rigidbody.
        /// </summary>
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

        /// <summary>
        /// Pick one of the controllers as the KomodoControllerInteraction.
        /// </summary>
        void Awake ()
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

        /// <summary>
        /// Initialize entityManager and set up hands.
        /// </summary>
        void Start ()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            SetUpHands();

            //Register the OnUpdate Loop
            GameStateManager.Instance.RegisterUpdatableObject(this);
        }

        /// <summary>
        /// Set up animator, webXR controller, and entity type of users' hands.
        /// </summary>
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
        /// <summary>
        /// An update function that processes animation, different button inputs, and physics parameters in real time.
        /// </summary>
        /// <param name="realTime">time since the game started</param>
        public void OnUpdate(float realTime)
        {
            UpdatePhysicsParameters();

            UpdateHandAnimationState();

            ProcessGripInput();

            ProcessTriggerInput();

            ProcessButtonsInput();

            ProcessThumbstickInput();
        }

        /// <summary>
        /// Thumbstick input contains both flicking and pressing. Invoke a corresponding Unity event based on the user's actions, whether they are flicking or not.
        /// </summary>
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
        }

        /// <summary>
        /// Process both primary and secondary buttons inputs. Invoke the corresponding Unity events if an action is performed.
        /// </summary>
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

        /// <summary>
        /// Process trigger buttons inputs. Invoke the corresponding Unity events if an action is performed.
        /// </summary>
        private void ProcessTriggerInput()
        {
            if (webXRController.GetButtonUp(WebXRController.ButtonTypes.Trigger))
            {
                onTriggerButtonUp.Invoke();

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
        }

        /// <summary>
        /// Process grip buttons inputs. Invoke the corresponding Unity events if an action is performed.
        /// </summary>
        private void ProcessGripInput()
        {
            if (webXRController.GetButtonDown(WebXRController.ButtonTypes.Grip))
            {
                onGripButtonDown.Invoke();

                StartGrab();

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

                EndGrab();

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
        }

        /// <summary>
        /// Update hand's animation based on users' action.
        /// </summary>
        private void UpdateHandAnimationState()
        {
            float handAnimationNormalizedTime = webXRController.GetButton(WebXRController.ButtonTypes.Trigger) ? 1 : webXRController.GetAxis(WebXRController.AxisTypes.Grip);

            //Set anim current state depending on grip and trigger pressure
            thisHandAnimator.Play("Take", -1, handAnimationNormalizedTime);
        }

        /// <summary>
        /// Not sure what this does.
        /// </summary>
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

        [ContextMenu("Start Grab")]
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

        /// <summary>
        /// Send grab information (interaction type, ids of the grabbed objects) to <c>NetworkUpdateHandler</c>.
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
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

        [ContextMenu("End Grab")]
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

        /// <summary>
        /// Send interaction data to <c>NetworkUpdateHandler</c> when grab action has ended.
        /// </summary>
        /// <param name="netIDComp"></param>
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

        /// <summary>
        /// Another version of end-grab interaction, which will be sent to NetworkUpdateHandler.
        /// </summary>
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
