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

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Unity.Entities;
using System;

[System.Serializable]
public class Int_UnityEvent : UnityEvent<int> { }

public struct Position
{
    public int clientId;
    public int entityId;
    public int entityType;
    public float scaleFactor;
    public Quaternion rot;
    public Vector3 pos;

    public Position(int clientId, int entityId, int entityType, float scaleFactor, Quaternion rot, Vector3 pos)
    {
        this.clientId = clientId;
        this.entityId = entityId;
        this.entityType = entityType;
        this.scaleFactor = scaleFactor;
        this.rot = rot;
        this.pos = pos;
    }

}

public struct Interaction
{

    public int sourceEntity_id;
    public int targetEntity_id;
    public int interactionType;

    public Interaction(int sourceEntity_id, int targetEntity_id, int interactionType)
    {

        this.sourceEntity_id = sourceEntity_id;
        this.targetEntity_id = targetEntity_id;
        this.interactionType = interactionType;

    }

}

public struct Draw
{
    public int clientId;
    public int strokeId;
    public int strokeType;
    public float lineWidth;
    public Vector3 curStrokePos;
    public Vector4 curColor;

    public Draw(int clientId, int strokeId, int strokeType, float lineWidth, Vector3 curStrokePos, Vector4 curColor)
    {
        this.clientId = clientId;
        this.strokeId = strokeId;
        this.strokeType = strokeType;
        this.lineWidth = lineWidth;
        this.curStrokePos = curStrokePos;
        this.curColor = curColor;
    }
}

[System.Serializable]
public struct User
{
    public int student_id;
    public string first_name;
    public string last_name;
    public string email;
}

[System.Serializable]
public struct SessionDetails
{
    public List<AssetDataTemplate.AssetImportData> assets;
    public string build;
    public int course_id;
    public string create_at;
    public string description;
    public string end_time;
    public int session_id;
    public string session_name;
    public string start_time;
    public List<User> users;
}


//We use interfaces to centralize our update calls and optimize crossing between manage and native code see GameStateManager.cs
public class NetworkUpdateHandler : SingletonComponent<NetworkUpdateHandler>, IUpdatable
{

    public static NetworkUpdateHandler Instance
    {
        get { return ((NetworkUpdateHandler)_Instance); }
        set { _Instance = value; }
    }

    // import callable js functions
    // socket.io with webgl
    // https://www.gamedev.net/articles/programming/networking-and-multiplayer/integrating-socketio-with-unity-5-webgl-r4365/
    [DllImport("__Internal")]
    private static extern void InitSessionStateHandler();

    [DllImport("__Internal")]
    private static extern void InitSessionState();

    [DllImport("__Internal")]
    private static extern void InitSocketIOClientCounter();

    [DllImport("__Internal")]
    private static extern void InitClientDisconnectHandler();

    [DllImport("__Internal")]
    private static extern void InitMicTextHandler();

    [DllImport("__Internal")]
    private static extern int GetClientIdFromBrowser();

    [DllImport("__Internal")]
    private static extern int GetSessionIdFromBrowser();

    [DllImport("__Internal")]
    private static extern int GetIsTeacherFlagFromBrowser();

    [DllImport("__Internal")]
    private static extern void InitSocketIOReceivePosition(float[] array, int size);

    [DllImport("__Internal")]
    private static extern void SocketIOSendPosition(float[] array, int size);

    [DllImport("__Internal")]
    private static extern void SocketIOSendInteraction(int[] array, int size);

    [DllImport("__Internal")]
    private static extern void InitSocketIOReceiveInteraction(int[] array, int size);

    [DllImport("__Internal")]
    private static extern void InitReceiveDraw(float[] array, int size);

    [DllImport("__Internal")]
    private static extern void SendDraw(float[] array, int size);

    [DllImport("__Internal")]
    private static extern void EnableVRButton();

    [DllImport("__Internal")]
    private static extern string GetSessionDetails();

    [DllImport("__Internal")]
    private static extern void BrowserEmitMessage(string message);

    [DllImport("__Internal")]
    private static extern void InitBrowserReceiveMessage();


    // Message System: WIP
    // to send a message
    // 1. pack a struct with the data you need
    // 2. serialize that struct
    // 3. pass the message `type` and the serialized struct in the constructor
    // 4. call the .Send() method
    // 5. write a handler and register it in the ProcessMessage function below
    // 6. this is still a hacky way to do it, so feel free to change/improve as you see fit. 
    [System.Serializable]
    public struct KomodoMessage
    {
        public string type;
        public string data;

        public KomodoMessage(string type, string messageData)
        {
            this.type = type;
            this.data = messageData;
        }

        public void Send()
        {
            var message = JsonUtility.ToJson(this);
            BrowserEmitMessage(message);
        }

    }

#if !UNITY_EDITOR && UNITY_WEBGL
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

    public AssetDataTemplate assetImportDataContainer;

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
        arr[CLIENT_ID] = (float)client_id;
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


