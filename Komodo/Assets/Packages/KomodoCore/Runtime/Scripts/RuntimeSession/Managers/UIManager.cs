using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using Komodo.Utilities;

namespace Komodo.Runtime
{
    public class UIManager : SingletonComponent<UIManager>
    {
        public static UIManager Instance
        {
            get { return ((UIManager)_Instance); }
            set { _Instance = value; }
        }

        [Header("Player Menu")]
        
        [Tooltip("Hierarchy: KomodoMenu -> Panels -> SettingsMenu")]
        public GameObject settingsMenu;

        [Tooltip("Hierarchy: KomodoMenu -> Panels -> SettingsMenu -> HeightCalibration")]
        public GameObject heightCalibration;

        [Tooltip("Hierarchy: KomodoMenu -> Panels -> SettingsMenu -> NotCalibrating -> CalibrateHeightButton")]
        public GameObject calibrationButtons;

        [Tooltip("Hierarchy: KomodoMenu -> Panels -> SettingsMenu -> NotCalibrating -> ManuallyAdjustHeight")]
        public GameObject manuallyAdjustHeight;

        [Tooltip("Hierarchy: The create tab in player menu")]
        public GameObject createTab;

        [Tooltip("Hierarchy: The Instructor menu button in the settings tab")]
        public GameObject instructorMenuButton;
        
        public GameObject menuPrefab;

        [ShowOnly]
        public GameObject menu;

        [ShowOnly]
        public MainUIReferences menuReferences;

        [ShowOnly]
        public CanvasGroup menuCanvasGroup;

        [ShowOnly]
        public Canvas menuCanvas;

        [ShowOnly]
        private RectTransform menuTransform;

        [ShowOnly]
        public HoverCursor hoverCursor;

        private bool _isRightHanded;

        private bool _isMenuVisible;

        /// <summary>
        /// default scale for XR hands.
        /// </summary>
        public Vector3 eitherHandRectScale = new Vector3(0.001f, 0.001f, 0.001f);

        /// <summary>
        /// Default rotation for the left hand menu.
        /// </summary>
        public Vector3 leftHandedMenuRectRotation = new Vector3(-30, 180, 180);

        /// <summary>
        /// Default position for left hand menu.
        /// </summary>
        public Vector3 leftHandedMenuRectPosition;

        /// <summary>
        /// Anchor for left hand menu.
        /// </summary>
        public GameObject leftHandedMenuAnchor;

        /// <summary>
        /// Default rotation for right hand menu.
        /// </summary>
        public Vector3 rightHandedMenuRectRotation = new Vector3(-30, 180, 180);

        /// <summary>
        /// defualt position for right hand menu. This, for some reasons, is not initialized.
        /// </summary>
        public Vector3 rightHandMenuRectPosition;

        /// <summary>
        /// Anchor for right hand menu.
        /// </summary>
        public GameObject rightHandedMenuAnchor;

        /// <summary>
        /// The loading process UI canvas group. These canvas group are up at the beginning of the session, indicating the progress of loading models.
        /// </summary>
        [Header("Initial Loading Process UI")]
        public CanvasGroup initialLoadingCanvas;

        /// <summary>
        /// Text object for loading canvas progress text. 
        /// </summary>
        public Text initialLoadingCanvasProgressText;


        [ShowOnly]
        public bool isModelButtonListReady;

        [ShowOnly]
        public bool isSceneButtonListReady;

        [HideInInspector]
        public ChildTextCreateOnCall clientTagSetup;

        //References for displaying user name tags and speechtotext text
        private List<Text> clientUser_Names_UITextReference_list = new List<Text>();

        private List<Text> clientUser_SpeechToText_UITextReference_list = new List<Text>();

        [HideInInspector]
        public List<VisibilityToggle> modelVisibilityToggleList;

        [HideInInspector]
        public List<LockToggle> modelLockToggleList = new List<LockToggle>();

        [HideInInspector]
        public Text sessionAndBuildName;

        private ToggleExpandability menuExpandability;

        [Header("UI Cursor for Menu")]

        [ShowOnly]
        public GameObject cursor;

        [ShowOnly]
        public GameObject cursorGraphic;

        private Image cursorImage;

