using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : SingletonComponent<UIManager>
{
    public static UIManager Instance
    {
        get { return ((UIManager)_Instance); }
        set { _Instance = value; }
    }

    [Header("Player Main UI")]
    public CanvasGroup mainUIDashboard;

    [Header("Initial Loading Process UI")]
    public CanvasGroup initialLoadingCanvas;
    public Text initialLoadingCanvasProgressText;
    [ShowOnly] public bool isModelButtonListReady;
    [ShowOnly] public bool isSceneButtonListReady;

    [Header("UI Client Tag ")]
    public ChildTextCreateOnCall clientTagSetup;
    //References for displaying user name tags and dialogue text
    private List<Text> clientUser_Names_UITextReference_list = new List<Text>();
    private List<Text> clientUser_Dialogue_UITextReference_list = new List<Text>();

    [HideInInspector] public List<Button> assetButtonRegister_List;
    [HideInInspector] public List<Toggle> assetLockToggleRegister_List = new List<Toggle>();

    [Header("UIButtons to Deactivate/Activate")]
    public Alternate_Button_Function abilityButton_Drawing;

    [Header("Network UI References")]
    public Text sessionAndBuildName;

    //public GameObject sceneScaler;
    //public GameObject currentEnvironment;

    [Header("UI Cursor to detect if we are currently interacting with the UI")]
    public GameObject cursorGraphic;

    private EntityManager entityManager;

    ClientSpawnManager clientManager;
    public void Start()
    {
        clientManager = ClientSpawnManager.Instance;
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        //if (sceneListContainer.sceneList.Count == 0)
        //    Debug.LogError("No Scenes available to Activate check scene reference");

        if (sessionAndBuildName)
        {
            sessionAndBuildName.text = "<color=purple>SESSION: </color>" + NetworkUpdateHandler.Instance.sessionName; //sessionName.text = " < color=purple>SESSION: </color> thisISATESTOFALONGNAMESESSION";
            sessionAndBuildName.text += Environment.NewLine + "<color=purple>BUILD: </color>" + NetworkUpdateHandler.Instance.buildName;
        }
    }

    public bool GetCursorActiveState() => cursorGraphic.activeInHierarchy;

    public void SetDrawingAbilityAvailability(bool activeState)
    {
        if (!activeState)
        {
            //set button to call appropriate funcion to deactivate the user button and ability completely
            abilityButton_Drawing.isFirstClick = true;
            abilityButton_Drawing.AlternateButtonFunctions();
            abilityButton_Drawing.gameObject.SetActive(false);

        }
        else
            abilityButton_Drawing.gameObject.SetActive(true);

    }

    /// <summary>
    /// used to turn on assets that were setup with SetUp_ButtonURL.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="button"></param>
    /// <param name="sendNetworkCall is used to determine if we should send a call for others to render the specified object"></param>
    public void ToggleModelVisibility(int index, bool activeState)
    {
        GameObject currentObj = default;
        Entity currentEntity = default;

        NetworkedGameObject netRegisterComponent = default;
        int entityID = default;

        currentObj = clientManager.GetNetworkedGameObject(index).gameObject;
        netRegisterComponent = currentObj.GetComponent<NetworkedGameObject>();

        if (!netRegisterComponent)
            Debug.LogError("no netRegisterComponet found on currentObj in ClientSpawnManager.cs");

        entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(netRegisterComponent.Entity).entityID;
        currentEntity = clientManager.GetEntity(index);

        if (activeState)
        {
            currentObj.SetActive(true);

            //if (GameStateManager.Instance.useEntityComponentSystem)
            //    if (currentEntity != Entity.Null)
            //        entityManager.SetEnabled(currentEntity, true);


            NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
            {
                sourceEntity_id = NetworkUpdateHandler.Instance.client_id,
                targetEntity_id = entityID,
                interactionType = (int)INTERACTIONS.RENDERING,
            });
        }
        else
        {
            currentObj.SetActive(false);

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
    /// Render a new asset for this client only without inputing button reference
    /// </summary>
    /// <param name="entityID"></param>
    /// <param name="activeState"></param>
    public void SimulateToggleModelVisibility(int entityID, bool activeState)
    {
        var index = entityManager.GetSharedComponentData<ButtonIDSharedComponentData>(clientManager.networkedObjectFromEntityId[entityID].Entity).buttonID;
        GameObject currentObj = clientManager.GetNetworkedGameObject(index).gameObject;
        Button button = assetButtonRegister_List[index];

        if (!activeState)
        {
            button.SetButtonStateColor(Color.green, true);
            currentObj.SetActive(true);
        }
        else
        {
            button.SetButtonStateColor(Color.white, false);
            currentObj.SetActive(false);
        }
    }


    //we need funcions for our UI buttons to link up, which can be affected by our client selecting the button or when we get a call to invoke it.
    //We attach funcions through SetUp_ButtonURL.cs but those funcions send network events, to avoid sending a network event when receiving a call, we created
    //another funcion here to avoid sending them by simulating a button press (Having the same funcionality when pressing the button and when receiving a call from 
    //network that it was turned on/off)
    public void SimulateLockToggleButtonPress(int assetIndex, bool currentLockStatus, bool isNetwork)
    {
        foreach (NetworkedGameObject item in clientManager.GetNetworkedSubObjectList(assetIndex))
        {

            if (currentLockStatus)
            {
                if (!entityManager.HasComponent<TransformLockTag>(item.Entity))
                    entityManager.AddComponentData(item.Entity, new TransformLockTag { });
            }
            else
            {
                if (entityManager.HasComponent<TransformLockTag>(item.Entity))
                    entityManager.RemoveComponent<TransformLockTag>(item.Entity);
            }

        }

        //foreach (Net_Register_GameObject item in decomposedAssetReferences_Dict[assetIndex])
        //    item.entity_data.isCurrentlyGrabbed = currentLockStatus;

        //Unity's UIToggle funcionality does not show the graphic element until someone fires the event (is on), simmulating this behavior when receiving 
        //other peoples calls makes us use a image as a parent of a graphic element that we can use to turn on and off instead   
        assetLockToggleRegister_List[assetIndex].graphic.transform.parent.gameObject.SetActive(currentLockStatus);

        if (isNetwork)
        {
            int entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(clientManager.GetNetworkedSubObjectList(assetIndex)[0].Entity).entityID;

            int lockState = 0;

            //SETUP and send network lockstate
            if (currentLockStatus)
                lockState = (int)INTERACTIONS.LOCK;
            else
                lockState = (int)INTERACTIONS.UNLOCK;

            NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
            {
                sourceEntity_id = NetworkUpdateHandler.Instance.client_id,//assetIndex, // TODO(rob): use client hand ids or 0 for desktop? 
                targetEntity_id = entityID,
                interactionType = lockState,

            });
        }
    }

    public void ToggleMenuVisibility(bool activeState)
    {
        if (activeState)
        {
           mainUIDashboard.alpha = 1;
            mainUIDashboard.blocksRaycasts = true;
        }
        else
        {
            mainUIDashboard.alpha = 0;  //SetActive(false);
            mainUIDashboard.blocksRaycasts = false;
        }
    }

    
    public bool IsReady () {
        return isModelButtonListReady && isSceneButtonListReady;
    }

}
