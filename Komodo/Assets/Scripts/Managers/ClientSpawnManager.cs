// University of Illinois/NCSA
// Open Source License
// http://otm.illinois.edu/disclose-protect/illinois-open-source-license

// Copyright (c) 2020 Grainger Engineering Library Information Center.  All rights reserved.

// Developed by: IDEA Lab
//               Grainger Engineering Library Information Center - University of Illinois Urbana-Champaign
//               https://library.illinois.edu/enx

// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal with
// the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to
// do so, subject to the following conditions:
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimers.
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimers in the documentation
//   and/or other materials provided with the distribution.
// * Neither the names of IDEA Lab, Grainger Engineering Library Information Center,
//   nor the names of its contributors may be used to endorse or promote products
//   derived from this Software without specific prior written permission.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL THE
// CONTRIBUTORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS WITH THE
// SOFTWARE.


using BzKovSoft.ObjectSlicerSamples;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//UnityEvent_Extemsions to send client information to funcions specified in editor
[System.Serializable] public class UnityEvent_Int : UnityEvent<int> { }
[System.Serializable] public class UnityEvent_String : UnityEvent<string> { }

//For handling different type of text between clients
public enum STRINGTYPE
{
    TUTORIAL,
    CLIENT_NAME,
    DIALOGUE,
}
public struct New_Text
{
    public int target;
    public int stringType;
    public string text;

}

/// <summary>
/// This class is meant to:
/// --- set up main player
/// --- add/remove users
/// --- maintain references to all network elements in scene 
/// --- provides funcions to attach to make connection between button and imported assets (SetUp_ButtonURL.cs)
/// --- provides funcions to call to update elements (NetworkUpdateHandler.cs)
/// </summary>
public class ClientSpawnManager : SingletonComponent<ClientSpawnManager>
{
    public static ClientSpawnManager Instance
    {
        get { return ((ClientSpawnManager)_Instance); }
        set { _Instance = value; }
    }

    //Entities for our avatar refences => RootTransform, Head, Left and Right Hand 
    [HideInInspector] public Entity_Data mainPlayer_RootTransformData;
    [HideInInspector] public Entity_Data mainPlayer_head;
    [HideInInspector] public Entity_Data mainPlayer_L_Hand;
    [HideInInspector] public Entity_Data mainPlayer_R_Hand;

    [Header("Current Avatar Reference Setup")]
    public AvatarEntityGroup mainPlayer_AvatarEntityGroup;

    [Header("UI Client Tag ")]
    public ChildTextCreateOnCall clientTagSetup;
    private bool isMainClientInitialized = false;
    private GameObject mainPlayer;

    [Header("Spawn_Setup")]
    public GameObject clientPrefab;
    public Vector3 centerAvatarSpawnLocation;
    public int clientReserveCount;
    public float spreadRadius;

    [Header("Scenes To Make Available")]
    public SceneList sceneListContainer;

    [Header("Drawing Line Renderer Prefab")]
    public Transform drawLineRendererPrefab;
    LineRenderer templateLR;

    //References for displaying user name tags and dialogue text
    private List<Text> clientUser_Names_UITextReference_list = new List<Text>();
    private List<Text> clientUser_Dialogue_UITextReference_list = new List<Text>();

    #region Lists And Dictionaries to store references in scene
    private List<int> client_ID_List = new List<int>();
    [HideInInspector] public List<GameObject> availableGO_List = new List<GameObject>();
    public Dictionary<int, AvatarEntityGroup> availableClientIDToAvatar_Dict = new Dictionary<int, AvatarEntityGroup>();

    public Dictionary<int, Net_Register_GameObject> entityID_To_NetObject_Dict = new Dictionary<int, Net_Register_GameObject>();
    public Dictionary<int, Rigidbody> entityID_To_RigidBody = new Dictionary<int, Rigidbody>();
    private List<Net_Register_GameObject> topLevelNetRegImportList = new List<Net_Register_GameObject>();

    //To have a reference to client Avatar, name and hand animator components 
    private Dictionary<int, int> clientIDToAvatarIndex = new Dictionary<int, int>();
    private Dictionary<int, string> clientIDToName = new Dictionary<int, string>();
    public Dictionary<int, Animator> clientAnimatorsDictionary = new Dictionary<int, Animator>();

    //list of decomposed for entire set locking
    public Dictionary<int, List<Net_Register_GameObject>> decomposedAssetReferences_Dict = new Dictionary<int, List<Net_Register_GameObject>>();
    #endregion

    [Header("Attach Funcions to Call Depending on Who the User is")]
    public UnityEvent onClient_IsTeacher;
    public UnityEvent onClient_IsStudent;

    [Header("String References")]
    private List<int> currentTextProcessingList = new List<int>();
    private List<string> currentTextProcessingList_Strings = new List<string>();
    public Dictionary<int, float> secondsToWaitDic = new Dictionary<int, float>();

    [Header("Scene References")]
    [HideInInspector] public List<string> scene_Additives_Loaded = new List<string>();
    List<AsyncOperation> sceneloading_asyncOperList = new List<AsyncOperation>();

    //UI Button Refferences
    [HideInInspector] public List<Button> assetButtonRegister_List;
    [HideInInspector] public List<Toggle> assetLockToggleRegister_List = new List<Toggle>();
    [HideInInspector] public List<Button> sceneButtonRegister_List;

    //Keep track of what is rendered or not without using altenating between on and off, which brings issues. 
    [HideInInspector] public List<bool> renderAssetFlag = new List<bool>();

    //Current Session Information updated by the network
    [HideInInspector] public SessionState currentSessionState;

