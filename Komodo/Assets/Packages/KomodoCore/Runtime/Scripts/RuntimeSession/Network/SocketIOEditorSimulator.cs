using UnityEngine;
using Komodo.Utilities;

namespace Komodo.Runtime
{
    public class SocketIOEditorSimulator : SingletonComponent<SocketIOEditorSimulator>
    {
        public static SocketIOEditorSimulator Instance
        {
            get { return ((SocketIOEditorSimulator)_Instance); }
            set { _Instance = value; }
        }

        public bool isVerbose = false;
        public bool doLogClientEvents = true;
        public bool doLogCustomInteractions = true;
        public bool doLogPositionEvents = false;

        public int clientId;
        public int sessionId;
        public int isTeacher;

        public string sessionDetails = @"{""assets"":[{""id"":111420,""name"":""Dragon whole"",""url"":""https://s3.us-east-2.amazonaws.com/vrcat-assets/9bc7be11-8784-44a5-a621-0705f0e8e5dc/model.glb"",""isWholeObject"":true,""scale"":1},{""id"":111452,""name"":""Miller Index Planes"",""url"":""https://s3.us-east-2.amazonaws.com/vrcat-assets/feabc4e3-1cdf-4663-b1c7-c63efe677a56/model.glb"",""isWholeObject"":false,""scale"":1},{""id"":111470,""name"":""Sheer Dress"",""url"":""https://s3.us-east-2.amazonaws.com/vrcat-assets/b2dee1ca-a203-4e49-841d-fd81ce53eb1d/model.glb"",""isWholeObject"":true,""scale"":1},{""id"":111478,""name"":""TiltBrush BrushTests"",""url"":""https://s3.us-east-2.amazonaws.com/vrcat-assets/fe562af4-e660-454c-b9e5-b6c57086cc12/model.glb"",""isWholeObject"":true,""scale"":1}],""build"":""base/stable"",""course_id"":1,""create_at"":""2020-11-13T20:19:54.000Z"",""description"":"" This is a the demo session for our talk with TCNJ. "",""end_time"":""2020-11-13T19:11:00.000Z"",""session_id"":126,""session_name"":""TCNJ Demo"",""start_time"":""2020-11-13T18:11:00.000Z"",""users"":[{""student_id"":1,""email"":""admin@komodo.edu"",""first_name"":""Admin"",""last_name"":""Komodo""},{""student_id"":2,""email"":""first1@illinois.edu"",""first_name"":""First1"",""last_name"":""Last1""},{""student_id"":5,""email"":""first2@illinois.edu"",""first_name"":""First2"",""last_name"":""Last2""},{""student_id"":10,""email"":""dtamay3@illinois.edu"",""first_name"":""First3"",""last_name"":""Last3""},{""student_id"":14,""email"":""first3@illinois.edu"",""first_name"":""Alex"",""last_name"":""Cabada""},{""student_id"":26,""email"":""demo1@illinois.edu"",""first_name"":""First4"",""last_name"":""First5""},{""student_id"":27,""email"":""first5@illinois.edu"",""first_name"":""First5"",""last_name"":""Last5""},{""student_id"":28,""email"":""demo3@illinois.edu"",""first_name"":""Demo"",""last_name"":""3""},{""student_id"":29,""email"":""demo4@illinois.edu"",""first_name"":""Demo"",""last_name"":""4""},{""student_id"":30,""email"":""demo5@illinois.edu"",""first_name"":""Demo"",""last_name"":""5""},{""student_id"":31,""email"":""demo6@illinois.edu"",""first_name"":""Demo"",""last_name"":""6""}]}";

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
            DebugLog($"[SocketSim] {message}");
        }

        public void GameInstanceSendMessage(string who, string what, string data)
        {
            if (isVerbose) DebugLog($"GameInstanceSendMessage({who}, {what}, {data})");
        }

        public void Emit(string name, string data)
        {
            if (isVerbose) DebugLog($"Emit({name}, {data})");
        }

        public void OnState(string jsonStringifiedData)
        {
            if (isVerbose) DebugLog($"received state sync event: {jsonStringifiedData}");
            _ClientSpawnManager.SyncSessionState(jsonStringifiedData);
        }

        public void SendStateCatchUpRequest()
        {
            if (isVerbose) DebugLog("SendStateCatchUpRequest");
            Emit("state", "{ session_id: session_id, client_id: client_id }");
        }

        public void OnJoined(int clientId)
        {
            if (doLogClientEvents) DebugLog($"OnJoined({clientId})");
            _NetworkUpdateHandler.RegisterNewClientId(clientId);
        }

        public void OnDisconnected(int clientId)
        {
            if (doLogClientEvents) DebugLog($"OnDisconnected({clientId})");
            _NetworkUpdateHandler.UnregisterClientId(clientId);
        }

        public void OnMicText(string jsonStringifiedData)
        {
            DebugLog("OnMicText");
            _ClientSpawnManager.Text_Refresh(jsonStringifiedData);
        }

        public void SetChatEventListeners()
        {
            DebugLog("SetChatEventListeners");
            //todo(Brandon): call OnMicText with data
        }

        public void OnDraw(float[] data)
        {
            DebugLog($"OnDraw({data.ToString()})");
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
        public string GetSessionDetails ()
        {
            //TODO -- extend this with a public boolean to account for multiple code paths above.
            DebugLog($"GetSessionDetails()");

            return sessionDetails;
        }

        public void BrowserEmitMessage (string type, string message) 
        {
            DebugLog($"BrowserEmitMessage({type}, {message})");
        }

        public void Disconnect () {
            DebugLog("Disconnect");
        }

        public void SetSyncEventListeners () {
            DebugLog("SetSyncEventListeners()");
        }

        public void OpenSyncConnection() {
            DebugLog("OpenSyncConnection()");
        }

        public void OpenChatConnection() {
            DebugLog("OpenChatConnection()");
        }

        public void EnableVRButton () {
            DebugLog("EnableVRButton()");
        }

        public void JoinSyncSession () {
            DebugLog("JoinSyncSession()");
        }

        public void JoinChatSession () {
            DebugLog("JoinChatSession()");
        }
    }
}