        private EntityManager entityManager;

        ClientSpawnManager clientManager;

        /// <summary>
        /// Initiate UIManager singleton and set up clientManager.
        /// </summary>
        /// <exception cref="System.Exception">Thrown if there is no menuPrefab.</exception>
        public void Awake()
        {
            // instantiates this singleton in case it doesn't exist yet.
            var uiManager = Instance;

            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            clientManager = ClientSpawnManager.Instance;

            if (menuPrefab == null)
            {
                throw new System.Exception("You must set a menuPrefab");
            }
        }

        /// <summary>
        /// Check if there is any UI component that is null. Setup some of the UI menu compoenents.
        /// </summary>
        /// <exception cref="System.Exception">Thrown if there is no Canvas component, right-handed menu anchor, or left handed menu anchor.</exception>
        /// <exception cref="Exception">Thrown if there is no CanvasGroup component or selection canvas does not have a RectTransform component.</exception>
        public void Start () {

            menu = GameObject.FindWithTag(TagList.menuUI);

            // create a menu if there isn't one already
            if (menu == null) 
            {
                Debug.LogWarning("Couldn't find an object tagged MenuUI in the scene, so creating one now");

                menu = Instantiate(menuPrefab);
            }

            hoverCursor = menu.GetComponentInChildren<HoverCursor>(true);
            //TODO -- fix this, because right now Start is not guaranteed to execute after the menu prefab has instantiated its components.

            if (hoverCursor == null) {
                Debug.LogWarning("You must have a HoverCursor component");
            }

            if (hoverCursor.cursorGraphic == null)
            { 
                Debug.LogWarning("HoverCursor component does not have a cursorGraphic property");
            }

            cursor = hoverCursor.cursorGraphic.transform.parent.gameObject; //TODO -- is there a shorter way to say this?

            cursorGraphic = hoverCursor.cursorGraphic.gameObject;

            menuCanvas = menu.GetComponentInChildren<Canvas>(true);

            if (menuCanvas == null)
            {
                throw new System.Exception("You must have a Canvas component");
            }

            menuCanvasGroup = menu.GetComponentInChildren<CanvasGroup>(true);

            if (menuCanvasGroup == null)
            {
                throw new System.Exception("You must have a CanvasGroup component");
            }

            menuTransform = menuCanvas.GetComponent<RectTransform>();
           
            clientTagSetup = menu.GetComponent<ChildTextCreateOnCall>();

            sessionAndBuildName = menu.GetComponent<MainUIReferences>().sessionAndBuildText;

            if (menuTransform == null) 
            {
                throw new Exception("selection canvas must have a RectTransform component");
            }

            if (rightHandedMenuAnchor == null)
            {
                throw new System.Exception("You must set a right-handed menu anchor");
            }

            if (leftHandedMenuAnchor == null)
            {
                throw new System.Exception("You must set a left-handed menu anchor");
            }
            
            menu.transform.SetParent(leftHandedMenuAnchor.transform);
            
            if (!sessionAndBuildName)
            {
                Debug.LogWarning("sessionAndBuildName was null. Proceeding anyways.");
            }
            
            menuExpandability = menuCanvas.GetComponent<ToggleExpandability>();
    
            if (menuExpandability == null)
            {
                Debug.LogError("No ToggleExpandability component found", this);
            }

            cursorImage = menuCanvas.GetComponent<Image>();
    
            if (cursorImage == null) 
            {
                Debug.LogError("No Image component found on UI ", this);
            }

            DisplaySessionDetails();
        }

        /// <summary>
        /// Enable cursor.
        /// </summary>
        public void EnableCursor ()
        {
            //use ghost cursor on the menu in XR mode
            cursorImage.enabled = true;
        }

        /// <summary>
        /// Disable cursor.
        /// </summary>
        public void DisableCursor ()
        {
            //use ghost cursor on the menu in XR mode
            cursorImage.enabled = false;
        }

        /// <summary>
        /// DisplaySession Details with <c>sessionAndBuildName.text</c> by using <c>NetworkUpdateHandler</c>
        /// </summary>
        private void DisplaySessionDetails ()
        {
            sessionAndBuildName.text = NetworkUpdateHandler.Instance.sessionName;

            sessionAndBuildName.text += Environment.NewLine +  NetworkUpdateHandler.Instance.buildName;
        }

