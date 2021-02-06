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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Unity.Transforms;

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
//types of data in scene
public enum Entity_Type
{
    none = -1,
    users_head = 0,
    users_Lhand = 1,
    users_Rhand = 2,
    objects = 3,

    main_Player = 5,

    Line = 10,
    LineEnd = 11,
    LineDelete = 12,

    physicsObject = 4,
    physicsEnd = 8,
}

#region INTERACTION TYPES
public enum INTERACTIONS
{
    LOOK = 0,
    LOOK_END = 1,
    RENDERING = 2,
    NOT_RENDERING = 3,
    GRAB = 4,
    DROP = 5,
    CHANGE_SCENE = 6,
    SLICE_OBJECT = 7,
    LOCK = 8,
    UNLOCK = 9,
    LINE = 10,
    LINE_END = 11,

}
#endregion
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

    [Header("Current Avatar Reference Setup")]
    public AvatarEntityGroup mainPlayer_AvatarEntityGroup;

    [Header("UI Client Tag ")]
  //  public ChildTextCreateOnCall clientTagSetup;
    private bool isMainClientInitialized = false;
    private GameObject mainPlayer;

    [Header("Spawn_Setup")]
    public GameObject clientPrefab;
    public Vector3 centerAvatarSpawnLocation;
    public int clientReserveCount;
    public float spreadRadius;

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

    public Dictionary<int, NetworkAssociatedGameObject> entityID_To_NetObject_Dict = new Dictionary<int, NetworkAssociatedGameObject>();
    public Dictionary<int, Rigidbody> entityID_To_RigidBody = new Dictionary<int, Rigidbody>();
    public List<NetworkAssociatedGameObject> rootLevelNetworkAssociatedGameObjectList = new List<NetworkAssociatedGameObject>();

    //To have a reference to client Avatar, name and hand animator components 
    private Dictionary<int, int> clientIDToAvatarIndex = new Dictionary<int, int>();
    private Dictionary<int, string> clientIDToName = new Dictionary<int, string>();
    public Dictionary<int, Animator> clientAnimatorsDictionary = new Dictionary<int, Animator>();

    //list of decomposed for entire set locking
    public Dictionary<int, List<NetworkAssociatedGameObject>> decomposedAssetReferences_Dict = new Dictionary<int, List<NetworkAssociatedGameObject>>();
    #endregion

    [Header("Attach Funcions to Call Depending on Who the User is")]
    public UnityEvent onClient_IsTeacher;
    public UnityEvent onClient_IsStudent;

    [Header("String References")]
    private List<int> currentTextProcessingList = new List<int>();
    private List<string> currentTextProcessingList_Strings = new List<string>();
    public Dictionary<int, float> secondsToWaitDic = new Dictionary<int, float>();

    //Current Session Information updated by the network
    [HideInInspector] public SessionState currentSessionState;

    #region ECS Funcionality Fields 
    public List<Entity> topLevelEntityList = new List<Entity>();
    EntityManager entityManager;
    #endregion

    #region Initiation process --> ClientAvatars --> URL Downloads --> UI Setup --> SyncState
    public IEnumerator Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        //Get our template LR to use for drawing
        //make a copy of prefav
        drawLineRendererPrefab = Instantiate(drawLineRendererPrefab);

        drawLineRendererPrefab.parent = this.transform;
        drawLineRendererPrefab.name = "External Client Drawing Contatiner";
        templateLR = drawLineRendererPrefab.GetComponent<LineRenderer>();
        if (!templateLR) Debug.LogError("No LineRender Template Referenced in ClientSpawnManager.cs");

        mainPlayer = GameObject.FindGameObjectWithTag("Player");
        if (!mainPlayer) Debug.LogError("Could not find mainplayer with tag: Player in ClientSpawnManager.cs");

        //wait until our avatars are setup in the scene
        yield return StartCoroutine(Instantiate_Reserved_Clients());
        GameStateManager.Instance.isAvatarLoadingFinished = true;

        //add ourselves
        AddNewClient(NetworkUpdateHandler.Instance.client_id, true);

        yield return new WaitUntil(() => (UIManager.Instance.IsReady() && currentSessionState != null));

        if (NetworkUpdateHandler.Instance.isTeacher != 0)
            onClient_IsTeacher.Invoke();
        else
            onClient_IsStudent.Invoke();

        //Set everything to its default state before getting sync state
        //if (sceneListContainer.sceneList.Count == 0)
        //    Debug.LogError("No Scenes available to Activate check scene reference");

        Refresh_CurrentState();
        NetworkUpdateHandler.Instance.On_Initiation_Loading_Finished();
     
    }

    #endregion

    #region Add and Remove Client Funcions

    //to indicate where our client should be placed considering early initiation calls
    private int mainClienSpawntIndex = -1;

    public void AddNewClient(int clientID, bool isMainPlayer = false)
    {
        if (GameStateManager.IsAlive)
            if (!GameStateManager.Instance.isAvatarLoadingFinished)
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

                AvatarEntityGroup avatarEntityGroup = availableGO_List[i].GetComponentInChildren<AvatarEntityGroup>();
                avatarEntityGroup.clientID = clientID;


               // entityManager.AddComponentData(avatarEntityGroup.rootEntity, new NetworkEntityIdentificationComponentData {clientID = clientID });
                


                if (!availableClientIDToAvatar_Dict.ContainsKey(clientID))
                    availableClientIDToAvatar_Dict.Add(clientID, avatarEntityGroup);
                else
                    availableClientIDToAvatar_Dict[clientID] = avatarEntityGroup;

                if (!clientIDToAvatarIndex.ContainsKey(clientID))
                    clientIDToAvatarIndex.Add(clientID, i);
                else
                    clientIDToAvatarIndex[clientID] = i;

                if (!clientIDToName.ContainsKey(clientID))
                    clientIDToName.Add(clientID, nameLabel);
                else
                    clientIDToName[clientID] = nameLabel;

                //create main ui tag
                UIManager.Instance.clientTagSetup.CreateTextFromString(nameLabel, clientID);
              //  clientTagSetup.CreateTextFromString(nameLabel);

                //select how to handle avatars
                if (!isMainPlayer)
                {
                    availableGO_List[i].SetActive(true);

                    //setAnimatorReferences what I use to reference other peoples hands
                    int idForLeftHand = (clientID * 100) + (2);
                    int idForRightHand = (clientID * 100) + (3);

                    if (!clientAnimatorsDictionary.ContainsKey(idForLeftHand))
                    {
                        clientAnimatorsDictionary.Add(idForLeftHand, availableClientIDToAvatar_Dict[clientID].avatarComponent_hand_L.GetComponent<Animator>());
                        clientAnimatorsDictionary.Add(idForRightHand, availableClientIDToAvatar_Dict[clientID].avatarComponent_hand_R.GetComponent<Animator>());
                    }
                    else
                    {
                        clientAnimatorsDictionary[idForLeftHand] = availableClientIDToAvatar_Dict[clientID].avatarComponent_hand_L.GetComponent<Animator>();
                        clientAnimatorsDictionary[idForRightHand] = availableClientIDToAvatar_Dict[clientID].avatarComponent_hand_R.GetComponent<Animator>();
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
                    var ROT = entityManager.GetComponentData<Rotation>(availableClientIDToAvatar_Dict[clientID].rootEntity).Value.value;//.entity_data.rot;
                    
                    //To prevent offset issues when working with editor
#if !UNITY_EDITOR
                    //GameObject
                    mainPlayer.transform.position = temp.position;
                    mainPlayer.transform.rotation = new Quaternion(ROT.x, ROT.y, ROT.z, ROT.w);

                    //hands entity data
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
            if (!GameStateManager.Instance.isAssetImportFinished)
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

            UIManager.Instance.clientTagSetup.DeleteTextFromString(clientIDToName[clientID]);

            availableClientIDToAvatar_Dict[clientID].transform.parent.gameObject.SetActive(false);
            //   _availableClientIDToGODict.Remove(clientID);

            client_ID_List.Remove(clientID);

        }

    }
    #endregion

    #region Create A Network Managed Objects
    /// <summary>
    /// Allows ClientSpawnManager have reference to the network reference gameobject to update with calls
    /// </summary>
    /// <param name="nGO"></param>
    /// <param name="asseListIndex"> This is the url index in list</param>
    /// <param name="customEntityID"></param>
    public NetworkAssociatedGameObject CreateNetworkAssociatedGameObject(GameObject nGO, int asseListIndex = -1, int customEntityID = 0, bool doNotLinkWithButtonID = false)
    {
        //add a Net component to the object
        NetworkAssociatedGameObject tempNet = nGO.AddComponent<NetworkAssociatedGameObject>();

        //to look a decomposed set of objects we need to keep track of what Index we are iterating over regarding or importing assets to create sets
        //we keep a list reference for each index and keepon adding to it if we find an asset with the same id
        //make sure we are using it as a button reference
        if (!doNotLinkWithButtonID)
        {
            if (asseListIndex != -1)
            {
                if (!decomposedAssetReferences_Dict.ContainsKey(asseListIndex))
                {
                    List<NetworkAssociatedGameObject> newNetLst = new List<NetworkAssociatedGameObject>();
                    newNetLst.Add(tempNet);
                    decomposedAssetReferences_Dict.Add(asseListIndex, newNetLst);


                }
                else
                {
                    List<NetworkAssociatedGameObject> netList = decomposedAssetReferences_Dict[asseListIndex];
                    netList.Add(tempNet);
                    decomposedAssetReferences_Dict[asseListIndex] = netList;
                }

                //to enable only imported objects to be grabbed, need to change for drawings
                tempNet.tag = "Interactable";
            }
        }

        //We then setup the data to be used through networking
        if (customEntityID != 0)
            tempNet.Instantiate(asseListIndex, customEntityID);
        else
            tempNet.Instantiate(asseListIndex);

        return tempNet;
    }

    /// <summary>
    /// Setup References For NetObjects 
    /// </summary>
    /// <param name="entityID"></param>
    /// <param name="nAGO"></param>
    /// <returns></returns>
    public void RegisterNetWorkAssociatedGameObject(int entityID, NetworkAssociatedGameObject nAGO)
    {
        entityID_To_NetObject_Dict.Add(entityID, nAGO);

    
            if (entityManager.HasComponent<ButtonIDSharedComponentData>(nAGO.Entity))
            {
                var buttonID = entityManager.GetSharedComponentData<ButtonIDSharedComponentData>(nAGO.Entity).buttonID;
                if (buttonID == rootLevelNetworkAssociatedGameObjectList.Count && -1 != buttonID)
                {
                    rootLevelNetworkAssociatedGameObjectList.Add(nAGO);
                }
            }
      
    }

    public void DeleteAndUnregisterNetworkAssociatedGameObject(int entityID)
    {
        if (entityID_To_NetObject_Dict.ContainsKey(entityID))
        {
            entityManager.DestroyEntity(entityID_To_NetObject_Dict[entityID].Entity);
            Destroy(Instance.entityID_To_NetObject_Dict[entityID].gameObject);
            Instance.entityID_To_NetObject_Dict.Remove(entityID);
        }
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
            if (!GameStateManager.Instance.isAssetImportFinished)
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
                    var headTransform = availableClientIDToAvatar_Dict[newData.clientId].avatarComponent_Head.transform;
                    var lHandTransform = availableClientIDToAvatar_Dict[newData.clientId].avatarComponent_hand_L.transform;
                    var rHandTransform = availableClientIDToAvatar_Dict[newData.clientId].avatarComponent_hand_R.transform;

                    headTransform.position = newData.pos;
                    headTransform.rotation = new Quaternion(newData.rot.x, newData.rot.y, newData.rot.z, newData.rot.w);

                    //to use for scalling avatars completely
                    //head has 3.5 scalling so we have to represent it instead of Vector3.one
                    //headTransform.SetGlobalScale((Vector3.one * 3.5f) * newData.scaleFactor);
                    //lHandTransform.SetGlobalScale(Vector3.one * newData.scaleFactor);
                    //rHandTransform.SetGlobalScale(Vector3.one * newData.scaleFactor);
                }
                else
                    Debug.LogWarning("Client ID : " + newData.clientId + " not found in Dictionary dropping head movement packet");
                break;

            //HANDL MOVE
            case (int)Entity_Type.users_Lhand:

                if (availableClientIDToAvatar_Dict.ContainsKey(newData.clientId))
                {
                    Transform handTransRef = availableClientIDToAvatar_Dict[newData.clientId].avatarComponent_hand_L.transform;

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
                    Transform handTransRef = availableClientIDToAvatar_Dict[newData.clientId].avatarComponent_hand_R.transform;

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

                    if (entityManager.HasComponent<TransformLockTag>(entityID_To_NetObject_Dict[newData.entityId].Entity))
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
            if (!UIManager.Instance.IsReady())
                return;


        switch (newData.interactionType)
        {

            case (int)INTERACTIONS.RENDERING:

               UIManager.Instance.Simulate_On_Button_RenderAsset(newData.targetEntity_id, false);

                break;

            case (int)INTERACTIONS.NOT_RENDERING:

                UIManager.Instance.Simulate_On_Button_RenderAsset(newData.targetEntity_id, true);

                break;

            case (int)INTERACTIONS.GRAB:

                entityManager.AddComponentData(entityID_To_NetObject_Dict[newData.targetEntity_id].Entity, new TransformLockTag { });


                break;

            case (int)INTERACTIONS.DROP:


                if (entityManager.HasComponent<TransformLockTag>(entityID_To_NetObject_Dict[newData.targetEntity_id].Entity))
                    entityManager.RemoveComponent<TransformLockTag>(entityID_To_NetObject_Dict[newData.targetEntity_id].Entity);
                else
                    Debug.LogWarning("Client Entity does not exist for Drop interaction--- EntityID" + newData.targetEntity_id);



                break;

            case (int)INTERACTIONS.CHANGE_SCENE:

                //check the loading wait for changing into a new scene - to avoid loading multiple scenes
                SceneManagerExtensions.Instance.SimulateSelectingSceneReference(newData.targetEntity_id);

                break;

            case (int)INTERACTIONS.LOCK:

                if (!entityID_To_NetObject_Dict.ContainsKey(newData.targetEntity_id))
                    return;

                var buttID = -1;
                  //  var buttonID = entityManager.GetSharedComponentData<ButtonIDSharedComponentData>(nRGentityID_To_NetObject_Dict[newData.targetEntity_id].Entity).buttonID;

                if (entityManager.HasComponent<ButtonIDSharedComponentData>(entityID_To_NetObject_Dict[newData.targetEntity_id].Entity))
                   buttID = entityManager.GetSharedComponentData<ButtonIDSharedComponentData>(entityID_To_NetObject_Dict[newData.targetEntity_id].Entity).buttonID;//entityID_To_NetObject_Dict[newData.targetEntity_id].buttonID;
                //if button does not have a button id assign to it return;
                if (buttID == -1) return;

                //disable button interaction for others
                UIManager.Instance.SimulateLockToggleButtonPress(buttID, true, false);

                break;

            case (int)INTERACTIONS.UNLOCK:

                if (!entityID_To_NetObject_Dict.ContainsKey(newData.targetEntity_id))
                    return;

                buttID = -1;

                if (entityManager.HasComponent<ButtonIDSharedComponentData>(entityID_To_NetObject_Dict[newData.targetEntity_id].Entity))
                    buttID = entityManager.GetSharedComponentData<ButtonIDSharedComponentData>(entityID_To_NetObject_Dict[newData.targetEntity_id].Entity).buttonID;//entityID_To_NetObject_Dict[newData.targetEntity_id].buttonID;
                //if button does not have a button id assign to it return;
                if (buttID == -1) return;

                UIManager.Instance.SimulateLockToggleButtonPress(buttID, false, false);

                break;

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
                currentLineRenderer.widthMultiplier = newData.lineWidth;

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
            
                //pivot.tag = "Drawing";

                NetworkAssociatedGameObject nAGO = default;

                if (!entityID_To_NetObject_Dict.ContainsKey(newData.strokeId))
                  nAGO =  CreateNetworkAssociatedGameObject(pivot, newData.strokeId, newData.strokeId, true);
               
                nAGO.tag = "Drawing";

                //tag created drawing object
                entityManager.AddComponentData(nAGO.Entity, new DrawingTag { });
               // entityManager.AddComponentData(nAGO.Entity, new NetworkEntityIdentificationComponentData { clientID = newData.clientId, entityID = newData.strokeId, sessionID = NetworkUpdateHandler.Instance.session_id, current_Entity_Type = Entity_Type.none });

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
        if (GameStateManager.IsAlive)
            if (!GameStateManager.Instance.isAssetImportFinished)
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

    List<AvatarEntityGroup> listOfAvatarsAvailable = new List<AvatarEntityGroup>();

    #region Setup Client Spots
    public IEnumerator Instantiate_Reserved_Clients()
    {

        float degrees = 360f / clientReserveCount;
        Vector3 offset = new Vector3(0, 0, 4);

        //set it under our instantiation manager
        var clientCollection = new GameObject("Client Collection").transform;
        //do not parent to instance manager because avatars are scaled when modifyfing world scale
      //  clientCollection.parent = this.transform;

        GameObject instantiation = default;

        EntityCommandBuffer ecb = entityManager.World.GetOrCreateSystem<EntityCommandBufferSystem>().CreateCommandBuffer();

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
            listOfAvatarsAvailable.Add(non_mainClientData);

            non_mainClientData.rootEntity = entityManager.CreateEntity();
#if UNITY_EDITOR
            entityManager.SetName(non_mainClientData.rootEntity, $"Client_{i + 1}");
#endif
            var buff = entityManager.AddBuffer<LinkedEntityGroup>(non_mainClientData.rootEntity);
            ecb.AppendToBuffer<LinkedEntityGroup>(non_mainClientData.rootEntity, non_mainClientData.rootEntity);


            non_mainClientData.entityHead = entityManager.CreateEntity();
            entityManager.AddComponentData(non_mainClientData.entityHead, new Parent { Value = non_mainClientData.rootEntity });
            ecb.AppendToBuffer<LinkedEntityGroup>(non_mainClientData.rootEntity, non_mainClientData.entityHead); //buff.Add(non_mainClientData.entityHead);

            non_mainClientData.entityHand_L = entityManager.CreateEntity();
            entityManager.AddComponentData(non_mainClientData.entityHand_L, new Parent { Value = non_mainClientData.rootEntity });
            ecb.AppendToBuffer<LinkedEntityGroup>(non_mainClientData.rootEntity, non_mainClientData.entityHand_L);


            non_mainClientData.entityHand_R = entityManager.CreateEntity();
            entityManager.AddComponentData(non_mainClientData.entityHand_R, new Parent { Value = non_mainClientData.rootEntity });
            ecb.AppendToBuffer<LinkedEntityGroup>(non_mainClientData.rootEntity, non_mainClientData.entityHand_R);

            instantiation.transform.position = TransformRelative;

            //Same orientation
            TransformRelative.y = centerAvatarSpawnLocation.y;

            Quaternion newRot = Quaternion.LookRotation(centerAvatarSpawnLocation - TransformRelative, Vector3.up);

            entityManager.AddComponentData(non_mainClientData.rootEntity, new Rotation { Value = new float4(newRot.x, newRot.y, newRot.z, newRot.w) });
       //     clientData_Main.rot = new Vector4(newRot.x, newRot.y, newRot.z, newRot.w);

            non_mainClientData.transform.parent.localRotation = new Quaternion(newRot.x, newRot.y, newRot.z, newRot.w);

       

            availableGO_List.Add(instantiation);

            instantiation.SetActive(false);

            instantiation.transform.SetParent(clientCollection);


           // ecb.ShouldPlayback = true;
        }
        ecb.Playback(entityManager);
        ecb.ShouldPlayback = false;

        //set all entities off corresponding to avatar
        foreach (var item in listOfAvatarsAvailable)
            entityManager.SetEnabled(item.rootEntity, false);

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
        SceneManagerExtensions.Instance.SimulateSelectingSceneReference(currentSessionState.scene);

        //add clients
        foreach (var clientID in currentSessionState.clients)
        {
            if (clientID != NetworkUpdateHandler.Instance.client_id)
                AddNewClient(clientID);
        }


        foreach (var downloadedGO_NetReg in rootLevelNetworkAssociatedGameObjectList)
        {
            var isAssetOn = false;
            var isLockOn = false;

            int entityIDToCheckFor = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(downloadedGO_NetReg.Entity).entityID;

            foreach (var entity in currentSessionState.entities)
            {
                if (entity.id == entityIDToCheckFor)
                {
                    if (entity.render)
                        isAssetOn = true;

                    if (entity.locked)
                        isLockOn = true;

                    break;
                }
            }

            if (isAssetOn)
                UIManager.Instance.Simulate_On_Button_RenderAsset(entityIDToCheckFor, false);
            else
                UIManager.Instance.Simulate_On_Button_RenderAsset(entityIDToCheckFor, true);

            if (isLockOn)
                Interaction_Refresh(new Interaction(sourceEntity_id: -1, targetEntity_id: entityIDToCheckFor, interactionType: (int)INTERACTIONS.LOCK));
            else
                Interaction_Refresh(new Interaction(sourceEntity_id: -1, targetEntity_id: entityIDToCheckFor, interactionType: (int)INTERACTIONS.UNLOCK));

        }

        foreach (var entity in currentSessionState.entities)
        {
            if (entity.latest.Length > 1)
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
            if (UIManager.Instance.IsReady())
                Refresh_CurrentState();

    }


#endregion
}
