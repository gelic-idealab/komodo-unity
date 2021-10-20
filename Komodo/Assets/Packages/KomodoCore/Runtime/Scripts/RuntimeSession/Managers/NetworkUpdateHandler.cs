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

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using Unity.Entities;
using Komodo.AssetImport;
using Komodo.Utilities;

namespace Komodo.Runtime
{
    [System.Serializable]
    public class Int_UnityEvent : UnityEvent<int> { }

    //We use interfaces to centralize our update calls and optimize crossing between manage and native code see GameStateManager.cs
    public class NetworkUpdateHandler : SingletonComponent<NetworkUpdateHandler>, IUpdatable
    {
        public static NetworkUpdateHandler Instance
        {
            get { return ((NetworkUpdateHandler)_Instance); }
            set { _Instance = value; }
        }

#if UNITY_WEBGL && !UNITY_EDITOR 
        // don't declare a socket simulator for WebGL build
#else
        private SocketIOEditorSimulator SocketSim;
#endif

        // session id from JS
        [HideInInspector] public int session_id;
        [HideInInspector] public string sessionName;
        [HideInInspector] public string buildName;

        // client_id from JS
        [HideInInspector] public int client_id;

        // is the current client a teacher? from JS
        public int isTeacher;

        public ModelDataTemplate modelData;

        public bool useEditorModelsList = false;

        public Text socketIODisplay;

        // internal network update sequence counter
        private int seq = 0;

        // field to array index mapping
        const int SEQ = 0;
        const int SESSION_ID = 1;
        const int CLIENT_ID = 2;
        const int ENTITY_ID = 3;
        const int ENTITY_TYPE = 4;
        const int SCALE = 5;
        const int ROTX = 6;
        const int ROTY = 7;
        const int ROTZ = 8;
        const int ROTW = 9;
        const int POSX = 10;
        const int POSY = 11;
        const int POSZ = 12;
        const int DIRTY = 13;
        const int NUMBER_OF_POSITION_FIELDS = 14;

        float[] position_data = new float[NUMBER_OF_POSITION_FIELDS * 1024]; // 1024 slots to be checked per frame

        const int NUMBER_OF_INTERACTION_FIELDS = 7;
        int[] interaction_data = new int[NUMBER_OF_INTERACTION_FIELDS * 128]; // 128 slots

        const int NUMBER_OF_DRAW_FIELDS = 14;

        float[] draw_data = new float[NUMBER_OF_DRAW_FIELDS * 128]; // 128 slots

        public float[] SerializeCoordsStruct(Position coords)
        {
            float[] arr = new float[NUMBER_OF_POSITION_FIELDS];

            arr[SEQ] = (float)seq;
            arr[SESSION_ID] = (float)session_id;
            arr[CLIENT_ID] = (float)coords.clientId;
            arr[ENTITY_ID] = (float)coords.entityId;
            arr[ENTITY_TYPE] = (float)coords.entityType;
            arr[SCALE] = coords.scaleFactor;
            arr[ROTX] = coords.rot.x;
            arr[ROTY] = coords.rot.y;
            arr[ROTZ] = coords.rot.z;
            arr[ROTW] = coords.rot.w;
            arr[POSX] = coords.pos.x;
            arr[POSY] = coords.pos.y;
            arr[POSZ] = coords.pos.z;
            arr[DIRTY] = 1;

            return arr;
        }

        public Position DeSerializeCoordsStruct(float[] arr)
        {
            var pos = new Position(
                              (int)arr[CLIENT_ID],
                              (int)arr[ENTITY_ID],
                              (int)arr[ENTITY_TYPE],
                              arr[SCALE],
                              new Quaternion(arr[ROTX], arr[ROTY], arr[ROTZ], arr[ROTW]),
                              new Vector3(arr[POSX], arr[POSY], arr[POSZ])
                          );

            return pos;
        }

        private void _CreateSocketSimulator () 
        {
#if UNITY_WEBGL && !UNITY_EDITOR 
            //don't assign a SocketIO Simulator for WebGL build
#else
            SocketSim = SocketIOEditorSimulator.Instance;
            if (!SocketSim)
            {
                Debug.LogWarning("No SocketIOEditorSimulator was found in the scene. In-editor behavior may not be as expected.");
            }
#endif
        }