    #region Initiation process --> ClientAvatars --> URL Downloads --> UI Setup --> SyncState
    public IEnumerator Start()
    {
        //Get our template LR to use for drawing
        //make a copy of prefav
        drawLineRendererPrefab = Instantiate(drawLineRendererPrefab);
       
        drawLineRendererPrefab.parent = this.transform;
        drawLineRendererPrefab.name = "External Client Drawing Contatiner";
        templateLR = drawLineRendererPrefab.GetComponent<LineRenderer>();
        if (!templateLR) Debug.LogError("No LineRender Template Referenced in ClientSpawnManager.cs");


        mainPlayer = GameObject.FindGameObjectWithTag("Player");
        if (!mainPlayer) Debug.LogError("Could not find mainplayer with tag: Player in ClientSpawnManager.cs");

        #region Setup our Client Avatar Data
        //get reference to the entitData to have our avatar information handy
        mainPlayer_RootTransformData = NetworkUpdateHandler.Instance.mainEntityData;

        //Create Entity_Data for each of our avatar components
        var mainPlayer_head = mainPlayer_AvatarEntityGroup._EntityContainer_Head.entity_data = ScriptableObject.CreateInstance<Entity_Data>();
        var mainPlayer_L_Hand = mainPlayer_AvatarEntityGroup._EntityContainer_hand_L.entity_data = ScriptableObject.CreateInstance<Entity_Data>();
        var mainPlayer_R_Hand = mainPlayer_AvatarEntityGroup._EntityContainer_hand_R.entity_data = ScriptableObject.CreateInstance<Entity_Data>();

        //Set Client IDs for each component
        mainPlayer_AvatarEntityGroup.clientID = NetworkUpdateHandler.Instance.client_id;
        mainPlayer_head.clientID = NetworkUpdateHandler.Instance.client_id;
        mainPlayer_L_Hand.clientID = NetworkUpdateHandler.Instance.client_id;
        mainPlayer_R_Hand.clientID = NetworkUpdateHandler.Instance.client_id;

        //Setup Entity ID 
        // TODO(rob): document and use correct entity IDs
        mainPlayer_head.entityID = 0;
        mainPlayer_L_Hand.entityID = 1;
        mainPlayer_R_Hand.entityID = 2;

        #endregion

        //wait until our avatars are setup in the scene
        yield return StartCoroutine(Instantiate_Reserved_Clients());
        GameStateManager.Instance.isClientAvatarLoading_Finished = true;

        //add ourselves
        AddNewClient(NetworkUpdateHandler.Instance.client_id, true);
     
        yield return new WaitUntil(() => (GameStateManager.Instance.isUISetup_Finished && currentSessionState != null));

        if (mainPlayer_RootTransformData.isTeacher)
            onClient_IsTeacher.Invoke();
        else
            onClient_IsStudent.Invoke();

        //Set everything to its default state before getting sync state
        if (sceneListContainer.sceneList.Count == 0)
            Debug.LogError("No Scenes available to Activate check scene reference");

        Refresh_CurrentState();
        NetworkUpdateHandler.Instance.On_Initiation_Loading_Finished();
    }
 
    #endregion

    #region Add and Remove Client Funcions

    //to indicate where our client should be placed considering early initiation calls
    private int mainClienSpawntIndex = -1;

    public void AddNewClient(int clientID, bool isMainPlayer = false)
    {
        if(GameStateManager.IsAlive)
        if (!GameStateManager.Instance.isClientAvatarLoading_Finished)
            return;

        //setup newclient
        if (!client_ID_List.Contains(clientID))
        {
            client_ID_List.Add(clientID);

            string nameLabel = default;
            //get name of client
            nameLabel = NetworkUpdateHandler.Instance.GetPlayerNameFromClientID(clientID);


            if (client_ID_List.Count >= clientReserveCount)
            {
                Debug.LogWarning("REACHED MAX CLIENTS IN GAME - ADD TO CLIENT RESERVE COUNT FOR MORE USERS");
                return;
            }

            //go through all available slots
            for (int i = 0; i < clientReserveCount - 1; i++)
            {
                //skip avatars that are already on, and your reserved spot
                if (availableGO_List[i].activeInHierarchy || mainClienSpawntIndex == i)
                    continue;

                AvatarEntityGroup non_mainClientData = availableGO_List[i].GetComponentInChildren<AvatarEntityGroup>();
                non_mainClientData.clientID = clientID;


                if (!availableClientIDToAvatar_Dict.ContainsKey(clientID))
                    availableClientIDToAvatar_Dict.Add(clientID, non_mainClientData);
                else
                    availableClientIDToAvatar_Dict[clientID] = non_mainClientData;

                if (!clientIDToAvatarIndex.ContainsKey(clientID))
                    clientIDToAvatarIndex.Add(clientID, i);
                else
                    clientIDToAvatarIndex[clientID] = i;

                if (!clientIDToName.ContainsKey(clientID))
                    clientIDToName.Add(clientID, nameLabel);
                else
                    clientIDToName[clientID] = nameLabel;

                //create main ui tag
                clientTagSetup.CreateTextFromString(nameLabel);

                //select how to handle avatars
                if (!isMainPlayer)
                {
                    availableGO_List[i].SetActive(true);

                    //setAnimatorReferences what I use to reference other peoples hands
                    int idForLeftHand = (clientID * 100) + (2);
                    int idForRightHand = (clientID * 100) + (3);

                    if (!clientAnimatorsDictionary.ContainsKey(idForLeftHand))
                    {
                        clientAnimatorsDictionary.Add(idForLeftHand, availableClientIDToAvatar_Dict[clientID]._EntityContainer_hand_L.GetComponent<Animator>());
                        clientAnimatorsDictionary.Add(idForRightHand, availableClientIDToAvatar_Dict[clientID]._EntityContainer_hand_R.GetComponent<Animator>());
                    }
                    else
                    {
                        clientAnimatorsDictionary[idForLeftHand] = availableClientIDToAvatar_Dict[clientID]._EntityContainer_hand_L.GetComponent<Animator>();
                        clientAnimatorsDictionary[idForRightHand] = availableClientIDToAvatar_Dict[clientID]._EntityContainer_hand_R.GetComponent<Animator>();
                    }

                    //set text label
                    New_Text newText = new New_Text
                    {
                        stringType = (int)STRINGTYPE.CLIENT_NAME,
                        target = clientID,//clientIDToAvatarIndex[clientID],
                        text = nameLabel,
                    };

                    Text_Refresh_Process(newText);
                }
                else
                {
                    availableGO_List[i].SetActive(false);

                    mainClienSpawntIndex = i;

                    var temp = availableClientIDToAvatar_Dict[clientID].transform;
                    var ROT = availableClientIDToAvatar_Dict[clientID]._EntityContainer_Head.entity_data.rot;

                    //To prevent offset issues when working with editor
#if !UNITY_EDITOR
                    //GameObject
                    mainPlayer.transform.position = temp.position;
                    mainPlayer.transform.rotation = new Quaternion(ROT.x, ROT.y, ROT.z, ROT.w);

                    //hands entity data
                    mainPlayer_RootTransformData.pos = temp.position;
                    mainPlayer_RootTransformData.rot = new Vector4(ROT.x, ROT.y, ROT.z, ROT.w);
                    mainPlayer.transform.parent.GetChild(0).localPosition = temp.position;
                    mainPlayer.transform.parent.GetChild(0).localRotation = new Quaternion(ROT.x, ROT.y, ROT.z, ROT.w);
#endif

                    //Turn Off Dummy 
                    var parObject = temp.parent.parent.gameObject;
                    parObject.name = "Main_Client";

                    isMainClientInitialized = true;

                }
                break;
            }



        }

    }
    public void RemoveClient(int clientID)
    {
        if (GameStateManager.IsAlive)
            if (!GameStateManager.Instance.isAssetLoading_Finished)
            return;

        DestroyClient(clientID);
    }

