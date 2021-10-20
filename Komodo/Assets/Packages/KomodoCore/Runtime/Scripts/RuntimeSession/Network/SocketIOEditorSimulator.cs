using UnityEngine;
using Komodo.Utilities;
using System.Collections.Generic;

namespace Komodo.Runtime
{
    public class SocketIOEditorSimulator : SingletonComponent<SocketIOEditorSimulator>
    {
        public static SocketIOEditorSimulator Instance
        {
            get { return (SocketIOEditorSimulator) _Instance; }
            set { _Instance = value; }
        }

        public bool isVerbose = false;
        public bool doLogClientEvents = true;
        public bool doLogCustomInteractions = true;
        public bool doLogPositionEvents = false;

        public bool setSocketIOAdapterNameFails;

        public bool openSyncConnectionFails;

        public bool openChatConnectionFails;

        public bool setSyncEventListenersFails;

        public bool setChatEventListenersFails;

        public bool joinSyncSessionFails;

        public bool joinChatSessionFails;

        public bool leaveSyncSessionFails;

        public bool leaveChatSessionFails;

        public bool sendStateCatchUpRequestFails;

        public bool enableVRButtonFails;

        public bool closeSyncConnectionFails;

        public bool closeChatConnectionFails;

        public bool doLogMessageEvents = false;

        public int clientId;

        public int sessionId;
        
        public int isTeacher;

        public string sessionDetails = @"{""assets"":[{""id"":111550,""name"":""GraceGremer"",""url"":""https://s3.us-east-2.amazonaws.com/vrcat-assets/8f5fef97-a735-4c2e-8d28-fc3badfe09a3/model.glb"",""isWholeObject"":true,""scale"":1},{""id"":111576,""name"":""GarmentSetup1"",""url"":""https://s3.us-east-2.amazonaws.com/vrcat-assets/1843c37c-dc40-4520-91cb-ad1cdc70d72e/model.glb"",""isWholeObject"":false,""scale"":1},{""id"":111577,""name"":""GarmentSetup2"",""url"":""https://s3.us-east-2.amazonaws.com/vrcat-assets/cb3464eb-f96e-48c6-b774-e4dfbdc8ab78/model.glb"",""isWholeObject"":false,""scale"":1},{""id"":111578,""name"":""GarmentSetup3"",""url"":""https://s3.us-east-2.amazonaws.com/vrcat-assets/e8567627-ade7-493e-a017-e8b1b61e71af/model.glb"",""isWholeObject"":false,""scale"":1},{""id"":111579,""name"":""GarmentSetup4"",""url"":""https://s3.us-east-2.amazonaws.com/vrcat-assets/9a133a69-c8ba-4cf8-9539-a9a2b2827226/model.glb"",""isWholeObject"":false,""scale"":1},{""id"":111580,""name"":""GarmentSetup5"",""url"":""https://s3.us-east-2.amazonaws.com/vrcat-assets/0529709a-803e-4f61-86d1-092dabf0c2cb/model.glb"",""isWholeObject"":false,""scale"":1},{""id"":111589,""name"":""ShanenHaigler"",""url"":""https://s3.us-east-2.amazonaws.com/vrcat-assets/7bb5d74d-39f3-4b3d-aa56-4708b62bda95/model.glb"",""isWholeObject"":true,""scale"":1},{""id"":111597,""name"":""Garment O'Donnell"",""url"":""https://s3.us-east-2.amazonaws.com/vrcat-assets/ee63a354-b427-4ca3-a2a0-6e230efabe55/model.glb"",""isWholeObject"":true,""scale"":1},{""id"":111602,""name"":""GraceGremer"",""url"":""https://s3.us-east-2.amazonaws.com/vrcat-assets/14bacabd-7f09-4120-b420-ccfa09b23e03/model.glb"",""isWholeObject"":true,""scale"":1},{""id"":111604,""name"":""CarleeIhde"",""url"":""https://s3.us-east-2.amazonaws.com/vrcat-assets/de622ac8-eabd-4d3a-ab8f-ab56ee415813/model.glb"",""isWholeObject"":true,""scale"":1},{""id"":111605,""name"":""CarleeI"",""url"":""https://s3.us-east-2.amazonaws.com/vrcat-assets/29394a64-0e31-4824-86f8-133297f9f84c/model.glb"",""isWholeObject"":true,""scale"":1},{""id"":111607,""name"":""G-SarahMiranda"",""url"":""https://s3.us-east-2.amazonaws.com/vrcat-assets/f3f1442c-f9cc-454b-8b32-8f3597d79272/model.glb"",""isWholeObject"":true,""scale"":1},{""id"":111608,""name"":""SarahMirandaMood"",""url"":""https://s3.us-east-2.amazonaws.com/vrcat-assets/0b5036d8-37f0-4c32-b65a-7030cef4718d/model.glb"",""isWholeObject"":true,""scale"":1},{""id"":111609,""name"":""G-ShanenHaigleer"",""url"":""https://s3.us-east-2.amazonaws.com/vrcat-assets/5539b18d-ed67-48cb-8a87-164fc161319a/model.glb"",""isWholeObject"":true,""scale"":1}],""build"":""/test/Brandon-develop-2021-10-19-15xx/"",""course_id"":3,""create_at"":""2021-03-26T01:00:58.000Z"",""description"":""(No description added)"",""end_time"":""2021-03-31T19:03:00.000Z"",""session_id"":141,""session_name"":""SP21 - Critique Group C"",""start_time"":""2021-03-31T18:03:00.000Z"",""users"":[]}";