        /// <summary>
        /// Check if <c>cursorGraphic</c> is active/true in the Unity hierarchy.
        /// </summary>
        /// <returns></returns>
        public bool GetCursorActiveState() 
        { 
            return cursorGraphic.activeInHierarchy;
        }

        /// <summary>
        /// Make all the panels in the UI menu expanded.
        /// </summary>
        public void ConvertMenuToAlwaysExpanded ()
        {
            menuExpandability.ConvertToAlwaysExpanded();
        }

        /// <summary>
        /// Convert the menu to expandable.
        /// </summary>
        /// <param name="isExpanded">whether we want the menu to be expandable.</param>
        public void ConvertMenuToExpandable (bool isExpanded)
        {
            menuExpandability.ConvertToExpandable(isExpanded);
        }

        /// <summary>
        /// hide or unhide model functionality in UI menu.
        /// </summary>
        /// <param name="index">index represents which models in the scene</param>
        /// <param name="doShow">true = show; false = not show</param>
        public void ToggleModelVisibility (int index, bool doShow)
        {
            GameObject gObject = NetworkedObjectsManager.Instance.GetNetworkedGameObject(index).gameObject;

            gObject.SetActive(doShow);
        }

        /// <summary>
        /// This sends whether the visibility of user's menu to the NetworkUpdateHandler. This method is primarily used for capturing user's interactions and actions in Komodo.
        /// </summary>
        /// <param name="visibility">True = visible; flase = invisible</param>
        public void SendMenuVisibilityUpdate(bool visibility)
        {
            if (visibility) 
            {
                NetworkUpdateHandler.Instance.SendSyncInteractionMessage (new Interaction 
                {

                sourceEntity_id = NetworkUpdateHandler.Instance.client_id,

                interactionType = (int)INTERACTIONS.SHOW_MENU,

                targetEntity_id = 0,
                });
            } else {
                NetworkUpdateHandler.Instance.SendSyncInteractionMessage (new Interaction 
                {

                sourceEntity_id = NetworkUpdateHandler.Instance.client_id,

                interactionType = (int)INTERACTIONS.HIDE_MENU,

                targetEntity_id = 0,
                });
            }
        }

        /// <summary>
        /// This sends visibility of models in the scene to the NetworkUpdateHandler. This method is used for capturing if users have enable or disable a model in the scene, as well as which model a user has enabled or disabled. 
        /// </summary>
        /// <param name="index"> index of a model</param>
        /// <param name="doShow"> whether the model is visible or not</param>
        public void SendVisibilityUpdate (int index, bool doShow)
        {
            GameObject gObject = NetworkedObjectsManager.Instance.GetNetworkedGameObject(index).gameObject;

            NetworkedGameObject netObject = gObject.GetComponent<NetworkedGameObject>();

            if (!netObject)
            {
                Debug.LogError("no NetworkedGameObject found on currentObj in ClientSpawnManager.cs");

                return;
            }

            int entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(netObject.Entity).entityID;

            if (doShow)
            {
                NetworkUpdateHandler.Instance.SendSyncInteractionMessage(new Interaction
                {
                    sourceEntity_id = NetworkUpdateHandler.Instance.client_id,
                    targetEntity_id = entityID,
                    interactionType = (int) INTERACTIONS.SHOW,
                });
            }
            else
            {
                NetworkUpdateHandler.Instance.SendSyncInteractionMessage(new Interaction
                {
                    sourceEntity_id = NetworkUpdateHandler.Instance.client_id,
                    targetEntity_id = entityID,
                    interactionType = (int) INTERACTIONS.HIDE,
                });
            }

            //TODO(Brandon): what is this code for?
            try
            {
                Entity currentEntity = NetworkedObjectsManager.Instance.GetEntity(index);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Tried to get entity with index {index}. Error: {e.Message}");
            }
        }

        /* TODO: implement these two functions. Right now they don't work because ProcessNetworkToggleVisibility expects an entityID, not an index.
        [ContextMenu("Test Process Network Show Model 0")]
        public void TestProcessNetworkShow()
        {
            ProcessNetworkToggleVisibility(0, true);
        }

        [ContextMenu("Test Process Network Hide Model 0")]
        public void TestProcessNetworkHide()
        {
            ProcessNetworkToggleVisibility(0, false);
        }
        */

