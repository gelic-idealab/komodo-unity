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
        public GameObject menuPrefab;

        [ShowOnly]
        public GameObject menu;

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

        //Default values to use to move menu for XR hands 
        public Vector3 eitherHandRectScale = new Vector3(0.001f, 0.001f, 0.001f);

        public Vector3 leftHandedMenuRectRotation = new Vector3(-30, 180, 180);

        public Vector3 leftHandedMenuRectPosition;

        public GameObject leftHandedMenuAnchor;

        public Vector3 rightHandedMenuRectRotation = new Vector3(-30, 180, 180);

        public Vector3 rightHandMenuRectPosition;

        public GameObject rightHandedMenuAnchor;


        [Header("Initial Loading Process UI")]

        public CanvasGroup initialLoadingCanvas;

        public Text initialLoadingCanvasProgressText;

        [ShowOnly]
        public bool isModelButtonListReady;

        [ShowOnly]
        public bool isSceneButtonListReady;

        [HideInInspector]
        public ChildTextCreateOnCall clientTagSetup;

        //References for displaying user name tags and dialogue text
        private List<Text> clientUser_Names_UITextReference_list = new List<Text>();

        private List<Text> clientUser_Dialogue_UITextReference_list = new List<Text>();

        [HideInInspector]
        public List<Button> modelVisibilityButtonList;

        [HideInInspector]
        public List<Toggle> modelLockButtonList = new List<Toggle>();

        [HideInInspector]
        public Text sessionAndBuildName;



        [Header("UI Cursor for Menu")]

        [ShowOnly]
        public GameObject cursor;

        [ShowOnly]
        public GameObject cursorGraphic;
        [Header("Button Colors")]
        public Color modelIsActiveColor = new Color(80, 30, 120, 1);

        public Color modelIsInactiveColor = new Color(0, 0, 0, 0);

        private EntityManager entityManager;

        ClientSpawnManager clientManager;
        
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

        public void Start () {

            menu = GameObject.FindGameObjectWithTag(TagList.menuUI);

            // create a menu if there isn't one already
            if (menu == null) 
            {
                menu = Instantiate(menuPrefab);
            }

            hoverCursor = menu.GetComponentInChildren<HoverCursor>(true);
            //TODO -- fix this, because right now Start is not guaranteed to execute after the menu prefab has instantiated its components.

            if (hoverCursor == null) {
                Debug.LogError("You must have a HoverCursor component");
            }

            if (hoverCursor.cursorGraphic == null) { 
                Debug.LogError("HoverCursor component does not have a cursorGraphic property");
            }

            cursor = hoverCursor.cursorGraphic.transform.parent.gameObject; //TODO -- is there a shorter way to say this?

            cursorGraphic = hoverCursor.cursorGraphic.gameObject;

            menuCanvas = menu.GetComponentInChildren<Canvas>(true);

            if (menuCanvas == null) {
                throw new System.Exception("You must have a Canvas component");
            }

            menuCanvasGroup = menu.GetComponentInChildren<CanvasGroup>(true);

            if (menuCanvasGroup == null) {
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

            DisplaySessionDetails();
        }

        private void DisplaySessionDetails () {
            sessionAndBuildName.text = NetworkUpdateHandler.Instance.sessionName;

            sessionAndBuildName.text += Environment.NewLine +  NetworkUpdateHandler.Instance.buildName;
        }
        public bool GetCursorActiveState() 
        { 
            return cursorGraphic.activeInHierarchy;
        }

        /// <summary>
        /// used to turn on models that were setup with SetUp_ButtonURL.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="button"></param>
        /// <param name="sendNetworkCall is used to determine if we should send a call for others to render the specified object"></param>
        public void ToggleModelVisibility(int index, bool activeState)
        {

            GameObject gObject = clientManager.GetNetworkedGameObject(index).gameObject;

            NetworkedGameObject netObject = gObject.GetComponent<NetworkedGameObject>();

            if (!netObject)
            {
                Debug.LogError("no NetworkedGameObject found on currentObj in ClientSpawnManager.cs");

                return;
            }

            int entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(netObject.Entity).entityID;

            Entity currentEntity = clientManager.GetEntity(index);

            if (activeState)
            {
                gObject.SetActive(true);

                NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
                {
                    sourceEntity_id = NetworkUpdateHandler.Instance.client_id,
                    targetEntity_id = entityID,
                    interactionType = (int)INTERACTIONS.RENDERING,
                });
            }
            else
            {
                gObject.SetActive(false);

                //if (GameStateManager.Instance.useEntityComponentSystem)
                //    if (currentEntity != Entity.Null)
                //        entityManager.SetEnabled(currentEntity, false);

                NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
                {
                    sourceEntity_id = NetworkUpdateHandler.Instance.client_id,
                    targetEntity_id = entityID,
                    interactionType = (int)INTERACTIONS.NOT_RENDERING,

                });
            }
        }

        /// <summary>
        /// Render a new model for this client only without inputing button reference
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="activeState"></param>
        public void SimulateToggleModelVisibility(int entityID, bool activeState)
        {
            var currentEntity = clientManager.networkedObjectFromEntityId[entityID].Entity;

            if (currentEntity == null) {
                Debug.LogError($"Could not get entity with id {entityID}");

                return;
            }

            var index = entityManager.GetSharedComponentData<ButtonIDSharedComponentData>(currentEntity).buttonID;

            GameObject currentObj = clientManager.GetNetworkedGameObject(index).gameObject;

            if (!currentObj) {
                Debug.LogError($"Could not get networked game object at {index}");

                return;
            }

            Button button = modelVisibilityButtonList[index];

            if (!activeState)
            {
                button.SetButtonColor(true, modelIsActiveColor, modelIsInactiveColor);

                currentObj.SetActive(true);
            }
            else
            {
                button.SetButtonColor(false, modelIsActiveColor, modelIsInactiveColor);

                currentObj.SetActive(false);
            }
        }


        //we need funcions for our UI buttons to link up, which can be affected by our client selecting the button or when we get a call to invoke it.
        //We attach funcions through SetUp_ButtonURL.cs but those funcions send network events, to avoid sending a network event when receiving a call, we created
        //another funcion here to avoid sending them by simulating a button press (Having the same funcionality when pressing the button and when receiving a call from 
        //network that it was turned on/off)
        public void SimulateLockToggleButtonPress(int index, bool currentLockStatus, bool isNetwork)
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
            modelLockButtonList[index].graphic.transform.parent.gameObject.SetActive(currentLockStatus); //TODO: is there a shorter way to say this? 

            if (isNetwork)
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

                NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
                {
                    sourceEntity_id = NetworkUpdateHandler.Instance.client_id,//index, // TODO(rob): use client hand ids or 0 for desktop? 
                    targetEntity_id = entityID,
                    interactionType = lockState,
                });
            }
        }

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
            }
            else
            {
                menuCanvasGroup.alpha = 0;

                menuCanvasGroup.blocksRaycasts = false;
            }
        }

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

        public void SetLeftHandedMenu() {
            SetHandednessAndPlaceMenu(false);
        }

        public void SetRightHandedMenu() {
            SetHandednessAndPlaceMenu(true);
        }

        public void SetHandednessAndPlaceMenu(bool isRightHanded) {
            SetMenuHandedness(isRightHanded);

            PlaceMenuOnCurrentHand();
        }

        public void SetMenuHandedness (bool isRightHanded) {
            _isRightHanded = isRightHanded;
        }

        public void PlaceMenuOnCurrentHand () {

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
        }
        
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
    }
}