        private float[] _arrayPointer;
        private int _relayUpdateSize;
        private int _posCursor;

        public string InstantiationManagerName = "InstantiationManager";
        public string NetworkManagerName = "NetworkManager";
        private ClientSpawnManager _ClientSpawnManager;
        private NetworkUpdateHandler _NetworkUpdateHandler;

        public void Awake()
        {
            //used to set our managers alive state to true to detect if it exist within scene
            var initManager = Instance;
        }

        public void Start()
        {
            var instMgr = GameObject.Find(InstantiationManagerName);
            if (!instMgr)
            {
                throw new System.Exception($"You must have a GameObject named {InstantiationManagerName} in your scene.");
            }
            _ClientSpawnManager = instMgr.GetComponent<ClientSpawnManager>();

            if (!_ClientSpawnManager)
            {
                throw new System.Exception($"{InstantiationManagerName} must have a ClientSpawnManager component.");
            }

            var netMgr = GameObject.Find(NetworkManagerName);
            if (!netMgr)
            {
                throw new System.Exception($"You must have a GameObject named {NetworkManagerName} in your scene.");
            }
            _NetworkUpdateHandler = netMgr.GetComponent<NetworkUpdateHandler>();

            if (!_NetworkUpdateHandler)
            {
                throw new System.Exception($"{NetworkManagerName} must have a NetworkUpdateHandler component.");
            }
        }

        private void DebugLog (string message) {
            Debug.Log($"[SocketSim] {message}");
        }

        public void GameInstanceSendMessage(string who, string what, string data)
        {
            if (isVerbose) DebugLog($"GameInstanceSendMessage({who}, {what}, {data})");
        }

        public void Emit(string name, string data)
        {
            if (isVerbose) DebugLog($"Emit({name}, {data})");
        }

        [ContextMenu("OnReceiveEmptyStateCatchUp")]
        public void OnReceiveEmptyStateCatchUp() {
            var socketIOAdapter = (SocketIOAdapter) FindObjectOfType(typeof(SocketIOAdapter));

            socketIOAdapter.OnReceiveStateCatchup("{\"clients\": [99, 98], \"entities\": [], \"isRecording\": false,\"scene\": null}");
        }

        //{"assets":[{"id":111420,"name":"Dragon whole","url":"https://s3.us-east-2.amazonaws.com/vrcat-assets/9bc7be11-8784-44a5-a621-0705f0e8e5dc/model.glb","isWholeObject":true,"scale":1},{"id":111452,"name":"Miller Index Planes","url":"https://s3.us-east-2.amazonaws.com/vrcat-assets/feabc4e3-1cdf-4663-b1c7-c63efe677a56/model.glb","isWholeObject":false,"scale":1},{"id":111470,"name":"Sheer Dress","url":"https://s3.us-east-2.amazonaws.com/vrcat-assets/b2dee1ca-a203-4e49-841d-fd81ce53eb1d/model.glb","isWholeObject":true,"scale":1},{"id":111478,"name":"TiltBrush BrushTests","url":"https://s3.us-east-2.amazonaws.com/vrcat-assets/fe562af4-e660-454c-b9e5-b6c57086cc12/model.glb","isWholeObject":true,"scale":1}],"build":"base/stable","course_id":1,"create_at":"2020-11-13T20:19:54.000Z","description":" This is a the demo session for our talk with TCNJ. ","end_time":"2020-11-13T19:11:00.000Z","session_id":126,"session_name":"TCNJ Demo","start_time":"2020-11-13T18:11:00.000Z","users":[{"student_id":1,"email":"admin@komodo.edu","first_name":"Admin","last_name":"Komodo"},{"student_id":2,"email":"first1@illinois.edu","first_name":"First1","last_name":"Last1"},{"student_id":5,"email":"first2@illinois.edu","first_name":"First2","last_name":"Last2"},{"student_id":10,"email":"dtamay3@illinois.edu","first_name":"First3","last_name":"Last3"},{"student_id":14,"email":"first3@illinois.edu","first_name":"Alex","last_name":"Cabada"},{"student_id":26,"email":"demo1@illinois.edu","first_name":"First4","last_name":"First5"},{"student_id":27,"email":"first5@illinois.edu","first_name":"First5","last_name":"Last5"},{"student_id":28,"email":"demo3@illinois.edu","first_name":"Demo","last_name":"3"},{"student_id":29,"email":"demo4@illinois.edu","first_name":"Demo","last_name":"4"},{"student_id":30,"email":"demo5@illinois.edu","first_name":"Demo","last_name":"5"},{"student_id":31,"email":"demo6@illinois.edu","first_name":"Demo","last_name":"6"}]}