        /// <summary>
        /// Show or hide a model via a network update
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="doShow"> visible or invisible</param>
        public void ProcessNetworkToggleVisibility(int entityID, bool doShow)
        {
            var netObject = NetworkedObjectsManager.Instance.networkedObjectFromEntityId[entityID];

            if (netObject == null)
            {
                Debug.LogError($"Could not get entity with id {entityID}");

                return;
            }

            var index = entityManager.GetSharedComponentData<ButtonIDSharedComponentData>(netObject.Entity).buttonID;

            GameObject currentObj = NetworkedObjectsManager.Instance.GetNetworkedGameObject(index).gameObject;

            if (!currentObj)
            {
                Debug.LogError($"Could not get networked game object at {index}");

                return;
            }

            if (index > modelVisibilityToggleList.Count || !modelVisibilityToggleList[index])
            {
                Debug.LogError($"Tried to change state of model lock button, but there was none with index {index}");

                return;
            }

            modelVisibilityToggleList[index].ProcessNetworkToggle(doShow);
        }

        /// <summary>
        /// A testing method for locking model 0. This is not being used anywhere. It is just a testing method.
        /// </summary>
        [ContextMenu("Test Process Network Lock Model 0")]
        public void TestProcessNetworkLock()
        {
            ProcessNetworkToggleLock(0, true);
        }

        /// <summary>
        /// A testing method for unlocking model 0. This is not being used anywhere. It is just a testing method. 
        /// </summary>
        [ContextMenu("Test Process Network Unlock Model 0")]
        public void TestProcessNetworkUnlock()
        {
            ProcessNetworkToggleLock(0, false);
        }

        /// <summary>
        /// Lock the model and update the changes to others.
        /// </summary>
        /// <param name="index">index of the model being locked</param>
        /// <param name="doLock">to lock or not to lock</param>
        public void ProcessNetworkToggleLock (int index, bool doLock)
        {
            if (index > modelLockToggleList.Count || !modelLockToggleList[index])
            {
                Debug.LogError($"Tried to change state of model lock button, but there was none with index {index}");

                return;
            }

            modelLockToggleList[index].ProcessNetworkToggle(doLock, index);
        }

        //we need funcions for our UI buttons to link up, which can be affected by our client selecting the button or when we get a call to invoke it.
        //We attach funcions through SetUp_ButtonURL.cs but those funcions send network events, to avoid sending a network event when receiving a call, we created
        //another funcion here to avoid sending them by simulating a button press (Having the same funcionality when pressing the button and when receiving a call from 
        //network that it was turned on/off)
        /*
        public void SimulateLockTogglePress(int index, bool currentLockStatus, bool doSendNetworkUpdate)
        {
            foreach (NetworkedGameObject item in clientManager.GetNetworkedSubObjectList(index))
            {
                if (currentLockStatus)
                {
                    if (!entityManager.HasComponent<TransformLockTag>(item.Entity)) {
                        entityManager.AddComponentData(item.Entity, new TransformLockTag { });
                    }
                }
                else
                {
                    if (entityManager.HasComponent<TransformLockTag>(item.Entity)) {
                        entityManager.RemoveComponent<TransformLockTag>(item.Entity);
                    }
                }
            }

            //Unity's UIToggle funcionality does not show the graphic element until someone fires the event (is on), simmulating this behavior when receiving 
            //other peoples calls makes us use a image as a parent of a graphic element that we can use to turn on and off instead   
            //modelLockButtonList[index].graphic.transform.parent.gameObject.SetActive(currentLockStatus); //TODO: is there a shorter way to say this? 

            if (index > modelLockToggleList.Count || !modelLockToggleList[index])
            {
                Debug.LogError($"Tried to change state of model lock button, but there was none with index {index}");

                return;
            }

            modelLockToggleList[index].UpdateUI(currentLockStatus);

            if (doSendNetworkUpdate)
            {
                int entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(clientManager.GetNetworkedSubObjectList(index)[0].Entity).entityID;

                int lockState = 0;

                //SETUP and send network lockstate
                if (currentLockStatus) 
                {
                    lockState = (int)INTERACTIONS.LOCK;
                }
                else
                {
                    lockState = (int)INTERACTIONS.UNLOCK;
                }

                NetworkUpdateHandler.Instance.SendSyncInteractionMessage(new Interaction
                {
                    sourceEntity_id = NetworkUpdateHandler.Instance.client_id,//index, // TODO(rob): use client hand ids or 0 for desktop? 
                    targetEntity_id = entityID,
                    interactionType = lockState,
                });
            }
        }
        */