        private void _GetParams ()
        {
#if UNITY_WEBGL && !UNITY_EDITOR 
            client_id = SocketIOJSLib.GetClientIdFromBrowser();
            session_id = SocketIOJSLib.GetSessionIdFromBrowser();
            isTeacher  = SocketIOJSLib.GetIsTeacherFlagFromBrowser();
#else
            client_id = SocketSim.GetClientIdFromBrowser();
            session_id = SocketSim.GetSessionIdFromBrowser();
            isTeacher = SocketSim.GetIsTeacherFlagFromBrowser();
#endif
        }

        private void _GetModelsAndSessionDetails ()
        {
#if UNITY_WEBGL && !UNITY_EDITOR 
            string SessionDetailString;

            if (useEditorModelsList) 
            {
#if DEVELOPMENT_BUILD
                //in dev builds, don't clear models list
                Debug.LogWarning("Using editor's model list. You should turn off 'Use Editor Models List' off in NetworkManager.");
#else
                //in non-dev build, ignore the flag. 
                modelData.models.Clear();
#endif
                SessionDetailsString = SocketIOEditorSimulator.GetSessionDetails();

            }
            else 
            {
                modelData.models.Clear();

                SessionDetailsString = SocketIOJSLib.GetSessionDetails();
            }

            if (System.String.IsNullOrEmpty(SessionDetailsString)) 
            {
                Debug.Log("Error: Details are null or empty.");
            } 
            else 
            {
                Debug.Log("SessionDetails: " + SessionDetailsString);
                var details = JsonUtility.FromJson<SessionDetails>(SessionDetailsString);
                
                if (useEditorModelsList) 
                {
#if DEVELOPMENT_BUILD
                    //in dev builds, don't pass details to the models list if the flag is enabled.
                    Debug.LogWarning("Using editor's model list. You should turn off 'Use Editor Models List' off in NetworkManager.");
#else
                    //in non-dev build, ignore the flag. 
                    modelData.models = details.assets;
#endif
                }
                else 
                {
                    modelData.models = details.assets;
                }

                if (sessionName != null)
                {
                    sessionName = details.session_name;
                    buildName = details.build;
                }
                else
                {
                    Debug.LogError("SessionName Ref in NetworkUpdateHandler's Text Component is missing from editor");
                }
            }
#endif
        }

        public void Awake()
        {
            //used to set our managers alive state to true to detect if it exist within scene
            var initManager = Instance;

            //optimization - register our update calls
            // procesing all update loops from one main update loop is optimal to avoid  
            // crossing native to manage code 
            GameStateManager.Instance.RegisterUpdatableObject(this);

            //WebGLMemoryStats.LogMoreStats("NetworkUpdateHandler.Awake BEFORE");

            if (modelData == null) {
                Debug.LogWarning("No model data template was found for NetworkManager. Imported models may use editor template.");
            }

            if (socketIODisplay == null) {
                throw new System.Exception("You must assign a socketIODisplay in NetworkUpdateHandler.");
            }

            _CreateSocketSimulator();

            _GetParams();

            _GetModelsAndSessionDetails();

            //WebGLMemoryStats.LogMoreStats("NetworkUpdateHandler.Awake AFTER");
        }

        public void Start()
        {
            GlobalMessageManager.Instance.Subscribe("sync", (data) => _DeserializeAndProcessSyncData(data));
            GlobalMessageManager.Instance.Subscribe("interaction", (data) => _DeserializeAndProcessInteractionData(data));

            #region ECS Funcionality: Set up our User Data

            //WebGLMemoryStats.LogMoreStats("NetworkUpdateHandler Start BEFORE");

            //set up data for our player components
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            var eqDesc = new EntityQueryDesc
            { 
                All = new ComponentType[] 
                { 
                    typeof(OurPlayerTag), 
                    typeof(NetworkEntityIdentificationComponentData) 
                } 
            };

            var entities = entityManager.CreateEntityQuery(eqDesc).ToEntityArray(Unity.Collections.Allocator.Temp);

            foreach (var entity in entities)
            {
                var entityIDFromType = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(entity).current_Entity_Type;

                entityManager.SetComponentData(
                    entity, 
                    new NetworkEntityIdentificationComponentData { 
                        clientID = this.client_id, 
                        sessionID = this.session_id, 
                        entityID = (int)entityIDFromType, 
                        current_Entity_Type = entityIDFromType 
                    }
                );

                if (isTeacher != 0) 
                {
                    entityManager.AddComponent<TeacherTag>(entity);
                }
            }

            entities.Dispose();

            //WebGLMemoryStats.LogMoreStats("NetworkUpdateHandler Start AFTER");
            #endregion
        }