        [ContextMenu("OnReceiveExampleStateCatchUp")]
        public void OnReceiveExampleStateCatchUp()
        {
            var socketIOAdapter = (SocketIOAdapter) FindObjectOfType(typeof(SocketIOAdapter));

            int id0 = NetworkedObjectsManager.Instance.GenerateEntityIDBase() + 0; //111420; // 
            int id1 = NetworkedObjectsManager.Instance.GenerateEntityIDBase() + 1; //111452; //
            int id2 = NetworkedObjectsManager.Instance.GenerateEntityIDBase() + 2; //111470; //
            int id3 = NetworkedObjectsManager.Instance.GenerateEntityIDBase() + 3; //111478; //

            Position pos0 = new Position(-1, id0, (int) Entity_Type.objects, 1, new Quaternion(), new Vector3(0.5f, 0.5f, 0.5f));
            Position pos1 = new Position(-1, id1, (int) Entity_Type.objects, 1, new Quaternion(), new Vector3(1.0f, 1.0f, 1.0f));
            Position pos2 = new Position(-1, id2, (int) Entity_Type.objects, 1, new Quaternion(), new Vector3(1.5f, 1.5f, 1.5f));
            Position pos3 = new Position(-1, id3, (int) Entity_Type.objects, 1, new Quaternion(), new Vector3(2.0f, 2.0f, 2.0f));

            string latest0 = "[" + string.Join(",", NetworkUpdateHandler.Instance.SerializeCoordsStruct(pos0)) + "]";
            string latest1 = "[" + string.Join(",", NetworkUpdateHandler.Instance.SerializeCoordsStruct(pos1)) + "]";
            string latest2 = "[" + string.Join(",", NetworkUpdateHandler.Instance.SerializeCoordsStruct(pos2)) + "]";
            string latest3 = "[" + string.Join(",", NetworkUpdateHandler.Instance.SerializeCoordsStruct(pos3)) + "]";

            //arr[SEQ] = (float)seq;
            // arr[SESSION_ID] = (float)session_id;
            // arr[CLIENT_ID] = (float)coords.clientId;
            // arr[ENTITY_ID] = (float)coords.entityId;
            // arr[ENTITY_TYPE] = (float)coords.entityType;
            // arr[SCALE] = coords.scaleFactor;
            // arr[ROTX] = coords.rot.x;
            // arr[ROTY] = coords.rot.y;
            // arr[ROTZ] = coords.rot.z;
            // arr[ROTW] = coords.rot.w;
            // arr[POSX] = coords.pos.x;
            // arr[POSY] = coords.pos.y;
            // arr[POSZ] = coords.pos.z;
            // arr[DIRTY] = 1;

            string stateString = "{\"clients\": [99, 98, 97, 96, 95, 94], \"entities\": [ {\"id\":" + id0 + ",\"latest\": " + latest0 + ",\"render\":true,\"locked\":true}, {\"id\":" + id1 + ",\"latest\": " + latest1 + ",\"render\":true,\"locked\":true}, {\"id\":" + id2 + ",\"latest\": " + latest2 + ",\"render\":true,\"locked\":true}, {\"id\":" + id3 + ",\"latest\": " + latest3 + ",\"render\":true,\"locked\":true}], \"isRecording\": false, \"scene\": null}";

            Debug.Log(stateString);

            socketIOAdapter.OnReceiveStateCatchup(stateString);
        }

        public void OnReceiveStateCatchUp(string jsonStringifiedData)
        {
            if (isVerbose) DebugLog($"received state sync event: {jsonStringifiedData}");

            Debug.LogError("Need to call SocketIOAdapter.OnReceiveStateCatchup(jsonStringifiedData); here");
        }

