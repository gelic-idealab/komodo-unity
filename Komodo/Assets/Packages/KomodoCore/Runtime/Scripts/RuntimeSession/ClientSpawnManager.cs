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

//#define TESTING_BEFORE_BUILDING

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
using Komodo.Utilities;
//using Komodo.AssetImport;

namespace Komodo.Runtime
{
    //UnityEvent_Extemsions to send client information to funcions specified in editor
    [System.Serializable] public class UnityEvent_Int : UnityEvent<int> { }
    [System.Serializable] public class UnityEvent_String : UnityEvent<string> { }

    //For handling different type of text between clients
    public enum STRINGTYPE
    {
        TUTORIAL,
        CLIENT_NAME,
        SPEECH_TO_TEXT,
    }
    public struct SpeechToTextSnippet
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
        physicsObject = 4,
        main_Player = 5,
        physicsEnd = 8,
        Line = 10,
        LineEnd = 11,
        LineDelete = 12,
        LineRender = 13,
        LineNotRender = 14,
    }

    #region INTERACTION TYPES
    public enum INTERACTIONS
    {
        LOOK = 0,
        LOOK_END = 1,
        SHOW = 2,
        HIDE = 3,
        GRAB = 4,
        DROP = 5,
        CHANGE_SCENE = 6,
        SLICE_OBJECT = 7,
        LOCK = 8,
        UNLOCK = 9,
        LINE = 10,
        LINE_END = 11,
        SHOW_MENU = 12,
        HIDE_MENU = 13,

        SETTING_TAB = 14,
        PEOPLE_TAB = 15,
        INTERACTION_TAB = 16,
        CREATE_TAB = 17,
    }
    #endregion
    /// <summary>
    /// This class is meant to:
    /// --- set up main player
    /// --- add/remove users
    /// --- maintain references to all network elements in scene
    /// --- provides funcions to attach to make connection between button and imported models (SetUp_ButtonURL.cs)
    /// --- provides funcions to call to update elements (NetworkUpdateHandler.cs)
    /// </summary>
    public class ClientSpawnManager : SingletonComponent<ClientSpawnManager>
    {
        public static ClientSpawnManager Instance
        {
            get { return (ClientSpawnManager) _Instance; }
            set { _Instance = value; }
        }

        [Header("Current Avatar Reference Setup")]
        public AvatarEntityGroup mainPlayer_AvatarEntityGroup;

        [Header("UI Client Tag ")]
        private GameObject mainPlayer;
        private GameObject handsParent;

        [Header("Spawn_Setup")]
        public GameObject clientPrefab;
        public Vector3 centerAvatarSpawnLocation;
        public int clientReserveCount;

        private int nextAvailableSlot = 0;
        public float spreadRadius;

        //References for displaying user name tags and speechtotext text
        private List<Text> clientUsernameDisplays = new List<Text>();
        private List<Text> clientSpeechToTextDisplays = new List<Text>();

        #region Lists And Dictionaries to store references in scene
        private List<int> clientIDs = new List<int>();
        [HideInInspector] public List<GameObject> gameObjects = new List<GameObject>();

        public Dictionary<int, AvatarEntityGroup> avatarEntityGroupFromClientId = new Dictionary<int, AvatarEntityGroup>();

        //To have a reference to client Avatar, name and hand animator components 
        private Dictionary<int, int> avatarIndexFromClientId = new Dictionary<int, int>();
        private Dictionary<int, string> usernameFromClientId = new Dictionary<int, string>();
        public Dictionary<int, Animator> animatorFromClientId = new Dictionary<int, Animator>();

        private string mainClientName = "Unset Name";
        #endregion


        public int maxWordsPerBubble = 20;
        public float secondsPerWord = 0.9f;

        [Header("Attach Funcions to Call Depending on Who the User is")]
        public UnityEvent onClient_IsTeacher;
        public UnityEvent onClient_IsStudent;

        [Header("String References")]
        private List<int> currentTextProcessingList = new List<int>();
        private List<string> currentTextProcessingList_Strings = new List<string>();
        public Dictionary<int, float> secondsToWaitDic = new Dictionary<int, float>();

        #region ECS Funcionality Fields 
        EntityManager entityManager;
        #endregion

        public void Awake() 
        {
            //used to set our manager's alive state to true to detect if it exists within scene
            var initManager = Instance;

            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        #region Initiation process --> ClientAvatars --> URL Downloads --> UI Setup --> SyncState
        public IEnumerator Start()
        {
#if TESTING_BEFORE_BUILDING
            Debug.LogWarning("Directive TESTING_BEFORE_BUILDING was enabled. Please disable it before production.");
#endif
            //WebGLMemoryStats.LogMoreStats("ClientSpawnManager Start BEFORE");
            mainPlayer = GameObject.FindWithTag(TagList.player);

            if (!mainPlayer)
            {
                Debug.LogError($"Could not find object with tag {TagList.player}. ClientSpawnManager.cs");
            }

            handsParent = GameObject.FindWithTag(TagList.hands);

            if (!handsParent)
            {
                Debug.LogError($"Could not find object with tag {TagList.hands}. ClientSpawnManager.cs");
            }

            //wait until our avatars are setup in the scene
            yield return StartCoroutine(InstantiateReservedClients());

            GameStateManager.Instance.isAvatarLoadingFinished = true;

            AddOwnClient();

            if (UIManager.IsAlive)
            {
                yield return new WaitUntil(() => UIManager.Instance.IsReady()); //TODO(Brandon): prevent a failed menu from stopping the whole client
            }

            //yield return new WaitUntil(() => SessionStateManager.Instance.IsReady()); TODO(Brandon): fully remove this line if it's working

            if (NetworkUpdateHandler.Instance.isTeacher != 0)
            {
                onClient_IsTeacher.Invoke();
            }
            else
            {
                onClient_IsStudent.Invoke();
            }

            //SessionStateManager.Instance.ApplyCatchup(); TODO(Brandon) -- evaluate if we need to create an empty state object.

            SocketIOAdapter.Instance.OpenConnectionAndJoin();

            //WebGLMemoryStats.LogMoreStats("ClientSpawnManager Start AFTER");
        }

        public int GetAvatarIndex(int clientId)
        {
            int result;

            bool success = avatarIndexFromClientId.TryGetValue(clientId, out result);

            if (!success)
            {
                throw new System.Exception($"Value was not found in client's avatar indices dictionary for key {clientId}.");
            }

            return result;
        }

        public AvatarEntityGroup GetAvatarEntityGroup(int clientId)
        {
            AvatarEntityGroup result;

            bool success = avatarEntityGroupFromClientId.TryGetValue(clientId, out result);

            if (!success)
            {
                throw new System.Exception($"Value was not found in client's avatar entity groups dictionary for key {clientId}.");
            }

            return result;
        }

        public string GetUsername(int clientId)
        {
            string result;

            bool success = usernameFromClientId.TryGetValue(clientId, out result);

            if (!success)
            {
                throw new System.Exception($"Value was not found in client's usernames dictionary for key {clientId}.");
            }

            return result;
        }

        public Animator GetAnimator(int clientId)
        {
            Animator result;

            bool success = animatorFromClientId.TryGetValue(clientId, out result);

            if (!success)
            {
                throw new System.Exception($"Value was not found in client's animators dictionary for key {clientId}.");
            }

            return result;
        }

        #endregion

        #region Add and Remove Client Funcions

        //to indicate where our client should be placed considering early initiation calls
        private int mainClientIndex = -1;

        public void AddNewClients(int[] clientIDs)
        {
            foreach (var clientID in clientIDs)
            {
                if (clientID != NetworkUpdateHandler.Instance.client_id)
                {
                    AddNewClient(clientID);
                }
            }
        }

        public void AddOwnClient()
        {
            int clientID = NetworkUpdateHandler.Instance.client_id;

            if (GameStateManager.IsAlive && !GameStateManager.Instance.isAvatarLoadingFinished)
            {
                return;
            }

            //setup newclient
            if (clientIDs.Contains(clientID))
            {
                return;
            }

            clientIDs.Add(clientID);

            mainClientName = NetworkUpdateHandler.Instance.GetPlayerNameFromClientID(clientID);

            gameObjects[nextAvailableSlot].SetActive(false);

            InitializeAvatar(clientID);

            mainClientIndex = nextAvailableSlot;

            var temp = avatarEntityGroupFromClientId[clientID].transform;

            var ROT = entityManager.GetComponentData<Rotation>(avatarEntityGroupFromClientId[clientID].rootEntity).Value.value;//.entity_data.rot;

            //To prevent offset issues when working with editor
#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING

                mainPlayer.transform.position = temp.position;

                mainPlayer.transform.rotation = new Quaternion(ROT.x, ROT.y, ROT.z, ROT.w);

                handsParent.transform.position = temp.position;

                handsParent.transform.rotation = new Quaternion(ROT.x, ROT.y, ROT.z, ROT.w);
#endif
            //Turn Off Dummy 
            var parObject = temp.parent.parent.gameObject;

            parObject.name = "Main_Client";

            nextAvailableSlot += 1;
        }

        public void DisplayOwnClientIsConnected ()
        {
            DisplayClientIsConnected(mainClientName, NetworkUpdateHandler.Instance.client_id);
        }

        public void DisplayClientIsConnected (string name, int clientID)
        {
            if (!UIManager.IsAlive)
            {
                Debug.LogWarning("AddNewClient2: UIManager.IsAlive was false");
            }
            else
            {
                UIManager.Instance.clientTagSetup.CreateTextFromString(name, clientID); // TODO(Brandon): rename to CreateClientConnectedIndicator
            }
        }

        public void InitializeAvatar(int clientID)
        {
            AvatarEntityGroup avatarEntityGroup = gameObjects[nextAvailableSlot].GetComponentInChildren<AvatarEntityGroup>();
            avatarEntityGroup.clientID = clientID;

            if (!avatarEntityGroupFromClientId.ContainsKey(clientID))
            {
                avatarEntityGroupFromClientId.Add(clientID, avatarEntityGroup);
            }
            else
            {
                avatarEntityGroupFromClientId[clientID] = avatarEntityGroup;
            }

            if (!avatarIndexFromClientId.ContainsKey(clientID))
            {
                avatarIndexFromClientId.Add(clientID, nextAvailableSlot);
            }
            else
            {
                avatarIndexFromClientId[clientID] = nextAvailableSlot;
            }
        }

        public void AddNewClient2(int clientID)
        {
            if (GameStateManager.IsAlive && !GameStateManager.Instance.isAvatarLoadingFinished)
            {
                return;
            }

            //setup newclient
            if (clientIDs.Contains(clientID))
            {
                return;
            }

            clientIDs.Add(clientID);

            string nameLabel = NetworkUpdateHandler.Instance.GetPlayerNameFromClientID(clientID);

            // Skip avatars that are already on, and your own client's spot. We need this loop because 
            // if someone in the middle of the list disconnected, we should reuse their avatar index.
            for (int i = 0; i < clientReserveCount; i += 1)
            {
                if (!gameObjects[i].activeInHierarchy && mainClientIndex != i)
                {
                    nextAvailableSlot = i;

                    break;
                }
            }

            InitializeAvatar(clientID);

            if (!usernameFromClientId.ContainsKey(clientID))
            {
                usernameFromClientId.Add(clientID, nameLabel);
            }
            else
            {
                usernameFromClientId[clientID] = nameLabel;
            }

            DisplayClientIsConnected(nameLabel, clientID);

            gameObjects[nextAvailableSlot].SetActive(true);

            //setAnimatorReferences what I use to reference other peoples hands
            int idForLeftHand = (clientID * 100) + (2);
            int idForRightHand = (clientID * 100) + (3);

            if (!animatorFromClientId.ContainsKey(idForLeftHand))
            {
                animatorFromClientId.Add(idForLeftHand, avatarEntityGroupFromClientId[clientID].avatarComponent_hand_L.GetComponent<Animator>());
                animatorFromClientId.Add(idForRightHand, avatarEntityGroupFromClientId[clientID].avatarComponent_hand_R.GetComponent<Animator>());
            }
            else
            {
                animatorFromClientId[idForLeftHand] = avatarEntityGroupFromClientId[clientID].avatarComponent_hand_L.GetComponent<Animator>();
                animatorFromClientId[idForRightHand] = avatarEntityGroupFromClientId[clientID].avatarComponent_hand_R.GetComponent<Animator>();
            }

            //set text label
            SpeechToTextSnippet newText = new SpeechToTextSnippet
            {
                stringType = (int)STRINGTYPE.CLIENT_NAME,
                target = clientID,//clientIDToAvatarIndex[clientID],
                text = nameLabel,
            };

            ProcessSpeechToTextSnippet(newText);

            nextAvailableSlot += 1;
        }

        public void AddNewClient(int clientID, bool isMainPlayer = false)
        {
            if (clientID == NetworkUpdateHandler.Instance.client_id && !isMainPlayer)
            {
                Debug.LogError($"AddNewClient was requested for own client ID ({clientID}) when isMainPlayer was false. Skipping.");

                return;
            }

            if (GameStateManager.IsAlive && !GameStateManager.Instance.isAvatarLoadingFinished)
            {
                return;
            }

            //setup newclient
            if (clientIDs.Contains(clientID))
            {
                return;
            }

            clientIDs.Add(clientID);

            string nameLabel = NetworkUpdateHandler.Instance.GetPlayerNameFromClientID(clientID);

            if (clientIDs.Count >= clientReserveCount)
            {
                Debug.LogWarning("REACHED MAX CLIENTS IN GAME - ADD TO CLIENT RESERVE COUNT FOR MORE USERS");

                return;
            }

            //go through all available slots
            for (int i = 0; i < clientReserveCount - 1; i++)
            {
                //skip avatars that are already on, and your reserved spot
                if (gameObjects[i].activeInHierarchy || mainClientIndex == i)
                {
                    continue;
                }

                AvatarEntityGroup avatarEntityGroup = gameObjects[i].GetComponentInChildren<AvatarEntityGroup>();
                avatarEntityGroup.clientID = clientID;

                // entityManager.AddComponentData(avatarEntityGroup.rootEntity, new NetworkEntityIdentificationComponentData {clientID = clientID });

                if (!avatarEntityGroupFromClientId.ContainsKey(clientID))
                {
                    avatarEntityGroupFromClientId.Add(clientID, avatarEntityGroup);
                }
                else
                {
                    avatarEntityGroupFromClientId[clientID] = avatarEntityGroup;
                }

                if (!avatarIndexFromClientId.ContainsKey(clientID))
                {
                    avatarIndexFromClientId.Add(clientID, i);
                }
                else
                {
                    avatarIndexFromClientId[clientID] = i;
                }

                if (!usernameFromClientId.ContainsKey(clientID))
                {
                    usernameFromClientId.Add(clientID, nameLabel);
                }
                else
                {
                    usernameFromClientId[clientID] = nameLabel;
                }

                //create main ui tag
                if (!UIManager.IsAlive)
                {
                    Debug.LogWarning("AddNewClient: UIManager.IsAlive was false");
                }
                else
                {
                    UIManager.Instance.clientTagSetup.CreateTextFromString(nameLabel, clientID);
                }

                //select how to handle avatars
                if (isMainPlayer)
                {
                    gameObjects[i].SetActive(false);

                    mainClientIndex = i;

                    var temp = avatarEntityGroupFromClientId[clientID].transform;

                    var ROT = entityManager.GetComponentData<Rotation>(avatarEntityGroupFromClientId[clientID].rootEntity).Value.value;//.entity_data.rot;

                    //To prevent offset issues when working with editor
#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING

                        mainPlayer.transform.position = temp.position;

                        mainPlayer.transform.rotation = new Quaternion(ROT.x, ROT.y, ROT.z, ROT.w);

                        handsParent.transform.position = temp.position;

                        handsParent.transform.rotation = new Quaternion(ROT.x, ROT.y, ROT.z, ROT.w);
#endif
                    //Turn Off Dummy 
                    var parObject = temp.parent.parent.gameObject;

                    parObject.name = "Main_Client";
                }
                else
                {
                    gameObjects[i].SetActive(true);

                    //setAnimatorReferences what I use to reference other peoples hands
                    int idForLeftHand = (clientID * 100) + (2);
                    int idForRightHand = (clientID * 100) + (3);

                    if (!animatorFromClientId.ContainsKey(idForLeftHand))
                    {
                        animatorFromClientId.Add(idForLeftHand, avatarEntityGroupFromClientId[clientID].avatarComponent_hand_L.GetComponent<Animator>());
                        animatorFromClientId.Add(idForRightHand, avatarEntityGroupFromClientId[clientID].avatarComponent_hand_R.GetComponent<Animator>());
                    }
                    else
                    {
                        animatorFromClientId[idForLeftHand] = avatarEntityGroupFromClientId[clientID].avatarComponent_hand_L.GetComponent<Animator>();
                        animatorFromClientId[idForRightHand] = avatarEntityGroupFromClientId[clientID].avatarComponent_hand_R.GetComponent<Animator>();
                    }

                    //set text label
                    SpeechToTextSnippet newText = new SpeechToTextSnippet
                    {
                        stringType = (int)STRINGTYPE.CLIENT_NAME,
                        target = clientID,//clientIDToAvatarIndex[clientID],
                        text = nameLabel,
                    };

                    ProcessSpeechToTextSnippet(newText);
                }
                break;
            }
        }
        public void RemoveClient (int clientID)
        {
            if (!GameStateManager.IsAlive)
            {
                Debug.LogWarning("Tried to remove client, but there was no GameStateManager.");

                return;
            }

            if (!GameStateManager.Instance.isAssetImportFinished)
            {
                Debug.LogWarning("Tried to remove client, but asset import was not finished.");

                return;
            }

            DestroyClient2(clientID);
        }

        public void DisplayOwnClientIsDisconnected ()
        {
            DisplayClientIsDisconnected(NetworkUpdateHandler.Instance.client_id);
        }

        public void DisplayClientIsDisconnected (int clientID)
        {
            if (UIManager.IsAlive)
            {
                UIManager.Instance.clientTagSetup.DeleteTextFromString(usernameFromClientId[clientID]);
            }
            else
            {
                Debug.LogWarning($"Couldn't remove client username {clientID} because UIManager didn't exist. Continuing.");
            }
        }

        public void DestroyClient2 (int clientID)
        {
            if (!clientIDs.Contains(clientID))
            {
                Debug.LogWarning($"Couldn't destroy client {clientID} because it didn't exist. Continuing.");

                if (!avatarEntityGroupFromClientId.ContainsKey(clientID))
                {
                    Debug.LogError($"Couldn't destroy client {clientID} because avatarEntityGroupFromClientId didn't contain an entry for it.");

                    return;
                }

                avatarEntityGroupFromClientId[clientID].transform.parent.gameObject.SetActive(false);

                return;
            }

            DisplayClientIsDisconnected(clientID);

            if (!avatarEntityGroupFromClientId.ContainsKey(clientID))
            {
                Debug.LogError($"Couldn't destroy client {clientID} because avatarEntityGroupFromClientId didn't contain an entry for it.");

                return;
            }

            avatarEntityGroupFromClientId[clientID].transform.parent.gameObject.SetActive(false);

            clientIDs.Remove(clientID);
        }

        public void RemoveAllClients ()
        {
            //Remove them in reverse order, because the collection will be modified.

            for (int i = clientIDs.Count - 1; i > 0; i -= 1)
            {
                int id = clientIDs[i];

                // Don't remove your own client.
                if (clientIDs[i] == NetworkUpdateHandler.Instance.client_id)
                {
                    i -= 1;

                    continue;
                }

                RemoveClient(id);
            }

            nextAvailableSlot = 1;
        }

        public async void DestroyClient(int clientID)
        {
            if (clientIDs.Contains(clientID))
            {
                //wait for setup to be complete for early calls
                while (!avatarEntityGroupFromClientId.ContainsKey(clientID))
                    await Task.Delay(1);

                if (UIManager.IsAlive)
                    UIManager.Instance.clientTagSetup.DeleteTextFromString(usernameFromClientId[clientID]);

                avatarEntityGroupFromClientId[clientID].transform.parent.gameObject.SetActive(false);
                //   _availableClientIDToGODict.Remove(clientID);

                clientIDs.Remove(clientID);

            }

        }
        #endregion

        #region Create A Network Managed Objects


        #endregion

        #region Draw Receive Calls
        //Setting up Line Rendering Calls
        private Dictionary<int, LineRenderer> lineRenderersInQueue = new Dictionary<int, LineRenderer>();
        private Dictionary<int, int> allStrokeIDValidator = new Dictionary<int, int>();

        //To avoid duplicating stroke ids because sending different ids states ma 
        public void Draw_Refresh(string stringData)//Draw newData)
        {
            Draw newData = JsonUtility.FromJson<Draw>(stringData);

                
            LineRenderer currentLineRenderer = default;

            //we start a new line if there is no ID already corresponding to one in the scene
            if (!allStrokeIDValidator.ContainsKey(newData.strokeId))
            {
                GameObject lineRendCopy = Instantiate(DrawingInstanceManager.Instance.lineRendererContainerPrefab).gameObject;
                lineRendCopy.name = "LineR:" + newData.strokeId;

                lineRendCopy.transform.SetParent(DrawingInstanceManager.Instance.externalStrokeParent, true);
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

                    //Create external client stroke instance
                    DrawingInstanceManager.Instance.CreateExternalClientStrokeInstance(newData.strokeId, currentLineRenderer); //new GameObject("LineRender:" + (newData.strokeId), typeof(BoxCollider//;

                    break;

                //Deletes a Line
                case (int)Entity_Type.LineDelete:

                    if (NetworkedObjectsManager.Instance.networkedObjectFromEntityId.ContainsKey(newData.strokeId))
                    {
                        if (lineRenderersInQueue.ContainsKey(newData.strokeId))
                            lineRenderersInQueue.Remove(newData.strokeId);

                        Destroy(NetworkedObjectsManager.Instance.networkedObjectFromEntityId[newData.strokeId].gameObject);
                        NetworkedObjectsManager.Instance.networkedObjectFromEntityId.Remove(newData.strokeId);
                    }
                    break;

                case (int)Entity_Type.LineRender:

                    if (NetworkedObjectsManager.Instance.networkedObjectFromEntityId.ContainsKey(newData.strokeId))
                        NetworkedObjectsManager.Instance.networkedObjectFromEntityId[newData.strokeId].gameObject.SetActive(true);

                    break;

                case (int)Entity_Type.LineNotRender:

                    if (NetworkedObjectsManager.Instance.networkedObjectFromEntityId.ContainsKey(newData.strokeId))
                        NetworkedObjectsManager.Instance.networkedObjectFromEntityId[newData.strokeId].gameObject.SetActive(false);

                    break;
            }
        }

        #endregion

        public void AddClientIfNeeded (int id)
        {
            if (NetworkUpdateHandler.Instance.client_id == id)
            {
                Debug.LogWarning($"AddClientIfNeeded: {id} is own client. Skipping.");

                return;
            }

            if (!clientIDs.Contains(id))
            {
                AddNewClient(id);

                Debug.LogWarning($"ClientSpawnManager: client with {id} not found. Creating.");
            }
        }

        // Returns true iff the entity type was valid and the position 
        // was applied to the head or hands.
        public bool TryToApplyPosition (Position positionData)
        {
            switch (positionData.entityType)
            {
                case (int) Entity_Type.users_head:

                    ApplyPositionToHead(positionData);

                    return true;

                case (int) Entity_Type.users_Lhand:

                    ApplyPositionToLeftHand(positionData);

                    return true;

                case (int) Entity_Type.users_Rhand:

                    ApplyPositionToRightHand(positionData);

                    return true;

                default:

                    return false;
            }
        }

        public void ApplyPositionToHead(Position positionData)
        {
            if (avatarEntityGroupFromClientId.ContainsKey(positionData.clientId))
            {
                var headTransform = avatarEntityGroupFromClientId[positionData.clientId].avatarComponent_Head.transform;
                var lHandTransform = avatarEntityGroupFromClientId[positionData.clientId].avatarComponent_hand_L.transform;
                var rHandTransform = avatarEntityGroupFromClientId[positionData.clientId].avatarComponent_hand_R.transform;

                headTransform.position = positionData.pos;
                headTransform.rotation = new Quaternion(positionData.rot.x, positionData.rot.y, positionData.rot.z, positionData.rot.w);

                //to use for scalling avatars completely
                //head has 3.5 scalling so we have to represent it instead of Vector3.one
                //headTransform.SetGlobalScale((Vector3.one * 3.5f) * newData.scaleFactor);
                //lHandTransform.SetGlobalScale(Vector3.one * newData.scaleFactor);
                //rHandTransform.SetGlobalScale(Vector3.one * newData.scaleFactor);
            }
            else
            {
                Debug.LogWarning("Client ID : " + positionData.clientId + " not found in Dictionary dropping head movement packet");
            }
        }

        public void ApplyPositionToLeftHand(Position positionData)
        {
            if (avatarEntityGroupFromClientId.ContainsKey(positionData.clientId))
            {
                Transform handTransRef = avatarEntityGroupFromClientId[positionData.clientId].avatarComponent_hand_L.transform;

                if (!handTransRef.gameObject.activeInHierarchy)
                {
                    handTransRef.gameObject.SetActive(true);
                }

                handTransRef.position = positionData.pos;

                handTransRef.rotation = new Quaternion(positionData.rot.x, positionData.rot.y, positionData.rot.z, positionData.rot.w);

                //To reference the correct hand animator
                int leftHandID = (positionData.clientId * 100) + (2);

                if (animatorFromClientId.ContainsKey(leftHandID))
                {
                    animatorFromClientId[leftHandID].Play("Take", -1, positionData.scaleFactor);
                }
            }
            else
            {
                Debug.LogWarning("Client ID : " + positionData.clientId + " not found in Dictionary dropping left hand movement packet");
            }
        }

        public void ApplyPositionToRightHand(Position positionData)
        {
            if (avatarEntityGroupFromClientId.ContainsKey(positionData.clientId))
            {
                Transform handTransRef = avatarEntityGroupFromClientId[positionData.clientId].avatarComponent_hand_R.transform;

                if (!handTransRef.gameObject.activeInHierarchy)
                {
                    handTransRef.gameObject.SetActive(true);
                }

                handTransRef.position = positionData.pos;

                handTransRef.rotation = new Quaternion(positionData.rot.x, positionData.rot.y, positionData.rot.z, positionData.rot.w);

                int rightHandID = (positionData.clientId * 100) + (3);

                if (animatorFromClientId.ContainsKey(rightHandID))
                {
                    animatorFromClientId[rightHandID].Play("Take", -1, positionData.scaleFactor);
                }
            }
            else
            {
                Debug.LogWarning("Client ID : " + positionData.clientId + " not found in Dictionary dropping right hand movement packet");
            }
        }

        #region Text Receive Calls

        public struct SpeechToText
        {
            public int session_id;
            public int client_id;
            public string text;
            public string type;
            public int ts;
        }

        public void OnReceiveSpeechToTextSnippet(string data)
        {
            var deserializedData = JsonUtility.FromJson<SpeechToText>(data);
            SpeechToTextSnippet snippet;
            snippet.target = deserializedData.client_id;
            snippet.text = deserializedData.text;
            snippet.stringType = (int)STRINGTYPE.SPEECH_TO_TEXT;
            ProcessSpeechToTextSnippet(snippet);
        }
        public void ProcessSpeechToTextSnippet(SpeechToTextSnippet newText)
        {
            if (GameStateManager.IsAlive)
                if (!GameStateManager.Instance.isAssetImportFinished)
                    return;

            if (!clientIDs.Contains(newText.target))
            {
                Debug.LogWarning("No client Found, Can't render text");
                return;
            }

            switch (newText.stringType)
            {
                case (int)STRINGTYPE.TUTORIAL:
                    break;

                case (int)STRINGTYPE.SPEECH_TO_TEXT:
                    //Get client index for text look up to use for displaying
                    var clientIndex = avatarIndexFromClientId[newText.target];
                    string foo = SplitWordsByLength(newText.text, maxWordsPerBubble);
                    StartCoroutine(SetTextTimer(clientIndex, foo, secondsPerWord * Mathf.Log(newText.text.Length)));
                    break;

                case (int)STRINGTYPE.CLIENT_NAME:

                    clientIndex = avatarIndexFromClientId[newText.target];
                    clientUsernameDisplays[clientIndex].text = newText.text;
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
                clientSpeechToTextDisplays[textIndex].text = textD;
                currentTextProcessingList.Add(textIndex);
                StartCoroutine(ShutOffText(textIndex, seconds));

            }
            else

            {
                currentTextProcessingList.Add(textIndex);
                yield return new WaitForSeconds(secondsToWaitDic[textIndex]);

                secondsToWaitDic[textIndex] -= seconds;

                StartCoroutine(ShutOffText(textIndex, seconds));
                clientSpeechToTextDisplays[textIndex].text = textD;

            }
            yield return null;

        }


        public IEnumerator ShutOffText(int textIndex, float seconds)
        {

            clientSpeechToTextDisplays[textIndex].transform.parent.gameObject.SetActive(true);

            //  secondsToWaitDic[index] -= seconds;
            yield return new WaitForSeconds(seconds);

            currentTextProcessingList.Remove(textIndex);

            if (!currentTextProcessingList.Contains(textIndex))
            {
                clientSpeechToTextDisplays[textIndex].transform.parent.gameObject.SetActive(false);
            }

        }
        #endregion

        List<AvatarEntityGroup> avatars = new List<AvatarEntityGroup>();

        #region Setup Client Spots
        public IEnumerator InstantiateReservedClients()
        {


            //set it under our instantiation manager
            var clientCollection = new GameObject("Client Collection").transform;

            GameObject avatar = default;

            EntityCommandBuffer ecb = entityManager.World.GetOrCreateSystem<EntityCommandBufferSystem>().CreateCommandBuffer();

            //Create all players with simple GameObject Representation
            for (int i = 0; i < clientReserveCount; i++)
            {
               CreateAvatar(avatar, i, ecb, clientCollection);
            }
            ecb.Playback(entityManager);
            ecb.ShouldPlayback = false;

            //set all entities off corresponding to avatar
            foreach (var item in avatars)
            {
                entityManager.SetEnabled(item.rootEntity, false);
            }

            yield return null;
        }

        public void CreateAvatar(GameObject avatar, int i, EntityCommandBuffer ecb, Transform clientCollection)
        {
            float degrees = 360f / clientReserveCount;
            Vector3 offset = new Vector3(0, 0, 4);

            Vector3 TransformRelative = Quaternion.Euler(0f, degrees * i + 1, 0f) * (transform.position + offset);

            avatar = Instantiate(clientPrefab, Vector3.zero, Quaternion.identity);

            avatar.name = $"Client {i + 1}";
            //Obtain each avatars UI_TEXT REFERENCE FROM THEIR CANVAS IN PREFAB
            //GET HEAD TO GET CANVAS FOR TEXT COMPONENTS
            Transform canvas = avatar.transform.GetChild(0).GetComponentInChildren<Canvas>().transform;

            //GET APPROPRIATE TEXT COMPONENT CHARACTER NAME AND SPEECH_TO_TEXT
            clientUsernameDisplays.Add(canvas.GetChild(0).GetComponent<Text>());
            canvas.GetChild(0).GetComponent<Text>().text = $"Client {i + 1}";

            clientSpeechToTextDisplays.Add(canvas.GetChild(1).GetChild(0).GetComponent<Text>());

            //Set up links for network call references
            var otherClientAvatars = avatar.GetComponentInChildren<AvatarEntityGroup>(true);
            avatars.Add(otherClientAvatars);

            //set up our entitiies to store data about components
            otherClientAvatars.rootEntity = entityManager.CreateEntity();
#if (UNITY_WEBGL && !UNITY_EDITOR) || TESTING_BEFORE_BUILDING
//do nothing
#else
            entityManager.SetName(otherClientAvatars.rootEntity, $"Client {i + 1}");
#endif
            var buff = entityManager.AddBuffer<LinkedEntityGroup>(otherClientAvatars.rootEntity);
            ecb.AppendToBuffer<LinkedEntityGroup>(otherClientAvatars.rootEntity, otherClientAvatars.rootEntity);

            otherClientAvatars.entityHead = entityManager.CreateEntity();
            entityManager.AddComponentData(otherClientAvatars.entityHead, new Parent { Value = otherClientAvatars.rootEntity });
            ecb.AppendToBuffer<LinkedEntityGroup>(otherClientAvatars.rootEntity, otherClientAvatars.entityHead); //buff.Add(non_mainClientData.entityHead);

            otherClientAvatars.entityHand_L = entityManager.CreateEntity();
            entityManager.AddComponentData(otherClientAvatars.entityHand_L, new Parent { Value = otherClientAvatars.rootEntity });
            ecb.AppendToBuffer<LinkedEntityGroup>(otherClientAvatars.rootEntity, otherClientAvatars.entityHand_L);

            otherClientAvatars.entityHand_R = entityManager.CreateEntity();
            entityManager.AddComponentData(otherClientAvatars.entityHand_R, new Parent { Value = otherClientAvatars.rootEntity });
            ecb.AppendToBuffer<LinkedEntityGroup>(otherClientAvatars.rootEntity, otherClientAvatars.entityHand_R);

            avatar.transform.position = TransformRelative;

            //Same orientation
            TransformRelative.y = centerAvatarSpawnLocation.y;

            Quaternion newRot = Quaternion.LookRotation(centerAvatarSpawnLocation - TransformRelative, Vector3.up);

            entityManager.AddComponentData(otherClientAvatars.rootEntity, new Rotation { Value = new float4(newRot.x, newRot.y, newRot.z, newRot.w) });

            otherClientAvatars.transform.parent.localRotation = new Quaternion(newRot.x, newRot.y, newRot.z, newRot.w);

            gameObjects.Add(avatar);

            avatar.SetActive(false);

            avatar.transform.SetParent(clientCollection);
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

        public void SendMenuInteractionsType(int interaction) {
            NetworkUpdateHandler.Instance.SendSyncInteractionMessage (new Interaction 
            {
                sourceEntity_id = NetworkUpdateHandler.Instance.client_id,

                interactionType = interaction,

                targetEntity_id = 0,
            });
        }

        #endregion
    }
}