        /// <summary>
        /// show or hide Komodo UI menu.
        /// </summary>
        /// <param name="activeState">true = show menu; false = hide menu</param>
        public void ToggleMenuVisibility(bool activeState)
        {
            if (menuCanvasGroup == null) {
                Debug.LogWarning("Tried to toggle visibility for menuCanvasGroup, but it was null. Skipping.");

                return;
            }

            if (activeState)
            {
                menuCanvasGroup.alpha = 1;

                menuCanvasGroup.blocksRaycasts = true;

                SendMenuVisibilityUpdate(activeState);
            }
            else
            {
                menuCanvasGroup.alpha = 0;

                menuCanvasGroup.blocksRaycasts = false;

                SendMenuVisibilityUpdate(activeState);
            }
        }

        /// <summary>
        /// move the Komodo UI menu to left hand.
        /// </summary>
        public void ToggleLeftHandedMenu ()
        {
            if (_isRightHanded && _isMenuVisible)
            {
                SetLeftHandedMenu();

                return;
            }

            if (_isRightHanded && !_isMenuVisible)
            {
                ToggleMenuVisibility(true);

                SetLeftHandedMenu();

                return;
            }

            if (!_isRightHanded && !_isMenuVisible)
            {
                ToggleMenuVisibility(true);

                return;
            }

            // the menu is already left-handed and already visible.
            ToggleMenuVisibility(false);
        }

        /// <summary>
        /// Move Komodo UI menu to right hand.
        /// </summary>
        public void ToggleRightHandedMenu ()
        {
            if (!_isRightHanded && _isMenuVisible)
            {
                SetRightHandedMenu();
                return;
            }

            if (!_isRightHanded && !_isMenuVisible)
            {
                ToggleMenuVisibility(true);
                SetRightHandedMenu();
                return;
            }

            if (_isRightHanded && !_isMenuVisible)
            {
                ToggleMenuVisibility(true);
                return;
            }

            // the menu is already right-handed and already visible.
            ToggleMenuVisibility(false);
        }

        /// <summary>
        /// Set Komodo UI menu to left hand.
        /// </summary>
        public void SetLeftHandedMenu() {
            SetHandednessAndPlaceMenu(false);
        }

        /// <summary>
        /// set Komodo UI menu to right hand.
        /// </summary>
        public void SetRightHandedMenu() {
            SetHandednessAndPlaceMenu(true);
        }

        /// <summary>
        /// Set the menu to the target hand and then place the menu to that hand.
        /// </summary>
        /// <param name="isRightHanded"> whether the target hand is right hand or not.</param>
        public void SetHandednessAndPlaceMenu(bool isRightHanded) {
            SetMenuHandedness(isRightHanded);

            PlaceMenuOnCurrentHand();
        }

        /// <summary>
        /// configuring the targeted hand that the menu is going to be placed.
        /// </summary>
        /// <param name="isRightHanded"></param>
        public void SetMenuHandedness (bool isRightHanded) {
            _isRightHanded = isRightHanded;
        }

