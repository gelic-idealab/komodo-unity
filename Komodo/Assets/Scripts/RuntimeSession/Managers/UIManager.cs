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

    [Header("Player Menu")]
    public CanvasGroup menuCanvasGroup;

    [Header("Initial Loading Process UI")]

    public CanvasGroup initialLoadingCanvas;

    public Text initialLoadingCanvasProgressText;

    [ShowOnly] public bool isModelButtonListReady;
    
    [ShowOnly] public bool isSceneButtonListReady;

    [Header("Client Nametag")]
    public ChildTextCreateOnCall clientTagSetup;

    //References for displaying user name tags and dialogue text
    private List<Text> clientUser_Names_UITextReference_list = new List<Text>();

    private List<Text> clientUser_Dialogue_UITextReference_list = new List<Text>();

    [HideInInspector] public List<Button> assetButtonRegister_List;

    [HideInInspector] public List<Toggle> assetLockToggleRegister_List = new List<Toggle>();

    [Header("Network UI References")]
    public Text sessionAndBuildName;

    [Header("UI Cursor to detect if we are currently interacting with the UI")]
    public GameObject cursorGraphic;

    public Color modelIsActiveColor = new Color(180, 0, 180);

    public Color modelIsInactiveColor = new Color(0, 0, 0, 0);

    public Color modelButtonHoverColor = new Color(255, 180, 255);

    private EntityManager entityManager;

    ClientSpawnManager clientManager;

    public void Start()
    {
        clientManager = ClientSpawnManager.Instance;
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        if (sessionAndBuildName)
        {
            sessionAndBuildName.text = "<color=purple>SESSION: </color>" + NetworkUpdateHandler.Instance.sessionName;

            sessionAndBuildName.text += Environment.NewLine + "<color=purple>BUILD: </color>" + NetworkUpdateHandler.Instance.buildName;
        }
    }

    public bool GetCursorActiveState() => cursorGraphic.activeInHierarchy;

    /// <summary>
    /// used to turn on assets that were setup with SetUp_ButtonURL.
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
            Debug.LogError("no NetworkedGameObject component found on GameObject in ClientSpawnManager.cs");
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
            button.SetButtonStateColor(modelIsActiveColor, true);
            currentObj.SetActive(true);
        }
        else
        {
            button.SetButtonStateColor(modelIsInactiveColor, false);
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
        assetLockToggleRegister_List[assetIndex].graphic.transform.parent.gameObject.SetActive(currentLockStatus);

        if (isNetwork)
        {
            int entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(clientManager.GetNetworkedSubObjectList(assetIndex)[0].Entity).entityID;

            int lockState = 0;

            //set up and send network lock state
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
                sourceEntity_id = NetworkUpdateHandler.Instance.client_id,//assetIndex, // TODO(rob): use client hand ids or 0 for desktop? 
                targetEntity_id = entityID,
                interactionType = lockState,
            });
        }
    }

    public void ToggleMenuVisibility (bool activeState)
    {
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
    
    public bool IsReady () {
        return isModelButtonListReady && isSceneButtonListReady;
    }

}