        public int SendStateCatchUpRequest()
        {
            if (isVerbose) DebugLog("SendStateCatchUpRequest");
            Emit("state", "{ session_id: session_id, client_id: client_id }");

            return sendStateCatchUpRequestFails ? SocketIOJSLib.FAILURE : SocketIOJSLib.SUCCESS;
        }

        public void OnJoined(int clientId)
        {
            if (doLogClientEvents) DebugLog($"OnJoined({clientId})");
            _NetworkUpdateHandler.RegisterClient(clientId);
        }

        public void OnDisconnected(int clientId)
        {
            if (doLogClientEvents) DebugLog($"OnDisconnected({clientId})");
            _NetworkUpdateHandler.UnregisterClient(clientId);
        }

        public void OnMicText(string jsonStringifiedData)
        {
            DebugLog("OnMicText");
            _ClientSpawnManager.OnReceiveSpeechToTextSnippet(jsonStringifiedData);
        }

        public int SetChatEventListeners()
        {
            DebugLog("SetChatEventListeners");
            //todo(Brandon): call OnMicText with data

            return setChatEventListenersFails ? SocketIOJSLib.FAILURE : SocketIOJSLib.SUCCESS;
        }

        public void OnDraw(float[] data)
        {
            DebugLog($"OnDraw({data})");
        }

        public void InitReceiveDraw(float[] arrayPointer, int size)
        {
            DebugLog("InitReceiveDraw");
            // int drawCursor = 0;
            //todo(Brandon): call OnDraw with data and pass in drawCursor also
        }

        public void SendDraw(float[] arrayPointer, int size)
        {
            DebugLog("SendDraw");
            Emit("draw", arrayPointer.ToString());
        }

        public int GetClientIdFromBrowser()
        {
            if (doLogClientEvents) DebugLog("GetClientIdFromBrowser -- returning user-set value");
            return clientId;
        }

        public int GetSessionIdFromBrowser()
        {
            DebugLog("GetSessionIdFromBrowser -- returning user-set value");
            return sessionId;
        }

        public int GetIsTeacherFlagFromBrowser()
        {
            DebugLog("GetIsTeacherFlagFromBrowser -- returning user-set value");
            return isTeacher;
        }

        public void SocketIOSendPosition(float[] array, int size)
        {
            if (doLogPositionEvents) DebugLog("SocketIOSendPosition");
            Emit("update", array.ToString());
        }

        public void SocketIOSendInteraction(int[] array, int size)
        {
            if (doLogCustomInteractions) DebugLog($"SocketIOSendInteraction({array.ToString()}, {size})");
            Emit("interact", array.ToString());
        }

        /**
            InitSocketIOReceivePosition: function(arrayPointer, size) {
                if (window.socket) {
                    var posCursor = 0;

                    // NOTE(rob):
                    // we use "arrayPointer >> 2" to change the pointer location on the module heap
                    // when interpreted as float32 values ("HEAPF32[]"). 
                    // for example, an original arrayPointer value (which is a pointer to a 
                    // position on the module heap) of 400 would right shift to 100 
                    // which would be the correct corresponding index on the heap
                    // for elements of 32-bit size.

                    window.socket.on('relayUpdate', function(data) {
                        if (data.length + posCursor > size) {
                            posCursor = 0;
                        }
                        for (var i = 0; i < data.length; i++) {
                            HEAPF32[(arrayPointer >> 2) + posCursor + i] = data[i];
                        }
                        posCursor += data.length;
                    });
                }
            },
        */
        public void InitSocketIOReceivePosition(float[] arrayPointer, int size)
        {
            _arrayPointer = arrayPointer;
            
            DebugLog($"InitSocketIOReceivePosition({_arrayPointer}, {size})");

            _relayUpdateSize = size;
            //  var posCursor = 0;
            //todo(Brandon): call OnRelayUpdate, passing in data, and updating posCursor
        }

        /** 
         * See the body of InitSocketIOReceivePosition for the relayUpdate 
         * event listener.
         */
        public void RelayPositionUpdate(float[] data) 
        {

            if (doLogPositionEvents)
            {
                string dataString = string.Join(" ", data);

                DebugLog($"RelayUpdate({(dataString != "" ? dataString : "null")})");
            }

            if (data.Length + _posCursor > _relayUpdateSize) {
                _posCursor = 0;
            }

            for (var i = 0; i < data.Length; i++) {
                _arrayPointer[_posCursor + i] = data[i];
            }

            _posCursor += data.Length;
        }