    public void Awake()
    {
        //optimization - register our update calls
        // procesing all update loops from one main update loop is optimal to avoid  
        // crossing native to manage code 
        GameStateManager.Instance.RegisterUpdatableObject(this);

#if !UNITY_EDITOR && UNITY_WEBGL
        //don't assign a SocketIO Simulator for WebGL build
#else
        SocketSim = SocketIOEditorSimulator.Instance;
        if (!SocketSim)
        {
            Debug.LogWarning("No SocketIOEditorSimulator was found in the scene. In-editor behavior may not be as expected.");
        }
#endif

#if !UNITY_EDITOR && UNITY_WEBGL
        InitSessionStateHandler();
        
        InitSocketIOClientCounter();
        InitClientDisconnectHandler();
        InitMicTextHandler();

        InitBrowserReceiveMessage();
        
        client_id = GetClientIdFromBrowser();
        session_id = GetSessionIdFromBrowser();
        isTeacher  = GetIsTeacherFlagFromBrowser();

#else 
        SocketSim.InitSessionStateHandler();
        SocketSim.InitSessionState();
        SocketSim.InitSocketIOClientCounter();
        SocketSim.InitClientDisconnectHandler();
        SocketSim.InitMicTextHandler();

        client_id = SocketSim.GetClientIdFromBrowser();
        session_id = SocketSim.GetSessionIdFromBrowser();
     //   isTeacher = SocketSim.GetIsTeacherFlagFromBrowser();
#endif


#if !UNITY_EDITOR && UNITY_WEBGL
        // set up shared memory with js context
        InitSocketIOReceivePosition(position_data, position_data.Length);
        InitSocketIOReceiveInteraction(interaction_data, interaction_data.Length);
        InitReceiveDraw(draw_data, draw_data.Length);
#else
        // set up shared memory with js context
        SocketSim.InitSocketIOReceivePosition(position_data, position_data.Length);
        SocketSim.InitSocketIOReceiveInteraction(interaction_data, interaction_data.Length);
        SocketSim.InitReceiveDraw(draw_data, draw_data.Length);
#endif

#if !UNITY_EDITOR && UNITY_WEBGL
        // clear the assets list
        assetImportDataContainer.assets.Clear();

        // Get session details from browser api call
        string SessionDetailsString = GetSessionDetails();
        if (System.String.IsNullOrEmpty(SessionDetailsString)) {
            Debug.Log("Error: Details are null or empty.");
        } else {
            Debug.Log("SessionDetails: " + SessionDetailsString);
            var Details = JsonUtility.FromJson<SessionDetails>(SessionDetailsString);
            assetImportDataContainer.assets = Details.assets;

            if (sessionName != null)
            {
                sessionName = Details.session_name;
                buildName = Details.build;
            }
            else
                Debug.LogError("SessionName Ref in NetworkUpdateHandler's Text Component is missing from editor");

  
        }

#else
        //        // Get Assets object from browser context
        //        // string Assets = SocketSim.GrabAssets();
#endif
    }

    public void Start()
    {
        #region ECS Funcionality Setup our User Data

        //setup data for our player components
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var eqDesc = new EntityQueryDesc { All = new ComponentType[] { typeof(OurPlayerTag), typeof(NetworkEntityIdentificationComponentData) } };

        var entities = entityManager.CreateEntityQuery(eqDesc).ToEntityArray(Unity.Collections.Allocator.Temp);

        foreach (var entity in entities)
        {
            var entityIDFromType = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(entity).current_Entity_Type;

            entityManager.SetComponentData(entity, new NetworkEntityIdentificationComponentData { clientID = this.client_id, sessionID = this.session_id, entityID = (int)entityIDFromType, current_Entity_Type = entityIDFromType });

            if (isTeacher != 0) entityManager.AddComponent<TeacherTag>(entity);
        }

        entities.Dispose();

        #endregion
    }

    public void NetworkUpdate(Position pos)
    {
        float[] arr_pos = SerializeCoordsStruct(pos);
#if !UNITY_EDITOR && UNITY_WEBGL
        SocketIOSendPosition(arr_pos, arr_pos.Length);
#else 
        SocketSim.SocketIOSendPosition(arr_pos, arr_pos.Length);
#endif
    }

    public void InteractionUpdate(Interaction interact)
    {
        int[] arr_inter = new int[NUMBER_OF_INTERACTION_FIELDS];
        arr_inter[0] = seq;
        arr_inter[1] = session_id;
        arr_inter[2] = (int)client_id;
        arr_inter[3] = interact.sourceEntity_id;
        arr_inter[4] = interact.targetEntity_id;
        arr_inter[5] = (int)interact.interactionType;
        arr_inter[6] = 1; // dirty bit
#if !UNITY_EDITOR && UNITY_WEBGL
        SocketIOSendInteraction(arr_inter, arr_inter.Length);
#else 
        SocketSim.SocketIOSendInteraction(arr_inter, arr_inter.Length);
#endif
    }