        /// <summary>
        /// place komodo UI menu to the current hand. This current hand is determined by other methods.
        /// </summary>
        public void PlaceMenuOnCurrentHand () {
            if (menuCanvas == null) {
                Debug.LogWarning("Could not find menu canvas. Proceeding anyways.");
            }

            Camera leftHandEventCamera = null;

            Camera rightHandEventCamera = null;

            if (EventSystemManager.IsAlive)
            {
                 leftHandEventCamera = EventSystemManager.Instance.inputSource_LeftHand.eventCamera;

                 rightHandEventCamera = EventSystemManager.Instance.inputSource_RighttHand.eventCamera;
            }

            menuTransform.localScale = eitherHandRectScale;

            //enables menu selection laser
            if (_isRightHanded)
            {
                menu.transform.SetParent(rightHandedMenuAnchor.transform);

                menuTransform.localRotation = Quaternion.Euler(rightHandedMenuRectRotation);

                menuTransform.anchoredPosition3D = rightHandMenuRectPosition;

                menuCanvas.worldCamera = rightHandEventCamera;
            }
            else
            {
                menu.transform.SetParent(leftHandedMenuAnchor.transform);

                menuTransform.localRotation = Quaternion.Euler(leftHandedMenuRectRotation);

                menuTransform.anchoredPosition3D = leftHandedMenuRectPosition;

                menuCanvas.worldCamera = leftHandEventCamera;
            }

            menuTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500); //TODO this might have to go after renderMode changes

            menuCanvas.renderMode = RenderMode.WorldSpace;
        }

        /// <summary>
        /// Check if models and scenes are loaded as well as our UI manager is ready.
        /// </summary>
        /// <returns></returns>
        public bool IsReady ()
        {
            //check managers that we are using for our session
            if (!SceneManagerExtensions.IsAlive && !ModelImportInitializer.IsAlive) {
                return true;
            }

            if (SceneManagerExtensions.IsAlive && !ModelImportInitializer.IsAlive) {
                return isSceneButtonListReady;
            }

            if (!SceneManagerExtensions.IsAlive && ModelImportInitializer.IsAlive) {
                return isModelButtonListReady;
            }

            if (SceneManagerExtensions.IsAlive && ModelImportInitializer.IsAlive)
            {
                return isModelButtonListReady && isSceneButtonListReady;
            }

            return false;
        }

        /// <summary> 
        /// This function will enable Create and Height Calibration Panels for VR view.
        /// </summary>
        public void HeightCalibrationButtonsSettings(bool state) 
        {
            heightCalibration.gameObject.SetActive(state);
            calibrationButtons.gameObject.SetActive(state);
            manuallyAdjustHeight.gameObject.SetActive(state);
        }

        /// <summary>
        /// This enables the create menu in VR mode. In desktop mode, the create menu is disable. 
        /// </summary>
        /// <param name="state">whether the create menu is enabled or not.</param>
        public void EnableCreateMenu(bool state)
        {
            createTab.gameObject.SetActive(state);
        }

        /// <summary>
        /// Enable the instructor menu button when in the desktop mode. This is called in <c>SwitchMenuToDesktopMode()</c>.
        /// </summary>
        /// <param name="state">this boolean value determines whether instructor menu button is enable. True for desktop mode, and false for VR mode.</param>
        public void EnableInstructorMenuButton(bool state)
        {
            instructorMenuButton.gameObject.SetActive(state);
        }

        /// <summary>
        /// This ignores layout for the VR mode. This is necessary for making the menu looks clean in the VR mode; otherwise everything is sticked together.
        /// </summary>
        /// <param name="state">true for VR mode; false for desktop mode.</param>
        public void EnableIgnoreLayoutForVRmode(bool state) 
        {
            LayoutElement RecenterButton = settingsMenu.transform.Find("NotCalibrating").transform.Find("RecenterButton").GetComponent<LayoutElement>();

            LayoutElement settingsMenuTitle = settingsMenu.transform.Find("Text").GetComponent<LayoutElement>();

            RecenterButton.ignoreLayout = state;
            settingsMenuTitle.ignoreLayout = state;
        }

        /// <summary>
        /// Switch menu to desktop mode.
        /// </summary>
        public void SwitchMenuToDesktopMode() 
        {
            DisableCursor();

            //TODO: One of the above actually does the job. Which is it?
            menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            ConvertMenuToExpandable(false);

            HeightCalibrationButtonsSettings(false);

            EnableCreateMenu(false);

            HeightCalibrationButtonsSettings(false);

            EnableInstructorMenuButton(true);

            createTab.GetComponent<TabButton>().onTabDeselected.Invoke();

            LayoutRebuilder.ForceRebuildLayoutImmediate(settingsMenu.GetComponent<RectTransform>());

        }
    }
}