    public async void DestroyClient(int clientID)
    {
        if (client_ID_List.Contains(clientID))
        {
            //wait for setup to be complete for early calls
            while (!availableClientIDToAvatar_Dict.ContainsKey(clientID))
                await Task.Delay(1);


            clientTagSetup.DeleteTextFromString(clientIDToName[clientID]);

            availableClientIDToAvatar_Dict[clientID].transform.parent.gameObject.SetActive(false);
            //   _availableClientIDToGODict.Remove(clientID);

            client_ID_List.Remove(clientID);

        }

    }
    #endregion

    #region Register Network Managed Objects
    /// <summary>
    /// Allows ClientSpawnManager have reference to the network reference gameobject to update with calls
    /// </summary>
    /// <param name="nGO"></param>
    /// <param name="asseListIndex"> This is the url index in list</param>
    /// <param name="customEntityID"></param>
    public void LinkNewNetworkObject(GameObject nGO, int asseListIndex = -1, int customEntityID = 0)
    {
        //add a Net component to the object
        Net_Register_GameObject tempNet = nGO.AddComponent<Net_Register_GameObject>();

        //to look a decomposed set of objects we need to keep track of what Index we are iterating over regarding or importing assets to create sets
        //we keep a list reference for each index and keepon adding to it if we find an asset with the same id
        if (asseListIndex != -1)
            if (!decomposedAssetReferences_Dict.ContainsKey(asseListIndex))
            {
                List<Net_Register_GameObject> newNetLst = new List<Net_Register_GameObject>();
                newNetLst.Add(tempNet);
                decomposedAssetReferences_Dict.Add(asseListIndex, newNetLst);
            }
            else
            {
                List<Net_Register_GameObject> netList = decomposedAssetReferences_Dict[asseListIndex];
                netList.Add(tempNet);
                decomposedAssetReferences_Dict[asseListIndex] = netList;
            }

        //We then setup the data to be used through networking
        if (customEntityID != 0)
            tempNet.Instantiate(asseListIndex, customEntityID);
        else
            tempNet.Instantiate(asseListIndex);

    }

    /// <summary>
    /// Setup References For NetObjects 
    /// </summary>
    /// <param name="entityID"></param>
    /// <param name="nRG"></param>
    /// <returns></returns>
    public void RegisterNetWorkObject(int entityID, Net_Register_GameObject nRG)
    {
        entityID_To_NetObject_Dict.Add(entityID, nRG);

        //to have a list of the root gameobjects imported to use with our UI Buttons, we check if our import index is 
        //consistent with the asset imported object that is being registered and we check if it is a decomposed object
        if (nRG.assetImportIndex == topLevelNetRegImportList.Count && -1 != nRG.assetImportIndex)
            topLevelNetRegImportList.Add(nRG);
       
    }
    #endregion

    #region Funcions for UI Buttons