        //TODO(Brandon): Suggestion: rename this to PositionUpdate
        public void NetworkUpdate(Position pos) 
        {
            var posString = JsonUtility.ToJson(pos);
            var message = new KomodoMessage("sync", posString);
            message.Send();
            // float[] arr_pos = SerializeCoordsStruct(pos);

// #if UNITY_WEBGL && !UNITY_EDITOR 
//             SocketIOSendPosition(arr_pos, arr_pos.Length);
// #else
//             SocketSim.SocketIOSendPosition(arr_pos, arr_pos.Length);
// #endif
        }

        public void InteractionUpdate(Interaction interaction)
        {
            var intString = JsonUtility.ToJson(interaction);
            var message = new KomodoMessage("interaction", intString);
            message.Send();

            // NOTE(rob): DEPRECATED. 8/18/21. 
//             int[] arr_inter = new int[NUMBER_OF_INTERACTION_FIELDS];
//             arr_inter[0] = seq;
//             arr_inter[1] = session_id;
//             arr_inter[2] = (int)client_id;
//             arr_inter[3] = interact.sourceEntity_id;
//             arr_inter[4] = interact.targetEntity_id;
//             arr_inter[5] = (int)interact.interactionType;
//             arr_inter[6] = 1; // dirty bit

// #if UNITY_WEBGL && !UNITY_EDITOR 
//             SocketIOSendInteraction(arr_inter, arr_inter.Length);
// #else
//             SocketSim.SocketIOSendInteraction(arr_inter, arr_inter.Length);
// #endif
        }

// NOTE(rob): DEPRECATED. 8/18/21. 

//         public void DrawUpdate(Draw draw)
//         {
//             var arr_draw = new float[NUMBER_OF_DRAW_FIELDS];
//             arr_draw[0] = (float)seq;
//             arr_draw[1] = (float)session_id;
//             arr_draw[2] = (float)draw.clientId;
//             arr_draw[3] = (float)draw.strokeId;
//             arr_draw[4] = (float)draw.strokeType;
//             arr_draw[5] = draw.lineWidth;
//             arr_draw[6] = draw.curStrokePos.x;
//             arr_draw[7] = draw.curStrokePos.y;
//             arr_draw[8] = draw.curStrokePos.z;
//             arr_draw[9] = draw.curColor.x;
//             arr_draw[10] = draw.curColor.y;
//             arr_draw[11] = draw.curColor.z;
//             arr_draw[12] = draw.curColor.w;
//             arr_draw[13] = 1; // dirty bit

// #if UNITY_WEBGL && !UNITY_EDITOR 
//             SendDraw(arr_draw, arr_draw.Length);
// #else
//             SocketSim.SendDraw(arr_draw, arr_draw.Length);
// #endif
//         }

        private int _ClampFloatToInt32 (float value) 
        {
            float minInt = (float) Int32.MinValue;

            float maxInt = (float) Int32.MaxValue;

            return (int) Mathf.Clamp(value, minInt, maxInt);
        }

        private void _DeserializeAndProcessSyncData(string data)
        {
            var pos = JsonUtility.FromJson<Position>(data);

            // send new network data to client spawn manager
            if (ClientSpawnManager.IsAlive)
            { 
                ClientSpawnManager.Instance.ApplyPosition(pos);
            }
        }

        private void _DeserializeAndProcessInteractionData(string data)
        {
            var interaction = JsonUtility.FromJson<Interaction>(data);
            
            if (SessionStateManager.IsAlive)
            {
                SessionStateManager.Instance.ApplyInteraction(interaction);
            }
        }

        // NOTE(rob): DEPRECATED. 8/18/21. 
        // private void _CheckHeapForNewPositionData () 
        // {
        //     for (int i = 0; i < position_data.Length; i += NUMBER_OF_POSITION_FIELDS)
        //     {
        //         if (_ClampFloatToInt32(position_data[i + DIRTY]) != 0)
        //         {
        //             position_data[i + DIRTY] = 0; // reset the dirty bit
                    
        //             // unpack entity update into Position struct
        //             var pos = new Position
        //             (
        //                 _ClampFloatToInt32(position_data[i + CLIENT_ID]),