    public void DrawUpdate(Draw draw)
    {
        var arr_draw = new float[NUMBER_OF_DRAW_FIELDS];
        arr_draw[0] = (float)seq;
        arr_draw[1] = (float)session_id;
        arr_draw[2] = (float)draw.clientId;
        arr_draw[3] = (float)draw.strokeId;
        arr_draw[4] = (float)draw.strokeType;
        arr_draw[5] = draw.lineWidth;
        arr_draw[6] = draw.curStrokePos.x;
        arr_draw[7] = draw.curStrokePos.y;
        arr_draw[8] = draw.curStrokePos.z;
        arr_draw[9] = draw.curColor.x;
        arr_draw[10] = draw.curColor.y;
        arr_draw[11] = draw.curColor.z;
        arr_draw[12] = draw.curColor.w;
        arr_draw[13] = 1; // dirty bit

        Debug.Log("sending draw update");
        Debug.Log(arr_draw[3].ToString());

#if !UNITY_EDITOR && UNITY_WEBGL
        SendDraw(arr_draw, arr_draw.Length);
#else
        SocketSim.SendDraw(arr_draw, arr_draw.Length);
#endif
    }
    public void OnUpdate(float realTime)
    {

        // iterate over the position_data heap and checks for new data.
        for (int i = 0; i < position_data.Length; i += NUMBER_OF_POSITION_FIELDS)
        {
            if ((int)position_data[i + DIRTY] != 0)
            {
                position_data[i + DIRTY] = 0; // reset the dirty bit
                                              // unpack entity update into Position struct
                var pos = new Position(
                    (int)position_data[i + CLIENT_ID],
                    (int)position_data[i + ENTITY_ID],
                    (int)position_data[i + ENTITY_TYPE],
                    position_data[i + SCALE],
                    new Quaternion(position_data[i + ROTX], position_data[i + ROTY], position_data[i + ROTZ], position_data[i + ROTW]),
                    new Vector3(position_data[i + POSX], position_data[i + POSY], position_data[i + POSZ])
                );
                // send new network data to client spawn manager
                if (ClientSpawnManager.IsAlive) { ClientSpawnManager.Instance.Client_Refresh(pos); }
            }
        }

        // checks interaction shared memory for new updates
        for (int i = 0; i < interaction_data.Length; i += NUMBER_OF_INTERACTION_FIELDS)
        {
            if (interaction_data[i + 6] != 0)
            { // check the dirty bit in the slot
                interaction_data[i + 6] = 0;  // reset the dirty bit
                var interaction = new Interaction(interaction_data[i + 3],
                                                  interaction_data[i + 4],
                                                  interaction_data[i + 5]);

                // send new network data to client spawn manager
                if (ClientSpawnManager.IsAlive) { ClientSpawnManager.Instance.Interaction_Refresh(interaction); }
            }
        }

        // checks for draw updates
        for (int i = 0; i < draw_data.Length; i += NUMBER_OF_DRAW_FIELDS)
        {
            if ((int)draw_data[i + 13] != 0)
            {
                draw_data[i + 13] = 0; // reset the dirty bit
                var drawing = new Draw((int)draw_data[i + 2],
                                       (int)draw_data[i + 3],
                                       (int)draw_data[i + 4],
                                       draw_data[i + 5],
                                       new Vector3(draw_data[i + 6], draw_data[i + 7], draw_data[i + 8]),
                                       new Vector4(draw_data[i + 9], draw_data[i + 10], draw_data[i + 11], draw_data[i + 12]));
                // process new drawing
                ClientSpawnManager.Instance.Draw_Refresh(drawing);
            }
        }

        seq++; // local sequence counter

    }


    public void RegisterNewClientId(int client_id)
    {
        ClientSpawnManager.Instance.AddNewClient(client_id);
    }

    public void UnregisterClientId(int client_id)
    {
        ClientSpawnManager.Instance.RemoveClient(client_id);
    }

    public void On_Initiation_Loading_Finished()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        EnableVRButton();
        InitSessionState();
        Debug.Log("Enabling EnterVR button");
#endif
    }

    public string GetPlayerNameFromClientID(int clientID)
    {
#if !UNITY_EDITOR && UNITY_WEBGL

        string SessionDetailsString = GetSessionDetails();
        var Details = JsonUtility.FromJson<SessionDetails>(SessionDetailsString);
        var hasName = false;
        foreach (User user in Details.users)
        {

            if (clientID != user.student_id)
                continue;

            hasName = true;
            return user.first_name + "  " + user.last_name;

        }

       return "Client : " + clientID;
#else
        return "Client: " + clientID;
#endif
    }


    public void ProcessMessage(string json)
    {
        var message = JsonUtility.FromJson<KomodoMessage>(json);
    
        // Message handlers
        // TODO(rob): thinking about SDK... register new handlers using global Message Manager?
        // ie. MessageManager.RegisterHandler("messageTypeName", MessageHandlerFunction);
        // so in ProcessMessage here we would call MessageManager.GetHandler(message.type)
        if (message.type == "test")
        {
            Debug.Log("Message of type test received");
            Debug.Log(message);
        } else {
            Debug.Log("Unknown message type");
        }
    }

    public void OnDestroy()
    {
        //deregister our update loops
        if (GameStateManager.IsAlive)
            GameStateManager.Instance.DeRegisterUpdatableObject(this);
    }
}