    /// <summary>
    /// used to turn on assets that were setup with SetUp_ButtonURL.
    /// </summary>
    /// <param name="assetImportIndex"></param>
    /// <param name="button"></param>
    /// <param name="sendNetworkCall is used to determine if we should send a call for others to render the specified object"></param>
    public void On_Button_RenderAsset(int assetImportIndex, bool activeState)
    {
        GameObject currentObj = default;
        Net_Register_GameObject netRegisterComponent = default;

        //if it is a call from button
        currentObj = topLevelNetRegImportList[assetImportIndex].gameObject;

       // var AssetImportIndex = entityID_To_NetObject_Dict[entityID].assetImportIndex;
        currentObj = topLevelNetRegImportList[assetImportIndex].gameObject;

        if (!currentObj)
            Debug.LogError("currentObj not found to render in ClientSpawnManager.cs");

        //use _renderAssetFlag list to maintain control of what should be rendered or not
        if (activeState)
        {
            currentObj.SetActive(true);

            //set call to update accross network
                netRegisterComponent = currentObj.GetComponent<Net_Register_GameObject>();

                if (!netRegisterComponent)
                    Debug.LogError("no netRegisterComponet found on currentObj in ClientSpawnManager.cs");

                NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
                {
                    sourceEntity_id = mainPlayer_RootTransformData.entityID,
                    targetEntity_id = netRegisterComponent.entity_data.entityID,
                    interactionType = (int)INTERACTIONS.RENDERING,
                });
        }
        else
        {
            currentObj.SetActive(false);

                netRegisterComponent = currentObj.GetComponent<Net_Register_GameObject>();

                NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
                {
                    sourceEntity_id = mainPlayer_RootTransformData.entityID,
                    targetEntity_id = netRegisterComponent.entity_data.entityID,
                    interactionType = (int)INTERACTIONS.NOT_RENDERING,

                });

        }
    }

    /// <summary>
    /// Render a new asset for this client only without inputing button reference
    /// </summary>
    /// <param name="entityID"></param>
    /// <param name="activeState"></param>
    public void Simulate_On_Button_RenderAsset(int entityID, bool activeState)
    {
        var AssetImportIndex = entityID_To_NetObject_Dict[entityID].assetImportIndex;
        GameObject currentObj = topLevelNetRegImportList[AssetImportIndex].gameObject;
        Button button = assetButtonRegister_List[AssetImportIndex];
       
        if (!activeState)
        {
            renderAssetFlag[AssetImportIndex] = true;

            button.SetButtonStateColor(Color.green, true);
    
            currentObj.SetActive(true);
        }
        else
        {
            renderAssetFlag[AssetImportIndex] = false;

            button.SetButtonStateColor(Color.white, false);

            currentObj.SetActive(false);
        }
    }


    /// <summary>
    /// Select which scene should be rendered by providing data of the scenereference and appropriate button of UI
    /// </summary>
    /// <param name="sceneRef"></param>
    /// <param name="button"></param>
    public void On_Select_Scene_Refence_Button(SceneReference sceneRef, Button button)
    {

        //  Refresh_CurrentState();
        NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
        {
            sourceEntity_id = mainPlayer_RootTransformData.entityID,
            targetEntity_id = sceneRef.sceneIndex,
            interactionType = (int)INTERACTIONS.CHANGE_SCENE,//cant covert since it gives 0 at times instead of the real type?

        });

        Simulate_On_Select_Scene_Refence(sceneRef.sceneIndex);
    }

    /// <summary>
    /// Load a new scene additively and remove the other ones for this client only
    /// </summary>
    /// <param name="sceneID"></param>
    public void Simulate_On_Select_Scene_Refence(int sceneID)
    {
        StartCoroutine(CoroutineSimulate_On_Select_Scene_Refence(sceneID));
    }
    public IEnumerator CoroutineSimulate_On_Select_Scene_Refence(int sceneID)
    {
        for (int i = 0; i < sceneloading_asyncOperList.Count; i++)
            yield return new WaitUntil(() => sceneloading_asyncOperList[i].isDone);

        sceneloading_asyncOperList.Clear();

        foreach (string sceneLoaded in scene_Additives_Loaded)
        {
            foreach (SceneReference sceneInList in sceneListContainer.sceneReferenceList)
            {
                if (sceneInList.name == sceneLoaded)
                {
                    //we already loaded this scene return;
                    if (sceneInList.sceneIndex == sceneID)
                        yield break;


                }
            }
        }

        Debug.LogError(SceneManager.sceneCount);
      //  var Scenes = SceneManager.GetAllScenes();
        //unload all present scenes
        for (int i = 1; i < SceneManager.sceneCount; i++)
        {
            sceneloading_asyncOperList.Add( SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i)));
        }

        //clear the list
        scene_Additives_Loaded.Clear();

        //add the scene that is being loaded
        scene_Additives_Loaded.Add(sceneListContainer.sceneReferenceList[sceneID].name);

        sceneloading_asyncOperList.Add(SceneManager.LoadSceneAsync(sceneListContainer.sceneReferenceList[sceneID].name, LoadSceneMode.Additive));

        //enable previous scene button
        foreach (var button in sceneButtonRegister_List)
            button.interactable = true;

        if (sceneButtonRegister_List.Count > 0)
            //disable current scene button to avoid re loading same scene again
            sceneButtonRegister_List[sceneID].interactable = false;

        
    }


  

    #endregion

    #region Client and Object Receive Calls

    /// <summary>
    /// To receive calls to update avatars and assets in scene
    /// </summary>
    /// <param name="newData"></param>
    public void Client_Refresh(Position newData)
    {
        if (GameStateManager.IsAlive)
            if (!GameStateManager.Instance.isAssetLoading_Finished)
            return;

        // CLIENT_REFRESH_PROCESS(newData);
        if (!client_ID_List.Contains(newData.clientId) && newData.entityType != (int)Entity_Type.objects && newData.entityType != (int)Entity_Type.physicsObject)
        {
            AddNewClient(newData.clientId);
            Debug.Log(newData.clientId + " : client ID is being registered through Client_Refresh");
        }

        //MOVE CLIENTS AND OBJECTS
        switch (newData.entityType)
        {
            //HEAD MOVE
            case (int)Entity_Type.users_head:

                if (availableClientIDToAvatar_Dict.ContainsKey(newData.clientId))
                {
                    availableClientIDToAvatar_Dict[newData.clientId]._EntityContainer_Head.transform.position = newData.pos;
                    availableClientIDToAvatar_Dict[newData.clientId]._EntityContainer_Head.transform.rotation = new Quaternion(newData.rot.x, newData.rot.y, newData.rot.z, newData.rot.w);
                }
                else
                    Debug.LogWarning("Client ID : " + newData.clientId + " not found in Dictionary dropping head movement packet");
                break;

            //HANDL MOVE
            case (int)Entity_Type.users_Lhand:

                if (availableClientIDToAvatar_Dict.ContainsKey(newData.clientId))
                {
                    Transform handTransRef = availableClientIDToAvatar_Dict[newData.clientId]._EntityContainer_hand_L.transform;

                    if (!handTransRef.gameObject.activeInHierarchy)
                        handTransRef.gameObject.SetActive(true);

                    handTransRef.position = newData.pos;
                    handTransRef.rotation = new Quaternion(newData.rot.x, newData.rot.y, newData.rot.z, newData.rot.w);

                    //To reference the correct hand animator
                    int leftHandID = (newData.clientId * 100) + (2);
                    if (clientAnimatorsDictionary.ContainsKey(leftHandID))
                    {
                        clientAnimatorsDictionary[leftHandID].Play("Take", -1, newData.scaleFactor);
                    }
                }
                else
                    Debug.LogWarning("Client ID : " + newData.clientId + " not found in Dictionary dropping left hand movement packet");
                break;

            //HANDR MOVE
            case (int)Entity_Type.users_Rhand:
                if (availableClientIDToAvatar_Dict.ContainsKey(newData.clientId))
                {
                    Transform handTransRef = availableClientIDToAvatar_Dict[newData.clientId]._EntityContainer_hand_R.transform;

                    if (!handTransRef.gameObject.activeInHierarchy)
                        handTransRef.gameObject.SetActive(true);

                    handTransRef.position = newData.pos;
                    handTransRef.rotation = new Quaternion(newData.rot.x, newData.rot.y, newData.rot.z, newData.rot.w);

                    int rightHandID = (newData.clientId * 100) + (3);
                    if (clientAnimatorsDictionary.ContainsKey(rightHandID))
                    {
                        clientAnimatorsDictionary[rightHandID].Play("Take", -1, newData.scaleFactor);
                    }
                }
                else
                    Debug.LogWarning("Client ID : " + newData.clientId + " not found in Dictionary dropping right hand movement packet");
                break;
            //OBJECT MOVE
            case (int)Entity_Type.objects:

                if (entityID_To_NetObject_Dict.ContainsKey(newData.entityId))
                {
                    entityID_To_NetObject_Dict[newData.entityId].transform.position = newData.pos;
                    entityID_To_NetObject_Dict[newData.entityId].transform.rotation = newData.rot;

                    UnityExtensionMethods.SetGlobalScale(entityID_To_NetObject_Dict[newData.entityId].transform, Vector3.one * newData.scaleFactor);
                }
                else
                    Debug.LogWarning("Entity ID : " + newData.entityId + "not found in Dictionary dropping object movement packet");

                break;

            case (int)Entity_Type.physicsObject:

                //alternate kinematic to allow for sending non physics transform updates;
                if (entityID_To_NetObject_Dict.ContainsKey(newData.entityId))
                {
                    if (!entityID_To_RigidBody.ContainsKey(newData.entityId))
                    {
                        entityID_To_RigidBody.Add(newData.entityId, entityID_To_NetObject_Dict[newData.entityId].GetComponent<Rigidbody>());
                    }

                    var rb = entityID_To_RigidBody[newData.entityId];

                    if (!rb)
                    {
                        Debug.LogError("There is no rigidbody in netobject entity id: " + newData.entityId);
                        return;
                    }

                    rb.isKinematic = true;
                    entityID_To_NetObject_Dict[newData.entityId].transform.position = newData.pos;
                    entityID_To_NetObject_Dict[newData.entityId].transform.rotation = newData.rot;
                    UnityExtensionMethods.SetGlobalScale(entityID_To_NetObject_Dict[newData.entityId].transform, Vector3.one * newData.scaleFactor);



                }
                else
                    Debug.LogWarning("Entity ID : " + newData.entityId + "not found in Dictionary dropping physics object movement packet");

                break;

            case (int)Entity_Type.physicsEnd:

                //alternate kinematic to allow for sending non physics transform updates;
                if (entityID_To_NetObject_Dict.ContainsKey(newData.entityId))
                {
                    //skip opperation if current object is grabbed to avoid turning physics back on
                    if (entityID_To_NetObject_Dict[newData.entityId].entity_data.isCurrentlyGrabbed)
                        return;


                    if (!entityID_To_RigidBody.ContainsKey(newData.entityId))
                    {
                        entityID_To_RigidBody.Add(newData.entityId, entityID_To_NetObject_Dict[newData.entityId].GetComponent<Rigidbody>());
                    }

                    var rb = entityID_To_RigidBody[newData.entityId];

                    if (!rb)
                    {
                        Debug.LogError("There is no rigidbody in netobject entity id: " + newData.entityId);
                        return;
                    }

                    rb = entityID_To_NetObject_Dict[newData.entityId].GetComponent<Rigidbody>();
                    rb.isKinematic = false;

                }
                else
                    Debug.LogWarning("Entity ID : " + newData.entityId + "not found in Dictionary dropping physics object movement packet");

                break;
        }
    }
    #endregion

    #region Interaction Receive Calls
    /// <summary>
    /// To receive interactio events to update our scene accordingly
    /// </summary>
    /// <param name="newData"></param>
    public void Interaction_Refresh(Interaction newData)
    {
        if (GameStateManager.IsAlive)
            if (!GameStateManager.Instance.isUISetup_Finished)
                return;
            

        switch (newData.interactionType)
        {

            case (int)INTERACTIONS.RENDERING:

                Simulate_On_Button_RenderAsset(newData.targetEntity_id, false);

                break;

            case (int)INTERACTIONS.NOT_RENDERING:

                Simulate_On_Button_RenderAsset(newData.targetEntity_id, true);

                break;

            case (int)INTERACTIONS.GRAB:

                if (entityID_To_NetObject_Dict.ContainsKey(newData.targetEntity_id))
                    entityID_To_NetObject_Dict[newData.targetEntity_id].entity_data.isCurrentlyGrabbed = true;
                else
                    Debug.LogWarning("Client Entity does not exist for Grab interaction--- EntityID " + newData.targetEntity_id);

                break;

            case (int)INTERACTIONS.DROP:

                if (entityID_To_NetObject_Dict.ContainsKey(newData.targetEntity_id))
                    entityID_To_NetObject_Dict[newData.targetEntity_id].entity_data.isCurrentlyGrabbed = false;
                else
                    Debug.LogWarning("Client Entity does not exist for Drop interaction--- EntityID" + newData.targetEntity_id);

                break;


            case (int)INTERACTIONS.CHANGE_SCENE:

                //check the loading wait for changing into a new scene - to avoid loading multiple scenes
                Simulate_On_Select_Scene_Refence(newData.targetEntity_id);

                break;

            case (int)INTERACTIONS.LOCK:

                //disable button interaction for others
                if (newData.sourceEntity_id != -1)
                    SimulateLockToggleButtonPress(newData.sourceEntity_id, true, false);

                break;

            case (int)INTERACTIONS.UNLOCK:

                if (newData.sourceEntity_id != -1)
                    SimulateLockToggleButtonPress(newData.sourceEntity_id, false, false);

                break;

        }

    }


    //we need funcions for our UI buttons to link up, which can be affected by our client selecting the button or when we get a call to invoke it.
    //We attach funcions through SetUp_ButtonURL.cs but those funcions send network events, to avoid sending a network event when receiving a call, we created
    //another funcion here to avoid sending them by simulating a button press (Having the same funcionality when pressing the button and when receiving a call from 
    //network that it was turned on/off)
    public void SimulateLockToggleButtonPress(int assetIndex, bool currentLockStatus, bool isNetwork)
    {
        foreach (Net_Register_GameObject item in decomposedAssetReferences_Dict[assetIndex])
            item.entity_data.isCurrentlyGrabbed = currentLockStatus;

        //Unity's UIToggle funcionality does not show the graphic element until someone fires the event (is on), simmulating this behavior when receiving 
        //other peoples calls makes us use a image as a parent of a graphic element that we can use to turn on and off instead   
        assetLockToggleRegister_List[assetIndex].graphic.transform.parent.gameObject.SetActive(currentLockStatus);
 

        if (isNetwork)
        {
            int lockState = 0;

            //SETUP and send network lockstate
            if (currentLockStatus)
                lockState = (int)INTERACTIONS.LOCK;
            else
                lockState = (int)INTERACTIONS.UNLOCK;

            NetworkUpdateHandler.Instance.InteractionUpdate(new Interaction
            {
                sourceEntity_id = assetIndex, // TODO(rob): use client hand ids or 0 for desktop? 
                targetEntity_id = ClientSpawnManager.Instance.decomposedAssetReferences_Dict[assetIndex][0].entity_data.entityID,
                interactionType = lockState,

            });
        }
    }

   

    
    #endregion

    #region Draw Receive Calls
    //Setting up Line Rendering Calls
    private Dictionary<int, LineRenderer> lineRenderersInQueue = new Dictionary<int, LineRenderer>();
    private Dictionary<int, int> allStrokeIDValidator = new Dictionary<int, int>();

    //To avoid duplicating stroke ids because sending different ids states ma 
    public void Draw_Refresh(Draw newData)
    {
        LineRenderer currentLineRenderer = default;

        //we start a new line if there is no ID already corresponding to one in the scene
        if (!allStrokeIDValidator.ContainsKey(newData.strokeId))
        {
            //  GameObject lineRendGO = new GameObject("LineR:" + newData.strokeId);
            GameObject lineRendCopy = Instantiate(drawLineRendererPrefab).gameObject;
            lineRendCopy.name = "LineR:" + newData.strokeId;

            lineRendCopy.transform.SetParent(drawLineRendererPrefab, true);
            currentLineRenderer = lineRendCopy.GetComponent<LineRenderer>();
            currentLineRenderer.positionCount = 0;

            allStrokeIDValidator.Add(newData.strokeId, newData.strokeId);
            lineRenderersInQueue.Add(newData.strokeId, currentLineRenderer);
        }

        //we get reference to the linenderer we are supposed to be working with 
        if (lineRenderersInQueue.ContainsKey(newData.strokeId))
            currentLineRenderer = lineRenderersInQueue[newData.strokeId];

        switch (newData.strokeType)
        {
            //Continues A Line
            case (int)Entity_Type.Line:

                currentLineRenderer.sharedMaterial = templateLR.sharedMaterial;

                var brushColor = new Vector4(newData.curColor.x, newData.curColor.y, newData.curColor.z, newData.curColor.w);
                currentLineRenderer.startColor = brushColor;
                currentLineRenderer.endColor = brushColor;

                ++currentLineRenderer.positionCount;
                currentLineRenderer.SetPosition(currentLineRenderer.positionCount - 1, newData.curStrokePos);

                break;

            //Ends A Line A completes its setup
            case (int)Entity_Type.LineEnd:

                ++currentLineRenderer.positionCount;
                currentLineRenderer.SetPosition(currentLineRenderer.positionCount - 1, newData.curStrokePos);

                LineRenderer lr = drawLineRendererPrefab.GetComponent<LineRenderer>();

                //used to set correct pivot point when scalling object by grabbing
                GameObject pivot = new GameObject("LineRender:" + (newData.strokeId), typeof(BoxCollider));
                pivot.tag = "Drawing";

                if (!entityID_To_NetObject_Dict.ContainsKey(newData.strokeId))
                    LinkNewNetworkObject(pivot, newData.strokeId, newData.strokeId);

                var bColl = pivot.GetComponent<BoxCollider>();

                Bounds newBounds = new Bounds(currentLineRenderer.GetPosition(0), Vector3.one * 0.01f);

                for (int i = 0; i < currentLineRenderer.positionCount; i++)
                    newBounds.Encapsulate(new Bounds(currentLineRenderer.GetPosition(i), Vector3.one * 0.01f));

                pivot.transform.position = newBounds.center;
                bColl.center = currentLineRenderer.transform.position;
                bColl.size = newBounds.size;

                currentLineRenderer.transform.SetParent(pivot.transform, true);
                pivot.transform.SetParent(drawLineRendererPrefab.transform);

                break;

            //Deletes a Line
            case (int)Entity_Type.LineDelete:


                if (entityID_To_NetObject_Dict.ContainsKey(newData.strokeId))
                {
                    if (lineRenderersInQueue.ContainsKey(newData.strokeId))
                        lineRenderersInQueue.Remove(newData.strokeId);


                    Destroy(entityID_To_NetObject_Dict[newData.strokeId].gameObject);
                    entityID_To_NetObject_Dict.Remove(newData.strokeId);
                }
                break;
        }
    }

    #endregion

    #region Text Receive Calls

    public struct SpeechToText
    {
        public int session_id;
        public int client_id;
        public string text;
        public string type;
        public int ts;
    }

    public void Text_Refresh(String data)
    {
        var deserializedData = JsonUtility.FromJson<SpeechToText>(data);
        New_Text newStt;
        newStt.target = deserializedData.client_id;
        newStt.text = deserializedData.text;
        newStt.stringType = (int)STRINGTYPE.DIALOGUE;
        Text_Refresh_Process(newStt);
    }
    public void Text_Refresh_Process(New_Text newText)
    {
        if(GameStateManager.IsAlive)
        if (!GameStateManager.Instance.isAssetLoading_Finished)
            return;

        if (!client_ID_List.Contains(newText.target))
        {
            Debug.LogWarning("No client Found, Can't render text");
            return;
        }

        switch (newText.stringType)
        {
            case (int)STRINGTYPE.TUTORIAL:
                break;

            case (int)STRINGTYPE.DIALOGUE:

                //Get client index for text look up to use for displaying
                var clientIndex = clientIDToAvatarIndex[newText.target];
                string foo = SplitWordsByLength(newText.text, 20);
                StartCoroutine(SetTextTimer(clientIndex, foo, 0.9f * Mathf.Log(newText.text.Length)));


                break;

            case (int)STRINGTYPE.CLIENT_NAME:

                clientIndex = clientIDToAvatarIndex[newText.target];
                clientUser_Names_UITextReference_list[clientIndex].text = newText.text;
                break;

        }

    }



    private static string SplitWordsByLength(string str, int maxLength)
    {
        List<string> chunks = new List<string>();
        while (str.Length > 0)
        {
            if (str.Length <= maxLength)                    //if remaining string is less than length, add to list and break out of loop
            {
                chunks.Add(str);
                break;
            }

            string chunk = str.Substring(0, maxLength);     //Get maxLength chunk from string.

            if (char.IsWhiteSpace(str[maxLength]))          //if next char is a space, we can use the whole chunk and remove the space for the next line
            {
                chunks.Add(chunk + "\n");
                str = str.Substring(chunk.Length + 1);      //Remove chunk plus space from original string
            }
            else
            {
                int splitIndex = chunk.LastIndexOf(' ');    //Find last space in chunk.
                if (splitIndex != -1)                       //If space exists in string,
                    chunk = chunk.Substring(0, splitIndex); //  remove chars after space.
                str = str.Substring(chunk.Length + (splitIndex == -1 ? 0 : 1));      //Remove chunk plus space (if found) from original string
                chunks.Add(chunk + "\n");                          //Add to list
            }
        }
        return string.Concat(chunks);
    }

    //to avoid changing text too quickly before being viable
    public IEnumerator SetTextTimer(int textIndex, string textD, float seconds = 5)
    {
        if (!secondsToWaitDic.ContainsKey(textIndex))
            secondsToWaitDic.Add(textIndex, seconds);
        else
            secondsToWaitDic[textIndex] += seconds;

        if (!currentTextProcessingList.Contains(textIndex))
        {
            clientUser_Dialogue_UITextReference_list[textIndex].text = textD;
            currentTextProcessingList.Add(textIndex);
            StartCoroutine(ShutOFFText(textIndex, seconds));

        }
        else

        {
            currentTextProcessingList.Add(textIndex);
            yield return new WaitForSeconds(secondsToWaitDic[textIndex]);

            secondsToWaitDic[textIndex] -= seconds;

            StartCoroutine(ShutOFFText(textIndex, seconds));
            clientUser_Dialogue_UITextReference_list[textIndex].text = textD;

        }
        yield return null;

    }


    public IEnumerator ShutOFFText(int textIndex, float seconds)
    {

        clientUser_Dialogue_UITextReference_list[textIndex].transform.parent.gameObject.SetActive(true);

        //  secondsToWaitDic[index] -= seconds;
        yield return new WaitForSeconds(seconds);

        currentTextProcessingList.Remove(textIndex);

        if (!currentTextProcessingList.Contains(textIndex))
        {
            clientUser_Dialogue_UITextReference_list[textIndex].transform.parent.gameObject.SetActive(false);
        }

    }
    #endregion


    #region Setup Client Spots
    public IEnumerator Instantiate_Reserved_Clients()
    {

        float degrees = 360f / clientReserveCount;
        Vector3 offset = new Vector3(0, 0, 4);

        //set it under our instantiation manager
        var clientCollection = new GameObject("Client Collection").transform;
        clientCollection.parent = this.transform;

        GameObject instantiation = default;

        //Create all players with simple GameObject Representation
        for (int i = 0; i < clientReserveCount; i++)
        {

            Vector3 TransformRelative = Quaternion.Euler(0f, degrees * i + 1, 0f) * (transform.position + offset);

            instantiation = Instantiate(clientPrefab, Vector3.zero, Quaternion.identity);

            instantiation.name = $"Client_{i + 1}";
            //Obtain each avatars UI_TEXT REFERENCE FROM THEIR CANVAS IN PREFAB
            //GET HEAD TO GET CANVAS FOR TEXT COMPONENTS
            Transform canvas = instantiation.transform.GetChild(0).GetComponentInChildren<Canvas>().transform;

            //GET APPROPRIATE TEXT COMPONENT CHARACTER NAME AND DIALOGUE
            clientUser_Names_UITextReference_list.Add(canvas.GetChild(0).GetComponent<Text>());
            canvas.GetChild(0).GetComponent<Text>().text = $"Client_{i + 1}";

            clientUser_Dialogue_UITextReference_list.Add(canvas.GetChild(1).GetChild(0).GetComponent<Text>());

            //Set up links for network call references
            var non_mainClientData = instantiation.GetComponentInChildren<AvatarEntityGroup>(true);

            non_mainClientData._EntityContainer_hand_L.entity_data = ScriptableObject.CreateInstance<Entity_Data>();
            non_mainClientData._EntityContainer_hand_R.entity_data = ScriptableObject.CreateInstance<Entity_Data>();
            var clientData_Main = non_mainClientData._EntityContainer_Head.entity_data = ScriptableObject.CreateInstance<Entity_Data>();

            instantiation.transform.position = TransformRelative;

            //Same orientation
            TransformRelative.y = centerAvatarSpawnLocation.y;

            Quaternion newRot = Quaternion.LookRotation(centerAvatarSpawnLocation - TransformRelative, Vector3.up);

            clientData_Main.rot = new Vector4(newRot.x, newRot.y, newRot.z, newRot.w);

            non_mainClientData.transform.parent.localRotation = new Quaternion(newRot.x, newRot.y, newRot.z, newRot.w);

            availableGO_List.Add(instantiation);

            instantiation.SetActive(false);

            instantiation.transform.SetParent(clientCollection);
        }

        yield return null;
    }

    private Vector3 GetSlotLocation(int slot)
    {

        Vector3 location = transform.position;

        float degrees = 360f / clientReserveCount + 1;
        // degrees = 30;
        degrees *= slot;

        //   location.y = -1f;

        location.x = Mathf.Cos(Mathf.Deg2Rad * degrees);
        location.x *= spreadRadius;
        location.z = Mathf.Cos(Mathf.Deg2Rad * degrees);
        location.z *= spreadRadius;
        return location;
    }

    #endregion

    #region SessionState Sync Calls

    public void Refresh_CurrentState()
    {
        Simulate_On_Select_Scene_Refence(currentSessionState.scene);


        //add clients
        foreach (var clientID in currentSessionState.clients)
        {
            if (clientID != NetworkUpdateHandler.Instance.client_id)
                AddNewClient(clientID);
        }


        foreach (var downloadedGO_NetReg in topLevelNetRegImportList)
        {

            var isAssetOn = false;
            var isLockOn = false;

            foreach (var entity in currentSessionState.entities)
            {

                if (entity.id == downloadedGO_NetReg.entity_data.entityID)
                {
                    if (entity.render)
                        isAssetOn = true;

                    if (entity.locked)
                        isLockOn = true;

                    break;
                }
            }

            if (isAssetOn)
                Simulate_On_Button_RenderAsset(downloadedGO_NetReg.entity_data.entityID, false);
            else
                Simulate_On_Button_RenderAsset(downloadedGO_NetReg.entity_data.entityID, true);


            if (isLockOn)
                Interaction_Refresh(new Interaction(sourceEntity_id: entityID_To_NetObject_Dict[downloadedGO_NetReg.entity_data.entityID].assetImportIndex, targetEntity_id: downloadedGO_NetReg.entity_data.entityID, interactionType: (int)INTERACTIONS.LOCK));
            else
                Interaction_Refresh(new Interaction(sourceEntity_id: entityID_To_NetObject_Dict[downloadedGO_NetReg.entity_data.entityID].assetImportIndex, targetEntity_id: downloadedGO_NetReg.entity_data.entityID, interactionType: (int)INTERACTIONS.UNLOCK));

          
        }

        foreach (var entity in currentSessionState.entities)
        {
            if(entity.latest.Length > 1)
            Client_Refresh(NetworkUpdateHandler.Instance.DeSerializeCoordsStruct(entity.latest));
        }
    }

    [System.Serializable]
    public struct EntityState
    {
        public int id;
        public float[] latest; //possition struct 
        public bool render;
        public bool locked;
    }

    [System.Serializable]
    public class SessionState
    {
        public int[] clients;
        public EntityState[] entities;
        public int scene;
        public bool isRecording;

    }

    public void SyncSessionState(string stateString)
    {
        var stateStruct = JsonUtility.FromJson<SessionState>(stateString);

        currentSessionState = stateStruct;

        //only update when things are setup if not keep reference in current session state class.
        if (GameStateManager.IsAlive)
            if (GameStateManager.Instance.isUISetup_Finished)
                Refresh_CurrentState();

    }


    #endregion
}