        //                 _ClampFloatToInt32(position_data[i + ENTITY_ID]),

        //                 _ClampFloatToInt32(position_data[i + ENTITY_TYPE]),

        //                 position_data[i + SCALE],

        //                 new Quaternion(
        //                     position_data[i + ROTX], 
        //                     position_data[i + ROTY], 
        //                     position_data[i + ROTZ], 
        //                     position_data[i + ROTW]
        //                 ),
                        
        //                 new Vector3(
        //                     position_data[i + POSX], 
        //                     position_data[i + POSY], 
        //                     position_data[i + POSZ]
        //                 )
        //             );

        //             // send new network data to client spawn manager
        //             if (ClientSpawnManager.IsAlive) 
        //             { 
        //                 ClientSpawnManager.Instance.Client_Refresh(pos);
        //             }
        //         }
        //     }
        // }

        // NOTE(rob): DEPRECATED. 8/18/21. 
        // private void _CheckHeapForNewInteractionData ()
        // {
        //     // checks interaction shared memory for new updates
        //     for (int i = 0; i < interaction_data.Length; i += NUMBER_OF_INTERACTION_FIELDS)
        //     {
        //         // check the dirty bit
        //         if (interaction_data[i + 6] != 0)
        //         { 
        //             // reset the dirty bit
        //             interaction_data[i + 6] = 0;

        //             var interaction = new Interaction
        //             (
        //                 interaction_data[i + 3],
        //                 interaction_data[i + 4],
        //                 interaction_data[i + 5]
        //             );

        //             // send new network data to client spawn manager
        //             if (ClientSpawnManager.IsAlive) 
        //             {
        //                 ClientSpawnManager.Instance.Interaction_Refresh(interaction);
        //             }
        //         }
        //     }
        // }

        private void _Tick ()
        {
            seq++; // local sequence counter
        }

        public void OnUpdate(float realTime)
        {
            // _CheckHeapForNewPositionData();

            // _CheckHeapForNewInteractionData();

            _Tick();
        }

        public string GetPlayerNameFromClientID(int clientID)
        {
#if UNITY_WEBGL && !UNITY_EDITOR 
            string SessionDetailsString = SocketIOJSLib.GetSessionDetails();
#else
            string SessionDetailsString = SocketIOEditorSimulator.GetSessionDetails();
#endif
            var Details = JsonUtility.FromJson<SessionDetails>(SessionDetailsString);

            foreach (User user in Details.users)
            {
                if (clientID != user.student_id)
                {
                    continue;
                }

                return user.first_name + "  " + user.last_name;
            }

            return "Client " + clientID;
        }

        // Use the inspector to call this method.
        [ContextMenu("TestProcessMessage")]
        public void TestProcessMessage()
        {
            ProcessMessage("greeting|{\"it's\":\"nice\",\"to\":\"meet\",\"you\":\"NOT!!\"}");
        }

        // TODO(rob): move this to GlobalMessageManager.cs
        public void ProcessMessage(string typeAndMessage)
        {
            //var message = JsonUtility.FromJson<KomodoMessge>(messageJSON);
            var splitTypeAndMessage = typeAndMessage.Split(new Char[] {'|'}, 2);

            if (splitTypeAndMessage.Length != 2)
            {
                Debug.LogError($"In NetUpdateHandler.ProcessMessage, expected an array of length 2 but length was {splitTypeAndMessage.Length}");

                return;
            }

            string type = splitTypeAndMessage[0];

            string message = splitTypeAndMessage[1];

            //TODO(Brandon): refactor this into an event-based system. Otherwise, the main thread could get blocked by a lot of callbacks getting called for one message, I believe.
            //check our subsribers for the type of message and call the corresponding funcions
            if (GlobalMessageManager.Instance.subscribers.TryGetValue(type, out List<System.Action<string>> funcsToCall))
            {
                //call our message associated funcions
                foreach (var func in funcsToCall)
                {
                    func(message);
                }
            } 
            else 
            {
                Debug.LogWarning($"Unknown message type {type}; Make sure you register the type and associated functions to call with GlobalMessageManager.cs. Payload: {message}");
            }
        }

        public void OnApplicationQuit()
        {
            //deregister our update loops
            if (GameStateManager.IsAlive)
            {
                GameStateManager.Instance.DeRegisterUpdatableObject(this);
            }
        }
    }
}