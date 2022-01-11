using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Utilities;

namespace Komodo.Runtime
{
    public class SocketIOAdapter : SingletonComponent<SocketIOAdapter>
    {
        //Reminder -- socket-funcs.jslib can only send zero arguments, one string, or one number via the SendMessage function.

        public static SocketIOAdapter Instance
        {
            get { return (SocketIOAdapter)_Instance; }

            set { _Instance = value; }
        }
        private ConnectionAdapter connectionAdapter;

        private SocketIOEditorSimulator socketSim;

        private NetworkUpdateHandler netUpdateHandler;


        void Start()
        {
            connectionAdapter = (ConnectionAdapter) FindObjectOfType(typeof(ConnectionAdapter));

            if (connectionAdapter == null)
            {
                Debug.LogError("SocketIOAdapter: No object of type ConnectionAdapter was found in the scene.");
            }

            socketSim = SocketIOEditorSimulator.Instance;

            if (socketSim == null)
            {
                Debug.LogError("SocketIOAdapter: No SocketIOEditorSimulator.Instance was found in the scene.");
            }

            netUpdateHandler = NetworkUpdateHandler.Instance;

            if (netUpdateHandler == null)
            {
                Debug.LogError("SocketIOAdapter: No netUpdateHandler was found in the scene.");
            }

            SetName();
        }

        // Set the window.socketIOAdapterName variable in JS so that SendMessage calls in jslib are guaranteed to talk to the gameObject that has this script on it.
        public void SetName ()
        {
#if !UNITY_EDITOR && UNITY_WEBGL

            string nameOnWindow = SocketIOJSLib.SetSocketIOAdapterName(gameObject.name);
#else

            string nameOnWindow = SocketIOEditorSimulator.Instance.SetSocketIOAdapterName(gameObject.name);
#endif

            if (nameOnWindow != gameObject.name)
            {
                Debug.LogError($"SocketIOAdapter: window.socketIOAdapterName: Expected: {gameObject.name}, Actual: {nameOnWindow}");

                connectionAdapter.DisplaySocketIOAdapterError($"window.socketIOAdapterName: Expected: {gameObject.name}, Actual: {nameOnWindow}");
            }
        }

        public void OpenConnectionAndJoin()
        {
            OpenSyncConnection();

            OpenChatConnection();

            SetSyncEventListeners();

            SetChatEventListeners();

            JoinSyncSession();

            JoinChatSession();

            SendStateCatchUpRequest();

            EnableVRButton();
        }

        public void Leave ()
        {
            LeaveSyncSession();

            LeaveChatSession();
        }

        public void LeaveAndCloseConnection ()
        {
            LeaveSyncSession();

            LeaveChatSession();

            ClientSpawnManager.Instance.RemoveAllClients();

            CloseSyncConnection();

            CloseChatConnection();
        }

        public void LeaveAndRejoin ()
        {
            LeaveSyncSession();

            LeaveChatSession();

            ClientSpawnManager.Instance.RemoveAllClients();

            JoinSyncSession();

            JoinChatSession();

            SendStateCatchUpRequest();

            EnableVRButton();
        }

        public void CloseConnectionAndRejoin ()
        {
            LeaveSyncSession();

            LeaveChatSession();

            ClientSpawnManager.Instance.RemoveAllClients();

            CloseSyncConnection();

            CloseChatConnection();

            OpenSyncConnection();

            OpenChatConnection();

            SetSyncEventListeners();

            SetChatEventListeners();

            JoinSyncSession();

            JoinChatSession();

            SendStateCatchUpRequest();

            EnableVRButton();
        }

        public void OpenSyncConnection()
        {
            int result;

#if UNITY_WEBGL && !UNITY_EDITOR 
            result = SocketIOJSLib.OpenSyncConnection();
#else
            result = socketSim.OpenSyncConnection();
#endif
            if (result != SocketIOJSLib.SUCCESS)
            {
                connectionAdapter.DisplaySocketIOAdapterError("OpenSyncConnection failed.");
            }
        }

        public void OpenChatConnection()
        {
            int result;

#if UNITY_WEBGL && !UNITY_EDITOR 
            result = SocketIOJSLib.OpenChatConnection();
#else       
            result = socketSim.OpenChatConnection();
#endif
            if (result != SocketIOJSLib.SUCCESS)
            {
                connectionAdapter.DisplaySocketIOAdapterError("OpenChatConnection failed.");
            }
        }