        public void OnInteractionUpdate(float[] data)
        {
            if (doLogCustomInteractions) DebugLog($"OnInteractionUpdate({data.ToString()})");
        }

        public void InitSocketIOReceiveInteraction(int[] arrayPointer, int size)
        {
            DebugLog("InitSocketIOReceiveInteraction");
            // var intCursor = 0;
            //todo(Brandon): call OnInteractionUpdate, passing in data, and updating intCursor
        }

        public void ToggleCapture(int operation, int session_id)
        {
            if (operation == 0)
            {
                Emit("start_recording", session_id.ToString());
            }
            else
            {
                Emit("end_recording", session_id.ToString());
            }
        }

        /**
            GetSessionDetails: function() {
                if (window.details) {
                    var serializedDetails = JSON.stringify(window.details);
                    if (serializedDetails) {
                        var bufferSize = lengthBytesUTF8(serializedDetails) + 1;
                        var buffer = _malloc(bufferSize);
                        stringToUTF8(serializedDetails, buffer, bufferSize);
                        return buffer;
                    } else {
                        console.log("Unable to serialize details: " + window.details)
                        var bufferSize = lengthBytesUTF8("{}") + 1;
                        var buffer = _malloc(bufferSize);
                        stringToUTF8("", buffer, bufferSize);
                        return buffer;
                    }
                } else {
                    // var bufferSize = lengthBytesUTF8("{details:{}}") + 1;
                    // var buffer = _malloc(bufferSize);
                    // stringToUTF8("", buffer, bufferSize);
                    // return buffer;
                    return null;
                }
            },
        **/
        public static string GetSessionDetails ()
        {
            //TODO -- extend this with a public boolean to account for multiple code paths above.
            Instance.DebugLog($"GetSessionDetails()");

            return Instance.sessionDetails;
        }

        public void BrowserEmitMessage (string type, string message)
        {
            if (!doLogMessageEvents)
            {
                return;
            }

            DebugLog($"BrowserEmitMessage({type}, {message})");
        }

        public int CloseSyncConnection () {
            DebugLog("CloseSyncConnection");

            return closeSyncConnectionFails ? SocketIOJSLib.FAILURE : SocketIOJSLib.SUCCESS;
        }

        public int CloseChatConnection () {
            DebugLog("CloseSyncConnection");

            return closeChatConnectionFails ? SocketIOJSLib.FAILURE : SocketIOJSLib.SUCCESS;
        }

        public int SetSyncEventListeners () {
            DebugLog("SetSyncEventListeners()");

            return setSyncEventListenersFails ? SocketIOJSLib.FAILURE : SocketIOJSLib.SUCCESS;
        }

        public int OpenSyncConnection() {
            DebugLog("OpenSyncConnection()");

            return openSyncConnectionFails ? SocketIOJSLib.FAILURE : SocketIOJSLib.SUCCESS;
        }

        public int OpenChatConnection() {
            DebugLog("OpenChatConnection()");

            return openChatConnectionFails ? SocketIOJSLib.FAILURE : SocketIOJSLib.SUCCESS;
        }

        public int EnableVRButton () {
            DebugLog("EnableVRButton()");

            return enableVRButtonFails ? SocketIOJSLib.FAILURE : SocketIOJSLib.SUCCESS;
        }

        public int JoinSyncSession () {
            DebugLog("JoinSyncSession()");

            return joinSyncSessionFails ? SocketIOJSLib.FAILURE : SocketIOJSLib.SUCCESS;
        }

        public int JoinChatSession () {
            DebugLog("JoinChatSession()");

            return joinChatSessionFails ? SocketIOJSLib.FAILURE : SocketIOJSLib.SUCCESS;
        }

        public int LeaveSyncSession () {
            DebugLog("LeaveSyncSession()");

            return leaveSyncSessionFails ? SocketIOJSLib.FAILURE : SocketIOJSLib.SUCCESS;
        }

        public int LeaveChatSession () {
            DebugLog("LeaveChatSession()");

            return leaveChatSessionFails ? SocketIOJSLib.FAILURE : SocketIOJSLib.SUCCESS;
        }

        public string SetSocketIOAdapterName (string name) {
            DebugLog($"window.socketIOAdapterName = {name}");

            if (setSocketIOAdapterNameFails)
            {
                return "INCORRECT_NAME";
            }
            return SocketIOAdapter.Instance.gameObject.name;
        }

        [ContextMenu("Ping Example")]
        public void PingExample () {
            SocketIOAdapter.Instance.OnPing();
        }

        [ContextMenu("Pong Example")]
        public void PongExample () {
            SocketIOAdapter.Instance.OnPong(56789);
        }
    }
}