        public void SetSyncEventListeners()
        {
            int result;

#if UNITY_WEBGL && !UNITY_EDITOR 
            result = SocketIOJSLib.SetSyncEventListeners();
#else       
            result = socketSim.SetSyncEventListeners();
#endif
            if (result != SocketIOJSLib.SUCCESS)
            {
                connectionAdapter.DisplaySocketIOAdapterError("SetSyncEventListeners failed.");
            }
        }

        public void SetChatEventListeners()
        {
            int result;

#if UNITY_WEBGL && !UNITY_EDITOR 
            result = SocketIOJSLib.SetChatEventListeners();
#else       
            result = socketSim.SetChatEventListeners();
#endif
            if (result != SocketIOJSLib.SUCCESS)
            {
                connectionAdapter.DisplaySocketIOAdapterError("SetChatEventListeners failed.");
            }
        }

        public void JoinSyncSession()
        {
            int result;

#if UNITY_WEBGL && !UNITY_EDITOR 
            result = SocketIOJSLib.JoinSyncSession();
#else       
            result = socketSim.JoinSyncSession();
#endif
            if (result != SocketIOJSLib.SUCCESS)
            {
                connectionAdapter.DisplaySocketIOAdapterError("JoinSyncSession failed.");
            }
        }

        public void JoinChatSession()
        {
            int result;

#if UNITY_WEBGL && !UNITY_EDITOR 
            result = SocketIOJSLib.JoinChatSession();
#else       
            result = socketSim.JoinChatSession();
#endif
            if (result != SocketIOJSLib.SUCCESS)
            {
                connectionAdapter.DisplaySocketIOAdapterError("JoinChatSession failed.");
            }
        }
        public void LeaveSyncSession()
        {
            int result;

#if UNITY_WEBGL && !UNITY_EDITOR 
            result = SocketIOJSLib.LeaveSyncSession();
#else       
            result = socketSim.LeaveSyncSession();
#endif
            if (result != SocketIOJSLib.SUCCESS)
            {
                connectionAdapter.DisplaySocketIOAdapterError("LeaveSyncSession failed.");
            }
        }

        public void LeaveChatSession()
        {
            int result;

#if UNITY_WEBGL && !UNITY_EDITOR 
            result = SocketIOJSLib.LeaveChatSession();
#else       
            result = socketSim.LeaveChatSession();
#endif
            if (result != SocketIOJSLib.SUCCESS)
            {
                connectionAdapter.DisplaySocketIOAdapterError("LeaveChatSession failed.");
            }
        }

        public void SendStateCatchUpRequest()
        {
            int result;

#if UNITY_WEBGL && !UNITY_EDITOR 
            result = SocketIOJSLib.SendStateCatchUpRequest();
#else       
            result = socketSim.SendStateCatchUpRequest();
#endif
            if (result != SocketIOJSLib.SUCCESS)
            {
                connectionAdapter.DisplaySocketIOAdapterError("SendStateCatchUpRequest failed.");
            }
        }

        public void EnableVRButton()
        {
            int result;

#if UNITY_WEBGL && !UNITY_EDITOR 
            result = SocketIOJSLib.EnableVRButton();
#else       
            result = socketSim.EnableVRButton();
#endif
            if (result != SocketIOJSLib.SUCCESS)
            {
                connectionAdapter.DisplaySocketIOAdapterError("EnableVRButton failed.");
            }
        }

        public void CloseSyncConnection()
        {
            int result;

#if UNITY_WEBGL && !UNITY_EDITOR 
            result = SocketIOJSLib.CloseSyncConnection();
#else       
            result = socketSim.CloseSyncConnection();
#endif
            if (result != SocketIOJSLib.SUCCESS)
            {
                connectionAdapter.DisplaySocketIOAdapterError("CloseSyncConnection failed.");
            }
        }

        public void CloseChatConnection ()
        {
            int result;

#if UNITY_WEBGL && !UNITY_EDITOR 
            result = SocketIOJSLib.CloseChatConnection();
#else       
            result = socketSim.CloseChatConnection();
#endif
            if (result != SocketIOJSLib.SUCCESS)
            {
                connectionAdapter.DisplaySocketIOAdapterError("CloseChatConnection failed.");
            }
        }

        public void OnConnect(string socketId)
        {
            connectionAdapter.SetSocketID(socketId);

            connectionAdapter.DisplayConnected();
        }

        public void OnServerName(string serverName)
        {
            connectionAdapter.SetServerName(serverName);

            connectionAdapter.DisplayConnected();
        }

        public void OnDisconnect (string reason) {
            connectionAdapter.DisplayDisconnect(reason);
        }

        public void OnError (string error) {
            connectionAdapter.SetError(error);
        }

        public void OnConnectError (string error) {
            connectionAdapter.DisplayConnectError(error);
        }

        public void OnConnectTimeout() {
            connectionAdapter.DisplayConnectTimeout();
        }

        public void OnReconnectSucceeded ()
        {
            connectionAdapter.DisplayReconnectSucceeded();
        }

        public void OnReconnectAttempt (string packedString)
        {
            string[] unpackedString = packedString.Split(',');

            string socketId = unpackedString[0];

            string attemptNumber = unpackedString[1];

            connectionAdapter.DisplayReconnectAttempt(socketId, attemptNumber);
        }

        public void OnReconnectError (string error) {
            connectionAdapter.DisplayReconnectError(error);
        }

        public void OnReconnectFailed () {
            connectionAdapter.DisplayReconnectFailed();
        }

        public void OnPing ()
        {
            connectionAdapter.DisplayPing();
        }

        public void OnPong (int latency)
        {
            connectionAdapter.DisplayPong(latency);
        }

        public void OnSessionInfo (string info) {
            connectionAdapter.DisplaySessionInfo(info);
        }

        public void OnReceiveStateCatchup (string packedData)
        {
            var state = JsonUtility.FromJson<SessionState>(packedData);

            SessionStateManager.Instance.SetSessionState(state);

            SessionStateManager.Instance.ApplyCatchup();
        }

        public void OnClientJoined (int client_id)
        {
            ClientSpawnManager.Instance.AddNewClient2(client_id);

            connectionAdapter.DisplayOtherClientJoined(client_id);
        }

        public void OnOwnClientJoined (int session_id)
        {
            connectionAdapter.SetSessionName(session_id);

            connectionAdapter.DisplayOwnClientJoined(session_id);

            ClientSpawnManager.Instance.DisplayOwnClientIsConnected();
        }

        public void OnFailedToJoin(int session_id)
        {
            connectionAdapter.DisplayFailedToJoin(session_id);

            ClientSpawnManager.Instance.DisplayOwnClientIsDisconnected();
        }

        public void OnOtherClientLeft (int client_id)
        {
            ClientSpawnManager.Instance.RemoveClient(client_id);
        }

        public void OnOwnClientLeft (int session_id)
        {
            connectionAdapter.SetSessionName(session_id);

            connectionAdapter.DisplayOwnClientLeft(session_id);

            ClientSpawnManager.Instance.DisplayOwnClientIsDisconnected();
        }

        public void OnFailedToLeave(int session_id)
        {
            connectionAdapter.DisplayFailedToLeave(session_id);

            ClientSpawnManager.Instance.DisplayOwnClientIsConnected();
        }

        public void OnOwnClientDisconnected (int client_id)
        {
            // Don't do anything for now, because in theory we should not hear about a client disconnecting after it has left the session.
            Debug.Log($"OnOwnClientDisconnected({client_id})");
        }

        public void OnOtherClientDisconnected (int client_id)
        {
            connectionAdapter.DisplayOtherClientDisconnected(client_id);

            ClientSpawnManager.Instance.RemoveClient(client_id);
        }

        public void OnMessage (string typeAndMessage)
        {
            netUpdateHandler.ProcessMessage(typeAndMessage);
        }

        public void OnSendMessageFailed (string reason)
        {
            connectionAdapter.DisplaySendMessageFailed(reason);
        }

        public void OnBump (int session_id)
        {
            connectionAdapter.DisplayBump(session_id);

            ClientSpawnManager.Instance.RemoveAllClients();
        }

        public void OnTabClosed ()
        {
            Instance.LeaveAndCloseConnection();
        }

        // Unity lifecycle function -- will not fire in WebGL
        public void OnApplicationQuit ()
        {
            Instance.LeaveAndCloseConnection();
        }

        public void OnCaptureStarted () 
        {
            CaptureManager.DisableCaptureButton();
        }
    }
}
